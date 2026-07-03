using System;

namespace AgrupadorConceptos.Models
{
    public enum TipoMatch
    {
        FechaImporte,
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
        public string FechaExterno { get; set; }
        public decimal ImporteExterno { get; set; }
        public string DetalleExterno { get; set; }
        public string FechaExtracto { get; set; }
        public decimal ImporteExtracto { get; set; }
        public string ConceptoFinalExtracto { get; set; }
    }
}
