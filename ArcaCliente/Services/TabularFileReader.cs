using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ClosedXML.Excel;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Resultado de leer un archivo tabular: nombres de columna + filas como
    /// diccionarios columna -> valor.
    /// </summary>
    public class TablaLeida
    {
        public List<string> Columnas { get; set; } = new();
        public List<Dictionary<string, string>> Filas { get; set; } = new();
    }

    /// <summary>
    /// Lector generico de archivos tabulares (CSV / TXT delimitado / Excel .xlsx) con
    /// fila de encabezado configurable. Devuelve columnas y filas para mapearlas luego a
    /// los campos de una entidad. Reutilizable para cualquier maestro de PRESEA.
    /// </summary>
    public static class TabularFileReader
    {
        public static TablaLeida Leer(
            string ruta,
            string separador,
            string encoding,
            int filaEncabezado,
            string hojaExcel)
        {
            string ext = Path.GetExtension(ruta).ToLowerInvariant();
            return ext is ".xlsx" or ".xls"
                ? LeerExcel(ruta, filaEncabezado, hojaExcel)
                : LeerTexto(ruta, separador, encoding, filaEncabezado);
        }

        // ── Texto (CSV / TXT) ───────────────────────────────────────────────────────

        private static TablaLeida LeerTexto(string ruta, string separador, string encoding, int filaEncabezado)
        {
            var tabla = new TablaLeida();
            var enc   = ObtenerEncoding(encoding);
            char sep  = ObtenerSeparador(separador);

            var lineas = File.ReadAllLines(ruta, enc);
            int headerIdx = Math.Max(0, filaEncabezado - 1);
            if (lineas.Length <= headerIdx) return tabla;

            tabla.Columnas = NombresUnicos(SplitLine(lineas[headerIdx], sep));

            for (int i = headerIdx + 1; i < lineas.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lineas[i])) continue;
                var valores = SplitLine(lineas[i], sep);
                tabla.Filas.Add(ArmarFila(tabla.Columnas, valores));
            }
            return tabla;
        }

        // ── Excel ─────────────────────────────────────────────────────────────────

        private static TablaLeida LeerExcel(string ruta, int filaEncabezado, string hojaExcel)
        {
            var tabla = new TablaLeida();
            using var wb = new XLWorkbook(ruta);
            var hoja = string.IsNullOrWhiteSpace(hojaExcel)
                ? wb.Worksheets.First()
                : wb.Worksheet(hojaExcel);

            var rango = hoja.RangeUsed();
            if (rango == null) return tabla;

            var filas = rango.RowsUsed().ToList();
            int headerIdx = Math.Max(0, filaEncabezado - 1);
            if (filas.Count <= headerIdx) return tabla;

            var headerRow = filas[headerIdx];
            int n = headerRow.CellCount();

            var crudas = new List<string>();
            for (int c = 1; c <= n; c++)
                crudas.Add(headerRow.Cell(c).GetString().Trim());
            tabla.Columnas = NombresUnicos(crudas);

            for (int r = headerIdx + 1; r < filas.Count; r++)
            {
                var fila = filas[r];
                var valores = new List<string>();
                for (int c = 1; c <= n; c++)
                    valores.Add(fila.Cell(c).GetString().Trim());
                tabla.Filas.Add(ArmarFila(tabla.Columnas, valores));
            }
            return tabla;
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static Dictionary<string, string> ArmarFila(List<string> columnas, List<string> valores)
        {
            var fila = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int c = 0; c < columnas.Count; c++)
                fila[columnas[c]] = c < valores.Count ? valores[c].Trim() : string.Empty;
            return fila;
        }

        /// <summary>Garantiza nombres de columna no vacios y unicos.</summary>
        private static List<string> NombresUnicos(List<string> crudas)
        {
            var resultado = new List<string>();
            var vistos = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < crudas.Count; i++)
            {
                string nombre = string.IsNullOrWhiteSpace(crudas[i]) ? $"Columna{i + 1}" : crudas[i].Trim();
                if (vistos.TryGetValue(nombre, out int cuenta))
                {
                    vistos[nombre] = cuenta + 1;
                    nombre = $"{nombre}_{cuenta + 1}";
                }
                else vistos[nombre] = 1;
                resultado.Add(nombre);
            }
            return resultado;
        }

        private static List<string> SplitLine(string line, char sep)
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

        private static Encoding ObtenerEncoding(string nombre) =>
            nombre?.ToUpperInvariant() switch
            {
                "LATIN-1" or "ISO-8859-1" or "LATIN1" => Encoding.Latin1,
                _ => new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
            };

        private static char ObtenerSeparador(string sep) =>
            sep switch
            {
                "\t" or "TAB" => '\t',
                "|"           => '|',
                ","           => ',',
                _             => ';'
            };
    }
}
