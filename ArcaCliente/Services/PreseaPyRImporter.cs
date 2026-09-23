// Fecha: 21/09/2026 - TAREA: 00041 - Linea: 4 - Levanta el mayor de PRESEA de una cuenta de percepciones/retenciones
// A diferencia de ARCA, PRESEA no trae tipo y número en columnas: vienen juntos en el concepto,
// "SEGUN FACTURA A    36900627623 de CIA INDUSTRIAL C". Las anulaciones NO traen " de …"
// ("POR ANULACION FACTURA A    73200019077"), por eso esa parte del patrón es opcional.
// Lo que no respeta el patrón (minutas financieras) se carga igual, marcado SinComprobante,
// para que las directivas de la etapa siguiente decidan qué hacer: no se descarta en silencio.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ArcaCliente.Models;
using ExcelDataReader;

namespace ArcaCliente.Services
{
    /// <summary>El mayor no tiene alguna de las columnas configuradas en el perfil.</summary>
    public class ColumnasFaltantesException : Exception
    {
        public IReadOnlyList<string> Faltantes { get; }

        public ColumnasFaltantesException(IReadOnlyList<string> faltantes)
            : base("Faltan las columnas " + string.Join(", ", faltantes) + ".")
            => Faltantes = faltantes;
    }

    public static class PreseaPyRImporter
    {
        static PreseaPyRImporter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public static MayorPreseaPyR Importar(string ruta, PerfilOfflinePyR perfil, CuentaPyR cuenta)
        {
            string nombre = Path.GetFileName(ruta);

            // FileShare.ReadWrite: el mayor suele estar abierto en Excel mientras se trabaja.
            using var fs = new FileStream(ruta, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = ExcelReaderFactory.CreateReader(fs);

            if (!string.IsNullOrWhiteSpace(perfil.HojaExcel))
            {
                while (!string.Equals(reader.Name, perfil.HojaExcel.Trim(), StringComparison.OrdinalIgnoreCase))
                    if (!reader.NextResult())
                        throw new InvalidDataException($"El archivo no tiene la hoja \"{perfil.HojaExcel}\".");
            }

            var mayor = new MayorPreseaPyR { Archivo = nombre, Ruta = ruta, Cuenta = cuenta };
            Dictionary<string, int> cols = null;

            while (reader.Read())
            {
                if (cols == null)
                {
                    if (!perfil.TieneCabecera)
                    {
                        cols = ColumnasPorPosicion(perfil);
                    }
                    else
                    {
                        var enc = Enumerable.Range(0, reader.FieldCount)
                            .Select(i => TextParsingUtils.NormalizarEncabezado(Convert.ToString(reader.GetValue(i), CultureInfo.InvariantCulture)))
                            .ToList();
                        if (enc.All(string.IsNullOrEmpty)) continue;
                        cols = ColumnasPorNombre(perfil, enc);
                        continue;
                    }
                }

                var fila = LeerFila(reader, cols, perfil, cuenta, nombre);
                if (fila != null) mayor.Registros.Add(fila);
            }

            if (cols == null)
                throw new InvalidDataException("El archivo está vacío.");

            return mayor;
        }

        // ── Columnas ─────────────────────────────────────────────────────────────

        private static IEnumerable<(string Campo, string Valor)> Configuradas(PerfilOfflinePyR p) => new[]
        {
            ("fecha", p.ColFecha), ("asiento", p.ColAsiento), ("concepto", p.ColConcepto),
            ("debe", p.ColDebe),   ("haber", p.ColHaber)
        };

        private static Dictionary<string, int> ColumnasPorNombre(PerfilOfflinePyR perfil, List<string> encabezado)
        {
            var cols = new Dictionary<string, int>();
            var faltantes = new List<string>();
            foreach (var (campo, valor) in Configuradas(perfil))
            {
                int i = encabezado.IndexOf(TextParsingUtils.NormalizarEncabezado(valor ?? string.Empty));
                if (string.IsNullOrWhiteSpace(valor) || i < 0) faltantes.Add(string.IsNullOrWhiteSpace(valor) ? campo : valor);
                else cols[campo] = i;
            }
            if (faltantes.Count > 0) throw new ColumnasFaltantesException(faltantes);
            return cols;
        }

        private static Dictionary<string, int> ColumnasPorPosicion(PerfilOfflinePyR perfil)
        {
            var cols = new Dictionary<string, int>();
            var faltantes = new List<string>();
            foreach (var (campo, valor) in Configuradas(perfil))
            {
                if (int.TryParse(valor, out int pos) && pos >= 1) cols[campo] = pos - 1;
                else faltantes.Add($"{campo} (número de columna inválido: \"{valor}\")");
            }
            if (faltantes.Count > 0) throw new ColumnasFaltantesException(faltantes);
            return cols;
        }

        // ── Fila ─────────────────────────────────────────────────────────────────

        private static RegistroPreseaPyR LeerFila(IExcelDataReader reader, Dictionary<string, int> cols,
            PerfilOfflinePyR perfil, CuentaPyR cuenta, string archivo)
        {
            object Valor(string campo) =>
                cols[campo] < reader.FieldCount ? reader.GetValue(cols[campo]) : null;

            var fecha = ParsearFecha(Valor("fecha"), perfil.FormatoFecha);
            var debe  = ParsearImporte(Valor("debe"),  perfil.SeparadorDecimal);
            var haber = ParsearImporte(Valor("haber"), perfil.SeparadorDecimal);

            // Renglones en blanco y totales: sin fecha o sin debe ni haber.
            if (fecha == null || (debe == null && haber == null)) return null;

            string concepto = Convert.ToString(Valor("concepto"), CultureInfo.InvariantCulture)?.Trim() ?? string.Empty;
            var parte = ConceptoPreseaParser.Parsear(concepto);

            return new RegistroPreseaPyR
            {
                Cuenta           = cuenta,
                Fecha            = fecha,
                Asiento          = Convert.ToString(Valor("asiento"), CultureInfo.InvariantCulture)?.Trim(),
                ConceptoOriginal = concepto,
                TipoComprobante  = parte?.Tipo,
                Numero           = parte?.Numero,
                Proveedor        = parte?.Proveedor,
                EsAnulacion      = parte?.EsAnulacion ?? false,
                SinComprobante   = parte == null,
                Importe          = (debe ?? 0m) - (haber ?? 0m),
                ArchivoOrigen    = archivo
            };
        }

        private static DateTime? ParsearFecha(object v, string formato)
        {
            if (v is DateTime dt) return dt.Date;
            if (v is double oa) return DateTime.FromOADate(oa).Date;   // fecha guardada como número
            var s = Convert.ToString(v, CultureInfo.InvariantCulture)?.Trim();
            if (string.IsNullOrEmpty(s)) return null;
            return DateTime.TryParseExact(s, formato ?? "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
                ? d : null;
        }

        private static decimal? ParsearImporte(object v, string separadorDecimal)
        {
            switch (v)
            {
                case null:     return null;
                case double d: return (decimal)d;
                case decimal m: return m;
                case int i:    return i;
            }
            var s = Convert.ToString(v, CultureInfo.InvariantCulture)?.Trim();
            if (string.IsNullOrEmpty(s)) return null;
            var nfi = new NumberFormatInfo
            {
                NumberDecimalSeparator = separadorDecimal == "," ? "," : ".",
                NumberGroupSeparator   = separadorDecimal == "," ? "." : ","
            };
            return decimal.TryParse(s, NumberStyles.Number, nfi, out var r) ? r : null;
        }
    }

    /// <summary>Separa el concepto de PRESEA en tipo, número y proveedor.</summary>
    public static class ConceptoPreseaParser
    {
        public record Partes(string Tipo, string Numero, string Proveedor, bool EsAnulacion);

        private static readonly Regex Rx = new(
            @"^(?<prefijo>SEGUN|POR ANULACION)\s+(?<tipo>.+?)\s+(?<numero>\d+)(?:\s+de\s+(?<proveedor>.*))?$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly Regex Espacios = new(@"\s+");

        /// <summary>Null si el concepto no respeta el patrón (fila "sin comprobante").</summary>
        public static Partes Parsear(string concepto)
        {
            var m = Rx.Match(Espacios.Replace(concepto ?? string.Empty, " ").Trim());
            if (!m.Success) return null;

            return new Partes(
                m.Groups["tipo"].Value.Trim().ToUpperInvariant(),
                NumeroPyR.Normalizar(m.Groups["numero"].Value),
                m.Groups["proveedor"].Success ? m.Groups["proveedor"].Value.Trim() : string.Empty,
                m.Groups["prefijo"].Value.StartsWith("POR", StringComparison.OrdinalIgnoreCase));
        }
    }
}
