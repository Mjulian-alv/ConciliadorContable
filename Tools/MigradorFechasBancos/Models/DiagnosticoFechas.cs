// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 2 - Informar incidencias por fila antes de modificar el esquema.
namespace MigradorFechasBancos.Models;
public sealed record IncidenciaFecha(string Tabla, int? Id, string Texto, DateTime? Intermedia, string Motivo);
public sealed class DiagnosticoFechas
{
    public List<DiagnosticoTabla> Tablas { get; } = new();
    public List<IncidenciaFecha> Incidencias { get; } = new();
    public bool PuedeAplicar => Incidencias.Count == 0 && Tablas.Count == 2;
    public bool YaMigrada => PuedeAplicar && Tablas.All(t => t.Migrada);
}
public sealed record DiagnosticoTabla(string Tabla, int Cantidad, int IntermediasVacias, bool Migrada);
