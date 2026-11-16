// Fecha: 12/11/2026 - TAREA: 00180 - Linea: 2 - Parser DEVENGAMIENTOS con planillas chicas armadas en el test.
using AsientosEstudio.Core;

static class ParserDevengamientosTests
{
    static object?[] F(params object?[] c) => c;
    static readonly object?[] Titulos = F("Código de cuenta", "Nombre de la cuenta", "Debe", "Haber");

    static IReadOnlyList<HojaLeida> Leer(params HojaCruda[] hojas) =>
        new ParserDevengamientos().Leer(new LibroCrudo { Nombre = "d.xlsx", HashExcel = new string('0', 64), Hojas = hojas });

    public static void Run(Action<string, Action> test)
    {
        test("Devengamientos: el concepto es el texto del encabezado y el mes sale del nombre de la hoja", () =>
        {
            var a = Leer(ParserBancosTests.Hoja("05-2026", F("DEVENGAMIENTOS 05-2026"), F(), F("IVA - Asiento principal"), Titulos,
                F("2.01.04.004", "IVA 21", 965356929.73m, 0), F("2.01.04.003", "A SALDO IVA A PAGAR", 0, 965356929.73m), F("TOTAL", null, 965356929.73m, 965356929.73m)))
                .Single().Asientos.Single();
            Afirmar.Igual("IVA - Asiento principal", a.Concepto); Afirmar.Igual("IVA - Asiento principal", a.Titulo);
            Afirmar.Igual(new Mes(2026, 5), a.Mes!.Value); Afirmar.Igual("05-2026", a.Hoja);
            Afirmar.Igual(EstadoAsiento.Registrable, a.Estado); Afirmar.Igual(2, a.Lineas.Count);
            Afirmar.Igual(5, a.Lineas[0].Fila); Afirmar.Igual(0m, a.Lineas[0].Haber);
        });
        test("Devengamientos: el título de la hoja no es un asiento y la fila TOTAL no es una línea", () =>
        {
            var a = Leer(ParserBancosTests.Hoja("06-2026", F("DEVENGAMIENTOS 06-2026"), F("IIBB - Retenciones"), Titulos,
                F("1.01.04.015", "RETENCIONES IIBB", 100, 0), F("1.01.03.007", "A DEUDORES", 0, 100), F("TOTAL", null, 100, 100))).Single().Asientos;
            Afirmar.Igual(1, a.Count); Afirmar.Igual(2, a[0].Lineas.Count);
        });
        test("Devengamientos: «Codigo» sin tilde en la fila de títulos también abre el asiento", () =>
        {
            var a = Leer(ParserBancosTests.Hoja("05-2026", F("DREI - Devengamiento"), F("Codigo", "Nombre de la cuenta", "Debe", "Haber"),
                F("4.01.04.012", "Drei ", 10, 0), F("2.01.04.022", "a Drei a pagar", 0, 10), F("TOTAL", null, 10, 10))).Single().Asientos;
            Afirmar.Igual(1, a.Count); Afirmar.Igual("DREI - Devengamiento", a[0].Concepto);
        });
        test("Devengamientos: un asiento sin líneas de cuenta queda omitido por vacío y no se confunde con el siguiente (A8)", () =>
        {
            var a = Leer(ParserBancosTests.Hoja("05-2026",
                F("Ret. de 3eros. - 1era quincena - SUSS"), Titulos, F("TOTAL", null, 0, 0),
                F(), F("Ret. de 3eros. - 1era quincena - Ganancias"), Titulos,
                F("2.01.01.001", "PROVEEDORES", 28833498, 0), F("2.01.04.008", "A AFIP RET GANANCIAS A 3ROS", 0, 28833498), F("TOTAL", null, 28833498, 28833498))).Single().Asientos;
            Afirmar.Igual(2, a.Count);
            Afirmar.Igual(EstadoAsiento.VacioOmitido, a[0].Estado); Afirmar.Igual(0, a[0].Lineas.Count);
            Afirmar.Igual(EstadoAsiento.Registrable, a[1].Estado); Afirmar.Igual(2, a[1].Lineas.Count);
        });
        test("Devengamientos: un asiento que no cerró con TOTAL termina cuando empieza el siguiente", () =>
        {
            var a = Leer(ParserBancosTests.Hoja("05-2026", F("A"), Titulos, F("1.1", "X", 5, 0), F("1.2", "Y", 0, 5),
                F(), F("B"), Titulos, F("1.1", "X", 7, 0), F("1.2", "Y", 0, 7), F("TOTAL", null, 7, 7))).Single().Asientos;
            Afirmar.Igual(2, a.Count); Afirmar.Igual(2, a[0].Lineas.Count); Afirmar.Igual("B", a[1].Concepto);
        });
        test("Devengamientos: un TOTAL sin importes cargados no se toma por el encabezado del asiento que sigue", () =>
        {
            var a = Leer(ParserBancosTests.Hoja("05-2026", F("A"), Titulos, F("1.1", "X", 5, 0), F("1.2", "Y", 0, 5), F("TOTAL"),
                F(), F("B"), Titulos, F("1.1", "X", 7, 0), F("1.2", "Y", 0, 7), F("TOTAL"))).Single().Asientos;
            Afirmar.Igual(2, a.Count); Afirmar.Igual("A", a[0].Concepto); Afirmar.Igual("B", a[1].Concepto);
        });
        test("Devengamientos: el descuadre de origen de 0,01 se lee tal cual, sin ajustar (A6 lo decide el validador)", () =>
        {
            var a = Leer(ParserBancosTests.Hoja("05-2026", F("IVA - Asiento principal"), Titulos,
                F("2.01.04.004", "IVA 21", 1169596185.03m, 0), F("2.01.04.003", "A SALDO IVA A PAGAR", 0, 1169596185.04m), F("TOTAL"))).Single().Asientos.Single();
            Afirmar.Igual(1169596185.03m, a.TotalDebe); Afirmar.Igual(1169596185.04m, a.TotalHaber);
        });
        test("Devengamientos: una hoja que no se llama como un mes deja sus asientos sin mes", () =>
        {
            var a = Leer(ParserBancosTests.Hoja("Resumen", F("A"), Titulos, F("1.1", "X", 5, 0))).Single().Asientos.Single();
            Afirmar.Verdad(a.Mes is null, "sin mes");
        });
    }
}
