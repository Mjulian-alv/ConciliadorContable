// Fecha: 14/10/2026 - TAREA: 00097 - Linea: 5 - El Excel de la conciliación interna sale de un solo lugar para el escritorio y la web: importes como números reales, en valor absoluto, y fechas tipadas.
using System;
using System.Collections.Generic;
using System.IO;
using AgrupadorConceptos.Models;
using ClosedXML.Excel;

namespace AgrupadorConceptos.Services
{
    public static class ConciliacionInternaExcelExporter
    {
        public static byte[] ExportarBytes(IReadOnlyList<ConciliacionInternaPar> pares, IEnumerable<MovimientoProcesado> pendientesDebitos, IEnumerable<MovimientoProcesado> pendientesCreditos)
        {
            using var libro = new XLWorkbook();
            var conciliados = libro.Worksheets.Add("Conciliados");
            string[] titulos = { "Fecha Débito", "Importe Débito", "Banco Débito", "Fecha Crédito", "Importe Crédito", "Banco Crédito", "Tipo Match" };
            for (int c = 0; c < titulos.Length; c++) conciliados.Cell(1, c + 1).Value = titulos[c];
            for (int i = 0; i < pares.Count; i++)
            {
                var p = pares[i]; int r = i + 2;
                conciliados.Cell(r, 1).Value = p.FechaA.Date; conciliados.Cell(r, 2).Value = Math.Abs(p.ImporteA); conciliados.Cell(r, 3).Value = p.NombreBancoA ?? "";
                conciliados.Cell(r, 4).Value = p.FechaB.Date; conciliados.Cell(r, 5).Value = Math.Abs(p.ImporteB); conciliados.Cell(r, 6).Value = p.NombreBancoB ?? "";
                conciliados.Cell(r, 7).Value = p.TipoMatch ?? "";
            }
            conciliados.Column(1).Style.DateFormat.Format = "dd/mm/yyyy";
            conciliados.Column(4).Style.DateFormat.Format = "dd/mm/yyyy";
            conciliados.Column(2).Style.NumberFormat.Format = "#,##0.00";
            conciliados.Column(5).Style.NumberFormat.Format = "#,##0.00";

            var pendientes = libro.Worksheets.Add("Pendientes");
            string[] titulosPend = { "Lado", "Fecha", "Importe", "Banco" };
            for (int c = 0; c < titulosPend.Length; c++) pendientes.Cell(1, c + 1).Value = titulosPend[c];
            int fila = 2;
            void Escribir(string lado, MovimientoProcesado m)
            {
                pendientes.Cell(fila, 1).Value = lado; pendientes.Cell(fila, 2).Value = m.Fecha.Date;
                pendientes.Cell(fila, 3).Value = ComparadorConciliacion.ImporteEfectivo(m); pendientes.Cell(fila, 4).Value = m.NombreBanco ?? "";
                fila++;
            }
            foreach (var m in pendientesDebitos) Escribir("Débito", m);
            foreach (var m in pendientesCreditos) Escribir("Crédito", m);
            pendientes.Column(2).Style.DateFormat.Format = "dd/mm/yyyy";
            pendientes.Column(3).Style.NumberFormat.Format = "#,##0.00";
            conciliados.Columns().AdjustToContents(); pendientes.Columns().AdjustToContents();

            using var ms = new MemoryStream(); libro.SaveAs(ms); return ms.ToArray();
        }
    }
}
