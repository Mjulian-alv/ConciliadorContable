using System.Collections.Generic;

namespace ArcaCliente.Models
{
    /// <summary>
    /// Configuracion parametrizable para importar un maestro de PRESEA desde un archivo
    /// CSV/TXT/Excel: formato del archivo + mapeo de cada campo de la entidad al nombre de
    /// columna del archivo. Es generica: se persiste por <see cref="Entidad"/>, de modo que
    /// agregar nuevos maestros (monedas, provincias, etc.) sea "copiar y pegar".
    /// </summary>
    public class MapeoColumnasArchivo
    {
        /// <summary>Identificador de la entidad/maestro, p.ej. "ProveedoresPresea".</summary>
        public string Entidad { get; set; } = string.Empty;

        /// <summary>Separador de campos para CSV/TXT. ";" "," "\t" "|".</summary>
        public string Separador { get; set; } = ";";

        /// <summary>Encoding del archivo de texto. "UTF-8" | "Latin-1".</summary>
        public string Encoding { get; set; } = "UTF-8";

        /// <summary>Fila del encabezado (1-based). Permite saltar filas previas en Excel/CSV.</summary>
        public int FilaEncabezado { get; set; } = 1;

        /// <summary>Nombre de la hoja Excel. Null/vacio = primera hoja.</summary>
        public string HojaExcel { get; set; }

        /// <summary>Separador decimal para parsear importes. "." o ",".</summary>
        public string SeparadorDecimal { get; set; } = ".";

        /// <summary>Mapa campoEntidad -> nombre de columna del archivo. Campo sin entrada = no mapeado.</summary>
        public Dictionary<string, string> Mapeo { get; set; } = new();
    }
}
