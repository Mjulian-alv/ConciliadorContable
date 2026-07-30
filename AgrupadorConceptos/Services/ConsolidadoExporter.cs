using System;
using System.Collections.Generic;
using System.Linq;
using AgrupadorConceptos.Models;
using ClosedXML.Excel;

namespace AgrupadorConceptos.Services
{
    /// <summary>Una fila del consolidado: totales agrupados por concepto final.</summary>
    public class LineaConsolidado
    {
        public string Concepto { get; set; }
        public decimal Debitos { get; set; }
        public decimal Creditos { get; set; }
        public decimal Saldo { get; set; }
    }

    /// <summary>
    /// Consolidado bancario: agrupa los movimientos homologados por concepto final
    /// y lo vuelca a Excel.
    /// </summary>
    public static class ConsolidadoExporter
    {
        /// <summary>
        /// Agrupa por ConceptoFinal, ignorando los movimientos sin homologar.
        /// Los débitos se informan en valor absoluto; el saldo es créditos menos débitos.
        /// </summary>
        public static List<LineaConsolidado> Calcular(IEnumerable<MovimientoProcesado> movimientos)
        {
            return movimientos
                .Where(m => !ConceptosBancarios.EstaPendiente(m.ConceptoFinal))
                .GroupBy(m => m.ConceptoFinal)
                .Select(g => new LineaConsolidado
                {
                    Concepto = g.Key,
                    Debitos  = Math.Abs(Math.Round(g.Sum(x => x.Debitos), 2)),
                    Creditos = Math.Round(g.Sum(x => x.Creditos), 2),
                    Saldo    = Math.Round(g.Sum(x => x.Creditos) - Math.Abs(g.Sum(x => x.Debitos)), 2)
                })
                .OrderBy(x => x.Concepto)
                .ToList();
        }

        /// <summary>Cuántos movimientos quedarían afuera del consolidado por estar sin homologar.</summary>
        public static int ContarPendientes(IEnumerable<MovimientoProcesado> movimientos) =>
            movimientos.Count(m => ConceptosBancarios.EstaPendiente(m.ConceptoFinal));

        public static void ExportarAExcel(List<LineaConsolidado> lineas, string titulo, string rutaDestino)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Consolidado");

            ws.Cell(1, 1).Value = titulo;
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 12;
            ws.Range(1, 1, 1, 4).Merge();

            const int headerRow = 2;
            ws.Cell(headerRow, 1).Value = "Concepto Final";
            ws.Cell(headerRow, 2).Value = "Débitos";
            ws.Cell(headerRow, 3).Value = "Créditos";
            ws.Cell(headerRow, 4).Value = "Saldo";

            var hr = ws.Range(headerRow, 1, headerRow, 4);
            hr.Style.Font.Bold = true;
            hr.Style.Font.FontColor = XLColor.White;
            hr.Style.Fill.BackgroundColor = XLColor.FromArgb(50, 50, 50);
            hr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            hr.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
            hr.Style.Border.BottomBorderColor = XLColor.Black;

            int row = headerRow + 1;
            foreach (var item in lineas)
            {
                ws.Cell(row, 1).Value = item.Concepto;
                ws.Cell(row, 2).Value = item.Debitos;
                ws.Cell(row, 3).Value = item.Creditos;
                ws.Cell(row, 4).Value = item.Saldo;

                ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";

                row++;
            }

            ws.Columns().AdjustToContents(1, row);
            ws.SheetView.Freeze(headerRow, 0);

            var dataRange = ws.Range(1, 1, row - 1, 4);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.OutsideBorderColor = XLColor.Gray;

            wb.SaveAs(rutaDestino);
        }
    }
}
