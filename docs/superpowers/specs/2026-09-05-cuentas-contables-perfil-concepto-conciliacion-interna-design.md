# Cuentas contables en AgrupadorConceptos: import legacy, perfil, concepto estándar, cuenta final y conciliación interna (diseño)

**Estado: propuesta pendiente de aprobación del usuario.** No hay código tocado todavía. El
usuario pidió mockups + esta explicación antes de convertirlo en un plan de implementación con
número de `TAREA`. Mockups visuales: ver el Artifact adjunto (`cuentas-contables-mockups.html`).

**Pedido**, numerado por ítem (de ahí sale el `Linea:` de cada comentario cuando esto se
implemente):

> 1. Importar cuentas contables de sistema legacy, sería cuenta y descripción y centro de costo.
> 2. Asignar cuenta contable a perfil.
> 3. Asignar cuenta contable a concepto standard. Acá vamos a tener que crear una ventana para
>    esto ya que no hay un mantenimiento del concepto standard. Hay que pensarlo bien ya que
>    tiene varias implicancias complicadas. Al momento de homologar ahora solo pide una
>    descripción y ahora tiene que pedir la cuenta. La misma puede estar vacía.
> 4. Crear una columna de cuenta final así como está la de concepto final. Por defecto lleva la
>    del concepto standard.
> 5. Creación de ventana para conciliación interna de un concepto entre extractos, las
>    mediciones son por fecha + importe automáticos y manual. Copiar el formato de conciliación
>    con archivo externo.

| Ítem | Secciones de este diseño |
|---|---|
| 1 | Catálogo `bancos.CuentasContables` · Pantalla de importación |
| 2 | Cuenta contable en el perfil |
| 3 | Cuenta contable en el concepto estándar · Ventana nueva de mantenimiento · `HomologarForm` |
| 4 | Columna "Cuenta final" · El punto técnico delicado |
| 5 | Conciliación interna entre extractos |

## Contexto

Hoy `AgrupadorConceptos` (schema `bancos`) no conoce el concepto de cuenta contable en absoluto.
El precedente más cercano en el resto de la solución es `ArcaCliente` (schema `arca`), que ya
modela cuenta contable y centro de costo para exportar comprobantes a PRESEA
(`arca.PreseaProveedores.CuentaContableProveedor` / `CuentaDebe` / `Centro`, texto libre, con una
cascada default→override resuelta en `PreseaExportResolver`). Es un módulo distinto (proveedores
fiscales, no bancos), así que no se reusa código, pero confirma el patrón ya aceptado en esta
solución: **cuenta contable como texto libre, no como FK rígida a un plan de cuentas normalizado
del sistema legacy** — el legacy es la fuente de verdad del plan de cuentas, acá solo se importa
una copia de trabajo.

El otro precedente relevante es el mecanismo de `ConceptoFinal`, ya resuelto y con un fix reciente
(commit `6af1bce`, ver `docs/superpowers/specs/2026-08-28-gestion-homologaciones-design.md`): es
una columna física editable a mano en la grilla, que el matcher completa por default pero nunca
pisa si el usuario la corrigió. Todo el diseño de "Cuenta final" (ítem 4) es ese mismo mecanismo
aplicado a un campo nuevo.

**Decisiones ya acordadas con el usuario** (antes de este documento):

- Import de cuentas legacy: **Excel/CSV con mapeo de columnas**, mismo patrón que
  `ImportacionService` ya usa para extractos — no un TXT de formato fijo como el de PRESEA.
- Cascada de "Cuenta final": **sin cascada**. La cuenta del perfil (ítem 2) es solo informativa;
  el único origen del default de `CuentaFinal` es la cuenta del concepto estándar (ítem 3).
- Matching de conciliación interna: **mismo importe, signo opuesto** (débito en un extracto =
  crédito en el otro), fecha igual o cercana.
- Persistencia de conciliación interna: **tablas nuevas paralelas**, no se toca
  `ConciliacionExternService` ni `bancos.ConciliacionSesiones` existentes.

## Ítem 1 — Catálogo `bancos.CuentasContables`

Tabla nueva:

```sql
CREATE TABLE bancos.CuentasContables (
    Id INT IDENTITY PRIMARY KEY,
    Cuenta NVARCHAR(50) NOT NULL,
    Descripcion NVARCHAR(300) NOT NULL,
    CentroCosto NVARCHAR(50) NULL
);
CREATE UNIQUE INDEX UX_CuentasContables_CuentaCentro
    ON bancos.CuentasContables(Cuenta, CentroCosto);
```

**Clave natural de import/upsert: `(Cuenta, CentroCosto)`**, no `Cuenta` sola — se asume que una
misma cuenta puede repetirse con distinto centro de costo en el export legacy (ej. "Gastos
bancarios" imputado a distintas áreas). Es una asunción a confirmar contra un export real antes
de implementar; si en la práctica la cuenta es única sin importar el centro, la clave se achica a
`Cuenta` sola sin impacto en el resto del diseño.

**Pantalla de importación** (nueva, `ImportarCuentasContablesForm` o similar): mismo esqueleto que
ya usa el importador de extractos — seleccionar archivo (Excel/CSV vía `ExcelDataReader`, igual
que `ImportacionService`), mapear qué columna es Cuenta / Descripción / Centro de costo,
previsualizar las primeras filas, confirmar. El import hace upsert por la clave natural (`MERGE`
o `SELECT` + `INSERT`/`UPDATE` en una transacción) para poder reimportar sin duplicar cuando el
legacy vuelva a exportar.

Capas:
- `Data/CuentaContableStorage.cs` (nuevo): `ObtenerTodas()`, `ObtenerPorId(id)`,
  `UpsertLote(IEnumerable<CuentaContable>)` (transacción).
- `Services/ImportacionCuentasContablesService.cs` (nuevo): lectura del Excel/CSV con
  `ExcelDataReader`, igual patrón que `ImportacionService.ImportarArchivo`.
- `Models/CuentaContable.cs` (nuevo): `Id`, `Cuenta`, `Descripcion`, `CentroCosto`, más una
  `DisplayName` calculada (`"{Cuenta} — {Descripcion}"`) para los combos de los ítems 2 y 3.

## Ítem 2 — Cuenta contable en el perfil

```sql
ALTER TABLE bancos.PerfilesBanco ADD IdCuentaContable INT NULL
    REFERENCES bancos.CuentasContables(Id);
```

`MainForm` (alta/edición de perfil) suma un combo `cmbCuentaContable`, opcional, poblado con
`CuentaContableStorage.ObtenerTodas()` vía `DisplayName`. Se guarda junto con el resto del perfil
en `PerfilBancoStorage.Insertar`/`Actualizar`.

Es **dato de referencia únicamente** — por la decisión ya acordada de "sin cascada", no participa
del cálculo de `CuentaFinal` en ningún movimiento. Sirve para que quede documentado con qué cuenta
contable opera ese banco, sin que compita con la cuenta del concepto estándar del ítem 3. Esto se
aclara en la UI con un texto de ayuda bajo el combo (ver mockup del ítem 2 en el Artifact).

## Ítem 3 — Cuenta contable en el concepto estándar

Este es el ítem que el propio pedido marca como el de "varias implicancias complicadas", y tiene
razón: hoy `bancos.ConceptosEstandar` **no tiene pantalla de mantenimiento** — el spec anterior de
este módulo (`2026-08-28-gestion-homologaciones-design.md`) lo dejó explícitamente fuera de
alcance ("los conceptos estándar no se dan de baja en este negocio; se crean solos al tipear uno
nuevo en `HomologarForm`"). Ese supuesto ya no alcanza: ahora hace falta editar un atributo del
concepto (la cuenta) después de creado, en bloque, sin pasar por una homologación puntual.

```sql
ALTER TABLE bancos.ConceptosEstandar ADD IdCuentaContable INT NULL
    REFERENCES bancos.CuentasContables(Id);
```

### Ventana nueva: `GestionConceptosEstandarForm`

Mismo look que `GestionHomologacionesForm` (grid `RadGridView` + botones `Nueva`/`Editar`/
`Eliminar` que abren un dialog aparte, sin edición inline — coherente con el resto del módulo).
Columnas: `Nombre` del concepto, `Cuenta contable` (código + descripción, o vacío), `Movimientos`
(conteo de uso, mismo cálculo que ya existe en `GestionHomologacionesForm` vía
`HomologacionAdminService.ContarUsoPorRegla`, agregado por concepto en vez de por regla). El
dialog de edición es simple: nombre (solo lectura, no se renombra un concepto desde acá — eso
sigue fuera de alcance) + combo de cuenta contable, que también puede quedar vacío.

Esta ventana es el mantenimiento masivo — cubre el caso de "ya se homologaron 40 conceptos sin
cuenta, ahora hay que asignárselas todas".

### `HomologarForm` suma el campo de cuenta

El pedido es explícito: "al momento de homologar ahora solo pide una descripción y ahora tiene
que pedir la cuenta". Se agrega un combo `cmbCuenta` junto al `cmbEstandar` existente, editable y
opcional. Al guardar (`btnGuardar_Click`), si el concepto estándar ya existía y el combo de cuenta
trae un valor distinto al que tenía, se actualiza `ConceptosEstandar.IdCuentaContable`.

**Implicancia a resolver, documentada como comportamiento esperado:** como el mismo nombre de
concepto estándar puede recibirse desde homologaciones distintas (perfiles distintos, o el mismo
perfil en momentos distintos), y `HomologarForm` deja fijar la cuenta ahí mismo, dos homologaciones
que fijan cuentas distintas para el mismo concepto van a hacer que **gane la última que se
guardó** — no hay validación de conflicto. Se propone documentarlo así para esta primera versión;
si el usuario prefiere que el campo quede bloqueado una vez asignado (y el ajuste posterior se
haga solo desde `GestionConceptosEstandarForm`), es un cambio menor de una condición en
`btnGuardar_Click`, a decidir antes de implementar.

## Ítem 4 — Columna "Cuenta final"

```sql
ALTER TABLE bancos.MovimientosArchivo ADD CuentaFinal NVARCHAR(50) NULL;
```

Mismo patrón que `ConceptoFinal`: columna física de texto libre (no FK — se puede escribir
cualquier cosa a mano, igual que hoy con el concepto), aparece sola en la grilla de
`ProcesadorForm` porque Telerik autogenera columnas por reflexión sobre `MovimientoProcesado`
(alcanza con agregar la propiedad `CuentaFinal` al modelo). `ConfigurarGrilla()` la marca como
editable, igual que `ConceptoFinal`; `DgvDatos_CellValueChanged` suma el `case` correspondiente
llamando a `MovimientoStorage.ActualizarCuentaFinal(id, valor)` (nuevo, mismo patrón que
`ActualizarConceptoFinal`).

### El punto técnico delicado

El default de `CuentaFinal` tiene que completarse en el mismo momento en que se resuelve
`ConceptoFinal` — al importar (`ImportacionService`) y al re-homologar
(`SesionMovimientosService.ReaplicarHomologacion`) — y respetando ediciones manuales, igual que
`HomologacionMatcher.EscribirConcepto` hace hoy con `ConceptoFinal`. Eso obliga a tocar dos
lugares que hoy solo conocen el nombre del concepto:

1. **`HomologacionStorage.ObtenerDiccionario`** hoy devuelve `Dictionary<string, string>`
   (`ValorOriginal` → nombre del concepto). Tiene que devolver también la cuenta asociada al
   concepto — por ejemplo `Dictionary<string, (string Concepto, string Cuenta)>`, o una clase
   chica `ResultadoHomologacion { Concepto, Cuenta }` — sin romper el uso actual de
   `HomologacionMatcher.Resolver`/`ResolverClave`, que siguen operando sobre claves de texto para
   la resolución en sí.

2. **`HomologacionMatcher`** suma un análogo a `EscribirConcepto`:

   ```csharp
   /// Escribe CuentaFinal salvo que el usuario ya la haya editado a mano
   /// (mismo criterio que EscribirConcepto: solo pisa si estaba pendiente/vacía
   /// o si seguía a la cuenta del ConceptoEstandar anterior).
   public static void EscribirCuenta(MovimientoProcesado mov, string cuentaNueva);
   ```

   Se llama junto con `EscribirConcepto` en los mismos puntos: `ImportacionService` al armar cada
   `MovimientoProcesado`, y `SesionMovimientosService.ReaplicarHomologacion`/
   `HomologacionAdminService.AplicarBaja`/`Reapuntar` cuando recalculan en bloque.

Como no hay cascada al perfil (decisión ya tomada), si el concepto estándar no tiene cuenta
asignada, `CuentaFinal` queda vacía y el usuario la completa a mano igual que hace hoy con
`ConceptoFinal` cuando el concepto es nuevo.

## Ítem 5 — Conciliación interna entre extractos

Caso distinto del que ya resuelve `ConciliacionExternForm`: no hay archivo externo, sino un
movimiento que aparece en dos extractos propios — típicamente una transferencia entre cuentas del
mismo titular, débito en un banco y crédito en el otro por el mismo importe.

Tablas nuevas y paralelas (no se toca `ConciliacionSesiones`/`ConciliacionItemsExternos`/
`ConciliacionPares`, que siguen sirviendo solo a la conciliación con archivo externo):

```sql
CREATE TABLE bancos.ConciliacionInternaSesiones (
    Id INT IDENTITY PRIMARY KEY,
    Nombre NVARCHAR(200) NOT NULL,
    FechaCreacion DATETIME2(0) NOT NULL,
    ArchivosJson NVARCHAR(MAX) NOT NULL,   -- ids de los dos (o más) extractos en juego
    ConceptosJson NVARCHAR(MAX) NOT NULL,  -- ConceptoFinal(es) a conciliar, mismo criterio que la externa
    Estado NVARCHAR(50) NOT NULL DEFAULT 'EnProceso'
);

CREATE TABLE bancos.ConciliacionInternaPares (
    Id INT IDENTITY PRIMARY KEY,
    IdSesion INT NOT NULL REFERENCES bancos.ConciliacionInternaSesiones(Id),
    IdMovimientoA INT NOT NULL,   -- FK lógica a MovimientosArchivo.Id, sin constraint (mismo criterio que ConciliacionPares)
    IdMovimientoB INT NOT NULL,
    TipoMatch NVARCHAR(50) NOT NULL,       -- FechaImporte | SoloImporte | Manual (mismo enum TipoMatch)
    FechaConciliacion DATETIME2(0) NOT NULL
);
CREATE INDEX IX_ConciliacionInternaPares_IdSesion ON bancos.ConciliacionInternaPares(IdSesion);
```

**Ventana nueva `ConciliacionInternaForm`**, clonando el esqueleto de `ConciliacionExternForm`:
panel de sesiones, panel de alta (acá se eligen **dos extractos/perfiles** y el concepto a
conciliar, en vez de cargar un archivo externo), tabs de Pendientes/Conciliados con `RadGridView`
de solo lectura mostrando los dos extractos lado a lado, resaltado de candidatos por
`RowFormatting`, y los mismos botones: auto-conciliar, manual, desconciliar, finalizar, exportar.

**Matching automático** — variante de `ComparadorConciliacion` (o un
`ComparadorConciliacionInterna` nuevo si conviene no tocar el existente): en vez de
`ImporteEfectivo` igual en ambos lados, acá se busca **importe absoluto igual con signo
opuesto** — uno con el importe en `Debitos`, el otro en `Creditos` — y fecha igual o cercana. Dos
pasadas igual que la conciliación externa: 1) fecha+importe exacto, 2) solo importe entre lo que
quedó sin conciliar. Duplicados se resuelven con el mismo `SeleccionCandidatoDialog` ya existente.

No hay `ConciliacionInternaItemExterno` porque los dos lados ya son `MovimientoProcesado`
persistidos — el par se arma directo entre dos `Id` de `bancos.MovimientosArchivo`.

### Cuenta contable al cerrar la sesión (agregado tras la primera versión de este documento)

Al finalizar la sesión completa (no antes, no por par individual), cada par conciliado recibe
como `CuentaFinal` la cuenta contable del **perfil del otro lado** — la contrapartida de la
transferencia: el movimiento del extracto A queda con la cuenta del perfil del extracto B, y
viceversa. Se pisa aunque el usuario ya hubiera editado esa `CuentaFinal` a mano: cerrar la
conciliación es una acción deliberada, distinta del autocompletado pasivo que rige en el resto
del sistema. Si el perfil de algún lado no tiene cuenta contable asignada (ítem 2), se **bloquea
el cierre de toda la sesión** hasta que se complete — no finaliza nada a medias.

Esto es una excepción puntual, acotada a esta acción de cierre, a la regla general de "sin
cascada" del ítem 4 (esa regla rige el autocompletado por defecto al homologar/importar; acá el
disparador es otro: el cierre explícito de la conciliación interna).

### Edición manual bloqueada mientras está conciliada

Un movimiento con un par en una sesión interna todavía "EnProceso" no deja editar su `CuentaFinal`
a mano en la grilla del Procesador: como esa cuenta se va a pisar sola al cerrar la sesión (arriba),
editarla antes sería trabajo perdido, así que el sistema avisa y cancela la edición en vez de
dejarla y perderla en silencio. Deja de estar bloqueada si el par se desconcilia o si la sesión ya
se finalizó (no hay ningún cierre futuro pendiente que la vuelva a tocar).

### Selección por perfil + rango de fechas, no por archivo

Al armar una sesión, cada lado se define eligiendo un perfil de banco y un rango de fechas
`[Desde, Hasta]`, no tildando archivos importados puntuales — los movimientos de ese lado son los
del perfil elegido cuya fecha cae en el rango, recorriendo todo lo importado de ese perfil (no una
lista fija de `ArchivosImportados`). Como `MovimientosArchivo.Fecha` es texto libre (tal como lo
exporta cada banco), el filtro por rango se resuelve en memoria con el mismo parseo de fechas que
ya usa el resaltado de la conciliación externa, no con una condición de rango en SQL.

## Preguntas abiertas / a confirmar antes de implementar

1. **Clave natural del import de cuentas** (`Cuenta, CentroCosto` vs. `Cuenta` sola) — confirmar
   contra un export real del legacy.
2. **Conflicto de cuenta en `HomologarForm`** (ítem 3) — ¿gana la última homologación que la fija,
   o se bloquea una vez asignada y el ajuste posterior va solo por
   `GestionConceptosEstandarForm`?
3. Si en algún momento se decide que la cuenta del perfil SÍ debería servir de default cuando el
   concepto no tiene cuenta propia, es un cambio acotado (una condición más en
   `HomologacionMatcher.EscribirCuenta`) — queda anotado por si la decisión de "sin cascada"
   cambia con el uso real.

## Cómo evaluar esta propuesta

Los mockups de las 5 pantallas/cambios están en el Artifact publicado junto con este documento
(`cuentas-contables-mockups.html`). Con el visto bueno del usuario, este documento pasa a un plan
de implementación (`docs/superpowers/plans/`) con número de `TAREA` asignado desde
`~/.claude/TAREAS.md`, y ahí sí se listan los archivos a tocar con su checklist de verificación
manual, siguiendo el mismo formato que
`docs/superpowers/specs/2026-08-28-gestion-homologaciones-design.md`.
