// Fecha: 08/10/2026 - TAREA: 00076 - Linea: 2, 3 - Excel representa cada externo una sola vez y conserva números y fechas tipadas.
using System;
using System.Collections.Generic;
using System.Linq;
using AgrupadorConceptos.Models;
using ClosedXML.Excel;

namespace AgrupadorConceptos.Services
{
    public static class ConciliacionExcelExporter
    {
        public static void Exportar(IEnumerable<ConciliacionPar> pares, IEnumerable<ConciliacionItemExterno> pendientesExternos,
            IEnumerable<MovimientoProcesado> pendientesExtracto, string ruta)
        {
            using var libro = new XLWorkbook();
            var conciliados = libro.Worksheets.Add("Conciliados");
            string[] titulos = { "Fecha Externo", "Importe Externo", "Detalle", "Fecha Extracto", "Importe Extracto",
                "Concepto Final", "Tipo Match" };
            for (int c = 0; c < titulos.Length; c++) conciliados.Cell(1, c + 1).Value = titulos[c];
            int fila = 2;
            foreach (var grupo in pares.GroupBy(p => (p.IdSesion, p.IdItemExterno)).OrderBy(g => g.Key.IdSesion).ThenBy(g => g.Key.IdItemExterno))
            {
                bool primero = true;
                foreach (var par in grupo)
                {
                    if (primero)
                    {
                        EscribirFecha(conciliados, fila, 1, par.FechaExterno);
                        conciliados.Cell(fila, 2).Value = par.ImporteExterno;
                        conciliados.Cell(fila, 3).Value = par.DetalleExterno ?? "";
                    }
                    EscribirFecha(conciliados, fila, 4, par.FechaExtracto);
                    conciliados.Cell(fila, 5).Value = par.ImporteExtracto;
                    conciliados.Cell(fila, 6).Value = par.ConceptoFinalExtracto ?? "";
                    conciliados.Cell(fila, 7).Value = par.TipoMatch ?? "";
                    fila++;
                    primero = false;
                }
            }
            conciliados.Column(2).Style.NumberFormat.Format = "#,##0.00";
            conciliados.Column(5).Style.NumberFormat.Format = "#,##0.00";

            var pendientes = libro.Worksheets.Add("Pendientes");
            pendientes.Cell(1, 1).Value = "Origen";
            pendientes.Cell(1, 2).Value = "Fecha";
            pendientes.Cell(1, 3).Value = "Importe";
            pendientes.Cell(1, 4).Value = "Detalle / Concepto";
            int pendienteFila = 2;
            foreach (var externo in pendientesExternos)
            {
                pendientes.Cell(pendienteFila, 1).Value = "Externo";
                EscribirFecha(pendientes, pendienteFila, 2, externo.Fecha);
                pendientes.Cell(pendienteFila, 3).Value = externo.Importe;
                pendientes.Cell(pendienteFila, 4).Value = externo.Detalle ?? "";
                pendienteFila++;
            }
            foreach (var movimiento in pendientesExtracto)
            {
                pendientes.Cell(pendienteFila, 1).Value = "Extracto";
                EscribirFecha(pendientes, pendienteFila, 2, movimiento.Fecha);
                pendientes.Cell(pendienteFila, 3).Value = movimiento.Debitos != 0 ? movimiento.Debitos : movimiento.Creditos;
                pendientes.Cell(pendienteFila, 4).Value = movimiento.ConceptoFinal ?? "";
                pendienteFila++;
            }
            pendientes.Column(3).Style.NumberFormat.Format = "#,##0.00";
            conciliados.Columns().AdjustToContents();
            pendientes.Columns().AdjustToContents();
            libro.SaveAs(ruta);
        }

        // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Operar sobre días tipados evita reinterpretaciones culturales.
        private static void EscribirFecha(IXLWorksheet hoja, int fila, int columna, DateTime fecha)
        {
            hoja.Cell(fila, columna).Value = fecha.Date;
            hoja.Cell(fila, columna).Style.DateFormat.Format = "dd/mm/yyyy";
        }
    }
}
