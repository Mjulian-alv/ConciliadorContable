// Fecha: 13/11/2026 - TAREA: 00180 - Linea: 5 - Modelo de las alertas (§6): severidad, ubicación exacta, causa, acción y cómo se resolvió.
// Cambios respecto del prompt, decididos por el dueño del pedido (guía, «Cambios respecto del prompt»): A1 es advertencia, no hay mapeo
// nombre → cuenta y A14 (plan de cuentas) queda fuera. «M1» (asiento sin mes) no está en el catálogo del prompt: sin mes no hay fecha para el TXT.
namespace AsientosEstudio.Core;

public enum Severidad { Bloqueante, Advertencia, Informativa }

/// <summary>Cómo terminó una alerta. Las bloqueantes impiden generar hasta quedar <see cref="Ajustada"/> o <see cref="Aceptada"/>.</summary>
public enum ResolucionAlerta
{
    /// <summary>Abierta: nadie la resolvió.</summary>
    Ninguna,
    /// <summary>El código se normalizó solo («4,01,05,050» → «4.01.05.050»).</summary>
    Normalizada,
    /// <summary>Se ajustó un centavo en una línea (solo, por la regla A5, o a pedido del usuario en A6).</summary>
    Ajustada,
    /// <summary>La fila no es un movimiento y se ignoró (A3).</summary>
    Ignorada,
    /// <summary>El usuario aceptó generar igual. Queda registrado quién.</summary>
    Aceptada,
}

/// <summary>Dónde está un asiento: la hoja y la fila de su encabezado en el Excel. Es único por libro.</summary>
public readonly record struct ClaveAsiento(string Hoja, int FilaEncabezado);

public sealed record Alerta
{
    /// <summary>«A1»…«A16», o «M1».</summary>
    public required string Codigo { get; init; }
    public required Severidad Severidad { get; init; }
    public required string Titulo { get; init; }
    public required string Causa { get; init; }
    public required string Accion { get; init; }

    public string? Hoja { get; init; }
    public Mes? Mes { get; init; }
    public ClaveAsiento? Asiento { get; init; }
    /// <summary>El concepto o el encabezado del asiento, para mostrarlo.</summary>
    public string? AsientoTexto { get; init; }
    public int? Fila { get; init; }
    public string? Cuenta { get; init; }
    /// <summary>Lo concreto: «RICARDO +0,01», «celda D118», «2 líneas».</summary>
    public string? Detalle { get; init; }

    /// <summary>Solo A6: Debe − Haber del asiento, redondeado por línea. Positivo = sobra Debe.</summary>
    public decimal? Diferencia { get; init; }

    public ResolucionAlerta Resolucion { get; init; } = ResolucionAlerta.Ninguna;
    public string? ResueltaPor { get; init; }

    /// <summary>Bloquea la generación: es bloqueante y nadie la resolvió ni la aceptó.</summary>
    public bool BloqueaGeneracion => Severidad == Severidad.Bloqueante && Resolucion is ResolucionAlerta.Ninguna;
}

public enum EstadoVista { Ok, Advertencia, Error, Omitido }

/// <summary>Una línea tal como va al TXT: código normalizado e importes ya redondeados a 2 decimales (y ajustados, si hubo ajuste de centavo).</summary>
public sealed record LineaCalculada(LineaLeida Origen, string Codigo, decimal Debe, decimal Haber, decimal Ajuste)
{
    public bool Ajustada => Ajuste != 0m;
}

public sealed class AsientoCalculado
{
    public required AsientoLeido Origen { get; init; }
    public required EstadoVista Estado { get; init; }

    /// <summary>Solo las líneas que van al TXT. Vacío para los asientos omitidos o excluidos. No incluye las filas sueltas (A3).</summary>
    public IReadOnlyList<LineaCalculada> Lineas { get; init; } = Array.Empty<LineaCalculada>();

    public ClaveAsiento Clave => new(Origen.Hoja, Origen.FilaEncabezado);
    public decimal TotalDebe => Lineas.Sum(l => l.Debe);
    public decimal TotalHaber => Lineas.Sum(l => l.Haber);
    public decimal Diferencia => TotalDebe - TotalHaber;

    /// <summary>Entra al TXT: es registrable. Que tenga alertas bloqueantes lo decide el resultado completo.</summary>
    public bool Registra => Origen.Estado == EstadoAsiento.Registrable;
}

public sealed class ResultadoValidacion
{
    public required IReadOnlyList<AsientoCalculado> Asientos { get; init; }
    public required IReadOnlyList<Alerta> Alertas { get; init; }

    public bool HayBloqueantes => Alertas.Any(a => a.BloqueaGeneracion);
    public IEnumerable<Alerta> De(string codigo) => Alertas.Where(a => a.Codigo == codigo);
}

/// <summary>El usuario decide ajustar el centavo de un asiento descuadrado en el origen (A6, salida b) en la línea que elige.</summary>
public sealed record DecisionAjuste(ClaveAsiento Asiento, int FilaLinea, string Usuario);

/// <summary>El usuario acepta generar igual con una alerta bloqueante (A6 salida c, o cualquier otra que lo permita). Queda registrado quién.</summary>
public sealed record DecisionAceptar(string Codigo, ClaveAsiento Asiento, string Usuario);
