using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace ArcaCliente.Services
{
    public readonly record struct ConnectionTestResult(bool Exitoso, string Mensaje);

    /// <summary>
    /// Prueba una connection string contra la base de conciliación o contra Octosis.
    /// Antes vivía inline en FormPerfilDetalle, con SqlConnection abierta directo desde
    /// el handler del botón: era el único SQL crudo del módulo fuera de Services/.
    /// No muestra UI: devuelve el resultado, el Form arma el mensaje.
    /// </summary>
    public static class ConnectionTester
    {
        public static Task<ConnectionTestResult> ProbarConciliacion(string connString) =>
            Task.Run(() => Probar(connString, ProbarConciliacionCore));

        public static Task<ConnectionTestResult> ProbarOctosis(string connString) =>
            Task.Run(() => Probar(connString, ProbarOctosisCore));

        private static ConnectionTestResult Probar(string connString, Action<SqlConnection> core)
        {
            try
            {
                using var conn = new SqlConnection(connString);
                conn.Open();
                core(conn);
                return new ConnectionTestResult(true, null);
            }
            catch (Exception ex)
            {
                return new ConnectionTestResult(false, ex.Message);
            }
        }

        private static void ProbarConciliacionCore(SqlConnection conn)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1";
            cmd.ExecuteScalar();
        }

        /// <summary>Valida además que sea una base de Octosis probando la tabla sy_system.</summary>
        private static void ProbarOctosisCore(SqlConnection conn)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT TOP 1 idsqlsucur FROM sy_system";
            var result = cmd.ExecuteScalar();

            if (result == null || result == DBNull.Value)
                throw new InvalidOperationException(
                    "Conexión exitosa pero no se encontró configuración en sy_system. ¿Es una base de Octosis?");
        }
    }
}
