using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Helpers de parseo de texto compartidos por los importadores tabulares
    /// (TabularFileReader, LocalFileImporter, OfflineCsvImporter) y los exportadores
    /// PRESEA. Antes cada uno tenía su propia copia privada, sin divergencia detectada
    /// pero con cuatro lugares para mantener sincronizados.
    /// </summary>
    internal static class TextParsingUtils
    {
        /// <summary>
        /// Separa una línea por <paramref name="sep"/>, respetando comillas: un separador
        /// dentro de comillas no corta el campo.
        /// </summary>
        public static List<string> SplitLine(string line, char sep)
        {
            var result  = new List<string>();
            var current = new StringBuilder();
            bool inQuote = false;

            foreach (char c in line)
            {
                if (c == '"')                    inQuote = !inQuote;
                else if (c == sep && !inQuote) { result.Add(current.ToString()); current.Clear(); }
                else                             current.Append(c);
            }
            result.Add(current.ToString());
            return result;
        }

        public static Encoding ObtenerEncoding(string nombre) =>
            nombre?.ToUpperInvariant() switch
            {
                "LATIN-1" or "ISO-8859-1" or "LATIN1" => Encoding.Latin1,
                _ => new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
            };

        public static char ObtenerSeparador(string sep) =>
            sep switch
            {
                "\t" or "TAB" => '\t',
                "|"           => '|',
                ","           => ',',
                _             => ';'
            };

        /// <summary>Quita tildes/diacríticos y pasa a minúsculas, para matchear headers
        /// de archivo contra nombres de columna configurados sin depender de acentos.</summary>
        public static string NormalizarEncabezado(string header)
        {
            if (string.IsNullOrEmpty(header)) return string.Empty;

            header = header.Trim().ToLowerInvariant();

            var normalized = header.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);
            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>Extrae solo los dígitos de un string (p.ej. para normalizar un CUIT).</summary>
        public static string SoloDigitos(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;

            var sb = new StringBuilder(s.Length);
            foreach (char c in s)
                if (char.IsDigit(c)) sb.Append(c);
            return sb.ToString();
        }
    }
}
