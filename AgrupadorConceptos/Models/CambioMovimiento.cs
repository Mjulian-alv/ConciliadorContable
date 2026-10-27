// Fecha: 26/10/2026 - TAREA: 00135 - Linea: 2 - Una fila del historial de cambios de un movimiento
using System;

namespace AgrupadorConceptos.Models
{
    public class CambioMovimiento
    {
        public DateTime Fecha { get; set; }
        public string Campo { get; set; }
        public string ValorAnterior { get; set; }
        public string ValorNuevo { get; set; }
        public string Origen { get; set; }
        public string Usuario { get; set; }
    }
}
