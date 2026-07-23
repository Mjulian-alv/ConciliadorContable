# Migración de ArcaCliente a SQL Server — Etapa 2 (diseño)

## Contexto

Etapa 1 (rama `feature/migracion-sql-etapa0-1`, ya mergeada/completa) migró AgrupadorConceptos (schema `bancos`) de SQLite a la base común SQL Server, y convirtió ArcaCliente y AgrupadorConceptos de WinExe a Library. Esta etapa continúa el mismo camino con ArcaCliente: sus 6 tablas de configuración (`ArcaSqliteStorage.cs`) pasan al schema `arca` de la misma base.

**Fuera de alcance (decisión explícita):** la tabla `Usuarios`/login del shell (`ConciliadorContable/Data/DatabaseHelper.cs`, futuro schema `seg`) queda para un plan aparte — es un dominio distinto (autenticación vs. configuración de negocio) y separarlo mantiene este plan chico y revisable.

## Arquitectura

Mismo patrón que Etapa 1: `ArcaCliente` consume `Conciliador.Comun.SqlDb` (ya referenciado desde Etapa 0). El módulo se auto-inicializa sin coordinación del host — mismo principio de autosuficiencia ya establecido: *cualquier host que referencie la DLL debe poder abrir sus forms sin pasos de inicialización especiales*.

**Se elimina la coordinación host↔módulo que existe hoy:** `ConciliadorContable/Forms/FormMenuPrincipal.cs:18-19` asigna `ArcaStorageConfig.DbPath = DatabaseHelper.DbPath` y llama `ArcaStorageConfig.Initialize()` — el shell hoy tiene que conocer la ruta del SQLite de ArcaCliente. Con SQL Server esa coordinación no tiene sentido (la conexión la resuelve `SqlDb` sola) y esas 2 líneas se eliminan del shell.

`ArcaStorageConfig` pierde `DbPath`/`ConnectionString` (específicos de SQLite) y queda solo como gate de inicialización idempotente: `Initialize()` llama a `ArcaSqlStorage.InitializeDatabase()` una vez por proceso (bandera estática), invocado como primera línea del constructor de los 3 forms de entrada que el shell instancia: `FormPerfilesOffline`, `FormComprobantesOffline`, `FormEquivalencias`. Mismo patrón exacto que `AgrupadorConceptos.Data.DatabaseHelper.InitializeDatabase()` en Etapa 1 Task 5.

**Rename:** `ArcaSqliteStorage` → `ArcaSqlStorage`. El nombre actual dice "Sqlite" pero tras este plan usa `SqlConnection` — es un nombre activamente engañoso. Rename mecánico: 8 archivos la referencian, siempre por prefijo estático (`ArcaSqliteStorage.Xxx`).

Los 6 wrappers de una línea (`PerfilStorage`, `PerfilOfflineStorage`, `MapeoColumnasStorage`, `PreseaExportMemoryStorage`, `PreseaProveedorStorage`, `TipoComprobanteStorage`) **no cambian** — su interfaz pública queda intacta, solo cambia la implementación interna de `ArcaSqlStorage`.

## Schema `arca` — mapeo de tipos

Mismo criterio que en `bancos` (Etapa 1): donde el modelo C# ya expone un tipo real, la columna SQL usa ese tipo — no el workaround de texto que SQLite forzaba.

| Campo | Hoy (SQLite) | Pasa a | Motivo |
|---|---|---|---|
| `ConfigPreseaProveedor.Descuento` | `TEXT` (`ToString(Invariant)`/`ParseDec`) | `DECIMAL(9,2)` | El modelo ya es `decimal`; el TEXT era workaround de SQLite |
| `PreseaComprobanteExportado.Importe` | `TEXT` | `DECIMAL(18,2)` | ídem, mismo criterio que `bancos.MovimientosArchivo` |
| `PreseaComprobanteExportado.FechaExportacion` | `TEXT` (formato `"o"` + `RoundtripKind`) | `DATETIME2(3)` | Modelo es `DateTime` real |
| `TieneCabecera`, `IntegracionHabilitada` | `INTEGER` 0/1 | `BIT` | Modelo es `bool` |
| `Pos*`, `FilaEncabezado`, `TipoArchivo`, `TipoImporte`, `Sistema`, `SistemaExportacion` (enums) | `INTEGER` | `INT` | directo |
| `PerfilOffline.Id`, `PerfilFiscal.Id` (GUID) | `TEXT` | `NVARCHAR(36)` — **no** `UNIQUEIDENTIFIER` | El código hace `p.Id.ToString()`/`Guid.Parse(...)` en cada lectura/escritura; cambiar el tipo obligaría a tocar esos call sites sin ganancia real. Mismo criterio de mínimo-diff que `Fecha` en `bancos` |
| `ConfigPreseaJson`, `DirectivasJson`, `ConfigJson` (blobs JSON) | `TEXT` | `NVARCHAR(MAX)` | directo |
| Resto de columnas `string` | `TEXT` | `NVARCHAR(n)` dimensionado por uso | — |

**Password/connection strings en texto plano** (`ArcaPerfilesFiscales.Password`, `ConciliacionConnectionString`, `OctosisConnectionString`): se portan **tal cual**, sin cifrar. Es el mismo esquema de hoy; cifrar es una decisión de seguridad con su propio diseño (dónde vive la clave), fuera de alcance de este plan. Queda documentado como riesgo conocido: centralizar estas columnas en un SQL Server compartido amplía el radio de exposición de "un puesto" a "todos los perfiles fiscales de todos los puestos a la vez".

**Placeholders:** `$Param` (convención SQLite) → `@Param` (SQL Server no soporta `$`). Cambio mecánico pero extendido: ~60 ocurrencias en `ArcaSqlStorage.cs`, tanto en el texto SQL como en `Parameters.AddWithValue`.

## Upserts: dialecto SQLite → T-SQL

Tres statements a portar:

**`UpsertPreseaProveedor`** (`ON CONFLICT(Cuit) DO UPDATE SET ...`) → `MERGE`:
```sql
MERGE arca.PreseaProveedores AS t
USING (SELECT @Cuit AS Cuit) AS s ON t.Cuit = s.Cuit
WHEN MATCHED THEN UPDATE SET
    Nombre = @Nombre, CodigoProveedor = @CodigoProveedor,
    CuentaContableProveedor = @CuentaContableProveedor, CuentaDebe = @CuentaDebe,
    Centro = @Centro, Provincia = @Provincia, Condicion = @Condicion,
    Descuento = @Descuento, Fiscal = @Fiscal
WHEN NOT MATCHED THEN INSERT
    (Cuit, Nombre, CodigoProveedor, CuentaContableProveedor, CuentaDebe, Centro, Provincia, Condicion, Descuento, Fiscal)
    VALUES (@Cuit, @Nombre, @CodigoProveedor, @CuentaContableProveedor, @CuentaDebe, @Centro, @Provincia, @Condicion, @Descuento, @Fiscal);
```

**`SaveMapeoColumnas`** (`ON CONFLICT(Entidad) DO UPDATE SET ConfigJson = excluded.ConfigJson`) → mismo patrón `MERGE`, una sola columna a actualizar.

**`RegistrarComprobanteExportado`** (`INSERT OR IGNORE`) → es "ignorar duplicados", no "actualizar" — más simple como guard explícito que como `MERGE`:
```sql
IF NOT EXISTS (SELECT 1 FROM arca.PreseaComprobantesExportados WHERE Clave = @Clave)
    INSERT INTO arca.PreseaComprobantesExportados (...) VALUES (...);
```

El resto (`SavePerfilesOffline`, `SavePerfilesFiscales`, `SaveEquivalencias`) usa el patrón "reemplazar toda la colección" (`DELETE FROM tabla` + reinsertar todo en una transacción) — se porta sin cambios de lógica, funciona idéntico en T-SQL.

## Migración legacy JSON: se elimina

`ArcaSqliteStorage.MigrarDesdeLegacyIfNeeded()` (y sus 3 helpers privados `MigrarPerfilesOffline`, `MigrarPerfilesFiscales`, `MigrarEquivalencias`) importan datos desde archivos JSON/`.db` de una migración anterior (`perfiles_offline.json`, `perfiles.json`, `equivalencias.db`), guardados por "si la tabla destino está vacía, importar; si no, borrar el JSON sin importar".

Confirmado con el usuario: ya no quedan instalaciones con esos archivos legacy sin migrar. Se elimina el método y sus 3 helpers privados por completo — es código muerto que, bajo un store *compartido*, pasaría de inofensivo a un riesgo real: el primer puesto que arrancara post-migración importaría su JSON sin problema, pero cualquier otro puesto que todavía tuviera JSON legacy sin migrar se encontraría la tabla ya no-vacía y **borraría su JSON sin importarlo** (pérdida de datos silenciosa).

## Migración de datos existentes

Mismo enfoque que `Tools/MigradorDataBanks` (Etapa 1): console app nueva `Tools/MigradorArcaCliente` que lee el `conciliador.db` local y copia las 6 tablas de ArcaCliente (no toca `Usuarios`). Como las PK son GUID/CUIT/Entidad (no `IDENTITY`), es más simple que el migrador de bancos — no requiere reseed de identity.

Mismo guard que Etapa 1: aborta si el destino (`arca.ArcaPerfilesOffline`) ya tiene filas, para no duplicar en una re-ejecución.

**Asunción documentada:** si hay más de una instalación con `conciliador.db` propios y perfiles *distintos* configurados localmente, el migrador solo puede correr limpio contra una de ellas (la primera "gana" la tabla compartida, por el guard). Cuáles perfiles de cada sitio terminan siendo los "canónicos" en la base central es una decisión manual — consecuencia inherente de centralizar algo que hoy es config por puesto, no un problema técnico del migrador.

## Efecto colateral positivo: memoria anti-duplicado de PRESEA

`ExisteComprobanteExportado`/`LoadClavesExportadas` pasan a consultar la tabla compartida `arca.PreseaComprobantesExportados`. Hoy, si el mismo comprobante se exporta desde dos puestos distintos, cada uno tiene su memoria local y no se detectan entre sí — centralizado, sí se detectan. No es el objetivo de este plan pero cae gratis.

## Fuera de alcance

- Schema `seg` (Usuarios/login del shell) — plan aparte.
- Cifrado de `Password`/connection strings — decisión de seguridad con diseño propio.
- Reconciliación manual de perfiles si existen múltiples instalaciones con configuraciones distintas — es una decisión humana, no del migrador.
