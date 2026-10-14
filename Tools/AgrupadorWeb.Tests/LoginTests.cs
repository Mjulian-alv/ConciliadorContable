// Fecha: 14/10/2026 - TAREA: 00097 - Linea: 1 - Cualquier usuario válido ingresa y aterriza en el menú; lo que ve depende de sus permisos.
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Conciliador.Web.Data;
using Conciliador.Web.Pages.Account;
using Conciliador.Web.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
static class LoginTests
{
    sealed class CaptureAuth : IAuthenticationService
    {
        public bool SignedIn;
        public Task<AuthenticateResult> AuthenticateAsync(HttpContext c, string? s) => Task.FromResult(AuthenticateResult.NoResult());
        public Task ChallengeAsync(HttpContext c, string? s, AuthenticationProperties? p) => Task.CompletedTask;
        public Task ForbidAsync(HttpContext c, string? s, AuthenticationProperties? p) => Task.CompletedTask;
        public Task SignInAsync(HttpContext c, string? s, ClaimsPrincipal user, AuthenticationProperties? p) { SignedIn = true; return Task.CompletedTask; }
        public Task SignOutAsync(HttpContext c, string? s, AuthenticationProperties? p) => Task.CompletedTask;
    }
    static string Hash(string clave) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(clave))).ToLowerInvariant();
    static (bool Entro, IActionResult Respuesta) Ingresar(string db, string usuario, string clave)
    {
        var captura = new CaptureAuth();
        using var servicios = new ServiceCollection().AddSingleton<IAuthenticationService>(captura).BuildServiceProvider();
        var modelo = new LoginModel(new UserStorage(db), new WebMode(false), new SessionRegistry(), NullLogger<LoginModel>.Instance)
        { Username = usuario, Password = clave, PageContext = new PageContext { HttpContext = new DefaultHttpContext { RequestServices = servicios } } };
        var r = modelo.OnPostAsync().GetAwaiter().GetResult();
        return (captura.SignedIn, r);
    }
    public static void Run(Action<string, Action> test)
    {
        test("Login: ingresa cualquier usuario válido (sólo PyR, sólo Agrupador o sin permisos) y va al menú", () =>
        {
            var db = Path.Combine(Path.GetTempPath(), "agr-users-" + Guid.NewGuid() + ".db");
            try
            {
                using (var c = new SqliteConnection("Data Source=" + db))
                {
                    c.Open(); using var cmd = c.CreateCommand();
                    cmd.CommandText = "CREATE TABLE Usuarios(Username TEXT,Nombre TEXT,Rol TEXT,PermisosJson TEXT,PasswordHash TEXT,Activo INTEGER);" +
                        "INSERT INTO Usuarios VALUES('pyr','Solo PyR','Usuario','[\"ArcaPyR\"]',@H,1),('agr','Solo Agrupador','Usuario','[\"AgrProcesador\"]',@H,1),('nada','Sin permisos','Usuario','[]',@H,1),('baja','De baja','Usuario','[]',@H,0)";
                    cmd.Parameters.AddWithValue("@H", Hash("prueba")); cmd.ExecuteNonQuery();
                }
                foreach (var u in new[] { "pyr", "agr", "nada" })
                {
                    var (entro, r) = Ingresar(db, u, "prueba");
                    if (!entro || r is not LocalRedirectResult red || red.Url != "/") throw new Exception($"{u}: no ingresó o no fue al menú ({r.GetType().Name})");
                }
                if (Ingresar(db, "agr", "otra").Entro) throw new Exception("Aceptó una contraseña incorrecta");
                if (Ingresar(db, "baja", "prueba").Entro) throw new Exception("Aceptó un usuario dado de baja");
            }
            finally { SqliteConnection.ClearAllPools(); if (File.Exists(db)) File.Delete(db); }
        });
    }
}
