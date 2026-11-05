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
        // Fecha: 05/11/2026 - TAREA: 00162 - Linea: 2 - Sólo para mostrar: no son columnas de la tabla (Dapper las ignora al leer con mov.*)
        // Se completan al cargar el archivo (ParesMovimientoStorage.Marcar) con una consulta por carga, no una por fila. La regla la sigue
        // aplicando el storage dentro de la transacción: esto sólo evita dejar entrar en edición lo que se va a rechazar.
        /// <summary>El movimiento figura en un par de conciliación interna o externa, de cualquier sesión.</summary>
        public bool Apareado { get; set; }
        /// <summary>Dónde está apareado, para la grilla: «Conciliación interna «X» · en proceso». Null si está libre.</summary>
        public string ApareadoEn { get; set; }
        /// <summary>El mensaje de bloqueo ya armado (el mismo que da el storage al rechazar), para que las pantallas no repitan el texto. Null si está libre.</summary>
        public string MensajeApareado { get; set; }
    }
}