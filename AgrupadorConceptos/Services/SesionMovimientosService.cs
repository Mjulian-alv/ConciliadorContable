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
        /// Devuelve a "Pendiente Homologar" todos los movimientos del archivo que tengan
        /// ese concepto estándar. Se usa al re-homologar un valor que ya estaba mapeado:
        /// los movimientos que arrastraban el concepto viejo tienen que volver a resolverse.
        /// </summary>
        /// <returns>Cantidad de movimientos afectados.</returns>
        public static int MarcarComoPendiente(int idArchivo, string conceptoEstandar)
        {
            var movs = MovimientoStorage.ObtenerPorArchivo(idArchivo);

            var vueltosAPendiente = new List<MovimientoProcesado>();
            foreach (var mov in movs)
            {
                if (mov.ConceptoEstandar != conceptoEstandar) continue;

                mov.ConceptoEstandar = ConceptosBancarios.PendienteHomologar;
                mov.ConceptoFinal = ConceptosBancarios.PendienteHomologar;
                vueltosAPendiente.Add(mov);
            }

            MovimientoStorage.ActualizarConceptos(vueltosAPendiente);
            return vueltosAPendiente.Count;
        }
    }
}
