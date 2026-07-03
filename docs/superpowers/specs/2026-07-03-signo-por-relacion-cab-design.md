# Signo por relación origen↔destino para el Importe Neto del CAB

**Proyecto:** LiquidacionesAuditar
**Fecha:** 2026-07-03

## Problema

El **Importe Neto** del registro **CAB** es un agregado de varias columnas de origen
distintas (bruto, comisión, IVA, retención, etc.) sumadas sobre todas las filas del
grupo de liquidación (`ExportadorLiquidacion.Generar`, rama de sumatoria CAB).

El signo hoy se resuelve con `LiqCol.CondicionSigno`, un IF por **fila**
(`COLUMNA==VALOR|+|-`) que se aplica **por igual a todas las columnas de origen**
relacionadas. Eso no permite decir *"la columna Comisión resta y la columna Bruto suma"*
cuando el archivo trae ambos valores en positivo y no hay una columna-bandera en la fila
que los distinga.

DET y CON ya están resueltos con su IF y **no se tocan**: el signo del detalle es un tema
de "mostrar negativo" que depende de la lógica del sistema receptor, y esos campos no
necesariamente van en negativo en el detalle.

## Solución

Agregar un **signo fijo por relación** (origen↔destino), editable en el grid de selección
de columnas. Solo tiene efecto en la **rama numérica de CAB**.

### Alcance

- **Aplica:** columnas destino de tipo `CAB` con `TipoDato` `decimal` o `int`.
- **No aplica:** DET, CON, ni columnas no numéricas (guardan el signo pero se ignora).
- Default `+` (Suma) ⇒ toda configuración existente produce **resultado idéntico**.

## Cambios por capa

### 1. Base de datos — `Auditar_RelacionCols`

Migración segura (try/catch) en `DatabaseHelper.InitializeDatabase`, mismo patrón que las
demás:

```sql
ALTER TABLE Auditar_RelacionCols ADD COLUMN Signo TEXT NOT NULL DEFAULT '+'
```

Se guarda `+` / `-` internamente (reusa la convención de `ParseDecimal`). Las relaciones
existentes quedan en `+` = Suma.

### 2. Modelo — `RelacionCol`

Agregar `public string Signo { get; set; } = "+";`.

### 3. Repositorio

- **Nuevo** `GetRelacionesConSigno(int idLiqCol) : List<RelacionCol>`
  (`SELECT IdColumnasCSV, COALESCE(Signo,'+') ...`). Lo consumen la rama CAB del exportador
  y el form.
- `GetRelaciones(int) : List<string>` se mantiene sin cambios para DET / date / string /
  `ExtraerFecha`.
- `SetRelaciones` cambia de firma a `SetRelaciones(int idLiqCol, List<RelacionCol> relaciones)`
  para persistir el signo por relación. Único llamador: `FormColumnasDestino`.

### 4. Exportador — `ExportadorLiquidacion` (solo rama CAB)

En la sumatoria CAB de `Generar` (ramas `decimal` e `int`), reemplazar
`GetRelaciones` por `GetRelacionesConSigno` y reestructurar el loop para combinar el signo
del IF (una vez por fila) con el signo de cada relación:

```csharp
foreach (var fila in grupo)
{
    var signoIf = EvaluarCondicion(col.CondicionSigno, fila);   // 1 vez por fila
    foreach (var rel in relaciones)
        if (fila.TryGetValue(rel.IdColumnasCSV, out var v))
        {
            var signoFinal = CombinarSignos(signoIf, rel.Signo);
            suma += ParseDecimal(v, sepDec, sepMil, signoFinal);   // o ParseLong para int
        }
}
```

Helper nuevo (privado), regla **XOR de negativos**:

```csharp
private static string CombinarSignos(string signoIf, string signoRel)
{
    bool negIf  = signoIf  != null && signoIf.Trim()  == "-";
    bool negRel = signoRel != null && signoRel.Trim() == "-";
    return (negIf ^ negRel) ? "-" : "+";
}
```

Con default `signoRel = "+"`, `signoFinal == signoIf` ⇒ comportamiento actual intacto.

**`ResolverCampo` (DET) NO se modifica.** La asimetría existente CAB/DET (el IF general
`col.Condicion` corta el signo en DET pero no en CAB) se mantiene tal cual — fuera de alcance.

### 5. UI — `FormColumnasDestino`

Reemplazar el control `System.Windows.Forms.CheckedListBox clbRelaciones` por un
`Telerik.WinControls.UI.RadGridView gridRelaciones` en el mismo lugar (10, 384 / 500×166),
con 3 columnas:

| Columna | Tipo | Detalle |
|---|---|---|
| **¿Interviene?** | `GridViewCheckBoxColumn` (bool) | reemplaza el check actual |
| **Columna** | texto solo-lectura | nombre de la columna de origen |
| **Signo** | `GridViewComboBoxColumn` | muestra **Suma / Resta** ↔ valor `+` / `-`; default **Suma** |

- **Carga** (`CargarRelaciones`): por cada `ColumnaCSV` de la marca, una fila; check = si
  está relacionada (de `GetRelacionesConSigno`); Signo = el guardado o `+`.
- **Guardar** (`btnGuardar_Click`): filas tildadas → `List<RelacionCol>` con su signo →
  `SetRelaciones`.
- **Limpiar** (`LimpiarFormulario`): limpiar el grid en vez de `clbRelaciones.Items`.
- La columna **Signo queda deshabilitada (read-only, en Suma)** cuando la columna destino
  seleccionada no es `CAB` numérica, para no inducir a error (el signo solo tiene efecto ahí).

## Casos de prueba

1. **Backward-compat:** marca existente sin tocar signos ⇒ salida byte-idéntica a la actual.
2. **Resta simple:** CAB Importe Neto con Bruto=Suma, Comisión=Resta ⇒ neto = bruto − comisión.
3. **Combinación con IF:** relación=Resta + `CondicionSigno` que da `-` en la fila ⇒ doble
   negativo = positivo (XOR).
4. **No numérico:** signo Resta en columna CAB string ⇒ se ignora, sin efecto.
5. **DET intacto:** columna DET con relación marcada Resta ⇒ el detalle no cambia (usa su
   `CondicionSigno`, ignora el signo de relación).

## Fuera de alcance / notas

- DET y CON no se modifican.
- No se extrae helper compartido CAB/DET (mundos separados por decisión de diseño).
- Asimetría preexistente CAB vs DET ante `col.Condicion` no se corrige.
- Bug adyacente detectado (no relacionado, no se toca aquí): `InsertLiqCol`/`UpdateLiqCol`
  agregan el parámetro `@esFiltro` pero el SQL no escribe la columna `esFiltro`, por lo que
  ese flag nunca se persiste.
