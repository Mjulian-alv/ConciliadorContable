using System;
using System.Collections.Generic;
using System.Linq;
using AgrupadorConceptos.Models;

namespace AgrupadorConceptos.Services
{
    /// <summary>
    /// Única implementación del algoritmo de homologación: dado el diccionario
    /// ValorOriginal → ConceptoEstandar de un perfil, resuelve qué concepto le
    /// corresponde a un movimiento.
    ///
    /// El perfil define la estrategia: si EsCodigo, el valor del banco es un código
    /// y el match es exacto; si no, es texto libre y se busca la primera clave del
    /// diccionario contenida en la descripción.
    /// </summary>
    public static class HomologacionMatcher
    {
        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 1 - Devolver la clave que gana, no solo el concepto
        // Saber QUE regla resuelve un movimiento (y no solo con que concepto quedo) es lo
        // que permite calcular el impacto real de una baja. Atribuir por texto de concepto
        // no sirve: dos reglas distintas pueden apuntar al mismo ConceptoEstandar, y al
        // borrar una despegariamos movimientos que la otra sigue cubriendo.
        /// <summary>
        /// Devuelve la clave del diccionario que resuelve el valor, o null si no hay match.
        /// La clave devuelta es igual a la almacenada ignorando mayusculas (el diccionario
        /// es OrdinalIgnoreCase), asi que quien la compare tiene que usar OrdinalIgnoreCase.
        /// </summary>
        public static string ResolverClave(IDictionary<string, string> dicHomologacion, string valorABuscar, bool esCodigo)
        {
            if (dicHomologacion == null || string.IsNullOrEmpty(valorABuscar))
                return null;

            if (esCodigo)
                return dicHomologacion.ContainsKey(valorABuscar) ? valorABuscar : null;

            var match = dicHomologacion.FirstOrDefault(
                d => valorABuscar.IndexOf(d.Key, StringComparison.OrdinalIgnoreCase) >= 0);

            return match.Key;
        }

        /// <summary>
        /// Devuelve el concepto estándar homologado, o null si no hay match.
        /// </summary>
        /// <param name="dicHomologacion">ValorOriginal → ConceptoEstandar. Se espera
        /// construido con StringComparer.OrdinalIgnoreCase y en orden estable
        /// (ver HomologacionStorage), porque con varias claves candidatas gana la primera.</param>
        public static string Resolver(IDictionary<string, string> dicHomologacion, string valorABuscar, bool esCodigo)
        {
            // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 1 - Reimplementado sobre ResolverClave
            // Una sola definicion de "cual gana": si las dos rutinas resolvieran por su
            // cuenta, el impacto calculado podria no coincidir con lo que hizo la importacion.
            string clave = ResolverClave(dicHomologacion, valorABuscar, esCodigo);

            return clave != null && dicHomologacion.TryGetValue(clave, out string homologado)
                ? homologado
                : null;
        }

        /// <summary>
        /// Aplica el match a un movimiento ya existente. Pisa siempre ConceptoEstandar,
        /// pero respeta un ConceptoFinal que el usuario haya editado a mano en la grilla:
        /// solo lo sobrescribe si seguía pendiente.
        /// No hace nada si no hubo match.
        /// </summary>
        public static void AplicarA(MovimientoProcesado mov, bool esCodigo, IDictionary<string, string> dicHomologacion)
        {
            string valorABuscar = mov.ConceptoOriginal;

            string homologado = Resolver(dicHomologacion, valorABuscar, esCodigo);
            if (homologado == null) return;

            EscribirConcepto(mov, homologado);
        }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 6 - Unica regla de escritura del concepto
        // Estaba duplicada con criterios distintos: AplicarA solo miraba EstaPendiente, y el
        // despegue de la re-homologacion pisaba ConceptoFinal sin mirar nada, borrando en
        // silencio lo que el usuario habia corregido a mano en la grilla.
        /// <summary>
        /// Escribe el concepto en el movimiento. <see cref="MovimientoProcesado.ConceptoEstandar"/>
        /// siempre; <see cref="MovimientoProcesado.ConceptoFinal"/> sólo si estaba pendiente o
        /// si venía siguiendo al ConceptoEstandar viejo — una edición manual no se pisa.
        /// </summary>
        /// <returns>True si algo cambió, para no persistir movimientos que quedaron iguales.</returns>
        public static bool EscribirConcepto(MovimientoProcesado mov, string conceptoNuevo)
        {
            string estandarViejo = mov.ConceptoEstandar;

            // Se evalúa ANTES de pisar ConceptoEstandar: después ya no se sabría si el
            // ConceptoFinal venía siguiendo al estándar o lo había escrito el usuario.
            bool finalSeguiaAlEstandar =
                ConceptosBancarios.EstaPendiente(mov.ConceptoFinal) ||
                string.Equals(mov.ConceptoFinal, estandarViejo, StringComparison.OrdinalIgnoreCase);

            bool cambio = false;

            if (!string.Equals(estandarViejo, conceptoNuevo, StringComparison.OrdinalIgnoreCase))
            {
                mov.ConceptoEstandar = conceptoNuevo;
                cambio = true;
            }

            if (finalSeguiaAlEstandar &&
                !string.Equals(mov.ConceptoFinal, conceptoNuevo, StringComparison.OrdinalIgnoreCase))
            {
                mov.ConceptoFinal = conceptoNuevo;
                cambio = true;
            }

            return cambio;
        }
    }
}
