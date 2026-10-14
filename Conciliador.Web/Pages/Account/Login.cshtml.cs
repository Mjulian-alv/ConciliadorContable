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
        // Fecha: 06/10/2026 - TAREA: 00072 - Linea: 1 - Un usuario exclusivo de PyR debe poder ingresar sin permiso de comprobantes.
        // Fecha: 14/10/2026 - TAREA: 00097 - Linea: 1 - Ingresa cualquier usuario válido: el menú muestra lo que sus permisos habilitan (también un usuario sólo del Agrupador).
        if(user==null){Error="No se pudo ingresar. Revisá tus credenciales.";return Page();}
        var principal=user.Principal(Demo);sessions.Register(principal);await HttpContext.SignInAsync("Cookies",principal);return LocalRedirect("/");
    }
}
