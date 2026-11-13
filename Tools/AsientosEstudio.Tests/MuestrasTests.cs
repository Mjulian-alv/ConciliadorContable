// Fecha: 12/11/2026 - TAREA: 00180 - Linea: 1 - Criterios de aceptación de §7 con los Excel reales. Se saltan, avisando, si la carpeta no está:
// los Excel son datos contables reales y no se versionan (decisión D3). Para correrlos: ASIENTOS_MUESTRAS=<carpeta con los .xlsx>.
using AsientosEstudio.Core;

static class MuestrasTests
{
    static string Carpeta() =>
        Environment.GetEnvironmentVariable("ASIENTOS_MUESTRAS") is { Length: > 0 } c && Directory.Exists(c) ? c
        : throw new Saltada("definí ASIENTOS_MUESTRAS con la carpeta de los Excel reales");

    static LibroLeido Bancos()
    {
        var crudo = LectorLibro.Leer(Path.Combine(Carpeta(), "BANCOS FCE 2027.xlsx"));
        return new LibroLeido { Tipo = TipoPlanilla.Bancos, Nombre = crudo.Nombre, HashExcel = crudo.HashExcel, Hojas = new ParserBancos().Leer(crudo) };
    }

    static LibroLeido Devengamientos()
    {
        var crudo = LectorLibro.Leer(Path.Combine(Carpeta(), "ASIENTO DEVENGAMIENTOS.xlsx"));
        return new LibroLeido { Tipo = TipoPlanilla.Devengamientos, Nombre = crudo.Nombre, HashExcel = crudo.HashExcel, Hojas = new ParserDevengamientos().Leer(crudo) };
    }

    public static void Run(Action<string, Action> test)
    {
        // Fecha: 12/11/2026 - TAREA: 00180 - Linea: 2 - DEVENGAMIENTOS real (§7): 44 asientos, 156 líneas, 1 omitido por vacío, 4 descuadres de 0,01.
        test("DEVENGAMIENTOS real: 4 hojas (05 a 08/2026), 44 asientos registrables, 156 líneas y 1 omitido por vacío (A8)", () =>
        {
            var l = Devengamientos();
            Afirmar.Igual("05-2026,06-2026,07-2026,08-2026", string.Join(",", l.Hojas.Select(h => h.Nombre)));
            var reg = l.Asientos.Where(a => a.Estado == EstadoAsiento.Registrable).ToList();
            Afirmar.Igual(44, reg.Count, "registrables");
            Afirmar.Igual(156, reg.Sum(a => a.Lineas.Count), "líneas");
            var vacios = l.Asientos.Where(a => a.Estado == EstadoAsiento.VacioOmitido).ToList();
            Afirmar.Igual(1, vacios.Count, "omitidos");
            Afirmar.Igual("Ret. de 3eros. - 1era quincena - SUSS", vacios[0].Concepto); Afirmar.Igual("05-2026", vacios[0].Hoja);
            Afirmar.Igual(0, l.Asientos.Count(a => a.Mes is null), "asientos sin mes");
        });
        test("DEVENGAMIENTOS real: el concepto es el encabezado y el orden es el de aparición", () =>
        {
            var l = Devengamientos();
            var primero = l.Hojas[0].Asientos[0];
            Afirmar.Igual("IVA - Asiento principal", primero.Concepto); Afirmar.Igual(3, primero.FilaEncabezado);
            Afirmar.Igual("2.01.04.004", primero.Lineas[0].Codigo); Afirmar.Igual(965356929.73m, primero.Lineas[0].Debe);
            Afirmar.Igual("IVA - CF no computable", l.Hojas[0].Asientos[1].Concepto);
            Afirmar.Verdad(l.Asientos.Any(a => a.Concepto == "IVA - Origen Plan de Pago IVA 08/2026"), "el asiento propio de agosto");
        });
        test("DEVENGAMIENTOS real: 4 asientos descuadrados 0,01 en el origen (IVA principal 05 y 08, IVA CF no computable 05 y 06) y sin resolución Debe 6.936.847.173,80 vs Haber 6.936.847.173,78", () =>
        {
            var reg = Devengamientos().Asientos.Where(a => a.Estado == EstadoAsiento.Registrable).ToList();
            var desc = reg.Where(a => a.TotalDebe != a.TotalHaber).ToList();
            Afirmar.Igual(4, desc.Count, "descuadrados");
            Afirmar.Verdad(desc.All(a => Math.Abs(a.TotalDebe - a.TotalHaber) == 0.01m), "todos por 0,01");
            Afirmar.Igual("05-2026:IVA - Asiento principal|05-2026:IVA - CF no computable|06-2026:IVA - CF no computable|08-2026:IVA - Asiento principal",
                string.Join("|", desc.Select(a => a.Hoja + ":" + a.Concepto).OrderBy(x => x, StringComparer.Ordinal)));
            Afirmar.Igual(6936847173.80m, reg.Sum(a => a.TotalDebe), "Debe");
            Afirmar.Igual(6936847173.78m, reg.Sum(a => a.TotalHaber), "Haber");
        });
        test("BANCOS real: 14 hojas, 140 asientos registrables, 38 informativos excluidos y 965 líneas", () =>
        {
            var l = Bancos();
            Afirmar.Igual(14, l.Hojas.Count);
            var reg = l.Asientos.Where(a => a.Estado == EstadoAsiento.Registrable).ToList();
            Afirmar.Igual(140, reg.Count, "registrables");
            Afirmar.Igual(38, l.Asientos.Count(a => a.Estado == EstadoAsiento.InformativoExcluido), "informativos");
            Afirmar.Igual(0, l.Asientos.Count(a => a.Estado == EstadoAsiento.VacioOmitido), "vacíos");
            // 965 son las líneas que salen al TXT: la fila de totales suelta (A3) se lee pero no es un movimiento, así que no cuenta.
            Afirmar.Igual(965, reg.Sum(a => a.Lineas.Count(x => !x.FilaSuelta)), "líneas");
            Afirmar.Igual(966, reg.Sum(a => a.Lineas.Count), "líneas leídas, con la fila suelta");
        });
        test("BANCOS real: hojas en el orden del Excel, meses 05 a 08/2026 y conceptos «MM-AAAA-HOJA»", () =>
        {
            var l = Bancos();
            Afirmar.Igual("BBVA", l.Hojas[0].Nombre); Afirmar.Igual("SANTANDER", l.Hojas[^1].Nombre);
            var meses = l.Asientos.Where(a => a.Mes is not null).Select(a => a.Mes!.Value).Distinct().OrderBy(m => m).ToList();
            Afirmar.Igual("05/2026,06/2026,07/2026,08/2026", string.Join(",", meses.Select(m => m.Texto)));
            Afirmar.Igual(0, l.Asientos.Count(a => a.Mes is null), "asientos sin mes");
            var primero = l.Hojas[0].Asientos[0];
            Afirmar.Igual("05-2026-BBVA", primero.Concepto);
            Afirmar.Igual("2.01.01.003", primero.Lineas[0].Codigo); Afirmar.Igual(1398900661.6m, primero.Lineas[0].Debe);
        });
        test("BANCOS real: la fila de totales suelta de SANTANDER 05/2026 asiento 1 se conserva (A3)", () =>
        {
            var sueltas = Bancos().Asientos.SelectMany(a => a.Lineas.Select(x => (a, x))).Where(t => t.x.FilaSuelta).ToList();
            Afirmar.Igual(1, sueltas.Count, "filas sueltas");
            var (asiento, linea) = sueltas[0];
            Afirmar.Igual("SANTANDER", asiento.Hoja); Afirmar.Igual("05-2026-SANTANDER", asiento.Concepto); Afirmar.Igual("1", asiento.NumeroOrigen);
            Afirmar.Igual(20, linea.Fila); Afirmar.Igual(488708350.94m, linea.Debe); Afirmar.Igual(488708350.94m, linea.Haber);
        });
        test("BANCOS real: los importes con fórmula se leen del valor guardado (ninguna celda sin valor)", () =>
        {
            var crudo = LectorLibro.Leer(Path.Combine(Carpeta(), "BANCOS FCE 2027.xlsx"));
            var conFormula = crudo.Hojas.SelectMany(h => Enumerable.Range(1, h.UltimaFila).SelectMany(f => new[] { 3, 4 }.Select(c => (h, f, c))))
                .Where(t => t.h.Celda(t.f, t.c) is { TieneFormula: true }).ToList();
            Afirmar.Verdad(conFormula.Count > 0, "el archivo debería tener importes con fórmula");
            var malas = conFormula.Where(t => t.h.Problema(t.f, t.c) != ProblemaImporte.Ninguno).ToList();
            Afirmar.Igual(0, malas.Count, $"importes con fórmula sin valor ({conFormula.Count} con fórmula)");
        });
    }
}
