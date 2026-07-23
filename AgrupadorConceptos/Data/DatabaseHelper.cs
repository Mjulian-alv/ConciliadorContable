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
        /// Garantiza la inicialización del schema (idempotente), así cualquier
        /// punto de entrada al módulo queda cubierto sin depender de que cada
        /// form nuevo recuerde llamar a InitializeDatabase() explícitamente.
        /// </summary>
        public static SqlConnection GetConnection()
        {
            InitializeDatabase();
            return SqlDb.GetConnection();
        }

        /// <summary>
        /// Crea schema/tablas/índices si no existen. Run-once por proceso:
        /// idempotente gracias al flag _inicializada.
        /// </summary>
        public static void InitializeDatabase()
        {
            if (_inicializada) return;
            using var cn = SqlDb.GetConnection();
            cn.Open();
            try
            {
                cn.Execute(SqlSchema.Ddl);
            }
            catch (SqlException ex) when (ex.Number == 2714)
            {
                // Ventana de carrera entre el chequeo IF OBJECT_ID(...) IS NULL y el
                // CREATE TABLE: dos instalaciones abriendo el módulo por primera vez
                // casi simultáneamente contra una base recién provisionada pueden
                // pasar ambas el chequeo antes de que la otra confirme el CREATE.
                // El objeto ya existe (creado por el proceso concurrente) => no-op.
            }
            _inicializada = true;
        }
    }
}
