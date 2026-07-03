namespace AgrupadorConceptos.Models
{
    public class HomologacionConcepto
    {
        public int Id { get; set; }
        public int IdPerfilBanco { get; set; }
        public string ValorOriginal { get; set; } // El codigo o la descripcion que viene en el excel
        public int IdConceptoEstandar { get; set; } // Maped to ConceptoEstandar
    }
}