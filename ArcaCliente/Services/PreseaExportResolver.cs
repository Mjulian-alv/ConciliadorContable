using System.Collections.Generic;
using System.Linq;
using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Construye las <see cref="PreseaLineaExport"/> a partir de los items SOLO ARCA
    /// seleccionados, resolviendo cada campo con la cascada:
    /// mapa por proveedor (por CUIT) -> defaults generales (<see cref="ConfigPresea"/>).
    /// </summary>
    public static class PreseaExportResolver
    {
        public static List<PreseaLineaExport> Resolver(IEnumerable<ItemConciliacion> items, ConfigPresea cfg)
        {
            cfg ??= new ConfigPresea();
            var result = new List<PreseaLineaExport>();

            foreach (var item in items)
            {
                var csv = item.SourceArca;

                string cuitRaw = !string.IsNullOrWhiteSpace(csv?.NroDocEmisor) ? csv.NroDocEmisor : item.CuitProveedor;
                string cuit = TextParsingUtils.SoloDigitos(cuitRaw);
                string tipo = !string.IsNullOrWhiteSpace(csv?.TipoComprobante) ? csv.TipoComprobante : item.TipoComprobante;
                string pv   = !string.IsNullOrWhiteSpace(csv?.PuntoVenta) ? csv.PuntoVenta : item.PuntoVenta;
                string nro  = !string.IsNullOrWhiteSpace(csv?.NumeroDesde) ? csv.NumeroDesde : item.Numero;

                var prov  = AppServices.GetPreseaProveedor(cuit);
                bool esNC = TipoComprobanteMapper.Parse((tipo ?? string.Empty).PadLeft(3, '0')).TipoOctosis == "3";

                string descTipo = !string.IsNullOrWhiteSpace(item.DescripcionTipoComprobante)
                    ? item.DescripcionTipoComprobante
                    : TablaComprobanteDescripcionMapper.ObtenerDescripcion(tipo);

                bool enMapa = prov != null && !string.IsNullOrWhiteSpace(prov.CodigoProveedor);

                result.Add(new PreseaLineaExport
                {
                    Item            = item,
                    Csv             = csv,
                    Cuit            = cuit,
                    Clave           = ComprobanteClave.Generar(cuit, tipo, pv, nro),
                    EsNotaCredito   = esNC,
                    ProveedorEnMapa = enMapa,

                    Comprobante = $"{descTipo} {(pv ?? "").PadLeft(4, '0')}-{(nro ?? "").PadLeft(8, '0')}",
                    Emisor      = item.Emisor ?? csv?.DenominacionEmisor ?? string.Empty,
                    Importe     = item.TotalARCA ?? csv?.ImpTotal ?? string.Empty,

                    CodigoProveedor = Coalesce(prov?.CodigoProveedor, cfg.CodigoProveedor),
                    CuentaProveedor = Coalesce(prov?.CuentaContableProveedor, cfg.CuentaContableProveedor),
                    CuentaDebe      = Coalesce(prov?.CuentaDebe, cfg.CuentaDebe),
                    Centro          = Coalesce(prov?.Centro, cfg.Centro),
                    VencimientoCai  = string.Empty,
                    Observacion     = string.Empty,
                    Estado          = enMapa ? "Proveedor OK" : "Revisar proveedor",

                    CuentaIVA  = cfg.CuentaIVA,
                    CuentaIVA2 = cfg.CuentaIVA2,
                    Provincia  = Coalesce(prov?.Provincia, cfg.Provincia),
                    Condicion  = Coalesce(prov?.Condicion, cfg.Condicion),
                    Fiscal     = Coalesce(prov?.Fiscal, cfg.Fiscal),
                    Imputa     = cfg.Imputa,
                    Descuento  = prov?.Descuento ?? 0m,
                });
            }

            return result;
        }

        private static string Coalesce(string preferido, string fallback) =>
            !string.IsNullOrWhiteSpace(preferido) ? preferido.Trim() : (fallback ?? string.Empty).Trim();
    }
}
