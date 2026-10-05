// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 3 - No crea usuarios ni modifica credenciales de producción.
using Conciliador.Web.Data;
using Conciliador.Web.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Conciliador.Web.Pages.Account;
[AllowAnonymous]
public sealed class LoginModel(UserStorage users,WebMode mode,SessionRegistry sessions,ILogger<LoginModel> logger) : PageModel
{
    public bool Demo=>mode.Demo;public string? Error{get;set;}
    [BindProperty]public string Username{get;set;}="";[BindProperty]public string Password{get;set;}="";
    public async Task<IActionResult> OnPostAsync(){
        WebUser? user=null;
        try{user=Demo?new WebUser("demo","Demostración local","Admin",new()):await users.ValidateAsync(Username.Trim(),Password);}catch(Exception ex){logger.LogError(ex,"No se pudo consultar la base de usuarios.");}
        if(user==null||!user.Allows("ArcaOffline")){Error="No se pudo ingresar. Revisá tus credenciales y el permiso ARCA offline.";return Page();}
        var principal=user.Principal(Demo);sessions.Register(principal);await HttpContext.SignInAsync("Cookies",principal);return LocalRedirect("/");
    }
}

