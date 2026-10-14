using System;
using System.Collections.Generic;
using System.Linq;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;

namespace AgrupadorConceptos.Services
{
    // Fecha: 07/10/2026 - TAREA: 00073 - Linea: 2 - Validar antes de abrir la conexión y compartir las reglas con cualquier host del módulo.
    public static class ConsultaMovimientosService
    {
        public static List<string> ObtenerConceptos() => ConsultaMovimientosStorage.ObtenerConceptos();

        public static List<MovimientoConsulta> Consultar(DateTime desde, DateTime hasta, IEnumerable<string> conceptos)
        {
            var seleccion = ValidarFiltros(desde, hasta, conceptos);
            // Fecha: 07/10/2026 - TAREA: 00074 - Linea: 1 - Presentar importes absolutos en grilla, totales y Excel sin modificar los signos almacenados.
            var movimientos = ConsultaMovimientosStorage.Consultar(desde, hasta, seleccion);
            foreach (var movimiento in movimientos)
            {
                movimiento.Debitos = movimiento.Debitos.HasValue ? Math.Abs(movimiento.Debitos.Value) : null;
                movimiento.Creditos = movimiento.Creditos.HasValue ? Math.Abs(movimiento.Creditos.Value) : null;
            }
            return movimientos;
        }

        public static string[] ValidarFiltros(DateTime desde, DateTime hasta, IEnumerable<string> conceptos)
        {
            if (desde.Date > hasta.Date)
                throw new ArgumentException("La fecha Desde no puede ser posterior a Hasta.");
            var seleccion = conceptos?.Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToArray() ?? Array.Empty<string>();
            if (seleccion.Length == 0)
                throw new ArgumentException("Seleccione al menos un concepto final.");
            if (seleccion.Length > 2000)
                throw new ArgumentException("Seleccione hasta 2.000 conceptos por consulta.");
            return seleccion;
        }

        // Fecha: 07/10/2026 - TAREA: 00073 - Linea: 3 - Exportar la misma lista consultada mantiene paridad con la pantalla y conserva fechas e importes reales en Excel.
        public static void Exportar(IReadOnlyCollection<MovimientoConsulta> movimientos, string ruta)
        {
            ValidarExportacion(movimientos);
            TablaExcelExporter.Exportar(Encabezados, Filas(movimientos), Titulo, ruta, "Movimientos");
        }

        // Fecha: 14/10/2026 - TAREA: 00097 - Linea: 3 - La web descarga el mismo libro como bytes.
        public static byte[] ExportarBytes(IReadOnlyCollection<MovimientoConsulta> movimientos)
        {
            ValidarExportacion(movimientos);
            return TablaExcelExporter.ExportarBytes(Encabezados, Filas(movimientos), Titulo, "Movimientos");
        }

        private const string Titulo = "Consulta de movimientos por conceptos finales";
        private static readonly List<string> Encabezados = new List<string> { "Fecha", "Concepto original", "Descripción original", "Débitos", "Créditos",
            "Concepto estándar", "Concepto final", "Cuenta final", "Perfil" };

        private static void ValidarExportacion(IReadOnlyCollection<MovimientoConsulta> movimientos)
        {
            if (movimientos == null || movimientos.Count == 0)
                throw new ArgumentException("No hay movimientos para exportar.");
        }

        private static List<object[]> Filas(IReadOnlyCollection<MovimientoConsulta> movimientos) =>
            movimientos.Select(m => new object[] { m.Fecha, m.ConceptoOriginal, m.DescripcionOriginal,
                m.Debitos, m.Creditos, m.ConceptoEstandar, m.ConceptoFinal, m.CuentaFinal, m.NombrePerfil }).ToList();
    }
}
