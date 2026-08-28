// Fecha: 28/08/2026 - TAREA: 00003 - Linea: 1 - Resultado del calculo de impacto de una baja
using System.Collections.Generic;

namespace AgrupadorConceptos.Models
{
    /// <summary>Un movimiento que hoy resuelve la regla, con lo que le quedaría sin ella.</summary>
    public class MovimientoImpactado
    {
        public MovimientoProcesado Movimiento { get; set; }

        /// <summary>
        /// Concepto que otra homologación del perfil le sigue dando si esta regla desaparece,
        /// o null si ninguna lo cubre. Se calcula una sola vez, al mostrar el impacto, y se
        /// reusa al aplicar: así lo que se ejecuta es exactamente lo que se le mostró al usuario.
        /// </summary>
        public string ConceptoSinLaRegla { get; set; }
    }

    /// <summary>
    /// Movimientos que resuelve una regla, partidos según sobrevivan o no a su baja.
    /// </summary>
    public class ImpactoHomologacion
    {
        /// <summary>Se quedan sin ninguna regla que los cubra (ConceptoSinLaRegla == null).</summary>
        public List<MovimientoImpactado> Afectados { get; } = new List<MovimientoImpactado>();

        /// <summary>Los sigue resolviendo otra homologación; no se despegan.</summary>
        public List<MovimientoImpactado> CubiertosPorOtraRegla { get; } = new List<MovimientoImpactado>();

        public int Total => Afectados.Count + CubiertosPorOtraRegla.Count;
    }
}
