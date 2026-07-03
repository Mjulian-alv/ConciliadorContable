# Signo por relación origen↔destino (CAB) — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Permitir marcar un signo fijo (Suma/Resta) por cada columna de origen que
interviene en el Importe Neto del registro CAB, editable en un grid.

**Architecture:** Se agrega la columna `Signo` a la tabla de relaciones origen↔destino
(`Auditar_RelacionCols`). El exportador la usa **solo en la rama de sumatoria CAB**,
combinándola con el IF de signo existente (`CondicionSigno`) vía XOR de negativos. En la UI,
el `CheckedListBox` de relaciones se reemplaza por un `RadGridView` con check + combo de signo.
DET y CON no se tocan.

**Tech Stack:** .NET 8 WinForms, Telerik WinControls (`RadGridView`), Microsoft.Data.Sqlite.

## Global Constraints

- Signo se guarda como `+` / `-` en la BD; se muestra como **Suma** / **Resta** en la UI.
- Default `+` (Suma) ⇒ toda configuración existente produce salida **byte-idéntica**.
- El signo por relación **solo aplica a columnas destino `CAB` de tipo `decimal`/`int`**.
- **No se modifican** `ResolverCampo` (DET), las líneas CON, ni la asimetría preexistente
  CAB/DET frente a `col.Condicion`.
- **No hay arnés de tests** en el proyecto: la verificación de cada tarea es
  `dotnet build LiquidacionesAuditar/LiquidacionesAuditar.csproj -c Debug` (0 errores) +
  el escenario manual indicado.
- **Git no está inicializado** en `D:\Sistemas\ConciliadorContable`. Los pasos de commit
  asumen que se corrió `git init` antes. Si no se quiere versionar, omitir los pasos de commit.

---

### Task 1: Capa de datos — columna `Signo`, modelo y repositorio

**Files:**
- Modify: `LiquidacionesAuditar/Data/DatabaseHelper.cs` (agregar migración segura)
- Modify: `LiquidacionesAuditar/Models/Modelos.cs` (agregar `RelacionCol.Signo`)
- Modify: `LiquidacionesAuditar/Data/Repositorio.cs` (nuevo `GetRelacionesConSigno`, nueva firma de `SetRelaciones`)

**Interfaces:**
- Produces:
  - `RelacionCol.Signo : string` (default `"+"`)
  - `Repositorio.GetRelacionesConSigno(int idLiqCol) : List<RelacionCol>`

> **Nota de orden:** esta tarea es **puramente aditiva** y compila sola. El cambio de firma de
> `SetRelaciones` (que rompería el build hasta ajustar la UI) se hace en la **Task 3**, junto a
> su único llamador `FormColumnasDestino`.

- [ ] **Step 1: Migración segura de la tabla de relaciones**

En `DatabaseHelper.cs`, dentro de `InitializeDatabase`, después de la línea
`try { cn.ExecuteNonQuery("ALTER TABLE Auditar_LineasCON ADD COLUMN CondicionSigno TEXT"); } catch { }`
(línea ~94), agregar:

```csharp
// Signo por relacion origen<->destino (migracion segura)
try { cn.ExecuteNonQuery("ALTER TABLE Auditar_RelacionCols ADD COLUMN Signo TEXT NOT NULL DEFAULT '+'"); } catch { }
```

- [ ] **Step 2: Agregar `Signo` al modelo `RelacionCol`**

En `Modelos.cs`, en la clase `RelacionCol` (líneas 71-75), agregar la propiedad:

```csharp
public class RelacionCol
{
    public string IdLiqCols { get; set; } = "";
    public string IdColumnasCSV { get; set; } = "";
    public string Signo { get; set; } = "+";   // "+" (suma) o "-" (resta)
}
```

- [ ] **Step 3: Nuevo método `GetRelacionesConSigno` en el repositorio**

En `Repositorio.cs`, justo debajo de `GetRelaciones` (después de la línea 248), agregar:

```csharp
public static List<RelacionCol> GetRelacionesConSigno(int idLiqCol)
{
    var list = new List<RelacionCol>();
    using var cn = DatabaseHelper.GetConnection(); cn.Open();
    using var cmd = cn.CreateCommand();
    cmd.CommandText = "SELECT IdColumnasCSV, COALESCE(Signo,'+') FROM Auditar_RelacionCols WHERE IdLiqCols=@id";
    cmd.Parameters.AddWithValue("@id", idLiqCol.ToString());
    using var r = cmd.ExecuteReader();
    while (r.Read())
        list.Add(new RelacionCol
        {
            IdLiqCols     = idLiqCol.ToString(),
            IdColumnasCSV = r.GetString(0),
            Signo         = r.IsDBNull(1) ? "+" : r.GetString(1)
        });
    return list;
}
```

- [ ] **Step 4: Verificar build**

Run: `dotnet build LiquidacionesAuditar/LiquidacionesAuditar.csproj -c Debug`
Expected: build con 0 errores (la tarea es aditiva, no cambia firmas existentes).

- [ ] **Step 5: Commit**

```bash
git add LiquidacionesAuditar/Data/DatabaseHelper.cs LiquidacionesAuditar/Models/Modelos.cs LiquidacionesAuditar/Data/Repositorio.cs
git commit -m "feat(liq): columna Signo en Auditar_RelacionCols + GetRelacionesConSigno"
```

---

### Task 2: Exportador — aplicar signo por relación en la sumatoria CAB

**Files:**
- Modify: `LiquidacionesAuditar/Services/ExportadorLiquidacion.cs` (rama CAB decimal/int + helper `CombinarSignos`)

**Interfaces:**
- Consumes: `Repositorio.GetRelacionesConSigno(int) : List<RelacionCol>` (Task 1)
- Produces: `CombinarSignos(string signoIf, string signoRel) : string` (privado, solo uso interno)

- [ ] **Step 1: Agregar el helper `CombinarSignos`**

En `ExportadorLiquidacion.cs`, agregar el método privado junto a los otros helpers de signo
(por ejemplo debajo de `ParseLong`, después de la línea 337):

```csharp
/// <summary>
/// Combina el signo del IF por fila con el signo fijo de la relación (XOR de negativos).
/// Negativo solo si exactamente uno de los dos es "-". Cualquier otro valor cuenta como "+".
/// </summary>
private static string CombinarSignos(string signoIf, string signoRel)
{
    bool negIf  = signoIf  != null && signoIf.Trim()  == "-";
    bool negRel = signoRel != null && signoRel.Trim() == "-";
    return (negIf ^ negRel) ? "-" : "+";
}
```

- [ ] **Step 2: Reescribir la rama de sumatoria CAB**

En `ExportadorLiquidacion.cs`, reemplazar el bloque de sumatoria CAB (líneas 98-122, desde
`var relaciones = Repositorio.GetRelaciones(col.Id);` hasta el cierre del `else` del `long suma`)
por:

```csharp
var relaciones = Repositorio.GetRelacionesConSigno(col.Id);
if (col.TipoDato.ToLower() == "decimal")
{
    decimal suma = 0m;
    foreach (var fila in grupo)
    {
        var signoIf = EvaluarCondicion(col.CondicionSigno, fila);
        foreach (var rel in relaciones)
            if (fila.TryGetValue(rel.IdColumnasCSV, out var v))
            {
                var signoFinal = CombinarSignos(signoIf, rel.Signo);
                suma += ParseDecimal(v, sepDec, sepMil, signoFinal);
            }
    }
    linCAB.Add(FormatDecimal(suma));
}
else
{
    long suma = 0;
    foreach (var fila in grupo)
    {
        var signoIf = EvaluarCondicion(col.CondicionSigno, fila);
        foreach (var rel in relaciones)
            if (fila.TryGetValue(rel.IdColumnasCSV, out var v))
            {
                var signoFinal = CombinarSignos(signoIf, rel.Signo);
                suma += ParseLong(v, signoFinal);
            }
    }
    linCAB.Add(suma.ToString());
}
```

> `ResolverCampo` (DET) y la rama CON **no se modifican**.

- [ ] **Step 3: Verificar build**

Run: `dotnet build LiquidacionesAuditar/LiquidacionesAuditar.csproj -c Debug`
Expected: build con 0 errores. (Si la Task 3 aún no está hecha, el error será solo el del
llamador de `SetRelaciones` en `FormColumnasDestino`; en ese caso completar Task 3 antes de
dar por buena la compilación.)

- [ ] **Step 4: Commit**

```bash
git add LiquidacionesAuditar/Services/ExportadorLiquidacion.cs
git commit -m "feat(liq): signo por relacion en sumatoria CAB (combinado con CondicionSigno)"
```

---

### Task 3: UI — grid de relaciones con check + signo

**Files:**
- Modify: `LiquidacionesAuditar/Data/Repositorio.cs` (nueva firma de `SetRelaciones`)
- Modify: `LiquidacionesAuditar/Forms/FormColumnasDestino.Designer.cs` (reemplazar control)
- Modify: `LiquidacionesAuditar/Forms/FormColumnasDestino.cs` (carga/guardado/limpieza del grid)

**Interfaces:**
- Consumes: `Repositorio.GetRelacionesConSigno(int)`, `Repositorio.GetColumnasCSV(string)` (Task 1)
- Produces: `Repositorio.SetRelaciones(int idLiqCol, List<RelacionCol> relaciones) : void` (reemplaza la firma vieja `List<string>`)

- [ ] **Step 0: Cambiar la firma de `SetRelaciones` para persistir el signo**

En `Repositorio.cs`, reemplazar el método `SetRelaciones` completo (líneas 250-269) por:

```csharp
public static void SetRelaciones(int idLiqCol, List<RelacionCol> relaciones)
{
    using var cn = DatabaseHelper.GetConnection(); cn.Open();
    using var tx = cn.BeginTransaction();
    using var del = cn.CreateCommand();
    del.Transaction = tx;
    del.CommandText = "DELETE FROM Auditar_RelacionCols WHERE IdLiqCols=@id";
    del.Parameters.AddWithValue("@id", idLiqCol.ToString());
    del.ExecuteNonQuery();
    foreach (var rel in relaciones)
    {
        using var ins = cn.CreateCommand();
        ins.Transaction = tx;
        ins.CommandText = "INSERT OR IGNORE INTO Auditar_RelacionCols(IdLiqCols,IdColumnasCSV,Signo) VALUES(@l,@c,@s)";
        ins.Parameters.AddWithValue("@l", idLiqCol.ToString());
        ins.Parameters.AddWithValue("@c", rel.IdColumnasCSV);
        ins.Parameters.AddWithValue("@s", string.IsNullOrEmpty(rel.Signo) ? "+" : rel.Signo);
        ins.ExecuteNonQuery();
    }
    tx.Commit();
}
```

> Este cambio rompe el build hasta completar el Step 5 (ajuste del llamador en `btnGuardar_Click`).
> Por eso va junto con la UI en esta misma tarea: compilar recién en el Step 6.

- [ ] **Step 1: Reemplazar el control en el Designer**

En `FormColumnasDestino.Designer.cs`:

1. Línea 45 — cambiar la instanciación:
```csharp
gridRelaciones = new Telerik.WinControls.UI.RadGridView();
```
2. Línea ~58 (junto al `BeginInit` de `gridDest`) — agregar el BeginInit del nuevo grid:
```csharp
((System.ComponentModel.ISupportInitialize)gridRelaciones).BeginInit();
((System.ComponentModel.ISupportInitialize)gridRelaciones.MasterTemplate).BeginInit();
```
3. Línea 165 — cambiar el add al panel:
```csharp
grpDetalle.Controls.Add(gridRelaciones);
```
4. Bloque `// clbRelaciones` (líneas 315-320) — reemplazar por la config del grid:
```csharp
//
// gridRelaciones
//
gridRelaciones.Location = new System.Drawing.Point(10, 384);
gridRelaciones.Name = "gridRelaciones";
gridRelaciones.Size = new System.Drawing.Size(500, 166);
gridRelaciones.TabIndex = 13;
```
5. Línea ~387 (junto al `EndInit` de `gridDest`) — agregar el EndInit del nuevo grid:
```csharp
((System.ComponentModel.ISupportInitialize)gridRelaciones.MasterTemplate).EndInit();
((System.ComponentModel.ISupportInitialize)gridRelaciones).EndInit();
```
6. Línea 426 — cambiar la declaración del campo:
```csharp
private Telerik.WinControls.UI.RadGridView gridRelaciones;
```

- [ ] **Step 2: Agregar usings y el view-model de fila**

En `FormColumnasDestino.cs`, asegurar los usings (arriba del archivo):
```csharp
using System.ComponentModel;
```
(`System.Linq`, `Telerik.WinControls.UI` y `LiquidacionesAuditar.Models` ya están.)

Dentro de la clase `FormColumnasDestino`, agregar el campo y la clase interna:
```csharp
private BindingList<RelacionVM> _relacionesVM = new BindingList<RelacionVM>();

private class RelacionVM
{
    public bool Sel { get; set; }
    public string Columna { get; set; } = "";
    public string Signo { get; set; } = "+";   // "+" o "-"
    public string SignoTexto
    {
        get => Signo == "-" ? "Resta" : "Suma";
        set => Signo = value == "Resta" ? "-" : "+";
    }
}
```

- [ ] **Step 3: Configurar las columnas del grid en el constructor**

En `FormColumnasDestino.cs`, agregar el método y llamarlo desde el constructor
(después de `CargarGrid();` en la línea 19):

```csharp
private void ConfigurarGridRelaciones()
{
    gridRelaciones.AutoGenerateColumns = false;
    gridRelaciones.MasterTemplate.Columns.Clear();
    gridRelaciones.AllowAddNewRow = false;
    gridRelaciones.ReadOnly = false;

    var colSel = new GridViewCheckBoxColumn("Sel")
        { HeaderText = "¿Interviene?", FieldName = "Sel", Width = 90 };
    var colNom = new GridViewTextBoxColumn("Columna")
        { HeaderText = "Columna", FieldName = "Columna", ReadOnly = true, Width = 260 };
    var colSig = new GridViewComboBoxColumn("Signo")
        { HeaderText = "Signo", FieldName = "SignoTexto", Width = 110 };
    colSig.DataSource = new[] { "Suma", "Resta" };

    gridRelaciones.MasterTemplate.Columns.AddRange(colSel, colNom, colSig);
}
```

Constructor resultante:
```csharp
public FormColumnasDestino()
{
    InitializeComponent();
    CargarMarcasCombo();
    CargarGrid();
    ConfigurarGridRelaciones();
}
```

- [ ] **Step 4: Reescribir `CargarRelaciones` para poblar el grid**

Reemplazar el método `CargarRelaciones` (líneas 80-91) por:

```csharp
private void CargarRelaciones()
{
    _relacionesVM = new BindingList<RelacionVM>();
    if (_colActual == null || string.IsNullOrEmpty(MarcaActual))
    {
        gridRelaciones.DataSource = _relacionesVM;
        return;
    }

    var colsCSV = Repositorio.GetColumnasCSV(MarcaActual);
    var relacionadas = Repositorio.GetRelacionesConSigno(_colActual.Id)
                       .ToDictionary(x => x.IdColumnasCSV, x => x.Signo, StringComparer.OrdinalIgnoreCase);

    foreach (var col in colsCSV)
    {
        bool sel = relacionadas.TryGetValue(col.IdColumnaArchivo, out var s);
        _relacionesVM.Add(new RelacionVM
        {
            Sel = sel,
            Columna = col.IdColumnaArchivo,
            Signo = sel ? (string.IsNullOrEmpty(s) ? "+" : s) : "+"
        });
    }
    gridRelaciones.DataSource = _relacionesVM;
    AplicarHabilitacionSigno();
}

private void AplicarHabilitacionSigno()
{
    var td = (_colActual?.TipoDato ?? "").ToLower();
    bool cabNumerico = _colActual?.TipoRegistro == "CAB" && (td == "decimal" || td == "int");
    var colSigno = gridRelaciones.Columns["Signo"];
    if (colSigno != null) colSigno.ReadOnly = !cabNumerico;
}
```

- [ ] **Step 5: Actualizar `LimpiarFormulario` y `btnGuardar_Click`**

En `LimpiarFormulario` (líneas 69-78), reemplazar la línea `clbRelaciones.Items.Clear();` por:
```csharp
_relacionesVM = new BindingList<RelacionVM>();
gridRelaciones.DataSource = _relacionesVM;
```

En `btnGuardar_Click`, reemplazar el `Repositorio.SetRelaciones(...)` actual (líneas 132-133) por:
```csharp
gridRelaciones.EndEdit();
var relaciones = _relacionesVM
    .Where(v => v.Sel)
    .Select(v => new RelacionCol { IdColumnasCSV = v.Columna, Signo = v.Signo })
    .ToList();
Repositorio.SetRelaciones(col.Id, relaciones);
```

- [ ] **Step 6: Verificar build**

Run: `dotnet build LiquidacionesAuditar/LiquidacionesAuditar.csproj -c Debug`
Expected: build con 0 errores.

- [ ] **Step 7: Prueba manual (escenarios del spec)**

Ejecutar la app (`LiquidacionesAuditar`), ir a la config de columnas destino de una marca de
prueba y verificar:

1. **Backward-compat:** una marca ya configurada, sin tocar signos, procesa un archivo y da la
   **misma salida** que antes (todas las relaciones en Suma).
2. **Resta simple:** en una columna CAB Importe Neto (decimal), marcar Bruto=Suma y
   Comisión=Resta ⇒ el neto del CAB = bruto − comisión.
3. **Combinación con IF:** relación=Resta + `CondicionSigno` que dé `-` en la fila ⇒ resultado
   positivo (doble negativo).
4. **No numérico / no CAB:** al seleccionar una columna DET o una columna string, la columna
   **Signo del grid queda deshabilitada**.
5. **DET intacto:** el detalle sigue exportando igual que antes.

- [ ] **Step 8: Commit**

```bash
git add LiquidacionesAuditar/Data/Repositorio.cs LiquidacionesAuditar/Forms/FormColumnasDestino.Designer.cs LiquidacionesAuditar/Forms/FormColumnasDestino.cs
git commit -m "feat(liq): grid de relaciones con check + signo Suma/Resta"
```

---

## Notas de ejecución

- **Orden sugerido:** Task 1 → Task 3 → Task 2, o Task 1 y Task 3 seguidas antes de compilar
  (el cambio de firma de `SetRelaciones` deja el proyecto sin compilar hasta ajustar el form).
  El único llamador de `SetRelaciones` es `FormColumnasDestino.btnGuardar_Click`.
- Fuera de alcance (no tocar): `ResolverCampo`/DET, CON, y el bug preexistente de `esFiltro`
  que nunca se persiste (`InsertLiqCol`/`UpdateLiqCol`).
