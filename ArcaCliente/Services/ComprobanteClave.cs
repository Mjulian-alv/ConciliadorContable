namespace ArcaCliente.Services
{
    /// <summary>
    /// Genera la clave normalizada de identidad de un comprobante, comun a los datos del
    /// CSV de ARCA y a los datos del QR. Se usa para matchear comprobantes escaneados con
    /// la lista SOLO ARCA y para la memoria anti-duplicado de exportados a PRESEA.
    /// <para>
    /// La normalizacion garantiza que valores con distinto relleno coincidan
    /// (p.ej. "0001" del CSV y 1 del QR producen la misma clave).
    /// </para>
    /// </summary>
    public static class ComprobanteClave
    {
        /// <summary>
        /// Construye la clave "{cuit}|{tipo}|{ptoVta}|{nro}" con todos los componentes
        /// reducidos a digitos y sin ceros a la izquierda.
        /// </summary>
        public static string Generar(string cuit, string tipoCmp, string ptoVta, string nro)
        {
            string c  = TextParsingUtils.SoloDigitos(cuit);
            string t  = NormalizarNumero(tipoCmp);
            string pv = NormalizarNumero(ptoVta);
            string n  = NormalizarNumero(nro);
            return $"{c}|{t}|{pv}|{n}";
        }

        private static string NormalizarNumero(string s)
        {
            string d = TextParsingUtils.SoloDigitos(s).TrimStart('0');
            return d.Length == 0 ? "0" : d;
        }
    }
}
