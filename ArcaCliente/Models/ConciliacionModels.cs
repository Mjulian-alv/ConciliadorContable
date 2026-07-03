using System;

namespace ArcaCliente.Models
{
    public enum EstadoConciliacion
    {
        Conciliado,
        DiferenciaImporte,
        SoloARCA,
        SoloSistema
    }

    /// <summary>
    /// Resultado del parseo de TipoComprobante (texto ARCA) al tipo interno de Octosis.
    /// </summary>
    public record TipoComprobanteParseado(
        /// <summary>"1" Factura · "2" Nota de Débito · "3" Nota de Crédito. Vacío si no se reconoció.</summary>
        string TipoOctosis,
        /// <summary>Letra del comprobante: A, B, C, M, E. Vacío si no se reconoció.</summary>
        string Letra,
        /// <summary>True si el texto fue mapeado de forma explícita o por fallback con éxito.</summary>
        bool Reconocido);

    public class ComprobanteLocal
    {
        public DateTime Fecha           { get; set; }
        public string   PuntoVenta      { get; set; }
        public string   Numero          { get; set; }
        public string   TipoComprobante { get; set; }
        public string   Cuit            { get; set; }
        public string   NombreProveedor { get; set; }
        public decimal  Total           { get; set; }
    }

    public class ItemConciliacion
    {
        public EstadoConciliacion Estado { get; set; }
        public string Fecha { get; set; }
        public string TipoComprobante { get; set; }
        public string PuntoVenta { get; set; }
        public string Numero { get; set; }
        public string Emisor { get; set; }
        public string TotalARCA                  { get; set; }
        public string TotalSistema                { get; set; }
        public string Diferencia                  { get; set; }
        public string DescripcionTipoComprobante  { get; set; }
        public string CuitProveedor               { get; set; }
        public string NombreProveedor             { get; set; }

        // ?? Campos de integración ????????????????????????????????????????????????

        /// <summary>
        /// Tipo Octosis parseado del TipoComprobante de ARCA.
        /// "1" = Factura, "2" = Nota de Débito, "3" = Nota de Crédito.
        /// Vacío si el tipo no pudo reconocerse.
        /// Solo se popula para items con Estado = SoloARCA.
        /// </summary>
        public string TipoOctosis { get; set; }

        /// <summary>
        /// Letra del comprobante (A, B, C, M, E) parseada del TipoComprobante de ARCA.
        /// Vacío si no pudo reconocerse.
        /// Solo se popula para items con Estado = SoloARCA.
        /// </summary>
        public string Letra { get; set; }

        /// <summary>
        /// Comprobante original de ARCA con todos sus campos (importes, moneda, CUIT emisor, etc.).
        /// Solo presente cuando Estado = SoloARCA. Null en los demás casos.
        /// </summary>
        public ComprobanteCsv SourceArca { get; set; }

        /// <summary>
        /// Resultado de la resolución de moneda del comprobante ARCA.
        /// Indica si es moneda extranjera y provee el tipo de cambio (o lo marca como pendiente).
        /// Solo presente cuando Estado = SoloARCA. Null en los demás casos.
        /// </summary>
        public MonedaInfo Moneda { get; set; }

        /// <summary>
        /// Número de directiva (1-based) que resolvió la conciliación.
        /// 0 cuando el registro quedó sin emparejar (SoloARCA / SoloSistema).
        /// </summary>
        public int DirectivaAplicada { get; set; }

        /// <summary>Descripción de la directiva que resolvió la conciliación. Vacío si no fue emparejado.</summary>
        public string DescripcionDirectiva { get; set; }

        /// <summary>
        /// Valores normalizados usados como clave de matcheo, separados por " | ".
        /// Ejemplo: "CUIT=20332965523 | Tipo=1 | Pto.Vta.=00004 | Núm.=00000686".
        /// Vacío para registros no emparejados (SoloARCA / SoloSistema).
        /// </summary>
        public string DetalleMatcheo { get; set; }

        /// <summary>
        /// Claves intentadas en las directivas anteriores que no encontraron coincidencia,
        /// una por línea. Vacío cuando la directiva 1 fue la que resolvió el matcheo.
        /// Ejemplo:  "? Dir.1: CUIT=20332965523 | Tipo=1 | Pto.Vta.=00004 | Núm.=00000686"
        /// </summary>
        public string FalloDirectivas { get; set; }

        public string EstadoTexto => Estado switch
        {
            EstadoConciliacion.Conciliado        => "? Conciliado",
            EstadoConciliacion.DiferenciaImporte => "? Diferencia",
            EstadoConciliacion.SoloARCA          => "? Solo ARCA",
            EstadoConciliacion.SoloSistema       => "? Solo sistema",
            _                                    => string.Empty
        };
    }

    /// <summary>
    /// Resultado de la conciliación agrupada por CUIT.
    /// Los totales de Notas de Crédito se suman con signo negativo.
    /// </summary>
    public class ItemConciliacionXCuit
    {
        public string  CuitProveedor   { get; set; }
        public string  NombreProveedor { get; set; }
        /// <summary>Suma de importes ARCA para este CUIT (NC con signo negativo).</summary>
        public decimal TotalARCA       { get; set; }
        /// <summary>Suma de importes del sistema para este CUIT (NC con signo negativo).</summary>
        public decimal TotalSistema    { get; set; }
        public decimal Diferencia      => TotalARCA - TotalSistema;
        public int     CantARCA        { get; set; }
        public int     CantSistema     { get; set; }
        public bool    TieneDiferencia => Math.Abs(Diferencia) >= 0.01m;
        public string  EstadoTexto     => TieneDiferencia ? "? Diferencia" : "? OK";
    }
}

