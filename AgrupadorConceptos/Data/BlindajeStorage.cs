// Fecha: 26/10/2026 - TAREA: 00135 - Linea: 3 - Blindaje del archivo (cierre de homologación): marca, guardias de escritura y registro
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using Dapper;

namespace AgrupadorConceptos.Data
{
    /// <summary>Se intentó modificar un archivo blindado.</summary>
    public class ArchivoBlindadoException : InvalidOperationException
    {
        public ArchivoBlindadoException()
            : base("El archivo está blindado (cierre de homologación). Para modificarlo hay que desblindarlo con la clave maestra.") { }
    }

    /// <summary>Se intentó conciliar con un archivo que todavía no está blindado.</summary>
    public class ArchivoSinBlindarException : InvalidOperationException
    {
        public IReadOnlyList<string> Archivos { get; }
        public ArchivoSinBlindarException(IReadOnlyList<string> archivos)
            : base($"Para conciliar, blindá primero {(archivos.Count == 1 ? "el archivo" : "los archivos")}: {string.Join(", ", archivos)}.")
            => Archivos = archivos;
    }

    /// <summary>
    /// Marca de blindaje de bancos.ArchivosImportados. Un solo lugar decide qué se puede escribir: los *Storage llaman a
    /// una de las guardias dentro de su transacción y con el bloqueo de mutaciones ya tomado, así el blindaje no se puede
    /// saltear con una escritura concurrente.
    /// </summary>
    internal static class BlindajeStorage
    {
        // ---- guardias ----

        /// <summary>Rechaza la escritura si el archivo está blindado. Un archivo inexistente no se juzga acá.</summary>
        public static void ExigirNoBlindado(IDbConnection cn, IDbTransaction tx, int idArchivo)
        {
            if (cn.ExecuteScalar<int>("SELECT COUNT(*) FROM bancos.ArchivosImportados WHERE Id = @Id AND Blindado = 1", new { Id = idArchivo }, tx) != 0)
                throw new ArchivoBlindadoException();
        }

        /// <summary>Igual que <see cref="ExigirNoBlindado"/> para movimientos: mira los archivos a los que pertenecen.</summary>
        public static void ExigirNoBlindadoPorMovimientos(IDbConnection cn, IDbTransaction tx, IReadOnlyCollection<int> idsMovimientos)
        {
            if (idsMovimientos == null || idsMovimientos.Count == 0) return;
            if (cn.ExecuteScalar<int>(@"SELECT COUNT(*) FROM bancos.MovimientosArchivo m
                    JOIN bancos.ArchivosImportados a ON a.Id = m.IdArchivo
                    WHERE a.Blindado = 1 AND m.Id IN (SELECT CAST([value] AS int) FROM OPENJSON(@Ids))",
                new { Ids = JsonSerializer.Serialize(idsMovimientos) }, tx) != 0)
                throw new ArchivoBlindadoException();
        }

        /// <summary>Conciliar exige que todos los archivos que intervienen estén blindados (y activos).</summary>
        public static void ExigirBlindados(IDbConnection cn, IDbTransaction tx, IReadOnlyCollection<int> idsArchivos)
        {
            if (idsArchivos == null || idsArchivos.Count == 0) return;
            var sinBlindar = cn.Query<string>(@"SELECT ISNULL(p.NombreBanco + N' · ', N'') + a.NombreArchivo
                    FROM bancos.ArchivosImportados a LEFT JOIN bancos.PerfilesBanco p ON p.Id = a.IdPerfilBanco
                    WHERE a.Id IN (SELECT CAST([value] AS int) FROM OPENJSON(@Ids)) AND (a.Blindado = 0 OR a.Activo = 0)
                    ORDER BY a.Id", new { Ids = JsonSerializer.Serialize(idsArchivos.Distinct().ToList()) }, tx).ToList();
            if (sinBlindar.Count > 0) throw new ArchivoSinBlindarException(sinBlindar);
        }

        /// <summary>Igual que <see cref="ExigirBlindados"/> partiendo de los movimientos que se van a conciliar.</summary>
        public static void ExigirBlindadosPorMovimientos(IDbConnection cn, IDbTransaction tx, IReadOnlyCollection<int> idsMovimientos)
        {
            if (idsMovimientos == null || idsMovimientos.Count == 0) return;
            var archivos = cn.Query<int>(@"SELECT DISTINCT m.IdArchivo FROM bancos.MovimientosArchivo m
                    WHERE m.Id IN (SELECT CAST([value] AS int) FROM OPENJSON(@Ids))",
                new { Ids = JsonSerializer.Serialize(idsMovimientos.Distinct().ToList()) }, tx).ToList();
            ExigirBlindados(cn, tx, archivos);
        }

        // ---- consulta ----

        public static bool EstaBlindado(int idArchivo)
        {
            using var cn = DatabaseHelper.Open();
            return cn.ExecuteScalar<int>("SELECT COUNT(*) FROM bancos.ArchivosImportados WHERE Id = @Id AND Blindado = 1", new { Id = idArchivo }) != 0;
        }

        // ---- cambio de estado ----

        /// <summary>Blinda un archivo activo. Devuelve false si ya estaba blindado (no se registra de nuevo).</summary>
        public static bool Blindar(int idArchivo, string usuario)
        {
            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();
            ConciliacionMutationStorage.Bloquear(cn, tx);
            int cambios = cn.Execute(@"UPDATE bancos.ArchivosImportados
                SET Blindado = 1, BlindadoFecha = SYSDATETIME(), BlindadoPor = @Usuario
                WHERE Id = @Id AND Activo = 1 AND Blindado = 0", new { Id = idArchivo, Usuario = usuario }, tx);
            if (cambios == 0)
            {
                if (cn.ExecuteScalar<int>("SELECT COUNT(*) FROM bancos.ArchivosImportados WHERE Id = @Id AND Activo = 1", new { Id = idArchivo }, tx) == 0)
                    throw new InvalidOperationException("El archivo ya no existe. Actualizá la lista.");
                return false;
            }
            Registrar(cn, tx, idArchivo, "Blindar", usuario, null);
            tx.Commit();
            return true;
        }

        /// <summary>
        /// Quita el blindaje. La clave ya la validó <see cref="Services.BlindajeService"/>; acá sólo se verifica que ninguna
        /// sesión de conciliación en proceso esté usando el archivo.
        /// </summary>
        public static void Desblindar(int idArchivo, string usuario, string motivo)
        {
            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();
            ConciliacionMutationStorage.Bloquear(cn, tx);
            string sesion = SesionEnProceso(cn, tx, idArchivo);
            if (sesion != null)
                throw new InvalidOperationException($"El archivo está en la sesión de conciliación «{sesion}», todavía en proceso. Cerrala o eliminala antes de desblindar.");
            int cambios = cn.Execute(@"UPDATE bancos.ArchivosImportados
                SET Blindado = 0, BlindadoFecha = NULL, BlindadoPor = NULL
                WHERE Id = @Id AND Blindado = 1", new { Id = idArchivo }, tx);
            if (cambios == 0) throw new InvalidOperationException("El archivo no está blindado. Actualizá la lista.");
            Registrar(cn, tx, idArchivo, "Desblindar", usuario, motivo);
            tx.Commit();
        }

        /// <summary>Nombre de la primera sesión de conciliación en proceso que usa el archivo, o null.</summary>
        private static string SesionEnProceso(IDbConnection cn, IDbTransaction tx, int idArchivo)
        {
            foreach (var s in cn.Query<(string Nombre, int IdArchivoImportado, string ArchivosJson)>(
                "SELECT Nombre, IdArchivoImportado, ArchivosJson FROM bancos.ConciliacionSesiones WHERE Estado = N'EnProceso'", transaction: tx))
            {
                var archivos = new HashSet<int> { s.IdArchivoImportado };
                if (!string.IsNullOrWhiteSpace(s.ArchivosJson))
                {
                    try { foreach (int a in JsonSerializer.Deserialize<List<int>>(s.ArchivosJson) ?? new()) archivos.Add(a); }
                    catch (JsonException) { return s.Nombre; } // ilegible: se asume en uso, es lo seguro
                }
                if (archivos.Contains(idArchivo)) return s.Nombre;
            }
            return cn.QueryFirstOrDefault<string>(@"SELECT TOP 1 s.Nombre
                FROM bancos.ConciliacionInternaPares p
                JOIN bancos.ConciliacionInternaSesiones s ON s.Id = p.IdSesion
                JOIN bancos.MovimientosArchivo m ON m.Id IN (p.IdMovimientoA, p.IdMovimientoB)
                WHERE s.Estado = N'EnProceso' AND m.IdArchivo = @Id", new { Id = idArchivo }, tx);
        }

        public static void Registrar(IDbConnection cn, IDbTransaction tx, int idArchivo, string accion, string usuario, string motivo) =>
            cn.Execute(@"INSERT INTO bancos.BlindajeLog (IdArchivo, Accion, Usuario, Fecha, Motivo)
                VALUES (@IdArchivo, @Accion, @Usuario, SYSDATETIME(), @Motivo)",
                new { IdArchivo = idArchivo, Accion = accion, Usuario = usuario ?? "", Motivo = motivo }, tx);

        /// <summary>Registra un intento fallido de desblindar. Va en su propia transacción: tiene que quedar aunque la operación se aborte.</summary>
        public static void RegistrarFallido(int idArchivo, string usuario)
        {
            using var cn = DatabaseHelper.Open();
            Registrar(cn, null, idArchivo, "DesblindoFallido", usuario, null);
        }
    }
}
