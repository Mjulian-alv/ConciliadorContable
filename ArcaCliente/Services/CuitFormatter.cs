namespace ArcaCliente.Services
{
    /// <summary>
    /// Formatea un CUIT a XX-XXXXXXXX-X para mostrarlo en pantalla.
    /// Estaba duplicado en FormExportarPreseaQr y FormProcesarSoloArca, y divergido: uno
    /// filtraba dígitos antes de validar longitud 11, el otro validaba length!=11 sobre el
    /// string crudo — un CUIT que llegara con guiones o espacios no se reformateaba ahí.
    /// </summary>
    public static class CuitFormatter
    {
        public static string Formatear(string cuit)
        {
            var digitos = TextParsingUtils.SoloDigitos(cuit);
            return digitos.Length == 11
                ? $"{digitos[..2]}-{digitos[2..10]}-{digitos[10]}"
                : cuit ?? string.Empty;
        }
    }
}
