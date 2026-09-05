using System;
using System.Collections.Generic;
using System.Data;
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

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 1 - Baja con los movimientos ya resueltos
        /// <summary>
        /// Borra la regla y persiste en la misma transacción los movimientos que la baja
        /// dejó modificados. Atómico a propósito: media baja aplicada es peor que ninguna.
        /// </summary>
        /// <param name="movimientos">Ya vienen con ConceptoEstandar/ConceptoFinal decididos
        /// por HomologacionAdminService. Puede venir vacío: la regla se borra igual.</param>
        public static void EliminarYActualizarMovimientos(
            int idHomologacion, IReadOnlyCollection<MovimientoProcesado> movimientos)
        {
            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();

            cn.Execute("DELETE FROM bancos.HomologacionConceptos WHERE Id = @Id",
                new { Id = idHomologacion }, tx);

            MovimientoStorage.ActualizarConceptos(movimientos, cn, tx);

            tx.Commit();
        }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 2 - Edicion: reapuntar la regla y arrastrar
        /// <summary>
        /// Reapunta la regla a otro concepto (buscándolo o creándolo) y persiste en la misma
        /// transacción los movimientos que arrastra. Hace UPDATE en vez del DELETE+INSERT que
        /// usa <see cref="Guardar"/>, así el Id de la regla sobrevive a la edición.
        /// </summary>
        /// <returns>Id del concepto estándar al que quedó apuntando.</returns>
        public static int ReapuntarYActualizarMovimientos(
            int idHomologacion, string nombreConcepto, IReadOnlyCollection<MovimientoProcesado> movimientos)
        {
            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();

            int idConcepto = ObtenerOCrearConcepto(cn, tx, nombreConcepto);

            cn.Execute(@"
                UPDATE bancos.HomologacionConceptos
                SET IdConceptoEstandar = @IdConcepto
                WHERE Id = @Id",
                new { IdConcepto = idConcepto, Id = idHomologacion }, tx);

            MovimientoStorage.ActualizarConceptos(movimientos, cn, tx);

            tx.Commit();
            return idConcepto;
        }

        public static List<ConceptoEstandar> ObtenerConceptosEstandar()
        {
            using var cn = DatabaseHelper.Open();
            return cn.Query<ConceptoEstandar>(
                "SELECT * FROM bancos.ConceptosEstandar ORDER BY Nombre").ToList();
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Listado para Gestion de Conceptos Estandar
        /// <summary>
        /// Un concepto por fila, con su cuenta (si tiene) y cuántos movimientos la usan hoy.
        /// </summary>
        public static List<ConceptoEstandarListado> ObtenerListadoConceptosEstandar()
        {
            using var cn = DatabaseHelper.Open();
            return cn.Query<ConceptoEstandarListado>(@"
                SELECT c.Id, c.Nombre, c.IdCuentaContable,
                       cc.Cuenta, cc.Descripcion AS DescripcionCuenta,
                       ISNULL(m.Movimientos, 0) AS Movimientos
                FROM bancos.ConceptosEstandar c
                LEFT JOIN bancos.CuentasContables cc ON c.IdCuentaContable = cc.Id
                LEFT JOIN (
                    SELECT ConceptoEstandar, COUNT(*) AS Movimientos
                    FROM bancos.MovimientosArchivo
                    GROUP BY ConceptoEstandar
                ) m ON m.ConceptoEstandar = c.Nombre
                ORDER BY c.Nombre").ToList();
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Asignar/quitar la cuenta de un concepto
        public static void ActualizarCuentaConcepto(int idConcepto, int? idCuentaContable)
        {
            using var cn = DatabaseHelper.Open();
            cn.Execute("UPDATE bancos.ConceptosEstandar SET IdCuentaContable = @IdCuenta WHERE Id = @Id",
                new { IdCuenta = idCuentaContable, Id = idConcepto });
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Cuenta de cada concepto, para completar CuentaFinal
        /// <summary>
        /// Nombre del concepto (case-insensitive) → código de cuenta, o "" si no tiene
        /// asignada. Es lo que necesita HomologacionMatcher.EscribirCuenta para autocompletar
        /// CuentaFinal en el mismo momento en que se resuelve el concepto.
        /// </summary>
        public static Dictionary<string, string> ObtenerCuentasPorConcepto()
        {
            using var cn = DatabaseHelper.Open();
            return cn.Query(@"
                SELECT c.Nombre, cc.Cuenta
                FROM bancos.ConceptosEstandar c
                LEFT JOIN bancos.CuentasContables cc ON c.IdCuentaContable = cc.Id")
                .ToDictionary(x => (string)x.Nombre, x => (string)(x.Cuenta ?? ""),
                              StringComparer.OrdinalIgnoreCase);
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

            // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 2 - Extraido para reusarlo en el reapuntado
            int idConcepto = ObtenerOCrearConcepto(cn, tx, nombreConcepto);

            cn.Execute(@"
                DELETE FROM bancos.HomologacionConceptos
                WHERE IdPerfilBanco = @IdPerfilBanco AND ValorOriginal = @ValorOriginal",
                new { IdPerfilBanco = idPerfilBanco, ValorOriginal = valorOriginal }, tx);

            cn.Execute(@"
                INSERT INTO bancos.HomologacionConceptos (IdPerfilBanco, ValorOriginal, IdConceptoEstandar)
                VALUES (@IdPerfilBanco, @ValorOriginal, @IdConceptoEstandar)",
                new { IdPerfilBanco = idPerfilBanco, ValorOriginal = valorOriginal, IdConceptoEstandar = idConcepto }, tx);

            tx.Commit();
            return idConcepto;
        }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 2 - Buscar o crear el concepto estandar
        /// <summary>
        /// Id del concepto con ese nombre, creándolo si no existía. Va siempre dentro de la
        /// transacción del llamador: si se creara suelto y el resto fallara, quedaría un
        /// ConceptoEstandar huérfano sin ninguna homologación que lo use.
        /// </summary>
        private static int ObtenerOCrearConcepto(IDbConnection cn, IDbTransaction tx, string nombreConcepto)
        {
            int? id = cn.QueryFirstOrDefault<int?>(
                "SELECT Id FROM bancos.ConceptosEstandar WHERE LOWER(Nombre) = LOWER(@Nombre)",
                new { Nombre = nombreConcepto }, tx);

            return id ?? cn.QuerySingle<int>(
                "INSERT INTO bancos.ConceptosEstandar (Nombre) OUTPUT INSERTED.Id VALUES (@Nombre);",
                new { Nombre = nombreConcepto }, tx);
        }
    }
}
