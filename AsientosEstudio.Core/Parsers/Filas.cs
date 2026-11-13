// Fecha: 12/11/2026 - TAREA: 00180 - Linea: 1,2,3 - Reconocedores de fila y borrador de asiento que comparten los tres parsers.
// Las planillas no tienen estructura rígida: todo se reconoce por texto (normalizado) y por el contexto de la fila, nunca por su número.
namespace AsientosEstudio.Core;

internal static class Filas
{
    /// <summary>Hay algo en alguna de las columnas A a D.</summary>
    public static bool TieneContenido(HojaCruda h, int fila) =>
        h.Texto(fila, 1).Length > 0 || h.Texto(fila, 2).Length > 0 || h.TieneImporte(fila, 3) || h.TieneImporte(fila, 4);

    /// <summary>La fila de títulos de las columnas: «Código | Cuenta / concepto | Debe | Haber» (también «Codigo» sin tilde, «Código de cuenta | Nombre de la cuenta»).</summary>
    public static bool EsTitulos(HojaCruda h, int fila)
    {
        var a = Texto.Normalizar(h.Texto(fila, 1));
        var b = Texto.Normalizar(h.Texto(fila, 2));
        return a.StartsWith("CODIGO", StringComparison.Ordinal) && (b.StartsWith("CUENTA", StringComparison.Ordinal) || b.StartsWith("NOMBRE", StringComparison.Ordinal));
    }

    /// <summary>La fila siguiente que tenga algo escrito, o 0 si no hay más.</summary>
    public static int SiguienteConContenido(HojaCruda h, int fila)
    {
        for (var f = fila + 1; f <= h.UltimaFila; f++)
            if (TieneContenido(h, f)) return f;
        return 0;
    }

    /// <summary>Solo texto en la columna A: sin nombre, sin importes. Puede ser un encabezado de asiento, un título de hoja o una nota.</summary>
    public static bool EsSoloTextoEnA(HojaCruda h, int fila) =>
        h.Texto(fila, 1).Length > 0 && h.Texto(fila, 2).Length == 0 && !h.TieneImporte(fila, 3) && !h.TieneImporte(fila, 4);

    /// <summary>
    /// Encabezado de asiento por contexto: texto solo en A y, a continuación, la fila de títulos o directamente una línea con código.
    /// Lo que no tiene eso detrás (el título de la hoja, una nota al pie) no abre un asiento.
    /// </summary>
    public static bool EsEncabezadoPorContexto(HojaCruda h, int fila)
    {
        if (!EsSoloTextoEnA(h, fila) || Texto.PareceCodigo(h.Texto(fila, 1))) return false;
        var sig = SiguienteConContenido(h, fila);
        if (sig == 0) return false;
        return EsTitulos(h, sig) || Texto.PareceCodigo(h.Texto(sig, 1));
    }
}

/// <summary>Un asiento mientras se lee la hoja. Lo cierra el parser cuando aparece el cierre, otro encabezado o el fin de la hoja.</summary>
internal sealed class BorradorAsiento
{
    private readonly string _numero, _titulo, _concepto;
    private readonly int _filaEncabezado;
    private readonly Mes? _mes;
    private readonly bool _noSeRegistra;
    private readonly EstadoAsiento _estadoSiNoSeRegistra;

    public BorradorAsiento(string numero, string titulo, string concepto, int filaEncabezado, Mes? mes,
        bool noSeRegistra = false, EstadoAsiento estadoSiNoSeRegistra = EstadoAsiento.InformativoExcluido)
    {
        _numero = numero; _titulo = titulo; _concepto = concepto; _filaEncabezado = filaEncabezado; _mes = mes;
        _noSeRegistra = noSeRegistra; _estadoSiNoSeRegistra = estadoSiNoSeRegistra;
    }

    public List<LineaLeida> Lineas { get; } = new();

    public AsientoLeido Construir(string hoja)
    {
        // Un asiento con sus líneas todas en cero, o sin líneas, no se registra (A8). Un «no se registra» explícito manda sobre eso.
        var estado = _noSeRegistra ? _estadoSiNoSeRegistra
            : Lineas.All(l => l.EnCero) ? EstadoAsiento.VacioOmitido
            : EstadoAsiento.Registrable;
        return new AsientoLeido
        {
            Hoja = hoja, Mes = _mes, NumeroOrigen = _numero, Titulo = _titulo, Concepto = _concepto,
            Estado = estado, FilaEncabezado = _filaEncabezado, Lineas = Lineas,
        };
    }
}
