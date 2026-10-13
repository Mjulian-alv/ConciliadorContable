// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Conservar ubicación y valor permite corregir todas las filas del archivo rechazado.
namespace AgrupadorConceptos.Models;
public sealed record IncidenciaFechaImportacion(string Archivo, string Hoja, int Fila, string ValorOriginal, string Motivo);
