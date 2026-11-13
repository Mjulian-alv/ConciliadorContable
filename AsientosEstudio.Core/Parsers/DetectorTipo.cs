// Fecha: 13/11/2026 - TAREA: 00180 - Linea: 4 - Detección del tipo de planilla (§2) y fábrica de parsers.
// Cada tipo tiene una firma propia que los otros no comparten: la fila de cierre. BANCOS cierra con «SUMATORIA DEL ASIENTO», VARIOS con
// «TOTAL ASIENTO» / «TOTAL DEL ASIENTO» y DEVENGAMIENTOS con «TOTAL» a secas, en hojas que se llaman como un mes. No se prueba cada parser contra el libro
// para ver cuál «encuentra más»: ParserBancos, por ejemplo, también encuentra asientos en la hoja VENTAS de un VARIOS, y daría una detección engañosa.
namespace AsientosEstudio.Core;

public sealed record Deteccion(TipoPlanilla? Tipo, IReadOnlyDictionary<TipoPlanilla, int> Puntajes, string Evidencia);

public static class DetectorTipo
{
    public static Deteccion Detectar(LibroCrudo libro)
    {
        int bancos = 0, varios = 0, devengamientos = 0;
        foreach (var h in libro.Hojas)
        {
            if (Meses.TryParse(h.Nombre, out _) && h.Nombre.Trim().Length <= 8) devengamientos++; // una hoja por mes, nombrada «MM-AAAA»
            for (var f = 1; f <= h.UltimaFila; f++)
            {
                var a = Texto.Normalizar(h.Texto(f, 1));
                var b = Texto.Normalizar(h.Texto(f, 2));
                if (b == "SUMATORIA DEL ASIENTO") bancos++;
                else if (a == "TOTAL ASIENTO" || b == "TOTAL DEL ASIENTO" || a == "TOTAL DEL ASIENTO") varios++;
                else if (a == "TOTAL" || b == "TOTAL") devengamientos++;
            }
        }

        var puntajes = new Dictionary<TipoPlanilla, int>
        {
            [TipoPlanilla.Bancos] = bancos, [TipoPlanilla.Devengamientos] = devengamientos, [TipoPlanilla.Varios] = varios,
        };
        var mejor = puntajes.OrderByDescending(kv => kv.Value).First();
        if (mejor.Value == 0) return new Deteccion(null, puntajes, "No se reconoció ningún asiento: faltan las filas de cierre («SUMATORIA DEL ASIENTO», «TOTAL DEL ASIENTO» o «TOTAL»).");

        var evidencia = mejor.Key switch
        {
            TipoPlanilla.Bancos => $"{mejor.Value} filas «SUMATORIA DEL ASIENTO» en {libro.Hojas.Count} hojas.",
            TipoPlanilla.Varios => $"{mejor.Value} filas «TOTAL ASIENTO» / «TOTAL DEL ASIENTO».",
            _ => $"Hojas con nombre de mes y {mejor.Value} filas de cierre «TOTAL».",
        };
        return new Deteccion(mejor.Key, puntajes, evidencia);
    }
}

public static class Parsers
{
    public static IAsientoParser Para(TipoPlanilla tipo) => tipo switch
    {
        TipoPlanilla.Bancos => new ParserBancos(),
        TipoPlanilla.Devengamientos => new ParserDevengamientos(),
        TipoPlanilla.Varios => new ParserVarios(),
        _ => throw new ArgumentOutOfRangeException(nameof(tipo)),
    };

    /// <summary>Lee el libro con el parser del tipo dado. El tipo se detecta antes y el usuario puede cambiarlo.</summary>
    public static LibroLeido Leer(LibroCrudo libro, TipoPlanilla tipo) =>
        new() { Tipo = tipo, Nombre = libro.Nombre, HashExcel = libro.HashExcel, Hojas = Para(tipo).Leer(libro) };
}
