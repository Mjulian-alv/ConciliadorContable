// Fecha: 13/11/2026 - TAREA: 00180 - Linea: 4,5 - Validador, selección de meses, rango habitual y detector de tipo, con libros armados en el test.
using AsientosEstudio.Core;

static class ValidadorTests
{
    static readonly Mes May = new(2026, 5), Jun = new(2026, 6), Sep = new(2026, 9);

    static LineaLeida L(int fila, string cod, string nombre, decimal d = 0, decimal h = 0, ProblemaImporte pd = ProblemaImporte.Ninguno, ProblemaImporte ph = ProblemaImporte.Ninguno) =>
        new(fila, cod, nombre, d, h, pd, ph);

    static AsientoLeido A(string hoja, int filaEnc, Mes? mes, params LineaLeida[] lineas) =>
        new() { Hoja = hoja, Mes = mes, Titulo = "T" + filaEnc, Concepto = "C" + filaEnc, Estado = EstadoAsiento.Registrable, FilaEncabezado = filaEnc, Lineas = lineas };

    static AsientoLeido Con(AsientoLeido a, EstadoAsiento e) =>
        new() { Hoja = a.Hoja, Mes = a.Mes, Titulo = a.Titulo, Concepto = a.Concepto, Estado = e, FilaEncabezado = a.FilaEncabezado, Lineas = a.Lineas };

    static LibroLeido Libro(params AsientoLeido[] asientos) => new()
    {
        Tipo = TipoPlanilla.Bancos, Nombre = "x.xlsx", HashExcel = new string('0', 64),
        Hojas = asientos.GroupBy(a => a.Hoja).Select((g, i) => new HojaLeida { Nombre = g.Key, Orden = i, Asientos = g.ToList() }).ToList(),
    };

    // Un asiento simple que cuadra: 100 al Debe y 100 al Haber.
    static AsientoLeido Ok(string hoja = "BBVA", int fila = 2, Mes? mes = null) =>
        A(hoja, fila, mes ?? May, L(fila + 2, "1.1", "CAJA", d: 100m), L(fila + 3, "2.1", "A BANCO", h: 100m));

    static ResultadoValidacion V(LibroLeido l, OpcionesValidacion? o = null) => Validador.Validar(l, o);

    public static void Run(Action<string, Action> test)
    {
        test("Validador: un asiento limpio queda OK, sin alertas, con sus líneas en 2 decimales", () =>
        {
            var r = V(Libro(Ok()));
            Afirmar.Igual(0, r.Alertas.Count); Afirmar.Verdad(!r.HayBloqueantes, "sin bloqueantes");
            var c = r.Asientos.Single(); Afirmar.Igual(EstadoVista.Ok, c.Estado); Afirmar.Igual(2, c.Lineas.Count); Afirmar.Igual(100m, c.TotalDebe); Afirmar.Igual(100m, c.TotalHaber);
        });
        test("A1: una línea sin código es una ADVERTENCIA (no bloquea) y sale con la cuenta vacía", () =>
        {
            var r = V(Libro(A("COMAFI", 2, May, L(4, "", "COMAFI BURSATIL", d: 500m), L(5, "1.1", "BANCO", h: 500m))));
            var a = r.De("A1").Single();
            Afirmar.Igual(Severidad.Advertencia, a.Severidad); Afirmar.Igual("COMAFI", a.Hoja!); Afirmar.Igual(4, a.Fila!.Value); Afirmar.Igual("COMAFI BURSATIL", a.Cuenta!);
            Afirmar.Verdad(!r.HayBloqueantes, "no bloquea"); Afirmar.Igual("", r.Asientos.Single().Lineas[0].Codigo); Afirmar.Igual(EstadoVista.Advertencia, r.Asientos.Single().Estado);
        });
        test("A2: «4,01,05,050» se normaliza a «4.01.05.050», avisa y no bloquea", () =>
        {
            var r = V(Libro(A("COMPRAS", 2, May, L(70, "4,01,05,050", "ASSAL", d: 179067.47m), L(71, "1.1", "CAJA", h: 179067.47m))));
            var a = r.De("A2").Single();
            Afirmar.Igual(Severidad.Advertencia, a.Severidad); Afirmar.Igual(ResolucionAlerta.Normalizada, a.Resolucion); Afirmar.Igual("4,01,05,050 → 4.01.05.050", a.Detalle!);
            Afirmar.Igual("4.01.05.050", r.Asientos.Single().Lineas[0].Codigo); Afirmar.Verdad(!r.HayBloqueantes, "no bloquea");
        });
        test("A2: un código que sigue sin tener forma de código tras normalizar es BLOQUEANTE, y se puede aceptar", () =>
        {
            var l = Libro(A("H", 2, May, L(4, "ABC-1", "X", d: 5m), L(5, "1.1", "Y", h: 5m)));
            var r = V(l);
            Afirmar.Igual(Severidad.Bloqueante, r.De("A2").Single().Severidad); Afirmar.Verdad(r.HayBloqueantes, "bloquea");
            var ac = V(l, new OpcionesValidacion { Aceptaciones = new[] { new DecisionAceptar("A2", new ClaveAsiento("H", 2), "M. Julián") } });
            Afirmar.Verdad(!ac.HayBloqueantes, "aceptada"); Afirmar.Igual(ResolucionAlerta.Aceptada, ac.De("A2").Single().Resolucion); Afirmar.Igual("M. Julián", ac.De("A2").Single().ResueltaPor!);
        });
        test("A3: una fila sin código ni nombre no es un movimiento: se ignora, se avisa y no entra al TXT", () =>
        {
            var r = V(Libro(A("SANTANDER", 2, May, L(4, "1.1", "X", d: 100m), L(5, "1.2", "Y", h: 100m), L(20, "", "", d: 488708350.94m, h: 488708350.94m))));
            var a = r.De("A3").Single();
            Afirmar.Igual(20, a.Fila!.Value); Afirmar.Igual(Severidad.Advertencia, a.Severidad); Afirmar.Igual(ResolucionAlerta.Ignorada, a.Resolucion);
            Afirmar.Igual(2, r.Asientos.Single().Lineas.Count); Afirmar.Verdad(!r.HayBloqueantes, "no bloquea"); Afirmar.Igual(0, r.De("A4").Count());
        });
        test("A4: una línea con código y con Debe y Haber a la vez es BLOQUEANTE", () =>
        {
            var r = V(Libro(A("H", 2, May, L(4, "1.1", "X", d: 100m, h: 100m), L(5, "1.2", "Y", h: 0m))));
            Afirmar.Igual(Severidad.Bloqueante, r.De("A4").Single().Severidad); Afirmar.Verdad(r.HayBloqueantes, "bloquea"); Afirmar.Igual(EstadoVista.Error, r.Asientos.Single().Estado);
        });
        test("A5: un descuadre de 0,01 por redondeo se INFORMA y no se ajusta: los importes salen redondeados tal cual", () =>
        {
            var r = V(Libro(A("MACRO 663", 2, May,
                L(4, "2.01.01.017", "RICARDO", d: 100.0049m), L(5, "2.01.01.003", "PROVEEDORES", d: 200.0044m),
                L(6, "1.01.02.007", "MACRO 663", h: 150.0054m), L(7, "4.01.05.012", "INTERESES", h: 150.0039m))));
            var a = r.De("A5").Single();
            Afirmar.Igual(Severidad.Advertencia, a.Severidad); Afirmar.Igual(ResolucionAlerta.Ninguna, a.Resolucion); Afirmar.Igual(-0.01m, a.Diferencia!.Value);
            Afirmar.Igual("Debe 300,00 · Haber 300,01 · diferencia −0,01", a.Detalle!); Afirmar.Verdad(a.Accion.Contains("No se ajusta"), "dice que no se ajusta");
            var c = r.Asientos.Single(); Afirmar.Igual(300.00m, c.TotalDebe); Afirmar.Igual(300.01m, c.TotalHaber); // sin tocar
            Afirmar.Igual(100.00m, c.Lineas[0].Debe); Afirmar.Igual(150.01m, c.Lineas[2].Haber);
            Afirmar.Verdad(!r.HayBloqueantes, "no bloquea"); Afirmar.Igual(EstadoVista.Advertencia, c.Estado);
        });
        test("A6 de un centavo: un asiento descuadrado 0,01 en el origen se informa, no bloquea y no se toca", () =>
        {
            var r = V(Libro(A("05-2026", 3, May, L(5, "2.01.04.004", "IVA 21", d: 1169596185.03m), L(6, "2.01.04.003", "A SALDO IVA A PAGAR", h: 1169596185.04m))));
            var a = r.De("A6").Single();
            Afirmar.Igual(Severidad.Advertencia, a.Severidad); Afirmar.Igual(-0.01m, a.Diferencia!.Value); Afirmar.Verdad(!r.HayBloqueantes, "no bloquea");
            Afirmar.Verdad(a.Causa.Contains("sobra Haber") && a.Accion.Contains("no se ajusta"), a.Causa + " / " + a.Accion);
            Afirmar.Igual(1169596185.03m, r.Asientos.Single().TotalDebe); Afirmar.Igual(1169596185.04m, r.Asientos.Single().TotalHaber); // sin tocar
            Afirmar.Igual(EstadoVista.Advertencia, r.Asientos.Single().Estado);
        });
        test("A6: una diferencia mayor a 0,01 BLOQUEA, se puede aceptar y queda registrado quién la aceptó", () =>
        {
            var l = Libro(A("H", 2, May, L(4, "1.1", "X", d: 100m), L(5, "1.2", "Y", h: 99.5m)));
            var r = V(l);
            Afirmar.Verdad(r.HayBloqueantes, "bloquea"); Afirmar.Igual(Severidad.Bloqueante, r.De("A6").Single().Severidad); Afirmar.Igual(EstadoVista.Error, r.Asientos.Single().Estado);
            Afirmar.Verdad(r.De("A6").Single().Accion.Contains("mayor a 0,01"), "dice por qué");
            var ac = V(l, new OpcionesValidacion { Aceptaciones = new[] { new DecisionAceptar("A6", new ClaveAsiento("H", 2), "M. Julián") } });
            Afirmar.Verdad(!ac.HayBloqueantes, "aceptada"); Afirmar.Igual(ResolucionAlerta.Aceptada, ac.De("A6").Single().Resolucion); Afirmar.Igual("M. Julián", ac.De("A6").Single().ResueltaPor!);
            Afirmar.Igual(0.5m, ac.Asientos.Single().Diferencia); // el asiento sale descuadrado, como vino
        });
        test("A7, A8 y A10: informativos, vacíos y bloques «no se registra» se listan, no se validan y no tienen líneas", () =>
        {
            var base1 = A("BBVA", 2, May, L(4, "", "HIPOTECARIO", d: 147000000m));
            var r = V(Libro(Con(base1, EstadoAsiento.InformativoExcluido), Con(A("BBVA", 8, May), EstadoAsiento.VacioOmitido), Con(A("BBVA", 12, May), EstadoAsiento.NoSeRegistra)));
            Afirmar.Igual(1, r.De("A7").Count()); Afirmar.Igual(1, r.De("A8").Count()); Afirmar.Igual(1, r.De("A10").Count());
            Afirmar.Verdad(r.Alertas.All(a => a.Severidad == Severidad.Informativa), "todas informativas");
            Afirmar.Verdad(r.Asientos.All(c => c.Estado == EstadoVista.Omitido && c.Lineas.Count == 0), "omitidos sin líneas");
            Afirmar.Igual(0, r.De("A1").Count()); // un informativo no valida sus líneas aunque no tengan código ni cuadren
        });
        test("A9: un mes fuera del rango esperado se informa, con la cantidad y las hojas", () =>
        {
            var l = Libro(Ok("CHEQUES EMITIDOS", 2, May), Ok("CHEQUES EMITIDOS", 20, Sep), Ok("CHEQUES ENDOSADOS", 2, Sep));
            var r = V(l, new OpcionesValidacion { RangoHabitual = (May, new Mes(2026, 8)) });
            var a = r.De("A9").Single();
            Afirmar.Igual(Severidad.Informativa, a.Severidad); Afirmar.Igual(Sep, a.Mes!.Value); Afirmar.Verdad(a.Causa.Contains("2 hojas") && a.Causa.Contains("2 asientos"), a.Causa);
            Afirmar.Igual(0, V(l).De("A9").Count()); // sin rango esperado no se compara
        });
        test("Selección de meses: lo que está fuera de la selección no se valida, no genera alertas y no sale", () =>
        {
            var l = Libro(Ok("H", 2, May), Ok("H", 10, Jun), A("H", 20, Sep, L(22, "", "SIN CODIGO", d: 5m), L(23, "1.1", "X", h: 9m)));
            var r = V(l, new OpcionesValidacion { Meses = new HashSet<Mes> { May, Jun } });
            Afirmar.Igual(2, r.Asientos.Count); Afirmar.Igual(0, r.Alertas.Count); Afirmar.Verdad(!r.HayBloqueantes, "septiembre quedó afuera");
            Afirmar.Verdad(V(l).HayBloqueantes, "con septiembre incluido sí bloquea (A6)");
        });
        test("A11: un importe sin valor calculado bloquea y señala la celda exacta; no se lee como cero", () =>
        {
            var r = V(Libro(A("COMPRAS", 2, May, L(118, "4.01", "X", d: 0m, pd: ProblemaImporte.FormulaSinValor), L(119, "1.1", "Y", h: 0m, ph: ProblemaImporte.ErrorDeExcel))));
            var a = r.De("A11").ToList();
            Afirmar.Igual(2, a.Count); Afirmar.Igual("celda C118", a[0].Detalle!); Afirmar.Igual("celda D119", a[1].Detalle!); Afirmar.Verdad(r.HayBloqueantes, "bloquea");
            Afirmar.Igual(0, r.De("A6").Count()); // no se evalúa el cuadre con importes que no se pudieron leer
        });
        test("A12: los importes con más de 2 decimales se informan una vez por hoja, con la cantidad de líneas", () =>
        {
            var r = V(Libro(A("VENTAS", 2, May, L(4, "1.1", "X", d: 10.0049m), L(5, "1.2", "Y", h: 10.0049m), L(6, "1.3", "Z", d: 1m), L(7, "1.4", "W", h: 1m))));
            var a = r.De("A12").Single(); Afirmar.Igual("VENTAS", a.Hoja!); Afirmar.Igual("2 líneas", a.Detalle!); Afirmar.Igual(Severidad.Informativa, a.Severidad);
        });
        test("A13: un importe negativo avisa y no se invierte solo", () =>
        {
            var r = V(Libro(A("H", 2, May, L(4, "1.1", "X", d: -50m), L(5, "1.2", "Y", h: -50m))));
            Afirmar.Igual(2, r.De("A13").Count()); Afirmar.Igual(Severidad.Advertencia, r.De("A13").First().Severidad);
            Afirmar.Igual(-50m, r.Asientos.Single().Lineas[0].Debe); // sin invertir
        });
        test("A15: un código con nombres distintos es informativo y solo aparece una vez", () =>
        {
            var r = V(Libro(A("H", 2, May, L(4, "2.01.04.005", "IVA CF 21", d: 5m), L(5, "2.01.04.005", "IVA CF 10,5", d: 5m), L(6, "1.1", "X", h: 10m)),
                A("H", 10, Jun, L(12, "2.01.04.005", "iva cf 21", d: 5m), L(13, "1.1", "X", h: 5m))));
            var a = r.De("A15").Single(); Afirmar.Igual("2.01.04.005", a.Cuenta!); Afirmar.Igual(Severidad.Informativa, a.Severidad);
            Afirmar.Verdad(!r.HayBloqueantes && r.Asientos.All(c => c.Estado == EstadoVista.Ok), "no marca los asientos");
        });
        test("M1: un asiento registrable sin mes bloquea: sin mes no hay fecha para el TXT", () =>
        {
            var r = V(Libro(A("H", 2, null, L(4, "1.1", "X", d: 5m), L(5, "1.2", "Y", h: 5m))));
            Afirmar.Igual(Severidad.Bloqueante, r.De("M1").Single().Severidad); Afirmar.Verdad(r.HayBloqueantes, "bloquea");
        });
        test("Meses: el resumen trae cantidades por mes y el rango se arma sobre los meses que existen", () =>
        {
            var l = Libro(Ok("H", 2, May), Ok("H", 10, May), Ok("H", 20, Sep), Con(Ok("H", 30, Jun), EstadoAsiento.VacioOmitido));
            var rs = SeleccionMeses.Resumir(l);
            Afirmar.Igual("05/2026,09/2026", string.Join(",", rs.Select(x => x.Mes.Texto)));
            Afirmar.Igual(2, rs[0].Asientos); Afirmar.Igual(4, rs[0].Lineas); Afirmar.Igual(200m, rs[0].Debe);
            Afirmar.Igual(1, SeleccionMeses.Rango(l, May, new Mes(2026, 8)).Count); Afirmar.Igual(2, SeleccionMeses.Todos(l).Count);
        });
        test("Rango habitual: sale del nombre del archivo en cualquiera de sus formas", () =>
        {
            Afirmar.Igual((May, new Mes(2026, 8)), RangoHabitual.DesdeNombre("ASIENTOS VARIOS 05-2026 a 08-2026.xlsx")!.Value);
            Afirmar.Igual((May, new Mes(2026, 8)), RangoHabitual.DesdeNombre("varios 5/2026 al 8/2026.xlsx")!.Value);
            Afirmar.Verdad(RangoHabitual.DesdeNombre("BANCOS FCE 2027.xlsx") is null && RangoHabitual.DesdeNombre(null) is null && RangoHabitual.DesdeNombre("ASIENTO DEVENGAMIENTOS.xlsx") is null, "sin rango");
        });
        test("Detector: reconoce BANCOS, DEVENGAMIENTOS y VARIOS por su fila de cierre, y no confunde a VARIOS con BANCOS", () =>
        {
            LibroCrudo C(params HojaCruda[] h) => new() { Nombre = "x.xlsx", HashExcel = new string('0', 64), Hojas = h };
            var bancos = C(ParserBancosTests.Hoja("BBVA", new object?[] { "MES 05/2026" }, new object?[] { "ASIENTO 1" }, new object?[] { "1.1", "X", 5 }, new object?[] { null, "SUMATORIA DEL ASIENTO", 5, 5 }));
            var deven = C(ParserBancosTests.Hoja("05-2026", new object?[] { "IVA" }, new object?[] { "Código de cuenta", "Nombre de la cuenta" }, new object?[] { "1.1", "X", 5 }, new object?[] { "TOTAL", null, 5, 5 }));
            // Un VARIOS con una hoja VENTAS que usa «ASIENTO 1 — …»: ParserBancos encontraría asientos ahí, pero la fila de cierre es «TOTAL ASIENTO».
            var varios = C(ParserBancosTests.Hoja("VENTAS", new object?[] { "05-2026" }, new object?[] { "ASIENTO 1 — VENTAS" }, new object?[] { "1.1", "X", 5 }, new object?[] { "TOTAL ASIENTO", null, 5, 5 }));
            Afirmar.Igual(TipoPlanilla.Bancos, DetectorTipo.Detectar(bancos).Tipo!.Value);
            Afirmar.Igual(TipoPlanilla.Devengamientos, DetectorTipo.Detectar(deven).Tipo!.Value);
            Afirmar.Igual(TipoPlanilla.Varios, DetectorTipo.Detectar(varios).Tipo!.Value);
            Afirmar.Verdad(DetectorTipo.Detectar(C(ParserBancosTests.Hoja("X", new object?[] { "hola" }))).Tipo is null, "no reconocido");
        });
    }
}
