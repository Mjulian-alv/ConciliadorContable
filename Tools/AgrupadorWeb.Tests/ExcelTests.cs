// Fecha: 14/10/2026 - TAREA: 00097 - Linea: 3 - La web descarga el libro como bytes: debe ser el mismo que genera el escritorio, con fechas e importes reales.
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;
using ClosedXML.Excel;
static class ExcelTests
{
    static void Check(bool ok, string mensaje = "Condición falsa") { if (!ok) throw new Exception(mensaje); }
    static List<MovimientoConsulta> Datos() => new()
    {
        new() { Fecha = new DateTime(2026, 9, 1), ConceptoOriginal = "PAGO CON TRANSF-BANE", DescripcionOriginal = "PAGO CON TRANSF-BANE", Debitos = 0m, Creditos = 58049.41m,
                ConceptoEstandar = "TRANSFERENCIAS RECIBIDAS", ConceptoFinal = "TRANSFERENCIAS RECIBIDAS", CuentaFinal = "00123", NombrePerfil = "BNA CA" },
        new() { Fecha = new DateTime(2026, 9, 2), ConceptoOriginal = "DB.TRANSF.INTER-LINK", DescripcionOriginal = "DB.TRANSF.INTER-LINK", Debitos = 5000000m, Creditos = null,
                ConceptoEstandar = "TRANSF ANTONIAZZI", ConceptoFinal = "TRANSF ANTONIAZZI", CuentaFinal = "110105", NombrePerfil = "BNA CA" },
    };
    public static void Run(Action<string, Action> test)
    {
        test("Excel: la consulta se descarga como bytes con fechas reales, importes numéricos y la cuenta como texto", () =>
        {
            var bytes = ConsultaMovimientosService.ExportarBytes(Datos());
            Check(bytes.Length > 1000 && bytes[0] == 0x50 && bytes[1] == 0x4B, "No es un .xlsx (zip)");
            using var libro = new XLWorkbook(new MemoryStream(bytes));
            var hoja = libro.Worksheet("Movimientos");
            Check(hoja.Cell(1, 1).GetString() == "Consulta de movimientos por conceptos finales", "Título");
            Check(hoja.Cell(2, 4).GetString() == "Débitos" && hoja.Cell(2, 9).GetString() == "Perfil", "Encabezados");
            Check(hoja.Cell(3, 1).DataType == XLDataType.DateTime && hoja.Cell(3, 1).GetDateTime() == new DateTime(2026, 9, 1) && hoja.Cell(3, 1).Style.DateFormat.Format == "dd/mm/yyyy", "Fecha real con formato");
            Check(hoja.Cell(3, 5).DataType == XLDataType.Number && hoja.Cell(3, 5).GetDouble() == 58049.41 && hoja.Cell(3, 5).Style.NumberFormat.Format == "#,##0.00", "Crédito numérico con miles y decimales");
            Check(hoja.Cell(4, 4).GetDouble() == 5000000 && hoja.Cell(4, 5).IsEmpty(), "Débito numérico y crédito nulo en blanco");
            Check(hoja.Cell(3, 8).DataType == XLDataType.Text && hoja.Cell(3, 8).GetString() == "00123", "Cuenta con ceros iniciales como texto");
        });
        test("Excel: los bytes y el archivo en disco contienen lo mismo", () =>
        {
            var ruta = Path.Combine(Path.GetTempPath(), "consulta-" + Guid.NewGuid() + ".xlsx");
            try
            {
                ConsultaMovimientosService.Exportar(Datos(), ruta);
                using var a = new XLWorkbook(ruta); using var b = new XLWorkbook(new MemoryStream(ConsultaMovimientosService.ExportarBytes(Datos())));
                var ha = a.Worksheet(1); var hb = b.Worksheet(1);
                Check(ha.LastRowUsed().RowNumber() == hb.LastRowUsed().RowNumber() && ha.LastColumnUsed().ColumnNumber() == hb.LastColumnUsed().ColumnNumber(), "Dimensiones");
                for (int f = 1; f <= ha.LastRowUsed().RowNumber(); f++)
                    for (int c = 1; c <= ha.LastColumnUsed().ColumnNumber(); c++)
                        Check(ha.Cell(f, c).Value.ToString() == hb.Cell(f, c).Value.ToString(), $"Celda {f},{c}");
            }
            finally { if (File.Exists(ruta)) File.Delete(ruta); }
        });
        test("Excel: no se exporta una consulta vacía", () =>
        {
            try { ConsultaMovimientosService.ExportarBytes(new List<MovimientoConsulta>()); throw new Exception("Aceptó una consulta vacía"); }
            catch (ArgumentException) { }
        });
        test("Consulta: validación de filtros (rango invertido, sin conceptos, demasiados)", () =>
        {
            void Rechaza(Action a) { try { a(); throw new Exception("Aceptó un filtro inválido"); } catch (ArgumentException) { } }
            Rechaza(() => ConsultaMovimientosService.ValidarFiltros(new DateTime(2026, 9, 30), new DateTime(2026, 9, 1), new[] { "A" }));
            Rechaza(() => ConsultaMovimientosService.ValidarFiltros(new DateTime(2026, 9, 1), new DateTime(2026, 9, 30), Array.Empty<string>()));
            Rechaza(() => ConsultaMovimientosService.ValidarFiltros(new DateTime(2026, 9, 1), new DateTime(2026, 9, 30), Enumerable.Range(0, 2001).Select(i => "C" + i)));
            Check(ConsultaMovimientosService.ValidarFiltros(new DateTime(2026, 9, 1), new DateTime(2026, 9, 1), new[] { "A", "A", " " }).Length == 1, "Día único, repetidos y vacíos");
        });
    }
}
