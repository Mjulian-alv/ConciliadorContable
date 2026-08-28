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

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 3 - Ids para operar sobre la fila
        // Con "(Todos los perfiles)" la pantalla no tiene un perfil en el combo: el
        // perfil sobre el que se calcula el impacto sale de la fila.
        public int IdPerfilBanco { get; set; }
        public int IdConceptoEstandar { get; set; }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 1 - Cuantos movimientos resuelve hoy
        // No sale del SQL: que regla cubre cada movimiento lo decide el matcher, no una FK.
        // Lo completa HomologacionAdminService.ContarUsoPorRegla.
        public int Movimientos { get; set; }
    }
}
