// Fecha: 06/10/2026 - TAREA: 00072 - Linea: 2 - Comparte la tabla de WinForms con escrituras por entidad y detección de cambios concurrentes.
using ArcaCliente.Models;
using ArcaCliente.Web.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Text.Json;
namespace ArcaCliente.Web.Data;
public sealed class SqlPyRProfileStorage(string connectionString) : IPyRProfileStore
{
    sealed class Row : PerfilOfflinePyR {
        public string CuentasJson { get; set; }="[]";
        public string DirectivasJson { get; set; }="[]";
        public string Version { get; set; }="";
    }
    public async Task Initialize()
    {
        await using var c=new SqlConnection(connectionString);await c.OpenAsync();using var tx=c.BeginTransaction();
        await c.ExecuteAsync("EXEC sp_getapplock @Resource='ArcaWebSchema', @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=30000",transaction:tx);
        await c.ExecuteAsync("""
            -- Fecha: 06/10/2026 - TAREA: 00072 - Linea: 2 - Detecta también los cambios escritos desde el escritorio.
            IF COL_LENGTH('arca.ArcaPerfilesPyR','WebVersion') IS NULL
                ALTER TABLE arca.ArcaPerfilesPyR ADD WebVersion rowversion;
            """,transaction:tx);tx.Commit();
    }
    public async Task<List<WebPyRProfile>> Profiles()
    {
        await using var c=new SqlConnection(connectionString);
        var rows=await c.QueryAsync<Row>("""
            SELECT CONVERT(uniqueidentifier,Id) AS Id,Nombre,HojaExcel,TieneCabecera,
            ColFecha,ColAsiento,ColConcepto,ColDebe,ColHaber,ColOperador,FormatoFecha,SeparadorDecimal,
            CarpetaArca,CuentasJson,DirectivasJson,CONVERT(varchar(18),WebVersion,1) AS Version
            FROM arca.ArcaPerfilesPyR ORDER BY Nombre
            """);
        return rows.Select(r=>{
            r.Cuentas=JsonSerializer.Deserialize<List<CuentaPyR>>(r.CuentasJson)??new();
            r.DirectivasConciliacion=JsonSerializer.Deserialize<List<DirectivaPyR>>(r.DirectivasJson)??new();
            if(r.DirectivasConciliacion.Count==0)r.DirectivasConciliacion=DirectivaPyR.CrearPredeterminadas();
            return new WebPyRProfile{Profile=r,Version=r.Version};
        }).ToList();
    }
    public async Task<WebPyRProfile> Save(WebPyRProfile value)
    {
        PyRProfileValidator.Validate(value);var p=value.Profile;
        await using var c=new SqlConnection(connectionString);await c.OpenAsync();using var tx=c.BeginTransaction(System.Data.IsolationLevel.Serializable);
        var current=await c.QuerySingleOrDefaultAsync<string>("SELECT CONVERT(varchar(18),WebVersion,1) FROM arca.ArcaPerfilesPyR WITH(UPDLOCK,HOLDLOCK) WHERE Id=@Id",new{Id=p.Id.ToString()},tx);
        if((current??"")!=value.Version)throw new ConfigurationConflictException();
        if(current==null)await c.ExecuteAsync("INSERT INTO arca.ArcaPerfilesPyR(Id,Nombre) VALUES(@Id,@Nombre)",new{Id=p.Id.ToString(),p.Nombre},tx);
        var args=new DynamicParameters(p);args.Add("Id",p.Id.ToString());args.Add("CuentasJson",JsonSerializer.Serialize(p.Cuentas));args.Add("DirectivasJson",JsonSerializer.Serialize(p.DirectivasConciliacion));
        await c.ExecuteAsync("""
            UPDATE arca.ArcaPerfilesPyR SET Nombre=@Nombre,HojaExcel=@HojaExcel,TieneCabecera=@TieneCabecera,
            ColFecha=@ColFecha,ColAsiento=@ColAsiento,ColConcepto=@ColConcepto,ColDebe=@ColDebe,ColHaber=@ColHaber,
            ColOperador=@ColOperador,FormatoFecha=@FormatoFecha,SeparadorDecimal=@SeparadorDecimal,CarpetaArca=@CarpetaArca,
            CuentasJson=@CuentasJson,DirectivasJson=@DirectivasJson WHERE Id=@Id
            """,args,tx);
        var saved=value.Copy();saved.Version=await c.QuerySingleAsync<string>("SELECT CONVERT(varchar(18),WebVersion,1) FROM arca.ArcaPerfilesPyR WHERE Id=@Id",new{Id=p.Id.ToString()},tx);
        tx.Commit();return saved;
    }
    public async Task Delete(WebPyRProfile value)
    {
        await using var c=new SqlConnection(connectionString);
        var count=await c.ExecuteAsync("DELETE FROM arca.ArcaPerfilesPyR WHERE Id=@Id AND CONVERT(varchar(18),WebVersion,1)=@Version",new{Id=value.Profile.Id.ToString(),value.Version});
        if(count!=1)throw new ConfigurationConflictException();
    }
}
