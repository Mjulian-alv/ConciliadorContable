using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;
using ExcelDataReader;
using Conciliador.Comun.Services;

namespace AgrupadorConceptos.Services
{
    /// <summary>Avance de la importación, para que la UI dibuje sus steps.</summary>
    public sealed class ProgresoImportacion
    {
        public const int PasoLeyendo    = 0;
        public const int PasoHomologando = 1;
        public const int PasoGuardando  = 2;

        public int Paso { get; }
        public int Guardados { get; }
        public int Total { get; }

        public ProgresoImportacion(int paso, int guardados = 0, int total = 0)
        {
            Paso = paso;
            Guardados = guardados;
            Total = total;
        }
    }

    /// <summary>
    /// Importación de un extracto (Excel o CSV) según el perfil del banco:
    /// lee, homologa contra el diccionario del perfil y persiste.
    /// </summary>
    public static class ImportacionService
    {
        /// <summary>
        /// Devuelve los movimientos importados, ya con su Id asignado.
        /// No muestra UI: ante un archivo que no matchea el perfil lanza excepción
        /// y el llamador decide cómo informarlo.
        /// </summary>
        /// <param name="progreso">
        /// Se invoca de forma síncrona, en el hilo que llama. A propósito no es
        /// IProgress&lt;T&gt;: Progress&lt;T&gt; despacha al SynchronizationContext capturado
        /// y, corriendo esto en el thread pool, no hay contexto, con lo que los avisos
        /// saldrían encolados y podrían llegar desordenados.
        /// </param>
        public static List<MovimientoProcesado> ImportarArchivo(
            string filePath, PerfilBanco perfil, Action<ProgresoImportacion> progreso = null)
        {
            var swTotal = Stopwatch.StartNew();
            var swParseo = new Stopwatch();
            var swGuardado = new Stopwatch();

            progreso?.Invoke(new ProgresoImportacion(ProgresoImportacion.PasoLeyendo));

            progreso?.Invoke(new ProgresoImportacion(ProgresoImportacion.PasoHomologando));
            var dicHomologacion = HomologacionStorage.ObtenerDiccionario(perfil.Id);
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Cuenta de cada concepto, para CuentaFinal
            var cuentasPorConcepto = HomologacionStorage.ObtenerCuentasPorConcepto();

            swParseo.Start();
            var movimientos = Parsear(filePath, perfil, dicHomologacion, cuentasPorConcepto);
            swParseo.Stop();

            // La cabecera del archivo se inserta recién acá, con el parseo ya resuelto:
            // si el archivo no matchea el perfil, Parsear lanza y no queda un
            // ArchivosImportados huérfano sin movimientos.
            progreso?.Invoke(new ProgresoImportacion(ProgresoImportacion.PasoGuardando, 0, movimientos.Count));
            swGuardado.Start();

            // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Un fallo SQL tampoco debe dejar una importación parcial.
            ImportacionStorage.GuardarArchivo(perfil.Id, Path.GetFileName(filePath), DateTime.Now, movimientos, (guardados, total) =>
                progreso?.Invoke(new ProgresoImportacion(ProgresoImportacion.PasoGuardando, guardados, total)));

            swGuardado.Stop();
            Debug.WriteLine($"[Importar] {movimientos.Count} movs | parseo {swParseo.ElapsedMilliseconds} ms | " +
                            $"guardado {swGuardado.ElapsedMilliseconds} ms | total {swTotal.ElapsedMilliseconds} ms");

            return movimientos;
        }

        /// <exception cref="InvalidOperationException">
        /// El archivo no tiene, en la fila de encabezado del perfil, la columna del concepto.
        /// </exception>
        private static List<MovimientoProcesado> Parsear(
            string filePath, PerfilBanco perfil, IDictionary<string, string> dicHomologacion,
            IDictionary<string, string> cuentasPorConcepto)
        {
            var movimientos = new List<MovimientoProcesado>();
            // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Acumular incidencias sin perder el tipo original de la celda Excel.
            var incidencias = new List<IncidenciaFechaImportacion>();

            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            string ext = Path.GetExtension(filePath).ToLowerInvariant();

            // Fecha: 14/09/2026 - TAREA: 00039 - Linea: 1 - Error al parsear csv que tiene con punto y coma. 
            // TODO: debo poner un selector de separacion para el archivo - esto lo pone fijo
            var config = new ExcelReaderConfiguration()
            {
                AutodetectSeparators = new char[] { ';' }
            };
            using var reader = ext == ".csv"
                ? ExcelReaderFactory.CreateCsvReader(stream,config)
                : ExcelReaderFactory.CreateReader(stream);

            // Avanzar hasta la fila de encabezado
            for (int i = 1; i < perfil.FilaEncabezado; i++)
                reader.Read();

            if (!reader.Read()) throw new FechasImportacionException(new[] { FechasImportacionException.Incidencia(filePath, reader.Name, perfil.FilaEncabezado, null, "No se encontró el encabezado de fecha.") });

            var headers = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
                headers.Add(reader.GetValue(i)?.ToString()?.Trim() ?? "");

            int idxConcepto     = headers.IndexOf(perfil.ColumnaConcepto ?? "");
            int idxDescripcion  = string.IsNullOrEmpty(perfil.ColumnaDescripcion) ? -1 : headers.IndexOf(perfil.ColumnaDescripcion);
            int idxFecha        = string.IsNullOrEmpty(perfil.ColumnaFecha) ? -1 : headers.IndexOf(perfil.ColumnaFecha);
            int idxImporteUnico = perfil.TipoImporte == 1 ? headers.IndexOf(perfil.ColumnaImporteUnico ?? "") : -1;
            int idxDebe         = perfil.TipoImporte == 2 ? headers.IndexOf(perfil.ColumnaDebe ?? "") : -1;
            int idxHaber        = perfil.TipoImporte == 2 ? headers.IndexOf(perfil.ColumnaHaber ?? "") : -1;

            if (idxConcepto == -1)
                throw new InvalidOperationException(
                    "No se encontró la columna del concepto en la fila especificada (verifique el perfil y el archivo).");

            if (idxFecha == -1)
                throw new FechasImportacionException(new[] { FechasImportacionException.Incidencia(filePath, reader.Name, perfil.FilaEncabezado, perfil.ColumnaFecha, "No se encontró la columna de fecha del perfil.") });
            int fila = perfil.FilaEncabezado;

            while (reader.Read())
            {
                fila++;
                string concepto    = reader.GetValue(idxConcepto)?.ToString() ?? "";
                string descripcion = idxDescripcion != -1 ? (reader.GetValue(idxDescripcion)?.ToString() ?? "") : concepto;

                if (string.IsNullOrWhiteSpace(concepto)) continue;
                if (!FechaBancariaParser.TryParse(reader.GetValue(idxFecha), out var fecha))
                {
                    incidencias.Add(FechasImportacionException.Incidencia(filePath, reader.Name, fila, reader.GetValue(idxFecha)));
                    continue;
                }

                decimal debitos = 0, creditos = 0;
                if (perfil.TipoImporte == 1 && idxImporteUnico != -1)
                {
                    decimal importeUnico = ParsearDecimal(reader.GetValue(idxImporteUnico));
                    if (importeUnico < 0) debitos = Math.Abs(importeUnico);
                    else creditos = Math.Abs(importeUnico);
                }
                else if (perfil.TipoImporte == 2)
                {
                    decimal debe  = idxDebe  != -1 ? ParsearDecimal(reader.GetValue(idxDebe))  : 0m;
                    decimal haber = idxHaber != -1 ? ParsearDecimal(reader.GetValue(idxHaber)) : 0m;
                    debitos  = debe > 0 ? -debe : debe;
                    creditos = haber;
                }

                string valorABuscar = perfil.EsCodigo ? concepto : descripcion;
                string conceptoEstandar =
                    HomologacionMatcher.Resolver(dicHomologacion, valorABuscar, perfil.EsCodigo)
                    ?? ConceptosBancarios.PendienteHomologar;

                // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Cuenta final por defecto del concepto
                string cuentaFinal = conceptoEstandar != ConceptosBancarios.PendienteHomologar &&
                                     cuentasPorConcepto.TryGetValue(conceptoEstandar, out string cta)
                    ? cta : "";

                movimientos.Add(new MovimientoProcesado
                {
                    ConceptoOriginal    = concepto,
                    DescripcionOriginal = descripcion,
                    Fecha               = fecha,
                    Debitos             = debitos,
                    Creditos            = creditos,
                    ConceptoEstandar    = conceptoEstandar,
                    ConceptoFinal       = conceptoEstandar == ConceptosBancarios.PendienteHomologar ? "" : conceptoEstandar,
                    CuentaFinal         = cuentaFinal
                });
            }

            if (incidencias.Count > 0) throw new FechasImportacionException(incidencias);
            return movimientos;
        }

        /// <summary>
        /// Los importes pueden venir tipados desde Excel o como texto desde CSV, y con
        /// separador decimal local o invariante: se prueban ambos.
        /// </summary>
        private static decimal ParsearDecimal(object valor)
        {
            if (valor == null) return 0m;
            if (valor is double d) return (decimal)d;
            if (valor is decimal dec) return dec;
            if (valor is int i) return i;

            string strValor = valor.ToString().Trim();
            if (string.IsNullOrEmpty(strValor)) return 0m;

            if (decimal.TryParse(strValor, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal res1)) return res1;
            if (decimal.TryParse(strValor, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal res2)) return res2;

            return 0m;
        }
    }
}
