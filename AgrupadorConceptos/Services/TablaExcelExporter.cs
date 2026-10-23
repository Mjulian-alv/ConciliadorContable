using System;
// Fecha: 14/10/2026 - TAREA: 00097 - Linea: 3 - La web descarga el libro como bytes: el armado se separa de dónde se guarda, sin cambiar lo que se produce.
using System.Collections.Generic;
using System.IO;
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
            using var wb = Construir(encabezados, filas, titulo, nombreHoja);
            wb.SaveAs(rutaDestino);
        }

        /// <summary>El mismo libro que Exportar, como bytes (descarga desde la web).</summary>
        public static byte[] ExportarBytes(
            List<string> encabezados, List<object[]> filas, string titulo, string nombreHoja = "Datos")
        {
            using var wb = Construir(encabezados, filas, titulo, nombreHoja);
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        private static XLWorkbook Construir(
            List<string> encabezados, List<object[]> filas, string titulo, string nombreHoja)
        {
            var wb = new XLWorkbook();
            AgregarHoja(wb, encabezados, filas, titulo, nombreHoja);
            return wb;
        }

        // Fecha: 23/10/2026 - TAREA: 00126 - Linea: 1 - Un libro con varias hojas (el consolidado suma el asiento): la hoja se arma igual que siempre, sólo que sobre un libro ajeno.
        /// <summary>Una hoja más del libro, con el mismo formato que Exportar. Las últimas <paramref name="filasEnNegrita"/> filas (p. ej. totales) salen en negrita.</summary>
        public static void AgregarHoja(
            XLWorkbook wb, List<string> encabezados, List<object[]> filas, string titulo, string nombreHoja, int filasEnNegrita = 0)
        {
            if (encabezados == null || encabezados.Count == 0)
                throw new ArgumentException("No hay columnas para exportar.", nameof(encabezados));

            filas ??= new List<object[]>();
            int columnas = encabezados.Count;

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
                    // Fecha: 07/10/2026 - TAREA: 00073 - Linea: 3 - Una fecha debe poder ordenarse y filtrarse en Excel sin depender de la cultura del equipo.
                    else if (valor is DateTime fecha)
                    {
                        celda.Value = fecha.Date;
                        celda.Style.DateFormat.Format = "dd/mm/yyyy";
                    }
                    else
                    {
                        celda.Value = valor?.ToString() ?? string.Empty;
                    }
                }
                row++;
            }

            if (filasEnNegrita > 0 && filas.Count > 0)
                ws.Range(row - Math.Min(filasEnNegrita, filas.Count), 1, row - 1, columnas).Style.Font.Bold = true;

            ws.Columns().AdjustToContents(1, row);
            ws.SheetView.Freeze(headerRow, 0);

            var dataRange = ws.Range(1, 1, row - 1, columnas);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.OutsideBorderColor = XLColor.Gray;
        }
    }
}
