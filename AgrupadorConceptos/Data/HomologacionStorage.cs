using System;
using System.Collections.Generic;
using System.Linq;
using AgrupadorConceptos.Models;
using Dapper;

namespace AgrupadorConceptos.Data
{
    /// <summary>
    /// Acceso a bancos.HomologacionConceptos y bancos.ConceptosEstandar:
    /// el mapeo ValorOriginal (lo que dice el banco) → ConceptoEstandar (lo que
    /// usa la contabilidad).
    /// </summary>
    internal static class HomologacionStorage
    {
        /// <summary>
        /// Diccionario ValorOriginal → ConceptoEstandar del perfil, case-insensitive.
        ///
        /// El ORDER BY no es cosmético: cuando el perfil no es por código, el match se
        /// hace por substring y puede haber varias claves candidatas para la misma
        /// descripción; gana la primera del diccionario. Sin un orden fijo, dos
        /// pantallas resolvían el mismo movimiento de forma distinta.
        /// </summary>
        /// <param name="idHomologacionAExcluir">
        /// Regla que hay que dejar afuera. Sirve para preguntar "¿qué pasaría si esta
        /// regla no existiera?" antes de borrarla. Se excluye en el SQL y no sacando la
        /// clave después, porque quitarla del diccionario ya armado no garantiza que el
        /// resto conserve el orden del ORDER BY, y ese orden es la precedencia.
        /// </param>
        public static Dictionary<string, string> ObtenerDiccionario(int idPerfilBanco, int? idHomologacionAExcluir = null)
        {
            using var cn = DatabaseHelper.Open();
            return cn.Query(@"
                SELECT h.ValorOriginal, c.Nombre AS ConceptoEstandar
                FROM bancos.HomologacionConceptos h
                INNER JOIN bancos.ConceptosEstandar c ON h.IdConceptoEstandar = c.Id
                WHERE h.IdPerfilBanco = @IdPerfil
                  AND (@IdExcluir IS NULL OR h.Id <> @IdExcluir)
                ORDER BY h.ValorOriginal DESC",
                new { IdPerfil = idPerfilBanco, IdExcluir = idHomologacionAExcluir })
                .ToDictionary(x => (string)x.ValorOriginal, x => (string)x.ConceptoEstandar,
                              StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>Listado para la pantalla de gestión, con el banco resuelto.</summary>
        /// <param name="idPerfilBanco">Perfil a listar, o null para todos.</param>
        public static List<HomologacionListado> ObtenerListado(int? idPerfilBanco = null)
        {
            using var cn = DatabaseHelper.Open();
            // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 3 - Filtro por perfil y orden por clave
            // El ORDER BY replica el del diccionario (ValorOriginal DESC): la grilla tiene
            // que leerse en el mismo orden de precedencia con el que resuelve el matcher.
            return cn.Query<HomologacionListado>(@"
                SELECT h.Id, h.IdPerfilBanco, h.IdConceptoEstandar,
                       p.NombreBanco AS Banco, h.ValorOriginal, c.Nombre AS ConceptoEstandar
                FROM bancos.HomologacionConceptos h
                JOIN bancos.PerfilesBanco     p ON h.IdPerfilBanco      = p.Id
                JOIN bancos.ConceptosEstandar c ON h.IdConceptoEstandar = c.Id
                WHERE (@IdPerfil IS NULL OR h.IdPerfilBanco = @IdPerfil)
                ORDER BY p.NombreBanco, h.ValorOriginal DESC",
                new { IdPerfil = idPerfilBanco }).ToList();
        }

        public static void Eliminar(int id)
        {
            using var cn = DatabaseHelper.Open();
            cn.Execute("DELETE FROM bancos.HomologacionConceptos WHERE Id = @Id", new { Id = id });
        }

        public static List<ConceptoEstandar> ObtenerConceptosEstandar()
        {
            using var cn = DatabaseHelper.Open();
            return cn.Query<ConceptoEstandar>(
                "SELECT * FROM bancos.ConceptosEstandar ORDER BY Nombre").ToList();
        }

        /// <summary>
        /// Da de alta la homologación del valor para el perfil: busca o crea el concepto
        /// estándar, pisa la homologación previa de ese mismo valor y guarda la nueva.
        ///
        /// Las tres operaciones van en una transacción: antes se hacían sueltas y un
        /// fallo intermedio podía dejar un ConceptoEstandar recién creado sin
        /// homologación, o el valor sin ninguna homologación tras el DELETE.
        /// </summary>
        /// <returns>Id del concepto estándar usado.</returns>
        public static int Guardar(int idPerfilBanco, string valorOriginal, string nombreConcepto)
        {
            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();

            int? idConcepto = cn.QueryFirstOrDefault<int?>(
                "SELECT Id FROM bancos.ConceptosEstandar WHERE LOWER(Nombre) = LOWER(@Nombre)",
                new { Nombre = nombreConcepto }, tx);

            if (idConcepto == null)
                idConcepto = cn.QuerySingle<int>(
                    "INSERT INTO bancos.ConceptosEstandar (Nombre) OUTPUT INSERTED.Id VALUES (@Nombre);",
                    new { Nombre = nombreConcepto }, tx);

            cn.Execute(@"
                DELETE FROM bancos.HomologacionConceptos
                WHERE IdPerfilBanco = @IdPerfilBanco AND ValorOriginal = @ValorOriginal",
                new { IdPerfilBanco = idPerfilBanco, ValorOriginal = valorOriginal }, tx);

            cn.Execute(@"
                INSERT INTO bancos.HomologacionConceptos (IdPerfilBanco, ValorOriginal, IdConceptoEstandar)
                VALUES (@IdPerfilBanco, @ValorOriginal, @IdConceptoEstandar)",
                new { IdPerfilBanco = idPerfilBanco, ValorOriginal = valorOriginal, IdConceptoEstandar = idConcepto.Value }, tx);

            tx.Commit();
            return idConcepto.Value;
        }
    }
}
