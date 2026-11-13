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

    static LibroLeido Varios()
    {
        var crudo = LectorLibro.Leer(Path.Combine(Carpeta(), "ASIENTOS VARIOS 05-2026 a 08-2026.xlsx"));
        return new LibroLeido { Tipo = TipoPlanilla.Varios, Nombre = crudo.Nombre, HashExcel = crudo.HashExcel, Hojas = new ParserVarios().Leer(crudo) };
    }

    static decimal Redondeado(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

    public static void Run(Action<string, Action> test)
    {
        // Fecha: 12/11/2026 - TAREA: 00180 - Linea: 3 - VARIOS real (§7): 05–08: 93 asientos / 581 líneas; 05–09: 98 / 600; por hoja 12-17-19-11-19-20;
        // 9 asientos en cero omitidos (A8) y 1 bloque del ejercicio anterior excluido (A10).
        test("VARIOS real: 6 hojas por circuito y, en 05–09/2026, 98 asientos registrables y 600 líneas", () =>
        {
            var l = Varios();
            Afirmar.Igual("VENTAS,COMPRAS,CAJAS,CHEQUES EMITIDOS,CHEQUES ENDOSADOS,SUELDOS", string.Join(",", l.Hojas.Select(h => h.Nombre)));
            var reg = l.Asientos.Where(a => a.Estado == EstadoAsiento.Registrable).ToList();
            Afirmar.Igual(98, reg.Count, "registrables");
            Afirmar.Igual(600, reg.Sum(a => a.Lineas.Count(x => !x.FilaSuelta)), "líneas");
            // El único asiento sin mes es el bloque del ejercicio anterior, que está antes del primer marcador de mes de su hoja y no se registra.
            var sinMes = l.Asientos.Where(a => a.Mes is null).ToList();
            Afirmar.Igual(1, sinMes.Count, "asientos sin mes"); Afirmar.Igual(EstadoAsiento.NoSeRegistra, sinMes[0].Estado);
        });
        test("VARIOS real: por hoja (05–09) VENTAS 12, COMPRAS 17, CAJAS 19, CHEQUES EMITIDOS 11, CHEQUES ENDOSADOS 19, SUELDOS 20", () =>
        {
            var porHoja = Varios().Hojas.ToDictionary(h => h.Nombre, h => h.Asientos.Count(a => a.Estado == EstadoAsiento.Registrable));
            Afirmar.Igual("12,17,19,11,19,20", string.Join(",", new[] { "VENTAS", "COMPRAS", "CAJAS", "CHEQUES EMITIDOS", "CHEQUES ENDOSADOS", "SUELDOS" }.Select(n => porHoja[n])));
        });
        test("VARIOS real: sin septiembre (05–08/2026) son 93 asientos y 581 líneas; septiembre trae 5 asientos y 19 líneas en 2 hojas", () =>
        {
            var reg = Varios().Hojas.SelectMany(h => h.Asientos).Where(a => a.Estado == EstadoAsiento.Registrable).ToList();
            var hasta08 = reg.Where(a => a.Mes!.Value.CompareTo(new Mes(2026, 8)) <= 0).ToList();
            Afirmar.Igual(93, hasta08.Count, "asientos 05–08"); Afirmar.Igual(581, hasta08.Sum(a => a.Lineas.Count), "líneas 05–08");
            var sep = reg.Where(a => a.Mes!.Value == new Mes(2026, 9)).ToList();
            Afirmar.Igual(5, sep.Count, "asientos de septiembre"); Afirmar.Igual(19, sep.Sum(a => a.Lineas.Count), "líneas de septiembre");
            Afirmar.Igual("CHEQUES EMITIDOS,CHEQUES ENDOSADOS", string.Join(",", sep.Select(a => a.Hoja).Distinct()));
        });
        test("VARIOS real: 9 asientos en cero omitidos (A8) y 1 bloque del ejercicio anterior excluido (A10)", () =>
        {
            var l = Varios();
            Afirmar.Igual(9, l.Asientos.Count(a => a.Estado == EstadoAsiento.VacioOmitido), "en cero");
            var anterior = l.Asientos.Where(a => a.Estado == EstadoAsiento.NoSeRegistra).ToList();
            Afirmar.Igual(1, anterior.Count, "ejercicio anterior"); Afirmar.Igual("CHEQUES ENDOSADOS", anterior[0].Hoja);
            Afirmar.Verdad(l.Asientos.Any(a => a.Concepto == "Por la emisión de echeqs a CAM" && a.Estado == EstadoAsiento.VacioOmitido), "el de echeqs a CAM");
            Afirmar.Verdad(l.Asientos.Any(a => a.Hoja == "CHEQUES ENDOSADOS" && a.Concepto == "Por endoso de echeq a MUNDO" && a.Mes == new Mes(2026, 9) && a.Estado == EstadoAsiento.VacioOmitido), "MUNDO de septiembre");
        });
        test("VARIOS real: el concepto no lleva el prefijo «ASIENTO n — » y las notas no son asientos", () =>
        {
            var l = Varios();
            var ventas = l.Hojas[0].Asientos.Where(a => a.Mes == new Mes(2026, 5)).ToList();
            Afirmar.Igual("VENTAS, COMISIONES E IMPUESTOS|OPERACIONES EXTRACASH|VENTA EN MONEDA EXTRANJERA", string.Join("|", ventas.Select(a => a.Concepto)));
            Afirmar.Igual(0, l.Asientos.Count(a => a.Concepto.StartsWith("ASIENTO ", StringComparison.OrdinalIgnoreCase)), "conceptos con prefijo");
            Afirmar.Igual(0, l.Asientos.Count(a => a.Concepto.StartsWith("Por operaciones de extracash", StringComparison.Ordinal) || a.Concepto.StartsWith("USD ", StringComparison.Ordinal)), "notas tomadas por asiento");
        });
        test("VARIOS real: el código con comas «4,01,05,050» llega tal cual (A2): una sola línea, ASSAL en COMPRAS de mayo, asiento Caja, fila 70", () =>
        {
            // El prompt habla de «3 líneas» pero son tres líneas de la cuenta ASSAL y sólo la de mayo tiene comas: las de julio y agosto ya traen puntos
            // (el TXT hecho a mano lo confirma). Acá se verifica lo que de verdad hay en el archivo.
            var todas = Varios().Asientos.SelectMany(a => a.Lineas.Select(x => (a, x))).ToList();
            var conComa = todas.Where(t => t.x.Codigo.Contains(',')).ToList();
            Afirmar.Igual(1, conComa.Count, "líneas con coma");
            var (asiento, linea) = conComa[0];
            Afirmar.Igual("4,01,05,050", linea.Codigo); Afirmar.Igual("ASSAL", linea.Nombre); Afirmar.Igual(70, linea.Fila); Afirmar.Igual(179067.47m, linea.Debe);
            Afirmar.Igual("COMPRAS", asiento.Hoja); Afirmar.Igual("Caja", asiento.Concepto); Afirmar.Igual(new Mes(2026, 5), asiento.Mes!.Value);
            Afirmar.Igual(3, todas.Count(t => t.x.Nombre == "ASSAL"), "líneas de la cuenta ASSAL");
        });
        test("VARIOS real: los importes de 4 a 6 decimales se leen sin perder precisión (los totales redondeados por línea dan los de §7 con a lo sumo 0,02 de diferencia)", () =>
        {
            var reg = Varios().Asientos.Where(a => a.Estado == EstadoAsiento.Registrable).ToList();
            var hasta08 = reg.Where(a => a.Mes!.Value.CompareTo(new Mes(2026, 8)) <= 0).ToList();
            Afirmar.Verdad(Math.Abs(hasta08.Sum(a => a.Lineas.Sum(x => Redondeado(x.Debe))) - 71914422835.05m) <= 0.02m, "Debe 05–08");
            Afirmar.Verdad(Math.Abs(hasta08.Sum(a => a.Lineas.Sum(x => Redondeado(x.Haber))) - 71914422835.05m) <= 0.02m, "Haber 05–08");
            Afirmar.Verdad(Math.Abs(reg.Sum(a => a.Lineas.Sum(x => Redondeado(x.Debe))) - 74568487094.93m) <= 0.02m, "Debe 05–09");
            Afirmar.Verdad(Math.Abs(reg.Sum(a => a.Lineas.Sum(x => Redondeado(x.Haber))) - 74568487094.93m) <= 0.02m, "Haber 05–09");
        });
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
