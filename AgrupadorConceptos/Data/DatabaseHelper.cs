using Conciliador.Comun;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AgrupadorConceptos.Data
{
    public static class DatabaseHelper
    {
        private static bool _inicializada;

        /// <summary>
        /// Conexión a la base común Conciliador (schema bancos).
        /// Mismo contrato que antes: se devuelve cerrada.
        /// </summary>
        public static SqlConnection GetConnection() => SqlDb.GetConnection();

        /// <summary>
        /// Crea schema/tablas/índices si no existen. Run-once por proceso:
        /// el shell la llama antes de abrir cada form del módulo.
        /// </summary>
        public static void InitializeDatabase()
        {
            if (_inicializada) return;
            using var cn = GetConnection();
            cn.Open();
            cn.Execute(SqlSchema.Ddl);
            _inicializada = true;
        }
    }
}
