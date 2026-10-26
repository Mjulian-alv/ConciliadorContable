// Fecha: 26/10/2026 - TAREA: 00135 - Linea: 1 - Resumen de lo modificado en un archivo, para avisar antes de darlo de baja
namespace AgrupadorConceptos.Models
{
    public class ModificacionesArchivo
    {
        /// <summary>Movimientos con al menos una modificación (cada uno cuenta una vez).</summary>
        public int Total { get; set; }
        public int ConceptosFinalesEditados { get; set; }
        public int CuentasEditadas { get; set; }
        public int Conciliados { get; set; }
        public bool HayModificaciones => Total > 0;
    }
}
