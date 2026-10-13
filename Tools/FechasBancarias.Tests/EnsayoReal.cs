// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 2 - El ensayo sólo lee el origen; recrea el esquema anterior en una base LocalDB de nombre único.
namespace FechasBancarias.Tests;
using System.Data;
using System.Text.Json;
using System.Xml.Linq;
using Dapper;
using Microsoft.Data.SqlClient;
using MigradorFechasBancos.Services;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;
using Conciliador.Comun;
using System.Reflection;
internal static class EnsayoReal
{
    public static void Run(string config,string informe)
    {
        string cadena=XDocument.Load(config).Descendants("add").Single(e=>(string)e.Attribute("key")=="ConciliadorSqlConnectionString").Attribute("value").Value;
        using var src=new SqlConnection(cadena);src.Open();
        using var local=new SqlFixture();
        string ddl=File.ReadAllText("Tools/FechasBancarias.Tests/EsquemaAnterior.sql");
        using(var tx=local.Cn.BeginTransaction()) {local.Cn.Execute(ddl,transaction:tx);local.Cn.Execute("ALTER TABLE bancos.MovimientosArchivo ADD fecha2 smalldatetime NULL",transaction:tx);tx.Commit();}
        string[] tablas=local.Cn.Query<string>("SELECT name FROM sys.tables WHERE schema_id=SCHEMA_ID('bancos') ORDER BY name").ToArray();
        var datos=new Dictionary<string,DataTable>();var relaciones=new Dictionary<string,string>();
        using(var lectura=src.BeginTransaction(IsolationLevel.Serializable))
        {
            var origen=src.Query<string>("SELECT name FROM sys.tables WHERE schema_id=SCHEMA_ID('bancos') ORDER BY name",transaction:lectura).ToArray();
            Program.Check(origen.SequenceEqual(tablas),"El esquema real tiene tablas diferentes del esquema anterior del ensayo.");
            foreach(string tabla in tablas)
            {
                const string estructura=@"SELECT name,TYPE_NAME(system_type_id) Tipo,max_length,precision,scale,is_nullable,is_identity FROM sys.columns WHERE object_id=OBJECT_ID(@Tabla) ORDER BY name FOR JSON PATH";
                string objeto="bancos."+tabla;
                Program.Check(src.QuerySingle<string>(estructura,new{Tabla=objeto},lectura)==local.Cn.QuerySingle<string>(estructura,new{Tabla=objeto}),"Columnas diferentes para "+objeto);
                using var cmd=src.CreateCommand();cmd.Transaction=lectura;
                cmd.CommandText="DECLARE @sql nvarchar(max)='SELECT * FROM bancos.'+QUOTENAME(@Tabla)+' ORDER BY Id'; EXEC sys.sp_executesql @sql";
                cmd.Parameters.Add("@Tabla",SqlDbType.NVarChar,128).Value=tabla;
                using var rd=cmd.ExecuteReader();var dt=new DataTable();dt.Load(rd);datos[tabla]=dt;
            }
            lectura.Commit();
        }
        using(var tx=local.Cn.BeginTransaction())
        {
            foreach(var tabla in tablas) local.Cn.Execute("DECLARE @sql nvarchar(max)='ALTER TABLE bancos.'+QUOTENAME(@Tabla)+' NOCHECK CONSTRAINT ALL';EXEC(@sql)",new{Tabla=tabla},tx);
            foreach(var tabla in tablas)
            {
                // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 2 - Se copia con INSERT parametrizados y no con SqlBulkCopy: en una LocalDB con poca RAM libre el BULK INSERT pedía 9 MB de concesión y el pool de consultas tenía 4 MB de objetivo, así que esperaba en RESOURCE_SEMAPHORE hasta vencer. Un INSERT necesita menos de 1 MB.
                var tabla_=datos[tabla];Console.WriteLine("Copiando "+tabla+": "+tabla_.Rows.Count);
                string[] columnas=tabla_.Columns.Cast<DataColumn>().Select(c=>c.ColumnName).ToArray();
                bool identidad=local.Cn.ExecuteScalar<int>("SELECT COUNT(*) FROM sys.identity_columns WHERE object_id=OBJECT_ID(@Tabla)",new{Tabla="bancos."+tabla},tx)>0;
                string insertar=$"INSERT bancos.{"["+tabla+"]"} ({string.Join(",",columnas.Select(c=>"["+c+"]"))}) VALUES ({string.Join(",",columnas.Select((_,i)=>"@p"+i))})";
                if(identidad)local.Cn.Execute($"SET IDENTITY_INSERT bancos.[{tabla}] ON",transaction:tx);
                foreach(DataRow fila in tabla_.Rows)
                {
                    var p=new DynamicParameters();
                    for(int i=0;i<columnas.Length;i++)
                    {
                        object v=fila[i];bool nulo=v is DBNull;
                        p.Add("p"+i,nulo?null:v,v is DateTime?DbType.DateTime2:null);
                    }
                    local.Cn.Execute(insertar,p,tx);
                }
                if(identidad)local.Cn.Execute($"SET IDENTITY_INSERT bancos.[{tabla}] OFF",transaction:tx);
            }
            foreach(var tabla in tablas) local.Cn.Execute("DECLARE @sql nvarchar(max)='ALTER TABLE bancos.'+QUOTENAME(@Tabla)+' WITH CHECK CHECK CONSTRAINT ALL';EXEC(@sql)",new{Tabla=tabla},tx);
            tx.Commit();
        }
        var antes=MigracionFechaService.Diagnosticar(local.Cn);
        Program.Check(antes.PuedeAplicar,"Incidencias en copia: "+JsonSerializer.Serialize(antes.Incidencias));
        var despues=MigracionFechaService.Aplicar(local.Cn);Program.Check(despues.YaMigrada,"Ensayo no migró ambas tablas");
        Program.Check(MigracionFechaService.Aplicar(local.Cn).YaMigrada,"Ensayo no idempotente");
        foreach(string tabla in tablas)
        {
            using var cmd=local.Cn.CreateCommand();cmd.CommandText="DECLARE @sql nvarchar(max)='SELECT * FROM bancos.'+QUOTENAME(@Tabla)+' ORDER BY Id'; EXEC sys.sp_executesql @sql";cmd.Parameters.AddWithValue("@Tabla",tabla);
            using var rd=cmd.ExecuteReader();var fin=new DataTable();fin.Load(rd);var inicial=datos[tabla];
            Program.Check(inicial.Rows.Count==fin.Rows.Count,"Cambió cantidad de "+tabla);
            bool fechaOperativa=tabla is "MovimientosArchivo" or "ConciliacionItemsExternos";
            for(int i=0;i<inicial.Rows.Count;i++)foreach(DataColumn col in inicial.Columns)
            {
                if(fechaOperativa && col.ColumnName.Equals("fecha2",StringComparison.OrdinalIgnoreCase))continue;
                string destino=fechaOperativa && col.ColumnName.Equals("Fecha",StringComparison.OrdinalIgnoreCase)?"FechaTextoOriginal":col.ColumnName;
                Program.Check(Equals(inicial.Rows[i][col],fin.Rows[i][destino]),$"Dato alterado: {tabla} fila {i+1} columna {col.ColumnName}");
            }
        }
        SqlDb.ConnectionString=local.Cn.ConnectionString;
        typeof(DatabaseHelper).GetField("_inicializada",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,false);
        DatabaseHelper.InitializeDatabase();
        var movimientos=local.Cn.Query<MovimientoProcesado>("SELECT * FROM bancos.MovimientosArchivo").ToList();
        var externos=local.Cn.Query<ConciliacionItemExterno>("SELECT * FROM bancos.ConciliacionItemsExternos").ToList();
        var archivos=SelectorExtractosService.Obtener(movimientos.Min(m=>m.Fecha),movimientos.Max(m=>m.Fecha),true);
        var pares=new List<ConciliacionPar>();
        foreach(int id in local.Cn.Query<int>("SELECT Id FROM bancos.ConciliacionSesiones"))pares.AddRange(ConciliacionExternService.ObtenerPares(id));
        int paresInternos=0;
        foreach(int id in local.Cn.Query<int>("SELECT Id FROM bancos.ConciliacionInternaSesiones"))paresInternos+=ConciliacionInternaService.ObtenerPares(id).Count;
        string excel=Path.ChangeExtension(informe,"xlsx");ConciliacionExcelExporter.Exportar(pares,externos.Where(e=>!e.Conciliado),movimientos.Take(20),excel);
        File.WriteAllText(informe,JsonSerializer.Serialize(new{Fecha=DateTimeOffset.Now,Servidor=src.DataSource,Base=src.Database,Tablas=datos.ToDictionary(x=>x.Key,x=>x.Value.Rows.Count),Antes=antes,Despues=despues,Comparacion="Todos los valores históricos e IDs iguales; fechas tipadas verificadas por el migrador.",Archivos=archivos.Count,ParesExternos=pares.Count,ParesInternos=paresInternos,Excel=excel},new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"Ensayo correcto: {movimientos.Count} movimientos, {externos.Count} externos, {paresInternos} pares internos. Informe: {informe}");
    }
}
