using System;

namespace AgrupadorConceptos.Models
{
    public enum TipoMatch
    {
        FechaImporte,
        SeleccionManual,
        SoloImporte,
        Manual
    }

    public class ConciliacionPar
    {
        public int Id { get; set; }
        public int IdSesion { get; set; }
        public int IdItemExterno { get; set; }
        public int IdMovimientoProcesado { get; set; }
        public string TipoMatch { get; set; }
        public DateTime FechaConciliacion { get; set; }

        // Campos de visualización (no persisten)
        // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Operar sobre días tipados evita reinterpretaciones culturales.
        public System.DateTime FechaExterno { get; set; }
        public decimal ImporteExterno { get; set; }
        public string DetalleExterno { get; set; }
        // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Operar sobre días tipados evita reinterpretaciones culturales.
        public System.DateTime FechaExtracto { get; set; }
        public decimal ImporteExtracto { get; set; }
        public string ConceptoFinalExtracto { get; set; }
    }
}
