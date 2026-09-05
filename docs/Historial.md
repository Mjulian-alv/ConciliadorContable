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

### Verificación

Sin proyecto de tests (decisión explícita): cada tarea se verificó con `dotnet build` limpio y
un review de spec + calidad; esta entrada resume los hallazgos reales, no repite el detalle
tarea por tarea. **Pendiente del usuario, contra un servidor real**: probar el flujo completo
(importar cuentas → asignar a perfil y concepto → ver `Cuenta Final` autocompletarse → crear y
cerrar una conciliación interna) antes de mergear.
El checklist funcional contra base real está en la Tarea 9 del plan.
