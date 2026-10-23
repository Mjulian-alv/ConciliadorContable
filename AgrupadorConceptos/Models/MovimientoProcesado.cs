using System;

namespace AgrupadorConceptos.Models
{
    public class MovimientoProcesado
    {
        public int Id { get; set; }
        public int IdArchivo { get; set; }
        // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Operar sobre días tipados evita reinterpretaciones culturales.
        public System.DateTime Fecha { get; set; }
        public string ConceptoOriginal { get; set; }
        public string DescripcionOriginal { get; set; }
        public string ConceptoEstandar { get; set; }
        public decimal Debitos { get; set; }
        public decimal Creditos { get; set; }
        public string ConceptoFinal { get; set; }
        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Cuenta final, mismo patron que ConceptoFinal
        public string CuentaFinal { get; set; }
        public string NombreBanco { get; set; }
        // Fecha: 15/10/2026 - TAREA: 00098 - Linea: 1 - Nombre de la cuenta final (join con CuentasContables), solo lectura
        public string NombreCuenta { get; set; }
        // Fecha: 23/10/2026 - TAREA: 00125 - Linea: 1 - Mismo nombre que la columna para que "mov.*" lo mapee; la fecha es NULL mientras no esté conciliado
        public bool Conciliado { get; set; }
        public DateTime? ConciliadoFecha { get; set; }
    }
}