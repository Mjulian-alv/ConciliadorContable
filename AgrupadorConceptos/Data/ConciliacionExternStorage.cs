// Fecha: 08/10/2026 - TAREA: 00076 - Linea: 1, 2 - Persistencia transaccional de sesiones y grupos externos.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;
using Dapper;

namespace AgrupadorConceptos.Data
{
    internal static class ConciliacionExternStorage
    {
        // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Operar sobre días tipados evita reinterpretaciones culturales.
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

        public static ConciliacionSesion CrearSesion(string nombre, List<int> idsArchivos,
            IEnumerable<string> conceptos, IEnumerable<ConciliacionItemExterno> itemsExternos)
        {
            if (idsArchivos == null || idsArchivos.Count == 0 || idsArchivos.Distinct().Count() != idsArchivos.Count)
                throw new ArgumentException("Seleccioná archivos válidos y sin repetir.", nameof(idsArchivos));
            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();
            ConciliacionMutationStorage.Bloquear(cn, tx);
            int presentes = cn.ExecuteScalar<int>("SELECT COUNT(*) FROM bancos.ArchivosImportados WHERE Id IN @Ids", new { Ids = idsArchivos }, tx);
            if (presentes != idsArchivos.Count) throw new InvalidOperationException("Uno de los archivos seleccionados ya no existe. Actualizá la lista.");
            int id = cn.QuerySingle<int>(@"INSERT INTO bancos.ConciliacionSesiones
                (Nombre, FechaCreacion, IdArchivoImportado, ArchivosJson, ConceptosJson, Estado)
                OUTPUT INSERTED.Id VALUES (@Nombre, @Fecha, @PrimerArchivo, @Archivos, @Conceptos, 'EnProceso')",
                new { Nombre = nombre, Fecha = DateTime.Now, PrimerArchivo = idsArchivos[0],
                    Archivos = JsonSerializer.Serialize(idsArchivos), Conceptos = JsonSerializer.Serialize(conceptos.ToList()) }, tx);
            // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Operar sobre días tipados evita reinterpretaciones culturales.
            foreach (var item in itemsExternos)
            {
                var p=new DynamicParameters(new {IdSesion=id,item.Importe,item.Detalle});
                p.Add("Fecha",item.Fecha.Date,System.Data.DbType.Date);
                cn.Execute(@"INSERT INTO bancos.ConciliacionItemsExternos (IdSesion, Fecha, Importe, Detalle, Conciliado)
                    VALUES (@IdSesion, @Fecha, @Importe, @Detalle, 0)",p,tx);
            }
            tx.Commit();
            return cn.QuerySingle<ConciliacionSesion>("SELECT * FROM bancos.ConciliacionSesiones WHERE Id = @Id", new { Id = id });
        }

        public static void EliminarSesion(int idSesion)
        {
            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();
            ConciliacionMutationStorage.Bloquear(cn, tx);
            cn.Execute("DELETE FROM bancos.ConciliacionPares WHERE IdSesion = @Id", new { Id = idSesion }, tx);
            cn.Execute("DELETE FROM bancos.ConciliacionItemsExternos WHERE IdSesion = @Id", new { Id = idSesion }, tx);
            cn.Execute("DELETE FROM bancos.ConciliacionSesiones WHERE Id = @Id", new { Id = idSesion }, tx);
            tx.Commit();
        }

        public static void MarcarFinalizada(int idSesion)
        {
            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();
            ConciliacionMutationStorage.Bloquear(cn, tx);
            int cambios = cn.Execute("UPDATE bancos.ConciliacionSesiones SET Estado = 'Finalizada' WHERE Id = @Id AND Estado = 'EnProceso'",
                new { Id = idSesion }, tx);
            if (cambios != 1) throw new InvalidOperationException("La sesión ya no está en proceso. Actualizá la pantalla.");
            tx.Commit();
        }

        public static void ConciliarSeleccion(int idSesion, int idItemExterno, IReadOnlyCollection<int> idsMovimientos, TipoMatch tipoMatch)
        {
            if (idsMovimientos == null || idsMovimientos.Count == 0 ||
                idsMovimientos.Any(id => id <= 0) || idsMovimientos.Distinct().Count() != idsMovimientos.Count)
                throw new ArgumentException("Seleccioná movimientos válidos y sin repetir.", nameof(idsMovimientos));
            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();
            ConciliacionMutationStorage.Bloquear(cn, tx);
            var sesion = cn.QuerySingleOrDefault<ConciliacionSesion>(
                "SELECT * FROM bancos.ConciliacionSesiones WHERE Id = @Id", new { Id = idSesion }, tx);
            if (sesion == null || sesion.Estado != "EnProceso")
                throw new InvalidOperationException("La sesión ya no está en proceso. Actualizá la pantalla.");
            var externo = cn.QuerySingleOrDefault<ConciliacionItemExterno>(
                "SELECT * FROM bancos.ConciliacionItemsExternos WHERE Id = @Id AND IdSesion = @Sesion",
                new { Id = idItemExterno, Sesion = idSesion }, tx);
            if (externo == null) throw new InvalidOperationException("El registro externo ya no pertenece a esta sesión.");
            int paresExistentes = cn.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM bancos.ConciliacionPares WHERE IdSesion = @Sesion AND IdItemExterno = @Externo",
                new { Sesion = idSesion, Externo = idItemExterno }, tx);
            if (externo.Conciliado || paresExistentes != 0)
                throw new InvalidOperationException("El registro externo ya está conciliado o tiene datos inconsistentes. Actualizá la pantalla.");
            var archivos = LeerArchivos(sesion);
            var movimientos = cn.Query<MovimientoProcesado>(
                "SELECT * FROM bancos.MovimientosArchivo WHERE Id IN @Ids", new { Ids = idsMovimientos }, tx).ToList();
            if (movimientos.Count != idsMovimientos.Count || movimientos.Any(m => !archivos.Contains(m.IdArchivo)))
                throw new InvalidOperationException("Hay movimientos que ya no pertenecen a los archivos de la sesión.");
            int ocupados = cn.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM bancos.ConciliacionPares WHERE IdSesion = @Sesion AND IdMovimientoProcesado IN @Ids",
                new { Sesion = idSesion, Ids = idsMovimientos }, tx);
            if (ocupados != 0) throw new InvalidOperationException("Hay movimientos ya asignados en esta sesión. Actualizá la pantalla.");
            if (movimientos.Sum(ComparadorConciliacion.ImporteEfectivo) != Math.Abs(externo.Importe))
                throw new InvalidOperationException("La suma seleccionada no coincide exactamente con el importe externo.");
            foreach (var idMovimiento in idsMovimientos)
                cn.Execute(@"INSERT INTO bancos.ConciliacionPares
                    (IdSesion, IdItemExterno, IdMovimientoProcesado, TipoMatch, FechaConciliacion)
                    VALUES (@IdSesion, @IdItemExterno, @IdMovimiento, @TipoMatch, @Fecha)",
                    new { IdSesion = idSesion, IdItemExterno = idItemExterno,
                        IdMovimiento = idMovimiento, TipoMatch = tipoMatch.ToString(), Fecha = DateTime.Now }, tx);
            int marcados = cn.Execute("UPDATE bancos.ConciliacionItemsExternos SET Conciliado = 1 WHERE Id = @Id AND IdSesion = @Sesion AND Conciliado = 0",
                new { Id = idItemExterno, Sesion = idSesion }, tx);
            if (marcados != 1) throw new InvalidOperationException("El registro externo cambió. Actualizá la pantalla.");
            tx.Commit();
        }

        public static void DesconciliarPar(int idPar)
        {
            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();
            ConciliacionMutationStorage.Bloquear(cn, tx);
            var par = cn.QuerySingleOrDefault<ConciliacionPar>("SELECT * FROM bancos.ConciliacionPares WHERE Id = @Id", new { Id = idPar }, tx);
            if (par == null) return;
            string estado = cn.QuerySingleOrDefault<string>(
                "SELECT Estado FROM bancos.ConciliacionSesiones WHERE Id = @Id", new { Id = par.IdSesion }, tx);
            if (estado != "EnProceso") throw new InvalidOperationException("La sesión ya no está en proceso. Actualizá la pantalla.");
            cn.Execute("DELETE FROM bancos.ConciliacionPares WHERE IdSesion = @Sesion AND IdItemExterno = @Externo",
                new { Sesion = par.IdSesion, Externo = par.IdItemExterno }, tx);
            cn.Execute("UPDATE bancos.ConciliacionItemsExternos SET Conciliado = 0 WHERE Id = @Id AND IdSesion = @Sesion",
                new { Id = par.IdItemExterno, Sesion = par.IdSesion }, tx);
            tx.Commit();
        }

        private static HashSet<int> LeerArchivos(ConciliacionSesion sesion)
        {
            try
            {
                var archivos = string.IsNullOrWhiteSpace(sesion.ArchivosJson)
                    ? new List<int>() : JsonSerializer.Deserialize<List<int>>(sesion.ArchivosJson) ?? throw new JsonException();
                archivos.Add(sesion.IdArchivoImportado);
                return archivos.ToHashSet();
            }
            catch (JsonException)
            { throw new InvalidOperationException("Los archivos de la sesión no se pueden interpretar."); }
        }
    }
}
