using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
                @"SELECT * FROM ConciliacionSesiones
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
                "SELECT * FROM ConciliacionSesiones ORDER BY FechaCreacion DESC").ToList();
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
                INSERT INTO ConciliacionSesiones (Nombre, FechaCreacion, IdArchivoImportado, ArchivosJson, ConceptosJson, Estado)
                VALUES (@Nombre, @Fecha, @IdArchivo, @ArchivosJson, @Conceptos, 'EnProceso');
                SELECT last_insert_rowid();",
                new { Nombre = nombre, Fecha = DateTime.Now, IdArchivo = primerArchivo,
                      ArchivosJson = archivosJson, Conceptos = conceptosJson },
                tx);

            foreach (var item in itemsExternos)
            {
                cn.Execute(@"
                    INSERT INTO ConciliacionItemsExternos (IdSesion, Fecha, Importe, Detalle, Conciliado)
                    VALUES (@IdSesion, @Fecha, @Importe, @Detalle, 0)",
                    new { IdSesion = idSesion, item.Fecha, item.Importe, item.Detalle }, tx);
            }

            tx.Commit();

            return cn.QuerySingle<ConciliacionSesion>(
                "SELECT * FROM ConciliacionSesiones WHERE Id = @Id", new { Id = idSesion });
        }

        public static void EliminarSesion(int idSesion)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            cn.Execute("DELETE FROM ConciliacionSesiones WHERE Id = @Id", new { Id = idSesion });
        }

        public static void MarcarFinalizada(int idSesion)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            cn.Execute("UPDATE ConciliacionSesiones SET Estado = 'Finalizada' WHERE Id = @Id",
                new { Id = idSesion });
        }

        // ── Ítems externos ────────────────────────────────────────────────────────

        public static List<ConciliacionItemExterno> ObtenerItemsPendientes(int idSesion)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            return cn.Query<ConciliacionItemExterno>(
                "SELECT * FROM ConciliacionItemsExternos WHERE IdSesion = @Id AND Conciliado = 0 ORDER BY Fecha, Importe",
                new { Id = idSesion }).ToList();
        }

        public static List<ConciliacionItemExterno> ObtenerTodosItemsExternos(int idSesion)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            return cn.Query<ConciliacionItemExterno>(
                "SELECT * FROM ConciliacionItemsExternos WHERE IdSesion = @Id ORDER BY Fecha, Importe",
                new { Id = idSesion }).ToList();
        }

        // ── Movimientos del extracto ──────────────────────────────────────────────

        public static List<MovimientoProcesado> ObtenerMovimientosPendientes(int idSesion)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();

            var sesion = cn.QuerySingle<ConciliacionSesion>(
                "SELECT * FROM ConciliacionSesiones WHERE Id = @Id", new { Id = idSesion });

            var conceptos = System.Text.Json.JsonSerializer.Deserialize<List<string>>(sesion.ConceptosJson)
                            ?? new List<string>();

            var idsConciliados = cn.Query<int>(
                "SELECT IdMovimientoProcesado FROM ConciliacionPares WHERE IdSesion = @Id",
                new { Id = idSesion }).ToHashSet();

            var ids = string.Join(",", sesion.IdsArchivos);
            var todos = cn.Query<MovimientoProcesado>(
                $"SELECT * FROM MovimientosArchivo WHERE IdArchivo IN ({ids})").ToList();

            return todos
                .Where(m => conceptos.Contains(m.ConceptoFinal, StringComparer.OrdinalIgnoreCase)
                         && !idsConciliados.Contains(m.Id))
                .ToList();
        }

        public static List<MovimientoProcesado> ObtenerMovimientosSinConcepto(int idSesion)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();

            var sesion = cn.QuerySingle<ConciliacionSesion>(
                "SELECT * FROM ConciliacionSesiones WHERE Id = @Id", new { Id = idSesion });

            var idsConciliados = cn.Query<int>(
                "SELECT IdMovimientoProcesado FROM ConciliacionPares WHERE IdSesion = @Id",
                new { Id = idSesion }).ToHashSet();

            var ids = string.Join(",", sesion.IdsArchivos);
            return cn.Query<MovimientoProcesado>(
                $"SELECT * FROM MovimientosArchivo WHERE IdArchivo IN ({ids})")
                .Where(m => !idsConciliados.Contains(m.Id))
                .ToList();
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
                FROM ConciliacionPares p
                JOIN ConciliacionItemsExternos e ON p.IdItemExterno         = e.Id
                JOIN MovimientosArchivo        m ON p.IdMovimientoProcesado = m.Id
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
                INSERT INTO ConciliacionPares (IdSesion, IdItemExterno, IdMovimientoProcesado, TipoMatch, FechaConciliacion)
                VALUES (@IdSesion, @IdItemExterno, @IdMovimiento, @TipoMatch, @Fecha)",
                new { IdSesion = idSesion, IdItemExterno = idItemExterno,
                      IdMovimiento = idMovimiento, TipoMatch = tipoMatch.ToString(),
                      Fecha = DateTime.Now }, tx);

            cn.Execute("UPDATE ConciliacionItemsExternos SET Conciliado = 1 WHERE Id = @Id",
                new { Id = idItemExterno }, tx);

            tx.Commit();
        }

        public static void DesconciliarPar(int idPar)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            using var tx = cn.BeginTransaction();

            var par = cn.QuerySingleOrDefault<ConciliacionPar>(
                "SELECT * FROM ConciliacionPares WHERE Id = @Id", new { Id = idPar });
            if (par == null) return;

            cn.Execute("DELETE FROM ConciliacionPares WHERE Id = @Id", new { Id = idPar }, tx);
            cn.Execute("UPDATE ConciliacionItemsExternos SET Conciliado = 0 WHERE Id = @Id",
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
                             && FechasIguales(ext.Fecha, m.Fecha)
                             && ImportesIguales(ext.Importe, m))
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
                             && ImportesIguales(ext.Importe, m))
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

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static bool FechasIguales(string fechaExt, string fechaExtracto)
        {
            if (string.IsNullOrWhiteSpace(fechaExt) || string.IsNullOrWhiteSpace(fechaExtracto))
                return false;

            string[] formatos = { "dd/MM/yyyy", "yyyy-MM-dd", "d/M/yyyy", "MM/dd/yyyy", "dd-MM-yyyy" };
            if (DateTime.TryParseExact(fechaExt.Trim(),     formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d1) &&
                DateTime.TryParseExact(fechaExtracto.Trim(), formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d2))
                return d1.Date == d2.Date;

            return string.Equals(fechaExt.Trim(), fechaExtracto.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        public static decimal ImporteEfectivo(MovimientoProcesado m) =>
            m.Debitos != 0 ? Math.Abs(m.Debitos) : Math.Abs(m.Creditos);

        private static bool ImportesIguales(decimal importeExt, MovimientoProcesado m) =>
            Math.Abs(importeExt) == ImporteEfectivo(m);
    }
}
