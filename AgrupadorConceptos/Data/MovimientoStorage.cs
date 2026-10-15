// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Operar sobre días tipados evita reinterpretaciones culturales.
using AgrupadorConceptos.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
// Fecha: 14/10/2026 - TAREA: 00097 - Linea: 1 - Se retira un using de Telerik que no se usaba: el Core no referencia WinForms.

namespace AgrupadorConceptos.Data
{
    /// <summary>
    /// Acceso a bancos.MovimientosArchivo: las líneas del extracto importado.
    /// </summary>
    internal static class MovimientoStorage
    {
        // Fecha: 15/10/2026 - TAREA: 00098 - Linea: 1 - Traer el nombre de la cuenta final (LEFT JOIN con bancos.CuentasContables)
        private const string SelectPorArchivo =
            $@"SELECT mov.*,cc.descripcion as nombrecuenta FROM bancos.MovimientosArchivo mov 
            LEFT JOIN bancos.CuentasContables CC on mov.cuentafinal = cc.cuenta 
            WHERE mov.IdArchivo = @IdArchivo";

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
            // Fecha: 09/10/2026 - TAREA: 00084 - Linea: 1 - Identificar el perfil permite distinguir movimientos de varios extractos en una misma conciliación.
            return cn.Query<MovimientoProcesado>(
                @"SELECT m.*, p.NombreBanco
                  FROM bancos.MovimientosArchivo m
                  LEFT JOIN bancos.ArchivosImportados a ON a.Id = m.IdArchivo
                  LEFT JOIN bancos.PerfilesBanco p ON p.Id = a.IdPerfilBanco
                  WHERE m.IdArchivo IN @Ids",
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
            return cn.Query<MovimientoProcesado>(@"SELECT mv.Id, mv.IdArchivo, mv.Fecha,
                mv.ConceptoOriginal, mv.DescripcionOriginal, mv.Debitos, mv.Creditos, mv.ConceptoEstandar, mv.ConceptoFinal, mv.CuentaFinal, pf.NombreBanco
               FROM bancos.MovimientosArchivo mv
               left join bancos.archivosimportados ai on ai.id = mv.IdArchivo
               left join bancos.perfilesbanco pf on pf.id = ai.IdPerfilBanco").ToList();
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
            // Fecha: 08/10/2026 - TAREA: 00076 - Linea: 1 - Insertar movimientos espera la decisión de baja de archivo.
            ConciliacionMutationStorage.Bloquear(cn, tx);

            var ids = InsertarLote(movimientos, cn, tx, null, progreso);
            tx.Commit();
            for (int i=0;i<movimientos.Count;i++) movimientos[i].Id=ids[i];
        }

        // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Operar sobre días tipados evita reinterpretaciones culturales.
        public static List<int> InsertarLote(List<MovimientoProcesado> movimientos, IDbConnection cn, IDbTransaction tx, int? idArchivo, Action<int,int> progreso = null)
        {
            var ids=new List<int>();
            foreach(var mov in movimientos)
            {
                var p=new DynamicParameters(mov);
                p.Add("IdArchivo", idArchivo ?? mov.IdArchivo);
                p.Add("Fecha", mov.Fecha.Date, DbType.Date);
                ids.Add(cn.QuerySingle<int>(@"INSERT INTO bancos.MovimientosArchivo
                    (IdArchivo,Fecha,ConceptoOriginal,DescripcionOriginal,Debitos,Creditos,ConceptoEstandar,ConceptoFinal,CuentaFinal)
                    OUTPUT INSERTED.Id VALUES (@IdArchivo,@Fecha,@ConceptoOriginal,@DescripcionOriginal,@Debitos,@Creditos,@ConceptoEstandar,@ConceptoFinal,@CuentaFinal)",p,tx));
                progreso?.Invoke(ids.Count,movimientos.Count);
            }
            return ids;
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
            // Fecha: 08/10/2026 - TAREA: 00076 - Linea: 1 - Cambiar conceptos puede alterar el alcance interno de una sesión.
            ConciliacionMutationStorage.Bloquear(cn, tx);

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
            // Fecha: 08/10/2026 - TAREA: 00076 - Linea: 1 - También los llamadores con transacción ajena respetan la decisión de baja.
            ConciliacionMutationStorage.Bloquear(cn, tx);
            foreach (var mov in lista)
                cn.Execute(UpdateConceptos, mov, tx);
        }

        /// <summary>Edición puntual del ConceptoFinal desde la grilla.</summary>
        public static void ActualizarConceptoFinal(int id, string conceptoFinal)
        {
            using var cn = DatabaseHelper.Open();
            // Fecha: 08/10/2026 - TAREA: 00076 - Linea: 1 - La edición puntual también modifica el alcance interno.
            using var tx = cn.BeginTransaction();
            ConciliacionMutationStorage.Bloquear(cn, tx);
            cn.Execute("UPDATE bancos.MovimientosArchivo SET ConceptoFinal = @ConceptoFinal WHERE Id = @Id",
                new { ConceptoFinal = conceptoFinal, Id = id }, tx);
            tx.Commit();
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
