using Microsoft.Data.SqlClient;
using System.Configuration;

namespace Conciliador.Comun
{
    /// <summary>
    /// Punto único de conexión a la base común "alvesu".
    /// La connection string se puede overridear vía App.config
    /// (appSetting "ConciliadorSqlConnectionString") o seteando la propiedad.
    /// </summary>
    public static class SqlDb
    {
        // Fallback sin credencial: este archivo se versiona en un repo público.
        // La password real va en ConciliadorContable/secrets.config (ignorado por git).
        private const string Default =
            "Server=192.168.7.51;Database=alvesu;User Id=conciliador;Password=DEFINIR;TrustServerCertificate=True;";

        public static string ConnectionString { get; set; } =
            ConfigurationManager.AppSettings["ConciliadorSqlConnectionString"] ?? Default;

        public static SqlConnection GetConnection() => new SqlConnection(ConnectionString);
    }

    public delegate void Notify();

    public class ClaseBase
    {

        public event EventHandler<mensajeEventArgs> Mensajes;
        public string VBCrLf = Environment.NewLine;


        protected virtual void EnvioMensaje(mensajeEventArgs e)
        {
            Mensajes?.Invoke(this, e);
        }

    }


    public class mensajeEventArgs : EventArgs
    {
        public bool IsSuccessful { get; set; }
        public DateTime CompletionTime { get; set; }
        public string mensaje { get; set; }
        public mensajeEventArgs(bool pIsSuccessful = false, DateTime pCompletionTime = new DateTime(), string pmensaje = "")
        {
            IsSuccessful = pIsSuccessful;
            CompletionTime = pCompletionTime;
            mensaje = pmensaje;
        }
    }
}
