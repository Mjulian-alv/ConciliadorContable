// Fecha: 06/10/2026 - TAREA: 00072 - Linea: 2 - Las operaciones individuales evitan borrar perfiles de otro usuario.
using ArcaCliente.Web.Models;
namespace ArcaCliente.Web.Data;
public interface IPyRProfileStore
{
    Task<List<WebPyRProfile>> Profiles();
    Task<WebPyRProfile> Save(WebPyRProfile value);
    Task Delete(WebPyRProfile value);
}
