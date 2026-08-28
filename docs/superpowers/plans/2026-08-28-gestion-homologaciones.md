# Gestión de homologaciones — Plan de implementación (TAREA 00003)

> **Para agentes:** SUB-SKILL REQUERIDA: usar `superpowers:subagent-driven-development` (recomendado) o `superpowers:executing-plans` para ejecutar tarea por tarea. Los pasos usan checkbox (`- [ ]`) para seguimiento.

**Goal:** Que la pantalla de gestión de homologaciones deje de ser de consulta: que la baja decida qué pasa con los movimientos ya homologados, que se pueda dar de alta y editar reglas, y que se agrupe y filtre por perfil.

**Architecture:** El cálculo de qué movimientos cubre cada regla lo hace el matcher (clave ganadora), no una comparación de textos. La decisión y la propagación viven en `Services/HomologacionAdminService`; la escritura atómica (regla + movimientos en una transacción) en `Data/HomologacionStorage`; los forms sólo arman UI. Ver el diseño en `docs/superpowers/specs/2026-08-28-gestion-homologaciones-design.md`.

**Tech Stack:** .NET 8 (`net8.0-windows`), WinForms, Telerik UI for WinForms 2024.4.1113 (`RadGridView`), Dapper 2.1.35 sobre SQL Server (schema `bancos`).

## Global Constraints

- **Sin proyecto de tests.** Decisión explícita del usuario. La verificación de cada tarea es `dotnet build` limpio; la verificación funcional es el checklist manual de la Tarea 9, contra una base real.
- **Comentario de versionado en cada bloque agregado o modificado**, según `~/.claude/CLAUDE.md`:
  `// Fecha: 28/08/2026 - TAREA: 00003 - Linea: N - Descripción`
  donde `N` es el ítem del pedido. En archivo nuevo va arriba de todo; en edición, inmediatamente encima del bloque.
- **Ítems del pedido:** 1 baja con decisión sobre los ya homologados · 2 alta y edición desde la pantalla · 3 agrupación por perfil · 4 filtro por el perfil que la invoca.
- **Comentarios y textos de UI en español.** Los comentarios explican *por qué*, no qué hace la línea.
- **SQL siempre parametrizado** (`@Param`), nunca interpolado. Operaciones de varias sentencias en transacción explícita.
- **`ConceptoFinal` editado a mano nunca se pisa.** Sólo se actualiza si estaba pendiente o si venía igual al `ConceptoEstandar` viejo.
- **Comparaciones de conceptos y claves con `StringComparison.OrdinalIgnoreCase`.** El diccionario del matcher es `OrdinalIgnoreCase`, así que la clave que devuelve puede diferir en mayúsculas de la almacenada.
- **Encoding:** los archivos nuevos y los `.Designer.cs` que se reescriben van en **UTF-8 con BOM**. Los `.Designer.cs` actuales están en cp1252 sin BOM, así que sus acentos hoy compilan corruptos.
- Comando de build único para todas las tareas:
  ```bash
  dotnet build ConciliadorContable.slnx -v q --nologo
  ```
  Esperado: `0 Errores`. Las ~41 advertencias `CS8632`/DPI son preexistentes y no se tocan.

---

## Estructura de archivos

| Archivo | Responsabilidad | Tarea |
|---|---|---|
| `AgrupadorConceptos/Services/HomologacionMatcher.cs` | *Modificar.* Suma `ResolverClave`: qué regla gana, no sólo con qué concepto. | 1 |
| `AgrupadorConceptos/Models/HomologacionListado.cs` | *Modificar.* Suma los ids y el conteo de uso. | 2 |
| `AgrupadorConceptos/Data/HomologacionStorage.cs` | *Modificar.* Lecturas con filtro/exclusión y las dos escrituras transaccionales; se borra `Eliminar`, que queda sin uso. | 2, 3, 7 |
| `AgrupadorConceptos/Data/MovimientoStorage.cs` | *Modificar.* Lectura por perfil y sobrecarga que escribe dentro de una transacción ajena. | 2 |
| `AgrupadorConceptos/Models/ImpactoHomologacion.cs` | *Crear.* El resultado del cálculo de impacto. | 4 |
| `AgrupadorConceptos/Services/HomologacionAdminService.cs` | *Crear.* Calcula el impacto y orquesta baja/reapuntado. Sin conexiones propias. | 4 |
| `AgrupadorConceptos/HomologarForm.cs` | *Modificar.* Modo edición (valor bloqueado, sin persistir) y validación del valor. | 5 |
| `AgrupadorConceptos/BajaHomologacionDialog.cs` + `.Designer.cs` | *Crear.* El diálogo de baja con el resumen de impacto y las dos salidas. | 6 |
| `AgrupadorConceptos/GestionHomologacionesForm.cs` + `.Designer.cs` | *Reescribir.* Combo de perfil, agrupación, columna de uso, tres botones. | 7 |
| `AgrupadorConceptos/Services/SesionMovimientosService.cs` | *Modificar.* Refresco en sitio desde la base. | 8 |
| `AgrupadorConceptos/ProcesadorForm.cs` | *Modificar.* Pasa el perfil y refresca al volver. | 8 |
| `docs/Historial.md`, `~/.claude/TAREAS.md` | *Modificar.* Los dos registros de la convención. | 9 |

---

### Task 1: Clave ganadora en el matcher

Sin esto no hay forma correcta de saber qué movimientos depende de qué regla: dos reglas distintas pueden producir el mismo `ConceptoEstandar`.

**Files:**
- Modify: `AgrupadorConceptos/Services/HomologacionMatcher.cs:22-38`

**Interfaces:**
- Consumes: nada.
- Produces: `public static string HomologacionMatcher.ResolverClave(IDictionary<string,string> dicHomologacion, string valorABuscar, bool esCodigo)` → la clave del diccionario que resuelve el valor, o `null`. La clave devuelta es igual a la almacenada **ignorando mayúsculas**. `Resolver` conserva su firma y su comportamiento.

- [ ] **Step 1: Reemplazar el método `Resolver`**

Reemplazar el bloque completo que va desde el comentario `/// <summary>` de `Resolver` hasta su llave de cierre (líneas 22-38) por:

```csharp
        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 1 - Devolver la clave que gana, no solo el concepto
        // Saber QUE regla resuelve un movimiento (y no solo con que concepto quedo) es lo
        // que permite calcular el impacto real de una baja. Atribuir por texto de concepto
        // no sirve: dos reglas distintas pueden apuntar al mismo ConceptoEstandar, y al
        // borrar una despegariamos movimientos que la otra sigue cubriendo.
        /// <summary>
        /// Devuelve la clave del diccionario que resuelve el valor, o null si no hay match.
        /// La clave devuelta es igual a la almacenada ignorando mayusculas (el diccionario
        /// es OrdinalIgnoreCase), asi que quien la compare tiene que usar OrdinalIgnoreCase.
        /// </summary>
        public static string ResolverClave(IDictionary<string, string> dicHomologacion, string valorABuscar, bool esCodigo)
        {
            if (dicHomologacion == null || string.IsNullOrEmpty(valorABuscar))
                return null;

            if (esCodigo)
                return dicHomologacion.ContainsKey(valorABuscar) ? valorABuscar : null;

            var match = dicHomologacion.FirstOrDefault(
                d => valorABuscar.IndexOf(d.Key, StringComparison.OrdinalIgnoreCase) >= 0);

            return match.Key;
        }

        /// <summary>
        /// Devuelve el concepto estándar homologado, o null si no hay match.
        /// </summary>
        /// <param name="dicHomologacion">ValorOriginal → ConceptoEstandar. Se espera
        /// construido con StringComparer.OrdinalIgnoreCase y en orden estable
        /// (ver HomologacionStorage), porque con varias claves candidatas gana la primera.</param>
        public static string Resolver(IDictionary<string, string> dicHomologacion, string valorABuscar, bool esCodigo)
        {
            // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 1 - Reimplementado sobre ResolverClave
            // Una sola definicion de "cual gana": si las dos rutinas resolvieran por su
            // cuenta, el impacto calculado podria no coincidir con lo que hizo la importacion.
            string clave = ResolverClave(dicHomologacion, valorABuscar, esCodigo);

            return clave != null && dicHomologacion.TryGetValue(clave, out string homologado)
                ? homologado
                : null;
        }
```

- [ ] **Step 2: Compilar**

```bash
dotnet build ConciliadorContable.slnx -v q --nologo
```

Esperado: `0 Errores`.

- [ ] **Step 3: Commit**

```bash
git add AgrupadorConceptos/Services/HomologacionMatcher.cs
git commit -m "feat(agrupador): el matcher devuelve la clave que gana"
```

---

### Task 2: Lecturas que necesita la gestión

**Files:**
- Modify: `AgrupadorConceptos/Models/HomologacionListado.cs`
- Modify: `AgrupadorConceptos/Data/HomologacionStorage.cs` (métodos `ObtenerDiccionario` y `ObtenerListado`)
- Modify: `AgrupadorConceptos/Data/MovimientoStorage.cs`

**Interfaces:**
- Consumes: nada de tareas anteriores.
- Produces:
  - `HomologacionListado` suma `int IdPerfilBanco`, `int IdConceptoEstandar`, `int Movimientos`.
  - `HomologacionStorage.ObtenerDiccionario(int idPerfilBanco, int? idHomologacionAExcluir = null)`
  - `HomologacionStorage.ObtenerListado(int? idPerfilBanco = null)`
  - `MovimientoStorage.ObtenerPorPerfil(int idPerfilBanco)` → `List<MovimientoProcesado>`
  - `MovimientoStorage.ActualizarConceptos(IEnumerable<MovimientoProcesado>, IDbConnection, IDbTransaction)`

- [ ] **Step 1: Ampliar el modelo de la fila**

Reemplazar el cuerpo de la clase en `AgrupadorConceptos/Models/HomologacionListado.cs`:

```csharp
namespace AgrupadorConceptos.Models
{
    /// <summary>
    /// Fila de la pantalla de gestión de homologaciones: la homologación
    /// con el banco y el concepto estándar ya resueltos por JOIN.
    /// </summary>
    public class HomologacionListado
    {
        public int Id { get; set; }
        public string Banco { get; set; }
        public string ValorOriginal { get; set; }
        public string ConceptoEstandar { get; set; }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 3 - Ids para operar sobre la fila
        // Con "(Todos los perfiles)" la pantalla no tiene un perfil en el combo: el
        // perfil sobre el que se calcula el impacto sale de la fila.
        public int IdPerfilBanco { get; set; }
        public int IdConceptoEstandar { get; set; }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 1 - Cuantos movimientos resuelve hoy
        // No sale del SQL: que regla cubre cada movimiento lo decide el matcher, no una FK.
        // Lo completa HomologacionAdminService.ContarUsoPorRegla.
        public int Movimientos { get; set; }
    }
}
```

- [ ] **Step 2: Diccionario con exclusión de una regla**

En `AgrupadorConceptos/Data/HomologacionStorage.cs`, reemplazar el método `ObtenerDiccionario` completo (firma, cuerpo y su comentario `<summary>`) por:

```csharp
        /// <summary>
        /// Diccionario ValorOriginal → ConceptoEstandar del perfil, case-insensitive.
        ///
        /// El ORDER BY no es cosmético: cuando el perfil no es por código, el match se
        /// hace por substring y puede haber varias claves candidatas para la misma
        /// descripción; gana la primera del diccionario. Sin un orden fijo, dos
        /// pantallas resolvían el mismo movimiento de forma distinta.
        /// </summary>
        /// <param name="idHomologacionAExcluir">
        /// Regla que hay que dejar afuera. Sirve para preguntar "¿qué pasaría si esta
        /// regla no existiera?" antes de borrarla. Se excluye en el SQL y no sacando la
        /// clave después, porque quitarla del diccionario ya armado no garantiza que el
        /// resto conserve el orden del ORDER BY, y ese orden es la precedencia.
        /// </param>
        public static Dictionary<string, string> ObtenerDiccionario(int idPerfilBanco, int? idHomologacionAExcluir = null)
        {
            using var cn = DatabaseHelper.Open();
            return cn.Query(@"
                SELECT h.ValorOriginal, c.Nombre AS ConceptoEstandar
                FROM bancos.HomologacionConceptos h
                INNER JOIN bancos.ConceptosEstandar c ON h.IdConceptoEstandar = c.Id
                WHERE h.IdPerfilBanco = @IdPerfil
                  AND (@IdExcluir IS NULL OR h.Id <> @IdExcluir)
                ORDER BY h.ValorOriginal DESC",
                new { IdPerfil = idPerfilBanco, IdExcluir = idHomologacionAExcluir })
                .ToDictionary(x => (string)x.ValorOriginal, x => (string)x.ConceptoEstandar,
                              StringComparer.OrdinalIgnoreCase);
        }
```

- [ ] **Step 3: Listado con filtro por perfil**

En el mismo archivo, reemplazar el método `ObtenerListado` completo por:

```csharp
        /// <summary>Listado para la pantalla de gestión, con el banco resuelto.</summary>
        /// <param name="idPerfilBanco">Perfil a listar, o null para todos.</param>
        public static List<HomologacionListado> ObtenerListado(int? idPerfilBanco = null)
        {
            using var cn = DatabaseHelper.Open();
            // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 3 - Filtro por perfil y orden por clave
            // El ORDER BY replica el del diccionario (ValorOriginal DESC): la grilla tiene
            // que leerse en el mismo orden de precedencia con el que resuelve el matcher.
            return cn.Query<HomologacionListado>(@"
                SELECT h.Id, h.IdPerfilBanco, h.IdConceptoEstandar,
                       p.NombreBanco AS Banco, h.ValorOriginal, c.Nombre AS ConceptoEstandar
                FROM bancos.HomologacionConceptos h
                JOIN bancos.PerfilesBanco     p ON h.IdPerfilBanco      = p.Id
                JOIN bancos.ConceptosEstandar c ON h.IdConceptoEstandar = c.Id
                WHERE (@IdPerfil IS NULL OR h.IdPerfilBanco = @IdPerfil)
                ORDER BY p.NombreBanco, h.ValorOriginal DESC",
                new { IdPerfil = idPerfilBanco }).ToList();
        }
```

- [ ] **Step 4: Movimientos por perfil y update dentro de una transacción ajena**

En `AgrupadorConceptos/Data/MovimientoStorage.cs`, agregar `using System.Data;` a los `using` del archivo. Después, insertar el método nuevo inmediatamente debajo de `ObtenerPorArchivos`:

```csharp
        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 1 - Movimientos de todos los archivos del perfil
        // La homologacion es del perfil, no del archivo: una baja tiene que alcanzar
        // tambien a las sesiones historicas, no solo a la que este abierta.
        /// <summary>Movimientos de todos los archivos importados de un perfil.</summary>
        public static List<MovimientoProcesado> ObtenerPorPerfil(int idPerfilBanco)
        {
            using var cn = DatabaseHelper.Open();
            return cn.Query<MovimientoProcesado>(@"
                SELECT m.* FROM bancos.MovimientosArchivo m
                JOIN bancos.ArchivosImportados a ON m.IdArchivo = a.Id
                WHERE a.IdPerfilBanco = @IdPerfil",
                new { IdPerfil = idPerfilBanco }).ToList();
        }
```

Y reemplazar el método `ActualizarConceptos` existente por este par (el público delega en la sobrecarga, así el SQL de `UpdateConceptos` no se duplica):

```csharp
        /// <summary>
        /// Persiste ConceptoEstandar/ConceptoFinal de varios movimientos en una sola
        /// transacción, por el mismo motivo que InsertarLote.
        /// </summary>
        public static void ActualizarConceptos(IEnumerable<MovimientoProcesado> movimientos)
        {
            var lista = movimientos?.ToList() ?? new List<MovimientoProcesado>();
            if (lista.Count == 0) return;

            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();

            ActualizarConceptos(lista, cn, tx);

            tx.Commit();
        }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 1 - Escribir dentro de una transaccion ajena
        // La baja tiene que borrar la regla y reescribir los movimientos de forma atomica.
        // Si fueran dos transacciones y fallara la segunda, la regla quedaria borrada y los
        // movimientos apuntando a un concepto que ya no tiene quien lo respalde.
        /// <summary>
        /// Igual que <see cref="ActualizarConceptos(IEnumerable{MovimientoProcesado})"/> pero
        /// dentro de la conexión y transacción que abrió el llamador. No commitea.
        /// </summary>
        public static void ActualizarConceptos(
            IEnumerable<MovimientoProcesado> movimientos, IDbConnection cn, IDbTransaction tx)
        {
            var lista = movimientos?.ToList() ?? new List<MovimientoProcesado>();
            if (lista.Count == 0) return;

            foreach (var mov in lista)
                cn.Execute(UpdateConceptos, mov, tx);
        }
```

- [ ] **Step 5: Compilar**

```bash
dotnet build ConciliadorContable.slnx -v q --nologo
```

Esperado: `0 Errores`. Si falla en `IDbConnection`/`IDbTransaction`, falta el `using System.Data;` del Step 4.

- [ ] **Step 6: Commit**

```bash
git add AgrupadorConceptos/Models/HomologacionListado.cs AgrupadorConceptos/Data/HomologacionStorage.cs AgrupadorConceptos/Data/MovimientoStorage.cs
git commit -m "feat(agrupador): lecturas por perfil y update transaccional de conceptos"
```

---

### Task 3: Escrituras atómicas de la regla + sus movimientos

**Files:**
- Modify: `AgrupadorConceptos/Data/HomologacionStorage.cs` (extraer un privado y agregar dos métodos)

**Interfaces:**
- Consumes: `MovimientoStorage.ActualizarConceptos(IEnumerable<MovimientoProcesado>, IDbConnection, IDbTransaction)` (Tarea 2).
- Produces:
  - `HomologacionStorage.EliminarYActualizarMovimientos(int idHomologacion, IReadOnlyCollection<MovimientoProcesado> movimientos)` → `void`
  - `HomologacionStorage.ReapuntarYActualizarMovimientos(int idHomologacion, string nombreConcepto, IReadOnlyCollection<MovimientoProcesado> movimientos)` → `int` (Id del concepto usado)

- [ ] **Step 1: Agregar el `using` de `System.Data`**

En `AgrupadorConceptos/Data/HomologacionStorage.cs`, agregar a los `using` del archivo:

```csharp
using System.Data;
```

- [ ] **Step 2: Extraer "buscar o crear concepto" de `Guardar`**

Dentro de `Guardar`, reemplazar el bloque que hoy resuelve `idConcepto`:

```csharp
            int? idConcepto = cn.QueryFirstOrDefault<int?>(
                "SELECT Id FROM bancos.ConceptosEstandar WHERE LOWER(Nombre) = LOWER(@Nombre)",
                new { Nombre = nombreConcepto }, tx);

            if (idConcepto == null)
                idConcepto = cn.QuerySingle<int>(
                    "INSERT INTO bancos.ConceptosEstandar (Nombre) OUTPUT INSERTED.Id VALUES (@Nombre);",
                    new { Nombre = nombreConcepto }, tx);
```

por:

```csharp
            // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 2 - Extraido para reusarlo en el reapuntado
            int idConcepto = ObtenerOCrearConcepto(cn, tx, nombreConcepto);
```

El método queda así de la transacción para abajo (ojo con los dos `idConcepto.Value` que
desaparecen, en el parámetro del INSERT y en el `return`):

```csharp
            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();

            // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 2 - Extraido para reusarlo en el reapuntado
            int idConcepto = ObtenerOCrearConcepto(cn, tx, nombreConcepto);

            cn.Execute(@"
                DELETE FROM bancos.HomologacionConceptos
                WHERE IdPerfilBanco = @IdPerfilBanco AND ValorOriginal = @ValorOriginal",
                new { IdPerfilBanco = idPerfilBanco, ValorOriginal = valorOriginal }, tx);

            cn.Execute(@"
                INSERT INTO bancos.HomologacionConceptos (IdPerfilBanco, ValorOriginal, IdConceptoEstandar)
                VALUES (@IdPerfilBanco, @ValorOriginal, @IdConceptoEstandar)",
                new { IdPerfilBanco = idPerfilBanco, ValorOriginal = valorOriginal, IdConceptoEstandar = idConcepto }, tx);

            tx.Commit();
            return idConcepto;
        }
```

Agregar el privado al final de la clase:

```csharp
        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 2 - Buscar o crear el concepto estandar
        /// <summary>
        /// Id del concepto con ese nombre, creándolo si no existía. Va siempre dentro de la
        /// transacción del llamador: si se creara suelto y el resto fallara, quedaría un
        /// ConceptoEstandar huérfano sin ninguna homologación que lo use.
        /// </summary>
        private static int ObtenerOCrearConcepto(IDbConnection cn, IDbTransaction tx, string nombreConcepto)
        {
            int? id = cn.QueryFirstOrDefault<int?>(
                "SELECT Id FROM bancos.ConceptosEstandar WHERE LOWER(Nombre) = LOWER(@Nombre)",
                new { Nombre = nombreConcepto }, tx);

            return id ?? cn.QuerySingle<int>(
                "INSERT INTO bancos.ConceptosEstandar (Nombre) OUTPUT INSERTED.Id VALUES (@Nombre);",
                new { Nombre = nombreConcepto }, tx);
        }
```

- [ ] **Step 3: Agregar las dos escrituras transaccionales**

Inmediatamente debajo del método `Eliminar` existente:

```csharp
        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 1 - Baja con los movimientos ya resueltos
        /// <summary>
        /// Borra la regla y persiste en la misma transacción los movimientos que la baja
        /// dejó modificados. Atómico a propósito: media baja aplicada es peor que ninguna.
        /// </summary>
        /// <param name="movimientos">Ya vienen con ConceptoEstandar/ConceptoFinal decididos
        /// por HomologacionAdminService. Puede venir vacío: la regla se borra igual.</param>
        public static void EliminarYActualizarMovimientos(
            int idHomologacion, IReadOnlyCollection<MovimientoProcesado> movimientos)
        {
            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();

            cn.Execute("DELETE FROM bancos.HomologacionConceptos WHERE Id = @Id",
                new { Id = idHomologacion }, tx);

            MovimientoStorage.ActualizarConceptos(movimientos, cn, tx);

            tx.Commit();
        }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 2 - Edicion: reapuntar la regla y arrastrar
        /// <summary>
        /// Reapunta la regla a otro concepto (buscándolo o creándolo) y persiste en la misma
        /// transacción los movimientos que arrastra. Hace UPDATE en vez del DELETE+INSERT que
        /// usa <see cref="Guardar"/>, así el Id de la regla sobrevive a la edición.
        /// </summary>
        /// <returns>Id del concepto estándar al que quedó apuntando.</returns>
        public static int ReapuntarYActualizarMovimientos(
            int idHomologacion, string nombreConcepto, IReadOnlyCollection<MovimientoProcesado> movimientos)
        {
            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();

            int idConcepto = ObtenerOCrearConcepto(cn, tx, nombreConcepto);

            cn.Execute(@"
                UPDATE bancos.HomologacionConceptos
                SET IdConceptoEstandar = @IdConcepto
                WHERE Id = @Id",
                new { IdConcepto = idConcepto, Id = idHomologacion }, tx);

            MovimientoStorage.ActualizarConceptos(movimientos, cn, tx);

            tx.Commit();
            return idConcepto;
        }
```

- [ ] **Step 4: Compilar**

```bash
dotnet build ConciliadorContable.slnx -v q --nologo
```

Esperado: `0 Errores`.

- [ ] **Step 5: Commit**

```bash
git add AgrupadorConceptos/Data/HomologacionStorage.cs
git commit -m "feat(agrupador): baja y reapuntado atomicos de la homologacion y sus movimientos"
```

---

### Task 4: Servicio de impacto y propagación

Es el corazón de la tarea. No abre conexiones: lee por los Storage, decide en memoria con el matcher y delega la escritura atómica.

**Files:**
- Create: `AgrupadorConceptos/Models/ImpactoHomologacion.cs`
- Create: `AgrupadorConceptos/Services/HomologacionAdminService.cs`

**Interfaces:**
- Consumes: `HomologacionMatcher.ResolverClave` (T1); `HomologacionStorage.ObtenerDiccionario(int, int?)`, `MovimientoStorage.ObtenerPorPerfil(int)` (T2); `HomologacionStorage.EliminarYActualizarMovimientos`, `ReapuntarYActualizarMovimientos` (T3).
- Produces:
  - `MovimientoImpactado { MovimientoProcesado Movimiento; string ConceptoSinLaRegla; }`
  - `ImpactoHomologacion { List<MovimientoImpactado> Afectados; List<MovimientoImpactado> CubiertosPorOtraRegla; int Total; }`
  - `HomologacionAdminService.ContarUsoPorRegla(PerfilBanco perfil)` → `Dictionary<string,int>` (`OrdinalIgnoreCase`, clave = `ValorOriginal`)
  - `HomologacionAdminService.CalcularImpactoBaja(HomologacionListado regla, PerfilBanco perfil)` → `ImpactoHomologacion`
  - `HomologacionAdminService.AplicarBaja(HomologacionListado regla, ImpactoHomologacion impacto, string conceptoDestino)` → `void`
  - `HomologacionAdminService.Reapuntar(HomologacionListado regla, PerfilBanco perfil, string nombreConcepto)` → `void`

- [ ] **Step 1: Crear el modelo del impacto**

Crear `AgrupadorConceptos/Models/ImpactoHomologacion.cs` (UTF-8 con BOM):

```csharp
// Fecha: 28/08/2026 - TAREA: 00003 - Linea: 1 - Resultado del calculo de impacto de una baja
using System.Collections.Generic;

namespace AgrupadorConceptos.Models
{
    /// <summary>Un movimiento que hoy resuelve la regla, con lo que le quedaría sin ella.</summary>
    public class MovimientoImpactado
    {
        public MovimientoProcesado Movimiento { get; set; }

        /// <summary>
        /// Concepto que otra homologación del perfil le sigue dando si esta regla desaparece,
        /// o null si ninguna lo cubre. Se calcula una sola vez, al mostrar el impacto, y se
        /// reusa al aplicar: así lo que se ejecuta es exactamente lo que se le mostró al usuario.
        /// </summary>
        public string ConceptoSinLaRegla { get; set; }
    }

    /// <summary>
    /// Movimientos que resuelve una regla, partidos según sobrevivan o no a su baja.
    /// </summary>
    public class ImpactoHomologacion
    {
        /// <summary>Se quedan sin ninguna regla que los cubra (ConceptoSinLaRegla == null).</summary>
        public List<MovimientoImpactado> Afectados { get; } = new List<MovimientoImpactado>();

        /// <summary>Los sigue resolviendo otra homologación; no se despegan.</summary>
        public List<MovimientoImpactado> CubiertosPorOtraRegla { get; } = new List<MovimientoImpactado>();

        public int Total => Afectados.Count + CubiertosPorOtraRegla.Count;
    }
}
```

- [ ] **Step 2: Crear el servicio**

Crear `AgrupadorConceptos/Services/HomologacionAdminService.cs` (UTF-8 con BOM):

```csharp
// Fecha: 28/08/2026 - TAREA: 00003 - Linea: 1 - Gestion de homologaciones: impacto y propagacion
using System;
using System.Collections.Generic;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;

namespace AgrupadorConceptos.Services
{
    /// <summary>
    /// Lo que la pantalla de gestión necesita saber y hacer sobre una homologación:
    /// a cuántos movimientos alcanza, qué pasa si se la borra, y cómo se propaga el cambio.
    ///
    /// No abre conexiones: lee por los Storage, decide en memoria con el matcher y delega
    /// la escritura atómica a HomologacionStorage.
    ///
    /// La fuente de verdad de "qué resuelve cada regla" es el matcher, no el texto guardado
    /// en el movimiento. Si un movimiento arrastraba un concepto viejo porque las reglas
    /// cambiaron desde su importación, estas operaciones se lo recalculan.
    /// </summary>
    internal static class HomologacionAdminService
    {
        /// <summary>
        /// Cuántos movimientos del perfil resuelve hoy cada ValorOriginal. Es el número que
        /// muestra la columna "Movimientos" de la grilla.
        /// </summary>
        public static Dictionary<string, int> ContarUsoPorRegla(PerfilBanco perfil)
        {
            var conteo = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (perfil == null) return conteo;

            var dic = HomologacionStorage.ObtenerDiccionario(perfil.Id);

            foreach (var mov in MovimientoStorage.ObtenerPorPerfil(perfil.Id))
            {
                string clave = HomologacionMatcher.ResolverClave(dic, mov.ConceptoOriginal, perfil.EsCodigo);
                if (clave == null) continue;

                conteo[clave] = conteo.TryGetValue(clave, out int n) ? n + 1 : 1;
            }

            return conteo;
        }

        /// <summary>
        /// Qué le pasa a los movimientos del perfil si se borra esta regla. No escribe nada
        /// ni modifica los movimientos: sólo mira.
        /// </summary>
        public static ImpactoHomologacion CalcularImpactoBaja(HomologacionListado regla, PerfilBanco perfil)
        {
            var impacto = new ImpactoHomologacion();
            if (regla == null || perfil == null) return impacto;

            var dicCompleto = HomologacionStorage.ObtenerDiccionario(perfil.Id);
            var dicSinRegla = HomologacionStorage.ObtenerDiccionario(perfil.Id, regla.Id);

            foreach (var mov in MovimientoStorage.ObtenerPorPerfil(perfil.Id))
            {
                string clave = HomologacionMatcher.ResolverClave(dicCompleto, mov.ConceptoOriginal, perfil.EsCodigo);
                if (!string.Equals(clave, regla.ValorOriginal, StringComparison.OrdinalIgnoreCase)) continue;

                var item = new MovimientoImpactado
                {
                    Movimiento = mov,
                    ConceptoSinLaRegla = HomologacionMatcher.Resolver(dicSinRegla, mov.ConceptoOriginal, perfil.EsCodigo)
                };

                if (item.ConceptoSinLaRegla == null) impacto.Afectados.Add(item);
                else impacto.CubiertosPorOtraRegla.Add(item);
            }

            return impacto;
        }

        /// <summary>
        /// Borra la regla y propaga: los afectados van a <paramref name="conceptoDestino"/>
        /// (o a Pendiente Homologar si es null), y los que otra regla sigue cubriendo se
        /// recalculan con el concepto que les corresponde.
        /// </summary>
        /// <param name="impacto">El mismo que se le mostró al usuario, para que lo que se
        /// ejecuta sea exactamente lo que él aceptó.</param>
        public static void AplicarBaja(HomologacionListado regla, ImpactoHomologacion impacto, string conceptoDestino)
        {
            var cambiados = new List<MovimientoProcesado>();

            string paraAfectados = string.IsNullOrWhiteSpace(conceptoDestino)
                ? ConceptosBancarios.PendienteHomologar
                : conceptoDestino.Trim();

            foreach (var item in impacto.Afectados)
                if (AplicarConcepto(item.Movimiento, paraAfectados)) cambiados.Add(item.Movimiento);

            foreach (var item in impacto.CubiertosPorOtraRegla)
                if (AplicarConcepto(item.Movimiento, item.ConceptoSinLaRegla)) cambiados.Add(item.Movimiento);

            HomologacionStorage.EliminarYActualizarMovimientos(regla.Id, cambiados);
        }

        /// <summary>
        /// Reapunta la regla a otro concepto y arrastra los movimientos que hoy resuelve.
        /// </summary>
        public static void Reapuntar(HomologacionListado regla, PerfilBanco perfil, string nombreConcepto)
        {
            var dic = HomologacionStorage.ObtenerDiccionario(perfil.Id);
            var cambiados = new List<MovimientoProcesado>();

            foreach (var mov in MovimientoStorage.ObtenerPorPerfil(perfil.Id))
            {
                string clave = HomologacionMatcher.ResolverClave(dic, mov.ConceptoOriginal, perfil.EsCodigo);
                if (!string.Equals(clave, regla.ValorOriginal, StringComparison.OrdinalIgnoreCase)) continue;

                if (AplicarConcepto(mov, nombreConcepto)) cambiados.Add(mov);
            }

            HomologacionStorage.ReapuntarYActualizarMovimientos(regla.Id, nombreConcepto, cambiados);
        }

        /// <summary>
        /// Escribe el concepto en el movimiento. ConceptoEstandar siempre; ConceptoFinal sólo
        /// si estaba pendiente o venía igual al ConceptoEstandar viejo: lo que el usuario
        /// editó a mano en la grilla del Procesador no se pisa. Es el mismo criterio que ya
        /// aplica HomologacionMatcher.AplicarA en la importación.
        /// </summary>
        /// <returns>True si algo cambió, para no mandar UPDATEs que no hacen nada.</returns>
        private static bool AplicarConcepto(MovimientoProcesado mov, string conceptoNuevo)
        {
            string estandarViejo = mov.ConceptoEstandar;

            // Se evalúa ANTES de pisar ConceptoEstandar: después ya no se sabría si el
            // ConceptoFinal venía siguiendo al estándar o lo había escrito el usuario.
            bool finalSeguiaAlEstandar =
                ConceptosBancarios.EstaPendiente(mov.ConceptoFinal) ||
                string.Equals(mov.ConceptoFinal, estandarViejo, StringComparison.OrdinalIgnoreCase);

            bool cambio = false;

            if (!string.Equals(estandarViejo, conceptoNuevo, StringComparison.OrdinalIgnoreCase))
            {
                mov.ConceptoEstandar = conceptoNuevo;
                cambio = true;
            }

            if (finalSeguiaAlEstandar &&
                !string.Equals(mov.ConceptoFinal, conceptoNuevo, StringComparison.OrdinalIgnoreCase))
            {
                mov.ConceptoFinal = conceptoNuevo;
                cambio = true;
            }

            return cambio;
        }
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
git add AgrupadorConceptos/Models/ImpactoHomologacion.cs AgrupadorConceptos/Services/HomologacionAdminService.cs
git commit -m "feat(agrupador): servicio de impacto y propagacion de homologaciones"
```

---

### Task 5: `HomologarForm` en modo edición

**Files:**
- Modify: `AgrupadorConceptos/HomologarForm.cs`

**Interfaces:**
- Consumes: nada de tareas anteriores.
- Produces: en `HomologarForm` — `bool BloquearValorOriginal { get; set; }`, `bool SoloSeleccionar { get; set; }`, `string sValorOriginal { get; private set; }`. `HomologacionExitosa` y `sConcepto` no cambian de semántica.

- [ ] **Step 1: Agregar las propiedades**

En `AgrupadorConceptos/HomologarForm.cs`, debajo de `public string sConcepto { get; private set; } = "";` agregar:

```csharp
        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 2 - Valor con el que se guardo
        /// <summary>Valor original efectivamente usado (el usuario pudo acortarlo).</summary>
        public string sValorOriginal { get; private set; } = "";

        private bool _bloquearValorOriginal;

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 2 - Modo edicion: la clave no se toca
        /// <summary>
        /// Deja el valor del banco en sólo lectura. La edición desde la gestión no puede
        /// cambiar la clave: Guardar pisa la homologación previa borrando por
        /// (IdPerfilBanco, ValorOriginal), así que cambiarla dejaría viva la regla vieja.
        /// Para cambiar la clave hay que eliminar y dar de alta de nuevo.
        /// </summary>
        public bool BloquearValorOriginal
        {
            get => _bloquearValorOriginal;
            set
            {
                _bloquearValorOriginal = value;
                txtOriginal.ReadOnly = value;
                lblAyuda.Visible = !value;
            }
        }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 2 - Devolver el concepto sin persistir
        /// <summary>
        /// El form no guarda: sólo devuelve el concepto elegido en <see cref="sConcepto"/>.
        /// Lo usa la edición, que necesita guardar la regla y arrastrar los movimientos en
        /// una sola transacción y no puede dejar que este form commitee por su cuenta.
        /// </summary>
        public bool SoloSeleccionar { get; set; }
```

- [ ] **Step 2: Validar el valor y respetar el modo selección**

Reemplazar el método `btnGuardar_Click` completo por:

```csharp
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            string conceptoEstandarTexto = cmbEstandar.Text.Trim();

            if (string.IsNullOrEmpty(conceptoEstandarTexto))
            {
                MessageBox.Show("Debe ingresar o seleccionar un Concepto Estándar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Si el valor es texto largo, el usuario pudo haber editado txtOriginal
            // para dejar solo la palabra clave que se va a buscar por substring.
            string valorClave = txtOriginal.Text.Trim();

            // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 2 - Validar el valor del banco
            // El boton "Nueva" de la gestion abre este form en blanco. Un ValorOriginal
            // vacio entraria a la base y, al buscarse por substring, haria match con todo.
            if (string.IsNullOrEmpty(valorClave))
            {
                MessageBox.Show("Debe indicar el concepto o la palabra clave del banco.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 2 - En modo seleccion no persiste
                if (!SoloSeleccionar)
                    HomologacionStorage.Guardar(_idPerfilBanco, valorClave, conceptoEstandarTexto);

                HomologacionExitosa = true;
                sConcepto = conceptoEstandarTexto;
                sValorOriginal = valorClave;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar homologación: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
git add AgrupadorConceptos/HomologarForm.cs
git commit -m "feat(agrupador): HomologarForm en modo edicion y validacion del valor"
```

---

### Task 6: Diálogo de baja

**Files:**
- Create: `AgrupadorConceptos/BajaHomologacionDialog.cs`
- Create: `AgrupadorConceptos/BajaHomologacionDialog.Designer.cs`

**Interfaces:**
- Consumes: `ImpactoHomologacion` (T4); `HomologacionStorage.ObtenerConceptosEstandar()` (ya existe).
- Produces: `BajaHomologacionDialog(HomologacionListado regla, ImpactoHomologacion impacto)` con `string ConceptoDestino { get; private set; }` — el concepto al que reasignar, o `null` para dejar pendientes. Devuelve `DialogResult.OK` si se confirma.

- [ ] **Step 1: Crear el Designer**

Crear `AgrupadorConceptos/BajaHomologacionDialog.Designer.cs` (UTF-8 **con BOM**, si no los acentos salen corruptos):

```csharp
namespace AgrupadorConceptos
{
    partial class BajaHomologacionDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblResumen = new System.Windows.Forms.Label();
            this.lblPregunta = new System.Windows.Forms.Label();
            this.rbPendientes = new System.Windows.Forms.RadioButton();
            this.rbReasignar = new System.Windows.Forms.RadioButton();
            this.cmbConcepto = new System.Windows.Forms.ComboBox();
            this.lblAviso = new System.Windows.Forms.Label();
            this.btnAceptar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblResumen
            // 
            this.lblResumen.Location = new System.Drawing.Point(16, 16);
            this.lblResumen.Name = "lblResumen";
            this.lblResumen.Size = new System.Drawing.Size(520, 60);
            this.lblResumen.TabIndex = 0;
            // 
            // lblPregunta
            // 
            this.lblPregunta.Location = new System.Drawing.Point(16, 86);
            this.lblPregunta.Name = "lblPregunta";
            this.lblPregunta.Size = new System.Drawing.Size(520, 20);
            this.lblPregunta.TabIndex = 1;
            this.lblPregunta.Text = "¿Qué se hace con los movimientos que quedan sin regla?";
            // 
            // rbPendientes
            // 
            this.rbPendientes.Location = new System.Drawing.Point(24, 112);
            this.rbPendientes.Name = "rbPendientes";
            this.rbPendientes.Size = new System.Drawing.Size(510, 22);
            this.rbPendientes.TabIndex = 2;
            this.rbPendientes.Text = "Dejarlos pendientes de homologar";
            this.rbPendientes.UseVisualStyleBackColor = true;
            // 
            // rbReasignar
            // 
            this.rbReasignar.Location = new System.Drawing.Point(24, 140);
            this.rbReasignar.Name = "rbReasignar";
            this.rbReasignar.Size = new System.Drawing.Size(150, 22);
            this.rbReasignar.TabIndex = 3;
            this.rbReasignar.Text = "Reasignarlos a:";
            this.rbReasignar.UseVisualStyleBackColor = true;
            // 
            // cmbConcepto
            // 
            this.cmbConcepto.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cmbConcepto.FormattingEnabled = true;
            this.cmbConcepto.Location = new System.Drawing.Point(180, 140);
            this.cmbConcepto.Name = "cmbConcepto";
            this.cmbConcepto.Size = new System.Drawing.Size(356, 23);
            this.cmbConcepto.TabIndex = 4;
            // 
            // lblAviso
            // 
            this.lblAviso.ForeColor = System.Drawing.Color.Gray;
            this.lblAviso.Location = new System.Drawing.Point(24, 170);
            this.lblAviso.Name = "lblAviso";
            this.lblAviso.Size = new System.Drawing.Size(512, 34);
            this.lblAviso.TabIndex = 5;
            this.lblAviso.Text = "La homologación se borra igual: una importación futura de ese mismo valor va a quedar pendiente.";
            // 
            // btnAceptar
            // 
            this.btnAceptar.Location = new System.Drawing.Point(340, 214);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new System.Drawing.Size(95, 30);
            this.btnAceptar.TabIndex = 6;
            this.btnAceptar.Text = "Aceptar";
            this.btnAceptar.UseVisualStyleBackColor = true;
            this.btnAceptar.Click += new System.EventHandler(this.btnAceptar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelar.Location = new System.Drawing.Point(441, 214);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(95, 30);
            this.btnCancelar.TabIndex = 7;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            // 
            // BajaHomologacionDialog
            // 
            this.AcceptButton = this.btnAceptar;
            this.CancelButton = this.btnCancelar;
            this.ClientSize = new System.Drawing.Size(552, 258);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnAceptar);
            this.Controls.Add(this.lblAviso);
            this.Controls.Add(this.cmbConcepto);
            this.Controls.Add(this.rbReasignar);
            this.Controls.Add(this.rbPendientes);
            this.Controls.Add(this.lblPregunta);
            this.Controls.Add(this.lblResumen);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BajaHomologacionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Eliminar homologación";
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label lblResumen;
        private System.Windows.Forms.Label lblPregunta;
        private System.Windows.Forms.RadioButton rbPendientes;
        private System.Windows.Forms.RadioButton rbReasignar;
        private System.Windows.Forms.ComboBox cmbConcepto;
        private System.Windows.Forms.Label lblAviso;
        private System.Windows.Forms.Button btnAceptar;
        private System.Windows.Forms.Button btnCancelar;
    }
}
```

- [ ] **Step 2: Crear el code-behind**

Crear `AgrupadorConceptos/BajaHomologacionDialog.cs` (UTF-8 con BOM):

```csharp
// Fecha: 28/08/2026 - TAREA: 00003 - Linea: 1 - Que hacer con los movimientos al dar de baja
using System;
using System.Windows.Forms;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;

namespace AgrupadorConceptos
{
    /// <summary>
    /// Pregunta qué hacer con los movimientos que la baja de una homologación deja sin
    /// regla. Sólo decide: no borra ni escribe nada — de eso se ocupa el llamador con
    /// el mismo objeto de impacto que se mostró acá.
    /// </summary>
    public partial class BajaHomologacionDialog : Form
    {
        /// <summary>
        /// Concepto al que hay que reasignar los movimientos afectados, o null para
        /// dejarlos pendientes de homologar.
        /// </summary>
        public string ConceptoDestino { get; private set; }

        public BajaHomologacionDialog(HomologacionListado regla, ImpactoHomologacion impacto)
        {
            InitializeComponent();
            this.Icon = AppIcon.GetIcon();

            lblResumen.Text =
                $"La homologación '{regla.ValorOriginal}' → '{regla.ConceptoEstandar}' del perfil " +
                $"'{regla.Banco}' resuelve hoy {impacto.Total} movimiento(s)." + Environment.NewLine +
                $"{impacto.Afectados.Count} quedan sin ninguna regla que los cubra. " +
                $"Los otros {impacto.CubiertosPorOtraRegla.Count} los sigue resolviendo otra " +
                $"homologación y no se tocan.";

            cmbConcepto.DataSource = HomologacionStorage.ObtenerConceptosEstandar();
            cmbConcepto.DisplayMember = "Nombre";
            cmbConcepto.ValueMember = "Id";
            cmbConcepto.SelectedIndex = -1;
            cmbConcepto.Enabled = false;

            rbPendientes.Checked = true;
            rbReasignar.CheckedChanged += (s, e) => cmbConcepto.Enabled = rbReasignar.Checked;
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            if (rbReasignar.Checked)
            {
                string concepto = cmbConcepto.Text.Trim();
                if (string.IsNullOrEmpty(concepto))
                {
                    MessageBox.Show("Elija o escriba el concepto al que se reasignan los movimientos.",
                        "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                ConceptoDestino = concepto;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
```

- [ ] **Step 3: Compilar**

```bash
dotnet build ConciliadorContable.slnx -v q --nologo
```

Esperado: `0 Errores`. Si falla en `cmbConcepto.SelectedIndex = -1;` porque el `DataSource` está vacío, reemplazar esa línea por `if (cmbConcepto.Items.Count > 0) cmbConcepto.SelectedIndex = -1;`.

- [ ] **Step 4: Commit**

```bash
git add AgrupadorConceptos/BajaHomologacionDialog.cs AgrupadorConceptos/BajaHomologacionDialog.Designer.cs
git commit -m "feat(agrupador): dialogo de baja con el impacto sobre los movimientos"
```

---

### Task 7: La pantalla de gestión

Se reescriben los dos archivos. El `.Designer.cs` actual está en cp1252 sin BOM, así que su `Text = "Gestión de Homologaciones"` hoy compila con el acento roto; el archivo nuevo va en UTF-8 con BOM y eso queda arreglado de paso.

**Files:**
- Modify (reescribir): `AgrupadorConceptos/GestionHomologacionesForm.Designer.cs`
- Modify (reescribir): `AgrupadorConceptos/GestionHomologacionesForm.cs`

**Interfaces:**
- Consumes: `HomologacionStorage.ObtenerListado(int?)` (T2); `HomologacionAdminService.ContarUsoPorRegla / CalcularImpactoBaja / AplicarBaja / Reapuntar` (T4); `HomologarForm.BloquearValorOriginal / SoloSeleccionar` (T5); `BajaHomologacionDialog(regla, impacto)` con `ConceptoDestino` (T6); `PerfilBancoStorage.ObtenerTodos() / ObtenerPorId(int)` (ya existen).
- Produces: `GestionHomologacionesForm(PerfilBanco perfilInicial = null)` con `bool HuboCambios { get; private set; }`.

- [ ] **Step 1: Reescribir el Designer**

Reemplazar el contenido completo de `AgrupadorConceptos/GestionHomologacionesForm.Designer.cs` (UTF-8 **con BOM**):

```csharp
namespace AgrupadorConceptos
{
    partial class GestionHomologacionesForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            Telerik.WinControls.UI.TableViewDefinition tableViewDefinition1 = new Telerik.WinControls.UI.TableViewDefinition();
            lblPerfil = new System.Windows.Forms.Label();
            cboPerfil = new System.Windows.Forms.ComboBox();
            dgvHomologaciones = new Telerik.WinControls.UI.RadGridView();
            btnNueva = new System.Windows.Forms.Button();
            btnEditar = new System.Windows.Forms.Button();
            btnEliminar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)dgvHomologaciones).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvHomologaciones.MasterTemplate).BeginInit();
            SuspendLayout();
            // 
            // lblPerfil
            // 
            lblPerfil.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            lblPerfil.AutoSize = true;
            lblPerfil.Location = new System.Drawing.Point(12, 17);
            lblPerfil.Name = "lblPerfil";
            lblPerfil.Size = new System.Drawing.Size(40, 15);
            lblPerfil.TabIndex = 0;
            lblPerfil.Text = "Perfil:";
            // 
            // cboPerfil
            // 
            cboPerfil.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cboPerfil.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboPerfil.FormattingEnabled = true;
            cboPerfil.Location = new System.Drawing.Point(70, 14);
            cboPerfil.Name = "cboPerfil";
            cboPerfil.Size = new System.Drawing.Size(602, 23);
            cboPerfil.TabIndex = 1;
            // 
            // dgvHomologaciones
            // 
            dgvHomologaciones.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dgvHomologaciones.EnableCustomFiltering = true;
            dgvHomologaciones.Location = new System.Drawing.Point(12, 48);
            // 
            // 
            // 
            dgvHomologaciones.MasterTemplate.AllowAddNewRow = false;
            dgvHomologaciones.MasterTemplate.AllowDeleteRow = false;
            dgvHomologaciones.MasterTemplate.AllowEditRow = false;
            dgvHomologaciones.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            dgvHomologaciones.MasterTemplate.EnableCustomFiltering = true;
            dgvHomologaciones.MasterTemplate.EnableFiltering = true;
            dgvHomologaciones.MasterTemplate.ViewDefinition = tableViewDefinition1;
            dgvHomologaciones.Name = "dgvHomologaciones";
            dgvHomologaciones.ReadOnly = true;
            dgvHomologaciones.Size = new System.Drawing.Size(660, 300);
            dgvHomologaciones.TabIndex = 2;
            // 
            // btnNueva
            // 
            btnNueva.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnNueva.Location = new System.Drawing.Point(12, 356);
            btnNueva.Name = "btnNueva";
            btnNueva.Size = new System.Drawing.Size(150, 30);
            btnNueva.TabIndex = 3;
            btnNueva.Text = "Nueva";
            btnNueva.UseVisualStyleBackColor = true;
            btnNueva.Click += btnNueva_Click;
            // 
            // btnEditar
            // 
            btnEditar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnEditar.Location = new System.Drawing.Point(168, 356);
            btnEditar.Name = "btnEditar";
            btnEditar.Size = new System.Drawing.Size(150, 30);
            btnEditar.TabIndex = 4;
            btnEditar.Text = "Editar";
            btnEditar.UseVisualStyleBackColor = true;
            btnEditar.Click += btnEditar_Click;
            // 
            // btnEliminar
            // 
            btnEliminar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnEliminar.Location = new System.Drawing.Point(324, 356);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new System.Drawing.Size(150, 30);
            btnEliminar.TabIndex = 5;
            btnEliminar.Text = "Eliminar";
            btnEliminar.UseVisualStyleBackColor = true;
            btnEliminar.Click += btnEliminar_Click;
            // 
            // GestionHomologacionesForm
            // 
            ClientSize = new System.Drawing.Size(684, 397);
            Controls.Add(btnEliminar);
            Controls.Add(btnEditar);
            Controls.Add(btnNueva);
            Controls.Add(dgvHomologaciones);
            Controls.Add(cboPerfil);
            Controls.Add(lblPerfil);
            MinimumSize = new System.Drawing.Size(700, 400);
            Name = "GestionHomologacionesForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Gestión de Homologaciones";
            ((System.ComponentModel.ISupportInitialize)dgvHomologaciones.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvHomologaciones).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.Label lblPerfil;
        private System.Windows.Forms.ComboBox cboPerfil;
        private Telerik.WinControls.UI.RadGridView dgvHomologaciones;
        private System.Windows.Forms.Button btnNueva;
        private System.Windows.Forms.Button btnEditar;
        private System.Windows.Forms.Button btnEliminar;
    }
}
```

- [ ] **Step 2: Reescribir el code-behind**

Reemplazar el contenido completo de `AgrupadorConceptos/GestionHomologacionesForm.cs` (UTF-8 con BOM):

```csharp
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;
using Telerik.WinControls.Data;

namespace AgrupadorConceptos
{
    /// <summary>
    /// Alta, baja y modificación de las homologaciones de un perfil. La baja no es un DELETE
    /// suelto: el concepto homologado quedó escrito como texto en cada movimiento, así que
    /// hay que decidir qué pasa con ellos (ver HomologacionAdminService).
    /// </summary>
    public partial class GestionHomologacionesForm : Form
    {
        // Entrada centinela del combo: evita un flag aparte para "no hay perfil elegido".
        private const int IdTodosLosPerfiles = 0;

        private readonly PerfilBanco _perfilInicial;

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 4 - Avisar al llamador que refresque
        /// <summary>True si se tocó alguna homologación: el llamador tiene que refrescar.</summary>
        public bool HuboCambios { get; private set; }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 4 - Abrir filtrada por el perfil que la invoca
        /// <param name="perfilInicial">
        /// Perfil con el que arranca filtrada. Null abre en "(Todos los perfiles)".
        /// </param>
        public GestionHomologacionesForm(PerfilBanco perfilInicial = null)
        {
            InitializeComponent();
            this.Icon = AppIcon.GetIcon();
            _perfilInicial = perfilInicial;

            this.Load += (s, e) =>
            {
                CargarPerfiles();
                CargarDatos();
            };

            dgvHomologaciones.CurrentRowChanged += (s, e) => ActualizarBotones();
        }

        /// <summary>Perfil elegido, o null cuando está en "(Todos los perfiles)".</summary>
        private PerfilBanco PerfilSeleccionado
        {
            get
            {
                var perfil = cboPerfil.SelectedItem as PerfilBanco;
                return perfil != null && perfil.Id != IdTodosLosPerfiles ? perfil : null;
            }
        }

        private HomologacionListado FilaSeleccionada =>
            dgvHomologaciones.CurrentRow?.DataBoundItem as HomologacionListado;

        /// <summary>
        /// Perfil sobre el que opera una fila. Con "(Todos los perfiles)" el combo no lo sabe,
        /// pero la fila trae su IdPerfilBanco.
        /// </summary>
        private PerfilBanco PerfilDeLaFila(HomologacionListado fila) =>
            PerfilSeleccionado ?? PerfilBancoStorage.ObtenerPorId(fila.IdPerfilBanco);

        private void CargarPerfiles()
        {
            var perfiles = PerfilBancoStorage.ObtenerTodos()
                .OrderBy(p => p.NombreBanco)
                .ToList();

            perfiles.Insert(0, new PerfilBanco
            {
                Id = IdTodosLosPerfiles,
                NombreBanco = "(Todos los perfiles)"
            });

            cboPerfil.DataSource = perfiles;
            cboPerfil.DisplayMember = "NombreBanco";
            cboPerfil.ValueMember = "Id";
            cboPerfil.SelectedValue = _perfilInicial?.Id ?? IdTodosLosPerfiles;

            // El handler se engancha despues de fijar la seleccion inicial: si no,
            // el Load dispararia CargarDatos dos veces.
            cboPerfil.SelectedIndexChanged += (s, e) => CargarDatos();
        }

        private void CargarDatos()
        {
            var perfil = PerfilSeleccionado;

            Cursor = Cursors.WaitCursor;
            try
            {
                var filas = HomologacionStorage.ObtenerListado(perfil?.Id);

                // El conteo de uso no sale del SQL: que regla resuelve cada movimiento lo
                // decide el matcher, no una FK. Se calcula perfil por perfil.
                foreach (var grupo in filas.GroupBy(f => f.IdPerfilBanco))
                {
                    var perfilDelGrupo = perfil ?? PerfilBancoStorage.ObtenerPorId(grupo.Key);
                    if (perfilDelGrupo == null) continue;

                    var uso = HomologacionAdminService.ContarUsoPorRegla(perfilDelGrupo);
                    foreach (var fila in grupo)
                        fila.Movimientos = uso.TryGetValue(fila.ValorOriginal, out int n) ? n : 0;
                }

                dgvHomologaciones.DataSource = null;
                dgvHomologaciones.DataSource = filas;

                ConfigurarGrilla(agrupar: perfil == null);
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            ActualizarBotones();
        }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 3 - Agrupar por perfil y ordenar por clave
        // El orden por ValorOriginal descendente es el mismo del diccionario del matcher:
        // la grilla tiene que leerse como la directiva de precedencia que efectivamente rige.
        private void ConfigurarGrilla(bool agrupar)
        {
            OcultarColumna("Id");
            OcultarColumna("IdPerfilBanco");
            OcultarColumna("IdConceptoEstandar");

            RenombrarColumna("Banco", "Perfil / Banco");
            RenombrarColumna("ValorOriginal", "Valor del banco");
            RenombrarColumna("ConceptoEstandar", "Concepto estándar");
            RenombrarColumna("Movimientos", "Movimientos");

            var colMovimientos = dgvHomologaciones.Columns["Movimientos"];
            if (colMovimientos != null)
            {
                colMovimientos.TextAlignment = ContentAlignment.MiddleRight;
                colMovimientos.MaxWidth = 120;
            }

            dgvHomologaciones.SortDescriptors.Clear();
            dgvHomologaciones.SortDescriptors.Add(
                new SortDescriptor("ValorOriginal", ListSortDirection.Descending));

            dgvHomologaciones.GroupDescriptors.Clear();
            if (agrupar)
            {
                var porBanco = new GroupDescriptor();
                porBanco.GroupNames.Add("Banco", ListSortDirection.Ascending);
                dgvHomologaciones.GroupDescriptors.Add(porBanco);
            }
        }

        private void OcultarColumna(string nombre)
        {
            var col = dgvHomologaciones.Columns[nombre];
            if (col != null) col.IsVisible = false;
        }

        private void RenombrarColumna(string nombre, string titulo)
        {
            var col = dgvHomologaciones.Columns[nombre];
            if (col != null) col.HeaderText = titulo;
        }

        private void ActualizarBotones()
        {
            // El alta necesita un perfil concreto: con "(Todos los perfiles)" no hay
            // a cual dar de alta.
            btnNueva.Enabled = PerfilSeleccionado != null;

            bool haySeleccion = FilaSeleccionada != null;
            btnEditar.Enabled = haySeleccion;
            btnEliminar.Enabled = haySeleccion;
        }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 2 - Alta desde la gestion
        private void btnNueva_Click(object sender, EventArgs e)
        {
            var perfil = PerfilSeleccionado;
            if (perfil == null)
            {
                MessageBox.Show("Elija un perfil para dar de alta una homologación.",
                    "Atención", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var frm = new HomologarForm(perfil.Id, "");
            frm.ShowDialog(this);

            if (!frm.HomologacionExitosa) return;

            HuboCambios = true;
            CargarDatos();
        }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 2 - Edicion: reapuntar y arrastrar
        private void btnEditar_Click(object sender, EventArgs e)
        {
            var fila = FilaSeleccionada;
            if (fila == null)
            {
                MessageBox.Show("Seleccione una homologación para editar.",
                    "Atención", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var perfil = PerfilDeLaFila(fila);
            if (perfil == null) return;

            var frm = new HomologarForm(perfil.Id, fila.ValorOriginal)
            {
                BloquearValorOriginal = true,
                SoloSeleccionar = true,
                Text = "Editar homologación"
            };
            frm.ShowDialog(this);

            if (!frm.HomologacionExitosa) return;
            if (string.Equals(frm.sConcepto, fila.ConceptoEstandar, StringComparison.OrdinalIgnoreCase))
                return;

            Cursor = Cursors.WaitCursor;
            try
            {
                HomologacionAdminService.Reapuntar(fila, perfil, frm.sConcepto);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al editar la homologación: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            HuboCambios = true;
            CargarDatos();
        }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 1 - Baja preguntando por los ya homologados
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            var fila = FilaSeleccionada;
            if (fila == null)
            {
                MessageBox.Show("Seleccione una homologación para eliminar.",
                    "Atención", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var perfil = PerfilDeLaFila(fila);
            if (perfil == null) return;

            ImpactoHomologacion impacto;
            Cursor = Cursors.WaitCursor;
            try
            {
                impacto = HomologacionAdminService.CalcularImpactoBaja(fila, perfil);
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            string conceptoDestino = null;

            if (impacto.Total == 0)
            {
                // Sin movimientos que dependan de la regla no hay nada que preguntar.
                var confirma = MessageBox.Show(
                    $"¿Eliminar la homologación '{fila.ValorOriginal}' → '{fila.ConceptoEstandar}'?",
                    "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirma != DialogResult.Yes) return;
            }
            else
            {
                using var dlg = new BajaHomologacionDialog(fila, impacto);
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                conceptoDestino = dlg.ConceptoDestino;
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                HomologacionAdminService.AplicarBaja(fila, impacto, conceptoDestino);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar la homologación: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            HuboCambios = true;
            CargarDatos();
        }
    }
}
```

- [ ] **Step 3: Borrar `HomologacionStorage.Eliminar`**

Con la baja pasando siempre por `AplicarBaja` (incluso cuando el impacto es cero, porque
`EliminarYActualizarMovimientos` borra la regla igual con la lista vacía), este método queda
sin ningún llamador. Borrarlo de `AgrupadorConceptos/Data/HomologacionStorage.cs`:

```csharp
        public static void Eliminar(int id)
        {
            using var cn = DatabaseHelper.Open();
            cn.Execute("DELETE FROM bancos.HomologacionConceptos WHERE Id = @Id", new { Id = id });
        }
```

Antes de borrarlo, confirmar que no quedó ningún uso:

```bash
grep -rn "HomologacionStorage.Eliminar" --include=*.cs .
```

Esperado: sin resultados.

- [ ] **Step 4: Compilar**

```bash
dotnet build ConciliadorContable.slnx -v q --nologo
```

Esperado: `0 Errores`.

Si el compilador rechaza `porBanco.GroupNames.Add("Banco", ListSortDirection.Ascending)`, esa API cambió de nombre en la versión instalada de Telerik (2024.4.1113). El reemplazo equivalente es:

```csharp
                dgvHomologaciones.GroupDescriptors.Add(
                    new GroupDescriptor(new[] { new SortDescriptor("Banco", ListSortDirection.Ascending) }));
```

- [ ] **Step 5: Commit**

```bash
git add AgrupadorConceptos/GestionHomologacionesForm.cs AgrupadorConceptos/GestionHomologacionesForm.Designer.cs AgrupadorConceptos/Data/HomologacionStorage.cs
git commit -m "feat(agrupador): gestion de homologaciones con alta, edicion, baja y filtro por perfil"
```

---

### Task 8: El Procesador pasa el perfil y refresca al volver

**Files:**
- Modify: `AgrupadorConceptos/Services/SesionMovimientosService.cs`
- Modify: `AgrupadorConceptos/ProcesadorForm.cs:248-251` (`btnGestionarHomologaciones_Click`)

**Interfaces:**
- Consumes: `GestionHomologacionesForm(PerfilBanco)` con `HuboCambios` (T7); `MovimientoStorage.ObtenerPorArchivo` (ya existe).
- Produces: `SesionMovimientosService.RefrescarDesdeBase(List<MovimientoProcesado> movs, int idArchivo)` → `void`.

- [ ] **Step 1: Refresco en sitio desde la base**

En `AgrupadorConceptos/Services/SesionMovimientosService.cs`, agregar `using System.Linq;` a los `using` del archivo y el método al final de la clase:

```csharp
        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 4 - Traer lo que cambio la gestion sin rebindear
        /// <summary>
        /// Vuelve a leer los conceptos del archivo y los copia sobre las instancias que la
        /// grilla ya tiene bindeadas, macheando por Id.
        ///
        /// No reemplaza la lista a propósito: el llamador refresca la vista sin rebindear y
        /// el usuario no pierde la fila donde estaba parado. Se usa al volver de la gestión
        /// de homologaciones, que escribió directo en la base sobre todos los archivos del
        /// perfil y por lo tanto también sobre el que está en pantalla.
        /// </summary>
        public static void RefrescarDesdeBase(List<MovimientoProcesado> movs, int idArchivo)
        {
            if (movs == null || movs.Count == 0) return;

            var enBase = MovimientoStorage.ObtenerPorArchivo(idArchivo).ToDictionary(m => m.Id);

            foreach (var mov in movs)
            {
                if (!enBase.TryGetValue(mov.Id, out var actualizado)) continue;

                mov.ConceptoEstandar = actualizado.ConceptoEstandar;
                mov.ConceptoFinal = actualizado.ConceptoFinal;
            }
        }
```

- [ ] **Step 2: Pasar el perfil y refrescar al volver**

En `AgrupadorConceptos/ProcesadorForm.cs`, reemplazar el método `btnGestionarHomologaciones_Click` completo por:

```csharp
        private void btnGestionarHomologaciones_Click(object sender, EventArgs e)
        {
            // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 4 - Abrir filtrada por el perfil en uso
            // _perfilEnGrilla es el perfil de lo que se esta trabajando de verdad; si todavia
            // no se cargo una sesion, alcanza con el del combo.
            var perfil = _perfilEnGrilla ?? cboPerfiles.SelectedItem as PerfilBanco;

            var frm = new GestionHomologacionesForm(perfil);
            frm.ShowDialog();

            if (frm.HuboCambios) RefrescarSesionDesdeBase();
        }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 4 - Reflejar lo que cambio la gestion
        /// <summary>
        /// Trae de la base los conceptos que quedaron después de gestionar homologaciones y
        /// refresca la grilla en el lugar. No rebindea: el cursor no se mueve.
        /// </summary>
        private void RefrescarSesionDesdeBase()
        {
            if (_archivoEnGrilla == null) return;
            if (dgvDatos.DataSource is not List<MovimientoProcesado> movs) return;

            SesionMovimientosService.RefrescarDesdeBase(movs, _archivoEnGrilla.Id);

            RefrescarGrillaConservandoPosicion();
            ActualizarResumen(movs);
        }
```

- [ ] **Step 3: Compilar**

```bash
dotnet build ConciliadorContable.slnx -v q --nologo
```

Esperado: `0 Errores`.

- [ ] **Step 4: Commit**

```bash
git add AgrupadorConceptos/Services/SesionMovimientosService.cs AgrupadorConceptos/ProcesadorForm.cs
git commit -m "feat(agrupador): el procesador abre la gestion filtrada y refresca al volver"
```

---

### Task 9: Verificación manual y registros de la tarea

Sin proyecto de tests, este es el único filtro funcional. **Correr el checklist completo antes de dar la tarea por terminada.**

**Files:**
- Modify: `docs/Historial.md`
- Modify: `~/.claude/TAREAS.md` (fuera de git)

**Interfaces:**
- Consumes: todo lo anterior.
- Produces: nada de código.

- [ ] **Step 1: Preparar los datos de prueba**

En un perfil de **texto libre** (`EsCodigo = false`), que es donde está el riesgo, dar de alta dos homologaciones que se solapen y apunten al mismo concepto:

- `COMISION MANT` → `Comisiones`
- `COMIS` → `Comisiones`

Importar (o tener importado) un archivo con movimientos cuyo `ConceptoOriginal` contenga `COMISION MANT` y otros que contengan sólo `COMIS`.

- [ ] **Step 2: Recorrer el checklist**

Marcar cada punto:

1. **Solapamiento.** Eliminar `COMIS`. El diálogo tiene que reportar en "los sigue resolviendo otra homologación" a los movimientos que dicen `COMISION MANT`, y como afectados sólo al resto. Al aceptar, los de `COMISION MANT` **no** vuelven a pendientes.
2. **Baja dejando pendientes.** Los afectados quedan en `Pendiente Homologar` en `ConceptoEstandar` y `ConceptoFinal`, en **todos** los archivos del perfil (revisar una sesión vieja, no sólo la abierta). Crear después una regla que los agarre y verificar que se resuelven solos al cargar la sesión.
3. **Baja reasignando.** Los afectados quedan con el concepto elegido. Importar de nuevo un archivo con ese valor: tiene que quedar pendiente, porque la regla ya no está.
4. **`ConceptoFinal` a mano.** Editar a mano el `ConceptoFinal` de un movimiento en la grilla del Procesador; después hacer una baja o una edición que lo alcance. El `ConceptoFinal` editado tiene que quedar **intacto**, aunque el `ConceptoEstandar` cambie.
5. **Edición.** Editar una regla a otro concepto: los movimientos que resolvía quedan con el concepto nuevo, la regla conserva su `Id` (verificar con `SELECT Id, IdConceptoEstandar FROM bancos.HomologacionConceptos WHERE ValorOriginal = '...'`) y **no** aparece una regla duplicada.
6. **Filtro y agrupación.** Abrir la pantalla desde el Procesador con un perfil seleccionado: arranca filtrada en ese perfil. Pasar a `(Todos los perfiles)`: la grilla se agrupa por banco y `Nueva` queda deshabilitado. `Editar` y `Eliminar` siguen funcionando sobre la fila.
7. **Perfil por código.** En un perfil con `EsCodigo = true`, los conteos de la columna `Movimientos` coinciden con el match exacto.
8. **Refresco.** Al cerrar la pantalla después de un cambio, la grilla del Procesador muestra los conceptos nuevos **sin moverse** de la fila donde estaba el cursor.
9. **Acentos.** El título de la ventana dice `Gestión de Homologaciones` (no `Gesti?n`), y los textos del diálogo de baja se ven bien.

Si algún punto falla, arreglarlo y volver a correr el checklist desde el 1.

- [ ] **Step 3: Anotar en la bitácora del repo**

Agregar al final de `docs/Historial.md`:

```markdown
## TAREA 00003 — 28/08/2026

### Lo que se pidió

> 1. Al dar de baja una homologación, preguntar qué hacer con los ítems ya homologados:
>    dejarlos pendientes o cambiarlos a otro concepto.
> 2. Alta y edición de homologaciones desde la misma pantalla.
> 3. Que agrupe bien por perfil.
> 4. Que al invocarla filtre por el perfil desde el cual se la llama.

### Lo que se ejecutó

**Línea 1 · Baja con impacto real**

- `Services/HomologacionMatcher.cs` — `ResolverClave` devuelve qué regla gana, no sólo el
  concepto. `Resolver` pasa a implementarse sobre ella. Sin esto la atribución habría que
  hacerla por texto de concepto, y dos reglas distintas pueden apuntar al mismo.
- `Models/ImpactoHomologacion.cs`, `Services/HomologacionAdminService.cs` (nuevos) — calculan
  qué movimientos pierden cobertura al borrar una regla y cuáles sigue tomando otra, y
  propagan la decisión sobre todos los archivos del perfil.
- `BajaHomologacionDialog.cs` (nuevo) — el diálogo con el resumen y las dos salidas.
- `Data/HomologacionStorage.cs` — `EliminarYActualizarMovimientos`: la regla y sus movimientos
  en una sola transacción.
- `Data/MovimientoStorage.cs` — `ObtenerPorPerfil` y la sobrecarga de `ActualizarConceptos`
  que escribe dentro de una transacción ajena.

**Línea 2 · Alta y edición**

- `HomologarForm.cs` — `BloquearValorOriginal` y `SoloSeleccionar` para el modo edición;
  validación del valor del banco (con el botón "Nueva" el form abre en blanco y un valor
  vacío haría match con todo).
- `Data/HomologacionStorage.cs` — `ReapuntarYActualizarMovimientos` hace `UPDATE` en vez del
  `DELETE`+`INSERT` de `Guardar`, así el `Id` de la regla sobrevive; `ObtenerOCrearConcepto`
  extraído para reusarlo.

**Línea 3 · Agrupación por perfil**

- `Models/HomologacionListado.cs` — suma `IdPerfilBanco`, `IdConceptoEstandar` y `Movimientos`.
- `Data/HomologacionStorage.cs` — `ObtenerListado` con filtro opcional y orden
  `ValorOriginal DESC`, el mismo del diccionario del matcher.
- `GestionHomologacionesForm` — agrupación por banco cuando no hay perfil elegido.

**Línea 4 · Filtro por el perfil que la invoca**

- `GestionHomologacionesForm` — combo de perfil precargado y `HuboCambios`.
- `ProcesadorForm.cs` — pasa `_perfilEnGrilla` (o el del combo) y refresca al volver.
- `Services/SesionMovimientosService.cs` — `RefrescarDesdeBase` copia los conceptos sobre las
  instancias bindeadas, sin rebindear, para no mover al usuario de fila.

**De paso:** los `.Designer.cs` reescritos pasan a UTF-8 con BOM. Estaban en cp1252 sin BOM,
así que sus acentos compilaban corruptos (`Gesti?n de Homologaciones` en el título).

**Riesgo anotado, no resuelto:** `bancos.ConciliacionSesiones.ConceptosJson` guarda los
`ConceptoFinal` elegidos al armar una conciliación. Una baja que cambie el `ConceptoFinal` de
movimientos que participan de una sesión guardada puede dejar esa sesión filtrando por un
concepto que ya no existe en los datos.
```

- [ ] **Step 4: Anotar en el registro global**

En `~/.claude/TAREAS.md`, agregar la fila a la tabla y actualizar el comentario final:

```markdown
| 00003 | 28/08/2026 | ConciliadorContable | 4 | La pantalla de gestión de homologaciones pasa de consulta a gestión: la baja calcula el impacto real sobre los movimientos ya homologados y decide entre dejarlos pendientes o reasignarlos; alta y edición de reglas; agrupación y filtro por perfil. Detalle en `D:\Sistemas\ConciliadorContable\docs\Historial.md`. |
```

Y reemplazar `<!-- Próxima tarea: 00003 -->` por `<!-- Próxima tarea: 00004 -->`.

- [ ] **Step 5: Commit**

```bash
git add docs/Historial.md
git commit -m "docs: bitacora de la TAREA 00003"
```

(`~/.claude/TAREAS.md` está fuera de git; no entra en el commit.)

---

## Cierre

Con las 9 tareas hechas, la rama `worktree-gestion-homologaciones` queda lista para probar contra la base real. Recién después de pasar el checklist de la Tarea 9 corresponde fusionar a `main` — usar `superpowers:finishing-a-development-branch`.
