using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;
using ExcelDataReader;

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

            swParseo.Start();
            var movimientos = Parsear(filePath, perfil, dicHomologacion);
            swParseo.Stop();

            // La cabecera del archivo se inserta recién acá, con el parseo ya resuelto:
            // si el archivo no matchea el perfil, Parsear lanza y no queda un
            // ArchivosImportados huérfano sin movimientos.
            progreso?.Invoke(new ProgresoImportacion(ProgresoImportacion.PasoGuardando, 0, movimientos.Count));
            swGuardado.Start();

            int idArchivo = ArchivoImportadoStorage.Insertar(perfil.Id, Path.GetFileName(filePath), DateTime.Now);
            foreach (var mov in movimientos)
                mov.IdArchivo = idArchivo;

            MovimientoStorage.InsertarLote(movimientos, (guardados, total) =>
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
            string filePath, PerfilBanco perfil, IDictionary<string, string> dicHomologacion)
        {
            var movimientos = new List<MovimientoProcesado>();

            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            using var reader = ext == ".csv"
                ? ExcelReaderFactory.CreateCsvReader(stream)
                : ExcelReaderFactory.CreateReader(stream);

            // Avanzar hasta la fila de encabezado
            for (int i = 1; i < perfil.FilaEncabezado; i++)
                reader.Read();

            if (!reader.Read()) return movimientos;

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

            while (reader.Read())
            {
                string concepto    = reader.GetValue(idxConcepto)?.ToString() ?? "";
                string descripcion = idxDescripcion != -1 ? (reader.GetValue(idxDescripcion)?.ToString() ?? "") : concepto;
                string fecha       = idxFecha != -1 ? (reader.GetValue(idxFecha)?.ToString() ?? "") : "";

                if (string.IsNullOrWhiteSpace(concepto)) continue;

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

                movimientos.Add(new MovimientoProcesado
                {
                    ConceptoOriginal    = concepto,
                    DescripcionOriginal = descripcion,
                    Fecha               = fecha,
                    Debitos             = debitos,
                    Creditos            = creditos,
                    ConceptoEstandar    = conceptoEstandar,
                    ConceptoFinal       = conceptoEstandar == ConceptosBancarios.PendienteHomologar ? "" : conceptoEstandar
                });
            }

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
