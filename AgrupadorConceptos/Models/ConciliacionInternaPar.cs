// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Par conciliado entre los dos extractos
using System;

namespace AgrupadorConceptos.Models
{
    public class ConciliacionInternaPar
    {
        public int Id { get; set; }
        public int IdSesion { get; set; }
        public int IdMovimientoA { get; set; }
        public int IdMovimientoB { get; set; }
        public string TipoMatch { get; set; }
        public DateTime FechaConciliacion { get; set; }

        // Campos de visualización (no persisten)
        public string FechaA { get; set; }
        public decimal ImporteA { get; set; }
        public string ConceptoFinalA { get; set; }
        public string FechaB { get; set; }
        public decimal ImporteB { get; set; }
        public string ConceptoFinalB { get; set; }
    }
}
