// Fecha: 28/10/2026 - TAREA: 00140 - Linea: 2,3 - Pruebas de la identidad de AppHub: token, usuarios de Microsoft y middleware. Cubre el checklist del contrato (Docs/Contrato-Apps.md de AppHub).
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Conciliador.Web.Data;
using Conciliador.Web.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
static class IdentidadAppHubTests
{
    const string Clave = "clave-compartida-de-prueba-0123456789-ABCDEF";
    static readonly IdentidadAppHubOptions Opciones = new("conciliador", Clave, new[] { "jefe@alvear.test" }, 30);
    static string B64(byte[] b) => Convert.ToBase64String(b).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    static string Emitir(Dictionary<string, object?>? cambios = null, string clave = Clave, string alg = "HS256", int largoFirma = 0)
    {
        var ahora = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var carga = new Dictionary<string, object?> { ["iss"] = "AppHub", ["aud"] = "conciliador", ["sub"] = "oid-1", ["email"] = "ana@alvear.test", ["name"] = "Ana López", ["iat"] = ahora, ["exp"] = ahora + 60, ["jti"] = Guid.NewGuid().ToString() };
        foreach (var (k, v) in cambios ?? new()) { if (v == null) carga.Remove(k); else carga[k] = v; }
        var h = B64(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { alg, typ = "JWT" })));
        var p = B64(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(carga)));
        var firma = alg == "none" ? "" : B64(HMACSHA256.HashData(Encoding.UTF8.GetBytes(clave), Encoding.ASCII.GetBytes(h + "." + p)));
        return h + "." + p + "." + firma;
    }
    sealed class Auth : IAuthenticationService
    {
        public ClaimsPrincipal? Cookie; public ClaimsPrincipal? Emitida;
        public Task<AuthenticateResult> AuthenticateAsync(HttpContext c, string? s) => Task.FromResult(Cookie == null ? AuthenticateResult.NoResult() : AuthenticateResult.Success(new AuthenticationTicket(Cookie, "Cookies")));
        public Task ChallengeAsync(HttpContext c, string? s, AuthenticationProperties? p) => Task.CompletedTask;
        public Task ForbidAsync(HttpContext c, string? s, AuthenticationProperties? p) => Task.CompletedTask;
        public Task SignInAsync(HttpContext c, string? s, ClaimsPrincipal user, AuthenticationProperties? p) { Emitida = user; return Task.CompletedTask; }
        public Task SignOutAsync(HttpContext c, string? s, AuthenticationProperties? p) => Task.CompletedTask;
    }
    static string NuevaBase(bool conColumnas, params string[] filas)
    {
        var db = Path.Combine(Path.GetTempPath(), "agr-ident-" + Guid.NewGuid() + ".db");
        using var c = new SqliteConnection("Data Source=" + db); c.Open(); using var cmd = c.CreateCommand();
        cmd.CommandText = "CREATE TABLE Usuarios(Id INTEGER PRIMARY KEY AUTOINCREMENT,Username TEXT NOT NULL UNIQUE COLLATE NOCASE,PasswordHash TEXT NOT NULL,Nombre TEXT NOT NULL DEFAULT '',Activo INTEGER NOT NULL DEFAULT 1,Rol TEXT NOT NULL DEFAULT 'Usuario',PermisosJson TEXT NOT NULL DEFAULT '[]'" + (conColumnas ? ",Correo TEXT NULL,OidMicrosoft TEXT NULL" : "") + ");" + string.Join(";", filas);
        cmd.ExecuteNonQuery(); return db;
    }
    static T Una<T>(string db, string sql) { using var c = new SqliteConnection("Data Source=" + db); c.Open(); using var cmd = c.CreateCommand(); cmd.CommandText = sql; return (T)Convert.ChangeType(cmd.ExecuteScalar()!, typeof(T)); }
    static void Limpiar(string db) { SqliteConnection.ClearAllPools(); if (File.Exists(db)) File.Delete(db); }
    static (bool Siguio, Auth Auth, HttpContext Ctx) Pasar(string db, string? token, Auth? auth = null, IdentidadAppHubOptions? o = null, int repetido = 1)
    {
        auth ??= new(); var siguio = false;
        var mw = new IdentidadAppHub(_ => { siguio = true; return Task.CompletedTask; }, o ?? Opciones, new UserStorage(db), new SessionRegistry(), NullLogger<IdentidadAppHub>.Instance);
        using var sp = new ServiceCollection().AddSingleton<IAuthenticationService>(auth).BuildServiceProvider();
        var ctx = new DefaultHttpContext { RequestServices = sp };
        if (token != null) for (var i = 0; i < repetido; i++) ctx.Request.Headers.Append(TokenAppHub.Encabezado, token);
        mw.InvokeAsync(ctx).GetAwaiter().GetResult();
        return (siguio, auth, ctx);
    }
    public static void Run(Action<string, Action> test)
    {
        // ---------- token ----------
        test("AppHub token: uno correcto se acepta y trae correo, nombre y oid", () =>
        {
            var t = TokenAppHub.Validar(Emitir(), Opciones, DateTimeOffset.UtcNow, out var m) ?? throw new Exception("lo rechazó: " + m);
            if (t.Correo != "ana@alvear.test" || t.Nombre != "Ana López" || t.Oid != "oid-1") throw new Exception("datos incorrectos");
        });
        test("AppHub token: se rechazan firma mala, alg=none, otro algoritmo, otro destinatario, otro emisor y formato roto", () =>
        {
            var casos = new Dictionary<string, string>
            {
                ["firma de otra clave"] = Emitir(clave: "otra-clave-completamente-distinta-0123456789-XYZ"),
                ["alg none"] = Emitir(alg: "none"),
                ["alg HS512"] = Emitir(alg: "HS512"),
                ["aud de otra app"] = Emitir(new() { ["aud"] = "ofertas" }),
                ["iss distinto"] = Emitir(new() { ["iss"] = "otro" }),
                ["dos partes"] = "aaa.bbb",
                ["vacío"] = "",
                ["gigante"] = new string('a', 5000) + "." + "b" + "." + "c",
            };
            foreach (var (nombre, token) in casos)
                if (TokenAppHub.Validar(token, Opciones, DateTimeOffset.UtcNow, out _) != null) throw new Exception("aceptó: " + nombre);
        });
        test("AppHub token: vencido, emitido en el futuro y sin correo o con correo inválido se rechazan; la tolerancia vale", () =>
        {
            var ahora = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (TokenAppHub.Validar(Emitir(new() { ["iat"] = ahora - 200, ["exp"] = ahora - 100 }), Opciones, DateTimeOffset.UtcNow, out var m) != null || m != "vencido") throw new Exception("aceptó un token vencido");
            if (TokenAppHub.Validar(Emitir(new() { ["iat"] = ahora - 100, ["exp"] = ahora - 10 }), Opciones, DateTimeOffset.UtcNow, out _) == null) throw new Exception("la tolerancia de 30 s no se aplicó");
            if (TokenAppHub.Validar(Emitir(new() { ["iat"] = ahora + 600, ["exp"] = ahora + 700 }), Opciones, DateTimeOffset.UtcNow, out _) != null) throw new Exception("aceptó un token del futuro");
            foreach (var correo in new object?[] { null, "", "sin-arroba", "a@@b", "Ana <ana@alvear.test>" })
                if (TokenAppHub.Validar(Emitir(new() { ["email"] = correo }), Opciones, DateTimeOffset.UtcNow, out _) != null) throw new Exception("aceptó el correo " + correo);
        });
        test("AppHub configuración: clave vacía = modo autónomo; clave corta o sin nombre de app no arranca", () =>
        {
            IdentidadAppHubOptions Cfg(params (string, string?)[] v) => IdentidadAppHubOptions.Desde(new ConfigurationBuilder().AddInMemoryCollection(v.Select(x => new KeyValuePair<string, string?>(x.Item1, x.Item2))).Build());
            if (Cfg(("AppHub:Aplicacion", "conciliador")).Habilitada) throw new Exception("sin clave debería estar apagada");
            if (Cfg().Habilitada) throw new Exception("sin sección debería estar apagada");
            try { Cfg(("AppHub:Aplicacion", "conciliador"), ("AppHub:ClaveCompartida", "corta")); throw new Exception("aceptó una clave corta"); } catch (InvalidOperationException) { }
            try { Cfg(("AppHub:ClaveCompartida", Clave)); throw new Exception("aceptó la clave sin nombre de app"); } catch (InvalidOperationException) { }
            if (!Cfg(("AppHub:Aplicacion", "conciliador"), ("AppHub:ClaveCompartida", Clave), ("AppHub:AdministradoresIniciales:0", "Jefe@Alvear.test")).EsAdministradorInicial("jefe@alvear.test")) throw new Exception("la lista de administradores no ignora mayúsculas");
        });
        // ---------- usuarios ----------
        test("AppHub usuarios: el primer ingreso crea un usuario sin permisos ni contraseña utilizable, y agrega las columnas que faltan", () =>
        {
            var db = NuevaBase(false);
            try
            {
                var r = new UserStorage(db).IngresarMicrosoftAsync("oid-1", "ana@alvear.test", "Ana López", false).GetAwaiter().GetResult();
                if (r.User == null || !r.Creado || r.User.Role != "Usuario" || r.User.Permissions.Count != 0 || r.User.Username != "ana") throw new Exception("no creó el usuario esperado");
                if (Una<string>(db, "SELECT PasswordHash FROM Usuarios WHERE Username='ana'") != "!") throw new Exception("la contraseña no quedó en el marcador");
                if (Una<string>(db, "SELECT OidMicrosoft FROM Usuarios WHERE Username='ana'") != "oid-1") throw new Exception("no guardó el oid");
                foreach (var clave in new[] { "!", "", "ana", "cualquiera" })
                    if (new UserStorage(db).ValidateAsync("ana", clave).GetAwaiter().GetResult() != null) throw new Exception("entró por el login de contraseña con «" + clave + "»");
            }
            finally { Limpiar(db); }
        });
        test("AppHub usuarios: la lista inicial crea administrador sólo al crear; el segundo ingreso reconoce por oid sin duplicar", () =>
        {
            var db = NuevaBase(true);
            try
            {
                var s = new UserStorage(db);
                var a = s.IngresarMicrosoftAsync("oid-j", "jefe@alvear.test", "Jefe", true).GetAwaiter().GetResult();
                if (a.User?.Role != "Admin") throw new Exception("no quedó como administrador");
                var b = s.IngresarMicrosoftAsync("oid-j", "otro.correo@alvear.test", "Jefe", false).GetAwaiter().GetResult();
                if (b.Creado || b.User?.Username != "jefe" || b.User.Role != "Admin") throw new Exception("no lo reconoció por oid");
                if (Una<long>(db, "SELECT COUNT(*) FROM Usuarios") != 1) throw new Exception("duplicó el usuario");
            }
            finally { Limpiar(db); }
        });
        test("AppHub usuarios: un usuario existente se vincula por correo y conserva rol, permisos y contraseña; una lista inicial no lo asciende", () =>
        {
            var db = NuevaBase(true, "INSERT INTO Usuarios(Username,PasswordHash,Nombre,Rol,PermisosJson,Correo) VALUES('lfernandez','hash-viejo','Luis','Usuario','[\"ArcaPyR\"]','Lfernandez@Alvear.Test')");
            try
            {
                var r = new UserStorage(db).IngresarMicrosoftAsync("oid-l", "lfernandez@alvear.test", "Luis F.", true).GetAwaiter().GetResult();
                if (r.User == null || r.Creado || r.User.Username != "lfernandez" || r.User.Role != "Usuario" || !r.User.Permissions.Contains("ArcaPyR")) throw new Exception("no lo vinculó o le cambió el rol");
                if (Una<string>(db, "SELECT OidMicrosoft FROM Usuarios WHERE Username='lfernandez'") != "oid-l") throw new Exception("no guardó el oid");
                if (Una<string>(db, "SELECT PasswordHash FROM Usuarios WHERE Username='lfernandez'") != "hash-viejo") throw new Exception("tocó la contraseña");
                if (Una<string>(db, "SELECT Nombre FROM Usuarios WHERE Username='lfernandez'") != "Luis") throw new Exception("pisó el nombre cargado");
            }
            finally { Limpiar(db); }
        });
        test("AppHub usuarios: no se vincula por parecido de nombre ni por un correo que ya es de otra cuenta de Microsoft", () =>
        {
            var db = NuevaBase(true, "INSERT INTO Usuarios(Username,PasswordHash,Nombre,Rol,PermisosJson,Correo,OidMicrosoft) VALUES('mjulian','h','Matías','Admin','[]','mjulian@alvear.test','oid-original')",
                                     "INSERT INTO Usuarios(Username,PasswordHash,Nombre,Rol,PermisosJson) VALUES('mgomez','h','María','Admin','[]')");
            try
            {
                var s = new UserStorage(db);
                var r = s.IngresarMicrosoftAsync("oid-n", "mgomez@alvear.test", "Otra María", false).GetAwaiter().GetResult();
                if (!r.Creado || r.User?.Role != "Usuario" || r.User.Username == "mgomez") throw new Exception("vinculó por parecido de nombre: " + r.User?.Username);
                try { s.IngresarMicrosoftAsync("oid-impostor", "mjulian@alvear.test", "Impostor", false).GetAwaiter().GetResult(); throw new Exception("vinculó un correo ya asociado a otro oid"); }
                catch (InvalidOperationException) { }
                if (Una<string>(db, "SELECT OidMicrosoft FROM Usuarios WHERE Username='mjulian'") != "oid-original") throw new Exception("pisó el oid original");
            }
            finally { Limpiar(db); }
        });
        test("AppHub usuarios: un usuario desactivado no entra y los nombres repetidos se numeran", () =>
        {
            var db = NuevaBase(true, "INSERT INTO Usuarios(Username,PasswordHash,Nombre,Activo,Correo) VALUES('baja','h','Baja',0,'baja@alvear.test')", "INSERT INTO Usuarios(Username,PasswordHash,Nombre) VALUES('ana','h','Otra Ana')");
            try
            {
                var s = new UserStorage(db);
                if (s.IngresarMicrosoftAsync("oid-b", "baja@alvear.test", "Baja", false).GetAwaiter().GetResult().User != null) throw new Exception("entró un usuario desactivado");
                var a = s.IngresarMicrosoftAsync("oid-a", "ana@alvear.test", "Ana", false).GetAwaiter().GetResult();
                if (a.User?.Username != "ana2") throw new Exception("no numeró el nombre repetido: " + a.User?.Username);
            }
            finally { Limpiar(db); }
        });
        // ---------- middleware ----------
        test("AppHub middleware: sin clave configurada ignora el encabezado y no abre sesión (modo autónomo)", () =>
        {
            var db = NuevaBase(true);
            try
            {
                var (siguio, auth, _) = Pasar(db, Emitir(), o: new("conciliador", "", Array.Empty<string>(), 30));
                if (!siguio || auth.Emitida != null) throw new Exception("abrió sesión sin tener la clave configurada");
                if (Una<long>(db, "SELECT COUNT(*) FROM Usuarios") != 0) throw new Exception("creó un usuario");
            }
            finally { Limpiar(db); }
        });
        test("AppHub middleware: con un token correcto abre la sesión sin pedir login y crea el usuario sin permisos", () =>
        {
            var db = NuevaBase(true);
            try
            {
                var (siguio, auth, ctx) = Pasar(db, Emitir());
                if (!siguio || auth.Emitida == null || !ctx.User.HasClaim("via", "apphub") || ctx.User.Identity?.Name != "ana") throw new Exception("no abrió la sesión");
                if (ctx.User.HasClaim(c => c.Type == "permission") || ctx.User.IsInRole("Admin")) throw new Exception("el usuario nuevo tiene permisos");
                if (ctx.Items.ContainsKey(IdentidadAppHub.ItemAviso)) throw new Exception("dejó un aviso de error");
            }
            finally { Limpiar(db); }
        });
        test("AppHub middleware: administrador inicial entra como Admin; el resto, sin permisos", () =>
        {
            var db = NuevaBase(true);
            try
            {
                var (_, _, ctx) = Pasar(db, Emitir(new() { ["email"] = "Jefe@Alvear.test", ["sub"] = "oid-j" }));
                if (!ctx.User.IsInRole("Admin")) throw new Exception("el administrador inicial no es Admin");
            }
            finally { Limpiar(db); }
        });
        test("AppHub middleware: token vencido, de otra app, con firma ajena o repetido no abren sesión y avisan", () =>
        {
            var db = NuevaBase(true);
            try
            {
                var ahora = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var malos = new[] { Emitir(new() { ["iat"] = ahora - 500, ["exp"] = ahora - 400 }), Emitir(new() { ["aud"] = "ofertas" }), Emitir(clave: "otra-clave-completamente-distinta-0123456789-XYZ"), "no-es-un-token" };
                foreach (var t in malos)
                {
                    var (siguio, auth, ctx) = Pasar(db, t);
                    if (!siguio || auth.Emitida != null || ctx.Items[IdentidadAppHub.ItemAviso] as string != "rechazada") throw new Exception("no rechazó un token malo");
                }
                var (_, a2, c2) = Pasar(db, Emitir(), repetido: 2);
                if (a2.Emitida != null || c2.Items[IdentidadAppHub.ItemAviso] as string != "rechazada") throw new Exception("aceptó el encabezado repetido");
                if (Una<long>(db, "SELECT COUNT(*) FROM Usuarios") != 0) throw new Exception("un token malo creó un usuario");
            }
            finally { Limpiar(db); }
        });
        test("AppHub middleware: usuario desactivado no entra y deja el aviso; sin encabezado no hace nada", () =>
        {
            var db = NuevaBase(true, "INSERT INTO Usuarios(Username,PasswordHash,Nombre,Activo,Correo) VALUES('ana','h','Ana',0,'ana@alvear.test')");
            try
            {
                var (_, auth, ctx) = Pasar(db, Emitir());
                if (auth.Emitida != null || ctx.Items[IdentidadAppHub.ItemAviso] as string != "desactivado") throw new Exception("dejó entrar al desactivado");
                var (siguio, a2, c2) = Pasar(db, null);
                if (!siguio || a2.Emitida != null || c2.Items.ContainsKey(IdentidadAppHub.ItemAviso)) throw new Exception("actuó sin encabezado");
            }
            finally { Limpiar(db); }
        });
        test("AppHub pipeline real: el token abre la sesión, la cookie sirve en los pedidos siguientes y sin identidad no se entra", () =>
        {
            var db = NuevaBase(true);
            WebApplication? app = null;
            try
            {
                var b = WebApplication.CreateBuilder(); b.WebHost.UseUrls("http://127.0.0.1:0"); b.Logging.ClearProviders();
                b.Services.AddSingleton(Opciones).AddSingleton(new UserStorage(db)).AddSingleton<SessionRegistry>();
                b.Services.AddAuthentication("Cookies").AddCookie(o => { o.LoginPath = "/account/login"; o.Events.OnValidatePrincipal = async c => { if (!c.HttpContext.RequestServices.GetRequiredService<SessionRegistry>().IsActive(c.Principal!) || await c.HttpContext.RequestServices.GetRequiredService<UserStorage>().Find(c.Principal?.Identity?.Name ?? "") == null) c.RejectPrincipal(); }; });
                b.Services.AddAuthorization();
                app = b.Build(); app.UseMiddleware<IdentidadAppHub>(); app.UseAuthentication(); app.UseAuthorization();
                app.MapGet("/privado", (HttpContext c) => c.User.Identity!.Name + "|" + c.User.HasClaim("via", "apphub")).RequireAuthorization();
                app.StartAsync().GetAwaiter().GetResult();
                var url = app.Urls.First();
                using var http = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = true, CookieContainer = new System.Net.CookieContainer() });
                var sin = http.GetAsync(url + "/privado").GetAwaiter().GetResult();
                if ((int)sin.StatusCode != 302) throw new Exception("sin identidad ni cookie debería pedir login y dio " + (int)sin.StatusCode);
                var falso = new HttpRequestMessage(HttpMethod.Get, url + "/privado"); falso.Headers.TryAddWithoutValidation(TokenAppHub.Encabezado, Emitir(clave: "otra-clave-completamente-distinta-0123456789-XYZ"));
                if ((int)http.SendAsync(falso).GetAwaiter().GetResult().StatusCode != 302) throw new Exception("un token con firma ajena dejó entrar");
                var bueno = new HttpRequestMessage(HttpMethod.Get, url + "/privado"); bueno.Headers.TryAddWithoutValidation(TokenAppHub.Encabezado, Emitir());
                var r1 = http.SendAsync(bueno).GetAwaiter().GetResult(); var t1 = r1.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                if ((int)r1.StatusCode != 200 || t1 != "ana|True") throw new Exception("el token no abrió la sesión en el mismo pedido: " + (int)r1.StatusCode + " " + t1);
                var r2 = http.GetAsync(url + "/privado").GetAwaiter().GetResult(); var t2 = r2.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                if ((int)r2.StatusCode != 200 || t2 != "ana|True") throw new Exception("la cookie no sirvió en el pedido siguiente: " + (int)r2.StatusCode + " " + t2);
                if (Una<long>(db, "SELECT COUNT(*) FROM Usuarios") != 1) throw new Exception("duplicó el usuario");
            }
            finally { app?.StopAsync().GetAwaiter().GetResult(); Limpiar(db); }
        });
        test("AppHub middleware: con sesión del mismo usuario no repite el trabajo; con la de otro, la reemplaza", () =>
        {
            var db = NuevaBase(true);
            try
            {
                var s = new UserStorage(db);
                var otra = new WebUser("otro", "Otro", "Usuario", new()).Principal(false);
                ((ClaimsIdentity)otra.Identity!).AddClaim(new Claim("correo", "otro@alvear.test"));
                var (_, a1, c1) = Pasar(db, Emitir(), new Auth { Cookie = otra });
                if (a1.Emitida == null || c1.User.Identity?.Name != "ana") throw new Exception("no reemplazó la sesión de otro usuario");
                var propia = new WebUser("ana", "Ana", "Usuario", new()).Principal(false);
                var id = (ClaimsIdentity)propia.Identity!; id.AddClaim(new Claim("via", "apphub")); id.AddClaim(new Claim("correo", "ANA@alvear.test"));
                var (siguio, a2, c2) = Pasar(db, Emitir(), new Auth { Cookie = propia });
                if (!siguio || a2.Emitida != null || c2.User != propia) throw new Exception("volvió a abrir la sesión de la misma persona");
                _ = s;
            }
            finally { Limpiar(db); }
        });
    }
}
