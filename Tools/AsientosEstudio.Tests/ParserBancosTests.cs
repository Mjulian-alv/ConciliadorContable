// Fecha: 12/11/2026 - TAREA: 00180 - Linea: 1 - Parser BANCOS con planillas chicas armadas en el test (sin archivos): reconocimiento por texto,
// asientos informativos, notas, fila de totales suelta, líneas sin código y concepto «MM-AAAA-HOJA».
using AsientosEstudio.Core;

static class ParserBancosTests
{
    /// <summary>Arma una hoja: cada fila es una lista de celdas desde la columna A; null deja la celda vacía.</summary>
    public static HojaCruda Hoja(string nombre, params object?[][] filas)
    {
        var h = new HojaCruda(nombre);
        for (var i = 0; i < filas.Length; i++)
            for (var j = 0; j < filas[i].Length; j++)
                switch (filas[i][j])
                {
                    case null: break;
                    case string s: h.Poner(i + 1, j + 1, s); break;
                    case int n: h.Poner(i + 1, j + 1, (decimal)n); break;
                    case decimal d: h.Poner(i + 1, j + 1, d); break;
                    default: throw new ArgumentException("tipo de celda no soportado");
                }
        return h;
    }

    static object?[] F(params object?[] c) => c;
    static readonly object?[] Titulos = F("Código", "Cuenta / concepto", "Debe", "Haber");
    static readonly object?[] Suma = F(null, "SUMATORIA DEL ASIENTO", 0, 0);

    static IReadOnlyList<HojaLeida> Leer(params HojaCruda[] hojas) =>
        new ParserBancos().Leer(new LibroCrudo { Nombre = "t.xlsx", HashExcel = new string('0', 64), Hojas = hojas });

    public static void Run(Action<string, Action> test)
    {
        test("Bancos: un asiento con sus líneas, la fila de origen y el concepto «MM-AAAA-HOJA»", () =>
        {
            var r = Leer(Hoja("BBVA", F("BANCO BBVA"), F(), F("MES 05/2026"), F(), F(), F("ASIENTO 1"), Titulos,
                F("2.01.01.003", "CHEQUES DIFERIDOS", 1398900661.6m), F("1.01.01.007", "A BBVA", null, 1398900661.6m), Suma));
            var a = r.Single().Asientos.Single();
            Afirmar.Igual("BBVA", a.Hoja); Afirmar.Igual(new Mes(2026, 5), a.Mes!.Value);
            Afirmar.Igual("05-2026-BBVA", a.Concepto); Afirmar.Igual(EstadoAsiento.Registrable, a.Estado);
            Afirmar.Igual("1", a.NumeroOrigen); Afirmar.Igual(6, a.FilaEncabezado);
            Afirmar.Igual(2, a.Lineas.Count);
            Afirmar.Igual(8, a.Lineas[0].Fila); Afirmar.Igual("2.01.01.003", a.Lineas[0].Codigo); Afirmar.Igual(1398900661.6m, a.Lineas[0].Debe);
            Afirmar.Igual(9, a.Lineas[1].Fila); Afirmar.Igual(1398900661.6m, a.Lineas[1].Haber); Afirmar.Igual(0m, a.Lineas[1].Debe);
        });
        test("Bancos: «SUMATORIA DEL ASIENTO» y la fila de títulos no son líneas", () =>
        {
            var a = Leer(Hoja("BBVA", F("MES 05/2026"), F("ASIENTO 1"), Titulos, F("1.1", "X", 10), F("1.2", "A X", null, 10), F(null, "SUMATORIA DEL ASIENTO", 10, 10)))
                .Single().Asientos.Single();
            Afirmar.Igual(2, a.Lineas.Count);
        });
        test("Bancos: «INFORMATIVO — NO SE REGISTRA» en el encabezado excluye el asiento (A7), con cualquier mayúscula o tilde", () =>
        {
            var r = Leer(Hoja("BBVA", F("MES 05/2026"),
                F("ASIENTO 1 · INFORMATIVO — NO SE REGISTRA"), Titulos, F(null, "HIPOTECARIO", 147000000), F(null, "A BBVA", null, 147000000), Suma,
                F("ASIENTO 2 · informativo — no se registra"), Titulos, F("1.1", "X", 5), F("1.2", "Y", null, 5), Suma,
                F("ASIENTO 3"), Titulos, F("1.1", "X", 7), F("1.2", "Y", null, 7), Suma)).Single().Asientos;
            Afirmar.Igual(EstadoAsiento.InformativoExcluido, r[0].Estado);
            Afirmar.Igual(EstadoAsiento.InformativoExcluido, r[1].Estado);
            Afirmar.Igual(EstadoAsiento.Registrable, r[2].Estado);
            Afirmar.Igual("3", r[2].NumeroOrigen);
        });
        test("Bancos: las notas del pie no son encabezados ni líneas", () =>
        {
            var a = Leer(Hoja("BBVA", F("MES 05/2026"), F("ASIENTO 1"), Titulos, F("1.1", "X", 5), F("1.2", "Y", null, 5), Suma,
                F("Nota: Las transferencias hacia otros bancos se registran en el banco receptor."), F(), F("ASIENTO 2"), Titulos, F("1.1", "X", 1), Suma)).Single().Asientos;
            Afirmar.Igual(2, a.Count); Afirmar.Igual(2, a[0].Lineas.Count); Afirmar.Igual(1, a[1].Lineas.Count);
        });
        test("Bancos: una línea sin código se conserva con su nombre (A1)", () =>
        {
            var l = Leer(Hoja("COMAFI", F("MES 05/2026"), F("ASIENTO 1"), Titulos, F("1.1", "BANCO", 5000), F(null, "COMAFI BURSATIL", null, 5000), Suma))
                .Single().Asientos.Single().Lineas;
            Afirmar.Verdad(l[1].SinCodigo && !l[1].FilaSuelta, "sin código pero con nombre"); Afirmar.Igual("COMAFI BURSATIL", l[1].Nombre);
        });
        test("Bancos: la fila de totales suelta (sin código ni nombre) se conserva para avisarla (A3)", () =>
        {
            var l = Leer(Hoja("SANTANDER", F("MES 05/2026"), F("ASIENTO 1"), Titulos, F("1.1", "X", 100), F("1.2", "Y", null, 100), F(null, null, 488708350.94m, 488708350.94m), Suma))
                .Single().Asientos.Single().Lineas;
            Afirmar.Igual(3, l.Count); Afirmar.Verdad(l[2].FilaSuelta, "la tercera es la fila suelta");
            Afirmar.Igual(6, l[2].Fila); Afirmar.Igual(488708350.94m, l[2].Debe); Afirmar.Igual(488708350.94m, l[2].Haber);
        });
        test("Bancos: un asiento sin líneas o con todo en cero queda como vacío omitido (A8)", () =>
        {
            var a = Leer(Hoja("BBVA", F("MES 05/2026"), F("ASIENTO 1"), Titulos, Suma,
                F("ASIENTO 2"), Titulos, F("1.1", "X", 0), F("1.2", "Y", null, 0), Suma)).Single().Asientos;
            Afirmar.Igual(EstadoAsiento.VacioOmitido, a[0].Estado); Afirmar.Igual(EstadoAsiento.VacioOmitido, a[1].Estado);
        });
        test("Bancos: cada MES nuevo cambia la fecha y el concepto de los asientos que siguen", () =>
        {
            var a = Leer(Hoja("MACRO 663", F("MES 05/2026"), F("ASIENTO 1"), Titulos, F("1.1", "X", 5), Suma, F("MES 06/2026"), F("ASIENTO 1"), Titulos, F("1.1", "X", 6), Suma))
                .Single().Asientos;
            Afirmar.Igual("05-2026-MACRO 663", a[0].Concepto); Afirmar.Igual("06-2026-MACRO 663", a[1].Concepto);
            Afirmar.Igual(new Mes(2026, 6), a[1].Mes!.Value);
        });
        test("Bancos: «Codigo» sin tilde y títulos con otra mayúscula también se reconocen; no se mira por número de fila", () =>
        {
            var a = Leer(Hoja("BBVA", F(), F(), F(), F(), F(), F(), F(), F("mes 05/2026"), F("Asiento 4"), F("Codigo", "CUENTA / CONCEPTO", "Debe", "Haber"),
                F("1.1", "X", 5), F("1.2", "Y", null, 5))).Single().Asientos.Single();
            Afirmar.Igual(2, a.Lineas.Count); Afirmar.Igual("4", a.NumeroOrigen); Afirmar.Igual(9, a.FilaEncabezado);
        });
        test("Bancos: el código con comas se conserva tal cual para que A2 lo normalice", () =>
        {
            var l = Leer(Hoja("COMPRAS", F("MES 05/2026"), F("ASIENTO 1"), Titulos, F("4,01,05,050", "ASSAL", 5), Suma)).Single().Asientos.Single().Lineas;
            Afirmar.Igual("4,01,05,050", l[0].Codigo);
        });
        test("Bancos: las hojas salen en el orden del libro y una hoja sin asientos no rompe nada", () =>
        {
            var r = Leer(Hoja("B1", F("BANCO B1")), Hoja("B2", F("MES 05/2026"), F("ASIENTO 1"), Titulos, F("1.1", "X", 5), Suma));
            Afirmar.Igual(2, r.Count); Afirmar.Igual(0, r[0].Asientos.Count); Afirmar.Igual(1, r[1].Asientos.Count);
        });
    }
}
