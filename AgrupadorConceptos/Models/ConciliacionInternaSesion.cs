// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Sesion de conciliacion interna entre extractos
using System;

namespace AgrupadorConceptos.Models
{
    /// <summary>
    /// Sesión de conciliación interna: dos lados, cada uno un perfil de banco y un rango de
    /// fechas (no una lista fija de archivos importados — los movimientos se recalculan del
    /// perfil filtrando por rango cada vez que se abre la sesión).
    /// </summary>
    public class ConciliacionInternaSesion
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int IdPerfilA { get; set; }
        public DateTime FechaDesdeA { get; set; }
        public DateTime FechaHastaA { get; set; }
        public int IdPerfilB { get; set; }
        public DateTime FechaDesdeB { get; set; }
        public DateTime FechaHastaB { get; set; }
        public string ConceptosJson { get; set; }
        public string Estado { get; set; }

        public string DisplayName => $"{Nombre} ({FechaCreacion:dd/MM/yyyy HH:mm}) [{Estado}]";
    }
}
