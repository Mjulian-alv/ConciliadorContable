// Fecha: 14/10/2026 - TAREA: 00097 - Linea: 5 - Fila de las grillas de la conciliación interna: el movimiento original más el extracto de donde viene (los lados son globales y fecha+importe no alcanzan para distinguir dos candidatos).
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;
namespace Conciliador.Web.Services;
public sealed record MovimientoInterno(MovimientoProcesado Original, string Extracto)
{
    public int Id => Original.Id;
    public DateTime Fecha => Original.Fecha;
    public decimal Importe => ComparadorConciliacion.ImporteEfectivo(Original);
    public string ConceptoFinal => Original.ConceptoFinal;
}
