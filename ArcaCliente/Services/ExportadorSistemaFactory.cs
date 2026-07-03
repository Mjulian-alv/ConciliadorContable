using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Crea el exportador concreto según el sistema configurado en el perfil offline.
    /// Para agregar un nuevo sistema: implementar <see cref="IExportadorSistema"/> y
    /// agregar un caso en <see cref="ObtenerExportador"/> y en <see cref="ObtenerNombre"/>.
    /// </summary>
    public static class ExportadorSistemaFactory
    {
        /// <summary>
        /// Retorna la instancia del exportador correspondiente al perfil,
        /// o <c>null</c> si el perfil no tiene sistema configurado.
        /// </summary>
        public static IExportadorSistema ObtenerExportador(PerfilOffline perfil) =>
            perfil.SistemaExportacion switch
            {
                SistemaExportacionOffline.Presea => new PreseaExportador(perfil.ConfigPresea ?? new ConfigPresea()),
                _                                => null
            };

        /// <summary>Retorna el nombre legible del sistema dado su enum.</summary>
        public static string ObtenerNombre(SistemaExportacionOffline sistema) =>
            sistema switch
            {
                SistemaExportacionOffline.Presea => "PRESEA",
                _                                => string.Empty
            };
    }
}
