namespace ArcaCliente.Models
{
    /// <summary>
    /// Sistemas contables con los que ArcaCliente puede integrarse
    /// para la descarga automática de comprobantes de proveedores.
    /// </summary>
    public enum SistemaIntegracion
    {
        /// <summary>Sin integración activa. Solo conciliación visual.</summary>
        Ninguno = 0,

        /// <summary>Sistema Octosis (wsmbcg2010). Tablas PV_D5_DOCUM / PV_P_TASAS0.</summary>
        Octosis = 1
    }
}
