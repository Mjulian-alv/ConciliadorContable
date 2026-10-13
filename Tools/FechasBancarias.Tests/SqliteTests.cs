// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Una fecha SQLite inválida debe rechazar toda la transferencia.
namespace FechasBancarias.Tests;
using Microsoft.Data.Sqlite;
using MigradorDataBanks.Data;
using AgrupadorConceptos.Data;
using Conciliador.Comun;
using Dapper;
using System.Reflection;
internal static class SqliteTests
{
    public static void Run()
    {
        using var db=new SqlFixture();SqlDb.ConnectionString=db.Cn.ConnectionString;
        typeof(DatabaseHelper).GetField("_inicializada",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,false);DatabaseHelper.InitializeDatabase();
        using var src=new SqliteConnection("Data Source=:memory:");src.Open();
        src.Execute(@"CREATE TABLE PerfilesBanco(Id INTEGER,NombreBanco TEXT,ColumnaConcepto TEXT,EsCodigo INTEGER,FilaEncabezado INTEGER,TipoImporte INTEGER);
            CREATE TABLE ConceptosEstandar(Id INTEGER,Nombre TEXT);
            CREATE TABLE HomologacionConceptos(Id INTEGER,IdPerfilBanco INTEGER,IdConceptoEstandar INTEGER);
            CREATE TABLE ArchivosImportados(Id INTEGER,IdPerfilBanco INTEGER,NombreArchivo TEXT,Fecha TEXT);
            CREATE TABLE MovimientosArchivo(Id INTEGER,IdArchivo INTEGER,Fecha TEXT,fecha2 TEXT);
            CREATE TABLE ConciliacionSesiones(Id INTEGER,IdArchivoImportado INTEGER);
            CREATE TABLE ConciliacionItemsExternos(Id INTEGER,IdSesion INTEGER,Fecha TEXT);
            CREATE TABLE ConciliacionPares(Id INTEGER,IdSesion INTEGER,IdItemExterno INTEGER);
            INSERT INTO PerfilesBanco VALUES(7,'Prueba','Concepto',0,1,1);
            INSERT INTO ArchivosImportados VALUES(9,7,'Archivo','2026-10-13');
            INSERT INTO MovimientosArchivo VALUES(12,9,'03/04/2026','ignorada'),(13,9,'inválida','ignorada');");
        bool rechazo=false;try{SqliteMigrationStorage.Migrar(src,db.Cn);}catch(InvalidOperationException){rechazo=true;}
        Program.Check(rechazo && db.Cn.ExecuteScalar<int>("SELECT COUNT(*) FROM bancos.PerfilesBanco")==0,"SQLite inválida dejó tablas copiadas");
        src.Execute("UPDATE MovimientosArchivo SET Fecha='2026-04-30' WHERE Id=13");
        SqliteMigrationStorage.Migrar(src,db.Cn);
        Program.Check(db.Cn.QuerySingle<DateTime>("SELECT Fecha FROM bancos.MovimientosArchivo WHERE Id=12")==new DateTime(2026,4,3),"SQLite perdió día/ID");
        Program.Check(db.Cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.MovimientosArchivo")==2,"SQLite perdió filas");
        int id=db.Cn.QuerySingle<int>("INSERT bancos.PerfilesBanco(NombreBanco,ColumnaConcepto,EsCodigo,TipoImporte) OUTPUT INSERTED.Id VALUES('Nuevo','C',0,1)");
        Program.Check(id>7,"Identity quedó antes de los IDs migrados");
        Console.WriteLine("OK: SQLite valida antes de copiar, fecha tipada, IDs y fecha2 auxiliar");
    }
}
