// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Sesion de conciliacion interna entre extractos
using System;

namespace AgrupadorConceptos.Models
{
    /// <summary>
    /// Sesión de conciliación interna: un solo rango de fechas, sin elegir perfil. Se concilian
    /// débitos contra créditos de TODOS los extractos que caen en el rango — la exclusión de un
    /// mismo extracto contra sí mismo vive en el matching (ver
    /// ConciliacionInternaService.AutoConciliar), no en el alta de la sesión.
    /// </summary>
    public class ConciliacionInternaSesion
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public string ConceptosJson { get; set; }
        public string Estado { get; set; }

        public string DisplayName => $"{Nombre} ({FechaCreacion:dd/MM/yyyy HH:mm}) [{Estado}]";
    }
}
