using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

// Uso: MigradorDataBanks <ruta DataBanks.db> <connection string SQL Server>
if (args.Length != 2)
{
    Console.WriteLine("Uso: MigradorDataBanks <ruta DataBanks.db> <connection string>");
    return 1;
}

using var src = new SqliteConnection($"Data Source={args[0]};Mode=ReadOnly");
using var dst = new SqlConnection(args[1]);
try
{
    src.Open();
    dst.Open();
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR: no se pudo abrir la conexion de origen/destino: {ex.Message}");
    return 3;
}

// 0) Abortamos si el destino ya tiene datos (evita duplicar en re-ejecuciones)
using (var check = dst.CreateCommand())
{
    check.CommandText = "SELECT COUNT(*) FROM bancos.PerfilesBanco";
    if ((int)check.ExecuteScalar()! > 0)
    {
        Console.WriteLine("ERROR: bancos.PerfilesBanco ya tiene datos. Vaciar el schema bancos antes de migrar.");
        return 2;
    }
}

// 1) Diagnóstico de huérfanos en SQLite (las cascadas nunca corrieron:
//    Microsoft.Data.Sqlite no activa PRAGMA foreign_keys). Solo informamos
//    y los excluimos en los SELECT; el .db original no se toca (ReadOnly).
var filtros = new Dictionary<string, string>
{
    ["ArchivosImportados"]        = "WHERE IdPerfilBanco IN (SELECT Id FROM PerfilesBanco)",
    ["MovimientosArchivo"]        = "WHERE IdArchivo IN (SELECT Id FROM ArchivosImportados WHERE IdPerfilBanco IN (SELECT Id FROM PerfilesBanco))",
    ["HomologacionConceptos"]     = "WHERE IdPerfilBanco IN (SELECT Id FROM PerfilesBanco) AND IdConceptoEstandar IN (SELECT Id FROM ConceptosEstandar)",
    ["ConciliacionSesiones"]      = "WHERE IdArchivoImportado IN (SELECT Id FROM ArchivosImportados WHERE IdPerfilBanco IN (SELECT Id FROM PerfilesBanco))",
    ["ConciliacionItemsExternos"] = "WHERE IdSesion IN (SELECT Id FROM ConciliacionSesiones WHERE IdArchivoImportado IN (SELECT Id FROM ArchivosImportados WHERE IdPerfilBanco IN (SELECT Id FROM PerfilesBanco)))",
    ["ConciliacionPares"]         = "WHERE IdSesion IN (SELECT Id FROM ConciliacionSesiones WHERE IdArchivoImportado IN (SELECT Id FROM ArchivosImportados WHERE IdPerfilBanco IN (SELECT Id FROM PerfilesBanco))) AND IdItemExterno IN (SELECT Id FROM ConciliacionItemsExternos WHERE IdSesion IN (SELECT Id FROM ConciliacionSesiones WHERE IdArchivoImportado IN (SELECT Id FROM ArchivosImportados WHERE IdPerfilBanco IN (SELECT Id FROM PerfilesBanco))))",
};

// 2) Copia en orden de dependencias, preservando IDs
string[] tablas =
{
    "PerfilesBanco", "ConceptosEstandar", "HomologacionConceptos",
    "ArchivosImportados", "MovimientosArchivo",
    "ConciliacionSesiones", "ConciliacionItemsExternos", "ConciliacionPares",
};

foreach (var tabla in tablas)
{
    var where = filtros.TryGetValue(tabla, out var f) ? f : "";
    using var cmd = src.CreateCommand();
    cmd.CommandText = $"SELECT * FROM {tabla} {where} ORDER BY Id";
    using var reader = cmd.ExecuteReader();
    var dt = new DataTable();
    dt.Load(reader);

    using var bulk = new SqlBulkCopy(dst, SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.CheckConstraints, null)
    {
        DestinationTableName = $"bancos.{tabla}",
        BatchSize = 5000,
    };
    foreach (DataColumn c in dt.Columns)
        bulk.ColumnMappings.Add(c.ColumnName, c.ColumnName);
    try
    {
        bulk.WriteToServer(dt);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR: fallo al copiar bancos.{tabla}: {ex.Message}");
        return 4;
    }
    Console.WriteLine($"bancos.{tabla}: {dt.Rows.Count} filas");
}

// 3) Reseed de identity: SqlBulkCopy con KeepIdentity copia los valores de Id
//    pero NO reajusta el contador interno de IDENTITY de la tabla destino.
//    Sin este paso, el primer INSERT normal de la app (Id automático) choca
//    contra los Id ya migrados. Todas las tablas de bancos usan Id IDENTITY.
foreach (var tabla in tablas)
{
    using var reseed = dst.CreateCommand();
    reseed.CommandText = $"DBCC CHECKIDENT ('bancos.{tabla}', RESEED, (SELECT ISNULL(MAX(Id), 0) FROM bancos.{tabla}))";
    reseed.ExecuteNonQuery();
}

Console.WriteLine("Migracion OK.");
return 0;
