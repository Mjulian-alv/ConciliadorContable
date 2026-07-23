# ArcaCliente → SQL Server (Etapa 2) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Migrar las 6 tablas de configuración de ArcaCliente (`ArcaSqliteStorage.cs`) de SQLite al schema `arca` de la base SQL Server común, con el módulo auto-inicializándose (sin coordinación del shell), y un migrador one-shot para los datos existentes.

**Architecture:** Mismo patrón que Etapa 1 (`bancos`): `ArcaCliente` consume `Conciliador.Comun.SqlDb` para la conexión. Se renombra `ArcaSqliteStorage` → `ArcaSqlStorage` (el nombre actual es engañoso tras el cambio). `ArcaStorageConfig` deja de coordinar una ruta de archivo SQLite con el shell y pasa a ser solo el gate de auto-inicialización, invocado desde los 3 forms de entrada del módulo. Se elimina `MigrarDesdeLegacyIfNeeded` (código muerto confirmado, sería un riesgo de pérdida de datos bajo un store compartido).

**Tech Stack:** .NET 8 WinForms, Microsoft.Data.SqlClient 6.1.4 (ya referenciado en `ArcaCliente.csproj` para la integración Octosis — no requiere cambio de versión), ADO.NET puro (sin Dapper, como ya está en este proyecto).

## Global Constraints

- `dotnet restore` está roto en este repo (nuget.config) — compilar con `dotnet build ConciliadorContable.slnx --no-restore`. Si un paquete nuevo no está en caché local, restaurar solo ese proyecto (`dotnet restore <proyecto>.csproj`) o dejarlo para Visual Studio.
- No hay proyecto de tests en la solución — la verificación es build + regresión manual.
- Mensajes de commit en español, prefijos `feat:`/`refactor:`/`chore:`/`fix:`.
- El SQL de negocio no se porta al dialecto T-SQL (prefijo `arca.`, `@Param`, `MERGE`, tipos) hasta las Tasks 2-4 — Task 1 solo deja compilando la plomería (DDL + tipos de conexión). Esto es igual al patrón de Etapa 1 (Task 5 → Task 6): el módulo **no funciona end-to-end** hasta terminar Task 4.
- El login SQL `conciliador` no tiene `DEFAULT_SCHEMA` configurado (default es `dbo`) — toda tabla `arca.*` debe referenciarse con el prefijo explícito o la query falla con "Invalid object name".
- Los 6 wrappers (`PerfilStorage.cs`, `PerfilOfflineStorage.cs`, `MapeoColumnasStorage.cs`, `PreseaExportMemoryStorage.cs`, `PreseaProveedorStorage.cs`, `TipoComprobanteStorage.cs`) NO cambian su interfaz pública — solo el rename mecánico de la clase que invocan.

---

### Task 1: Schema `arca`, rename a `ArcaSqlStorage`, autosuficiencia

**Files:**
- Create: `ArcaCliente/Services/ArcaSqlSchema.cs`
- Rename + rewrite: `ArcaCliente/Services/ArcaSqliteStorage.cs` → `ArcaCliente/Services/ArcaSqlStorage.cs`
- Modify: `ArcaCliente/Services/ArcaStorageConfig.cs`
- Modify: `ArcaCliente/Services/PerfilStorage.cs`, `PerfilOfflineStorage.cs`, `MapeoColumnasStorage.cs`, `PreseaExportMemoryStorage.cs`, `PreseaProveedorStorage.cs`, `TipoComprobanteStorage.cs` (rename mecánico `ArcaSqliteStorage.` → `ArcaSqlStorage.`)
- Modify: `ArcaCliente/ArcaCliente.csproj` (agregar ProjectReference)
- Modify: `ArcaCliente/FormPerfilesOffline.cs`, `ArcaCliente/FormComprobantesOffline.cs`, `ArcaCliente/FormEquivalencias.cs` (auto-init)
- Modify: `ConciliadorContable/Forms/FormMenuPrincipal.cs:18-19` (eliminar coordinación host↔módulo)

**Interfaces:**
- Consumes: `Conciliador.Comun.SqlDb.GetConnection()` (Etapa 1, ya existe).
- Produces: `ArcaCliente.Services.ArcaSqlStorage` con los mismos métodos públicos que tenía `ArcaSqliteStorage` (mismas firmas, mismos nombres) — Tasks 2-4 solo tocan los *cuerpos* SQL de estos métodos, no sus firmas. `ArcaStorageConfig.Initialize()` sigue siendo el método público que gatilla la inicialización, ahora idempotente y sin parámetros de configuración de ruta.

- [ ] **Step 1: Agregar la referencia a Conciliador.Comun**

En `ArcaCliente/ArcaCliente.csproj`, agregar dentro del `<ItemGroup>` existente (junto a los `PackageReference`):

```xml
<ItemGroup>
  <ProjectReference Include="..\Conciliador.Comun\Conciliador.Comun.csproj" />
</ItemGroup>
```

(Como `<ItemGroup>` distinto al de los `PackageReference`, siguiendo el mismo patrón que se usó en `AgrupadorConceptos.csproj` en Etapa 1.) No tocar el `PackageReference` de `Microsoft.Data.SqlClient` que ya está en el csproj — lo sigue usando el código de integración con Octosis (`OctosisDocumentoService.cs` y otros), que no forma parte de este plan.

- [ ] **Step 2: Crear el DDL del schema arca**

Crear `ArcaCliente/Services/ArcaSqlSchema.cs`:

```csharp
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
```

- [ ] **Step 3: Renombrar y reescribir ArcaSqliteStorage.cs → ArcaSqlStorage.cs**

Borrar `ArcaCliente/Services/ArcaSqliteStorage.cs` y crear `ArcaCliente/Services/ArcaSqlStorage.cs` con este contenido (mismos métodos y firmas que el original; cambios: tipos de conexión SQL Server, `InitializeDatabase()` ejecuta el DDL nuevo, y se elimina `MigrarDesdeLegacyIfNeeded` + sus 3 helpers privados + `SafeDelete`, que solo ellos usaban):

```csharp
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using ArcaCliente.Models;
using Conciliador.Comun;
using Microsoft.Data.SqlClient;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Acceso SQL unificado para la configuración de ArcaCliente.
    /// Todas las tablas viven en el schema arca de la base común (Conciliador.Comun.SqlDb).
    /// </summary>
    internal static class ArcaSqlStorage
    {
        private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = false };

        // ── Inicialización ────────────────────────────────────────────────────────

        public static void InitializeDatabase()
        {
            using var cn = Open();
            Execute(cn, ArcaSqlSchema.Ddl);
        }

        // ── Perfiles Offline ──────────────────────────────────────────────────────

        public static List<PerfilOffline> LoadPerfilesOffline()
        {
            var list = new List<PerfilOffline>();
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT * FROM ArcaPerfilesOffline ORDER BY Nombre";
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var p = new PerfilOffline
                {
                    Id                 = Guid.Parse(r.GetString(r.GetOrdinal("Id"))),
                    Nombre             = r.GetString(r.GetOrdinal("Nombre")),
                    TipoArchivo        = (TipoArchivoOffline)r.GetInt32(r.GetOrdinal("TipoArchivo")),
                    Separador          = r.GetString(r.GetOrdinal("Separador")),
                    Encoding           = r.GetString(r.GetOrdinal("Encoding")),
                    HojaExcel          = r.IsDBNull(r.GetOrdinal("HojaExcel")) ? null : r.GetString(r.GetOrdinal("HojaExcel")),
                    TieneCabecera      = r.GetInt32(r.GetOrdinal("TieneCabecera")) == 1,
                    ColFecha           = Str(r, "ColFecha"),
                    ColPuntoVenta      = Str(r, "ColPuntoVenta"),
                    ColNumero          = Str(r, "ColNumero"),
                    ColTipoComprobante = Str(r, "ColTipoComprobante"),
                    ColCuit            = Str(r, "ColCuit"),
                    ColNombreProveedor = Str(r, "ColNombreProveedor"),
                    ColTotal           = Str(r, "ColTotal"),
                    PosFecha           = r.GetInt32(r.GetOrdinal("PosFecha")),
                    PosPuntoVenta      = r.GetInt32(r.GetOrdinal("PosPuntoVenta")),
                    PosNumero          = r.GetInt32(r.GetOrdinal("PosNumero")),
                    PosTipoComprobante = r.GetInt32(r.GetOrdinal("PosTipoComprobante")),
                    PosCuit            = r.GetInt32(r.GetOrdinal("PosCuit")),
                    PosNombreProveedor = r.GetInt32(r.GetOrdinal("PosNombreProveedor")),
                    PosTotal           = r.GetInt32(r.GetOrdinal("PosTotal")),
                    FormatoFecha       = r.GetString(r.GetOrdinal("FormatoFecha")),
                    SeparadorDecimal   = r.GetString(r.GetOrdinal("SeparadorDecimal")),
                    CarpetaCsvArca     = Str(r, "CarpetaCsvArca"),
                    SistemaExportacion = (SistemaExportacionOffline)r.GetInt32(r.GetOrdinal("SistemaExportacion")),
                    ConfigPresea       = DeserializeOrNull<ConfigPresea>(Str(r, "ConfigPreseaJson")),
                    DirectivasConciliacion = Deserialize<List<DirectivaConciliacion>>(Str(r, "DirectivasJson")) ?? new()
                };

                if (p.DirectivasConciliacion.Count == 0)
                    p.DirectivasConciliacion.Add(DirectivaConciliacion.CrearPrimaria());

                list.Add(p);
            }
            return list;
        }

        public static void SavePerfilesOffline(List<PerfilOffline> perfiles)
        {
            using var cn = Open();
            using var tx = cn.BeginTransaction();

            Execute(cn, "DELETE FROM ArcaPerfilesOffline", tx);

            foreach (var p in perfiles)
            {
                using var cmd = cn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    INSERT INTO ArcaPerfilesOffline
                        (Id, Nombre, TipoArchivo, Separador, Encoding, HojaExcel,
                         TieneCabecera, ColFecha, ColPuntoVenta, ColNumero, ColTipoComprobante,
                         ColCuit, ColNombreProveedor, ColTotal,
                         PosFecha, PosPuntoVenta, PosNumero, PosTipoComprobante,
                         PosCuit, PosNombreProveedor, PosTotal,
                         FormatoFecha, SeparadorDecimal, CarpetaCsvArca,
                         SistemaExportacion, ConfigPreseaJson, DirectivasJson)
                    VALUES
                        ($Id, $Nombre, $TipoArchivo, $Separador, $Encoding, $HojaExcel,
                         $TieneCabecera, $ColFecha, $ColPuntoVenta, $ColNumero, $ColTipoComprobante,
                         $ColCuit, $ColNombreProveedor, $ColTotal,
                         $PosFecha, $PosPuntoVenta, $PosNumero, $PosTipoComprobante,
                         $PosCuit, $PosNombreProveedor, $PosTotal,
                         $FormatoFecha, $SeparadorDecimal, $CarpetaCsvArca,
                         $SistemaExportacion, $ConfigPreseaJson, $DirectivasJson)";

                cmd.Parameters.AddWithValue("$Id",                  p.Id.ToString());
                cmd.Parameters.AddWithValue("$Nombre",              p.Nombre ?? "");
                cmd.Parameters.AddWithValue("$TipoArchivo",         (int)p.TipoArchivo);
                cmd.Parameters.AddWithValue("$Separador",           p.Separador ?? ";");
                cmd.Parameters.AddWithValue("$Encoding",            p.Encoding ?? "UTF-8");
                cmd.Parameters.AddWithValue("$HojaExcel",           (object?)p.HojaExcel ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$TieneCabecera",       p.TieneCabecera ? 1 : 0);
                cmd.Parameters.AddWithValue("$ColFecha",            (object?)p.ColFecha ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$ColPuntoVenta",       (object?)p.ColPuntoVenta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$ColNumero",           (object?)p.ColNumero ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$ColTipoComprobante",  (object?)p.ColTipoComprobante ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$ColCuit",             (object?)p.ColCuit ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$ColNombreProveedor",  (object?)p.ColNombreProveedor ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$ColTotal",            (object?)p.ColTotal ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$PosFecha",            p.PosFecha);
                cmd.Parameters.AddWithValue("$PosPuntoVenta",       p.PosPuntoVenta);
                cmd.Parameters.AddWithValue("$PosNumero",           p.PosNumero);
                cmd.Parameters.AddWithValue("$PosTipoComprobante",  p.PosTipoComprobante);
                cmd.Parameters.AddWithValue("$PosCuit",             p.PosCuit);
                cmd.Parameters.AddWithValue("$PosNombreProveedor",  p.PosNombreProveedor);
                cmd.Parameters.AddWithValue("$PosTotal",            p.PosTotal);
                cmd.Parameters.AddWithValue("$FormatoFecha",        p.FormatoFecha ?? "dd/MM/yyyy");
                cmd.Parameters.AddWithValue("$SeparadorDecimal",    p.SeparadorDecimal ?? ".");
                cmd.Parameters.AddWithValue("$CarpetaCsvArca",      (object?)p.CarpetaCsvArca ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$SistemaExportacion",  (int)p.SistemaExportacion);
                cmd.Parameters.AddWithValue("$ConfigPreseaJson",    p.ConfigPresea == null ? DBNull.Value : JsonSerializer.Serialize(p.ConfigPresea, JsonOpts));
                cmd.Parameters.AddWithValue("$DirectivasJson",      JsonSerializer.Serialize(p.DirectivasConciliacion, JsonOpts));
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }

        // ── Perfiles Fiscales ─────────────────────────────────────────────────────

        public static List<PerfilFiscal> LoadPerfilesFiscales()
        {
            var list = new List<PerfilFiscal>();
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT * FROM ArcaPerfilesFiscales ORDER BY Nombre";
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new PerfilFiscal
                {
                    Id                           = Guid.Parse(r.GetString(r.GetOrdinal("Id"))),
                    Nombre                       = r.GetString(r.GetOrdinal("Nombre")),
                    Username                     = r.GetString(r.GetOrdinal("Username")),
                    Password                     = r.GetString(r.GetOrdinal("Password")),
                    Cuit                         = r.GetString(r.GetOrdinal("Cuit")),
                    IntegracionHabilitada        = r.GetInt32(r.GetOrdinal("IntegracionHabilitada")) == 1,
                    Sistema                      = (SistemaIntegracion)r.GetInt32(r.GetOrdinal("Sistema")),
                    ConciliacionConnectionString = Str(r, "ConciliacionConnectionString"),
                    ConciliacionQuery            = Str(r, "ConciliacionQuery"),
                    OctosisConnectionString      = Str(r, "OctosisConnectionString"),
                    ArcaApiUrl                   = Str(r, "ArcaApiUrl"),
                    DirectivasConciliacion       = Deserialize<List<DirectivaConciliacion>>(Str(r, "DirectivasJson")) ?? new()
                });
            }
            return list;
        }

        public static void SavePerfilesFiscales(List<PerfilFiscal> perfiles)
        {
            using var cn = Open();
            using var tx = cn.BeginTransaction();

            Execute(cn, "DELETE FROM ArcaPerfilesFiscales", tx);

            foreach (var p in perfiles)
            {
                using var cmd = cn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    INSERT INTO ArcaPerfilesFiscales
                        (Id, Nombre, Username, Password, Cuit,
                         IntegracionHabilitada, Sistema,
                         ConciliacionConnectionString, ConciliacionQuery,
                         OctosisConnectionString, ArcaApiUrl, DirectivasJson)
                    VALUES
                        ($Id, $Nombre, $Username, $Password, $Cuit,
                         $IntegracionHabilitada, $Sistema,
                         $ConciliacionConnectionString, $ConciliacionQuery,
                         $OctosisConnectionString, $ArcaApiUrl, $DirectivasJson)";

                cmd.Parameters.AddWithValue("$Id",                           p.Id.ToString());
                cmd.Parameters.AddWithValue("$Nombre",                       p.Nombre ?? "");
                cmd.Parameters.AddWithValue("$Username",                     p.Username ?? "");
                cmd.Parameters.AddWithValue("$Password",                     p.Password ?? "");
                cmd.Parameters.AddWithValue("$Cuit",                         p.Cuit ?? "");
                cmd.Parameters.AddWithValue("$IntegracionHabilitada",        p.IntegracionHabilitada ? 1 : 0);
                cmd.Parameters.AddWithValue("$Sistema",                      (int)p.Sistema);
                cmd.Parameters.AddWithValue("$ConciliacionConnectionString", (object?)p.ConciliacionConnectionString ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$ConciliacionQuery",            (object?)p.ConciliacionQuery ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$OctosisConnectionString",      (object?)p.OctosisConnectionString ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$ArcaApiUrl",                   (object?)p.ArcaApiUrl ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$DirectivasJson",               JsonSerializer.Serialize(p.DirectivasConciliacion, JsonOpts));
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }

        // ── Equivalencias ─────────────────────────────────────────────────────────

        public static List<EquivalenciaTipoComprobante> LoadEquivalencias()
        {
            var list = new List<EquivalenciaTipoComprobante>();
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT CodigoAfip, TipoSistema, Letra FROM ArcaEquivalencias";
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new EquivalenciaTipoComprobante
                {
                    CodigoAfip  = r.GetString(0),
                    TipoSistema = r.GetString(1),
                    Letra       = r.GetString(2)
                });
            return list;
        }

        public static void SaveEquivalencias(IEnumerable<EquivalenciaTipoComprobante> items)
        {
            using var cn = Open();
            using var tx = cn.BeginTransaction();

            Execute(cn, "DELETE FROM ArcaEquivalencias", tx);

            foreach (var i in items)
            {
                using var cmd = cn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = "INSERT INTO ArcaEquivalencias (CodigoAfip, TipoSistema, Letra) VALUES ($c, $t, $l)";
                cmd.Parameters.AddWithValue("$c", i.CodigoAfip ?? "");
                cmd.Parameters.AddWithValue("$t", i.TipoSistema ?? "");
                cmd.Parameters.AddWithValue("$l", i.Letra ?? "");
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }

        // ── Presea: proveedores (mapa por CUIT) ────────────────────────────────────

        public static List<ConfigPreseaProveedor> LoadPreseaProveedores()
        {
            var list = new List<ConfigPreseaProveedor>();
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT * FROM PreseaProveedores ORDER BY Nombre";
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(ReadPreseaProveedor(r));
            return list;
        }

        public static ConfigPreseaProveedor GetPreseaProveedor(string cuit)
        {
            string key = SoloDigitos(cuit);
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT * FROM PreseaProveedores WHERE Cuit = $cuit";
            cmd.Parameters.AddWithValue("$cuit", key);
            using var r = cmd.ExecuteReader();
            return r.Read() ? ReadPreseaProveedor(r) : null;
        }

        public static void UpsertPreseaProveedor(ConfigPreseaProveedor p)
        {
            using var cn = Open();
            UpsertPreseaProveedor(cn, null, p);
        }

        public static void UpsertPreseaProveedores(IEnumerable<ConfigPreseaProveedor> proveedores)
        {
            using var cn = Open();
            using var tx = cn.BeginTransaction();
            foreach (var p in proveedores)
                UpsertPreseaProveedor(cn, tx, p);
            tx.Commit();
        }

        private static void UpsertPreseaProveedor(SqlConnection cn, SqlTransaction tx, ConfigPreseaProveedor p)
        {
            using var cmd = cn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
                INSERT INTO PreseaProveedores
                    (Cuit, Nombre, CodigoProveedor, CuentaContableProveedor, CuentaDebe,
                     Centro, Provincia, Condicion, Descuento, Fiscal)
                VALUES
                    ($Cuit, $Nombre, $CodigoProveedor, $CuentaContableProveedor, $CuentaDebe,
                     $Centro, $Provincia, $Condicion, $Descuento, $Fiscal)
                ON CONFLICT(Cuit) DO UPDATE SET
                    Nombre = excluded.Nombre,
                    CodigoProveedor = excluded.CodigoProveedor,
                    CuentaContableProveedor = excluded.CuentaContableProveedor,
                    CuentaDebe = excluded.CuentaDebe,
                    Centro = excluded.Centro,
                    Provincia = excluded.Provincia,
                    Condicion = excluded.Condicion,
                    Descuento = excluded.Descuento,
                    Fiscal = excluded.Fiscal";

            cmd.Parameters.AddWithValue("$Cuit",                    SoloDigitos(p.Cuit));
            cmd.Parameters.AddWithValue("$Nombre",                  p.Nombre ?? "");
            cmd.Parameters.AddWithValue("$CodigoProveedor",         p.CodigoProveedor ?? "");
            cmd.Parameters.AddWithValue("$CuentaContableProveedor", p.CuentaContableProveedor ?? "");
            cmd.Parameters.AddWithValue("$CuentaDebe",              p.CuentaDebe ?? "");
            cmd.Parameters.AddWithValue("$Centro",                  p.Centro ?? "");
            cmd.Parameters.AddWithValue("$Provincia",               p.Provincia ?? "");
            cmd.Parameters.AddWithValue("$Condicion",               p.Condicion ?? "");
            cmd.Parameters.AddWithValue("$Descuento",               p.Descuento.ToString(CultureInfo.InvariantCulture));
            cmd.Parameters.AddWithValue("$Fiscal",                  p.Fiscal ?? "");
            cmd.ExecuteNonQuery();
        }

        private static ConfigPreseaProveedor ReadPreseaProveedor(SqlDataReader r) => new()
        {
            Cuit                    = r.GetString(r.GetOrdinal("Cuit")),
            Nombre                  = r.GetString(r.GetOrdinal("Nombre")),
            CodigoProveedor         = r.GetString(r.GetOrdinal("CodigoProveedor")),
            CuentaContableProveedor = r.GetString(r.GetOrdinal("CuentaContableProveedor")),
            CuentaDebe              = r.GetString(r.GetOrdinal("CuentaDebe")),
            Centro                  = r.GetString(r.GetOrdinal("Centro")),
            Provincia               = r.GetString(r.GetOrdinal("Provincia")),
            Condicion               = r.GetString(r.GetOrdinal("Condicion")),
            Descuento               = ParseDec(Str(r, "Descuento")),
            Fiscal                  = r.GetString(r.GetOrdinal("Fiscal")),
        };

        // ── Presea: memoria de exportados ───────────────────────────────────────────

        public static bool ExisteComprobanteExportado(string clave)
        {
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT 1 FROM PreseaComprobantesExportados WHERE Clave = $clave LIMIT 1";
            cmd.Parameters.AddWithValue("$clave", clave ?? "");
            return cmd.ExecuteScalar() != null;
        }

        public static HashSet<string> LoadClavesExportadas()
        {
            var set = new HashSet<string>();
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT Clave FROM PreseaComprobantesExportados";
            using var r = cmd.ExecuteReader();
            while (r.Read())
                set.Add(r.GetString(0));
            return set;
        }

        public static void RegistrarComprobanteExportado(PreseaComprobanteExportado e)
        {
            using var cn = Open();
            RegistrarComprobanteExportado(cn, null, e);
        }

        public static void RegistrarComprobantesExportados(IEnumerable<PreseaComprobanteExportado> exportados)
        {
            using var cn = Open();
            using var tx = cn.BeginTransaction();
            foreach (var e in exportados)
                RegistrarComprobanteExportado(cn, tx, e);
            tx.Commit();
        }

        private static void RegistrarComprobanteExportado(SqlConnection cn, SqlTransaction tx, PreseaComprobanteExportado e)
        {
            using var cmd = cn.CreateCommand();
            cmd.Transaction = tx;
            // INSERT OR IGNORE: registrar dos veces la misma clave no es un error.
            cmd.CommandText = @"
                INSERT OR IGNORE INTO PreseaComprobantesExportados
                    (Clave, CuitEmisor, TipoCmp, PtoVta, Nro, CodAut, Importe,
                     FechaComprobante, FechaExportacion, ArchivoGenerado, PerfilOfflineId)
                VALUES
                    ($Clave, $CuitEmisor, $TipoCmp, $PtoVta, $Nro, $CodAut, $Importe,
                     $FechaComprobante, $FechaExportacion, $ArchivoGenerado, $PerfilOfflineId)";

            cmd.Parameters.AddWithValue("$Clave",            e.Clave ?? "");
            cmd.Parameters.AddWithValue("$CuitEmisor",       e.CuitEmisor ?? "");
            cmd.Parameters.AddWithValue("$TipoCmp",          e.TipoCmp ?? "");
            cmd.Parameters.AddWithValue("$PtoVta",           e.PtoVta ?? "");
            cmd.Parameters.AddWithValue("$Nro",              e.Nro ?? "");
            cmd.Parameters.AddWithValue("$CodAut",           e.CodAut ?? "");
            cmd.Parameters.AddWithValue("$Importe",          e.Importe.ToString(CultureInfo.InvariantCulture));
            cmd.Parameters.AddWithValue("$FechaComprobante", e.FechaComprobante ?? "");
            cmd.Parameters.AddWithValue("$FechaExportacion", e.FechaExportacion.ToString("o", CultureInfo.InvariantCulture));
            cmd.Parameters.AddWithValue("$ArchivoGenerado",  e.ArchivoGenerado ?? "");
            cmd.Parameters.AddWithValue("$PerfilOfflineId",  e.PerfilOfflineId ?? "");
            cmd.ExecuteNonQuery();
        }

        public static List<PreseaComprobanteExportado> LoadComprobantesExportados()
        {
            var list = new List<PreseaComprobanteExportado>();
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT * FROM PreseaComprobantesExportados ORDER BY FechaExportacion DESC";
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new PreseaComprobanteExportado
                {
                    Clave            = r.GetString(r.GetOrdinal("Clave")),
                    CuitEmisor       = r.GetString(r.GetOrdinal("CuitEmisor")),
                    TipoCmp          = r.GetString(r.GetOrdinal("TipoCmp")),
                    PtoVta           = r.GetString(r.GetOrdinal("PtoVta")),
                    Nro              = r.GetString(r.GetOrdinal("Nro")),
                    CodAut           = r.GetString(r.GetOrdinal("CodAut")),
                    Importe          = ParseDec(Str(r, "Importe")),
                    FechaComprobante = r.GetString(r.GetOrdinal("FechaComprobante")),
                    FechaExportacion = ParseFecha(Str(r, "FechaExportacion")),
                    ArchivoGenerado  = r.GetString(r.GetOrdinal("ArchivoGenerado")),
                    PerfilOfflineId  = r.GetString(r.GetOrdinal("PerfilOfflineId")),
                });
            }
            return list;
        }

        // ── Presea: mapeo de columnas de importacion (por entidad) ──────────────────

        public static MapeoColumnasArchivo LoadMapeoColumnas(string entidad)
        {
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT ConfigJson FROM PreseaMapeoColumnas WHERE Entidad = $e";
            cmd.Parameters.AddWithValue("$e", entidad ?? "");
            var json = cmd.ExecuteScalar() as string;
            return Deserialize<MapeoColumnasArchivo>(json);
        }

        public static void SaveMapeoColumnas(MapeoColumnasArchivo mapeo)
        {
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO PreseaMapeoColumnas (Entidad, ConfigJson)
                VALUES ($e, $j)
                ON CONFLICT(Entidad) DO UPDATE SET ConfigJson = excluded.ConfigJson";
            cmd.Parameters.AddWithValue("$e", mapeo.Entidad ?? "");
            cmd.Parameters.AddWithValue("$j", JsonSerializer.Serialize(mapeo, JsonOpts));
            cmd.ExecuteNonQuery();
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static SqlConnection Open()
        {
            var cn = SqlDb.GetConnection();
            cn.Open();
            return cn;
        }

        private static void Execute(SqlConnection cn, string sql, SqlTransaction? tx = null)
        {
            using var cmd = cn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        private static string? Str(SqlDataReader r, string col)
        {
            int ord = r.GetOrdinal(col);
            return r.IsDBNull(ord) ? null : r.GetString(ord);
        }

        private static string SoloDigitos(string? s) =>
            string.IsNullOrEmpty(s) ? string.Empty : new string(System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Where(s, char.IsDigit)));

        private static decimal ParseDec(string? s) =>
            decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0m;

        private static DateTime ParseFecha(string? s) =>
            DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt) ? dt : DateTime.MinValue;

        private static T? Deserialize<T>(string? json) where T : class
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try { return JsonSerializer.Deserialize<T>(json); }
            catch { return null; }
        }

        private static T? DeserializeOrNull<T>(string? json) where T : class
            => Deserialize<T>(json);
    }
}
```

Nota: este contenido todavía tiene el SQL de negocio en dialecto SQLite (`$Param`, `ON CONFLICT`, `INSERT OR IGNORE`, `LIMIT 1`, sin prefijo `arca.`) — **a propósito**. Tasks 2-4 lo portan por grupo de tablas. Lo que este paso deja funcionando es la plomería: tipos `SqlConnection`/`SqlTransaction`/`SqlDataReader`, `Open()` vía `SqlDb`, `InitializeDatabase()` ejecutando el DDL real. El build debe compilar limpio porque el compilador no valida el contenido de un string de SQL.

- [ ] **Step 4: Reescribir ArcaStorageConfig.cs**

Reemplazar el contenido completo de `ArcaCliente/Services/ArcaStorageConfig.cs`:

```csharp
namespace ArcaCliente.Services
{
    /// <summary>
    /// Gate de inicialización del módulo ArcaCliente. Autosuficiente: cualquier
    /// host que abra un form de ArcaCliente dispara la inicialización sola,
    /// sin coordinación externa (a diferencia del viejo esquema SQLite, donde
    /// el host tenía que asignar la ruta del archivo antes de abrir cualquier form).
    /// </summary>
    public static class ArcaStorageConfig
    {
        private static bool _inicializado;

        /// <summary>Crea el schema/tablas si no existen. Run-once por proceso.</summary>
        public static void Initialize()
        {
            if (_inicializado) return;
            ArcaSqlStorage.InitializeDatabase();
            _inicializado = true;
        }
    }
}
```

- [ ] **Step 5: Rename mecánico en los 6 wrappers**

En cada uno de estos archivos, reemplazar todas las ocurrencias de `ArcaSqliteStorage.` por `ArcaSqlStorage.` (nada más cambia — mismas firmas, mismos nombres de método):

- `ArcaCliente/Services/PerfilStorage.cs`
- `ArcaCliente/Services/PerfilOfflineStorage.cs`
- `ArcaCliente/Services/MapeoColumnasStorage.cs`
- `ArcaCliente/Services/PreseaExportMemoryStorage.cs`
- `ArcaCliente/Services/PreseaProveedorStorage.cs`
- `ArcaCliente/Services/TipoComprobanteStorage.cs` (además, actualizar el comentario de `InitializeDatabase()` en este archivo: `// Las tablas se crean en ArcaSqliteStorage.InitializeDatabase()` → `// Las tablas se crean en ArcaSqlStorage.InitializeDatabase()`)

- [ ] **Step 6: Autosuficiencia — auto-init en los 3 forms de entrada**

Agregar `ArcaStorageConfig.Initialize();` como primera línea del constructor, antes de `InitializeComponent();`, en:

`ArcaCliente/FormPerfilesOffline.cs` (constructor en línea 18):
```csharp
public FormPerfilesOffline()
{
    ArcaStorageConfig.Initialize();
    InitializeComponent();
    Icon = AppIcons.Arca;
    CargarPerfiles();
}
```

`ArcaCliente/FormComprobantesOffline.cs` (constructor en línea 26):
```csharp
public FormComprobantesOffline(PerfilOffline perfil)
{
    ArcaStorageConfig.Initialize();
    InitializeComponent();
    Icon = AppIcons.Arca;

    _perfil = perfil;
    // ... resto del constructor sin cambios
```

`ArcaCliente/FormEquivalencias.cs` (constructor en línea 22):
```csharp
public FormEquivalencias()
{
    ArcaStorageConfig.Initialize();
    InitializeComponent();
    Icon = AppIcons.Arca;

    CargarEquivalencias();
}
```

(`using ArcaCliente.Services;` ya está presente en los tres archivos — no hace falta agregar using nuevo.)

- [ ] **Step 7: Eliminar la coordinación host↔módulo en el shell**

En `ConciliadorContable/Forms/FormMenuPrincipal.cs`, eliminar las líneas 18-19:

```csharp
ArcaStorageConfig.DbPath = DatabaseHelper.DbPath;
ArcaStorageConfig.Initialize();
```

(Si el archivo tiene `using ArcaCliente.Services;` solo para esas dos líneas y no se usa en ningún otro lado del archivo, quitar también el using — verificar con grep antes de borrar.)

- [ ] **Step 8: Compilar**

```bash
dotnet build ConciliadorContable.slnx --no-restore
```

Expected: build OK, sin errores. El módulo compila pero **no funciona correctamente contra el servidor real todavía** (dialecto SQLite en el SQL de negocio) — eso es esperado, se resuelve en Tasks 2-4.

- [ ] **Step 9: Commit**

```bash
git add ArcaCliente/Services/ArcaSqlSchema.cs ArcaCliente/Services/ArcaSqlStorage.cs \
        ArcaCliente/Services/ArcaStorageConfig.cs ArcaCliente/Services/PerfilStorage.cs \
        ArcaCliente/Services/PerfilOfflineStorage.cs ArcaCliente/Services/MapeoColumnasStorage.cs \
        ArcaCliente/Services/PreseaExportMemoryStorage.cs ArcaCliente/Services/PreseaProveedorStorage.cs \
        ArcaCliente/Services/TipoComprobanteStorage.cs ArcaCliente/ArcaCliente.csproj \
        ArcaCliente/FormPerfilesOffline.cs ArcaCliente/FormComprobantesOffline.cs ArcaCliente/FormEquivalencias.cs \
        ConciliadorContable/Forms/FormMenuPrincipal.cs
git rm ArcaCliente/Services/ArcaSqliteStorage.cs
git commit -m "feat(arca): schema arca, rename ArcaSqliteStorage->ArcaSqlStorage, autosuficiencia del modulo"
```

---

### Task 2: Portar Perfiles Offline + Perfiles Fiscales

**Files:**
- Modify: `ArcaCliente/Services/ArcaSqlStorage.cs` (métodos `LoadPerfilesOffline`, `SavePerfilesOffline`, `LoadPerfilesFiscales`, `SavePerfilesFiscales`)

**Interfaces:**
- Consumes: DDL de Task 1 (tablas `arca.ArcaPerfilesOffline`, `arca.ArcaPerfilesFiscales` ya existen).
- Produces: sin cambio de firmas — mismos métodos públicos que ya usan `PerfilOfflineStorage.cs` y `PerfilStorage.cs`.

Estas 4 tablas no tienen upserts ni tipos especiales — es prefijo `arca.` + `$` → `@` en texto SQL y en `AddWithValue`.

- [ ] **Step 1: LoadPerfilesOffline / SavePerfilesOffline**

En `ArcaCliente/Services/ArcaSqlStorage.cs`, reemplazar:
```csharp
cmd.CommandText = "SELECT * FROM ArcaPerfilesOffline ORDER BY Nombre";
```
por:
```csharp
cmd.CommandText = "SELECT * FROM arca.ArcaPerfilesOffline ORDER BY Nombre";
```

Reemplazar:
```csharp
Execute(cn, "DELETE FROM ArcaPerfilesOffline", tx);
```
por:
```csharp
Execute(cn, "DELETE FROM arca.ArcaPerfilesOffline", tx);
```

Reemplazar el bloque `INSERT INTO ArcaPerfilesOffline` completo (texto SQL y los 27 `AddWithValue`) cambiando: `INSERT INTO ArcaPerfilesOffline` → `INSERT INTO arca.ArcaPerfilesOffline`, y cada placeholder `$Xxx` en el `VALUES (...)` y en cada `cmd.Parameters.AddWithValue("$Xxx", ...)` por `@Xxx` (28 nombres: `Id, Nombre, TipoArchivo, Separador, Encoding, HojaExcel, TieneCabecera, ColFecha, ColPuntoVenta, ColNumero, ColTipoComprobante, ColCuit, ColNombreProveedor, ColTotal, PosFecha, PosPuntoVenta, PosNumero, PosTipoComprobante, PosCuit, PosNombreProveedor, PosTotal, FormatoFecha, SeparadorDecimal, CarpetaCsvArca, SistemaExportacion, ConfigPreseaJson, DirectivasJson`).

- [ ] **Step 2: LoadPerfilesFiscales / SavePerfilesFiscales**

Mismo tratamiento: `FROM ArcaPerfilesFiscales` → `FROM arca.ArcaPerfilesFiscales`, `DELETE FROM ArcaPerfilesFiscales` → `DELETE FROM arca.ArcaPerfilesFiscales`, `INSERT INTO ArcaPerfilesFiscales` → `INSERT INTO arca.ArcaPerfilesFiscales`, y los 12 placeholders `$Xxx` → `@Xxx` (`Id, Nombre, Username, Password, Cuit, IntegracionHabilitada, Sistema, ConciliacionConnectionString, ConciliacionQuery, OctosisConnectionString, ArcaApiUrl, DirectivasJson`).

- [ ] **Step 3: Compilar**

```bash
dotnet build ConciliadorContable.slnx --no-restore
```
Expected: build OK.

- [ ] **Step 4: Probar contra el servidor**

Ejecutar el shell, abrir "Perfiles Offline" y "Perfiles Fiscales" (los forms que consumen estos métodos), crear un perfil de cada tipo y guardarlo. Verificar en SSMS:
```sql
SELECT COUNT(*) FROM arca.ArcaPerfilesOffline;
SELECT COUNT(*) FROM arca.ArcaPerfilesFiscales;
```

- [ ] **Step 5: Commit**

```bash
git add ArcaCliente/Services/ArcaSqlStorage.cs
git commit -m "feat(arca): portar Perfiles Offline y Perfiles Fiscales a T-SQL"
```

---

### Task 3: Portar Equivalencias + Presea Proveedores (upsert MERGE)

**Files:**
- Modify: `ArcaCliente/Services/ArcaSqlStorage.cs` (métodos `LoadEquivalencias`, `SaveEquivalencias`, `LoadPreseaProveedores`, `GetPreseaProveedor`, `UpsertPreseaProveedor` privado, `ReadPreseaProveedor`)

**Interfaces:**
- Consumes: DDL de Task 1 (`arca.ArcaEquivalencias`, `arca.PreseaProveedores`).
- Produces: sin cambio de firmas.

- [ ] **Step 1: Equivalencias**

Reemplazar:
```csharp
cmd.CommandText = "SELECT CodigoAfip, TipoSistema, Letra FROM ArcaEquivalencias";
```
por:
```csharp
cmd.CommandText = "SELECT CodigoAfip, TipoSistema, Letra FROM arca.ArcaEquivalencias";
```

Reemplazar:
```csharp
Execute(cn, "DELETE FROM ArcaEquivalencias", tx);
```
por:
```csharp
Execute(cn, "DELETE FROM arca.ArcaEquivalencias", tx);
```

Reemplazar:
```csharp
cmd.CommandText = "INSERT INTO ArcaEquivalencias (CodigoAfip, TipoSistema, Letra) VALUES ($c, $t, $l)";
cmd.Parameters.AddWithValue("$c", i.CodigoAfip ?? "");
cmd.Parameters.AddWithValue("$t", i.TipoSistema ?? "");
cmd.Parameters.AddWithValue("$l", i.Letra ?? "");
```
por:
```csharp
cmd.CommandText = "INSERT INTO arca.ArcaEquivalencias (CodigoAfip, TipoSistema, Letra) VALUES (@c, @t, @l)";
cmd.Parameters.AddWithValue("@c", i.CodigoAfip ?? "");
cmd.Parameters.AddWithValue("@t", i.TipoSistema ?? "");
cmd.Parameters.AddWithValue("@l", i.Letra ?? "");
```

- [ ] **Step 2: LoadPreseaProveedores / GetPreseaProveedor**

Reemplazar:
```csharp
cmd.CommandText = "SELECT * FROM PreseaProveedores ORDER BY Nombre";
```
por:
```csharp
cmd.CommandText = "SELECT * FROM arca.PreseaProveedores ORDER BY Nombre";
```

Reemplazar:
```csharp
cmd.CommandText = "SELECT * FROM PreseaProveedores WHERE Cuit = $cuit";
cmd.Parameters.AddWithValue("$cuit", key);
```
por:
```csharp
cmd.CommandText = "SELECT * FROM arca.PreseaProveedores WHERE Cuit = @cuit";
cmd.Parameters.AddWithValue("@cuit", key);
```

- [ ] **Step 3: UpsertPreseaProveedor — ON CONFLICT a MERGE + Descuento decimal**

Reemplazar el método privado completo:
```csharp
private static void UpsertPreseaProveedor(SqlConnection cn, SqlTransaction tx, ConfigPreseaProveedor p)
{
    using var cmd = cn.CreateCommand();
    cmd.Transaction = tx;
    cmd.CommandText = @"
        MERGE arca.PreseaProveedores AS t
        USING (SELECT @Cuit AS Cuit) AS s ON t.Cuit = s.Cuit
        WHEN MATCHED THEN UPDATE SET
            Nombre = @Nombre, CodigoProveedor = @CodigoProveedor,
            CuentaContableProveedor = @CuentaContableProveedor, CuentaDebe = @CuentaDebe,
            Centro = @Centro, Provincia = @Provincia, Condicion = @Condicion,
            Descuento = @Descuento, Fiscal = @Fiscal
        WHEN NOT MATCHED THEN INSERT
            (Cuit, Nombre, CodigoProveedor, CuentaContableProveedor, CuentaDebe, Centro, Provincia, Condicion, Descuento, Fiscal)
            VALUES (@Cuit, @Nombre, @CodigoProveedor, @CuentaContableProveedor, @CuentaDebe, @Centro, @Provincia, @Condicion, @Descuento, @Fiscal);";

    cmd.Parameters.AddWithValue("@Cuit",                    SoloDigitos(p.Cuit));
    cmd.Parameters.AddWithValue("@Nombre",                  p.Nombre ?? "");
    cmd.Parameters.AddWithValue("@CodigoProveedor",         p.CodigoProveedor ?? "");
    cmd.Parameters.AddWithValue("@CuentaContableProveedor", p.CuentaContableProveedor ?? "");
    cmd.Parameters.AddWithValue("@CuentaDebe",              p.CuentaDebe ?? "");
    cmd.Parameters.AddWithValue("@Centro",                  p.Centro ?? "");
    cmd.Parameters.AddWithValue("@Provincia",               p.Provincia ?? "");
    cmd.Parameters.AddWithValue("@Condicion",               p.Condicion ?? "");
    cmd.Parameters.AddWithValue("@Descuento",               p.Descuento);
    cmd.Parameters.AddWithValue("@Fiscal",                  p.Fiscal ?? "");
    cmd.ExecuteNonQuery();
}
```

Nota: `@Descuento` ahora recibe `p.Descuento` (el `decimal` directo, sin `.ToString(CultureInfo.InvariantCulture)`) — la columna ya es `DECIMAL(9,2)`, no hace falta el roundtrip de texto que exigía SQLite.

- [ ] **Step 4: ReadPreseaProveedor — leer Descuento como decimal**

Reemplazar:
```csharp
Descuento               = ParseDec(Str(r, "Descuento")),
```
por:
```csharp
Descuento               = r.GetDecimal(r.GetOrdinal("Descuento")),
```

- [ ] **Step 5: Compilar**

```bash
dotnet build ConciliadorContable.slnx --no-restore
```
Expected: build OK.

- [ ] **Step 6: Probar contra el servidor**

Abrir el form de Equivalencias, guardar una lista. Si hay un form de administración de proveedores PRESEA, cargar/editar un proveedor dos veces (para ejercitar el camino INSERT y el camino UPDATE del `MERGE`). Verificar:
```sql
SELECT COUNT(*) FROM arca.ArcaEquivalencias;
SELECT Cuit, Descuento FROM arca.PreseaProveedores;
```

- [ ] **Step 7: Commit**

```bash
git add ArcaCliente/Services/ArcaSqlStorage.cs
git commit -m "feat(arca): portar Equivalencias y Presea Proveedores (upsert MERGE) a T-SQL"
```

---

### Task 4: Portar Presea Comprobantes Exportados + Presea Mapeo Columnas

**Files:**
- Modify: `ArcaCliente/Services/ArcaSqlStorage.cs` (métodos `ExisteComprobanteExportado`, `LoadClavesExportadas`, `RegistrarComprobanteExportado` privado, `LoadComprobantesExportados`, `LoadMapeoColumnas`, `SaveMapeoColumnas`)

**Interfaces:**
- Consumes: DDL de Task 1 (`arca.PreseaComprobantesExportados`, `arca.PreseaMapeoColumnas`).
- Produces: sin cambio de firmas. Última task de porteo — al terminar, el módulo funciona end-to-end contra SQL Server.

- [ ] **Step 1: ExisteComprobanteExportado — quitar LIMIT 1, no hace falta (Clave es PK)**

Reemplazar:
```csharp
cmd.CommandText = "SELECT 1 FROM PreseaComprobantesExportados WHERE Clave = $clave LIMIT 1";
cmd.Parameters.AddWithValue("$clave", clave ?? "");
```
por:
```csharp
cmd.CommandText = "SELECT 1 FROM arca.PreseaComprobantesExportados WHERE Clave = @clave";
cmd.Parameters.AddWithValue("@clave", clave ?? "");
```

- [ ] **Step 2: LoadClavesExportadas**

Reemplazar:
```csharp
cmd.CommandText = "SELECT Clave FROM PreseaComprobantesExportados";
```
por:
```csharp
cmd.CommandText = "SELECT Clave FROM arca.PreseaComprobantesExportados";
```

- [ ] **Step 3: RegistrarComprobanteExportado — INSERT OR IGNORE a IF NOT EXISTS + tipos reales**

Reemplazar el método privado completo:
```csharp
private static void RegistrarComprobanteExportado(SqlConnection cn, SqlTransaction tx, PreseaComprobanteExportado e)
{
    using var cmd = cn.CreateCommand();
    cmd.Transaction = tx;
    // Ignorar duplicados: registrar dos veces la misma clave no es un error.
    cmd.CommandText = @"
        IF NOT EXISTS (SELECT 1 FROM arca.PreseaComprobantesExportados WHERE Clave = @Clave)
            INSERT INTO arca.PreseaComprobantesExportados
                (Clave, CuitEmisor, TipoCmp, PtoVta, Nro, CodAut, Importe,
                 FechaComprobante, FechaExportacion, ArchivoGenerado, PerfilOfflineId)
            VALUES
                (@Clave, @CuitEmisor, @TipoCmp, @PtoVta, @Nro, @CodAut, @Importe,
                 @FechaComprobante, @FechaExportacion, @ArchivoGenerado, @PerfilOfflineId)";

    cmd.Parameters.AddWithValue("@Clave",            e.Clave ?? "");
    cmd.Parameters.AddWithValue("@CuitEmisor",       e.CuitEmisor ?? "");
    cmd.Parameters.AddWithValue("@TipoCmp",          e.TipoCmp ?? "");
    cmd.Parameters.AddWithValue("@PtoVta",           e.PtoVta ?? "");
    cmd.Parameters.AddWithValue("@Nro",              e.Nro ?? "");
    cmd.Parameters.AddWithValue("@CodAut",           e.CodAut ?? "");
    cmd.Parameters.AddWithValue("@Importe",          e.Importe);
    cmd.Parameters.AddWithValue("@FechaComprobante", e.FechaComprobante ?? "");
    cmd.Parameters.AddWithValue("@FechaExportacion", e.FechaExportacion);
    cmd.Parameters.AddWithValue("@ArchivoGenerado",  e.ArchivoGenerado ?? "");
    cmd.Parameters.AddWithValue("@PerfilOfflineId",  e.PerfilOfflineId ?? "");
    cmd.ExecuteNonQuery();
}
```

Nota: `@Importe` recibe `e.Importe` (decimal directo) y `@FechaExportacion` recibe `e.FechaExportacion` (DateTime directo) — sin el roundtrip `ToString(Invariant)`/`ToString("o")` que exigía guardar como TEXT en SQLite. Las columnas ya son `DECIMAL(18,2)` y `DATETIME2(3)`.

- [ ] **Step 4: LoadComprobantesExportados — leer Importe y FechaExportacion como tipos reales**

Reemplazar:
```csharp
cmd.CommandText = "SELECT * FROM PreseaComprobantesExportados ORDER BY FechaExportacion DESC";
```
por:
```csharp
cmd.CommandText = "SELECT * FROM arca.PreseaComprobantesExportados ORDER BY FechaExportacion DESC";
```

Reemplazar:
```csharp
Importe          = ParseDec(Str(r, "Importe")),
FechaComprobante = r.GetString(r.GetOrdinal("FechaComprobante")),
FechaExportacion = ParseFecha(Str(r, "FechaExportacion")),
```
por:
```csharp
Importe          = r.GetDecimal(r.GetOrdinal("Importe")),
FechaComprobante = r.GetString(r.GetOrdinal("FechaComprobante")),
FechaExportacion = r.GetDateTime(r.GetOrdinal("FechaExportacion")),
```

- [ ] **Step 5: LoadMapeoColumnas / SaveMapeoColumnas — ON CONFLICT a MERGE**

Reemplazar:
```csharp
cmd.CommandText = "SELECT ConfigJson FROM PreseaMapeoColumnas WHERE Entidad = $e";
cmd.Parameters.AddWithValue("$e", entidad ?? "");
```
por:
```csharp
cmd.CommandText = "SELECT ConfigJson FROM arca.PreseaMapeoColumnas WHERE Entidad = @e";
cmd.Parameters.AddWithValue("@e", entidad ?? "");
```

Reemplazar el método `SaveMapeoColumnas` completo:
```csharp
public static void SaveMapeoColumnas(MapeoColumnasArchivo mapeo)
{
    using var cn = Open();
    using var cmd = cn.CreateCommand();
    cmd.CommandText = @"
        MERGE arca.PreseaMapeoColumnas AS t
        USING (SELECT @e AS Entidad) AS s ON t.Entidad = s.Entidad
        WHEN MATCHED THEN UPDATE SET ConfigJson = @j
        WHEN NOT MATCHED THEN INSERT (Entidad, ConfigJson) VALUES (@e, @j);";
    cmd.Parameters.AddWithValue("@e", mapeo.Entidad ?? "");
    cmd.Parameters.AddWithValue("@j", JsonSerializer.Serialize(mapeo, JsonOpts));
    cmd.ExecuteNonQuery();
}
```

- [ ] **Step 6: Limpieza de helpers que quedan sin uso**

Tras este step, `ParseDec` y `ParseFecha` ya no se usan en ningún lado del archivo (los dos call sites que quedaban eran `ReadPreseaProveedor.Descuento` — portado en Task 3 — y `LoadComprobantesExportados` — portado en este step). Verificar con grep:
```bash
grep -n "ParseDec\|ParseFecha" ArcaCliente/Services/ArcaSqlStorage.cs
```
Si efectivamente no quedan usos, eliminar ambos métodos privados del archivo.

- [ ] **Step 7: Compilar**

```bash
dotnet build ConciliadorContable.slnx --no-restore
```
Expected: build OK.

- [ ] **Step 8: Regresión completa contra el servidor**

En este punto el módulo funciona end-to-end. Recorrer el ciclo completo desde el shell:
1. Crear/editar un perfil offline y un perfil fiscal → verificar en `arca.ArcaPerfilesOffline` / `arca.ArcaPerfilesFiscales`.
2. Guardar equivalencias.
3. Si hay flujo de exportación PRESEA disponible para probar: exportar un comprobante, volver a intentarlo (debe detectarse como ya exportado — ejercita `ExisteComprobanteExportado` + el `IF NOT EXISTS`).
4. Reabrir el shell: `ArcaStorageConfig.Initialize()` no debe fallar ni recrear nada (schema ya existe).

- [ ] **Step 9: Commit**

```bash
git add ArcaCliente/Services/ArcaSqlStorage.cs
git commit -m "feat(arca): portar Presea Comprobantes Exportados y Mapeo Columnas a T-SQL"
```

---

### Task 5: Migrador de datos conciliador.db (tablas ArcaCliente) → SQL Server

**Files:**
- Create: `Tools/MigradorArcaCliente/MigradorArcaCliente.csproj`
- Create: `Tools/MigradorArcaCliente/Program.cs`
- Modify: `ConciliadorContable.slnx`

**Interfaces:**
- Consumes: schema `arca` (Task 1), `conciliador.db` existente del puesto (contiene tanto `Usuarios` del shell como las 6 tablas de ArcaCliente — el migrador solo toca estas últimas).
- Produces: console app `MigradorArcaCliente.exe <ruta conciliador.db> <connection string>`.

- [ ] **Step 1: Crear el csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Data.Sqlite" Version="8.0.0" />
    <PackageReference Include="Microsoft.Data.SqlClient" Version="6.1.4" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Implementar Program.cs**

```csharp
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

// Uso: MigradorArcaCliente <ruta conciliador.db> <connection string SQL Server>
if (args.Length != 2)
{
    Console.WriteLine("Uso: MigradorArcaCliente <ruta conciliador.db> <connection string>");
    return 1;
}

using var src = new SqliteConnection($"Data Source={args[0]};Mode=ReadOnly");
src.Open();
using var dst = new SqlConnection(args[1]);
dst.Open();

// 0) Abortamos si el destino ya tiene datos (evita duplicar en re-ejecuciones).
//    No usamos ArcaPerfilesOffline como referencia unica: cada tabla se
//    verifica antes de copiarla, para permitir migrar en mas de una corrida
//    si alguna tabla vino vacia en el origen.
string[] tablas =
{
    "ArcaPerfilesOffline", "ArcaPerfilesFiscales", "ArcaEquivalencias",
    "PreseaProveedores", "PreseaComprobantesExportados", "PreseaMapeoColumnas",
};

foreach (var tabla in tablas)
{
    using var check = dst.CreateCommand();
    check.CommandText = $"SELECT COUNT(*) FROM arca.{tabla}";
    if ((int)check.ExecuteScalar()! > 0)
    {
        Console.WriteLine($"ERROR: arca.{tabla} ya tiene datos. Vaciar el schema arca antes de migrar.");
        return 2;
    }
}

// 1) Copia directa: no hay IDENTITY (las PK son GUID/CUIT/Entidad como texto),
//    asi que no hace falta el reseed que si necesito el migrador de bancos.
foreach (var tabla in tablas)
{
    using var cmd = src.CreateCommand();
    cmd.CommandText = $"SELECT * FROM {tabla}";
    using var reader = cmd.ExecuteReader();
    var dt = new DataTable();
    dt.Load(reader);

    if (dt.Rows.Count == 0)
    {
        Console.WriteLine($"arca.{tabla}: 0 filas (origen vacio, se omite)");
        continue;
    }

    using var bulk = new SqlBulkCopy(dst)
    {
        DestinationTableName = $"arca.{tabla}",
        BatchSize = 1000,
    };
    foreach (DataColumn c in dt.Columns)
        bulk.ColumnMappings.Add(c.ColumnName, c.ColumnName);
    bulk.WriteToServer(dt);
    Console.WriteLine($"arca.{tabla}: {dt.Rows.Count} filas");
}

Console.WriteLine("Migracion OK.");
return 0;
```

Nota: a diferencia de `MigradorDataBanks` (Etapa 1), acá no se usa `SqlBulkCopyOptions.KeepIdentity` porque ninguna columna es `IDENTITY` — todas las PK son texto (GUID, CUIT, Entidad, Clave) que la aplicación genera, así que `SqlBulkCopy` las copia tal cual sin configuración especial.

- [ ] **Step 3: Agregar a la solución y compilar**

```bash
dotnet sln ConciliadorContable.slnx add Tools/MigradorArcaCliente/MigradorArcaCliente.csproj
dotnet build ConciliadorContable.slnx --no-restore
```

- [ ] **Step 4: Ejecutar contra el conciliador.db real**

```bash
Tools/MigradorArcaCliente/bin/Debug/net8.0/MigradorArcaCliente.exe "<ruta al conciliador.db del puesto>" "Server=192.168.7.51;Database=Conciliador;User Id=conciliador;Password=...;TrustServerCertificate=True;"
```

Expected: una línea por tabla con conteo (o "0 filas, se omite") y `Migracion OK.`

- [ ] **Step 5: Verificar conteos origen vs destino**

Comparar `SELECT COUNT(*)` de cada una de las 6 tablas entre el `conciliador.db` original (con DB Browser for SQLite o la CLI) y SSMS. Deben coincidir. Abrir la app y confirmar que los perfiles/equivalencias/proveedores cargados antes de migrar siguen apareciendo igual.

- [ ] **Step 6: Commit**

```bash
git add Tools/MigradorArcaCliente ConciliadorContable.slnx
git commit -m "feat(arca): migrador one-shot de conciliador.db (tablas ArcaCliente) a SQL Server"
```

---

### Task 6: Limpieza final

**Files:**
- Modify: `ArcaCliente/ArcaCliente.csproj` (quitar `PackageReference` de `Microsoft.Data.Sqlite`)

**Interfaces:**
- Consumes: todo lo anterior.

- [ ] **Step 1: Confirmar que Microsoft.Data.Sqlite ya no se usa en ArcaCliente**

```bash
grep -rn "Sqlite" ArcaCliente --include="*.cs"
```
Expected: 0 resultados (el archivo que lo usaba, `ArcaSqliteStorage.cs`, ya no existe desde Task 1).

- [ ] **Step 2: Quitar el PackageReference**

En `ArcaCliente/ArcaCliente.csproj`, eliminar la línea:
```xml
<PackageReference Include="Microsoft.Data.Sqlite" Version="10.0.5" />
```

- [ ] **Step 3: Build completo**

```bash
dotnet build ConciliadorContable.slnx --no-restore
```
Expected: build OK.

- [ ] **Step 4: Verificar que conciliador.db ya no gana tablas de ArcaCliente**

Con la base ya migrada, borrar (o renombrar) `conciliador.db` del directorio de trabajo y abrir el shell + los 3 forms de ArcaCliente: no debe fallar (el login `Usuarios` recreará `conciliador.db` con solo esa tabla; los datos de ArcaCliente vienen de SQL Server).

- [ ] **Step 5: Commit**

```bash
git add ArcaCliente/ArcaCliente.csproj
git commit -m "chore(arca): retirar Microsoft.Data.Sqlite de ArcaCliente"
```

---

## Fuera de alcance (spec)

- Schema `seg` (Usuarios/login del shell) — plan aparte.
- Cifrado de `Password`/connection strings en `ArcaPerfilesFiscales` — decisión de seguridad con diseño propio.
- Reconciliación manual de perfiles si existen múltiples instalaciones con configuraciones locales distintas — decisión humana antes de correr el migrador, no algo que el migrador resuelva.
