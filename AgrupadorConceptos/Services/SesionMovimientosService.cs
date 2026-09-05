using System;
using System.Collections.Generic;
using System.Linq;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;

namespace AgrupadorConceptos.Services
{
    /// <summary>
    /// Operaciones sobre los movimientos ya importados de un archivo.
    /// </summary>
    public static class SesionMovimientosService
    {
        /// <summary>
        /// Vuelve a pasar la homologación sobre los movimientos que siguen pendientes,
        /// por si se agregaron homologaciones desde la última carga, y persiste el resultado.
        /// </summary>
        /// <returns>Todos los movimientos del archivo, con los pendientes ya resueltos.</returns>
        public static List<MovimientoProcesado> RehomologarPendientes(int idArchivo, PerfilBanco perfil)
        {
            var movs = MovimientoStorage.ObtenerPorArchivo(idArchivo);
            var dicHomologacion = HomologacionStorage.ObtenerDiccionario(perfil.Id);
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Completar CuentaFinal al rehomologar
            var cuentasPorConcepto = HomologacionStorage.ObtenerCuentasPorConcepto();

            var rehomologados = new List<MovimientoProcesado>();
            foreach (var mov in movs)
            {
                if (mov.ConceptoEstandar != ConceptosBancarios.PendienteHomologar) continue;

                HomologacionMatcher.AplicarA(mov, perfil.EsCodigo, dicHomologacion, cuentasPorConcepto);
                rehomologados.Add(mov);
            }

            MovimientoStorage.ActualizarConceptos(rehomologados);
            return movs;
        }

        /// <summary>
        /// Re-aplica las homologaciones del perfil sobre la lista que ya está en memoria
        /// (la que tiene bindeada la grilla) y persiste sólo los movimientos que cambiaron.
        ///
        /// A diferencia de <see cref="RehomologarPendientes"/> no vuelve a leer de la base:
        /// el llamador conserva la misma lista y las mismas instancias, así puede refrescar
        /// la vista sin rebindear la grilla (sin perder el archivo en pantalla ni la fila
        /// donde está parado el usuario).
        /// </summary>
        /// <returns>Los movimientos que quedaron modificados.</returns>
        public static ISet<MovimientoProcesado> RehomologarEnMemoria(
            List<MovimientoProcesado> movs, PerfilBanco perfil)
        {
            var cambiados = new HashSet<MovimientoProcesado>();
            if (movs == null || movs.Count == 0) return cambiados;

            var dicHomologacion = HomologacionStorage.ObtenerDiccionario(perfil.Id);
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Completar CuentaFinal al rehomologar
            var cuentasPorConcepto = HomologacionStorage.ObtenerCuentasPorConcepto();
            foreach (var mov in movs)
            {
                if (mov.ConceptoEstandar != ConceptosBancarios.PendienteHomologar) continue;

                HomologacionMatcher.AplicarA(mov, perfil.EsCodigo, dicHomologacion, cuentasPorConcepto);
                cambiados.Add(mov);
            }

            MovimientoStorage.ActualizarConceptos(cambiados);
            return cambiados;
        }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 6 - Atribuir por regla, no por texto de concepto
        // Reemplaza al viejo "conceptoADespegar", que devolvia a pendiente todos los
        // movimientos con ese texto de concepto: despegaba tambien los que venian de OTRAS
        // reglas que apuntan al mismo concepto. Se recuperaban solos al re-resolver, pero en
        // esa ida y vuelta se perdian las ediciones manuales de ConceptoFinal.
        /// <summary>
        /// Movimientos de la lista que resuelve la misma homologación que
        /// <paramref name="valorOriginal"/>, según las reglas vigentes.
        ///
        /// Se llama **después** de guardar la homologación, pasándole el valor con el que se
        /// guardó: el conjunto que devuelve son los que esa regla cubre ahora, que es
        /// exactamente lo que hay que re-resolver con <see cref="ReaplicarHomologacion"/>.
        /// Entran también los que tenían un concepto huérfano de una regla ya borrada.
        /// </summary>
        public static List<MovimientoProcesado> MovimientosDeLaMismaRegla(
            List<MovimientoProcesado> movs, PerfilBanco perfil, string valorOriginal)
        {
            var resultado = new List<MovimientoProcesado>();
            if (movs == null || movs.Count == 0) return resultado;

            var dic = HomologacionStorage.ObtenerDiccionario(perfil.Id);

            string clave = HomologacionMatcher.ResolverClave(dic, valorOriginal, perfil.EsCodigo);
            if (clave == null) return resultado;

            foreach (var mov in movs)
            {
                string claveDelMov = HomologacionMatcher.ResolverClave(dic, mov.ConceptoOriginal, perfil.EsCodigo);
                if (string.Equals(claveDelMov, clave, StringComparison.OrdinalIgnoreCase))
                    resultado.Add(mov);
            }

            return resultado;
        }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 6 - Re-resolver despues de cambiar la regla
        /// <summary>
        /// Vuelve a resolver estos movimientos con las homologaciones vigentes y persiste los
        /// que cambiaron. Si ninguna regla los toma, quedan pendientes.
        ///
        /// A diferencia del despegue viejo, no los manda a pendiente para volver a levantarlos:
        /// escribe directo el concepto que corresponde, así <see cref="MovimientoProcesado.ConceptoFinal"/>
        /// editado a mano nunca pasa por un estado intermedio en el que se pierda.
        /// </summary>
        public static ISet<MovimientoProcesado> ReaplicarHomologacion(
            IEnumerable<MovimientoProcesado> movs, PerfilBanco perfil)
        {
            var cambiados = new HashSet<MovimientoProcesado>();
            if (movs == null) return cambiados;

            var dic = HomologacionStorage.ObtenerDiccionario(perfil.Id);
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Completar CuentaFinal al reaplicar
            var cuentasPorConcepto = HomologacionStorage.ObtenerCuentasPorConcepto();

            foreach (var mov in movs)
            {
                string concepto = HomologacionMatcher.Resolver(dic, mov.ConceptoOriginal, perfil.EsCodigo)
                                  ?? ConceptosBancarios.PendienteHomologar;

                bool cambioConcepto = HomologacionMatcher.EscribirConcepto(mov, concepto);
                bool cambioCuenta = cuentasPorConcepto.TryGetValue(concepto, out string cuenta) &&
                                     HomologacionMatcher.EscribirCuenta(mov, cuenta);

                if (cambioConcepto || cambioCuenta) cambiados.Add(mov);
            }

            MovimientoStorage.ActualizarConceptos(cambiados);
            return cambiados;
        }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 4 - Traer lo que cambio la gestion sin rebindear
        /// <summary>
        /// Vuelve a leer los conceptos del archivo y los copia sobre las instancias que la
        /// grilla ya tiene bindeadas, macheando por Id.
        ///
        /// No reemplaza la lista a propósito: el llamador refresca la vista sin rebindear y
        /// el usuario no pierde la fila donde estaba parado. Se usa al volver de la gestión
        /// de homologaciones, que escribió directo en la base sobre todos los archivos del
        /// perfil y por lo tanto también sobre el que está en pantalla.
        /// </summary>
        public static void RefrescarDesdeBase(List<MovimientoProcesado> movs, int idArchivo)
        {
            if (movs == null || movs.Count == 0) return;

            var enBase = MovimientoStorage.ObtenerPorArchivo(idArchivo).ToDictionary(m => m.Id);

            foreach (var mov in movs)
            {
                if (!enBase.TryGetValue(mov.Id, out var actualizado)) continue;

                mov.ConceptoEstandar = actualizado.ConceptoEstandar;
                mov.ConceptoFinal = actualizado.ConceptoFinal;
                // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Sincronizar tambien CuentaFinal al refrescar desde la base
                mov.CuentaFinal = actualizado.CuentaFinal;
            }
        }
    }
}
