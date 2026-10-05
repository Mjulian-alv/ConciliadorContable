# Conciliación contra ARCA por lista de CUIT y contra comprobantes de proveedor

**Fecha:** 05/10/2026. **TAREA:** 00063. **Estado:** especificación funcional para revisión.

Este documento consolida el taller de análisis. No autoriza implementación ni modifica código,
base de datos o archivos de proveedores. Las decisiones confirmadas se distinguen de las
propuestas del analista que requieren revisión. No se elaboró un plan de implementación.

## 1. Pedido y objetivo

1. Incorporar una lista de CUIT al perfil de conciliación contra ARCA y filtrar ambas fuentes
   antes de conciliar.
2. Incorporar la modalidad contra cuenta corriente de proveedor: seleccionar su CUIT, importar
   su archivo mediante un formato guardado por proveedor y reutilizar el motor de conciliación.
3. Reservar el número de tarea y registrar el análisis y sus entregables en el historial.
4. Preparar un archivo resumido para enviar al cliente por correo.
5. Diseñar las pantallas sin implementar cambios en la aplicación.

El resultado esperado es comparar comprobantes fiscales y sus importes, informar coincidencias,
diferencias y registros presentes en una sola fuente. Aunque el proveedor entregue su cuenta
corriente pendiente, no se concilian pagos ni se calcula deuda.

## 2. Decisiones confirmadas durante el taller

| ID | Decisión |
|---|---|
| D01 | El perfil elige entre «Contra ARCA» y «Contra cuenta corriente de proveedor». |
| D02 | Solo la modalidad ARCA ofrece el filtro opcional por lista de CUIT. |
| D03 | Activar ese filtro exige una lista válida y no vacía. Se aplica a ambas fuentes antes de conciliar. |
| D04 | Cada importación de CUIT reemplaza la lista anterior; no se acumulan listas. |
| D05 | En modalidad proveedor se solicita su CUIT antes de abrir la ventana de conciliación. |
| D06 | Se comparan únicamente facturas, notas de crédito y notas de débito. |
| D07 | El proveedor envía comprobantes pendientes de pago. Esto no cambia los estados de conciliación. |
| D08 | Se configura una única columna de importe. Se compara el valor informado contra el del sistema, sin interpretar si es total original o saldo pendiente. |
| D09 | Los resultados son Conciliado, Diferencia de importe, Solo sistema y Solo proveedor. |
| D10 | Se guarda un único formato por proveedor/CUIT, reutilizable desde los perfiles. |
| D11 | El archivo del proveedor no trae CUIT. Se asigna al modelo el CUIT solicitado al ingresar. |
| D12 | El formato se define o ajusta al cargar el archivo, incluyendo cómo obtener punto de venta y número. |
| D13 | Los cambios de formato se guardan automáticamente por CUIT. |
| D14 | Hay previsualización de solo lectura. Se puede corregir el formato, nunca los valores del archivo. |
| D15 | Los tipos del proveedor se traducen mediante equivalencias al catálogo de comprobantes existente. |
| D16 | Se sugieren equivalencias; las nuevas requieren confirmación inicial. Las confirmadas se reutilizan automáticamente. |
| D17 | Los errores de importación bloquean la carga hasta corregirse. No se importan solamente las filas válidas. |
| D18 | Las directivas y el tratamiento de fechas conservan su funcionamiento; no se agrega un criterio especial por tratarse de pendientes. |
| D19 | En modalidad proveedor se exportan resultados, pero no se envían comprobantes a PRESEA. |
| D20 | Se compara la magnitud del importe de FC, NC y ND; el resumen asigna el signo por tipo. El valor original se conserva en la previsualización. |
| D21 | Pagos, recibos y otros movimientos ajenos al alcance se clasifican como «Excluir de la conciliación». La clasificación se guarda por proveedor, se cuentan las filas excluidas y los tipos desconocidos siguen bloqueando hasta clasificarse. |

## 3. Límites funcionales

El cambio afecta la conciliación offline de comprobantes de ArcaCliente. No modifica la
conciliación de percepciones y retenciones, los módulos bancarios ni la integración online.

No comprende conciliación de pagos, recibos, saldos iniciales, imputaciones, compensaciones,
cancelaciones parciales, conversión de monedas ni cálculo de deuda. Tampoco comprende edición
de comprobantes, escritura en los archivos de origen ni nuevos algoritmos de emparejamiento.

Que un comprobante resulte «Conciliado» significa que cumplió las reglas actuales de comparación;
no certifica que esté pendiente en el sistema. «Solo sistema» mantiene su significado de
presencia en una sola fuente aunque el proveedor omita comprobantes ya pagados.

## 4. Perfil y configuración visible

El perfil conserva la configuración del archivo del sistema y sus directivas. Agrega la modalidad.

| Configuración | Contra ARCA | Contra proveedor |
|---|---|---|
| Archivo del sistema y directivas | Se mantienen | Se mantienen |
| Fuente externa | Carpeta/archivos ARCA actuales | Archivo del proveedor seleccionado |
| Filtro por lista de CUIT | Opcional | No aplica |
| CUIT individual al ingresar | No requerido por este cambio | Obligatorio |
| Formato del proveedor | No aplica | Se recupera por CUIT |
| Exportar resultados | Disponible como hoy | Disponible con etiquetas de proveedor |
| Exportar comprobantes a PRESEA | Se mantiene como hoy | No disponible |

**Propuesta P01 — compatibilidad:** los perfiles existentes se interpretan como «Contra ARCA»
con filtro de CUIT desactivado. Cambiar de modalidad conserva la configuración de la otra,
pero no la aplica mientras esté inactiva. El CUIT individual pertenece a la sesión, no fija
un proveedor permanente en el perfil.

<!-- Fecha: 05/10/2026 - TAREA: 00063 - Linea: 1 - Se separa la selección de proveedores del emparejamiento para preservar las directivas existentes. -->
## 5. Ítem 1: lista de CUIT para ARCA

### 5.1 Circuito

1. Elegir modalidad ARCA y activar «Filtrar por lista de CUIT».
2. Seleccionar el archivo de CUIT e identificar su columna.
3. Previsualizar y validar la lista candidata.
4. Reemplazar la lista del perfil únicamente si la candidata es válida y no está vacía.
5. Al conciliar, seleccionar en ARCA y en el sistema solamente los CUIT incluidos.
6. Ejecutar las directivas existentes sobre esos comprobantes.

Un proveedor fuera de la lista no produce «Solo ARCA» ni «Solo sistema»: no integra el
universo de esta ejecución. Un CUIT incluido sin comprobantes en ninguna fuente no crea
un comprobante ficticio ni un resultado faltante.

El filtro debe ser idéntico para conciliación detallada, resumen por CUIT, apertura del
detalle desde ese resumen y exportación de resultados. Los contadores deben distinguir
registros cargados de registros incluidos cuando las cantidades difieran.

### 5.2 Propuesta P02 — lectura y reemplazo seguro

- Admitir XLSX, CSV y TXT delimitado; seleccionar hoja, encabezado/posición y columna de CUIT.
- Normalizar espacios y guiones; almacenar el CUIT como texto de once dígitos, sin conversión
  a número. Rechazar otros caracteres y longitudes incorrectas. No hacer consultas externas.
- Validar estructura de once dígitos en esta versión; no introducir una nueva validación
  fiscal del dígito verificador como requisito no acordado.
- Unificar CUIT repetidos e informar su cantidad. Ignorar filas completamente vacías.
- Una celda CUIT vacía dentro de una fila de datos, o un CUIT mal formado, bloquea el reemplazo.
- Mostrar cantidad de CUIT finales, incorporados y retirados frente a la lista anterior.
- Cancelación, error o archivo vacío conservan la lista anterior completa.
- El reemplazo persistido es atómico. Si falla, no queda una lista parcial ni vacía.

Con el filtro desactivado, una lista guardada no tiene efecto. Con el filtro activado y sin
lista válida, no se permite iniciar la conciliación. Desactivar el filtro no borra la lista.

<!-- Fecha: 05/10/2026 - TAREA: 00063 - Linea: 2 - Se documenta la adaptación de archivos de proveedor sin atribuirles origen ARCA ni interpretar su deuda. -->
## 6. Ítem 2: conciliación contra proveedor

### 6.1 Ingreso y selección del proveedor

1. Seleccionar un perfil de modalidad proveedor.
2. Solicitar CUIT y validarlo antes de abrir la conciliación.
3. Recuperar su formato y equivalencias, si existen.
4. Mostrar permanentemente el CUIT de la sesión.
5. Cargar el archivo del sistema con el formato del perfil y seleccionar sus comprobantes
   correspondientes al CUIT solicitado.
6. Seleccionar el archivo del proveedor y abrir la previsualización/configuración.
7. Validar el archivo completo; con cero errores, permitir «Importar».
8. Asignar el CUIT de contexto a cada comprobante y ejecutar el motor actual.

**Propuesta P03 — identificación:** permitir ingresar un CUIT nuevo sin exigir alta en el
maestro PRESEA. Ofrecer los CUIT con formato guardado como ayuda de selección. Una descripción
opcional facilita reconocer al proveedor; no interviene en el emparejamiento. Cancelar la
selección inicial no abre una conciliación incompleta.

El archivo del proveedor no se busca ni se filtra por una columna de CUIT. Si contiene datos
de otra empresa, la aplicación no puede verificarlo a partir de un CUIT ausente; por eso
la previsualización identifica con claridad el CUIT al que se atribuirá el archivo.

### 6.2 Formato único por CUIT

Un CUIT tiene una sola configuración actual, compartida por los perfiles de la misma base.
Modificarla sustituye esa configuración; no se ofrecen múltiples formatos para elegir.

**Propuesta P04 — opciones de lectura:** XLSX y CSV/TXT delimitado, con selección de hoja,
codificación, separador de campos, existencia y fila de encabezado, primera y última fila
de datos. El fin puede ser «hasta el final». Las columnas se identifican por nombre o posición.
Las filas vacías se omiten. El rango permite dejar fuera títulos y totales del reporte;
no habrá casillas para excluir comprobantes individuales con errores.

No se incluye en esta primera versión PDF, OCR, XLS binario ni TXT de ancho fijo. La extracción
por posiciones dentro del número combinado sí está incluida y es distinta del ancho fijo
del archivo completo. Estos límites son propuestas para revisión, no acuerdos previos.

### 6.3 Campos de comprobante

| Campo normalizado | Procedencia | Regla |
|---|---|---|
| CUIT | Selección inicial | Igual para todos los comprobantes del archivo. |
| Tipo | Columna elegida y equivalencia confirmada | Referencia al catálogo existente. |
| Fecha | Columna elegida | Interpretación según formato configurado. |
| Punto de venta | Columna propia o extracción de campo combinado | No se inventa si no puede obtenerse. |
| Número | Columna propia o extracción de campo combinado | Se conserva su identidad, incluidos ceros significativos. |
| Importe | Una columna elegida | Sin reconstruir pagos ni distinguir original/pendiente. |

La separación de punto de venta y número admite columnas independientes, un separador
configurado o posiciones/longitudes dentro de una columna. La vista previa muestra el resultado.
Ejemplos de formato: `00005-00001234` por separador y `0000500001234` por posiciones.
Si la regla no puede aplicarse a una fila, esa fila tiene un error; no se asigna cero por defecto.

El importe admite configuración de separadores decimal y de miles. Una fecha o importe
ilegible no se convierte silenciosamente en fecha mínima o cero. Un cero explícito es un
valor leído, distinto de un fallo de lectura. No se agrega una tolerancia nueva: el criterio
de comparación permanece en el motor actual.

### 6.4 Tipos y equivalencias

Se reutiliza el catálogo existente de tipos; no se crea un segundo catálogo fiscal. Sí se
persisten las equivalencias particulares del proveedor: CUIT + valor de origen → tipo existente.

El mismo código de origen puede significar cosas distintas para dos proveedores. Por ello,
confirmar `FA` para un CUIT no confirma esa equivalencia para todos los demás.

La previsualización agrupa los valores distintos de la columna de tipo y muestra:

| Dato | Contenido |
|---|---|
| Valor de origen | Texto tal como vino en el archivo. |
| Cantidad | Número de registros que utilizan ese valor. |
| Tratamiento | Código y descripción del catálogo existente, o «Excluir de la conciliación». |
| Estado | Confirmado, Sugerido o Sin resolver. |

La detección aplica primero equivalencias guardadas. Para valores nuevos propone coincidencias
por descripción, alias y código, normalizando para la búsqueda mayúsculas, espacios, tildes y
puntuación. Una descripción genérica como `FC` no permite inventar letra o variante fiscal.
Un código numérico coincidente con el catálogo tampoco se asume confirmado.

Toda sugerencia nueva exige confirmación. Confirmarla guarda la equivalencia automáticamente
y actualiza todas las filas afectadas. Si no hay coincidencia única, se selecciona manualmente
un tipo del catálogo. No se deduce el tipo a partir del signo del importe.

<!-- Fecha: 05/10/2026 - TAREA: 00063 - Linea: 2 - La exclusión confirmada por tipo permite leer reportes mixtos sin confundir movimientos ajenos al alcance con errores fiscales. -->
Solo FC, NC y ND integran el alcance. **Decisión D21 (antes P05):** pagos, recibos y otros
movimientos ajenos al alcance pueden clasificarse como «Excluir de la conciliación» en la
tabla de equivalencias. Se confirma una sola vez por valor de tipo y CUIT, se guarda
automáticamente y se aplica en futuras importaciones.

Las filas excluidas se muestran en la previsualización con su motivo y cantidad, pero no
ingresan al motor ni a sus totales. No son registros erróneos ni producen resultados
«Solo proveedor». La clasificación se realiza antes de validar los campos fiscales: un
recibo excluido no requiere un punto de venta fiscal. Un tipo desconocido no se excluye
por descarte; bloquea hasta que el usuario le asigne un tipo fiscal o confirme su exclusión.
No se ofrece exclusión por fila individual para eludir un error de un comprobante fiscal.

### 6.5 Previsualización y guardado automático

La previsualización muestra los datos originales y su interpretación; ambas grillas son de
solo lectura. Solo son editables los controles de formato y la tabla de equivalencias.

Debe poder consultarse el archivo completo mediante navegación; una muestra visual no
sustituye la validación de todas sus filas. Cada error identifica archivo, hoja si corresponde,
fila original, campo, valor leído y motivo. Se muestran cantidades de registros válidos,
excluidos por tipo y con error. «Importar» permanece deshabilitado mientras haya errores, tipos sin resolver o
una validación en curso.

Al modificar el formato se invalida la validación anterior y se recalcula la previsualización.
La importación debe usar exactamente la configuración y el archivo que superaron la última
validación; si el archivo cambia, hay que volver a validarlo.

**Propuesta P06 — semántica del guardado automático:** guardar cada cambio de configuración
estructuralmente válido al terminar de editar el control, sin botón Guardar. Las equivalencias
se guardan al confirmarlas. Las selecciones todavía incompletas permanecen en edición y no
pisan una configuración persistida válida. Una configuración guardada no implica que el
archivo haya pasado la validación. Mostrar «Guardando», «Guardado» o el error de persistencia.
No informar éxito de importación si no pudo guardarse la configuración correspondiente.

Cancelar la importación no incorpora comprobantes, pero mantiene los cambios de formato que
ya se guardaron automáticamente. Un fallo de lectura tampoco reemplaza los comprobantes de
una importación anterior por una colección parcial.

### 6.6 Validaciones bloqueantes

| Situación | Resolución esperada |
|---|---|
| Falta una columna obligatoria o no existe la hoja | Corregir la configuración o elegir el archivo correcto. |
| Tipo nuevo sugerido o sin equivalencia | Confirmar o seleccionar su tipo de catálogo. |
| Fecha/importe ilegible en comprobante incluido | Ajustar formato; si el dato es incorrecto, corregir el archivo fuera de la aplicación. |
| Punto de venta/número no extraíble | Corregir columnas o regla de separación. |
| Campo obligatorio vacío | Corregir el archivo de origen; no completar valores en la grilla. |
| Archivo sin comprobantes | Informar y no importar. |
| Fila con error fuera de la muestra visible | Bloquear igual: la validación cubre todo el archivo. |

Corregir el formato no es editar los valores. No se permite eliminar filas desde la grilla,
forzar «Importar de todos modos» ni omitir automáticamente comprobantes inválidos.

## 7. Conciliación, fechas y resultados

Una vez importados y seleccionados los comprobantes, se reutilizan orden, campos, reglas,
tolerancia y tratamiento de remanentes de las directivas existentes. No se agrega comparación
por deuda, vencimiento o fecha de pago, ni emparejamiento especial por proveedor.

El código actual aplica un rango de fechas en `FormComprobantesOffline` antes de invocar el
motor; además, `Fecha` puede integrar una directiva. Son mecanismos distintos. Ambos deben
mantenerse, sin ampliar períodos automáticamente por tratarse de comprobantes pendientes.

| Resultado proveedor | Significado |
|---|---|
| Conciliado | Emparejado por una directiva y coincidente según la tolerancia actual. |
| Diferencia de importe | Emparejado, con diferencia según el criterio actual. |
| Solo sistema | No emparejado del lado del sistema, dentro del alcance seleccionado. |
| Solo proveedor | No emparejado del lado del proveedor, dentro del alcance seleccionado. |

Se conserva el detalle de directiva aplicada y las explicaciones de emparejamiento. No se
eliminan comprobantes repetidos como si fueran CUIT duplicados de una lista: son problemas
distintos. Este cambio no redefine cómo el motor actual resuelve duplicados.

<!-- Fecha: 05/10/2026 - TAREA: 00063 - Linea: 2 - La confirmación del usuario fija la comparación por magnitud y evita que una NC negativa invierta dos veces su signo en el resumen. -->
### 7.1 Tratamiento de signos confirmado (D20; anteriormente P07)

La lectura actual del sistema en `LocalFileImporter` elimina guiones y paréntesis del importe.
La conciliación detallada resta los importes recibidos, mientras que el resumen por CUIT
aplica signo negativo según el tipo NC. Por eso preservar sin más un importe negativo del
proveedor podría producir una diferencia artificial o invertir dos veces el signo del resumen.

La nueva importación debe comparar la magnitud del importe para FC, NC y ND,
conservando en la previsualización el valor original y mostrando el valor normalizado. El
resumen conserva la asignación de signo por tipo que ya utiliza. No se cambia el importador
actual ni se interpretan saldos. Ejemplo: una NC informada como `-1.000,00` se compara como
`1.000,00` y aporta `-1.000,00` al resumen.

El usuario confirmó este criterio el 05/10/2026. FC y ND aportan magnitud positiva al resumen;
NC aporta magnitud negativa, independientemente del signo con que figure en el archivo.

## 8. Pantalla y exportación

En modalidad proveedor se usa «Proveedor» en títulos, columnas, estados, contadores,
detalles, resumen por CUIT y archivo de resultados. El importe externo se titula «Importe
proveedor» y el estado externo no emparejado, «Solo proveedor». El CUIT seleccionado es visible.

La exportación conserva el contenido útil del resultado actual e identifica la modalidad y
el proveedor. No presenta los registros como provenientes de ARCA. No ofrece exportación
de comprobantes a PRESEA, incluido el circuito por QR; la restricción debe comprobarse
también al ejecutar la acción, no depender únicamente de ocultar un botón.

**Propuesta P08 — vigencia del resultado:** recargar un archivo o cambiar la selección de
CUIT, lista, formato o directivas invalida el resultado anterior y exige volver a conciliar
antes de exportarlo. Una ventana abierta conserva una copia consistente de la configuración
con la que importó; un cambio realizado desde otra ventana no reinterpreta sus datos en silencio.

## 9. Modelo de información propuesto

Los nombres siguientes son orientativos de diseño. No constituyen un script de migración.

| Entidad | Identidad | Información y restricciones |
|---|---|---|
| Perfil offline existente | Id del perfil | Modalidad y opción de filtro CUIT, además de sus campos actuales. |
| Lista CUIT del perfil | Perfil + CUIT normalizado | Un CUIT por fila, sin duplicados dentro del perfil. |
| Formato de proveedor | CUIT normalizado | Un único formato actual, opciones de lectura y extracción; descripción opcional. |
| Equivalencia de proveedor | CUIT + valor de tipo normalizado | Acción «Conciliar» con tipo del catálogo, o «Excluir» sin tipo fiscal destino; valor de origen para mostrar. Solo confirmadas. |

Como propuesta física, las nuevas relaciones pueden vivir en `arca.ArcaPerfilesOfflineCuits`,
`arca.ArcaFormatosProveedor` y `arca.ArcaTiposProveedor`. Las equivalencias del proveedor no
deben sobrescribir `arca.ArcaEquivalencias`, que ya cumple otra función compartida.

Guardar formatos/equivalencias modifica únicamente el CUIT correspondiente. Reemplazar una
lista modifica únicamente el perfil correspondiente. Las operaciones de varias sentencias
usan transacción explícita; SQL parametrizado y Dapper en los nuevos componentes de datos.

**Propuesta P09 — concurrencia:** controlar la versión al actualizar formato/lista para no
pisar cambios de otra estación. Ante conflicto, conservar la edición visible, informar y
permitir recargar; no afirmar que se guardó. No se propone versionado histórico de formatos.

No se agrega persistencia de sesiones ni de archivos originales como requisito de este cambio.
La trazabilidad de fila se conserva durante la importación para explicar errores y lecturas.

## 10. Integración con la solución actual

Se recomienda adaptar las entradas al motor existente. Un segundo motor por proveedor
duplicaría las directivas; una reescritura general del motor ampliaría el alcance sin ser
necesaria para lo acordado. La propuesta concentra la variación en lectura, selección de
comprobantes y presentación de la fuente.

| Componente actual | Incidencia prevista, sin cambios realizados |
|---|---|
| `PerfilOffline` y editor/listado de perfiles | Modalidad y lista CUIT. Ya existe una incorporación local de `ListaCuits`; revisar su integración, no asumir que está persistida. |
| `FormComprobantesOffline` | Selección de origen, contexto de proveedor, alcance común y etiquetas. |
| `LocalFileImporter` / `TabularFileReader` | Evaluar reutilización de lectura tabular; no reutilizar sus omisiones silenciosas como validación de proveedor. |
| Catálogo, `TipoComprobanteStorage` y descripciones | Reutilizar códigos/descripciones; mantener separadas las equivalencias específicas por CUIT. |
| `ConciliacionService` | Conservar reglas; adaptar los datos normalizados de ambas fuentes a representaciones compatibles. |
| `ConciliacionExcelExporter` / resumen por CUIT | Etiquetas y contexto de origen, respetando el alcance filtrado. |
| Inicialización de ArcaCliente | Crear/actualizar esquema de manera idempotente desde el propio módulo. |

El motor offline no aplica automáticamente la equivalencia online de tipos. El diseño técnico
debe garantizar que proveedor y sistema llegan con representaciones comparables de tipo,
punto de venta, número, fecha e importe, sin modificar las claves o directivas del modo ARCA.
No basta con guardar una equivalencia si no se aplica al preparar el modelo.

Si se utiliza un adaptador al modelo externo actual, debe conservarse explícitamente el
origen Proveedor: no inventar CAE, datos impositivos ni habilitar PRESEA por reutilizar un tipo
de objeto cuyo nombre contiene ARCA.

Actualmente `ArcaSqlStorage.SavePerfilesOffline` borra y reinserta todos los perfiles.
Introducir relaciones hijas sin revisar ese guardado podría borrar listas por cascada o
fallar por integridad referencial. El futuro diseño técnico debe resolver el guardado de
los perfiles afectados de forma que preserve listas y cambios de otras estaciones; no se
propone una refactorización de todos los almacenamientos del módulo.

Las nuevas clases de almacenamiento se ubican en `Data/*Storage.cs`, la lógica en `Services/`
y los formularios no contienen SQL. El módulo debe poder abrir sus formularios desde cualquier
host que referencie la DLL, sin una inicialización especial a cargo del shell.

## 11. Criterios de aceptación

Estos casos describen la verificación de la futura implementación; no son pruebas ejecutadas.
Los casos de propuestas P01–P04, P06 y P08–P09 quedan sujetos a su revisión. Los criterios
de signos y exclusión, antes P07/P05, están confirmados como D20/D21.

| ID | Caso | Resultado esperado |
|---|---|---|
| A01 | Perfil ARCA con filtro desactivado | Mismo universo y resultado que el funcionamiento actual. |
| A02 | Lista contiene CUIT A, archivos contienen A y B | Solo A participa en detalle, resumen y exportación. |
| A03 | A aparece solo en una fuente y está incluido | Solo ARCA o Solo sistema según corresponda. |
| A04 | Importar lista B después de lista A | B reemplaza A; no queda la unión. |
| A05 | Nueva lista vacía o inválida | No reemplaza la lista válida anterior. |
| A06 | Filtro activado sin lista válida | No inicia la conciliación. |
| A07 | CUIT repetidos y con guiones/espacios | Una entrada normalizada por CUIT. |
| A08 | Perfil proveedor, cancelar selección CUIT | No abre una conciliación sin proveedor. |
| A09 | Archivo proveedor sin CUIT | Cada comprobante recibe el CUIT de contexto. |
| A10 | Archivo del sistema contiene otros proveedores | Los otros CUIT no participan ni aparecen como Solo sistema. |
| A11 | Proveedor usado desde otro perfil | Recupera el mismo formato y equivalencias de ese CUIT. |
| A12 | Número combinado por separador o posiciones | Vista previa y modelo muestran punto de venta/número correctos. |
| A13 | Intentar editar una celda de previsualización | No se permite; solo se editan opciones de formato. |
| A14 | Nuevo tipo con sugerencia | Requiere confirmación antes de importar. |
| A15 | Tipo ya confirmado en otro archivo del mismo CUIT | Se resuelve automáticamente. |
| A16 | Mismo alias en otro proveedor | No hereda automáticamente la confirmación del primero. |
| A17 | Una fila inválida entre filas válidas | No se importa ninguna hasta corregir; se informa ubicación y causa. |
| A18 | Error después de la porción visible de la vista previa | Bloquea igualmente. |
| A19 | Cambiar formato | Se recalcula la vista previa, se guarda según P06 y se revalida. |
| A20 | Factura proveedor por 60.000 y sistema por 100.000 | Se compara 60.000 contra 100.000; no se reconstruyen pagos. |
| A21 | Comprobante solo en sistema | Se muestra Solo sistema aunque el proveedor informe únicamente pendientes. |
| A22 | Comprobante solo en proveedor | Se muestra Solo proveedor. |
| A23 | Misma entrada normalizada y mismas directivas | Mismos emparejamientos, orden de directivas y tolerancia del motor actual. |
| A24 | Fecha en directiva y rango de pantalla | Se conserva el comportamiento actual, sin extensión automática del período. |
| A25 | Exportar resultado proveedor | Identifica proveedor/CUIT y no usa títulos Solo ARCA o Total ARCA. |
| A26 | Intentar enviar comprobantes de proveedor a PRESEA/QR | Acción no disponible y ejecución impedida. |
| A27 | NC negativa según D20 | Se compara su magnitud y se resta en el resumen, sin doble inversión del signo. |
| A28 | Error de guardado o actualización concurrente | No informa guardado exitoso ni deja configuración/lista parcial. |
| A29 | Abrir el módulo desde otro host y desde el shell | Ambos inicializan las estructuras necesarias sin pasos especiales. |
| A30 | Repetir inicialización con perfiles existentes | No se pierden perfiles, directivas, listas ni formatos. |
| A31 | Reporte mixto con REC/PAGO clasificados para excluir | Se cuentan y muestran como excluidos; no exigen campos fiscales ni participan en importación, conciliación o totales. |
| A32 | Tipo desconocido en reporte mixto | Bloquea hasta confirmar tipo fiscal o exclusión; no se excluye silenciosamente. |
| A33 | Reimportar otro archivo del mismo CUIT con REC/PAGO ya clasificados | Reutiliza automáticamente sus exclusiones y muestra la nueva cantidad excluida. |

## 12. Revisión solicitada y cierre de esta etapa

Las decisiones D01–D21 recogen lo acordado. Los signos y las exclusiones, antes P07/P05,
quedaron confirmados como D20/D21. Las propuestas P01–P04, P06 y P08–P09 completan aspectos
de operación y diseño que no se aprobaron individualmente, entre ellos los límites de
formatos. Las confirmaciones puntuales no constituyen aprobación general de esas propuestas
ni autorización para programar. El usuario sí solicitó las maquetas y el resumen para cliente.

La revisión del código fue de solo lectura. No se cargaron archivos reales de proveedores,
no se consultó la base y no se ejecutó una conciliación. Los formatos propuestos deben
contrastarse con ejemplos representativos antes de cerrar el alcance de implementación.

El entregable de esta etapa es esta especificación para revisión. No se inicia programación
ni se considera aprobada la especificación por el solo hecho de guardarla o versionarla.

<!-- Fecha: 05/10/2026 - TAREA: 00063 - Linea: 4 - El resumen comunica al cliente solo el alcance confirmado, sin presentar el desarrollo como realizado. -->
<!-- Fecha: 05/10/2026 - TAREA: 00063 - Linea: 5 - Las pantallas son un artefacto de revisión separado de la aplicación. -->
## 13. Resumen para cliente y diseño de pantallas

La TAREA 00063 se encuentra reservada en el registro global y anotada en `docs/Historial.md`.
Se mantiene el mismo número para el análisis y estos entregables; no se reserva otro número
para una futura implementación que todavía no fue solicitada.

- [Resumen para enviar al cliente en PDF](../../../output/pdf/TAREA-00063-resumen-cliente.pdf):
  una página con las modalidades, previsualización, tipos y exclusiones, resultados y límites.
- [Maqueta navegable de siete pantallas](../../Diseno/00063-conciliaciones/pantallas.html):
  incluye modalidades, lista CUIT, selección de proveedor, formato, equivalencias/exclusiones,
  validación bloqueada/válida y resultado.
- [Guía de pantallas y recorridos](../../Diseno/00063-conciliaciones/guia-pantallas.md):
  explica qué simula la maqueta y cómo revisar cada caso.

La distribución visual es una propuesta para revisión. Los ejemplos son ficticios; no se
procesan datos del cliente. El resumen está listo para adjuntar a un correo, pero no se envió.
