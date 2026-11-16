// Fecha: 16/11/2026 - TAREA: 00192 - Linea: 4 - Filtros de la pantalla de resultados fuera del Razor para poder probarlos.
using ArcaCliente.Models;
namespace ArcaCliente.Web.Services;
public static class ResultFilter
{
    // Sólo los comprobantes que algún criterio emparejó (conciliados y con diferencia) tienen directiva.
    public static bool HasDirectives(string state)=>state is nameof(EstadoConciliacion.Conciliado) or nameof(EstadoConciliacion.DiferenciaImporte);
    public static IEnumerable<ItemConciliacion> Apply(IEnumerable<ItemConciliacion> items,string state,string search,IReadOnlySet<int> directives)=>
        items.Where(x=>(state==""||x.Estado.ToString()==state)
            &&(directives.Count==0||!HasDirectives(state)||directives.Contains(x.DirectivaAplicada))
            &&(search==""||((x.CuitProveedor??"")+" "+(x.NombreProveedor??"")+" "+x.Numero).Contains(search,StringComparison.OrdinalIgnoreCase)));
    // Cuántos comprobantes del estado elegido concilió cada directiva (número de directiva -> cantidad).
    public static SortedDictionary<int,int> CountByDirective(IEnumerable<ItemConciliacion> items,string state)=>
        new(items.Where(x=>x.Estado.ToString()==state&&x.DirectivaAplicada>0).GroupBy(x=>x.DirectivaAplicada).ToDictionary(g=>g.Key,g=>g.Count()));
}
