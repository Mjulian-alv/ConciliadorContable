// Fecha: 05/11/2026 - TAREA: 00162 - Linea: 2 - Un movimiento apareado en una conciliación no se toca, aunque la conciliación no esté cerrada
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using Dapper;

namespace AgrupadorConceptos.Data
{
    /// <summary>Dónde está apareado un movimiento: la vía (interna o externa), la sesión y su estado.</summary>
    public sealed class ParDeMovimiento
    {
        public int IdMovimiento { get; set; }
        /// <summary><c>interna</c> o <c>externa</c>.</summary>
        public string Via { get; set; }
        /// <summary>Nombre de la sesión; vacío si la sesión ya no existe (el par sobrevive a su sesión: no tiene FK al movimiento).</summary>
        public string Sesion { get; set; }
        /// <summary><c>EnProceso</c>, <c>Finalizada</c>, o vacío si la sesión ya no existe.</summary>
        public string Estado { get; set; }

        /// <summary>«conciliación interna «Interna octubre 2026»»; sin sesión, «una conciliación».</summary>
        public string Descripcion =>
            string.IsNullOrWhiteSpace(Sesion) ? "una conciliación" : $"una conciliación {Via} («{Sesion}»)";

        /// <summary>Texto corto para la grilla: «Conciliación interna «X» · en proceso».</summary>
        public string Detalle
        {
            get
            {
                string via = Via == "externa" ? "Conciliación externa" : "Conciliación interna";
                if (string.IsNullOrWhiteSpace(Sesion)) return via;
                string estado = Estado == "EnProceso" ? "en proceso" : Estado == "Finalizada" ? "finalizada" : "";
                return estado.Length == 0 ? $"{via} «{Sesion}»" : $"{via} «{Sesion}» · {estado}";
            }
        }
    }

    /// <summary>Se intentó modificar un movimiento que figura en un par de conciliación.</summary>
    public class MovimientoApareadoException : InvalidOperationException
    {
        public ParDeMovimiento Par { get; }
        public MovimientoApareadoException(ParDeMovimiento par)
            : base($"Este movimiento está apareado en {par.Descripcion} y no se puede modificar. Deshacé el par en esa conciliación si hay que cambiarlo.")
            => Par = par;
    }

    /// <summary>
    /// Un solo lugar decide si un movimiento está apareado: figura en <c>bancos.ConciliacionPares</c> (externa) o en
    /// <c>bancos.ConciliacionInternaPares</c> (interna), sin importar el estado de la sesión ni que la sesión siga existiendo
    /// (mismo criterio que la baja de archivos y movimientos). Las guardias se llaman dentro de la transacción del llamador y
    /// con el bloqueo de mutaciones ya tomado, como las del blindaje, para que una escritura concurrente no las saltee.
    /// </summary>
    internal static class ParesMovimientoStorage
    {
        // Las tres ramas devuelven la misma forma. LEFT JOIN a la sesión: un par puede haber quedado sin sesión.
        private const string SqlApareados = @"
            SELECT p.IdMovimientoProcesado AS IdMovimiento, N'externa' AS Via, s.Nombre AS Sesion, s.Estado AS Estado
              FROM bancos.ConciliacionPares p LEFT JOIN bancos.ConciliacionSesiones s ON s.Id = p.IdSesion
             WHERE p.IdMovimientoProcesado IN (SELECT CAST([value] AS int) FROM OPENJSON(@Ids))
            UNION ALL
            SELECT p.IdMovimientoA, N'interna', s.Nombre, s.Estado
              FROM bancos.ConciliacionInternaPares p LEFT JOIN bancos.ConciliacionInternaSesiones s ON s.Id = p.IdSesion
             WHERE p.IdMovimientoA IN (SELECT CAST([value] AS int) FROM OPENJSON(@Ids))
            UNION ALL
            SELECT p.IdMovimientoB, N'interna', s.Nombre, s.Estado
              FROM bancos.ConciliacionInternaPares p LEFT JOIN bancos.ConciliacionInternaSesiones s ON s.Id = p.IdSesion
             WHERE p.IdMovimientoB IN (SELECT CAST([value] AS int) FROM OPENJSON(@Ids))";

        /// <summary>Rechaza si alguno de los movimientos está apareado. Un id inexistente no se juzga acá.</summary>
        public static void ExigirSinPar(IDbConnection cn, IDbTransaction tx, IReadOnlyCollection<int> idsMovimientos)
        {
            var par = Consultar(cn, tx, idsMovimientos).FirstOrDefault();
            if (par != null) throw new MovimientoApareadoException(par);
        }

        /// <summary>Dónde está apareado cada uno de los movimientos que lo estén (el primero hallado si figura en más de un par).</summary>
        public static Dictionary<int, ParDeMovimiento> ObtenerApareados(IReadOnlyCollection<int> idsMovimientos)
        {
            using var cn = DatabaseHelper.Open();
            return Consultar(cn, null, idsMovimientos).GroupBy(p => p.IdMovimiento).ToDictionary(g => g.Key, g => g.First());
        }

        /// <summary>Los movimientos apareados de un archivo, con una sola consulta (para marcar la grilla al cargarla).</summary>
        public static Dictionary<int, ParDeMovimiento> ObtenerApareadosPorArchivo(int idArchivo)
        {
            using var cn = DatabaseHelper.Open();
            var ids = cn.Query<int>("SELECT Id FROM bancos.MovimientosArchivo WHERE IdArchivo = @Id", new { Id = idArchivo }).ToList();
            return Consultar(cn, null, ids).GroupBy(p => p.IdMovimiento).ToDictionary(g => g.Key, g => g.First());
        }

        /// <summary>Marca <see cref="Models.MovimientoProcesado.Apareado"/> y <see cref="Models.MovimientoProcesado.ApareadoEn"/> en los movimientos dados.</summary>
        public static void Marcar(IEnumerable<Models.MovimientoProcesado> movimientos, IDictionary<int, ParDeMovimiento> apareados)
        {
            foreach (var m in movimientos)
            {
                bool esta = apareados.TryGetValue(m.Id, out var par);
                m.Apareado = esta;
                m.ApareadoEn = esta ? par.Detalle : null;
                m.MensajeApareado = esta ? new MovimientoApareadoException(par).Message : null;
            }
        }

        private static List<ParDeMovimiento> Consultar(IDbConnection cn, IDbTransaction tx, IReadOnlyCollection<int> ids)
        {
            if (ids == null || ids.Count == 0) return new List<ParDeMovimiento>();
            return cn.Query<ParDeMovimiento>(SqlApareados, new { Ids = JsonSerializer.Serialize(ids.Distinct().ToList()) }, tx).ToList();
        }
    }
}
