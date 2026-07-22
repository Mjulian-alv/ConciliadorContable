# Migración a SQL Server — Etapa 0 (DLL) + Etapa 1 (schema bancos)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Convertir ArcaCliente y AgrupadorConceptos de WinExe a Library, y migrar la persistencia de AgrupadorConceptos de SQLite (`DataBanks.db`) a una base SQL Server común `Conciliador` con schema `bancos`, como primera etapa del ecosistema centralizado.

**Architecture:** Una base `Conciliador` en la misma instancia que Octosis (192.168.7.51), con un schema por módulo (`bancos` ahora; `arca` y `seg` en etapas futuras). Un proyecto nuevo `Conciliador.Comun` (classlib) centraliza la connection string y la fábrica de conexiones; cada módulo conserva su `DatabaseHelper` pero devuelve `SqlConnection`. Dapper se mantiene (es agnóstico del proveedor). Migración de datos one-shot con una console app usando `SqlBulkCopy` + `KeepIdentity`.

**Tech Stack:** .NET 8 WinForms, Dapper, Microsoft.Data.SqlClient 6.1.4 (misma versión que ya usa ArcaCliente), Microsoft.Data.Sqlite (solo queda en el migrador y en LiquidacionesAuditar/shell hasta etapas futuras).

## Contexto

- **Disparador:** piden incorporar justificaciones, historiales y estadísticas de los resultados de conciliaciones (ARCA y bancos). Eso requiere datos centralizados y consultables — SQLite local por puesto no alcanza.
- **Decisiones tomadas con el usuario:**
  - Nueva base `Conciliador` en la misma instancia SQL Server que Octosis (192.168.7.51).
  - Migración por módulo: primero AgrupadorConceptos (bancos), después ArcaCliente + shell (etapa 2, plan aparte). Las features nuevas nacen directo en SQL (etapa 3, plan aparte).
  - LiquidacionesAuditar queda en SQLite, fuera de alcance.
  - ArcaCliente y AgrupadorConceptos dejan de ser EXE; LiquidacionesAuditar sigue standalone.
- **Principio de arquitectura:** ConciliadorContable es un host delgado (login/menú); **ArcaCliente y AgrupadorConceptos son los proyectos centrales**. Los módulos deben ser autosuficientes: cualquier host que referencie la DLL debe poder abrir sus forms sin pasos de inicialización especiales. Lo único que queda en el shell es lo inherente al proceso (el `App.config` del exe, de donde .NET lee la configuración en runtime).
- **Rol de cada pieza como concentrador:** el shell concentra seguridad (login/`Usuarios`, futuro schema `seg`) y la configuración del despliegue (una sola connection string en su `App.config` para todos los módulos). `Conciliador.Comun` concentra lo compartido entre módulos (`SqlDb` hoy; helpers y modelos comunes a medida que aparezcan) — va ahí y no en el shell porque los módulos no pueden referenciar al shell (referencia circular). Objetivo final: una base, una conexión, un schema por dominio (`seg`, `bancos`, `arca`, futuro `auditoria`), lo que permite estadísticas cruzadas entre módulos con un JOIN. Las connection strings de Octosis por perfil fiscal son aparte (apuntan al ERP externo) y siguen siendo configuración por perfil.
- **Hallazgos que condicionan el diseño:**
  - El shell ya instancia los forms de ambos módulos directamente (`ConciliadorContable/Forms/FormMenuPrincipal.cs`) — el pasaje a DLL es solo `OutputType` + mover inicializaciones del `Program.cs` de cada módulo al shell.
  - `Microsoft.Data.Sqlite` NO aplica `PRAGMA foreign_keys=ON` por defecto → las cascadas de SQLite probablemente nunca se ejecutaron → **puede haber filas huérfanas en `DataBanks.db`** que violarían las FK reales de SQL Server. El migrador debe limpiarlas antes de copiar.
  - SQL Server rechaza `ON DELETE CASCADE` con caminos múltiples (`ConciliacionPares` → Sesión directo y vía ItemsExternos). Se resuelve con borrado explícito ordenado en `EliminarSesion`.
  - Los modelos ya usan `decimal` para importes; en SQL Server las columnas `REAL` pasan a `DECIMAL(18,2)` (corrige de raíz la representación de plata).
  - Las columnas `Fecha` de movimientos/items son `string` en los modelos → quedan `NVARCHAR(30)` para no cambiar lógica de la app en esta etapa.

## Global Constraints

- `dotnet restore` está roto en este repo (nuget.config con mapping incompleto y Telerik apuntando a Bin90). **Compilar siempre desde Visual Studio** o con `dotnet build --no-restore` si los paquetes ya están restaurados. Los paquetes nuevos se agregan editando el `.csproj` a mano y restaurando desde VS.
- No hay proyecto de tests en la solución; la verificación es build + ejecución manual + queries de control en SQL Server. No introducir infraestructura de tests en este plan.
- Mensajes de commit en español, prefijos `feat:`/`chore:`/`refactor:` como en el historial.
- Antes de empezar: commitear los cambios pendientes del working tree (fix de performance de importación) para partir de un árbol limpio.
- Versión de `Microsoft.Data.SqlClient`: **6.1.4** en todos los proyectos (igual que ArcaCliente).
- Los nombres de tablas y columnas NO cambian — solo se agrega el prefijo de schema `bancos.`.

---

## Etapa 0 — WinExe → Library

### Task 1: Commit del trabajo pendiente y conversión de ArcaCliente a Library

**Files:**
- Modify: `ArcaCliente/ArcaCliente.csproj`
- Delete: `ArcaCliente/Program.cs`

**Interfaces:**
- Produces: `ArcaCliente.dll` como classlib WinForms; el shell sigue usando `ArcaCliente.FormPerfilesOffline`, `FormComprobantesOffline`, `FormEquivalencias` sin cambios.

- [ ] **Step 1: Commitear los cambios pendientes del árbol de trabajo**

```bash
git add -A
git commit -m "perf(agrupador): transaccion unica en importacion, WAL e indices FK en SQLite"
```

- [ ] **Step 2: Cambiar OutputType en ArcaCliente.csproj**

En `ArcaCliente/ArcaCliente.csproj` reemplazar:

```xml
<OutputType>WinExe</OutputType>
```

por:

```xml
<OutputType>Library</OutputType>
```

Si el csproj tiene `<ApplicationIcon>` o `<StartupObject>`, eliminarlos.

- [ ] **Step 3: Eliminar ArcaCliente/Program.cs**

El único contenido no trivial es `EnsureIcoFile()` (genera `arca.ico` junto al exe) — deja de tener sentido sin exe propio. Los forms que usan `AppIcons.Arca` como ícono de ventana no dependen de ese archivo.

```bash
git rm ArcaCliente/Program.cs
```

- [ ] **Step 4: Compilar la solución (desde VS o msbuild) y verificar**

Expected: build OK; en `ConciliadorContable/bin/Debug/...` aparece `ArcaCliente.dll` y ya no se genera `ArcaCliente.exe`.

- [ ] **Step 5: Smoke test del shell**

Ejecutar ConciliadorContable, abrir desde el menú: Comprobantes Offline, Perfiles, Equivalencias (los tres entry points de ArcaCliente en `FormMenuPrincipal.cs:42-52`). Deben abrir igual que antes.

- [ ] **Step 6: Commit**

```bash
git add ArcaCliente/ArcaCliente.csproj
git commit -m "refactor(arca): ArcaCliente pasa de WinExe a Library"
```

### Task 2: Conversión de AgrupadorConceptos a Library

**Files:**
- Modify: `AgrupadorConceptos/AgrupadorConceptos.csproj`
- Delete: `AgrupadorConceptos/Program.cs`
- Create: `AgrupadorConceptos/ModuleInit.cs` (autosuficiencia del módulo)

**Interfaces:**
- Consumes: nada de Task 1.
- Produces: `AgrupadorConceptos.dll` autosuficiente; el shell sigue usando `ProcesadorForm`, `GestionHomologacionesForm`, `ConciliacionExternForm` y `Data.DatabaseHelper.InitializeDatabase()` sin cambios y sin necesitar setup propio.

- [ ] **Step 1: Cambiar OutputType**

En `AgrupadorConceptos/AgrupadorConceptos.csproj`: `<OutputType>WinExe</OutputType>` → `<OutputType>Library</OutputType>`. Eliminar `<ApplicationIcon>`/`<StartupObject>` si existen.

- [ ] **Step 2: Registro de encoding DENTRO del módulo (no en el host)**

`AgrupadorConceptos/Program.cs:19` registra `CodePagesEncodingProvider`, **requerido por ExcelDataReader** para leer los extractos. Como el módulo es lo central y el host no debe conocer sus detalles internos, el registro va dentro de la propia DLL con un `[ModuleInitializer]` (corre automáticamente al cargar el assembly, sin que ningún host tenga que acordarse):

Crear `AgrupadorConceptos/ModuleInit.cs`:

```csharp
using System.Runtime.CompilerServices;

namespace AgrupadorConceptos
{
    internal static class ModuleInit
    {
        /// <summary>
        /// Corre al cargar el assembly. Requerido por ExcelDataReader en .NET 5+
        /// para leer archivos con encodings ANSI. Vive acá y no en el host:
        /// el módulo debe funcionar con cualquier exe que lo referencie.
        /// </summary>
        [ModuleInitializer]
        internal static void Init()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }
    }
}
```

- [ ] **Step 3: Eliminar AgrupadorConceptos/Program.cs**

`InitializeDatabase()` ya lo llama el shell antes de abrir cada form (`FormMenuPrincipal.cs:59,65,72`). `IconGenerator.GenerateIconFile()` generaba el .ico del exe propio — se descarta (no borrar la clase `IconGenerator` si otros forms la usan; verificar con grep antes).

```bash
git rm AgrupadorConceptos/Program.cs
```

- [ ] **Step 4: Compilar y smoke test**

Build de la solución; ejecutar el shell y abrir Procesador de Extractos, Gestión de Homologaciones y Conciliación Externa. Importar un archivo de extracto de prueba para validar que ExcelDataReader sigue funcionando (valida el Step 2).

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "refactor(agrupador): AgrupadorConceptos pasa de WinExe a Library; encoding via ModuleInitializer"
```

---

## Etapa 1 — Base común SQL Server + migración del schema bancos

### Task 3: Aprovisionar base y login en el servidor

**Files:**
- Create: `docs/sql/00-provision-conciliador.sql` (se ejecuta a mano en SSMS, una sola vez, con un usuario admin del servidor)

**Interfaces:**
- Produces: base `Conciliador` y login `conciliador` que usan todas las tareas siguientes.

- [ ] **Step 1: Crear el script de aprovisionamiento**

```sql
-- Ejecutar en 192.168.7.51 con permisos de sysadmin. Una sola vez.
IF DB_ID(N'Conciliador') IS NULL
    CREATE DATABASE Conciliador;
GO
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'conciliador')
    CREATE LOGIN conciliador WITH PASSWORD = N'DEFINIR_PASSWORD_REAL', CHECK_POLICY = OFF;
GO
USE Conciliador;
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'conciliador')
BEGIN
    CREATE USER conciliador FOR LOGIN conciliador;
    ALTER ROLE db_owner ADD MEMBER conciliador;
END
GO
```

Nota: el password real lo define el usuario al ejecutarlo; NO commitear el password real en el script (dejar el placeholder).

- [ ] **Step 2: Ejecutarlo en SSMS y verificar**

Conectarse con el login nuevo desde SSMS: `SELECT DB_NAME();` → `Conciliador`.

- [ ] **Step 3: Commit**

```bash
git add docs/sql/00-provision-conciliador.sql
git commit -m "feat(sql): script de aprovisionamiento de base Conciliador y login"
```

### Task 4: Proyecto Conciliador.Comun con la conexión compartida

**Files:**
- Create: `Conciliador.Comun/Conciliador.Comun.csproj`
- Create: `Conciliador.Comun/SqlDb.cs`
- Modify: `ConciliadorContable.sln` (agregar proyecto)
- Modify: `AgrupadorConceptos/AgrupadorConceptos.csproj` (ProjectReference)
- Modify: `ConciliadorContable/App.config` (appSetting con la connection string)

**Interfaces:**
- Produces: `Conciliador.Comun.SqlDb.GetConnection()` → `Microsoft.Data.SqlClient.SqlConnection` (cerrada; el llamador hace `Open()`), y `SqlDb.ConnectionString` (get/set para override en runtime o tests manuales).

- [ ] **Step 1: Crear el csproj**

`Conciliador.Comun/Conciliador.Comun.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>disable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Data.SqlClient" Version="6.1.4" />
    <PackageReference Include="System.Configuration.ConfigurationManager" Version="8.0.0" />
  </ItemGroup>
</Project>
```

Nota: `TargetFramework` debe ser compatible con los módulos (que son `net8.0-windows`); `net8.0` puro es referenciable por ellos. Si la solución usa otra versión base, igualarla.

- [ ] **Step 2: Crear SqlDb.cs**

```csharp
using Microsoft.Data.SqlClient;
using System.Configuration;

namespace Conciliador.Comun
{
    /// <summary>
    /// Punto único de conexión a la base común "Conciliador".
    /// La connection string se puede overridear vía App.config
    /// (appSetting "ConciliadorSqlConnectionString") o seteando la propiedad.
    /// </summary>
    public static class SqlDb
    {
        private const string Default =
            "Server=192.168.7.51;Database=Conciliador;User Id=conciliador;Password=DEFINIR;TrustServerCertificate=True;";

        public static string ConnectionString { get; set; } =
            ConfigurationManager.AppSettings["ConciliadorSqlConnectionString"] ?? Default;

        public static SqlConnection GetConnection() => new SqlConnection(ConnectionString);
    }
}
```

- [ ] **Step 3: Agregar a la solución y referenciar**

```bash
dotnet sln ConciliadorContable.sln add Conciliador.Comun/Conciliador.Comun.csproj
```

En `AgrupadorConceptos/AgrupadorConceptos.csproj` agregar:

```xml
<ItemGroup>
  <ProjectReference Include="..\Conciliador.Comun\Conciliador.Comun.csproj" />
</ItemGroup>
```

- [ ] **Step 4: App.config del shell**

En `ConciliadorContable/App.config`, dentro de `<appSettings>` (ya existe por el tema Telerik):

```xml
<add key="ConciliadorSqlConnectionString"
     value="Server=192.168.7.51;Database=Conciliador;User Id=conciliador;Password=DEFINIR;TrustServerCertificate=True;" />
```

El password real se configura en cada puesto editando el `.config` desplegado; en el repo queda el placeholder (mismo criterio que ya se usa con las connection strings de Octosis en perfiles).

Nota de arquitectura: el appSetting vive en el `App.config` del shell solo porque .NET lee la configuración del **proceso** (el `.exe.config`), no porque el shell sea central. La lógica de resolución (`SqlDb`) vive en `Conciliador.Comun`, y `SqlDb.ConnectionString` tiene setter público: cualquier otro host (o un módulo corriendo bajo otro exe) puede setearla programáticamente sin depender del config del shell.

- [ ] **Step 5: Compilar y commit**

Build OK desde VS (restaurar el paquete nuevo desde VS por el nuget.config roto).

```bash
git add -A
git commit -m "feat(comun): proyecto Conciliador.Comun con conexion compartida a SQL Server"
```

### Task 5: Schema bancos (DDL) y DatabaseHelper de AgrupadorConceptos sobre SqlConnection

**Files:**
- Create: `AgrupadorConceptos/Data/SqlSchema.cs` (DDL idempotente embebido)
- Modify: `AgrupadorConceptos/Data/DatabaseHelper.cs` (reescritura completa)
- Modify: `AgrupadorConceptos/AgrupadorConceptos.csproj` (quitar Microsoft.Data.Sqlite queda para Task 8)

**Interfaces:**
- Consumes: `Conciliador.Comun.SqlDb.GetConnection()` (Task 4).
- Produces: `AgrupadorConceptos.Data.DatabaseHelper.GetConnection()` → `SqlConnection` (cerrada, igual contrato que antes: los llamadores hacen `Open()` o dejan que Dapper abra); `InitializeDatabase()` idempotente y run-once por proceso.

- [ ] **Step 1: Crear SqlSchema.cs con el DDL completo**

Decisiones de tipos: importes `REAL` → `DECIMAL(18,2)`; fechas-texto de los modelos (`string Fecha`) → `NVARCHAR(30)`; fechas reales (`DateTime` en modelos) → `DATETIME2(0)`; flags `INTEGER` 0/1 con modelo `bool` → `BIT`. Cascadas: solo `MovimientosArchivo→ArchivosImportados` (camino único); las tablas de conciliación NO llevan cascada (SQL Server rechaza los caminos múltiples de `ConciliacionPares`) — el borrado ordenado se hace en código (Task 6, Step 3).

```csharp
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
```

- [ ] **Step 2: Reescribir DatabaseHelper.cs**

Reemplazar el contenido completo de `AgrupadorConceptos/Data/DatabaseHelper.cs`:

```csharp
using Conciliador.Comun;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AgrupadorConceptos.Data
{
    public static class DatabaseHelper
    {
        private static bool _inicializada;

        /// <summary>
        /// Conexión a la base común Conciliador (schema bancos).
        /// Mismo contrato que antes: se devuelve cerrada.
        /// </summary>
        public static SqlConnection GetConnection() => SqlDb.GetConnection();

        /// <summary>
        /// Crea schema/tablas/índices si no existen. Run-once por proceso:
        /// el shell la llama antes de abrir cada form del módulo.
        /// </summary>
        public static void InitializeDatabase()
        {
            if (_inicializada) return;
            using var cn = GetConnection();
            cn.Open();
            cn.Execute(SqlSchema.Ddl);
            _inicializada = true;
        }
    }
}
```

Nota de autosuficiencia: `InitializeDatabase()` sigue siendo pública y las llamadas existentes del shell (`FormMenuPrincipal.cs:59,65,72`) siguen funcionando, pero al ser run-once y barata tras la primera vez, los constructores de `ProcesadorForm`, `GestionHomologacionesForm` y `ConciliacionExternForm` deben llamarla también como primera línea — así el módulo se inicializa solo y ningún host necesita saberlo. (Las llamadas del shell pueden quedar; son inofensivas.)

Nota: los llamadores usan `using var cn = DatabaseHelper.GetConnection()` con `var`, así que el cambio de tipo `SqliteConnection`→`SqlConnection` compila sin tocarlos. Los que declaren el tipo explícito se ajustan en esta task (buscar `SqliteConnection` en el proyecto: solo debería estar en `DatabaseHelper.cs`).

- [ ] **Step 3: Compilar**

El proyecto va a compilar pero AÚN NO FUNCIONA contra SQL Server (el SQL embebido sigue en dialecto SQLite y sin prefijo de schema) — eso es Task 6. Verificar solamente que no queden referencias a `Microsoft.Data.Sqlite` en el código de AgrupadorConceptos (el package se quita en Task 8).

- [ ] **Step 4: Commit**

```bash
git add AgrupadorConceptos/Data/SqlSchema.cs AgrupadorConceptos/Data/DatabaseHelper.cs
git commit -m "feat(bancos): DDL schema bancos y DatabaseHelper sobre SqlConnection"
```

### Task 6: Portar el dialecto SQLite → T-SQL en AgrupadorConceptos

**Files:**
- Modify: `AgrupadorConceptos/ProcesadorForm.cs` (líneas ~508, ~621 y todo SQL)
- Modify: `AgrupadorConceptos/Services/ConciliacionExternService.cs` (líneas ~50, ~73 y todo SQL)
- Modify: `AgrupadorConceptos/HomologarForm.cs` (línea ~70 y todo SQL)
- Modify: `AgrupadorConceptos/MainForm.cs`, `GestionHomologacionesForm.cs`, `ConciliacionExternForm.cs`, `SeleccionCandidatoDialog.cs` y cualquier otro `.cs` con SQL (enumerar con el grep del Step 1)

**Interfaces:**
- Consumes: `DatabaseHelper.GetConnection()` (Task 5), tablas `bancos.*` (Task 5).
- Produces: todo el SQL del módulo ejecutable contra SQL Server.

- [ ] **Step 1: Enumerar todos los statements a tocar**

```bash
grep -rnE "(FROM|INTO|UPDATE|JOIN|DELETE FROM|EXISTS)\s+(PerfilesBanco|ConceptosEstandar|HomologacionConceptos|ArchivosImportados|MovimientosArchivo|ConciliacionSesiones|ConciliacionItemsExternos|ConciliacionPares)" AgrupadorConceptos --include="*.cs"
```

Anotar la lista completa; sirve de checklist para los steps siguientes.

- [ ] **Step 2: Prefijar schema `bancos.` en todos los statements**

Reemplazo mecánico sobre la lista del Step 1: cada nombre de tabla pasa a `bancos.NombreTabla` (solo dentro de strings SQL; no tocar nombres de clases/modelos). Ejemplo: `SELECT * FROM MovimientosArchivo WHERE IdArchivo = @IdArchivo` → `SELECT * FROM bancos.MovimientosArchivo WHERE IdArchivo = @IdArchivo`.

- [ ] **Step 3: Portar los 4 usos de dialecto SQLite**

1. `ProcesadorForm.cs:508` — `RETURNING Id` → `OUTPUT INSERTED.Id` (va ANTES de `VALUES`):

```csharp
"INSERT INTO bancos.ArchivosImportados (IdPerfilBanco, NombreArchivo, Fecha) OUTPUT INSERTED.Id VALUES (@IdPerfil, @Nombre, @Fecha);",
```

2. `ProcesadorForm.cs:621` — ídem:

```csharp
mov.Id = connection.QuerySingle<int>(@"
    INSERT INTO bancos.MovimientosArchivo (IdArchivo, Fecha, ConceptoOriginal, DescripcionOriginal, Debitos, Creditos, ConceptoEstandar, ConceptoFinal)
    OUTPUT INSERTED.Id
    VALUES (@IdArchivo, @Fecha, @ConceptoOriginal, @DescripcionOriginal, @Debitos, @Creditos, @ConceptoEstandar, @ConceptoFinal);", mov, tx);
```

3. `ConciliacionExternService.cs:50` — `SELECT last_insert_rowid();` → `SELECT CAST(SCOPE_IDENTITY() AS INT);` (queda en el mismo batch que el INSERT, funciona igual con Dapper `QuerySingle<int>`).

4. `HomologarForm.cs:70` — `connection.QueryFirst<int>("SELECT last_insert_rowid()")` es una llamada SEPARADA del INSERT: `SCOPE_IDENTITY()` devuelve NULL en otro batch. Unificar INSERT + `SELECT CAST(SCOPE_IDENTITY() AS INT);` en un solo `QuerySingle<int>` (mover el SELECT al mismo string SQL del INSERT que está unas líneas arriba).

- [ ] **Step 4: Borrado explícito en EliminarSesion (reemplaza la cascada)**

En `ConciliacionExternService.cs:73`, la sesión se borraba confiando en `ON DELETE CASCADE`. En SQL Server esas FK quedaron sin cascada (caminos múltiples). Reemplazar por borrado ordenado transaccional:

```csharp
using var cn = DatabaseHelper.GetConnection();
cn.Open();
using var tx = cn.BeginTransaction();
cn.Execute("DELETE FROM bancos.ConciliacionPares WHERE IdSesion = @Id", new { Id = idSesion }, tx);
cn.Execute("DELETE FROM bancos.ConciliacionItemsExternos WHERE IdSesion = @Id", new { Id = idSesion }, tx);
cn.Execute("DELETE FROM bancos.ConciliacionSesiones WHERE Id = @Id", new { Id = idSesion }, tx);
tx.Commit();
```

(Adaptar nombres al método real; conservar la firma pública existente.)

- [ ] **Step 5: Revisar restos de dialecto**

```bash
grep -rniE "last_insert_rowid|RETURNING |INSERT OR |ON CONFLICT|AUTOINCREMENT|PRAGMA|LIMIT [0-9]|strftime|julianday|COLLATE NOCASE" AgrupadorConceptos --include="*.cs"
```

Expected: 0 resultados (si aparece `LIMIT n`, convertir a `SELECT TOP (n)`).

- [ ] **Step 6: Compilar, probar contra el servidor y commit**

Build OK. Ejecutar el shell → abrir Procesador de Extractos: al inicializar debe crear el schema `bancos` en el servidor. Verificar en SSMS:

```sql
SELECT s.name, t.name FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id WHERE s.name = 'bancos';
-- Expected: las 8 tablas
```

Crear un perfil de banco de prueba e importar un extracto chico. Verificar `SELECT COUNT(*) FROM bancos.MovimientosArchivo;`.

```bash
git add -A
git commit -m "feat(bancos): SQL del modulo portado a T-SQL contra schema bancos"
```

### Task 7: Migrador de datos DataBanks.db → SQL Server

**Files:**
- Create: `Tools/MigradorDataBanks/MigradorDataBanks.csproj`
- Create: `Tools/MigradorDataBanks/Program.cs`
- Modify: `ConciliadorContable.sln`

**Interfaces:**
- Consumes: schema `bancos` (Task 5), `DataBanks.db` existente de cada puesto.
- Produces: console app `MigradorDataBanks.exe <ruta DataBanks.db> <connection string>` — copia con IDs preservados, re-ejecutable solo sobre base destino vacía.

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

// Uso: MigradorDataBanks <ruta DataBanks.db> <connection string SQL Server>
if (args.Length != 2)
{
    Console.WriteLine("Uso: MigradorDataBanks <ruta DataBanks.db> <connection string>");
    return 1;
}

using var src = new SqliteConnection($"Data Source={args[0]};Mode=ReadOnly");
src.Open();
using var dst = new SqlConnection(args[1]);
dst.Open();

// 0) Abortamos si el destino ya tiene datos (evita duplicar en re-ejecuciones)
using (var check = dst.CreateCommand())
{
    check.CommandText = "SELECT COUNT(*) FROM bancos.PerfilesBanco";
    if ((int)check.ExecuteScalar()! > 0)
    {
        Console.WriteLine("ERROR: bancos.PerfilesBanco ya tiene datos. Vaciar el schema bancos antes de migrar.");
        return 2;
    }
}

// 1) Diagnóstico de huérfanos en SQLite (las cascadas nunca corrieron:
//    Microsoft.Data.Sqlite no activa PRAGMA foreign_keys). Solo informamos
//    y los excluimos en los SELECT; el .db original no se toca (ReadOnly).
var filtros = new Dictionary<string, string>
{
    ["ArchivosImportados"]        = "WHERE IdPerfilBanco IN (SELECT Id FROM PerfilesBanco)",
    ["MovimientosArchivo"]        = "WHERE IdArchivo IN (SELECT Id FROM ArchivosImportados WHERE IdPerfilBanco IN (SELECT Id FROM PerfilesBanco))",
    ["HomologacionConceptos"]     = "WHERE IdPerfilBanco IN (SELECT Id FROM PerfilesBanco) AND IdConceptoEstandar IN (SELECT Id FROM ConceptosEstandar)",
    ["ConciliacionSesiones"]      = "WHERE IdArchivoImportado IN (SELECT Id FROM ArchivosImportados WHERE IdPerfilBanco IN (SELECT Id FROM PerfilesBanco))",
    ["ConciliacionItemsExternos"] = "WHERE IdSesion IN (SELECT Id FROM ConciliacionSesiones WHERE IdArchivoImportado IN (SELECT Id FROM ArchivosImportados WHERE IdPerfilBanco IN (SELECT Id FROM PerfilesBanco)))",
    ["ConciliacionPares"]         = "WHERE IdSesion IN (SELECT Id FROM ConciliacionSesiones) AND IdItemExterno IN (SELECT Id FROM ConciliacionItemsExternos)",
};

// 2) Copia en orden de dependencias, preservando IDs
string[] tablas =
{
    "PerfilesBanco", "ConceptosEstandar", "HomologacionConceptos",
    "ArchivosImportados", "MovimientosArchivo",
    "ConciliacionSesiones", "ConciliacionItemsExternos", "ConciliacionPares",
};

foreach (var tabla in tablas)
{
    var where = filtros.TryGetValue(tabla, out var f) ? f : "";
    using var cmd = src.CreateCommand();
    cmd.CommandText = $"SELECT * FROM {tabla} {where} ORDER BY Id";
    using var reader = cmd.ExecuteReader();
    var dt = new DataTable();
    dt.Load(reader);

    using var bulk = new SqlBulkCopy(dst, SqlBulkCopyOptions.KeepIdentity, null)
    {
        DestinationTableName = $"bancos.{tabla}",
        BatchSize = 5000,
    };
    foreach (DataColumn c in dt.Columns)
        bulk.ColumnMappings.Add(c.ColumnName, c.ColumnName);
    bulk.WriteToServer(dt);
    Console.WriteLine($"bancos.{tabla}: {dt.Rows.Count} filas");
}

Console.WriteLine("Migracion OK.");
return 0;
```

- [ ] **Step 3: Agregar a la solución y compilar**

```bash
dotnet sln ConciliadorContable.sln add Tools/MigradorDataBanks/MigradorDataBanks.csproj
```

- [ ] **Step 4: Ejecutar contra el DataBanks.db real**

```bash
Tools/MigradorDataBanks/bin/Debug/net8.0/MigradorDataBanks.exe "<ruta al DataBanks.db del puesto>" "Server=192.168.7.51;Database=Conciliador;User Id=conciliador;Password=...;TrustServerCertificate=True;"
```

Expected: una línea por tabla con conteos y `Migracion OK.`

- [ ] **Step 5: Verificar conteos origen vs destino**

En SQLite (con la CLI o DB Browser) y en SSMS, comparar `SELECT COUNT(*)` por tabla. Deben coincidir salvo los huérfanos excluidos; si hay diferencia, revisar cuántas filas filtró cada `WHERE` antes de dar por buena la migración. Verificar también un caso de negocio: abrir una sesión de conciliación vieja desde la app y confirmar que carga igual que antes.

- [ ] **Step 6: Commit**

```bash
git add Tools/MigradorDataBanks ConciliadorContable.sln
git commit -m "feat(bancos): migrador one-shot de DataBanks.db a SQL Server"
```

### Task 8: Limpieza final y verificación end-to-end

**Files:**
- Modify: `AgrupadorConceptos/AgrupadorConceptos.csproj` (quitar PackageReference Microsoft.Data.Sqlite)

**Interfaces:**
- Consumes: todo lo anterior.

- [ ] **Step 1: Quitar Microsoft.Data.Sqlite de AgrupadorConceptos**

Eliminar el `<PackageReference Include="Microsoft.Data.Sqlite" ...>` del csproj. Verificar:

```bash
grep -rn "Sqlite" AgrupadorConceptos --include="*.cs"
```

Expected: 0 resultados.

- [ ] **Step 2: Build completo y regresión manual**

Con la base migrada, recorrer el ciclo completo desde el shell:
1. Crear un perfil de banco nuevo → aparece en `bancos.PerfilesBanco`.
2. Importar un extracto real (el caso que originó todo) → cronometrar; debe mantenerse en segundos (la transacción única ya está; la latencia de red por fila queda amortizada por la transacción).
3. Homologar conceptos (valida `SCOPE_IDENTITY` de HomologarForm).
4. Crear una sesión de conciliación externa, conciliar pares, eliminar la sesión (valida el borrado explícito de Task 6 Step 4).
5. Eliminar un archivo importado (valida la cascada de `MovimientosArchivo`).
6. Reabrir la app: `InitializeDatabase` no debe fallar ni recrear nada.

- [ ] **Step 3: Verificar que el archivo DataBanks.db ya no se crea**

Borrar (o renombrar) `DataBanks.db` del directorio de trabajo y abrir el módulo: no debe recrearse ningún `.db` nuevo.

- [ ] **Step 4: Commit final**

```bash
git add -A
git commit -m "chore(bancos): retirar Microsoft.Data.Sqlite de AgrupadorConceptos"
```

---

## Fuera de alcance (etapas futuras, cada una con su propio plan)

- **Etapa 2 — ArcaCliente + shell:** schema `arca` (6 tablas de `ArcaSqliteStorage.cs`, con sus upserts `ON CONFLICT`/`INSERT OR IGNORE` → `MERGE`/`IF EXISTS`) y schema `seg` (tabla `Usuarios` del shell, ojo con `COLLATE NOCASE` → definir collation explícita CS o CI a propósito). Al terminar, `conciliador.db` desaparece.
- **Etapa 3 — Justificaciones, historial y estadísticas:** tablas nuevas (probablemente schema propio `auditoria` referenciando `bancos.ConciliacionPares` / sesiones y su equivalente ARCA). Requiere brainstorming propio de requerimientos antes de diseñar.
- **LiquidacionesAuditar:** queda en SQLite por decisión del usuario.
