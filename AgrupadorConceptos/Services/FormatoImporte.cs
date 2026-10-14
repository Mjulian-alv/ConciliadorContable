// Fecha: 14/10/2026 - TAREA: 00097 - Linea: 7 - Un único formateador de importes para toda la web del Agrupador: miles y decimales, en absoluto.
using System;
using System.Globalization;

namespace AgrupadorConceptos.Services
{
    /// <summary>
    /// Presentación de importes de movimientos: valor absoluto, con separador de miles (punto) y dos decimales (coma).
    /// El formato es fijo y no depende de la cultura del servidor ni del navegador: se arma con un
    /// NumberFormatInfo propio, que además funciona con la globalización invariante (contenedores).
    /// Es sólo presentación: lo que se guarda en la base no cambia.
    /// </summary>
    public static class FormatoImporte
    {
        private static readonly NumberFormatInfo Formato = new NumberFormatInfo
        {
            NumberGroupSeparator = ".",
            NumberDecimalSeparator = ",",
            NumberGroupSizes = new[] { 3 },
            NumberDecimalDigits = 2,
        };

        /// <summary>1234567.891 → "1.234.567,89"; negativo → sin signo; nulo → en blanco.</summary>
        public static string Absoluto(decimal? importe)
        {
            if (importe == null) return "";
            return Math.Round(Math.Abs(importe.Value), 2, MidpointRounding.AwayFromZero).ToString("N2", Formato);
        }

        /// <summary>El cero se muestra "0,00" pero la pantalla lo atenúa: sirve para decidirlo.</summary>
        public static bool EsCero(decimal? importe) =>
            importe != null && Math.Round(Math.Abs(importe.Value), 2, MidpointRounding.AwayFromZero) == 0m;

        /// <summary>
        /// La diferencia (créditos − débitos) es un resultado, no un importe de movimiento: conserva el signo.
        /// "+1.951.317,66", "−3.100,00" (signo menos tipográfico) y "0,00".
        /// </summary>
        public static string Diferencia(decimal diferencia)
        {
            decimal redondeada = Math.Round(diferencia, 2, MidpointRounding.AwayFromZero);
            string texto = Absoluto(redondeada);
            return redondeada > 0 ? "+" + texto : redondeada < 0 ? "−" + texto : texto;
        }
    }
}
