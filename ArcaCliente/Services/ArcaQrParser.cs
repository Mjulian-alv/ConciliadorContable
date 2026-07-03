using System;
using System.Globalization;
using System.Text;
using System.Text.Json;
using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Parser del QR de comprobantes electronicos de ARCA/AFIP.
    /// El QR NO esta encriptado: contiene la URL https://www.afip.gob.ar/fe/qr/?p=&lt;BASE64&gt;
    /// con un JSON codificado en Base64. Acepta tanto la URL completa como el Base64 suelto,
    /// tal como lo entrega un lector USB de mano (keyboard-wedge).
    /// </summary>
    public static class ArcaQrParser
    {
        /// <summary>
        /// Intenta parsear el contenido escaneado del QR.
        /// </summary>
        /// <param name="raw">Texto crudo escaneado (URL completa o Base64).</param>
        /// <param name="data">Datos del comprobante si el parseo fue exitoso.</param>
        /// <param name="error">Mensaje de error legible si fallo.</param>
        /// <returns>True si se pudo parsear; false en caso contrario.</returns>
        public static bool TryParse(string raw, out ArcaQrData data, out string error)
        {
            data = null;
            error = null;

            if (string.IsNullOrWhiteSpace(raw))
            {
                error = "El texto escaneado esta vacio.";
                return false;
            }

            string payload = ExtraerPayload(raw.Trim());
            string b64 = NormalizarBase64(payload);

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(b64);
            }
            catch
            {
                error = "El contenido no es un Base64 valido. Verifique que sea un QR de ARCA.";
                return false;
            }

            string json = Encoding.UTF8.GetString(bytes);

            try
            {
                using var doc = JsonDocument.Parse(json);
                var r = doc.RootElement;

                data = new ArcaQrData
                {
                    Version    = GetInt(r, "ver"),
                    Fecha      = GetString(r, "fecha"),
                    Cuit       = GetString(r, "cuit"),
                    PtoVta     = GetInt(r, "ptoVta"),
                    TipoCmp    = GetInt(r, "tipoCmp"),
                    NroCmp     = GetLong(r, "nroCmp"),
                    Importe    = GetDecimal(r, "importe"),
                    Moneda     = GetString(r, "moneda"),
                    Cotizacion = GetDecimal(r, "ctz"),
                    TipoDocRec = GetInt(r, "tipoDocRec"),
                    NroDocRec  = GetString(r, "nroDocRec"),
                    TipoCodAut = GetString(r, "tipoCodAut"),
                    CodAut     = GetString(r, "codAut"),
                };

                if (string.IsNullOrWhiteSpace(data.Cuit) || data.TipoCmp == 0)
                {
                    error = "El QR no contiene los datos minimos de un comprobante de ARCA.";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                error = "El QR no contiene un JSON de ARCA valido: " + ex.Message;
                return false;
            }
        }

        // ── Helpers de extraccion ─────────────────────────────────────────────────

        /// <summary>
        /// Si el texto es una URL con parametro "p=", devuelve su valor (desescapado).
        /// Si no, devuelve el texto tal cual (se asume que ya es el Base64).
        /// </summary>
        private static string ExtraerPayload(string s)
        {
            int idx = s.IndexOf("p=", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return s;

            string after = s.Substring(idx + 2);
            int amp = after.IndexOf('&');
            if (amp >= 0) after = after.Substring(0, amp);

            return Uri.UnescapeDataString(after);
        }

        /// <summary>
        /// Convierte Base64 url-safe a estandar y agrega el padding faltante.
        /// </summary>
        private static string NormalizarBase64(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                if (char.IsWhiteSpace(c)) continue;
                sb.Append(c switch { '-' => '+', '_' => '/', _ => c });
            }

            int mod = sb.Length % 4;
            if (mod == 2) sb.Append("==");
            else if (mod == 3) sb.Append('=');
            else if (mod == 1) sb.Append("==="); // invalido, dejara fallar el FromBase64String

            return sb.ToString();
        }

        // ── Helpers de lectura JSON (toleran numero o string) ─────────────────────

        private static string GetString(JsonElement r, string prop)
        {
            if (!r.TryGetProperty(prop, out var el)) return string.Empty;
            return el.ValueKind switch
            {
                JsonValueKind.String => el.GetString() ?? string.Empty,
                JsonValueKind.Number => el.GetRawText(),
                _ => string.Empty
            };
        }

        private static int GetInt(JsonElement r, string prop)
        {
            if (!r.TryGetProperty(prop, out var el)) return 0;
            if (el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out var n)) return n;
            return int.TryParse(GetString(r, prop), NumberStyles.Integer, CultureInfo.InvariantCulture, out var p) ? p : 0;
        }

        private static long GetLong(JsonElement r, string prop)
        {
            if (!r.TryGetProperty(prop, out var el)) return 0;
            if (el.ValueKind == JsonValueKind.Number && el.TryGetInt64(out var n)) return n;
            return long.TryParse(GetString(r, prop), NumberStyles.Integer, CultureInfo.InvariantCulture, out var p) ? p : 0;
        }

        private static decimal GetDecimal(JsonElement r, string prop)
        {
            if (!r.TryGetProperty(prop, out var el)) return 0m;
            if (el.ValueKind == JsonValueKind.Number && el.TryGetDecimal(out var n)) return n;
            return decimal.TryParse(GetString(r, prop), NumberStyles.Any, CultureInfo.InvariantCulture, out var p) ? p : 0m;
        }
    }
}
