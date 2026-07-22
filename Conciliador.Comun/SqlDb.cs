using Microsoft.Data.SqlClient;
using System.Configuration;

namespace Conciliador.Comun
{
    /// <summary>
    /// Punto único de conexión a la base común "Conciliador".
    /// La connection string se puede overridear vía App.config
    /// (appSetting "ConciliadorSqlConnectionString") o seteando la propiedad.
    /// </summary>
    public static class SqlDb
    {
        private const string Default =
            "Server=192.168.7.51;Database=Conciliador;User Id=conciliador;Password=DEFINIR;TrustServerCertificate=True;";

        public static string ConnectionString { get; set; } =
            ConfigurationManager.AppSettings["ConciliadorSqlConnectionString"] ?? Default;

        public static SqlConnection GetConnection() => new SqlConnection(ConnectionString);
    }
}
