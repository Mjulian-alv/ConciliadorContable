using System;

namespace AgrupadorConceptos.Models
{
    public class ConciliacionItemExterno
    {
        public int Id { get; set; }
        public int IdSesion { get; set; }
        public string Fecha { get; set; }
        public decimal Importe { get; set; }
        public string Detalle { get; set; }
        public bool Conciliado { get; set; }
    }
}
