// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Operar sobre días tipados evita reinterpretaciones culturales.
using System;
using System.Collections.Generic;
using System.Linq;
using AgrupadorConceptos.Models;
using Dapper;

namespace AgrupadorConceptos.Data
{
    // Fecha: 07/10/2026 - TAREA: 00073 - Linea: 2 - Filtrar en SQL evita cargar todos los extractos en el hilo de la ventana.
    internal static class ConsultaMovimientosStorage
    {
        public static List<string> ObtenerConceptos()
        {
            using var cn = DatabaseHelper.Open();
            return cn.Query<string>(@"
                SELECT DISTINCT ConceptoFinal FROM bancos.MovimientosArchivo
                WHERE NULLIF(LTRIM(RTRIM(ConceptoFinal)), '') IS NOT NULL
                ORDER BY ConceptoFinal").ToList();
        }

        public static List<MovimientoConsulta> Consultar(DateTime desde, DateTime hasta, string[] conceptos)
        {
            using var cn = DatabaseHelper.Open();
            // Fecha: 07/10/2026 - TAREA: 00073 - Linea: 2 - La fecha operativa ya es date: filtrar sin conversiones.
            return cn.Query<MovimientoConsulta>(@"
                SELECT m.Fecha, m.ConceptoOriginal, m.DescripcionOriginal,
                       m.Debitos, m.Creditos, m.ConceptoEstandar, m.ConceptoFinal,
                       m.CuentaFinal, p.NombreBanco AS NombrePerfil
                FROM bancos.MovimientosArchivo m
                LEFT JOIN bancos.ArchivosImportados a ON a.Id = m.IdArchivo
                LEFT JOIN bancos.PerfilesBanco p ON p.Id = a.IdPerfilBanco
                WHERE m.Fecha >= @Desde AND m.Fecha <= @Hasta
                  AND m.ConceptoFinal IN @Conceptos
                ORDER BY m.Fecha, p.NombreBanco, m.ConceptoFinal, m.Id",
                new { Desde = desde.Date, Hasta = hasta.Date, Conceptos = conceptos },
                commandTimeout: 120).ToList();
        }
    }
}
