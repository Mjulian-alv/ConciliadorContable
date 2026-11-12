// Fecha: 12/11/2026 - TAREA: 00180 - Linea: 1 - Modelo en memoria de lo que se lee de la planilla: libro → hoja → asiento → línea.
// Los importes son decimal, siempre, y se guardan SIN redondear: el redondeo a 2 decimales y la regla del centavo (A5/A6)
// se aplican después, sobre la suma sin redondear, así que acá no se pierde nada de lo que trae el Excel.
using System.Globalization;

namespace AsientosEstudio.Core;

public enum TipoPlanilla { Bancos, Devengamientos, Varios }

public enum EstadoAsiento
{
    /// <summary>Entra al TXT y consume número correlativo.</summary>
    Registrable,
    /// <summary>«INFORMATIVO — NO SE REGISTRA» en el encabezado (A7): se lista y no se valida su balance.</summary>
    InformativoExcluido,
    /// <summary>Sin líneas o todas en cero (A8): se lista y no consume número.</summary>
    VacioOmitido,
    /// <summary>Bloque «no se registra» / del ejercicio anterior (A10).</summary>
    NoSeRegistra,
}

/// <summary>Un mes calendario. Orden natural: por año y mes.</summary>
public readonly record struct Mes(int Anio, int Numero) : IComparable<Mes>
{
    /// <summary>Último día real del mes (28/29 en febrero según el año). Es la fecha que lleva cada asiento en el TXT.</summary>
    public DateTime UltimoDia => new(Anio, Numero, DateTime.DaysInMonth(Anio, Numero));

    /// <summary>«05/2026».</summary>
    public string Texto => Numero.ToString("00", CultureInfo.InvariantCulture) + "/" + Anio.ToString(CultureInfo.InvariantCulture);

    /// <summary>«05-2026»: el prefijo del concepto en BANCOS y el nombre de hoja en DEVENGAMIENTOS.</summary>
    public string TextoGuion => Numero.ToString("00", CultureInfo.InvariantCulture) + "-" + Anio.ToString(CultureInfo.InvariantCulture);

    public int CompareTo(Mes otro) => Anio != otro.Anio ? Anio.CompareTo(otro.Anio) : Numero.CompareTo(otro.Numero);

    public override string ToString() => Texto;
}

/// <summary>Una fila de cuenta de un asiento. <see cref="Fila"/> es el número de fila del Excel, para poder señalarla en las alertas.</summary>
public sealed record LineaLeida(int Fila, string Codigo, string Nombre, decimal Debe, decimal Haber)
{
    public bool SinCodigo => Codigo.Length == 0;

    /// <summary>Sin código y sin nombre: no es un movimiento (A3). Se conserva para poder avisarla, no se descarta en silencio.</summary>
    public bool FilaSuelta => Codigo.Length == 0 && Nombre.Length == 0;

    public bool EnCero => Debe == 0m && Haber == 0m;
}

public sealed class AsientoLeido
{
    public required string Hoja { get; init; }
    public Mes? Mes { get; init; }

    /// <summary>El número que trae el Excel («1» de «ASIENTO 1»), o vacío si el encabezado no lo tiene.</summary>
    public string NumeroOrigen { get; init; } = "";

    /// <summary>El texto completo del encabezado tal como está en la planilla.</summary>
    public required string Titulo { get; init; }

    /// <summary>La columna Concepto del TXT. Depende del tipo de planilla (§4).</summary>
    public required string Concepto { get; init; }

    public required EstadoAsiento Estado { get; init; }
    public int FilaEncabezado { get; init; }
    public IReadOnlyList<LineaLeida> Lineas { get; init; } = Array.Empty<LineaLeida>();

    public decimal TotalDebe => Lineas.Sum(l => l.Debe);
    public decimal TotalHaber => Lineas.Sum(l => l.Haber);
}

/// <summary>Una hoja del Excel con sus asientos en el orden en que aparecen. El orden de las hojas es el orden de salida del TXT.</summary>
public sealed class HojaLeida
{
    public required string Nombre { get; init; }
    public int Orden { get; init; }
    public IReadOnlyList<AsientoLeido> Asientos { get; init; } = Array.Empty<AsientoLeido>();
}

public sealed class LibroLeido
{
    public required TipoPlanilla Tipo { get; init; }
    public required string Nombre { get; init; }

    /// <summary>SHA-256 del .xlsx, en minúsculas. Sirve para el historial y para detectar «ya se generó» (A16).</summary>
    public required string HashExcel { get; init; }

    public IReadOnlyList<HojaLeida> Hojas { get; init; } = Array.Empty<HojaLeida>();

    public IEnumerable<AsientoLeido> Asientos => Hojas.SelectMany(h => h.Asientos);
}
