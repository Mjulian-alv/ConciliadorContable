using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Importa comprobantes ARCA desde archivos CSV exportados del portal AFIP/ARCA.
    /// Escanea todos los archivos .csv del directorio indicado y los combina en una lista.
    /// </summary>
    public static class OfflineCsvImporter
    {
        // Mapa: encabezado del CSV (normalizado, sin tildes, lowercase) ? propiedad de ComprobanteCsv
        private static readonly Dictionary<string, string> _headerMap =
            new(StringComparer.OrdinalIgnoreCase)
        {
            ["fecha de emision"]                    = nameof(ComprobanteCsv.FechaEmision),
            ["fecha emision"]                       = nameof(ComprobanteCsv.FechaEmision),
            ["tipo de comprobante"]                 = nameof(ComprobanteCsv.TipoComprobante),
            ["punto de venta"]                      = nameof(ComprobanteCsv.PuntoVenta),
            ["numero desde"]                        = nameof(ComprobanteCsv.NumeroDesde),
            ["nro desde"]                           = nameof(ComprobanteCsv.NumeroDesde),
            ["numero hasta"]                        = nameof(ComprobanteCsv.NumeroHasta),
            ["nro hasta"]                           = nameof(ComprobanteCsv.NumeroHasta),
            ["codigo de autorizacion"]              = nameof(ComprobanteCsv.CodAutorizacion),
            ["cae / cai"]                           = nameof(ComprobanteCsv.CodAutorizacion),
            ["cae/cai"]                             = nameof(ComprobanteCsv.CodAutorizacion),
            ["tipo de doc. emisor"]                 = nameof(ComprobanteCsv.TipoDocEmisor),
            ["tipo doc emisor"]                     = nameof(ComprobanteCsv.TipoDocEmisor),
            ["nrodoc. emisor"]                      = nameof(ComprobanteCsv.NroDocEmisor),
            ["nro. doc. emisor"]                    = nameof(ComprobanteCsv.NroDocEmisor),
            ["nro doc emisor"]                      = nameof(ComprobanteCsv.NroDocEmisor),
            ["denominacion emisor"]                 = nameof(ComprobanteCsv.DenominacionEmisor),
            ["tipo de doc. receptor"]               = nameof(ComprobanteCsv.TipoDocReceptor),
            ["tipo doc receptor"]                   = nameof(ComprobanteCsv.TipoDocReceptor),
            ["nrodoc. receptor"]                    = nameof(ComprobanteCsv.NroDocReceptor),
            ["nro. doc. receptor"]                  = nameof(ComprobanteCsv.NroDocReceptor),
            ["nro doc receptor"]                    = nameof(ComprobanteCsv.NroDocReceptor),
            ["tipo de cambio"]                      = nameof(ComprobanteCsv.TipoCambio),
            ["moneda"]                              = nameof(ComprobanteCsv.Moneda),
            ["imp. neto gravado iva 0%"]            = nameof(ComprobanteCsv.ImpNetoGravadoIVA0),
            ["imp neto gravado iva 0%"]             = nameof(ComprobanteCsv.ImpNetoGravadoIVA0),
            ["iva 2,5%"]                            = nameof(ComprobanteCsv.Iva25),
            ["imp. neto gravado iva 2,5%"]          = nameof(ComprobanteCsv.ImpNetoGravadoIVA25),
            ["imp neto gravado iva 2,5%"]           = nameof(ComprobanteCsv.ImpNetoGravadoIVA25),
            ["iva 5%"]                              = nameof(ComprobanteCsv.Iva5),
            ["imp. neto gravado iva 5%"]            = nameof(ComprobanteCsv.ImpNetoGravadoIVA5),
            ["imp neto gravado iva 5%"]             = nameof(ComprobanteCsv.ImpNetoGravadoIVA5),
            ["iva 10,5%"]                           = nameof(ComprobanteCsv.Iva105),
            ["imp. neto gravado iva 10,5%"]         = nameof(ComprobanteCsv.ImpNetoGravadoIVA105),
            ["imp neto gravado iva 10,5%"]          = nameof(ComprobanteCsv.ImpNetoGravadoIVA105),
            ["iva 21%"]                             = nameof(ComprobanteCsv.Iva21),
            ["imp. neto gravado iva 21%"]           = nameof(ComprobanteCsv.ImpNetoGravadoIVA21),
            ["imp neto gravado iva 21%"]            = nameof(ComprobanteCsv.ImpNetoGravadoIVA21),
            ["iva 27%"]                             = nameof(ComprobanteCsv.Iva27),
            ["imp. neto gravado iva 27%"]           = nameof(ComprobanteCsv.ImpNetoGravadoIVA27),
            ["imp neto gravado iva 27%"]            = nameof(ComprobanteCsv.ImpNetoGravadoIVA27),
            ["imp. neto gravado total"]             = nameof(ComprobanteCsv.ImpNetoGravadoTotal),
            ["imp neto gravado total"]              = nameof(ComprobanteCsv.ImpNetoGravadoTotal),
            ["imp.neto gravado total"]              = nameof(ComprobanteCsv.ImpNetoGravadoTotal),
            ["imp. neto no gravado"]                = nameof(ComprobanteCsv.ImpNetoNoGravado),
            ["imp neto no gravado"]                 = nameof(ComprobanteCsv.ImpNetoNoGravado),
            ["imp. operac. exentas"]                = nameof(ComprobanteCsv.ImpOpExentas),
            ["imp operac. exentas"]                 = nameof(ComprobanteCsv.ImpOpExentas),
            ["imp. op. exentas"]                    = nameof(ComprobanteCsv.ImpOpExentas),
            ["otros tributos"]                      = nameof(ComprobanteCsv.OtrosTributos),
            ["iva total"]                           = nameof(ComprobanteCsv.TotalIVA),
            ["importe total"]                       = nameof(ComprobanteCsv.ImpTotal),
            ["imp. total"]                          = nameof(ComprobanteCsv.ImpTotal),
            };

        private static readonly Encoding Windows1252;

        static OfflineCsvImporter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Windows1252 = Encoding.GetEncoding("windows-1252");
        }

        /// <summary>
        /// Importa todos los archivos .csv de la carpeta indicada y los combina.
        /// </summary>
        /// <param name="carpeta">Ruta de la carpeta con los CSV exportados de ARCA.</param>
        /// <returns>Lista combinada de comprobantes de todos los archivos encontrados.</returns>
        public static List<ComprobanteCsv> ImportarDesdeCarpeta(string carpeta)
        {
            if (!Directory.Exists(carpeta))
                throw new DirectoryNotFoundException($"La carpeta no existe: {carpeta}");

            var archivos = Directory.GetFiles(carpeta, "*.csv", SearchOption.TopDirectoryOnly);
            var resultado = new List<ComprobanteCsv>();

            foreach (var archivo in archivos)
                resultado.AddRange(LeerArchivo(archivo));

            return resultado;
        }

        private static List<ComprobanteCsv> LeerArchivo(string ruta)
        {
            var resultado = new List<ComprobanteCsv>();
            var encoding = DetectarEncoding(ruta);

            string fileContent;
            using (var reader = new StreamReader(ruta, encoding))
            {
                fileContent = reader.ReadToEnd();
            }

            // Si detectamos caracteres de "Mojibake",
            // los revertimos a sus bytes originales (Windows1252) y los reinterpretamos como UTF-8
            if (fileContent.Contains("Ã³") || fileContent.Contains("Ãº") || fileContent.Contains("Ã"))
            {
                 var bytes = Windows1252.GetBytes(fileContent);
                 fileContent = Encoding.UTF8.GetString(bytes);
            }

            using var stringReader = new StringReader(fileContent);

            var headerLine = stringReader.ReadLine();
            if (string.IsNullOrWhiteSpace(headerLine))
                return resultado;

            var separador = headerLine.Contains(';') ? ';' : ',';
            var headers = SplitLine(headerLine, separador);

            // Construir mapa: índice de columna ? nombre de propiedad
            var colMap = new Dictionary<int, string>();
            for (int i = 0; i < headers.Length; i++)
            {
                var h = NormalizarEncabezado(headers[i]);
                if (_headerMap.TryGetValue(h, out var propName))
                    colMap[i] = propName;
            }

            string line;
            while ((line = stringReader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var values = SplitLine(line, separador);
                resultado.Add(MapearFila(values, colMap));
            }

            return resultado;
        }

        private static ComprobanteCsv MapearFila(string[] values, Dictionary<int, string> colMap)
        {
            var comp = new ComprobanteCsv();
            var type = typeof(ComprobanteCsv);

            foreach (var (idx, propName) in colMap)
            {
                if (idx >= values.Length) continue;
                if(propName=="ImpTotal")
                {
                    var valor = values[idx].Trim();
                    if (decimal.TryParse(valor, out decimal total))
                    {
                        comp.ImpTotal = total.ToString("F2");
                    }
                    else
                    {
                        comp.ImpTotal = valor; // Asignar el valor original si no se puede parsear
                    }
                } else if(propName== "FechaEmision")
                {
                    var valor = values[idx].Trim();
                    if (DateTime.TryParse(valor, out DateTime fecha))
                    {
                        comp.FechaEmision = fecha.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        comp.FechaEmision = valor; // Asignar el valor original si no se puede parsear
                    }
                }
                else
                    type.GetProperty(propName)?.SetValue(comp, values[idx].Trim());
            }

            return comp;
        }

        /// <summary>
        /// Normaliza un encabezado: minúsculas, sin espacios extremos, sin tildes.
        /// </summary>
        private static string NormalizarEncabezado(string header)
        {
            if (string.IsNullOrEmpty(header)) return string.Empty;

            header = header.Trim().ToLowerInvariant();

            // Quitar tildes / diacríticos
            var normalized = header.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);
            foreach (char c in normalized)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) !=
                    System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private static string[] SplitLine(string line, char separator)
        {
            var result  = new List<string>();
            var current = new StringBuilder();
            bool inQuote = false;

            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuote = !inQuote;
                }
                else if (c == separator && !inQuote)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            result.Add(current.ToString());
            return result.ToArray();
        }

        /// <summary>
        /// Detecta el encoding del archivo leyendo el BOM.
        /// Los CSV de ARCA suelen ser UTF-8 con BOM o Latin-1 (ISO-8859-1).
        /// </summary>
        private static Encoding DetectarEncoding(string ruta)
        {
            using var fs = new FileStream(ruta, FileMode.Open, FileAccess.Read);
            var bom = new byte[3];
            int read = fs.Read(bom, 0, 3);

            if (read >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
                return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

            if (read >= 2 && bom[0] == 0xFF && bom[1] == 0xFE)
                return Encoding.Unicode;

            // Sin BOM: asumir formato ANSI/Latin-1 (común en archivos guardados por Excel en Windows)
            return Windows1252;
        }
    }
}
