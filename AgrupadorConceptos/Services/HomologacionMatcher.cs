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
        /// <summary>
        /// Devuelve el concepto estándar homologado, o null si no hay match.
        /// </summary>
        /// <param name="dicHomologacion">ValorOriginal → ConceptoEstandar. Se espera
        /// construido con StringComparer.OrdinalIgnoreCase y en orden estable
        /// (ver HomologacionStorage), porque con varias claves candidatas gana la primera.</param>
        public static string Resolver(IDictionary<string, string> dicHomologacion, string valorABuscar, bool esCodigo)
        {
            if (dicHomologacion == null || string.IsNullOrEmpty(valorABuscar))
                return null;

            if (esCodigo)
                return dicHomologacion.TryGetValue(valorABuscar, out string homologado) ? homologado : null;

            var match = dicHomologacion.FirstOrDefault(
                d => valorABuscar.IndexOf(d.Key, StringComparison.OrdinalIgnoreCase) >= 0);

            return match.Key != null ? match.Value : null;
        }

        /// <summary>
        /// Aplica el match a un movimiento ya existente. Pisa siempre ConceptoEstandar,
        /// pero respeta un ConceptoFinal que el usuario haya editado a mano en la grilla:
        /// solo lo sobrescribe si seguía pendiente.
        /// No hace nada si no hubo match.
        /// </summary>
        public static void AplicarA(MovimientoProcesado mov, bool esCodigo, IDictionary<string, string> dicHomologacion)
        {
            string valorABuscar = esCodigo ? mov.ConceptoOriginal : mov.DescripcionOriginal;

            string homologado = Resolver(dicHomologacion, valorABuscar, esCodigo);
            if (homologado == null) return;

            mov.ConceptoEstandar = homologado;

            if (ConceptosBancarios.EstaPendiente(mov.ConceptoFinal))
                mov.ConceptoFinal = homologado;
        }
    }
}
