// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 3 - El cierre revoca circuitos abiertos; reiniciar el host exige un nuevo ingreso.
using System.Collections.Concurrent;
using System.Security.Claims;
namespace Conciliador.Web.Security;
public sealed class SessionRegistry
{
    readonly ConcurrentDictionary<string,long> active=new();
    public void Register(ClaimsPrincipal p){foreach(var item in active.Where(x=>x.Value<=DateTimeOffset.UtcNow.ToUnixTimeSeconds()))active.TryRemove(item.Key,out _);active[p.FindFirst("session")!.Value]=long.Parse(p.FindFirst("expires")!.Value);}
    public bool IsActive(ClaimsPrincipal p)=>active.TryGetValue(p.FindFirst("session")?.Value??"",out var expires)&&expires>DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public void Revoke(ClaimsPrincipal p)=>active.TryRemove(p.FindFirst("session")?.Value??"",out _);
}
