// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - El formato visual no debe convertir fechas a texto ni alterar el orden.
namespace FechasBancarias.Tests;
using AgrupadorConceptos;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;
using ClosedXML.Excel;
using Telerik.WinControls.UI;
internal static class PresentacionTests
{
    public static void Run()
    {
        SqliteTests.Run();
        using(var form=new ConciliacionExternForm())
        foreach(string nombre in new[]{"dgvExtractoPendiente","dgvExtractoSinConcepto"})
        {
            var grid=(RadGridView)form.Controls.Find(nombre,true).Single(); grid.BindingContext=new BindingContext();
            grid.DataSource=new[]{new MovimientoProcesado{Fecha=new DateTime(2026,4,3),NombreBanco="Perfil prueba"}};
            Program.Check(grid.Columns.Count==9 && grid.Columns["Fecha"] is GridViewDateTimeColumn && grid.Columns["Fecha"].FormatString=="{0:dd/MM/yyyy}","Fecha o columnas del extracto perdidas");
            Program.Check(grid.Rows[0].Cells["Fecha"].Value is DateTime,"Fecha de grilla perdió el tipo");
        }
        using(var form=new ConciliacionInternaForm())
        {
            var grid=(RadGridView)form.Controls.Find("dgvDebitos",true).Single();grid.BindingContext=new BindingContext();
            grid.DataSource=new[]{new MovimientoProcesado{Fecha=new DateTime(2026,4,3)}};
            Program.Check(grid.Columns["Fecha"].FormatString=="{0:dd/MM/yyyy}","Grilla interna muestra fecha con hora o formato regional");
        }
        string path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".xlsx");
        try
        {
            ConciliacionExcelExporter.Exportar(new[]{
                new ConciliacionPar{IdSesion=1,IdItemExterno=1,FechaExterno=new(2026,4,3),FechaExtracto=new(2026,4,3),ImporteExterno=100,ImporteExtracto=60},
                new ConciliacionPar{IdSesion=1,IdItemExterno=1,FechaExterno=new(2026,4,3),FechaExtracto=new(2026,4,4),ImporteExterno=100,ImporteExtracto=40}},Array.Empty<ConciliacionItemExterno>(),Array.Empty<MovimientoProcesado>(),path);
            using var wb=new XLWorkbook(path);var ws=wb.Worksheet("Conciliados");
            Program.Check(ws.Cell(2,1).DataType==XLDataType.DateTime && ws.Cell(2,1).GetFormattedString()=="03/04/2026","Excel debe guardar fecha y aplicar formato");
            Program.Check(ws.Cell(3,1).IsEmpty() && ws.Cell(3,2).IsEmpty() && ws.Cell(2,4).GetDateTime()<ws.Cell(3,4).GetDateTime(),"Grupo duplicado u orden textual");
        }finally{File.Delete(path);}
        Console.WriteLine("OK: fechas tipadas en grillas y Excel, perfil y grupos 1:N");
    }
}
