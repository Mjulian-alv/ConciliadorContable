// Fecha: 14/10/2026 - TAREA: 00097 - Linea: 6 - Cuentas contables: el archivo llega del navegador como flujo; se prueba la lectura sin tocar disco ni SQL.
using System.Text;
using AgrupadorConceptos.Services;
using ClosedXML.Excel;
using Conciliador.Web.Services;
static class CuentasTests
{
    static void Check(bool ok, string mensaje = "Condición falsa") { if (!ok) throw new Exception(mensaje); }
    static MemoryStream Csv(string texto) => new(Encoding.UTF8.GetBytes(texto));
    static MemoryStream Xlsx(params string[][] filas)
    {
        using var libro = new XLWorkbook();
        var hoja = libro.AddWorksheet("Hoja1");
        for (int r = 0; r < filas.Length; r++) for (int c = 0; c < filas[r].Length; c++) hoja.Cell(r + 1, c + 1).Value = filas[r][c];
        var ms = new MemoryStream(); libro.SaveAs(ms); ms.Position = 0; return ms;
    }
    public static void Run(Action<string, Action> test)
    {
        const string cuentas = "Cuenta,Descripcion,Centro\n110105,TRANSFERENCIAS PROPIAS,\n410330,DEVOLUCIONES DE COBROS,VENTAS\n,fila sin cuenta,X\n520115,IMPUESTO DB/CR,ADMINISTRACION\n";
        test("Cuentas: encabezados y vista previa desde un flujo CSV", () =>
        {
            var h = ImportacionCuentasContablesService.LeerEncabezados(Csv(cuentas), ".csv");
            Check(h.SequenceEqual(new[] { "Cuenta", "Descripcion", "Centro" }), "Encabezados: " + string.Join("|", h));
            var vista = ImportacionCuentasContablesService.LeerVistaPrevia(Csv(cuentas), ".csv", 2);
            Check(vista.Count == 2 && vista[0][0] == "110105" && vista[1][2] == "VENTAS", "Vista previa");
        });
        test("Cuentas: lectura con y sin centro de costo; se saltean las filas sin cuenta", () =>
        {
            var con = ImportacionCuentasContablesService.Leer(Csv(cuentas), ".CSV", "Cuenta", "Descripcion", "Centro");
            Check(con.Count == 3, "Filas: " + con.Count);
            Check(con[1].Cuenta == "410330" && con[1].CentroCosto == "VENTAS" && con[0].CentroCosto == "");
            var sin = ImportacionCuentasContablesService.Leer(Csv(cuentas), ".csv", "Cuenta", "Descripcion", null);
            Check(sin.Count == 3 && sin.All(c => c.CentroCosto == null), "Sin centro de costo debe quedar nulo");
        });
        test("Cuentas: Excel .xlsx desde un flujo y columnas inexistentes rechazadas", () =>
        {
            var ms = Xlsx(new[] { "Cuenta", "Descripción" }, new[] { "365", "BANCO NACION CTA. CTE." });
            var h = ImportacionCuentasContablesService.LeerEncabezados(ms, ".xlsx");
            Check(h.SequenceEqual(new[] { "Cuenta", "Descripción" }));
            var c = ImportacionCuentasContablesService.Leer(Xlsx(new[] { "Cuenta", "Descripción" }, new[] { "365", "BANCO" }), ".xlsx", "Cuenta", "Descripción", null);
            Check(c.Count == 1 && c[0].Cuenta == "365" && c[0].Descripcion == "BANCO");
            try { ImportacionCuentasContablesService.Leer(Csv(cuentas), ".csv", "NoExiste", "Descripcion", null); throw new Exception("Aceptó una columna inexistente"); }
            catch (InvalidOperationException) { }
        });
        test("Archivos subidos: sólo .xls, .xlsx y .csv, hasta 25 MB, y no vacíos", () =>
        {
            Check(ArchivosWeb.Motivo("Plan de cuentas.XLSX", 1000) == null && ArchivosWeb.Motivo("a.csv", 1) == null && ArchivosWeb.Motivo("b.xls", 1) == null);
            Check(ArchivosWeb.Motivo("virus.exe", 1000) != null && ArchivosWeb.Motivo("sin-extension", 1000) != null);
            Check(ArchivosWeb.Motivo("grande.xlsx", ArchivosWeb.LimiteBytes + 1) != null && ArchivosWeb.Motivo("justo.xlsx", ArchivosWeb.LimiteBytes) == null);
            Check(ArchivosWeb.Motivo("vacio.csv", 0) != null, "Aceptó un archivo vacío");
        });
    }
}
