using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ArcaCliente.Models;
using ClosedXML.Excel;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Importa comprobantes locales desde un archivo Excel (.xlsx).
    /// La primera fila debe ser el encabezado. Las columnas requeridas son:
    /// Fecha, PuntoVenta (o "Punto de Venta"), Numero (o "Número"), Total (o "Importe").
    /// </summary>
    public static class ExcelImporter
    {
        // Mapa: encabezado normalizado ? campo de ComprobanteLocal
        private static readonly Dictionary<string, string> _headerMap =
            new(StringComparer.OrdinalIgnoreCase)
        {
            ["fecha"]            = "Fecha",
            ["fecha emision"]    = "Fecha",
            ["fecha de emision"] = "Fecha",
            ["punto de venta"]   = "PuntoVenta",
            ["pto. vta."]        = "PuntoVenta",
            ["pto vta"]          = "PuntoVenta",
            ["ptoventa"]         = "PuntoVenta",
            ["puntoventa"]       = "PuntoVenta",
            ["sucursal"]         = "PuntoVenta",
            ["numero"]           = "Numero",
            ["nro"]              = "Numero",
            ["nro."]             = "Numero",
            ["nro comprobante"]  = "Numero",
            ["total"]            = "Total",
            ["importe"]          = "Total",
            ["importe total"]    = "Total",
            ["imp total"]        = "Total",
        };

        private static readonly CultureInfo _culturaAr =
            CultureInfo.GetCultureInfo("es-AR");

        /// <summary>
        /// Lee el archivo Excel y retorna los comprobantes locales.
        /// </summary>
        /// <param name="rutaArchivo">Ruta completa al archivo .xlsx.</param>
        /// <param name="nombreHoja">
        /// Nombre de la hoja a leer. Si es null se usa la primera hoja.
        /// </param>
        public static List<ComprobanteLocal> ImportarDesdeExcel(
            string rutaArchivo,
            string nombreHoja = null)
        {
            using var wb = new XLWorkbook(rutaArchivo);

            var hoja = string.IsNullOrWhiteSpace(nombreHoja)
                ? wb.Worksheets.First()
                : wb.Worksheets.Worksheet(nombreHoja);

            var resultado = new List<ComprobanteLocal>();
            var rango = hoja.RangeUsed();
            if (rango == null) return resultado;

            var filas = rango.RowsUsed().ToList();
            if (filas.Count < 2) return resultado;

            // Mapear encabezados ? índice de columna (1-based relativo al rango)
            var headerRow = filas[0];
            var colMap = new Dictionary<int, string>();
            int firstCol = rango.FirstColumn().ColumnNumber();

            for (int c = 0; c < headerRow.CellCount(); c++)
            {
                var cell = headerRow.Cell(c + 1);
                var header = NormalizarEncabezado(cell.GetString());
                if (_headerMap.TryGetValue(header, out var campo))
                    colMap[c + 1] = campo;
            }

            for (int r = 1; r < filas.Count; r++)
            {
                var fila = filas[r];
                var local = new ComprobanteLocal();

                foreach (var (col, campo) in colMap)
                {
                    var cell = fila.Cell(col);
                    switch (campo)
                    {
                        case "Fecha":
                            if (cell.TryGetValue(out DateTime dt))
                                local.Fecha = dt;
                            else if (DateTime.TryParse(cell.GetString(), _culturaAr,
                                         DateTimeStyles.None, out DateTime dt2))
                                local.Fecha = dt2;
                            break;

                        case "PuntoVenta":
                            local.PuntoVenta = cell.GetString().Trim();
                            break;

                        case "Numero":
                            local.Numero = cell.GetString().Trim();
                            break;

                        case "Total":
                            if (cell.TryGetValue(out decimal dec))
                                local.Total = dec;
                            else if (decimal.TryParse(cell.GetString(),
                                         NumberStyles.Any, _culturaAr, out decimal d2))
                                local.Total = d2;
                            break;
                    }
                }

                if (local.Fecha != default)
                    resultado.Add(local);
            }

            return resultado;
        }

        private static string NormalizarEncabezado(string header)
        {
            if (string.IsNullOrEmpty(header)) return string.Empty;

            header = header.Trim().ToLowerInvariant();
            var normalized = header.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);
            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
