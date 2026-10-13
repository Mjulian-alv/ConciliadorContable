// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Criterio de matching de la conciliacion interna
using System;
using AgrupadorConceptos.Models;

namespace AgrupadorConceptos.Services
{
    /// <summary>
    /// Criterio de igualdad para emparejar dos movimientos de extractos propios distintos:
    /// una transferencia entre cuentas propias aparece como débito en un lado y crédito en
    /// el otro, por el mismo importe. La igualdad de fecha se resuelve con
    /// <see cref="ComparadorConciliacion.FechasIguales"/>, compartida con la conciliación externa.
    /// </summary>
    public static class ComparadorConciliacionInterna
    {
        public static bool ImportesOpuestos(MovimientoProcesado a, MovimientoProcesado b) =>
            ComparadorConciliacion.ImporteEfectivo(a) == ComparadorConciliacion.ImporteEfectivo(b)
            && SignoOpuesto(a, b);

        private static bool SignoOpuesto(MovimientoProcesado a, MovimientoProcesado b)
        {
            bool aEsDebito = a.Debitos != 0;
            bool bEsDebito = b.Debitos != 0;
            return aEsDebito != bEsDebito;
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Filtro por rango para armar cada lado de la sesion
        /// <summary>True si la fecha del movimiento cae dentro del rango, inclusive.</summary>
        public static bool EstaEnRango(DateTime fecha, DateTime desde, DateTime hasta)
        {
            // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Operar sobre días tipados evita reinterpretaciones culturales.
            return fecha.Date >= desde.Date && fecha.Date <= hasta.Date;
        }
    }
}
