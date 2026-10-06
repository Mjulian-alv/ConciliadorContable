// Fecha: 05/10/2026 - TAREA: 00066 - Linea: 2 - Compara el archivo web contra el exportador real del escritorio, no contra una copia de su formato.
using ArcaCliente.Models;
using ArcaCliente.Services;
using ArcaCliente.Web.Models;
using ArcaCliente.Web.Services;
using Conciliador.Web.Services;
using ClosedXML.Excel;
using System.Text.RegularExpressions;
static class ExportCompatibilityTests
{
    static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    public static void Run(Action<string,Action> test)
    {
        ReconciliationResult Example()=>ReconciliationRunner.Run(new(),ReconciliationRunner.Arca(DemoFiles.Arca),ProviderImporter.Preview(DemoFiles.System,new FileFormat()),null);
        test("Excel ARCA web conserva todas las columnas, valores y estilos de WinForms",()=>{
            var result=Example();var path=Path.Combine(Path.GetTempPath(),"arca-export-reference-"+Guid.NewGuid()+".xlsx");
            try{
                ConciliacionExcelExporter.Exportar(result.Items,path);
                using var expected=new XLWorkbook(path);
                using var actual=new XLWorkbook(new MemoryStream(ResultExporter.Export(result)));
                Check(actual.Worksheets.Count==expected.Worksheets.Count,"Cantidad de hojas distinta de WinForms");
                var e=expected.Worksheet(1);var a=actual.Worksheet(1);
                Check(a.Name==e.Name&&a.LastRowUsed()!.RowNumber()==e.LastRowUsed()!.RowNumber()&&a.LastColumnUsed()!.ColumnNumber()==14,"Estructura distinta de WinForms");
                for(int row=1;row<=e.LastRowUsed()!.RowNumber();row++)for(int col=1;col<=14;col++){
                    var ec=e.Cell(row,col);var ac=a.Cell(row,col);
                    var ev=ec.GetString();var av=ac.GetString();
                    if(row==1&&col==1){ev=Regex.Replace(ev,@"\d{2}/\d{2}/\d{4} \d{2}:\d{2}","");av=Regex.Replace(av,@"\d{2}/\d{2}/\d{4} \d{2}:\d{2}","");}
                    Check(ev==av&&ec.DataType==ac.DataType,$"Valor/tipo distinto en {row},{col}");
                    Check(ec.Style.Equals(ac.Style),$"Estilo distinto en {row},{col}");
                }
                for(int col=1;col<=14;col++)Check(e.Column(col).Width==a.Column(col).Width,"Ancho distinto");
                Check(e.SheetView.SplitRow==a.SheetView.SplitRow&&e.MergedRanges.Count()==a.MergedRanges.Count(),"Encabezado congelado o título distinto");
            }finally{if(File.Exists(path))File.Delete(path);}
        });
        test("Excel proveedor conserva layout y adapta las etiquetas de la fuente",()=>{
            var result=Example() with {Source="Proveedor"};
            using var book=new XLWorkbook(new MemoryStream(ResultExporter.Export(result)));var sheet=book.Worksheet(1);
            Check(book.Worksheets.Count==1&&sheet.LastColumnUsed()!.ColumnNumber()==14,"No conserva layout");
            Check(sheet.Cell(2,12).GetString()=="Total Proveedor","Encabezado incorrecto para proveedor");
            Check(sheet.Cell(1,1).GetString().Contains("Proveedor"),"Título incorrecto");
            var index=result.Items.FindIndex(x=>x.Estado==EstadoConciliacion.SoloARCA);
            Check(sheet.Cell(index+3,1).GetString().Contains("Solo proveedor"),"Estado incorrecto");
            Check(sheet.Cell(index+3,11).GetString()==result.Items[index].NombreProveedor,"Falta nombre del proveedor");
            Check(sheet.Cell(index+3,4).GetString()==result.Items[index].FalloDirectivas,"Falta detalle de intentos");
        });
    }
}
