using System.ComponentModel;

namespace ArcaCliente.Models
{
    /// <summary>
    /// Linea de exportacion a PRESEA resuelta para un comprobante SOLO ARCA (Fase 4).
    /// Los campos derivables del CSV de ARCA (IVA, importe, fecha, sucursal+numero) los
    /// calcula el generador en el momento. Aqui viven los datos resueltos desde la config
    /// general + el mapa por proveedor, mas los que el usuario puede editar.
    /// </summary>
    public class PreseaLineaExport
    {
        [Browsable(false)] public ItemConciliacion Item { get; set; }
        [Browsable(false)] public ComprobanteCsv  Csv  { get; set; }
        [Browsable(false)] public string Cuit  { get; set; }
        [Browsable(false)] public string Clave { get; set; }
        [Browsable(false)] public bool   EsNotaCredito { get; set; }
        [Browsable(false)] public bool   ProveedorEnMapa { get; set; }

        // Solo lectura en la grilla (informativos)
        public string Comprobante { get; set; }
        public string Emisor      { get; set; }
        public string Importe     { get; set; }

        // Editables
        public string CodigoProveedor { get; set; }
        public string CuentaProveedor { get; set; }
        public string CuentaDebe      { get; set; }
        public string Centro          { get; set; }
        public string VencimientoCai  { get; set; }
        public string Observacion     { get; set; }
        public string Estado          { get; set; }

        // Resueltos internos (no se muestran; se aplican al exportar)
        [Browsable(false)] public string  CuentaIVA  { get; set; }
        [Browsable(false)] public string  CuentaIVA2 { get; set; }
        [Browsable(false)] public string  Provincia  { get; set; }
        [Browsable(false)] public string  Condicion  { get; set; }
        [Browsable(false)] public string  Fiscal     { get; set; }
        [Browsable(false)] public string  Imputa     { get; set; }
        [Browsable(false)] public decimal Descuento  { get; set; }
    }
}
