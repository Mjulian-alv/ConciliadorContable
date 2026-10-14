// Fecha: 14/10/2026 - TAREA: 00097 - Linea: 3 - Lo que devuelve el diálogo de homologar, igual que HomologarForm: concepto, valor guardado y cuenta elegida.
namespace Conciliador.Web.Services;
/// <param name="IdCuentaContable">Cuenta elegida, o null si quedó «sin asignar».</param>
public sealed record ResultadoHomologar(bool Exitosa, string Concepto = "", string Valor = "", int? IdCuentaContable = null);
