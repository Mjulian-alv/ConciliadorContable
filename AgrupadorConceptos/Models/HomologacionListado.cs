namespace AgrupadorConceptos.Models
{
    /// <summary>
    /// Fila de la pantalla de gestión de homologaciones: la homologación
    /// con el banco y el concepto estándar ya resueltos por JOIN.
    /// </summary>
    public class HomologacionListado
    {
        public int Id { get; set; }
        public string Banco { get; set; }
        public string ValorOriginal { get; set; }
        public string ConceptoEstandar { get; set; }
    }
}
