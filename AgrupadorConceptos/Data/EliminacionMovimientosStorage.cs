// Fecha: 08/10/2026 - TAREA: 00076 - Linea: 1 - Una baja sólo se confirma si todos los movimientos siguen libres bajo el bloqueo compartido.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;
using Dapper;

namespace AgrupadorConceptos.Data
{
    internal static class EliminacionMovimientosStorage
    {
        public static void EliminarMovimientos(int idArchivo, IReadOnlyCollection<int> ids)
        {
            if (ids == null || ids.Count == 0 || ids.Any(id => id <= 0) || ids.Distinct().Count() != ids.Count)
                throw new ArgumentException("Seleccioná movimientos válidos y sin repetir.", nameof(ids));
            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();
            ConciliacionMutationStorage.Bloquear(cn, tx);
            var movimientos = cn.Query<MovimientoProcesado>(
                "SELECT * FROM bancos.MovimientosArchivo WHERE Id IN @Ids", new { Ids = ids }, tx).ToList();
            if (movimientos.Count != ids.Count || movimientos.Any(m => m.IdArchivo != idArchivo))
                throw new InvalidOperationException("La selección cambió o contiene movimientos de otro archivo. Actualizá la grilla.");
            ValidarLibres(cn, tx, movimientos, null);
            int borrados = cn.Execute("DELETE FROM bancos.MovimientosArchivo WHERE Id IN @Ids AND IdArchivo = @Archivo",
                new { Ids = ids, Archivo = idArchivo }, tx);
            if (borrados != ids.Count) throw new InvalidOperationException("La selección cambió. Actualizá la grilla.");
            tx.Commit();
        }

        public static void EliminarArchivo(int idArchivo)
        {
            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();
            ConciliacionMutationStorage.Bloquear(cn, tx);
            var archivo = cn.QuerySingleOrDefault<ArchivoImportado>(
                "SELECT * FROM bancos.ArchivosImportados WHERE Id = @Id", new { Id = idArchivo }, tx);
            if (archivo == null) throw new InvalidOperationException("El archivo ya no existe. Actualizá la lista.");
            var movimientos = cn.Query<MovimientoProcesado>(
                "SELECT * FROM bancos.MovimientosArchivo WHERE IdArchivo = @Id", new { Id = idArchivo }, tx).ToList();
            ValidarLibres(cn, tx, movimientos, idArchivo);
            int borrados = cn.Execute("DELETE FROM bancos.ArchivosImportados WHERE Id = @Id", new { Id = idArchivo }, tx);
            if (borrados != 1) throw new InvalidOperationException("El archivo cambió. Actualizá la lista.");
            tx.Commit();
        }

        private static void ValidarLibres(System.Data.IDbConnection cn, System.Data.IDbTransaction tx,
            List<MovimientoProcesado> movimientos, int? idArchivo)
        {
            var ids = movimientos.Select(m => m.Id).ToArray();
            if (ids.Length > 0)
            {
                // Los pares históricos protegen aun cuando falte la sesión que los originó.
                bool hayPares = cn.ExecuteScalar<int>(@"SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM bancos.ConciliacionPares WHERE IdMovimientoProcesado IN @Ids) OR EXISTS (
                    SELECT 1 FROM bancos.ConciliacionInternaPares WHERE IdMovimientoA IN @Ids OR IdMovimientoB IN @Ids)
                    THEN 1 ELSE 0 END", new { Ids = ids }, tx) != 0;
                if (hayPares) throw new InvalidOperationException("Hay movimientos tomados por pares de conciliación. No se eliminó nada.");
            }
            var externas = cn.Query<ConciliacionSesion>(@"SELECT * FROM bancos.ConciliacionSesiones
                WHERE Estado IN (N'EnProceso', N'Finalizada')", transaction: tx).ToList();
            foreach (var sesion in externas)
            {
                List<int> archivos;
                try
                {
                    archivos = string.IsNullOrWhiteSpace(sesion.ArchivosJson)
                        ? new List<int> { sesion.IdArchivoImportado }
                        : JsonSerializer.Deserialize<List<int>>(sesion.ArchivosJson)
                          ?? throw new JsonException();
                }
                catch (JsonException)
                {
                    throw new InvalidOperationException($"La sesión externa '{sesion.Nombre}' tiene archivos ilegibles. No se puede asegurar la baja.");
                }
                archivos.Add(sesion.IdArchivoImportado);
                if ((idArchivo.HasValue && archivos.Contains(idArchivo.Value)) ||
                    movimientos.Any(m => archivos.Contains(m.IdArchivo)))
                    throw new InvalidOperationException($"La sesión externa '{sesion.Nombre}' [{sesion.Estado}] utiliza este archivo. No se eliminó nada.");
            }
            if (movimientos.Count == 0) return;
            var internas = cn.Query<ConciliacionInternaSesion>(@"SELECT * FROM bancos.ConciliacionInternaSesiones
                WHERE Estado IN (N'EnProceso', N'Finalizada')", transaction: tx).ToList();
            foreach (var sesion in internas)
            {
                List<string> conceptos;
                try { conceptos = JsonSerializer.Deserialize<List<string>>(sesion.ConceptosJson) ?? throw new JsonException(); }
                catch (JsonException)
                { throw new InvalidOperationException($"La sesión interna '{sesion.Nombre}' tiene conceptos ilegibles. No se puede asegurar la baja."); }
                foreach (var movimiento in movimientos.Where(m => conceptos.Contains(m.ConceptoFinal, StringComparer.OrdinalIgnoreCase)))
                {
                    // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Operar sobre días tipados evita reinterpretaciones culturales.
                    var fecha = movimiento.Fecha.Date;
                    if (fecha >= sesion.FechaDesde.Date && fecha <= sesion.FechaHasta.Date)
                        throw new InvalidOperationException($"La sesión interna '{sesion.Nombre}' [{sesion.Estado}] alcanza movimientos seleccionados. No se eliminó nada.");
                }
            }
        }
    }
}
