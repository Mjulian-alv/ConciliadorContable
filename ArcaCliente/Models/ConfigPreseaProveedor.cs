namespace ArcaCliente.Models
{
    /// <summary>
    /// Datos de PRESEA que dependen del proveedor (emisor), indexados por CUIT.
    /// Se cargan/actualizan desde los CSV maestros de PRESEA o se completan a mano cuando
    /// aparece un CUIT nuevo. Cuando un campo queda vacio, la exportacion usa el default
    /// general definido en <see cref="ConfigPresea"/>.
    /// </summary>
    public class ConfigPreseaProveedor
    {
        /// <summary>CUIT del emisor (solo digitos). Clave del mapa.</summary>
        public string Cuit { get; set; } = string.Empty;

        /// <summary>Nombre del proveedor (cache para mostrar; no se exporta).</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Codigo de proveedor en PRESEA (campo Proveedor I/8).</summary>
        public string CodigoProveedor { get; set; } = string.Empty;

        /// <summary>Cuenta contable del proveedor (campo Cuenta contable proveedor B/16).</summary>
        public string CuentaContableProveedor { get; set; } = string.Empty;

        /// <summary>Cuenta contable que se registra en el Debe / gasto (campo Cuenta contable del debe B/16).</summary>
        public string CuentaDebe { get; set; } = string.Empty;

        /// <summary>Centro de costos para la cuenta de resultados (campo Centro C/10).</summary>
        public string Centro { get; set; } = string.Empty;

        /// <summary>Codigo de provincia en PRESEA (campo Provincia I/3).</summary>
        public string Provincia { get; set; } = string.Empty;

        /// <summary>Codigo de condicion de pago en PRESEA (campo Condicion I/5).</summary>
        public string Condicion { get; set; } = string.Empty;

        /// <summary>Porcentaje de descuento por pronto pago (campo Descuento B/-12,2).</summary>
        public decimal Descuento { get; set; }

        /// <summary>"S" o "N" segun si el comprobante es de impresor fiscal (campo Fiscal C/1). Vacio = usar default general.</summary>
        public string Fiscal { get; set; } = string.Empty;
    }
}
