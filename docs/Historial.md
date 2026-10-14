# Historial de tareas — ConciliadorContable

Bitácora de este repo: qué se pidió y qué se ejecutó, tarea por tarea. El correlativo es
**general a todos los proyectos** y vive en `~/.claude/TAREAS.md`; acá queda la mitad
versionada en git.

El formato de los comentarios en el código y las reglas de la convención están en la sección
"Versionado de cambios" de `CLAUDE.md`.

**El compromiso es de las dos partes**: un cambio hecho a mano, sin pasar por Claude, también
se anota acá y en el registro global. Si no, el correlativo miente.

---

## TAREA 00003 — 28/08/2026

### Lo que se pidió

> 1. Al dar de baja una homologación, preguntar qué hacer con los ítems ya homologados:
>    dejarlos pendientes o cambiarlos a otro concepto.
> 2. Alta y edición de homologaciones desde la misma pantalla.
> 3. Que agrupe bien por perfil.
> 4. Que al invocarla filtre por el perfil desde el cual se la llama.
> 5. En el diálogo de baja, detallar cómo afecta a los archivos importados para poder medir
>    el impacto.
> 6. Analizar el `HomologarForm` que se abre con el doble click: si también afecta a todo,
>    debe tener un tratamiento similar al de la gestión.

Diseño en `docs/superpowers/specs/2026-08-28-gestion-homologaciones-design.md`,
plan en `docs/superpowers/plans/2026-08-28-gestion-homologaciones.md`.

### El problema de fondo

El concepto homologado no vive sólo en la regla: al importar se escribe **como texto** en
`MovimientosArchivo.ConceptoEstandar` y `ConceptoFinal`. La baja era un `DELETE` seco, así que
los movimientos ya homologados quedaban con el concepto viejo sin ninguna regla que lo
respaldara — ni pendientes ni reasignados, y nadie los volvía a mirar porque
`RehomologarPendientes` sólo toca los que están en `Pendiente Homologar`.

Y como en los perfiles de texto libre el match es por substring, dos reglas distintas pueden
producir el mismo concepto. Decidir el impacto comparando textos habría despegado movimientos
que otra regla sigue cubriendo.

### Lo que se ejecutó

**Línea 1 · Baja con impacto real**

- `Services/HomologacionMatcher.cs` — `ResolverClave` devuelve qué regla gana, no sólo el
  concepto; `Resolver` pasa a implementarse sobre ella, así hay una sola definición de "cuál
  gana" y el impacto no puede discrepar de lo que hizo la importación.
- `Models/ImpactoHomologacion.cs`, `Services/HomologacionAdminService.cs` (nuevos) — calculan
  qué movimientos pierden cobertura al borrar una regla y cuáles sigue tomando otra, y propagan
  la decisión sobre **todos los archivos del perfil**, no sólo el que esté abierto.
- `BajaHomologacionDialog.cs` (nuevo) — el diálogo con el resumen del impacto y las dos
  salidas: dejarlos pendientes o reasignarlos a otro concepto.
- `Data/HomologacionStorage.cs` — `EliminarYActualizarMovimientos`: la regla y sus movimientos
  en una sola transacción. Se borra `Eliminar`, que quedó sin llamadores.
- `Data/MovimientoStorage.cs` — `ObtenerPorPerfil` y la sobrecarga de `ActualizarConceptos` que
  escribe dentro de una transacción ajena.

`ConceptoFinal` editado a mano nunca se pisa: sólo se actualiza si estaba pendiente o si venía
igual al `ConceptoEstandar` viejo.

**Línea 2 · Alta y edición**

- `HomologarForm.cs` — `BloquearValorOriginal` y `SoloSeleccionar` para el modo edición, y
  validación del valor del banco (con el botón "Nueva" el form abre en blanco, y un valor
  vacío haría match por substring con todo).
- `Data/HomologacionStorage.cs` — `ReapuntarYActualizarMovimientos` hace `UPDATE` en vez del
  `DELETE`+`INSERT` de `Guardar`, así el `Id` de la regla sobrevive a la edición;
  `ObtenerOCrearConcepto` extraído para reusarlo entre ambos.

La edición no permite cambiar el `ValorOriginal`: `Guardar` pisa la homologación previa
borrando por `(IdPerfilBanco, ValorOriginal)`, así que cambiar la clave dejaría viva la regla
vieja. Para eso, eliminar y dar de alta.

**Línea 3 · Agrupación por perfil**

- `Models/HomologacionListado.cs` — suma `IdPerfilBanco`, `IdConceptoEstandar` y `Movimientos`.
- `Data/HomologacionStorage.cs` — `ObtenerListado` con filtro opcional y orden
  `ValorOriginal DESC`, el mismo del diccionario del matcher: la grilla se lee como la
  directiva de precedencia que efectivamente rige.
- `GestionHomologacionesForm` — agrupación por banco cuando no hay perfil elegido, y columna
  con cuántos movimientos resuelve cada regla.

**Línea 4 · Filtro por el perfil que la invoca**

- `GestionHomologacionesForm` — combo de perfil precargado, centinela `(Todos los perfiles)`,
  y `HuboCambios` para avisarle al llamador.
- `ProcesadorForm.cs` — pasa `_perfilEnGrilla` (o el del combo si no hay sesión cargada) y
  refresca al volver.
- `Services/SesionMovimientosService.cs` — `RefrescarDesdeBase` copia los conceptos sobre las
  instancias ya bindeadas, sin rebindear, para no mover al usuario de la fila donde estaba.

**De paso:** los `.Designer.cs` reescritos pasan a UTF-8 con BOM. Estaban en cp1252 sin BOM y
compilaban bien sólo porque Roslyn, ante bytes que no son UTF-8 válido, cae al codepage ANSI
del sistema — que en esta máquina es cp1252. Funcionaba por coincidencia del equipo; ahora no
depende de eso.

**Línea 5 · Desglose del impacto por archivo**

- `Models/ImpactoHomologacion.cs` — `ImpactoPorArchivo`: por archivo importado, cuántos quedan
  sin regla, cuántos toma otra, y los débitos y créditos de los que efectivamente cambian.
- `Services/HomologacionAdminService.cs` — `DesglosarPorArchivo` reparte un impacto ya
  calculado entre los archivos del perfil. No vuelve a leer movimientos: agrupa por
  `IdArchivo` lo que el cálculo del impacto ya trajo.
- `BajaHomologacionDialog` — grilla con ese desglose arriba de la pregunta.

El total suelto no alcanzaba para decidir: 28 movimientos del mes en curso no es lo mismo que
28 repartidos sobre tres cierres ya conciliados. Los importes son sólo los de los movimientos
sin regla, que son los únicos que la baja cambia.

**Línea 6 · La re-homologación desde la grilla**

El análisis mostró que el doble click **no** afectaba a todo: la regla que guarda es del
perfil, pero la propagación tocaba sólo el archivo abierto. Se decidió **dejar ese alcance
como estaba** — es la intención buscada, "reclasificar de acá en adelante y arreglar el
archivo que tengo abierto" — y corregir dos bugs que ocurrían dentro de ese archivo:

- El despegue hacía `ConceptoFinal = Pendiente Homologar` sin mirar, y después el matcher lo
  reescribía: una corrección hecha a mano en la grilla se perdía en silencio.
- El despegue filtraba por texto del concepto (`mov.ConceptoEstandar != conceptoADespegar`),
  así que alcanzaba también a movimientos de otras reglas que apuntan al mismo concepto.

Qué se tocó:

- `Services/HomologacionMatcher.cs` — `EscribirConcepto`: la regla de "cómo se escribe un
  concepto en un movimiento" pasa a estar en un solo lugar. Estaba duplicada en `AplicarA` y
  en el despegue, con criterios distintos. `AplicarA` y `HomologacionAdminService` la usan.
- `Services/SesionMovimientosService.cs` — se elimina el parámetro `conceptoADespegar` y su
  bloque. En su lugar, `MovimientosDeLaMismaRegla` (atribución por regla ganadora) y
  `ReaplicarHomologacion`, que escribe el concepto directo sin pasar por un estado pendiente
  intermedio donde la edición manual se perdería.
- `ProcesadorForm.cs` — el conjunto a re-resolver se calcula **después** de guardar, con el
  diccionario ya actualizado. Así entran también los movimientos que arrastraban un concepto
  huérfano de una regla borrada, que el despegue por texto sí recuperaba. Sólo corre en el
  caso re-homologación, el que el usuario confirma: en el alta sobre un pendiente alcanza con
  el barrido de pendientes, para no tocar por sorpresa movimientos ya resueltos.

### Riesgo anotado, no resuelto

`bancos.ConciliacionSesiones.ConceptosJson` guarda los `ConceptoFinal` elegidos al armar una
conciliación. Una baja que cambie el `ConceptoFinal` de movimientos que participan de una
sesión guardada puede dejar esa sesión filtrando por un concepto que ya no existe en los datos.

### Verificación

Sin proyecto de tests (decisión explícita): se verificó con `dotnet build` limpio en cada paso.

---

## TAREA 00022 — 05/09/2026

**Nota de numeración**: todos los comentarios de versionado en el código de esta tarea dicen
`TAREA: 00021` — es el número que indicaba `~/.claude/TAREAS.md` al arrancar, pero una sesión
concurrente en otro repo (ConsultasGenericas) ya lo había tomado antes de que se registrara acá.
Mismo criterio que ya usó este registro para la misma situación en las TAREA 00015/00019: se
documenta con el número real de esta fila (00022) sin reescribir los ~30 commits ya hechos con
`00021`. Si buscás esta tarea en el código, buscá `TAREA: 00021`, no `00022`.

### Lo que se pidió

> 1. Importar cuentas contables de sistema legacy, sería cuenta y descripción y centro de costo.
> 2. Asignar cuenta contable a perfil.
> 3. Asignar cuenta contable a concepto standard. Acá vamos a tener que crear una ventana para
>    esto ya que no hay un mantenimiento del concepto standard. Al momento de homologar ahora
>    solo pide una descripción y ahora tiene que pedir la cuenta. La misma puede estar vacía.
> 4. Crear una columna de cuenta final así como está la de concepto final. Por defecto lleva
>    la del concepto standard.
> 5. Creación de ventana para conciliación interna de un concepto entre extractos, las
>    mediciones son por fecha + importe automáticos y manual. Copiar el formato de
>    conciliación con archivo externo.

Diseño en
`docs/superpowers/specs/2026-09-05-cuentas-contables-perfil-concepto-conciliacion-interna-design.md`,
con mockups previos evaluados con el usuario antes de planificar. Dos planes de implementación,
ejecutados en orden porque el segundo depende de piezas del primero:
`docs/superpowers/plans/2026-09-05-cuentas-contables.md` (ítems 1-4, 9 tareas) y
`docs/superpowers/plans/2026-09-05-conciliacion-interna.md` (ítem 5, 7 tareas). Ejecutados con
`superpowers:subagent-driven-development`: un subagente implementador por tarea, review de spec
+ calidad por tarea, y fix cuando el review encontraba algo — el detalle de cada hallazgo está
en el historial de revisión de cada tarea, acá va el resumen que le sirve a alguien que lea esto
después.

**Líneas 1-2 · Catálogo de cuentas contables y cuenta en el perfil**

- `bancos.CuentasContables` (Cuenta, Descripción, CentroCosto), con upsert por la clave natural
  `(Cuenta, CentroCosto)` para poder reimportar el export del legacy sin duplicar.
- Pantalla nueva `CuentasContablesForm`: listado + panel de import inline (Excel/CSV con mapeo
  de columnas), mismo patrón que ya usa el importador de extractos.
- `PerfilesBanco.IdCuentaContable`: **solo informativo**. Decisión explícita del usuario: no
  alimenta ningún cálculo de `CuentaFinal` — esa cuenta sale únicamente del concepto estándar
  (ítem 4). El combo vive en `MainForm` (alta/edición de perfil).

**Línea 3 · Cuenta en el concepto estándar, con ventana de mantenimiento nueva**

`bancos.ConceptosEstandar` no tenía pantalla propia — se creaba al vuelo tipeando un nombre
nuevo en `HomologarForm`. Se agregó `IdCuentaContable` a la tabla y dos entradas para asignarla:

- Ventana nueva `GestionConceptosEstandarForm` (accesible desde "Gestión de Homologaciones"):
  grid de conceptos con su cuenta asignada (o vacía) y un diálogo chico para cambiarla. Es el
  mantenimiento masivo que no existía; renombrar/borrar un concepto sigue fuera de alcance,
  como ya estaba decidido en la TAREA 00003.
- `HomologarForm` suma un combo de cuenta junto al de concepto estándar, opcional.

**Línea 4 · Columna "Cuenta Final"**

Mismo mecanismo que `ConceptoFinal`: columna física (`MovimientosArchivo.CuentaFinal`),
editable inline en la grilla del Procesador, autocompletada desde la cuenta del concepto
estándar en los mismos puntos donde ya se resuelve el concepto (import, re-homologación, baja y
reapuntado de una regla). La regla de "no pisar lo editado a mano" es más simple que la de
`ConceptoFinal`: como no hay una `CuentaEstandar` que trackee el último valor resuelto por el
sistema, `CuentaFinal` se autocompleta **solo mientras está vacía** — una vez que tiene
contenido, sólo cambia si el usuario la edita.

**Línea 5 · Conciliación interna entre extractos propios**

Ventana nueva `ConciliacionInternaForm` para el caso de una transferencia entre cuentas propias
(débito en un extracto, crédito en el otro, mismo importe). Copia el formato de la conciliación
con archivo externo (sesiones, pestañas Pendientes/Conciliados, resaltado de candidatos,
auto-conciliar en dos pasadas), en tablas nuevas y paralelas
(`ConciliacionInternaSesiones`/`ConciliacionInternaPares`) que no tocan el código de la
conciliación externa ya en producción. Dos decisiones que surgieron después de armar el primer
plan, agregadas antes de implementar:

- Cada lado de la sesión se elige por **perfil + rango de fechas**, no tildando archivos
  importados puntuales.
- Al **finalizar** una sesión, cada movimiento conciliado recibe como `CuentaFinal` la cuenta
  contable del **perfil del otro lado** (la contrapartida de la transferencia), pisando incluso
  una edición manual — es la única excepción a la regla de la Línea 4, porque acá el disparador
  es una acción explícita de cierre, no un autocompletado pasivo. Si algún perfil no tiene
  cuenta asignada, se bloquea el cierre completo. Mientras un movimiento está conciliado en una
  sesión todavía en proceso, su `CuentaFinal` no se puede editar a mano en el Procesador (se
  avisa y se cancela la edición) — se libera al desconciliar o al finalizar.

### Bugs reales encontrados en el proceso de revisión

Ninguno llegó a la rama sin corregir, pero quedan anotados porque son la clase de error que
`dotnet build` no detecta:

- **`//` como comentario dentro de SQL**: el DDL de `SqlSchema.cs` es texto T-SQL embebido en un
  string de C#; `//` no es un comentario válido ahí (T-SQL usa `--`) y como `DatabaseHelper`
  ejecuta todo `SqlSchema.Ddl` como un solo batch, un error de sintaxis ahí rompía la
  inicialización de **todo** el schema `bancos` en una base nueva, no solo la tabla nueva.
  Corregido apenas apareció (Tarea 1 del primer plan) y evitado en el resto de las tareas.
- **Índice único sobre una expresión inválida**: `CREATE UNIQUE INDEX ... (Cuenta,
  ISNULL(CentroCosto, N''))` no es sintaxis válida de SQL Server (la lista de columnas de un
  índice no acepta expresiones). Mismo riesgo que el bug anterior (rompía todo el batch).
  Corregido a un índice compuesto simple `(Cuenta, CentroCosto)` — SQL Server ya trata dos
  `CentroCosto` NULL como iguales a los fines de unicidad, así que el efecto práctico es el
  mismo.
- **La cuenta se descartaba en silencio al editar sin renombrar el concepto**: en
  `GestionHomologacionesForm.btnEditar_Click`, un early-return pre-existente cortaba antes de
  persistir la cuenta cuando el usuario dejaba el mismo nombre de concepto — exactamente el
  caso de uso principal ("solo quiero fijar la cuenta"). Corregido para persistir la cuenta
  siempre, reapuntando la regla solo si el nombre del concepto realmente cambió.
- **`RefrescarDesdeBase` no sincronizaba `CuentaFinal`**: después de una baja/reapuntado desde
  la gestión de homologaciones, una grilla ya abierta en el Procesador no reflejaba la cuenta
  recién resuelta hasta reabrir el archivo (el dato en la base sí quedaba correcto). Encontrado
  por el propio implementador antes de que llegara a review. Fix de una línea.
- **Botones de la ventana de conciliación interna clippeables al achicar la ventana**: un primer
  intento de arreglo infló `MinimumSize`, pero el cálculo olvidaba el chrome no-cliente de la
  ventana (barra de título + bordes) y seguía clippeando ~19px. Solución definitiva: anclar los
  botones al borde inferior real del panel (`Anchor = Bottom | Right`) en vez de calcular a ojo
  un tamaño mínimo que los evite.

### Corrección post-merge (05/09/2026): la Línea 5 quedó mal diseñada, no solo con un bug visual

Ya mergeado y pusheado a `main`, el usuario probó `ConciliacionInternaForm` corriendo de verdad
y reportó (con captura) dos problemas, uno de implementación y uno de diseño:

- **Bug real de z-order**: `pnlConfigNueva` se agregaba último a `Controls`, y en WinForms el
  último control agregado queda **atrás** del z-order de pintado (al revés de lo intuitivo). Al
  mostrarse, el panel de alta se dibujaba detrás de la lista de sesiones y de los botones en vez
  de taparlos, superponiendo todo. Ninguna revisión (todas basadas en `dotnet build` + lectura de
  código) lo iba a detectar porque nadie corrió la ventana. Fix: `pnlConfigNueva.BringToFront()`.
- **El diseño de "perfil + rango por lado" (línea 226 más arriba) estaba mal**: el usuario aclaró
  que fue un error de redacción suyo en el pedido original. El diseño correcto, que reemplaza al
  anterior:
  - La sesión se crea con **un solo rango de fechas**, sin elegir perfil ni extracto.
  - Los dos paneles de la pestaña Pendientes son **Débitos** y **Créditos globales** — de
    cualquier extracto/perfil que caiga en el rango y los conceptos elegidos — no "Extracto A" /
    "Extracto B" atados a un perfil puntual.
  - La auto-conciliación **no empareja dos movimientos del mismo extracto** (`IdArchivo` igual):
    se excluye explícitamente en las dos pasadas del matching.
  - Como una sesión ya no tiene un perfil fijo por lado, `Finalizar` resuelve el perfil (y su
    cuenta contable) **por par**, a través del `IdArchivo` de cada uno de los dos movimientos
    conciliados — antes lo resolvía una sola vez para toda la sesión.

Se reescribieron `ConciliacionInternaSesion` (ya no guarda `IdPerfilA/B` ni rangos por lado, solo
`FechaDesde`/`FechaHasta`), `ConciliacionInternaPar` (recupera `IdArchivoA/B`, necesarios para
`Finalizar`), `ConciliacionInternaService` completo (`CrearSesion`, el armado de los pools de
pendientes, `AutoConciliar`, `ObtenerPares`, `Finalizar`) y las dos mitades de
`ConciliacionInternaForm` (panel de alta con un rango único, paneles "Débitos"/"Créditos" en vez
de "Extracto A"/"Extracto B"). Se agregó `MovimientoStorage.ObtenerTodos()` porque los pools ya
no se arman por perfil. La tabla `ConciliacionInternaSesiones` se recreó (no había datos reales
todavía) en vez de migrar columna por columna.

### Segunda ronda de correcciones tras pruebas en vivo (07–11/09/2026)

Con el diseño corregido ya funcionando, el usuario siguió probando la ventana real contra un
servidor real y aparecieron tres ajustes más, más una duda todavía sin resolver:

- **Columna "Extracto" en los paneles y en el desempate manual**: como Débitos/Créditos ahora
  mezclan cualquier banco, dos candidatos con la misma fecha e importe eran indistinguibles a
  simple vista (el usuario lo reportó con una captura del diálogo de desempate). Se agregó una
  columna "Extracto" (banco — archivo) en `dgvDebitos`/`dgvCreditos` (vía un wrapper
  `MovimientoConciliable` liviano, ya que `RadGridView` genera las columnas por reflexión) y en
  `SeleccionCandidatoInternoDialog`, más el nombre del extracto de origen en su texto de
  cabecera. Nuevo `ConciliacionInternaService.ObtenerNombresExtracto()`.

- **Bug real: un mismo movimiento quedaba conciliado dos veces en la misma sesión** — encontrado
  por el usuario en producción (dos pares con el mismo `IdMovimientoB`, visto directo en una
  consulta SQL). Causa: `ConciliarPar` sólo chequeaba conflictos contra *otras* sesiones
  `EnProceso` (a propósito, para no bloquearse a sí misma), pero nada impedía repetir el mismo
  movimiento dentro de la propia sesión; y el loop de desempate de Auto-conciliar
  (`btnAutoConciliar_Click`) resolvía cada duplicado contra una lista de candidatos ya vieja, sin
  descontar lo que un desempate anterior del mismo click ya había asignado. Fix en dos capas:
  - Capa de datos (la garantía dura): dos índices únicos nuevos,
    `UX_ConciliacionInternaPares_MovimientoA/B` sobre `(IdSesion, IdMovimientoA)` y
    `(IdSesion, IdMovimientoB)`, con un *dedupe* previo en el mismo `SqlSchema.cs` (conserva el
    par más viejo de cada duplicado) para no romper la inicialización sobre una base que ya tenía
    el duplicado real. `ConciliarPar` atrapa la violación (SQL 2601/2627) y la devuelve como
    mensaje legible en vez de reventar.
  - Capa de UI: `btnAutoConciliar_Click` lleva un `HashSet<int>` de créditos ya asignados durante
    el mismo click, filtra los candidatos de cada desempate contra él antes de abrir el diálogo,
    y cuenta como "pendiente" al que se quedó sin candidatos válidos.

- **Se perdió el "ocultar todo" al crear una sesión nueva**: un edit a mano (fuera de esta sesión
  de Claude) había agregado `.Hide()` de algunos controles en `btnNuevaSesion_Click`, pero sin el
  `.Show()` de vuelta en Confirmar/Cancelar, y sin cubrir la pestaña Pendientes/Conciliados ni la
  barra de botones inferior (que el panel de alta nunca tapó geométricamente, en ninguna
  versión — ni siquiera la original). Se reemplazó por `MostrarPanelNuevaSesion(bool)`,
  simétrico, que oculta/muestra explícitamente todo lo que no es el panel de alta.

- **Duda del usuario, todavía sin acción de código**: reportó que "cada vez que pone una versión
  nueva" se pierde la tabla `Usuarios`. Diagnóstico: no es una migración de SQLite a SQL (ese
  módulo, `ConciliadorContable`, nunca migró; `Auth/DatabaseHelper.InitializeDatabase()` es
  no-destructivo, sólo `CREATE TABLE IF NOT EXISTS`/`INSERT OR IGNORE`). Causa más probable:
  `conciliador.db` vive en `AppContext.BaseDirectory` — dentro de la misma carpeta que el build —
  así que cualquier deploy que limpie esa carpeta antes de copiar el build nuevo se lo lleva
  puesto (`LiquidacionesAuditar` tiene el mismo patrón). Propuesta — moverlo a
  `%ProgramData%\ConciliadorContable\`, migrando el archivo existente una sola vez — **todavía no
  implementada**: el usuario va a probar primero si su proceso de deploy realmente toca el
  archivo antes de decidir.

### Verificación

Sin proyecto de tests (decisión explícita): cada tarea se verificó con `dotnet build` limpio y
un review de spec + calidad; esta entrada resume los hallazgos reales, no repite el detalle
tarea por tarea. Las dos rondas de correcciones post-merge muestran el límite real de ese
proceso: ni una sola de las revisiones automatizadas corrió la ventana — todos los hallazgos de
ambas rondas salieron de pruebas en vivo del usuario contra un servidor real. **Pendiente**:
seguir probando `ConciliacionInternaForm` (en particular el fix del doble-conciliado y el de
ocultar/mostrar al crear sesión) antes de darlo por cerrado.

## TAREA 00039 — 14/09/2026

### Lo que se pidió

> 1. Verificar el error que daba al importar los archivos del banco hipotecario.

### Lo que se hizo
**Línea 1** : se verifico que dentro de la configuracion falta el separador del archivo. 
se incorpora una configuracion en la linea del csvreader para fijar el separador en punto y coma
### Pendiente 
Hay que hacer una configuracion para que no sea fija. por ahora esta funcionando. 
-
## TAREA 00041 — 16/09/2026 (líneas 2 a 7: 21/09/2026)

### Lo que se pidió

> 1. Poder Conciliar con archivos de ARCA sacados de los sistemas de retenciones y percepciones.
>    Para hacer esto vamos a hacer una copia del sistema de conciliación de ARCA OffLine con
>    directivas y archivos xlsx, xls como fuente de datos y archivo CSV de PRESEA.
> 2. Tenemos que colocar en el perfil qué cuenta significa percepción y cuál retención.
> 3. Hacer como con ARCA una carpeta donde vamos a tomar todo el contenido como base de datos.
> 4. Levantar los archivos que van a contener las percepciones y las retenciones del sistema. A
>    diferencia de ARCA, acá viene el tipo de comprobante y el documento en la misma casilla de
>    concepto, una columna de debe y otra de haber.
> 5. Que deje tomar dos archivos de PRESEA, uno por cada cuenta configurada.
> 6. Directivas de conciliación.
> 7. Conciliación. "Solo debe conciliar contra las cuentas subidas en el archivo de PRESEA. Por
>    ejemplo en mi ejemplo solo tenemos percepciones."

Diseño previo en `docs/Diseño/00041-conciliacion-pyr/`: guía, dos flujogramas y 14 maquetas.

### Lo que se hizo

**Línea 1** (hecha a mano por el usuario, antes de esta sesión): copia inicial de los modelos de
ARCA Offline en `Models/Conciliacion PyR/`. Al implementar se reemplazó por `Models/PyR/`: se
borró `PerfilFiscalPyR` (era el perfil online, con clave fiscal y API, que acá no se usa) y se
sacó del `.csproj` una carpeta `<Folder>` que apuntaba a un directorio inexistente.

**Línea 2** — `PerfilOfflinePyR` con una lista de `CuentaPyR` (código, nombre, percepción o
retención, e impuesto IVA/IIBB/Ganancias), en la tabla nueva `arca.ArcaPerfilesPyR` (cuentas y
directivas en JSON, como `ArcaPerfilesOffline`). Pantallas: lista de perfiles, detalle con la
grilla de cuentas y diálogo de cuenta. El perfil quedó más chico que la copia: PRESEA exporta el
mayor siempre en Excel, así que se sacaron tipo de archivo, separador, encoding y las posiciones
`Pos*`; sin cabecera, las mismas casillas de columna se cargan con el número de columna.

**Línea 3** — `ArcaPyRImporter` lee toda la carpeta de ARCA. **El formato se detecta por
contenido, no por extensión**, porque los dos archivos vienen como `.xls` pero sólo uno lo es: el
de IVA es un Excel binario (BIFF, que ClosedXML no abre — se agregó ExcelDataReader 3.8.0) y el de
IIBB es una tabla HTML en UTF-8 sin BOM ni charset declarado (si se adivina el encoding se lee
"RetenciÃ³n" y la operación deja de reconocerse). El número se normaliza igual de los dos lados
(sólo dígitos, sin ceros a la izquierda). Un archivo no reconocido no corta la carga.

**Línea 4** — `PreseaPyRImporter` levanta un mayor por cuenta, elegida al agregar el archivo. El
concepto se separa con `^(SEGUN|POR ANULACION)\s+(tipo)\s+(número)(?:\s+de\s+(proveedor))?$`:
la parte del proveedor es opcional porque **las anulaciones no la traen** (22 en julio), y con el
patrón original habrían quedado como filas sin comprobante. Lo que no respeta el patrón (17
minutas financieras en julio) se carga marcado y resaltado, no se descarta.

**Línea 5** (pedida por el usuario después de probarlo) — el diálogo de elegir cuenta arranca en
la primera cuenta **sin mayor cargado** y marca las que ya tienen archivo. Antes arrancaba siempre
en la primera y el segundo archivo pedía reemplazar al primero.

**Línea 6** — `DirectivaPyR` con cinco campos propios de PyR (número, número últimos 8 dígitos,
importe, fecha, proveedor): el mayor de PRESEA no tiene CUIT ni punto de venta, así que las
directivas de Offline no servían. Tres por defecto: número completo (fija), últimos 8 dígitos más
importe, e importe más fecha más proveedor. La de los últimos 8 dígitos existe porque AFIP no
siempre escribe el número como PRESEA (DREAMCO `166000442096` contra `16600442096`; ALIMENTOS
SARANDI `315520`, sin punto de venta, contra `10200315520`). Se editan desde Perfiles PyR y desde
la propia pantalla de conciliación.

**Línea 7** — `ConciliacionPyRService`: alcance por las cuentas con mayor cargado (lo pedido: las
392 retenciones de IIBB de ARCA quedan fuera y sólo se informan), exclusión de las filas de ARCA
con importe 0 (6 notas de crédito de IIBB que PRESEA no registra; decisión del usuario),
neutralización de las anulaciones que suman cero con su factura, y las directivas en orden con
comparación de importe exacta al centavo. El resultado va en una pestaña nueva con resumen por
cuenta, aviso de lo excluido, filtro por estado y colores; caduca solo si cambia la carpeta, un
mayor, las directivas o el perfil. Exportación a Excel con hojas Detalle y Resumen.

Entrada en el menú principal del shell (`FormMenuPrincipal`), no en el `FormMenu` de ArcaCliente,
que no se instancia en ningún lado: dos botones ("Percepciones y Retenciones" y "Perfiles PyR")
con el permiso nuevo `ArcaPyR`, que nadie tiene hasta que se lo otorguen.

### Verificación

Sin proyecto de tests (decisión del repo). Se verificó así:

- **Importadores y servicio** contra los cuatro archivos reales de julio 2026, con una consola
  descartable: ARCA 3851 registros; PRESEA IVA 2056 filas (17 sin comprobante, 22 anulaciones);
  los números coinciden entre los dos lados en 1750 de 2039 filas de IVA y 901 de 984 de IIBB.
- **Conciliación** contra `_fuente/simulacion-julio.py`, una simulación en Python de las reglas de
  la guía escrita antes del código: **los números dieron idénticos** — IVA 1827 conciliados / 24
  diferencias / 718 sólo ARCA / 161 sólo PRESEA / 44 anuladas; IIBB 838 / 20 / 26 / 118 / 8; por
  directiva 2530, 91 y 88. Además, cada fila de ARCA y de PRESEA aparece exactamente una vez.
- **Pantallas**, con capturas contra una base LocalDB descartable comparadas con las maquetas:
  guardar y releer el perfil, estados vacío y de error, conciliar, filtrar, exportar (se abrió el
  Excel generado) y la caducidad del resultado al quitar un mayor.
- El servidor compartido (`192.168.7.51`) no se tocó en ningún momento.

**Pendiente de prueba real**: los botones que abren diálogos (elegir archivo, guardar el Excel) y
el uso completo en la aplicación, que el usuario ya probó por su cuenta antes del cierre.

### Decisiones y desvíos

- **Revisión de Codex salteada por decisión del usuario** ("podemos saltear la revisión porque es
  lo que acabo de hacer"). La convención pide hasta 2 rondas; no se hizo ninguna y no hay archivos
  en `docs/Revisiones/`. Queda anotado para que no parezca un paso olvidado.
- **Flujograma y maquetas los hizo Claude, no Codex**, que estuvo sin cuota hasta el 23/09.
- **Archivos repetidos en la carpeta de ARCA**: se saltean por contenido idéntico. El portal baja
  el reporte como `Listado - <fecha>.xls`; bajarlo dos veces duplicaba en silencio todos los
  importes de IIBB (pasó con la carpeta de ejemplo).
- El combo de perfil de la pantalla permite cambiarlo y descarta lo cargado, con confirmación.
- Una directiva sin campos se rechaza en su propio diálogo, no al aceptar la lista como mostraba
  la maqueta `05-directivas-pyr-error`.

### Pendiente

- Al primer uso se crea la tabla `arca.ArcaPerfilesPyR` en el servidor compartido.
- IIBB dejó 118 filas "Sólo PRESEA" de proveedores que no están en la consulta de Santa Fe
  (MASTELLONE, PONDESUR): puede ser que esa cuenta incluya percepciones de otras provincias.
- El separador fijo del CSV de la TAREA 00039 sigue sin ser configurable (no es de esta tarea).

## TAREA 00046 — 23/09/2026

### Lo que se pidió

> 1. Estaba haciendo unos cambios en la importación para agregar un campo operador, ya tengo todo
>    hecho. También agregué campo nuevo en la tabla de perfiles.
> 2. Tengo que cambiar el archivo y no me lee los encabezados
>    (`presea\iibb202607.XLSX`; en la tabla el perfil está ajustado a lo que viene en el archivo).
> 3. Se me desconfiguró la pantalla cuando entré en FormConciliacionPyR.

Sin diseño previo: son ajustes sobre pantallas que ya existen, sin cambio de circuito.

### Lo que se hizo

**Línea 1** — El campo operador lo agregó el usuario a mano (modelo, perfil, grilla, storage y
columna en el `CREATE TABLE`). Acá se completó con dos correcciones que impedían que funcionara:

- `PreseaPyRImporter.LeerFila` buscaba la columna como `Valor("Operador")`, pero la clave del
  diccionario es `"operador"`: lanzaba `KeyNotFoundException` en la primera fila, y ese tipo de
  excepción no lo atrapa la pantalla, así que el error salía como cuelgue y no como aviso.
- Faltaba el `ALTER TABLE`. El DDL del módulo crea las tablas sólo `IF OBJECT_ID(...) IS NULL`,
  así que en una base donde el módulo ya se usó (la de producción, desde la TAREA 00041) la
  columna `ColOperador` nunca se creaba y la lectura del perfil fallaba. Se agregó un `ALTER`
  idempotente con `IF COL_LENGTH(...) IS NULL`.

Por decisión del usuario el operador es **obligatorio**: si el perfil no lo mapea o el archivo no
lo trae, el mayor se rechaza por columnas faltantes.

**Línea 2** — El export nuevo de PRESEA tiene otro encabezado: `codigo, nombre, fecha, concepto,
concepto_1, concepto_2, centro, debe, haber, fecha_comp, operador`. Dos diferencias que rompían la
lectura: **no trae `asiento`**, que era obligatorio, y **`concepto` pasó a ser el nombre de la
cuenta** ("PERCEPCIONES IIBB SANTA FE") mientras el concepto real, con tipo, número y proveedor,
quedó en `concepto_1`. El asiento pasa a ser opcional (si el perfil lo deja vacío, el mayor se lee
y la columna queda en blanco) y el resto se resuelve mapeando el perfil, sin tocar código. De
paso, el archivo nuevo trae el **nombre completo del proveedor**, no cortado a 16 caracteres.

**Línea 3** — La pantalla no se desconfiguró sola: al abrir `FormConciliacionPyR` en el diseñador
de Visual Studio, el diseñador reescribió `FormConciliacionPyR.Designer.cs` y
`FormPerfilPyRDetalle.Designer.cs` enteros y creó tres `.resx`. Lo concreto: el cuadro de la
carpeta pasó de 426 a 1026 px y tapaba los botones, y la grilla de mayores de 544 a 600. Se
restauraron los dos designer commiteados (los verificados contra las maquetas) y se repuso a mano
el control del operador que el usuario había agregado desde el diseñador, en la misma posición.

### Verificación

- `iibb202607.XLSX` con el perfil ajustado: 984 filas, 0 sin comprobante, 4 anulaciones, operador
  cargado en todas (OLOPEZ, CBONINO, JVALLEJO…), y la conciliación contra ARCA da los mismos
  números que con el archivo viejo: 838 conciliados / 20 diferencias / 26 sólo ARCA / 118 sólo
  PRESEA / 8 anuladas.
- Capturas de las dos pantallas restauradas, con la columna y el campo Operador a la vista.
- Los cinco casos de error de encabezados quedaron reproducidos en la consola de prueba (ver la
  respuesta al usuario en el chat): con el mapeo viejo contra el archivo nuevo da "Faltan las
  columnas asiento", y apuntando a `concepto` en vez de `concepto_1` se lee todo como filas sin
  comprobante.

### Pendiente

- **Un perfil mapea las columnas de todos sus mayores.** Si el export de IVA sigue con el formato
  viejo y el de IIBB pasa al nuevo, un mismo perfil no puede leer los dos: hoy hay que usar un
  perfil por formato. Si los dos exports pasan al formato nuevo, el tema desaparece.
- Los archivos del formato viejo ya no se pueden leer, porque el operador es obligatorio.
- El archivo nuevo trae `codigo` y `nombre` de la cuenta: se podría preseleccionar sola la cuenta
  al agregar el mayor, en vez de la primera libre. Quedó anotado, no implementado.



<!-- Fecha: 05/10/2026 - TAREA: 00063 - Linea: 1 - Se registra el análisis del filtro por lista de CUIT sin implementar cambios. -->
<!-- Fecha: 05/10/2026 - TAREA: 00063 - Linea: 2 - Se registra el diseño de importación de proveedor y se distinguen los acuerdos de las propuestas para revisión. -->
## TAREA 00063 — 05/10/2026

### Pedido

1. Analizar conciliación contra ARCA con filtro por lista de CUIT asociado al perfil.
2. Analizar conciliación contra comprobantes pendientes de un proveedor, con formato de archivo
   guardado por CUIT. El usuario pidió actuar como analista y no modificar código.
3. Reservar el número de tarea y registrar el resultado en el historial.
4. Preparar un archivo resumido para enviar al cliente por correo.
5. Diseñar las pantallas como propuesta visual.

### Documentación elaborada

Especificación: [Conciliación ARCA por CUIT y proveedor](superpowers/specs/2026-10-05-conciliacion-arca-cuits-proveedor-design.md).

Consolida 21 decisiones del taller: modalidades excluyentes en el perfil, reemplazo de la
lista CUIT, formato único por proveedor, CUIT agregado desde el contexto, importe sin
interpretación de deuda, previsualización de solo lectura, equivalencias al catálogo existente
con confirmación inicial, bloqueo total por errores y exportación de resultados sin PRESEA
en modalidad proveedor. Mantiene directivas y fechas actuales.

Incluye circuitos, modelo de información propuesto, impacto localizado y 33 criterios de
aceptación futuros. Las propuestas P01–P04, P06 y P08–P09 se distinguen de las decisiones confirmadas;
los signos P07 y las exclusiones P05 fueron confirmados como D20/D21. Estado: **para revisión**, no aprobado
para implementación. No se creó un plan de implementación.

<!-- Fecha: 05/10/2026 - TAREA: 00063 - Linea: 2 - Se registra la confirmación puntual de signos sin extenderla al resto de las propuestas. -->
Actualización del taller: el usuario confirmó comparar la magnitud del importe y asignar el
signo del resumen por tipo (FC/ND positivo, NC negativo), conservando el valor original en
la previsualización. Se actualizó la especificación y el criterio A27; no se modificó código.

<!-- Fecha: 05/10/2026 - TAREA: 00063 - Linea: 2 - Se registra la exclusión por tipo confirmada sin permitir que se omitan errores fiscales individuales. -->
El usuario confirmó excluir pagos, recibos y movimientos ajenos al alcance mediante una
clasificación por tipo y CUIT. La previsualización los identifica y cuenta; las decisiones
confirmadas se reutilizan. Un tipo desconocido sigue bloqueando hasta clasificarlo.

<!-- Fecha: 05/10/2026 - TAREA: 00063 - Linea: 4 - El archivo para correo resume el alcance sin incluir propuestas técnicas todavía no confirmadas. -->
**Resumen para cliente:** [PDF de una página](../output/pdf/TAREA-00063-resumen-cliente.pdf),
con [texto editable](Diseno/00063-conciliaciones/resumen-cliente.md). Preparado para adjuntar;
no se envió ningún correo.

<!-- Fecha: 05/10/2026 - TAREA: 00063 - Linea: 5 - Las maquetas permiten revisar estados e interacciones sin modificar formularios de producción. -->
**Diseño:** [maqueta navegable](Diseno/00063-conciliaciones/pantallas.html) y
[guía de pantallas](Diseno/00063-conciliaciones/guia-pantallas.md). Siete pantallas de escritorio:
perfil, lista CUIT, selección de proveedor, archivo/formato, tipos/exclusiones,
previsualización y resultado. Incluye ejemplos de bloqueo y carga válida, sin acceso a SQL.

### Verificación y límites

Revisión de código y coherencia documental únicamente. Se identificaron el filtro de fechas
previo al motor, el tratamiento actual de signos, la normalización de tipos del modo offline,
las etiquetas ARCA en los exportadores y el riesgo del guardado de perfiles mediante borrado
completo al incorporar relaciones hijas. No se modificó ninguno de esos comportamientos.
No se consultó la base, no se ejecutaron pruebas funcionales de la aplicación ni se procesaron archivos reales.
Los cambios de código que ya estaban presentes en el directorio quedaron fuera de esta tarea.

Se verificó el PDF mediante render y revisión visual de su única página. Se recorrieron las
siete maquetas en navegador: modalidad, clasificación y exclusiones, bloqueo por tipo o fecha,
corrección del formato, carga válida, filtro del resultado y reemplazo de la lista. Sin errores
de ejecución ni desbordes horizontales de página en 768, 850, 1280 y 1536 píxeles.

### Correlativo

`~/.codex/TAREAS.md` no existía. El registro histórico `~/.claude/TAREAS.md` llegaba a 00062;
se registró 00063 allí y en la ruta requerida por AGENTS.md, dejando documentada la continuidad.

## TAREA 00064 — 05/10/2026 — ArcaCliente web

### Pedido

1. Trabajar en un worktree separado mientras se modifica el homologador.
2. Reemplazar los formularios por una interfaz web basada en la maqueta, usando el mismo backend.
3. Primera entrega limitada a conciliación ARCA y proveedores según TAREA 00063.

### Implementación

- `ArcaCliente.Core` compila las fuentes de servicios/modelos existentes. WinForms lo referencia.
- `Conciliador.Web` y solución independiente: perfiles, lista CUIT reemplazable, CUIT contextual,
  configuración automática por proveedor, equivalencias/exclusiones, vista previa estricta,
  directivas, cuatro estados, resumen por CUIT y exportación del resultado a Excel.
- Configuración SQL por entidad con control de versiones; autenticación con los usuarios SQLite
  actuales y permisos por acción. Archivos/resultados privados por circuito.
- Demostración local explícita con datos sintéticos; sin acceso a la base productiva.
- 23 pruebas automatizadas, integración con SQL Server LocalDB y recorrido de navegador.
- Revisión independiente corregida: fechas ISO, período original, tiques FC/NC/ND,
  guardados fallidos y equivalencias ambiguas o con distinta capitalización.
- Documentación de ejecución, límites, coexistencia y pruebas en `docs/ArcaWeb.md`.
- Sin cambios en AgrupadorConceptos ni en el directorio de trabajo original.

Rama `codex/arca-web`, base `30e6924`; pendiente de revisión e integración por el usuario.
No se desplegó en producción. PyR, conexión online y exportación fiscal PRESEA fuera de alcance.

## TAREA 00065 — 05/10/2026 — Web con el perfil PRESEA existente

1. Conectar la web a la misma configuración del escritorio y verificar la importación real.
2. Excluir los dos registros con código AFIP 0, confirmados como no fiscales por el usuario.

Se retiró el intento de detector de layout: la causa inicial era que la web estaba en Demo.
Se verificó la aplicación activa en `Y:\ConciliadorContable`, su SQL Server y el perfil
PRESEA en `arca.ArcaPerfilesOffline`. El lanzador `Tools/Start-ArcaWeb.ps1` reutiliza esa
configuración y los usuarios SQLite actuales, sin copiar credenciales ni modificar el escritorio.
Instancia local HTTPS en localhost:5085; se inicializaron las tablas web y rowversion.

Además se reprodujo una diferencia de lectura: el perfil dice UTF-8, pero el archivo PRESEA
contiene Latin-1. Adaptador del sistema con aviso visible y sin cambiar el perfil; conserva
los campos fiscales/importes de WinForms. Se mantiene validación estricta para proveedores.
Exclusión explícita `000` guardada sólo en `arca.WebPerfiles` del perfil PRESEA.
Se comprobó que el registro base del perfil quedó idéntico tras guardar la exclusión.

24 pruebas correctas. Archivos de septiembre verificados íntegramente: PRESEA 3.364 filas,
3.362 válidas/2 excluidas; ARCA 4.448 filas, 4.412 válidas/36 excluidas. Con sus directivas:
3.216 conciliados, 49 diferencias, 1.147 sólo ARCA y 43 sólo sistema.
Login HTTPS verificado en navegador; ingreso con credenciales reales pendiente del usuario.
Sin cambios en homologador ni en el checkout original. Sin archivos comerciales versionados.

Corrección de arranque dentro de 00065: la instancia `dotnet run` en Production no servía
Blazor, CSS aislado ni el módulo de reconexión (HTTP 500). El lanzador ahora publica Release
en `bin/LocalHost` y ejecuta ese contenido completo. Login y cinco recursos verificados con
HTTP 200; formulario de ingreso abierto y revisado visualmente. Conexión y perfil conservados.

## TAREA 00066 — 05–06/10/2026 — Colores y exportación compatibles con WinForms

1. Aplicar los cuatro colores de estado del escritorio a tarjetas y filas de la web.
2. Conservar exactamente la planilla de conciliación exportada por WinForms.

La pantalla consulta `EstadoConciliacionColores.Rgb`: conciliados verde (170,240,209),
diferencias amarillo (254,240,186), sólo ARCA/proveedor rosado (255,205,210), sólo sistema
lavanda (206,212,237). Texto oscuro y selección diferenciada con borde.

El adaptador web reutiliza `ConciliacionExcelExporter.Exportar`, con un temporal independiente
por descarga que se elimina al finalizar. Se recuperan la hoja original, sus 14 columnas,
nombre del proveedor, descripción fiscal, intentos de directivas, título, resumen final,
colores, bordes, anchos y encabezado congelado. El circuito proveedor cambia sólo las
etiquetas de la fuente. Nombre de descarga igual al escritorio: Conciliacion_yyyyMMdd_HHmm.xlsx.
El exportador original del escritorio no fue modificado.

26 pruebas correctas. Regresión comparada celda por celda contra el exportador original,
incluyendo tipos, estilos y estructura; variante de proveedor verificada. Colores CSS
comprobados en navegador. Captura de ejemplo en `docs/Diseno/00066-colores-exportacion/colores.jpg`.
La automatización del navegador no recibió el evento de descarga de Blob; no hubo errores
de aplicación ni consola. El contenido Excel está verificado por las pruebas de servicios.

Salida Release publicada localmente. El 06/10 hubo una interrupción temporal de acceso a Y;
tras recuperarse se inició la instancia con la configuración real del escritorio.
Login HTTPS abierto en navegador y recursos web con HTTP 200 en https://localhost:5085.
Se conservaron el lanzador, el perfil y los usuarios existentes. Checkout original sin cambios.

## TAREA 00067 — 06/10/2026 — Carpeta ARCA completa y rango desde sistema

1. Recuperar la lectura de todos los CSV de la carpeta ARCA, como en WinForms.
2. Obtener el rango inicial de la fecha mínima y máxima del archivo del sistema.

La web usa la carpeta del perfil, con respaldo configurable en el lanzador. Reúne CSV
sin subcarpetas; proveedor y sistema mantienen un único archivo. No limita el histórico
completo a diez archivos o 50.000 filas agregadas. Cada CSV conserva sus límites de lectura.
El período queda vacío hasta una importación válida del sistema, se recalcula si cambia
el archivo/formato y conserva ajustes manuales al actualizar ARCA. Cargas fallidas invalidan
entradas y resultados anteriores.

Los históricos reales contienen columnas vacías y dos filas con una columna sobrante por
la representación de moneda «&euro;». Se conserva el mapeo por posición del escritorio,
con aviso en filas discrepantes y acceso a la fila original; no se cambian sus valores.
Se mantienen las validaciones fiscales y la lectura estricta del proveedor/sistema.

29 pruebas correctas. Carpeta real: 8 CSV y 75.595 filas. PRESEA determina 04/05/2026 a
30/09/2026 y deja 53.744 ARCA fuera del filtro. Verificación en memoria sin escrituras SQL.
Navegador: control de carpeta y rango 05/09 a 26/09 derivado del ejemplo comprobados.
Captura: docs/Diseno/00067-carpeta-arca/ejemplo.jpg. Checkout original sin cambios.

Salida Release actualizada e iniciada con configuración real del escritorio. Login HTTPS
y recursos web HTTP 200; formulario abierto en navegador en https://localhost:5085/account/login.

## TAREA 00068 — 06/10/2026 — Traslado del worktree ArcaCliente a D:\Codex

1. Trasladar el trabajo web aislado a D:\Codex y conservar rama e historial.

El worktree Git está en D:\Codex\arca-web\ConciliadorContable, rama codex/arca-web,
commit 5b46918. El worktree anterior se archivó después de verificar que no tenía
cambios pendientes. El original D:\Sistemas\ConciliadorContable conserva su rama y
checkout. Se restauraron las dependencias y se reinició el host web desde D:, con login
HTTPS 200 en https://localhost:5085/account/login.

La opción projectlessWorkspaceRoot de la configuración de Codex determina espacios sin
proyecto; no cambió la ubicación del worktree Git ya creado. La app no admite registrar
el checkout trasladado como worktree administrado mediante attach_worktree, aunque Git
lo reconoce correctamente. Los próximos comandos de este trabajo deben usar la ruta D:.

## TAREA 00069 — 06/10/2026 — Continuidad del perfil de proveedor

1. Conservar el perfil editado al pasar de Perfiles a Conciliación.

El enlace «Continuar con este perfil» transmite su identificador y la pantalla de
conciliación lo presenta seleccionado. En proveedor, el CUIT contextual se solicita
allí, con regla visible de 11 dígitos, aviso junto al campo y botón Continuar
habilitado sólo cuando es válido. La validación del servidor se mantiene. El texto
de Perfiles indica correctamente que el CUIT se completa en el siguiente paso.

Causa observada: el enlace anterior abría la conciliación con el selector vacío.
El registro del intento del usuario muestra además una validación fallida de CUIT,
que antes sólo aparecía después de pulsar Continuar. Recorrido reproducido en
navegador desde nuevo perfil hasta la carga de archivos, con CUIT de ejemplo.
28 pruebas de servicios correctas y compilación web sin errores. Captura de la
validación: docs/Diseno/00069-perfil-proveedor/cuit-requerido.jpg.

Salida Release actualizada en D:\Codex. Login HTTPS 200 en https://localhost:5085/account/login.

## TAREA 00072 — 06-07/10/2026 — Percepciones y retenciones en la web

1. Permitir el acceso con permiso ArcaPyR y administrar los perfiles PyR existentes.
2. Leer la carpeta ARCA y los mayores Excel por cuenta con los importadores del escritorio.
3. Conciliar con las directivas originales y mostrar cinco estados, resumen y Excel compartido.
4. Verificar compatibilidad, documentar y preparar una entrega piloto aislada.

Se agregaron las pantallas de perfiles y conciliación PyR al host web. El perfil
conserva cuentas, formato del mayor, carpeta y directivas de la tabla SQL existente.
La web guarda por perfil y comprueba WebVersion para evitar sobrescribir una edición
concurrente. Los usuarios con ArcaPyR pueden ingresar sin requerir ArcaOffline.

La carpeta ARCA usa el importador original y informa duplicados/no reconocidos.
Cada mayor XLS/XLSX se asocia a una cuenta y conserva Operador, Asiento opcional,
conceptos sin comprobante y anulaciones. Las fuentes se previsualizan sin editar;
los cambios de perfil, carpeta, mayor o directivas invalidan resultados anteriores.
El motor y exportador de la TAREA 00041 producen el resultado y sus dos hojas Excel.
No se incorporó la exportación fiscal a PRESEA, reservada para un motor compartido.

Verificación del 07/10: compilación Release web y escritorio sin advertencias ni
errores; 35 pruebas correctas con SQL LocalDB desechable. En navegador, el ejemplo
recorrió selección, carga, cinco estados (1/1/1/2/2), exclusiones, resumen por
cuenta y guardado/continuación de un perfil actualizado. La paridad de las celdas
y estilos Excel se verificó por prueba automatizada. Captura:
docs/Diseno/00072-pyr-web/resultados.jpg. Falta validar archivos PyR comerciales
en la instalación del cliente antes de habilitarla a otros usuarios.

## TAREA 00073 — 07/10/2026 — Consulta de movimientos del homologador WinForms

1. Completar la ventana ConceptosListado iniciada por el usuario y abrirla desde el
   quinto botón de Gestión de Homologaciones, después de Conceptos Estándar.
2. Consultar por fecha del movimiento (desde/hasta inclusivos) y uno o varios conceptos
   finales, de todos los perfiles, únicamente al presionar Consultar.
3. Exportar a Excel las filas consultadas, con fechas e importes tipados y formato uniforme.

La selección con casillas sigue el manejo de la conciliación interna. Se muestran las
nueve columnas funcionales: Fecha, Concepto original, Descripción original, Débitos,
Créditos, Concepto estándar, Concepto final, Cuenta final y Perfil. Id e IdArchivo no
se exponen. Se conservan importes nulos y las cuentas como texto (incluidos ceros iniciales).
La grilla usa dd/MM/yyyy y cultura es-AR con miles y dos decimales; Excel conserva
fechas reales dd/mm/yyyy y números con formato #,##0.00 mediante TablaExcelExporter.

El catálogo carga al abrir, sin traer movimientos. La consulta y la exportación corren
fuera del hilo de interfaz. Cambiar cualquier filtro limpia los resultados y deshabilita
la exportación hasta volver a consultar. La consulta SQL parametrizada usa la fecha
histórica de texto: prioriza día/mes, admite ISO y horas; valores vacíos o inválidos
quedan fuera del rango (aviso visible). No se alteró el almacenamiento ni la conciliación
interna, que tenían cambios locales previos. Se agregaron storage, modelo y servicio
específicos para preservar nulos y evitar que IDs aparezcan por autogeneración de columnas.

Verificación: build Debug del shell WinForms correcto, cero errores y siete advertencias
preexistentes en inicialización del módulo, nulabilidad y DPI. Ocho pruebas ejecutables
correctas contra una base SQL LocalDB temporal: auto-inicialización, validación, extremos,
fechas mixtas, varios perfiles/conceptos, apóstrofes parametrizados, Excel, disparo manual,
invalidez de resultados y posición del quinto botón. La instancia automática MSSQLLocalDB
no inicia en este equipo; se utilizó ArcaWeb00064 y se eliminó sólo la base temporal creada
para las pruebas. No se consultaron movimientos de la base productiva.

Pruebas: `dotnet run --project Tools/Homologador.Tests/Homologador.Tests.csproj`.
Ejecutable actualizado: `ConciliadorContable/bin/Debug/net10.0-windows/ConciliadorContable.exe`.

## TAREA 00074 — 07/10/2026 — Débitos y créditos absolutos en la consulta

1. Aplicar valor absoluto a ambas columnas de la consulta de movimientos.

ConsultaMovimientosService normaliza Débitos y Créditos después de leerlos, conservando
los nulos. Grilla, totales y Excel reciben los mismos valores absolutos. La base mantiene
los signos originales. Se ajustaron las pruebas de SQL y Excel para comprobar negativos,
positivos, cero, nulos y preservación de los valores almacenados.

Verificación: compilación Debug del shell correcta, sin advertencias ni errores en la
compilación incremental. Ocho pruebas correctas contra SQL LocalDB temporal. Dos intentos
previos agotaron la espera de 20 segundos de la segunda consulta en la prueba de ventana;
el recorrido completo pasó al repetir con diagnóstico adicional, sin modificar la lógica
ni aumentar el límite. Queda registrado ese comportamiento intermitente del ensayo UI.
Ejecutable Debug actualizado.

## TAREA 00075 — 07/10/2026 — Diferencia entre débitos y créditos en el pie

1. Agregar la diferencia Débitos − Créditos junto a los totales de la consulta.

El pie de ConceptosListado incluye «Diferencia (D − C)» calculada con los mismos
importes absolutos que alimentan los totales visibles. La diferencia conserva su signo
y usa cultura es-AR con separadores de miles y dos decimales.

Verificación: compilación Debug del shell correcta, cero errores y la advertencia
preexistente CA2255 del inicializador del módulo. Ejecutable Debug actualizado.

## TAREA 00076 — 07/10/2026 — Borrado protegido y mejoras de conciliación externa

1. Borrar filas de movimientos desde ProcesadorForm, impidiendo borrar movimientos
   tomados para conciliaciones internas o externas en curso o finalizadas.
2. Conciliar un registro externo contra varios movimientos de extracto: el usuario
   confirmó que la suma de los importes debe coincidir exactamente.
3. Homogeneizar fechas e importes de la conciliación externa y su exportación.
4. Ampliar la selección de extractos y sugerirlos por rango editable, inicialmente
   obtenido de las fechas del archivo externo.

Estado: diseño y plan preparados con Astra, razonamiento medio, y aprobados por
el usuario el 07/10/2026. Implementación en curso con Sol 6, razonamiento medio.
Los cambios locales anteriores se preservan; la verificación usa datos de prueba.

Documentos: docs/superpowers/specs/2026-10-07-conciliacion-segura-design.md y
docs/superpowers/plans/2026-10-07-conciliacion-segura.md.

La propuesta protege también pendientes dentro del alcance guardado de sesiones,
rechaza completo un lote con movimientos bloqueados, exige suma decimal exacta,
desconcilia grupos completos y evita duplicar importes externos en Excel. El selector
amplio usa fechas originales de movimientos y conserva las selecciones al filtrar.
No se ejecutaron pruebas ni compilaciones en esta etapa documental.

## TAREA 00082 — 09/10/2026 — Conciliación externa editable desde el Designer

1. Trasladar a `ConciliacionExternForm.Designer.cs` los controles y la configuración
   visual incorporados por código: fechas, Mostrar todos, estado y aviso del rango,
   panel y divisor de extractos/conceptos, grilla y columnas del selector, resumen
   inferior, selección múltiple, tamaños, posiciones y texto del botón de conciliación.

Los controles se declaran como campos y se inicializan en `InitializeComponent`, con
los ciclos BeginInit/EndInit de Telerik y SplitContainer. Los eventos se conectan a
métodos del `.cs`, que conserva la carga de datos, la selección persistente, el resumen
y el formato dependiente de los registros. Se elimina la lista de archivos oculta
reemplazada por la grilla y los helpers visuales sin uso. Se preservan los demás cambios
locales que ya estaban pendientes.

Verificación: build Debug del shell correcto, cero errores y cuatro advertencias
preexistentes (CA2255, dos CS8632 y WFO0003). Las 13 pruebas existentes seleccionadas con
`CONCILIACION_SEGURA_ONLY=1` pasan, incluida la construcción del formulario, carga de
extractos, selección de casillas, conservación fuera del rango y recuperación al mostrar
todos. Datos en una base SQL LocalDB temporal; captura del formulario revisada visualmente.
`git diff --check` correcto para los dos archivos del formulario. No se abrió el diseñador
de Visual Studio en esta sesión.

## TAREA 00083 — 09/10/2026 — Fechas tipadas y migración con Fecha2

1. Adaptar importación, modelos y consumidores para que la fecha operativa sea un
   valor tipado y se persista como SQL `date`, validándola al entrar.
2. Acomodar la base existente usando la columna `fecha2` que creó el usuario como
   intermedia y conservando el texto original.

Decisiones expresas: rechazar el archivo completo si alguna fecha importable falta
u ofrece un valor inválido, informando filas; utilizar `fecha2` para evitar perder
el dato anterior. El orden día/mes se propone por coherencia con los datos actuales;
no hubo confirmación expresa de ese criterio para todas las fuentes futuras.

Diagnóstico sólo lectura sobre la base configurada, sin ejecutar el inicializador:
64.875 movimientos, ninguno vacío ni no convertible por las conversiones SQL
consultadas. 20.892 fechas con barras admiten también una interpretación mes/día
con otro resultado. `fecha2` es `smalldatetime NULL`: 45.992 valores coinciden con
la conversión día/mes, ninguno difiere ni tiene hora distinta de cero; 18.883 están
vacíos. No hay ítems ni sesiones externas; existen tres sesiones internas.

Propuesta: `Fecha date NOT NULL` en movimientos e ítems externos, fechas .NET
tipadas, parser de entrada determinista, consultas sin reparsing y migración
transaccional con diagnóstico previo. Completar sólo faltantes de `fecha2`, verificar
las conversiones y conservar la columna textual como `FechaTextoOriginal` antes
del cambio de nombres. Ensayar antes en una base aislada y coordinar el corte.

Diseño: `docs/superpowers/specs/2026-10-09-fechas-tipadas-design.md`.
Estado: diseño preparado para revisión. Sin cambios de código ni escrituras en
la base operativa para esta tarea; implementación, pruebas y migración pendientes.

Aprobación del 09/10/2026: el usuario confirmó el diseño con «esta ok», incluyendo
la interpretación día/mes/año. Se actualizó el spec y se preparó el plan
`docs/superpowers/plans/2026-10-09-fechas-tipadas.md`: parser compartido, migrador
independiente, contrato tipado e importación atómica, presentación/SQLite y ensayo
con corte coordinado. Plan revisado contra el spec; recomendada ejecución nativa
con revisión independiente final. Pendientes revisión del plan y elección del
método. Esta etapa sólo modifica documentación; no se ejecutó la migración.

### Ejecución y cierre — 13/10/2026

Se agrega el ítem 3 a pedido del usuario durante la verificación, antes de cerrar:

3. Corregir las ventanas que no muestran los acentos «por un tema de codificación»
   y retirar el texto «Los registros sin fecha válida quedan fuera del rango» de la
   ventana de consulta de conceptos, que dejó de corresponder al contrato nuevo.

**Ítem 1 — fechas tipadas.** Parser común (`FechaBancariaParser`) independiente de
cultura y zona horaria. La importación acumula las incidencias (archivo, hoja, fila,
valor, motivo) y rechaza el archivo completo antes de crear cabecera o movimientos;
cabecera y lote comparten una transacción, probada con un fallo SQL provocado. Modelos,
consultas, comparadores, selector, diálogos, grillas y Excel usan fecha real
(`dd/MM/yyyy` en pantalla; fechas con formato en Excel). `MigradorDataBanks` valida la
fecha antes de copiar desde SQLite. El inicializador del módulo detecta el esquema
anterior y se detiene con un mensaje que pide ejecutar la migración; no convierte solo.

**Ítem 2 — base existente.** Herramienta `Tools/MigradorFechasBancos` con
`diagnosticar` y `aplicar --respaldo`. Transaccional e idempotente: completa sólo los
`fecha2` vacíos, se detiene ante cualquier diferencia o fecha inválida, conserva el texto
como `FechaTextoOriginal` y deja `Fecha date NOT NULL`. Se aplica también a
`ConciliacionItemsExternos`, que hoy tiene datos (1.992 ítems y 4 sesiones; el
diagnóstico del 09/10 decía que no había).

**Ítem 3 — acentos.** Causa: 71 signos de reemplazo (U+FFFD) guardados literalmente en
7 archivos, por ediciones hechas leyendo como UTF-8 archivos que estaban en ANSI. Tres
(`PreseaExportador.cs`, `MainForm.Designer.cs`, `HomologarForm.Designer.cs`) se
recuperaron exactos del commit inicial; los otros cuatro (`FormPerfilOfflineDetalle.Designer.cs`,
`ConfigPresea.cs`, `FormConfigPresea.cs`, `AppServices.cs`) ya nacieron dañados y se
repusieron por contexto: «Nº», «después», «Condición», etc., y la raya larga (—), que es
la que más usa el código original. Comprobado en las DLL compiladas: 0 cadenas con
U+FFFD (antes 34 en ArcaCliente y 9 en AgrupadorConceptos). Los 47 archivos ANSI
restantes compilan bien con el compilador actual y no se tocaron (una conversión de
prueba no cambió el resultado y se revirtió byte a byte). Queda como riesgo latente:
mientras sigan en ANSI, otra edición que los lea como UTF-8 volverá a dañarlos.

**Ensayo.** Primero con una copia local (`EnsayoReal`); la copia por `SqlBulkCopy` se
colgaba por falta de memoria de la LocalDB (pedía 9 MB de concesión contra un objetivo de
4 MB) y se cambió por `INSERT` parametrizados. Después en `alvesu_pruebas`, creada en el
servidor a pedido del usuario como copia de `alvesu` (misma intercalación y nivel de
compatibilidad): 74.703 filas, 0 incidencias, `Fecha date NOT NULL`, texto original
idéntico en las 72.711 filas de movimientos, 51.476 de 51.476 fechas con barras
coinciden con la conversión día/mes, segunda ejecución sin cambios. `alvesu` no se
modificó. El usuario probó la app contra esa base con su login y la dio por buena; un
arnés automático repitió consulta, selector, conciliación interna y Excel.

**Pruebas.** `FechasBancarias.Tests` por grupo (parser, migración, importación,
presentación): correctas. `Homologador.Tests`: 14 de 14 con
`CONCILIACION_SKIP_LEGACY_UI=1`. Las dos baterías aceptan ahora una instancia por
variable de entorno (`FECHAS_TEST_SQL`, `FECHAS_TEST_DIR`, `HOMOLOGADOR_TEST_SQL`). El
smoke de la ventana de consulta (`ProbarVentana`) es inestable —pasó una vez, falló otra
por recuento y otra por tiempo— y se saltó; sospecha de una carrera del arnés al tildar
conceptos antes de que termine de cargar la lista, no probada. La corrida conjunta de
`FechasBancarias.Tests` puede fallar por memoria de LocalDB (quedaban unos 370 MB de RAM
libre); por grupo pasa.

**Revisión de Codex: omitida.** Codex no tenía saldo y el usuario decidió saltearla. No
hay archivos `Docs/Revisiones/00083-review-*.md`.

**Pendiente.** La migración real sobre `alvesu` no se ejecutó. Requiere respaldo
identificado, ejecutables cerrados, un diagnóstico nuevo justo antes del corte (la base
sigue recibiendo importaciones) y desplegar la versión nueva a la vez: los ejecutables
anteriores no funcionan contra el esquema migrado. `alvesu_pruebas` queda en el servidor.

**Script SQL para el corte.** `Tools/MigradorFechasBancos/migrar-fechas-bancos.sql` hace en
T-SQL lo mismo que la herramienta, para correr en SSMS: con `@Aplicar = 0` sólo
diagnostica; con `@Aplicar = 1` migra en una transacción y revierte ante cualquier
diferencia. Probado en LocalDB con filas que cubren todos los formatos reales (también
hora tardía, bisiesto y zona ISO) y con datos malos (fecha inexistente, `fecha2` que no
coincide, texto vacío, mes/día invertido): aborta sin cambiar nada; segunda ejecución sin
efecto. En la `alvesu` real corrió sólo en modo diagnóstico: 72.711 movimientos y 1.992
ítems, 0 incidencias, mismo resultado que el migrador. La verificación final tenía un
error (un `FULL JOIN` con el filtro por tabla en el `ON`) que el propio script detectó
revirtiendo todo; ya está corregido. No se creó ninguna otra base en el servidor.

**Integración (13/10/2026).** La rama `feature/00046-pyr-operador-formato` se mergeó a
`main` con un commit de merge (`df41bc5`; antes, `f5d823c` con el trabajo y `773b82d` con
el script SQL) y se subió a `origin/main`. `main` compiló sin errores antes del push. El
avance directo era de 5 commits, pero `main` local tenía además 17 sin subir de otras
tareas (00046, 00063 a 00066, 00069 y 00072: ARCA web), que salieron en el mismo push;
antes se revisó que ninguno de los 22 commits contuviera credenciales. La rama se dejó sin
borrar. `codex/arca-web` (otro worktree, en `6226c55`, ya contenido en `main`) no se tocó.
`alvesu_pruebas` queda en el servidor a pedido del usuario; la app y la carpeta de prueba
con credenciales se borraron.

**Primer intento de corte en `alvesu` (13/10/2026): falló y se revirtió.** El usuario corrió el
script con `@Aplicar = 1` y terminó con `Msg 468` (conflicto de intercalación entre
`Modern_Spanish_CI_AS`, la de `tempdb`, y `SQL_Latin1_General_CP1_CI_AS`, la de `alvesu`) en la
verificación final, que compara `FechaTextoOriginal` con el texto guardado en la tabla temporal
`#F`. Por `XACT_ABORT` se revirtió todo: verificado en la base real, `Fecha` seguía `nvarchar`,
`fecha2` seguía presente, 72.711 movimientos y 1.992 ítems, sin transacciones abiertas ni
bloqueos. Causa: error mío; en LocalDB `tempdb` y la base compartían intercalación y la prueba
no podía mostrarlo. Corrección: `Texto` de `#F` lleva `COLLATE DATABASE_DEFAULT`. Reproducido
a propósito en LocalDB con intercalaciones distintas (mismo `Msg 468`) y probado el arreglo
con ambas combinaciones. El corte queda pendiente de repetirse con el script corregido.

**Segundo intento: `Msg 207` por reejecutar en la misma ventana.** Al repetir el script en la
misma ventana de SSMS dio `Msg 207` («FechaTextoOriginal no es válido», líneas 247 y 249) sin
imprimir nada. Causa: el primer intento falló antes del `DROP` final y dejó vivas en la sesión
`#F`, `#Tablas` y `#Dependencias`; al recompilar, SQL Server las encuentra ya creadas, no
difiere la resolución de nombres y busca `FechaTextoOriginal` antes de que exista (además
conservaban la intercalación vieja). `alvesu` seguía intacta, sin transacciones abiertas.
Corrección: un lote propio, antes del principal, con `DROP TABLE IF EXISTS #F, #Tablas,
#Dependencias`. Reproducido en una sola sesión de LocalDB (corrida 1 falla, 2 y 3 daban
`Msg 207`) y comprobado el arreglo: la corrida 2 llega otra vez al aviso de datos
inválidos y la 3, con datos corregidos, migra. Aun así conviene ejecutar el script en una
ventana de consulta nueva.

**Corte en `alvesu` aplicado (13/10/2026).** Tercer intento con el script corregido: confirmado
(`MIGRACIÓN CONFIRMADA`). Verificado en solo lectura: `Fecha` es `date NOT NULL` y
`FechaTextoOriginal` `nvarchar(max) NULL` en `MovimientosArchivo` y `ConciliacionItemsExternos`,
sin `fecha2`; 72.711 movimientos y 1.992 ítems, ninguna fecha nula, ninguno sin texto original,
rangos 02/02–30/09/2026 y 04/05–31/08/2026. Contrastado con `alvesu_pruebas` (migrada por la
herramienta C#, otro camino de código): las 72.711 fechas y textos, y los 1.992 ítems, son
idénticos; no hay filas nuevas ni faltantes entre ambas. Relaciones sin cambios (80 archivos,
4 sesiones externas con 2.076 pares, 3 internas con 347 pares, 15 perfiles), sin claves
foráneas no confiables ni transacciones abiertas. Queda desplegar la versión nueva de los
ejecutables en todos los puestos: los anteriores no funcionan contra el esquema migrado.

## TAREA 00084 — 09/10/2026 — Perfil en las grillas del extracto externo

1. Mostrar el nombre del perfil en la grilla del extracto de Conciliación Externa,
   antes de continuar con la tarea 00083 de fechas tipadas.

`MovimientoStorage.ObtenerPorArchivos` completa `MovimientoProcesado.NombreBanco`
mediante el archivo importado y su perfil, con joins izquierdos para conservar el
conjunto de movimientos. Las dos grillas del extracto (Con concepto seleccionado
 y Resto del extracto) declaran en el Designer una columna de sólo lectura titulada
`Perfil`, vinculada a ese nombre. Se conservan los cambios locales del usuario.

Verificación: compilación Debug del shell correcta, cero errores y siete advertencias
preexistentes. El primer intento de pruebas coincidió con el build y falló por bloqueo
de una DLL (CS2012); se reinició después de terminar la compilación.
La corrida siguiente detectó que el smoke UI buscaba los nombres anteriores al
último guardado del Designer. Se actualizaron únicamente esas referencias para
respetar los nombres actuales con prefijo. Resultado final: 13 pruebas existentes
correctas sobre SQL LocalDB temporal, incluida la construcción del formulario.

## TAREA 00085 — 09/10/2026 — Recuperar columnas del extracto junto al perfil

1. Corregir la regresión introducida por la tarea 00084: al declarar únicamente la
   columna Perfil en el Designer, las grillas Telerik no generaban las demás.

Se reprodujo el error antes de corregirlo: al enlazar un MovimientoProcesado, la
grilla de pendientes contenía sólo NombreBanco y faltaba Fecha. Las 13 pruebas
anteriores no cubrían columnas/valores en esas grillas enlazadas y no detectaron
la regresión.

Ambas grillas del extracto ahora definen el conjunto completo en el Designer:
Fecha, Concepto original, Descripción, Concepto estándar, Débitos, Créditos,
Concepto final, Cuenta final y Perfil. La generación automática queda deshabilitada
para ese conjunto explícito. Se conserva el vínculo de Perfil con NombreBanco.

Verificación: nueva regresión de UI, sin conexión SQL, comprobada primero fallando
por la columna Fecha ausente y luego pasando. Verifica las nueve columnas visibles,
sus valores, un único Perfil y la conservación después de volver a enlazar datos,
en ambas pestañas. Corrida final: 14 pruebas correctas con
`CONCILIACION_SEGURA_ONLY=1`; las pruebas SQL existentes usan LocalDB temporal.
Build incremental Debug del shell correcto, cero errores y cero advertencias;
el build previo de las pruebas mostró la advertencia preexistente CA2255.
Ejecutable Debug actualizado. No se modificó la base operativa.

## TAREA 00096 — 13/10/2026 — Baja de archivo sin lista de IDs

Pedido del usuario (textual): «cuando quiero borrar un archivo da error porque dice que tiene
muchos parametros y el mensaje en pantalla es que esta conciliado. ambos son errores. tenemos
que cambiar el algoritmo de busqueda de items conciliados. con un query se puede hacer. no es
necesario tener todos los ids y hay que chequear en dos archivos por ahora.
conciliacioninternapares, idmovimientoa y b, y en conciliacionpares con el campo
IdMovimientoPROCESADO». Numeración confirmada por el usuario:

1. Reemplazar la lista de IDs por una consulta SQL por archivo, sobre las dos tablas de pares.
2. Que el mensaje de pantalla deje de decir «conciliado» ante cualquier error de base de datos.
3. Aplicar lo mismo a la baja de una selección de movimientos, que tenía el mismo límite.

**Causa.** `EliminarArchivo` cargaba todas las filas del archivo y las pasaba como lista a
`WHERE ... IN @Ids`. SQL Server admite 2.100 parámetros y un archivo real tiene miles de
movimientos (error 8003, `EliminacionMovimientosStorage.ValidarLibres`). Además,
`ProcesadorForm` capturaba cualquier `SqlException` y respondía «el archivo tiene sesiones de
conciliación asociadas», de modo que un error técnico parecía un archivo conciliado.

**Ítem 1.** La validación recibe un filtro de movimientos (todo el archivo, o una selección
dentro de él) y pregunta con tres `EXISTS`: `ConciliacionPares.IdMovimientoProcesado`,
`ConciliacionInternaPares.IdMovimientoA` y `IdMovimientoB`, unidos a `MovimientosArchivo`. Ya no
se cargan los movimientos: el alcance de las sesiones internas (conceptos y rango de fechas) se
evalúa también en SQL, sobre `Fecha date`, con los conceptos de la sesión pasados como JSON.
Las reglas y los mensajes de bloqueo no cambian: los pares siguen protegiendo aunque falte la
sesión, y las sesiones externas e internas en proceso o finalizadas siguen bloqueando.

**Ítem 2.** Un `SqlException` al borrar un archivo muestra «No se pudo borrar el archivo por un
error de la base de datos» con el motivo real. La conciliación se informa por
`InvalidOperationException`, con su mensaje específico. Se corrige además el comentario de
`ArchivoImportadoStorage.Eliminar`, que atribuía el bloqueo a una FK.

**Ítem 3.** La baja de una selección envía los IDs como un único parámetro JSON
(`OPENJSON`), tanto para contar, validar y borrar, sin tope de cantidad. Mantiene el rechazo
atómico: si un solo movimiento está tomado, no se borra ninguno.

**Prueba.** Nueva `BajaGrande` en `Homologador.Tests`, escrita antes del arreglo y fallando con
el error 8003 en `ValidarLibres`: archivo libre de 2.500 movimientos (se borra), par externo
sobre el último movimiento, par interno por el lado A y por el lado B (se rechaza con motivo
de pares y sin borrar nada), archivo ya sin pares (se borra), y selección de 2.500 IDs con y
sin un movimiento tomado. Resultado: 15 de 15 con `CONCILIACION_SKIP_LEGACY_UI=1`, incluidas
las pruebas previas de protección de bajas. Los límites de este arreglo: probado sobre una
base LocalDB, no sobre `alvesu`; el comportamiento en pantalla no se probó con la interfaz.

**Revisión de Codex: omitida** por decisión del usuario (Codex sin saldo). Sin archivos en
`Docs/Revisiones/`.

**Integración (13/10/2026).** Mergeada a `main` con un commit de merge (`d013af5`) y subida a
`origin/main`. Antes de subir, `main` compiló sin errores y `Homologador.Tests` dio 15 de 15. La rama
`feature/00096-baja-archivo-consulta` se conserva. Pendiente: desplegar la versión nueva del ejecutable
en los puestos para que el borrado de archivos grandes funcione.

**Verificación en producción (14/10/2026).** El usuario intentó borrar el archivo 121
(`09-2026 .xlsx`, perfil 8, 7.840 movimientos) y creyó que estaba tomado por una conciliación.
No lo estaba: la validación nueva, ejecutada en una transacción revertida sobre `alvesu` y sobre
`alvesu_pruebas`, dio libre (sin pares, sin sesiones externas que lo incluyan, sin sesiones
internas que alcancen sus fechas, sin claves foráneas ni triggers que lo frenen). Causa: corría el
Release de `bin/Release`, compilado el 13/10 a las 15:40, 16 minutos antes del commit del arreglo,
que conservaba el mensaje viejo (un error de 2.100 parámetros disfrazado de «sesiones de
conciliación asociadas»). Se recompiló el Release (14/10 10:54), con `conciliador.db` y
`secrets.config` intactos (comparados por hash), y el usuario borró el archivo sin problemas.
Pendiente: copiar esa carpeta `bin/Release` a los demás puestos, sin pisar su `conciliador.db` ni
su `secrets.config`. La rama `feature/00096-baja-archivo-consulta` se borró el 13/10.
