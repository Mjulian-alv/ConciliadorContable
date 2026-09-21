# Conciliación de Percepciones y Retenciones (PyR) — guía de diseño

TAREA 00041 · Módulo `ArcaCliente` · Líneas 2, 3 y 4 (la Línea 1 fue el duplicado inicial).

> Esta guía cumple el rol del spec de brainstorming para esta tarea (el `CLAUDE.md` del repo
> los ubica en `docs/superpowers/specs/`); se deja acá, junto a las maquetas, para no tener
> dos documentos del mismo diseño que se desincronicen.

## Qué es

Una copia de la **Conciliación Offline de ARCA** adaptada a percepciones y retenciones: de un
lado los certificados que informan los agentes en ARCA (y en rentas provincial para IIBB), del
otro el mayor contable de PRESEA de cada cuenta de percepciones/retenciones.

Esta etapa cubre **perfil, lectura de la carpeta de ARCA y lectura de los mayores de PRESEA**.
Las directivas y la conciliación propiamente dicha quedan para la etapa siguiente: la pantalla
se construye con el botón **CONCILIAR** visible pero deshabilitado.

## Pedido

> 1. *(ya registrada)* Duplicar los modelos, servicios y formularios de ARCA OffLine.
> 2. Tenemos que colocar en el perfil qué cuenta significa percepción y cuál retención.
> 3. Hacer como con ARCA una carpeta donde vamos a tomar todo el contenido como base de datos.
> 4. Levantar los archivos que van a contener las percepciones y las retenciones del sistema.
>    A diferencia de ARCA, acá viene el tipo de comprobante y el documento en la misma casilla
>    de concepto, una columna de debe y otra de haber.

## Archivos de ejemplo (julio 2026)

Ruta: `D:\DESARROLLOS CONTABLE\CONCILIADOR\PERCEP Y RETEN\`

| Archivo | Formato real | Contenido |
|---|---|---|
| `presea\PERC. IVA 07-2026.XLSX` | xlsx | Mayor de **una** cuenta: `fecha, asiento, incidencia, origen, concepto, centro, debe, haber, saldo`. 2056 filas. |
| `presea\PERC. IIBB 07-2026.XLSX` | xlsx | Ídem, 984 filas. |
| `arca\Percepciones IVA 07-2026.xls` | **xls binario (BIFF)** | Hoja "Retenciones Impositivas", formato "Mis Retenciones" de AFIP. |
| `arca\Retenciones y Percepciones II.BB 07-2026.xls` | **HTML** con extensión `.xls` | Tabla `Cuit, Nombre, Operación, Fecha, Comprobante, Importe` (consulta provincial). |

## Modelo de datos

### Perfil — `PerfilOfflinePyR` (Línea 2)

Se mantiene lo que ya tiene la copia (tipo de archivo, hoja, cabecera, formato de fecha,
separador decimal, directivas) y se agrega:

| Campo | Tipo | Notas |
|---|---|---|
| `ColAsiento` / `PosAsiento` | string / int | Para mostrar el asiento de origen en la grilla. Default `asiento` / 2. |
| `CarpetaArca` | string | Renombre de `CarpetaCsvArca`: ya no son sólo CSV. |
| `Cuentas` | `List<CuentaPyR>` | Ver abajo. |

Defaults de columnas pensados para el mayor de PRESEA: `fecha`, `asiento`, `concepto`, `debe`,
`haber`.

`CuentaPyR`:

| Campo | Tipo | Valores |
|---|---|---|
| `Id` | Guid | |
| `Codigo` | string | Código de la cuenta contable en PRESEA. Obligatorio, único dentro del perfil. |
| `Nombre` | string | Obligatorio. Ej. "Percepciones IVA". |
| `Tipo` | enum `TipoOperacionPyR` | `Percepcion`, `Retencion` |
| `Impuesto` | enum `ImpuestoPyR` | `Iva`, `Iibb`, `Ganancias` |

Persistencia: tabla nueva `arca.ArcaPerfilesPyR` en `ArcaSqlSchema.cs`, con el mismo patrón
que `ArcaPerfilesOffline` (mismas columnas de lectura de archivo, sin `ColPuntoVenta`,
`ColNumero`, `ColTipoComprobante`, `ColCuit`, `ColNombreProveedor`, `ColTotal`,
`SistemaExportacion`, `ConfigPreseaJson`), más `ColAsiento`, `PosAsiento`, `CarpetaArca`,
`CuentasJson NVARCHAR(MAX) NOT NULL DEFAULT '[]'` y `DirectivasJson`. Acceso en
`ArcaSqlStorage` (`LoadPerfilesPyR` / `SavePerfilesPyR`) detrás de `PerfilPyRStorage`.

Se **elimina** `Models/Conciliacion PyR/PerfilFiscal.cs` (`PerfilFiscalPyR`): es el perfil
online (usuario, clave fiscal, API) y esta conciliación trabaja sólo con archivos.

### Fila de ARCA — `RegistroArcaPyR` (Línea 3)

| Campo | Origen AFIP (xls/xlsx) | Origen provincial (HTML) |
|---|---|---|
| `Cuit` | CUIT Agente Ret./Perc. | Cuit |
| `Denominacion` | Denominación o Razón Social | Nombre |
| `Impuesto` | Código de Impuesto: 767 → IVA, 217 → Ganancias, otro → no reconocido | Siempre IIBB |
| `Operacion` | Descripción Operación (PERCEPCION / RETENCION) | Operación (Percepción / Retención) |
| `Fecha` | Fecha Ret./Perc. | Fecha |
| `TipoComprobante` | Descripción Comprobante (FACTURA, NOTA DE CREDITO, …) | Texto antes del número en Comprobante ("Factura", "Nota Crédito", "Liquidación de pago") |
| `Letra` | — (no la informa) | Letra pegada al número, si hay ("Factura **A**000200118618") |
| `Numero` | Número Comprobante **sin ceros a la izquierda** | Dígitos de Comprobante sin ceros a la izquierda |
| `NumeroCertificado` | Número Certificado | — |
| `Importe` | Importe Ret./Perc. | Importe |
| `ArchivoOrigen` | nombre del archivo | nombre del archivo |

Regla del número: se normaliza igual de los dos lados (sólo dígitos, sin ceros a la izquierda),
así `Factura A000200118618` y `001000215963` quedan `200118618` y `1000215963`, que es como
los escribe PRESEA.

### Fila de PRESEA — `RegistroPreseaPyR` (Línea 4)

| Campo | Origen |
|---|---|
| `Cuenta` | La `CuentaPyR` elegida al agregar el archivo (de ahí salen Tipo e Impuesto). |
| `Fecha`, `Asiento` | Columnas del perfil. |
| `ConceptoOriginal` | Columna concepto, tal cual. |
| `TipoComprobante` | Parte del concepto, ej. "FACTURA A", "NOTA DE CREDITO A". |
| `Numero` | Parte del concepto, sin ceros a la izquierda. |
| `Proveedor` | Parte del concepto después de " de " (viene cortado a 16 caracteres por PRESEA). |
| `EsAnulacion` | El concepto empieza con "POR ANULACION". |
| `SinComprobante` | El concepto no respeta el patrón (ver reglas). |
| `Importe` | debe − haber (con signo). |
| `ArchivoOrigen` | nombre del archivo. |

## Reglas

### Lectura de la carpeta de ARCA

1. Se leen todos los archivos `*.xls`, `*.xlsx`, `*.htm`, `*.html` de la carpeta (sin
   subcarpetas). Los temporales de Office (`~$*`) se ignoran.
2. **El formato se detecta por contenido, no por extensión**:
   - Empieza (salteando blancos/BOM) con `<` → HTML: se toma la primera `<table>` con
     encabezados `Cuit … Importe`. Parser propio de `<tr>/<th>/<td>` +
     `WebUtility.HtmlDecode`, sin dependencias nuevas.
   - Firma OLE (`D0 CF 11 E0`) o ZIP (`PK`) → Excel, leído con **ExcelDataReader** (se agrega
     al `.csproj` de ArcaCliente la misma versión que usa LiquidacionesAuditar, 3.8.0).
     Se busca la hoja cuyo encabezado contenga `CUIT Agente Ret./Perc.`.
   - Cualquier otra cosa → archivo no reconocido.
3. Un archivo no reconocido, o una fila con impuesto no reconocido, **no corta la carga**:
   se cuenta y se informa al final ("2 archivos cargados, 1 no reconocido: X.xls").
4. Si la carpeta no existe o no tiene ningún archivo reconocible → error, no se carga nada.
5. Al cargar bien, `CarpetaArca` se guarda en el perfil (igual que hoy `CarpetaCsvArca`).
6. Encabezados: se comparan normalizados (sin tildes, minúsculas, espacios colapsados),
   porque el xls de AFIP viene en Latin-1 ("Denominaci�n").

### Lectura de los mayores de PRESEA

1. El perfil tiene que tener al menos una cuenta; si no, **Agregar** está deshabilitado con
   el aviso "El perfil no tiene cuentas configuradas".
2. **Agregar** pide el archivo y, en un diálogo, **a qué cuenta del perfil corresponde**.
   Una cuenta tiene a lo sumo un archivo cargado: si ya había uno, se pregunta si se reemplaza.
3. Faltan columnas del perfil en el encabezado → error con la lista de las que faltan; el
   archivo no se agrega.
4. Patrón del concepto (sin distinguir mayúsculas, espacios múltiples colapsados):

   ```
   ^(SEGUN|POR ANULACION)\s+(?<tipo>.+?)\s+(?<numero>\d+)\s+de\s+(?<proveedor>.*)$
   ```

   Ejemplos reales:
   - `SEGUN FACTURA A    36900627623 de CIA INDUSTRIAL C` → FACTURA A · 36900627623 · CIA INDUSTRIAL C
   - `SEGUN NOTA DE CREDITO A ...` → NOTA DE CREDITO A
   - `POR ANULACION FACTURA A ...` → FACTURA A, `EsAnulacion = true`
5. Lo que no respeta el patrón (en julio: `Según MINUTA FINANCIERA Nº …`, `M.FINANCIERA`) se
   carga igual con `SinComprobante = true` y se **resalta** en la grilla. No se descarta: las
   directivas de la etapa siguiente deciden qué hacer con esas filas.
6. Filas con fecha vacía o debe y haber vacíos se ignoran (totales, renglones en blanco).
7. **Quitar** saca el archivo seleccionado de la lista.

### Generales

- Todo lo leído queda **en memoria** mientras la pantalla está abierta, como en la
  Conciliación Offline. No se persisten filas.
- CONCILIAR queda deshabilitado en esta etapa.

## Pantallas

| Pantalla | Base | Cambio |
|---|---|---|
| Perfiles PyR | copia de `FormPerfilesOffline` | Sólo título y tipos. Sin maqueta (no cambia). |
| Detalle de perfil PyR | copia de `FormPerfilOfflineDetalle` | Columnas Fecha/Asiento/Concepto/Debe/Haber en vez de las de comprobante; **grilla de cuentas** con Agregar/Editar/Quitar; se quita la sección de exportación a sistema. |
| Cuenta PyR (diálogo) | nuevo | Código, Nombre, Tipo, Impuesto. |
| Elegir cuenta (diálogo) | nuevo | Combo con las cuentas del perfil ("1.1.4.05 — Percepciones IVA (Percepción · IVA)"), al agregar un mayor. |
| Conciliación PyR | copia de `FormComprobantesOffline` | Grupo ARCA: carpeta + Cargar + resumen por impuesto/operación. Grupo PRESEA: lista de archivos (Archivo · Cuenta · Filas · Sin comprobante) con Agregar/Quitar. Dos grillas de resultado (ARCA / PRESEA). CONCILIAR deshabilitado. |

Entrada desde el menú: nueva opción "Conciliación Percepciones y Retenciones" junto a la de
Conciliación Offline en `FormMenu`.

## Flujograma

<!-- Lo produce Codex. -->

## Maquetas

- `01-perfil-pyr-detalle.png` — perfil con 3 cuentas cargadas (Perc. IVA, Perc. IIBB, Ret. IIBB).
- `01-perfil-pyr-detalle-vacio.png` — perfil nuevo, grilla de cuentas vacía.
- `01-perfil-pyr-detalle-error.png` — intento de guardar con código de cuenta repetido.
- `02-cuenta-pyr.png` — diálogo de alta/edición de cuenta.
- `03-elegir-cuenta.png` — diálogo de elegir cuenta al agregar un mayor de PRESEA.
- `04-conciliacion-pyr.png` — carpeta ARCA cargada (2 archivos) y 2 mayores de PRESEA cargados, con filas "sin comprobante" resaltadas.
- `04-conciliacion-pyr-vacio.png` — pantalla recién abierta, nada cargado.
- `04-conciliacion-pyr-error.png` — carpeta con un archivo no reconocido y un mayor rechazado por columnas faltantes.

## Orden de implementación

1. Limpieza de la copia: borrar `PerfilFiscalPyR`; mover `Models/Conciliacion PyR/` a un
   nombre sin espacio (`Models/PyR/`) y ajustar el `.csproj` (la entrada `<Folder>` actual
   apunta a una carpeta que no existe).
2. Modelos: `CuentaPyR`, enums, `PerfilOfflinePyR` ajustado, `RegistroArcaPyR`,
   `RegistroPreseaPyR`.
3. Esquema y storage: `arca.ArcaPerfilesPyR` + `LoadPerfilesPyR` / `SavePerfilesPyR`.
4. Pantallas de perfil: lista, detalle con grilla de cuentas, diálogo de cuenta. (Línea 2)
5. `ArcaPyRImporter` (detección de formato, lector AFIP, lector HTML, normalización). (Línea 3)
6. `PreseaPyRImporter` + parser de concepto. (Línea 4)
7. `FormConciliacionPyR` + diálogo de elegir cuenta + entrada en el menú. (Líneas 3 y 4)
8. Verificación con los cuatro archivos de ejemplo: cantidades leídas por archivo, filas sin
   comprobante (en julio: 17 en Perc. IVA) y números normalizados que coincidan entre ambos
   lados en una muestra.
