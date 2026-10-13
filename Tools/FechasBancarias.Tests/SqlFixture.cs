namespace FechasBancarias.Tests;
// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 2 - Cada escenario destruye únicamente su propia base temporal.
using Dapper;
using Microsoft.Data.SqlClient;
internal sealed class SqlFixture : IDisposable
{
    readonly SqlConnection admin;
    readonly string nombre = "FechasTest_" + Guid.NewGuid().ToString("N");
    public SqlConnection Cn { get; }
    public SqlFixture()
    {
        admin = new SqlConnection(Environment.GetEnvironmentVariable("FECHAS_TEST_SQL") ?? @"Server=(localdb)\FechasBancarias00083;Database=master;Integrated Security=true;TrustServerCertificate=true");
        admin.Open();
        // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 2 - FECHAS_TEST_DIR ubica los archivos de la base temporal en otro disco; sin ella LocalDB los crea en el perfil de usuario (C:), donde el ensayo con 65 mil filas se quedaba sin espacio de log y vencía por tiempo.
        string? carpeta = Environment.GetEnvironmentVariable("FECHAS_TEST_DIR");
        if (string.IsNullOrWhiteSpace(carpeta)) admin.Execute("DECLARE @sql nvarchar(max)='CREATE DATABASE '+QUOTENAME(@Nombre); EXEC(@sql)", new { Nombre = nombre });
        else
        {
            Directory.CreateDirectory(carpeta);
            admin.Execute(@"DECLARE @sql nvarchar(max)='CREATE DATABASE '+QUOTENAME(@Nombre)+' ON (NAME=N'''+@Nombre+''', FILENAME=N'''+REPLACE(@Datos,'''','''''')+''', SIZE=64MB, FILEGROWTH=64MB) LOG ON (NAME=N'''+@Nombre+'_log'', FILENAME=N'''+REPLACE(@Log,'''','''''')+''', SIZE=64MB, FILEGROWTH=64MB)'; EXEC(@sql)",
                new { Nombre = nombre, Datos = Path.Combine(carpeta, nombre + ".mdf"), Log = Path.Combine(carpeta, nombre + "_log.ldf") });
        }
        Cn = new SqlConnection(new SqlConnectionStringBuilder(admin.ConnectionString) { InitialCatalog = nombre }.ConnectionString);
        Cn.Open();
    }
    public void Anterior(bool intermedia = true)
    {
        Cn.Execute("CREATE SCHEMA bancos");
        Cn.Execute("CREATE TABLE bancos.MovimientosArchivo(Id int PRIMARY KEY, Fecha nvarchar(30) NULL, Importe decimal(18,2)); CREATE TABLE bancos.ConciliacionItemsExternos(Id int PRIMARY KEY, Fecha nvarchar(30) NULL);");
        if (intermedia) Cn.Execute("ALTER TABLE bancos.MovimientosArchivo ADD fecha2 smalldatetime NULL");
        Cn.Execute("INSERT bancos.MovimientosArchivo(Id,Fecha,Importe) VALUES (1,'03/04/2026',50),(2,'2026-04-30',70); INSERT bancos.ConciliacionItemsExternos VALUES(1,'30-04-2026');");
        if (intermedia) Cn.Execute("UPDATE bancos.MovimientosArchivo SET fecha2=@Fecha WHERE Id=1", new { Fecha = new DateTime(2026,4,3) });
    }
    public void Dispose()
    {
        Cn.Dispose();
        SqlConnection.ClearAllPools();
        admin.Execute("DECLARE @sql nvarchar(max)='ALTER DATABASE '+QUOTENAME(@Nombre)+' SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE '+QUOTENAME(@Nombre); EXEC(@sql)", new { Nombre = nombre });
        admin.Dispose();
    }
}
