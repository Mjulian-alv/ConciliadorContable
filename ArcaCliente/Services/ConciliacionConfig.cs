using System;
using System.Configuration;
using System.Data;
using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Configuración de la conciliación con la base de datos local (Octosis).
    /// Modificá ConnectionString, Query y ConnectionFactory según tu entorno.
    /// Los valores por perfil tienen precedencia; si no están configurados se usan estos defaults.
    /// </summary>
    public static class ConciliacionConfig
    {
        // ── 1. Cadena de conexión (default) ─────────────────────────────────────
        // La credencial real NO va acá: este archivo se versiona en un repo público.
        // Se toma del appSetting "OctosisConnectionString", que se define en
        // ConciliadorContable/secrets.config (ignorado por git). Sin ese archivo queda
        // el placeholder y la conexión falla, que es el comportamiento buscado en una
        // instalación sin configurar.
        private const string Default =
            "Server=192.168.7.51;Database=Logistica;User Id=octosis;Password=DEFINIR;TrustServerCertificate=True;";

        public static string ConnectionString { get; set; } =
            ConfigurationManager.AppSettings["OctosisConnectionString"] ?? Default;

        // ── 2. Query (default) ───────────────────────────────────────────────────
        // Columnas requeridas por ConciliacionService:
        //   Fecha      (DateTime) – fecha del comprobante        → doc_fecemi
        //   PuntoVenta (string)   – punto de venta               → doc_nrosuc
        //   Numero     (string)   – número del comprobante        → doc_nrodoc
        //   Total      (decimal)  – importe total del documento   → doc_impaus
        //
        // Columna adicional incluida pero no usada en el matching actual:
        //   Sucursal   (string)   – sucursal del sistema          → doc_sucurs
        //   (útil para entornos multi-sucursal; filtrar por OctosisConfig.Sucursal
        //    si se quiere conciliar una sola sucursal a la vez)
        //
        // Parámetros disponibles: @FechaDesde y @FechaHasta (ambos DateTime)
        public static string Query { get; set; } = @"
             SELECT
                 doc_fecemi AS Fecha,
                 doc_nrosuc AS PuntoVenta,
                 doc_nrodoc AS Numero,
                 isnull(doc_impaus,0.00) AS Total,
                 doc_sucurs AS Sucursal,
                 PRO_CUIT   AS Cuit,
	             PRO_NOMBRE AS Nombre,
                 doc_tipdoc AS TipoComprobante
             FROM pv_d5_docum
             left join PV_D3_PROVE on DOC_PROVEE=PRO_NUMERO
             WHERE doc_fecemi BETWEEN @FechaDesde AND @FechaHasta
             AND (pv_d5_docum.DELETEADO IS NULL OR pv_d5_docum.DELETEADO != 'X')
             AND doc_tipdoc IN ('1', '2', '3')
             AND doc_minuta = ''
             UNION ALL
             SELECT
                 doc_fecemi AS Fecha,
                 doc_nrosuc AS PuntoVenta,
                 doc_nrodoc AS Numero,
                  isnull(doc_impaus,0.00) AS Total,
                 doc_sucurs AS Sucursal,
                 PRO_CUIT   AS Cuit,
	             PRO_NOMBRE AS Nombre,
                 doc_tipdoc AS TipoComprobante
             FROM PV_D5_DOCLI
             left join PV_D3_PROLI on DOC_PROVEE=PRO_NUMERO
             WHERE doc_fecemi BETWEEN @FechaDesde AND @FechaHasta
             AND (PV_D5_DOCLI.DELETEADO IS NULL OR PV_D5_DOCLI.DELETEADO != 'X')
             AND doc_tipdoc IN ('1', '2', '3')
             AND doc_minuta = ''
            ";

        // ── 3. Fábrica de conexión (default) ─────────────────────────────────────
        // Reemplazá con el driver de tu base de datos. Ejemplos:
        //   SQL Server  : () => new Microsoft.Data.SqlClient.SqlConnection(ConnectionString)
        //   MySQL       : () => new MySql.Data.MySqlClient.MySqlConnection(ConnectionString)
        //   PostgreSQL  : () => new Npgsql.NpgsqlConnection(ConnectionString)
        //   ODBC        : () => new System.Data.Odbc.OdbcConnection(ConnectionString)
        public static Func<IDbConnection> ConnectionFactory { get; set; } = () => new Microsoft.Data.SqlClient.SqlConnection(ConnectionString);

        // ── 4. Resolución efectiva por perfil ────────────────────────────────────

        /// <summary>
        /// Retorna la fábrica de conexión y el query efectivos para el perfil dado.
        /// <list type="bullet">
        ///   <item><description>
        ///     <b>ConnectionFactory</b>: usa <see cref="PerfilFiscal.ConciliacionConnectionFactory"/> si está
        ///     asignada; si no, construye una <c>SqlConnection</c> con
        ///     <see cref="PerfilFiscal.ConciliacionConnectionString"/> del perfil (si tiene valor);
        ///     en caso contrario usa <see cref="ConnectionFactory"/>.
        ///   </description></item>
        ///   <item><description>
        ///     <b>Query</b>: usa <see cref="PerfilFiscal.ConciliacionQuery"/> del perfil si tiene valor;
        ///     en caso contrario usa <see cref="Query"/>.
        ///   </description></item>
        /// </list>
        /// </summary>
        public static (string ConnectionString, Func<IDbConnection> ConnectionFactory, string Query) GetEffective(PerfilFiscal perfil)
        {
            string connectionString = !string.IsNullOrWhiteSpace(perfil?.ConciliacionConnectionString)
                ? perfil.ConciliacionConnectionString
                : ConnectionString;

            Func<IDbConnection> factory;

            if (perfil?.ConciliacionConnectionFactory != null)
                factory = perfil.ConciliacionConnectionFactory;
            else if (!string.IsNullOrWhiteSpace(perfil?.ConciliacionConnectionString))
                factory = () => new Microsoft.Data.SqlClient.SqlConnection(perfil.ConciliacionConnectionString);
            else
                factory = ConnectionFactory;

            string query = !string.IsNullOrWhiteSpace(perfil?.ConciliacionQuery)
                ? perfil.ConciliacionQuery
                : Query;

            return (connectionString, factory, query);
        }
    }
}
