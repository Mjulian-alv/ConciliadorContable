using System;
using System.Data;
using System.Threading.Tasks;
using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Configuración de la instalación Octosis requerida para dar de alta documentos.
    /// Equivale a las variables globales de <c>modCargaInstalacion</c> en wsmbcg2010.
    /// <para>
    /// Llamar a <see cref="InicializarAsync"/> una sola vez al seleccionar un perfil con
    /// <c>IntegracionHabilitada = true</c>, antes de invocar
    /// <c>OctosisDocumentoService.AltaAsync()</c>.
    /// </para>
    /// </summary>
    public static class OctosisConfig
    {
        // ?? Estado ????????????????????????????????????????????????????????????

        /// <summary>
        /// <c>true</c> una vez que <see cref="InicializarAsync"/> completó con éxito.
        /// <c>false</c> tras llamar a <see cref="Reset"/> o antes de la primera inicialización.
        /// </summary>
        public static bool Inicializado { get; private set; } = false;

        // ?? Propiedades configurables ?????????????????????????????????????????

        /// <summary>
        /// Código de 4 dígitos de la sucursal de la instalación.
        /// Equivale a <c>modCargaInstalacion.sucursal</c> en wsmbcg2010.
        /// Se usa en <c>idsqlsucur</c>, <c>DOC_SUCURS</c>, <c>TAS_SUCURS</c>
        /// y en la generación del NRO_INTERNO (<c>PV_D5_PARAM</c>).
        /// Poblado por <see cref="InicializarAsync"/> desde <c>sy_system.idsqlsucur</c>.
        /// </summary>
        public static string Sucursal { get; set; } = "0001";

        /// <summary>
        /// Identificador del usuario que ejecuta el alta.
        /// Se graba en el campo de auditoría <c>con_usugra</c> de cada INSERT.
        /// </summary>
        public static string UsuarioGrabacion { get; set; } = "ARCA";

        // ?? Constantes ????????????????????????????????????????????????????????

        /// <summary>
        /// Valor del campo <c>DOC_DOLARE</c> / <c>TAS_DOLARE</c> para documentos en moneda extranjera.
        /// Equivale a <c>marca_documento_en_dolares_si</c> en wsmbcg2010.
        /// </summary>
        public const string MarcaDolaresSi = "S";

        /// <summary>
        /// Valor del campo <c>DOC_DOLARE</c> / <c>TAS_DOLARE</c> para documentos en pesos.
        /// Equivale a <c>marca_documento_en_dolares_no</c> en wsmbcg2010.
        /// </summary>
        public const string MarcaDolaresNo = "N";

        // ?? Conexión por perfil ???????????????????????????????????????????????

        /// <summary>
        /// Retorna la fábrica de conexión efectiva para Octosis según el perfil.
        /// <para>
        /// Orden de precedencia:
        /// <list type="number">
        ///   <item><see cref="PerfilFiscal.OctosisConnectionFactory"/> si está asignada</item>
        ///   <item><see cref="PerfilFiscal.OctosisConnectionString"/> si tiene valor (crea SqlConnection)</item>
        ///   <item><see cref="PerfilFiscal.ConciliacionConnectionFactory"/> si está asignada</item>
        ///   <item><see cref="PerfilFiscal.ConciliacionConnectionString"/> si tiene valor (crea SqlConnection)</item>
        ///   <item><see cref="ConciliacionConfig.ConnectionFactory"/> como fallback</item>
        /// </list>
        /// </para>
        /// </summary>
        public static Func<IDbConnection> GetConnectionFactory(PerfilFiscal perfil)
        {
            // Primero intentar con configuración específica de Octosis
            if (perfil?.OctosisConnectionFactory != null)
                return perfil.OctosisConnectionFactory;

            if (!string.IsNullOrWhiteSpace(perfil?.OctosisConnectionString))
                return () => new Microsoft.Data.SqlClient.SqlConnection(perfil.OctosisConnectionString);

            // Fallback a configuración de conciliación (misma base)
            if (perfil?.ConciliacionConnectionFactory != null)
                return perfil.ConciliacionConnectionFactory;

            if (!string.IsNullOrWhiteSpace(perfil?.ConciliacionConnectionString))
                return () => new Microsoft.Data.SqlClient.SqlConnection(perfil.ConciliacionConnectionString);

            // Fallback final a configuración global
            return ConciliacionConfig.ConnectionFactory;
        }

        // ?? Ciclo de vida ?????????????????????????????????????????????????????

        /// <summary>
        /// Lee <c>idsqlsucur</c> de <c>sy_system</c> y actualiza <see cref="Sucursal"/>.
        /// Marca <see cref="Inicializado"/> = <c>true</c> si la lectura es exitosa.
        /// <para>
        /// Equivale a lo que hacía <c>modCargaInstalacion</c> al arrancar wsmbcg2010.
        /// </para>
        /// </summary>
        /// <param name="perfil">Perfil fiscal activo. Se usa para obtener la conexión a Octosis.</param>
        /// <exception cref="InvalidOperationException">
        /// Si <c>sy_system</c> no devuelve ninguna fila (base vacía o sin permisos).
        /// </exception>
        public static async Task InicializarAsync(PerfilFiscal perfil = null)
        {
            await Task.Run(() =>
            {
                using var conn = GetConnectionFactory(perfil)();
                conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT TOP 1 idsqlsucur FROM sy_system";

                var resultado = cmd.ExecuteScalar();

                if (resultado == null || resultado == DBNull.Value)
                    throw new InvalidOperationException(
                        "No se encontró la fila de configuración en sy_system. " +
                        "Verificar la cadena de conexión y los permisos de acceso.");

                Sucursal     = (resultado.ToString()?.Trim() ?? "0001").PadLeft(4, '0');
                Inicializado = true;
            });
        }

        /// <summary>
        /// Reinicia el estado de inicialización.
        /// Llamar cuando el usuario cambia de perfil o desactiva la integración.
        /// </summary>
        public static void Reset()
        {
            Inicializado = false;
            Sucursal     = "0001";
        }
    }
}
