using System;
using System.Collections.Generic;
using ArcaCliente.Models;
using ClosedXML.Excel;

namespace ArcaCliente.Services
{
    /// <summary>
    // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 3 - Se restauran los acentos que el archivo había perdido por un guardado con codificación equivocada (se veían como signo de reemplazo).
    /// Genera el Excel de importación de compras para el sistema PRESEA.
    /// Las columnas siguen exactamente la especificación oficial de PRESEA:
    /// tipos I (entero), B (decimal), C (carácter), D (fecha yyyyMMdd), N (numérico), M (memo).
    /// Los campos que no se pueden derivar del CSV de ARCA se toman de <see cref="ConfigPresea"/>.
    /// </summary>
    public class PreseaExportador : IExportadorSistema
    {
        private readonly ConfigPresea _config;

        public string NombreSistema => "PRESEA";

        public PreseaExportador(ConfigPresea config) =>
            _config = config ?? new ConfigPresea();

        // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 3 - Se restauran los acentos que el archivo había perdido por un guardado con codificación equivocada (se veían como signo de reemplazo).
        // ?? Exportación principal ?????????????????????????????????????????????

        // Los cálculos de moneda/importe/sucursal/fecha/IVA viven en PreseaCalculos,
        // compartida con PreseaTxtExportador (antes había una copia privada acá que
        // ya había divergido en FormatearFecha y BuildSucursalNumero).

        public void Exportar(List<ComprobanteCsv> comprobantes, string rutaArchivo)
        {
            using var wb = new XLWorkbook();
            var ws       = wb.Worksheets.Add("PRESEA");

            EscribirEncabezado(ws);

            int row = 2;
            foreach (var comp in comprobantes)
                EscribirFila(ws, row++, comp);

            ws.Columns().AdjustToContents();
            // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 3 - Se restauran los acentos que el archivo había perdido por un guardado con codificación equivocada (se veían como signo de reemplazo).
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
                // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 3 - Se restauran los acentos que el archivo había perdido por un guardado con codificación equivocada (se veían como signo de reemplazo).
                "Cotización",                        // 5  B/-14,8
                "Provincia",                         // 6  I/3
                // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 3 - Se restauran los acentos que el archivo había perdido por un guardado con codificación equivocada (se veían como signo de reemplazo).
                "Condición",                         // 7  I/5
                "Descuento",                         // 8  B/-12,2
                // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 3 - Se restauran los acentos que el archivo había perdido por un guardado con codificación equivocada (se veían como signo de reemplazo).
                "Sucursal y número",                 // 9  N/14
                "Fiscal",                            // 10 C/1
                "Fecha",                             // 11 D/8
                "Fecha contable",                    // 12 D/8
                "CAI",                               // 13 B/14
                "Vencimiento CAI",                   // 14 D/8
                // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 3 - Se restauran los acentos que el archivo había perdido por un guardado con codificación equivocada (se veían como signo de reemplazo).
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
                // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 3 - Se restauran los acentos que el archivo había perdido por un guardado con codificación equivocada (se veían como signo de reemplazo).
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
                // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 3 - Se restauran los acentos que el archivo había perdido por un guardado con codificación equivocada (se veían como signo de reemplazo).
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
                 iva2Rate, iva2Neto, iva2Iva) = PreseaCalculos.ResolverSlotIva(comp);

            var fechaStr = PreseaCalculos.FormatearFecha(comp.FechaEmision, _config.FormatoFecha);
            var sucNum   = PreseaCalculos.BuildSucursalNumero(comp.PuntoVenta, comp.NumeroDesde);

            Set(ws, row, 1,  ParseInt(_config.CodigoProveedor));           // Proveedor         I/8
            Set(ws, row, 2,  _config.CuentaContableProveedor);             // Cta. ctble. prov. B/16
            Set(ws, row, 3,  Truncate(ObtenerDescripcionComprobante(comp), 24)); // Comprobante  C/24
            Set(ws, row, 4,  PreseaCalculos.MapMoneda(comp.Moneda));       // Moneda            I/3
            // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 3 - Se restauran los acentos que el archivo había perdido por un guardado con codificación equivocada (se veían como signo de reemplazo).
            Set(ws, row, 5,  PreseaCalculos.ParseDecimal(comp.TipoCambio)); // Cotización       B/-14,8
            Set(ws, row, 6,  ParseInt(_config.Provincia));                 // Provincia         I/3
            // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 3 - Se restauran los acentos que el archivo había perdido por un guardado con codificación equivocada (se veían como signo de reemplazo).
            Set(ws, row, 7,  ParseInt(_config.Condicion));                 // Condición         I/5
            var provDescuento = AppServices.GetPreseaProveedor(TextParsingUtils.SoloDigitos(comp.NroDocEmisor));
            Set(ws, row, 8,  provDescuento?.Descuento ?? 0m);              // Descuento         B/-12,2
            // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 3 - Se restauran los acentos que el archivo había perdido por un guardado con codificación equivocada (se veían como signo de reemplazo).
            Set(ws, row, 9,  sucNum);                                      // Sucursal y núm.   N/14
            Set(ws, row, 10, _config.Fiscal);                              // Fiscal            C/1
            Set(ws, row, 11, fechaStr);                                    // Fecha             D/8
            Set(ws, row, 12, fechaStr);                                    // Fecha contable    D/8
            Set(ws, row, 13, PreseaCalculos.ParseDecimal(comp.CodAutorizacion));          // CAI               B/14
            Set(ws, row, 14, string.Empty);                                // Vencimiento CAI   D/8
            // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 3 - Se restauran los acentos que el archivo había perdido por un guardado con codificación equivocada (se veían como signo de reemplazo).
            Set(ws, row, 15, string.Empty);                                // Observación       M/4
            Set(ws, row, 16, _config.CuentaIVA);                          // Cuenta IVA        B/16
            Set(ws, row, 17, iva1Rate);                                    // Pct. IVA          N/-6,2
            Set(ws, row, 18, iva1Neto);                                    // Neto              B/-12,2
            Set(ws, row, 19, iva1Iva);                                     // IVA               B/-12,2
            Set(ws, row, 20, _config.CuentaIVA2);                         // Cuenta IVA 2      B/16
            Set(ws, row, 21, iva2Rate);                                    // Pct. IVA 2        N/-6,2
            Set(ws, row, 22, iva2Neto);                                    // Neto 2            B/-12,2
            Set(ws, row, 23, iva2Iva);                                     // IVA 2             B/-12,2
            Set(ws, row, 24, PreseaCalculos.ParseDecimal(comp.ImpTotal));                 // Importe           B/-12,2
            // Sin ventana de revision en este camino (exportacion masiva): el 100% de
            // "Otros Tributos" (ARCA) se vuelca por defecto a Percepcion IB (IIBB), igual
            // que el default del camino TXT (ver PreseaExportResolver.Resolver).
            Set(ws, row, 25, 0m);                                          // Sobretasa         B/-12,2
            // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 3 - Se restauran los acentos que el archivo había perdido por un guardado con codificación equivocada (se veían como signo de reemplazo).
            Set(ws, row, 26, PreseaCalculos.ParseDecimal(comp.OtrosTributos));            // Percepción IB     B/-12,2
            Set(ws, row, 27, 0m);                                          // Percepción IM     B/-12,2
            Set(ws, row, 28, 0m);                                          // Percepción IV     B/-12,2
            Set(ws, row, 29, 0m);                                          // Percepción IN     B/-12,2
            Set(ws, row, 30, 0);                                           // Cód. Percepc. 1   I/3
            Set(ws, row, 31, 0m);                                          // Percepc. 1 imp.   B/-12,2
            // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 3 - Se restauran los acentos que el archivo había perdido por un guardado con codificación equivocada (se veían como signo de reemplazo).
            Set(ws, row, 32, 0);                                           // Cód. Percepc. 2   I/3
            Set(ws, row, 33, 0m);                                          // Percepc. 2 imp.   B/-12,2
            Set(ws, row, 34, 0m);                                          // Imp. internos     B/-12,2
            Set(ws, row, 35, _config.CuentaDebe);                         // Cta. ctble. debe  B/16
            Set(ws, row, 36, Truncate(_config.Centro, 10));               // Centro            C/10
            Set(ws, row, 37, "BASE");                                // Lista de precios  C/10
            // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 3 - Se restauran los acentos que el archivo había perdido por un guardado con codificación equivocada (se veían como signo de reemplazo).
            Set(ws, row, 38, 1);                                           // Versión           I/4
            Set(ws, row, 39, _config.Imputa);                             // Imputa            C/1
        }

        // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 3 - Se restauran los acentos que el archivo había perdido por un guardado con codificación equivocada (se veían como signo de reemplazo).
        // ?? Helpers de formateo y conversión ??????????????????????????????????

        private static string ObtenerDescripcionComprobante(ComprobanteCsv comp) =>
            TablaComprobanteDescripcionMapper.ObtenerDescripcion(comp.TipoComprobante);

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
