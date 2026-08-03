using System;
using System.Collections.Generic;
using System.Linq;
using ArcaCliente.Models;
using ClosedXML.Excel;

namespace ArcaCliente.Services
{
    public static class ConciliacionExcelExporter
    {
        // Misma paleta que la grilla (EstadoConciliacionColores), convertida a XLColor.
        private static readonly Dictionary<EstadoConciliacion, XLColor> _colores =
            EstadoConciliacionColores.Rgb.ToDictionary(
                kv => kv.Key,
                kv => XLColor.FromArgb(kv.Value.R, kv.Value.G, kv.Value.B));

        private static readonly (string Header, Func<ItemConciliacion, object> Value)[] _columnas =
        [
            ("Estado",             x => x.EstadoTexto),
            ("Directiva",          x => x.DescripcionDirectiva),
            ("Clave matcheo",      x => x.DetalleMatcheo),
            ("Dif. con sist.",     x => x.FalloDirectivas),
            ("Fecha",              x => x.Fecha),
            ("Tipo",               x => x.TipoComprobante),
            ("Descripción",        x => x.DescripcionTipoComprobante),
            ("Pto. Vta.",          x => x.PuntoVenta),
            ("Número",             x => x.Numero),
            ("CUIT",               x => x.CuitProveedor),
            ("Nombre proveedor",   x => x.NombreProveedor),
            ("Total ARCA",         x => ParseDecimal(x.TotalARCA)),
            ("Total Sist.",        x => ParseDecimal(x.TotalSistema)),
            ("Diferencia",         x => ParseDecimal(x.Diferencia)),
        ];

        public static void Exportar(List<ItemConciliacion> items, string rutaArchivo)
        {
            using var wb = new XLWorkbook();
            var ws        = wb.Worksheets.Add("Conciliación");

            // ?? Título ????????????????????????????????????????????????????????????
            ws.Cell(1, 1).Value                    = $"Conciliación ARCA — {DateTime.Now:dd/MM/yyyy HH:mm}";
            ws.Cell(1, 1).Style.Font.Bold          = true;
            ws.Cell(1, 1).Style.Font.FontSize      = 12;
            ws.Range(1, 1, 1, _columnas.Length).Merge();

            // ?? Encabezado ????????????????????????????????????????????????????????
            const int headerRow = 2;
            for (int c = 0; c < _columnas.Length; c++)
            {
                var cell = ws.Cell(headerRow, c + 1);
                cell.Value = _columnas[c].Header;
                cell.Style.Font.Bold                       = true;
                cell.Style.Font.FontColor                  = XLColor.White;
                cell.Style.Fill.BackgroundColor            = XLColor.FromArgb(50, 50, 50);
                cell.Style.Alignment.Horizontal            = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.BottomBorder             = XLBorderStyleValues.Medium;
                cell.Style.Border.BottomBorderColor        = XLColor.Black;
            }

            // ?? Datos ?????????????????????????????????????????????????????????????
            int row = headerRow + 1;
            foreach (var item in items)
            {
                for (int c = 0; c < _columnas.Length; c++)
                    ws.Cell(row, c + 1).Value = _columnas[c].Value(item)?.ToString() ?? string.Empty;

                // Wrap "Intentos previos" (col 4) para que las líneas sean legibles en Excel
                ws.Cell(row, 4).Style.Alignment.WrapText = true;

                // Alineación de columnas numéricas (Total ARCA, Total Sist., Diferencia)
                for (int c = 12; c <= 14; c++)
                    ws.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                // Color por estado
                if (_colores.TryGetValue(item.Estado, out var color))
                    ws.Range(row, 1, row, _columnas.Length).Style.Fill.BackgroundColor = color;

                row++;
            }

            // ?? Resumen ???????????????????????????????????????????????????????????
            row++;  // fila vacía de separación
            int conciliados = items.Count(x => x.Estado == EstadoConciliacion.Conciliado);
            int diferencias = items.Count(x => x.Estado == EstadoConciliacion.DiferenciaImporte);
            int soloArca    = items.Count(x => x.Estado == EstadoConciliacion.SoloARCA);
            int soloSistema = items.Count(x => x.Estado == EstadoConciliacion.SoloSistema);

            AgregarResumen(ws, row++, "Conciliados",   conciliados, _colores[EstadoConciliacion.Conciliado]);
            AgregarResumen(ws, row++, "Diferencias",   diferencias, _colores[EstadoConciliacion.DiferenciaImporte]);
            AgregarResumen(ws, row++, "Solo ARCA",     soloArca,    _colores[EstadoConciliacion.SoloARCA]);
            AgregarResumen(ws, row++, "Solo sistema",  soloSistema, _colores[EstadoConciliacion.SoloSistema]);
            AgregarResumen(ws, row,   $"Total ({items.Count})", items.Count, XLColor.FromArgb(210, 210, 210));

            // ?? Formato final ?????????????????????????????????????????????????????
            ws.Columns().AdjustToContents(1, row);   // ajustar sin considerar el título
            ws.Column(1).Width  = Math.Max(ws.Column(1).Width,  16);  // "Estado" mínimo
            ws.Column(12).Width = Math.Min(ws.Column(12).Width, 40);  // "Nombre proveedor" máximo

            ws.SheetView.Freeze(headerRow, 0);  // fijar encabezado

            // Bordes del rango de datos
            var dataRange = ws.Range(headerRow, 1, row, _columnas.Length);
            dataRange.Style.Border.OutsideBorder      = XLBorderStyleValues.Thin;
            dataRange.Style.Border.OutsideBorderColor = XLColor.Gray;

            wb.SaveAs(rutaArchivo);
        }

        private static void AgregarResumen(IXLWorksheet ws, int row, string etiqueta, int count, XLColor color)
        {
            ws.Cell(row, 1).Value = etiqueta;
            ws.Cell(row, 2).Value = count;
            ws.Cell(row, 1).Style.Font.Bold           = true;
            ws.Range(row, 1, row, 2).Style.Fill.BackgroundColor = color;
        }

        /// <summary>Intenta devolver el valor como decimal para que Excel lo trate como número.</summary>
        private static object ParseDecimal(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            return decimal.TryParse(s.Replace(",", "."), System.Globalization.NumberStyles.Any,
                       System.Globalization.CultureInfo.InvariantCulture, out var d)
                   ? (object)d : s;
        }
    }
}
