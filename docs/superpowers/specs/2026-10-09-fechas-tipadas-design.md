# TAREA 00083 — Fechas tipadas en importación y migración de bancos

Fecha: 09/10/2026. Estado: propuesta escrita para revisión; implementación y
migración de datos pendientes. Dos trabajos coordinados, numerados por pedido.

## Objetivo y decisiones del usuario

1. Sistema: interpretar la fecha al importar y usar un campo SQL `date` en todo
   el circuito, eliminando la persistencia operativa y comparación de fechas como texto.
2. Base existente: convertir los datos conservando los originales. Usar `fecha2`,
   creada por el usuario para probar la conversión, como columna intermedia.

El usuario confirmó que una fecha vacía o no interpretable debe rechazar el archivo
completo e informar las filas. No se guardarán parcialmente los movimientos válidos.
La configuración visual que resulte necesaria irá en los archivos Designer.

## Diagnóstico realizado en lectura

Se consultó la base configurada por el shell, sin ejecutar inicializadores del módulo
ni cambios de estructura o datos.

| Elemento | Situación al 09/10/2026 |
|---|---|
| `bancos.MovimientosArchivo.Fecha` | `nvarchar(30) NULL`, 64.875 movimientos |
| Fechas vacías o no convertibles en el diagnóstico SQL | 0 |
| Fechas con barras que admiten día/mes y mes/día diferentes | 20.892 |
| `bancos.MovimientosArchivo.fecha2` | `smalldatetime NULL` |
| Valores cargados en `fecha2` | 45.992; todos coinciden con la conversión día/mes de `Fecha` |
| Valores pendientes en `fecha2` | 18.883 |
| Valores de `fecha2` con hora distinta de cero | 0 |
| `bancos.ConciliacionItemsExternos.Fecha` | `nvarchar(30) NULL`, tabla sin filas |
| Sesiones externas / internas | 0 / 3 |

Estos conteos son una fotografía y deben recalcularse al ejecutar la migración.
La conversión admisible en SQL no demuestra por sí sola cuál era la intención del
archivo de origen. La propuesta usa día/mes/año, coherente con los valores existentes
en `fecha2` y el criterio prioritario actual. La respuesta del usuario indicó el uso
de `fecha2`; no confirmó expresamente el orden día/mes para todos los archivos futuros.
La aprobación de esta propuesta debe incluir ese criterio. Si hay fuentes mes/día,
deberán distinguirse por origen antes de implementarlo; no se adivinará por fila.

El defecto empieza en `ImportacionService.Parsear`: convierte la celda de fecha a
texto mediante `ToString()`, que puede depender de la cultura del equipo. Los modelos
operativos también usan `string`. Después se interpreta nuevamente en comparadores,
filtros, exportadores y algunas consultas SQL, con criterios distintos.

## Enfoque elegido para la propuesta

Usar `fecha2` como transición y conservar el texto anterior es la alternativa indicada
por el usuario. Permite comparar antes de convertir y revisar el valor de origen.
Cambiar directamente el tipo de `Fecha` no ofrece ese control intermedio. Mantener
dos campos operativos permanentes exigiría sincronizarlos y conservaría dos posibles
fuentes de verdad; por eso se propone un único campo operativo final.

## Trabajo 1: sistema

### Contrato de fecha

- `bancos.MovimientosArchivo.Fecha` y `bancos.ConciliacionItemsExternos.Fecha` serán
  `date NOT NULL`.
- Usar `DateTime` en los modelos .NET existentes, normalizado a `.Date`, y parámetros
  de fecha tipados al persistir. No convertir fechas operativas a texto para SQL.
- El formato de pantalla y Excel será `dd/MM/yyyy`. El almacenamiento no dependerá
  de la configuración regional del equipo.
- Las marcas de auditoría —fecha de importación, creación de sesión y conciliación—
  conservarán sus tipos con hora. Son eventos distintos de la fecha del movimiento.

### Importación

Un servicio común interpretará fechas únicamente en la frontera de entrada:
celdas Excel ya tipadas como `DateTime`, textos día/mes/año con barras o guiones y
textos ISO año-mes-día. Admitirá horas en los formatos explícitamente soportados y
conservará el día calendario, sin transformaciones de zona horaria.

No usará `CurrentCulture`, comparación de textos ni fallback a mes/día. Números que
ExcelDataReader no entregue como fecha no se asumirán automáticamente como seriales
Excel: deberán informarse como inválidos, evitando confundir otros números con días.

La columna fecha será obligatoria. Se recorrerán las filas importables, acumulando
las incidencias con archivo, hoja cuando corresponda, número de fila, valor original
y motivo. Una fila completamente vacía no es un movimiento. Se conserva la definición
actual de fila importable; este trabajo no cambia las reglas de conceptos e importes.

Si hay incidencias, se rechazará el archivo antes de crear su cabecera o movimientos.
La persistencia de cabecera y lote deberá compartir una transacción explícita para
que un error de guardado tampoco deje una cabecera huérfana. La lectura del archivo
externo aplicará el mismo contrato antes de crear una sesión.

### Consumidores y presentación

Actualizar los modelos de movimientos, ítems externos y proyecciones de pares internos
y externos. Los comparadores usarán fechas tipadas; los filtros SQL usarán `Fecha`
directamente y los límites del rango como parámetros. Se retirarán las conversiones
texto-fecha-texto y los avisos de fechas ilegibles que dejen de corresponder al contrato.

Revisar el selector de extractos, consulta de conceptos, conciliación interna/externa,
protección de bajas, diálogos de candidatos y exportaciones. Los Excel contendrán
fechas reales con formato visual, conservando las reglas actuales de importes y grupos.
La columna histórica de texto no se expondrá como otra fecha operativa en las grillas.

### Inicialización y compatibilidad

Las instalaciones nuevas se auto-inicializarán con las columnas correctas. Para una
base anterior, el módulo detectará la incompatibilidad y dará un mensaje específico
para ejecutar la migración; no hará conversiones masivas silenciosas al abrir un form.
La aplicación nueva y el cambio de estructura deben habilitarse coordinadamente:
no se garantiza el funcionamiento de ejecutables viejos contra el esquema convertido.

También se revisará `Tools/MigradorDataBanks`, porque copia fechas históricas de SQLite
y no puede escribir textos directamente en el contrato nuevo ni eludir su validación.

## Trabajo 2: base existente

### Diagnóstico reproducible

Preparar una herramienta o scripts con una fase de diagnóstico sin modificaciones,
separada de la aplicación. Verificar tipos, dependencias, conteos y conversiones de
ambas tablas. Informar faltantes, fechas no interpretables y discrepancias entre el
texto y `fecha2`, con identificadores que permitan corregirlos.

La existencia de un valor en `fecha2` no autoriza a sobrescribirlo. Cualquier diferencia
frena la conversión y requiere resolver la fila. La herramienta debe tolerar que la
columna intermedia no exista en otra instalación y crearla sólo en la fase de aplicación.

### Aplicación transaccional

Con la aplicación cerrada durante el cambio y una copia de seguridad identificada:

1. Releer y bloquear las tablas afectadas en una transacción, sin basarse exclusivamente
   en el diagnóstico anterior. Verificar que el estado de las columnas sea reconocible.
2. Comparar los valores preexistentes en `fecha2` contra la conversión acordada y
   comprobar que no tengan una hora significativa antes de reducirlos a `date`.
3. Llevar la columna intermedia a `date` y completar exclusivamente sus valores nulos.
   Cualquier fecha inválida, diferencia o dependencia no contemplada abortará el cambio.
4. Exigir que todas las filas tengan una fecha válida y que los conteos coincidan.
5. Conservar `Fecha` textual bajo el nombre `FechaTextoOriginal`; renombrar la columna
   intermedia como `Fecha` y exigir `NOT NULL`. Los textos originales no se eliminan.
6. Aplicar el mismo resultado a `ConciliacionItemsExternos`, creando su columna
   intermedia cuando sea necesaria. Aunque hoy esté vacía, contemplar instalaciones
   con datos y conservar sus textos originales de la misma manera.
7. Verificar tipos finales, equivalencia por ID, cantidades y relaciones de sesiones
   y pares, y confirmar la transacción sólo si todas las comprobaciones pasan.

Los IDs, importes, conceptos, archivos y relaciones de conciliación se conservan.
Una segunda ejecución debe reconocer el esquema terminado y verificarlo sin repetir
renombrados. Un estado parcial o desconocido debe detenerse con un diagnóstico claro.

`FechaTextoOriginal` es evidencia histórica, nullable para las nuevas filas. No se
usará para conciliar ni se mantendrá como una segunda fecha operativa. Su eliminación
no forma parte de este trabajo. Si la transacción falla, se revierte completa. Después
del corte, una reversión requerirá coordinar la versión del sistema y los movimientos
nuevos; conservar el texto histórico no sustituye una copia de seguridad completa.

## Comprobación antes de aplicar a la base real

1. Parser: Excel tipado, DMY con barras/guiones, ISO, fecha con hora, años bisiestos,
   valores vacíos e inválidos, ejecutando bajo culturas argentina y estadounidense.
2. Importación: varias incidencias se informan con sus filas y el archivo inválido
   no deja cabecera ni movimientos; un fallo de escritura revierte el archivo completo.
3. SQL: fechas guardadas con tipo `date`; rangos, orden y extremos inclusivos correctos.
4. Regresiones: conciliación interna y externa, grupos 1:N, protección de bajas,
   selector, grillas y Excel con fechas tipadas.
5. Migración en una base aislada: `fecha2` parcial, completa, inexistente o discrepante;
   texto inválido o vacío; tabla externa con filas; fallo transaccional y repetición.
6. Ensayo sobre una copia de los datos reales, contrastando los resultados por ID y
   verificando que los textos originales y las tres sesiones actuales se conservan.
7. Compilar el shell y las herramientas afectadas. Las pruebas de datos deben usar
   bases aisladas, no modificar la base operativa.

## Entrega y secuencia

Primero se prepara y verifica el sistema junto con la migración ensayada. Después se
realiza el corte de base y ejecutables de forma coordinada. Ambos trabajos se registran
por separado dentro de esta tarea; un build correcto no equivale a una base migrada.

Esta etapa deja diagnóstico y diseño. No se modificaron datos productivos ni código
del sistema para esta tarea, y todavía no se ejecutaron pruebas de la implementación.
