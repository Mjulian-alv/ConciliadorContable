using System;

namespace ArcaCliente.Models
{
    /// <summary>
    /// Resultado de la resolución de moneda de un comprobante ARCA.
    /// <para>
    /// ARCA siempre expresa los importes en pesos argentinos, incluso para comprobantes
    /// emitidos en moneda extranjera (los convierte usando el TC declarado en el comprobante).
    /// Esta clase captura esa información para que el formulario de procesamiento
    /// pueda pedir el TC al usuario cuando no esté disponible en el CSV.
    /// </para>
    /// </summary>
    public class MonedaInfo
    {
        /// <summary>
        /// True cuando el comprobante está emitido en moneda extranjera.
        /// En ese caso los importes del CSV ya vienen en pesos (convertidos por ARCA).
        /// </summary>
        public bool EsMonedaExtranjera { get; set; }

        /// <summary>
        /// Código de moneda tal como viene del CSV de ARCA: "PES", "DOL", "EUR", etc.
        /// </summary>
        public string CodigoMoneda { get; set; }

        /// <summary>
        /// Tipo de cambio extraído del campo TipoCambio del CSV.
        /// <list type="bullet">
        ///   <item>1 ? pesos (o TC 1:1, sin conversión).</item>
        ///   <item>0 ? no fue posible determinarlo; el usuario debe proveerlo antes del alta.</item>
        ///   <item>Mayor que 1 ? TC válido para moneda extranjera.</item>
        /// </list>
        /// </summary>
        public decimal TipoCambio { get; set; } = 1m;

        /// <summary>
        /// True si el TC fue extraído del CSV con un valor mayor a 1 (confiable para moneda extranjera).
        /// False cuando el usuario debe confirmar o ingresar el valor antes del alta.
        /// </summary>
        public bool TipoCambioConfirmado { get; set; }

        /// <summary>Descripción legible para mostrar en la UI, ej: "Dólar estadounidense".</summary>
        public string DescripcionMoneda { get; set; }

        // ?? Helpers de conversión ????????????????????????????????????????????????

        /// <summary>
        /// Devuelve el importe equivalente en moneda extranjera a partir de un importe en pesos.
        /// ARCA reporta todos los importes en pesos; esta operación revierte esa conversión.
        /// Retorna 0 si <see cref="TipoCambio"/> es 0.
        /// </summary>
        public decimal ConvertirDesdePesos(decimal importeEnPesos)
            => TipoCambio > 0 ? Math.Round(importeEnPesos / TipoCambio, 2) : 0m;

        /// <summary>
        /// Representación compacta para logging / tooltips.
        /// </summary>
        public override string ToString()
            => EsMonedaExtranjera
                ? $"{DescripcionMoneda} (TC: {(TipoCambio > 0 ? TipoCambio.ToString("F4") : "pendiente")})"
                : DescripcionMoneda;
    }
}
