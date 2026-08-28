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
El checklist funcional contra base real está en la Tarea 9 del plan.
