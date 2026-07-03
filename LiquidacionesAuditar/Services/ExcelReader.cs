using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ExcelDataReader;

namespace LiquidacionesAuditar.Services
{
    /// <summary>
    /// Lee un archivo Excel (.xls / .xlsx) y retorna columnas + filas como diccionarios,
    /// con la misma firma que CsvReader.Leer para intercambiarlos de forma transparente.
    /// </summary>
    public static class ExcelReader
    {
        /// <summary>
        /// Lee el Excel y retorna (Columnas, Filas).
        /// </summary>
        /// <param name="rutaArchivo">Ruta completa al .xls o .xlsx.</param>
        /// <param name="filaEncabezado">Número de fila (1-based) donde están los títulos de columna.</param>
        public static (List<string> Columnas, List<Dictionary<string, string>> Filas)
            Leer(string rutaArchivo, int filaEncabezado = 1)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using var stream = File.Open(rutaArchivo, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = ExcelReaderFactory.CreateReader(stream);

            if (filaEncabezado < 1) filaEncabezado = 1;

            // Avanzar hasta la fila de encabezado (1-based: leer filaEncabezado-1 veces sin usar)
            int filaActual = 0;
            while (filaActual < filaEncabezado - 1)
            {
                if (!reader.Read()) break;
                filaActual++;
            }

            // Leer fila de encabezado
            var columnas = new List<string>();
            if (!reader.Read())
                return (columnas, new List<Dictionary<string, string>>());

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var titulo = reader.GetValue(i)?.ToString()?.Trim() ?? "";
                // Evitar columnas duplicadas vacías — las nombramos Col_N
                if (string.IsNullOrWhiteSpace(titulo))
                    titulo = $"Col_{i + 1}";
                columnas.Add(titulo);
            }

            // Leer filas de datos
            var filas = new List<Dictionary<string, string>>();
            while (reader.Read())
            {
                // Saltear filas completamente vacías
                bool todosVacios = true;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.GetValue(i) != null && !string.IsNullOrWhiteSpace(reader.GetValue(i).ToString()))
                    { todosVacios = false; break; }
                }
                if (todosVacios) continue;

                var fila = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < columnas.Count && i < reader.FieldCount; i++)
                {
                    var valor = reader.GetValue(i);
                    fila[columnas[i]] = valor switch
                    {
                        null => "",
                        double d => d.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        DateTime dt => dt.ToString("yyyy-MM-dd"),
                        _ => valor.ToString()?.Trim() ?? ""
                    };
                }
                filas.Add(fila);
            }

            return (columnas, filas);
        }

        /// <summary>Solo retorna los nombres de columna (equivalente a CsvReader.LeerColumnas).</summary>
        public static List<string> LeerColumnas(string rutaArchivo, int filaEncabezado = 1)
        {
            var (cols, _) = Leer(rutaArchivo, filaEncabezado);
            return cols;
        }
    }
}
