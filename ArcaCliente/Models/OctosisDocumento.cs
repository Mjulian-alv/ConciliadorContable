using System;
using System.Collections.Generic;

namespace ArcaCliente.Models
{
    // ??? Modelos del módulo Proveedores de Octosis ?????????????????????????????
    // Trasladados desde wsmbcg2010 (VB.NET / .NET Framework 4.8) a C# / .NET 8.
    // Reglas: Double ? decimal · Date ? DateTime · listas con new().
    // Sin lógica de negocio: solo propiedades con valores por defecto.
    // Ver §13.2 del análisis para la tabla de equivalencias VB ? C#.

    /// <summary>
    /// Cabecera de un comprobante de proveedores.
    /// Equivale a <c>clsPVDocumento</c> en wsmbcg2010.
    /// Se persiste en <c>PV_D5_DOCUM</c>.
    /// </summary>
    public class OctosisDocumento
    {
        // ?? Identificación ????????????????????????????????????????????????????
        public string NroInterno    { get; set; } = string.Empty;  // DOC_NROINT — generado por NroInternoAsync()
        public string TipoDocumento { get; set; } = string.Empty;  // DOC_TIPDOC: "1" Factura · "2" ND · "3" NC
        public string CodProveedor  { get; set; } = string.Empty;  // DOC_PROVEE — FK a PV_D3_PROVE
        public string Descripcion   { get; set; } = string.Empty;  // DOC_DESCRI
        public string Comentario    { get; set; } = string.Empty;  // DOC_MEMO1

        // ?? Fechas ????????????????????????????????????????????????????????????
        public DateTime FecEmision   { get; set; } = new DateTime(1900, 1, 1);  // DOC_FECEMI
        public DateTime FecRecepcion { get; set; } = new DateTime(1900, 1, 1);  // DOC_FECREC
        public DateTime FecVto       { get; set; } = new DateTime(1900, 1, 1);  // DOC_FECVTO

        // ?? Número de comprobante ?????????????????????????????????????????????
        // En VB era un campo compuesto NRO_DOCUMENTO = Letra(1) + NroSuc(4) + NroDoc(8).
        // En ArcaCliente se almacenan por separado para claridad y para poder
        // poblar directamente desde los campos del CSV de ARCA.
        public string Letra  { get; set; } = string.Empty;  // DOC_LETRA  (1 char)
        public string NroSuc { get; set; } = string.Empty;  // DOC_NROSUC (punto de venta, 4 chars)
        public string NroDoc { get; set; } = string.Empty;  // DOC_NRODOC (8 chars)

        // ?? Sucursal propia de la instalación ?????????????????????????????????
        public string NumeroSucursal { get; set; } = string.Empty;  // DOC_SUCURS — de OctosisConfig.Sucursal

        // ?? Importes ??????????????????????????????????????????????????????????
        public decimal Importe { get; set; }  // DOC_IMPAUS
        public decimal Saldo   { get; set; }  // DOC_SALDO

        // ?? Moneda extranjera ?????????????????????????????????????????????????
        public bool    DocumentoEnDolares { get; set; }  // DOC_DOLARE = "S"/"N" (ver OctosisConfig.MarcaDolares*)
        public decimal TipoCambio         { get; set; }  // DOC_CAMBIO
        public decimal ImporteDolares     { get; set; }  // DOC_IMPDOL
        public decimal SaldoDolares       { get; set; }  // DOC_SALDOL

        // ?? Minuta ????????????????????????????????????????????????????????????
        public string Minuta      { get; set; } = string.Empty;  // DOC_MINUTA (vacío en documentos reales)
        public string MultiMinuta { get; set; } = string.Empty;  // DOC_MULTIM

        // ?? CAE / CAI ?????????????????????????????????????????????????????????
        public string   NroCai         { get; set; } = string.Empty;              // DOC_NROCAI
        public DateTime VencimientoCai { get; set; } = new DateTime(1900, 1, 1);  // DOC_CAIVEN

        // ?? Campos opcionales (vacíos en alta desde ARCA) ?????????????????????
        public string CodigoFactura   { get; set; } = string.Empty;  // DOC_CODIGO
        public string TipoCancelacion { get; set; } = string.Empty;  // DOC_TIPCAN
        public string TipoDiferencia  { get; set; } = string.Empty;  // DOC_TIPDIF
        public string NroRelacion     { get; set; } = string.Empty;  // DOC_NROREL
        public string IdReemplazo     { get; set; } = string.Empty;  // DOC_REEMP
        public string TipoFactura     { get; set; } = string.Empty;  // DOC_TIPFAC
        public string FacturaIv       { get; set; } = string.Empty;  // DOC_FACIV
        public string MonedaOrigen    { get; set; } = string.Empty;  // DOC_MONEDA
        public string Rubro           { get; set; } = string.Empty;  // DOC_RUBRO
        public string Clasif          { get; set; } = string.Empty;  // DOC_CLASIF

        // ?? Códigos impositivos y retenciones (vacíos en alta desde ARCA) ?????
        public string Citi              { get; set; } = string.Empty;  // DOC_CCITI
        public string Capr              { get; set; } = string.Empty;  // DOC_CAPR
        public string Caiva             { get; set; } = string.Empty;  // DOC_CAIVA
        public string Caib              { get; set; } = string.Empty;  // DOC_CAIB
        public string TipoIibb          { get; set; } = string.Empty;  // DOC_TIPOIB
        public bool   RetStaFe          { get; set; }                  // DOC_STAFE
        public string ExencionIibb      { get; set; } = string.Empty;  // DOC_EXEIB
        public string ExencionGanancias { get; set; } = string.Empty;  // DOC_EXEGAN
        public string ExencionIva       { get; set; } = string.Empty;  // DOC_EXEIVA
        public string ExencionStafe     { get; set; } = string.Empty;  // DOC_EXESFE
        public string ExencionSuss      { get; set; } = string.Empty;  // DOC_EXESUS
        public string ExencionMunicipal { get; set; } = string.Empty;  // DOC_EXEMUN
        public string Cass              { get; set; } = string.Empty;  // DOC_CASS
        public string CaMuni            { get; set; } = string.Empty;  // DOC_CAMUNI

        /// <summary>Líneas de impuesto del comprobante. Equivale a <c>aTasas</c>.</summary>
        public List<OctosisDocumentoTasa> Tasas { get; set; } = new();
    }

    /// <summary>
    /// Línea de impuesto de un comprobante.
    /// Equivale a <c>clsPVdocumentoTasa</c> en wsmbcg2010.
    /// Se persiste en <c>PV_P_TASAS0</c>.
    /// </summary>
    public class OctosisDocumentoTasa
    {
        public string  TipoDocumento    { get; set; } = string.Empty;  // TAS_TIPDOC
        public string  NroInterno       { get; set; } = string.Empty;  // TAS_NROINT (FK a PV_D5_DOCUM)
        public string  Linea            { get; set; } = string.Empty;  // TAS_LINEA  (FK a MI_I_PARCF0.par_linea, max 4 chars)
        public decimal Porcentaje       { get; set; }                  // TAS_PORCEN
        public decimal Neto             { get; set; }                  // TAS_NETO
        public decimal Importe          { get; set; }                  // TAS_IMPORT
        public string  NumeroSucursal   { get; set; } = string.Empty;  // TAS_SUCURS
        public string  ColumnaLibroIva  { get; set; } = string.Empty;  // informativa
        public bool    TotalizaSeparado { get; set; }
        public bool    Resta            { get; set; }
        public string  Descripcion      { get; set; } = string.Empty;
        public bool    EsExento         { get; set; }

        // ?? Moneda extranjera ?????????????????????????????????????????????????
        public bool    DocumentoEnDolares { get; set; }  // TAS_DOLARE
        public decimal TipoCambio         { get; set; }  // TAS_CAMBIO
        public decimal ImporteDolares     { get; set; }  // TAS_IMPDOL
        public decimal NetoDolares        { get; set; }  // TAS_NETDOL
    }

    /// <summary>
    /// Tasa del master de impuestos.
    /// Equivale a <c>clsMITasa</c> en wsmbcg2010.
    /// Se lee de <c>MI_I_PARCF0</c> (sin escrituras).
    /// </summary>
    public class OctosisTasa
    {
        public string  NumeroLinea          { get; set; } = string.Empty;  // par_linea
        public string  Descripcion          { get; set; } = string.Empty;  // par_descri
        public string  Decreto              { get; set; } = string.Empty;  // par_decret
        public decimal Porcentaje           { get; set; }                  // par_porcen
        public string  CuentaContable       { get; set; } = string.Empty;  // par_cuenta (incluye prefijo de 2 chars)
        public string  ColumnaLibroIva      { get; set; } = string.Empty;  // par_coliva
        public bool    TotalizaSeparado     { get; set; }                  // par_lintot = 'S'
        public bool    Habilitado           { get; set; }                  // par_habili = 'S'
        public bool    IncluyeEnProveedores { get; set; }                  // par_habmpv = 'S'
        public bool    Resta                { get; set; }                  // par_resnet = 'S'
        public bool    Genera               { get; set; }                  // par_genper = 'S'
        public string  ColumnaImputa        { get; set; } = string.Empty;  // par_column: "D" = Debe · "H" = Haber
        public bool    EsIvaGeneral         { get; set; }                  // par_ivagra = 'S'
        public bool    ImputaNeto           { get; set; }                  // par_impnet = 'S'
        public string  CuentaNeto           { get; set; } = string.Empty;  // par_ctneto (incluye prefijo)
        public string  RelTasaMba           { get; set; } = string.Empty;  // par_relmba
    }

    /// <summary>
    /// Cuenta del plan de cuentas (campos mínimos para la contabilización).
    /// Equivale a <c>clsCtCuenta</c> en wsmbcg2010.
    /// Se lee de <c>ct_ct_pctas</c>.
    /// </summary>
    public class OctosisCuenta
    {
        public string Cuenta        { get; set; } = string.Empty;
        public string Descripcion   { get; set; } = string.Empty;
        public string Estado        { get; set; } = " ";
        public bool   TieneSubmayor { get; set; }
        public string TipoSubmayor  { get; set; } = string.Empty;
    }

    /// <summary>
    /// Línea de asiento contable.
    /// Equivale a <c>clsCTImputacion_Uni</c> en wsmbcg2010.
    /// Se persiste vía el stored procedure <c>sp_pv_conj_imputaciones_uni_alta</c>.
    /// </summary>
    public class OctosisImputacionUni
    {
        public string   Clave             { get; set; } = string.Empty;  // imp_clave  (30 chars, padded)
        public string   Codigo            { get; set; } = string.Empty;  // imp_codigo (6 chars, número de ítem)
        public DateTime Fecha             { get; set; }                  // imp_fecha
        public string   Cuenta            { get; set; } = string.Empty;  // imp_cuenta
        public string   Concepto          { get; set; } = string.Empty;  // imp_concep
        public string   Moneda            { get; set; } = string.Empty;  // imp_moneda
        public decimal  Debe              { get; set; }                  // imp_debe
        public decimal  Haber             { get; set; }                  // imp_haber
        public string   CuentaDescripcion { get; set; } = string.Empty;
        public string   CuentaEstado      { get; set; } = " ";
        public decimal  Cambio            { get; set; }                  // imp_cambio (tipo de cambio bimonetario)
        public string   TipoDocumento     { get; set; } = string.Empty;  // imp_tipdoc
        public string   NroDocumento      { get; set; } = string.Empty;  // imp_numdoc
        public string   Sistema           { get; set; } = string.Empty;  // imp_sistem
        public string   SubSistema        { get; set; } = string.Empty;  // imp_subsis
        public string   Tipo              { get; set; } = string.Empty;  // imp_tipo
        public string   TipoAsiento       { get; set; } = string.Empty;  // imp_tipasi
        public string   NroCosto          { get; set; } = string.Empty;  // imp_numcos
        public string   CodCierre         { get; set; } = string.Empty;  // imp_cierre
        public string   Exclusiva         { get; set; } = string.Empty;  // imp_exclus
        // Submayor (para cuentas con tipo_submayor = "1" — cuentas de proveedor)
        public bool     TieneSubmayor     { get; set; }
        public string   TipoSubmayor      { get; set; } = string.Empty;
        public string   CodProveedor      { get; set; } = string.Empty;  // descripción del submayor
    }
}
