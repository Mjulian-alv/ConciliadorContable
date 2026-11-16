// Fecha: 12/11/2026 - TAREA: 00180 - Linea: 3 - Parser VARIOS con planillas chicas armadas en el test.
using AsientosEstudio.Core;

static class ParserVariosTests
{
    static object?[] F(params object?[] c) => c;
    static readonly object?[] Titulos = F("Código", "Cuenta / concepto", "Debe", "Haber");

    static IReadOnlyList<HojaLeida> Leer(params HojaCruda[] hojas) =>
        new ParserVarios().Leer(new LibroCrudo { Nombre = "v.xlsx", HashExcel = new string('0', 64), Hojas = hojas });

    static IReadOnlyList<AsientoLeido> Asientos(params object?[][] filas) => Leer(ParserBancosTests.Hoja("HOJA", filas)).Single().Asientos;

    public static void Run(Action<string, Action> test)
    {
        test("Varios: los dos formatos de mes («05-2026» y «MAYO 2026») en la misma hoja", () =>
        {
            var a = Asientos(F("ASIENTO DE VENTAS"), F("05-2026"), F("Compras"), Titulos, F("1.1", "X", 5, 0), F("1.2", "Y", 0, 5), F(null, "TOTAL DEL ASIENTO", 5, 5),
                F("JUNIO 2026"), F("Seguros"), Titulos, F("1.1", "X", 7, 0), F("1.2", "Y", 0, 7), F(null, "TOTAL DEL ASIENTO", 7, 7));
            Afirmar.Igual(2, a.Count);
            Afirmar.Igual(new Mes(2026, 5), a[0].Mes!.Value); Afirmar.Igual(new Mes(2026, 6), a[1].Mes!.Value);
            Afirmar.Igual("Compras", a[0].Concepto); Afirmar.Igual("Seguros", a[1].Concepto);
        });
        test("Varios: el prefijo «ASIENTO n — » se quita del concepto (raya, guion medio o dos puntos) y el número queda aparte", () =>
        {
            var a = Asientos(F("05-2026"), F("ASIENTO 1 — VENTAS, COMISIONES E IMPUESTOS"), Titulos, F("1.1", "X", 5), F("TOTAL ASIENTO", null, 5, 5),
                F("ASIENTO 2 - OPERACIONES EXTRACASH"), Titulos, F("1.1", "X", 5), F("TOTAL ASIENTO", null, 5, 5),
                F("ASIENTO 3: VENTA EN MONEDA EXTRANJERA"), Titulos, F("1.1", "X", 5), F("TOTAL ASIENTO", null, 5, 5));
            Afirmar.Igual("VENTAS, COMISIONES E IMPUESTOS", a[0].Concepto); Afirmar.Igual("1", a[0].NumeroOrigen);
            Afirmar.Igual("OPERACIONES EXTRACASH", a[1].Concepto); Afirmar.Igual("VENTA EN MONEDA EXTRANJERA", a[2].Concepto);
            Afirmar.Igual("ASIENTO 1 — VENTAS, COMISIONES E IMPUESTOS", a[0].Titulo);
        });
        test("Varios: un encabezado sin la fila «Código…», directo a una línea con código, también abre el asiento", () =>
        {
            var a = Asientos(F("MAYO 2026"), F("Por la emisión de echeqs diferidos"), F("2.1", "X", 5, 0), F("2.2", "Y", 0, 5), F(null, "TOTAL DEL ASIENTO", 5, 5));
            Afirmar.Igual(1, a.Count); Afirmar.Igual(2, a[0].Lineas.Count); Afirmar.Igual("Por la emisión de echeqs diferidos", a[0].Concepto);
        });
        test("Varios: las notas debajo de un asiento no son encabezados ni líneas", () =>
        {
            var a = Asientos(F("05-2026"), F("ASIENTO 2 — OPERACIONES EXTRACASH"), Titulos, F("1.1", "X", 5), F("1.2", "Y", null, 5), F("TOTAL ASIENTO", null, 5, 5),
                F("Por operaciones de extracash que luego se acreditan en bancos"), F(), F(),
                F("ASIENTO 3 — VENTA EN MONEDA EXTRANJERA"), Titulos, F("1.1", "X", 7), F("1.2", "Y", null, 7), F("TOTAL ASIENTO", null, 7, 7),
                F("Por ventas en dólares — Representan USD 26.080,00 a TC: 1.424,97"),
                F("Pago a CAM con dólares"), Titulos, F("1.1", "X", 9), F("1.2", "Y", null, 9), F(null, "TOTAL DEL ASIENTO", 9, 9),
                F("USD 15.000,00 | Tipo de cambio: 1.405,00 | Equivalente: 21.075.000,00"));
            Afirmar.Igual(3, a.Count);
            Afirmar.Igual("OPERACIONES EXTRACASH", a[0].Concepto); Afirmar.Igual(2, a[0].Lineas.Count);
            Afirmar.Igual("VENTA EN MONEDA EXTRANJERA", a[1].Concepto); Afirmar.Igual("Pago a CAM con dólares", a[2].Concepto);
            Afirmar.Verdad(a.All(x => x.Lineas.All(l => !l.FilaSuelta && l.Codigo.Length > 0)), "ninguna nota entró como línea");
        });
        test("Varios: «TOTAL ASIENTO» en A y «TOTAL DEL ASIENTO» en B cierran el asiento y no son líneas", () =>
        {
            var a = Asientos(F("05-2026"), F("Uno"), Titulos, F("1.1", "X", 5, 0), F("TOTAL ASIENTO", null, 5, 5), F("Dos"), Titulos, F("1.1", "X", 7, 0), F(null, "TOTAL DEL ASIENTO", 7, 7));
            Afirmar.Igual(1, a[0].Lineas.Count); Afirmar.Igual(1, a[1].Lineas.Count);
        });
        test("Varios: las columnas F a N (cálculos, comentarios, tablas de cheques) no se leen", () =>
        {
            var h = ParserBancosTests.Hoja("CHEQUES", F("MAYO 2026"), F("Por pagos a LA SEGUNDA"), Titulos, F("1.1", "X", 5, 0, null, "calculo auxiliar", 99999), F("1.2", "Y", 0, 5), F(null, "TOTAL DEL ASIENTO", 5, 5));
            h.Poner(2, 8, "Tabla de cheques").Poner(3, 9, 12345m);
            var a = Leer(h).Single().Asientos.Single();
            Afirmar.Igual(2, a.Lineas.Count); Afirmar.Igual(5m, a.Lineas[0].Debe);
        });
        test("Varios: un asiento con todas sus líneas en cero se omite y no consume número (A8)", () =>
        {
            var a = Asientos(F("MAYO 2026"), F("Por la emisión de echeqs a CAM"), F("1.1", "X", 0, 0), F("1.2", "Y", 0, 0), F(null, "TOTAL DEL ASIENTO", 0, 0));
            Afirmar.Igual(EstadoAsiento.VacioOmitido, a.Single().Estado);
        });
        test("Varios: el bloque del ejercicio anterior se excluye entero y no contamina al asiento de después (A10)", () =>
        {
            var a = Asientos(F("CHEQUES ENDOSADOS"), F(),
                F("Asiendo del ejercicio anterior"), F(null, new DateTime(2026, 4, 1).ToString("yyyy-MM-dd")),
                F(null, "VALORES A DEPOSITAR", 18648096.29m), F(null, "A TARJETAS A COBRAR", null, 18434028.89m), F(null, "A DEUDORES POR VENTAS", null, 214067.4m),
                F("ASIENTO QUE NO SE REGISTRA"), F(), F(), F("MAYO 2026"),
                F("Por recepción de echeqs"), Titulos, F("1.1", "X", 5), F("1.2", "Y", null, 5), F(null, "TOTAL DEL ASIENTO", 5, 5));
            Afirmar.Igual(2, a.Count);
            Afirmar.Igual(EstadoAsiento.NoSeRegistra, a[0].Estado); Afirmar.Igual(3, a[0].Lineas.Count(l => !l.EnCero));
            Afirmar.Igual(EstadoAsiento.Registrable, a[1].Estado); Afirmar.Igual("Por recepción de echeqs", a[1].Concepto); Afirmar.Igual(2, a[1].Lineas.Count);
            Afirmar.Igual(new Mes(2026, 5), a[1].Mes!.Value);
        });
        test("Varios: un encabezado que dice «NO SE REGISTRA» o «INFORMATIVO» se excluye con el estado que corresponde", () =>
        {
            var a = Asientos(F("MAYO 2026"), F("Por X — NO SE REGISTRA"), Titulos, F("1.1", "X", 5), F(null, "TOTAL DEL ASIENTO", 5, 5),
                F("Por Y · INFORMATIVO"), Titulos, F("1.1", "X", 5), F(null, "TOTAL DEL ASIENTO", 5, 5));
            Afirmar.Igual(EstadoAsiento.NoSeRegistra, a[0].Estado); Afirmar.Igual(EstadoAsiento.InformativoExcluido, a[1].Estado);
        });
        test("Varios: el título de la hoja («ASIENTOS DE CAJA») no abre un asiento", () =>
        {
            var a = Asientos(F("ASIENTOS DE CAJA"), F(), F("MAYO 2026"), F("Por egresos de caja"), Titulos, F("1.1", "X", 5), F(null, "TOTAL DEL ASIENTO", 5, 5));
            Afirmar.Igual(1, a.Count); Afirmar.Igual("Por egresos de caja", a[0].Concepto);
        });
        test("Varios: el código con comas se conserva tal cual (A2) y el asiento sin mes queda sin mes", () =>
        {
            var a = Asientos(F("Compras"), Titulos, F("4,01,05,050", "ASSAL", 5), F(null, "TOTAL DEL ASIENTO", 5, 5));
            Afirmar.Igual("4,01,05,050", a[0].Lineas[0].Codigo); Afirmar.Verdad(a[0].Mes is null, "sin mes");
        });
    }
}
