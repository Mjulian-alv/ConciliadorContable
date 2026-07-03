namespace ArcaCliente.Models
{
    /// <summary>
    /// Sistemas contables a los que el conciliador offline puede exportar comprobantes.
    /// Cada valor activa el exportador correspondiente al guardar el perfil offline.
    /// </summary>
    public enum SistemaExportacionOffline
    {
        /// <summary>Sin exportación a sistema externo. Solo conciliación visual.</summary>
        Ninguno = 0,

        /// <summary>Sistema PRESEA. Genera el Excel de importación de compras.</summary>
        Presea = 1
    }
}
