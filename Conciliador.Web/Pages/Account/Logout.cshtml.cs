// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 3 - Invalida la cookie de esta sesión.
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Conciliador.Web.Pages.Account;
public sealed class LogoutModel(Conciliador.Web.Security.SessionRegistry sessions) : PageModel { public async Task<IActionResult> OnPostAsync(){sessions.Revoke(User);await HttpContext.SignOutAsync("Cookies");return LocalRedirect("/account/login");} }

