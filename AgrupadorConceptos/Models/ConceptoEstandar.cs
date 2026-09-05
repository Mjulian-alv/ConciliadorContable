namespace AgrupadorConceptos.Models
{
    public class ConceptoEstandar
    {
        public int Id { get; set; }
        public string Nombre { get; set; }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Cuenta contable del concepto
        public int? IdCuentaContable { get; set; }
    }
}