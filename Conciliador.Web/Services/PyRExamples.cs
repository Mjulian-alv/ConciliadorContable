// Fecha: 06/10/2026 - TAREA: 00072 - Linea: 3 - Datos ficticios reproducibles para revisar las cinco situaciones del motor.
using ArcaCliente.Models;
using ArcaCliente.Web.Models;
using ClosedXML.Excel;
namespace Conciliador.Web.Services;
public static class PyRExamples
{
    public static CargaArcaPyR Arca()
    {
        var result=new CargaArcaPyR();result.ArchivosLeidos.Add("ARCA-ejemplo.xls");
        foreach(var (number,amount) in new[]{("100",100m),("101",220m),("102",300m),("103",0m)})
            result.Registros.Add(new(){Impuesto=ImpuestoPyR.Iva,Operacion=TipoOperacionPyR.Percepcion,Fecha=new(2026,7,10),Cuit="30123456781",Denominacion="Proveedor de ejemplo",Numero=number,Importe=amount,ArchivoOrigen="ARCA-ejemplo.xls"});
        result.Registros.Add(new(){Impuesto=ImpuestoPyR.Iibb,Operacion=TipoOperacionPyR.Retencion,Numero="200",Importe=50});
        return result;
    }
    public static UploadedDocument Major()
    {
        using var book=new XLWorkbook();var sheet=book.AddWorksheet("Mayor");
        var headers=new[]{"fecha","concepto","debe","haber","operador"};for(var i=0;i<headers.Length;i++)sheet.Cell(1,i+1).Value=headers[i];
        var rows=new[]{("SEGUN FACTURA A 100 de Proveedor de ejemplo",100m,0m),("SEGUN FACTURA A 101 de Proveedor de ejemplo",200m,0m),
            ("SEGUN FACTURA A 999 de Otro proveedor",80m,0m),("SEGUN FACTURA A 888 de Anulado",50m,0m),("POR ANULACION FACTURA A 888",0m,50m),
            ("MINUTA FINANCIERA",10m,0m)};
        for(var i=0;i<rows.Length;i++){sheet.Cell(i+2,1).Value=new DateTime(2026,7,10);sheet.Cell(i+2,2).Value=rows[i].Item1;sheet.Cell(i+2,3).Value=rows[i].Item2;sheet.Cell(i+2,4).Value=rows[i].Item3;sheet.Cell(i+2,5).Value="Operador ejemplo";}
        using var ms=new MemoryStream();book.SaveAs(ms);return new("Mayor-ejemplo.xlsx",ms.ToArray());
    }
}
