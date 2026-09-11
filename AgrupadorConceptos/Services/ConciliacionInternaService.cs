// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - CRUD y auto-conciliacion entre extractos propios
// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Rediseño: sin seleccion de perfil, un solo rango de
// fechas y dos lados globales (Debitos/Creditos) en vez de "Extracto A"/"Extracto B" por perfil.
// El lado A de un par siempre es el movimiento debito, el lado B siempre el credito (ver
// CargarSinConciliar) — así Finalizar no necesita adivinar cuál es cuál.
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AgrupadorConceptos.Services
{
    public static class ConciliacionInternaService
    {
        // ── Sesiones ─────────────────────────────────────────────────────────────

        public static List<ConciliacionInternaSesion> ObtenerTodasSesiones()
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            return cn.Query<ConciliacionInternaSesion>(
                "SELECT * FROM bancos.ConciliacionInternaSesiones ORDER BY FechaCreacion DESC").ToList();
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Un solo rango de fechas, sin perfil
        public static ConciliacionInternaSesion CrearSesion(
            string nombre, DateTime desde, DateTime hasta, IEnumerable<string> conceptos)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();

            string conceptosJson = JsonSerializer.Serialize(conceptos.ToList());

            var idSesion = cn.ExecuteScalar<int>(@"
                INSERT INTO bancos.ConciliacionInternaSesiones
                    (Nombre, FechaCreacion, FechaDesde, FechaHasta, ConceptosJson, Estado)
                VALUES (@Nombre, @Fecha, @Desde, @Hasta, @Conceptos, 'EnProceso');
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new { Nombre = nombre, Fecha = DateTime.Now, Desde = desde.Date, Hasta = hasta.Date, Conceptos = conceptosJson });

            return cn.QuerySingle<ConciliacionInternaSesion>(
                "SELECT * FROM bancos.ConciliacionInternaSesiones WHERE Id = @Id", new { Id = idSesion });
        }

        public static void EliminarSesion(int idSesion)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            using var tx = cn.BeginTransaction();

            cn.Execute("DELETE FROM bancos.ConciliacionInternaPares WHERE IdSesion = @Id", new { Id = idSesion }, tx);
            cn.Execute("DELETE FROM bancos.ConciliacionInternaSesiones WHERE Id = @Id", new { Id = idSesion }, tx);

            tx.Commit();
        }

        private static void MarcarFinalizada(int idSesion, IDbConnection cn, IDbTransaction tx)
        {
            cn.Execute("UPDATE bancos.ConciliacionInternaSesiones SET Estado = 'Finalizada' WHERE Id = @Id",
                new { Id = idSesion }, tx);
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Cierre: cuenta del perfil opuesto, resuelta por par
        /// <summary>
        /// Finaliza la sesión. Ya no hay un perfil fijo por lado (cada par puede venir de dos
        /// bancos distintos, incluso variar de un par a otro dentro de la misma sesión), así que
        /// la cuenta contrapartida se resuelve por par: IdArchivoA/B (ver
        /// <see cref="ObtenerPares"/>) llevan al perfil de cada movimiento, y de ahí a su cuenta
        /// contable. Cada movimiento del lado A (débito) queda con la cuenta del perfil del lado
        /// B (crédito), y viceversa. Se pisa aunque el usuario la hubiera editado a mano — es una
        /// acción deliberada del cierre, no el autocompletado pasivo que usa el resto del sistema.
        ///
        /// Si algún perfil involucrado no tiene cuenta contable asignada, NO finaliza nada (ni la
        /// sesión ni ningún movimiento).
        /// </summary>
        public static ResultadoFinalizacionInterna Finalizar(int idSesion)
        {
            var resultado = new ResultadoFinalizacionInterna { Exito = true };

            var pares = ObtenerPares(idSesion);
            if (pares.Count == 0) return resultado;

            var archivos = ArchivoImportadoStorage.ObtenerTodos().ToDictionary(a => a.Id);
            var perfilesCache = new Dictionary<int, PerfilBanco>();
            PerfilBanco PerfilDelArchivo(int idArchivo) =>
                perfilesCache.TryGetValue(archivos[idArchivo].IdPerfilBanco, out var p)
                    ? p
                    : perfilesCache[archivos[idArchivo].IdPerfilBanco] = PerfilBancoStorage.ObtenerPorId(archivos[idArchivo].IdPerfilBanco);

            var cuentaPorPar = new List<(int IdMovimientoA, string CuentaParaA, int IdMovimientoB, string CuentaParaB)>();

            foreach (var par in pares)
            {
                var perfilA = PerfilDelArchivo(par.IdArchivoA);
                var perfilB = PerfilDelArchivo(par.IdArchivoB);

                if (perfilA.IdCuentaContable == null || perfilB.IdCuentaContable == null)
                {
                    resultado.Exito = false;
                    resultado.ParesSinCuenta.Add(
                        $"{perfilA.NombreBanco} ↔ {perfilB.NombreBanco}: falta cuenta contable en el perfil.");
                    continue;
                }

                string cuentaParaA = CuentaContableStorage.ObtenerPorId(perfilB.IdCuentaContable.Value).Cuenta;
                string cuentaParaB = CuentaContableStorage.ObtenerPorId(perfilA.IdCuentaContable.Value).Cuenta;
                cuentaPorPar.Add((par.IdMovimientoA, cuentaParaA, par.IdMovimientoB, cuentaParaB));
            }

            if (!resultado.Exito) return resultado;

            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            using var tx = cn.BeginTransaction();

            foreach (var (idA, cuentaA, idB, cuentaB) in cuentaPorPar)
            {
                cn.Execute("UPDATE bancos.MovimientosArchivo SET CuentaFinal = @Cuenta WHERE Id = @Id",
                    new { Cuenta = cuentaA, Id = idA }, tx);
                cn.Execute("UPDATE bancos.MovimientosArchivo SET CuentaFinal = @Cuenta WHERE Id = @Id",
                    new { Cuenta = cuentaB, Id = idB }, tx);
            }

            MarcarFinalizada(idSesion, cn, tx);

            tx.Commit();
            return resultado;
        }

        // ── Conceptos disponibles ────────────────────────────────────────────────

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Conceptos disponibles en el rango, sin filtrar por perfil
        /// <summary>Conceptos finales distintos, de cualquier extracto, con movimientos en el rango dado.</summary>
        public static List<string> ObtenerConceptosDisponibles(DateTime desde, DateTime hasta) =>
            MovimientoStorage.ObtenerTodos()
                .Where(m => ComparadorConciliacionInterna.EstaEnRango(m.Fecha, desde, hasta)
                         && !string.IsNullOrWhiteSpace(m.ConceptoFinal))
                .Select(m => m.ConceptoFinal)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c)
                .ToList();

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Identificar el extracto de cada movimiento
        // Ahora que los pools de Debitos/Creditos son globales (cualquier extracto), la fecha e
        // importe solos no alcanzan para distinguir dos candidatos — hace falta saber de qué
        // banco/archivo viene cada uno. Se usa en la grilla y en el desempate manual.
        /// <summary>Por cada archivo importado, "NombreBanco — NombreArchivo" para mostrar en pantalla.</summary>
        public static Dictionary<int, string> ObtenerNombresExtracto()
        {
            var perfiles = PerfilBancoStorage.ObtenerTodos().ToDictionary(p => p.Id);
            return ArchivoImportadoStorage.ObtenerTodos().ToDictionary(
                a => a.Id,
                a => perfiles.TryGetValue(a.IdPerfilBanco, out var p)
                    ? $"{p.NombreBanco} — {a.NombreArchivo}"
                    : a.NombreArchivo);
        }

        // ── Movimientos pendientes de cada lado ──────────────────────────────────

        public static List<MovimientoProcesado> ObtenerPendientesDebitos(int idSesion) =>
            CargarSinConciliar(idSesion).PendientesDebitos;

        public static List<MovimientoProcesado> ObtenerPendientesCreditos(int idSesion) =>
            CargarSinConciliar(idSesion).PendientesCreditos;

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Pools globales de debitos/creditos, no por perfil
        /// <summary>
        /// Arma los dos lados de la sesión: todos los movimientos débito (cualquier extracto) y
        /// todos los movimientos crédito (cualquier extracto) que caen en el rango y concepto de
        /// la sesión y todavía no están conciliados en ella. El lado A de un par es siempre el
        /// débito, el B siempre el crédito (ver <see cref="ConciliarPar"/>).
        /// </summary>
        private static (ConciliacionInternaSesion Sesion, List<MovimientoProcesado> PendientesDebitos, List<MovimientoProcesado> PendientesCreditos)
            CargarSinConciliar(int idSesion)
        {
            ConciliacionInternaSesion sesion;
            HashSet<int> conciliadosDebitos, conciliadosCreditos;

            using (var cn = DatabaseHelper.Open())
            {
                sesion = cn.QuerySingle<ConciliacionInternaSesion>(
                    "SELECT * FROM bancos.ConciliacionInternaSesiones WHERE Id = @Id", new { Id = idSesion });

                conciliadosDebitos = cn.Query<int>(
                    "SELECT IdMovimientoA FROM bancos.ConciliacionInternaPares WHERE IdSesion = @Id",
                    new { Id = idSesion }).ToHashSet();
                conciliadosCreditos = cn.Query<int>(
                    "SELECT IdMovimientoB FROM bancos.ConciliacionInternaPares WHERE IdSesion = @Id",
                    new { Id = idSesion }).ToHashSet();
            }

            var conceptos = JsonSerializer.Deserialize<List<string>>(sesion.ConceptosJson) ?? new List<string>();

            // MovimientosArchivo.Fecha es NVARCHAR (texto tal como lo exporta cada banco), asi que
            // el filtro por rango se hace en memoria, igual que el resaltado de la conciliacion
            // externa (ComparadorConciliacion.ParsearFecha), no en SQL.
            var todos = MovimientoStorage.ObtenerTodos()
                .Where(m => conceptos.Contains(m.ConceptoFinal, StringComparer.OrdinalIgnoreCase)
                         && ComparadorConciliacionInterna.EstaEnRango(m.Fecha, sesion.FechaDesde, sesion.FechaHasta))
                .ToList();

            var pendientesDebitos = todos.Where(m => m.Debitos != 0 && !conciliadosDebitos.Contains(m.Id)).ToList();
            var pendientesCreditos = todos.Where(m => m.Debitos == 0 && !conciliadosCreditos.Contains(m.Id)).ToList();

            return (sesion, pendientesDebitos, pendientesCreditos);
        }

        // ── Pares conciliados ────────────────────────────────────────────────────

        public static List<ConciliacionInternaPar> ObtenerPares(int idSesion)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            return cn.Query<ConciliacionInternaPar>(@"
                SELECT p.*,
                      TRY_CONVERT( NVARCHAR(30), TRY_CONVERT(DATE, a.fecha, 103) )  AS FechaA, CASE WHEN a.Debitos <> 0 THEN a.Debitos ELSE a.Creditos END AS ImporteA, a.ConceptoFinal AS ConceptoFinalA, a.IdArchivo AS IdArchivoA, pa.NombreBanco as NombreBancoA,
                      TRY_CONVERT( NVARCHAR(30), TRY_CONVERT(DATE, b.fecha, 103) )  AS FechaB, CASE WHEN b.Debitos <> 0 THEN b.Debitos ELSE b.Creditos END AS ImporteB, b.ConceptoFinal AS ConceptoFinalB, b.IdArchivo AS IdArchivoB, pb.NombreBanco as NombreBancoB
                FROM bancos.ConciliacionInternaPares p
                JOIN bancos.MovimientosArchivo a ON p.IdMovimientoA = a.Id
                JOIN bancos.MovimientosArchivo b ON p.IdMovimientoB = b.Id
                JOIN bancos.ArchivosImportados ia on a.IdArchivo = ia.Id
                JOIN bancos.ArchivosImportados ib on b.IdArchivo = ib.Id
                join bancos.PerfilesBanco pa on pa.Id = ia.IdPerfilBanco
                join bancos.PerfilesBanco pb on pb.Id = ib.IdPerfilBanco
                WHERE p.IdSesion = @Id
                ORDER BY p.FechaConciliacion",
                new { Id = idSesion }).ToList();
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Bloquear un movimiento ya conciliado en OTRA sesion EnProceso
        // Fecha: 11/09/2026 - TAREA: 00021 - Linea: 5 - Backstop: indice unico para DENTRO de esta misma sesion
        /// <summary>
        /// Antes de insertar el par, chequea que ninguno de los dos movimientos esté ya conciliado
        /// en otra sesión todavía "EnProceso". Sin este chequeo, el mismo movimiento puede quedar
        /// enrolado en dos sesiones independientes (rangos/conceptos superpuestos) y, al finalizar
        /// ambas, la segunda pisa en silencio la CuentaFinal que dejó la primera.
        ///
        /// Ese chequeo excluye a propósito la sesión actual (no tendría sentido bloquearse a sí
        /// misma), así que por sí solo NO evita que el mismo movimiento quede conciliado dos veces
        /// DENTRO de esta sesión (ej.: dos desempates manuales de un mismo Auto-conciliar con
        /// candidatos superpuestos, resueltos con una lista de candidatos ya vieja). Esa garantía
        /// la da <c>UX_ConciliacionInternaPares_MovimientoA/B</c> en <see cref="SqlSchema"/>: si el
        /// INSERT la viola, se traduce a un mensaje igual de amigable en vez de reventar con una
        /// excepción sin manejar.
        ///
        /// Devuelve null si conciliό sin problema, o un mensaje ya listo para mostrarle al usuario
        /// explicando por qué no se pudo (distinto según el caso, ver el cuerpo del método).
        /// </summary>
        public static string ConciliarPar(int idSesion, int idMovimientoA, int idMovimientoB, TipoMatch tipoMatch)
        {
            string sesionConflicto = ObtenerSesionEnProcesoDelMovimiento(idMovimientoA, idSesion)
                                   ?? ObtenerSesionEnProcesoDelMovimiento(idMovimientoB, idSesion);
            if (sesionConflicto != null)
                return $"Uno de los dos movimientos ya está conciliado en la sesión \"{sesionConflicto}\", " +
                       "todavía en proceso. Cerrala o desconcilialo ahí antes de continuar.";

            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            try
            {
                cn.Execute(@"
                    INSERT INTO bancos.ConciliacionInternaPares (IdSesion, IdMovimientoA, IdMovimientoB, TipoMatch, FechaConciliacion)
                    VALUES (@IdSesion, @IdMovimientoA, @IdMovimientoB, @TipoMatch, @Fecha)",
                    new { IdSesion = idSesion, IdMovimientoA = idMovimientoA, IdMovimientoB = idMovimientoB,
                          TipoMatch = tipoMatch.ToString(), Fecha = DateTime.Now });
            }
            catch (SqlException ex) when (ex.Number is 2601 or 2627)
            {
                return "Uno de los dos movimientos ya quedó conciliado con otro par en esta misma sesión " +
                       "(la lista que se estaba mostrando había quedado desactualizada). Actualizá la pantalla e intentá de nuevo.";
            }

            return null;
        }

        public static void DesconciliarPar(int idPar)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            cn.Execute("DELETE FROM bancos.ConciliacionInternaPares WHERE Id = @Id", new { Id = idPar });
        }

        // ── Auto-conciliación ────────────────────────────────────────────────────

        /// <summary>
        /// Dos pasadas, igual que la conciliación externa: 1) fecha + importe opuesto exacto,
        /// 2) sólo importe opuesto entre lo que quedó sin conciliar. En las dos pasadas se excluye
        /// emparejar dos movimientos del mismo extracto (mismo IdArchivo): un mismo extracto no
        /// se concilia contra sí mismo. Los ítems con múltiples candidatos quedan para resolución
        /// manual.
        /// </summary>
        public static (int conciliados, List<(MovimientoProcesado A, List<MovimientoProcesado> Candidatos)> duplicados)
            AutoConciliar(int idSesion)
        {
            var (_, pendienteDebitos, pendienteCreditos) = CargarSinConciliar(idSesion);
            var conciliadosCreditos = new HashSet<int>();
            int total = 0;
            var duplicados = new List<(MovimientoProcesado, List<MovimientoProcesado>)>();

            // ── Pasada 1: Fecha + Importe opuesto ────────────────────────────────
            foreach (var a in pendienteDebitos.ToList())
            {
                var candidatos = pendienteCreditos
                    .Where(b => !conciliadosCreditos.Contains(b.Id)
                             && b.IdArchivo != a.IdArchivo
                             && ComparadorConciliacion.FechasIguales(a.Fecha, b.Fecha)
                             && ComparadorConciliacionInterna.ImportesOpuestos(a, b))
                    .ToList();

                if (candidatos.Count == 1)
                {
                    if (ConciliarPar(idSesion, a.Id, candidatos[0].Id, TipoMatch.FechaImporte) == null)
                    {
                        conciliadosCreditos.Add(candidatos[0].Id);
                        pendienteDebitos.Remove(a);
                        total++;
                    }
                }
                else if (candidatos.Count > 1)
                {
                    duplicados.Add((a, candidatos));
                    pendienteDebitos.Remove(a);
                }
            }

            // ── Pasada 2: Sólo importe opuesto ────────────────────────────────────
            var (_, _, pendienteCreditos2Base) = CargarSinConciliar(idSesion);
            var pendienteCreditos2 = pendienteCreditos2Base.Where(b => !conciliadosCreditos.Contains(b.Id)).ToList();

            foreach (var a in pendienteDebitos.ToList())
            {
                var candidatos = pendienteCreditos2
                    .Where(b => !conciliadosCreditos.Contains(b.Id)
                             && b.IdArchivo != a.IdArchivo
                             && ComparadorConciliacionInterna.ImportesOpuestos(a, b))
                    .ToList();

                if (candidatos.Count == 1)
                {
                    if (ConciliarPar(idSesion, a.Id, candidatos[0].Id, TipoMatch.SoloImporte) == null)
                    {
                        conciliadosCreditos.Add(candidatos[0].Id);
                        total++;
                    }
                }
                else if (candidatos.Count > 1)
                {
                    duplicados.Add((a, candidatos));
                }
            }

            return (total, duplicados);
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Bloquear edicion manual mientras este conciliado
        /// <summary>
        /// Nombre de la sesión de conciliación interna, todavía "EnProceso", que tiene a este
        /// movimiento conciliado — o null si no hay ninguna. Lo usa la grilla del Procesador
        /// para impedir editar CuentaFinal a mano: se va a pisar sola cuando esa sesión cierre.
        ///
        /// También la reutiliza <see cref="ConciliarPar"/>, una vez por cada lado del par, pasando
        /// la sesión actual en <paramref name="idSesionExcluir"/>: lo que importa ahí es si el
        /// movimiento ya está enrolado en OTRA sesión EnProceso, no en la propia.
        /// </summary>
        public static string ObtenerSesionEnProcesoDelMovimiento(int idMovimiento, int? idSesionExcluir = null)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            return cn.QueryFirstOrDefault<string>(@"
                SELECT TOP 1 s.Nombre
                FROM bancos.ConciliacionInternaPares p
                JOIN bancos.ConciliacionInternaSesiones s ON p.IdSesion = s.Id
                WHERE (p.IdMovimientoA = @Id OR p.IdMovimientoB = @Id)
                  AND s.Estado = 'EnProceso'
                  AND (@IdSesionExcluir IS NULL OR s.Id <> @IdSesionExcluir)",
                new { Id = idMovimiento, IdSesionExcluir = idSesionExcluir });
        }
    }
}
