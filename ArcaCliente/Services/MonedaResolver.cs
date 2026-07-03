using System;
using System.Collections.Generic;
using System.Globalization;
using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Resuelve la información de moneda de un comprobante ARCA a partir de los campos
    /// <c>Moneda</c> y <c>TipoCambio</c> del CSV.
    /// <para>
    /// ARCA siempre convierte los importes a pesos en el CSV, incluso para comprobantes
    /// emitidos en moneda extranjera.  Esta clase determina:
    /// <list type="bullet">
    ///   <item>Si el comprobante es en pesos o en moneda extranjera.</item>
    ///   <item>El tipo de cambio a usar:
    ///     <list type="bullet">
    ///       <item><b>Opción C</b>: si el CSV trae un TC &gt; 1, se usa directamente (confirmado).</item>
    ///       <item><b>Opción B</b>: si el TC no está disponible o es ? 1, se marca como pendiente
    ///            y el usuario deberá proveerlo antes del alta en Octosis.</item>
    ///     </list>
    ///   </item>
    /// </list>
    /// </para>
    /// <para>
    /// Mapeo resultante a campos Octosis (usado al construir <c>clsPVdocumentoTasa</c>):
    /// <list type="table">
    ///   <item><term>TAS_DOLARE</term><description>"S" si <see cref="MonedaInfo.EsMonedaExtranjera"/>, "N" si no.</description></item>
    ///   <item><term>TAS_CAMBIO</term><description><see cref="MonedaInfo.TipoCambio"/> (0 si pendiente).</description></item>
    ///   <item><term>TAS_IMPDOL</term><description><see cref="MonedaInfo.ConvertirDesdePesos"/>(TAS_IMPORT).</description></item>
    ///   <item><term>TAS_NETDOL</term><description><see cref="MonedaInfo.ConvertirDesdePesos"/>(TAS_NETO).</description></item>
    /// </list>
    /// </para>
    /// </summary>
    public static class MonedaResolver
    {
        /// <summary>
        /// Códigos de moneda que ARCA reporta como peso argentino.
        /// </summary>
        private static readonly HashSet<string> _monedaPesos =
            new(StringComparer.OrdinalIgnoreCase) { "PES", "ARS", "$" };

        /// <summary>
        /// Descripciones legibles por código de moneda (para mostrar en la UI).
        /// </summary>
        private static readonly Dictionary<string, string> _descripciones =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["PES"] = "Peso argentino",
                ["ARS"] = "Peso argentino",
                ["DOL"] = "Dólar estadounidense",
                ["USD"] = "Dólar estadounidense",
                ["EUR"] = "Euro",
                ["BRL"] = "Real brasileño",
                ["UYU"] = "Peso uruguayo",
                ["CHF"] = "Franco suizo",
                ["GBP"] = "Libra esterlina",
            };

        /// <summary>
        /// Resuelve la información de moneda de un comprobante ARCA.
        /// </summary>
        /// <param name="comprobante">Comprobante descargado del portal ARCA.</param>
        /// <returns>
        /// Un <see cref="MonedaInfo"/> con los datos necesarios para construir el documento en Octosis.
        /// <para>
        /// Si <see cref="MonedaInfo.TipoCambioConfirmado"/> es <c>false</c>, el formulario de
        /// procesamiento debe pedir el tipo de cambio al usuario antes de dar de alta el documento.
        /// </para>
        /// </returns>
        public static MonedaInfo Resolver(ComprobanteCsv comprobante)
        {
            if (comprobante is null)
                throw new ArgumentNullException(nameof(comprobante));

            var codigoMoneda = (comprobante.Moneda ?? string.Empty).Trim().ToUpperInvariant();

            // Código vacío o desconocido ? asumir pesos (caso más frecuente)
            if (string.IsNullOrEmpty(codigoMoneda) || _monedaPesos.Contains(codigoMoneda))
            {
                return new MonedaInfo
                {
                    EsMonedaExtranjera   = false,
                    CodigoMoneda         = string.IsNullOrEmpty(codigoMoneda) ? "PES" : codigoMoneda,
                    TipoCambio           = 1m,
                    TipoCambioConfirmado = true,
                    DescripcionMoneda    = "Peso argentino",
                };
            }

            // Moneda extranjera: intentar leer el TC del CSV (Opción C)
            var tcCsv = ParseTipoCambio(comprobante.TipoCambio);
            bool tcValido = tcCsv > 1m;

            return new MonedaInfo
            {
                EsMonedaExtranjera   = true,
                CodigoMoneda         = codigoMoneda,
                TipoCambio           = tcValido ? tcCsv : 0m,
                TipoCambioConfirmado = tcValido,
                DescripcionMoneda    = _descripciones.TryGetValue(codigoMoneda, out var desc)
                                           ? desc
                                           : $"Moneda extranjera ({codigoMoneda})",
            };
        }

        // ?? Internals ?????????????????????????????????????????????????????????????

        /// <summary>
        /// Parsea el campo TipoCambio del CSV.
        /// Intenta primero el formato argentino (punto como miles, coma como decimal)
        /// y luego el formato invariante como fallback.
        /// </summary>
        private static decimal ParseTipoCambio(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return 0m;

            if (decimal.TryParse(valor, NumberStyles.Any,
                    CultureInfo.GetCultureInfo("es-AR"), out var resultAr))
                return resultAr;

            if (decimal.TryParse(valor, NumberStyles.Any,
                    CultureInfo.InvariantCulture, out var resultInv))
                return resultInv;

            return 0m;
        }
    }
}
