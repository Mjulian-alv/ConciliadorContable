# Exportación a PRESEA — layout del archivo TXT

Referencia del archivo de texto que genera `ArcaCliente` para importar comprobantes de
compra en **PRESEA** (Neuralsoft). Documenta el layout de 39 campos, de dónde sale cada
valor, las reglas de formateo y las particularidades del armado.

**Código de referencia:** [`ArcaCliente/Services/PreseaTxtExportador.cs`](../ArcaCliente/Services/PreseaTxtExportador.cs)

---

## 1. Panorama

El archivo se arma a partir de los comprobantes en estado **SOLO ARCA** de una
conciliación: comprobantes que ARCA informa como recibidos pero que todavía no están
cargados en el ERP. El operador los revisa uno a uno, completa los datos contables que
ARCA no provee, y el resultado se vuelca a un TXT que PRESEA importa.

### Flujo

```
FormComprobantesOffline              items en estado SOLO ARCA
  └─ FormExportarPreseaQr            selección por escaneo de QR o tilde manual;
     │                               marca en gris lo ya exportado
     └─ FormCompletarPresea          revisión 1x1: completar datos contables y
        │                            repartir "Otros Tributos" entre las percepciones
        └─ PreseaExportResolver      resuelve cada campo → List<PreseaLineaExport>
           └─ PreseaTxtExportador.Exportar(lineas, ruta, cfg, perfilId)
              ├─ File.WriteAllLines(ruta, filas, encoding)
              └─ PreseaExportMemoryStorage.RegistrarRange(...)   ← memoria anti-duplicado
```

`FormExportarPreseaQr` existe porque el operador tiene el comprobante físico en la mano:
escanea el QR con un lector USB *keyboard-wedge*, el parser offline (`ArcaQrParser`)
extrae CUIT/tipo/punto de venta/número, se matchea por `ComprobanteClave` y se tilda la
fila. Sirve como control de que el papel existe antes de contabilizarlo.

### El otro camino: exportación a Excel

Existe un segundo exportador, [`PreseaExportador`](../ArcaCliente/Services/PreseaExportador.cs)
(ClosedXML), que se elige desde `ExportadorSistemaFactory` con
`SistemaExportacionOffline.Presea`. Usa **el mismo layout de 39 columnas** y las mismas
funciones de `PreseaCalculos`, pero sin ventana de revisión. Sus diferencias están
listadas en la [sección 11](#11-inconsistencias-detectadas).

---

## 2. Formato del archivo

| Aspecto | Valor | Origen |
|---|---|---|
| Separador de campos | `\|` | `ConfigPresea.Separador` (default `"\|"`) |
| Encabezado | **no lleva** | — |
| Registros | una línea por comprobante, 39 campos | — |
| Comillas / escapes | **no se usan** | el saneado quita el separador y los saltos de línea (`San`) |
| Encoding | Latin-1 (ISO-8859-1) | `ConfigPresea.Encoding`; cualquier otro valor cae en UTF-8 **sin BOM** (`TextParsingUtils.ObtenerEncoding`) |
| Separador decimal | `.` | `ConfigPresea.SeparadorDecimal` — sólo `.` o `,` |
| Formato de fecha | `yyyyMMdd` | `ConfigPresea.FormatoFecha` (también `ddMMyyyy` o `dd/MM/yyyy` desde la UI) |
| Nombre sugerido | `PRESEA_yyyyMMdd_HHmm.txt` | diálogo de `FormCompletarPresea` |

Los tipos que aparecen en la columna *Tipo* de la tabla siguiente son los de la
especificación PRESEA: **I** entero, **B** decimal, **C** carácter, **D** fecha,
**N** numérico, **M** memo. El número posterior es el largo (`-12,2` = con signo,
12 posiciones, 2 decimales).

---

## 3. Los 39 campos

Los nombres son los de la especificación PRESEA, transcriptos en el array `headers` de
`PreseaExportador.cs:53-94`. El orden es el de `campos[0]..campos[38]` en
`PreseaTxtExportador.cs:47-85`.

Columna **Origen**:
`ARCA` = derivado del CSV de ARCA · `Cascada` = proveedor por CUIT → `ConfigPresea`
(ver [sección 8](#8-cascada-de-resolución)) · `Config` = sólo `ConfigPresea` ·
`Usuario` = editable por comprobante en la ventana de revisión · `Fijo` = hardcodeado.

| # | Campo | Tipo | Origen | Valor y formateo |
|---|---|---|---|---|
| 1 | Proveedor | I/8 | Cascada | `CodigoProveedor` → `Ent()` |
| 2 | Cuenta contable proveedor | B/16 | Cascada + Usuario | `CuentaProveedor` → `San(16)` |
| 3 | Comprobante | C/24 | ARCA | `TablaComprobanteDescripcionMapper.ObtenerDescripcion(TipoComprobante)` → `San(24)`. Código AFIP no reconocido ⇒ **vacío** |
| 4 | Moneda | I/3 | ARCA | `PreseaCalculos.MapMoneda(Moneda)`: PES/ARS/$→1, DOL/USD→2, EUR→3, BRL→4, UYU→5, resto→**1** |
| 5 | Cotización | B/-14,8 | ARCA | `TipoCambio`; si parsea 0 se fuerza `1` → `Num(4)` — **4 decimales, no 8** |
| 6 | Provincia | I/3 | Cascada | `Ent(Provincia)`; **si da `"0"` se escribe `"21"`** |
| 7 | Condición | I/5 | Cascada | `Ent(Condicion)` |
| 8 | Descuento | B/-12,2 | Proveedor | `ConfigPreseaProveedor.Descuento` (0 si el proveedor no está en el maestro) → `Num(2)`. **No se le aplica el signo de NC** |
| 9 | Sucursal y número | N/14 | ARCA | `BuildSucursalNumero`: `PuntoVenta.PadLeft(4,'0')` + `NumeroDesde.PadLeft(8,'0')`, concatenados sin guion |
| 10 | Fiscal | C/1 | Cascada | `San1(Fiscal)` — `S` fiscal / `N` no fiscal |
| 11 | Fecha | D/8 | ARCA | `FormatearFecha(FechaEmision, cfg.FormatoFecha)` |
| 12 | Fecha contable | D/8 | — | **igual al campo 11**, siempre |
| 13 | CAI / CAE | B/14 | ARCA | `Digitos(CodAutorizacion)` |
| 14 | Vencimiento CAI | D/8 | Usuario | `FormatearFecha(VencimientoCai, cfg.FormatoFecha)`; si no parsea → vacío |
| 15 | Observación | M | Usuario | `San(240)` |
| 16 | Cuenta IVA | B/16 | Config | `CuentaIVA` (viene de `cfg.CuentaIVA`, sin override por proveedor) → `San(16)`. **Obligatoria**: sin ella no se puede confirmar |
| 17 | Porcentaje de IVA | N/-6,2 | ARCA | `r1` del slot 1 → `Num(2)`. **Sin signo** |
| 18 | Neto | B/-12,2 | ARCA | `n1 * signo` → `Num(2)` |
| 19 | IVA | B/-12,2 | ARCA | `i1 * signo` → `Num(2)` |
| 20 | Cuenta IVA 2 | B/16 | Config | `CuentaIVA2` (viene de `cfg.CuentaIVA2`) → `San(16)` |
| 21 | Porcentaje de IVA 2 | N/-6,2 | ARCA | `r2` del slot 2 → `Num(2)`. **Sin signo** |
| 22 | Neto 2 | B/-12,2 | ARCA | `n2 * signo` → `Num(2)` |
| 23 | IVA 2 | B/-12,2 | ARCA | `i2 * signo` → `Num(2)` |
| 24 | Importe | B/-12,2 | ARCA | `ImpTotal * signo` → `Num(2)` |
| 25 | Sobretasa | B/-12,2 | Usuario | Σ percepciones con `CampoDestino == "Sobretasa"`, × signo |
| 26 | Percepción IB | B/-12,2 | Usuario | Σ `"IB"` × signo — **destino por defecto de Otros Tributos** |
| 27 | Percepción IM | B/-12,2 | Usuario | Σ `"IM"` × signo |
| 28 | Percepción IV | B/-12,2 | Usuario | Σ `"IV"` × signo |
| 29 | Percepción IN | B/-12,2 | Usuario | Σ `"IN"` × signo |
| 30 | Código Percepción configurable 1 | I/3 | Config | `cfg.CodigoPercepcionConfig1.Trim()` — texto crudo, **no** pasa por `Ent()` |
| 31 | Percepción Configurable 1 importe | B/-12,2 | Usuario | Σ `"Config1"` × signo |
| 32 | Código Percepción configurable 2 | I/3 | Config | `cfg.CodigoPercepcionConfig2.Trim()` |
| 33 | Percepción Configurable 2 importe | B/-12,2 | Usuario | Σ `"Config2"` × signo |
| 34 | Impuestos internos | B/-12,2 | Usuario | Σ `"ImpuestosInternos"` × signo |
| 35 | Cuenta contable del debe | B/16 | Cascada + Usuario | `CuentaDebe` → `San(16)`. **Obligatoria** |
| 36 | Centro | C/10 | Cascada + Usuario | `Centro` → `San(10)` |
| 37 | Lista de precios | C/10 | Fijo | siempre `BASE` |
| 38 | Versión | I/4 | Fijo | siempre `1` |
| 39 | Imputa | C/1 | Config | `San1(Imputa)`, que viene de `cfg.Imputa` — `N` no imputa / `D` imputa después / `P` imputa contra recepciones |

> **Sobre el largo del campo 9.** `BuildSucursalNumero` sólo rellena con `PadLeft`, nunca
> trunca. El CSV de ARCA suele traer el punto de venta con 5 dígitos (`00001`) y el número
> con 8 (`00000123`), con lo que el campo sale de **13 caracteres**, no de 12 ni de 14.

---

## 4. Reglas de formateo

Los cinco helpers privados de `PreseaTxtExportador` son los que explican el aspecto final
de cada campo:

| Helper | Qué hace |
|---|---|
| `Ent(s)` | Deja sólo dígitos y quita los ceros a la izquierda. Si queda vacío devuelve `"0"` |
| `Num(v, dec, sepDec)` | `v.ToString("F{dec}")` en cultura invariante; después cambia `.` por `,` si el separador decimal configurado es coma. **Siempre emite los decimales**, incluso en ceros (`0.00`) |
| `San(s, maxLen, sep)` | Reemplaza el separador de campos, CR y LF por espacio, hace `Trim()` y trunca a `maxLen` |
| `San1(s)` | Primer carácter en mayúscula; vacío si el texto está en blanco |
| `Digitos(s)` | Filtra por `char.IsDigit` |

`San` es lo que garantiza que ningún dato del comprobante rompa el archivo: al reemplazar
el separador por espacio, una razón social o una observación con `|` adentro no corre las
posiciones. **No hay entrecomillado**, así que un separador dentro de un valor se pierde
como carácter, no se escapa.

---

## 5. Signo y Notas de Crédito

`PreseaExportResolver` marca la línea como NC cuando:

```csharp
TipoComprobanteMapper.Parse(tipo.PadLeft(3, '0')).TipoOctosis == "3"
```

En el exportador eso se traduce en `signo = -1`, que **se aplica** a los campos:

- 18, 19, 22, 23 — netos e IVA de ambos slots
- 24 — Importe
- 25 a 29 — las cinco percepciones fijas
- 31 y 33 — las dos percepciones configurables
- 34 — Impuestos internos

Y **no se aplica** a:

- 8 — Descuento
- 17 y 21 — los porcentajes de IVA (una alícuota del 21% sigue siendo 21, no −21)

---

## 6. Los dos slots de IVA

PRESEA acepta sólo **dos** aperturas de IVA por comprobante (campos 16-19 y 20-23),
mientras que ARCA informa hasta cinco alícuotas. `PreseaCalculos.ResolverSlotIva` resuelve
el encaje así:

1. Arma un slot por cada alícuota (2.5 / 5 / 10.5 / 21 / 27 %) tomando neto e IVA del CSV,
   y **descarta** los que tienen ambos valores en cero.
2. Ordena los slots por importe de IVA, descendente.
3. **Slot 1** = la alícuota de mayor IVA (tasa, neto e IVA propios).
4. **Slot 2** = la **tasa del segundo** slot, pero con neto e IVA **acumulados de todos
   los restantes**. Si el comprobante tiene tres o más alícuotas, el excedente se apila
   acá y la tasa del campo 21 deja de representar al importe del campo 22.
5. **Fallback Factura C**: si el slot 1 quedó con neto 0 y el `ImpTotal` es positivo, se
   asigna `n1 = ImpTotal` dejando `r1 = 0`. Es el caso de los comprobantes clase C, que no
   discriminan IVA.

### Lo que no se exporta

`ImpOpExentas` (operaciones exentas) e `ImpNetoNoGravado` (neto no gravado) **no tienen
campo en el layout PRESEA y no se exportan**. Son neto sin IVA y no afectan el total del
comprobante. La ventana de revisión los muestra igual, marcados como
*"No exportado (sin campo en PRESEA)"*, vía `PreseaCalculos.DetalleIva` — está pensado
para que el operador pueda cuadrar contra el comprobante físico y entienda por qué la
suma de netos exportados no da el total.

---

## 7. Percepciones y "Otros Tributos"

ARCA informa un **único** importe agregado, `OtrosTributos`. PRESEA tiene **ocho** destinos
posibles, declarados en `PreseaCalculos.CamposPercepcion`:

| Código | Descripción | Campo del TXT |
|---|---|---|
| `Sobretasa` | Sobretasa | 25 |
| `IB` | Percepción IIBB | 26 |
| `IM` | Percepción Municipal | 27 |
| `IV` | Percepción IVA | 28 |
| `IN` | Percepción Ingresos Nacional | 29 |
| `Config1` | Percepción configurable 1 | 31 (código en 30) |
| `Config2` | Percepción configurable 2 | 33 (código en 32) |
| `ImpuestosInternos` | Impuestos internos | 34 |

`PreseaExportResolver` precarga **el 100% en `IB`** (Percepción IIBB) como una sola línea
`PreseaPercepcionLinea` con concepto `"Otros Tributos (ARCA)"`. En la grilla de
`FormCompletarPresea` el operador puede partir ese importe en varias líneas y asignar cada
una a su campo. Al exportar, `SumaPorCampo` agrupa por `CampoDestino`.

Una línea sin `CampoDestino` elegido **bloquea la confirmación** del comprobante.

---

## 8. Cascada de resolución

Los campos que ARCA no provee se resuelven en `PreseaExportResolver.Resolver` con la
cascada **proveedor → configuración general**, y algunos quedan además editables por
comprobante:

| Campo del TXT | Proveedor (por CUIT) | Config general | Editable por comprobante |
|---|---|---|---|
| 1 Proveedor | sí (`CodigoProveedor`) | sí | sí |
| 2 Cuenta proveedor | sí (`CuentaContableProveedor`) | sí | sí |
| 35 Cuenta del debe | sí (`CuentaDebe`) | sí | sí |
| 36 Centro | sí (`Centro`) | sí | sí |
| 6 Provincia | sí | sí | no |
| 7 Condición | sí | sí | no |
| 10 Fiscal | sí | sí | no |
| 8 Descuento | sí (0 si no está) | no | no |
| 16 Cuenta IVA | no | sí | no |
| 20 Cuenta IVA 2 | no | sí | no |
| 39 Imputa | no | sí | no |
| 14 Vencimiento CAI | no | no | sí |
| 15 Observación | no | no | sí |

**Dónde vive cada nivel:**

- **Proveedor**: `ConfigPreseaProveedor`, tabla `arca.PreseaProveedores`, clave CUIT.
  Se carga con *Importar proveedores (CSV)* desde la pantalla de configuración.
- **Config general**: `ConfigPresea`, serializada como JSON en
  `arca.ArcaPerfilesOffline.ConfigPreseaJson` — es decir, **por perfil offline**.

El helper es `Coalesce(preferido, fallback)`: toma el valor del proveedor si no está en
blanco, si no el de la config, siempre con `Trim()`.

Si el proveedor no está en el maestro (o está sin código), la línea arranca con estado
`"Revisar proveedor"` en lugar de `"Proveedor OK"`.

---

## 9. Memoria anti-duplicado

La identidad de un comprobante es `ComprobanteClave.Generar(cuit, tipo, ptoVta, nro)`, que
produce `"{cuit}|{tipo}|{ptoVta}|{nro}"` con todos los componentes reducidos a dígitos y
sin ceros a la izquierda. Esa normalización es lo que permite que el `"0001"` del CSV y el
`1` del QR den la misma clave.

Al terminar `Exportar`, y **después** de escribir el archivo, se registra el lote en
`arca.PreseaComprobantesExportados` (`PreseaExportMemoryStorage.RegistrarRange`). Cada
registro guarda clave, CUIT, tipo, punto de venta, número, CAE, importe con signo, fecha
del comprobante, fecha de exportación, ruta del archivo generado y el perfil offline.

El `INSERT` es idempotente (`IF NOT EXISTS`). `FormExportarPreseaQr` levanta el conjunto
completo con `ClavesExportadas()` para marcar en la grilla lo ya exportado.

> Como la tabla es compartida en SQL Server, un comprobante exportado desde un puesto se
> detecta como duplicado desde cualquier otro.

---

## 10. Validaciones antes de exportar

`FormCompletarPresea` no deja confirmar un comprobante si falta:

- el código de proveedor (campo 1),
- la cuenta contable del proveedor (campo 2),
- la cuenta contable del debe (campo 35),
- `cfg.CuentaIVA` en la configuración general (campo 16),
- el campo destino de **alguna** línea de percepción.

Los comprobantes omitidos y los no confirmados no llegan al archivo. El botón
*Generar TXT* se habilita sólo si hay al menos un confirmado.

---

## 11. Inconsistencias detectadas

Relevadas al documentar. **Ninguna está corregida** — quedan asentadas acá para decidir.

1. **`ConfigPresea.UsaListaPrecios` nunca se lee.** Su doc-comment dice que si está en
   `false` los campos 37 y 38 se exportan vacíos, pero `PreseaTxtExportador.cs:83-84`
   escribe siempre `"BASE"` y `"1"`. Ídem `PreseaExportador.cs:157-158`.
2. **`ConfigPresea.FechaContableIgualEmision` nunca se lee.**
   `PreseaTxtExportador.cs:38` fija `fechaCont = fecha` con un comentario "por ahora".
3. **`FormConfigPresea` pisa 7 campos al guardar.** Su constructor
   (`FormConfigPresea.cs:20-33`) construye una **copia parcial** de `ConfigPresea` con
   sólo 11 propiedades, y `FormPerfilOfflineDetalle.cs:353` asigna esa copia al perfil. En
   consecuencia `Separador`, `Encoding`, `SeparadorDecimal`, `CodigoPercepcionConfig1`,
   `CodigoPercepcionConfig2`, `UsaListaPrecios` y `FechaContableIgualEmision` vuelven a su
   valor por defecto cada vez que se abre y se guarda la configuración de PRESEA.
4. **No hay UI para los códigos de percepción configurable.** Ninguna pantalla setea
   `CodigoPercepcionConfig1/2`, y el punto anterior los resetearía igual. Los campos 30 y
   32 se exportan **siempre vacíos**, con lo que 31 y 33 quedan sin código asociado.
5. **Fallback mágico `Provincia = "21"`** (`PreseaTxtExportador.cs:52`) cuando el valor
   resuelto es 0, sin comentario que lo justifique. El camino Excel no lo aplica: exporta
   el 0.
6. **Cotización con 4 decimales** (`Num(ctz, 4, ...)`, línea 51) aunque el tipo declarado
   es `B/-14,8`.
7. **Largo del campo 9 variable.** `BuildSucursalNumero` sólo hace `PadLeft` y nunca
   trunca; con el punto de venta de 5 dígitos que informa ARCA el campo sale de 13
   caracteres, contra los 14 declarados.
8. **Divergencias del camino Excel** (`PreseaExportador`) respecto del TXT: no aplica
   signo negativo a las notas de crédito, no usa la cascada por proveedor salvo para el
   descuento, exporta los códigos de percepción configurable como `0`, y **no registra
   nada en la memoria anti-duplicado**.
9. **`MapMoneda` es provisorio.** Tabla hardcodeada de cinco monedas; todo lo no
   reconocido cae en `1` (peso), incluido un comprobante en una moneda ajena a la lista.
   El propio comentario del código lo marca como pendiente de reemplazo por la tabla
   maestra de monedas.

---

## 12. Ejemplo de línea

Factura A con una sola alícuota (21%) y percepción de IIBB, con la configuración por
defecto (separador `|`, decimal `.`, fecha `yyyyMMdd`):

```
1234|2.1.1.01.001|FACTURA A|1|1.0000|1|30|0.00|0000300012345|N|20260115|20260115|74123456789012||Compra insumos|1.1.4.02.001|21.00|100000.00|21000.00|1.1.4.02.002|0.00|0.00|0.00|124500.00|0.00|3500.00|0.00|0.00|0.00||0.00||0.00|0.00|5.1.1.01.001|ADM|BASE|1|N
```

Desglose posicional:

| # | Valor | # | Valor | # | Valor |
|---|---|---|---|---|---|
| 1 | `1234` | 14 | *(vacío)* | 27 | `0.00` |
| 2 | `2.1.1.01.001` | 15 | `Compra insumos` | 28 | `0.00` |
| 3 | `FACTURA A` | 16 | `1.1.4.02.001` | 29 | `0.00` |
| 4 | `1` | 17 | `21.00` | 30 | *(vacío)* |
| 5 | `1.0000` | 18 | `100000.00` | 31 | `0.00` |
| 6 | `1` | 19 | `21000.00` | 32 | *(vacío)* |
| 7 | `30` | 20 | `1.1.4.02.002` | 33 | `0.00` |
| 8 | `0.00` | 21 | `0.00` | 34 | `0.00` |
| 9 | `0000300012345` | 22 | `0.00` | 35 | `5.1.1.01.001` |
| 10 | `N` | 23 | `0.00` | 36 | `ADM` |
| 11 | `20260115` | 24 | `124500.00` | 37 | `BASE` |
| 12 | `20260115` | 25 | `0.00` | 38 | `1` |
| 13 | `74123456789012` | 26 | `3500.00` | 39 | `N` |

El importe del campo 24 (`124500.00`) es el **total de ARCA**: neto 100.000 + IVA 21.000 +
otros tributos 3.500. La misma factura como **nota de crédito** saldría con los campos 18,
19, 24 y 26 en negativo, y los campos 17 y 21 sin cambios.
