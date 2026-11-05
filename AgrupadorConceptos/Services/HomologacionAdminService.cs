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
            // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 3 - Los archivos blindados tienen la homologación cerrada: la baja no los toca
            var blindados = ArchivoImportadoStorage.ObtenerIdsBlindados();

            // Fecha: 05/11/2026 - TAREA: 00162 - Linea: 2 - Primero se reúnen los que la regla alcanza y después se pregunta cuáles están apareados, en una sola consulta
            var alcanzados = new List<MovimientoProcesado>();
            foreach (var mov in MovimientoStorage.ObtenerPorPerfil(perfil.Id))
            {
                string clave = HomologacionMatcher.ResolverClave(dicCompleto, mov.ConceptoOriginal, perfil.EsCodigo);
                if (!string.Equals(clave, regla.ValorOriginal, StringComparison.OrdinalIgnoreCase)) continue;
                if (blindados.Contains(mov.IdArchivo)) { impacto.EnArchivosBlindados++; continue; }
                alcanzados.Add(mov);
            }
            var apareados = ParesMovimientoStorage.ObtenerApareados(alcanzados.Select(m => m.Id).ToList());

            foreach (var mov in alcanzados)
            {
                // Fecha: 05/11/2026 - TAREA: 00162 - Linea: 2 - Un movimiento apareado en una conciliación no se toca: se cuenta aparte
                if (apareados.ContainsKey(mov.Id)) { impacto.EnMovimientosApareados++; continue; }

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
            // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 2 - El historial registra que el cambio vino de la baja de una regla
            using var origen = AuditoriaContexto.Usar(AuditoriaContexto.BajaDeRegla);
            var cambiados = new List<MovimientoProcesado>();
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Completar CuentaFinal al propagar la baja
            var cuentasPorConcepto = HomologacionStorage.ObtenerCuentasPorConcepto();

            string paraAfectados = string.IsNullOrWhiteSpace(conceptoDestino)
                ? ConceptosBancarios.PendienteHomologar
                : conceptoDestino.Trim();

            foreach (var item in impacto.Afectados)
                if (AplicarConcepto(item.Movimiento, paraAfectados, cuentasPorConcepto)) cambiados.Add(item.Movimiento);

            foreach (var item in impacto.CubiertosPorOtraRegla)
                if (AplicarConcepto(item.Movimiento, item.ConceptoSinLaRegla, cuentasPorConcepto)) cambiados.Add(item.Movimiento);

            // Fecha: 05/11/2026 - TAREA: 00162 - Linea: 2 - Revisión 1, hallazgo 4: si otra sesión apareó un movimiento después de calcular el impacto, la base lo salteó
            // y el conteo del impacto (calculado antes) no lo sabía: lo que la transacción no escribió se suma a los apareados.
            int escritos = HomologacionStorage.EliminarYActualizarMovimientos(regla.Id, cambiados);
            impacto.EnMovimientosApareados += cambiados.Count - escritos;
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Devuelve el Id para que el llamador persista la cuenta
        /// <summary>
        /// Reapunta la regla a otro concepto y arrastra los movimientos que hoy resuelve.
        /// </summary>
        /// <returns>Id del concepto estándar al que quedó apuntando.</returns>
        public static int Reapuntar(HomologacionListado regla, PerfilBanco perfil, string nombreConcepto) =>
            Reapuntar(regla, perfil, nombreConcepto, out _, out _);

        // Fecha: 05/11/2026 - TAREA: 00162 - Linea: 2 - Se mantiene la firma de la 00135 y se agrega la que además cuenta los apareados
        public static int Reapuntar(HomologacionListado regla, PerfilBanco perfil, string nombreConcepto, out int saltadosPorBlindaje) =>
            Reapuntar(regla, perfil, nombreConcepto, out saltadosPorBlindaje, out _);

        // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 3 - Informar cuántos movimientos de archivos blindados no se arrastraron
        /// <param name="saltadosPorBlindaje">Movimientos que la regla alcanza en archivos blindados y que no se modificaron.</param>
        /// <param name="saltadosApareados">Fecha: 05/11/2026 - TAREA: 00162 - Linea: 2 - Movimientos que la regla alcanza apareados en una conciliación y que no se modificaron.</param>
        public static int Reapuntar(HomologacionListado regla, PerfilBanco perfil, string nombreConcepto,
            out int saltadosPorBlindaje, out int saltadosApareados)
        {
            saltadosPorBlindaje = 0;
            saltadosApareados = 0;
            var blindados = ArchivoImportadoStorage.ObtenerIdsBlindados();
            // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 2 - El historial registra que el cambio vino de reapuntar una regla
            using var origen = AuditoriaContexto.Usar(AuditoriaContexto.ReapuntarRegla);
            var dic = HomologacionStorage.ObtenerDiccionario(perfil.Id);
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Completar CuentaFinal al reapuntar
            var cuentasPorConcepto = HomologacionStorage.ObtenerCuentasPorConcepto();
            var cambiados = new List<MovimientoProcesado>();

            // Fecha: 05/11/2026 - TAREA: 00162 - Linea: 2 - Primero se reúnen los que la regla alcanza y después se pregunta cuáles están apareados, en una sola consulta
            var alcanzados = new List<MovimientoProcesado>();
            foreach (var mov in MovimientoStorage.ObtenerPorPerfil(perfil.Id))
            {
                string clave = HomologacionMatcher.ResolverClave(dic, mov.ConceptoOriginal, perfil.EsCodigo);
                if (!string.Equals(clave, regla.ValorOriginal, StringComparison.OrdinalIgnoreCase)) continue;
                if (blindados.Contains(mov.IdArchivo)) { saltadosPorBlindaje++; continue; }
                alcanzados.Add(mov);
            }
            var apareados = ParesMovimientoStorage.ObtenerApareados(alcanzados.Select(m => m.Id).ToList());

            foreach (var mov in alcanzados)
            {
                // Fecha: 05/11/2026 - TAREA: 00162 - Linea: 2 - Un movimiento apareado en una conciliación no se arrastra: se cuenta aparte
                if (apareados.ContainsKey(mov.Id)) { saltadosApareados++; continue; }
                if (AplicarConcepto(mov, nombreConcepto, cuentasPorConcepto)) cambiados.Add(mov);
            }

            // Fecha: 05/11/2026 - TAREA: 00162 - Linea: 2 - Revisión 1, hallazgo 4: se suma lo que la transacción no escribió (apareado después de reunir los movimientos)
            int idConcepto = HomologacionStorage.ReapuntarYActualizarMovimientos(regla.Id, nombreConcepto, cambiados, out int escritos);
            saltadosApareados += cambiados.Count - escritos;
            return idConcepto;
        }

        // Fecha: 05/11/2026 - TAREA: 00162 - Linea: 2 - Un solo texto para escritorio y web: cuántos movimientos no se tocaron y por qué
        /// <summary>
        /// El aviso de lo que una regla del perfil no modificó: movimientos de archivos blindados y movimientos apareados en una
        /// conciliación. Null si no se salteó ninguno.
        /// </summary>
        public static string AvisoNoModificados(int enArchivosBlindados, int enMovimientosApareados)
        {
            var c = System.Globalization.CultureInfo.CurrentCulture;
            if (enArchivosBlindados > 0 && enMovimientosApareados > 0)
                return $"{enArchivosBlindados.ToString("N0", c)} movimiento(s) de archivos blindados y {enMovimientosApareados.ToString("N0", c)} apareados en una conciliación no se modificaron.";
            if (enArchivosBlindados > 0)
                return $"{enArchivosBlindados.ToString("N0", c)} movimiento(s) de archivos blindados no se modificaron.";
            if (enMovimientosApareados > 0)
                return $"{enMovimientosApareados.ToString("N0", c)} movimiento(s) apareados en una conciliación no se modificaron.";
            return null;
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Tambien completa CuentaFinal, sin pisar lo editado
        /// <summary>
        /// Escribe el concepto y, si corresponde, la cuenta — respetando lo editado a mano.
        /// Ver <see cref="HomologacionMatcher.EscribirConcepto"/> y
        /// <see cref="HomologacionMatcher.EscribirCuenta"/>.
        /// </summary>
        private static bool AplicarConcepto(
            MovimientoProcesado mov, string conceptoNuevo, IDictionary<string, string> cuentasPorConcepto)
        {
            bool cambioConcepto = HomologacionMatcher.EscribirConcepto(mov, conceptoNuevo);
            bool cambioCuenta = cuentasPorConcepto.TryGetValue(conceptoNuevo, out string cuenta) &&
                                HomologacionMatcher.EscribirCuenta(mov, cuenta);
            return cambioConcepto || cambioCuenta;
        }
    }
}
