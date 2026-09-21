// Fecha: 21/09/2026 - TAREA: 00041 - Linea: 3 - Lee la carpeta de ARCA de percepciones y retenciones como base de datos
// Dos formatos conviven en la misma carpeta y NINGUNO se distingue por extensión:
//  - "Mis Retenciones" de AFIP: xls binario (BIFF) o xlsx, hoja con "CUIT Agente Ret./Perc.".
//    ClosedXML no abre BIFF, por eso se usa ExcelDataReader. El impuesto sale del código
//    (767 IVA, 217 Ganancias).
//  - Consulta provincial de IIBB: es una tabla HTML guardada con extensión .xls, en UTF-8 sin
//    BOM ni charset declarado. Si se deja adivinar el encoding se lee "RetenciÃ³n" y la
//    operación deja de reconocerse, por eso se decodifica explícitamente.
// Un archivo no reconocido no corta la carga: se informa y se sigue con el resto.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using ArcaCliente.Models;
using ExcelDataReader;
using ExcelDataReader.Exceptions;

namespace ArcaCliente.Services
{
    public static class ArcaPyRImporter
    {
        private static readonly string[] Extensiones = { ".xls", ".xlsx", ".htm", ".html" };

        private static readonly Encoding Utf8Estricto =
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        static ArcaPyRImporter()
        {
            // ExcelDataReader necesita las code pages de Windows para los .xls BIFF.
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        /// <summary>
        /// Lee todos los archivos reconocibles de <paramref name="carpeta"/> (sin subcarpetas).
        /// Lanza si la carpeta no existe o si no hay ningún archivo reconocible.
        /// </summary>
        public static CargaArcaPyR ImportarDesdeCarpeta(string carpeta)
        {
            if (!Directory.Exists(carpeta))
                throw new DirectoryNotFoundException($"La carpeta no existe: {carpeta}");

            var carga = new CargaArcaPyR();

            var archivos = Directory.GetFiles(carpeta, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => Extensiones.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .Where(f => !Path.GetFileName(f).StartsWith("~$"))   // temporales de Office abiertos
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase);

            // Fecha: 21/09/2026 - TAREA: 00041 - Linea: 3 - Saltear archivos repetidos de la carpeta
            // El portal baja el reporte como "Listado - <fecha>.xls": descargarlo dos veces deja dos
            // archivos idénticos y, como la carpeta es la base, duplicaría todos los importes sin aviso.
            var hashes = new Dictionary<string, string>();

            foreach (var ruta in archivos)
            {
                string nombre = Path.GetFileName(ruta);
                try
                {
                    string hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(ruta)));
                    if (hashes.TryGetValue(hash, out var original))
                    {
                        carga.ArchivosNoReconocidos.Add((nombre, $"es idéntico a {original}, se salteó"));
                        continue;
                    }
                    hashes[hash] = nombre;

                    var registros = LeerArchivo(ruta, carga, out string motivo);
                    if (registros == null)
                    {
                        carga.ArchivosNoReconocidos.Add((nombre, motivo));
                        continue;
                    }
                    carga.Registros.AddRange(registros);
                    carga.ArchivosLeidos.Add(nombre);
                }
                catch (Exception ex) when (ex is IOException or InvalidDataException or ExcelReaderException)
                {
                    carga.ArchivosNoReconocidos.Add((nombre, ex.Message));
                }
            }

            if (carga.ArchivosLeidos.Count == 0)
                throw new InvalidDataException(carga.ArchivosNoReconocidos.Count == 0
                    ? "La carpeta no tiene archivos de ARCA (.xls, .xlsx, .htm, .html)."
                    : "Ningún archivo de la carpeta tiene un formato de ARCA reconocible:\n" +
                      string.Join("\n", carga.ArchivosNoReconocidos.Select(a => $"· {a.Archivo}: {a.Motivo}")));

            return carga;
        }

        // Devuelve null (con motivo) si el formato no se reconoce.
        private static List<RegistroArcaPyR> LeerArchivo(string ruta, CargaArcaPyR carga, out string motivo)
        {
            byte[] bytes = File.ReadAllBytes(ruta);
            string nombre = Path.GetFileName(ruta);

            if (EsHtml(bytes))
                return LeerHtmlProvincial(bytes, nombre, out motivo);

            if (EsOle(bytes) || EsZip(bytes))
                return LeerExcelAfip(bytes, nombre, carga, out motivo);

            motivo = "formato no reconocido";
            return null;
        }

        // ── Detección de formato ─────────────────────────────────────────────────

        private static bool EsOle(byte[] b) =>
            b.Length >= 4 && b[0] == 0xD0 && b[1] == 0xCF && b[2] == 0x11 && b[3] == 0xE0;

        private static bool EsZip(byte[] b) =>
            b.Length >= 2 && b[0] == (byte)'P' && b[1] == (byte)'K';

        private static bool EsHtml(byte[] b)
        {
            int i = 0;
            if (b.Length >= 3 && b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF) i = 3;   // BOM UTF-8
            while (i < b.Length && (b[i] == ' ' || b[i] == '\t' || b[i] == '\r' || b[i] == '\n')) i++;
            return i < b.Length && b[i] == (byte)'<';
        }

        // ── Formato AFIP ("Mis Retenciones") ─────────────────────────────────────

        private const string EncAfipCuit = "cuit agente ret./perc.";

        private static List<RegistroArcaPyR> LeerExcelAfip(byte[] bytes, string nombre, CargaArcaPyR carga, out string motivo)
        {
            using var ms = new MemoryStream(bytes);
            using var reader = ExcelReaderFactory.CreateReader(ms);

            do
            {
                // Se busca la hoja cuyo primer renglón con datos es el encabezado de AFIP.
                Dictionary<string, int> cols = null;
                var resultado = new List<RegistroArcaPyR>();

                while (reader.Read())
                {
                    if (cols == null)
                    {
                        var encabezado = Enumerable.Range(0, reader.FieldCount)
                            .Select(i => NormalizarEncabezado(Convert.ToString(reader.GetValue(i), CultureInfo.InvariantCulture)))
                            .ToList();
                        if (encabezado.All(string.IsNullOrEmpty)) continue;
                        if (!encabezado.Contains(EncAfipCuit)) break;   // esta hoja no es de AFIP

                        cols = new Dictionary<string, int>();
                        for (int i = 0; i < encabezado.Count; i++)
                            if (!string.IsNullOrEmpty(encabezado[i])) cols.TryAdd(encabezado[i], i);
                        continue;
                    }

                    string Celda(string enc) =>
                        cols.TryGetValue(enc, out int i) && i < reader.FieldCount
                            ? Convert.ToString(reader.GetValue(i), CultureInfo.InvariantCulture)?.Trim()
                            : null;

                    string cuit = Celda(EncAfipCuit);
                    if (string.IsNullOrWhiteSpace(cuit)) continue;   // renglones en blanco / totales

                    var impuesto  = ImpuestoDesdeCodigoAfip(Celda("impuesto"));
                    var operacion = OperacionDesdeTexto(Celda("descripcion operacion"));
                    if (impuesto == null || operacion == null)
                    {
                        carga.FilasNoReconocidas++;
                        continue;
                    }

                    resultado.Add(new RegistroArcaPyR
                    {
                        Impuesto          = impuesto.Value,
                        Operacion         = operacion.Value,
                        Fecha             = ParsearFecha(reader, cols, "fecha ret./perc."),
                        Cuit              = TextParsingUtils.SoloDigitos(cuit),
                        Denominacion      = Celda("denominacion o razon social"),
                        TipoComprobante   = Celda("descripcion comprobante"),
                        Letra             = string.Empty,
                        Numero            = NumeroPyR.Normalizar(Celda("numero comprobante")),
                        NumeroCertificado = Celda("numero certificado"),
                        Importe           = ParsearImporte(Celda("importe ret./perc.")),
                        ArchivoOrigen     = nombre
                    });
                }

                if (cols != null)
                {
                    motivo = null;
                    return resultado;
                }
            }
            while (reader.NextResult());

            motivo = "es un Excel, pero ninguna hoja tiene el encabezado de AFIP (\"CUIT Agente Ret./Perc.\")";
            return null;
        }

        private static DateTime? ParsearFecha(IExcelDataReader reader, Dictionary<string, int> cols, string enc)
        {
            if (!cols.TryGetValue(enc, out int i) || i >= reader.FieldCount) return null;
            var v = reader.GetValue(i);
            if (v is DateTime dt) return dt.Date;
            return ParsearFechaTexto(Convert.ToString(v, CultureInfo.InvariantCulture));
        }

        private static ImpuestoPyR? ImpuestoDesdeCodigoAfip(string codigo) => codigo?.Trim() switch
        {
            "767" => ImpuestoPyR.Iva,
            "217" => ImpuestoPyR.Ganancias,
            _     => null
        };

        // ── Formato provincial IIBB (tabla HTML) ─────────────────────────────────

        private static readonly Regex RxTabla  = new(@"<table\b.*?</table>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex RxFila   = new(@"<tr\b[^>]*>(.*?)</tr>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex RxCelda  = new(@"<t[hd]\b[^>]*>(.*?)</t[hd]>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex RxTag    = new(@"<[^>]+>", RegexOptions.Singleline);

        // "Factura A000200118618" → tipo "Factura", letra "A", número "000200118618".
        // "Liquidación de pago 000000000049648" → sin letra.
        private static readonly Regex RxComprobante = new(@"^(?<tipo>.*?)\s*(?<letra>[A-Z])?(?<numero>\d+)\s*$", RegexOptions.Singleline);

        private static List<RegistroArcaPyR> LeerHtmlProvincial(byte[] bytes, string nombre, out string motivo)
        {
            string texto = DecodificarHtml(bytes);

            foreach (Match tabla in RxTabla.Matches(texto))
            {
                var filas = RxFila.Matches(tabla.Value)
                    .Select(f => RxCelda.Matches(f.Groups[1].Value)
                        .Select(c => WebUtility.HtmlDecode(RxTag.Replace(c.Groups[1].Value, "")).Trim())
                        .ToList())
                    .Where(f => f.Count > 0)
                    .ToList();
                if (filas.Count == 0) continue;

                var enc = filas[0].Select(NormalizarEncabezado).ToList();
                int iCuit = enc.IndexOf("cuit"), iNombre = enc.IndexOf("nombre"), iOper = enc.IndexOf("operacion"),
                    iFecha = enc.IndexOf("fecha"), iComp = enc.IndexOf("comprobante"), iImp = enc.IndexOf("importe");
                if (iCuit < 0 || iOper < 0 || iComp < 0 || iImp < 0) continue;   // otra tabla de la página

                var resultado = new List<RegistroArcaPyR>();
                foreach (var f in filas.Skip(1))
                {
                    string Celda(int i) => i >= 0 && i < f.Count ? f[i] : null;

                    var operacion = OperacionDesdeTexto(Celda(iOper));
                    if (operacion == null || string.IsNullOrWhiteSpace(Celda(iCuit))) continue;

                    var comp = RxComprobante.Match(Celda(iComp) ?? string.Empty);
                    resultado.Add(new RegistroArcaPyR
                    {
                        Impuesto        = ImpuestoPyR.Iibb,
                        Operacion       = operacion.Value,
                        Fecha           = ParsearFechaTexto(Celda(iFecha)),
                        Cuit            = TextParsingUtils.SoloDigitos(Celda(iCuit)),
                        Denominacion    = Celda(iNombre),
                        TipoComprobante = comp.Success ? comp.Groups["tipo"].Value.Trim() : Celda(iComp),
                        Letra           = comp.Success ? comp.Groups["letra"].Value : string.Empty,
                        Numero          = NumeroPyR.Normalizar(comp.Success ? comp.Groups["numero"].Value : Celda(iComp)),
                        Importe         = ParsearImporte(Celda(iImp)),
                        ArchivoOrigen   = nombre
                    });
                }

                motivo = null;
                return resultado;
            }

            motivo = "es HTML, pero no tiene una tabla con columnas Cuit / Operación / Comprobante / Importe";
            return null;
        }

        private static string DecodificarHtml(byte[] bytes)
        {
            int inicio = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;
            try { return Utf8Estricto.GetString(bytes, inicio, bytes.Length - inicio); }
            catch (DecoderFallbackException) { return Encoding.GetEncoding(1252).GetString(bytes); }
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static TipoOperacionPyR? OperacionDesdeTexto(string texto)
        {
            var t = NormalizarEncabezado(texto);
            if (t.StartsWith("percepcion")) return TipoOperacionPyR.Percepcion;
            if (t.StartsWith("retencion"))  return TipoOperacionPyR.Retencion;
            return null;
        }

        // Los importes de ARCA vienen con punto decimal y sin separador de miles ("22322.03").
        private static decimal ParsearImporte(string texto) =>
            decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? d : 0m;

        private static DateTime? ParsearFechaTexto(string texto) =>
            DateTime.TryParseExact(texto?.Trim(), new[] { "dd/MM/yyyy", "d/M/yyyy" }, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var d) ? d : null;

        // Además de tildes y mayúsculas, colapsa espacios: los encabezados HTML traen "Nombre ".
        private static string NormalizarEncabezado(string texto) =>
            Regex.Replace(TextParsingUtils.NormalizarEncabezado(texto ?? string.Empty), @"\s+", " ").Trim();
    }

    /// <summary>Normalización común del número de comprobante de ARCA y de PRESEA.</summary>
    public static class NumeroPyR
    {
        /// <summary>Sólo dígitos, sin ceros a la izquierda ("000200118618" → "200118618").</summary>
        public static string Normalizar(string texto)
        {
            var digitos = TextParsingUtils.SoloDigitos(texto).TrimStart('0');
            return digitos.Length == 0 && !string.IsNullOrEmpty(TextParsingUtils.SoloDigitos(texto)) ? "0" : digitos;
        }
    }
}
