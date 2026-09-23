// Fecha: 21/09/2026 - TAREA: 00041 - Linea: 2 - Perfil de la conciliación de percepciones y retenciones
// Nace de la copia de PerfilOffline (Línea 1), recortada a lo que tiene el mayor de PRESEA:
// fecha, asiento, concepto (tipo + número + proveedor juntos), debe y haber. Sin CUIT, punto de
// venta ni exportación a sistema externo, que acá no existen.
using System;
using System.Collections.Generic;

namespace ArcaCliente.Models
{
    public class PerfilOfflinePyR
    {
        public Guid   Id     { get; set; } = Guid.NewGuid();
        public string Nombre { get; set; }

        // ── Mayor de PRESEA (siempre Excel) ────────────────────────────────────────
        /// <summary>Nombre de la hoja. Null o vacío = primera hoja.</summary>
        public string HojaExcel { get; set; }

        /// <summary>
        /// Con cabecera, las Col* son nombres de encabezado. Sin cabecera, son números de
        /// columna 1-based ("1", "2", …), así no hace falta un segundo juego de campos Pos*.
        /// </summary>
        public bool TieneCabecera { get; set; } = true;

        public string ColFecha    { get; set; } = "fecha";
        public string ColAsiento  { get; set; } = "asiento";
        public string ColConcepto { get; set; } = "concepto";
        public string ColDebe     { get; set; } = "debe";
        public string ColHaber    { get; set; } = "haber";

        /// <summary>Formato para fechas que vengan como texto. Ejemplo: "dd/MM/yyyy"</summary>
        public string FormatoFecha { get; set; } = "dd/MM/yyyy";

        /// <summary>Separador decimal para importes que vengan como texto. "." o ",".</summary>
        public string SeparadorDecimal { get; set; } = ".";

        // ── Fuente ARCA ────────────────────────────────────────────────────────────
        /// <summary>Última carpeta usada con los archivos de ARCA (xls, xlsx, html).</summary>
        public string CarpetaArca { get; set; }

        // ── Cuentas ────────────────────────────────────────────────────────────────
        public List<CuentaPyR> Cuentas { get; set; } = new();

        // ── Directivas de conciliación ─────────────────────────────────────────────
        // Fecha: 21/09/2026 - TAREA: 00041 - Linea: 6 - Directivas propias de PyR (antes el tipo copiado de Offline); un perfil nuevo arranca con las 3 por defecto
        public List<DirectivaPyR> DirectivasConciliacion { get; set; } = DirectivaPyR.CrearPredeterminadas();

        public int CantidadCuentas => Cuentas?.Count ?? 0;

        public override string ToString() => Nombre ?? string.Empty;
    }
}
