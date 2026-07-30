using System;
using System.Collections.Generic;
using System.Linq;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;
using Dapper;

namespace AgrupadorConceptos.Services
{
    public static class ConciliacionExternService
    {
        // ── Sesiones ─────────────────────────────────────────────────────────────

        public static List<ConciliacionSesion> ObtenerSesiones(int idArchivoImportado)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            return cn.Query<ConciliacionSesion>(
                @"SELECT * FROM bancos.ConciliacionSesiones
                  WHERE IdArchivoImportado = @Id
                     OR (ArchivosJson IS NOT NULL AND ArchivosJson LIKE @Like)
                  ORDER BY FechaCreacion DESC",
                new { Id = idArchivoImportado, Like = $"%{idArchivoImportado}%" }).ToList();
        }

        public static List<ConciliacionSesion> ObtenerTodasSesiones()
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            return cn.Query<ConciliacionSesion>(
                "SELECT * FROM bancos.ConciliacionSesiones ORDER BY FechaCreacion DESC").ToList();
        }

        public static ConciliacionSesion CrearSesion(string nombre, List<int> idsArchivos,
            IEnumerable<string> conceptos, IEnumerable<ConciliacionItemExterno> itemsExternos)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            using var tx = cn.BeginTransaction();

            string conceptosJson = System.Text.Json.JsonSerializer.Serialize(conceptos.ToList());
            string archivosJson  = System.Text.Json.JsonSerializer.Serialize(idsArchivos);
            int primerArchivo    = idsArchivos.First();

            var idSesion = cn.ExecuteScalar<int>(@"
                INSERT INTO bancos.ConciliacionSesiones (Nombre, FechaCreacion, IdArchivoImportado, ArchivosJson, ConceptosJson, Estado)
                VALUES (@Nombre, @Fecha, @IdArchivo, @ArchivosJson, @Conceptos, 'EnProceso');
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new { Nombre = nombre, Fecha = DateTime.Now, IdArchivo = primerArchivo,
                      ArchivosJson = archivosJson, Conceptos = conceptosJson },
                tx);

            foreach (var item in itemsExternos)
            {
                cn.Execute(@"
                    INSERT INTO bancos.ConciliacionItemsExternos (IdSesion, Fecha, Importe, Detalle, Conciliado)
                    VALUES (@IdSesion, @Fecha, @Importe, @Detalle, 0)",
                    new { IdSesion = idSesion, item.Fecha, item.Importe, item.Detalle }, tx);
            }

            tx.Commit();

            return cn.QuerySingle<ConciliacionSesion>(
                "SELECT * FROM bancos.ConciliacionSesiones WHERE Id = @Id", new { Id = idSesion });
        }

        public static void EliminarSesion(int idSesion)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            using var tx = cn.BeginTransaction();

            // Borrado explícito y ordenado: las FK de ConciliacionItemsExternos/ConciliacionPares
            // hacia ConciliacionSesiones no tienen ON DELETE CASCADE en SQL Server (caminos múltiples).
            cn.Execute("DELETE FROM bancos.ConciliacionPares WHERE IdSesion = @Id", new { Id = idSesion }, tx);
            cn.Execute("DELETE FROM bancos.ConciliacionItemsExternos WHERE IdSesion = @Id", new { Id = idSesion }, tx);
            cn.Execute("DELETE FROM bancos.ConciliacionSesiones WHERE Id = @Id", new { Id = idSesion }, tx);

            tx.Commit();
        }

        public static void MarcarFinalizada(int idSesion)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            cn.Execute("UPDATE bancos.ConciliacionSesiones SET Estado = 'Finalizada' WHERE Id = @Id",
                new { Id = idSesion });
        }

        // ── Ítems externos ────────────────────────────────────────────────────────

        public static List<ConciliacionItemExterno> ObtenerItemsPendientes(int idSesion)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            return cn.Query<ConciliacionItemExterno>(
                "SELECT * FROM bancos.ConciliacionItemsExternos WHERE IdSesion = @Id AND Conciliado = 0 ORDER BY Fecha, Importe",
                new { Id = idSesion }).ToList();
        }

        public static List<ConciliacionItemExterno> ObtenerTodosItemsExternos(int idSesion)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            return cn.Query<ConciliacionItemExterno>(
                "SELECT * FROM bancos.ConciliacionItemsExternos WHERE IdSesion = @Id ORDER BY Fecha, Importe",
                new { Id = idSesion }).ToList();
        }

        // ── Movimientos del extracto ──────────────────────────────────────────────

        /// <summary>
        /// Movimientos de los archivos de la sesión que todavía no se conciliaron,
        /// restringidos a los conceptos que la sesión eligió conciliar.
        /// </summary>
        public static List<MovimientoProcesado> ObtenerMovimientosPendientes(int idSesion)
        {
            var (sesion, movimientosSinConciliar) = CargarMovimientosSinConciliar(idSesion);

            var conceptos = System.Text.Json.JsonSerializer.Deserialize<List<string>>(sesion.ConceptosJson)
                            ?? new List<string>();

            return movimientosSinConciliar
                .Where(m => conceptos.Contains(m.ConceptoFinal, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Igual que <see cref="ObtenerMovimientosPendientes"/> pero sin filtrar por
        /// concepto: se usa en la conciliación manual, donde el usuario puede emparejar
        /// contra cualquier movimiento del extracto.
        /// </summary>
        public static List<MovimientoProcesado> ObtenerMovimientosSinConcepto(int idSesion)
        {
            var (_, movimientosSinConciliar) = CargarMovimientosSinConciliar(idSesion);
            return movimientosSinConciliar;
        }

        private static (ConciliacionSesion Sesion, List<MovimientoProcesado> SinConciliar)
            CargarMovimientosSinConciliar(int idSesion)
        {
            ConciliacionSesion sesion;
            HashSet<int> idsConciliados;

            using (var cn = DatabaseHelper.Open())
            {
                sesion = cn.QuerySingle<ConciliacionSesion>(
                    "SELECT * FROM bancos.ConciliacionSesiones WHERE Id = @Id", new { Id = idSesion });

                idsConciliados = cn.Query<int>(
                    "SELECT IdMovimientoProcesado FROM bancos.ConciliacionPares WHERE IdSesion = @Id",
                    new { Id = idSesion }).ToHashSet();
            }

            var sinConciliar = MovimientoStorage.ObtenerPorArchivos(sesion.IdsArchivos)
                .Where(m => !idsConciliados.Contains(m.Id))
                .ToList();

            return (sesion, sinConciliar);
        }

        // ── Pares conciliados ────────────────────────────────────────────────────

        public static List<ConciliacionPar> ObtenerPares(int idSesion)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            return cn.Query<ConciliacionPar>(@"
                SELECT p.*, 
                       e.Fecha  AS FechaExterno,  e.Importe AS ImporteExterno, e.Detalle AS DetalleExterno,
                       m.Fecha  AS FechaExtracto, 
                       CASE WHEN m.Debitos <> 0 THEN m.Debitos ELSE m.Creditos END AS ImporteExtracto,
                       m.ConceptoFinal AS ConceptoFinalExtracto
                FROM bancos.ConciliacionPares p
                JOIN bancos.ConciliacionItemsExternos e ON p.IdItemExterno         = e.Id
                JOIN bancos.MovimientosArchivo        m ON p.IdMovimientoProcesado = m.Id
                WHERE p.IdSesion = @Id
                ORDER BY p.FechaConciliacion",
                new { Id = idSesion }).ToList();
        }

        public static void ConciliarPar(int idSesion, int idItemExterno, int idMovimiento, TipoMatch tipoMatch)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            using var tx = cn.BeginTransaction();

            cn.Execute(@"
                INSERT INTO bancos.ConciliacionPares (IdSesion, IdItemExterno, IdMovimientoProcesado, TipoMatch, FechaConciliacion)
                VALUES (@IdSesion, @IdItemExterno, @IdMovimiento, @TipoMatch, @Fecha)",
                new { IdSesion = idSesion, IdItemExterno = idItemExterno,
                      IdMovimiento = idMovimiento, TipoMatch = tipoMatch.ToString(),
                      Fecha = DateTime.Now }, tx);

            cn.Execute("UPDATE bancos.ConciliacionItemsExternos SET Conciliado = 1 WHERE Id = @Id",
                new { Id = idItemExterno }, tx);

            tx.Commit();
        }

        public static void DesconciliarPar(int idPar)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            using var tx = cn.BeginTransaction();

            var par = cn.QuerySingleOrDefault<ConciliacionPar>(
                "SELECT * FROM bancos.ConciliacionPares WHERE Id = @Id", new { Id = idPar });
            if (par == null) return;

            cn.Execute("DELETE FROM bancos.ConciliacionPares WHERE Id = @Id", new { Id = idPar }, tx);
            cn.Execute("UPDATE bancos.ConciliacionItemsExternos SET Conciliado = 0 WHERE Id = @Id",
                new { Id = par.IdItemExterno }, tx);

            tx.Commit();
        }

        // ── Auto-conciliación ────────────────────────────────────────────────────

        /// <summary>
        /// Ejecuta las dos pasadas automáticas. Devuelve la cantidad de pares generados.
        /// Los ítems con múltiples candidatos se dejan pendientes para resolución manual.
        /// </summary>
        public static (int conciliados, List<(ConciliacionItemExterno Externo, List<MovimientoProcesado> Candidatos)> duplicados)
            AutoConciliar(int idSesion)
        {
            var pendientesExternos  = ObtenerItemsPendientes(idSesion);
            var pendientesExtracto  = ObtenerMovimientosPendientes(idSesion);
            var conciliadosExtracto = new HashSet<int>();
            int totalConciliados    = 0;
            var duplicados          = new List<(ConciliacionItemExterno, List<MovimientoProcesado>)>();

            // ── Pasada 1: Fecha + Importe ────────────────────────────────────────
            foreach (var ext in pendientesExternos.ToList())
            {
                var candidatos = pendientesExtracto
                    .Where(m => !conciliadosExtracto.Contains(m.Id)
                             && ComparadorConciliacion.FechasIguales(ext.Fecha, m.Fecha)
                             && ComparadorConciliacion.ImportesIguales(ext.Importe, m))
                    .ToList();

                if (candidatos.Count == 1)
                {
                    ConciliarPar(idSesion, ext.Id, candidatos[0].Id, TipoMatch.FechaImporte);
                    conciliadosExtracto.Add(candidatos[0].Id);
                    pendientesExternos.Remove(ext);
                    totalConciliados++;
                }
                else if (candidatos.Count > 1)
                {
                    duplicados.Add((ext, candidatos));
                    pendientesExternos.Remove(ext);
                }
            }

            // ── Pasada 2: Solo Importe ───────────────────────────────────────────
            // Recargamos pendientes para reflejar lo conciliado en pasada 1
            var pendientes2 = ObtenerMovimientosPendientes(idSesion)
                              .Where(m => !conciliadosExtracto.Contains(m.Id))
                              .ToList();

            foreach (var ext in pendientesExternos.ToList())
            {
                var candidatos = pendientes2
                    .Where(m => !conciliadosExtracto.Contains(m.Id)
                             && ComparadorConciliacion.ImportesIguales(ext.Importe, m))
                    .ToList();

                if (candidatos.Count == 1)
                {
                    ConciliarPar(idSesion, ext.Id, candidatos[0].Id, TipoMatch.SoloImporte);
                    conciliadosExtracto.Add(candidatos[0].Id);
                    totalConciliados++;
                }
                else if (candidatos.Count > 1)
                {
                    duplicados.Add((ext, candidatos));
                }
            }

            return (totalConciliados, duplicados);
        }

        // Los criterios de igualdad (fecha/importe) viven en ComparadorConciliacion,
        // compartidos con el resaltado manual de ConciliacionExternForm.
    }
}
