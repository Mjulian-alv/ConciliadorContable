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
        // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 1 - Los procesos automáticos ya no escriben ConceptoFinal desde la copia en memoria
        // Una copia desactualizada (otra sesión, un diálogo de baja calculado antes de la edición) pisaba lo que el usuario había
        // editado y lo dejaba igual al estándar. Ahora la condición se evalúa en el UPDATE, contra lo que hay en la base en ese
        // instante: el final sólo sigue al estándar si está vacío, pendiente o es igual al estándar vigente. Con CuentaFinal
        // pasa lo mismo: sólo se completa si está vacía. Un archivo blindado no se toca (la homologación está cerrada).
        // Los SET de un UPDATE leen los valores previos, así que ConceptoEstandar se cambia en la misma sentencia sin perder la comparación.
        private const string UpdateConceptos = @"UPDATE m SET
                ConceptoFinal    = CASE WHEN LTRIM(RTRIM(ISNULL(m.ConceptoFinal, N''))) = N'' OR m.ConceptoFinal = @Pendiente
                                             OR m.ConceptoFinal = m.ConceptoEstandar THEN @ConceptoEstandar ELSE m.ConceptoFinal END,
                CuentaFinal      = CASE WHEN LTRIM(RTRIM(ISNULL(m.CuentaFinal, N''))) = N'' THEN @CuentaFinal ELSE m.CuentaFinal END,
                ConceptoEstandar = @ConceptoEstandar
            FROM bancos.MovimientosArchivo m
            WHERE m.Id = @Id
              AND NOT EXISTS (SELECT 1 FROM bancos.ArchivosImportados a WHERE a.Id = m.IdArchivo AND a.Blindado = 1)";

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
               left join bancos.perfilesbanco pf on pf.id = ai.IdPerfilBanco
               -- Fecha: 23/10/2026 - TAREA: 00125 - Linea: 3 - Lo ya conciliado (a mano o por una sesión finalizada) no es candidato de otra conciliación
               WHERE mv.Conciliado = 0").ToList();
        }

        // Fecha: 23/10/2026 - TAREA: 00125 - Linea: 3 - Marcado manual de movimientos como conciliados
        /// <summary>
        /// Marca como conciliados los movimientos libres. Los que ya figuran en algún par de conciliación
        /// (interna o externa, en proceso o finalizada) o ya están conciliados no se tocan y vuelven en
        /// <c>Omitidos</c> con el motivo. Todo bajo el lock de mutaciones y en una sola transacción.
        /// </summary>
        public static (int Marcados, List<(int Id, string Motivo)> Omitidos) MarcarConciliados(IReadOnlyCollection<int> ids)
        {
            var omitidos = new List<(int Id, string Motivo)>();
            var pedidos = ids?.Distinct().ToList() ?? new List<int>();
            if (pedidos.Count == 0) return (0, omitidos);

            // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 2 - El historial distingue el marcado manual de las conciliaciones de sesión
            using var origen = AuditoriaContexto.Usar(AuditoriaContexto.ConciliacionManual);
            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();
            ConciliacionMutationStorage.Bloquear(cn, tx);

            var yaConciliados = cn.Query<int>(
                "SELECT Id FROM bancos.MovimientosArchivo WHERE Id IN @Ids AND Conciliado = 1", new { Ids = pedidos }, tx).ToHashSet();
            // Los pares protegen aunque la sesión esté finalizada o ya no exista (mismo criterio que la baja de archivos).
            var enPares = cn.Query<(int Id, string Donde)>(@"
                SELECT p.IdMovimientoProcesado AS Id, N'conciliación externa' AS Donde FROM bancos.ConciliacionPares p WHERE p.IdMovimientoProcesado IN @Ids
                UNION ALL SELECT p.IdMovimientoA, N'conciliación interna' FROM bancos.ConciliacionInternaPares p WHERE p.IdMovimientoA IN @Ids
                UNION ALL SELECT p.IdMovimientoB, N'conciliación interna' FROM bancos.ConciliacionInternaPares p WHERE p.IdMovimientoB IN @Ids",
                new { Ids = pedidos }, tx).GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First().Donde);

            var libres = new List<int>();
            foreach (int id in pedidos)
            {
                if (yaConciliados.Contains(id)) omitidos.Add((id, "ya estaba conciliado"));
                else if (enPares.TryGetValue(id, out var donde)) omitidos.Add((id, $"está en una {donde}"));
                else libres.Add(id);
            }
            // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 3 - Conciliar exige que los archivos de los movimientos estén blindados; si alguno no lo está se rechaza todo
            BlindajeStorage.ExigirBlindadosPorMovimientos(cn, tx, libres);
            if (libres.Count > 0)
                cn.Execute(@"UPDATE bancos.MovimientosArchivo SET Conciliado = 1, ConciliadoFecha = CAST(GETDATE() AS date)
                    WHERE Id IN @Ids AND Conciliado = 0", new { Ids = libres }, tx);
            tx.Commit();
            return (libres.Count, omitidos);
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

            // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 3 - No se agregan movimientos a un archivo blindado
            foreach (int idArchivo in movimientos.Select(m => m.IdArchivo).Distinct())
                BlindajeStorage.ExigirNoBlindado(cn, tx, idArchivo);
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
        public static int ActualizarConceptos(IEnumerable<MovimientoProcesado> movimientos)
        {
            var lista = movimientos?.ToList() ?? new List<MovimientoProcesado>();
            if (lista.Count == 0) return 0;

            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();
            // Fecha: 08/10/2026 - TAREA: 00076 - Linea: 1 - Cambiar conceptos puede alterar el alcance interno de una sesión.
            ConciliacionMutationStorage.Bloquear(cn, tx);

            int actualizados = ActualizarConceptos(lista, cn, tx);

            tx.Commit();
            return actualizados;
        }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 1 - Escribir dentro de una transaccion ajena
        // La baja tiene que borrar la regla y reescribir los movimientos de forma atomica.
        // Si fueran dos transacciones y fallara la segunda, la regla quedaria borrada y los
        // movimientos apuntando a un concepto que ya no tiene quien lo respalde.
        /// <summary>
        /// Igual que <see cref="ActualizarConceptos(IEnumerable{MovimientoProcesado})"/> pero
        /// dentro de la conexión y transacción que abrió el llamador. No commitea.
        /// </summary>
        /// <returns>Cuántos movimientos se escribieron: los de un archivo blindado se saltean y no cuentan.</returns>
        public static int ActualizarConceptos(
            IEnumerable<MovimientoProcesado> movimientos, IDbConnection cn, IDbTransaction tx)
        {
            var lista = movimientos?.ToList() ?? new List<MovimientoProcesado>();
            if (lista.Count == 0) return 0;
            // Fecha: 08/10/2026 - TAREA: 00076 - Linea: 1 - También los llamadores con transacción ajena respetan la decisión de baja.
            ConciliacionMutationStorage.Bloquear(cn, tx);
            // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 1 - Se pasa sólo lo que el UPDATE usa: el ConceptoFinal en memoria ya no se escribe
            int actualizados = 0;
            foreach (var mov in lista)
                actualizados += cn.Execute(UpdateConceptos,
                    new { mov.Id, mov.ConceptoEstandar, mov.CuentaFinal, Pendiente = ConceptosBancarios.PendienteHomologar }, tx);
            SincronizarDesdeBase(lista, cn, tx);
            return actualizados;
        }

        // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 1 - La copia en memoria queda igual a lo que decidió la base
        // Si el final estaba editado, el objeto del llamador conservaba su valor viejo o el que la lógica en memoria suponía;
        // releerlo evita que la grilla muestre algo distinto de lo guardado (y que un guardado posterior lo "corrija" mal).
        private static void SincronizarDesdeBase(List<MovimientoProcesado> lista, IDbConnection cn, IDbTransaction tx)
        {
            var enBase = cn.Query<(int Id, string ConceptoEstandar, string ConceptoFinal, string CuentaFinal)>(
                @"SELECT Id, ConceptoEstandar, ConceptoFinal, CuentaFinal FROM bancos.MovimientosArchivo
                  WHERE Id IN (SELECT CAST([value] AS int) FROM OPENJSON(@Ids))",
                new { Ids = System.Text.Json.JsonSerializer.Serialize(lista.Select(m => m.Id).Distinct().ToList()) }, tx)
                .ToDictionary(x => x.Id);
            foreach (var mov in lista)
            {
                if (!enBase.TryGetValue(mov.Id, out var b)) continue;
                mov.ConceptoEstandar = b.ConceptoEstandar;
                mov.ConceptoFinal = b.ConceptoFinal;
                mov.CuentaFinal = b.CuentaFinal;
            }
        }

        /// <summary>Edición puntual del ConceptoFinal desde la grilla.</summary>
        public static void ActualizarConceptoFinal(int id, string conceptoFinal)
        {
            // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 1 - Es la única vía que escribe ConceptoFinal sin condición, por eso queda como edición manual en el historial
            using var origen = AuditoriaContexto.Usar(AuditoriaContexto.Manual);
            using var cn = DatabaseHelper.Open();
            // Fecha: 08/10/2026 - TAREA: 00076 - Linea: 1 - La edición puntual también modifica el alcance interno.
            using var tx = cn.BeginTransaction();
            ConciliacionMutationStorage.Bloquear(cn, tx);
            // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 3 - Con el archivo blindado la homologación está cerrada
            BlindajeStorage.ExigirNoBlindadoPorMovimientos(cn, tx, new[] { id });
            cn.Execute("UPDATE bancos.MovimientosArchivo SET ConceptoFinal = @ConceptoFinal WHERE Id = @Id",
                new { ConceptoFinal = conceptoFinal, Id = id }, tx);
            tx.Commit();
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Edicion puntual de CuentaFinal desde la grilla
        /// <summary>Edición puntual de la CuentaFinal desde la grilla.</summary>
        public static void ActualizarCuentaFinal(int id, string cuentaFinal)
        {
            using var cn = DatabaseHelper.Open();
            // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 3 - Con el archivo blindado la cuenta sólo cambia al conciliar internamente
            using var tx = cn.BeginTransaction();
            ConciliacionMutationStorage.Bloquear(cn, tx);
            BlindajeStorage.ExigirNoBlindadoPorMovimientos(cn, tx, new[] { id });
            cn.Execute("UPDATE bancos.MovimientosArchivo SET CuentaFinal = @CuentaFinal WHERE Id = @Id",
                new { CuentaFinal = cuentaFinal, Id = id }, tx);
            tx.Commit();
        }
    }
}
