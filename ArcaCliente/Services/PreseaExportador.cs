using System;
using System.Collections.Generic;
using System.Globalization;
using ArcaCliente.Models;
using ClosedXML.Excel;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Genera el Excel de importación de compras para el sistema PRESEA.
    /// Las columnas siguen exactamente la especificación oficial de PRESEA:
    /// tipos I (entero), B (decimal), C (carácter), D (fecha yyyyMMdd), N (numérico), M (memo).
    /// Los campos que no se pueden derivar del CSV de ARCA se toman de <see cref="ConfigPresea"/>.
    /// </summary>
    public class PreseaExportador : IExportadorSistema
    {
        private readonly ConfigPresea _config;

        private static readonly CultureInfo _culturaArca = CultureInfo.GetCultureInfo("es-AR");

        // Mapa ARCA moneda ? código PRESEA (1 = peso argentino, 2 = dólar, etc.)
        private static readonly Dictionary<string, int> _mapeoMonedas =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["PES"] = 1, ["ARS"] = 1, ["$"]   = 1,
                ["DOL"] = 2, ["USD"] = 2,
                ["EUR"] = 3,
                ["BRL"] = 4,
                ["UYU"] = 5,
            };

        public string NombreSistema => "PRESEA";

        public PreseaExportador(ConfigPresea config) =>
            _config = config ?? new ConfigPresea();

        // ?? Exportación principal ?????????????????????????????????????????????

        public void Exportar(List<ComprobanteCsv> comprobantes, string rutaArchivo)
        {
            using var wb = new XLWorkbook();
            var ws       = wb.Worksheets.Add("PRESEA");

            EscribirEncabezado(ws);

            int row = 2;
            foreach (var comp in comprobantes)
                EscribirFila(ws, row++, comp);

            ws.Columns().AdjustToContents();
            // Limitar ancho de columnas de descripción/cuenta para no generar archivos enormes
            ws.Column(3).Width  = Math.Min(ws.Column(3).Width, 30);   // Comprobante
            ws.Column(35).Width = Math.Min(ws.Column(35).Width, 20);  // Cuenta debe
            ws.SheetView.Freeze(1, 0);

            wb.SaveAs(rutaArchivo);
        }

        // ?? Encabezado ????????????????????????????????????????????????????????

        private static void EscribirEncabezado(IXLWorksheet ws)
        {
            string[] headers =
            [
                "Proveedor",                        // 1  I/8
                "Cuenta contable proveedor",         // 2  B/16
                "Comprobante",                       // 3  C/24
                "Moneda",                            // 4  I/3
                "Cotización",                        // 5  B/-14,8
                "Provincia",                         // 6  I/3
                "Condición",                         // 7  I/5
                "Descuento",                         // 8  B/-12,2
                "Sucursal y número",                 // 9  N/14
                "Fiscal",                            // 10 C/1
                "Fecha",                             // 11 D/8
                "Fecha contable",                    // 12 D/8
                "CAI",                               // 13 B/14
                "Vencimiento CAI",                   // 14 D/8
                "Observación",                       // 15 M/4
                "Cuenta IVA",                        // 16 B/16
                "Porcentaje de IVA",                 // 17 N/-6,2
                "Neto",                              // 18 B/-12,2
                "IVA",                               // 19 B/-12,2
                "Cuenta IVA 2",                      // 20 B/16
                "Porcentaje de IVA 2",               // 21 N/-6,2
                "Neto 2",                            // 22 B/-12,2
                "IVA 2",                             // 23 B/-12,2
                "Importe",                           // 24 B/-12,2
                "Sobretasa",                         // 25 B/-12,2
                "Percepción IB",                     // 26 B/-12,2
                "Percepción IM",                     // 27 B/-12,2
                "Percepción IV",                     // 28 B/-12,2
                "Percepción IN",                     // 29 B/-12,2
                "Código Percepción configurable 1",  // 30 I/3
                "Percepción Configurable 1 importe", // 31 B/-12,2
                "Código Percepción configurable 2",  // 32 I/3
                "Percepción Configurable 2 importe", // 33 B/-12,2
                "Impuestos internos",                // 34 B/-12,2
                "Cuenta contable del debe",          // 35 B/16
                "Centro",                            // 36 C/10
                "Lista de precios",                  // 37 C/10
                "Versión",                           // 38 I/4
                "Imputa",                            // 39 C/1
            ];

            for (int c = 0; c < headers.Length; c++)
            {
                var cell = ws.Cell(1, c + 1);
                cell.Value                         = headers[c];
                cell.Style.Font.Bold               = true;
                cell.Style.Font.FontColor          = XLColor.White;
                cell.Style.Fill.BackgroundColor    = XLColor.FromArgb(50, 50, 50);
                cell.Style.Alignment.Horizontal    = XLAlignmentHorizontalValues.Center;
            }
        }

        // ?? Fila de datos ?????????????????????????????????????????????????????

        private void EscribirFila(IXLWorksheet ws, int row, ComprobanteCsv comp)
        {
            var (iva1Rate, iva1Neto, iva1Iva,
                 iva2Rate, iva2Neto, iva2Iva) = ResolverSlotIva(comp);

            var fechaStr = FormatearFecha(comp.FechaEmision);
            var sucNum   = BuildSucursalNumero(comp.PuntoVenta, comp.NumeroDesde);

            Set(ws, row, 1,  ParseInt(_config.CodigoProveedor));           // Proveedor         I/8
            Set(ws, row, 2,  _config.CuentaContableProveedor);             // Cta. ctble. prov. B/16
            Set(ws, row, 3,  Truncate(ObtenerDescripcionComprobante(comp), 24)); // Comprobante  C/24
            Set(ws, row, 4,  MapMoneda(comp.Moneda));                      // Moneda            I/3
            Set(ws, row, 5,  ParseDecimal(comp.TipoCambio));               // Cotización        B/-14,8
            Set(ws, row, 6,  ParseInt(_config.Provincia));                 // Provincia         I/3
            Set(ws, row, 7,  ParseInt(_config.Condicion));                 // Condición         I/5
            Set(ws, row, 8,  0m);                                          // Descuento         B/-12,2
            Set(ws, row, 9,  sucNum);                                      // Sucursal y núm.   N/14
            Set(ws, row, 10, _config.Fiscal);                              // Fiscal            C/1
            Set(ws, row, 11, fechaStr);                                    // Fecha             D/8
            Set(ws, row, 12, fechaStr);                                    // Fecha contable    D/8
            Set(ws, row, 13, ParseDecimal(comp.CodAutorizacion));          // CAI               B/14
            Set(ws, row, 14, string.Empty);                                // Vencimiento CAI   D/8
            Set(ws, row, 15, string.Empty);                                // Observación       M/4
            Set(ws, row, 16, _config.CuentaIVA);                          // Cuenta IVA        B/16
            Set(ws, row, 17, iva1Rate);                                    // Pct. IVA          N/-6,2
            Set(ws, row, 18, iva1Neto);                                    // Neto              B/-12,2
            Set(ws, row, 19, iva1Iva);                                     // IVA               B/-12,2
            Set(ws, row, 20, _config.CuentaIVA2);                         // Cuenta IVA 2      B/16
            Set(ws, row, 21, iva2Rate);                                    // Pct. IVA 2        N/-6,2
            Set(ws, row, 22, iva2Neto);                                    // Neto 2            B/-12,2
            Set(ws, row, 23, iva2Iva);                                     // IVA 2             B/-12,2
            Set(ws, row, 24, ParseDecimal(comp.ImpTotal));                 // Importe           B/-12,2
            Set(ws, row, 25, 0m);                                          // Sobretasa         B/-12,2
            Set(ws, row, 26, 0m);                                          // Percepción IB     B/-12,2
            Set(ws, row, 27, 0m);                                          // Percepción IM     B/-12,2
            Set(ws, row, 28, 0m);                                          // Percepción IV     B/-12,2
            Set(ws, row, 29, 0m);                                          // Percepción IN     B/-12,2
            Set(ws, row, 30, 0);                                           // Cód. Percepc. 1   I/3
            Set(ws, row, 31, 0m);                                          // Percepc. 1 imp.   B/-12,2
            Set(ws, row, 32, 0);                                           // Cód. Percepc. 2   I/3
            Set(ws, row, 33, 0m);                                          // Percepc. 2 imp.   B/-12,2
            Set(ws, row, 34, ParseDecimal(comp.OtrosTributos));            // Imp. internos     B/-12,2
            Set(ws, row, 35, _config.CuentaDebe);                         // Cta. ctble. debe  B/16
            Set(ws, row, 36, Truncate(_config.Centro, 10));               // Centro            C/10
            Set(ws, row, 37, string.Empty);                                // Lista de precios  C/10
            Set(ws, row, 38, 0);                                           // Versión           I/4
            Set(ws, row, 39, _config.Imputa);                             // Imputa            C/1
        }

        // ?? IVA: selección de los dos slots más significativos ????????????????

        /// <summary>
        /// Recorre todas las tasas disponibles en el CSV y retorna los dos slots de IVA
        /// ordenados de mayor a menor importe de IVA.
        /// Si hay más de dos tasas con IVA, los excedentes se acumulan en el slot 2.
        /// </summary>
        private static (decimal r1, decimal n1, decimal i1,
                        decimal r2, decimal n2, decimal i2)
            ResolverSlotIva(ComprobanteCsv c)
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

            // Ordenar por importe de IVA desc (la tasa más importante va al slot 1)
            slots.Sort((a, b) => b.Iva.CompareTo(a.Iva));

            var (r1, n1, i1) = slots.Count > 0 ? slots[0] : (0m, 0m, 0m);

            // Si hay más de dos tasas, acumular el excedente en el slot 2
            decimal r2 = 0m, n2 = 0m, i2 = 0m;
            if (slots.Count > 1)
            {
                r2 = slots[1].Rate;
                for (int k = 1; k < slots.Count; k++) { n2 += slots[k].Neto; i2 += slots[k].Iva; }
            }

            return (r1, n1, i1, r2, n2, i2);
        }

        // ?? Helpers de formateo y conversión ??????????????????????????????????

        private string FormatearFecha(string valorArca)
        {
            if (string.IsNullOrWhiteSpace(valorArca)) return string.Empty;
            if (DateTime.TryParseExact(valorArca, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt.ToString(_config.FormatoFecha);
            if (DateTime.TryParse(valorArca, _culturaArca, DateTimeStyles.None, out var dt2))
                return dt2.ToString(_config.FormatoFecha);
            return string.Empty;
        }

        private static string BuildSucursalNumero(string puntoVenta, string numero)
        {
            var pv  = (puntoVenta ?? "0").TrimStart().PadLeft(4, '0');
            var num = (numero     ?? "0").TrimStart().PadLeft(8, '0');
            return pv + num;   // 12 caracteres máximo ? campo N/14 en PRESEA
        }

        private static string ObtenerDescripcionComprobante(ComprobanteCsv comp) =>
            TablaComprobanteDescripcionMapper.ObtenerDescripcion(comp.TipoComprobante);

        private int MapMoneda(string codigoArca)
        {
            if (string.IsNullOrWhiteSpace(codigoArca)) return 1;
            return _mapeoMonedas.TryGetValue(codigoArca.Trim(), out var code) ? code : 1;
        }

        private static decimal ParseDecimal(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return 0m;
            return decimal.TryParse(valor, NumberStyles.Any, _culturaArca, out var r) ? r : 0m;
        }

        private static int ParseInt(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return 0;
            return int.TryParse(valor, out var r) ? r : 0;
        }

        private static string Truncate(string s, int maxLen) =>
            s is null ? string.Empty
                      : s.Length > maxLen ? s[..maxLen] : s;

        // ?? Escritura de celda con tipo ???????????????????????????????????????

        private static void Set(IXLWorksheet ws, int row, int col, object value)
        {
            var cell = ws.Cell(row, col);
            switch (value)
            {
                case decimal d: cell.Value = d; break;
                case int    i: cell.Value = i; break;
                case string s: cell.Value = s; break;
                default:       cell.Value = value?.ToString() ?? string.Empty; break;
            }
        }
    }
}
