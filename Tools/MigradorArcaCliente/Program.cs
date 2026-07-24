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

    // Estas columnas eran TEXT en el SQLite viejo (workaround pre-migracion:
    // los valores se guardaban con .ToString(CultureInfo.InvariantCulture)/.ToString("o")).
    // Microsoft.Data.Sqlite mapea TEXT a System.String, asi que dt.Load(reader)
    // las deja tipadas como string. El destino SQL Server ya es DECIMAL/DATETIME2
    // (ver ArcaCliente/Services/ArcaSqlSchema.cs) y SqlBulkCopy no parsea strings
    // hacia esos tipos: hay que convertir la columna antes de copiar.
    if (tabla == "PreseaProveedores")
        ConvertColumn(dt, "Descuento", typeof(decimal), v => decimal.Parse((string)v, System.Globalization.CultureInfo.InvariantCulture));
    if (tabla == "PreseaComprobantesExportados")
    {
        ConvertColumn(dt, "Importe", typeof(decimal), v => decimal.Parse((string)v, System.Globalization.CultureInfo.InvariantCulture));
        ConvertColumn(dt, "FechaExportacion", typeof(DateTime), v => DateTime.Parse((string)v, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind));
    }

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
    try
    {
        bulk.WriteToServer(dt);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR: fallo al copiar arca.{tabla}: {ex.Message}");
        return 4;
    }
    Console.WriteLine($"arca.{tabla}: {dt.Rows.Count} filas");
}

Console.WriteLine("Migracion OK.");
return 0;

static void ConvertColumn(DataTable dt, string columnName, Type targetType, Func<object, object> convert)
{
    var oldIndex = dt.Columns[columnName]!.Ordinal;
    var newCol = new DataColumn($"{columnName}_tmp", targetType);
    dt.Columns.Add(newCol);
    foreach (DataRow row in dt.Rows)
        row[newCol] = row[columnName] is DBNull ? DBNull.Value : convert(row[columnName]);
    dt.Columns.Remove(columnName);
    newCol.ColumnName = columnName;
    newCol.SetOrdinal(oldIndex);
}
