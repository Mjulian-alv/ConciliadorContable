using System;

namespace AgrupadorConceptos.Models
{
    // Fecha: 07/10/2026 - TAREA: 00073 - Linea: 2 - La consulta conserva tipos y nulos sin exponer identificadores internos.
    public sealed class MovimientoConsulta
    {
        public DateTime Fecha { get; set; }
        public string ConceptoOriginal { get; set; }
        public string DescripcionOriginal { get; set; }
        public decimal? Debitos { get; set; }
        public decimal? Creditos { get; set; }
        public string ConceptoEstandar { get; set; }
        public string ConceptoFinal { get; set; }
        public string CuentaFinal { get; set; }
        public string NombrePerfil { get; set; }
    }
}
