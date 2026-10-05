// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 2 - Escribe una entidad por transacción; nunca usa el guardado masivo del escritorio.
using ArcaCliente.Models;
using ArcaCliente.Services;
using ArcaCliente.Web.Models;
using ArcaCliente.Web.Services;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Text.Json;
namespace ArcaCliente.Web.Data;
public sealed class SqlConfigurationStorage(string connectionString) : IWebConfigurationStore
{
    const string SelectProfile="""
        SELECT CONVERT(uniqueidentifier,p.Id) AS Id,p.Nombre,p.TipoArchivo,p.Separador,p.Encoding,p.HojaExcel,p.TieneCabecera,
        p.ColFecha,p.ColPuntoVenta,p.ColNumero,p.ColTipoComprobante,p.ColCuit,p.ColNombreProveedor,p.ColTotal,
        p.PosFecha,p.PosPuntoVenta,p.PosNumero,p.PosTipoComprobante,p.PosCuit,p.PosNombreProveedor,p.PosTotal,
        p.FormatoFecha,p.SeparadorDecimal,p.CarpetaCsvArca,p.SistemaExportacion,p.ConfigPreseaJson,p.DirectivasJson,o.SettingsJson,
        CONVERT(varchar(18),p.WebVersion,1)+COALESCE(CONVERT(varchar(18),o.Version,1),'') AS Version
        FROM arca.ArcaPerfilesOffline p LEFT JOIN arca.WebPerfiles o ON o.Id=p.Id
        """;
    public async Task Initialize() {
        await using var c=new SqlConnection(connectionString);await c.OpenAsync();using var tx=c.BeginTransaction();
        await c.ExecuteAsync("EXEC sp_getapplock @Resource='ArcaWebSchema', @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=30000",transaction:tx);
        await c.ExecuteAsync(ArcaSqlSchema.Ddl,transaction:tx);
        await c.ExecuteAsync("""
            -- Fecha: 05/10/2026 - TAREA: 00064 - Linea: 2 - Versiones y configuración complementaria sin alterar datos fiscales.
            IF COL_LENGTH('arca.ArcaPerfilesOffline','WebVersion') IS NULL
                ALTER TABLE arca.ArcaPerfilesOffline ADD WebVersion rowversion;
            IF OBJECT_ID('arca.WebPerfiles') IS NULL
                CREATE TABLE arca.WebPerfiles (Id nvarchar(36) PRIMARY KEY, SettingsJson nvarchar(max) NOT NULL, Version rowversion);
            IF OBJECT_ID('arca.WebPerfilCuits') IS NULL
                CREATE TABLE arca.WebPerfilCuits (PerfilId nvarchar(36) NOT NULL, Cuit char(11) NOT NULL, PRIMARY KEY(PerfilId,Cuit));
            IF OBJECT_ID('arca.WebFormatosProveedor') IS NULL
                CREATE TABLE arca.WebFormatosProveedor (Cuit char(11) PRIMARY KEY, SettingsJson nvarchar(max) NOT NULL, Version rowversion);
            """,transaction:tx);
        tx.Commit();
    }
    sealed class ProfileRow : PerfilOffline { public string? SettingsJson{get;set;} public string Version{get;set;}="";public string? DirectivasJson{get;set;} public string? ConfigPreseaJson{get;set;} }
    public async Task<List<WebProfile>> Profiles() {
        await using var c=new SqlConnection(connectionString);await c.OpenAsync();using var tx=c.BeginTransaction(System.Data.IsolationLevel.Serializable);
        var rows=await c.QueryAsync<ProfileRow>(SelectProfile+" ORDER BY p.Nombre",transaction:tx);var result=new List<WebProfile>();
        foreach(var r in rows){
            r.DirectivasConciliacion=JsonSerializer.Deserialize<List<DirectivaConciliacion>>(r.DirectivasJson??"[]")??new();
            if(r.DirectivasConciliacion.Count==0)r.DirectivasConciliacion.Add(DirectivaConciliacion.CrearPrimaria());
            r.ConfigPresea=string.IsNullOrWhiteSpace(r.ConfigPreseaJson)?null:JsonSerializer.Deserialize<ConfigPresea>(r.ConfigPreseaJson);
            var p=r.SettingsJson==null?new WebProfile():JsonSerializer.Deserialize<WebProfile>(r.SettingsJson)!;
            var extras=p.SystemFormat;p.Profile=r;p.Version=r.Version;p.SystemFormat=FileFormat.From(r);
            p.SystemFormat.FirstRow=extras.FirstRow;p.SystemFormat.NumberMode=extras.NumberMode;p.SystemFormat.NumberSeparator=extras.NumberSeparator;p.SystemFormat.PointLength=extras.PointLength;
            p.Cuits=(await c.QueryAsync<string>("SELECT Cuit FROM arca.WebPerfilCuits WHERE PerfilId=@Id ORDER BY Cuit",new{Id=r.Id.ToString()},tx)).ToList();result.Add(p);
        }tx.Commit();return result;
    }
    public async Task<WebProfile> Save(WebProfile value) {
        ConfigurationValidator.Profile(value);var p=value.Profile;ApplyFormat(p,value.SystemFormat);
        await using var c=new SqlConnection(connectionString);await c.OpenAsync();using var tx=c.BeginTransaction(System.Data.IsolationLevel.Serializable);
        var current=await c.QuerySingleOrDefaultAsync<string>("""
            SELECT CONVERT(varchar(18),p.WebVersion,1)+COALESCE(CONVERT(varchar(18),o.Version,1),'')
            FROM arca.ArcaPerfilesOffline p WITH(UPDLOCK,HOLDLOCK) LEFT JOIN arca.WebPerfiles o WITH(UPDLOCK,HOLDLOCK) ON o.Id=p.Id WHERE p.Id=@Id
            """,new{Id=p.Id.ToString()},tx);
        if((current??"")!=value.Version)throw new ConfigurationConflictException();
        if(current==null)await c.ExecuteAsync("INSERT INTO arca.ArcaPerfilesOffline(Id,Nombre) VALUES(@Id,@Nombre)",new{Id=p.Id.ToString(),p.Nombre},tx);
        var args=new DynamicParameters(p);args.Add("Id",p.Id.ToString());args.Add("DirectivasJson",JsonSerializer.Serialize(p.DirectivasConciliacion));args.Add("ConfigPreseaJson",p.ConfigPresea==null?null:JsonSerializer.Serialize(p.ConfigPresea));
        await c.ExecuteAsync("""
            UPDATE arca.ArcaPerfilesOffline SET Nombre=@Nombre,TipoArchivo=@TipoArchivo,Separador=@Separador,Encoding=@Encoding,HojaExcel=@HojaExcel,
            TieneCabecera=@TieneCabecera,ColFecha=@ColFecha,ColPuntoVenta=@ColPuntoVenta,ColNumero=@ColNumero,ColTipoComprobante=@ColTipoComprobante,
            ColCuit=@ColCuit,ColNombreProveedor=@ColNombreProveedor,ColTotal=@ColTotal,PosFecha=@PosFecha,PosPuntoVenta=@PosPuntoVenta,PosNumero=@PosNumero,
            PosTipoComprobante=@PosTipoComprobante,PosCuit=@PosCuit,PosNombreProveedor=@PosNombreProveedor,PosTotal=@PosTotal,
            FormatoFecha=@FormatoFecha,SeparadorDecimal=@SeparadorDecimal,DirectivasJson=@DirectivasJson WHERE Id=@Id
            """,args,tx);
        await c.ExecuteAsync("""
            UPDATE arca.WebPerfiles SET SettingsJson=@Json WHERE Id=@Id;
            IF @@ROWCOUNT=0 INSERT INTO arca.WebPerfiles(Id,SettingsJson) VALUES(@Id,@Json);
            DELETE FROM arca.WebPerfilCuits WHERE PerfilId=@Id;
            """,new{Id=p.Id.ToString(),Json=JsonSerializer.Serialize(value)},tx);
        foreach(var cu in value.Cuits)await c.ExecuteAsync("INSERT INTO arca.WebPerfilCuits(PerfilId,Cuit) VALUES(@Id,@Cuit)",new{Id=p.Id.ToString(),Cuit=cu},tx);
        value.Version=await c.QuerySingleAsync<string>("SELECT CONVERT(varchar(18),p.WebVersion,1)+CONVERT(varchar(18),o.Version,1) FROM arca.ArcaPerfilesOffline p JOIN arca.WebPerfiles o ON o.Id=p.Id WHERE p.Id=@Id",new{Id=p.Id.ToString()},tx);
        tx.Commit();return value;
    }
    sealed class ProviderRow {public string SettingsJson{get;set;}="";public string Version{get;set;}="";}
    public async Task<ProviderConfiguration> Provider(string cuit) {
        cuit=CuitListImporter.Normalize(cuit);await using var c=new SqlConnection(connectionString);
        var r=await c.QuerySingleOrDefaultAsync<ProviderRow>("SELECT SettingsJson,CONVERT(varchar(18),Version,1) AS Version FROM arca.WebFormatosProveedor WHERE Cuit=@Cuit",new{Cuit=cuit});
        if(r==null)return new(){Cuit=cuit};var p=JsonSerializer.Deserialize<ProviderConfiguration>(r.SettingsJson)!;p.Version=r.Version;return p;
    }
    public async Task<ProviderConfiguration> Save(ProviderConfiguration p) {
        ConfigurationValidator.Provider(p);await using var c=new SqlConnection(connectionString);await c.OpenAsync();using var tx=c.BeginTransaction(System.Data.IsolationLevel.Serializable);
        var current=await c.QuerySingleOrDefaultAsync<string>("SELECT CONVERT(varchar(18),Version,1) FROM arca.WebFormatosProveedor WITH(UPDLOCK,HOLDLOCK) WHERE Cuit=@Cuit",new{p.Cuit},tx);
        if((current??"")!=p.Version)throw new ConfigurationConflictException();
        await c.ExecuteAsync("""
            UPDATE arca.WebFormatosProveedor SET SettingsJson=@Json WHERE Cuit=@Cuit;
            IF @@ROWCOUNT=0 INSERT INTO arca.WebFormatosProveedor(Cuit,SettingsJson) VALUES(@Cuit,@Json);
            """,new{p.Cuit,Json=JsonSerializer.Serialize(p)},tx);
        p.Version=await c.QuerySingleAsync<string>("SELECT CONVERT(varchar(18),Version,1) FROM arca.WebFormatosProveedor WHERE Cuit=@Cuit",new{p.Cuit},tx);tx.Commit();return p;
    }
    static void ApplyFormat(PerfilOffline p,FileFormat f) {
        p.TipoArchivo=f.FileType;p.Separador=f.Separator;p.Encoding=f.Encoding;p.HojaExcel=f.Sheet;p.TieneCabecera=f.Header;p.FormatoFecha=f.DatePattern;p.SeparadorDecimal=f.DecimalSeparator;
        if(f.Header){p.ColFecha=f.Date;p.ColPuntoVenta=f.Point;p.ColNumero=f.Number;p.ColTipoComprobante=f.Type;p.ColTotal=f.Amount;p.ColCuit=f.Cuit;p.ColNombreProveedor=f.Name;}
        else {int Pos(string s)=>int.TryParse(s,out var i)?i:0;p.PosFecha=Pos(f.Date);p.PosPuntoVenta=Pos(f.Point);p.PosNumero=Pos(f.Number);p.PosTipoComprobante=Pos(f.Type);p.PosTotal=Pos(f.Amount);p.PosCuit=Pos(f.Cuit);p.PosNombreProveedor=Pos(f.Name);}
    }
}
