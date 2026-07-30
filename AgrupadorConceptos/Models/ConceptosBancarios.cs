namespace AgrupadorConceptos.Models
{
    /// <summary>
    /// Valores sentinela del proceso de homologación.
    /// </summary>
    public static class ConceptosBancarios
    {
        /// <summary>
        /// Marca que un movimiento todavía no tiene concepto estándar asignado.
        /// Se persiste tal cual en MovimientosArchivo.ConceptoEstandar/ConceptoFinal,
        /// así que el texto no se puede cambiar sin migrar los datos existentes.
        /// </summary>
        public const string PendienteHomologar = "Pendiente Homologar";

        /// <summary>
        /// True si el concepto está sin resolver: vacío o el sentinela.
        /// Ojo: no equivale a comparar contra <see cref="PendienteHomologar"/> a secas.
        /// ConceptoFinal usa esta versión (admite vacío, porque en la importación se
        /// guarda "" cuando no hubo match); ConceptoEstandar usa la comparación exacta.
        /// </summary>
        public static bool EstaPendiente(string concepto) =>
            string.IsNullOrWhiteSpace(concepto) || concepto == PendienteHomologar;
    }
}
