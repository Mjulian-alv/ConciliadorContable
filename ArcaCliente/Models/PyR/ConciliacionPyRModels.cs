// Fecha: 21/09/2026 - TAREA: 00041 - Linea: 7 - Resultado de la conciliación de percepciones y retenciones
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcaCliente.Models
{
    public enum EstadoConciliacionPyR
    {
        Conciliado        = 0,
        DiferenciaImporte = 1,
        SoloArca          = 2,
        SoloPresea        = 3,
        AnuladaPresea     = 4
    }

    public class ItemConciliacionPyR
    {
        public EstadoConciliacionPyR Estado     { get; set; }
        public CuentaPyR             Cuenta     { get; set; }
        /// <summary>Número de la directiva que emparejó (0 si no emparejó).</summary>
        public int                   Directiva  { get; set; }
        public string                DescripcionDirectiva { get; set; }
        public RegistroArcaPyR       Arca       { get; set; }
        public RegistroPreseaPyR     Presea     { get; set; }
        public decimal?              Diferencia { get; set; }

        // ── Propiedades planas para la grilla (RadGridView enlaza por nombre) ─────
        public string    EstadoTexto     => NombreEstado(Estado);
        public string    CuentaNombre    => Cuenta?.Nombre;
        public string    DirectivaTexto  => Directiva > 0 ? Directiva.ToString() : string.Empty;
        public DateTime? FechaArca       => Arca?.Fecha;
        public string    Cuit            => Arca?.Cuit;
        public string    Denominacion    => Arca?.Denominacion;
        public string    Tipo            => Arca?.TipoComprobante ?? Presea?.TipoComprobante;
        public string    NumeroArca      => Arca?.Numero;
        public decimal?  ImporteArca     => Arca?.Importe;
        public DateTime? FechaPresea     => Presea?.Fecha;
        public string    Asiento         => Presea?.Asiento;
        public string    NumeroPresea    => Presea?.Numero;
        public string    Proveedor       => Presea == null ? null
                                          : Presea.SinComprobante ? Presea.ConceptoOriginal : Presea.Proveedor;
        public decimal?  ImportePresea   => Presea?.Importe;

        public static string NombreEstado(EstadoConciliacionPyR e) => e switch
        {
            EstadoConciliacionPyR.Conciliado        => "Conciliado",
            EstadoConciliacionPyR.DiferenciaImporte => "Diferencia de importe",
            EstadoConciliacionPyR.SoloArca          => "Sólo ARCA",
            EstadoConciliacionPyR.SoloPresea        => "Sólo PRESEA",
            EstadoConciliacionPyR.AnuladaPresea     => "Anulada en PRESEA",
            _                                       => e.ToString()
        };
    }

    /// <summary>
    /// Colores por estado para la grilla y el Excel: los mismos RGB de Offline
    /// (<see cref="EstadoConciliacionColores"/>) y gris para las anuladas, que Offline no tiene.
    /// </summary>
    public static class EstadoConciliacionPyRColores
    {
        public static (byte R, byte G, byte B) Rgb(EstadoConciliacionPyR e) => e switch
        {
            EstadoConciliacionPyR.Conciliado        => EstadoConciliacionColores.Rgb[EstadoConciliacion.Conciliado],
            EstadoConciliacionPyR.DiferenciaImporte => EstadoConciliacionColores.Rgb[EstadoConciliacion.DiferenciaImporte],
            EstadoConciliacionPyR.SoloArca          => EstadoConciliacionColores.Rgb[EstadoConciliacion.SoloARCA],
            EstadoConciliacionPyR.SoloPresea        => EstadoConciliacionColores.Rgb[EstadoConciliacion.SoloSistema],
            _                                       => (228, 228, 228)
        };
    }

    /// <summary>Una fila del resumen por cuenta.</summary>
    public class ResumenCuentaPyR
    {
        public CuentaPyR Cuenta       { get; set; }
        public int       Conciliados  { get; set; }
        public int       Diferencias  { get; set; }
        public int       SoloArca     { get; set; }
        public int       SoloPresea   { get; set; }
        public int       Anuladas     { get; set; }
        public decimal   TotalArca    { get; set; }
        public decimal   TotalPresea  { get; set; }

        public string  CuentaTexto => Cuenta?.ToString();
        public decimal Diferencia  => TotalArca - TotalPresea;
    }

    public class ResultadoConciliacionPyR
    {
        public List<ItemConciliacionPyR> Items { get; } = new();

        /// <summary>"IIBB · Retención" → cantidad de filas de ARCA sin mayor cargado para ese grupo.</summary>
        public Dictionary<string, int> FueraDeAlcance { get; } = new();

        /// <summary>Filas de ARCA con importe 0 dentro del alcance, excluidas (decisión del usuario).</summary>
        public int ArcaImporteCero { get; set; }

        /// <summary>Directiva (número) → cantidad de pares que emparejó.</summary>
        public SortedDictionary<int, int> PorDirectiva { get; } = new();

        public List<ResumenCuentaPyR> Resumen => Items
            .GroupBy(i => i.Cuenta.Id)
            .Select(g => new ResumenCuentaPyR
            {
                Cuenta      = g.First().Cuenta,
                Conciliados = g.Count(i => i.Estado == EstadoConciliacionPyR.Conciliado),
                Diferencias = g.Count(i => i.Estado == EstadoConciliacionPyR.DiferenciaImporte),
                SoloArca    = g.Count(i => i.Estado == EstadoConciliacionPyR.SoloArca),
                SoloPresea  = g.Count(i => i.Estado == EstadoConciliacionPyR.SoloPresea),
                Anuladas    = g.Count(i => i.Estado == EstadoConciliacionPyR.AnuladaPresea),
                TotalArca   = g.Sum(i => i.Arca?.Importe ?? 0m),
                TotalPresea = g.Sum(i => i.Presea?.Importe ?? 0m)
            })
            .OrderBy(r => r.Cuenta.Codigo, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
