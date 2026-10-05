// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 2 - El Excel refleja la fuente y el universo completos, independientemente del filtro visual.
using ArcaCliente.Models;
using ClosedXML.Excel;
using System.Globalization;
namespace ArcaCliente.Web.Services;
public static class ResultExporter
{
    public static string Label(EstadoConciliacion status,string source)=>status switch{EstadoConciliacion.Conciliado=>"Conciliado",EstadoConciliacion.DiferenciaImporte=>"Diferencia de importe",EstadoConciliacion.SoloSistema=>"Solo sistema",_=>"Solo "+source.ToLowerInvariant()};
    public static byte[] Export(ReconciliationResult result)
    {
        using var book=new XLWorkbook();var ws=book.AddWorksheet("Comprobantes");
        string[] headers={"Estado","Fecha","CUIT","Tipo","Punto de venta","Número",result.Source,"Sistema","Diferencia","Directiva","Detalle"};
        for(int i=0;i<headers.Length;i++)ws.Cell(1,i+1).Value=headers[i];
        int row=2;foreach(var x in result.Items){
            string[] values={Label(x.Estado,result.Source),x.Fecha,x.CuitProveedor,x.TipoComprobante,x.PuntoVenta,x.Numero,x.TotalARCA,x.TotalSistema,x.Diferencia,x.DescripcionDirectiva,x.DetalleMatcheo};
            for(int i=0;i<values.Length;i++) {
                if(i is >=6 and <=8 && decimal.TryParse(values[i],NumberStyles.Number,CultureInfo.GetCultureInfo("es-AR"),out var n)) {ws.Cell(row,i+1).Value=n;ws.Cell(row,i+1).Style.NumberFormat.Format="#,##0.00";}
                else ws.Cell(row,i+1).Value=values[i]??"";
            }row++;
        }
        ws.Range(1,1,Math.Max(2,row-1),headers.Length).CreateTable();ws.SheetView.FreezeRows(1);ws.Columns().AdjustToContents(1,Math.Min(row,100));
        var summary=book.AddWorksheet("Por CUIT");summary.Cell(1,1).Value="CUIT";summary.Cell(1,2).Value=result.Source;summary.Cell(1,3).Value="Sistema";summary.Cell(1,4).Value="Diferencia";
        row=2;foreach(var x in result.Summary){summary.Cell(row,1).Value=x.Cuit;summary.Cell(row,2).Value=x.External;summary.Cell(row,3).Value=x.System;summary.Cell(row,4).Value=x.Difference;row++;}
        summary.Columns(2,4).Style.NumberFormat.Format="#,##0.00";summary.Columns().AdjustToContents();
        using var stream=new MemoryStream();book.SaveAs(stream);return stream.ToArray();
    }
}
