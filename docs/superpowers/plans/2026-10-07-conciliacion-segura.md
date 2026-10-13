# Conciliación segura Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implementar las cuatro líneas de TAREA 00076: borrado protegido en ProcesadorForm, conciliación externa uno a varios exacta, formatos argentinos y selección amplia de extractos.

**Architecture:** Reutilizar el grupo natural sesión/externo de `ConciliacionPares`; proteger validación y escritura con una transacción y un bloqueo común de SQL Server. Concentrar SQL tocado en almacenamiento y reglas en servicios; mantener formularios como consumidores y preservar las firmas heredadas que usan otros llamadores.

**Tech Stack:** .NET 8, WinForms, Telerik UI, SQL Server, Dapper, ClosedXML; pruebas ejecutables `net8.0-windows` con LocalDB.

**Spec:** `docs/superpowers/specs/2026-10-07-conciliacion-segura-design.md`.

Fecha: 07/10/2026 - TAREA: 00076 - Linea: 1, 2, 3, 4 - Plan revisable, todavía sin autorización de implementación.

## Global Constraints

- .NET 8, WinForms, Telerik UI, SQL Server, Dapper y ClosedXML existentes; sin dependencias nuevas.
- SQL nuevo o modificado en `Data/*Storage.cs`, parametrizado; reglas de negocio en `Services/`; formularios sin SQL.
- Toda operación de varias sentencias usa una transacción explícita y una sola conexión.
- Todo acceso nuevo abre mediante `DatabaseHelper.Open()` y conserva la auto-inicialización desde cualquier host.
- Comentarios y mensajes en español; no mostrar identificadores internos en grillas, avisos ni Excel.
- Cada bloque tocado lleva `Fecha: dd/MM/yyyy - TAREA: 00076 - Linea: N - Descripción`; usar `//` en C# y `--` en SQL.
- Preservar cambios locales ajenos, históricos y pares existentes; no deduplicar ni borrar datos como parte de una migración.
- Implementación solicitada: Sol 6, esfuerzo medio. No comenzar hasta revisión del diseño/plan. No crear otro worktree que pierda las modificaciones locales necesarias.

## Review Focus

1. Un archivo secundario externo o una fila pendiente protegida debe bloquear también el borrado de archivo completo; probar en tarea 1.
2. Dos ventanas con listas obsoletas deben producir una sola operación válida y ninguna escritura parcial; probar con dos conexiones en tareas 1 y 2.
3. Fecha con hora/ambigua/inválida y Windows en inglés deben conservar el día correcto y los valores originales; probar en tareas 1, 3 y 4.
4. Un grupo uno a varios no debe multiplicar totales externos ni permitir desconciliar una parte; probar en tareas 2 y 3.
5. Filtro, agrupación o cambio de archivo/rango no deben ejecutar acciones sobre selección distinta de la informada; probar en tareas 1, 2 y 4.

## Mapa de archivos y contratos

Todos los caminos a continuación son relativos a la raíz del repositorio. Métodos nuevos son contratos propuestos; firmas públicas existentes se conservan como adaptadores.

- Crear `AgrupadorConceptos/Data/ConciliacionMutacionStorage.cs`: `void AdquirirBloqueo(IDbConnection cn, IDbTransaction tx)`, recurso/timeout del spec, lanza `InvalidOperationException` legible ante fallo.
- Crear `AgrupadorConceptos/Models/BloqueoEliminacionMovimiento.cs`: registro con `int IdMovimiento`, `string NombreSesion`, `string TipoConciliacion`, `string Estado`, `string Motivo`; IDs sólo para enlace interno.
- Crear `AgrupadorConceptos/Data/EliminacionMovimientosStorage.cs`: consultas de estado bajo transacción y bajas; orquesta con servicio la misma unidad de trabajo, sin devolver una conexión abierta a la UI.
- Crear `AgrupadorConceptos/Services/EliminacionMovimientosService.cs`: `int EliminarSeleccionados(int idArchivo, IReadOnlyCollection<int> idsMovimientos)` y `void EliminarArchivo(int idArchivo)`; ambos todo-o-nada, excepción de negocio legible si bloqueados.
- Crear `AgrupadorConceptos/Data/ConciliacionExternStorage.cs` y `AgrupadorConceptos/Data/ConciliacionInternaStorage.cs`: SQL de mutaciones tocadas, lectura autoritativa y persistencia mediante conexión/transacción recibidas; no migrar lecturas independientes sólo por estética.
- Modificar `AgrupadorConceptos/Data/ArchivoImportadoStorage.cs`, `MovimientoStorage.cs`, servicios `ConciliacionExternService.cs` y `ConciliacionInternaService.cs`: adaptar rutas al protocolo común.
- Modificar `AgrupadorConceptos/ProcesadorForm.cs` y `.Designer.cs`: selección visible del archivo mostrado y acción de baja protegida.
- Crear `AgrupadorConceptos/Services/ConciliacionExternExcelExporter.cs`: `void Exportar(IReadOnlyList<ConciliacionPar> pares, IReadOnlyList<ConciliacionItemExterno> externosPendientes, IReadOnlyList<MovimientoProcesado> extractosPendientes, string ruta)`; exportador sin consultas SQL.
- Crear `AgrupadorConceptos/Services/PresentacionConciliacionService.cs`: `string FormatearFecha(string valor)` y `string FormatearImporte(decimal valor)`; proveedor `es-AR`, preserva fecha inválida. Son contratos de presentación; cada fila de vista conserva por separado `DateTime? FechaOrden` y `string FechaOriginal` (para cada lado en conciliados), de modo que ordenar no use el texto formateado.
- Crear `AgrupadorConceptos/Models/ArchivoConciliable.cs`: `Id`, `NombreArchivo`, `NombrePerfil`, `DateTime? FechaDesde`, `DateTime? FechaHasta`, `int CantidadMovimientos`, `int FechasInvalidas`, `bool TieneMovimientoEnRango`; evitar `DisplayName` con IDs.
- Crear `AgrupadorConceptos/Services/SeleccionExtractosService.cs`: `List<ArchivoConciliable> ObtenerArchivos(DateTime? desde, DateTime? hasta, bool mostrarTodos)` y `(DateTime? Desde, DateTime? Hasta, int FechasInvalidas) SugerirRango(IEnumerable<ConciliacionItemExterno> items)`; usa almacenamientos existentes, sin SQL propio.
- Modificar `AgrupadorConceptos/Services/ComparadorConciliacion.cs`: parseo compartido; no reescribir valores persistidos.
- Modificar `AgrupadorConceptos/ConciliacionExternForm.cs` y `.Designer.cs`: selección múltiple, resumen, formatos y selector amplio.
- Modificar `Tools/Homologador.Tests/Program.cs` para ejecutar nuevas suites; crear `Tools/Homologador.Tests/ConciliacionSeguraTests.cs` y `ConciliacionExternaPresentacionTests.cs`. Mantener el runner sin incorporar un framework nuevo.
- Actualizar `docs/Historial.md` y `C:/Users/mjulian/.codex/TAREAS.md` en la entrada ya reservada 00076; no reservar otro número ni duplicar entradas.

## Verificación común y control de cambios

Comando de pruebas: `dotnet run --project Tools/Homologador.Tests/Homologador.Tests.csproj`. Éxito: salida «N pruebas correctas.», código 0 y ninguna excepción; N crece al añadir los casos. Compilación: `dotnet build AgrupadorConceptos/AgrupadorConceptos.csproj --no-restore`; éxito código 0. Si no hay assets restaurados, ejecutar primero `dotnet restore` del proyecto afectado y repetir.

Antes de cada tarea revisar `git status --short` y `git diff` de sus archivos; conservar la base sucia del usuario. Cada commit debe contener sólo los hunks de la tarea y pruebas asociadas; usar `git add -p` o un índice temporal cuando compartan archivo con cambios anteriores. No usar `git add .`, no revertir otros cambios. Los commits de abajo son hitos para la implementación, no acciones autorizadas durante el diseño.

---

### Task 1: Borrado seguro y protocolo de concurrencia (línea 1)

**Files:** Crear modelos/servicio/storage de baja y `ConciliacionMutacionStorage` del mapa; crear storages de mutaciones externa/interna; modificar `ArchivoImportadoStorage`, `MovimientoStorage`, ambos servicios de conciliación y `ProcesadorForm` con diseñador; crear `ConciliacionSeguraTests.cs`, registrar en `Program.cs`. Modificar únicamente llamadores de `ActualizarConceptos(..., cn, tx)` que necesiten tomar el bloqueo antes de sus lecturas/escrituras de movimientos.

**Interfaces:** Consume `DatabaseHelper.Open()`, modelos de sesión/movimiento y `ComparadorConciliacion.ParsearFecha`. Produce `EliminarSeleccionados`, `EliminarArchivo` y `AdquirirBloqueo` del mapa. Crear en storage `List<MovimientoProcesado> CargarSeleccion(int idArchivo, IReadOnlyCollection<int> ids, IDbConnection cn, IDbTransaction tx)`, `List<ConciliacionSesion> CargarSesionesExternas(IDbConnection cn, IDbTransaction tx)`, `List<ConciliacionInternaSesion> CargarSesionesInternas(IDbConnection cn, IDbTransaction tx)` y `List<BloqueoEliminacionMovimiento> CargarBloqueosPorPares(IReadOnlyCollection<int> ids, IDbConnection cn, IDbTransaction tx)`; el servicio evalúa alcances JSON/fechas y el storage ejecuta `int EliminarFilas(int idArchivo, IReadOnlyCollection<int> ids, IDbConnection cn, IDbTransaction tx)` o `void EliminarArchivoValidado(int idArchivo, IDbConnection cn, IDbTransaction tx)`.

- [ ] **Step 1: Agregar pruebas fallidas de la regla de oro.** Casos parametrizados por interna/externa y `EnProceso`/`Finalizada`: par bloquea, pendiente en alcance bloquea, movimiento libre se elimina, selección mixta deja `COUNT(*)` intacto; interna protege A y B, externa protege principal y secundario y archivo vacío seleccionado. IDs repetidos/no existentes/de otro archivo se rechazan sin cambios. Par huérfano/JSON inválido produce error legible y preserva filas. Usar la base desechable del runner.
- [ ] **Step 2: Ejecutar el runner y comprobar fallos de funcionalidad ausente.** Comando de verificación común; fallo esperado por contratos nuevos o ausencia de bloqueo, sin aceptar un fallo de conexión como prueba roja válida.
- [ ] **Step 3: Implementar parseo de fecha seguro compartido necesario para pertenencia.** Ampliar `ParsearFecha` con los formatos explícitos del spec, incluyendo horas/ISO, prioridad día/mes. Probar `02/09/2026 23:59:59 -> 02/09/2026`, `2026-09-02T12:30:00.123 -> 02/09/2026` y `no-fecha -> null`. `FechasIguales` usa el mismo parseo; conservar su fallback de comparación cruda sólo para matching heredado, nunca para autorizar borrado.
- [ ] **Step 4: Implementar la unidad de trabajo de eliminación.** Servicio abre conexión/tx, adquiere bloqueo, carga datos y evalúa regla del spec, ejecuta baja y comprueba cantidad afectada antes de commit. `EliminarArchivo` comprueba referencias externas al archivo además de sus movimientos. `ArchivoImportadoStorage.Eliminar` delega al nuevo servicio; la baja SQL real vive en `EliminacionMovimientosStorage` para no provocar recursión. JSON inválido y alcance incierto rechazan la operación con nombres y causas, sin mostrar IDs.
- [ ] **Step 5: Coordinar escritores existentes.** Trasladar sólo las mutaciones tocadas a storages y adquirir el bloqueo común antes de lectura autoritativa en crear/eliminar/finalizar/conciliar/desconciliar sesiones, insertar movimientos y actualizar conceptos. Mantener conexión/tx hasta completar cada operación. Releer IDs y sesión tras obtener el bloqueo; no dejar comprobaciones de existencia previas como única defensa. Mantener firmas y políticas internas actuales; finalización interna conserva asignación de cuentas atómica. Auditar `rg` de escrituras de las tablas implicadas y los llamadores con tx ajena para que no quede una ruta fuera del protocolo.
- [ ] **Step 6: Agregar pruebas de carrera y rollback.** Con dos conexiones y barreras explícitas: creación de sesión versus baja, emparejamiento versus baja, cambio de concepto versus baja, y baja doble. La segunda operación espera el bloqueo y luego revalida: jamás queda un par nuevo apuntando a una fila borrada; si la baja ganó, el matching rechaza ID inexistente. Comprobar timeout legible y rollback de error posterior a validación. No usar sleeps como única sincronización ni ejecutar sobre base real.
- [ ] **Step 7: Integrar acción en ProcesadorForm.** Obtener sólo filas de datos seleccionadas visibles usando `AplanarFilasDelView`; capturar `_archivoEnGrilla.Id`, confirmar cantidad/archivo, llamar servicio y refrescar datos/totales. Deshabilitar ambas bajas durante importación y operación; ampliar catch de baja de archivo a error de negocio. Prueba de UI: combo cambiado pero grilla anterior, agrupación, filtro y selección vacía; la selección efectiva debe coincidir con confirmación y con filas eliminadas.
- [ ] **Step 8: Ejecutar runner y compilación.** Ambos código 0; verificar explícitamente auto-inicialización desde el host de pruebas y ausencia de cambios de la política de reutilización interna al finalizar.
- [ ] **Step 9: Revisar diff y commitear sólo cambios de esta tarea.** Mensaje `feat: proteger la eliminación de movimientos conciliables`; no incluir cambios locales previos.

### Task 2: Grupo externo exacto y desconciliación completa (línea 2)

**Files:** Modificar `ConciliacionExternService.cs`, `ConciliacionExternStorage.cs`, `ConciliacionExternForm.cs` y diseñador; ampliar `ConciliacionSeguraTests.cs`.

**Interfaces:** Consume `AdquirirBloqueo` de tarea 1. Produce `void ConciliacionExternService.ConciliarSeleccion(int idSesion, int idItemExterno, IReadOnlyCollection<int> idsMovimientos, TipoMatch tipoMatch)` y `void DesconciliarGrupo(int idSesion, int idItemExterno)`. Conservar `ConciliarPar(...)` como adaptador al primero y `DesconciliarPar(int idPar)` como resolución del grupo bajo la misma tx. Storage aporta lecturas autoritativas y `void InsertarPares(int idSesion, int idItemExterno, IReadOnlyCollection<int> idsMovimientos, TipoMatch tipoMatch, IDbConnection cn, IDbTransaction tx)` más `void EliminarGrupo(int idSesion, int idItemExterno, IDbConnection cn, IDbTransaction tx)`.

- [ ] **Step 1: Agregar pruebas fallidas de agrupación.** Externo `100m` y movimientos `60m, 40m` crean dos pares y un externo conciliado; `60m, 39.99m` no crea ninguno. Externo `-100m` coincide con importes efectivos `60m, 40m`; no compensar `60m` débito con `40m` crédito como neto `20m`. Selección `100m` valida uno a uno y `99.99m` lo rechaza. IDs repetidos, vacío, externo ajeno, movimiento ajeno a archivos, movimiento ya asignado o flag/pares históricos contradictorios rechazan todo sin limpieza de datos.
- [ ] **Step 2: Ejecutar runner y comprobar fallo esperado.** El runner debe fallar por validación/grupo aún ausente.
- [ ] **Step 3: Implementar validación exacta y persistencia atómica.** Releer bajo bloqueo sesión, externo y movimientos, exigir `EnProceso`, pertenencia y disponibilidad en esa sesión, sumar `decimal` sin redondeo; insertar N pares y actualizar flag una vez. AutoConciliar sigue seleccionando uno a uno mediante el adaptador. Desconciliar por fila resuelve y elimina grupo entero dentro de una tx; bloquear matching/desmatching/cierre mutante en finalizadas sin impedir consulta/exportación.
- [ ] **Step 4: Probar integridad concurrente y finalizadas.** Dos intentos sobre el mismo externo/movimiento dejan sólo el primer grupo; fallo provocado tras primer insert hace rollback de todos los pares y flag; desconciliar una fila de `60+40` deja ambos movimientos disponibles y cero pares. Repetir con grupo heredado uno a uno. Intentar conciliar/desconciliar directamente por servicio en finalizada deja datos intactos.
- [ ] **Step 5: Integrar multiselección y resumen en UI.** Externo único y multiselección en ambas grillas de extractos; tomar sólo filas de datos seleccionadas y visibles de la pestaña activa (`dgvExtractoPendiente` o `dgvExtractoSinConcepto`), deduplicadas. Total y diferencia `es-AR N2`, botón sólo habilitado con igualdad; cambiar externo o pestaña limpia selección y resumen. Contador muestra externos distintos y movimientos; la vista conciliada identifica grupo por sesión/externo sin IDs visibles. Al ordenar, columnas externas se pintan sólo en primera fila visual de cada grupo. Pruebas de control en ambas pestañas: `60+40` muestra `100,00`, diferencia `0,00`; selección `60` muestra `40,00` de diferencia y no habilita conciliación. Filtrar la fila de `40` debe excluirla del total y del envío; cambiar pestaña no arrastra ninguna selección oculta.
- [ ] **Step 6: Ejecutar runner y compilación.** Esperar código 0 y verificar que el flujo automático uno a uno existente sigue funcionando.
- [ ] **Step 7: Revisar diff y commitear cambios propios.** Mensaje `feat: conciliar un registro externo con varios movimientos`.

### Task 3: Presentación argentina y Excel sin importes duplicados (línea 3)

**Files:** Crear `PresentacionConciliacionService.cs`, `ConciliacionExternExcelExporter.cs` y `ConciliacionExternaPresentacionTests.cs`; modificar `ConciliacionExternForm.cs`/diseñador y registrar suite en `Tools/Homologador.Tests/Program.cs`.

**Interfaces:** Consume parseo de tarea 1 y grupo natural de tarea 2. Produce métodos de presentación y exportación del mapa. El exportador recibe listas completas de pendientes actualmente exportados y conserva esas categorías; el form sólo obtiene datos y elige destino.

- [ ] **Step 1: Agregar pruebas fallidas de valores y cultura.** Con `CurrentCulture = en-US`, esperar `FormatearFecha("02/09/2026 23:59:59") == "02/09/2026"`, `FormatearImporte(1234567.89m) == "1.234.567,89"`, negativos conservados y fecha inválida visible. Ordenar `15/08/2026` y `02/09/2026` debe ubicar agosto primero usando la clave `DateTime?`, no el texto; una fecha inválida conserva su texto y clave null. Restaurar cultura en finally. Comprobar las cuatro grillas, no sólo pendientes externos.
- [ ] **Step 2: Agregar prueba fallida de Excel.** Exportar externo `100m` con dos pares `60m,40m`, otro grupo y pendientes. Reabrir con ClosedXML: dos filas del primer grupo, una sola celda externa `100`, ambas celdas extracto numéricas, fecha válida `XLDataType.DateTime` con `dd/mm/yyyy`, importe `XLDataType.Number` con `#,##0.00`, fecha inválida original y observación, ningún encabezado de ID. Sumas deben igualar externos distintos y todos los movimientos, respectivamente.
- [ ] **Step 3: Ejecutar runner y comprobar fallo esperado.** No contar limitaciones de Excel instalado como falla: verificación utiliza ClosedXML.
- [ ] **Step 4: Implementar formateo de todas las vistas.** Mantener clave `DateTime? FechaOrden` y `string FechaOriginal` por fecha de vista; `FormatearFecha` se usa sólo al presentar. Ordenar fechas por la clave tipada, con inválidas al final y desempate por texto original. Mantener importes numéricos, cultura explícita y fecha inválida señalada; no modificar modelos persistidos ni aplicar valores absolutos por motivos de presentación. Aplicar formato de selección/resumen y de conciliados al refrescar.
- [ ] **Step 5: Extraer e implementar exportador.** Agrupar por `(IdSesion, IdItemExterno)`, ordenar grupos establemente, escribir columnas externas sólo en primera fila; valores de fecha/número reales y observaciones para inválidos. Llamar desde botón existente y permitirlo al consultar una finalizada.
- [ ] **Step 6: Ejecutar runner y compilación, abrir el Excel de prueba para revisión visual si hay visor disponible.** Código 0; revisar que no haya totales externos duplicados. Registrar aparte cualquier comprobación visual que no se pudo realizar.
- [ ] **Step 7: Revisar diff y commitear cambios propios.** Mensaje `fix: unificar formatos y totales de conciliación externa`.

### Task 4: Selector amplio con sugerencia por fechas y selección persistente (línea 4)

**Files:** Crear `ArchivoConciliable.cs`, `SeleccionExtractosService.cs`; modificar `ConciliacionExternForm.cs`/diseñador y ampliar `ConciliacionExternaPresentacionTests.cs`. Reutilizar `ArchivoImportadoStorage`, `PerfilBancoStorage` y `MovimientoStorage` para lecturas.

**Interfaces:** Consume `ParsearFecha` y métodos de storage existentes. Produce `ObtenerArchivos` y `SugerirRango` del mapa. En el form mantener `HashSet<int> _idsArchivosSeleccionados`, colección de ítems externos cargados y métodos privados `void AplicarFiltroArchivos()` y `void ActualizarResumenSeleccionArchivos()`; `ArchivosSeleccionados` debe resolver el conjunto completo y no `CheckedIndices` de la lista filtrada.

- [ ] **Step 1: Agregar pruebas fallidas del rango.** Externo con `01/09/2026`, `30/09/2026 23:59:59` y fecha inválida sugiere ambos extremos y cuenta `1` inválida; sin válidas sugiere null/null. Archivo importado en octubre con movimiento en septiembre aparece; importado en septiembre con todos sus movimientos en agosto no aparece. Incluir ambos extremos, archivo con fechas intermedias fuera del rango min/max y rango invertido; decidir por existencia de un movimiento en rango, no sólo solapamiento de períodos. Mostrar todos recupera archivos vacíos/con fechas inválidas.
- [ ] **Step 2: Ejecutar runner y comprobar fallo esperado.** El fallo debe señalar selector/contratos ausentes.
- [ ] **Step 3: Implementar servicio de candidatos.** Cargar archivos/perfiles y llamar `MovimientoStorage.ObtenerPorArchivos(idsArchivos)` una sola vez por refresco, conservando fecha fuente; no usar `ObtenerTodos`, cuya normalización SQL con estilo 103 pierde originales/inválidas. Agrupar por archivo, parsear con tarea 1 y producir período/cantidad/inválidas y coincidencia real de fechas. Validar Desde <= Hasta. En modo todos incluir archivos sin fecha válida y vacíos. Probar que la fecha fuente inválida se conserva y suma en `FechasInvalidas`.
- [ ] **Step 4: Ampliar panel y leer externo al seleccionarlo.** Reemplazar `clbArchivos` por grilla Telerik con casillas y columnas «Perfil», «Archivo», «Desde», «Hasta», «Cantidad» y «Fechas inválidas»; Dock y splitter redimensionable, perfil/archivo aprovechan ancho disponible. Incorporar rango editable, «Mostrar todos», cantidad visible/seleccionada y aviso de fechas inválidas. Reutilizar el mismo lote externo leído al confirmar, invalidándolo si se cambia de archivo. Sin fechas válidas activar todos. Mostrar el texto exacto del spec sobre archivos completos y no aplicar el rango al alcance persistido de la sesión.
- [ ] **Step 5: Mantener selección independiente de filtro y preservar conceptos.** Sincronizar casillas con `_idsArchivosSeleccionados`; reconstruir lista sin eventos que borren selecciones. Mostrar seleccionados ocultos y permitir revisarlos/limpiarlos; obtener conceptos desde todos los archivos seleccionados y mantener checks aún válidos. Si un archivo seleccionado ya no existe, informar antes de crear sesión y no sustituirlo silenciosamente.
- [ ] **Step 6: Probar estado de UI y ejecutar runner/compilación.** Seleccionar dos archivos, ocultar uno por rango, comprobar resumen «2 seleccionados, 1 fuera del rango», volver a todos y verificar ambos marcados; conceptos aún válidos conservados; rango invertido no altera selección/lista. Crear sesión y verificar `IdsArchivos` contiene ambos y pendientes incluyen movimientos fuera del rango visual. Revisar a 100 % y 150 % de escala que lista/botones no se superponen y el nombre de archivo es legible.
- [ ] **Step 7: Revisar diff y commitear cambios propios.** Mensaje `feat: ampliar la selección de extractos por período`.

### Task 5: Regresión integrada y cierre de la tarea

**Files:** Pruebas de tareas 1–4, estos dos documentos, `docs/Historial.md`, entrada global TAREA 00076. Corregir sólo defectos descubiertos en los archivos ya delimitados.

**Interfaces:** Consume todos los contratos previos; no crea nuevas APIs.

- [ ] **Step 1: Ejecutar una vez la regresión completa tras integrar.** Runner y build del módulo, código 0; añadir build de `ConciliadorContable/ConciliadorContable.csproj --no-restore` para comprobar integración del shell. Si hay error, corregir y repetir el control afectado, sin declarar éxito parcial.
- [ ] **Step 2: Hacer smoke del flujo completo en base de prueba.** Importar/exhibir extracto, eliminar una fila libre, crear sesión externa, intentar borrar una pendiente protegida, conciliar `60+40=100`, exportar, desconciliar por una fila y comprobar grupo libre, repetir/ finalizar, verificar baja bloqueada y exportación disponible. Interna: par en cada lado y pendiente del rango protegidos en ambos estados.
- [ ] **Step 3: Revisar diff de seguridad.** Confirmar parametrización, tx común, todas las rutas participantes usando bloqueo antes de lectura autoritativa, ausencia de SQL nuevo en forms/services, IDs ocultos y ausencia de migraciones que borren datos. Revalidar que no se incluyeron archivos ajenos y que cada bloque/documentación usa líneas 1–4 correctas.
- [ ] **Step 4: Actualizar trazabilidad con evidencia real.** Completar entrada reservada 00076 global y del repo, marcar checkboxes ejecutados, anotar pruebas y limitaciones. No afirmar que una inspección visual, carrera o prueba pasó si no se ejecutó.
- [ ] **Step 5: Commit final de documentación y correcciones propias verificadas.** Mensaje `docs: registrar validación de conciliación segura`; informar resultado y limitaciones al usuario con rutas de archivos y sin publicación remota implícita.

## Autorrevisión del plan realizada

- Cobertura: líneas 1–4 corresponden a tareas 1–4; pruebas integradas, registro y smoke en tarea 5.
- Consistencia: grupo siempre `(IdSesion, IdItemExterno)`; `ConciliarPar` y `DesconciliarPar` quedan adaptadores; firmas del mapa coinciden con consumidores.
- Seguridad: el bloqueo se toma antes de relectura; cubre alta/cierre y conceptos además de pares; no depende sólo de FK ni borra históricos inconsistentes.
- Review Focus: las cinco clases de riesgo tienen pruebas asignadas explícitamente; cultura, horas y alcance inválido se cubren además de ejemplos felices.
- Proporción: decisiones, contratos y aserciones concretas; no se entrega implementación encubierta ni se presupone aprobación de los supuestos.

## Entrega para revisión

Revisar especialmente los cuatro supuestos de negocio del spec. La implementación usará Sol 6, esfuerzo medio, como pidió el usuario; no volver a pedir elección de modelo o método. La revisión de este plan y diseño es el siguiente paso, no su ejecución automática.
