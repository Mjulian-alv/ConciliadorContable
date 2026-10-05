// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 2 - Las versiones evitan pisar cambios de otra sesión.
using ArcaCliente.Web.Models;
namespace ArcaCliente.Web.Data;
public interface IWebConfigurationStore
{
    Task<List<WebProfile>> Profiles();
    Task<WebProfile> Save(WebProfile profile);
    Task<ProviderConfiguration> Provider(string cuit);
    Task<ProviderConfiguration> Save(ProviderConfiguration provider);
}
