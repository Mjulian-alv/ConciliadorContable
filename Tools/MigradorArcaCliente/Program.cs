using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

// Uso: MigradorArcaCliente <ruta conciliador.db> <connection string SQL Server>
if (args.Length != 2)
{
    Console.WriteLine("Uso: MigradorArcaCliente <ruta conciliador.db> <connection string>");
    return 1;
}

using var src = new SqliteConnection($"Data Source={args[0]};Mode=ReadOnly");
src.Open();
using var dst = new SqlConnection(args[1]);
dst.Open();

// 0) Abortamos si el destino ya tiene datos (evita duplicar en re-ejecuciones).
//    No usamos ArcaPerfilesOffline como referencia unica: cada tabla se
//    verifica antes de copiarla, para permitir migrar en mas de una corrida
//    si alguna tabla vino vacia en el origen.
string[] tablas =
{
    "ArcaPerfilesOffline", "ArcaPerfilesFiscales", "ArcaEquivalencias",
    "PreseaProveedores", "PreseaComprobantesExportados", "PreseaMapeoColumnas",
};

foreach (var tabla in tablas)
{
    using var check = dst.CreateCommand();
    check.CommandText = $"SELECT COUNT(*) FROM arca.{tabla}";
    if ((int)check.ExecuteScalar()! > 0)
    {
        Console.WriteLine($"ERROR: arca.{tabla} ya tiene datos. Vaciar el schema arca antes de migrar.");
        return 2;
    }
}

// 1) Copia directa: no hay IDENTITY (las PK son GUID/CUIT/Entidad como texto),
//    asi que no hace falta el reseed que si necesito el migrador de bancos.
foreach (var tabla in tablas)
{
    using var cmd = src.CreateCommand();
    cmd.CommandText = $"SELECT * FROM {tabla}";
    using var reader = cmd.ExecuteReader();
    var dt = new DataTable();
    dt.Load(reader);

    if (dt.Rows.Count == 0)
    {
        Console.WriteLine($"arca.{tabla}: 0 filas (origen vacio, se omite)");
        continue;
    }

    using var bulk = new SqlBulkCopy(dst)
    {
        DestinationTableName = $"arca.{tabla}",
        BatchSize = 1000,
    };
    foreach (DataColumn c in dt.Columns)
        bulk.ColumnMappings.Add(c.ColumnName, c.ColumnName);
    bulk.WriteToServer(dt);
    Console.WriteLine($"arca.{tabla}: {dt.Rows.Count} filas");
}

Console.WriteLine("Migracion OK.");
return 0;
