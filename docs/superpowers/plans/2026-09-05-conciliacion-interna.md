# Conciliación interna entre extractos — Plan de implementación (TAREA 00021, ítem 5)

> **Para agentes:** SUB-SKILL REQUERIDA: usar `superpowers:subagent-driven-development` (recomendado) o `superpowers:executing-plans` para ejecutar tarea por tarea. Los pasos usan checkbox (`- [ ]`) para seguimiento.

**Goal:** Una ventana nueva para conciliar transferencias entre cuentas propias: el mismo movimiento aparece en dos extractos (débito en uno, crédito en el otro, mismo importe), y hay que emparejarlos por fecha + importe, automático o manual. Al finalizar la sesión, cada movimiento conciliado recibe como Cuenta Final la cuenta contable del *perfil* del otro lado (la contrapartida de la transferencia).

**Architecture:** Clon del esqueleto de `ConciliacionExternForm`/`ConciliacionExternService` (panel de sesiones, panel de alta, grillas de pendientes con resaltado de candidatos, auto/manual/desconciliar/finalizar/exportar) reemplazando el lado "archivo externo" por un segundo extracto: en vez de `ConciliacionItemExterno` leído de un Excel, el lado B también es `MovimientoProcesado`. Tablas nuevas y paralelas (`ConciliacionInternaSesiones` / `ConciliacionInternaPares`): no se toca el código de conciliación externa, que ya está en producción. Ver el diseño completo en `docs/superpowers/specs/2026-09-05-cuentas-contables-perfil-concepto-conciliacion-interna-design.md`.

**Tech Stack:** .NET 8 (`net8.0-windows`), WinForms, Telerik UI for WinForms 2024.4.1113 (`RadGridView`), Dapper 2.1.35 sobre SQL Server (schema `bancos`), ClosedXML para el export a Excel.

## Global Constraints

- **Sin proyecto de tests.** La verificación es `dotnet build` limpio + el checklist manual de la última tarea, contra una base real.
- **Comentario de versionado en cada bloque agregado o modificado:**
  `// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Descripción`
  En archivo nuevo va arriba de todo; en edición, inmediatamente encima del bloque. Este plan cubre un único ítem del pedido (el 5), así que todos los comentarios llevan `Linea: 5`.
- **Matching:** decidido con el usuario — mismo importe absoluto, **signo opuesto** (uno en `Debitos`, el otro en `Creditos`), fecha igual o cercana. Dos pasadas, igual que la conciliación externa: 1) fecha + importe opuesto exacto, 2) sólo importe opuesto entre lo que quedó sin conciliar.
- **Persistencia:** tablas nuevas y paralelas. No se modifica `ConciliacionExternService`, `ConciliacionSesiones`, `ConciliacionItemsExternos` ni `ConciliacionPares`.
- **Cuenta contable al finalizar (decidido con el usuario, agregado después de la primera versión de este plan):**
  al finalizar la sesión completa (no antes, no por par individual), cada par conciliado recibe la cuenta
  contable **del perfil del otro lado** como contrapartida: `CuentaFinal` de A pasa a ser la cuenta del
  perfil de B, y `CuentaFinal` de B pasa a ser la cuenta del perfil de A. Se pisa aunque el usuario ya
  hubiera editado esa `CuentaFinal` a mano (es una acción deliberada del cierre, no un autocompletado
  pasivo). Si el perfil de algún lado de algún par no tiene cuenta contable asignada (ítem 2), **se
  bloquea el cierre de toda la sesión** — no finaliza nada hasta que todos los perfiles involucrados
  tengan cuenta asignada. Esto es una excepción puntual a la regla general de "sin cascada" de
  `docs/superpowers/plans/2026-09-05-cuentas-contables.md`: esa regla rige el autocompletado por
  defecto al homologar/importar; esta es una acción explícita distinta, disparada por el cierre de la
  conciliación interna.
- **Edición manual bloqueada mientras está conciliada (agregado en la misma revisión):** un
  movimiento con un par en una sesión interna todavía "EnProceso" no deja editar su `CuentaFinal` a
  mano en la grilla del Procesador — se va a pisar sola al cerrar esa sesión, así que editarla antes
  sería trabajo perdido. Deja de estar bloqueada si se desconcilia el par o si la sesión ya se
  finalizó (ahí no hay ningún cierre futuro pendiente que la vuelva a tocar).
- **Selección por perfil + rango de fechas, no por archivo (agregado en la misma revisión):** cada
  lado de la sesión es un `PerfilBanco` y un rango `[Desde, Hasta]`, no una lista de
  `ArchivosImportados` tildados. Como `MovimientosArchivo.Fecha` es `NVARCHAR` (texto tal como lo
  exporta cada banco), el filtro por rango se resuelve en memoria reusando el mismo parseo de fechas
  que ya usa el resaltado de la conciliación externa, no con una condición de rango en SQL.
- **SQL siempre parametrizado**, nunca interpolado.
- **Comentarios y textos de UI en español.**
- **Encoding:** archivos nuevos en **UTF-8 con BOM**.
- Comando de build único:
  ```bash
  dotnet build ConciliadorContable.slnx -v q --nologo
  ```
  Esperado: `0 Errores`.

---

## Estructura de archivos

| Archivo | Responsabilidad | Tarea |
|---|---|---|
| `AgrupadorConceptos/Data/SqlSchema.cs` | *Modificar.* Tablas `ConciliacionInternaSesiones` (perfil+rango por lado) / `ConciliacionInternaPares`. | 1 |
| `AgrupadorConceptos/Models/ConciliacionInternaSesion.cs` | *Crear.* | 1 |
| `AgrupadorConceptos/Models/ConciliacionInternaPar.cs` | *Crear.* | 1 |
| `AgrupadorConceptos/Services/ComparadorConciliacion.cs` | *Modificar.* Suma `ParsearFecha`, sin tocar lo existente. | 2 |
| `AgrupadorConceptos/Services/ComparadorConciliacionInterna.cs` | *Crear.* Importe opuesto + filtro por rango de fechas. | 2 |
| `AgrupadorConceptos/Models/ResultadoFinalizacionInterna.cs` | *Crear.* Resultado de `Finalizar`: éxito o pares sin cuenta. | 2 |
| `AgrupadorConceptos/Services/ConciliacionInternaService.cs` | *Crear.* CRUD por perfil+rango, auto-conciliación, cierre. | 2 |
| `AgrupadorConceptos/SeleccionCandidatoInternoDialog.cs` | *Crear.* Desempate manual de duplicados. | 3 |
| `AgrupadorConceptos/ConciliacionInternaForm.cs` + `.Designer.cs` | *Crear.* Alta de sesión por perfil + rango de fechas. | 4 |
| `AgrupadorConceptos/ProcesadorForm.cs` | *Modificar.* Bloquea editar Cuenta Final si está conciliada (interna) en proceso. | 5 |
| `ConciliadorContable/Models/Usuario.cs` | *Modificar.* Permiso `AgrConciliacionInterna`. | 6 |
| `ConciliadorContable/Forms/FormMenuPrincipal.cs` + `.Designer.cs` | *Modificar.* Entrada de menú. | 6 |
| `docs/Historial.md`, `~/.claude/TAREAS.md` | *Modificar.* | 7 |

---

### Task 1: Tablas y modelos

**Files:**
- Modify: `AgrupadorConceptos/Data/SqlSchema.cs` (después de `bancos.ConciliacionPares`, antes del bloque de índices)
- Create: `AgrupadorConceptos/Models/ConciliacionInternaSesion.cs`
- Create: `AgrupadorConceptos/Models/ConciliacionInternaPar.cs`

**Interfaces:**
- Consumes: nada.
- Produces:
  - `ConciliacionInternaSesion { int Id; string Nombre; DateTime FechaCreacion; int IdPerfilA; DateTime FechaDesdeA; DateTime FechaHastaA; int IdPerfilB; DateTime FechaDesdeB; DateTime FechaHastaB; string ConceptosJson; string Estado; string DisplayName }`
  - `ConciliacionInternaPar { int Id; int IdSesion; int IdMovimientoA; int IdMovimientoB; string TipoMatch; DateTime FechaConciliacion; + campos de visualización FechaA/ImporteA/ConceptoFinalA/FechaB/ImporteB/ConceptoFinalB }`
  - Reusa el enum `Models.TipoMatch` ya existente (`FechaImporte`, `SoloImporte`, `Manual`) — misma idea, "importe" acá significa "importe opuesto".

**Nota (agregada tras la primera versión de este plan):** la selección de qué conciliar pasa de
"tildar archivos importados" a **elegir un perfil de banco + un rango de fechas por lado**. Los
movimientos de la sesión salen de recorrer todo lo importado de ese perfil y quedarse con lo que
cae en el rango — no de una lista fija de `IdArchivo`. Como consecuencia, la sesión ya no necesita
guardar una lista de archivos: guarda el perfil y el rango de cada lado directamente, y eso además
simplifica `Finalizar` (Tarea 2): el perfil de cada lado sale de la sesión, no hay que resolverlo
archivo por archivo.

- [ ] **Step 1: Tablas en el DDL**

En `AgrupadorConceptos/Data/SqlSchema.cs`, insertar inmediatamente después del bloque `CREATE TABLE bancos.ConciliacionPares (...)` (justo antes de la sección `IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MovimientosArchivo_IdArchivo')`):

```sql
// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Conciliacion interna entre extractos propios
IF OBJECT_ID(N'bancos.ConciliacionInternaSesiones', N'U') IS NULL
CREATE TABLE bancos.ConciliacionInternaSesiones (
    Id            INT IDENTITY(1,1) CONSTRAINT PK_ConciliacionInternaSesiones PRIMARY KEY,
    Nombre        NVARCHAR(200) NOT NULL,
    FechaCreacion DATETIME2(0) NOT NULL,
    IdPerfilA     INT NOT NULL CONSTRAINT FK_SesionInterna_PerfilA REFERENCES bancos.PerfilesBanco(Id),
    FechaDesdeA   DATE NOT NULL,
    FechaHastaA   DATE NOT NULL,
    IdPerfilB     INT NOT NULL CONSTRAINT FK_SesionInterna_PerfilB REFERENCES bancos.PerfilesBanco(Id),
    FechaDesdeB   DATE NOT NULL,
    FechaHastaB   DATE NOT NULL,
    ConceptosJson NVARCHAR(MAX) NOT NULL CONSTRAINT DF_SesionesInternas_Conceptos DEFAULT N'[]',
    Estado        NVARCHAR(50) NOT NULL CONSTRAINT DF_SesionesInternas_Estado DEFAULT N'EnProceso'
);

IF OBJECT_ID(N'bancos.ConciliacionInternaPares', N'U') IS NULL
CREATE TABLE bancos.ConciliacionInternaPares (
    Id                INT IDENTITY(1,1) CONSTRAINT PK_ConciliacionInternaPares PRIMARY KEY,
    IdSesion          INT NOT NULL CONSTRAINT FK_ParesInternos_Sesion
                          REFERENCES bancos.ConciliacionInternaSesiones(Id),
    IdMovimientoA     INT NOT NULL,
    IdMovimientoB     INT NOT NULL,
    TipoMatch         NVARCHAR(50) NOT NULL,
    FechaConciliacion DATETIME2(0) NOT NULL
);

```

Y sumar el índice al bloque de índices existente, junto a `IX_ConciliacionPares_IdSesion`:

```sql
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConciliacionInternaPares_IdSesion')
    CREATE INDEX IX_ConciliacionInternaPares_IdSesion ON bancos.ConciliacionInternaPares(IdSesion);
```

- [ ] **Step 2: Modelos**

Crear `AgrupadorConceptos/Models/ConciliacionInternaSesion.cs` (UTF-8 con BOM):

```csharp
// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Sesion de conciliacion interna entre extractos
using System;

namespace AgrupadorConceptos.Models
{
    /// <summary>
    /// Sesión de conciliación interna: dos lados, cada uno un perfil de banco y un rango de
    /// fechas (no una lista fija de archivos importados — los movimientos se recalculan del
    /// perfil filtrando por rango cada vez que se abre la sesión).
    /// </summary>
    public class ConciliacionInternaSesion
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int IdPerfilA { get; set; }
        public DateTime FechaDesdeA { get; set; }
        public DateTime FechaHastaA { get; set; }
        public int IdPerfilB { get; set; }
        public DateTime FechaDesdeB { get; set; }
        public DateTime FechaHastaB { get; set; }
        public string ConceptosJson { get; set; }
        public string Estado { get; set; }

        public string DisplayName => $"{Nombre} ({FechaCreacion:dd/MM/yyyy HH:mm}) [{Estado}]";
    }
}
```

Crear `AgrupadorConceptos/Models/ConciliacionInternaPar.cs` (UTF-8 con BOM):

```csharp
// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Par conciliado entre los dos extractos
using System;

namespace AgrupadorConceptos.Models
{
    public class ConciliacionInternaPar
    {
        public int Id { get; set; }
        public int IdSesion { get; set; }
        public int IdMovimientoA { get; set; }
        public int IdMovimientoB { get; set; }
        public string TipoMatch { get; set; }
        public DateTime FechaConciliacion { get; set; }

        // Campos de visualización (no persisten)
        public string FechaA { get; set; }
        public decimal ImporteA { get; set; }
        public string ConceptoFinalA { get; set; }
        public string FechaB { get; set; }
        public decimal ImporteB { get; set; }
        public string ConceptoFinalB { get; set; }
    }
}
```

- [ ] **Step 3: Compilar**

```bash
dotnet build ConciliadorContable.slnx -v q --nologo
```

Esperado: `0 Errores`.

- [ ] **Step 4: Commit**

```bash
git add AgrupadorConceptos/Data/SqlSchema.cs AgrupadorConceptos/Models/ConciliacionInternaSesion.cs AgrupadorConceptos/Models/ConciliacionInternaPar.cs
git commit -m "feat(agrupador): tablas y modelos de conciliacion interna"
```

---

### Task 2: Comparador y servicio

**Files:**
- Modify: `AgrupadorConceptos/Services/ComparadorConciliacion.cs` (sumar `ParsearFecha`, sin tocar los métodos existentes)
- Create: `AgrupadorConceptos/Services/ComparadorConciliacionInterna.cs`
- Create: `AgrupadorConceptos/Services/ConciliacionInternaService.cs`
- Create: `AgrupadorConceptos/Models/ResultadoFinalizacionInterna.cs`

**Interfaces:**
- Consumes: `ComparadorConciliacion.ImporteEfectivo` (ya existe); `MovimientoStorage.ObtenerPorPerfil(int)` (ya existe); `PerfilBancoStorage.ObtenerPorId(int)` (ya existe); `CuentaContableStorage.ObtenerPorId(int)` (del plan de ítems 1-4, `2026-09-05-cuentas-contables.md`, Tarea 1 — **prerrequisito de esta tarea**, ver nota abajo). `Finalizar` actualiza `CuentaFinal` con SQL directo dentro de su propia transacción, no vía `MovimientoStorage.ActualizarCuentaFinal` — así el update de los movimientos y el cambio de `Estado` a `Finalizada` quedan atómicos.
- Produces:
  - `ComparadorConciliacion.ParsearFecha(string fecha)` → `DateTime?` (nuevo, agregado a un archivo existente)
  - `ComparadorConciliacionInterna.ImportesOpuestos(MovimientoProcesado a, MovimientoProcesado b)` → `bool`
  - `ComparadorConciliacionInterna.EstaEnRango(string fecha, DateTime desde, DateTime hasta)` → `bool`
  - `ResultadoFinalizacionInterna { bool Exito; List<string> ParesSinCuenta }`
  - `ConciliacionInternaService` con `ObtenerTodasSesiones`, `CrearSesion`, `EliminarSesion`, `ObtenerPendientesA`, `ObtenerPendientesB`, `ObtenerPares`, `ConciliarPar`, `DesconciliarPar`, `AutoConciliar`, `Finalizar(int idSesion)` → `ResultadoFinalizacionInterna`, `ObtenerSesionEnProcesoDelMovimiento(int idMovimiento)` → `string` (Tarea 5).

**Nota de dependencia entre planes:** `Finalizar` necesita `PerfilBanco.IdCuentaContable` y `CuentaContableStorage.ObtenerPorId`, que agrega el plan `2026-09-05-cuentas-contables.md` (Tareas 1 y 3). Si ese plan todavía no corrió, ejecutar primero sus Tareas 1 y 3 (catálogo de cuentas + cuenta en el perfil) antes de esta tarea — no hace falta el resto de ese plan (concepto estándar, cuenta final, etc.), sólo esas dos piezas del modelo de datos.

- [ ] **Step 1: `ParsearFecha` en el comparador existente**

`ComparadorConciliacion.cs` (compartido con la conciliación externa) ya tiene la lista de formatos de fecha (`FormatosFecha`, privada) dentro de la clase. Se agrega un método público que la reusa, **sin tocar** `FechasIguales` ni `ImportesIguales` — cero riesgo para la conciliación externa. En `AgrupadorConceptos/Services/ComparadorConciliacion.cs`, insertar inmediatamente después del método `FechasIguales`:

```csharp
        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Parseo reutilizable para filtrar por rango
        /// <summary>
        /// Parsea la fecha de un movimiento con los mismos formatos que <see cref="FechasIguales"/>,
        /// o null si no matchea ninguno. La usa la conciliación interna para filtrar por rango.
        /// </summary>
        public static DateTime? ParsearFecha(string fecha)
        {
            if (string.IsNullOrWhiteSpace(fecha)) return null;

            return DateTime.TryParseExact(fecha.Trim(), FormatosFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
                ? d.Date
                : (DateTime?)null;
        }
```

- [ ] **Step 2: Comparador de la conciliación interna**

Crear `AgrupadorConceptos/Services/ComparadorConciliacionInterna.cs` (UTF-8 con BOM):

```csharp
// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Criterio de matching de la conciliacion interna
using System;
using AgrupadorConceptos.Models;

namespace AgrupadorConceptos.Services
{
    /// <summary>
    /// Criterio de igualdad para emparejar dos movimientos de extractos propios distintos:
    /// una transferencia entre cuentas propias aparece como débito en un lado y crédito en
    /// el otro, por el mismo importe. La igualdad de fecha se resuelve con
    /// <see cref="ComparadorConciliacion.FechasIguales"/>, compartida con la conciliación externa.
    /// </summary>
    public static class ComparadorConciliacionInterna
    {
        public static bool ImportesOpuestos(MovimientoProcesado a, MovimientoProcesado b) =>
            ComparadorConciliacion.ImporteEfectivo(a) == ComparadorConciliacion.ImporteEfectivo(b)
            && SignoOpuesto(a, b);

        private static bool SignoOpuesto(MovimientoProcesado a, MovimientoProcesado b)
        {
            bool aEsDebito = a.Debitos != 0;
            bool bEsDebito = b.Debitos != 0;
            return aEsDebito != bEsDebito;
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Filtro por rango para armar cada lado de la sesion
        /// <summary>True si la fecha del movimiento cae dentro del rango, inclusive.</summary>
        public static bool EstaEnRango(string fecha, DateTime desde, DateTime hasta)
        {
            var d = ComparadorConciliacion.ParsearFecha(fecha);
            return d.HasValue && d.Value >= desde.Date && d.Value <= hasta.Date;
        }
    }
}
```

- [ ] **Step 3: Modelo del resultado de `Finalizar`**

Crear `AgrupadorConceptos/Models/ResultadoFinalizacionInterna.cs` (UTF-8 con BOM):

```csharp
// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Resultado de finalizar una sesion interna
using System.Collections.Generic;

namespace AgrupadorConceptos.Models
{
    /// <summary>
    /// Resultado de intentar finalizar una sesión de conciliación interna. Si algún perfil
    /// involucrado no tiene cuenta contable asignada, la finalización se bloquea entera
    /// (no finaliza nada) y acá vienen los pares que la están bloqueando, para mostrarlos.
    /// </summary>
    public class ResultadoFinalizacionInterna
    {
        public bool Exito { get; set; }
        public List<string> ParesSinCuenta { get; } = new List<string>();
    }
}
```

- [ ] **Step 4: Servicio**

Crear `AgrupadorConceptos/Services/ConciliacionInternaService.cs` (UTF-8 con BOM):

```csharp
// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - CRUD y auto-conciliacion entre dos extractos propios
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;
using Dapper;

namespace AgrupadorConceptos.Services
{
    public static class ConciliacionInternaService
    {
        // ── Sesiones ─────────────────────────────────────────────────────────────

        public static List<ConciliacionInternaSesion> ObtenerTodasSesiones()
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            return cn.Query<ConciliacionInternaSesion>(
                "SELECT * FROM bancos.ConciliacionInternaSesiones ORDER BY FechaCreacion DESC").ToList();
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Se elige perfil + rango de fechas, no archivos
        public static ConciliacionInternaSesion CrearSesion(
            string nombre, int idPerfilA, DateTime desdeA, DateTime hastaA,
            int idPerfilB, DateTime desdeB, DateTime hastaB, IEnumerable<string> conceptos)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();

            string conceptosJson = JsonSerializer.Serialize(conceptos.ToList());

            var idSesion = cn.ExecuteScalar<int>(@"
                INSERT INTO bancos.ConciliacionInternaSesiones
                    (Nombre, FechaCreacion, IdPerfilA, FechaDesdeA, FechaHastaA, IdPerfilB, FechaDesdeB, FechaHastaB, ConceptosJson, Estado)
                VALUES (@Nombre, @Fecha, @IdPerfilA, @DesdeA, @HastaA, @IdPerfilB, @DesdeB, @HastaB, @Conceptos, 'EnProceso');
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new { Nombre = nombre, Fecha = DateTime.Now, IdPerfilA = idPerfilA, DesdeA = desdeA.Date, HastaA = hastaA.Date,
                      IdPerfilB = idPerfilB, DesdeB = desdeB.Date, HastaB = hastaB.Date, Conceptos = conceptosJson });

            return cn.QuerySingle<ConciliacionInternaSesion>(
                "SELECT * FROM bancos.ConciliacionInternaSesiones WHERE Id = @Id", new { Id = idSesion });
        }

        public static void EliminarSesion(int idSesion)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            using var tx = cn.BeginTransaction();

            cn.Execute("DELETE FROM bancos.ConciliacionInternaPares WHERE IdSesion = @Id", new { Id = idSesion }, tx);
            cn.Execute("DELETE FROM bancos.ConciliacionInternaSesiones WHERE Id = @Id", new { Id = idSesion }, tx);

            tx.Commit();
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Ya no se llama directo, ver Finalizar
        private static void MarcarFinalizada(int idSesion, IDbConnection cn, IDbTransaction tx)
        {
            cn.Execute("UPDATE bancos.ConciliacionInternaSesiones SET Estado = 'Finalizada' WHERE Id = @Id",
                new { Id = idSesion }, tx);
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Cierre: cuenta del perfil opuesto como contrapartida
        /// <summary>
        /// Finaliza la sesión. Todos los pares de una sesión comparten los mismos dos perfiles
        /// (uno por lado, ver <see cref="CrearSesion"/>), así que la cuenta contrapartida sale
        /// directo de la sesión: cada movimiento del lado A queda con la cuenta del perfil B, y
        /// cada uno del lado B con la del perfil A. Se pisa aunque el usuario la hubiera editado
        /// a mano — es una acción deliberada del cierre, no el autocompletado pasivo que usa el
        /// resto del sistema.
        ///
        /// Si alguno de los dos perfiles no tiene cuenta contable asignada, NO finaliza nada
        /// (ni la sesión ni ningún movimiento).
        /// </summary>
        public static ResultadoFinalizacionInterna Finalizar(int idSesion)
        {
            var resultado = new ResultadoFinalizacionInterna { Exito = true };

            ConciliacionInternaSesion sesion;
            using (var cn0 = DatabaseHelper.Open())
                sesion = cn0.QuerySingle<ConciliacionInternaSesion>(
                    "SELECT * FROM bancos.ConciliacionInternaSesiones WHERE Id = @Id", new { Id = idSesion });

            var perfilA = PerfilBancoStorage.ObtenerPorId(sesion.IdPerfilA);
            var perfilB = PerfilBancoStorage.ObtenerPorId(sesion.IdPerfilB);

            if (perfilA.IdCuentaContable == null || perfilB.IdCuentaContable == null)
            {
                resultado.Exito = false;
                resultado.ParesSinCuenta.Add(
                    $"{perfilA.NombreBanco} ↔ {perfilB.NombreBanco}: falta cuenta contable en el perfil.");
                return resultado;
            }

            string cuentaParaA = CuentaContableStorage.ObtenerPorId(perfilB.IdCuentaContable.Value).Cuenta;
            string cuentaParaB = CuentaContableStorage.ObtenerPorId(perfilA.IdCuentaContable.Value).Cuenta;

            var pares = ObtenerPares(idSesion);

            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            using var tx = cn.BeginTransaction();

            foreach (var par in pares)
            {
                cn.Execute("UPDATE bancos.MovimientosArchivo SET CuentaFinal = @Cuenta WHERE Id = @Id",
                    new { Cuenta = cuentaParaA, Id = par.IdMovimientoA }, tx);
                cn.Execute("UPDATE bancos.MovimientosArchivo SET CuentaFinal = @Cuenta WHERE Id = @Id",
                    new { Cuenta = cuentaParaB, Id = par.IdMovimientoB }, tx);
            }

            MarcarFinalizada(idSesion, cn, tx);

            tx.Commit();
            return resultado;
        }

        // ── Movimientos pendientes de cada lado ──────────────────────────────────

        public static List<MovimientoProcesado> ObtenerPendientesA(int idSesion) =>
            CargarSinConciliar(idSesion).PendientesA;

        public static List<MovimientoProcesado> ObtenerPendientesB(int idSesion) =>
            CargarSinConciliar(idSesion).PendientesB;

        private static (ConciliacionInternaSesion Sesion, List<MovimientoProcesado> PendientesA, List<MovimientoProcesado> PendientesB)
            CargarSinConciliar(int idSesion)
        {
            ConciliacionInternaSesion sesion;
            HashSet<int> conciliadosA, conciliadosB;

            using (var cn = DatabaseHelper.Open())
            {
                sesion = cn.QuerySingle<ConciliacionInternaSesion>(
                    "SELECT * FROM bancos.ConciliacionInternaSesiones WHERE Id = @Id", new { Id = idSesion });

                conciliadosA = cn.Query<int>(
                    "SELECT IdMovimientoA FROM bancos.ConciliacionInternaPares WHERE IdSesion = @Id",
                    new { Id = idSesion }).ToHashSet();
                conciliadosB = cn.Query<int>(
                    "SELECT IdMovimientoB FROM bancos.ConciliacionInternaPares WHERE IdSesion = @Id",
                    new { Id = idSesion }).ToHashSet();
            }

            var conceptos = JsonSerializer.Deserialize<List<string>>(sesion.ConceptosJson) ?? new List<string>();

            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Movimientos del perfil filtrados por rango
            // MovimientosArchivo.Fecha es NVARCHAR (texto tal como lo exporta cada banco), asi que
            // el filtro por rango se hace en memoria con el mismo parseo que ya usa el resaltado
            // de la conciliacion externa (ComparadorConciliacion.ParsearFecha), no en SQL.
            var pendientesA = MovimientoStorage.ObtenerPorPerfil(sesion.IdPerfilA)
                .Where(m => !conciliadosA.Contains(m.Id)
                         && conceptos.Contains(m.ConceptoFinal, StringComparer.OrdinalIgnoreCase)
                         && ComparadorConciliacionInterna.EstaEnRango(m.Fecha, sesion.FechaDesdeA, sesion.FechaHastaA))
                .ToList();

            var pendientesB = MovimientoStorage.ObtenerPorPerfil(sesion.IdPerfilB)
                .Where(m => !conciliadosB.Contains(m.Id)
                         && conceptos.Contains(m.ConceptoFinal, StringComparer.OrdinalIgnoreCase)
                         && ComparadorConciliacionInterna.EstaEnRango(m.Fecha, sesion.FechaDesdeB, sesion.FechaHastaB))
                .ToList();

            return (sesion, pendientesA, pendientesB);
        }

        // ── Pares conciliados ────────────────────────────────────────────────────

        public static List<ConciliacionInternaPar> ObtenerPares(int idSesion)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            return cn.Query<ConciliacionInternaPar>(@"
                SELECT p.*,
                       a.Fecha AS FechaA, CASE WHEN a.Debitos <> 0 THEN a.Debitos ELSE a.Creditos END AS ImporteA, a.ConceptoFinal AS ConceptoFinalA,
                       b.Fecha AS FechaB, CASE WHEN b.Debitos <> 0 THEN b.Debitos ELSE b.Creditos END AS ImporteB, b.ConceptoFinal AS ConceptoFinalB
                FROM bancos.ConciliacionInternaPares p
                JOIN bancos.MovimientosArchivo a ON p.IdMovimientoA = a.Id
                JOIN bancos.MovimientosArchivo b ON p.IdMovimientoB = b.Id
                WHERE p.IdSesion = @Id
                ORDER BY p.FechaConciliacion",
                new { Id = idSesion }).ToList();
        }

        public static void ConciliarPar(int idSesion, int idMovimientoA, int idMovimientoB, TipoMatch tipoMatch)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            cn.Execute(@"
                INSERT INTO bancos.ConciliacionInternaPares (IdSesion, IdMovimientoA, IdMovimientoB, TipoMatch, FechaConciliacion)
                VALUES (@IdSesion, @IdMovimientoA, @IdMovimientoB, @TipoMatch, @Fecha)",
                new { IdSesion = idSesion, IdMovimientoA = idMovimientoA, IdMovimientoB = idMovimientoB,
                      TipoMatch = tipoMatch.ToString(), Fecha = DateTime.Now });
        }

        public static void DesconciliarPar(int idPar)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            cn.Execute("DELETE FROM bancos.ConciliacionInternaPares WHERE Id = @Id", new { Id = idPar });
        }

        // ── Auto-conciliación ────────────────────────────────────────────────────

        /// <summary>
        /// Dos pasadas, igual que la conciliación externa: 1) fecha + importe opuesto exacto,
        /// 2) sólo importe opuesto entre lo que quedó sin conciliar. Los ítems con múltiples
        /// candidatos quedan para resolución manual.
        /// </summary>
        public static (int conciliados, List<(MovimientoProcesado A, List<MovimientoProcesado> Candidatos)> duplicados)
            AutoConciliar(int idSesion)
        {
            var (_, pendienteA, pendienteB) = CargarSinConciliar(idSesion);
            var conciliadosB = new HashSet<int>();
            int total = 0;
            var duplicados = new List<(MovimientoProcesado, List<MovimientoProcesado>)>();

            // ── Pasada 1: Fecha + Importe opuesto ────────────────────────────────
            foreach (var a in pendienteA.ToList())
            {
                var candidatos = pendienteB
                    .Where(b => !conciliadosB.Contains(b.Id)
                             && ComparadorConciliacion.FechasIguales(a.Fecha, b.Fecha)
                             && ComparadorConciliacionInterna.ImportesOpuestos(a, b))
                    .ToList();

                if (candidatos.Count == 1)
                {
                    ConciliarPar(idSesion, a.Id, candidatos[0].Id, TipoMatch.FechaImporte);
                    conciliadosB.Add(candidatos[0].Id);
                    pendienteA.Remove(a);
                    total++;
                }
                else if (candidatos.Count > 1)
                {
                    duplicados.Add((a, candidatos));
                    pendienteA.Remove(a);
                }
            }

            // ── Pasada 2: Sólo importe opuesto ────────────────────────────────────
            var (_, _, pendienteB2Base) = CargarSinConciliar(idSesion);
            var pendienteB2 = pendienteB2Base.Where(b => !conciliadosB.Contains(b.Id)).ToList();

            foreach (var a in pendienteA.ToList())
            {
                var candidatos = pendienteB2
                    .Where(b => !conciliadosB.Contains(b.Id) && ComparadorConciliacionInterna.ImportesOpuestos(a, b))
                    .ToList();

                if (candidatos.Count == 1)
                {
                    ConciliarPar(idSesion, a.Id, candidatos[0].Id, TipoMatch.SoloImporte);
                    conciliadosB.Add(candidatos[0].Id);
                    total++;
                }
                else if (candidatos.Count > 1)
                {
                    duplicados.Add((a, candidatos));
                }
            }

            return (total, duplicados);
        }
    }
}
```

- [ ] **Step 5: Compilar**

```bash
dotnet build ConciliadorContable.slnx -v q --nologo
```

Esperado: `0 Errores`.

- [ ] **Step 6: Commit**

```bash
git add AgrupadorConceptos/Services/ComparadorConciliacion.cs AgrupadorConceptos/Services/ComparadorConciliacionInterna.cs AgrupadorConceptos/Services/ConciliacionInternaService.cs AgrupadorConceptos/Models/ResultadoFinalizacionInterna.cs
git commit -m "feat(agrupador): servicio de conciliacion interna, seleccion por perfil+rango y cierre con contrapartida"
```

---

### Task 3: Diálogo de desempate manual

**Files:**
- Create: `AgrupadorConceptos/SeleccionCandidatoInternoDialog.cs`

**Interfaces:**
- Consumes: `ComparadorConciliacion.ImporteEfectivo` (ya existe).
- Produces: `SeleccionCandidatoInternoDialog(MovimientoProcesado a, List<MovimientoProcesado> candidatosB)` con `MovimientoProcesado MovimientoSeleccionado { get; }`.

- [ ] **Step 1: Diálogo**

Crear `AgrupadorConceptos/SeleccionCandidatoInternoDialog.cs` (UTF-8 con BOM) — mismo patrón hand-coded que `SeleccionCandidatoDialog` (sin `.Designer.cs` aparte), adaptado a que el lado "externo" también es un `MovimientoProcesado`:

```csharp
// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Desempate manual en la conciliacion interna
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;

namespace AgrupadorConceptos
{
    /// <summary>
    /// Cuando hay múltiples movimientos del extracto B que coinciden por importe opuesto
    /// con uno del extracto A, este diálogo permite elegir a cuál asignarlo.
    /// </summary>
    public class SeleccionCandidatoInternoDialog : Form
    {
        private DataGridView dgv;
        private Button btnAsignar;
        private Button btnOmitir;

        public MovimientoProcesado MovimientoSeleccionado { get; private set; }

        public SeleccionCandidatoInternoDialog(MovimientoProcesado a, List<MovimientoProcesado> candidatosB)
        {
            Text            = "Seleccionar movimiento a conciliar";
            ClientSize      = new Size(700, 380);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;

            var lblInfo = new Label
            {
                Text     = $"El movimiento  [{a.Fecha}  ${ComparadorConciliacion.ImporteEfectivo(a):N2}  {a.ConceptoFinal}]  " +
                           $"tiene {candidatosB.Count} candidatos en el otro extracto.\n" +
                           "Seleccione a cuál asignarlo (o Omitir para dejarlo pendiente).",
                Location = new Point(12, 10),
                Size     = new Size(676, 46),
                Font     = new Font("Segoe UI", 9F)
            };

            dgv = new DataGridView
            {
                Location            = new Point(12, 65),
                Size                = new Size(676, 255),
                ReadOnly            = true,
                AllowUserToAddRows  = false,
                SelectionMode       = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                RowHeadersVisible   = false,
                BackgroundColor     = Color.White
            };

            var display = candidatosB.ConvertAll(m => new
            {
                m.Id,
                m.Fecha,
                Importe = ComparadorConciliacion.ImporteEfectivo(m),
                m.ConceptoFinal
            });
            dgv.DataSource = display;
            dgv.CellDoubleClick += (s, e) => AsignarSeleccionado(candidatosB);

            btnAsignar = new Button
            { Text = "Asignar", Location = new Point(510, 333), Size = new Size(85, 28) };
            btnAsignar.Click += (s, e) => AsignarSeleccionado(candidatosB);

            btnOmitir = new Button
            { Text = "Omitir", Location = new Point(603, 333), Size = new Size(85, 28),
              DialogResult = DialogResult.Cancel };

            Controls.AddRange(new Control[] { lblInfo, dgv, btnAsignar, btnOmitir });
            CancelButton = btnOmitir;
        }

        private void AsignarSeleccionado(List<MovimientoProcesado> candidatos)
        {
            if (dgv.SelectedRows.Count == 0) return;
            int id = (int)dgv.SelectedRows[0].Cells["Id"].Value;
            MovimientoSeleccionado = candidatos.Find(m => m.Id == id);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
```

- [ ] **Step 2: Compilar**

```bash
dotnet build ConciliadorContable.slnx -v q --nologo
```

Esperado: `0 Errores`.

- [ ] **Step 3: Commit**

```bash
git add AgrupadorConceptos/SeleccionCandidatoInternoDialog.cs
git commit -m "feat(agrupador): dialogo de desempate manual para conciliacion interna"
```

---

### Task 4: Ventana "Conciliación Interna"

**Files:**
- Create: `AgrupadorConceptos/ConciliacionInternaForm.cs`
- Create: `AgrupadorConceptos/ConciliacionInternaForm.Designer.cs`

**Interfaces:**
- Consumes: todo lo de la Tarea 2 (`ConciliacionInternaService`, `ComparadorConciliacionInterna`) y la Tarea 3 (`SeleccionCandidatoInternoDialog`); `PerfilBancoStorage.ObtenerTodos()`, `MovimientoStorage.ObtenerPorPerfil(int)`, `NombreSesionDialog` (ya existen).
- Produces: `ConciliacionInternaForm()` (sin parámetros).

- [ ] **Step 1: Designer**

Crear `AgrupadorConceptos/ConciliacionInternaForm.Designer.cs` (UTF-8 con BOM):

```csharp
namespace AgrupadorConceptos
{
    partial class ConciliacionInternaForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lbSesiones = new System.Windows.Forms.ListBox();
            btnNuevaSesion = new System.Windows.Forms.Button();
            btnRetomar = new System.Windows.Forms.Button();
            btnEliminarSesion = new System.Windows.Forms.Button();
            lblSesionActiva = new System.Windows.Forms.Label();

            pnlConfigNueva = new System.Windows.Forms.Panel();
            lblExtractoA = new System.Windows.Forms.Label();
            lblPerfilA = new System.Windows.Forms.Label();
            cmbPerfilA = new System.Windows.Forms.ComboBox();
            lblDesdeA = new System.Windows.Forms.Label();
            dtpDesdeA = new System.Windows.Forms.DateTimePicker();
            lblHastaA = new System.Windows.Forms.Label();
            dtpHastaA = new System.Windows.Forms.DateTimePicker();
            lblExtractoB = new System.Windows.Forms.Label();
            lblPerfilB = new System.Windows.Forms.Label();
            cmbPerfilB = new System.Windows.Forms.ComboBox();
            lblDesdeB = new System.Windows.Forms.Label();
            dtpDesdeB = new System.Windows.Forms.DateTimePicker();
            lblHastaB = new System.Windows.Forms.Label();
            dtpHastaB = new System.Windows.Forms.DateTimePicker();
            lblConceptos = new System.Windows.Forms.Label();
            clbConceptos = new System.Windows.Forms.CheckedListBox();
            btnConfirmarNueva = new System.Windows.Forms.Button();
            btnCancelarNueva = new System.Windows.Forms.Button();

            tabControl = new System.Windows.Forms.TabControl();
            tabPendientes = new System.Windows.Forms.TabPage();
            splitPendientes = new System.Windows.Forms.SplitContainer();
            dgvPendienteA = new Telerik.WinControls.UI.RadGridView();
            dgvPendienteB = new Telerik.WinControls.UI.RadGridView();
            tabConciliados = new System.Windows.Forms.TabPage();
            dgvConciliados = new Telerik.WinControls.UI.RadGridView();

            btnAutoConciliar = new System.Windows.Forms.Button();
            btnConciliarManual = new System.Windows.Forms.Button();
            btnDesconciliar = new System.Windows.Forms.Button();
            btnFinalizar = new System.Windows.Forms.Button();
            btnExportar = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)dgvPendienteA).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvPendienteA.MasterTemplate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvPendienteB).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvPendienteB.MasterTemplate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvConciliados).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvConciliados.MasterTemplate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitPendientes).BeginInit();
            splitPendientes.Panel1.SuspendLayout();
            splitPendientes.Panel2.SuspendLayout();
            SuspendLayout();

            // ── Panel de sesiones (izquierda) ────────────────────────────────
            lbSesiones.Location = new System.Drawing.Point(12, 12);
            lbSesiones.Size = new System.Drawing.Size(220, 160);
            lbSesiones.SelectedIndexChanged += lbSesiones_SelectedIndexChanged;

            btnNuevaSesion.Location = new System.Drawing.Point(12, 178);
            btnNuevaSesion.Size = new System.Drawing.Size(70, 26);
            btnNuevaSesion.Text = "Nueva";
            btnNuevaSesion.UseVisualStyleBackColor = true;
            btnNuevaSesion.Click += btnNuevaSesion_Click;

            btnRetomar.Location = new System.Drawing.Point(86, 178);
            btnRetomar.Size = new System.Drawing.Size(70, 26);
            btnRetomar.Text = "Retomar";
            btnRetomar.UseVisualStyleBackColor = true;
            btnRetomar.Click += btnRetomar_Click;

            btnEliminarSesion.Location = new System.Drawing.Point(160, 178);
            btnEliminarSesion.Size = new System.Drawing.Size(72, 26);
            btnEliminarSesion.Text = "Eliminar";
            btnEliminarSesion.UseVisualStyleBackColor = true;
            btnEliminarSesion.Click += btnEliminarSesion_Click;

            lblSesionActiva.AutoSize = true;
            lblSesionActiva.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblSesionActiva.Location = new System.Drawing.Point(246, 16);
            lblSesionActiva.Text = "Sin sesión activa";

            // ── Panel de alta de sesión ───────────────────────────────────────
            pnlConfigNueva.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pnlConfigNueva.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pnlConfigNueva.Location = new System.Drawing.Point(12, 12);
            pnlConfigNueva.Size = new System.Drawing.Size(1160, 560);
            pnlConfigNueva.Visible = false;
            pnlConfigNueva.Controls.Add(lblExtractoA);
            pnlConfigNueva.Controls.Add(lblPerfilA);
            pnlConfigNueva.Controls.Add(cmbPerfilA);
            pnlConfigNueva.Controls.Add(lblDesdeA);
            pnlConfigNueva.Controls.Add(dtpDesdeA);
            pnlConfigNueva.Controls.Add(lblHastaA);
            pnlConfigNueva.Controls.Add(dtpHastaA);
            pnlConfigNueva.Controls.Add(lblExtractoB);
            pnlConfigNueva.Controls.Add(lblPerfilB);
            pnlConfigNueva.Controls.Add(cmbPerfilB);
            pnlConfigNueva.Controls.Add(lblDesdeB);
            pnlConfigNueva.Controls.Add(dtpDesdeB);
            pnlConfigNueva.Controls.Add(lblHastaB);
            pnlConfigNueva.Controls.Add(dtpHastaB);
            pnlConfigNueva.Controls.Add(lblConceptos);
            pnlConfigNueva.Controls.Add(clbConceptos);
            pnlConfigNueva.Controls.Add(btnConfirmarNueva);
            pnlConfigNueva.Controls.Add(btnCancelarNueva);

            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Se elige perfil + rango de fechas por lado
            // No se tildan archivos importados: el usuario elige un perfil de banco y un rango,
            // y la sesion se arma con lo que caiga ahi (ver ConciliacionInternaService.CargarSinConciliar).
            lblExtractoA.AutoSize = true;
            lblExtractoA.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblExtractoA.Location = new System.Drawing.Point(12, 12);
            lblExtractoA.Text = "Extracto A";

            lblPerfilA.AutoSize = true;
            lblPerfilA.Location = new System.Drawing.Point(12, 42);
            lblPerfilA.Text = "Perfil:";

            cmbPerfilA.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbPerfilA.Location = new System.Drawing.Point(70, 39);
            cmbPerfilA.Size = new System.Drawing.Size(300, 23);
            cmbPerfilA.SelectedIndexChanged += cmbPerfil_SelectedIndexChanged;

            lblDesdeA.AutoSize = true;
            lblDesdeA.Location = new System.Drawing.Point(390, 42);
            lblDesdeA.Text = "Desde:";

            dtpDesdeA.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            dtpDesdeA.Location = new System.Drawing.Point(440, 39);
            dtpDesdeA.Size = new System.Drawing.Size(120, 23);
            dtpDesdeA.ValueChanged += dtpRango_ValueChanged;

            lblHastaA.AutoSize = true;
            lblHastaA.Location = new System.Drawing.Point(570, 42);
            lblHastaA.Text = "Hasta:";

            dtpHastaA.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            dtpHastaA.Location = new System.Drawing.Point(620, 39);
            dtpHastaA.Size = new System.Drawing.Size(120, 23);
            dtpHastaA.ValueChanged += dtpRango_ValueChanged;

            lblExtractoB.AutoSize = true;
            lblExtractoB.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblExtractoB.Location = new System.Drawing.Point(12, 80);
            lblExtractoB.Text = "Extracto B";

            lblPerfilB.AutoSize = true;
            lblPerfilB.Location = new System.Drawing.Point(12, 110);
            lblPerfilB.Text = "Perfil:";

            cmbPerfilB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbPerfilB.Location = new System.Drawing.Point(70, 107);
            cmbPerfilB.Size = new System.Drawing.Size(300, 23);
            cmbPerfilB.SelectedIndexChanged += cmbPerfil_SelectedIndexChanged;

            lblDesdeB.AutoSize = true;
            lblDesdeB.Location = new System.Drawing.Point(390, 110);
            lblDesdeB.Text = "Desde:";

            dtpDesdeB.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            dtpDesdeB.Location = new System.Drawing.Point(440, 107);
            dtpDesdeB.Size = new System.Drawing.Size(120, 23);
            dtpDesdeB.ValueChanged += dtpRango_ValueChanged;

            lblHastaB.AutoSize = true;
            lblHastaB.Location = new System.Drawing.Point(570, 110);
            lblHastaB.Text = "Hasta:";

            dtpHastaB.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            dtpHastaB.Location = new System.Drawing.Point(620, 107);
            dtpHastaB.Size = new System.Drawing.Size(120, 23);
            dtpHastaB.ValueChanged += dtpRango_ValueChanged;

            lblConceptos.AutoSize = true;
            lblConceptos.Location = new System.Drawing.Point(12, 148);
            lblConceptos.Text = "Conceptos a conciliar:";

            clbConceptos.CheckOnClick = true;
            clbConceptos.Location = new System.Drawing.Point(12, 168);
            clbConceptos.Size = new System.Drawing.Size(748, 320);

            btnConfirmarNueva.Location = new System.Drawing.Point(636, 500);
            btnConfirmarNueva.Size = new System.Drawing.Size(124, 30);
            btnConfirmarNueva.Text = "Confirmar";
            btnConfirmarNueva.UseVisualStyleBackColor = true;
            btnConfirmarNueva.Click += btnConfirmarNueva_Click;

            btnCancelarNueva.Location = new System.Drawing.Point(504, 500);
            btnCancelarNueva.Size = new System.Drawing.Size(124, 30);
            btnCancelarNueva.Text = "Cancelar";
            btnCancelarNueva.UseVisualStyleBackColor = true;
            btnCancelarNueva.Click += btnCancelarNueva_Click;

            // ── Tabs (pendientes / conciliados) ──────────────────────────────
            tabControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tabControl.Location = new System.Drawing.Point(12, 210);
            tabControl.Size = new System.Drawing.Size(1160, 400);
            tabControl.TabPages.Add(tabPendientes);
            tabControl.TabPages.Add(tabConciliados);

            tabPendientes.Text = "⏳ Pendientes";
            tabPendientes.Controls.Add(splitPendientes);

            splitPendientes.Dock = System.Windows.Forms.DockStyle.Fill;
            splitPendientes.Panel1.Controls.Add(dgvPendienteA);
            splitPendientes.Panel2.Controls.Add(dgvPendienteB);

            dgvPendienteA.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvPendienteA.MasterTemplate.AllowAddNewRow = false;
            dgvPendienteA.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            dgvPendienteA.Name = "dgvPendienteA";
            dgvPendienteA.ReadOnly = true;
            dgvPendienteA.MultiSelect = false;
            dgvPendienteA.SelectionChanged += dgvPendienteA_SelectionChanged;

            dgvPendienteB.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvPendienteB.MasterTemplate.AllowAddNewRow = false;
            dgvPendienteB.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            dgvPendienteB.Name = "dgvPendienteB";
            dgvPendienteB.ReadOnly = true;
            dgvPendienteB.MultiSelect = false;
            dgvPendienteB.RowFormatting += dgvPendienteB_RowFormatting;

            tabConciliados.Text = "✅ Conciliados";
            tabConciliados.Controls.Add(dgvConciliados);

            dgvConciliados.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvConciliados.MasterTemplate.AllowAddNewRow = false;
            dgvConciliados.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            dgvConciliados.Name = "dgvConciliados";
            dgvConciliados.ReadOnly = true;
            dgvConciliados.MultiSelect = false;

            // ── Botonera inferior ─────────────────────────────────────────────
            btnAutoConciliar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnAutoConciliar.Location = new System.Drawing.Point(12, 616);
            btnAutoConciliar.Size = new System.Drawing.Size(140, 30);
            btnAutoConciliar.Text = "Auto-conciliar";
            btnAutoConciliar.UseVisualStyleBackColor = true;
            btnAutoConciliar.Click += btnAutoConciliar_Click;

            btnConciliarManual.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnConciliarManual.Location = new System.Drawing.Point(158, 616);
            btnConciliarManual.Size = new System.Drawing.Size(140, 30);
            btnConciliarManual.Text = "Conciliar manual";
            btnConciliarManual.UseVisualStyleBackColor = true;
            btnConciliarManual.Click += btnConciliarManual_Click;

            btnDesconciliar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnDesconciliar.Location = new System.Drawing.Point(304, 616);
            btnDesconciliar.Size = new System.Drawing.Size(120, 30);
            btnDesconciliar.Text = "Desconciliar";
            btnDesconciliar.UseVisualStyleBackColor = true;
            btnDesconciliar.Click += btnDesconciliar_Click;

            btnFinalizar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnFinalizar.Location = new System.Drawing.Point(940, 616);
            btnFinalizar.Size = new System.Drawing.Size(110, 30);
            btnFinalizar.Text = "Finalizar";
            btnFinalizar.UseVisualStyleBackColor = true;
            btnFinalizar.Click += btnFinalizar_Click;

            btnExportar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnExportar.Location = new System.Drawing.Point(1062, 616);
            btnExportar.Size = new System.Drawing.Size(110, 30);
            btnExportar.Text = "Exportar";
            btnExportar.UseVisualStyleBackColor = true;
            btnExportar.Click += btnExportar_Click;

            // ── ConciliacionInternaForm ───────────────────────────────────────
            ClientSize = new System.Drawing.Size(1184, 660);
            Controls.Add(lbSesiones);
            Controls.Add(btnNuevaSesion);
            Controls.Add(btnRetomar);
            Controls.Add(btnEliminarSesion);
            Controls.Add(lblSesionActiva);
            Controls.Add(tabControl);
            Controls.Add(pnlConfigNueva);
            Controls.Add(btnAutoConciliar);
            Controls.Add(btnConciliarManual);
            Controls.Add(btnDesconciliar);
            Controls.Add(btnFinalizar);
            Controls.Add(btnExportar);
            MinimumSize = new System.Drawing.Size(1000, 500);
            Name = "ConciliacionInternaForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Conciliación Interna — Transferencias entre cuentas propias";

            ((System.ComponentModel.ISupportInitialize)dgvPendienteA.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvPendienteA).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvPendienteB.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvPendienteB).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvConciliados.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvConciliados).EndInit();
            splitPendientes.Panel1.ResumeLayout(false);
            splitPendientes.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitPendientes).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.ListBox lbSesiones;
        private System.Windows.Forms.Button btnNuevaSesion;
        private System.Windows.Forms.Button btnRetomar;
        private System.Windows.Forms.Button btnEliminarSesion;
        private System.Windows.Forms.Label lblSesionActiva;

        private System.Windows.Forms.Panel pnlConfigNueva;
        private System.Windows.Forms.Label lblExtractoA;
        private System.Windows.Forms.Label lblPerfilA;
        private System.Windows.Forms.ComboBox cmbPerfilA;
        private System.Windows.Forms.Label lblDesdeA;
        private System.Windows.Forms.DateTimePicker dtpDesdeA;
        private System.Windows.Forms.Label lblHastaA;
        private System.Windows.Forms.DateTimePicker dtpHastaA;
        private System.Windows.Forms.Label lblExtractoB;
        private System.Windows.Forms.Label lblPerfilB;
        private System.Windows.Forms.ComboBox cmbPerfilB;
        private System.Windows.Forms.Label lblDesdeB;
        private System.Windows.Forms.DateTimePicker dtpDesdeB;
        private System.Windows.Forms.Label lblHastaB;
        private System.Windows.Forms.DateTimePicker dtpHastaB;
        private System.Windows.Forms.Label lblConceptos;
        private System.Windows.Forms.CheckedListBox clbConceptos;
        private System.Windows.Forms.Button btnConfirmarNueva;
        private System.Windows.Forms.Button btnCancelarNueva;

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPendientes;
        private System.Windows.Forms.SplitContainer splitPendientes;
        private Telerik.WinControls.UI.RadGridView dgvPendienteA;
        private Telerik.WinControls.UI.RadGridView dgvPendienteB;
        private System.Windows.Forms.TabPage tabConciliados;
        private Telerik.WinControls.UI.RadGridView dgvConciliados;

        private System.Windows.Forms.Button btnAutoConciliar;
        private System.Windows.Forms.Button btnConciliarManual;
        private System.Windows.Forms.Button btnDesconciliar;
        private System.Windows.Forms.Button btnFinalizar;
        private System.Windows.Forms.Button btnExportar;
    }
}
```

- [ ] **Step 2: Code-behind**

Crear `AgrupadorConceptos/ConciliacionInternaForm.cs` (UTF-8 con BOM):

```csharp
// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Conciliacion interna entre extractos propios
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;
using ClosedXML.Excel;
using Telerik.WinControls.UI;
using Telerik.WinControls;

namespace AgrupadorConceptos
{
    public partial class ConciliacionInternaForm : Form
    {
        private ConciliacionInternaSesion _sesionActiva;

        public ConciliacionInternaForm()
        {
            InitializeComponent();
            Icon = AppIcon.GetIcon();
            Load += OnLoad;
        }

        // ── Carga inicial ─────────────────────────────────────────────────────────

        private void OnLoad(object sender, EventArgs e)
        {
            CargarSesiones();
            ActualizarEstadoSesion();
        }

        private void CargarSesiones()
        {
            var sesiones = ConciliacionInternaService.ObtenerTodasSesiones();
            lbSesiones.DataSource     = sesiones;
            lbSesiones.DisplayMember  = "DisplayName";
            lbSesiones.ValueMember    = "Id";
            btnRetomar.Enabled        = sesiones.Count > 0;
            btnEliminarSesion.Enabled = sesiones.Count > 0;
            lbSesiones.SelectedIndex  = sesiones.Count > 0 ? 0 : -1;
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Perfiles y rango por defecto del panel de alta
        private void CargarPerfilesEnPanel()
        {
            var perfiles = PerfilBancoStorage.ObtenerTodos();

            cmbPerfilA.DataSource = perfiles;
            cmbPerfilA.DisplayMember = "NombreBanco";
            cmbPerfilA.ValueMember = "Id";
            cmbPerfilA.SelectedIndex = -1;

            cmbPerfilB.DataSource = perfiles.ToList(); // lista aparte: no comparten SelectedItem
            cmbPerfilB.DisplayMember = "NombreBanco";
            cmbPerfilB.ValueMember = "Id";
            cmbPerfilB.SelectedIndex = -1;

            dtpDesdeA.Value = dtpDesdeB.Value = DateTime.Today.AddMonths(-1);
            dtpHastaA.Value = dtpHastaB.Value = DateTime.Today;

            clbConceptos.Items.Clear();
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Conceptos disponibles segun perfil + rango elegidos
        private void CargarConceptosPorRango()
        {
            clbConceptos.Items.Clear();

            if (cmbPerfilA.SelectedValue is not int idPerfilA || cmbPerfilB.SelectedValue is not int idPerfilB)
                return;

            var conceptosA = MovimientoStorage.ObtenerPorPerfil(idPerfilA)
                .Where(m => ComparadorConciliacionInterna.EstaEnRango(m.Fecha, dtpDesdeA.Value, dtpHastaA.Value))
                .Select(m => m.ConceptoFinal);

            var conceptosB = MovimientoStorage.ObtenerPorPerfil(idPerfilB)
                .Where(m => ComparadorConciliacionInterna.EstaEnRango(m.Fecha, dtpDesdeB.Value, dtpHastaB.Value))
                .Select(m => m.ConceptoFinal);

            foreach (var c in conceptosA.Concat(conceptosB)
                         .Where(c => !string.IsNullOrWhiteSpace(c))
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .OrderBy(c => c))
                clbConceptos.Items.Add(c, false);
        }

        // ── Selección en la lista de sesiones ────────────────────────────────────

        private void lbSesiones_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool haySel = lbSesiones.SelectedItem != null;
            btnRetomar.Enabled        = haySel;
            btnEliminarSesion.Enabled = haySel;
        }

        // ── Botones de sesión ─────────────────────────────────────────────────────

        private void btnNuevaSesion_Click(object sender, EventArgs e)
        {
            CargarPerfilesEnPanel();
            pnlConfigNueva.Visible = true;
            btnNuevaSesion.Enabled = false;
        }

        private void cmbPerfil_SelectedIndexChanged(object sender, EventArgs e) => CargarConceptosPorRango();

        private void dtpRango_ValueChanged(object sender, EventArgs e) => CargarConceptosPorRango();

        private void btnConfirmarNueva_Click(object sender, EventArgs e)
        {
            if (cmbPerfilA.SelectedValue is not int idPerfilA || cmbPerfilB.SelectedValue is not int idPerfilB)
            { MessageBox.Show("Elija el perfil de cada extracto.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (dtpDesdeA.Value.Date > dtpHastaA.Value.Date || dtpDesdeB.Value.Date > dtpHastaB.Value.Date)
            { MessageBox.Show("La fecha 'Desde' no puede ser posterior a 'Hasta'.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var conceptos = clbConceptos.CheckedItems.Cast<string>().ToList();
            if (conceptos.Count == 0)
            { MessageBox.Show("Seleccione al menos un concepto.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var perfilA = (PerfilBanco)cmbPerfilA.SelectedItem;
            var perfilB = (PerfilBanco)cmbPerfilB.SelectedItem;
            string nombreSugerido = $"{perfilA.NombreBanco} ↔ {perfilB.NombreBanco} - {DateTime.Now:dd/MM HH:mm}";
            using var dlgNombre = new NombreSesionDialog(nombreSugerido);
            if (dlgNombre.ShowDialog(this) != DialogResult.OK) return;

            _sesionActiva = ConciliacionInternaService.CrearSesion(
                dlgNombre.NombreSesion, idPerfilA, dtpDesdeA.Value, dtpHastaA.Value,
                idPerfilB, dtpDesdeB.Value, dtpHastaB.Value, conceptos);

            pnlConfigNueva.Visible = false;
            btnNuevaSesion.Enabled = true;
            CargarSesiones();
            ActualizarEstadoSesion();
            RefrescarGrillas();
        }

        private void btnCancelarNueva_Click(object sender, EventArgs e)
        {
            pnlConfigNueva.Visible = false;
            btnNuevaSesion.Enabled = true;
        }

        private void btnRetomar_Click(object sender, EventArgs e)
        {
            if (lbSesiones.SelectedItem is not ConciliacionInternaSesion sesion) return;
            _sesionActiva = sesion;
            ActualizarEstadoSesion();
            RefrescarGrillas();
        }

        private void btnEliminarSesion_Click(object sender, EventArgs e)
        {
            if (lbSesiones.SelectedItem is not ConciliacionInternaSesion sesion) return;

            if (MessageBox.Show($"¿Eliminar la sesión '{sesion.Nombre}' y todos sus datos?",
                    "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            ConciliacionInternaService.EliminarSesion(sesion.Id);
            if (_sesionActiva?.Id == sesion.Id) { _sesionActiva = null; LimpiarGrillas(); }
            CargarSesiones();
            ActualizarEstadoSesion();
        }

        // ── Auto-conciliación ─────────────────────────────────────────────────────

        private void btnAutoConciliar_Click(object sender, EventArgs e)
        {
            if (_sesionActiva == null) { MessageBox.Show("No hay sesión activa.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var (conciliados, duplicados) = ConciliacionInternaService.AutoConciliar(_sesionActiva.Id);

            foreach (var (a, candidatos) in duplicados)
            {
                using var dlg = new SeleccionCandidatoInternoDialog(a, candidatos);
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.MovimientoSeleccionado != null)
                {
                    ConciliacionInternaService.ConciliarPar(
                        _sesionActiva.Id, a.Id, dlg.MovimientoSeleccionado.Id, TipoMatch.SoloImporte);
                    conciliados++;
                }
            }

            RefrescarGrillas();
            MessageBox.Show($"Auto-conciliación completada: {conciliados} par(es) conciliado(s).",
                "Resultado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Conciliación manual ───────────────────────────────────────────────────

        private MovimientoProcesado _movimientoASeleccionado;

        private void dgvPendienteA_SelectionChanged(object sender, EventArgs e)
        {
            _movimientoASeleccionado = dgvPendienteA.CurrentRow?.DataBoundItem as MovimientoProcesado;
            dgvPendienteB.TableElement.BeginUpdate();
            dgvPendienteB.TableElement.EndUpdate();
        }

        private void dgvPendienteB_RowFormatting(object sender, RowFormattingEventArgs e)
        {
            if (_movimientoASeleccionado == null || e.RowElement.RowInfo.DataBoundItem is not MovimientoProcesado b)
            {
                e.RowElement.ResetValue(LightVisualElement.BackColorProperty, ValueResetFlags.Local);
                return;
            }

            bool fechaMatch   = ComparadorConciliacion.FechasIguales(_movimientoASeleccionado.Fecha, b.Fecha);
            bool importeMatch = ComparadorConciliacionInterna.ImportesOpuestos(_movimientoASeleccionado, b);

            if (fechaMatch && importeMatch) e.RowElement.BackColor = Color.LightGreen;
            else if (importeMatch) e.RowElement.BackColor = Color.LightYellow;
            else e.RowElement.ResetValue(LightVisualElement.BackColorProperty, ValueResetFlags.Local);
        }

        private void btnConciliarManual_Click(object sender, EventArgs e)
        {
            if (_sesionActiva == null || _movimientoASeleccionado == null)
            { MessageBox.Show("Seleccione un movimiento del extracto A.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (dgvPendienteB.CurrentRow?.DataBoundItem is not MovimientoProcesado b)
            { MessageBox.Show("Seleccione un movimiento del extracto B.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            ConciliacionInternaService.ConciliarPar(_sesionActiva.Id, _movimientoASeleccionado.Id, b.Id, TipoMatch.Manual);

            _movimientoASeleccionado = null;
            RefrescarGrillas();
        }

        private void btnDesconciliar_Click(object sender, EventArgs e)
        {
            if (dgvConciliados.CurrentRow?.DataBoundItem is not ConciliacionInternaPar par) return;

            ConciliacionInternaService.DesconciliarPar(par.Id);
            RefrescarGrillas();
        }

        private void btnFinalizar_Click(object sender, EventArgs e)
        {
            if (_sesionActiva == null) return;
            var pendA = ConciliacionInternaService.ObtenerPendientesA(_sesionActiva.Id);
            if (pendA.Count > 0 &&
                MessageBox.Show($"Aún quedan {pendA.Count} movimientos sin conciliar. ¿Finalizar igual?",
                    "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Cierre pisa CuentaFinal con la del perfil opuesto
            var resultado = ConciliacionInternaService.Finalizar(_sesionActiva.Id);
            if (!resultado.Exito)
            {
                MessageBox.Show(
                    "No se puede finalizar: hay pares cuyo perfil no tiene cuenta contable asignada. " +
                    "Asignale una cuenta a esos perfiles (Agrupador → Procesador → Editar Perfil) y reintentá.\n\n" +
                    string.Join("\n", resultado.ParesSinCuenta),
                    "Faltan cuentas contables", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _sesionActiva.Estado = "Finalizada";
            ActualizarEstadoSesion();
            CargarSesiones();
            RefrescarGrillas(); // Cuenta Final quedo actualizada en los pares conciliados
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            if (_sesionActiva == null) return;

            using var dlg = new SaveFileDialog
            { Filter = "Excel|*.xlsx", FileName = $"ConciliacionInterna_{DateTime.Now:yyyyMMdd_HHmm}.xlsx" };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            var pares = ConciliacionInternaService.ObtenerPares(_sesionActiva.Id);
            var pendA = ConciliacionInternaService.ObtenerPendientesA(_sesionActiva.Id);
            var pendB = ConciliacionInternaService.ObtenerPendientesB(_sesionActiva.Id);

            using var wb = new XLWorkbook();
            var wsCon = wb.Worksheets.Add("Conciliados");
            wsCon.Cell(1,1).Value = "Fecha A"; wsCon.Cell(1,2).Value = "Importe A"; wsCon.Cell(1,3).Value = "Concepto A";
            wsCon.Cell(1,4).Value = "Fecha B"; wsCon.Cell(1,5).Value = "Importe B"; wsCon.Cell(1,6).Value = "Concepto B";
            wsCon.Cell(1,7).Value = "Tipo Match";
            for (int i = 0; i < pares.Count; i++)
            {
                var p = pares[i]; int r = i + 2;
                wsCon.Cell(r,1).Value = p.FechaA; wsCon.Cell(r,2).Value = (double)p.ImporteA; wsCon.Cell(r,3).Value = p.ConceptoFinalA;
                wsCon.Cell(r,4).Value = p.FechaB; wsCon.Cell(r,5).Value = (double)p.ImporteB; wsCon.Cell(r,6).Value = p.ConceptoFinalB;
                wsCon.Cell(r,7).Value = p.TipoMatch;
            }

            var wsPend = wb.Worksheets.Add("Pendientes");
            wsPend.Cell(1,1).Value = "Extracto"; wsPend.Cell(1,2).Value = "Fecha";
            wsPend.Cell(1,3).Value = "Importe"; wsPend.Cell(1,4).Value = "Concepto";
            int rp = 2;
            foreach (var m in pendA)
            { wsPend.Cell(rp,1).Value="A"; wsPend.Cell(rp,2).Value=m.Fecha; wsPend.Cell(rp,3).Value=(double)ComparadorConciliacion.ImporteEfectivo(m); wsPend.Cell(rp,4).Value=m.ConceptoFinal; rp++; }
            foreach (var m in pendB)
            { wsPend.Cell(rp,1).Value="B"; wsPend.Cell(rp,2).Value=m.Fecha; wsPend.Cell(rp,3).Value=(double)ComparadorConciliacion.ImporteEfectivo(m); wsPend.Cell(rp,4).Value=m.ConceptoFinal; rp++; }

            wb.SaveAs(dlg.FileName);
            MessageBox.Show("Exportado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Helpers de UI ─────────────────────────────────────────────────────────

        private void RefrescarGrillas()
        {
            if (_sesionActiva == null) { LimpiarGrillas(); return; }

            dgvPendienteA.DataSource = ConciliacionInternaService.ObtenerPendientesA(_sesionActiva.Id);
            dgvPendienteA.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;

            dgvPendienteB.DataSource = ConciliacionInternaService.ObtenerPendientesB(_sesionActiva.Id);
            dgvPendienteB.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;

            dgvConciliados.DataSource = ConciliacionInternaService.ObtenerPares(_sesionActiva.Id);
            dgvConciliados.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;
        }

        private void LimpiarGrillas()
        {
            dgvPendienteA.DataSource = null;
            dgvPendienteB.DataSource = null;
            dgvConciliados.DataSource = null;
        }

        private void ActualizarEstadoSesion()
        {
            bool activa = _sesionActiva != null;
            lblSesionActiva.Text = activa ? $"Sesión: {_sesionActiva.Nombre} [{_sesionActiva.Estado}]" : "Sin sesión activa";
            btnAutoConciliar.Enabled   = activa;
            btnConciliarManual.Enabled = activa;
            btnDesconciliar.Enabled    = activa;
            btnFinalizar.Enabled       = activa;
            btnExportar.Enabled        = activa;
        }
    }
}
```

- [ ] **Step 3: Compilar**

```bash
dotnet build ConciliadorContable.slnx -v q --nologo
```

Esperado: `0 Errores`. Si `SplitContainer` no expone `.Panel1`/`.Panel2` como se espera en el Designer (el control ya viene con esos paneles creados por el framework, no hace falta instanciarlos), ajustar sólo esa parte sin tocar el resto.

- [ ] **Step 4: Commit**

```bash
git add AgrupadorConceptos/ConciliacionInternaForm.cs AgrupadorConceptos/ConciliacionInternaForm.Designer.cs
git commit -m "feat(agrupador): ventana de conciliacion interna entre extractos"
```

---

### Task 5: No dejar editar Cuenta Final mientras está conciliada (interna) en proceso

Si un movimiento ya está conciliado en una sesión interna que sigue "EnProceso", su `CuentaFinal`
se va a pisar sola cuando esa sesión se cierre (Tarea 2, `Finalizar`) — editarla a mano ahora sería
trabajo tirado. Hay que avisarlo y no dejar editar, en vez de dejar editar y que se pierda solo.

**Prerrequisito:** esta tarea modifica la grilla del Procesador (`ProcesadorForm`), que ya tiene que
tener la columna `CuentaFinal` editable — eso lo agrega la Tarea 7 del plan de ítems 1-4
(`docs/superpowers/plans/2026-09-05-cuentas-contables.md`). Si ese plan no corrió todavía, ejecutar
esa tarea antes de esta.

**Files:**
- Modify: `AgrupadorConceptos/Services/ConciliacionInternaService.cs`
- Modify: `AgrupadorConceptos/ProcesadorForm.cs`

**Interfaces:**
- Consumes: nada nuevo de este plan; depende de `MovimientoProcesado.CuentaFinal` y `ProcesadorForm.ConfigurarGrilla`/`DgvDatos_CellValueChanged` (plan de ítems 1-4, Tarea 7).
- Produces: `ConciliacionInternaService.ObtenerSesionEnProcesoDelMovimiento(int idMovimiento)` → `string` (nombre de la sesión que bloquea, o `null`).

- [ ] **Step 1: Consulta en el servicio**

En `AgrupadorConceptos/Services/ConciliacionInternaService.cs`, agregar el método al final de la clase (antes de la última llave de cierre):

```csharp
        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Bloquear edicion manual mientras este conciliado
        /// <summary>
        /// Nombre de la sesión de conciliación interna, todavía "EnProceso", que tiene a este
        /// movimiento conciliado — o null si no hay ninguna. Lo usa la grilla del Procesador
        /// para impedir editar CuentaFinal a mano: se va a pisar sola cuando esa sesión cierre.
        /// </summary>
        public static string ObtenerSesionEnProcesoDelMovimiento(int idMovimiento)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            return cn.QueryFirstOrDefault<string>(@"
                SELECT TOP 1 s.Nombre
                FROM bancos.ConciliacionInternaPares p
                JOIN bancos.ConciliacionInternaSesiones s ON p.IdSesion = s.Id
                WHERE (p.IdMovimientoA = @Id OR p.IdMovimientoB = @Id)
                  AND s.Estado = 'EnProceso'",
                new { Id = idMovimiento });
        }
```

- [ ] **Step 2: Cancelar la edición en la grilla**

En `AgrupadorConceptos/ProcesadorForm.cs`, agregar la suscripción en el constructor, junto a las otras dos de `dgvDatos`:

```csharp
            this.dgvDatos.CellDoubleClick += DgvDatos_CellDoubleClick;
            this.dgvDatos.CellValueChanged += DgvDatos_CellValueChanged;
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - No dejar editar si esta conciliada en proceso
            this.dgvDatos.CellBeginEdit += DgvDatos_CellBeginEdit;
```

Y agregar el handler, inmediatamente después de `DgvDatos_CellValueChanged`. `ProcesadorForm.cs` ya
tiene `using AgrupadorConceptos.Services;` y `using Telerik.WinControls.UI;` (son los que usa para
`SesionMovimientosService` y para el resto de los tipos de grilla), así que no hace falta agregar
ningún `using` nuevo:

```csharp
        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Cuenta final conciliada: no se edita a mano
        // Se pisaria igual cuando la conciliacion interna cierre (ver ConciliacionInternaService.Finalizar
        // en el plan de conciliacion interna) — mejor avisar antes que dejar editar algo que se va a perder.
        private void DgvDatos_CellBeginEdit(object sender, GridViewCellCancelEventArgs e)
        {
            if (e.Column.Name != "CuentaFinal") return;
            if (e.Row.DataBoundItem is not MovimientoProcesado mov) return;

            string sesion = ConciliacionInternaService.ObtenerSesionEnProcesoDelMovimiento(mov.Id);
            if (sesion == null) return;

            e.Cancel = true;
            MessageBox.Show(
                $"Este movimiento está conciliado en la sesión interna '{sesion}', todavía en proceso.\n" +
                "La Cuenta Final se va a fijar sola cuando esa conciliación se cierre, así que no se puede editar a mano ahora.",
                "No se puede editar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
```

- [ ] **Step 3: Compilar**

```bash
dotnet build ConciliadorContable.slnx -v q --nologo
```

Esperado: `0 Errores`.

- [ ] **Step 4: Commit**

```bash
git add AgrupadorConceptos/Services/ConciliacionInternaService.cs AgrupadorConceptos/ProcesadorForm.cs
git commit -m "feat(agrupador): no dejar editar Cuenta Final si esta conciliada por la interna en proceso"
```

---

### Task 6: Entrada de menú y permiso

**Files:**
- Modify: `ConciliadorContable/Models/Usuario.cs`
- Modify: `ConciliadorContable/Forms/FormMenuPrincipal.cs`
- Modify: `ConciliadorContable/Forms/FormMenuPrincipal.Designer.cs`

**Interfaces:**
- Consumes: `ConciliacionInternaForm` (Tarea 4).
- Produces: nada nuevo, sólo wiring de UI.

- [ ] **Step 1: Permiso**

En `ConciliadorContable/Models/Usuario.cs`. Si el plan `2026-09-05-cuentas-contables.md` (ítems 1-4) ya corrió antes, `TodosLosModulos` ya tiene `AgrCuentasContables` — sumar `AgrConciliacionInterna` a continuación; si este plan corre primero, sumarlo igual, sin `AgrCuentasContables`:

```csharp
        public static readonly string[] TodosLosModulos =
        {
            "ArcaOffline", "ArcaPerfiles", "ArcaEquivalencias",
            "AgrProcesador", "AgrHomologaciones", "AgrConciliacion",
            "AgrCuentasContables", // presente sólo si el plan de items 1-4 ya corrió
            "AgrConciliacionInterna"
        };
```

```csharp
            ["AgrConciliacionInterna"] = "Agrupador - Conciliación Interna",
```

- [ ] **Step 2: Botón de menú**

Igual que en la Tarea 2 del plan de ítems 1-4: si ese plan ya corrió, el panel `pnlAgrupador` ya mide `340x430` con 4 filas — sumar la quinta en `y=340`. Si corre antes, agrandar el panel a `340x430` (una sola fila extra) siguiendo el mismo criterio: `ClientSize`/`MinimumSize` del form a `Size(780, 560)`, `pnlArca`/`pnlAgrupador` a `Size(340, 430)`.

Declarar:
```csharp
            btnAgrConciliacionInterna = new RadButton();
```

Configurar (después de la fila de `btnAgrCuentasContables` si existe, o después de `btnAgrConciliacion` si no):
```csharp
            ConfigurarBotonModulo(btnAgrConciliacionInterna, "Conciliación Interna", new System.Drawing.Point(16, 340), BtnAgrupadorConciliacionInterna_Click);
```

Sumarlo a `pnlAgrupador.Controls.AddRange` y a los `BeginInit`/`EndInit`, y declarar el campo:
```csharp
        private RadButton btnAgrConciliacionInterna;
```

En `ConciliadorContable/Forms/FormMenuPrincipal.cs`, agregar el handler:

```csharp
        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Abrir la conciliacion interna
        private void BtnAgrupadorConciliacionInterna_Click(object sender, EventArgs e)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            AgrupadorConceptos.Data.DatabaseHelper.InitializeDatabase();
            AbrirVentana(new AgrupadorConceptos.ConciliacionInternaForm());
        }
```

Y en `AplicarPermisos`:

```csharp
            btnAgrConciliacionInterna.Enabled = u.TienePermiso("AgrConciliacionInterna");
```

- [ ] **Step 3: Compilar**

```bash
dotnet build ConciliadorContable.slnx -v q --nologo
```

Esperado: `0 Errores`.

- [ ] **Step 4: Commit**

```bash
git add ConciliadorContable/Models/Usuario.cs ConciliadorContable/Forms/FormMenuPrincipal.cs ConciliadorContable/Forms/FormMenuPrincipal.Designer.cs
git commit -m "feat(agrupador): entrada de menu y permiso para conciliacion interna"
```

---

### Task 7: Bitácora

- [ ] **Step 1: Verificación manual end-to-end**

Contra una base real, con dos extractos de bancos distintos que compartan al menos dos transferencias:

1. Crear una sesión eligiendo el perfil y el rango de fechas de ambos lados (no archivos puntuales) y
   un concepto (ej. "Transferencia enviada"/"Transferencia recibida" si están homologados igual, o dos
   conceptos distintos uno por lado). Cambiar el rango o el perfil y verificar que la lista de
   conceptos disponibles se recalcula sola.
2. Auto-conciliar: verificar que las transferencias con misma fecha e importe opuesto quedan conciliadas (`TipoMatch = FechaImporte`).
3. Provocar un duplicado (dos movimientos del lado B con el mismo importe opuesto a uno del lado A, fechas distintas): verificar que aparece el diálogo de desempate y que asignar/omitir funciona.
4. Conciliar manualmente un par seleccionando una fila en cada grilla: verificar el resaltado verde/amarillo en el extracto B según haya fecha+importe o sólo importe.
5. **Con un par ya conciliado (sesión todavía "EnProceso"), intentar editar a mano la `CuentaFinal` de
   ese movimiento en la grilla del Procesador:** la edición se cancela y aparece el aviso de que está
   conciliado en esa sesión. Desconciliar el par y reintentar: ahora sí deja editar.
6. Desconciliar un par ya conciliado: verifica que vuelve a aparecer en "Pendientes" de ambos lados.
7. **Finalizar con ambos perfiles sin cuenta contable asignada:** confirma la advertencia de pendientes,
   pero el cierre se bloquea con el mensaje de "Faltan cuentas contables" listando los pares afectados;
   la sesión sigue "EnProceso" y ningún movimiento cambió su `CuentaFinal`.
8. **Asignar cuenta contable a ambos perfiles** (perfil A y perfil B, desde Editar Perfil) y finalizar de
   nuevo: la sesión pasa a "Finalizada" y, en la pestaña Conciliados, cada movimiento A quedó con la
   `CuentaFinal` del perfil de B, y cada movimiento B con la del perfil de A — verificar también en la
   grilla del Procesador de cada extracto.
9. Repetir el paso 8 sobre un movimiento cuya `CuentaFinal` ya estaba editada a mano con otro valor:
   verificar que el cierre la pisa igual (no la respeta).
10. **Después de finalizada la sesión**, verificar que la `CuentaFinal` de esos movimientos vuelve a
    ser editable a mano en el Procesador (ya no hay una sesión "EnProceso" que la vaya a pisar).
11. Exportar a Excel y verificar las dos hojas (Conciliados/Pendientes).
12. Verificar que ninguna operación de esta pantalla tocó `bancos.ConciliacionSesiones`/`ConciliacionItemsExternos`/`ConciliacionPares` (conciliación externa intacta).

- [ ] **Step 2: `~/.claude/TAREAS.md`**

Si la fila `00021` ya existe (por el plan de ítems 1-4), **editarla** para sumar el ítem 5 a la columna "Ítems" y una frase al final de la descripción, en vez de agregar una fila nueva. Si no existe todavía, crearla:

```
| 00021 | 05/09/2026 | ConciliadorContable | 5 | Ventana nueva de conciliación interna entre extractos propios (transferencias entre cuentas propias: mismo importe, débito en un extracto y crédito en el otro), seleccionando perfil + rango de fechas por lado en vez de archivos puntuales, con auto-conciliación por fecha+importe opuesto en dos pasadas y conciliación manual con resaltado de candidatos — mismo formato que la conciliación con archivo externo, tablas nuevas y paralelas. Al cerrar la sesión, cada movimiento recibe como Cuenta Final la cuenta contable del perfil del otro lado (bloqueando el cierre si falta alguna); mientras está conciliado y la sesión sigue en proceso, esa Cuenta Final no se puede editar a mano. Detalle en `D:\Sistemas\ConciliadorContable\docs\Historial.md`. |
```

Actualizar `<!-- Próxima tarea: NNNNN -->` sólo si este plan corre antes que el de ítems 1-4 (si ya se actualizó a `00022`, no tocarlo de nuevo).

- [ ] **Step 3: `docs/Historial.md`**

Agregar al final (o sumar una sección dentro de la entrada de TAREA 00021 si el otro plan ya la creó):

```markdown
### Ítem 5 — Conciliación interna entre extractos

**Pedido:**

> 5. Creación de ventana para conciliación interna de un concepto entre extractos, las
>    mediciones son por fecha + importe automáticos y manual. Copiar el formato de
>    conciliación con archivo externo.

**Ejecutado:**

- `ConciliacionInternaForm`, clon del esqueleto de `ConciliacionExternForm`: sesiones, panel de alta
  (elige **perfil + rango de fechas** de cada extracto, no archivos puntuales, y los conceptos a
  conciliar), pestañas Pendientes/Conciliados, auto-conciliación en dos pasadas (fecha+importe
  opuesto, después sólo importe opuesto), conciliación manual con resaltado de candidatos,
  desconciliar, finalizar, exportar a Excel.
- Matching por importe opuesto (uno débito, el otro crédito): `ComparadorConciliacionInterna`.
  `ComparadorConciliacion` suma `ParsearFecha`, reutilizado para filtrar los movimientos de cada
  perfil por el rango elegido (`MovimientosArchivo.Fecha` es texto, no hay filtro de rango en SQL).
- Al finalizar la sesión, cada par conciliado recibe la cuenta contable del perfil del otro lado
  como contrapartida (A queda con la cuenta del perfil de B y viceversa), pisando incluso una
  `CuentaFinal` editada a mano; si algún perfil no tiene cuenta asignada, se bloquea el cierre de
  toda la sesión hasta que se complete.
- Mientras un movimiento está conciliado en una sesión interna "EnProceso", su `CuentaFinal` no se
  puede editar a mano en la grilla del Procesador — se avisa que se va a fijar sola al cerrar la
  conciliación. Deja de estar bloqueada si se desconcilia o si la sesión ya se finalizó.
- Persistencia en tablas nuevas y paralelas (`ConciliacionInternaSesiones` /
  `ConciliacionInternaPares`) — no se tocó el código de conciliación con archivo externo.
- Diseño completo en `docs/superpowers/specs/2026-09-05-cuentas-contables-perfil-concepto-conciliacion-interna-design.md`.
```

- [ ] **Step 4: Commit**

```bash
git add docs/Historial.md
git commit -m "docs: TAREA 00021 (item 5) en la bitacora"
```
