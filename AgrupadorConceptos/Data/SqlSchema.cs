namespace AgrupadorConceptos.Data
{
    /// <summary>
    /// DDL idempotente del schema bancos. Se ejecuta en cada arranque
    /// (mismo criterio que el viejo CREATE TABLE IF NOT EXISTS de SQLite).
    /// </summary>
    internal static class SqlSchema
    {
        public const string Ddl = @"
IF SCHEMA_ID(N'bancos') IS NULL EXEC(N'CREATE SCHEMA bancos');

IF OBJECT_ID(N'bancos.PerfilesBanco', N'U') IS NULL
CREATE TABLE bancos.PerfilesBanco (
    Id                  INT IDENTITY(1,1) CONSTRAINT PK_PerfilesBanco PRIMARY KEY,
    NombreBanco         NVARCHAR(200) NOT NULL,
    ColumnaConcepto     NVARCHAR(100) NOT NULL,
    ColumnaDescripcion  NVARCHAR(100) NULL,
    EsCodigo            BIT NOT NULL,
    FilaEncabezado      INT NOT NULL CONSTRAINT DF_PerfilesBanco_Fila DEFAULT 1,
    TipoImporte         INT NOT NULL,
    ColumnaImporteUnico NVARCHAR(100) NULL,
    ColumnaDebe         NVARCHAR(100) NULL,
    ColumnaHaber        NVARCHAR(100) NULL,
    ColumnaFecha        NVARCHAR(100) NULL
);

-- Fecha: 05/09/2026 - TAREA: 00021 - Linea: 1 - Catalogo de cuentas contables del legacy
IF OBJECT_ID(N'bancos.CuentasContables', N'U') IS NULL
CREATE TABLE bancos.CuentasContables (
    Id          INT IDENTITY(1,1) CONSTRAINT PK_CuentasContables PRIMARY KEY,
    Cuenta      NVARCHAR(50) NOT NULL,
    Descripcion NVARCHAR(300) NOT NULL,
    CentroCosto NVARCHAR(50) NULL
);

-- Fecha: 05/09/2026 - TAREA: 00021 - Linea: 1 - Indice compuesto simple, sin expresion en la clave
-- SQL Server no acepta CREATE INDEX sobre una expresion (ISNULL(...)) en la lista de columnas;
-- alcanza con indexar las columnas tal cual, porque SQL Server ya trata dos NULL de CentroCosto
-- como iguales a los fines de la unicidad (a diferencia del estandar ANSI), asi que el efecto
-- practico -no duplicar (Cuenta, CentroCosto), incluido CentroCosto NULL- es el mismo.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_CuentasContables_CuentaCentro')
    CREATE UNIQUE INDEX UX_CuentasContables_CuentaCentro
        ON bancos.CuentasContables(Cuenta, CentroCosto);

-- Fecha: 05/09/2026 - TAREA: 00021 - Linea: 2 - Cuenta contable de referencia del perfil
IF COL_LENGTH(N'bancos.PerfilesBanco', N'IdCuentaContable') IS NULL
    ALTER TABLE bancos.PerfilesBanco ADD IdCuentaContable INT NULL
        CONSTRAINT FK_Perfil_CuentaContable REFERENCES bancos.CuentasContables(Id);

IF OBJECT_ID(N'bancos.ConceptosEstandar', N'U') IS NULL
CREATE TABLE bancos.ConceptosEstandar (
    Id     INT IDENTITY(1,1) CONSTRAINT PK_ConceptosEstandar PRIMARY KEY,
    Nombre NVARCHAR(200) NOT NULL CONSTRAINT UQ_ConceptosEstandar_Nombre UNIQUE
);

IF OBJECT_ID(N'bancos.HomologacionConceptos', N'U') IS NULL
CREATE TABLE bancos.HomologacionConceptos (
    Id                 INT IDENTITY(1,1) CONSTRAINT PK_HomologacionConceptos PRIMARY KEY,
    IdPerfilBanco      INT NOT NULL CONSTRAINT FK_Homologacion_Perfil
                           REFERENCES bancos.PerfilesBanco(Id),
    ValorOriginal      NVARCHAR(400) NOT NULL,
    IdConceptoEstandar INT NOT NULL CONSTRAINT FK_Homologacion_Concepto
                           REFERENCES bancos.ConceptosEstandar(Id)
);

IF OBJECT_ID(N'bancos.ArchivosImportados', N'U') IS NULL
CREATE TABLE bancos.ArchivosImportados (
    Id            INT IDENTITY(1,1) CONSTRAINT PK_ArchivosImportados PRIMARY KEY,
    IdPerfilBanco INT NOT NULL CONSTRAINT FK_Archivos_Perfil
                      REFERENCES bancos.PerfilesBanco(Id),
    NombreArchivo NVARCHAR(500) NOT NULL,
    Fecha         DATETIME2(0) NOT NULL
);

IF OBJECT_ID(N'bancos.MovimientosArchivo', N'U') IS NULL
CREATE TABLE bancos.MovimientosArchivo (
    Id                  INT IDENTITY(1,1) CONSTRAINT PK_MovimientosArchivo PRIMARY KEY,
    IdArchivo           INT NOT NULL CONSTRAINT FK_Movimientos_Archivo
                            REFERENCES bancos.ArchivosImportados(Id) ON DELETE CASCADE,
    Fecha               NVARCHAR(30) NULL,
    ConceptoOriginal    NVARCHAR(400) NULL,
    DescripcionOriginal NVARCHAR(1000) NULL,
    Debitos             DECIMAL(18,2) NULL,
    Creditos            DECIMAL(18,2) NULL,
    ConceptoEstandar    NVARCHAR(200) NULL,
    ConceptoFinal       NVARCHAR(400) NULL
);

IF OBJECT_ID(N'bancos.ConciliacionSesiones', N'U') IS NULL
CREATE TABLE bancos.ConciliacionSesiones (
    Id                 INT IDENTITY(1,1) CONSTRAINT PK_ConciliacionSesiones PRIMARY KEY,
    Nombre             NVARCHAR(200) NOT NULL,
    FechaCreacion      DATETIME2(0) NOT NULL,
    IdArchivoImportado INT NOT NULL CONSTRAINT FK_Sesiones_Archivo
                           REFERENCES bancos.ArchivosImportados(Id),
    ConceptosJson      NVARCHAR(MAX) NOT NULL CONSTRAINT DF_Sesiones_Conceptos DEFAULT N'[]',
    Estado             NVARCHAR(50) NOT NULL CONSTRAINT DF_Sesiones_Estado DEFAULT N'EnProceso',
    ArchivosJson       NVARCHAR(MAX) NULL
);

IF OBJECT_ID(N'bancos.ConciliacionItemsExternos', N'U') IS NULL
CREATE TABLE bancos.ConciliacionItemsExternos (
    Id         INT IDENTITY(1,1) CONSTRAINT PK_ConciliacionItemsExternos PRIMARY KEY,
    IdSesion   INT NOT NULL CONSTRAINT FK_ItemsExt_Sesion
                   REFERENCES bancos.ConciliacionSesiones(Id),
    Fecha      NVARCHAR(30) NULL,
    Importe    DECIMAL(18,2) NOT NULL,
    Detalle    NVARCHAR(1000) NULL,
    Conciliado BIT NOT NULL CONSTRAINT DF_ItemsExt_Conciliado DEFAULT 0
);

IF OBJECT_ID(N'bancos.ConciliacionPares', N'U') IS NULL
CREATE TABLE bancos.ConciliacionPares (
    Id                    INT IDENTITY(1,1) CONSTRAINT PK_ConciliacionPares PRIMARY KEY,
    IdSesion              INT NOT NULL CONSTRAINT FK_Pares_Sesion
                              REFERENCES bancos.ConciliacionSesiones(Id),
    IdItemExterno         INT NOT NULL CONSTRAINT FK_Pares_Item
                              REFERENCES bancos.ConciliacionItemsExternos(Id),
    IdMovimientoProcesado INT NOT NULL,
    TipoMatch             NVARCHAR(50) NOT NULL,
    FechaConciliacion     DATETIME2(0) NOT NULL
);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MovimientosArchivo_IdArchivo')
    CREATE INDEX IX_MovimientosArchivo_IdArchivo   ON bancos.MovimientosArchivo(IdArchivo);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_HomologacionConceptos_IdPerfil')
    CREATE INDEX IX_HomologacionConceptos_IdPerfil ON bancos.HomologacionConceptos(IdPerfilBanco);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ArchivosImportados_IdPerfil')
    CREATE INDEX IX_ArchivosImportados_IdPerfil    ON bancos.ArchivosImportados(IdPerfilBanco);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConciliacionItemsExt_IdSesion')
    CREATE INDEX IX_ConciliacionItemsExt_IdSesion  ON bancos.ConciliacionItemsExternos(IdSesion);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConciliacionPares_IdSesion')
    CREATE INDEX IX_ConciliacionPares_IdSesion     ON bancos.ConciliacionPares(IdSesion);
";
    }
}
