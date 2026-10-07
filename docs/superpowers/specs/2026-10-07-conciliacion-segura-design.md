# Conciliación segura y selección de extractos

Fecha: 07/10/2026 - TAREA: 00076 - Linea: 1, 2, 3, 4 - Diseño para proteger movimientos y ampliar la conciliación externa.

Estado: propuesta para revisión; no constituye aprobación de implementación. Diseño y plan preparados con Astra, esfuerzo medio. Implementación solicitada con Sol 6, esfuerzo medio, después de revisar estos documentos.

## Objetivo y alcance

Una sola tarea con cuatro líneas:

1. Borrar movimientos seleccionados desde `ProcesadorForm`, sin borrar movimientos tomados por conciliaciones internas o externas en proceso o finalizadas.
2. Conciliar manualmente un registro externo con uno o varios movimientos de extracto. El usuario confirmó que la suma debe coincidir exactamente con el importe externo.
3. Presentar fechas `dd/MM/yyyy` e importes `N2` con cultura `es-AR` en conciliación externa; exportación Excel consistente.
4. Ampliar la selección de extractos con rango de fechas editable sugerido desde el archivo externo y opción «Mostrar todos».

El alcance es el módulo `AgrupadorConceptos`. No se añade borrado a `ConceptosListado`; no se cambia la política de reutilización de movimientos tras finalizar una conciliación interna ni se migra todo el SQL heredado.

## Restricciones globales

- .NET 8, WinForms, Telerik UI, SQL Server, Dapper y ClosedXML existentes; sin dependencias nuevas.
- SQL nuevo o modificado en `Data/*Storage.cs`, parametrizado; reglas de negocio en `Services/`; formularios sin SQL.
- Toda operación de varias sentencias usa una transacción explícita y una sola conexión.
- Todo acceso nuevo abre mediante `DatabaseHelper.Open()` y conserva la auto-inicialización desde cualquier host.
- Comentarios y mensajes en español; no mostrar identificadores internos en grillas, avisos ni Excel.
- Cada bloque tocado lleva `Fecha: dd/MM/yyyy - TAREA: 00076 - Linea: N - Descripción`; usar `//` en C# y `--` en SQL.
- Preservar cambios locales ajenos, históricos y pares existentes; no deduplicar ni borrar datos como parte de una migración.

## 1. Borrado protegido

### Regla de pertenencia propuesta

«Tomado por conciliación» incluye pares existentes y movimientos todavía pendientes dentro del alcance guardado de una sesión `EnProceso` o `Finalizada`:

- Externa: cualquier movimiento de los archivos de la sesión, incluidos los archivos secundarios de `ArchivosJson` y el archivo principal de compatibilidad. Se incluyen todos los conceptos porque la pantalla externa permite acceder a movimientos fuera del filtro de conceptos.
- Interna: movimientos cuyo concepto final pertenece a `ConceptosJson` y cuya fecha cae en el rango inclusivo guardado; además, cualquier movimiento referido por un par en cualquiera de sus dos lados, aunque sus datos actuales ya no cumplan ese alcance.
- Los pares protegen incluso si falta su sesión o el estado es desconocido. Una referencia histórica inconsistente nunca habilita una baja.
- Metadatos de sesión que no se puedan interpretar impiden afirmar que la baja es segura: rechazar con una causa legible, sin modificar esos históricos. Para interna, una fecha inválida de un movimiento cuyo concepto coincide con una sesión relevante se trata como pertenencia incierta y bloquea la baja.

La consulta de bloqueo devuelve motivos y nombres/estados de sesiones. La UI muestra archivo, fecha, concepto e importe de los movimientos afectados, sin IDs. Un lote que mezcla filas libres y protegidas se rechaza entero; no hay eliminación parcial ni opción para forzarla.

### Experiencia en ProcesadorForm

Agregar «Eliminar movimientos seleccionados» junto a las acciones de la grilla. Actúa sólo sobre filas de datos seleccionadas y visibles del archivo realmente mostrado (`_archivoEnGrilla`), no sobre el archivo elegido en el combo si todavía no se cargó. Reutilizar el recorrido de filas agrupadas `AplanarFilasDelView`; no interpretar cabeceras de grupo como movimientos.

Antes de confirmar, mostrar cantidad, nombre de archivo y una advertencia de eliminación definitiva. La confirmación del usuario sobre una selección concreta no reemplaza la validación transaccional posterior. Si cambió el estado, informar el bloqueo y refrescar. Tras una baja exitosa, recargar grilla y totales desde base; conservar el archivo importado aunque quede vacío.

Deshabilitar borrado de filas y de archivo durante importación y durante la operación. Selección vacía no ejecuta SQL de baja. Los IDs deben ser distintos, existir y pertenecer todos a `_archivoEnGrilla`; si no, rechazar el lote.

El borrado existente de archivo completo (`ArchivoImportadoStorage.Eliminar`) debe pasar por la misma protección. También se bloquea un archivo sin movimientos si está seleccionado en una sesión externa. No se usa su cascada como vía para borrar conciliaciones. Adaptar el aviso de `BtnBorrarSesion` para errores de negocio además de errores SQL.

### Atomicidad y concurrencia

Un helper de almacenamiento toma `sp_getapplock` exclusivo, dueño `Transaction`, recurso `bancos.Conciliacion.Mutacion`, timeout de 15 segundos. Un resultado negativo cancela y presenta «Hay otra operación de conciliación en curso. Actualizá e intentá nuevamente.».

El bloqueo precede a la lectura autoritativa: revalidar existencia, pertenencia y estado, escribir y confirmar dentro de la misma conexión/transacción. Nunca validar en una conexión y borrar en otra. Ante error o inconsistencia, rollback completo.

Participan las rutas que pueden invalidar esa decisión: baja de movimientos/archivos; creación, eliminación, cierre, conciliación y desconciliación de ambas clases de sesiones; inserción de movimientos y actualizaciones de conceptos. Los SQL de escritura que se toquen se llevan a almacenamiento, conservando las firmas públicas existentes. En métodos que reciben transacción, adquirir el bloqueo antes de acceder a movimientos; revisar sus llamadores para mantener el mismo orden de bloqueos. No sostenerlo mientras se espera una confirmación o interacción de UI. Las lecturas preliminares de la pantalla no necesitan ese bloqueo.

El bloqueo coordina clientes que ejecutan esta versión. No garantiza integridad frente a aplicaciones antiguas o SQL manual que ignoren el protocolo; el despliegue debe actualizar los clientes que escriben estas tablas. No añadir FK o índices únicos que obliguen a limpiar históricos para arrancar.

## 2. Conciliación manual uno a varios

### Regla de importe y persistencia

Se mantiene el criterio monetario actual: `Math.Abs(externo.Importe) == movimientos.Sum(ComparadorConciliacion.ImporteEfectivo)`. La suma es `decimal`, exacta, sin epsilon, redondeo previo ni tolerancia. `ImporteEfectivo` sigue tomando el valor absoluto del débito cargado o, en su defecto, del crédito. Esto no introduce compensación algebraica entre débitos y créditos. La igualdad se aplica también a selección de un solo movimiento por coherencia con la confirmación del usuario.

Validar con datos recién leídos: sesión existente `EnProceso`; externo perteneciente a ella y sin pares/flag conciliado; selección no vacía sin IDs repetidos; movimientos existentes dentro de sus archivos y sin otra asignación en esa sesión. Mantener la política existente entre distintas sesiones, sin inventar exclusividad global. Desfase de suma, alcance o disponibilidad rechaza todo el grupo. Un flag histórico contradictorio con sus pares se informa y no se corrige destruyendo datos.

`ConciliacionPares` ya permite repetir `IdItemExterno` para varios movimientos. El grupo natural es `(IdSesion, IdItemExterno)`: insertar un par por movimiento y actualizar una sola vez `Conciliado = 1`, todo en una transacción. No hace falta una tabla nueva ni nuevo identificador de grupo. `ConciliarPar` conserva su firma como envoltorio de una selección de tamaño uno y usa la misma validación; la conciliación automática sigue siendo uno a uno.

Desconciliar cualquier fila del grupo libera todos sus movimientos y vuelve a marcar una sola vez el externo como pendiente en la misma transacción. Un grupo histórico uno a uno mantiene el comportamiento de siempre. En sesiones finalizadas no se admite conciliar, desconciliar ni volver a modificar su cierre; la comprobación existe en el almacenamiento/servicio aunque un llamador saltee la UI. La eliminación explícita de sesiones existente se conserva como acción separada, con su confirmación y protocolo de concurrencia; no se añade ni se ejecuta automáticamente para permitir bajas.

### Pantalla y exportación

La selección externa es única; ambas grillas de extractos permiten multiselección, pero la operación toma sólo filas de datos seleccionadas y visibles de la pestaña activa (`dgvExtractoPendiente` o `dgvExtractoSinConcepto`). No sumar selecciones ocultas por filtro ni de la otra pestaña. Mostrar «Seleccionados», «Total seleccionado», «Importe externo» y «Diferencia». Habilitar «Conciliar selección» sólo con sesión en proceso, un externo y suma exacta; el servicio igualmente revalida. Al cambiar el externo o la pestaña de extracto, limpiar la selección de movimientos y recalcular el resumen.

La grilla conciliada conserva una fila por movimiento y muestra las columnas externas sólo en la primera fila visual de cada grupo. El agrupamiento visible se sostiene al ordenar; si la grilla se ordena por campos de movimiento, recalcular cuál es la primera fila visible del grupo. No exponer claves internas. El contador distingue registros externos conciliados y movimientos de extracto conciliados. No sumar la columna repetida `ImporteExterno` del modelo sin agrupar.

Excel agrupa las filas por sesión/externo y escribe fecha, detalle e importe externo una sola vez; escribe cada movimiento asociado en su fila. Los totales externos cuentan grupos distintos, y los de extracto cuentan movimientos. Exportar y consultar sesiones finalizadas permanece disponible.

## 3. Fechas, importes y datos inválidos

Centralizar el parseo ya usado por `ComparadorConciliacion.ParsearFecha`; admitir sus formatos existentes y variantes explícitas con hora de archivos/importación: `d/M/yyyy H:mm:ss`, `dd/MM/yyyy HH:mm:ss`, `yyyy-MM-dd HH:mm:ss` e ISO con `T` y fracciones. Priorizar día/mes ante fechas ambiguas; mantener `MM/dd/yyyy` sólo como alternativa cuando la lectura día/mes no sea válida. No depender de la cultura de Windows ni convertir texto inválido en `DateTime.MinValue`.

En pendientes externos, pendientes de extracto, vista de otros conceptos y conciliados: fechas válidas en `dd/MM/yyyy`; números en `N2`, proveedor `es-AR` (`1.234.567,89`). `FormatearFecha` sólo produce texto de presentación: conservar aparte la clave `DateTime?` para ordenar cronológicamente y el texto fuente para fechas inválidas. No ordenar por la cadena formateada. Guardar importes numéricos y preservar los textos originales en los modelos persistidos. Una fecha inválida se muestra como texto original con indicación de que no es válida; no se oculta silenciosamente.

En Excel, escribir `DateTime` para fecha interpretable y número para importes, formato `dd/mm/yyyy` y `#,##0.00` respectivamente, siguiendo el exportador existente de consulta. Excel localiza los separadores según configuración regional; no escribir importes como cadenas. Una fecha inválida se conserva como texto y se identifica en una columna de observaciones. El signo de los valores mostrados no se modifica por el formateo.

## 4. Selección amplia de extractos

El panel de nueva sesión reemplaza la lista estrecha por una grilla con casillas y columnas «Perfil», «Archivo», «Desde», «Hasta», «Cantidad» y «Fechas inválidas», sin IDs visibles. Usar Dock y un splitter redimensionable para ampliar el espacio de selección; las columnas de perfil y archivo aprovechan el ancho disponible. Mantener los controles existentes de Telerik, sin nueva dependencia.

Leer el archivo externo al elegirlo y reutilizar esos ítems para crear la sesión, evitando lecturas distintas. Sugerir Desde/Hasta como mínimo y máximo de fechas válidas del externo; fechas editables, extremos inclusivos. Al cargar otro archivo, actualizar sugerencia y avisar cuántas fechas no se pudieron interpretar. Sin fechas válidas, activar «Mostrar todos» y explicar por qué.

El rango filtra candidatos por fechas de sus movimientos, no por `ArchivosImportados.Fecha` (fecha de importación): incluir un archivo si al menos un movimiento válido cae en el rango. Leer los movimientos mediante `MovimientoStorage.ObtenerPorArchivos(idsArchivos)` en un lote por refresco, conservando el texto fuente; no usar `ObtenerTodos`, cuya conversión SQL con estilo 103 pierde información de fechas originales/inválidas. Archivos con fechas inválidas se mantienen accesibles mediante «Mostrar todos», con conteo/aviso visible para que no desaparezcan inadvertidamente. Rango invertido muestra error y no cambia la lista ni selección.

Guardar la selección en un conjunto de IDs independiente de la lista visible. Cambiar rango, activar «Mostrar todos» o refrescar no desmarca archivos; mostrar «X seleccionados, Y fuera del rango» y permitir revisar/limpiar esos seleccionados. Conservar conceptos marcados mientras sigan presentes en los archivos seleccionados; recalcular desde la selección completa, incluidos los ocultos por filtro.

El rango sólo ayuda a elegir archivos. La sesión sigue incorporando todos los movimientos de los archivos seleccionados según el comportamiento existente; no se añade un filtro temporal oculto a su alcance. Explicarlo junto al rango: «El rango filtra la lista de archivos; la sesión incluye los archivos seleccionados completos».

## Supuestos concretos para revisión

1. Proteger también pendientes dentro del alcance guardado, en ambos estados, y rechazar lotes mixtos completos es la interpretación conservadora de la regla de oro.
2. «Suma exacta» conserva los importes efectivos absolutos del conciliador actual y también se exige para uno a uno. Si se necesita netear débitos/créditos, sería otra regla de negocio.
3. Filtrar candidatos por rango no recorta los movimientos de los archivos elegidos; las selecciones fuera del rango se conservan y se informan.
4. La eliminación explícita de sesiones continúa siendo una acción separada existente; esta tarea no introduce borrado de históricos ni limpieza automática de inconsistencias.

## Validación y aceptación

Pruebas en `Tools/Homologador.Tests`, ejecutable Windows sobre base LocalDB desechable `ArcaWeb00064`, sin tocar datos del usuario. Cubrir bloqueos por pares y pendientes de ambos tipos/estados, ambos lados internos, archivo externo secundario y archivo vacío; lote mixto sin bajas parciales; IDs obsoletos/de otro archivo; datos históricos inconsistentes; carreras reales con dos conexiones; rollback de inserción parcial; grupo exacto y diferencia de un centavo; grupo uno a uno; desconciliación completa; sesiones finalizadas inmutables para matching; contadores y Excel sin duplicación; fecha con hora, ambigua e inválida; cambio de cultura de Windows; rango por movimientos y selección retenida.

Smoke de UI: filas seleccionadas con filtro/agrupación en ProcesadorForm; archivo de combo distinto al mostrado; importación en curso; multiselección externa y diferencia; lectura/exportación de finalizadas; nueva sesión con lista amplia, rango y mostrar todos. Compilar módulo y pruebas, ejecutar regresiones existentes. Un caso no ejecutable se informa como limitación, nunca como aprobado.
