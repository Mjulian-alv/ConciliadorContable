// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - La copia se valida completa y se confirma en una única transacción.
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MigradorDataBanks.Data;
if(args.Length!=2) {Console.Error.WriteLine("Uso: MigradorDataBanks <ruta DataBanks.db> <connection string SQL Server>");return 1;}
try
{
    using var src=new SqliteConnection(new SqliteConnectionStringBuilder {DataSource=args[0],Mode=SqliteOpenMode.ReadOnly}.ToString());
    using var dst=new SqlConnection(args[1]);src.Open();dst.Open();
    foreach(var tabla in SqliteMigrationStorage.Migrar(src,dst))Console.WriteLine($"{tabla.Key}: {tabla.Value} filas");
    Console.WriteLine("Migración OK.");return 0;
}
catch(Exception ex) {Console.Error.WriteLine("No se completó la migración: "+ex.Message);return 2;}
