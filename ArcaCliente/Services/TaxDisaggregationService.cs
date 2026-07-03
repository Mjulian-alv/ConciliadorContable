using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Resultado de la desagregación de impuestos de un comprobante ARCA.
    /// </summary>
    public class TaxDisaggregationResult
    {
        /// <summary>Tasas construidas y listas para insertar en <c>PV_P_TASAS0</c>.</summary>
        public List<OctosisDocumentoTasa> Tasas { get; set; } = new();

        /// <summary>
        /// Tasas que no pudieron resolverse porque no existe parametrización en <c>MI_I_PARCF0</c>
        /// con <c>par_habmpv = 'S'</c> para ese porcentaje.
        /// Si la lista no está vacía, el documento <b>no se puede dar de alta</b>.
        /// </summary>
        public List<string> TasasFaltantes { get; set; } = new();

        /// <summary>
        /// Avisos sobre tributos que no pueden desagregarse automáticamente (ej: <c>OtrosTributos</c>).
        /// No impiden el alta, pero se informa al usuario.
        /// </summary>
        public List<string> Avisos { get; set; } = new();
    }

    /// <summary>
    /// Desagrega los impuestos de un <see cref="ComprobanteCsv"/> (ARCA) en una lista de
    /// <see cref="OctosisDocumentoTasa"/> lista para persistir en <c>PV_P_TASAS0</c>.
    /// <para>
    /// Implementa el PASO 6 del roadmap (§12 del análisis).
    /// </para>
    /// <para>
    /// Por cada par (neto gravado, monto IVA, porcentaje) con valor &gt; 0, busca la tasa
    /// correspondiente en <c>MI_I_PARCF0</c> (via <see cref="OctosisImpuestosService"/>).
    /// Si no está parametrizada con <c>par_habmpv = 'S'</c>, agrega una entrada a
    /// <see cref="TaxDisaggregationResult.TasasFaltantes"/>.
    /// </para>
    /// <para>
    /// <c>OtrosTributos</c> no se puede desagregar: se agrega un aviso sin bloquear el alta.
    /// </para>
    /// </summary>
    public class TaxDisaggregationService
    {
        private readonly OctosisImpuestosService _impuestosService;

        // Pares (porcentaje, selector-neto, selector-iva) sobre ComprobanteCsv
        private static readonly (decimal Porcentaje, Func<ComprobanteCsv, string?> Neto, Func<ComprobanteCsv, string?> Iva)[] Pares =
        {
            (21m,   c => c.ImpNetoGravadoIVA21,  c => c.Iva21),
            (10.5m, c => c.ImpNetoGravadoIVA105, c => c.Iva105),
            (27m,   c => c.ImpNetoGravadoIVA27,  c => c.Iva27),
            (5m,    c => c.ImpNetoGravadoIVA5,   c => c.Iva5),
            (2.5m,  c => c.ImpNetoGravadoIVA25,  c => c.Iva25),
        };

        public TaxDisaggregationService(OctosisImpuestosService impuestosService)
        {
            _impuestosService = impuestosService ?? throw new ArgumentNullException(nameof(impuestosService));
        }

        /// <summary>
        /// Desagrega los impuestos del comprobante ARCA y construye la lista de tasas para Octosis.
        /// </summary>
        /// <param name="csv">Comprobante descargado del portal ARCA.</param>
        /// <param name="moneda">Información de moneda resuelta por <see cref="MonedaResolver"/>.</param>
        /// <param name="tipoDocumento">Tipo Octosis del comprobante ("1", "2", "3").</param>
        /// <param name="sucursal">Sucursal de la instalación (<see cref="OctosisConfig.Sucursal"/>).</param>
        /// <param name="conn">Conexión abierta a la base Octosis.</param>
        public async Task<TaxDisaggregationResult> DesagregarAsync(
            ComprobanteCsv csv,
            MonedaInfo moneda,
            string tipoDocumento,
            string sucursal,
            IDbConnection conn)
        {
            if (csv is null)    throw new ArgumentNullException(nameof(csv));
            if (moneda is null) throw new ArgumentNullException(nameof(moneda));

            var result = new TaxDisaggregationResult();

            // 1. IVA por porcentaje
            foreach (var (porc, netoFn, ivaFn) in Pares)
            {
                decimal neto = ParseDecimal(netoFn(csv));
                decimal iva  = ParseDecimal(ivaFn(csv));

                if (neto == 0m && iva == 0m)
                    continue;

                var tasa = await _impuestosService.ObtenerPorPorcentajeAsync(porc, conn);
                if (tasa == null)
                {
                    result.TasasFaltantes.Add($"IVA {porc}% (neto: {neto:N2}, IVA: {iva:N2})");
                    continue;
                }

                result.Tasas.Add(ConstruirTasa(tasa, tipoDocumento, sucursal, neto, iva, moneda));
            }

            // 2. Exento / No Gravado (par_porcen = 0, si está parametrizado)
            decimal exento    = ParseDecimal(csv.ImpOpExentas);
            decimal noGravado = ParseDecimal(csv.ImpNetoNoGravado);
            if (exento > 0m || noGravado > 0m)
            {
                var tasaExenta = await _impuestosService.ObtenerPorPorcentajeAsync(0m, conn);
                if (tasaExenta != null)
                    result.Tasas.Add(ConstruirTasa(tasaExenta, tipoDocumento, sucursal,
                        exento + noGravado, 0m, moneda));
                // Si no existe tasa 0% no se bloquea: es opcional
            }

            // 3. OtrosTributos: se imputa a la línea 70 (Tasa 0%) sólo al campo Importe.
            decimal otros = ParseDecimal(csv.OtrosTributos);
            if (otros > 0m)
            {
                var tasaOtros = await _impuestosService.ObtenerPorLineaAsync("70", conn);
                if (tasaOtros != null)
                {
                    result.Tasas.Add(ConstruirTasa(tasaOtros, tipoDocumento, sucursal, 0m, otros, moneda));
                }
                else
                {
                    result.TasasFaltantes.Add($"Otros Tributos (Impuesto: {otros:N2}) - Falta parametrizar línea '70'.");
                }
            }

            return result;
        }

        // ?? Helpers ?????????????????????????????????????????????????????????????

        private static OctosisDocumentoTasa ConstruirTasa(
            OctosisTasa tasa,
            string tipoDocumento,
            string sucursal,
            decimal neto,
            decimal importe,
            MonedaInfo moneda)
        {
            var t = new OctosisDocumentoTasa
            {
                TipoDocumento      = tipoDocumento,
                NumeroSucursal     = sucursal,
                Linea              = tasa.NumeroLinea,
                Porcentaje         = tasa.Porcentaje,
                Neto               = neto,
                Importe            = importe,
                Descripcion        = tasa.Descripcion,
                ColumnaLibroIva    = tasa.ColumnaLibroIva,
                Resta              = tasa.Resta,
                TotalizaSeparado   = tasa.TotalizaSeparado,
                EsExento           = !tasa.EsIvaGeneral && tasa.Porcentaje == 0m,
                DocumentoEnDolares = moneda.EsMonedaExtranjera,
                TipoCambio         = moneda.TipoCambio,
            };

            if (moneda.EsMonedaExtranjera && moneda.TipoCambio > 0m)
            {
                t.ImporteDolares = moneda.ConvertirDesdePesos(importe);
                t.NetoDolares    = moneda.ConvertirDesdePesos(neto);
            }

            return t;
        }

        /// <summary>
        /// Parsea un campo decimal del CSV de ARCA.
        /// Soporta formato invariant (punto decimal), es-AR (coma decimal) y combinaciones.
        /// </summary>
        public static decimal ParseDecimal(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return 0m;

            // Remove any trailing internal representations, e.g. handle valid numbers
            valor = valor.Trim().Replace(".", ",");

            // Look for negative signs
            bool esNegativo = valor.StartsWith("-");
            if (esNegativo) valor = valor.Substring(1);

            if (decimal.TryParse(valor, NumberStyles.Number, CultureInfo.GetCultureInfo("es-AR"), out var r))
                return esNegativo ? -r : r;

            if (decimal.TryParse(valor.Replace(",", "."), NumberStyles.Number, CultureInfo.InvariantCulture, out r))
                return esNegativo ? -r : r;

            return 0m;
        }
    }
}
