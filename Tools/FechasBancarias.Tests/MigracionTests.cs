namespace FechasBancarias.Tests;
// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 2 - Una migración fallida debe conservar tipos, nombres y valores de ambas tablas.
using Dapper;
using MigradorFechasBancos.Services;
internal static class MigracionTests
{
    public static void Run()
    {
        foreach (int estado in new[] { 0, 1, 2 })
        {
            using var db = new SqlFixture(); db.Anterior(estado!=0);
            if(estado==2) db.Cn.Execute("UPDATE bancos.MovimientosArchivo SET fecha2=@Fecha WHERE Id=2",new{Fecha=new DateTime(2026,4,30)});
            var d = MigracionFechaService.Diagnosticar(db.Cn);
            Program.Check(d.PuedeAplicar && !d.YaMigrada && d.Tablas.Sum(t=>t.Cantidad)==3,"Diagnóstico anterior");
            Program.Check(db.Cn.ExecuteScalar<string>("SELECT TYPE_NAME(system_type_id) FROM sys.columns WHERE object_id=OBJECT_ID('bancos.MovimientosArchivo') AND name='Fecha'")=="nvarchar","Diagnóstico modificó schema");
            Program.Check(MigracionFechaService.Aplicar(db.Cn).YaMigrada,"No terminó migración");
            Program.Check(db.Cn.QuerySingle<DateTime>("SELECT Fecha FROM bancos.MovimientosArchivo WHERE Id=1")==new DateTime(2026,4,3),"Día invertido");
            Program.Check(db.Cn.QuerySingle<string>("SELECT FechaTextoOriginal FROM bancos.MovimientosArchivo WHERE Id=1")=="03/04/2026","Texto perdido");
            Program.Check(db.Cn.QuerySingle<decimal>("SELECT SUM(Importe) FROM bancos.MovimientosArchivo")==120m,"Importe alterado");
            Program.Check(MigracionFechaService.Aplicar(db.Cn).YaMigrada,"Repetición");
            db.Cn.Execute("INSERT bancos.MovimientosArchivo(Id,Fecha,Importe) VALUES(3,@Fecha,20)",new{Fecha=new DateTime(2026,5,1)});
            Program.Check(MigracionFechaService.Aplicar(db.Cn).YaMigrada,"Movimiento nuevo sin texto histórico");
        }
        foreach (string sql in new[] {
            "UPDATE bancos.MovimientosArchivo SET fecha2='20260304' WHERE Id=1",
            "UPDATE bancos.MovimientosArchivo SET fecha2='20260403 12:30' WHERE Id=1",
            "UPDATE bancos.MovimientosArchivo SET Fecha='inválida' WHERE Id=2",
            "UPDATE bancos.MovimientosArchivo SET Fecha='' WHERE Id=2",
            "ALTER TABLE bancos.MovimientosArchivo ADD FechaTextoOriginal nvarchar(30)",
            "CREATE INDEX IX_Fecha ON bancos.MovimientosArchivo(Fecha)",
            "DELETE bancos.ConciliacionItemsExternos; ALTER TABLE bancos.ConciliacionItemsExternos ALTER COLUMN Fecha date NOT NULL" })
        {
            using var db=new SqlFixture(); db.Anterior();
            Program.Check(MigracionFechaService.Diagnosticar(db.Cn).PuedeAplicar,"Precondición");
            db.Cn.Execute(sql);
            try { MigracionFechaService.Aplicar(db.Cn); throw new Exception("Aceptó conflicto: "+sql); }
            catch (InvalidOperationException) { }
            Program.Check(db.Cn.ExecuteScalar<string>("SELECT TYPE_NAME(system_type_id) FROM sys.columns WHERE object_id=OBJECT_ID('bancos.MovimientosArchivo') AND name='Fecha'")=="nvarchar","Cambio parcial");
        }
        using (var db=new SqlFixture())
        {
            db.Anterior();
            db.Cn.Execute("CREATE TRIGGER FallarSegundoCambio ON DATABASE FOR RENAME AS IF EVENTDATA().value('(/EVENT_INSTANCE/ObjectName)[1]','nvarchar(200)') LIKE '%ConciliacionItemsExternos%' THROW 51001, 'Fallo simulado', 1;");
            // El rollback se comprueba también con el rechazo de un renombrado del lado del servidor.
            db.Cn.Execute("ALTER TRIGGER FallarSegundoCambio ON DATABASE FOR RENAME AS IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('bancos.ConciliacionItemsExternos') AND name='FechaTextoOriginal') THROW 51001, 'Fallo simulado', 1;");
            try { MigracionFechaService.Aplicar(db.Cn); throw new Exception("No falló el DDL simulado"); }
            catch (Microsoft.Data.SqlClient.SqlException) { }
            Program.Check(db.Cn.ExecuteScalar<int>("SELECT COUNT(*) FROM sys.columns WHERE object_id=OBJECT_ID('bancos.MovimientosArchivo') AND name='FechaTextoOriginal'")==0,"No revirtió renombrado");
            Program.Check(db.Cn.QuerySingle<string>("SELECT Fecha FROM bancos.MovimientosArchivo WHERE Id=1")=="03/04/2026","Rollback perdió texto de primera tabla");
        }
        Concurrencia();
        Console.WriteLine("OK: migración, conflictos, relectura, rollback e idempotencia");
    }
    static void Concurrencia()
    {
        using var db=new SqlFixture();db.Anterior();
        using var tx=db.Cn.BeginTransaction();
        db.Cn.Execute("DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource=N'bancos.Conciliacion.Mutacion',@LockMode=N'Exclusive',@LockOwner=N'Transaction';",transaction:tx);
        using var inicio=new ManualResetEventSlim();
        var aplicar=Task.Run(()=>{using var otra=new Microsoft.Data.SqlClient.SqlConnection(db.Cn.ConnectionString);otra.Open();inicio.Set();try{MigracionFechaService.Aplicar(otra);return false;}catch(InvalidOperationException){return true;}});
        Program.Check(inicio.Wait(TimeSpan.FromSeconds(5))&&!aplicar.Wait(200),"Migrador no esperó bloqueo del módulo");
        db.Cn.Execute("UPDATE bancos.MovimientosArchivo SET fecha2=@Fecha WHERE Id=1",new{Fecha=new DateTime(2026,3,4)},tx);tx.Commit();
        Program.Check(aplicar.Wait(TimeSpan.FromSeconds(20))&&aplicar.Result,"Migrador no revalidó después de esperar");
    }
}
