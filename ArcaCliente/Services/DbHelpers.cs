using System;
using System.Data;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Helpers de bajo nivel para ADO.NET, compartidos por los servicios de acceso a
    /// Octosis y de conciliación. Antes cada servicio tenía su propio AddParameter
    /// privado; dos de las cinco copias no protegían contra null (asignaban
    /// IDataParameter.Value = null directo en vez de DBNull.Value), que en SqlParameter
    /// puede comportarse distinto según el driver.
    /// </summary>
    internal static class DbHelpers
    {
        public static void AddParameter(IDbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value         = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }
    }
}
