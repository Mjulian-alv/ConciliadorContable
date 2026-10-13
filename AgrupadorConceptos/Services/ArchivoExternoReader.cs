using Conciliador.Comun.Services;
// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Operar sobre días tipados evita reinterpretaciones culturales.
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using AgrupadorConceptos.Models;
using ExcelDataReader;

namespace AgrupadorConceptos.Services
{
    /// <summary>
    /// Lectura del archivo externo (el listado del sistema propio) que se concilia
    /// contra el extracto bancario. Acepta Excel o CSV; las columnas se ubican por
    /// nombre aproximado (contiene "fecha" / "importe" / "concepto").
    /// </summary>
    public static class ArchivoExternoReader
    {
        /// <exception cref="InvalidOperationException">
        /// Falta alguna de las columnas Fecha, Importe o Concepto.
        /// </exception>
        public static List<ConciliacionItemExterno> Leer(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            return ext == ".csv" ? LeerCsv(path) : LeerExcel(path);
        }

        private static List<ConciliacionItemExterno> LeerExcel(string path)
        {
            var result = new List<ConciliacionItemExterno>();
            var incidencias = new List<IncidenciaFechaImportacion>();

            using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var ds = reader.AsDataSet(new ExcelDataSetConfiguration
            { ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true } });

            var table = ds.Tables[0];
            int iF = BuscarColumna(table, "fecha");
            int iI = BuscarColumna(table, "importe");
            int iD = BuscarColumna(table, "concepto");

            if (iF < 0 || iI < 0 || iD < 0)
                throw new InvalidOperationException("El archivo debe tener columnas Fecha, Importe y Concepto.");

            int fila=1;
            foreach (DataRow row in table.Rows)
            {
                fila++;
                if (row[iI] == DBNull.Value) continue;
                if (!FechaBancariaParser.TryParse(row[iF],out var fecha)) { incidencias.Add(FechasImportacionException.Incidencia(path,table.TableName,fila,row[iF])); continue; }
                result.Add(new ConciliacionItemExterno
                {
                    // Fecha: 08/10/2026 - TAREA: 00076 - Linea: 2, 3, 4 - Las celdas tipadas no deben pasar por la cultura de Windows antes de conciliar.
                    Fecha   = fecha,
                    Importe = row[iI] is double or float or decimal or int or long
                        ? Math.Abs(Convert.ToDecimal(row[iI], CultureInfo.InvariantCulture))
                        : ParseImporte(row[iI]?.ToString()),
                    Detalle = row[iD]?.ToString()?.Trim()
                });
            }
            if(incidencias.Count>0) throw new FechasImportacionException(incidencias);
            return result;
        }

        private static List<ConciliacionItemExterno> LeerCsv(string path)
        {
            var result = new List<ConciliacionItemExterno>();
            var incidencias = new List<IncidenciaFechaImportacion>();
            var lines = File.ReadAllLines(path);
            if (lines.Length < 2) return result;

            char sep = lines[0].Contains(';') ? ';' : ',';
            var headers = lines[0].Split(sep);
            int iF = BuscarColumnaArr(headers, "fecha");
            int iI = BuscarColumnaArr(headers, "importe");
            int iD = BuscarColumnaArr(headers, "concepto");

            if (iF < 0 || iI < 0 || iD < 0)
                throw new InvalidOperationException("El archivo CSV debe tener columnas Fecha, Importe y Concepto.");

            for (int i = 1; i < lines.Length; i++)
            {
                if(string.IsNullOrWhiteSpace(lines[i])) continue;
                var cols = lines[i].Split(sep);
                if (cols.Length <= Math.Max(iF, Math.Max(iI, iD))) continue;
                if (!FechaBancariaParser.TryParse(cols[iF],out var fecha)) { incidencias.Add(FechasImportacionException.Incidencia(path,null,i+1,cols[iF])); continue; }
                result.Add(new ConciliacionItemExterno
                {
                    Fecha   = fecha,
                    Importe = ParseImporte(cols[iI]),
                    Detalle = cols[iD].Trim()
                });
            }
            if(incidencias.Count>0) throw new FechasImportacionException(incidencias);
            return result;
        }

        private static int BuscarColumna(DataTable table, string nombre) =>
            Enumerable.Range(0, table.Columns.Count)
                .FirstOrDefault(i => table.Columns[i].ColumnName.Trim()
                    .IndexOf(nombre, StringComparison.OrdinalIgnoreCase) >= 0, -1);

        private static int BuscarColumnaArr(string[] headers, string nombre) =>
            Array.FindIndex(headers, h => h.Trim().IndexOf(nombre, StringComparison.OrdinalIgnoreCase) >= 0);

        /// <summary>
        /// Devuelve el importe en valor absoluto: el signo lo aporta el movimiento del
        /// extracto, acá solo interesa la magnitud para emparejar.
        /// Ojo: no es intercambiable con el parseo de importes de la importación de
        /// extractos, que sí conserva el signo.
        /// </summary>
        // Fecha: 08/10/2026 - TAREA: 00076 - Linea: 2, 3 - Una coma argentina es decimal aun si la cultura del equipo es inglesa.
        private static decimal ParseImporte(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0;
            s = s.Replace("$", "").Replace(" ", "").Trim();
            var cultura = s.Contains(',') ? new CultureInfo("es-AR") : CultureInfo.InvariantCulture;
            if (decimal.TryParse(s, NumberStyles.Number | NumberStyles.AllowLeadingSign, cultura, out var v)) return Math.Abs(v);
            throw new FormatException($"Importe externo no válido: '{s}'.");
        }
    }
}
