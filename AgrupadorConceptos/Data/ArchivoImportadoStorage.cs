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
        public static List<ArchivoImportado> ObtenerPorPerfil(int idPerfilBanco)
        {
            using var cn = DatabaseHelper.Open();
            return cn.Query<ArchivoImportado>(
                "SELECT * FROM bancos.ArchivosImportados WHERE IdPerfilBanco = @IdPerfilBanco ORDER BY Fecha DESC",
                new { IdPerfilBanco = idPerfilBanco }).ToList();
        }

        public static List<ArchivoImportado> ObtenerTodos()
        {
            using var cn = DatabaseHelper.Open();
            return cn.Query<ArchivoImportado>(
                "SELECT * FROM bancos.ArchivosImportados ORDER BY Fecha DESC").ToList();
        }

        /// <summary>Inserta la cabecera del archivo y devuelve su Id.</summary>
        public static int Insertar(int idPerfilBanco, string nombreArchivo, DateTime fecha)
        {
            using var cn = DatabaseHelper.Open();
            return cn.QuerySingle<int>(@"
                INSERT INTO bancos.ArchivosImportados (IdPerfilBanco, NombreArchivo, Fecha)
                OUTPUT INSERTED.Id
                VALUES (@IdPerfil, @Nombre, @Fecha);",
                new { IdPerfil = idPerfilBanco, Nombre = nombreArchivo, Fecha = fecha });
        }

        /// <summary>
        /// Los movimientos se borran solos (FK ON DELETE CASCADE), pero si el archivo
        /// tiene sesiones de conciliación asociadas la FK lo impide y SQL Server tira
        /// SqlException: el llamador decide cómo informarlo.
        /// </summary>
        public static void Eliminar(int id)
        {
            using var cn = DatabaseHelper.Open();
            cn.Execute("DELETE FROM bancos.ArchivosImportados WHERE Id = @Id", new { Id = id });
        }
    }
}
