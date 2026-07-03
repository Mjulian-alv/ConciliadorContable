using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ArcaCliente.Models
{
    public class ExportarRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Cuit { get; set; }
        public string FechaInicio { get; set; }
        public string FechaFin { get; set; }
    }

    public class ExportarResponse : ArcaBaseResponse
    {
        public List<string> DownloadedFiles { get; set; }
        public int TotalRegistros { get; set; }
        public List<ComprobanteCsv> Comprobantes { get; set; }
        public List<string> Screenshots { get; set; }
    }

    public class ComprobanteCsv
    {
        public string FechaEmision { get; set; }
        public string TipoComprobante { get; set; }
        public string PuntoVenta { get; set; }
        public string NumeroDesde { get; set; }
        public string NumeroHasta { get; set; }
        public string CodAutorizacion { get; set; }
        public string TipoDocEmisor { get; set; }
        public string NroDocEmisor { get; set; }
        public string DenominacionEmisor { get; set; }
        public string TipoDocReceptor { get; set; }
        public string NroDocReceptor { get; set; }
        public string TipoCambio { get; set; }
        public string Moneda { get; set; }
        public string ImpNetoGravadoIVA0 { get; set; }
        [JsonPropertyName("ivA25")]
        public string Iva25 { get; set; }
        public string ImpNetoGravadoIVA25 { get; set; }
        [JsonPropertyName("ivA5")]
        public string Iva5 { get; set; }
        public string ImpNetoGravadoIVA5 { get; set; }
        [JsonPropertyName("ivA105")]
        public string Iva105 { get; set; }
        public string ImpNetoGravadoIVA105 { get; set; }      
        [JsonPropertyName("ivA21")]
        public string Iva21 { get; set; }
        public string ImpNetoGravadoIVA21 { get; set; }
        [JsonPropertyName("ivA27")]
        public string Iva27 { get; set; }
        public string ImpNetoGravadoIVA27 { get; set; }
        public string ImpNetoGravadoTotal { get; set; }
        public string ImpNetoNoGravado { get; set; }
        public string ImpOpExentas { get; set; }
        public string OtrosTributos { get; set; }
        public string TotalIVA { get; set; }
        public string ImpTotal                  { get; set; }
        public string DescripcionTipoComprobante { get; set; }
    }
}
