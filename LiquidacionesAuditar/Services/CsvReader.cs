using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LiquidacionesAuditar.Services
{
    /// <summary>
    /// Lee un CSV con cabecera y retorna filas como diccionarios columna->valor.
    /// </summary>
    public static class CsvReader
    {
        public static (List<string> Columnas, List<Dictionary<string, string>> Filas)
            Leer(string rutaArchivo)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var lineas = File.ReadAllLines(rutaArchivo, Encoding.UTF8);
            if (lineas.Length == 0)
                return (new List<string>(), new List<Dictionary<string, string>>());

            var columnas = ParsearLinea(lineas[0]);
            var filas = new List<Dictionary<string, string>>();

            for (int i = 1; i < lineas.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lineas[i])) continue;
                var valores = ParsearLinea(lineas[i]);
                var fila = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int c = 0; c < columnas.Count; c++)
                    fila[columnas[c]] = c < valores.Count ? valores[c] : "";
                filas.Add(fila);
            }
            return (columnas, filas);
        }

        public static List<string> LeerColumnas(string rutaArchivo)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var primera = File.ReadLines(rutaArchivo, Encoding.UTF8).FirstOrDefault() ?? "";
            return ParsearLinea(primera);
        }

        private static List<string> ParsearLinea(string linea)
        {
            var campos = new List<string>();
            bool enComillas = false;
            var actual = new StringBuilder();
            foreach (char c in linea)
            {
                if (c == '"') { enComillas = !enComillas; continue; }
                if ((c == ',' || c == ';') && !enComillas)
                { campos.Add(actual.ToString().Trim()); actual.Clear(); continue; }
                actual.Append(c);
            }
            campos.Add(actual.ToString().Trim());
            return campos;
        }
    }
}
