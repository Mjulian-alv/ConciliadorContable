// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 3 - Cada acción comprueba permisos actuales, también en circuitos ya abiertos.
using Conciliador.Web.Data;
using Microsoft.AspNetCore.Components.Authorization;
namespace Conciliador.Web.Security;
public sealed record WebMode(bool Demo);
public sealed class AccessGuard(AuthenticationStateProvider authentication,UserStorage users,WebMode mode,SessionRegistry sessions)
{
    public async Task Require(string permission) {
        var p=(await authentication.GetAuthenticationStateAsync()).User;
        if(!sessions.IsActive(p)||p.Identity?.IsAuthenticated!=true||!long.TryParse(p.FindFirst("expires")?.Value,out var expires)||DateTimeOffset.UtcNow.ToUnixTimeSeconds()>=expires)throw new UnauthorizedAccessException("La sesión venció. Volvé a ingresar.");
        if(mode.Demo&&p.FindFirst("demo")?.Value=="True")return;
        var user=await users.Find(p.Identity.Name??"");
        if(user==null||!user.Allows(permission))throw new UnauthorizedAccessException("Tu usuario no tiene permiso para esta operación.");
    }
}

