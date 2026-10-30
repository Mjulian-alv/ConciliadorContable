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

## TAREA 00097 — 14/10/2026 — Menú principal y Agrupador de Conceptos en la web

Pedido del usuario (textual): «En la web (Conciliador.Web) solo se hizo ArcaCliente. Completar el
trabajo: crear el menú principal de ConciliadorContable en la web y pasar el Agrupador de
Conceptos con todas sus ventanas y funcionalidades. Ya hay VPN para verificar el SQL.» Al aprobar
la guía agregó: «y ya que esta agrega que todos los importes deben tener separador de miles y
decimales y estar en absoluto en su columna correspondiente». Numeración definida con el usuario:

1. Menú principal de ConciliadorContable en la web (y biblioteca común `AgrupadorConceptos.Core`).
2. Procesador de movimientos: perfiles de banco, importar extracto, grilla, homologación
   individual y masiva, consolidado y detalle a Excel, bajas.
3. Gestión de homologaciones (con baja con impacto) y consulta de movimientos por conceptos.
4. Conciliación externa.
5. Conciliación interna.
6. Cuentas contables (con importación) y conceptos estándar.
7. Importes con separador de miles y dos decimales, en valor absoluto y en la columna que
   corresponde (la diferencia C − D conserva el signo; el cero se muestra `0,00` en gris; el
   dato nulo queda en blanco).

**Diseño previo.** Guía, flujogramas (Mermaid + PNG) y 37 maquetas en
`docs/Diseño/00097-web-agrupador/`. Las maquetas las hizo Claude (Codex sin saldo) y el usuario
las aprobó («apruebo flujogramas y maquetas, seguí con el código»). Los desvíos respecto de las
maquetas, línea por línea y con su motivo, están en «Ajustes al implementar» de la guía.

**Lo que se hizo (commits en `feature/00097-web-agrupador`).**

- `2229831` / `c0cdf02` (líneas 1 y 7): `AgrupadorConceptos.Core` compila por vínculo los modelos,
  datos y servicios de `AgrupadorConceptos` (misma receta que `ArcaCliente.Core`, TAREA 00064);
  el WinForms los excluye y referencia el Core. Menú principal por módulo y permisos
  (`MenuPrincipal`, estados Disponible / Sin permiso / Próximamente / Requiere base), catálogo
  `Permisos`, `FormatoImporte` (un único formateador `es-AR` fijo).
- `8e4781e` (línea 6): cuentas contables, importación desde Excel con vista previa, conceptos
  estándar y asignación de cuenta.
- `d28862f` (línea 3): homologaciones (alta, edición, baja con impacto por archivo) y consulta de
  movimientos con exportación.
- `53da1c0` (línea 2): Procesador (`Procesador.razor` y su código, `PerfilBancoEditor`, diálogos de
  aviso, pendientes agrupados y consolidado, `AgrupadorWorkspace`). Importa desde un flujo
  (`ImportacionService.ImportarArchivo`), con avance por pasos y rechazo del archivo completo ante
  una fecha inválida.
- `5f503c6` (línea 5): conciliación interna (`ConciliacionInterna.razor`, desempate de candidatos,
  `ConciliacionInternaExcelExporter`, que ahora usa también el escritorio).
- `f589319` (línea 4): conciliación externa (`ConciliacionExterna.razor`,
  `ArchivoExternoReader.Leer(Stream…)`, `ConciliacionExcelExporter.ExportarBytes`).
- Las mutaciones de la web piden `AccessGuard.Require(permiso)` y reutilizan el bloqueo
  `bancos.Conciliacion.Mutacion` de los almacenes, así que web y escritorio pueden convivir. Los
  formularios WinForms de las conciliaciones, `ConciliacionExcelExporter` y el resto del código
  del escritorio siguen funcionando igual salvo lo anotado abajo.

**Cambios que alcanzan al escritorio.** `ConciliacionExcelExporter` y el Excel de la conciliación
interna ahora escriben todos los importes en valor absoluto (línea 7), también los movimientos
que se guardaron con signo; `ConciliacionInternaForm.btnExportar_Click` usa el exportador común.
No se tocó `ConciliacionInternaForm.Designer.cs`: en el árbol de trabajo queda una modificación
ajena a esta tarea (el diseñador de Visual Studio lo reescribió) que no se commiteó; falta que el
usuario diga si se restaura.

**Pruebas.** `Tools/AgrupadorWeb.Tests` (consola, net10): 38 de 38 — formato de importes, menú y
permisos, login, cuentas, Excel, importación desde flujo, conciliación interna (auto-conciliar
entre extractos distintos con desempate, manual, desconciliar, finalizar con la cuenta del banco
contrario, Excel) y conciliación externa (lector del archivo externo en Excel y CSV, fechas
inválidas, auto-conciliar, grupo 1:N de suma exacta, desconciliar el grupo, finalizar, Excel).
Regresión: `ArcaWeb.Tests` 34/34, `Homologador.Tests` 15/15 (`CONCILIACION_SKIP_LEGACY_UI=1`),
`FechasBancarias.Tests` grupo importación correcto (migración no se volvió a correr en esta tarea). **No se pudo correr el grupo
«presentacion»** de `FechasBancarias.Tests`: la instancia LocalDB agota el tiempo en el bulk copy
(falta de memoria de la máquina); no toca lo cambiado. Ambas soluciones compilan sin errores.

**Verificación en el navegador** contra `alvesu_pruebas`, con un usuario y una clave descartables:
todas las pantallas de las líneas 1 a 6 con datos reales, incluidos los estados vacío y de error.
Se importó el extracto real `BNA 09-2026 A MANO MERI.xlsx` (7.840 movimientos; 330 homologados y
7.510 pendientes; débitos 253.073.251,55, créditos 255.024.569,21), se filtró, ordenó y exportó, y
se borró el archivo. En la conciliación interna se creó una sesión sobre julio, se auto-concilió
(el desempate abrió y un par se rechazó por estar tomado por otra sesión en proceso, con su aviso),
se desconcilió, se exportó y se eliminó. En la externa se probó un archivo con fechas inválidas
(2 incidencias, no se crea nada) y uno válido (rango sugerido, resaltado, suma exacta, conciliar,
auto-conciliar, desconciliar, finalizar cancelado, exportar, eliminar). Todos los datos de prueba
se borraron; la base conserva sus sesiones reales. Se encontró y corrigió un desborde horizontal de
las dos columnas de la conciliación externa; esa corrección de CSS quedó sin reverificar a ojo.
Límite: no se probó una conciliación 1:N desde la pantalla (sí en la prueba automática).

**Revisión de Codex: omitida** por decisión del usuario (Codex sin saldo). Sin archivos en
`Docs/Revisiones/`.

**Pendiente.** La rama no se mergeó ni se subió: decide el usuario. Quedan abiertas las decisiones
5 y 6 de la guía (importe único con signo y cero en la columna opuesta), que se dieron por aceptadas
al aprobar las maquetas. Falta desplegar la web y, para el escritorio, copiar `bin/Release` a los
puestos si se quiere el Excel con importes en absoluto.

## TAREA 00098 — 15/10/2026 — Nombre de cuenta en movimientos y columna NombreBanco oculta

**Pedido (cambios hechos a mano por el usuario, registrados después):**
1. En el modelo de movimientos y en el select del servicio, traer el nombre de la cuenta.
2. En `ProcesadorForm`, ocultar una columna.

**Ejecutado.**
1. `MovimientoStorage.SelectPorArchivo` hace LEFT JOIN con `bancos.CuentasContables` por
   `CuentaFinal` y trae `descripcion` como `nombrecuenta`; `MovimientoProcesado` suma `NombreCuenta`.
2. `ProcesadorForm` oculta la columna `NombreBanco` de la grilla.

Aparte, el Designer de `ConciliacionInternaForm` había perdido por accidente las suscripciones a
`FechasPresentacion.AlEnlazar` (TAREA 00083); se restauró con `git checkout` y no forma parte de esta tarea.

Sin pruebas nuevas. Revisión de Codex: no corrida (cambio chico, a pedido). Rama
`feature/00097-web-agrupador`, sin mergear.

## TAREA 00112 — 19/10/2026 — PyR: ARCA se concilia sólo dentro del rango de fechas de los mayores

**Pedido:**
1. En la conciliación PyR, el rango de fechas con el que se filtran los comprobantes del archivo de
   ARCA debe ser el rango máximo (fecha mínima a fecha máxima) de los archivos del sistema asignados
   a la cuenta.

**Ejecutado.**
1. `ConciliacionPyRService.Conciliar` calcula, por grupo (impuesto, tipo), la fecha más vieja y la más
   nueva de las filas de sus mayores, y deja afuera los registros de ARCA con fecha fuera de ese
   rango (bordes incluidos). Antes todo ARCA entraba y lo anterior o posterior al período del mayor
   salía como "Sólo ARCA". Se calcula por grupo y no por cuenta porque ARCA se reparte por
   (impuesto, tipo): con dos cuentas en un grupo el rango es la unión de ambas. Un registro de ARCA
   sin fecha y un grupo cuyo mayor no tiene ninguna fecha no se recortan. El servicio lo usan
   WinForms y la web, así que el cambio rige en las dos.
   `ResultadoConciliacionPyR` suma `FueraDeRango`, `RangoFechas` y `AvisosFueraDeRango()`; el aviso se
   muestra en la pestaña Conciliación de WinForms, en `PyRResults.razor` y en el Excel exportado.
   Sin diseño previo: una sola pantalla existente, sin cambio de flujo.

Prueba nueva en `Tools/ArcaWeb.Tests/PyRWebTests.cs` (bordes, antes, después, sin fecha, grupo sin
rango): 35 correctas. `ArcaCliente` y `Conciliador.Web` compilan. No se probó a ojo en la pantalla.
Rama `feature/00112-pyr-rango-fechas`, sin mergear.

**Revisión de Codex: omitida** por decisión del usuario. Sin archivos en `Docs/Revisiones/`.


## TAREA 00125 — 23/10/2026 — Conciliado / FinalizacionFecha y marcado manual de movimientos

**Pedido** (el usuario lo numeró al planificar; los cambios previos se habían hecho a mano, sin tarea):
1. Identificar y regularizar lo ya hecho a mano: `MovimientosArchivo.Conciliado`, `MovimientosArchivo.ConciliadoFecha`
   y `ConciliacionInternaSesiones.FinalizacionFecha`, escritos al finalizar la conciliación interna.
2. Hacer lo mismo con la conciliación con archivo externo (la cuenta no es requisito, como sí lo es en la interna).
3. Poder marcar movimientos como conciliados a mano en `ProcesadorForm`: toma lo visible si la grilla está filtrada, y
   no marca ítems que estén en otras conciliaciones (en proceso o finalizadas, internas o externas). Los marcados a
   mano quedan excluidos de las demás conciliaciones.
4. Lo mismo en el Procesador de Conciliador.Web (paridad con WinForms; pedido el 23/10/2026).

**Ejecutado.**
1. `Data/SqlSchema.cs` crea las tres columnas con el patrón `IF COL_LENGTH ... ALTER` (antes sólo existían en las bases
   tocadas a mano; una base nueva fallaba al finalizar). `ConciliacionInternaMutationStorage.Finalizar` pasa a
   `CAST(GETDATE() AS date)` y lleva los comentarios; `MovimientoProcesado` renombra `ConciliadoManual` a `Conciliado`
   (para que `mov.*` lo mapee) y `ConciliadoFecha` pasa a `DateTime?`. Commit aparte `2157e9d`.
2. `bancos.ConciliacionSesiones` suma `FinalizacionFecha`. `ConciliacionExternStorage.MarcarFinalizada` guarda la fecha
   y marca `Conciliado=1` en los movimientos con par de esa sesión, en la misma transacción. No toca `CuentaFinal`;
   los movimientos sin par quedan sin conciliar.
3. `MovimientoStorage.MarcarConciliados` (bajo el lock de mutaciones): omite y devuelve con motivo los que ya están
   conciliados o en cualquier par interno/externo, sin mirar el estado de la sesión; marca el resto.
   `ConciliacionManualService` lo expone. En `ProcesadorForm` el botón «Marcar como conciliados» actúa sobre la
   selección si la hay, o sobre todas las filas que deja pasar el filtro; informa cuántas marcó y cuántas omitió y
   por qué. Un movimiento conciliado no se edita en la grilla. Exclusión: `ObtenerTodos` (candidatos de la interna)
   filtra `Conciliado=0`, la externa descarta los conciliados al listar y `ConciliarSeleccion`/`ConciliarPar` los
   rechazan dentro de la transacción.
   Sin diseño previo: pantallas existentes, sin flujo nuevo.

4. `Procesador.razor(.cs)`: botón «Marcar conciliados (n)» que actúa sobre las casillas marcadas visibles o, sin ninguna, sobre
   todo lo que deja pasar el filtro (`vista`), con confirmación y `ConciliacionManualService.MarcarConciliados` (mismo criterio que
   WinForms: omite lo que está en cualquier par y lo informa). Columna «Conciliado» con la fecha, y «Concepto final»/«Cuenta final»
   deshabilitados y con guarda en el handler si el movimiento está conciliado. Los ítems 1-2 y la exclusión ya regían en la web
   porque comparte `AgrupadorConceptos.Core`. Sin diseño previo: una pantalla existente, sin flujo nuevo.

**Verificación:** el shell `ConciliadorContable` compila en Release sin errores (incluye `ProcesadorForm`); `Conciliador.Web` compila en Release (ítem 4). No se probó contra una base ni a ojo en la pantalla.
**Límite conocido:** las sesiones externas finalizadas antes de esta tarea no dejaron `Conciliado=1` en sus
movimientos; el marcado manual igual las respeta porque mira los pares, pero siguen apareciendo como candidatos
en otras conciliaciones hasta que se las marque.

**Revisión de Codex:** pendiente, no se hizo. Sin archivos en `Docs/Revisiones/`.


## TAREA 00126 — 23/10/2026 — Hoja «Asiento» en el Excel del consolidado

**Pedido** (el usuario lo escribió sin numerar; se tomó como un solo ítem, confirmado):
1. Parados en un archivo, generar un resumen tipo asiento: una hoja más en «Exportar consolidado» con los saldos finales de
   cada `CuentaFinal`; si un concepto final no tiene cuenta, se exporta sin cuenta y con el concepto como descripción.

**Ejecutado.**
1. `ConsolidadoExporter.CalcularAsiento` arma las líneas sobre los mismos movimientos que el consolidado (sin los pendientes):
   una por `CuentaFinal` (descripción = nombre de la cuenta) y, aparte, una por concepto final para lo que no tiene cuenta
   (cuenta vacía, descripción = concepto). Saldo = créditos − |débitos|: positivo al Haber, negativo al Debe, en positivo; saldo cero se omite.
   El Excel ahora lleva dos hojas, «Consolidado» (igual que antes) y «Asiento» (Cuenta, Descripción, Debe, Haber, más filas Total y
   Diferencia Haber − Debe). **Sin línea de contrapartida del banco**: el asiento no cierra a cero, la diferencia es el neto del extracto
   (decisión del usuario para probar así primero). `TablaExcelExporter.AgregarHoja` permite varias hojas sin cambiar el formato de las demás
   exportaciones. WinForms (`ProcesadorForm`, `ConsolidadoPreviewForm`) y web (`Procesador.razor.cs`) pasan el asiento al exportar; la vista
   previa en pantalla no cambia. `ExportarAExcel` y `ExportarBytes` cambian de firma (reciben el asiento).
   Sin diseño previo: una hoja más de un Excel existente, sin pantalla ni flujo nuevo.

Pruebas nuevas en `Tools/AgrupadorWeb.Tests/AsientoTests.cs` (cuentas, sin cuenta, Debe/Haber, neto igual al del consolidado, dos hojas con
totales, vacío): 42 correctas en total. Release de `ConciliadorContable` y `Conciliador.Web` compilan. No se abrió el Excel a ojo.

**Revisión de Codex:** pendiente, no se hizo. Sin archivos en `Docs/Revisiones/`.


## TAREA 00127 — 23/10/2026 — Orden de la hoja «Asiento»: primero el Debe, después el Haber

**Pedido:**
1. Ordenar las cuentas del asiento por su columna y luego por su cuenta o descripción: primero las del Debe y después las del Haber.

**Ejecutado.**
1. `ConsolidadoExporter.CalcularAsiento` parte las líneas en dos grupos, Debe y Haber (orden estable). Dentro de cada grupo se mantiene el
   orden de la 00126: las que tienen cuenta por cuenta y después las que no la tienen, por descripción (concepto). Se interpretó «cuenta o
   descripción» como ese orden; si se prefiere mezclar con y sin cuenta en una sola secuencia, es un cambio de una línea.
   Pruebas de `AsientoTests` ajustadas al nuevo orden. Sin diseño previo: cambia sólo el orden de filas de un Excel.

**Revisión de Codex:** pendiente, no se hizo. Sin archivos en `Docs/Revisiones/`.

## TAREA 00125 — 24/10/2026 — Conciliador web bajo una subruta del Concentrador (Línea 4)

**Pedido (ítem 4 del pedido «Concentrador», repo `D:\Sistemas\Concentrador`):**
4. Adaptar cada app para funcionar bajo subruta y aceptar la sesión del gateway. En este repo se hace sólo la parte de **subruta**, y sólo en
   `Conciliador.Web`; aceptar la sesión del gateway es Fase 2.

**Ejecutado.**
1. `PathBase` opcional en `appsettings` (vacío por defecto: la app anda igual que antes). Con valor, `Program.cs` aplica `UsePathBase` y
   pone `UseRouting` explícito a continuación (con `WebApplication` el implícito iría antes y emparejaría la ruta con el prefijo puesto).
   `App.razor` toma el `<base href>` de esa misma clave.
2. Rutas absolutas pasadas a relativas al `<base>`: menú lateral y tarjetas de Inicio (la ruta del catálogo `MenuPrincipal` sigue siendo
   absoluta porque es la identidad de la sección; se usa `TrimStart('/')` al armar el enlace), enlaces de `Cuentas`, `Homologaciones`,
   `ImportarCuentas`, `PerfilBancoEditor`, `Procesador`, `Profiles`, `PyRProfiles`, `PyRReconcile`, `Reconcile`, `Error`, `NotFound`,
   `Routes`, `MainLayout` y las dos `NavigateTo`. `Profiles` usa `./?perfil=` porque `?perfil=` solo se quedaría en la página actual.
3. Razor Pages de ingreso y salida: `~/app.css` y `LocalRedirect("~/...")`, que sí llevan el PathBase.
   Los `@page` no se tocan: el router los resuelve contra el `<base>`.

**Verificado** a mano, en modo demostración y detrás del gateway de prueba (`/conciliador`): login, Inicio, menú lateral, «Percepciones y
retenciones» y sus perfiles, circuito de Blazor por WebSocket y «Cerrar sesión» sin salir del prefijo; ningún enlace apunta fuera de
`/conciliador`. Sin `PathBase`, los 12 enlaces de Inicio resuelven a las mismas rutas de siempre. **No se probó** con la base de datos real,
con el IIS de producción ni con el Agrupador (requiere la base configurada).

**Pendiente para desplegar detrás del Concentrador:** la app corre hoy con `UseHttpsRedirection` y `UseHsts`; detrás del gateway por `http`
en localhost hay que resolver cómo conservar el esquema (`X-Forwarded-Proto`) o apagar esa redirección. No se tocó en esta tarea.

**Revisión de Codex:** pendiente, no se hizo (la cuenta de Codex no admite el modelo configurado). Sin archivos en `Docs/Revisiones/`.


## TAREA 00133 — 26/10/2026 — Versiones de paquetes de AgrupadorConceptos alineadas con el Core

**Pedido:**
1. «No puedo compilar el proyecto»: Visual Studio informaba NU1605 (degradación de paquete) en `AgrupadorConceptos` y `ConciliadorContable`.

**Ejecutado.**
1. La causa: `AgrupadorConceptos.Core.csproj` (cambio sin commitear de otra línea de trabajo, junto con `NuGet.Config`) subió Dapper a 2.1.89,
   ExcelDataReader y ExcelDataReader.DataSet a 3.9.0, ClosedXML a 0.105.1 y `System.Text.Encoding.CodePages` a 10.0.14, mientras
   `AgrupadorConceptos.csproj` seguía pidiendo las versiones anteriores; NuGet trata esa degradación como error. Se subieron en
   `AgrupadorConceptos.csproj` a las mismas versiones. Sólo se commitea ese archivo: el Core y `NuGet.Config` no son de esta tarea.
   Restore con el `NuGet.Config` del repo sin errores y Release de `ConciliadorContable` compila (0 errores).
   Sin diseño previo: corrección de bug.

**Anotación:** las ramas con commits «TAREA 00125 – Línea 4/11» (PathBase de Conciliador.Web) reutilizan el número 00125, que ya era la
tarea de las columnas Conciliado; el correlativo global está en 00133 y esas líneas tendrían que renumerarse.

**Revisión de Codex:** no se hizo (corrección de dependencias).

## TAREA 00134 — 26/10/2026 — Paquetes del Core y mapeo de NuGet

**Pedido:**
1. «Hacé commit y merge de esa rama»: el cambio sin commitear de `AgrupadorConceptos.Core.csproj` y `NuGet.Config` que la 00133 había dejado afuera.

**Ejecutado.**
1. Se commitea el cambio: el Core sube Dapper a 2.1.89, ExcelDataReader y .DataSet a 3.9.0, ClosedXML a 0.105.1 y CodePages a 10.0.14, y suma
   `System.Security.Cryptography.Pkcs` 10.0.14; `NuGet.Config` mapea esos paquetes (más `System.Formats.Asn1`) a nuget.org. Con la 00133 las
   versiones de `AgrupadorConceptos` y del Core quedan iguales. La rama `feature/00125-pathbase` ya estaba mergeada a `main`; este commit se mergea con `--no-ff`.
   Build del Core sin errores. Sin diseño previo: dependencias.

**Revisión de Codex:** no se hizo (cambio de dependencias).

## TAREA 00135 — 26/10/2026 — Concepto final protegido, historial de movimientos y blindaje de archivo

**Pedido:**
1. Chequear dónde se modifica el concepto final: el usuario lo edita y en algunos casos vuelve a quedar igual al estándar. Sólo se puede modificar a mano, o por un proceso automático únicamente si es igual al estándar. (Ahí se agregó: el borrado de un archivo con modificaciones avisa antes y el archivo queda inactivo, con sus movimientos archivados en un JSON dentro de la tabla de archivos.)
2. Tabla de historial de cambios de `MovimientosArchivo`: concepto estándar, concepto final, conciliado y fecha de conciliado.
3. Blindaje del archivo (cierre de homologación): cerrado sólo se modifica si alguien lo desblinda con una clave maestra distinta de la de los usuarios; con el archivo blindado sólo se puede conciliar o usarlo en conciliaciones externas; y conciliar exige que el archivo esté blindado.

**Ejecutado.**
1. La causa: el `UPDATE` de `MovimientoStorage` escribía `ConceptoFinal` desde la copia en memoria, y una copia vieja (otra sesión, diálogo de baja calculado antes de la edición) lo dejaba otra vez igual al estándar. Ahora `UpdateConceptos` evalúa la condición contra la base en el mismo `UPDATE` y relee la fila para dejar la copia en memoria igual; `RehomologarEnMemoria` ya no reescribe los pendientes sin match. La baja de archivo cuenta modificaciones, avisa (web y escritorio) y desactiva el archivo (`Activo = 0`) archivando los movimientos en `MovimientosJson` (gzip, con sus `Id`).
2. `bancos.MovimientosArchivoHistorial` (sin FK, una fila por campo) llenada por el trigger `TR_MovimientosArchivo_Historial`; origen y usuario viajan por `SESSION_CONTEXT` desde `DatabaseHelper.Open` (`AuditoriaContexto`). Consulta por movimiento en ambas pantallas.
3. `ArchivosImportados.Blindado` + `BlindajeLog` + `ClaveMaestra` (PBKDF2, bloqueo de 5 minutos tras 5 intentos, verificación serializada con un bloqueo de aplicación). Guardias en un solo lugar (`BlindajeStorage`) dentro de las transacciones de cada método de escritura; las reglas del perfil saltean los archivos blindados e informan cuántos movimientos no se tocaron. Permiso nuevo `AgrClaveMaestra`. No se desblinda un archivo alcanzado por una sesión de conciliación en proceso.
   Diseño previo (flujograma y 16 maquetas hechos por Codex) en `docs/Diseño/00135-blindaje-historial/`; la guía se actualizó con los desvíos de la revisión.

**Pruebas:** `Tools/AgrupadorWeb.Tests/BlindajeTests.cs` (19 pruebas contra SQL Server local) y adaptación de las existentes, que ahora blindan antes de conciliar. Web 60 correctas y 1 con error previo (Login, ya fallaba antes de esta tarea); `Homologador.Tests` 15 correctas. `FechasBancarias.Tests` no se pudo correr (no conecta a su instancia). Las pantallas no se probaron a mano.

**Revisión de Codex:** ronda 1 en `docs/Revisiones/00135-review-1.md` (4 hallazgos, corregidos). No hubo ronda 2.

**Ajuste posterior (a pedido):** la clave maestra se define y cambia desde el ABM de usuarios del escritorio (botón «Clave maestra del blindaje», permiso `AgrClaveMaestra`) y no desde el procesador de archivos; la web y el procesador sólo blindan y desblindan.

**Anotación:** el cambio suelto del Core y de `NuGet.Config` que había en el árbol de trabajo se registró aparte como TAREA 00134.

## TAREA 00136 — 27/10/2026 — CUITs excluidos de la conciliación PyR (cambios hechos a mano)

**Pedido:** «numerame con el siguiente en la lista de tareas las modificaciones que hice en el proyecto y ponele un numero de linea a cada modificacion». Numeración propuesta por Claude a partir del diff y aceptada:
1. Clase nueva `CUITSExcluidos` con la lista fija de 14 CUITs a excluir de PyR.
2. Campo `Cuit` en el registro de PRESEA y lectura de la columna `cuit` del mayor.
3. El importador de PRESEA no carga las filas cuyo CUIT está excluido.
4. El importador de ARCA salta las filas excluidas, contadas como `FilasNoReconocidas`.
5. En «Pendientes» del Excel de conciliación, la columna 4 muestra `DescripcionOriginal` en vez de `ConceptoFinal`.

**Ejecutado (código escrito a mano por el usuario; Claude sólo agregó los comentarios de versionado y registró).**
1. `ArcaCliente.Core/Web/Models/CUITSExcluidos.cs`. Va en `Core/Web` y no en `Models`/`Services` del Core porque esas carpetas son vínculos de solo lectura a `ArcaCliente` (TAREA 00064); así la ven escritorio y web.
2. `RegistrosPyR.cs` (`Cuit`) y `PreseaPyRImporter.LeerFila`.
3. `PreseaPyRImporter.Importar`: omite la fila si `excluidos.CUITS.Contains(fila.Cuit)`.
4. `ArcaPyRImporter.LeerExcelAfip`: compara contra `TextParsingUtils.SoloDigitos(cuit)`.
5. `AgrupadorConceptos/Services/ConciliacionExcelExporter.cs`.

Sin diseño previo (no cambia pantallas ni circuito) y sin revisión de Codex. Compilan `ArcaCliente` y `AgrupadorConceptos` (Debug, 0 errores); sin probar con archivos reales.

**Aclaración (27/10/2026):** PRESEA entrega el CUIT como número, sin guiones ni espacios, así que comparar el valor tal cual alcanza y no hace falta `SoloDigitos` (que sí usa ARCA). La lista sigue fija en el código.

**Anotación:** la rama `feature/00135-blindaje-historial` se mergeó a `main` (merge `24c158f`, sin push) y se borró antes de abrir esta.

## TAREA 00140 — 28/10/2026 — Identidad de AppHub en el Conciliador (piloto)

**Pedido (numerado, confirmado en el chat):**
1. AppHub manda a cada app activada un token firmado y de vida corta con la identidad del usuario.
2. Conciliador web acepta ese token, crea o vincula al usuario por correo y abre su sesión propia; el login propio sigue.
3. Primer ingreso: el usuario se crea sin permisos; `AdministradoresIniciales` por configuración.
4. Vinculación de usuarios que ya existen, por correo, desde el formulario de usuarios del escritorio.
5. Contrato de integración con AppHub y checklist para apps nuevas.
6. (Después del piloto) Ofertas y luego Portal y Admin: fuera de esta tarea.

Diseño aprobado en el chat: `Docs/Diseño/00140-identidad-apps/` del repo de AppHub (`D:\Sistemas\Concentrador`), con guía, contrato, flujograma y maquetas. Decisión: la web del Conciliador deja de ser solo lectura
sobre la base de usuarios (revierte lo decidido en la 00064) y solo puede crear usuarios de Microsoft y guardar su id; los permisos se asignan en el formulario de usuarios del escritorio.

**Ejecutado (rama `feature/00140-identidad-apphub`):**
- Web: `Security/TokenAppHub.cs` (validación del JWT HS256: firma en tiempo constante, `alg`, `iss`, `aud`, vencimiento con tolerancia y correo), `Security/IdentidadAppHub.cs` (middleware antes de la
  autenticación: abre la sesión con las mismas claims del login más `via=apphub` y `correo`, reemplaza la sesión de otro usuario y deja un aviso si el token no vale o el usuario está desactivado),
  `UserStorage.IngresarMicrosoftAsync` (busca por oid y luego por correo solo si el usuario no tiene oid; crea administrador solo para la lista inicial, si no usuario sin permisos y con contraseña «!»;
  agrega las columnas si faltan), sección `AppHub` en `appsettings.json` (clave vacía = modo autónomo; una clave de menos de 32 caracteres no arranca), avisos en el login y sin «Cerrar sesión» cuando
  la sesión vino de AppHub.
- Escritorio: columnas `Correo` y `OidMicrosoft` con índices únicos en `DatabaseHelper`, campo «Correo de Microsoft» y avisos en `FormAdminUsuarios` (contraseña opcional si hay correo, lista con la marca
  «Microsoft · sin permisos»), `Usuario` con `Correo`, `OidMicrosoft` y `SinClave`.
- Pruebas: `Tools/AgrupadorWeb.Tests/IdentidadAppHubTests.cs`, 15 pruebas (token, configuración, usuarios, middleware y un pipeline real con cookie). Total del proyecto: 77 correctas, 0 con error;
  `Tools/ArcaWeb.Tests`: 35 correctas. Además se corrigieron dos pruebas viejas (`LoginTests` y `PyRWebTests`) que esperaban que el login redirigiera a `/` y desde la 00125 redirige a `~/`.

**No probado:** el formulario de usuarios del escritorio en pantalla (compila, pero no se vio el diseño), la web contra una base de usuarios real compartida con el escritorio abierto, y el recorrido completo
con AppHub y el Conciliador publicados (la web solo arranca con SQL Server o en modo demostración, y el modo demostración no usa esta identidad).
**Sin revisión de Codex** (su cuenta falla).

## TAREA 00142 — 29/10/2026 — Excel del proveedor con tabla dinámica y letra del comprobante

**Pedido (numerado, confirmado en el chat):**
1. No se puede leer el xlsx del proveedor (`ECC 26-10-26.xlsx`) en la conciliación web: el lector debe ignorar las tablas dinámicas y leer la hoja elegida, con prueba que use ese caso.
2. Agregar la columna «Letra» del comprobante al formato del proveedor en el Conciliador web.

**Diagnóstico (línea 1):** el libro trae una tabla dinámica (hoja «TD ECC») cuyo caché declara 18 campos y registros de 16; ClosedXML 0.105 lanza `PartStructureException` en `PivotTableCacheDefinitionPartReader.ReadRecords` y se rechazaba el libro entero con el mensaje genérico. La hoja «ECC» estaba bien.

**Sin diseño previo:** una corrección de bug y un campo en una sola pantalla existente, sin cambio de flujo.

**Ejecutado (rama `tarea-00142-xlsx-pivote-letra`):**
- Línea 1: `TabularReader.WithoutPivots` reescribe el .xlsx en memoria sin las partes `xl/pivot*`, su referencia en `workbook.xml`, las relaciones y los `Override` de `[Content_Types].xml`; sólo actúa si el libro tiene pivotes.
- Línea 2: `FileFormat.Letter` (opcional, se guarda en el JSON del formato del proveedor), campo «Letra del comprobante (opcional)» en `FormatEditor` sólo para el proveedor, y `ProviderImporter` une tipo y letra en un solo valor («FC A»), de modo que cada combinación se homologa por separado (FC A → 001, FC B → 006). Una columna de letra inexistente se informa como error. Si ya había homologaciones guardadas con sólo «FC», al configurar la letra hay que homologar de nuevo.
- Pruebas (`Tools/ArcaWeb.Tests/ImportTests.cs`): libro con caché de pivote inconsistente (ClosedXML solo falla, el lector no) y letra opcional. 37 correctas. Con el archivo real y el formato Tipo CPT / Letra CPT / Prefijo / Nro CPT / Fecha CPT / Monto: 37 filas válidas.
- Revisión de Codex: `docs/Revisiones/00142-review-1.md` (1 hallazgo, corregido). La primera corrida no pudo leer el repo y no cuenta como ronda; queda 1 ronda disponible.

**No probado:** la pantalla en el navegador (el campo se agregó en el editor pero no se lo vio renderizado).

## TAREA 00150 — 30/10/2026 — Listado SAP (.htm) y comprobante con la letra en un solo campo

**Pedido (numerado, confirmado en el chat):**
1. Leer el listado SAP «Lista de partidas individuales de deudores» (.htm) en el formato del proveedor. Los PDF de «CC Antoniazzi» y «pendientes» se descartaron por no tener formato de tabla.
2. Comprobante con punto de venta, letra y número en un solo campo, con varias combinaciones.

**Sin diseño previo:** opciones nuevas en el editor de formato y un formato de archivo más, sobre una pantalla existente; la descripción en el chat hizo de acuerdo.

**Ejecutado (rama `tarea-00150-sap-htm-letra-combinada`):**
- Línea 1: `SapListReader` (nuevo) convierte la salida HTML de SAP en una tabla: lee el id de cada celda (`l{fila}{columna}`), toma los títulos de la fila «Referencia / Fecha doc. / Importe», descarta cabeceras repetidas, subtotales (`*`) y totales, pasa las fechas a `dd/MM/yyyy` y el importe «31 255,75-» a «-31255,75». `TabularReader` lo usa para `.htm`/`.html`. Al cargar un listado SAP en un proveedor, `Workspace.UploadExternal` precarga y guarda el formato (Clase, Referencia, Fecha doc., Importe, por posiciones con 4 dígitos de punto de venta y orden Punto · Letra · Número). Sugerencias de homologación nuevas: DI A/B/C = factura, DG A/B/C = nota de crédito, DZ = excluir (el usuario las confirma).
- Línea 2: `FileFormat.NumberOrder` (Punto · Número, Punto · Letra · Número, Letra · Punto · Número, Número · Letra) para los modos combinados, con o sin separador; la letra se separa antes de homologar y se une al tipo («FC A»); debe ser una sola letra A–Z o la fila queda con error. «Número · Letra» toma el punto de venta de su columna. Campo «Partes del comprobante» en el editor del proveedor.
- Pruebas (`Tools/ArcaWeb.Tests/ImportTests.cs`): 43 correctas. Con el listado real: 92 partidas, 91 comprobantes y 1 pago excluido.
- Revisión de Codex: `docs/Revisiones/00150-review-1.md` (2 hallazgos de HTML dañado, corregidos con tiempo máximo y tope de celdas). Queda 1 ronda disponible.

**No probado:** las pantallas en el navegador (editor con el campo nuevo y la carga del .htm). «DG = nota de crédito» es una suposición por el signo negativo: queda como sugerencia a confirmar.
