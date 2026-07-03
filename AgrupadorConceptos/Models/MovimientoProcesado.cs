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
    }
}