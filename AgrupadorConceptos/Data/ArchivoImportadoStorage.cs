using System;
using System.Collections.Generic;
using System.Linq;
using AgrupadorConceptos.Models;
using Dapper;

namespace AgrupadorConceptos.Data
{
    /// <summary>
    /// Acceso a bancos.ArchivosImportados: cada extracto que se importó,
    /// con sus movimientos colgando por FK.
    /// </summary>
    internal static class ArchivoImportadoStorage
    {
        // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 1 - Columnas explícitas y sólo archivos activos
        // SELECT * traería MovimientosJson (megas) en cada listado, y un archivo dado de baja no se ofrece en ninguna pantalla.
        private const string Columnas = "Id, IdPerfilBanco, NombreArchivo, Fecha, Blindado, BlindadoFecha, BlindadoPor, Activo";

        public static List<ArchivoImportado> ObtenerPorPerfil(int idPerfilBanco)
        {
            using var cn = DatabaseHelper.Open();
            return cn.Query<ArchivoImportado>(
                $"SELECT {Columnas} FROM bancos.ArchivosImportados WHERE IdPerfilBanco = @IdPerfilBanco AND Activo = 1 ORDER BY Fecha DESC",
                new { IdPerfilBanco = idPerfilBanco }).ToList();
        }

        public static List<ArchivoImportado> ObtenerTodos()
        {
            using var cn = DatabaseHelper.Open();
            return cn.Query<ArchivoImportado>(
                $"SELECT {Columnas} FROM bancos.ArchivosImportados WHERE Activo = 1 ORDER BY Fecha DESC").ToList();
        }

        /// <summary>Inserta la cabecera del archivo y devuelve su Id.</summary>
        public static int Insertar(int idPerfilBanco, string nombreArchivo, DateTime fecha)
        {
            using var cn = DatabaseHelper.Open();
            return Insertar(idPerfilBanco, nombreArchivo, fecha, cn, null);
        }

        // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Compartir la transacción del lote evita una cabecera huérfana.
        public static int Insertar(int idPerfilBanco, string nombreArchivo, DateTime fecha, System.Data.IDbConnection cn, System.Data.IDbTransaction tx)
        {
            return cn.QuerySingle<int>(@"
                INSERT INTO bancos.ArchivosImportados (IdPerfilBanco, NombreArchivo, Fecha)
                OUTPUT INSERTED.Id
                VALUES (@IdPerfil, @Nombre, @Fecha);",
                new { IdPerfil = idPerfilBanco, Nombre = nombreArchivo, Fecha = fecha }, tx);
        }

        /// <summary>
        /// Los movimientos se borran solos (FK ON DELETE CASCADE). Antes de borrar se valida que
        /// ningún movimiento esté tomado por un par ni alcanzado por una sesión: si lo está,
        /// lanza InvalidOperationException con el motivo. Un SqlException es un error de base de datos.
        /// </summary>
        public static void Eliminar(int id)
        {
            // Fecha: 08/10/2026 - TAREA: 00076 - Linea: 1 - La cascada del archivo no debe eludir la protección de pendientes y pares.
            EliminacionMovimientosStorage.EliminarArchivo(id);
        }
    }
}
