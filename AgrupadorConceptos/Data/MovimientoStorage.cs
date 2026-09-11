using System;
using System.Collections.Generic;
using System.Data;
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

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Persistir tambien CuentaFinal
        private const string UpdateConceptos =
            "UPDATE bancos.MovimientosArchivo SET ConceptoEstandar = @ConceptoEstandar, ConceptoFinal = @ConceptoFinal, CuentaFinal = @CuentaFinal WHERE Id = @Id";

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

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 1 - Movimientos de todos los archivos del perfil
        // La homologacion es del perfil, no del archivo: una baja tiene que alcanzar
        // tambien a las sesiones historicas, no solo a la que este abierta.
        /// <summary>Movimientos de todos los archivos importados de un perfil.</summary>
        public static List<MovimientoProcesado> ObtenerPorPerfil(int idPerfilBanco)
        {
            using var cn = DatabaseHelper.Open();
            return cn.Query<MovimientoProcesado>(@"
                SELECT m.* FROM bancos.MovimientosArchivo m
                JOIN bancos.ArchivosImportados a ON m.IdArchivo = a.Id
                WHERE a.IdPerfilBanco = @IdPerfil",
                new { IdPerfil = idPerfilBanco }).ToList();
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Conciliacion interna se arma sobre todos los extractos, no uno solo
        /// <summary>Todos los movimientos importados, de cualquier perfil/extracto.</summary>
        public static List<MovimientoProcesado> ObtenerTodos()
        {
            using var cn = DatabaseHelper.Open();
            return cn.Query<MovimientoProcesado>("SELECT Id, IdArchivo,  TRY_CONVERT( NVARCHAR(30), TRY_CONVERT(DATE, fecha, 103) ) AS Fecha, ConceptoOriginal, DescripcionOriginal, Debitos, Creditos, ConceptoEstandar, ConceptoFinal, CuentaFinal FROM bancos.MovimientosArchivo").ToList();
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
                // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Persistir CuentaFinal tambien al insertar el lote
                mov.Id = cn.QuerySingle<int>(@"
                    INSERT INTO bancos.MovimientosArchivo
                        (IdArchivo, Fecha, ConceptoOriginal, DescripcionOriginal, Debitos, Creditos, ConceptoEstandar, ConceptoFinal, CuentaFinal)
                    OUTPUT INSERTED.Id
                    VALUES (@IdArchivo, @Fecha, @ConceptoOriginal, @DescripcionOriginal, @Debitos, @Creditos, @ConceptoEstandar, @ConceptoFinal, @CuentaFinal);",
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

            ActualizarConceptos(lista, cn, tx);

            tx.Commit();
        }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 1 - Escribir dentro de una transaccion ajena
        // La baja tiene que borrar la regla y reescribir los movimientos de forma atomica.
        // Si fueran dos transacciones y fallara la segunda, la regla quedaria borrada y los
        // movimientos apuntando a un concepto que ya no tiene quien lo respalde.
        /// <summary>
        /// Igual que <see cref="ActualizarConceptos(IEnumerable{MovimientoProcesado})"/> pero
        /// dentro de la conexión y transacción que abrió el llamador. No commitea.
        /// </summary>
        public static void ActualizarConceptos(
            IEnumerable<MovimientoProcesado> movimientos, IDbConnection cn, IDbTransaction tx)
        {
            var lista = movimientos?.ToList() ?? new List<MovimientoProcesado>();
            if (lista.Count == 0) return;

            foreach (var mov in lista)
                cn.Execute(UpdateConceptos, mov, tx);
        }

        /// <summary>Edición puntual del ConceptoFinal desde la grilla.</summary>
        public static void ActualizarConceptoFinal(int id, string conceptoFinal)
        {
            using var cn = DatabaseHelper.Open();
            cn.Execute("UPDATE bancos.MovimientosArchivo SET ConceptoFinal = @ConceptoFinal WHERE Id = @Id",
                new { ConceptoFinal = conceptoFinal, Id = id });
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Edicion puntual de CuentaFinal desde la grilla
        /// <summary>Edición puntual de la CuentaFinal desde la grilla.</summary>
        public static void ActualizarCuentaFinal(int id, string cuentaFinal)
        {
            using var cn = DatabaseHelper.Open();
            cn.Execute("UPDATE bancos.MovimientosArchivo SET CuentaFinal = @CuentaFinal WHERE Id = @Id",
                new { CuentaFinal = cuentaFinal, Id = id });
        }
    }
}
