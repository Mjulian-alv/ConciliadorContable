// Fecha: 13/11/2026 - TAREA: 00180 - Linea: 4 - Selección de meses (§5): qué meses trae el libro, con cuánto cada uno, y el rango que se esperaba.
using System.Text.RegularExpressions;

namespace AsientosEstudio.Core;

public sealed record ResumenMes(Mes Mes, int Asientos, int Lineas, decimal Debe, IReadOnlyList<string> Hojas);

public static class SeleccionMeses
{
    /// <summary>Los meses con asientos a registrar, en orden, con sus cantidades. Es lo que se le muestra al usuario para que elija el rango.</summary>
    public static IReadOnlyList<ResumenMes> Resumir(LibroLeido libro) =>
        libro.Hojas.SelectMany(h => h.Asientos.Where(a => a.Estado == EstadoAsiento.Registrable && a.Mes is not null).Select(a => (Hoja: h.Nombre, a)))
            .GroupBy(t => t.a.Mes!.Value).OrderBy(g => g.Key)
            .Select(g => new ResumenMes(g.Key, g.Count(), g.Sum(t => t.a.Lineas.Count(l => !l.FilaSuelta)),
                g.Sum(t => t.a.Lineas.Where(l => !l.FilaSuelta).Sum(l => Centavo.Redondear(l.Debe))), g.Select(t => t.Hoja).Distinct().ToList()))
            .ToList();

    /// <summary>Todos los meses que trae el libro: la selección por defecto.</summary>
    public static IReadOnlySet<Mes> Todos(LibroLeido libro) => Resumir(libro).Select(r => r.Mes).ToHashSet();

    /// <summary>Desde un mes hasta otro, inclusive. Sirve para armar «de 05/2026 a 08/2026».</summary>
    public static IReadOnlySet<Mes> Rango(LibroLeido libro, Mes desde, Mes hasta) =>
        Resumir(libro).Select(r => r.Mes).Where(m => m.CompareTo(desde) >= 0 && m.CompareTo(hasta) <= 0).ToHashSet();
}

public static class RangoHabitual
{
    // «05-2026 a 08-2026», «5/2026 al 8/2026», «05-2026 hasta 08-2026».
    private static readonly Regex Rango = new(@"(\d{1,2})\s*[-/]\s*(\d{4})\s+(?:a|al|hasta)\s+(\d{1,2})\s*[-/]\s*(\d{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// El rango que anuncia el nombre del archivo («ASIENTOS VARIOS 05-2026 a 08-2026.xlsx»), o null si no dice ninguno. Un mes del libro fuera de ese
    /// rango da la alerta informativa A9 y el usuario decide si lo incluye.
    /// </summary>
    public static (Mes Desde, Mes Hasta)? DesdeNombre(string? nombreArchivo)
    {
        if (string.IsNullOrWhiteSpace(nombreArchivo)) return null;
        var m = Rango.Match(nombreArchivo);
        if (!m.Success) return null;
        if (!Meses.TryParse($"{m.Groups[1].Value}/{m.Groups[2].Value}", out var desde) || !Meses.TryParse($"{m.Groups[3].Value}/{m.Groups[4].Value}", out var hasta)) return null;
        return desde.CompareTo(hasta) <= 0 ? (desde, hasta) : (hasta, desde);
    }
}
