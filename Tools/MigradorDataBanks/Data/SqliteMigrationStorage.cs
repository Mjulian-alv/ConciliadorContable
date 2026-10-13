// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Prevalidar las dos fuentes antes de copiar cualquier tabla.
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using System.Data;
using Conciliador.Comun.Services;
namespace MigradorDataBanks.Data;
public static class SqliteMigrationStorage
{
    // Las consultas fijas conservan el criterio anterior de excluir huérfanos del SQLite.
    static readonly (string Tabla,string Leer)[] Tablas = {
        ("bancos.PerfilesBanco","SELECT * FROM PerfilesBanco ORDER BY Id"),
        ("bancos.ConceptosEstandar","SELECT * FROM ConceptosEstandar ORDER BY Id"),
        ("bancos.HomologacionConceptos","SELECT * FROM HomologacionConceptos WHERE IdPerfilBanco IN (SELECT Id FROM PerfilesBanco) AND IdConceptoEstandar IN (SELECT Id FROM ConceptosEstandar) ORDER BY Id"),
        ("bancos.ArchivosImportados","SELECT * FROM ArchivosImportados WHERE IdPerfilBanco IN (SELECT Id FROM PerfilesBanco) ORDER BY Id"),
        ("bancos.MovimientosArchivo","SELECT * FROM MovimientosArchivo WHERE IdArchivo IN (SELECT Id FROM ArchivosImportados WHERE IdPerfilBanco IN (SELECT Id FROM PerfilesBanco)) ORDER BY Id"),
        ("bancos.ConciliacionSesiones","SELECT * FROM ConciliacionSesiones WHERE IdArchivoImportado IN (SELECT Id FROM ArchivosImportados WHERE IdPerfilBanco IN (SELECT Id FROM PerfilesBanco)) ORDER BY Id"),
        ("bancos.ConciliacionItemsExternos","SELECT * FROM ConciliacionItemsExternos WHERE IdSesion IN (SELECT Id FROM ConciliacionSesiones WHERE IdArchivoImportado IN (SELECT Id FROM ArchivosImportados WHERE IdPerfilBanco IN (SELECT Id FROM PerfilesBanco))) ORDER BY Id"),
        ("bancos.ConciliacionPares","SELECT * FROM ConciliacionPares WHERE IdSesion IN (SELECT Id FROM ConciliacionSesiones WHERE IdArchivoImportado IN (SELECT Id FROM ArchivosImportados WHERE IdPerfilBanco IN (SELECT Id FROM PerfilesBanco))) AND IdItemExterno IN (SELECT Id FROM ConciliacionItemsExternos WHERE IdSesion IN (SELECT Id FROM ConciliacionSesiones WHERE IdArchivoImportado IN (SELECT Id FROM ArchivosImportados WHERE IdPerfilBanco IN (SELECT Id FROM PerfilesBanco)))) ORDER BY Id")
    };
    public static IReadOnlyDictionary<string,int> Migrar(SqliteConnection src,SqlConnection dst)
    {
        var datos=new Dictionary<string,DataTable>();var errores=new List<string>();
        using(var lectura=src.BeginTransaction())
        {
            foreach(var (tabla,sql) in Tablas)
            {
                using var cmd=src.CreateCommand();cmd.Transaction=lectura;cmd.CommandText=sql;
                using var reader=cmd.ExecuteReader();var dt=new DataTable();dt.Load(reader);
                if(tabla is "bancos.MovimientosArchivo" or "bancos.ConciliacionItemsExternos")
                {
                    var origen=dt.Columns["Fecha"] ?? throw new InvalidOperationException(tabla+": falta Fecha.");
                    origen.ColumnName="__TextoFecha";var tipada=dt.Columns.Add("Fecha",typeof(DateTime));
                    foreach(DataRow row in dt.Rows)
                    {
                        if(FechaBancariaParser.TryParse(row[origen],out var fecha)) row[tipada]=fecha;
                        else errores.Add($"{tabla} ID {row["Id"]}: fecha '{row[origen]}' inválida.");
                    }
                    dt.Columns.Remove(origen);
                    if(dt.Columns.Contains("fecha2"))dt.Columns.Remove("fecha2");
                }
                datos[tabla]=dt;
            }
            lectura.Commit();
        }
        if(errores.Count>0)throw new InvalidOperationException(string.Join(Environment.NewLine,errores));
        using var tx=dst.BeginTransaction(IsolationLevel.Serializable);
        using(var bloqueo=dst.CreateCommand())
        {
            bloqueo.Transaction=tx;
            bloqueo.CommandText=@"DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource=N'bancos.Conciliacion.Mutacion',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=15000; IF @r<0 THROW 51001,'Hay otra operación de bancos en curso.',1;
                IF EXISTS(SELECT 1 FROM bancos.PerfilesBanco WITH(TABLOCKX,HOLDLOCK)) OR EXISTS(SELECT 1 FROM bancos.ConceptosEstandar WITH(TABLOCKX,HOLDLOCK))
                OR EXISTS(SELECT 1 FROM bancos.HomologacionConceptos WITH(TABLOCKX,HOLDLOCK)) OR EXISTS(SELECT 1 FROM bancos.ArchivosImportados WITH(TABLOCKX,HOLDLOCK))
                OR EXISTS(SELECT 1 FROM bancos.MovimientosArchivo WITH(TABLOCKX,HOLDLOCK)) OR EXISTS(SELECT 1 FROM bancos.ConciliacionSesiones WITH(TABLOCKX,HOLDLOCK))
                OR EXISTS(SELECT 1 FROM bancos.ConciliacionItemsExternos WITH(TABLOCKX,HOLDLOCK)) OR EXISTS(SELECT 1 FROM bancos.ConciliacionPares WITH(TABLOCKX,HOLDLOCK))
                THROW 51002,'El destino ya contiene datos; no se copió ninguna tabla.',1;
                IF (SELECT COUNT(*) FROM sys.columns WHERE object_id IN (OBJECT_ID('bancos.MovimientosArchivo'),OBJECT_ID('bancos.ConciliacionItemsExternos')) AND name='Fecha' AND TYPE_NAME(system_type_id)='date' AND is_nullable=0)<>2
                THROW 51003,'El destino requiere esquema de fechas date NOT NULL.',1;";
            bloqueo.ExecuteNonQuery();
        }
        foreach(var (tabla,_) in Tablas)
        {
            var dt=datos[tabla];
            using var bulk=new SqlBulkCopy(dst,SqlBulkCopyOptions.KeepIdentity|SqlBulkCopyOptions.CheckConstraints,tx) { DestinationTableName=tabla,BatchSize=Math.Min(5000,Math.Max(1,dt.Rows.Count)) };
            foreach(DataColumn c in dt.Columns)bulk.ColumnMappings.Add(c.ColumnName,c.ColumnName);
            // El origen ya está ordenado por ID; avisarlo evita ordenar otra vez el lote en SQL.
            bulk.ColumnOrderHints.Add("Id",SortOrder.Ascending);
            bulk.WriteToServer(dt);
            if(dt.Rows.Count>0)
            {
                using var reseed=dst.CreateCommand();reseed.Transaction=tx;reseed.CommandText="DBCC CHECKIDENT (@Tabla, RESEED, @Id) WITH NO_INFOMSGS";
                reseed.Parameters.Add("@Tabla",SqlDbType.NVarChar,128).Value=tabla;
                reseed.Parameters.Add("@Id",SqlDbType.Int).Value=dt.AsEnumerable().Max(r=>Convert.ToInt32(r["Id"]));
                reseed.ExecuteNonQuery();
            }
        }
        tx.Commit();return datos.ToDictionary(k=>k.Key,k=>k.Value.Rows.Count);
    }
}
