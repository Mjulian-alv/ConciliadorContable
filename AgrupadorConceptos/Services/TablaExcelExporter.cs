using System;
using System.Collections.Generic;
using ClosedXML.Excel;

namespace AgrupadorConceptos.Services
{
    /// <summary>
    /// Vuelca una tabla cualquiera a un .xlsx con el formato del módulo: título arriba,
    /// encabezado oscuro fijo y bordes. No sabe nada de conceptos ni de movimientos, y
    /// tampoco de la grilla: recibe encabezados y filas ya resueltos por el llamador.
    /// </summary>
    public static class TablaExcelExporter
    {
        private const string FormatoImporte = "#,##0.00";

        /// <param name="filas">
        /// Cada fila tiene que tener tantos valores como encabezados. Los valores decimales
        /// salen como número con formato de importe; el resto, tal como los da ToString.
        /// </param>
        public static void Exportar(
            List<string> encabezados, List<object[]> filas, string titulo, string rutaDestino,
            string nombreHoja = "Datos")
        {
            if (encabezados == null || encabezados.Count == 0)
                throw new ArgumentException("No hay columnas para exportar.", nameof(encabezados));

            filas ??= new List<object[]>();
            int columnas = encabezados.Count;

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(nombreHoja);

            ws.Cell(1, 1).Value = titulo;
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 12;
            ws.Range(1, 1, 1, columnas).Merge();

            const int headerRow = 2;
            for (int c = 0; c < columnas; c++)
                ws.Cell(headerRow, c + 1).Value = encabezados[c];

            var hr = ws.Range(headerRow, 1, headerRow, columnas);
            hr.Style.Font.Bold = true;
            hr.Style.Font.FontColor = XLColor.White;
            hr.Style.Fill.BackgroundColor = XLColor.FromArgb(50, 50, 50);
            hr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            hr.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
            hr.Style.Border.BottomBorderColor = XLColor.Black;

            int row = headerRow + 1;
            foreach (var fila in filas)
            {
                for (int c = 0; c < columnas; c++)
                {
                    var celda = ws.Cell(row, c + 1);
                    object valor = c < fila.Length ? fila[c] : null;

                    if (valor is decimal importe)
                    {
                        celda.Value = importe;
                        celda.Style.NumberFormat.Format = FormatoImporte;
                    }
                    else
                    {
                        celda.Value = valor?.ToString() ?? string.Empty;
                    }
                }
                row++;
            }

            ws.Columns().AdjustToContents(1, row);
            ws.SheetView.Freeze(headerRow, 0);

            var dataRange = ws.Range(1, 1, row - 1, columnas);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.OutsideBorderColor = XLColor.Gray;

            wb.SaveAs(rutaDestino);
        }
    }
}
