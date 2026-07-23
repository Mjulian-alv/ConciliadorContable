namespace ArcaCliente.Services
{
    /// <summary>
    /// DDL idempotente del schema arca. Se ejecuta en cada arranque del módulo
    /// (mismo criterio que el CREATE TABLE IF NOT EXISTS que tenía SQLite).
    /// </summary>
    internal static class ArcaSqlSchema
    {
        public const string Ddl = @"
IF SCHEMA_ID(N'arca') IS NULL EXEC(N'CREATE SCHEMA arca');

IF OBJECT_ID(N'arca.ArcaPerfilesOffline', N'U') IS NULL
CREATE TABLE arca.ArcaPerfilesOffline (
    Id                  NVARCHAR(36)  NOT NULL CONSTRAINT PK_ArcaPerfilesOffline PRIMARY KEY,
    Nombre              NVARCHAR(200) NOT NULL CONSTRAINT DF_ArcaPerfilesOffline_Nombre DEFAULT '',
    TipoArchivo         INT           NOT NULL CONSTRAINT DF_ArcaPerfilesOffline_TipoArchivo DEFAULT 0,
    Separador           NVARCHAR(5)   NOT NULL CONSTRAINT DF_ArcaPerfilesOffline_Separador DEFAULT ';',
    Encoding            NVARCHAR(20)  NOT NULL CONSTRAINT DF_ArcaPerfilesOffline_Encoding DEFAULT 'UTF-8',
    HojaExcel           NVARCHAR(200) NULL,
    TieneCabecera       BIT           NOT NULL CONSTRAINT DF_ArcaPerfilesOffline_TieneCabecera DEFAULT 1,
    ColFecha            NVARCHAR(100) NULL, ColPuntoVenta     NVARCHAR(100) NULL, ColNumero           NVARCHAR(100) NULL,
    ColTipoComprobante  NVARCHAR(100) NULL, ColCuit           NVARCHAR(100) NULL, ColNombreProveedor  NVARCHAR(100) NULL,
    ColTotal            NVARCHAR(100) NULL,
    PosFecha            INT NOT NULL CONSTRAINT DF_ArcaPerfilesOffline_PosFecha DEFAULT 1,
    PosPuntoVenta       INT NOT NULL CONSTRAINT DF_ArcaPerfilesOffline_PosPuntoVenta DEFAULT 2,
    PosNumero           INT NOT NULL CONSTRAINT DF_ArcaPerfilesOffline_PosNumero DEFAULT 3,
    PosTipoComprobante  INT NOT NULL CONSTRAINT DF_ArcaPerfilesOffline_PosTipoComprobante DEFAULT 4,
    PosCuit             INT NOT NULL CONSTRAINT DF_ArcaPerfilesOffline_PosCuit DEFAULT 5,
    PosNombreProveedor  INT NOT NULL CONSTRAINT DF_ArcaPerfilesOffline_PosNombreProveedor DEFAULT 6,
    PosTotal            INT NOT NULL CONSTRAINT DF_ArcaPerfilesOffline_PosTotal DEFAULT 7,
    FormatoFecha        NVARCHAR(30)  NOT NULL CONSTRAINT DF_ArcaPerfilesOffline_FormatoFecha DEFAULT 'dd/MM/yyyy',
    SeparadorDecimal    NVARCHAR(5)   NOT NULL CONSTRAINT DF_ArcaPerfilesOffline_SeparadorDecimal DEFAULT '.',
    CarpetaCsvArca      NVARCHAR(500) NULL,
    SistemaExportacion  INT           NOT NULL CONSTRAINT DF_ArcaPerfilesOffline_SistemaExportacion DEFAULT 0,
    ConfigPreseaJson    NVARCHAR(MAX) NULL,
    DirectivasJson      NVARCHAR(MAX) NOT NULL CONSTRAINT DF_ArcaPerfilesOffline_DirectivasJson DEFAULT '[]'
);

IF OBJECT_ID(N'arca.ArcaPerfilesFiscales', N'U') IS NULL
CREATE TABLE arca.ArcaPerfilesFiscales (
    Id                            NVARCHAR(36)  NOT NULL CONSTRAINT PK_ArcaPerfilesFiscales PRIMARY KEY,
    Nombre                        NVARCHAR(200) NOT NULL CONSTRAINT DF_ArcaPerfilesFiscales_Nombre DEFAULT '',
    Username                      NVARCHAR(50)  NOT NULL CONSTRAINT DF_ArcaPerfilesFiscales_Username DEFAULT '',
    Password                      NVARCHAR(200) NOT NULL CONSTRAINT DF_ArcaPerfilesFiscales_Password DEFAULT '',
    Cuit                          NVARCHAR(20)  NOT NULL CONSTRAINT DF_ArcaPerfilesFiscales_Cuit DEFAULT '',
    IntegracionHabilitada         BIT           NOT NULL CONSTRAINT DF_ArcaPerfilesFiscales_IntegracionHabilitada DEFAULT 0,
    Sistema                       INT           NOT NULL CONSTRAINT DF_ArcaPerfilesFiscales_Sistema DEFAULT 0,
    ConciliacionConnectionString  NVARCHAR(1000) NULL,
    ConciliacionQuery             NVARCHAR(MAX)  NULL,
    OctosisConnectionString       NVARCHAR(1000) NULL,
    ArcaApiUrl                    NVARCHAR(500)  NULL,
    DirectivasJson                NVARCHAR(MAX)  NOT NULL CONSTRAINT DF_ArcaPerfilesFiscales_DirectivasJson DEFAULT '[]'
);

IF OBJECT_ID(N'arca.ArcaEquivalencias', N'U') IS NULL
CREATE TABLE arca.ArcaEquivalencias (
    CodigoAfip  NVARCHAR(20) NOT NULL CONSTRAINT PK_ArcaEquivalencias PRIMARY KEY,
    TipoSistema NVARCHAR(50) NOT NULL CONSTRAINT DF_ArcaEquivalencias_TipoSistema DEFAULT '',
    Letra       NVARCHAR(5)  NOT NULL CONSTRAINT DF_ArcaEquivalencias_Letra DEFAULT ''
);

IF OBJECT_ID(N'arca.PreseaProveedores', N'U') IS NULL
CREATE TABLE arca.PreseaProveedores (
    Cuit                     NVARCHAR(20)  NOT NULL CONSTRAINT PK_PreseaProveedores PRIMARY KEY,
    Nombre                   NVARCHAR(200) NOT NULL CONSTRAINT DF_PreseaProveedores_Nombre DEFAULT '',
    CodigoProveedor          NVARCHAR(50)  NOT NULL CONSTRAINT DF_PreseaProveedores_CodigoProveedor DEFAULT '',
    CuentaContableProveedor  NVARCHAR(50)  NOT NULL CONSTRAINT DF_PreseaProveedores_CuentaContableProveedor DEFAULT '',
    CuentaDebe               NVARCHAR(50)  NOT NULL CONSTRAINT DF_PreseaProveedores_CuentaDebe DEFAULT '',
    Centro                   NVARCHAR(50)  NOT NULL CONSTRAINT DF_PreseaProveedores_Centro DEFAULT '',
    Provincia                NVARCHAR(50)  NOT NULL CONSTRAINT DF_PreseaProveedores_Provincia DEFAULT '',
    Condicion                NVARCHAR(50)  NOT NULL CONSTRAINT DF_PreseaProveedores_Condicion DEFAULT '',
    Descuento                DECIMAL(9,2)  NOT NULL CONSTRAINT DF_PreseaProveedores_Descuento DEFAULT 0,
    Fiscal                   NVARCHAR(5)   NOT NULL CONSTRAINT DF_PreseaProveedores_Fiscal DEFAULT ''
);

IF OBJECT_ID(N'arca.PreseaComprobantesExportados', N'U') IS NULL
CREATE TABLE arca.PreseaComprobantesExportados (
    Clave            NVARCHAR(200) NOT NULL CONSTRAINT PK_PreseaComprobantesExportados PRIMARY KEY,
    CuitEmisor       NVARCHAR(20)  NOT NULL CONSTRAINT DF_PreseaComprobantesExportados_CuitEmisor DEFAULT '',
    TipoCmp          NVARCHAR(10)  NOT NULL CONSTRAINT DF_PreseaComprobantesExportados_TipoCmp DEFAULT '',
    PtoVta           NVARCHAR(10)  NOT NULL CONSTRAINT DF_PreseaComprobantesExportados_PtoVta DEFAULT '',
    Nro              NVARCHAR(20)  NOT NULL CONSTRAINT DF_PreseaComprobantesExportados_Nro DEFAULT '',
    CodAut           NVARCHAR(50)  NOT NULL CONSTRAINT DF_PreseaComprobantesExportados_CodAut DEFAULT '',
    Importe          DECIMAL(18,2) NOT NULL CONSTRAINT DF_PreseaComprobantesExportados_Importe DEFAULT 0,
    FechaComprobante NVARCHAR(30)  NOT NULL CONSTRAINT DF_PreseaComprobantesExportados_FechaComprobante DEFAULT '',
    FechaExportacion DATETIME2(3)  NOT NULL,
    ArchivoGenerado  NVARCHAR(500) NOT NULL CONSTRAINT DF_PreseaComprobantesExportados_ArchivoGenerado DEFAULT '',
    PerfilOfflineId  NVARCHAR(36)  NOT NULL CONSTRAINT DF_PreseaComprobantesExportados_PerfilOfflineId DEFAULT ''
);

IF OBJECT_ID(N'arca.PreseaMapeoColumnas', N'U') IS NULL
CREATE TABLE arca.PreseaMapeoColumnas (
    Entidad    NVARCHAR(100) NOT NULL CONSTRAINT PK_PreseaMapeoColumnas PRIMARY KEY,
    ConfigJson NVARCHAR(MAX) NOT NULL CONSTRAINT DF_PreseaMapeoColumnas_ConfigJson DEFAULT '{}'
);
";
    }
}
