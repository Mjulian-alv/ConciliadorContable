// Fecha: 14/10/2026 - TAREA: 00097 - Linea: 2 - Estado del Procesador por circuito: el archivo en trabajo sobrevive a ir a otra pantalla y volver, como la ventana abierta del escritorio. Nada se comparte entre usuarios.
using AgrupadorConceptos.Models;
namespace Conciliador.Web.Services;
public sealed class AgrupadorWorkspace
{
    public int PerfilId { get; set; }
    public int ArchivoId { get; set; }
    /// <summary>Los movimientos del archivo en pantalla; las mismas instancias que se editan y se re-homologan en memoria.</summary>
    public List<MovimientoProcesado>? Movimientos { get; set; }
    public void LimpiarArchivo() { ArchivoId = 0; Movimientos = null; }
}
/// <summary>Un grupo de movimientos pendientes con el mismo concepto y descripción (homologación masiva).</summary>
public sealed record PendienteAgrupado(string ConceptoOriginal, string DescripcionOriginal, int Cantidad, decimal TotalDebitos, decimal TotalCreditos);
