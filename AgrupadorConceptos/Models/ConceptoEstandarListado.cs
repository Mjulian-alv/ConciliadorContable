// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Fila de Gestion de Conceptos Estandar
namespace AgrupadorConceptos.Models
{
    /// <summary>Fila de la pantalla de mantenimiento de conceptos estándar.</summary>
    public class ConceptoEstandarListado
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int? IdCuentaContable { get; set; }
        public string Cuenta { get; set; }
        public string DescripcionCuenta { get; set; }

        /// <summary>
        /// Cuántos movimientos tienen hoy este concepto en ConceptoEstandar. Es un COUNT
        /// directo por texto, no el conteo por regla que usa la grilla de homologaciones:
        /// acá alcanza con saber "cuánto se usa", no de qué regla depende cada uno.
        /// </summary>
        public int Movimientos { get; set; }

        public string CuentaDisplay => IdCuentaContable.HasValue
            ? $"{Cuenta} — {DescripcionCuenta}"
            : "(sin asignar)";
    }
}
