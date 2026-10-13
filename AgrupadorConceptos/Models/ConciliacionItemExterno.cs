using System;

namespace AgrupadorConceptos.Models
{
    public class ConciliacionItemExterno
    {
        public int Id { get; set; }
        public int IdSesion { get; set; }
        // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Operar sobre días tipados evita reinterpretaciones culturales.
        public System.DateTime Fecha { get; set; }
        public decimal Importe { get; set; }
        public string Detalle { get; set; }
        public bool Conciliado { get; set; }
    }
}
