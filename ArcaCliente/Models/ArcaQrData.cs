namespace ArcaCliente.Models
{
    /// <summary>
    /// Datos contenidos en el QR de un comprobante electronico de ARCA/AFIP.
    /// El QR NO esta encriptado: es la URL https://www.afip.gob.ar/fe/qr/?p=&lt;BASE64&gt;
    /// donde &lt;BASE64&gt; es un JSON codificado en Base64. Se parsea 100% offline.
    /// Ver <see cref="ArcaCliente.Services.ArcaQrParser"/>.
    /// </summary>
    public class ArcaQrData
    {
        /// <summary>Version del formato del QR (campo "ver").</summary>
        public int Version { get; set; }

        /// <summary>Fecha de emision en formato "yyyy-MM-dd" (campo "fecha").</summary>
        public string Fecha { get; set; }

        /// <summary>CUIT del emisor del comprobante (campo "cuit").</summary>
        public string Cuit { get; set; }

        /// <summary>Punto de venta (campo "ptoVta").</summary>
        public int PtoVta { get; set; }

        /// <summary>Codigo AFIP de tipo de comprobante (campo "tipoCmp"). 1 = Factura A, 6 = Factura B, etc.</summary>
        public int TipoCmp { get; set; }

        /// <summary>Numero de comprobante (campo "nroCmp").</summary>
        public long NroCmp { get; set; }

        /// <summary>Importe total del comprobante (campo "importe").</summary>
        public decimal Importe { get; set; }

        /// <summary>Codigo de moneda ARCA: "PES", "DOL", etc. (campo "moneda").</summary>
        public string Moneda { get; set; }

        /// <summary>Cotizacion de la moneda (campo "ctz").</summary>
        public decimal Cotizacion { get; set; }

        /// <summary>Tipo de documento del receptor (campo "tipoDocRec").</summary>
        public int TipoDocRec { get; set; }

        /// <summary>Numero de documento del receptor (campo "nroDocRec").</summary>
        public string NroDocRec { get; set; }

        /// <summary>Tipo de codigo de autorizacion: "E" = CAE, "A" = CAEA (campo "tipoCodAut").</summary>
        public string TipoCodAut { get; set; }

        /// <summary>Codigo de autorizacion CAE/CAEA (campo "codAut").</summary>
        public string CodAut { get; set; }

        /// <summary>
        /// Clave normalizada de identidad del comprobante, usada para matchear contra los
        /// <see cref="ComprobanteCsv"/> de ARCA y contra la memoria de exportados.
        /// Formato: "{cuit}|{tipo}|{ptoVta}|{nro}" (todos sin ceros a la izquierda).
        /// </summary>
        public string Clave =>
            Services.ComprobanteClave.Generar(Cuit, TipoCmp.ToString(), PtoVta.ToString(), NroCmp.ToString());
    }
}
