// Fecha: 12/11/2026 - TAREA: 00180 - Linea: 1 - Lectura del .xlsx: valores, fórmulas sin valor guardado, errores y conversión a decimal.
using AsientosEstudio.Core;
using ClosedXML.Excel;

static class LectorLibroTests
{
    static LibroCrudo Leer(Action<IXLWorkbook> armar)
    {
        using var wb = new XLWorkbook();
        armar(wb);
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        return LectorLibro.Leer(ms, "prueba.xlsx");
    }

    public static void Run(Action<string, Action> test)
    {
        test("Lector: texto, números y hojas en el orden del Excel", () =>
        {
            var l = Leer(wb =>
            {
                var a = wb.AddWorksheet("BBVA"); a.Cell("A1").Value = "BANCO BBVA"; a.Cell("C8").Value = 1398900661.6;
                wb.AddWorksheet("MUNICIPAL").Cell("B2").Value = "x";
            });
            Afirmar.Igual(2, l.Hojas.Count);
            Afirmar.Igual("BBVA", l.Hojas[0].Nombre); Afirmar.Igual("MUNICIPAL", l.Hojas[1].Nombre);
            Afirmar.Igual("BANCO BBVA", l.Hojas[0].Texto(1, 1));
            Afirmar.Igual(1398900661.6m, l.Hojas[0].Importe(8, 3));
            Afirmar.Igual(8, l.Hojas[0].UltimaFila);
            Afirmar.Igual(0m, l.Hojas[0].Importe(9, 3)); // la celda vacía cuenta como 0
        });
        test("Lector: no pierde decimales (importes de 4 a 6 decimales como los de VENTAS)", () =>
        {
            var l = Leer(wb =>
            {
                var h = wb.AddWorksheet("V");
                h.Cell("C1").Value = 10394283.1476; h.Cell("C2").Value = 21103544.5724; h.Cell("C3").Value = 8452100.004901; h.Cell("C4").Value = 0.1;
            });
            Afirmar.Igual(10394283.1476m, l.Hojas[0].Importe(1, 3));
            Afirmar.Igual(21103544.5724m, l.Hojas[0].Importe(2, 3));
            Afirmar.Igual(8452100.004901m, l.Hojas[0].Importe(3, 3));
            Afirmar.Igual(0.1m, l.Hojas[0].Importe(4, 3));
        });
        test("Lector: la conversión no depende de la cultura (es-AR y en-US dan lo mismo)", () =>
        {
            var antes = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                foreach (var c in new[] { "es-AR", "en-US", "de-DE" })
                {
                    System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(c);
                    Afirmar.Igual(1234567.89m, LectorLibro.ADecimal(1234567.89), c);
                    Afirmar.Igual("1234567.89", new HojaCruda("h").Poner(1, 1, 1234567.89m).Texto(1, 1), c);
                }
            }
            finally { System.Globalization.CultureInfo.CurrentCulture = antes; }
        });
        test("Lector: una fórmula sin valor calculado guardado se marca, no se lee como cero (A11)", () =>
        {
            // Un libro armado en memoria no trae valores guardados: es justo el caso que hay que detectar.
            var l = Leer(wb => { var h = wb.AddWorksheet("COMPRAS"); h.Cell("H14").Value = 100; h.Cell("D118").FormulaA1 = "+H14"; });
            var c = l.Hojas[0].Celda(118, 4) ?? throw new Exception("la celda con fórmula no se leyó");
            Afirmar.Verdad(c.TieneFormula && c.SinValorCalculado, "debería estar marcada como fórmula sin valor");
            Afirmar.Igual(ProblemaImporte.FormulaSinValor, l.Hojas[0].Problema(118, 4));
            Afirmar.Verdad(l.Hojas[0].TieneImporte(118, 4), "hay algo escrito en la celda");
        });
        test("Lector: el hash es el SHA-256 de los bytes y cambia si cambia el archivo", () =>
        {
            var a = Leer(wb => wb.AddWorksheet("A").Cell("A1").Value = "x");
            var b = Leer(wb => wb.AddWorksheet("A").Cell("A1").Value = "y");
            Afirmar.Igual(64, a.HashExcel.Length);
            Afirmar.Verdad(a.HashExcel != b.HashExcel && a.HashExcel == a.HashExcel.ToLowerInvariant(), "hash");
        });
        test("Cruda: un texto en una celda de importe se informa y cuenta como 0", () =>
        {
            var h = new HojaCruda("h").Poner(1, 3, "1.234,56");
            Afirmar.Igual(0m, h.Importe(1, 3));
            Afirmar.Igual(ProblemaImporte.TextoNoNumerico, h.Problema(1, 3));
            Afirmar.Igual(ProblemaImporte.Ninguno, h.Problema(2, 3));
        });
    }
}
