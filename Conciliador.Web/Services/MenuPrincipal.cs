// Fecha: 14/10/2026 - TAREA: 00097 - Linea: 1 - Un solo catálogo de accesos alimenta el inicio y el menú lateral, con los permisos del escritorio.
using System.Security.Claims;
using Conciliador.Web.Security;
namespace Conciliador.Web.Services;
public enum EstadoAcceso { Disponible, SinPermiso, Proximamente, RequiereBase }
/// <param name="Tarjeta">Si aparece como tarjeta en el inicio (los conceptos estándar sólo están en el menú lateral).</param>
/// <param name="Implementado">Se pasa a true en la línea que construye la pantalla: hasta entonces la tarjeta dice «Próximamente».</param>
/// <param name="RequiereBase">No funciona en modo demostración, que no abre SQL.</param>
public sealed record Acceso(string Modulo, string Clave, string Glifo, string Titulo, string Descripcion, string Ruta, string Permiso, bool Implementado, bool RequiereBase = false, bool Tarjeta = true);
public static class MenuPrincipal
{
    public const string ModArca = "ARCA CLIENTE";
    public const string ModAgrupador = "AGRUPADOR DE CONCEPTOS";
    public static readonly IReadOnlyList<Acceso> Accesos = new Acceso[]
    {
        new(ModArca, "arca-comprobantes", "⇄", "Comprobantes", "Conciliá los comprobantes de ARCA contra el sistema por perfil y CUIT.", "/arca/comprobantes", "ArcaOffline", true),
        new(ModArca, "arca-perfiles", "▤", "Perfiles de comprobantes", "Formatos de archivo, listas de CUIT y directivas.", "/arca/perfiles", "ArcaPerfiles", true),
        new(ModArca, "arca-pyr", "⇄", "Percepciones y retenciones", "Conciliación de percepciones y retenciones de ARCA contra los mayores.", "/pyr", "ArcaPyR", true),
        new(ModArca, "arca-pyr-perfiles", "▤", "Perfiles PyR", "Cuentas, mayores y directivas de percepciones y retenciones.", "/pyr/perfiles", "ArcaPyR", true),
        new(ModAgrupador, "agr-procesador", "▦", "Procesador de movimientos", "Importá extractos bancarios y homologá sus conceptos.", "/agrupador/procesador", "AgrProcesador", true, true),
        new(ModAgrupador, "agr-homologaciones", "≡", "Gestión de homologaciones", "Reglas que traducen el texto del banco a un concepto estándar.", "/agrupador/homologaciones", "AgrHomologaciones", true, true),
        new(ModAgrupador, "agr-consulta", "⌕", "Consulta de movimientos", "Movimientos por rango de fechas y conceptos, con exportación.", "/agrupador/consulta", "AgrHomologaciones", true, true),
        new(ModAgrupador, "agr-externa", "⇄", "Conciliación externa", "Conciliá el extracto contra un listado del ERP, por sesiones.", "/agrupador/conciliacion-externa", "AgrConciliacion", false, true),
        new(ModAgrupador, "agr-interna", "↔", "Conciliación interna", "Conciliá débitos contra créditos propios dentro del extracto.", "/agrupador/conciliacion-interna", "AgrConciliacionInterna", true, true),
        new(ModAgrupador, "agr-cuentas", "#", "Cuentas contables", "Catálogo de cuentas importado del sistema y sus conceptos.", "/agrupador/cuentas", "AgrCuentasContables", true, true),
        new(ModAgrupador, "agr-conceptos", "◇", "Conceptos estándar", "Cada concepto estándar puede tener una cuenta contable.", "/agrupador/conceptos", "AgrCuentasContables", true, true, false),
    };
    public static IEnumerable<Acceso> Visibles(ClaimsPrincipal usuario) => Accesos.Where(a => Permisos.Tiene(usuario, a.Permiso));
    public static EstadoAcceso Estado(Acceso acceso, ClaimsPrincipal usuario, bool demostracion)
    {
        if (!Permisos.Tiene(usuario, acceso.Permiso)) return EstadoAcceso.SinPermiso;
        if (!acceso.Implementado) return EstadoAcceso.Proximamente;
        return acceso.RequiereBase && demostracion ? EstadoAcceso.RequiereBase : EstadoAcceso.Disponible;
    }
    /// <summary>Accesos del menú lateral: sólo lo permitido y construido, y en demostración nada que necesite base.</summary>
    public static IEnumerable<Acceso> Lateral(ClaimsPrincipal usuario, bool demostracion) =>
        Visibles(usuario).Where(a => a.Implementado && !(demostracion && a.RequiereBase));
}
