using System;
using System.Collections.Generic;
using System.Linq;
using AgrupadorConceptos.Models;
using Dapper;

namespace AgrupadorConceptos.Data
{
    /// <summary>
    /// Acceso a bancos.MovimientosArchivo: las líneas del extracto importado.
    /// </summary>
    internal static class MovimientoStorage
    {
        private const string SelectPorArchivo =
            "SELECT * FROM bancos.MovimientosArchivo WHERE IdArchivo = @IdArchivo";

        private const string UpdateConceptos =
            "UPDATE bancos.MovimientosArchivo SET ConceptoEstandar = @ConceptoEstandar, ConceptoFinal = @ConceptoFinal WHERE Id = @Id";

        public static List<MovimientoProcesado> ObtenerPorArchivo(int idArchivo)
        {
            using var cn = DatabaseHelper.Open();
            return cn.Query<MovimientoProcesado>(SelectPorArchivo, new { IdArchivo = idArchivo }).ToList();
        }

        /// <summary>
        /// Movimientos de varios archivos. Usa la expansión de listas de Dapper
        /// (IN @Ids) en vez de interpolar los ids en el SQL.
        /// </summary>
        public static List<MovimientoProcesado> ObtenerPorArchivos(IEnumerable<int> idsArchivos)
        {
            var ids = idsArchivos?.ToList() ?? new List<int>();
            if (ids.Count == 0) return new List<MovimientoProcesado>();

            using var cn = DatabaseHelper.Open();
            return cn.Query<MovimientoProcesado>(
                "SELECT * FROM bancos.MovimientosArchivo WHERE IdArchivo IN @Ids",
                new { Ids = ids }).ToList();
        }

        /// <summary>Conceptos finales distintos ya homologados, para elegir qué conciliar.</summary>
        public static List<string> ObtenerConceptosFinalesDistintos(IEnumerable<int> idsArchivos)
        {
            var ids = idsArchivos?.ToList() ?? new List<int>();
            if (ids.Count == 0) return new List<string>();

            using var cn = DatabaseHelper.Open();
            return cn.Query<string>(@"
                SELECT DISTINCT ConceptoFinal FROM bancos.MovimientosArchivo
                WHERE IdArchivo IN @Ids
                  AND ConceptoFinal IS NOT NULL AND ConceptoFinal <> ''
                ORDER BY ConceptoFinal",
                new { Ids = ids }).ToList();
        }

        /// <summary>
        /// Inserta el lote completo en una sola transacción y le asigna el Id a cada
        /// movimiento. Sin la transacción cada INSERT commitea por separado y espera
        /// un fsync de disco por fila.
        /// El callback se invoca una vez por fila: si actualiza UI, el llamador es
        /// responsable de throttlearlo.
        /// </summary>
        public static void InsertarLote(List<MovimientoProcesado> movimientos, Action<int, int> progreso = null)
        {
            if (movimientos == null || movimientos.Count == 0) return;

            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();

            int guardados = 0;
            int total = movimientos.Count;

            foreach (var mov in movimientos)
            {
                mov.Id = cn.QuerySingle<int>(@"
                    INSERT INTO bancos.MovimientosArchivo
                        (IdArchivo, Fecha, ConceptoOriginal, DescripcionOriginal, Debitos, Creditos, ConceptoEstandar, ConceptoFinal)
                    OUTPUT INSERTED.Id
                    VALUES (@IdArchivo, @Fecha, @ConceptoOriginal, @DescripcionOriginal, @Debitos, @Creditos, @ConceptoEstandar, @ConceptoFinal);",
                    mov, tx);

                guardados++;
                progreso?.Invoke(guardados, total);
            }

            tx.Commit();
        }

        /// <summary>
        /// Persiste ConceptoEstandar/ConceptoFinal de varios movimientos en una sola
        /// transacción, por el mismo motivo que InsertarLote.
        /// </summary>
        public static void ActualizarConceptos(IEnumerable<MovimientoProcesado> movimientos)
        {
            var lista = movimientos?.ToList() ?? new List<MovimientoProcesado>();
            if (lista.Count == 0) return;

            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();

            foreach (var mov in lista)
                cn.Execute(UpdateConceptos, mov, tx);

            tx.Commit();
        }

        /// <summary>Edición puntual del ConceptoFinal desde la grilla.</summary>
        public static void ActualizarConceptoFinal(int id, string conceptoFinal)
        {
            using var cn = DatabaseHelper.Open();
            cn.Execute("UPDATE bancos.MovimientosArchivo SET ConceptoFinal = @ConceptoFinal WHERE Id = @Id",
                new { ConceptoFinal = conceptoFinal, Id = id });
        }
    }
}
