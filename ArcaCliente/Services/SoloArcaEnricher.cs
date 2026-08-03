using System.Collections.Generic;
using System.Linq;
using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Resuelve, para los items en estado SoloARCA de una conciliación, el tipo Octosis,
    /// la letra y la moneda — el paso previo a poder darlos de alta en Octosis.
    /// Estaba copiado entero en FormComprobantes y FormComprobantesOffline; las copias ya
    /// habían divergido cosméticamente (bullet U+2022 vs middle dot U+00B7 en el mensaje de
    /// tipos desconocidos), señal de que un cambio en una no se estaba replicando a la otra.
    /// </summary>
    public static class SoloArcaEnricher
    {
        /// <summary>
        /// Enriquece in-place los items SoloARCA (TipoOctosis, Letra, Moneda) y devuelve los
        /// códigos de tipo de comprobante que TipoComprobanteMapper no reconoce, para que el
        /// llamador decida cómo avisarlo (no muestra UI).
        /// </summary>
        public static List<string> Enriquecer(IEnumerable<ItemConciliacion> items)
        {
            var soloArca = items.Where(x => x.Estado == EstadoConciliacion.SoloARCA).ToList();

            foreach (var item in soloArca)
            {
                var parseado = TipoComprobanteMapper.Parse(item.TipoComprobante.PadLeft(3, '0'));
                item.TipoOctosis = parseado.TipoOctosis;
                item.Letra = parseado.Letra;

                if (item.SourceArca != null)
                    item.Moneda = MonedaResolver.Resolver(item.SourceArca);
            }

            return TipoComprobanteMapper
                .ObtenerDesconocidos(soloArca.Select(x => x.TipoComprobante.PadLeft(3, '0')))
                .ToList();
        }
    }
}
