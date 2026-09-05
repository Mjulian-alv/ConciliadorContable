// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 1 - Import de cuentas contables del legacy
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AgrupadorConceptos.Models;
using ExcelDataReader;

namespace AgrupadorConceptos.Services
{
    /// <summary>
    /// Lectura de un Excel/CSV con el export de cuentas contables del sistema legacy.
    /// El encabezado siempre está en la primera fila: a diferencia de los extractos
    /// bancarios, este archivo lo arma un export propio y no hace falta la
    /// configurabilidad de fila que tiene PerfilBanco.
    /// </summary>
    public static class ImportacionCuentasContablesService
    {
        public static List<string> LeerEncabezados(string filePath)
        {
            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            using var reader = CrearReader(filePath, stream);

            if (!reader.Read()) return new List<string>();

            var headers = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
                headers.Add(reader.GetValue(i)?.ToString()?.Trim() ?? $"Columna{i}");

            return headers;
        }

        public static List<CuentaContable> Leer(
            string filePath, string colCuenta, string colDescripcion, string colCentroCosto)
        {
            var resultado = new List<CuentaContable>();

            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            using var reader = CrearReader(filePath, stream);

            if (!reader.Read()) return resultado;

            var headers = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
                headers.Add(reader.GetValue(i)?.ToString()?.Trim() ?? "");

            int idxCuenta      = headers.IndexOf(colCuenta ?? "");
            int idxDescripcion = headers.IndexOf(colDescripcion ?? "");
            int idxCentroCosto = string.IsNullOrEmpty(colCentroCosto) ? -1 : headers.IndexOf(colCentroCosto);

            if (idxCuenta == -1 || idxDescripcion == -1)
                throw new System.InvalidOperationException(
                    "No se encontraron las columnas de Cuenta y/o Descripción en el archivo.");

            while (reader.Read())
            {
                string cuenta = reader.GetValue(idxCuenta)?.ToString()?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(cuenta)) continue;

                resultado.Add(new CuentaContable
                {
                    Cuenta      = cuenta,
                    Descripcion = reader.GetValue(idxDescripcion)?.ToString()?.Trim() ?? "",
                    CentroCosto = idxCentroCosto != -1
                        ? (reader.GetValue(idxCentroCosto)?.ToString()?.Trim() ?? "")
                        : null
                });
            }

            return resultado;
        }

        private static IExcelDataReader CrearReader(string filePath, Stream stream)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            return ext == ".csv"
                ? ExcelReaderFactory.CreateCsvReader(stream)
                : ExcelReaderFactory.CreateReader(stream);
        }
    }
}
