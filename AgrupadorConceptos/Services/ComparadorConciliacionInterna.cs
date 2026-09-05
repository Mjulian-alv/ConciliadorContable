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
        public static bool EstaEnRango(string fecha, DateTime desde, DateTime hasta)
        {
            var d = ComparadorConciliacion.ParsearFecha(fecha);
            return d.HasValue && d.Value >= desde.Date && d.Value <= hasta.Date;
        }
    }
}
