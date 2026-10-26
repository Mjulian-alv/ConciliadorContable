// Fecha: 14/10/2026 - TAREA: 00097 - Linea: 1 - Catálogo único de permisos de la web: alimenta las políticas, el menú lateral y el inicio.
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
namespace Conciliador.Web.Security;
public static class Permisos
{
    public static readonly string[] Arca = { "ArcaOffline", "ArcaPerfiles", "ArcaEquivalencias", "ArcaPyR" };
    public static readonly string[] Agrupador = { "AgrProcesador", "AgrHomologaciones", "AgrConciliacion", "AgrConciliacionInterna", "AgrCuentasContables", "AgrClaveMaestra" }; // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 3 - AgrClaveMaestra: definir y cambiar la clave maestra del blindaje
    /// <summary>Una política por permiso: Admin o el permiso propio, como en el escritorio.</summary>
    public static void Registrar(AuthorizationOptions o)
    {
        foreach (var permiso in Arca.Concat(Agrupador))
            o.AddPolicy(permiso, p => p.RequireAuthenticatedUser().RequireAssertion(c => Tiene(c.User, permiso)));
    }
    public static bool Tiene(ClaimsPrincipal usuario, string permiso) => usuario.IsInRole("Admin") || usuario.HasClaim("permission", permiso);
}
