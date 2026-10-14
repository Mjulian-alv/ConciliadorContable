// Fecha: 14/10/2026 - TAREA: 00097 - Linea: 1 - Un solo catálogo de accesos alimenta las políticas, el menú lateral y el inicio.
using System.Reflection;
using System.Security.Claims;
using Conciliador.Web.Data;
using Conciliador.Web.Security;
using Conciliador.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
static class MenuTests
{
    static void Check(bool ok, string mensaje = "Condición falsa") { if (!ok) throw new Exception(mensaje); }
    static ClaimsPrincipal Usuario(string rol, params string[] permisos) => new WebUser("u", "Usuaria de prueba", rol, permisos.ToList()).Principal();
    static bool Autoriza(ClaimsPrincipal u, string politica)
    {
        using var sp = new ServiceCollection().AddLogging().AddAuthorization(Permisos.Registrar).BuildServiceProvider();
        return sp.GetRequiredService<IAuthorizationService>().AuthorizeAsync(u, null, politica).GetAwaiter().GetResult().Succeeded;
    }
    public static void Run(Action<string, Action> test)
    {
        test("Permisos: las cinco políticas del Agrupador y las cuatro de ARCA existen; Admin pasa todas", () =>
        {
            var admin = Usuario("Admin");
            foreach (var p in Permisos.Arca.Concat(Permisos.Agrupador)) Check(Autoriza(admin, p), "Admin rechazado en " + p);
            Check(Permisos.Agrupador.Length == 5 && Permisos.Arca.Length == 4, "Cantidad de permisos");
        });
        test("Permisos: un usuario sólo pasa las políticas que tiene, y un anónimo ninguna", () =>
        {
            var u = Usuario("Usuario", "AgrProcesador", "ArcaPyR");
            Check(Autoriza(u, "AgrProcesador") && Autoriza(u, "ArcaPyR"));
            Check(!Autoriza(u, "AgrConciliacion") && !Autoriza(u, "ArcaOffline") && !Autoriza(u, "AgrCuentasContables"));
            Check(!Autoriza(new ClaimsPrincipal(new ClaimsIdentity()), "AgrProcesador"), "Aceptó anónimo");
        });
        test("Menú: Admin ve todo, un usuario parcial ve lo suyo y uno sin permisos no ve nada", () =>
        {
            Check(MenuPrincipal.Visibles(Usuario("Admin")).Count() == MenuPrincipal.Accesos.Count);
            var parcial = MenuPrincipal.Visibles(Usuario("Usuario", "AgrConciliacion")).Select(a => a.Clave).ToArray();
            Check(parcial.SequenceEqual(new[] { "agr-externa" }), "Parcial: " + string.Join(",", parcial));
            Check(!MenuPrincipal.Visibles(Usuario("Usuario")).Any());
        });
        test("Menú: la tarjeta informa el motivo (sin permiso, próximamente, requiere base)", () =>
        {
            var agr = MenuPrincipal.Accesos.First(a => a.Modulo == MenuPrincipal.ModAgrupador);
            Check(MenuPrincipal.Estado(agr, Usuario("Usuario"), false) == EstadoAcceso.SinPermiso);
            var admin = Usuario("Admin");
            var estado = MenuPrincipal.Estado(agr, admin, false);
            Check(estado == (agr.Implementado ? EstadoAcceso.Disponible : EstadoAcceso.Proximamente), "Estado " + estado);
            var arca = MenuPrincipal.Accesos.First(a => a.Modulo == MenuPrincipal.ModArca);
            Check(MenuPrincipal.Estado(arca, admin, true) == EstadoAcceso.Disponible, "ARCA funciona en demostración");
            if (agr.Implementado) Check(MenuPrincipal.Estado(agr, admin, true) == EstadoAcceso.RequiereBase, "El Agrupador requiere base en demostración");
        });
        test("Menú: cada acceso implementado tiene su pantalla, y los permisos existen", () =>
        {
            var rutas = typeof(Conciliador.Web.Components.App).Assembly.GetTypes().SelectMany(t => t.GetCustomAttributes<RouteAttribute>()).Select(r => r.Template).ToHashSet();
            foreach (var a in MenuPrincipal.Accesos.Where(a => a.Implementado)) Check(rutas.Contains(a.Ruta), "Falta la pantalla " + a.Ruta);
            Check(rutas.Contains("/"), "Falta el inicio");
            Check(rutas.Contains("/perfiles"), "La URL vieja /perfiles debe seguir funcionando");
            foreach (var a in MenuPrincipal.Accesos) Check(Permisos.Arca.Concat(Permisos.Agrupador).Contains(a.Permiso), "Permiso desconocido " + a.Permiso);
            Check(MenuPrincipal.Accesos.Select(a => a.Clave).Distinct().Count() == MenuPrincipal.Accesos.Count, "Claves repetidas");
        });
        test("Menú lateral: sólo lo implementado y permitido; en demostración no aparece lo que requiere base", () =>
        {
            var admin = Usuario("Admin");
            var normal = MenuPrincipal.Lateral(admin, false).ToList();
            Check(normal.All(a => a.Implementado) && normal.Any(a => a.Modulo == MenuPrincipal.ModArca));
            Check(!MenuPrincipal.Lateral(admin, true).Any(a => a.RequiereBase), "Demostración muestra accesos que requieren base");
            Check(!MenuPrincipal.Lateral(Usuario("Usuario"), false).Any(), "Sin permisos no ve accesos");
        });
    }
}
