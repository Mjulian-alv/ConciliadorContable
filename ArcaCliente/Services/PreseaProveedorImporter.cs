using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Convierte las filas de un archivo tabular en <see cref="ConfigPreseaProveedor"/>
    /// segun un <see cref="MapeoColumnasArchivo"/>. No persiste; solo arma la lista.
    /// </summary>
    public static class PreseaProveedorImporter
    {
        /// <summary>Identificador de entidad para el mapeo persistido.</summary>
        public const string Entidad = "ProveedoresPresea";

        /// <summary>
        /// Campos de la entidad disponibles para mapear, con su etiqueta y si es requerido.
        /// El orden define la presentacion en la UI.
        /// </summary>
        public static readonly (string Campo, string Etiqueta, bool Requerido)[] Campos =
        {
            ("Cuit",                    "CUIT (*)",                  true),
            ("CodigoProveedor",         "Codigo proveedor (*)",      true),
            ("CuentaContableProveedor", "Cuenta contable prov. (*)", true),
            ("CuentaDebe",              "Cuenta del debe",           false),
            ("Centro",                  "Centro de costos",          false),
            ("Provincia",               "Provincia",                 false),
            ("Condicion",               "Condicion",                 false),
            ("Descuento",               "Descuento %",               false),
            ("Fiscal",                  "Fiscal (S/N)",              false),
            ("Nombre",                  "Nombre (referencia)",       false),
        };

        public static List<ConfigPreseaProveedor> Parsear(
            TablaLeida tabla, MapeoColumnasArchivo mapeo, out List<string> errores)
        {
            errores = new List<string>();
            var list = new List<ConfigPreseaProveedor>();

            if (tabla == null || tabla.Filas.Count == 0)
            {
                errores.Add("No hay filas para importar. Lea un archivo primero.");
                return list;
            }

            string colCuit = Col(mapeo, "Cuit");
            if (string.IsNullOrEmpty(colCuit))
            {
                errores.Add("Falta mapear la columna del CUIT.");
                return list;
            }

            var nfi = new NumberFormatInfo
            {
                NumberDecimalSeparator = mapeo.SeparadorDecimal == "," ? "," : ".",
                NumberGroupSeparator   = mapeo.SeparadorDecimal == "," ? "." : ","
            };

            int nroFila = mapeo.FilaEncabezado;
            int sinCuit = 0;

            foreach (var row in tabla.Filas)
            {
                nroFila++;
                string cuit = SoloDigitos(Get(row, colCuit));
                if (cuit.Length == 0) { sinCuit++; continue; }

                list.Add(new ConfigPreseaProveedor
                {
                    Cuit                    = cuit,
                    Nombre                  = Get(row, Col(mapeo, "Nombre")),
                    CodigoProveedor         = Get(row, Col(mapeo, "CodigoProveedor")),
                    CuentaContableProveedor = Get(row, Col(mapeo, "CuentaContableProveedor")),
                    CuentaDebe              = Get(row, Col(mapeo, "CuentaDebe")),
                    Centro                  = Get(row, Col(mapeo, "Centro")),
                    Provincia               = Get(row, Col(mapeo, "Provincia")),
                    Condicion               = Get(row, Col(mapeo, "Condicion")),
                    Descuento               = ParseDec(Get(row, Col(mapeo, "Descuento")), nfi),
                    Fiscal                  = Get(row, Col(mapeo, "Fiscal")),
                });
            }

            // Ultimo gana ante CUIT repetido dentro del mismo archivo.
            var dedup = list
                .GroupBy(p => p.Cuit)
                .Select(g => g.Last())
                .ToList();

            if (sinCuit > 0)
                errores.Add($"{sinCuit} fila(s) sin CUIT se omitieron.");

            return dedup;
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static string Col(MapeoColumnasArchivo mapeo, string campo) =>
            mapeo.Mapeo.TryGetValue(campo, out var col) ? col : null;

        private static string Get(Dictionary<string, string> row, string col) =>
            string.IsNullOrEmpty(col) ? string.Empty
            : row.TryGetValue(col, out var v) ? (v ?? string.Empty).Trim()
            : string.Empty;

        private static string SoloDigitos(string s) =>
            string.IsNullOrEmpty(s) ? string.Empty : new string(s.Where(char.IsDigit).ToArray());

        private static decimal ParseDec(string s, NumberFormatInfo nfi)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0m;
            s = s.Replace("(", "-").Replace(")", "").Replace("%", "").Trim();
            return decimal.TryParse(s, NumberStyles.Any, nfi, out var d) ? d : 0m;
        }
    }
}
