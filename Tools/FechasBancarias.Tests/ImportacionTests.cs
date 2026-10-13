namespace FechasBancarias.Tests;
// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Un error al final del archivo no debe dejar datos parciales.
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;
using Conciliador.Comun;
using ClosedXML.Excel;
using Dapper;
using System.Globalization;
using System.Reflection;
internal static class ImportacionTests
{
    public static void Run()
    {
        Compatibilidad();
        using var db=new SqlFixture();
        SqlDb.ConnectionString=db.Cn.ConnectionString;
        typeof(DatabaseHelper).GetField("_inicializada",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,false);
        DatabaseHelper.InitializeDatabase();
        int id=db.Cn.QuerySingle<int>("INSERT bancos.PerfilesBanco(NombreBanco,ColumnaConcepto,ColumnaFecha,EsCodigo,TipoImporte) OUTPUT INSERTED.Id VALUES('Fechas prueba','Concepto','Fecha',0,1)");
        var perfil=new PerfilBanco { Id=id, FilaEncabezado=1, ColumnaFecha="Fecha", ColumnaConcepto="Concepto", TipoImporte=1,ColumnaImporteUnico="Importe" };
        string path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".csv");
        try
        {
            File.WriteAllText(path,"Fecha;Concepto;Importe\ninválida;A;10\n03/04/2026;A;20\n;B;30\n");
            bool rechazo=false;
            try { ImportacionService.ImportarArchivo(path,perfil); }
            catch(InvalidOperationException ex) { rechazo=ex.Message.Contains("2") && ex.Message.Contains("4"); }
            Program.Check(rechazo,"No rechazó todas las filas inválidas (2 y 4)");
            Program.Check(db.Cn.ExecuteScalar<int>("SELECT COUNT(*) FROM bancos.ArchivosImportados")==0,"Cabecera huérfana");
            rechazo=false;
            try { ArchivoExternoReader.Leer(path); } catch(InvalidOperationException ex) { rechazo=ex.Message.Contains("2")&&ex.Message.Contains("4"); }
            Program.Check(rechazo,"Lector externo no informa ambas filas");
            File.WriteAllText(path,"Otra;Concepto;Importe\n03/04/2026;A;10\n");
            try { ImportacionService.ImportarArchivo(path,perfil); throw new Exception("Aceptó columna fecha ausente"); } catch(InvalidOperationException) { }
            File.WriteAllText(path,"Fecha;Concepto;Importe\n03/04/2026;A;10\n2026-04-30;B;20\n");
            var anterior=CultureInfo.CurrentCulture;
            try
            {
                foreach(var cultura in new[]{"en-US","es-AR"})
                {
                    CultureInfo.CurrentCulture=new CultureInfo(cultura);
                    var movs=ImportacionService.ImportarArchivo(path,perfil);
                    Program.Check((object)movs[0].Fecha is DateTime dt && dt==new DateTime(2026,4,3),"Importación sin fecha tipada");
                }
            } finally {CultureInfo.CurrentCulture=anterior;}
            int archivos=db.Cn.ExecuteScalar<int>("SELECT COUNT(*) FROM bancos.ArchivosImportados");
            int filas=db.Cn.ExecuteScalar<int>("SELECT COUNT(*) FROM bancos.MovimientosArchivo");
            db.Cn.Execute("ALTER TABLE bancos.MovimientosArchivo ADD CONSTRAINT CK_Prueba CHECK (ConceptoOriginal<>'FALLAR')");
            File.WriteAllText(path,"Fecha;Concepto;Importe\n03/04/2026;A;10\n03/04/2026;FALLAR;20\n");
            try {ImportacionService.ImportarArchivo(path,perfil);throw new Exception("No falló restricción");}catch(Microsoft.Data.SqlClient.SqlException){}
            Program.Check(db.Cn.ExecuteScalar<int>("SELECT COUNT(*) FROM bancos.ArchivosImportados")==archivos && db.Cn.ExecuteScalar<int>("SELECT COUNT(*) FROM bancos.MovimientosArchivo")==filas,"Persistencia parcial ante error SQL");
            string excel=Path.ChangeExtension(path,"xlsx");
            try
            {
                using(var wb=new XLWorkbook()) {var ws=wb.AddWorksheet("Datos");ws.Cell(1,1).Value="Fecha";ws.Cell(1,2).Value="Concepto";ws.Cell(1,3).Value="Importe";ws.Cell(2,1).Value=new DateTime(2026,4,3,22,0,0);ws.Cell(2,2).Value="A";ws.Cell(2,3).Value=10;wb.SaveAs(excel);}
                var movs=ImportacionService.ImportarArchivo(excel,perfil);
                Program.Check((object)movs.Single().Fecha is DateTime dt && dt==new DateTime(2026,4,3),"Excel no conserva día tipado");
                Program.Check((object)ArchivoExternoReader.Leer(excel).Single().Fecha is DateTime dt2 && dt2==new DateTime(2026,4,3),"Excel externo no conserva día");
            } finally {if(File.Exists(excel))File.Delete(excel);}
        } finally { File.Delete(path); }
        Program.Check(db.Cn.ExecuteScalar<int>("SELECT COUNT(*) FROM sys.columns WHERE object_id IN (OBJECT_ID('bancos.MovimientosArchivo'),OBJECT_ID('bancos.ConciliacionItemsExternos')) AND name='Fecha' AND TYPE_NAME(system_type_id)='date' AND is_nullable=0")==2,"Esquema nuevo no tipado");
        Console.WriteLine("OK: fechas de CSV/Excel, filas inválidas y persistencia atómica");
    }
    static void Compatibilidad()
    {
        foreach(bool anterior in new[]{true,false})
        {
            using var db=new SqlFixture();
            if(anterior) { using var tx=db.Cn.BeginTransaction();db.Cn.Execute(File.ReadAllText("Tools/FechasBancarias.Tests/EsquemaAnterior.sql"),transaction:tx);tx.Commit(); }
            int objetos=db.Cn.ExecuteScalar<int>("SELECT COUNT(*) FROM sys.objects WHERE is_ms_shipped=0");
            var info=new System.Diagnostics.ProcessStartInfo(Path.Combine(AppContext.BaseDirectory,"FechasBancarias.Tests.exe"),"--inicializar") {UseShellExecute=false,CreateNoWindow=true,RedirectStandardError=true,RedirectStandardOutput=true};
            info.Environment["FECHAS_TEST_DB"]=db.Cn.ConnectionString;
            using var proceso=System.Diagnostics.Process.Start(info);
            string error=proceso.StandardError.ReadToEnd();proceso.StandardOutput.ReadToEnd();proceso.WaitForExit();
            if(anterior)
            {
                // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - El mensaje del hijo llega en la página de códigos de la consola, así que "migración" cambia según la máquina; se compara sin la vocal acentuada para que la prueba no falle por un tema de codificación.
                Program.Check(proceso.ExitCode!=0 && error.Contains("requiere la migraci") && error.Contains("n de fechas"),"Host alternativo no detectó schema anterior. Código "+proceso.ExitCode+": "+error.Split('\n')[0]);
                Program.Check(objetos==db.Cn.ExecuteScalar<int>("SELECT COUNT(*) FROM sys.objects WHERE is_ms_shipped=0"),"Inicializador antiguo ejecutó DDL lateral");
            }
            else Program.Check(proceso.ExitCode==0 && db.Cn.ExecuteScalar<int>("SELECT COUNT(*) FROM sys.columns WHERE object_id IN (OBJECT_ID('bancos.MovimientosArchivo'),OBJECT_ID('bancos.ConciliacionItemsExternos')) AND name='Fecha' AND TYPE_NAME(system_type_id)='date' AND is_nullable=0")==2,"Host alternativo no auto-inicializa fechas tipadas");
        }
    }
}
