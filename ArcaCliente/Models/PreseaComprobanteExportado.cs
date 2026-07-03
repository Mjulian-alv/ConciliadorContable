using System;

namespace ArcaCliente.Models
{
    /// <summary>
    /// Registro de la memoria anti-duplicado: un comprobante ya exportado a PRESEA.
    /// Evita volver a exportar el mismo comprobante dos veces.
    /// </summary>
    public class PreseaComprobanteExportado
    {
        /// <summary>Clave de identidad "{cuit}|{tipo}|{ptoVta}|{nro}" (ver <see cref="ArcaCliente.Services.ComprobanteClave"/>).</summary>
        public string Clave { get; set; } = string.Empty;

        /// <summary>CUIT del emisor (solo digitos).</summary>
        public string CuitEmisor { get; set; } = string.Empty;

        /// <summary>Codigo AFIP de tipo de comprobante.</summary>
        public string TipoCmp { get; set; } = string.Empty;

        /// <summary>Punto de venta.</summary>
        public string PtoVta { get; set; } = string.Empty;

        /// <summary>Numero de comprobante.</summary>
        public string Nro { get; set; } = string.Empty;

        /// <summary>Codigo de autorizacion CAE/CAEA (control secundario de unicidad a nivel AFIP).</summary>
        public string CodAut { get; set; } = string.Empty;

        /// <summary>Importe total exportado.</summary>
        public decimal Importe { get; set; }

        /// <summary>Fecha del comprobante (texto, tal como vino de ARCA).</summary>
        public string FechaComprobante { get; set; } = string.Empty;

        /// <summary>Momento en que se genero la exportacion.</summary>
        public DateTime FechaExportacion { get; set; }

        /// <summary>Ruta/nombre del archivo TXT generado.</summary>
        public string ArchivoGenerado { get; set; } = string.Empty;

        /// <summary>Id del perfil offline que origino la exportacion.</summary>
        public string PerfilOfflineId { get; set; } = string.Empty;
    }
}
