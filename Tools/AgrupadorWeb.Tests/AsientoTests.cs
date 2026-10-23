// Fecha: 23/10/2026 - TAREA: 00126 - Linea: 1 - La hoja Asiento del consolidado: saldos por CuentaFinal, lo sin cuenta por concepto y el reparto en Debe/Haber.
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;
using ClosedXML.Excel;
static class AsientoTests
{
    static void Check(bool ok, string mensaje = "Condición falsa") { if (!ok) throw new Exception(mensaje); }
    static MovimientoProcesado Mov(string concepto, string cuenta, string nombreCuenta, decimal debitos, decimal creditos) =>
        new() { ConceptoFinal = concepto, ConceptoEstandar = concepto, CuentaFinal = cuenta, NombreCuenta = nombreCuenta, Debitos = debitos, Creditos = creditos, Fecha = new DateTime(2026, 9, 1) };
    static List<MovimientoProcesado> Datos() => new()
    {
        Mov("COMISIONES", "520101", "Comisiones bancarias", 100m, 0m),
        Mov("COMISIONES", "520101", "Comisiones bancarias", 55.5m, 0m),
        Mov("MERCADO PAGO", "410101", "Ventas", 0m, 930m),
        Mov("IVA 21", null, null, 32.65m, 0m),
        Mov("DEPOSITO EFECTIVO", "", null, 0m, 400m),
        Mov("AJUSTE", "999999", "Neutra", 10m, 10m),
        Mov(ConceptosBancarios.PendienteHomologar, "111111", "No entra", 5m, 0m),
    };
    public static void Run(Action<string, Action> test)
    {
        test("Asiento: una línea por cuenta con el saldo en Debe o Haber, y lo sin cuenta por concepto", () =>
        {
            var a = ConsolidadoExporter.CalcularAsiento(Datos());
            Check(a.Count == 4, "Líneas (el pendiente y el saldo cero no entran): " + a.Count);
            Check(a[0].Cuenta == "410101" && a[0].Descripcion == "Ventas" && a[0].Haber == 930m && a[0].Debe == 0m, "Cuenta con crédito va al Haber");
            Check(a[1].Cuenta == "520101" && a[1].Descripcion == "Comisiones bancarias" && a[1].Debe == 155.5m && a[1].Haber == 0m, "Cuenta con débitos suma y va al Debe");
            Check(a[2].Cuenta == "" && a[2].Descripcion == "DEPOSITO EFECTIVO" && a[2].Haber == 400m, "Sin cuenta: concepto como descripción");
            Check(a[3].Cuenta == "" && a[3].Descripcion == "IVA 21" && a[3].Debe == 32.65m, "Sin cuenta nula: concepto como descripción");
        });
        test("Asiento: el neto del asiento coincide con el saldo del consolidado", () =>
        {
            var datos = Datos();
            var a = ConsolidadoExporter.CalcularAsiento(datos);
            decimal neto = a.Sum(l => l.Haber - l.Debe);
            Check(neto == ConsolidadoExporter.Calcular(datos).Sum(l => l.Saldo), "Neto " + neto);
        });
        test("Asiento: el Excel trae las hojas Consolidado y Asiento con totales y diferencia", () =>
        {
            var datos = Datos();
            var bytes = ConsolidadoExporter.ExportarBytes(ConsolidadoExporter.Calcular(datos), ConsolidadoExporter.CalcularAsiento(datos), "Prueba");
            using var libro = new XLWorkbook(new MemoryStream(bytes));
            Check(libro.Worksheets.Count == 2 && libro.Worksheets.Contains("Consolidado") && libro.Worksheets.Contains("Asiento"), "Hojas");
            var h = libro.Worksheet("Asiento");
            Check(h.Cell(2, 1).GetString() == "Cuenta" && h.Cell(2, 4).GetString() == "Haber", "Encabezados");
            Check(h.Cell(3, 1).GetString() == "410101" && h.Cell(3, 4).DataType == XLDataType.Number && h.Cell(3, 4).GetDouble() == 930, "Primera línea numérica");
            Check(h.Cell(7, 2).GetString() == "Total" && h.Cell(7, 3).GetDouble() == 188.15 && h.Cell(7, 4).GetDouble() == 1330, "Totales");
            Check(h.Cell(8, 2).GetString().StartsWith("Diferencia") && h.Cell(8, 4).GetDouble() == 1141.85, "Diferencia");
            Check(libro.Worksheet("Consolidado").Cell(2, 1).GetString() == "Concepto Final", "La hoja Consolidado no cambia");
        });
        test("Asiento: sin movimientos sólo quedan los totales en cero", () =>
        {
            var bytes = ConsolidadoExporter.ExportarBytes(new List<LineaConsolidado>(), ConsolidadoExporter.CalcularAsiento(new List<MovimientoProcesado>()), "Vacío");
            using var libro = new XLWorkbook(new MemoryStream(bytes));
            Check(libro.Worksheet("Asiento").Cell(3, 2).GetString() == "Total" && libro.Worksheet("Asiento").Cell(3, 3).GetDouble() == 0, "Total en cero");
        });
    }
}
