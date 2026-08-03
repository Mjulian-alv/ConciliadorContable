using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ArcaCliente.Models;
using ClosedXML.Excel;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Importa comprobantes locales desde un archivo cuyo formato y mapeo de columnas
    /// están configurados en un <see cref="PerfilOffline"/>.
    /// </summary>
    public static class LocalFileImporter
    {
        public static List<ComprobanteLocal> Importar(string ruta, PerfilOffline perfil)
        {
            return perfil.TipoArchivo switch
            {
                TipoArchivoOffline.Xlsx                                    => ImportarExcel(ruta, perfil),
                TipoArchivoOffline.Csv or TipoArchivoOffline.Txt => ImportarTexto(ruta, perfil),
                _ => throw new NotSupportedException($"Tipo no soportado: {perfil.TipoArchivo}")
            };
        }

        // ?? Excel ?????????????????????????????????????????????????????????????????

        private static List<ComprobanteLocal> ImportarExcel(string ruta, PerfilOffline perfil)
        {
            using var wb  = new XLWorkbook(ruta);
            var hoja       = string.IsNullOrWhiteSpace(perfil.HojaExcel)
                ? wb.Worksheets.First()
                : wb.Worksheets.Worksheet(perfil.HojaExcel);

            var rango = hoja.RangeUsed();
            if (rango == null) return new();

            var filas = rango.RowsUsed().ToList();
            if (filas.Count < (perfil.TieneCabecera ? 2 : 1)) return new();

            if (perfil.TieneCabecera)
            {
                var colMap = MapColumnasPorNombreExcel(filas[0], perfil);
                return filas.Skip(1)
                    .Select(f => MapFilaExcel(f, colMap, perfil.FormatoFecha, perfil.SeparadorDecimal))
                    .Where(x => x.Fecha != default)
                    .ToList();
            }
            else
            {
                return filas
                    .Select(f => MapFilaExcelPorPosicion(f, perfil))
                    .Where(x => x.Fecha != default)
                    .ToList();
            }
        }

        private static Dictionary<int, string> MapColumnasPorNombreExcel(
            IXLRangeRow headerRow, PerfilOffline perfil)
        {
            var map = new Dictionary<int, string>();
            for (int c = 1; c <= headerRow.CellCount(); c++)
            {
                var h     = headerRow.Cell(c).GetString().Trim();
                var campo = DetectarCampo(h, perfil);
                if (campo != null) map[c] = campo;
            }
            return map;
        }

        private static ComprobanteLocal MapFilaExcel(
            IXLRangeRow fila, Dictionary<int, string> colMap, string formatoFecha, string separadorDecimal)
        {
            var local = new ComprobanteLocal();
            foreach (var (col, campo) in colMap)
                AsignarCampo(local, campo, fila.Cell(col).GetString().Trim(), formatoFecha, separadorDecimal);
            return local;
        }

        private static ComprobanteLocal MapFilaExcelPorPosicion(
            IXLRangeRow fila, PerfilOffline perfil)
        {
            var local = new ComprobanteLocal();
            AsignarExcelPos(local, fila, perfil.PosFecha,            "Fecha",            perfil.FormatoFecha, perfil.SeparadorDecimal);
            AsignarExcelPos(local, fila, perfil.PosPuntoVenta,       "PuntoVenta",       perfil.FormatoFecha, perfil.SeparadorDecimal);
            AsignarExcelPos(local, fila, perfil.PosNumero,           "Numero",           perfil.FormatoFecha, perfil.SeparadorDecimal);
            AsignarExcelPos(local, fila, perfil.PosTipoComprobante,  "TipoComprobante",  perfil.FormatoFecha, perfil.SeparadorDecimal);
            AsignarExcelPos(local, fila, perfil.PosCuit,             "Cuit",             perfil.FormatoFecha, perfil.SeparadorDecimal);
            AsignarExcelPos(local, fila, perfil.PosNombreProveedor,  "NombreProveedor",  perfil.FormatoFecha, perfil.SeparadorDecimal);
            AsignarExcelPos(local, fila, perfil.PosTotal,            "Total",            perfil.FormatoFecha, perfil.SeparadorDecimal);
            return local;
        }

        private static void AsignarExcelPos(ComprobanteLocal local,
            IXLRangeRow fila, int pos, string campo, string fmt, string sepDecimal)
        {
            if (pos >= 1 && pos <= fila.CellCount())
                AsignarCampo(local, campo, fila.Cell(pos).GetString().Trim(), fmt, sepDecimal);
        }

        // ?? Texto (CSV / TXT) ?????????????????????????????????????????????????????

        private static List<ComprobanteLocal> ImportarTexto(string ruta, PerfilOffline perfil)
        {
            var enc       = TextParsingUtils.ObtenerEncoding(perfil.Encoding);
            var sep       = TextParsingUtils.ObtenerSeparador(perfil.Separador);
            var resultado = new List<ComprobanteLocal>();

            using var reader = new StreamReader(ruta, enc);
            bool primera = true;
            Dictionary<int, string> colMap = null;

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var campos = TextParsingUtils.SplitLine(line, sep).ToArray();

                if (primera && perfil.TieneCabecera)
                {
                    colMap  = MapColumnasPorNombreTexto(campos, perfil);
                    primera = false;
                    continue;
                }
                primera = false;

                var local = perfil.TieneCabecera && colMap != null
                    ? MapCamposConHeader(campos, colMap, perfil.FormatoFecha, perfil.SeparadorDecimal)
                    : MapCamposPorPosicion(campos, perfil);

                if (local.Fecha != default)
                    resultado.Add(local);
            }

            return resultado;
        }

        private static Dictionary<int, string> MapColumnasPorNombreTexto(
            string[] headers, PerfilOffline perfil)
        {
            var map = new Dictionary<int, string>();
            for (int i = 0; i < headers.Length; i++)
            {
                var campo = DetectarCampo(headers[i].Trim(), perfil);
                if (campo != null) map[i] = campo;
            }
            return map;
        }

        private static ComprobanteLocal MapCamposConHeader(
            string[] campos, Dictionary<int, string> colMap, string formatoFecha, string separadorDecimal)
        {
            var local = new ComprobanteLocal();
            foreach (var (idx, campo) in colMap)
            {
                if (idx < campos.Length)
                    AsignarCampo(local, campo, campos[idx].Trim(), formatoFecha, separadorDecimal);
            }
            return local;
        }

        private static ComprobanteLocal MapCamposPorPosicion(
            string[] campos, PerfilOffline perfil)
        {
            var local = new ComprobanteLocal();
            var sep   = perfil.SeparadorDecimal;
            AsignarTextoPos(local, campos, perfil.PosFecha            - 1, "Fecha",           perfil.FormatoFecha, sep);
            AsignarTextoPos(local, campos, perfil.PosPuntoVenta       - 1, "PuntoVenta",      perfil.FormatoFecha, sep);
            AsignarTextoPos(local, campos, perfil.PosNumero           - 1, "Numero",          perfil.FormatoFecha, sep);
            AsignarTextoPos(local, campos, perfil.PosTipoComprobante  - 1, "TipoComprobante", perfil.FormatoFecha, sep);
            AsignarTextoPos(local, campos, perfil.PosCuit             - 1, "Cuit",            perfil.FormatoFecha, sep);
            AsignarTextoPos(local, campos, perfil.PosNombreProveedor  - 1, "NombreProveedor", perfil.FormatoFecha, sep);
            AsignarTextoPos(local, campos, perfil.PosTotal            - 1, "Total",           perfil.FormatoFecha, sep);
            return local;
        }

        private static void AsignarTextoPos(ComprobanteLocal local,
            string[] campos, int idx, string campo, string fmt, string sepDecimal)
        {
            if (idx >= 0 && idx < campos.Length)
                AsignarCampo(local, campo, campos[idx].Trim(), fmt, sepDecimal);
        }

        // ?? Helpers compartidos ???????????????????????????????????????????????????

        private static string DetectarCampo(string header, PerfilOffline perfil)
        {
            if (Igual(header, perfil.ColFecha))            return "Fecha";
            if (Igual(header, perfil.ColPuntoVenta))       return "PuntoVenta";
            if (Igual(header, perfil.ColNumero))            return "Numero";
            if (Igual(header, perfil.ColTipoComprobante))  return "TipoComprobante";
            if (Igual(header, perfil.ColCuit))             return "Cuit";
            if (Igual(header, perfil.ColNombreProveedor))  return "NombreProveedor";
            if (Igual(header, perfil.ColTotal))             return "Total";
            return null;
        }

        private static bool Igual(string a, string b)
            => !string.IsNullOrWhiteSpace(b) &&
               string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        private static readonly CultureInfo _culturaAr =
            CultureInfo.GetCultureInfo("es-AR");

        private static void AsignarCampo(ComprobanteLocal local,
            string campo, string valor, string formatoFecha, string separadorDecimal)
        {
            switch (campo)
            {
                case "Fecha":
                    if (DateTime.TryParseExact(valor, formatoFecha ?? "dd/MM/yyyy",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                        local.Fecha = dt;
                    else if (DateTime.TryParse(valor, _culturaAr,
                                 DateTimeStyles.None, out var dt2))
                        local.Fecha = dt2;
                    break;

                case "PuntoVenta":       local.PuntoVenta       = valor; break;
                case "Numero":           local.Numero            = valor; break;
                case "TipoComprobante":  local.TipoComprobante   = valor; break;
                case "Cuit":             local.Cuit              = valor; break;
                case "NombreProveedor":  local.NombreProveedor   = valor; break;

                case "Total":
                    var nfi = new NumberFormatInfo
                    {
                        NumberDecimalSeparator = separadorDecimal == "," ? "," : ".",
                        NumberGroupSeparator   = separadorDecimal == "," ? "." : ","
                    };

                    valor = valor.Replace("-", "").Replace("(", "").Replace(")", ""); // Eliminar guiones que a veces se usan para negativos en algunos formatos

                    if (decimal.TryParse(valor, NumberStyles.Any, nfi, out var d))
                        local.Total = d;
                    break;
            }
        }
    }
}
