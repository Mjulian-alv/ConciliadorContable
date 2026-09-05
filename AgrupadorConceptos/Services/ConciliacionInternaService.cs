// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - CRUD y auto-conciliacion entre dos extractos propios
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;
using Dapper;

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

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Se elige perfil + rango de fechas, no archivos
        public static ConciliacionInternaSesion CrearSesion(
            string nombre, int idPerfilA, DateTime desdeA, DateTime hastaA,
            int idPerfilB, DateTime desdeB, DateTime hastaB, IEnumerable<string> conceptos)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();

            string conceptosJson = JsonSerializer.Serialize(conceptos.ToList());

            var idSesion = cn.ExecuteScalar<int>(@"
                INSERT INTO bancos.ConciliacionInternaSesiones
                    (Nombre, FechaCreacion, IdPerfilA, FechaDesdeA, FechaHastaA, IdPerfilB, FechaDesdeB, FechaHastaB, ConceptosJson, Estado)
                VALUES (@Nombre, @Fecha, @IdPerfilA, @DesdeA, @HastaA, @IdPerfilB, @DesdeB, @HastaB, @Conceptos, 'EnProceso');
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new { Nombre = nombre, Fecha = DateTime.Now, IdPerfilA = idPerfilA, DesdeA = desdeA.Date, HastaA = hastaA.Date,
                      IdPerfilB = idPerfilB, DesdeB = desdeB.Date, HastaB = hastaB.Date, Conceptos = conceptosJson });

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

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Ya no se llama directo, ver Finalizar
        private static void MarcarFinalizada(int idSesion, IDbConnection cn, IDbTransaction tx)
        {
            cn.Execute("UPDATE bancos.ConciliacionInternaSesiones SET Estado = 'Finalizada' WHERE Id = @Id",
                new { Id = idSesion }, tx);
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Cierre: cuenta del perfil opuesto como contrapartida
        /// <summary>
        /// Finaliza la sesión. Todos los pares de una sesión comparten los mismos dos perfiles
        /// (uno por lado, ver <see cref="CrearSesion"/>), así que la cuenta contrapartida sale
        /// directo de la sesión: cada movimiento del lado A queda con la cuenta del perfil B, y
        /// cada uno del lado B con la del perfil A. Se pisa aunque el usuario la hubiera editado
        /// a mano — es una acción deliberada del cierre, no el autocompletado pasivo que usa el
        /// resto del sistema.
        ///
        /// Si alguno de los dos perfiles no tiene cuenta contable asignada, NO finaliza nada
        /// (ni la sesión ni ningún movimiento).
        /// </summary>
        public static ResultadoFinalizacionInterna Finalizar(int idSesion)
        {
            var resultado = new ResultadoFinalizacionInterna { Exito = true };

            ConciliacionInternaSesion sesion;
            using (var cn0 = DatabaseHelper.Open())
                sesion = cn0.QuerySingle<ConciliacionInternaSesion>(
                    "SELECT * FROM bancos.ConciliacionInternaSesiones WHERE Id = @Id", new { Id = idSesion });

            var perfilA = PerfilBancoStorage.ObtenerPorId(sesion.IdPerfilA);
            var perfilB = PerfilBancoStorage.ObtenerPorId(sesion.IdPerfilB);

            if (perfilA.IdCuentaContable == null || perfilB.IdCuentaContable == null)
            {
                resultado.Exito = false;
                resultado.ParesSinCuenta.Add(
                    $"{perfilA.NombreBanco} ↔ {perfilB.NombreBanco}: falta cuenta contable en el perfil.");
                return resultado;
            }

            string cuentaParaA = CuentaContableStorage.ObtenerPorId(perfilB.IdCuentaContable.Value).Cuenta;
            string cuentaParaB = CuentaContableStorage.ObtenerPorId(perfilA.IdCuentaContable.Value).Cuenta;

            var pares = ObtenerPares(idSesion);

            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            using var tx = cn.BeginTransaction();

            foreach (var par in pares)
            {
                cn.Execute("UPDATE bancos.MovimientosArchivo SET CuentaFinal = @Cuenta WHERE Id = @Id",
                    new { Cuenta = cuentaParaA, Id = par.IdMovimientoA }, tx);
                cn.Execute("UPDATE bancos.MovimientosArchivo SET CuentaFinal = @Cuenta WHERE Id = @Id",
                    new { Cuenta = cuentaParaB, Id = par.IdMovimientoB }, tx);
            }

            MarcarFinalizada(idSesion, cn, tx);

            tx.Commit();
            return resultado;
        }

        // ── Movimientos pendientes de cada lado ──────────────────────────────────

        public static List<MovimientoProcesado> ObtenerPendientesA(int idSesion) =>
            CargarSinConciliar(idSesion).PendientesA;

        public static List<MovimientoProcesado> ObtenerPendientesB(int idSesion) =>
            CargarSinConciliar(idSesion).PendientesB;

        private static (ConciliacionInternaSesion Sesion, List<MovimientoProcesado> PendientesA, List<MovimientoProcesado> PendientesB)
            CargarSinConciliar(int idSesion)
        {
            ConciliacionInternaSesion sesion;
            HashSet<int> conciliadosA, conciliadosB;

            using (var cn = DatabaseHelper.Open())
            {
                sesion = cn.QuerySingle<ConciliacionInternaSesion>(
                    "SELECT * FROM bancos.ConciliacionInternaSesiones WHERE Id = @Id", new { Id = idSesion });

                conciliadosA = cn.Query<int>(
                    "SELECT IdMovimientoA FROM bancos.ConciliacionInternaPares WHERE IdSesion = @Id",
                    new { Id = idSesion }).ToHashSet();
                conciliadosB = cn.Query<int>(
                    "SELECT IdMovimientoB FROM bancos.ConciliacionInternaPares WHERE IdSesion = @Id",
                    new { Id = idSesion }).ToHashSet();
            }

            var conceptos = JsonSerializer.Deserialize<List<string>>(sesion.ConceptosJson) ?? new List<string>();

            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Movimientos del perfil filtrados por rango
            // MovimientosArchivo.Fecha es NVARCHAR (texto tal como lo exporta cada banco), asi que
            // el filtro por rango se hace en memoria con el mismo parseo que ya usa el resaltado
            // de la conciliacion externa (ComparadorConciliacion.ParsearFecha), no en SQL.
            var pendientesA = MovimientoStorage.ObtenerPorPerfil(sesion.IdPerfilA)
                .Where(m => !conciliadosA.Contains(m.Id)
                         && conceptos.Contains(m.ConceptoFinal, StringComparer.OrdinalIgnoreCase)
                         && ComparadorConciliacionInterna.EstaEnRango(m.Fecha, sesion.FechaDesdeA, sesion.FechaHastaA))
                .ToList();

            var pendientesB = MovimientoStorage.ObtenerPorPerfil(sesion.IdPerfilB)
                .Where(m => !conciliadosB.Contains(m.Id)
                         && conceptos.Contains(m.ConceptoFinal, StringComparer.OrdinalIgnoreCase)
                         && ComparadorConciliacionInterna.EstaEnRango(m.Fecha, sesion.FechaDesdeB, sesion.FechaHastaB))
                .ToList();

            return (sesion, pendientesA, pendientesB);
        }

        // ── Pares conciliados ────────────────────────────────────────────────────

        public static List<ConciliacionInternaPar> ObtenerPares(int idSesion)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            return cn.Query<ConciliacionInternaPar>(@"
                SELECT p.*,
                       a.Fecha AS FechaA, CASE WHEN a.Debitos <> 0 THEN a.Debitos ELSE a.Creditos END AS ImporteA, a.ConceptoFinal AS ConceptoFinalA,
                       b.Fecha AS FechaB, CASE WHEN b.Debitos <> 0 THEN b.Debitos ELSE b.Creditos END AS ImporteB, b.ConceptoFinal AS ConceptoFinalB
                FROM bancos.ConciliacionInternaPares p
                JOIN bancos.MovimientosArchivo a ON p.IdMovimientoA = a.Id
                JOIN bancos.MovimientosArchivo b ON p.IdMovimientoB = b.Id
                WHERE p.IdSesion = @Id
                ORDER BY p.FechaConciliacion",
                new { Id = idSesion }).ToList();
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Bloquear un movimiento ya conciliado en OTRA sesion EnProceso
        /// <summary>
        /// Antes de insertar el par, chequea que ninguno de los dos movimientos esté ya conciliado
        /// en otra sesión todavía "EnProceso". Sin este chequeo, el mismo movimiento puede quedar
        /// enrolado en dos sesiones independientes (perfil+rango+concepto superpuestos) y, al
        /// finalizar ambas, la segunda pisa en silencio la CuentaFinal que dejó la primera.
        /// Devuelve null si conciliό sin problema, o el nombre de la sesión que lo está bloqueando.
        /// </summary>
        public static string ConciliarPar(int idSesion, int idMovimientoA, int idMovimientoB, TipoMatch tipoMatch)
        {
            string sesionConflicto = ObtenerSesionEnProcesoDelMovimiento(idMovimientoA, idSesion)
                                   ?? ObtenerSesionEnProcesoDelMovimiento(idMovimientoB, idSesion);
            if (sesionConflicto != null) return sesionConflicto;

            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            cn.Execute(@"
                INSERT INTO bancos.ConciliacionInternaPares (IdSesion, IdMovimientoA, IdMovimientoB, TipoMatch, FechaConciliacion)
                VALUES (@IdSesion, @IdMovimientoA, @IdMovimientoB, @TipoMatch, @Fecha)",
                new { IdSesion = idSesion, IdMovimientoA = idMovimientoA, IdMovimientoB = idMovimientoB,
                      TipoMatch = tipoMatch.ToString(), Fecha = DateTime.Now });

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
        /// 2) sólo importe opuesto entre lo que quedó sin conciliar. Los ítems con múltiples
        /// candidatos quedan para resolución manual.
        /// </summary>
        public static (int conciliados, List<(MovimientoProcesado A, List<MovimientoProcesado> Candidatos)> duplicados)
            AutoConciliar(int idSesion)
        {
            var (_, pendienteA, pendienteB) = CargarSinConciliar(idSesion);
            var conciliadosB = new HashSet<int>();
            int total = 0;
            var duplicados = new List<(MovimientoProcesado, List<MovimientoProcesado>)>();

            // ── Pasada 1: Fecha + Importe opuesto ────────────────────────────────
            foreach (var a in pendienteA.ToList())
            {
                var candidatos = pendienteB
                    .Where(b => !conciliadosB.Contains(b.Id)
                             && ComparadorConciliacion.FechasIguales(a.Fecha, b.Fecha)
                             && ComparadorConciliacionInterna.ImportesOpuestos(a, b))
                    .ToList();

                // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Si el candidato ya esta tomado por otra sesion EnProceso, se deja pendiente
                if (candidatos.Count == 1)
                {
                    if (ConciliarPar(idSesion, a.Id, candidatos[0].Id, TipoMatch.FechaImporte) == null)
                    {
                        conciliadosB.Add(candidatos[0].Id);
                        pendienteA.Remove(a);
                        total++;
                    }
                }
                else if (candidatos.Count > 1)
                {
                    duplicados.Add((a, candidatos));
                    pendienteA.Remove(a);
                }
            }

            // ── Pasada 2: Sólo importe opuesto ────────────────────────────────────
            var (_, _, pendienteB2Base) = CargarSinConciliar(idSesion);
            var pendienteB2 = pendienteB2Base.Where(b => !conciliadosB.Contains(b.Id)).ToList();

            foreach (var a in pendienteA.ToList())
            {
                var candidatos = pendienteB2
                    .Where(b => !conciliadosB.Contains(b.Id) && ComparadorConciliacionInterna.ImportesOpuestos(a, b))
                    .ToList();

                // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Idem pasada 1: no forzar un par bloqueado por otra sesion
                if (candidatos.Count == 1)
                {
                    if (ConciliarPar(idSesion, a.Id, candidatos[0].Id, TipoMatch.SoloImporte) == null)
                    {
                        conciliadosB.Add(candidatos[0].Id);
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
        /// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - idSesionExcluir para el chequeo multi-sesion de ConciliarPar
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
