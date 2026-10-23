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
            => ConciliacionExternStorage.CrearSesion(nombre, idsArchivos, conceptos, itemsExternos);

        public static void EliminarSesion(int idSesion)
            => ConciliacionExternStorage.EliminarSesion(idSesion);

        public static void MarcarFinalizada(int idSesion)
            => ConciliacionExternStorage.MarcarFinalizada(idSesion);

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
                // Fecha: 23/10/2026 - TAREA: 00125 - Linea: 3 - Los marcados como conciliados (a mano o por otra sesión finalizada) no son candidatos
                .Where(m => !idsConciliados.Contains(m.Id) && !m.Conciliado)
                .ToList();

            return (sesion, sinConciliar);
        }

        // ── Pares conciliados ────────────────────────────────────────────────────

        // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Operar sobre días tipados evita reinterpretaciones culturales.
        public static List<ConciliacionPar> ObtenerPares(int idSesion) => ConciliacionExternStorage.ObtenerPares(idSesion);

        public static void ConciliarPar(int idSesion, int idItemExterno, int idMovimiento, TipoMatch tipoMatch)
            => ConciliarSeleccion(idSesion, idItemExterno, new[] { idMovimiento }, tipoMatch);

        // Fecha: 08/10/2026 - TAREA: 00076 - Linea: 2 - Un externo y varios movimientos forman un grupo atómico de suma exacta.
        public static void ConciliarSeleccion(int idSesion, int idItemExterno, IReadOnlyCollection<int> idsMovimientos, TipoMatch tipoMatch)
            => ConciliacionExternStorage.ConciliarSeleccion(idSesion, idItemExterno, idsMovimientos, tipoMatch);

        public static void DesconciliarPar(int idPar)
            => ConciliacionExternStorage.DesconciliarPar(idPar);

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


                //Hacemos el cambio de que no haya duplicados. se concilia con el que primero caiga en esa fecha por caida
                if (candidatos.Count == 1)
                {
                    ConciliarPar(idSesion, ext.Id, candidatos[0].Id, TipoMatch.FechaImporte);
                    conciliadosExtracto.Add(candidatos[0].Id);
                    pendientesExternos.Remove(ext);
                    totalConciliados++;
                }
                else if (candidatos.Count > 1)
                {
                    ConciliarPar(idSesion, ext.Id, candidatos[0].Id, TipoMatch.FechaImporte);
                    conciliadosExtracto.Add(candidatos[0].Id);
                    pendientesExternos.Remove(ext);
                    totalConciliados++;
                    //duplicados.Add((ext, candidatos));
                    //pendientesExternos.Remove(ext);
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
                    ConciliarPar(idSesion, ext.Id, candidatos[0].Id, TipoMatch.SoloImporte);
                    conciliadosExtracto.Add(candidatos[0].Id);
                    totalConciliados++;
                    //duplicados.Add((ext, candidatos));
                }
            }

            return (totalConciliados, duplicados);
        }

        // Los criterios de igualdad (fecha/importe) viven en ComparadorConciliacion,
        // compartidos con el resaltado manual de ConciliacionExternForm.
    }
}
