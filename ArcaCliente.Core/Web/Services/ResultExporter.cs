// Fecha: 05/10/2026 - TAREA: 00066 - Linea: 2 - Reutiliza el exportador WinForms para conservar columnas, colores, estilos y resumen sin una segunda plantilla.
using ArcaCliente.Models;
using ArcaCliente.Services;
using ClosedXML.Excel;
namespace ArcaCliente.Web.Services;
public static class ResultExporter
{
    public static string Label(EstadoConciliacion status,string source)=>status switch{EstadoConciliacion.Conciliado=>"Conciliado",EstadoConciliacion.DiferenciaImporte=>"Diferencia de importe",EstadoConciliacion.SoloSistema=>"Solo sistema",_=>"Solo "+source.ToLowerInvariant()};
    public static byte[] Export(ReconciliationResult result)
    {
        // El contrato del exportador compartido recibe una ruta. Cada descarga usa un temporal propio y lo elimina incluso si falla.
        var path=Path.Combine(Path.GetTempPath(),"conciliacion-"+Guid.NewGuid().ToString("N")+".xlsx");
        try {
            ConciliacionExcelExporter.Exportar(result.Items,path);
            if(result.Source=="ARCA")return File.ReadAllBytes(path);
            using var book=new XLWorkbook(path);var sheet=book.Worksheet(1);
            // El circuito de proveedores conserva la misma planilla; sólo cambia el nombre de la fuente.
            sheet.Cell(1,1).Value=sheet.Cell(1,1).GetString().Replace("ARCA",result.Source);
            sheet.Cell(2,12).Value="Total "+result.Source;
            for(int i=0;i<result.Items.Count;i++)
                if(result.Items[i].Estado==EstadoConciliacion.SoloARCA)
                    sheet.Cell(i+3,1).Value=sheet.Cell(i+3,1).GetString().Replace("ARCA",result.Source.ToLowerInvariant());
            foreach(var row in sheet.RowsUsed().Where(r=>r.RowNumber()>result.Items.Count+2))
                if(row.Cell(1).GetString()=="Solo ARCA")row.Cell(1).Value="Solo "+result.Source.ToLowerInvariant();
            using var stream=new MemoryStream();book.SaveAs(stream);return stream.ToArray();
        }finally{if(File.Exists(path))File.Delete(path);}
    }
}
