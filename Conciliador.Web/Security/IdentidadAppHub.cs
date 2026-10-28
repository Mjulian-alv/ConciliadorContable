// Fecha: 28/10/2026 - TAREA: 00140 - Linea: 2,3 - Si el pedido trae una identidad válida de AppHub abre la sesión de la web sin pedir login; sin clave configurada no hace nada. Va antes de la autenticación.
using System.Security.Claims;
using Conciliador.Web.Data;
using Microsoft.AspNetCore.Authentication;
namespace Conciliador.Web.Security;
public sealed class IdentidadAppHub(RequestDelegate next,IdentidadAppHubOptions o,UserStorage users,SessionRegistry sessions,ILogger<IdentidadAppHub> log)
{
    /// <summary>La pantalla de ingreso lee este valor para explicar por qué no se pudo entrar por AppHub: «rechazada» o «desactivado».</summary>
    public const string ItemAviso="AppHub.Aviso";
    public async Task InvokeAsync(HttpContext ctx)
    {
        if(!o.Habilitada||!ctx.Request.Headers.TryGetValue(TokenAppHub.Encabezado,out var valores)){await next(ctx);return;}
        var motivo="encabezado repetido";IdentidadToken? id=null;
        if(valores.Count==1)id=TokenAppHub.Validar(valores[0]??"",o,DateTimeOffset.UtcNow,out motivo);
        if(id==null){log.LogWarning("Identidad de AppHub rechazada: {Motivo}.",motivo);ctx.Items[ItemAviso]="rechazada";await next(ctx);return;}
        // Ya hay una sesión de esta misma persona abierta por AppHub: no se toca la base en cada pedido.
        var actual=await ctx.AuthenticateAsync("Cookies");
        if(actual.Succeeded&&actual.Principal!.HasClaim("via","apphub")&&string.Equals(actual.Principal.FindFirstValue("correo"),id.Correo,StringComparison.OrdinalIgnoreCase)){ctx.User=actual.Principal;await next(ctx);return;}
        IngresoMicrosoft r;
        try{r=await users.IngresarMicrosoftAsync(id.Oid,id.Correo,id.Nombre,o.EsAdministradorInicial(id.Correo));}
        catch(Exception ex)when(ex is InvalidOperationException||ex is Microsoft.Data.Sqlite.SqliteException){log.LogWarning("No se pudo resolver el usuario de AppHub: {Mensaje}",ex.Message);ctx.Items[ItemAviso]="rechazada";await next(ctx);return;}
        if(r.User==null){log.LogInformation("Ingreso por AppHub de un usuario desactivado.");ctx.Items[ItemAviso]="desactivado";await next(ctx);return;}
        if(r.Creado)log.LogInformation("Se creó un usuario desde AppHub{Admin}.",o.EsAdministradorInicial(id.Correo)?" como administrador inicial":" sin permisos");
        var principal=r.User.Principal(false);var identidad=(ClaimsIdentity)principal.Identity!;
        identidad.AddClaim(new Claim("via","apphub"));identidad.AddClaim(new Claim("correo",id.Correo));
        sessions.Register(principal);
        await ctx.SignInAsync("Cookies",principal);ctx.User=principal;
        await next(ctx);
    }
}
