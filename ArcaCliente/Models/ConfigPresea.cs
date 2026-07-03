namespace ArcaCliente.Models
{
    /// <summary>
    /// Valores por defecto para los campos del Excel de importaci�n de PRESEA
    /// que no pueden derivarse directamente de los datos del CSV de ARCA.
    /// Todos los campos son editables por el usuario en el perfil offline.
    /// </summary>
    public class ConfigPresea
    {
        /// <summary>C�digo de proveedor en PRESEA (campo Proveedor I/8).</summary>
        public string CodigoProveedor { get; set; } = "0";

        /// <summary>Cuenta contable del proveedor (campo Cuenta contable proveedor B/16).</summary>
        public string CuentaContableProveedor { get; set; } = string.Empty;

        /// <summary>C�digo de provincia en PRESEA (campo Provincia I/3).</summary>
        public string Provincia { get; set; } = "0";

        /// <summary>C�digo de condici�n de pago en PRESEA (campo Condici�n I/5).</summary>
        public string Condicion { get; set; } = "0";

        /// <summary>"S" si fue emitido por impresor fiscal, "N" si no (campo Fiscal C/1).</summary>
        public string Fiscal { get; set; } = "N";

        /// <summary>Cuenta contable para IVA cr�dito fiscal principal (campo Cuenta IVA B/16).</summary>
        public string CuentaIVA { get; set; } = string.Empty;

        /// <summary>Cuenta contable para IVA cr�dito fiscal secundario (campo Cuenta IVA 2 B/16).</summary>
        public string CuentaIVA2 { get; set; } = string.Empty;

        /// <summary>Cuenta contable que se registra en el Debe (campo Cuenta contable del debe B/16).</summary>
        public string CuentaDebe { get; set; } = string.Empty;

        /// <summary>Centro de costos para cuentas de resultados (campo Centro C/10).</summary>
        public string Centro { get; set; } = string.Empty;

        /// <summary>"D", "N" o "P": indica si imputa contra recepciones (campo Imputa C/1).</summary>
        public string Imputa { get; set; } = "N";

        /// <summary>
        /// Formato de fecha para los campos tipo D/8.
        /// PRESEA espera 8 caracteres ? use "yyyyMMdd" (default) o "ddMMyyyy".
        /// </summary>
        public string FormatoFecha { get; set; } = "yyyyMMdd";

        // ── Nivel general: formato del archivo y defaults ─────────────────────────
        // Estos campos se agregaron para el flujo de exportacion TXT a PRESEA.
        // Son defaults que aplican cuando un proveedor (ConfigPreseaProveedor) no
        // define un valor propio. Los valores fijos por proveedor viven en la tabla
        // PreseaProveedores; el completado manual ocurre por comprobante.

        /// <summary>Separador de campos del TXT de importacion. PRESEA usa "|".</summary>
        public string Separador { get; set; } = "|";

        /// <summary>Encoding del archivo de texto. Neuralsoft suele requerir "Latin-1".</summary>
        public string Encoding { get; set; } = "Latin-1";

        /// <summary>Separador decimal para los importes del TXT. "." (punto) o "," (coma).</summary>
        public string SeparadorDecimal { get; set; } = ".";

        /// <summary>
        /// Indica si el parametro 459 de PRESEA esta en "S". Si es false, los campos
        /// "Lista de precios" (37) y "Version" (38) se exportan vacios.
        /// </summary>
        public bool UsaListaPrecios { get; set; } = false;

        /// <summary>
        /// Si es true, la Fecha contable (campo 12) se exporta igual a la Fecha de emision.
        /// Si es false, se completa manualmente segun el periodo contable.
        /// </summary>
        public bool FechaContableIgualEmision { get; set; } = true;

        /// <summary>Codigo de percepcion configurable 1 por defecto (campo 30, I/3).</summary>
        public string CodigoPercepcionConfig1 { get; set; } = string.Empty;

        /// <summary>Codigo de percepcion configurable 2 por defecto (campo 32, I/3).</summary>
        public string CodigoPercepcionConfig2 { get; set; } = string.Empty;
    }
}
