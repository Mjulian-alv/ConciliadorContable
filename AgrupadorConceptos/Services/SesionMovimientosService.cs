using System.Collections.Generic;
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

            var rehomologados = new List<MovimientoProcesado>();
            foreach (var mov in movs)
            {
                if (mov.ConceptoEstandar != ConceptosBancarios.PendienteHomologar) continue;

                HomologacionMatcher.AplicarA(mov, perfil.EsCodigo, dicHomologacion);
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
        /// <param name="conceptoADespegar">
        /// Concepto estándar que hay que devolver a "Pendiente Homologar" antes de re-homologar.
        /// Se usa cuando el usuario re-homologa un valor que ya estaba mapeado: los movimientos
        /// que arrastraban el concepto viejo tienen que volver a resolverse. Null si no aplica.
        /// </param>
        /// <returns>Los movimientos que quedaron modificados.</returns>
        public static ISet<MovimientoProcesado> RehomologarEnMemoria(
            List<MovimientoProcesado> movs, PerfilBanco perfil, string conceptoADespegar = null)
        {
            // HashSet por referencia: un movimiento despegado y vuelto a homologar
            // no tiene que persistirse dos veces.
            var cambiados = new HashSet<MovimientoProcesado>();
            if (movs == null || movs.Count == 0) return cambiados;

            if (!string.IsNullOrEmpty(conceptoADespegar))
            {
                foreach (var mov in movs)
                {
                    if (mov.ConceptoEstandar != conceptoADespegar) continue;

                    mov.ConceptoEstandar = ConceptosBancarios.PendienteHomologar;
                    mov.ConceptoFinal = ConceptosBancarios.PendienteHomologar;
                    cambiados.Add(mov);
                }
            }

            var dicHomologacion = HomologacionStorage.ObtenerDiccionario(perfil.Id);
            foreach (var mov in movs)
            {
                if (mov.ConceptoEstandar != ConceptosBancarios.PendienteHomologar) continue;

                HomologacionMatcher.AplicarA(mov, perfil.EsCodigo, dicHomologacion);
                cambiados.Add(mov);
            }

            MovimientoStorage.ActualizarConceptos(cambiados);
            return cambiados;
        }
    }
}
