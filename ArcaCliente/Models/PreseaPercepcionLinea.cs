using System.ComponentModel;

namespace ArcaCliente.Models
{
    /// <summary>
    /// Linea del desglose de "Otros Tributos" (ARCA) hacia los campos de percepcion/impuesto
    /// del layout PRESEA (Sobretasa, Percepcion IB/IM/IV/IN, Percepcion configurable 1/2,
    /// Impuestos internos). Se edita en la grilla de la ventana de revision (FormCompletarPresea).
    /// </summary>
    public class PreseaPercepcionLinea
    {
        /// <summary>Descripcion de la linea (ej. "Otros Tributos (ARCA)", "Diferencia / Restante").</summary>
        public string Concepto { get; set; } = string.Empty;

        /// <summary>
        /// Campo PRESEA de destino: uno de los codigos de <see cref="Services.PreseaCalculos.CamposPercepcion"/>
        /// ("Sobretasa", "IB", "IM", "IV", "IN", "Config1", "Config2", "ImpuestosInternos").
        /// Vacio = sin asignar todavia (bloquea la confirmacion).
        /// </summary>
        public string CampoDestino { get; set; } = string.Empty;

        public decimal Importe { get; set; }

        [Browsable(false)]
        public bool EsDiferencia => Concepto == DiferenciaConcepto;

        public const string DiferenciaConcepto = "Diferencia / Restante";
    }
}
