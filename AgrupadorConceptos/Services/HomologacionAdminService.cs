// Fecha: 28/08/2026 - TAREA: 00003 - Linea: 1 - Gestion de homologaciones: impacto y propagacion
using System;
using System.Collections.Generic;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;

namespace AgrupadorConceptos.Services
{
    /// <summary>
    /// Lo que la pantalla de gestión necesita saber y hacer sobre una homologación:
    /// a cuántos movimientos alcanza, qué pasa si se la borra, y cómo se propaga el cambio.
    ///
    /// No abre conexiones: lee por los Storage, decide en memoria con el matcher y delega
    /// la escritura atómica a HomologacionStorage.
    ///
    /// La fuente de verdad de "qué resuelve cada regla" es el matcher, no el texto guardado
    /// en el movimiento. Si un movimiento arrastraba un concepto viejo porque las reglas
    /// cambiaron desde su importación, estas operaciones se lo recalculan.
    /// </summary>
    internal static class HomologacionAdminService
    {
        /// <summary>
        /// Cuántos movimientos del perfil resuelve hoy cada ValorOriginal. Es el número que
        /// muestra la columna "Movimientos" de la grilla.
        /// </summary>
        public static Dictionary<string, int> ContarUsoPorRegla(PerfilBanco perfil)
        {
            var conteo = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (perfil == null) return conteo;

            var dic = HomologacionStorage.ObtenerDiccionario(perfil.Id);

            foreach (var mov in MovimientoStorage.ObtenerPorPerfil(perfil.Id))
            {
                string clave = HomologacionMatcher.ResolverClave(dic, mov.ConceptoOriginal, perfil.EsCodigo);
                if (clave == null) continue;

                conteo[clave] = conteo.TryGetValue(clave, out int n) ? n + 1 : 1;
            }

            return conteo;
        }

        /// <summary>
        /// Qué le pasa a los movimientos del perfil si se borra esta regla. No escribe nada
        /// ni modifica los movimientos: sólo mira.
        /// </summary>
        public static ImpactoHomologacion CalcularImpactoBaja(HomologacionListado regla, PerfilBanco perfil)
        {
            var impacto = new ImpactoHomologacion();
            if (regla == null || perfil == null) return impacto;

            var dicCompleto = HomologacionStorage.ObtenerDiccionario(perfil.Id);
            var dicSinRegla = HomologacionStorage.ObtenerDiccionario(perfil.Id, regla.Id);

            foreach (var mov in MovimientoStorage.ObtenerPorPerfil(perfil.Id))
            {
                string clave = HomologacionMatcher.ResolverClave(dicCompleto, mov.ConceptoOriginal, perfil.EsCodigo);
                if (!string.Equals(clave, regla.ValorOriginal, StringComparison.OrdinalIgnoreCase)) continue;

                var item = new MovimientoImpactado
                {
                    Movimiento = mov,
                    ConceptoSinLaRegla = HomologacionMatcher.Resolver(dicSinRegla, mov.ConceptoOriginal, perfil.EsCodigo)
                };

                if (item.ConceptoSinLaRegla == null) impacto.Afectados.Add(item);
                else impacto.CubiertosPorOtraRegla.Add(item);
            }

            return impacto;
        }

        /// <summary>
        /// Borra la regla y propaga: los afectados van a <paramref name="conceptoDestino"/>
        /// (o a Pendiente Homologar si es null), y los que otra regla sigue cubriendo se
        /// recalculan con el concepto que les corresponde.
        /// </summary>
        /// <param name="impacto">El mismo que se le mostró al usuario, para que lo que se
        /// ejecuta sea exactamente lo que él aceptó.</param>
        public static void AplicarBaja(HomologacionListado regla, ImpactoHomologacion impacto, string conceptoDestino)
        {
            var cambiados = new List<MovimientoProcesado>();

            string paraAfectados = string.IsNullOrWhiteSpace(conceptoDestino)
                ? ConceptosBancarios.PendienteHomologar
                : conceptoDestino.Trim();

            foreach (var item in impacto.Afectados)
                if (AplicarConcepto(item.Movimiento, paraAfectados)) cambiados.Add(item.Movimiento);

            foreach (var item in impacto.CubiertosPorOtraRegla)
                if (AplicarConcepto(item.Movimiento, item.ConceptoSinLaRegla)) cambiados.Add(item.Movimiento);

            HomologacionStorage.EliminarYActualizarMovimientos(regla.Id, cambiados);
        }

        /// <summary>
        /// Reapunta la regla a otro concepto y arrastra los movimientos que hoy resuelve.
        /// </summary>
        public static void Reapuntar(HomologacionListado regla, PerfilBanco perfil, string nombreConcepto)
        {
            var dic = HomologacionStorage.ObtenerDiccionario(perfil.Id);
            var cambiados = new List<MovimientoProcesado>();

            foreach (var mov in MovimientoStorage.ObtenerPorPerfil(perfil.Id))
            {
                string clave = HomologacionMatcher.ResolverClave(dic, mov.ConceptoOriginal, perfil.EsCodigo);
                if (!string.Equals(clave, regla.ValorOriginal, StringComparison.OrdinalIgnoreCase)) continue;

                if (AplicarConcepto(mov, nombreConcepto)) cambiados.Add(mov);
            }

            HomologacionStorage.ReapuntarYActualizarMovimientos(regla.Id, nombreConcepto, cambiados);
        }

        /// <summary>
        /// Escribe el concepto en el movimiento. ConceptoEstandar siempre; ConceptoFinal sólo
        /// si estaba pendiente o venía igual al ConceptoEstandar viejo: lo que el usuario
        /// editó a mano en la grilla del Procesador no se pisa. Es el mismo criterio que ya
        /// aplica HomologacionMatcher.AplicarA en la importación.
        /// </summary>
        /// <returns>True si algo cambió, para no mandar UPDATEs que no hacen nada.</returns>
        private static bool AplicarConcepto(MovimientoProcesado mov, string conceptoNuevo)
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
