using System;
using System.Collections.Generic;

namespace ArcaCliente.Models
{
    public class PerfilOffline
    {
        public Guid   Id     { get; set; } = Guid.NewGuid();
        public string Nombre { get; set; }

        // ?? Archivo del sistema local ?????????????????????????????????????????????
        public TipoArchivoOffline TipoArchivo { get; set; } = TipoArchivoOffline.Xlsx;

        /// <summary>Separador de columnas para CSV/TXT. Ejemplos: ";" "," "\t" "|"</summary>
        public string Separador { get; set; } = ";";

        /// <summary>Encoding del archivo de texto. "UTF-8" | "Latin-1"</summary>
        public string Encoding { get; set; } = "UTF-8";

        /// <summary>Nombre de la hoja Excel. Null o vacío = primera hoja.</summary>
        public string HojaExcel { get; set; }

        // ?? Cabecera ??????????????????????????????????????????????????????????????
        public bool TieneCabecera { get; set; } = true;

        // Nombres de columna (cuando TieneCabecera = true)
        public string ColFecha            { get; set; }
        public string ColPuntoVenta       { get; set; }
        public string ColNumero           { get; set; }
        public string ColTipoComprobante  { get; set; }
        public string ColCuit             { get; set; }
        public string ColNombreProveedor  { get; set; }
        public string ColTotal            { get; set; }

        // Posiciones 1-based (cuando TieneCabecera = false)
        public int PosFecha              { get; set; } = 1;
        public int PosPuntoVenta         { get; set; } = 2;
        public int PosNumero             { get; set; } = 3;
        public int PosTipoComprobante    { get; set; } = 4;
        public int PosCuit               { get; set; } = 5;
        public int PosNombreProveedor    { get; set; } = 6;
        public int PosTotal              { get; set; } = 7;

        /// <summary>Formato para parsear fechas. Ejemplo: "dd/MM/yyyy"</summary>
        public string FormatoFecha { get; set; } = "dd/MM/yyyy";

        /// <summary>Separador decimal para parsear importes. "." (punto) o "," (coma).</summary>
        public string SeparadorDecimal { get; set; } = ".";

        // ?? Fuente ARCA ???????????????????????????????????????????????????????????
        /// <summary>Última carpeta usada para los CSV de ARCA.</summary>
        public string CarpetaCsvArca { get; set; }

        // ?? Exportación a sistema externo ?????????????????????????????????????????
        /// <summary>
        /// Sistema contable al que se exportarán los comprobantes ARCA.
        /// <see cref="SistemaExportacionOffline.Ninguno"/> desactiva el botón de exportación.
        /// </summary>
        public SistemaExportacionOffline SistemaExportacion { get; set; } = SistemaExportacionOffline.Ninguno;

        /// <summary>
        /// Configuración de campos por defecto para la exportación PRESEA.
        /// Solo relevante cuando <see cref="SistemaExportacion"/> == <see cref="SistemaExportacionOffline.Presea"/>.
        /// </summary>
        public ConfigPresea ConfigPresea { get; set; }

        // ?? Directivas de conciliación ??????????????????????????????????????????
        /// <summary>
        /// Lista ordenada de directivas de conciliación.
        /// La primera es la directiva principal y no puede modificarse.
        /// Cada directiva sucesiva actúa sobre el residual (no emparejado) de la anterior.
        /// </summary>
        public List<DirectivaConciliacion> DirectivasConciliacion { get; set; } = new();

        public override string ToString() => Nombre ?? string.Empty;
    }
}
