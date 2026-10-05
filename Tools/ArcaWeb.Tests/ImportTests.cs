// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 3 - Casos que impiden conciliar un lote incompleto o alterar importes originales.
using ArcaCliente.Web.Models;
using ArcaCliente.Web.Services;
using ClosedXML.Excel;
using System.Text;
static class ImportTests
{
    public static void Run(Action<string, Action> test)
    {
        var format = new FileFormat { Date="Fecha", Type="Tipo", Number="Documento", Amount="Importe", NumberMode=NumberMode.Separated, NumberSeparator="-" };
        var config = new ProviderConfiguration { Cuit="30123456781", Format=format, Types=new() { ["FA"]="001", ["NCA"]="003", ["REC"]="EXCLUIR" } };
        UploadedDocument Csv(string rows) => new("proveedor.csv", Encoding.UTF8.GetBytes("Tipo;Fecha;Documento;Importe\n"+rows));
        void Check(bool x) { if(!x) throw new Exception("Fallo de validación"); }
        test("CSV citado y exclusión sin campos fiscales", () => {
            var p=ProviderImporter.Preview(Csv("FA;05/09/2026;00005-00001234;\"1.000,00\"\nREC;;;\nNCA;05/09/2026;00005-00000081;(100,00)"),config);
            Check(p.Ready && p.Included.Count==2 && p.ExcludedCount==1 && p.Included[1].Total==100.00m && p.Rows[2].RawAmount=="(100,00)");
        });
        test("Fila inválida después de la muestra bloquea todo", () => {
            var p=ProviderImporter.Preview(Csv(string.Join('\n',Enumerable.Repeat("FA;05/09/2026;5-1234;100,00",201))+"\nFA;31/02/2026;5-9999;100,00"),config);
            Check(!p.Ready && p.Rows.Last().Errors.Count>0);
        });
        test("Tipo desconocido no se excluye automáticamente", () => {
            var p=ProviderImporter.Preview(Csv("PAGO;;;"),config); Check(!p.Ready && p.UnknownTypes.Contains("PAGO") && p.ExcludedCount==0);
        });
        test("Importe inválido y agrupación inválida no se convierten en cero", () => {
            Check(!ProviderImporter.Preview(Csv("FA;05/09/2026;5-1234;abc"),config).Ready);
            Check(!ProviderImporter.Preview(Csv("FA;05/09/2026;5-1234;1.00,00"),config).Ready);
        });
        test("CUIT normaliza y deduplica; lista inválida no se acepta", () => {
            var r=CuitListImporter.Parse(new("cuits.csv",Encoding.UTF8.GetBytes("CUIT\n30-12345678-1\n30123456781")));
            Check(r.Count==1 && r[0]=="30123456781");
            try { CuitListImporter.Parse(new("cuits.txt",Encoding.UTF8.GetBytes("30123456781\nno"))); throw new Exception("Aceptó lista inválida"); } catch(ImportException) {}
        });
        test("XLSX conserva filas y celdas numéricas", () => {
            using var book=new XLWorkbook(); var ws=book.AddWorksheet("Pendientes");
            ws.Cell(1,1).Value="Tipo";ws.Cell(1,2).Value="Fecha";ws.Cell(1,3).Value="Documento";ws.Cell(1,4).Value="Importe";
            ws.Cell(2,1).Value="FA";ws.Cell(2,2).Value=new DateTime(2026,9,5);ws.Cell(2,3).Value="5-1234";ws.Cell(2,4).Value=1000.5;
            using var stream=new MemoryStream();book.SaveAs(stream);
            var p=ProviderImporter.Preview(new("ejemplo.xlsx",stream.ToArray()),config);Check(p.Ready && p.Included.Single().Total==1000.50m);
        });
        test("Cabeceras duplicadas y CSV roto se bloquean", () => {
            try { TabularReader.Read(new("m.csv",Encoding.UTF8.GetBytes("Tipo;Tipo\nFA;NC")),format);throw new Exception("Aceptó duplicadas"); } catch(ImportException) {}
            try { TabularReader.Read(Csv("\"FA;sin cierre"),format);throw new Exception("Aceptó CSV roto"); } catch(ImportException) {}
        });
    }
}
