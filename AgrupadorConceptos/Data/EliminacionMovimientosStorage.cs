// Fecha: 13/10/2026 - TAREA: 00096 - Linea: 1, 3 - La baja ya no arma listas de IDs: SQL Server admite 2100 parámetros y un archivo real tiene miles de movimientos.
// Fecha: 08/10/2026 - TAREA: 00076 - Linea: 1 - Una baja sólo se confirma si todos los movimientos siguen libres bajo el bloqueo compartido.
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;
using Dapper;

namespace AgrupadorConceptos.Data
{
    internal static class EliminacionMovimientosStorage
    {
        // Fecha: 13/10/2026 - TAREA: 00096 - Linea: 3 - Los IDs de la selección viajan como un único parámetro JSON, sin tope de cantidad.
        private const string FiltroSeleccion = "m.Id IN (SELECT CAST([value] AS int) FROM OPENJSON(@Ids)) AND m.IdArchivo = @Archivo";
        private const string FiltroArchivo = "m.IdArchivo = @Archivo";

        public static void EliminarMovimientos(int idArchivo, IReadOnlyCollection<int> ids)
        {
            if (ids == null || ids.Count == 0 || ids.Any(id => id <= 0) || ids.Distinct().Count() != ids.Count)
                throw new ArgumentException("Seleccioná movimientos válidos y sin repetir.", nameof(ids));
            string idsJson = JsonSerializer.Serialize(ids);
            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();
            ConciliacionMutationStorage.Bloquear(cn, tx);
            // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 3 - Un archivo blindado no pierde movimientos hasta que se lo desblinde
            BlindajeStorage.ExigirNoBlindado(cn, tx, idArchivo);
            // Fecha: 13/10/2026 - TAREA: 00096 - Linea: 3 - Se cuentan en el servidor, en vez de traer las filas completas sólo para contarlas.
            var cuentas = cn.QuerySingle<(int Existentes, int EnElArchivo)>(@"
                SELECT COUNT(*) AS Existentes, ISNULL(SUM(CASE WHEN IdArchivo = @Archivo THEN 1 ELSE 0 END), 0) AS EnElArchivo
                FROM bancos.MovimientosArchivo WHERE Id IN (SELECT CAST([value] AS int) FROM OPENJSON(@Ids))",
                new { Ids = idsJson, Archivo = idArchivo }, tx);
            if (cuentas.Existentes != ids.Count || cuentas.EnElArchivo != ids.Count)
                throw new InvalidOperationException("La selección cambió o contiene movimientos de otro archivo. Actualizá la grilla.");
            ValidarLibres(cn, tx, idArchivo, FiltroSeleccion, new { Ids = idsJson, Archivo = idArchivo });
            int borrados = cn.Execute(@"DELETE FROM bancos.MovimientosArchivo
                WHERE Id IN (SELECT CAST([value] AS int) FROM OPENJSON(@Ids)) AND IdArchivo = @Archivo",
                new { Ids = idsJson, Archivo = idArchivo }, tx);
            if (borrados != ids.Count) throw new InvalidOperationException("La selección cambió. Actualizá la grilla.");
            tx.Commit();
        }

        public static void EliminarArchivo(int idArchivo)
        {
            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();
            ConciliacionMutationStorage.Bloquear(cn, tx);
            // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 1 - No se trae la fila entera: MovimientosJson puede pesar megas y acá sólo hace falta saber si existe y está activo
            var archivo = cn.QuerySingleOrDefault<(int Id, bool Blindado, bool Activo)>(
                "SELECT Id, Blindado, Activo FROM bancos.ArchivosImportados WHERE Id = @Id", new { Id = idArchivo }, tx);
            if (archivo.Id == 0 || !archivo.Activo) throw new InvalidOperationException("El archivo ya no existe. Actualizá la lista.");
            // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 3 - Un archivo blindado no se da de baja hasta desblindarlo
            if (archivo.Blindado) throw new ArchivoBlindadoException();
            // Fecha: 13/10/2026 - TAREA: 00096 - Linea: 1 - Los movimientos del archivo ya no se cargan en memoria: la validación pregunta a la base.
            ValidarLibres(cn, tx, idArchivo, FiltroArchivo, new { Archivo = idArchivo });
            // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 1 - La baja deja el archivo inactivo y archiva sus movimientos
            // Los movimientos se guardan como JSON comprimido en la fila del archivo y recién después se borran de la tabla viva,
            // en la misma transacción: si el archivado falla no se pierde nada. Se serializan todas las columnas tal como vienen de
            // la base (con su Id), así una restauración futura reinserta exactamente lo que había y el historial sigue apuntando a esos Id.
            // La tabla de movimientos queda limpia y ninguna consulta tiene que filtrar por archivo inactivo.
            var filas = cn.Query("SELECT * FROM bancos.MovimientosArchivo WHERE IdArchivo = @Id ORDER BY Id", new { Id = idArchivo }, tx)
                .Cast<IDictionary<string, object>>().ToList();
            byte[] comprimido = Comprimir(JsonSerializer.SerializeToUtf8Bytes(filas));
            int cambiados = cn.Execute(@"UPDATE bancos.ArchivosImportados
                SET Activo = 0, InactivoFecha = SYSDATETIME(), InactivoPor = @Usuario, CantMovimientos = @Cant, MovimientosJson = @Json
                WHERE Id = @Id AND Activo = 1",
                new { Id = idArchivo, Usuario = AuditoriaContexto.Usuario ?? Environment.UserName, Cant = filas.Count, Json = comprimido }, tx);
            if (cambiados != 1) throw new InvalidOperationException("El archivo cambió. Actualizá la lista.");
            cn.Execute("DELETE FROM bancos.MovimientosArchivo WHERE IdArchivo = @Id", new { Id = idArchivo }, tx);
            tx.Commit();
        }

        // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 1 - Aviso previo a la baja: cuánto de lo hecho en el archivo se archivaría
        /// <summary>
        /// Cuenta lo que se modificó a mano o se cerró en el archivo, para avisar antes de darlo de baja. Los criterios son los de la guía:
        /// concepto final distinto del estándar (y no pendiente), cuenta final distinta de la que le corresponde al concepto, movimientos
        /// conciliados y movimientos con alguna edición manual registrada en el historial.
        /// </summary>
        public static ModificacionesArchivo ContarModificaciones(int idArchivo)
        {
            using var cn = DatabaseHelper.Open();
            return cn.QuerySingle<ModificacionesArchivo>(@"
                SELECT COUNT(*) AS Total,
                       ISNULL(SUM(CASE WHEN x.FinalEditado = 1 THEN 1 ELSE 0 END), 0) AS ConceptosFinalesEditados,
                       ISNULL(SUM(CASE WHEN x.CuentaEditada = 1 THEN 1 ELSE 0 END), 0) AS CuentasEditadas,
                       ISNULL(SUM(CASE WHEN x.Conciliado = 1 THEN 1 ELSE 0 END), 0) AS Conciliados
                FROM (
                    SELECT m.Conciliado,
                           CASE WHEN LTRIM(RTRIM(ISNULL(m.ConceptoFinal, N''))) <> N'' AND m.ConceptoFinal <> @Pendiente
                                     AND ISNULL(m.ConceptoFinal, N'') <> ISNULL(m.ConceptoEstandar, N'') THEN 1 ELSE 0 END AS FinalEditado,
                           CASE WHEN LTRIM(RTRIM(ISNULL(m.CuentaFinal, N''))) <> N''
                                     AND ISNULL(cc.Cuenta, N'') <> m.CuentaFinal THEN 1 ELSE 0 END AS CuentaEditada,
                           CASE WHEN EXISTS (SELECT 1 FROM bancos.MovimientosArchivoHistorial h
                                             WHERE h.IdMovimiento = m.Id AND h.Origen = N'Manual') THEN 1 ELSE 0 END AS EdicionManual
                    FROM bancos.MovimientosArchivo m
                    LEFT JOIN bancos.ConceptosEstandar ce ON ce.Nombre = m.ConceptoEstandar
                    LEFT JOIN bancos.CuentasContables cc ON cc.Id = ce.IdCuentaContable
                    WHERE m.IdArchivo = @Id
                ) x
                WHERE x.FinalEditado = 1 OR x.CuentaEditada = 1 OR x.Conciliado = 1 OR x.EdicionManual = 1",
                new { Id = idArchivo, Pendiente = ConceptosBancarios.PendienteHomologar });
        }

        private static byte[] Comprimir(byte[] datos)
        {
            using var salida = new MemoryStream();
            using (var gzip = new GZipStream(salida, CompressionLevel.Optimal, leaveOpen: true))
                gzip.Write(datos, 0, datos.Length);
            return salida.ToArray();
        }

        // Fecha: 13/10/2026 - TAREA: 00096 - Linea: 1, 3 - Los movimientos a dar de baja se describen con un filtro SQL (todo el archivo, o una selección dentro de él).
        // El filtro es una constante de este archivo y usa sólo parámetros: no se interpola texto del usuario.
        private static void ValidarLibres(System.Data.IDbConnection cn, System.Data.IDbTransaction tx,
            int idArchivo, string filtro, object parametros)
        {
            // Los pares históricos protegen aun cuando falte la sesión que los originó.
            bool hayPares = cn.ExecuteScalar<int>($@"SELECT CASE WHEN
                EXISTS (SELECT 1 FROM bancos.ConciliacionPares p JOIN bancos.MovimientosArchivo m ON m.Id = p.IdMovimientoProcesado WHERE {filtro})
                OR EXISTS (SELECT 1 FROM bancos.ConciliacionInternaPares p JOIN bancos.MovimientosArchivo m ON m.Id = p.IdMovimientoA WHERE {filtro})
                OR EXISTS (SELECT 1 FROM bancos.ConciliacionInternaPares p JOIN bancos.MovimientosArchivo m ON m.Id = p.IdMovimientoB WHERE {filtro})
                THEN 1 ELSE 0 END", parametros, tx) != 0;
            if (hayPares) throw new InvalidOperationException("Hay movimientos tomados por pares de conciliación. No se eliminó nada.");

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
                // Todos los movimientos a dar de baja pertenecen a este archivo.
                if (archivos.Contains(idArchivo))
                    throw new InvalidOperationException($"La sesión externa '{sesion.Nombre}' [{sesion.Estado}] utiliza este archivo. No se eliminó nada.");
            }
            var internas = cn.Query<ConciliacionInternaSesion>(@"SELECT * FROM bancos.ConciliacionInternaSesiones
                WHERE Estado IN (N'EnProceso', N'Finalizada')", transaction: tx).ToList();
            foreach (var sesion in internas)
            {
                try { if (JsonSerializer.Deserialize<List<string>>(sesion.ConceptosJson) == null) throw new JsonException(); }
                catch (JsonException)
                { throw new InvalidOperationException($"La sesión interna '{sesion.Nombre}' tiene conceptos ilegibles. No se puede asegurar la baja."); }
                // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Operar sobre días tipados evita reinterpretaciones culturales.
                // Fecha: 13/10/2026 - TAREA: 00096 - Linea: 1, 3 - El alcance de la sesión (conceptos y rango) se evalúa en SQL, sobre Fecha date.
                var p = new DynamicParameters(parametros);
                p.Add("Conceptos", sesion.ConceptosJson);
                p.Add("Desde", sesion.FechaDesde.Date);
                p.Add("Hasta", sesion.FechaHasta.Date);
                bool alcanza = cn.ExecuteScalar<int>($@"SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM bancos.MovimientosArchivo m WHERE {filtro}
                      AND m.Fecha >= @Desde AND m.Fecha <= @Hasta
                      AND m.ConceptoFinal IN (SELECT [value] FROM OPENJSON(@Conceptos))) THEN 1 ELSE 0 END", p, tx) != 0;
                if (alcanza)
                    throw new InvalidOperationException($"La sesión interna '{sesion.Nombre}' [{sesion.Estado}] alcanza movimientos seleccionados. No se eliminó nada.");
            }
        }
    }
}
