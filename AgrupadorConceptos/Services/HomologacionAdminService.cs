// Fecha: 28/08/2026 - TAREA: 00003 - Linea: 1 - Gestion de homologaciones: impacto y propagacion
using System;
using System.Collections.Generic;
using System.Linq;
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

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 5 - Repartir el impacto por archivo importado
        /// <summary>
        /// Reparte un impacto ya calculado entre los archivos importados del perfil, para que
        /// el usuario vea sobre qué extractos cae la baja antes de confirmarla. Los importes
        /// son los de los movimientos sin regla: los únicos que efectivamente cambian.
        /// </summary>
        public static List<ImpactoPorArchivo> DesglosarPorArchivo(ImpactoHomologacion impacto, int idPerfilBanco)
        {
            if (impacto == null || impacto.Total == 0) return new List<ImpactoPorArchivo>();

            var nombres = ArchivoImportadoStorage.ObtenerPorPerfil(idPerfilBanco)
                .ToDictionary(a => a.Id, a => a.DisplayName);

            var porArchivo = new Dictionary<int, ImpactoPorArchivo>();

            ImpactoPorArchivo FilaDe(MovimientoProcesado mov)
            {
                if (!porArchivo.TryGetValue(mov.IdArchivo, out var fila))
                {
                    fila = new ImpactoPorArchivo
                    {
                        // Un archivo borrado no puede tener movimientos (FK con ON DELETE
                        // CASCADE), pero el fallback evita una fila sin nombre si algun dia
                        // se toca el schema.
                        Archivo = nombres.TryGetValue(mov.IdArchivo, out string nombre)
                            ? nombre
                            : $"(archivo {mov.IdArchivo})"
                    };
                    porArchivo[mov.IdArchivo] = fila;
                }
                return fila;
            }

            foreach (var item in impacto.Afectados)
            {
                var fila = FilaDe(item.Movimiento);
                fila.SinRegla++;
                fila.Debitos += item.Movimiento.Debitos;
                fila.Creditos += item.Movimiento.Creditos;
            }

            foreach (var item in impacto.CubiertosPorOtraRegla)
                FilaDe(item.Movimiento).OtraRegla++;

            return porArchivo.Values
                .OrderByDescending(f => f.SinRegla)
                .ThenBy(f => f.Archivo)
                .ToList();
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

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Devuelve el Id para que el llamador persista la cuenta
        /// <summary>
        /// Reapunta la regla a otro concepto y arrastra los movimientos que hoy resuelve.
        /// </summary>
        /// <returns>Id del concepto estándar al que quedó apuntando.</returns>
        public static int Reapuntar(HomologacionListado regla, PerfilBanco perfil, string nombreConcepto)
        {
            var dic = HomologacionStorage.ObtenerDiccionario(perfil.Id);
            var cambiados = new List<MovimientoProcesado>();

            foreach (var mov in MovimientoStorage.ObtenerPorPerfil(perfil.Id))
            {
                string clave = HomologacionMatcher.ResolverClave(dic, mov.ConceptoOriginal, perfil.EsCodigo);
                if (!string.Equals(clave, regla.ValorOriginal, StringComparison.OrdinalIgnoreCase)) continue;

                if (AplicarConcepto(mov, nombreConcepto)) cambiados.Add(mov);
            }

            return HomologacionStorage.ReapuntarYActualizarMovimientos(regla.Id, nombreConcepto, cambiados);
        }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 6 - La regla de escritura se unifico en el matcher
        /// <summary>
        /// Escribe el concepto respetando lo editado a mano. Ver
        /// <see cref="HomologacionMatcher.EscribirConcepto"/>: la regla es una sola y vive ahí,
        /// para que la gestión y la re-homologación desde la grilla no puedan divergir.
        /// </summary>
        private static bool AplicarConcepto(MovimientoProcesado mov, string conceptoNuevo) =>
            HomologacionMatcher.EscribirConcepto(mov, conceptoNuevo);
    }
}
