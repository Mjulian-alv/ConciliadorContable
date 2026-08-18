using System;
using System.Collections.Generic;
using System.Linq;
using AgrupadorConceptos.Models;

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

        /// <summary>
        /// El consolidado se escribe con el mismo formato que el resto de las
        /// exportaciones del módulo; lo único propio es el armado de las cuatro columnas.
        /// </summary>
        public static void ExportarAExcel(List<LineaConsolidado> lineas, string titulo, string rutaDestino)
        {
            var encabezados = new List<string> { "Concepto Final", "Débitos", "Créditos", "Saldo" };

            var filas = new List<object[]>();
            foreach (var item in lineas)
                filas.Add(new object[] { item.Concepto, item.Debitos, item.Creditos, item.Saldo });

            TablaExcelExporter.Exportar(encabezados, filas, titulo, rutaDestino, "Consolidado");
        }
    }
}
