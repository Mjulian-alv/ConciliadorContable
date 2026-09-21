// Fecha: 21/09/2026 - TAREA: 00041 - Linea: 7 - Exporta la conciliación PyR a Excel: hoja Detalle y hoja Resumen por cuenta
// Mismo estilo que ConciliacionExcelExporter de Offline (encabezado oscuro, colores por estado).
using System;
using System.Linq;
using ArcaCliente.Models;
using ClosedXML.Excel;

namespace ArcaCliente.Services
{
    public static class ConciliacionPyRExcelExporter
    {
        private static readonly (string Header, Func<ItemConciliacionPyR, object> Value, bool Importe)[] Detalle =
        {
            ("Estado",         x => x.EstadoTexto,          false),
            ("Cuenta",         x => x.Cuenta?.ToString(),   false),
            ("Directiva",      x => x.DescripcionDirectiva, false),
            ("Fecha ARCA",     x => x.FechaArca,            false),
            ("CUIT",           x => x.Cuit,                 false),
            ("Denominación",   x => x.Denominacion,         false),
            ("Tipo",           x => x.Tipo,                 false),
            ("Número ARCA",    x => x.NumeroArca,           false),
            ("Importe ARCA",   x => x.ImporteArca,          true),
            ("Fecha PRESEA",   x => x.FechaPresea,          false),
            ("Asiento",        x => x.Asiento,              false),
            ("Número PRESEA",  x => x.NumeroPresea,         false),
            ("Proveedor",      x => x.Proveedor,            false),
            ("Importe PRESEA", x => x.ImportePresea,        true),
            ("Diferencia",     x => x.Diferencia,           true),
        };

        public static void Exportar(ResultadoConciliacionPyR resultado, string nombrePerfil, string rutaArchivo)
        {
            using var wb = new XLWorkbook();

            // ── Detalle ──────────────────────────────────────────────────────────
            var ws = wb.Worksheets.Add("Detalle");
            Titulo(ws, $"Conciliación percepciones y retenciones — {nombrePerfil} — {DateTime.Now:dd/MM/yyyy HH:mm}", Detalle.Length);
            Encabezado(ws, 2, Detalle.Select(c => c.Header).ToArray());

            int fila = 3;
            foreach (var item in resultado.Items.OrderBy(i => i.Cuenta?.Codigo).ThenBy(i => i.Estado))
            {
                for (int c = 0; c < Detalle.Length; c++)
                {
                    var cell = ws.Cell(fila, c + 1);
                    switch (Detalle[c].Value(item))
                    {
                        case null:       break;
                        case DateTime d: cell.Value = d; cell.Style.DateFormat.Format = "dd/MM/yyyy"; break;
                        case decimal m:  cell.Value = m; cell.Style.NumberFormat.Format = "#,##0.00"; break;
                        case object o:   cell.Value = o.ToString(); break;
                    }
                }
                var (r, g, b) = EstadoConciliacionPyRColores.Rgb(item.Estado);
                ws.Range(fila, 1, fila, Detalle.Length).Style.Fill.BackgroundColor = XLColor.FromArgb(r, g, b);
                fila++;
            }
            ws.SheetView.FreezeRows(2);
            ws.Range(2, 1, Math.Max(2, fila - 1), Detalle.Length).SetAutoFilter();
            ws.Columns().AdjustToContents(2, Math.Min(fila, 500));

            // ── Resumen ──────────────────────────────────────────────────────────
            var wr = wb.Worksheets.Add("Resumen");
            var enc = new[] { "Cuenta", "Conciliados", "Diferencias", "Sólo ARCA", "Sólo PRESEA", "Anuladas", "Total ARCA", "Total PRESEA", "Diferencia" };
            Titulo(wr, $"Resumen por cuenta — {nombrePerfil}", enc.Length);
            Encabezado(wr, 2, enc);
            fila = 3;
            foreach (var s in resultado.Resumen)
            {
                object[] v = { s.CuentaTexto, s.Conciliados, s.Diferencias, s.SoloArca, s.SoloPresea, s.Anuladas, s.TotalArca, s.TotalPresea, s.Diferencia };
                for (int c = 0; c < v.Length; c++)
                {
                    var cell = wr.Cell(fila, c + 1);
                    switch (v[c])
                    {
                        case decimal m: cell.Value = m; cell.Style.NumberFormat.Format = "#,##0.00"; break;
                        case int n:     cell.Value = n; break;
                        default:        cell.Value = v[c]?.ToString(); break;
                    }
                }
                fila++;
            }

            // Lo que quedó afuera se deja escrito: si no, el total de ARCA del resumen no cierra
            // contra el archivo y parece un error.
            fila++;
            foreach (var (grupo, n) in resultado.FueraDeAlcance)
                wr.Cell(fila++, 1).Value = $"{n} registros de ARCA fuera de alcance: {grupo} (sin mayor cargado)";
            if (resultado.ArcaImporteCero > 0)
                wr.Cell(fila++, 1).Value = $"{resultado.ArcaImporteCero} registros de ARCA con importe 0 excluidos";
            foreach (var (dir, n) in resultado.PorDirectiva)
                wr.Cell(fila++, 1).Value = $"Directiva {dir}: {n} emparejados";
            wr.Columns().AdjustToContents();

            wb.SaveAs(rutaArchivo);
        }

        private static void Titulo(IXLWorksheet ws, string texto, int columnas)
        {
            ws.Cell(1, 1).Value = texto;
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 12;
            ws.Range(1, 1, 1, columnas).Merge();
        }

        private static void Encabezado(IXLWorksheet ws, int fila, string[] titulos)
        {
            for (int c = 0; c < titulos.Length; c++)
            {
                var cell = ws.Cell(fila, c + 1);
                cell.Value = titulos[c];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(50, 50, 50);
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }
        }
    }
}
