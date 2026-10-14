// Fecha: 14/10/2026 - TAREA: 00097 - Linea: 1 - Los almacenes y servicios internos del Agrupador siguen siendo visibles para las pantallas WinForms y para las pruebas.
// Fecha: 14/10/2026 - TAREA: 00097 - Linea: 6 - La web accede a los mismos almacenes que el escritorio: comparten solución y reglas, y no se duplican como API pública.
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AgrupadorConceptos")]
[assembly: InternalsVisibleTo("Homologador.Tests")]
[assembly: InternalsVisibleTo("FechasBancarias.Tests")]
[assembly: InternalsVisibleTo("Conciliador.Web")]
[assembly: InternalsVisibleTo("AgrupadorWeb.Tests")]
