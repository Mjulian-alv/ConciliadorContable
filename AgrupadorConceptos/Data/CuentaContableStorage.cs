// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 1 - Acceso a bancos.CuentasContables
using System.Collections.Generic;
using System.Linq;
using AgrupadorConceptos.Models;
using Dapper;

namespace AgrupadorConceptos.Data
{
    /// <summary>
    /// Acceso a bancos.CuentasContables: el catálogo importado del sistema legacy.
    /// </summary>
    internal static class CuentaContableStorage
    {
        public static List<CuentaContable> ObtenerTodas()
        {
            using var cn = DatabaseHelper.Open();
            return cn.Query<CuentaContable>(
                "SELECT * FROM bancos.CuentasContables ORDER BY Cuenta, CentroCosto").ToList();
        }

        public static CuentaContable ObtenerPorId(int id)
        {
            using var cn = DatabaseHelper.Open();
            return cn.QueryFirstOrDefault<CuentaContable>(
                "SELECT * FROM bancos.CuentasContables WHERE Id = @Id", new { Id = id });
        }

        /// <summary>
        /// Alta o actualización por la clave natural (Cuenta, CentroCosto): reimportar el
        /// mismo export del legacy no duplica filas, sólo actualiza la descripción.
        /// </summary>
        public static int UpsertLote(IEnumerable<CuentaContable> cuentas)
        {
            var lista = cuentas?.ToList() ?? new List<CuentaContable>();
            if (lista.Count == 0) return 0;

            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();

            foreach (var c in lista)
            {
                cn.Execute(@"
                    MERGE bancos.CuentasContables AS destino
                    USING (SELECT @Cuenta AS Cuenta, @CentroCosto AS CentroCosto) AS origen
                    ON destino.Cuenta = origen.Cuenta
                       AND ISNULL(destino.CentroCosto, N'') = ISNULL(origen.CentroCosto, N'')
                    WHEN MATCHED THEN
                        UPDATE SET Descripcion = @Descripcion
                    WHEN NOT MATCHED THEN
                        INSERT (Cuenta, Descripcion, CentroCosto)
                        VALUES (@Cuenta, @Descripcion, @CentroCosto);",
                    new { c.Cuenta, c.Descripcion, c.CentroCosto }, tx);
            }

            tx.Commit();
            return lista.Count;
        }
    }
}
