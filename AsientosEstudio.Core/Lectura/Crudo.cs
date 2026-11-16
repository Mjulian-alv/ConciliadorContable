// Fecha: 12/11/2026 - TAREA: 00180 - Linea: 1 - Lo que hay en el Excel, sin interpretar: una grilla de celdas por hoja.
// Los parsers trabajan sobre esto y no sobre ClosedXML: así se prueban con planillas armadas en el propio test, sin archivos.
using System.Globalization;

namespace AsientosEstudio.Core;

/// <summary>Una celda con contenido. Las celdas vacías no se guardan.</summary>
public sealed record CeldaCruda
{
    public string? Texto { get; init; }
    public decimal? Numero { get; init; }
    public DateTime? Fecha { get; init; }

    /// <summary>La celda es una fórmula (A11 mira estas).</summary>
    public bool TieneFormula { get; init; }
    public string? Formula { get; init; }

    /// <summary>Fórmula sin valor calculado guardado en el archivo: no se recalcula, se avisa (A11).</summary>
    public bool SinValorCalculado { get; init; }

    /// <summary>La celda devuelve un error de Excel («#REF!», «#VALUE!»…).</summary>
    public string? Error { get; init; }
}

public enum ProblemaImporte { Ninguno, FormulaSinValor, ErrorDeExcel, TextoNoNumerico }

public sealed class HojaCruda
{
    private readonly Dictionary<(int Fila, int Col), CeldaCruda> _celdas = new();

    public HojaCruda(string nombre, int orden = 0) { Nombre = nombre; Orden = orden; }

    public string Nombre { get; }
    public int Orden { get; }
    public int UltimaFila { get; private set; }

    public CeldaCruda? Celda(int fila, int col) => _celdas.TryGetValue((fila, col), out var c) ? c : null;

    public HojaCruda Poner(int fila, int col, CeldaCruda celda)
    {
        _celdas[(fila, col)] = celda;
        if (fila > UltimaFila) UltimaFila = fila;
        return this;
    }

    public HojaCruda Poner(int fila, int col, string texto) => Poner(fila, col, new CeldaCruda { Texto = texto });
    public HojaCruda Poner(int fila, int col, decimal numero) => Poner(fila, col, new CeldaCruda { Numero = numero });

    /// <summary>
    /// El contenido como texto, sin espacios en los extremos, o «» si la celda está vacía. Un número sale con punto decimal y sin
    /// separador de miles: el resultado no depende de la cultura de la máquina.
    /// </summary>
    public string Texto(int fila, int col)
    {
        var c = Celda(fila, col);
        if (c is null) return "";
        if (c.Texto is not null) return c.Texto.Trim();
        if (c.Numero is { } n) return n.ToString(CultureInfo.InvariantCulture);
        if (c.Fecha is { } f) return f.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return "";
    }

    /// <summary>El importe de la celda; las celdas vacías cuentan como 0 (§4). Si la celda no es un número válido, 0 y <see cref="Problema"/> lo informa.</summary>
    public decimal Importe(int fila, int col) => Celda(fila, col)?.Numero ?? 0m;

    /// <summary>Hay algo escrito en la celda de importe (aunque sea un 0 explícito).</summary>
    public bool TieneImporte(int fila, int col) => Celda(fila, col) is { } c && (c.Numero is not null || c.SinValorCalculado || c.Error is not null || !string.IsNullOrWhiteSpace(c.Texto));

    /// <summary>Por qué un importe no se pudo leer como número, para las alertas A11 sin tener que releer el archivo.</summary>
    public ProblemaImporte Problema(int fila, int col)
    {
        var c = Celda(fila, col);
        if (c is null || c.Numero is not null) return ProblemaImporte.Ninguno;
        if (c.Error is not null) return ProblemaImporte.ErrorDeExcel;
        if (c.SinValorCalculado) return ProblemaImporte.FormulaSinValor;
        return string.IsNullOrWhiteSpace(c.Texto) ? ProblemaImporte.Ninguno : ProblemaImporte.TextoNoNumerico;
    }
}

public sealed class LibroCrudo
{
    public required string Nombre { get; init; }
    public required string HashExcel { get; init; }
    public IReadOnlyList<HojaCruda> Hojas { get; init; } = Array.Empty<HojaCruda>();
}
