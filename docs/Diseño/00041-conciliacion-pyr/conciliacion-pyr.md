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
> 5. *(21/09/2026, después de probarlo)* Que deje tomar dos archivos de PRESEA, uno por cada
>    cuenta configurada.

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

*(Ajustado al implementar)* De la copia quedan hoja, cabecera, formato de fecha, separador
decimal y directivas. Se **quitaron** tipo de archivo, separador, encoding y las posiciones
`Pos*`: PRESEA exporta el mayor siempre en Excel, así que el tipo de archivo se muestra fijo
("Excel (.xlsx / .xls)", deshabilitado), y sin cabecera las mismas casillas `Col*` se cargan
con el número de columna (1, 2, 3…) en vez de duplicar los campos.

| Campo | Tipo | Notas |
|---|---|---|
| `ColFecha`, `ColAsiento`, `ColConcepto`, `ColDebe`, `ColHaber` | string | Nombre del encabezado, o número de columna si no hay cabecera. |
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
`SistemaExportacion`, `ConfigPreseaJson`, `TipoArchivo`, `Separador`, `Encoding` ni `Pos*`),
más `ColAsiento`, `ColConcepto`, `ColDebe`, `ColHaber`, `CarpetaArca`,
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
   *(Decisión tomada al implementar)* Un archivo con **contenido idéntico** a otro ya leído se
   saltea y se informa ("es idéntico a X"): el portal provincial baja el reporte como
   `Listado - <fecha>.xls`, y bajarlo dos veces duplicaba en silencio todos los importes de IIBB
   (pasó con la carpeta de ejemplo el 18/09).
5. Al cargar bien, `CarpetaArca` se guarda en el perfil (igual que hoy `CarpetaCsvArca`).
6. Encabezados: se comparan normalizados (sin tildes, minúsculas, espacios colapsados),
   porque el xls de AFIP viene en Latin-1 ("Denominaci�n").
7. Encoding del HTML provincial: viene en **UTF-8 sin BOM y sin `charset` declarado**, así que
   se decodifica explícitamente como UTF-8 (y, si los bytes no son UTF-8 válido, como
   Windows-1252). Adivinarlo da mojibake: "RetenciÃ³n" en vez de "Retención", y la operación
   deja de reconocerse.

### Lectura de los mayores de PRESEA

1. El perfil tiene que tener al menos una cuenta; si no, **Agregar** está deshabilitado con
   el aviso "El perfil no tiene cuentas configuradas".
2. **Agregar** pide el archivo y, en un diálogo, **a qué cuenta del perfil corresponde**.
   Una cuenta tiene a lo sumo un archivo cargado: si ya había uno, se pregunta si se reemplaza.
   *(Línea 5)* El combo arranca en la **primera cuenta que todavía no tiene mayor**, y las que ya
   tienen muestran cuál: "… (ya cargada: PERC. IVA 07-2026.XLSX)". Antes arrancaba siempre en la
   primera cuenta y el segundo archivo, sin tocar el combo, pedía reemplazar al primero.
3. Faltan columnas del perfil en el encabezado → error con la lista de las que faltan; el
   archivo no se agrega.
4. Patrón del concepto (sin distinguir mayúsculas, espacios múltiples colapsados):

   ```
   ^(SEGUN|POR ANULACION)\s+(?<tipo>.+?)\s+(?<numero>\d+)(?:\s+de\s+(?<proveedor>.*))?$
   ```

   Ejemplos reales:
   - `SEGUN FACTURA A    36900627623 de CIA INDUSTRIAL C` → FACTURA A · 36900627623 · CIA INDUSTRIAL C
   - `SEGUN NOTA DE CREDITO A ...` → NOTA DE CREDITO A
   - `POR ANULACION FACTURA A    73200019077` → FACTURA A · 73200019077, `EsAnulacion = true`,
     sin proveedor: las anulaciones de PRESEA **no traen " de …"** (22 en julio), por eso esa
     parte del patrón es opcional.
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

Entrada desde el menú *(ajustado al implementar)*: `FormMenu` de ArcaCliente no se instancia en
ningún lado; la entrada real es el menú principal del shell (`FormMenuPrincipal`), panel ARCA:
botones **"Percepciones y Retenciones"** (elegir perfil → pantalla, igual que Offline) y
**"Perfiles PyR"**, los dos con el permiso nuevo `ArcaPyR` (criterio de TAREA 00021: un
permiso por módulo; los usuarios no admin no lo ven hasta que se les otorgue).

Decisiones tomadas al implementar, que ni la guía ni las maquetas cubrían:

- El combo **Perfil** del pie de la pantalla principal permite cambiar de perfil; como cambian
  las cuentas, lo cargado se descarta, con confirmación si había algo cargado.
- El código de cuenta repetido se valida **al guardar el perfil** (como la maqueta de error),
  no en el diálogo de cuenta; las filas en conflicto se pintan en rojo.
- Archivos repetidos de la carpeta de ARCA se informan como "repetido(s), se salteó", aparte
  de los no reconocidos. El motivo de cada uno va en el tooltip de la línea de estado.
- El resumen por impuesto/operación de ARCA es texto plano ("IVA · Percepción: 2.569"): el
  formato HTML de Telerik se comía el espacio antes del número en negrita.

## Flujograma

Exportado en `00-flujograma.png`.

```mermaid
flowchart TD
    A([Menú: Conciliación Percepciones y Retenciones]) --> B[Elegir perfil PyR]
    B --> P{¿El perfil tiene cuentas?}
    P -- No --> P1[Agregar PRESEA deshabilitado<br/>'El perfil no tiene cuentas configuradas']
    P1 --> PE[Editar perfil]
    P -- Sí --> C

    subgraph PERFIL [Detalle de perfil - Línea 2]
        PE --> PC[Agregar / Editar cuenta:<br/>Código, Nombre, Tipo, Impuesto]
        PC --> PV{¿Código y Nombre cargados<br/>y Código único?}
        PV -- No --> PX[Error en el diálogo / al guardar] --> PC
        PV -- Sí --> PG[Guardar perfil en arca.ArcaPerfilesPyR]
    end
    PG --> B

    C[Pantalla Conciliación PyR<br/>carpeta ARCA precargada del perfil] --> D

    subgraph ARCA [Carpeta ARCA - Línea 3]
        D[Cargar carpeta] --> D1{¿Existe la carpeta?}
        D1 -- No --> DX[Error: carpeta inexistente<br/>no se carga nada]
        D1 -- Sí --> D2[Por cada archivo .xls .xlsx .htm .html<br/>salvo ~$ temporales]
        D2 --> D3{Formato por contenido}
        D3 -- Empieza con '<' --> D4[Lector HTML provincial<br/>Impuesto = IIBB]
        D3 -- Firma OLE / ZIP --> D5[Lector AFIP ExcelDataReader<br/>767 IVA · 217 Ganancias]
        D3 -- Otro --> D6[Archivo no reconocido]
        D4 --> D7[Normalizar número<br/>sólo dígitos, sin ceros a la izquierda]
        D5 --> D7
        D5 -- Código de impuesto desconocido --> D8[Fila no reconocida]
        D7 --> D9{¿Quedó al menos un archivo reconocido?}
        D6 --> D9
        D8 --> D9
        D9 -- No --> DX2[Error: ningún archivo reconocible]
        D9 -- Sí --> D10[Grilla ARCA + resumen por impuesto/operación<br/>+ aviso de no reconocidos<br/>Guardar CarpetaArca en el perfil]
    end

    D10 --> E
    DX --> E
    DX2 --> E

    subgraph PRESEA [Mayores PRESEA - Línea 4]
        E[Agregar mayor] --> E1[Elegir archivo xlsx]
        E1 --> E2[Diálogo: elegir cuenta del perfil]
        E2 --> E3{¿La cuenta ya tiene archivo?}
        E3 -- Sí --> E4{¿Reemplazar?}
        E4 -- No --> E
        E4 -- Sí --> E5
        E3 -- No --> E5{¿Están las columnas del perfil?}
        E5 -- No --> EX[Error: columnas faltantes<br/>el archivo no se agrega]
        E5 -- Sí --> E6[Por cada fila con fecha y debe/haber]
        E6 --> E7{¿Concepto respeta el patrón<br/>SEGUN / POR ANULACION ... de ...?}
        E7 -- Sí --> E8[Tipo · Número · Proveedor<br/>EsAnulacion si POR ANULACION]
        E7 -- No --> E9[Sin comprobante<br/>se carga resaltada]
        E8 --> E10[Importe = debe − haber]
        E9 --> E10
        E10 --> E11[Archivo en la lista:<br/>Archivo · Cuenta · Filas · Sin comprobante]
        E11 --> Q[Quitar archivo seleccionado]
        Q --> E11
    end

    E11 --> F[CONCILIAR deshabilitado<br/>directivas y conciliación: próxima etapa]
    F --> Z([Cerrar pantalla: los datos cargados se descartan])
```

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
