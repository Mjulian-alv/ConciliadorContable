<!-- Fecha: 05/10/2026 - TAREA: 00063 - Linea: 5 - Se documentan las decisiones visuales y los escenarios de la maqueta para revisarlos antes de implementar. -->
# Diseño de pantallas de conciliación

**TAREA 00063 · 05/10/2026 · Propuesta visual para revisión.**

La [maqueta navegable](pantallas.html) funciona abriendo el archivo en un navegador. No requiere
instalación, conexión a Internet ni acceso a la base. Es un entregable de diseño independiente:
no es una nueva versión de la aplicación ni procesa archivos reales. Los nombres, CUIT y datos
mostrados son ficticios. La navegación lateral y los botones de escenarios pertenecen al visor
de diseño, no son nuevas opciones del producto.

## Criterio visual

Se propone una interfaz de escritorio clara, compatible conceptualmente con los controles
WinForms/Telerik existentes: encabezado estable, grupos de configuración, tablas con filas
alternadas y pie de acciones. Tipografía Segoe UI, fondo claro, encabezado azul oscuro y
acción principal verde. Estados con texto además de color: válido, pendiente, error y excluido.

La barra de contexto mantiene visibles proveedor y CUIT durante toda la importación.
La configuración editable está separada de las grillas de solo lectura. El guardado automático
se informa junto al título, sin un botón Guardar formato. La validación explica qué falta
resolver y no ofrece una acción para ignorar errores.

La maqueta reorganiza visualmente opciones del perfil para facilitar su lectura. Esto no
propone modificar las reglas de las directivas ni reemplazar sus editores existentes.
El tamaño final de los formularios y su ajuste a las estaciones del cliente se revisarán
antes de implementar; el visor permite recorrer las pantallas en distintos anchos.

## Pantallas

| Nº | Pantalla | Contenido y comportamiento |
|---|---|---|
| 01 | Perfil de conciliación | Modalidades excluyentes ARCA/proveedor. Solo ARCA muestra filtro y administración de lista CUIT. Conserva archivo del sistema y directivas. En proveedor informa que el CUIT se solicita al ingresar. |
| 02 | Lista de CUIT | Archivo candidato, hoja, encabezado y columna CUIT. Cuenta válidos, repetidos y errores. Muestra normalización y diferencia respecto de la lista actual. Reemplazar conserva la lista anterior hasta completar la operación. |
| 03 | Seleccionar proveedor | Solicita CUIT antes de abrir la conciliación. Ofrece como ayuda un proveedor con formato guardado y permite introducir otro CUIT. No exige columna CUIT en el archivo. |
| 04 | Archivo y formato | Opciones de lectura, mapeo de columnas y número combinado/separado/por posiciones. Pestañas de datos originales e interpretación, ambas de solo lectura. Incluye formato de fecha y decimal. |
| 05 | Tipos y exclusiones | Agrupa valores distintos con cantidad de filas. Propone tipo del catálogo o Excluir de la conciliación. Confirmar guarda la decisión para el CUIT. Modificar una decisión requiere volver a confirmarla. |
| 06 | Revisar e importar | Archivo completo de ejemplo, con originales y valores normalizados. Conteos de válidos, excluidos y pendientes/errores. Filtros de visualización sin modificar el universo. Importar queda deshabilitado ante cualquier pendiente o error fiscal. |
| 07 | Conciliación | Contexto de proveedor, fuentes, período actual, cuatro estados y resumen con signos por tipo. Conserva la acción Conciliar y permite exportar resultados; sin acción PRESEA. |

El paso de importación se presenta en tres partes: **Archivo y formato → Tipos y exclusiones
→ Previsualización**. Puede volverse a las anteriores para corregir la configuración.
La aplicación deberá conservar la posibilidad actual de ejecutar la conciliación después
de cargar las fuentes; abrir la pantalla de resultado de ejemplo en la maqueta no define
una conciliación automática al importar.

## Recorridos de revisión

### Contra ARCA

1. En Perfil, seleccionar Contra ARCA y activar Filtrar por lista de CUIT.
2. Abrir Administrar lista. Revisar la lista candidata: cinco filas, un repetido y cuatro CUIT únicos.
3. Reemplazar la lista y volver al perfil; el conteo cambia a cuatro.
4. Para revisar un error, elegir Ver con errores en el visor y luego abrir Lista de CUIT.
   El CUIT mal formado bloquea Reemplazar lista y conserva la anterior.

### Contra proveedor

1. Reiniciar recorrido, seleccionar Contra cuenta corriente de proveedor y guardar el perfil.
2. Confirmar el CUIT del ejemplo. Se abre la configuración del archivo.
3. Revisar la columna de tipo, fecha e importe y el número combinado.
4. En Tipos y exclusiones, confirmar NCA, NDA, REC y PAGO. FA ya figura guardado.
5. En Previsualización se muestran ocho movimientos: seis comprobantes válidos y dos excluidos.
6. Importar conduce al resultado ilustrativo del recorrido. La pantalla permite filtrar estados
   y descargar un CSV identificado como EJEMPLO, exclusivamente con datos ficticios.

### Error y corrección de formato

1. Elegir Ver con errores. La fila 10 presenta 26/09/2026 bajo un formato MM/dd/yyyy incompatible.
2. El pie indica Importación bloqueada y el botón Importar está deshabilitado.
3. Corregir formato lleva al primer paso. Elegir dd/MM/yyyy y volver a Previsualización.
4. La carga queda habilitada cuando no hay errores ni clasificaciones pendientes.

Los escenarios y los valores de salida están preparados para revisar el diseño. Los controles
generales de mapeo ilustran las opciones previstas; no implementan un lector tabular completo.
El resultado es un ejemplo fijo, no un motor de conciliación nuevo. La simulación conserva
decisiones durante el recorrido en memoria; no escribe configuraciones reales en disco o SQL.

## Coherencia del ejemplo

El archivo del proveedor contiene cuatro FC, una NC, una ND, un recibo y un pago. El recibo
y el pago se excluyen por tipo; nunca se eliminan mediante edición individual de filas.

Cinco comprobantes se emparejan: cuatro coinciden y uno presenta diferencia. Un comprobante
queda Solo proveedor y otro Solo sistema. El resultado tiene siete filas, que representan
seis comprobantes por fuente. Los totales con NC negativa son 261.500,00 para proveedor y
276.400,00 para sistema, con diferencia de -14.900,00. Los pagos y recibos no integran esos totales.

## Verificación del entregable

Se recorrieron las siete pantallas en navegador y se revisaron capturas de cada una. Se
verificaron cambio de modalidad, bloqueo por tipos pendientes, confirmación de exclusiones,
bloqueo por fecha inválida, corrección de formato, carga válida, filtro del resultado y
reemplazo de lista. Sin errores de ejecución ni desbordes horizontales de página en anchos
de 768, 850, 1280 y 1536 píxeles; las tablas anchas conservan desplazamiento interno.

Esta verificación corresponde a la maqueta. No se ejecutó ni se modificó el programa .NET.

## Entregables relacionados

- [Especificación funcional](../../superpowers/specs/2026-10-05-conciliacion-arca-cuits-proveedor-design.md).
- [Resumen para el cliente en PDF](../../../output/pdf/TAREA-00063-resumen-cliente.pdf).
- [Texto editable del resumen](resumen-cliente.md).
