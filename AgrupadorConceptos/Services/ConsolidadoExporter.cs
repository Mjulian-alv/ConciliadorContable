using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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

    // Fecha: 23/10/2026 - TAREA: 00126 - Linea: 1 - Línea de la hoja «Asiento»: saldo final de una cuenta (o de un concepto sin cuenta) repartido en Debe o Haber.
    public class LineaAsiento
    {
        public string Cuenta { get; set; }
        public string Descripcion { get; set; }
        public decimal Debe { get; set; }
        public decimal Haber { get; set; }
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

        // Fecha: 23/10/2026 - TAREA: 00126 - Linea: 1 - Asiento a partir de los saldos finales por CuentaFinal.
        /// <summary>
        /// Misma base que <see cref="Calcular"/> (sin los pendientes). Con CuentaFinal: una línea por cuenta, con el nombre de
        /// la cuenta como descripción. Sin cuenta: una línea por concepto final, sin cuenta y con el concepto como descripción.
        /// El saldo de la línea es créditos menos débitos (como el consolidado): positivo va al Haber, negativo al Debe, en positivo.
        /// Las líneas con saldo cero no se informan. Van primero las del Debe y después las del Haber; en cada grupo, las que tienen cuenta
        /// (por cuenta) y después las que no (por descripción).
        /// </summary>
        public static List<LineaAsiento> CalcularAsiento(IEnumerable<MovimientoProcesado> movimientos)
        {
            var baseCalculo = movimientos.Where(m => !ConceptosBancarios.EstaPendiente(m.ConceptoFinal)).ToList();
            decimal Saldo(IEnumerable<MovimientoProcesado> g) =>
                Math.Round(g.Sum(x => x.Creditos) - Math.Abs(g.Sum(x => x.Debitos)), 2);
            LineaAsiento Linea(string cuenta, string descripcion, decimal saldo) => new LineaAsiento
            {
                Cuenta = cuenta, Descripcion = descripcion,
                Debe = saldo < 0 ? -saldo : 0m, Haber = saldo > 0 ? saldo : 0m
            };

            var conCuenta = baseCalculo
                .Where(m => !string.IsNullOrWhiteSpace(m.CuentaFinal))
                .GroupBy(m => m.CuentaFinal.Trim())
                .OrderBy(g => g.Key, StringComparer.Ordinal)
                .Select(g => Linea(g.Key, g.Select(x => x.NombreCuenta).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? "", Saldo(g)));
            var sinCuenta = baseCalculo
                .Where(m => string.IsNullOrWhiteSpace(m.CuentaFinal))
                .GroupBy(m => m.ConceptoFinal)
                .OrderBy(g => g.Key, StringComparer.Ordinal)
                .Select(g => Linea("", g.Key, Saldo(g)));
            // Fecha: 23/10/2026 - TAREA: 00127 - Linea: 1 - Primero las líneas del Debe y después las del Haber; dentro de cada grupo se conserva el orden
            // anterior (con cuenta por cuenta, después las sin cuenta por descripción). OrderBy es estable, así que sólo parte la lista en dos.
            return conCuenta.Concat(sinCuenta).Where(l => l.Debe != 0 || l.Haber != 0).OrderBy(l => l.Debe != 0 ? 0 : 1).ToList();
        }

        // Fecha: 23/10/2026 - TAREA: 00126 - Linea: 1 - El libro lleva dos hojas: Consolidado y Asiento.
        private static XLWorkbook Construir(List<LineaConsolidado> lineas, List<LineaAsiento> asiento, string titulo)
        {
            var wb = new XLWorkbook();
            TablaExcelExporter.AgregarHoja(wb, new List<string> { "Concepto Final", "Débitos", "Créditos", "Saldo" },
                lineas.Select(i => new object[] { i.Concepto, i.Debitos, i.Creditos, i.Saldo }).ToList(), titulo, "Consolidado");

            asiento ??= new List<LineaAsiento>();
            var filas = asiento.Select(l => new object[] { l.Cuenta, l.Descripcion, l.Debe, l.Haber }).ToList();
            decimal debe = asiento.Sum(l => l.Debe), haber = asiento.Sum(l => l.Haber);
            // Sin contrapartida del banco el asiento no cierra: la diferencia es el neto del extracto.
            filas.Add(new object[] { "", "Total", debe, haber });
            filas.Add(new object[] { "", "Diferencia (Haber - Debe)", "", haber - debe });
            TablaExcelExporter.AgregarHoja(wb, new List<string> { "Cuenta", "Descripción", "Debe", "Haber" },
                filas, "Asiento - " + titulo, "Asiento", filasEnNegrita: 2);
            return wb;
        }

        /// <summary>
        /// El consolidado se escribe con el mismo formato que el resto de las
        /// exportaciones del módulo; lo propio es el armado de las columnas y, desde la 00126, la hoja «Asiento».
        /// </summary>
        public static void ExportarAExcel(List<LineaConsolidado> lineas, List<LineaAsiento> asiento, string titulo, string rutaDestino)
        {
            using var wb = Construir(lineas, asiento, titulo);
            wb.SaveAs(rutaDestino);
        }

        // Fecha: 14/10/2026 - TAREA: 00097 - Linea: 2 - La web descarga el mismo libro como bytes.
        public static byte[] ExportarBytes(List<LineaConsolidado> lineas, List<LineaAsiento> asiento, string titulo)
        {
            using var wb = Construir(lineas, asiento, titulo);
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}
