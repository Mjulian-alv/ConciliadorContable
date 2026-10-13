// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Operar sobre días tipados evita reinterpretaciones culturales.
// Fecha: 08/10/2026 - TAREA: 00076 - Linea: 4 - El rango de elección se calcula sobre fechas fuente de movimientos, no sobre la importación.
using System;
using System.Collections.Generic;
using System.Linq;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;

namespace AgrupadorConceptos.Services
{
    public sealed class ExtractoCandidato
    {
        public bool Seleccionado { get; set; }
        public int IdArchivo { get; init; }
        public string Perfil { get; init; }
        public string Archivo { get; init; }
        public DateTime? Desde { get; init; }
        public DateTime? Hasta { get; init; }
        public int Cantidad { get; init; }
        public int FechasInvalidas { get; init; }
        public bool CoincideRango { get; init; }
    }

    public static class SelectorExtractosService
    {
        public static (DateTime? Desde, DateTime? Hasta, int Invalidas) SugerirRango(IEnumerable<ConciliacionItemExterno> items)
        {
            var fechas = items.Select(i => i.Fecha.Date).ToList();
            var validas = fechas;
            return (validas.Count == 0 ? null : validas.Min(), validas.Count == 0 ? null : validas.Max(),
                fechas.Count - validas.Count);
        }

        public static List<ExtractoCandidato> Obtener(DateTime desde, DateTime hasta, bool mostrarTodos)
        {
            if (desde.Date > hasta.Date) throw new ArgumentException("La fecha Desde debe ser anterior o igual a Hasta.");
            var archivos = ArchivoImportadoStorage.ObtenerTodos();
            var perfiles = PerfilBancoStorage.ObtenerTodos().ToDictionary(p => p.Id);
            var movimientos = MovimientoStorage.ObtenerPorArchivos(archivos.Select(a => a.Id))
                .GroupBy(m => m.IdArchivo).ToDictionary(g => g.Key);
            return archivos.Select(a =>
            {
                var movs = movimientos.TryGetValue(a.Id, out var grupo) ? grupo.ToList() : new List<MovimientoProcesado>();
                var fechas = movs.Select(m => m.Fecha.Date).ToList();
                var validas = fechas;
                return new ExtractoCandidato
                {
                    IdArchivo = a.Id,
                    Perfil = perfiles.TryGetValue(a.IdPerfilBanco, out var p) ? p.NombreBanco : "Perfil desconocido",
                    Archivo = a.NombreArchivo,
                    Desde = validas.Count == 0 ? null : validas.Min(),
                    Hasta = validas.Count == 0 ? null : validas.Max(),
                    Cantidad = movs.Count,
                    FechasInvalidas = fechas.Count - validas.Count,
                    CoincideRango = validas.Any(f => f >= desde.Date && f <= hasta.Date)
                };
            }).Where(c => mostrarTodos || c.CoincideRango).ToList();
        }
    }
}
