using System;
using System.Collections.Generic;
using System.Globalization;
using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Calculos derivados de un <see cref="ComprobanteCsv"/> de ARCA para la exportacion a
    /// PRESEA: mapeo de moneda, parseo de importes, sucursal+numero, fecha y resolucion de
    /// los dos slots de IVA. Compartido por el generador TXT (Fase 5).
    /// </summary>
    public static class PreseaCalculos
    {
        private static readonly CultureInfo _ar = CultureInfo.GetCultureInfo("es-AR");

        // ARCA moneda -> codigo PRESEA (provisorio; reemplazar por la tabla maestra de monedas).
        private static readonly Dictionary<string, int> _monedas =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["PES"] = 1, ["ARS"] = 1, ["$"] = 1,
                ["DOL"] = 2, ["USD"] = 2,
                ["EUR"] = 3, ["BRL"] = 4, ["UYU"] = 5,
            };

        public static int MapMoneda(string codigoArca)
        {
            if (string.IsNullOrWhiteSpace(codigoArca)) return 1;
            return _monedas.TryGetValue(codigoArca.Trim(), out var c) ? c : 1;
        }

        /// <summary>
        /// Campos de percepcion/impuesto del layout PRESEA hacia los que se puede redistribuir
        /// "Otros Tributos" (ARCA): codigo interno (<see cref="Models.PreseaPercepcionLinea.CampoDestino"/>)
        /// y descripcion para mostrar en el combo de la grilla de revision.
        /// </summary>
        public static readonly (string Codigo, string Descripcion)[] CamposPercepcion =
        {
            ("IB",               "Percepcion IIBB"),
            ("IM",               "Percepcion Municipal"),
            ("IV",               "Percepcion IVA"),
            ("IN",               "Percepcion Ingresos Nacional"),
            ("Config1",          "Percepcion configurable 1"),
            ("Config2",          "Percepcion configurable 2"),
            ("Sobretasa",        "Sobretasa"),
            ("ImpuestosInternos","Impuestos internos"),
        };

        public static decimal ParseDecimal(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return 0m;
            return decimal.TryParse(valor, NumberStyles.Any, _ar, out var r) ? r : 0m;
        }

        public static string BuildSucursalNumero(string puntoVenta, string numero)
        {
            var pv  = (puntoVenta ?? "0").Trim().PadLeft(4, '0');
            var num = (numero     ?? "0").Trim().PadLeft(8, '0');
            return pv + num;
        }

        public static string FormatearFecha(string valorArca, string formato)
        {
            if (string.IsNullOrWhiteSpace(valorArca)) return string.Empty;
            formato = string.IsNullOrWhiteSpace(formato) ? "yyyyMMdd" : formato;

            if (DateTime.TryParseExact(valorArca, new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" },
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt.ToString(formato);
            if (DateTime.TryParse(valorArca, _ar, DateTimeStyles.None, out var dt2))
                return dt2.ToString(formato);
            return string.Empty;
        }

        /// <summary>
        /// Retorna los dos slots de IVA mas significativos (ordenados por importe de IVA desc).
        /// Si hay mas de dos alicuotas, el excedente se acumula en el slot 2.
        /// </summary>
        public static (decimal r1, decimal n1, decimal i1,
                       decimal r2, decimal n2, decimal i2) ResolverSlotIva(ComprobanteCsv c)
        {
            var slots = new List<(decimal Rate, decimal Neto, decimal Iva)>();

            void Add(decimal rate, string netoStr, string ivaStr)
            {
                var neto = ParseDecimal(netoStr);
                var iva  = ParseDecimal(ivaStr);
                if (neto != 0m || iva != 0m)
                    slots.Add((rate, neto, iva));
            }

            Add(2.5m,  c.ImpNetoGravadoIVA25,  c.Iva25);
            Add(5m,    c.ImpNetoGravadoIVA5,   c.Iva5);
            Add(10.5m, c.ImpNetoGravadoIVA105, c.Iva105);
            Add(21m,   c.ImpNetoGravadoIVA21,  c.Iva21);
            Add(27m,   c.ImpNetoGravadoIVA27,  c.Iva27);

            slots.Sort((a, b) => b.Iva.CompareTo(a.Iva));

            var (r1, n1, i1) = slots.Count > 0 ? slots[0] : (0m, 0m, 0m);

            decimal r2 = 0m, n2 = 0m, i2 = 0m;
            if (slots.Count > 1)
            {
                r2 = slots[1].Rate;
                for (int k = 1; k < slots.Count; k++) { n2 += slots[k].Neto; i2 += slots[k].Iva; }
            }

            return (r1, n1, i1, r2, n2, i2);
        }

        /// <summary>
        /// Detalle de IVA/Exento/No Gravado que ARCA informa por alicuota, para mostrar en la
        /// ventana de revision de PRESEA (comprobacion contra el comprobante fisico). Indica a
        /// que slot del layout PRESEA (Cuenta IVA / Cuenta IVA 2, ver <see cref="ResolverSlotIva"/>)
        /// queda mapeado cada componente, o si el layout PRESEA no tiene campo para el (Exento y
        /// No Gravado no se exportan hoy: son neto sin IVA, no afectan el total del comprobante).
        /// </summary>
        public static List<(string Concepto, decimal Neto, decimal Iva, string Slot)> DetalleIva(ComprobanteCsv c)
        {
            var detalle = new List<(string Concepto, decimal Neto, decimal Iva, string Slot)>();
            if (c == null) return detalle;

            var slots = new List<(decimal Rate, decimal Neto, decimal Iva)>();

            void AddIva(decimal rate, string netoStr, string ivaStr)
            {
                var neto = ParseDecimal(netoStr);
                var iva  = ParseDecimal(ivaStr);
                if (neto != 0m || iva != 0m)
                    slots.Add((rate, neto, iva));
            }

            AddIva(2.5m,  c.ImpNetoGravadoIVA25,  c.Iva25);
            AddIva(5m,    c.ImpNetoGravadoIVA5,   c.Iva5);
            AddIva(10.5m, c.ImpNetoGravadoIVA105, c.Iva105);
            AddIva(21m,   c.ImpNetoGravadoIVA21,  c.Iva21);
            AddIva(27m,   c.ImpNetoGravadoIVA27,  c.Iva27);
            slots.Sort((a, b) => b.Iva.CompareTo(a.Iva));

            for (int k = 0; k < slots.Count; k++)
            {
                string slotTxt = k == 0 ? "Cuenta IVA (slot 1)" : "Cuenta IVA 2 (slot 2, combinado)";
                detalle.Add(($"IVA {slots[k].Rate:0.##}%", slots[k].Neto, slots[k].Iva, slotTxt));
            }

            decimal exento = ParseDecimal(c.ImpOpExentas);
            if (exento != 0m)
                detalle.Add(("Operaciones exentas", exento, 0m, "No exportado (sin campo en PRESEA)"));

            decimal noGravado = ParseDecimal(c.ImpNetoNoGravado);
            if (noGravado != 0m)
                detalle.Add(("Neto no gravado", noGravado, 0m, "No exportado (sin campo en PRESEA)"));

            return detalle;
        }
    }
}
