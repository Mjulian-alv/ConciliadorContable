namespace AgrupadorConceptos.Models
{
    public class MovimientoProcesado
    {
        public int Id { get; set; }
        public int IdArchivo { get; set; }
        public string Fecha { get; set; }
        public string ConceptoOriginal { get; set; }
        public string DescripcionOriginal { get; set; }
        public string ConceptoEstandar { get; set; }
        public decimal Debitos { get; set; }
        public decimal Creditos { get; set; }
        public string ConceptoFinal { get; set; }
        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Cuenta final, mismo patron que ConceptoFinal
        public string CuentaFinal { get; set; }
    }
}