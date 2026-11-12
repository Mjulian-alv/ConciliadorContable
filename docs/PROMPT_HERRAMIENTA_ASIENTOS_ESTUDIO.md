# Herramienta "Asientos para el estudio contable" — prompt para Claude Code

> Ubicación sugerida en el repo: `docs/prompts/PROMPT_HERRAMIENTA_ASIENTOS_ESTUDIO.md`
> Archivos de ejemplo (copiar al repo): `samples/BANCOS FCE 2027.xlsx`, `samples/ASIENTO DEVENGAMIENTOS.xlsx`, `samples/ASIENTOS VARIOS 05-2026 a 08-2026.xlsx`, `samples/layout asientos.docx`

---

## 0. Cómo trabajar (obligatorio, en este orden)

1. **Explorá el repo primero.** Identificá stack, patrón de capas, cómo se organizan módulos/pantallas, si existe un **plan de cuentas** y cómo se accede a él, y dónde engancha un módulo nuevo en el menú. Respetá el `CLAUDE.md` del repo y el global (por defecto: Controllers/Services/Repositories, inyección de dependencias, FluentValidation, `ILogger`; si la app es Blazor, componentes Blazor; si usa Syncfusion, usá Syncfusion). Si algo del stack no está claro, hacé **una sola pregunta corta** antes de seguir.
2. **NO escribas código todavía.** Entregá primero, en `docs/asientos-estudio/`:
   - **Maquetas de todas las pantallas** (HTML estático o imágenes).
   - **Flujograma del circuito completo** (Mermaid `.mmd` o `.md` con Mermaid): carga del Excel → detección de formato → lectura → validaciones/alertas → resolución → vista previa → generación del TXT → guardado.
   - Un plan de implementación por etapas.
   Detenete y esperá mi OK sobre maquetas y flujograma. Este paso es obligatorio, igual que definir la tarea y revisar el commit.
3. Recién con el OK: implementá por etapas, con tests, y mostrame después de cada etapa qué funciona ya.

---

## 1. Objetivo

Una herramienta dentro de nuestra aplicación contable que:

1. Recibe planillas Excel con asientos armados por el área contable.
2. Las lee con perfiles de lectura por tipo de planilla (ver sección 4).
3. Valida todo y muestra **alertas** claras (sección 6) antes de generar nada.
4. Genera un **TXT con separador TAB** para importar en **Holistor Contable** (layout en sección 3), listo para enviar al estudio contable.

Hoy este proceso se hizo a mano con scripts. Esta herramienta lo convierte en algo repetible, auditable y que detecte errores antes de mandar el archivo.

---

## 2. Flujo de usuario (a maquetar)

1. **Cargar Excel** (arrastrar o elegir archivo).
2. La herramienta **detecta el tipo de planilla** (Bancos / Devengamientos / Varios) y lo muestra; el usuario puede cambiarlo.
3. Elegir **rango de meses** a incluir (por defecto, todos los meses encontrados; los meses fuera del rango habitual se resaltan).
4. **Vista previa**: árbol Mes → Hoja → Asiento, con estado por asiento (OK / Advertencia / Error / Omitido), total Debe, total Haber y diferencia.
5. **Panel de alertas** con filtro por severidad, y acciones para resolver (ver sección 6).
6. **Generar TXT**: muestra resumen (cantidad de asientos, líneas, totales, hash/checksum) y guarda en la carpeta configurada.
7. **Historial** de generaciones (quién, cuándo, archivo de origen, parámetros, alertas aceptadas) para auditoría.

Pantallas mínimas: Carga y detección · Vista previa de asientos · Panel de alertas / resolución (incluye mapeo nombre→cuenta) · Resumen y generación · Historial · Configuración.

---

## 3. Formato de salida (Holistor Contable — "Archivo Único de Movimientos", una columna Debe y otra Haber)

TXT, **una línea por movimiento**, **11 columnas separadas por TAB**, siempre las 11 aunque estén vacías (10 tabs por línea).

| # | Columna | Obligatorio | Contenido que generamos |
|---|---------|-------------|-------------------------|
| 1 | Fecha | Sí | **Último día del mes** del asiento, `dd/mm/aaaa` (ej. `31/05/2026`) |
| 2 | Número de asiento | Sí | **Correlativo por asiento**, 4 dígitos (`0001`…). Todas las líneas de un mismo asiento llevan el mismo número |
| 3 | Número de cuenta | Sí | Código tal cual, con puntos (ej. `2.01.01.003`) |
| 4 | Debe | Sí | Coma decimal, 2 decimales, sin separador de miles (`1398900661,60`). Si no tiene importe en Debe: `0,00` |
| 5 | Haber | Sí | Igual que Debe |
| 6 | Concepto | No | Depende del tipo de planilla (sección 4) |
| 7 | Leyenda | No | Vacío |
| 8 | Tipo | No | Vacío |
| 9 | Centro de Costo | No | Vacío |
| 10 | Fecha alternat. | No | Vacío |
| 11 | Unidad | No | Vacío |

Reglas del archivo:

- **Sin fila de títulos.**
- Fin de línea **CRLF**, codificación **ANSI Windows-1252** (los conceptos llevan acentos). Hacer la codificación configurable (cp1252 / UTF-8).
- Redondeo **half-up a 2 decimales** por línea (usar `decimal`, nunca `double`). Luego aplicar la regla de centavos de la sección 6.
- **Orden de salida:** por mes, luego por hoja (en el orden de las hojas del Excel), luego por orden de aparición del asiento. La numeración correlativa sigue ese mismo orden y **no se reinicia** entre meses ni hojas.
- Los **asientos informativos, vacíos o en cero no se numeran** (la numeración no tiene huecos).
- Carpeta de destino por defecto (configurable): `D:\DESARROLLOS CONTABLE\HOMOLOGADOR\ASIENTO CONTABLE ESTUDIO`. Nombre de archivo sugerido: `<nombre del origen> <mes desde> a <mes hasta>.txt`. No pisar archivos existentes sin confirmar.

Supuestos a dejar **configurables** y confirmar conmigo: formato de fecha (`dd/mm/aaaa` vs `aaaammdd`), regla de numeración (correlativo global vs. reinicio por fecha, el layout dice "número de orden dentro de la fecha"), codificación, y si va fila de títulos.

---

## 4. Perfiles de lectura (tres tipos de planilla)

Implementar cada tipo como una **estrategia de parser** (interfaz común `IAsientoParser` o equivalente), con tests propios, de modo que sumar un tipo nuevo no toque a los demás. Las planillas **no tienen estructura rígida**: se detecta por patrones de texto, no por posición de fila.

Todas las planillas usan columnas A = código, B = nombre de cuenta, C = Debe, D = Haber. Las celdas vacías cuentan como 0. Los importes con fórmula se leen del **valor calculado guardado** en el archivo (no recalcular).

### 4.1 BANCOS (ej. `BANCOS FCE 2027.xlsx`)

- **Una hoja por banco/cuenta** (BBVA, MUNICIPAL, BICA, COMAFI, CREDICOOP, GALICIA, HIPOTECARIO, MACRO 663, MACRO 664, BNA CA, BNA CC, NBSF, PATAGONIA, SANTANDER).
- Marcador de mes: celda `MES 05/2026`.
- Encabezado de asiento: `ASIENTO n` (puede seguir ` · INFORMATIVO — NO SE REGISTRA`).
- Fila de títulos de columnas: `Código | Cuenta / concepto | Debe | Haber`.
- Cierre: fila `SUMATORIA DEL ASIENTO` (se ignora).
- **Asientos con "INFORMATIVO" / "NO SE REGISTRA" en el encabezado: se excluyen** (en la muestra, 38). Hay uno con una nota "Las transferencias hacia otros bancos se registran en el banco receptor".
- **Concepto = `MM-AAAA-NOMBRE DE LA HOJA`** (ej. `05-2026-BBVA`, `06-2026-MACRO 663`).

### 4.2 DEVENGAMIENTOS (ej. `ASIENTO DEVENGAMIENTOS.xlsx`)

- **Una hoja por mes**, nombrada `MM-AAAA`.
- Cada asiento tiene una **línea de encabezado** de texto (ej. `IVA - Asiento principal`, `IIBB - Devengamiento`, `Ret. de 3eros. - 1era quincena - IIBB`), luego la fila `Código de cuenta | Nombre de la cuenta | Debe | Haber` (a veces sin tilde: `Codigo`), las líneas y una fila `TOTAL`.
- **Concepto = el texto de la línea de encabezado del asiento.**
- Un asiento sin ninguna línea de cuenta se omite (ej. `Ret. de 3eros. - 1era quincena - SUSS` en 05-2026).

### 4.3 VARIOS (ej. `ASIENTOS VARIOS 05-2026 a 08-2026.xlsx`)

- **Hojas por circuito:** VENTAS, COMPRAS, CAJAS, CHEQUES EMITIDOS, CHEQUES ENDOSADOS, SUELDOS. Varios meses por hoja, uno debajo de otro.
- Marcador de mes con **dos formatos posibles**: `05-2026` (VENTAS) o `MAYO 2026` (resto). Aceptar ambos.
- Encabezado de asiento = fila con texto en A y B, C, D vacías, **seguida** de la fila `Código…` o directamente de una línea con código. Ejemplos: `ASIENTO 1 — VENTAS, COMISIONES E IMPUESTOS`, `Compras`, `Seguros`, `Por egresos de caja`, `Por endoso de echeq a MUNDO`.
- **Concepto = texto del encabezado**, quitando el prefijo `ASIENTO n — ` si existe.
- **Notas y aclaraciones** debajo de un asiento (`Por operaciones de extracash…`, `USD 15.000,00 | Tipo de cambio…`, `Por ventas en dólares — Representan USD…`) **no son encabezados ni líneas**: se distinguen porque lo que sigue no es una fila de códigos. Ignorarlas.
- Columnas extra a la derecha (F a N: cálculos auxiliares, comentarios, tablas de cheques) **se ignoran**.
- Se excluye el bloque `Asiendo del ejercicio anterior` / `ASIENTO QUE NO SE REGISTRA` (hoja CHEQUES ENDOSADOS).
- Asientos donde todas las líneas están en cero/sin importe (ej. `Por la emisión de echeqs a CAM`) **se omiten** (el usuario lo confirmó).

---

## 5. Selección de meses

- Detectar todos los meses presentes y listarlos.
- Permitir elegir el rango. Un mes fuera del rango esperado se informa como alerta informativa y el usuario decide (en la muestra de VARIOS aparecía septiembre 2026 en dos hojas aunque el archivo se llamaba "05-2026 a 08-2026"; el usuario decidió incluirlo).

---

## 6. Catálogo de alertas (lo central de la herramienta)

Cada alerta tiene: **severidad** (Bloqueante / Advertencia / Informativa), **ubicación exacta** (hoja, mes, asiento, número de fila del Excel, nombre de cuenta), **causa** y **acción de resolución**. Una alerta Bloqueante impide generar el TXT hasta resolverla o aceptarla explícitamente (queda registrado quién la aceptó). Todas quedan en el historial y se exportan a un listado (CSV/PDF) para que el contador corrija el Excel.

| # | Alerta | Severidad | Qué pasó en los datos reales | Comportamiento esperado |
|---|--------|-----------|------------------------------|-------------------------|
| A1 | **Línea sin código de cuenta** | Bloqueante | 15 líneas en BANCOS: COMAFI BURSATIL, MACRO SECURITIES, SANTANDER VALORES, PRESTAMO NBSF / A PRÉSTAMO NBSF, MULTAS | Listar cada línea. Ofrecer **mapeo nombre→código** (normalizando mayúsculas, tildes y espacios; `A PRÉSTAMO NBSF ` y `PRESTAMO NBSF` son la misma cuenta). Guardar el mapeo para próximas cargas. Opción de generar igual con el campo cuenta vacío **solo si el usuario lo acepta explícitamente** (caso real: se hizo así mientras el estudio no pasaba los códigos) |
| A2 | **Código con formato inválido** | Advertencia | `4,01,05,050` (comas en vez de puntos) en COMPRAS | Normalizar a `4.01.05.050`, avisar y dejar registrado para que se corrija en el origen. Si luego sigue sin cumplir el patrón del plan de cuentas → Bloqueante |
| A3 | **Fila de totales suelta dentro del asiento** | Advertencia | SANTANDER 05/2026, asiento 1: fila sin código ni nombre con Debe y Haber cargados a mano (488.708.350,94), metida dentro del rango de la suma | Ignorar la fila y avisar. Regla general: una fila sin código **y** sin nombre no es un movimiento |
| A4 | **Línea con Debe y Haber a la vez** | Advertencia/Bloqueante | Fue la misma fila de A3 | Avisar; si no es una fila de totales, bloquear |
| A5 | **Asiento descuadrado por redondeo** | Advertencia | Importes de origen con 4 a 6 decimales (VENTAS, impuestos al débito/crédito) hacen que, al pasar a 2 decimales, quede $0,01 de diferencia (3 asientos en BANCOS, 1 en VARIOS) | Si la suma **sin redondear** cuadra y la diferencia tras redondear es exactamente 0,01: **ajustar el centavo** en la línea cuyo residuo de redondeo sea el mayor en la dirección correcta, y mostrar cuál línea se ajustó (ej. `RICARDO +0,01`, `IMP A LOS DEB Y CRED COMP −0,01`) |
| A6 | **Asiento descuadrado en el origen** | Bloqueante | DEVENGAMIENTOS: 4 asientos con $0,01 de diferencia que ya viene en el Excel (IVA Asiento principal 05/2026 y 08/2026; IVA CF no computable 05/2026 y 06/2026), con importes de solo 2 decimales | **No ajustar en silencio.** Mostrar diferencia y el lado que falta. Ofrecer: (a) corregir en el Excel y recargar, (b) ajustar el centavo en una línea elegida por el usuario (sugerida: `A SALDO IVA A PAGAR` en el asiento principal) con registro de la decisión, (c) generar igual aceptando la alerta. Diferencia mayor a 0,01 → siempre Bloqueante |
| A7 | **Asiento informativo excluido** | Informativa | 38 asientos "INFORMATIVO — NO SE REGISTRA" en BANCOS (algunos solo tienen Debe, por eso no cuadran: es esperable) | Listar como excluidos; **no** validar su balance |
| A8 | **Asiento vacío / en cero omitido** | Informativa | 9 asientos (ej. `Por la emisión de echeqs a CAM`, `Por endoso de echeq a MUNDO` en septiembre) y `Ret. de 3eros. 1era quincena SUSS` de 05-2026 | Listar como omitidos. No consumen número correlativo |
| A9 | **Mes fuera de rango** | Informativa | Septiembre 2026 en un archivo "05 a 08" | Mostrar y dejar elegir |
| A10 | **Bloque "no se registra" / de ejercicio anterior** | Informativa | `ASIENTO QUE NO SE REGISTRA` (CHEQUES ENDOSADOS) | Excluir y listar |
| A11 | **Fórmula sin valor calculado / referencia externa** | Bloqueante | Celdas tipo `='[1]05-2026'!F25` (vínculo a otro libro) y fórmulas del tipo `=+H14`, que dependen de que el Excel haya guardado el valor | Si una celda de importe tiene fórmula y no hay valor guardado, o devuelve error (`#REF!`, `#VALUE!`), bloquear indicando la celda. Si es solo una nota de texto con vínculo externo, ignorarla |
| A12 | **Importes con más de 2 decimales en origen** | Informativa | VENTAS con hasta 6 decimales | Informar cantidad de líneas afectadas y que se redondeó half-up |
| A13 | **Importe negativo** | Advertencia | No apareció en la muestra | Avisar; no invertir lado automáticamente |
| A14 | **Cuenta inexistente en el plan de cuentas** | Bloqueante | No se validó en el proceso manual; era un riesgo | Si la app tiene plan de cuentas, validar cada código contra él y bloquear los desconocidos. Si el código existe pero el nombre del Excel difiere mucho del nombre del plan, advertir |
| A15 | **Código de cuenta repetido con nombres distintos** | Informativa | Normal en la muestra (ej. `2.01.04.005` para varios IVA CF, `4.01.04.009` para aranceles y promociones) | No alertar como error; mostrar solo en detalle |
| A16 | **Archivo ya generado antes** | Advertencia | — | Detectar que el mismo origen (hash del Excel + parámetros) ya se generó y avisar antes de duplicar |

Validaciones globales previas a generar: total Debe = total Haber del archivo; cantidad de líneas con 11 columnas; fechas válidas (último día del mes real, incluyendo febrero bisiesto); numeración correlativa sin huecos ni repeticiones entre asientos.

---

## 7. Criterios de aceptación (casos reales como tests)

Con los tres Excel de `samples/`, la herramienta debe reproducir **exactamente**:

**BANCOS (`BANCOS FCE 2027.xlsx`, versión con las cuentas ya corregidas)**
- 140 asientos registrados, 38 informativos excluidos, 965 líneas, numeración `0001`–`0140`.
- Total Debe = total Haber = **44.074.058.175,97**.
- Fechas: 31/05, 30/06, 31/07 y 31/08/2026.
- Conceptos del tipo `05-2026-BBVA`.
- 3 ajustes de centavo (alerta A5): MACRO 663 07/2026 asiento 1 (`RICARDO` +0,01), MACRO 663 08/2026 asiento 1 (`RICARDO` +0,01), BNA CA 07/2026 asiento 1 (`IMP A LOS DEB Y CRED COMP` −0,01).
- 1 fila de totales suelta ignorada (alerta A3): SANTANDER 05/2026 asiento 1.
- Con la **versión anterior del Excel** (sin los códigos de COMAFI BURSATIL, MACRO SECURITIES, SANTANDER VALORES, PRESTAMO NBSF, MULTAS): **15 alertas A1**.
- Códigos resueltos tras la corrección: COMAFI BURSATIL `1.01.02.005`, SANTANDER VALORES `1.01.02.006`, MACRO SECURITIES `1.01.02.008`, PRESTAMO NBSF y A PRÉSTAMO NBSF `2.01.02.004`, MULTAS `4.01.05.005`.

**DEVENGAMIENTOS (`ASIENTO DEVENGAMIENTOS.xlsx`)**
- 44 asientos, 156 líneas, numeración `0001`–`0044`; 1 asiento omitido por vacío (A8).
- Concepto = texto del encabezado (ej. `IVA - Asiento principal`).
- **4 alertas A6** (descuadre de $0,01 en origen). Sin resolución: Debe 6.936.847.173,80 vs Haber 6.936.847.173,78.

**VARIOS (`ASIENTOS VARIOS 05-2026 a 08-2026.xlsx`)**
- Rango 05–08/2026: 93 asientos, 581 líneas, Debe = Haber = **71.914.422.835,05**.
- Rango 05–09/2026: **98 asientos, 600 líneas, Debe = Haber = 74.568.487.094,93**; fecha 30/09/2026 para septiembre.
- Por hoja (05–09): VENTAS 12, COMPRAS 17, CAJAS 19, CHEQUES EMITIDOS 11, CHEQUES ENDOSADOS 19, SUELDOS 20.
- 9 asientos en cero omitidos (A8), 1 bloque de ejercicio anterior excluido (A10).
- Alerta A2: `4,01,05,050` → `4.01.05.050` (3 líneas, cuenta ASSAL, COMPRAS mayo, asiento Caja).
- Alerta A5: VENTAS 07/2026 asiento 1, ajuste +0,01 en `DEUDORES POR VENTAS`.
- Conceptos sin el prefijo `ASIENTO n — ` (ej. `VENTAS, COMISIONES E IMPUESTOS`).

Además de los tests con los archivos reales, tests unitarios para: parseo de ambos formatos de mes, detección de encabezado vs. nota, normalización de códigos, redondeo half-up con `decimal`, regla del centavo (A5 vs A6), último día del mes (incl. años bisiestos), numeración sin huecos y formato exacto de línea (11 columnas, CRLF, cp1252).

---

## 8. Seguridad, auditoría y robustez

- Validar el archivo subido: extensión `.xlsx`, tamaño máximo, sin ejecutar macros (`.xlsm` rechazado o con macros ignoradas).
- Trazabilidad: guardar hash del Excel, parámetros, usuario, fecha/hora, alertas aceptadas y hash del TXT generado.
- Los mapeos nombre→cuenta y las configuraciones quedan por empresa/ejercicio.
- Logs con `ILogger` (o el equivalente del stack), sin volcar importes completos en logs de nivel bajo.
- Mensajes de error con **dónde, causa y arreglo** (hoja, fila, qué hacer).

---

## 9. Entregables

1. `docs/asientos-estudio/maquetas/` y `docs/asientos-estudio/flujograma.md` (**primero, con mi OK antes de codear**).
2. Módulo implementado en el repo siguiendo sus convenciones, con migraciones si hay base de datos.
3. Tests automáticos (sección 7) pasando.
4. `docs/asientos-estudio/README.md`: cómo agregar un nuevo tipo de planilla, catálogo de alertas, configuración.
5. Commits chicos por etapa, con mensaje claro.

Al terminar cada etapa: decime qué funciona ya y cuál es el siguiente paso.
