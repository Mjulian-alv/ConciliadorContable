# Cuentas contables en AgrupadorConceptos — Plan de implementación (TAREA 00021, ítems 1-4)

> **Para agentes:** SUB-SKILL REQUERIDA: usar `superpowers:subagent-driven-development` (recomendado) o `superpowers:executing-plans` para ejecutar tarea por tarea. Los pasos usan checkbox (`- [ ]`) para seguimiento.

**Goal:** Importar el catálogo de cuentas contables del sistema legacy, asignarlas a un perfil de banco (referencia) y a un concepto estándar (la que de verdad importa), y sumar una columna "Cuenta final" a la grilla de movimientos que se autocompleta desde el concepto y se puede corregir a mano sin que se vuelva a pisar.

**Architecture:** Catálogo nuevo (`bancos.CuentasContables`) con el mismo patrón Storage/Service/Models que ya usa el módulo. `CuentaFinal` replica exactamente el mecanismo de `ConceptoFinal` (columna física, autogenerada en el grid Telerik por reflexión, editable inline, con una regla de "no pisar lo editado a mano" que vive en `HomologacionMatcher`) pero **sin cascada**: la cuenta del perfil es sólo un dato de referencia, la única fuente del default es la cuenta del concepto estándar. Ver el diseño completo en `docs/superpowers/specs/2026-09-05-cuentas-contables-perfil-concepto-conciliacion-interna-design.md`.

**Tech Stack:** .NET 8 (`net8.0-windows`), WinForms, Telerik UI for WinForms 2024.4.1113 (`RadGridView`), Dapper 2.1.35 sobre SQL Server (schema `bancos`), ExcelDataReader para import de Excel/CSV.

## Global Constraints

- **Sin proyecto de tests.** La verificación de cada tarea es `dotnet build` limpio; la verificación funcional es el checklist manual de la última tarea, contra una base real.
- **Comentario de versionado en cada bloque agregado o modificado**, según `~/.claude/CLAUDE.md`:
  `// Fecha: 05/09/2026 - TAREA: 00021 - Linea: N - Descripción`
  donde `N` es el ítem del pedido (1 a 4 en este plan; el ítem 5, conciliación interna, es un plan aparte). En archivo nuevo va arriba de todo; en edición, inmediatamente encima del bloque.
- **Ítems del pedido cubiertos acá:** 1 importar cuentas contables legacy · 2 asignar cuenta a perfil · 3 asignar cuenta a concepto estándar (con ventana de mantenimiento nueva) · 4 columna "Cuenta final".
- **Comentarios y textos de UI en español.** Los comentarios explican *por qué*, no qué hace la línea.
- **SQL siempre parametrizado** (`@Param`), nunca interpolado. Operaciones de varias sentencias en transacción explícita.
- **`CuentaFinal` sólo se autocompleta mientras está vacía.** A diferencia de `ConceptoFinal`, no hay una `CuentaEstandar` que trackee el último valor resuelto por el sistema (el diseño aprobado no agrega esa columna), así que no hay forma de distinguir "el usuario la vació a propósito" de "nunca se tocó". La regla que se implementa es más simple pero segura: una vez que `CuentaFinal` tiene contenido, sólo cambia si el usuario la edita — nunca se vuelve a autocompletar sola. Está documentado así, no es un olvido.
- **Cascada:** decidido con el usuario — la cuenta del perfil (`PerfilesBanco.IdCuentaContable`) es sólo informativa. El único origen del default de `CuentaFinal` es la cuenta del concepto estándar homologado.
- **Comparaciones de conceptos y claves con `StringComparison.OrdinalIgnoreCase`** (mismo criterio que el resto del módulo).
- **Encoding:** archivos nuevos y `.Designer.cs` reescritos van en **UTF-8 con BOM**.
- Comando de build único para todas las tareas:
  ```bash
  dotnet build ConciliadorContable.slnx -v q --nologo
  ```
  Esperado: `0 Errores`.

---

## Estructura de archivos

| Archivo | Responsabilidad | Tarea |
|---|---|---|
| `AgrupadorConceptos/Data/SqlSchema.cs` | *Modificar.* Tabla `CuentasContables` + columnas nuevas en `PerfilesBanco`, `ConceptosEstandar`, `MovimientosArchivo`. | 1, 3, 4, 7 |
| `AgrupadorConceptos/Models/CuentaContable.cs` | *Crear.* | 1 |
| `AgrupadorConceptos/Data/CuentaContableStorage.cs` | *Crear.* CRUD + upsert por lote. | 1 |
| `AgrupadorConceptos/Services/ImportacionCuentasContablesService.cs` | *Crear.* Lectura de Excel/CSV con mapeo de columnas. | 1 |
| `AgrupadorConceptos/CuentasContablesForm.cs` + `.Designer.cs` | *Crear.* Listado + panel de importación inline. | 2 |
| `ConciliadorContable/Forms/FormMenuPrincipal.cs` + `.Designer.cs` | *Modificar.* Entrada de menú para Cuentas Contables. | 2 |
| `ConciliadorContable/Models/Usuario.cs` | *Modificar.* Nuevo módulo de permisos `AgrCuentasContables`. | 2 |
| `AgrupadorConceptos/Models/PerfilBanco.cs` | *Modificar.* `IdCuentaContable`. | 3 |
| `AgrupadorConceptos/Data/PerfilBancoStorage.cs` | *Modificar.* Insert/Update con la columna nueva. | 3 |
| `AgrupadorConceptos/MainForm.cs` + `.Designer.cs` | *Modificar.* Combo de cuenta contable. | 3 |
| `AgrupadorConceptos/Models/ConceptoEstandar.cs` | *Modificar.* `IdCuentaContable`. | 4 |
| `AgrupadorConceptos/Models/ConceptoEstandarListado.cs` | *Crear.* Fila de la nueva grilla de mantenimiento. | 4 |
| `AgrupadorConceptos/Data/HomologacionStorage.cs` | *Modificar.* `ObtenerListadoConceptosEstandar`, `ActualizarCuentaConcepto`, `ObtenerCuentasPorConcepto`. | 4, 8 |
| `AgrupadorConceptos/GestionConceptosEstandarForm.cs` + `.Designer.cs` | *Crear.* | 5 |
| `AgrupadorConceptos/AsignarCuentaConceptoDialog.cs` + `.Designer.cs` | *Crear.* | 5 |
| `AgrupadorConceptos/GestionHomologacionesForm.cs` + `.Designer.cs` | *Modificar.* Botón "Conceptos Estándar". | 5 |
| `AgrupadorConceptos/HomologarForm.cs` + `.Designer.cs` | *Modificar.* Combo de cuenta contable, opcional. | 6 |
| `AgrupadorConceptos/Models/MovimientoProcesado.cs` | *Modificar.* `CuentaFinal`. | 7 |
| `AgrupadorConceptos/Data/MovimientoStorage.cs` | *Modificar.* Incluir `CuentaFinal` en insert/update. | 7 |
| `AgrupadorConceptos/ProcesadorForm.cs` | *Modificar.* Columna editable + persistencia inline. | 7 |
| `AgrupadorConceptos/Services/HomologacionMatcher.cs` | *Modificar.* `EscribirCuenta`, `AplicarA` con cuentas. | 8 |
| `AgrupadorConceptos/Services/ImportacionService.cs` | *Modificar.* Completa `CuentaFinal` al importar. | 8 |
| `AgrupadorConceptos/Services/SesionMovimientosService.cs` | *Modificar.* Completa `CuentaFinal` al re-homologar. | 8 |
| `AgrupadorConceptos/Services/HomologacionAdminService.cs` | *Modificar.* Completa `CuentaFinal` en baja/reapuntado. | 8 |
| `docs/Historial.md`, `~/.claude/TAREAS.md` | *Modificar.* | 9 |

---

### Task 1: Catálogo `bancos.CuentasContables`

**Files:**
- Modify: `AgrupadorConceptos/Data/SqlSchema.cs:27` (justo antes de `ConceptosEstandar`)
- Create: `AgrupadorConceptos/Models/CuentaContable.cs`
- Create: `AgrupadorConceptos/Data/CuentaContableStorage.cs`

**Interfaces:**
- Consumes: nada.
- Produces:
  - `CuentaContable { int Id; string Cuenta; string Descripcion; string CentroCosto; string DisplayName }`
  - `CuentaContableStorage.ObtenerTodas()` → `List<CuentaContable>`
  - `CuentaContableStorage.ObtenerPorId(int id)` → `CuentaContable`
  - `CuentaContableStorage.UpsertLote(IEnumerable<CuentaContable>)` → `int` (cantidad procesada)

- [ ] **Step 1: Agregar la tabla al DDL**

En `AgrupadorConceptos/Data/SqlSchema.cs`, insertar **antes** del bloque `IF OBJECT_ID(N'bancos.ConceptosEstandar'...` (línea 27):

```sql
// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 1 - Catalogo de cuentas contables del legacy
IF OBJECT_ID(N'bancos.CuentasContables', N'U') IS NULL
CREATE TABLE bancos.CuentasContables (
    Id          INT IDENTITY(1,1) CONSTRAINT PK_CuentasContables PRIMARY KEY,
    Cuenta      NVARCHAR(50) NOT NULL,
    Descripcion NVARCHAR(300) NOT NULL,
    CentroCosto NVARCHAR(50) NULL
);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_CuentasContables_CuentaCentro')
    CREATE UNIQUE INDEX UX_CuentasContables_CuentaCentro
        ON bancos.CuentasContables(Cuenta, ISNULL(CentroCosto, N''));

```

(Es texto dentro del literal `@"..."` de C#, no un archivo `.sql` aparte: pegar tal cual, respetando que ya está dentro de las comillas verbatim del `const string Ddl`. El comentario de versionado en este archivo va una sola vez arriba del bloque, no repetido por cada `ALTER` que sumen las tareas siguientes.)

- [ ] **Step 2: Modelo**

Crear `AgrupadorConceptos/Models/CuentaContable.cs` (UTF-8 con BOM):

```csharp
// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 1 - Cuenta contable importada del sistema legacy
namespace AgrupadorConceptos.Models
{
    public class CuentaContable
    {
        public int Id { get; set; }
        public string Cuenta { get; set; }
        public string Descripcion { get; set; }
        public string CentroCosto { get; set; }

        public string DisplayName => string.IsNullOrEmpty(CentroCosto)
            ? $"{Cuenta} — {Descripcion}"
            : $"{Cuenta} — {Descripcion} ({CentroCosto})";
    }
}
```

- [ ] **Step 3: Storage**

Crear `AgrupadorConceptos/Data/CuentaContableStorage.cs` (UTF-8 con BOM):

```csharp
// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 1 - Acceso a bancos.CuentasContables
using System.Collections.Generic;
using System.Linq;
using AgrupadorConceptos.Models;
using Dapper;

namespace AgrupadorConceptos.Data
{
    /// <summary>
    /// Acceso a bancos.CuentasContables: el catálogo importado del sistema legacy.
    /// </summary>
    internal static class CuentaContableStorage
    {
        public static List<CuentaContable> ObtenerTodas()
        {
            using var cn = DatabaseHelper.Open();
            return cn.Query<CuentaContable>(
                "SELECT * FROM bancos.CuentasContables ORDER BY Cuenta, CentroCosto").ToList();
        }

        public static CuentaContable ObtenerPorId(int id)
        {
            using var cn = DatabaseHelper.Open();
            return cn.QueryFirstOrDefault<CuentaContable>(
                "SELECT * FROM bancos.CuentasContables WHERE Id = @Id", new { Id = id });
        }

        /// <summary>
        /// Alta o actualización por la clave natural (Cuenta, CentroCosto): reimportar el
        /// mismo export del legacy no duplica filas, sólo actualiza la descripción.
        /// </summary>
        public static int UpsertLote(IEnumerable<CuentaContable> cuentas)
        {
            var lista = cuentas?.ToList() ?? new List<CuentaContable>();
            if (lista.Count == 0) return 0;

            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();

            foreach (var c in lista)
            {
                cn.Execute(@"
                    MERGE bancos.CuentasContables AS destino
                    USING (SELECT @Cuenta AS Cuenta, @CentroCosto AS CentroCosto) AS origen
                    ON destino.Cuenta = origen.Cuenta
                       AND ISNULL(destino.CentroCosto, N'') = ISNULL(origen.CentroCosto, N'')
                    WHEN MATCHED THEN
                        UPDATE SET Descripcion = @Descripcion
                    WHEN NOT MATCHED THEN
                        INSERT (Cuenta, Descripcion, CentroCosto)
                        VALUES (@Cuenta, @Descripcion, @CentroCosto);",
                    new { c.Cuenta, c.Descripcion, c.CentroCosto }, tx);
            }

            tx.Commit();
            return lista.Count;
        }
    }
}
```

- [ ] **Step 4: Compilar**

```bash
dotnet build ConciliadorContable.slnx -v q --nologo
```

Esperado: `0 Errores`.

- [ ] **Step 5: Commit**

```bash
git add AgrupadorConceptos/Data/SqlSchema.cs AgrupadorConceptos/Models/CuentaContable.cs AgrupadorConceptos/Data/CuentaContableStorage.cs
git commit -m "feat(agrupador): catalogo de cuentas contables"
```

---

### Task 2: Pantalla "Cuentas Contables" (listado + import)

**Files:**
- Create: `AgrupadorConceptos/Services/ImportacionCuentasContablesService.cs`
- Create: `AgrupadorConceptos/CuentasContablesForm.cs`
- Create: `AgrupadorConceptos/CuentasContablesForm.Designer.cs`
- Modify: `ConciliadorContable/Models/Usuario.cs`
- Modify: `ConciliadorContable/Forms/FormMenuPrincipal.Designer.cs`
- Modify: `ConciliadorContable/Forms/FormMenuPrincipal.cs`

**Interfaces:**
- Consumes: `CuentaContableStorage.ObtenerTodas() / UpsertLote(...)` (Tarea 1).
- Produces:
  - `ImportacionCuentasContablesService.LeerEncabezados(string filePath)` → `List<string>`
  - `ImportacionCuentasContablesService.Leer(string filePath, string colCuenta, string colDescripcion, string colCentroCosto)` → `List<CuentaContable>`
  - `CuentasContablesForm` (sin parámetros)

- [ ] **Step 1: Servicio de import**

Crear `AgrupadorConceptos/Services/ImportacionCuentasContablesService.cs` (UTF-8 con BOM):

```csharp
// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 1 - Import de cuentas contables del legacy
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AgrupadorConceptos.Models;
using ExcelDataReader;

namespace AgrupadorConceptos.Services
{
    /// <summary>
    /// Lectura de un Excel/CSV con el export de cuentas contables del sistema legacy.
    /// El encabezado siempre está en la primera fila: a diferencia de los extractos
    /// bancarios, este archivo lo arma un export propio y no hace falta la
    /// configurabilidad de fila que tiene PerfilBanco.
    /// </summary>
    public static class ImportacionCuentasContablesService
    {
        public static List<string> LeerEncabezados(string filePath)
        {
            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            using var reader = CrearReader(filePath, stream);

            if (!reader.Read()) return new List<string>();

            var headers = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
                headers.Add(reader.GetValue(i)?.ToString()?.Trim() ?? $"Columna{i}");

            return headers;
        }

        public static List<CuentaContable> Leer(
            string filePath, string colCuenta, string colDescripcion, string colCentroCosto)
        {
            var resultado = new List<CuentaContable>();

            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            using var reader = CrearReader(filePath, stream);

            if (!reader.Read()) return resultado;

            var headers = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
                headers.Add(reader.GetValue(i)?.ToString()?.Trim() ?? "");

            int idxCuenta      = headers.IndexOf(colCuenta ?? "");
            int idxDescripcion = headers.IndexOf(colDescripcion ?? "");
            int idxCentroCosto = string.IsNullOrEmpty(colCentroCosto) ? -1 : headers.IndexOf(colCentroCosto);

            if (idxCuenta == -1 || idxDescripcion == -1)
                throw new System.InvalidOperationException(
                    "No se encontraron las columnas de Cuenta y/o Descripción en el archivo.");

            while (reader.Read())
            {
                string cuenta = reader.GetValue(idxCuenta)?.ToString()?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(cuenta)) continue;

                resultado.Add(new CuentaContable
                {
                    Cuenta      = cuenta,
                    Descripcion = reader.GetValue(idxDescripcion)?.ToString()?.Trim() ?? "",
                    CentroCosto = idxCentroCosto != -1
                        ? (reader.GetValue(idxCentroCosto)?.ToString()?.Trim() ?? "")
                        : null
                });
            }

            return resultado;
        }

        private static IExcelDataReader CrearReader(string filePath, Stream stream)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            return ext == ".csv"
                ? ExcelReaderFactory.CreateCsvReader(stream)
                : ExcelReaderFactory.CreateReader(stream);
        }
    }
}
```

- [ ] **Step 2: Designer de la pantalla**

Crear `AgrupadorConceptos/CuentasContablesForm.Designer.cs` (UTF-8 con BOM):

```csharp
namespace AgrupadorConceptos
{
    partial class CuentasContablesForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            Telerik.WinControls.UI.TableViewDefinition tableViewDefinition1 = new Telerik.WinControls.UI.TableViewDefinition();
            dgvCuentas = new Telerik.WinControls.UI.RadGridView();
            btnImportar = new System.Windows.Forms.Button();
            pnlImportar = new System.Windows.Forms.Panel();
            lblArchivo = new System.Windows.Forms.Label();
            txtArchivo = new System.Windows.Forms.TextBox();
            btnSeleccionarArchivo = new System.Windows.Forms.Button();
            lblColCuenta = new System.Windows.Forms.Label();
            cmbColCuenta = new System.Windows.Forms.ComboBox();
            lblColDescripcion = new System.Windows.Forms.Label();
            cmbColDescripcion = new System.Windows.Forms.ComboBox();
            lblColCentroCosto = new System.Windows.Forms.Label();
            cmbColCentroCosto = new System.Windows.Forms.ComboBox();
            dgvPreview = new Telerik.WinControls.UI.RadGridView();
            btnConfirmarImportar = new System.Windows.Forms.Button();
            btnCancelarImportar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)dgvCuentas).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvCuentas.MasterTemplate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvPreview).BeginInit();
            SuspendLayout();
            //
            // dgvCuentas
            //
            dgvCuentas.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dgvCuentas.Location = new System.Drawing.Point(12, 12);
            dgvCuentas.MasterTemplate.AllowAddNewRow = false;
            dgvCuentas.MasterTemplate.AllowDeleteRow = false;
            dgvCuentas.MasterTemplate.AllowEditRow = false;
            dgvCuentas.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            dgvCuentas.MasterTemplate.ViewDefinition = tableViewDefinition1;
            dgvCuentas.Name = "dgvCuentas";
            dgvCuentas.ReadOnly = true;
            dgvCuentas.Size = new System.Drawing.Size(660, 300);
            dgvCuentas.TabIndex = 0;
            //
            // btnImportar
            //
            btnImportar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnImportar.Location = new System.Drawing.Point(12, 320);
            btnImportar.Name = "btnImportar";
            btnImportar.Size = new System.Drawing.Size(200, 30);
            btnImportar.TabIndex = 1;
            btnImportar.Text = "Importar desde Excel/CSV...";
            btnImportar.UseVisualStyleBackColor = true;
            btnImportar.Click += btnImportar_Click;
            //
            // pnlImportar
            //
            pnlImportar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pnlImportar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pnlImportar.Location = new System.Drawing.Point(12, 12);
            pnlImportar.Size = new System.Drawing.Size(660, 358);
            pnlImportar.Visible = false;
            pnlImportar.Controls.Add(lblArchivo);
            pnlImportar.Controls.Add(txtArchivo);
            pnlImportar.Controls.Add(btnSeleccionarArchivo);
            pnlImportar.Controls.Add(lblColCuenta);
            pnlImportar.Controls.Add(cmbColCuenta);
            pnlImportar.Controls.Add(lblColDescripcion);
            pnlImportar.Controls.Add(cmbColDescripcion);
            pnlImportar.Controls.Add(lblColCentroCosto);
            pnlImportar.Controls.Add(cmbColCentroCosto);
            pnlImportar.Controls.Add(dgvPreview);
            pnlImportar.Controls.Add(btnConfirmarImportar);
            pnlImportar.Controls.Add(btnCancelarImportar);
            //
            // lblArchivo
            //
            lblArchivo.AutoSize = true;
            lblArchivo.Location = new System.Drawing.Point(12, 15);
            lblArchivo.Text = "Archivo:";
            //
            // txtArchivo
            //
            txtArchivo.Location = new System.Drawing.Point(90, 12);
            txtArchivo.ReadOnly = true;
            txtArchivo.Size = new System.Drawing.Size(430, 23);
            //
            // btnSeleccionarArchivo
            //
            btnSeleccionarArchivo.Location = new System.Drawing.Point(526, 11);
            btnSeleccionarArchivo.Size = new System.Drawing.Size(120, 25);
            btnSeleccionarArchivo.Text = "Examinar...";
            btnSeleccionarArchivo.UseVisualStyleBackColor = true;
            btnSeleccionarArchivo.Click += btnSeleccionarArchivo_Click;
            //
            // lblColCuenta
            //
            lblColCuenta.AutoSize = true;
            lblColCuenta.Location = new System.Drawing.Point(12, 50);
            lblColCuenta.Text = "Columna → Cuenta:";
            //
            // cmbColCuenta
            //
            cmbColCuenta.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbColCuenta.Location = new System.Drawing.Point(150, 47);
            cmbColCuenta.Size = new System.Drawing.Size(200, 23);
            //
            // lblColDescripcion
            //
            lblColDescripcion.AutoSize = true;
            lblColDescripcion.Location = new System.Drawing.Point(12, 80);
            lblColDescripcion.Text = "Columna → Descripción:";
            //
            // cmbColDescripcion
            //
            cmbColDescripcion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbColDescripcion.Location = new System.Drawing.Point(150, 77);
            cmbColDescripcion.Size = new System.Drawing.Size(200, 23);
            //
            // lblColCentroCosto
            //
            lblColCentroCosto.AutoSize = true;
            lblColCentroCosto.Location = new System.Drawing.Point(12, 110);
            lblColCentroCosto.Text = "Columna → Centro de costo:";
            //
            // cmbColCentroCosto
            //
            cmbColCentroCosto.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbColCentroCosto.Location = new System.Drawing.Point(190, 107);
            cmbColCentroCosto.Size = new System.Drawing.Size(200, 23);
            //
            // dgvPreview
            //
            dgvPreview.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dgvPreview.Location = new System.Drawing.Point(12, 140);
            dgvPreview.MasterTemplate.AllowAddNewRow = false;
            dgvPreview.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            dgvPreview.Name = "dgvPreview";
            dgvPreview.ReadOnly = true;
            dgvPreview.Size = new System.Drawing.Size(634, 168);
            //
            // btnConfirmarImportar
            //
            btnConfirmarImportar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnConfirmarImportar.Location = new System.Drawing.Point(480, 318);
            btnConfirmarImportar.Size = new System.Drawing.Size(166, 30);
            btnConfirmarImportar.Text = "Confirmar importación";
            btnConfirmarImportar.UseVisualStyleBackColor = true;
            btnConfirmarImportar.Click += btnConfirmarImportar_Click;
            //
            // btnCancelarImportar
            //
            btnCancelarImportar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnCancelarImportar.Location = new System.Drawing.Point(374, 318);
            btnCancelarImportar.Size = new System.Drawing.Size(100, 30);
            btnCancelarImportar.Text = "Cancelar";
            btnCancelarImportar.UseVisualStyleBackColor = true;
            btnCancelarImportar.Click += btnCancelarImportar_Click;
            //
            // CuentasContablesForm
            //
            ClientSize = new System.Drawing.Size(684, 362);
            Controls.Add(pnlImportar);
            Controls.Add(btnImportar);
            Controls.Add(dgvCuentas);
            MinimumSize = new System.Drawing.Size(700, 400);
            Name = "CuentasContablesForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Cuentas Contables";
            ((System.ComponentModel.ISupportInitialize)dgvCuentas.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvCuentas).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvPreview).EndInit();
            ResumeLayout(false);
        }

        private Telerik.WinControls.UI.RadGridView dgvCuentas;
        private System.Windows.Forms.Button btnImportar;
        private System.Windows.Forms.Panel pnlImportar;
        private System.Windows.Forms.Label lblArchivo;
        private System.Windows.Forms.TextBox txtArchivo;
        private System.Windows.Forms.Button btnSeleccionarArchivo;
        private System.Windows.Forms.Label lblColCuenta;
        private System.Windows.Forms.ComboBox cmbColCuenta;
        private System.Windows.Forms.Label lblColDescripcion;
        private System.Windows.Forms.ComboBox cmbColDescripcion;
        private System.Windows.Forms.Label lblColCentroCosto;
        private System.Windows.Forms.ComboBox cmbColCentroCosto;
        private Telerik.WinControls.UI.RadGridView dgvPreview;
        private System.Windows.Forms.Button btnConfirmarImportar;
        private System.Windows.Forms.Button btnCancelarImportar;
    }
}
```

Nota: `btnImportar` se solapa en pantalla con `pnlImportar` (ambos arrancan en `Location (12,12)`/`(12,320)` dentro del mismo `ClientSize`) — es intencional: `pnlImportar` está `Visible = false` al abrir y tapa a `dgvCuentas`/`btnImportar` sólo cuando se activa (mismo truco que `pnlConfigNueva` en `ConciliacionExternForm`).

- [ ] **Step 3: Code-behind**

Crear `AgrupadorConceptos/CuentasContablesForm.cs` (UTF-8 con BOM):

```csharp
// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 1 - Listado e import del catalogo de cuentas contables
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;

namespace AgrupadorConceptos
{
    public partial class CuentasContablesForm : Form
    {
        private List<CuentaContable> _preview = new();

        public CuentasContablesForm()
        {
            InitializeComponent();
            Icon = AppIcon.GetIcon();
            Load += (s, e) => CargarGrilla();
        }

        private void CargarGrilla()
        {
            dgvCuentas.DataSource = null;
            dgvCuentas.DataSource = CuentaContableStorage.ObtenerTodas();
        }

        private void btnImportar_Click(object sender, EventArgs e)
        {
            txtArchivo.Text = "";
            cmbColCuenta.Items.Clear();
            cmbColDescripcion.Items.Clear();
            cmbColCentroCosto.Items.Clear();
            dgvPreview.DataSource = null;
            _preview.Clear();
            pnlImportar.Visible = true;
        }

        private void btnCancelarImportar_Click(object sender, EventArgs e)
        {
            pnlImportar.Visible = false;
        }

        private void btnSeleccionarArchivo_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "Archivos Excel/CSV|*.xls;*.xlsx;*.csv" };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            txtArchivo.Text = ofd.FileName;

            try
            {
                var headers = ImportacionCuentasContablesService.LeerEncabezados(ofd.FileName);

                cmbColCuenta.Items.Clear();
                cmbColDescripcion.Items.Clear();
                cmbColCentroCosto.Items.Clear();
                cmbColCentroCosto.Items.Add(""); // el centro de costo es opcional

                foreach (var h in headers)
                {
                    cmbColCuenta.Items.Add(h);
                    cmbColDescripcion.Items.Add(h);
                    cmbColCentroCosto.Items.Add(h);
                }

                if (cmbColCuenta.Items.Count > 0) cmbColCuenta.SelectedIndex = 0;
                if (cmbColDescripcion.Items.Count > 1) cmbColDescripcion.SelectedIndex = 1;
                cmbColCentroCosto.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al leer el archivo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnConfirmarImportar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtArchivo.Text) || !File.Exists(txtArchivo.Text))
            { MessageBox.Show("Seleccione un archivo.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (cmbColCuenta.SelectedItem == null || cmbColDescripcion.SelectedItem == null)
            { MessageBox.Show("Indique al menos las columnas de Cuenta y Descripción.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            try
            {
                string colCentro = cmbColCentroCosto.SelectedItem as string;
                _preview = ImportacionCuentasContablesService.Leer(
                    txtArchivo.Text,
                    cmbColCuenta.SelectedItem as string,
                    cmbColDescripcion.SelectedItem as string,
                    string.IsNullOrEmpty(colCentro) ? null : colCentro);

                if (_preview.Count == 0)
                { MessageBox.Show("El archivo no tiene filas para importar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                dgvPreview.DataSource = null;
                dgvPreview.DataSource = _preview;

                int procesadas = CuentaContableStorage.UpsertLote(_preview);

                MessageBox.Show($"{procesadas} cuenta(s) importada(s)/actualizada(s).", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                pnlImportar.Visible = false;
                CargarGrilla();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al importar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
```

- [ ] **Step 4: Permiso y entrada de menú**

En `ConciliadorContable/Models/Usuario.cs`, reemplazar el array y el diccionario:

```csharp
        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 1 - Modulo nuevo: catalogo de cuentas contables
        public static readonly string[] TodosLosModulos =
        {
            "ArcaOffline", "ArcaPerfiles", "ArcaEquivalencias",
            "AgrProcesador", "AgrHomologaciones", "AgrConciliacion", "AgrCuentasContables"
        };

        public static readonly Dictionary<string, string> NombresModulos = new()
        {
            ["ArcaOffline"]         = "ARCA - Comprobantes Offline",
            ["ArcaPerfiles"]        = "ARCA - Perfiles Offline",
            ["ArcaEquivalencias"]   = "ARCA - Equivalencias",
            ["AgrProcesador"]       = "Agrupador - Procesador",
            ["AgrHomologaciones"]   = "Agrupador - Homologaciones",
            ["AgrConciliacion"]     = "Agrupador - Conciliación Externa",
            ["AgrCuentasContables"] = "Agrupador - Cuentas Contables",
        };
```

En `ConciliadorContable/Forms/FormMenuPrincipal.cs`, agregar el handler debajo de `BtnAgrupadorConciliacion_Click`:

```csharp
        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 1 - Abrir el catalogo de cuentas contables
        private void BtnAgrupadorCuentasContables_Click(object sender, EventArgs e)
        {
            AgrupadorConceptos.Data.DatabaseHelper.InitializeDatabase();
            AbrirVentana(new AgrupadorConceptos.CuentasContablesForm());
        }
```

Y sumar la línea al chequeo de permisos en `AplicarPermisos`:

```csharp
            btnAgrCuentasContables.Enabled = u.TienePermiso("AgrCuentasContables");
```

- [ ] **Step 5: Botón en el menú principal**

El panel `pnlAgrupador` mide `340x370` y ya tiene 3 filas de botones (y=100,160,220); agregar una cuarta fila requiere agrandar el panel y la ventana. En `ConciliadorContable/Forms/FormMenuPrincipal.Designer.cs`:

Reemplazar:
```csharp
            ClientSize  = new Size(780, 500);
            MinimumSize = new Size(780, 500);
```
por:
```csharp
            ClientSize  = new Size(780, 560);
            MinimumSize = new Size(780, 560);
```

Reemplazar (las dos apariciones, `pnlArca` y `pnlAgrupador`):
```csharp
            pnlArca.Size        = new Size(340, 370);
```
```csharp
            pnlAgrupador.Size        = new Size(340, 370);
```
por `new Size(340, 430)` en ambos, para que las dos tarjetas sigan del mismo alto aunque sólo una tenga el botón nuevo.

Declarar el control (junto a `btnAgrConciliacion`):
```csharp
            btnAgrCuentasContables = new RadButton();
```
y su `BeginInit`/`EndInit` junto a los de `btnAgrConciliacion`:
```csharp
            ((System.ComponentModel.ISupportInitialize)btnAgrCuentasContables).BeginInit();
```
```csharp
            ((System.ComponentModel.ISupportInitialize)btnAgrCuentasContables).EndInit();
```

Sumarlo al `Controls.AddRange` de `pnlAgrupador`:
```csharp
            pnlAgrupador.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                lblAgrTitulo, lblAgrDesc, btnAgrProcesador, btnAgrHomologaciones, btnAgrConciliacion, btnAgrCuentasContables
            });
```

Y la fila de configuración, debajo de la de `btnAgrConciliacion`:
```csharp
            ConfigurarBotonModulo(btnAgrCuentasContables, "Cuentas Contables",     new System.Drawing.Point(16, 280), BtnAgrupadorCuentasContables_Click);
```

Y el campo privado, junto a `btnAgrConciliacion`:
```csharp
        private RadButton btnAgrCuentasContables;
```

- [ ] **Step 6: Compilar**

```bash
dotnet build ConciliadorContable.slnx -v q --nologo
```

Esperado: `0 Errores`.

- [ ] **Step 7: Commit**

```bash
git add AgrupadorConceptos/Services/ImportacionCuentasContablesService.cs AgrupadorConceptos/CuentasContablesForm.cs AgrupadorConceptos/CuentasContablesForm.Designer.cs ConciliadorContable/Models/Usuario.cs ConciliadorContable/Forms/FormMenuPrincipal.cs ConciliadorContable/Forms/FormMenuPrincipal.Designer.cs
git commit -m "feat(agrupador): pantalla de cuentas contables con import de excel/csv"
```

---

### Task 3: Cuenta contable en el perfil

**Files:**
- Modify: `AgrupadorConceptos/Data/SqlSchema.cs` (después del bloque de `PerfilesBanco`)
- Modify: `AgrupadorConceptos/Models/PerfilBanco.cs`
- Modify: `AgrupadorConceptos/Data/PerfilBancoStorage.cs`
- Modify: `AgrupadorConceptos/MainForm.cs`
- Modify: `AgrupadorConceptos/MainForm.Designer.cs`

**Interfaces:**
- Consumes: `CuentaContableStorage.ObtenerTodas()` (Tarea 1).
- Produces: `PerfilBanco.IdCuentaContable` (`int?`).

- [ ] **Step 1: Columna en el schema**

En `AgrupadorConceptos/Data/SqlSchema.cs`, agregar inmediatamente después del `CREATE TABLE bancos.PerfilesBanco (...)` (que cierra en la línea 25 con `);`):

```sql
// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 2 - Cuenta contable de referencia del perfil
IF COL_LENGTH(N'bancos.PerfilesBanco', N'IdCuentaContable') IS NULL
    ALTER TABLE bancos.PerfilesBanco ADD IdCuentaContable INT NULL
        CONSTRAINT FK_Perfil_CuentaContable REFERENCES bancos.CuentasContables(Id);
```

Esta columna referencia `bancos.CuentasContables`, así que este bloque tiene que quedar **después** del `CREATE TABLE bancos.CuentasContables` agregado en la Tarea 1 (que ya está antes que `PerfilesBanco` en el archivo... verificar: en el Step 1 de la Tarea 1 se insertó antes de `ConceptosEstandar`, que está después de `PerfilesBanco`. Si el `ALTER` de esta tarea queda pegado al `CREATE TABLE bancos.PerfilesBanco` original, se ejecuta **antes** de que exista `CuentasContables` y la FK falla. Por eso este bloque va, en cambio, inmediatamente **después** del bloque de `CuentasContables` agregado en la Tarea 1 (antes de `ConceptosEstandar`), no pegado al `CREATE TABLE` de `PerfilesBanco`.

- [ ] **Step 2: Modelo**

En `AgrupadorConceptos/Models/PerfilBanco.cs`, agregar la propiedad al final de la clase:

```csharp
        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 2 - Cuenta contable de referencia (informativa)
        /// <summary>
        /// Sólo dato de referencia del perfil. No alimenta el default de CuentaFinal de los
        /// movimientos (decisión del usuario): esa cuenta sale únicamente del concepto
        /// estándar homologado.
        /// </summary>
        public int? IdCuentaContable { get; set; }
```

- [ ] **Step 3: Storage**

En `AgrupadorConceptos/Data/PerfilBancoStorage.cs`, reemplazar `Insertar` y `Actualizar`:

```csharp
        public static void Insertar(PerfilBanco perfil)
        {
            using var cn = DatabaseHelper.Open();
            cn.Execute(@"
                INSERT INTO bancos.PerfilesBanco
                    (NombreBanco, ColumnaConcepto, ColumnaDescripcion, EsCodigo, FilaEncabezado,
                     TipoImporte, ColumnaImporteUnico, ColumnaDebe, ColumnaHaber, ColumnaFecha, IdCuentaContable)
                VALUES
                    (@NombreBanco, @ColumnaConcepto, @ColumnaDescripcion, @EsCodigo, @FilaEncabezado,
                     @TipoImporte, @ColumnaImporteUnico, @ColumnaDebe, @ColumnaHaber, @ColumnaFecha, @IdCuentaContable)",
                perfil);
        }

        public static void Actualizar(PerfilBanco perfil)
        {
            using var cn = DatabaseHelper.Open();
            cn.Execute(@"
                UPDATE bancos.PerfilesBanco
                SET NombreBanco         = @NombreBanco,
                    ColumnaConcepto     = @ColumnaConcepto,
                    ColumnaDescripcion  = @ColumnaDescripcion,
                    EsCodigo            = @EsCodigo,
                    FilaEncabezado      = @FilaEncabezado,
                    TipoImporte         = @TipoImporte,
                    ColumnaImporteUnico = @ColumnaImporteUnico,
                    ColumnaDebe         = @ColumnaDebe,
                    ColumnaHaber        = @ColumnaHaber,
                    ColumnaFecha        = @ColumnaFecha,
                    IdCuentaContable    = @IdCuentaContable
                WHERE Id = @Id",
                perfil);
        }
```

- [ ] **Step 4: Combo en `MainForm`**

`MainForm.Designer.cs` hoy tiene `grpMapeo` en `(17,130)` tamaño `(500,324)` (fondo en y=454) y `btnGuardar` en `(17,460)` tamaño `(120,40)`, con `ClientSize (550,520)`. No hay hueco: hay que agrandar el form 40px y correr `btnGuardar` para abajo.

Declarar los dos controles nuevos (junto a `numFilaEncabezado`):

```csharp
            lblCuentaContable = new System.Windows.Forms.Label();
            cmbCuentaContable = new System.Windows.Forms.ComboBox();
```

Reemplazar la configuración de `btnGuardar`:

```csharp
            // 
            // btnGuardar
            // 
            btnGuardar.Location = new System.Drawing.Point(17, 500);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new System.Drawing.Size(120, 40);
            btnGuardar.TabIndex = 2;
            btnGuardar.Text = "Guardar Perfil";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // lblCuentaContable
            // 
            lblCuentaContable.AutoSize = true;
            lblCuentaContable.Location = new System.Drawing.Point(20, 464);
            lblCuentaContable.Name = "lblCuentaContable";
            lblCuentaContable.Text = "Cuenta Contable (opcional):";
            // 
            // cmbCuentaContable
            // 
            cmbCuentaContable.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbCuentaContable.FormattingEnabled = true;
            cmbCuentaContable.Location = new System.Drawing.Point(190, 461);
            cmbCuentaContable.Name = "cmbCuentaContable";
            cmbCuentaContable.Size = new System.Drawing.Size(340, 23);
```

(la nueva `btnGuardar.Location` reemplaza a la que hoy dice `new System.Drawing.Point(17, 460)`; el resto de su configuración no cambia).

Reemplazar el bloque `// MainForm` para agrandar el `ClientSize` y sumar los controles nuevos:

```csharp
            // 
            // MainForm
            // 
            ClientSize = new System.Drawing.Size(550, 560);
            Controls.Add(cmbCuentaContable);
            Controls.Add(lblCuentaContable);
            Controls.Add(numFilaEncabezado);
            Controls.Add(lblFilaEncabezado);
            Controls.Add(btnGuardar);
            Controls.Add(grpMapeo);
            Controls.Add(lblArchivoExcel);
            Controls.Add(btnCargarExcel);
            Controls.Add(txtBanco);
            Controls.Add(lblBanco);
            Controls.Add(lblTitulo);
            Name = "MainForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Agrupador de Conceptos - Perfiles Bancarios";
```

(reemplaza el bloque `// MainForm` completo, desde `ClientSize = new System.Drawing.Size(550, 520);` hasta el `Text = ...`, sin tocar lo que viene después —`grpMapeo.ResumeLayout(false);` en adelante— que sigue igual).

Declarar los campos, junto a `cmbColumnaFecha`:
```csharp
        private System.Windows.Forms.Label lblCuentaContable;
        private System.Windows.Forms.ComboBox cmbCuentaContable;
```

En `AgrupadorConceptos/MainForm.cs`, agregar el método de carga y llamarlo desde el constructor y desde `CargarPerfilParaEdicion`:

```csharp
        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 2 - Combo de cuenta contable del perfil
        private void CargarCuentasContables()
        {
            var cuentas = Data.CuentaContableStorage.ObtenerTodas();
            cmbCuentaContable.DataSource = null;
            cmbCuentaContable.DisplayMember = "DisplayName";
            cmbCuentaContable.ValueMember = "Id";

            var conBlanco = new System.Collections.Generic.List<Models.CuentaContable>
            {
                new Models.CuentaContable { Id = 0, Descripcion = "(sin asignar)" }
            };
            conBlanco.AddRange(cuentas);
            cmbCuentaContable.DataSource = conBlanco;
        }
```

Modificar el constructor (agregar la llamada antes de `this.Load += MainForm_Load;`):

```csharp
        public MainForm(int? idPerfil = null)
        {
            InitializeComponent();
            this.Icon = AppIcon.GetIcon();
            _idPerfilEditar = idPerfil;
            ConfigurarUI_Inicial();
            CargarCuentasContables();
            this.Load += MainForm_Load;
        }
```

En `CargarPerfilParaEdicion`, agregar al final del bloque `if (perfil != null)` (antes del cierre de la llave, después de la carga de `cmbColumnaFecha`):

```csharp
                    cmbCuentaContable.SelectedValue = perfil.IdCuentaContable ?? 0;
```

En `btnGuardar_Click`, agregar el campo al objeto `perfil`:

```csharp
                ColumnaFecha = cmbColumnaFecha.SelectedItem?.ToString() ?? "",
                // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 2 - 0 es el centinela "(sin asignar)"
                IdCuentaContable = (int)(cmbCuentaContable.SelectedValue ?? 0) == 0
                    ? (int?)null
                    : (int)cmbCuentaContable.SelectedValue
```

(reemplaza la línea `ColumnaFecha = cmbColumnaFecha.SelectedItem?.ToString() ?? ""` por esas dos líneas, agregando la coma que falte).

- [ ] **Step 5: Compilar**

```bash
dotnet build ConciliadorContable.slnx -v q --nologo
```

Esperado: `0 Errores`. Si `MainForm.Designer.cs` no compila por solapamiento de controles, ajustar sólo las coordenadas Y de los controles agregados (no tocar los existentes).

- [ ] **Step 6: Commit**

```bash
git add AgrupadorConceptos/Data/SqlSchema.cs AgrupadorConceptos/Models/PerfilBanco.cs AgrupadorConceptos/Data/PerfilBancoStorage.cs AgrupadorConceptos/MainForm.cs AgrupadorConceptos/MainForm.Designer.cs
git commit -m "feat(agrupador): cuenta contable de referencia en el perfil de banco"
```

---

### Task 4: Cuenta contable en el concepto estándar (modelo y datos)

**Files:**
- Modify: `AgrupadorConceptos/Data/SqlSchema.cs` (después del bloque de `CuentasContables`)
- Modify: `AgrupadorConceptos/Models/ConceptoEstandar.cs`
- Create: `AgrupadorConceptos/Models/ConceptoEstandarListado.cs`
- Modify: `AgrupadorConceptos/Data/HomologacionStorage.cs`

**Interfaces:**
- Consumes: nada nuevo.
- Produces:
  - `ConceptoEstandar.IdCuentaContable` (`int?`)
  - `ConceptoEstandarListado { int Id; string Nombre; int? IdCuentaContable; string Cuenta; string DescripcionCuenta; int Movimientos; string CuentaDisplay }`
  - `HomologacionStorage.ObtenerListadoConceptosEstandar()` → `List<ConceptoEstandarListado>`
  - `HomologacionStorage.ActualizarCuentaConcepto(int idConcepto, int? idCuentaContable)` → `void`

- [ ] **Step 1: Columna en el schema**

En `AgrupadorConceptos/Data/SqlSchema.cs`, agregar inmediatamente después del bloque de `bancos.CuentasContables` (el `CREATE UNIQUE INDEX UX_CuentasContables_CuentaCentro` agregado en la Tarea 1), antes del `CREATE TABLE bancos.ConceptosEstandar`:

```sql
// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Cuenta contable del concepto estandar
IF COL_LENGTH(N'bancos.ConceptosEstandar', N'IdCuentaContable') IS NULL
    ALTER TABLE bancos.ConceptosEstandar ADD IdCuentaContable INT NULL
        CONSTRAINT FK_Concepto_CuentaContable REFERENCES bancos.CuentasContables(Id);
```

`ConceptosEstandar` ya existe para cuando corre este `ALTER` porque el `CREATE TABLE` de esa tabla está más abajo en el mismo script pero el `IF COL_LENGTH` se ejecuta sin problema aunque la tabla no tenga la columna todavía — igual, para evitar dudas, este bloque va **después** del `CREATE TABLE bancos.ConceptosEstandar` (no antes), es decir a continuación de su cierre `);`.

- [ ] **Step 2: Modelo `ConceptoEstandar`**

En `AgrupadorConceptos/Models/ConceptoEstandar.cs`, agregar la propiedad:

```csharp
namespace AgrupadorConceptos.Models
{
    public class ConceptoEstandar
    {
        public int Id { get; set; }
        public string Nombre { get; set; }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Cuenta contable del concepto
        public int? IdCuentaContable { get; set; }
    }
}
```

- [ ] **Step 3: Modelo de la grilla de mantenimiento**

Crear `AgrupadorConceptos/Models/ConceptoEstandarListado.cs` (UTF-8 con BOM):

```csharp
// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Fila de Gestion de Conceptos Estandar
namespace AgrupadorConceptos.Models
{
    /// <summary>Fila de la pantalla de mantenimiento de conceptos estándar.</summary>
    public class ConceptoEstandarListado
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int? IdCuentaContable { get; set; }
        public string Cuenta { get; set; }
        public string DescripcionCuenta { get; set; }

        /// <summary>
        /// Cuántos movimientos tienen hoy este concepto en ConceptoEstandar. Es un COUNT
        /// directo por texto, no el conteo por regla que usa la grilla de homologaciones:
        /// acá alcanza con saber "cuánto se usa", no de qué regla depende cada uno.
        /// </summary>
        public int Movimientos { get; set; }

        public string CuentaDisplay => IdCuentaContable.HasValue
            ? $"{Cuenta} — {DescripcionCuenta}"
            : "(sin asignar)";
    }
}
```

- [ ] **Step 4: Lecturas y escritura en `HomologacionStorage`**

En `AgrupadorConceptos/Data/HomologacionStorage.cs`, agregar `using System.Data;` ya está presente (línea 3). Agregar estos tres métodos, después de `ObtenerConceptosEstandar()`:

```csharp
        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Listado para Gestion de Conceptos Estandar
        /// <summary>
        /// Un concepto por fila, con su cuenta (si tiene) y cuántos movimientos la usan hoy.
        /// </summary>
        public static List<ConceptoEstandarListado> ObtenerListadoConceptosEstandar()
        {
            using var cn = DatabaseHelper.Open();
            return cn.Query<ConceptoEstandarListado>(@"
                SELECT c.Id, c.Nombre, c.IdCuentaContable,
                       cc.Cuenta, cc.Descripcion AS DescripcionCuenta,
                       ISNULL(m.Movimientos, 0) AS Movimientos
                FROM bancos.ConceptosEstandar c
                LEFT JOIN bancos.CuentasContables cc ON c.IdCuentaContable = cc.Id
                LEFT JOIN (
                    SELECT ConceptoEstandar, COUNT(*) AS Movimientos
                    FROM bancos.MovimientosArchivo
                    GROUP BY ConceptoEstandar
                ) m ON m.ConceptoEstandar = c.Nombre
                ORDER BY c.Nombre").ToList();
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Asignar/quitar la cuenta de un concepto
        public static void ActualizarCuentaConcepto(int idConcepto, int? idCuentaContable)
        {
            using var cn = DatabaseHelper.Open();
            cn.Execute("UPDATE bancos.ConceptosEstandar SET IdCuentaContable = @IdCuenta WHERE Id = @Id",
                new { IdCuenta = idCuentaContable, Id = idConcepto });
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Cuenta de cada concepto, para completar CuentaFinal
        /// <summary>
        /// Nombre del concepto (case-insensitive) → código de cuenta, o "" si no tiene
        /// asignada. Es lo que necesita HomologacionMatcher.EscribirCuenta para autocompletar
        /// CuentaFinal en el mismo momento en que se resuelve el concepto.
        /// </summary>
        public static Dictionary<string, string> ObtenerCuentasPorConcepto()
        {
            using var cn = DatabaseHelper.Open();
            return cn.Query(@"
                SELECT c.Nombre, cc.Cuenta
                FROM bancos.ConceptosEstandar c
                LEFT JOIN bancos.CuentasContables cc ON c.IdCuentaContable = cc.Id")
                .ToDictionary(x => (string)x.Nombre, x => (string)(x.Cuenta ?? ""),
                              StringComparer.OrdinalIgnoreCase);
        }
```

- [ ] **Step 5: Compilar**

```bash
dotnet build ConciliadorContable.slnx -v q --nologo
```

Esperado: `0 Errores`.

- [ ] **Step 6: Commit**

```bash
git add AgrupadorConceptos/Data/SqlSchema.cs AgrupadorConceptos/Models/ConceptoEstandar.cs AgrupadorConceptos/Models/ConceptoEstandarListado.cs AgrupadorConceptos/Data/HomologacionStorage.cs
git commit -m "feat(agrupador): cuenta contable del concepto estandar (modelo y datos)"
```

---

### Task 5: Ventana "Gestión de Conceptos Estándar"

**Files:**
- Create: `AgrupadorConceptos/AsignarCuentaConceptoDialog.cs`
- Create: `AgrupadorConceptos/AsignarCuentaConceptoDialog.Designer.cs`
- Create: `AgrupadorConceptos/GestionConceptosEstandarForm.cs`
- Create: `AgrupadorConceptos/GestionConceptosEstandarForm.Designer.cs`
- Modify: `AgrupadorConceptos/GestionHomologacionesForm.cs`
- Modify: `AgrupadorConceptos/GestionHomologacionesForm.Designer.cs`

**Interfaces:**
- Consumes: `HomologacionStorage.ObtenerListadoConceptosEstandar() / ActualizarCuentaConcepto(...)` (Tarea 4); `CuentaContableStorage.ObtenerTodas()` (Tarea 1).
- Produces: `GestionConceptosEstandarForm()` (sin parámetros).

- [ ] **Step 1: Diálogo de asignación**

Crear `AgrupadorConceptos/AsignarCuentaConceptoDialog.Designer.cs` (UTF-8 con BOM):

```csharp
namespace AgrupadorConceptos
{
    partial class AsignarCuentaConceptoDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblConcepto = new System.Windows.Forms.Label();
            lblCuenta = new System.Windows.Forms.Label();
            cmbCuenta = new System.Windows.Forms.ComboBox();
            btnGuardar = new System.Windows.Forms.Button();
            btnCancelar = new System.Windows.Forms.Button();
            SuspendLayout();
            //
            // lblConcepto
            //
            lblConcepto.AutoSize = true;
            lblConcepto.Location = new System.Drawing.Point(16, 16);
            lblConcepto.Size = new System.Drawing.Size(400, 20);
            //
            // lblCuenta
            //
            lblCuenta.AutoSize = true;
            lblCuenta.Location = new System.Drawing.Point(16, 50);
            lblCuenta.Text = "Cuenta contable:";
            //
            // cmbCuenta
            //
            cmbCuenta.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbCuenta.Location = new System.Drawing.Point(140, 47);
            cmbCuenta.Size = new System.Drawing.Size(340, 23);
            //
            // btnGuardar
            //
            btnGuardar.Location = new System.Drawing.Point(300, 90);
            btnGuardar.Size = new System.Drawing.Size(90, 30);
            btnGuardar.Text = "Guardar";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Click += btnGuardar_Click;
            //
            // btnCancelar
            //
            btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancelar.Location = new System.Drawing.Point(396, 90);
            btnCancelar.Size = new System.Drawing.Size(90, 30);
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            //
            // AsignarCuentaConceptoDialog
            //
            AcceptButton = btnGuardar;
            CancelButton = btnCancelar;
            ClientSize = new System.Drawing.Size(500, 136);
            Controls.Add(btnCancelar);
            Controls.Add(btnGuardar);
            Controls.Add(cmbCuenta);
            Controls.Add(lblCuenta);
            Controls.Add(lblConcepto);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AsignarCuentaConceptoDialog";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Asignar cuenta contable";
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.Label lblConcepto;
        private System.Windows.Forms.Label lblCuenta;
        private System.Windows.Forms.ComboBox cmbCuenta;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnCancelar;
    }
}
```

Crear `AgrupadorConceptos/AsignarCuentaConceptoDialog.cs` (UTF-8 con BOM):

```csharp
// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Asignar/cambiar/quitar la cuenta de un concepto
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;

namespace AgrupadorConceptos
{
    public partial class AsignarCuentaConceptoDialog : Form
    {
        private const int SinAsignar = 0;

        /// <summary>Cuenta elegida, o null si quedó "(sin asignar)".</summary>
        public int? IdCuentaContable { get; private set; }

        public AsignarCuentaConceptoDialog(ConceptoEstandarListado concepto)
        {
            InitializeComponent();
            Icon = AppIcon.GetIcon();

            lblConcepto.Text = $"Concepto: {concepto.Nombre}";

            var opciones = new List<CuentaContable>
            {
                new CuentaContable { Id = SinAsignar, Descripcion = "(sin asignar)" }
            };
            opciones.AddRange(CuentaContableStorage.ObtenerTodas());

            cmbCuenta.DataSource = opciones;
            cmbCuenta.DisplayMember = "DisplayName";
            cmbCuenta.ValueMember = "Id";
            cmbCuenta.SelectedValue = concepto.IdCuentaContable ?? SinAsignar;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            int seleccion = (int)cmbCuenta.SelectedValue;
            IdCuentaContable = seleccion == SinAsignar ? (int?)null : seleccion;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
```

- [ ] **Step 2: Ventana de gestión**

Crear `AgrupadorConceptos/GestionConceptosEstandarForm.Designer.cs` (UTF-8 con BOM):

```csharp
namespace AgrupadorConceptos
{
    partial class GestionConceptosEstandarForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            Telerik.WinControls.UI.TableViewDefinition tableViewDefinition1 = new Telerik.WinControls.UI.TableViewDefinition();
            dgvConceptos = new Telerik.WinControls.UI.RadGridView();
            btnAsignarCuenta = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)dgvConceptos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvConceptos.MasterTemplate).BeginInit();
            SuspendLayout();
            //
            // dgvConceptos
            //
            dgvConceptos.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dgvConceptos.Location = new System.Drawing.Point(12, 12);
            dgvConceptos.MasterTemplate.AllowAddNewRow = false;
            dgvConceptos.MasterTemplate.AllowDeleteRow = false;
            dgvConceptos.MasterTemplate.AllowEditRow = false;
            dgvConceptos.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            dgvConceptos.MasterTemplate.ViewDefinition = tableViewDefinition1;
            dgvConceptos.Name = "dgvConceptos";
            dgvConceptos.ReadOnly = true;
            dgvConceptos.Size = new System.Drawing.Size(600, 300);
            dgvConceptos.TabIndex = 0;
            dgvConceptos.DoubleClick += dgvConceptos_DoubleClick;
            //
            // btnAsignarCuenta
            //
            btnAsignarCuenta.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnAsignarCuenta.Location = new System.Drawing.Point(12, 320);
            btnAsignarCuenta.Size = new System.Drawing.Size(200, 30);
            btnAsignarCuenta.Text = "Asignar cuenta...";
            btnAsignarCuenta.UseVisualStyleBackColor = true;
            btnAsignarCuenta.Click += btnAsignarCuenta_Click;
            //
            // GestionConceptosEstandarForm
            //
            ClientSize = new System.Drawing.Size(624, 362);
            Controls.Add(btnAsignarCuenta);
            Controls.Add(dgvConceptos);
            MinimumSize = new System.Drawing.Size(640, 400);
            Name = "GestionConceptosEstandarForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Gestión de Conceptos Estándar";
            ((System.ComponentModel.ISupportInitialize)dgvConceptos.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvConceptos).EndInit();
            ResumeLayout(false);
        }

        private Telerik.WinControls.UI.RadGridView dgvConceptos;
        private System.Windows.Forms.Button btnAsignarCuenta;
    }
}
```

Crear `AgrupadorConceptos/GestionConceptosEstandarForm.cs` (UTF-8 con BOM):

```csharp
// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Mantenimiento de la cuenta de cada concepto estandar
using System;
using System.Drawing;
using System.Windows.Forms;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;

namespace AgrupadorConceptos
{
    /// <summary>
    /// Único mantenimiento de bancos.ConceptosEstandar que existe: sólo asigna/cambia/quita
    /// la cuenta contable. Renombrar o borrar conceptos sigue fuera de alcance (se crean al
    /// vuelo desde HomologarForm, como siempre).
    /// </summary>
    public partial class GestionConceptosEstandarForm : Form
    {
        public GestionConceptosEstandarForm()
        {
            InitializeComponent();
            Icon = AppIcon.GetIcon();
            Load += (s, e) => CargarDatos();
        }

        private void CargarDatos()
        {
            dgvConceptos.DataSource = null;
            dgvConceptos.DataSource = HomologacionStorage.ObtenerListadoConceptosEstandar();
            ConfigurarGrilla();
        }

        private void ConfigurarGrilla()
        {
            OcultarColumna("Id");
            OcultarColumna("IdCuentaContable");
            OcultarColumna("Cuenta");
            OcultarColumna("DescripcionCuenta");

            RenombrarColumna("Nombre", "Concepto estándar");
            RenombrarColumna("CuentaDisplay", "Cuenta contable");
            RenombrarColumna("Movimientos", "Movimientos");

            var colMovimientos = dgvConceptos.Columns["Movimientos"];
            if (colMovimientos != null)
            {
                colMovimientos.TextAlignment = ContentAlignment.MiddleRight;
                colMovimientos.MaxWidth = 120;
            }
        }

        private void OcultarColumna(string nombre)
        {
            var col = dgvConceptos.Columns[nombre];
            if (col != null) col.IsVisible = false;
        }

        private void RenombrarColumna(string nombre, string titulo)
        {
            var col = dgvConceptos.Columns[nombre];
            if (col != null) col.HeaderText = titulo;
        }

        private void btnAsignarCuenta_Click(object sender, EventArgs e) => AsignarCuentaAFilaSeleccionada();

        private void dgvConceptos_DoubleClick(object sender, EventArgs e) => AsignarCuentaAFilaSeleccionada();

        private void AsignarCuentaAFilaSeleccionada()
        {
            if (dgvConceptos.CurrentRow?.DataBoundItem is not ConceptoEstandarListado fila)
            {
                MessageBox.Show("Seleccione un concepto.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dlg = new AsignarCuentaConceptoDialog(fila);
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            HomologacionStorage.ActualizarCuentaConcepto(fila.Id, dlg.IdCuentaContable);
            CargarDatos();
        }
    }
}
```

- [ ] **Step 3: Entrada desde Gestión de Homologaciones**

En `AgrupadorConceptos/GestionHomologacionesForm.Designer.cs`, agregar un cuarto botón. Declarar (junto a `btnEliminar`):

```csharp
            btnConceptosEstandar = new System.Windows.Forms.Button();
```

Configurarlo (junto a la configuración de `btnEliminar`):

```csharp
            //
            // btnConceptosEstandar
            //
            btnConceptosEstandar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnConceptosEstandar.Location = new System.Drawing.Point(480, 356);
            btnConceptosEstandar.Name = "btnConceptosEstandar";
            btnConceptosEstandar.Size = new System.Drawing.Size(186, 30);
            btnConceptosEstandar.TabIndex = 6;
            btnConceptosEstandar.Text = "Conceptos Estándar...";
            btnConceptosEstandar.UseVisualStyleBackColor = true;
            btnConceptosEstandar.Click += btnConceptosEstandar_Click;
```

Sumarlo a `Controls.Add` (junto a los otros tres botones):

```csharp
            Controls.Add(btnConceptosEstandar);
```

Declarar el campo:

```csharp
        private System.Windows.Forms.Button btnConceptosEstandar;
```

En `AgrupadorConceptos/GestionHomologacionesForm.cs`, agregar el handler al final de la clase (antes del cierre `}`):

```csharp
        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Entrada al mantenimiento de conceptos
        private void btnConceptosEstandar_Click(object sender, EventArgs e)
        {
            using var frm = new GestionConceptosEstandarForm();
            frm.ShowDialog(this);
        }
```

- [ ] **Step 4: Compilar**

```bash
dotnet build ConciliadorContable.slnx -v q --nologo
```

Esperado: `0 Errores`.

- [ ] **Step 5: Commit**

```bash
git add AgrupadorConceptos/AsignarCuentaConceptoDialog.cs AgrupadorConceptos/AsignarCuentaConceptoDialog.Designer.cs AgrupadorConceptos/GestionConceptosEstandarForm.cs AgrupadorConceptos/GestionConceptosEstandarForm.Designer.cs AgrupadorConceptos/GestionHomologacionesForm.cs AgrupadorConceptos/GestionHomologacionesForm.Designer.cs
git commit -m "feat(agrupador): ventana de mantenimiento de conceptos estandar"
```

---

### Task 6: `HomologarForm` pide la cuenta al homologar

**Files:**
- Modify: `AgrupadorConceptos/HomologarForm.cs`
- Modify: `AgrupadorConceptos/HomologarForm.Designer.cs`
- Modify: `AgrupadorConceptos/GestionHomologacionesForm.cs` (propagar la cuenta en la edición)
- Modify: `AgrupadorConceptos/Services/HomologacionAdminService.cs` (`Reapuntar` devuelve el Id del concepto)

**Interfaces:**
- Consumes: `CuentaContableStorage.ObtenerTodas()` (Tarea 1); `HomologacionStorage.ActualizarCuentaConcepto(int, int?)`, `ObtenerCuentasPorConcepto()` (Tarea 4).
- Produces: `HomologarForm.sIdCuentaContable` (`int?`, público); `HomologacionAdminService.Reapuntar(...)` pasa a devolver `int` (antes `void`).

- [ ] **Step 1: Combo en el Designer**

En `AgrupadorConceptos/HomologarForm.Designer.cs`, declarar (junto a `cmbEstandar`):

```csharp
            this.lblCuenta = new System.Windows.Forms.Label();
            this.cmbCuenta = new System.Windows.Forms.ComboBox();
```

Reemplazar el bloque de `btnGuardar`/`lblAyuda` para dejar espacio (mover `btnGuardar` más abajo y sumar los controles nuevos entre `cmbEstandar` y `btnGuardar`):

```csharp
            // lblCuenta
            this.lblCuenta.AutoSize = true;
            this.lblCuenta.Location = new System.Drawing.Point(20, 130);
            this.lblCuenta.Name = "lblCuenta";
            this.lblCuenta.Size = new System.Drawing.Size(126, 15);
            this.lblCuenta.Text = "Cuenta Contable:";

            // cmbCuenta
            this.cmbCuenta.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCuenta.FormattingEnabled = true;
            this.cmbCuenta.Location = new System.Drawing.Point(160, 127);
            this.cmbCuenta.Name = "cmbCuenta";
            this.cmbCuenta.Size = new System.Drawing.Size(300, 23);

            // btnGuardar
            this.btnGuardar.Location = new System.Drawing.Point(160, 180);
```

(la línea de `btnGuardar.Location` reemplaza a la que hoy dice `new System.Drawing.Point(160, 150)`; el resto de la configuración de `btnGuardar` no cambia).

Sumar los dos controles a `Controls.Add` y agrandar el form:

```csharp
            this.Controls.Add(this.cmbCuenta);
            this.Controls.Add(this.lblCuenta);
```

```csharp
            this.ClientSize = new System.Drawing.Size(584, 231);
```

(reemplaza el `new System.Drawing.Size(584, 201)` actual).

Declarar los campos:

```csharp
        private System.Windows.Forms.Label lblCuenta;
        private System.Windows.Forms.ComboBox cmbCuenta;
```

- [ ] **Step 2: Code-behind**

En `AgrupadorConceptos/HomologarForm.cs`, agregar la propiedad pública debajo de `sValorOriginal`:

```csharp
        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Cuenta elegida al homologar
        /// <summary>Cuenta contable con la que se guardó, o null si quedó sin asignar.</summary>
        public int? sIdCuentaContable { get; private set; }
```

Reemplazar `CargarConceptosEstandar` y agregar la carga de cuentas; en el constructor, después de `CargarConceptosEstandar();`, agregar `CargarCuentasContables();` y engancharse al cambio de texto del combo de concepto:

```csharp
        public HomologarForm(int idPerfilBanco, string valorOriginal)
        {
            InitializeComponent();
            this.Icon = AppIcon.GetIcon();
            _idPerfilBanco = idPerfilBanco;
            _valorOriginal = valorOriginal;

            txtOriginal.Text = _valorOriginal;
            CargarConceptosEstandar();
            CargarCuentasContables();

            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Precargar la cuenta si el concepto ya existe
            // Sin esto, tipear/seleccionar un concepto ya homologado en otro perfil y guardar
            // borraria en silencio la cuenta que ya tenia asignada.
            cmbEstandar.TextChanged += (s, e) => PrecargarCuentaDelConcepto();
        }

        private System.Collections.Generic.List<ConceptoEstandar> _conceptosCache = new();

        private void CargarConceptosEstandar()
        {
            _conceptosCache = HomologacionStorage.ObtenerConceptosEstandar();
            cmbEstandar.DataSource = _conceptosCache;
            cmbEstandar.DisplayMember = "Nombre";
            cmbEstandar.ValueMember = "Id";
        }

        private const int SinCuenta = 0;

        private void CargarCuentasContables()
        {
            var opciones = new System.Collections.Generic.List<Models.CuentaContable>
            {
                new Models.CuentaContable { Id = SinCuenta, Descripcion = "(sin asignar)" }
            };
            opciones.AddRange(Data.CuentaContableStorage.ObtenerTodas());

            cmbCuenta.DataSource = opciones;
            cmbCuenta.DisplayMember = "DisplayName";
            cmbCuenta.ValueMember = "Id";
        }

        private void PrecargarCuentaDelConcepto()
        {
            var existente = _conceptosCache.Find(c =>
                string.Equals(c.Nombre, cmbEstandar.Text.Trim(), StringComparison.OrdinalIgnoreCase));

            cmbCuenta.SelectedValue = existente?.IdCuentaContable ?? SinCuenta;
        }
```

Reemplazar `btnGuardar_Click` (agrega la persistencia de la cuenta después de guardar la homologación; en modo `SoloSeleccionar` sólo la deja en `sIdCuentaContable` para que el llamador la propague):

```csharp
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            string conceptoEstandarTexto = cmbEstandar.Text.Trim();

            if (string.IsNullOrEmpty(conceptoEstandarTexto))
            {
                MessageBox.Show("Debe ingresar o seleccionar un Concepto Estándar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string valorClave = txtOriginal.Text.Trim();

            if (string.IsNullOrEmpty(valorClave))
            {
                MessageBox.Show("Debe indicar el concepto o la palabra clave del banco.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Cuenta elegida (puede quedar vacia)
            int seleccionCuenta = (int)(cmbCuenta.SelectedValue ?? SinCuenta);
            sIdCuentaContable = seleccionCuenta == SinCuenta ? (int?)null : seleccionCuenta;

            try
            {
                if (!SoloSeleccionar)
                {
                    int idConcepto = HomologacionStorage.Guardar(_idPerfilBanco, valorClave, conceptoEstandarTexto);
                    HomologacionStorage.ActualizarCuentaConcepto(idConcepto, sIdCuentaContable);
                }

                HomologacionExitosa = true;
                sConcepto = conceptoEstandarTexto;
                sValorOriginal = valorClave;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar homologación: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
```

Agregar el `using AgrupadorConceptos.Models;` si no está (ya está, por `AgrupadorConceptos.Models` usado en la firma del constructor original).

- [ ] **Step 3: Propagar la cuenta en la edición (`GestionHomologacionesForm`)**

En `AgrupadorConceptos/Services/HomologacionAdminService.cs`, cambiar la firma de `Reapuntar` para que devuelva el Id del concepto (hoy es `void`):

```csharp
        /// <summary>
        /// Reapunta la regla a otro concepto y arrastra los movimientos que hoy resuelve.
        /// </summary>
        /// <returns>Id del concepto estándar al que quedó apuntando.</returns>
        public static int Reapuntar(HomologacionListado regla, PerfilBanco perfil, string nombreConcepto)
        {
            var dic = HomologacionStorage.ObtenerDiccionario(perfil.Id);
            var cambiados = new List<MovimientoProcesado>();

            foreach (var mov in MovimientoStorage.ObtenerPorPerfil(perfil.Id))
            {
                string clave = HomologacionMatcher.ResolverClave(dic, mov.ConceptoOriginal, perfil.EsCodigo);
                if (!string.Equals(clave, regla.ValorOriginal, StringComparison.OrdinalIgnoreCase)) continue;

                if (AplicarConcepto(mov, nombreConcepto)) cambiados.Add(mov);
            }

            return HomologacionStorage.ReapuntarYActualizarMovimientos(regla.Id, nombreConcepto, cambiados);
        }
```

En `AgrupadorConceptos/GestionHomologacionesForm.cs`, en `btnEditar_Click`, reemplazar la llamada:

```csharp
            Cursor = Cursors.WaitCursor;
            try
            {
                int idConcepto = HomologacionAdminService.Reapuntar(fila, perfil, frm.sConcepto);
                // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Propagar la cuenta elegida en la edicion
                HomologacionStorage.ActualizarCuentaConcepto(idConcepto, frm.sIdCuentaContable);
            }
```

(reemplaza únicamente la línea `HomologacionAdminService.Reapuntar(fila, perfil, frm.sConcepto);` por las dos líneas de arriba, dentro del mismo bloque `try`).

- [ ] **Step 4: `AplicarConcepto` recibe `cuentasPorConcepto` (preparación para la Tarea 8)**

No hace falta tocarlo todavía — queda para la Tarea 8, que es la que efectivamente completa `CuentaFinal`. Esta tarea sólo deja persistida la cuenta del **concepto**, no toca movimientos.

- [ ] **Step 5: Compilar**

```bash
dotnet build ConciliadorContable.slnx -v q --nologo
```

Esperado: `0 Errores`.

- [ ] **Step 6: Commit**

```bash
git add AgrupadorConceptos/HomologarForm.cs AgrupadorConceptos/HomologarForm.Designer.cs AgrupadorConceptos/GestionHomologacionesForm.cs AgrupadorConceptos/Services/HomologacionAdminService.cs
git commit -m "feat(agrupador): HomologarForm pide la cuenta contable del concepto"
```

---

### Task 7: Columna física "Cuenta Final"

**Files:**
- Modify: `AgrupadorConceptos/Data/SqlSchema.cs`
- Modify: `AgrupadorConceptos/Models/MovimientoProcesado.cs`
- Modify: `AgrupadorConceptos/Data/MovimientoStorage.cs`
- Modify: `AgrupadorConceptos/ProcesadorForm.cs`

**Interfaces:**
- Consumes: nada nuevo.
- Produces: `MovimientoProcesado.CuentaFinal` (`string`); `MovimientoStorage.ActualizarCuentaFinal(int, string)`.

- [ ] **Step 1: Columna en el schema**

En `AgrupadorConceptos/Data/SqlSchema.cs`, agregar inmediatamente después del `CREATE TABLE bancos.MovimientosArchivo (...)`:

```sql
// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Cuenta final del movimiento
IF COL_LENGTH(N'bancos.MovimientosArchivo', N'CuentaFinal') IS NULL
    ALTER TABLE bancos.MovimientosArchivo ADD CuentaFinal NVARCHAR(50) NULL;
```

- [ ] **Step 2: Modelo**

En `AgrupadorConceptos/Models/MovimientoProcesado.cs`, agregar la propiedad al final de la clase:

```csharp
        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Cuenta final, mismo patron que ConceptoFinal
        public string CuentaFinal { get; set; }
```

- [ ] **Step 3: `MovimientoStorage`**

En `AgrupadorConceptos/Data/MovimientoStorage.cs`, reemplazar la constante `UpdateConceptos`:

```csharp
        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Persistir tambien CuentaFinal
        private const string UpdateConceptos =
            "UPDATE bancos.MovimientosArchivo SET ConceptoEstandar = @ConceptoEstandar, ConceptoFinal = @ConceptoFinal, CuentaFinal = @CuentaFinal WHERE Id = @Id";
```

Reemplazar el `INSERT` de `InsertarLote`:

```csharp
                mov.Id = cn.QuerySingle<int>(@"
                    INSERT INTO bancos.MovimientosArchivo
                        (IdArchivo, Fecha, ConceptoOriginal, DescripcionOriginal, Debitos, Creditos, ConceptoEstandar, ConceptoFinal, CuentaFinal)
                    OUTPUT INSERTED.Id
                    VALUES (@IdArchivo, @Fecha, @ConceptoOriginal, @DescripcionOriginal, @Debitos, @Creditos, @ConceptoEstandar, @ConceptoFinal, @CuentaFinal);",
                    mov, tx);
```

Agregar el método análogo a `ActualizarConceptoFinal`, después de él:

```csharp
        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Edicion puntual de CuentaFinal desde la grilla
        /// <summary>Edición puntual de la CuentaFinal desde la grilla.</summary>
        public static void ActualizarCuentaFinal(int id, string cuentaFinal)
        {
            using var cn = DatabaseHelper.Open();
            cn.Execute("UPDATE bancos.MovimientosArchivo SET CuentaFinal = @CuentaFinal WHERE Id = @Id",
                new { CuentaFinal = cuentaFinal, Id = id });
        }
```

- [ ] **Step 4: Grilla del Procesador**

En `AgrupadorConceptos/ProcesadorForm.cs`, reemplazar `ConfigurarGrilla`:

```csharp
        private void ConfigurarGrilla()
        {
            foreach (var col in dgvDatos.Columns)
            {
                // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - CuentaFinal tambien editable inline
                col.ReadOnly = col.Name != "ConceptoFinal" && col.Name != "CuentaFinal";
                col.IsVisible = col.Name != "Id" && col.Name !="IdArchivo";
            }
        }
```

Reemplazar `DgvDatos_CellValueChanged`:

```csharp
        private void DgvDatos_CellValueChanged(object sender, GridViewCellEventArgs e)
        {
            if (e.Row.DataBoundItem is not MovimientoProcesado mov) return;

            if (e.Column.Name == "ConceptoFinal")
                MovimientoStorage.ActualizarConceptoFinal(mov.Id, mov.ConceptoFinal);
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Persistir la edicion inline de CuentaFinal
            else if (e.Column.Name == "CuentaFinal")
                MovimientoStorage.ActualizarCuentaFinal(mov.Id, mov.CuentaFinal);
        }
```

- [ ] **Step 5: Compilar**

```bash
dotnet build ConciliadorContable.slnx -v q --nologo
```

Esperado: `0 Errores`.

- [ ] **Step 6: Commit**

```bash
git add AgrupadorConceptos/Data/SqlSchema.cs AgrupadorConceptos/Models/MovimientoProcesado.cs AgrupadorConceptos/Data/MovimientoStorage.cs AgrupadorConceptos/ProcesadorForm.cs
git commit -m "feat(agrupador): columna fisica CuentaFinal, editable inline"
```

---

### Task 8: Autocompletar "Cuenta Final" desde el concepto

**Files:**
- Modify: `AgrupadorConceptos/Services/HomologacionMatcher.cs`
- Modify: `AgrupadorConceptos/Services/ImportacionService.cs`
- Modify: `AgrupadorConceptos/Services/SesionMovimientosService.cs`
- Modify: `AgrupadorConceptos/Services/HomologacionAdminService.cs`

**Interfaces:**
- Consumes: `HomologacionStorage.ObtenerCuentasPorConcepto()` (Tarea 4); `MovimientoProcesado.CuentaFinal` (Tarea 7).
- Produces: `HomologacionMatcher.EscribirCuenta(MovimientoProcesado, string)` → `bool`; `AplicarA` gana un parámetro opcional.

- [ ] **Step 1: `EscribirCuenta` y `AplicarA`**

En `AgrupadorConceptos/Services/HomologacionMatcher.cs`, reemplazar `AplicarA` y agregar `EscribirCuenta` a continuación de `EscribirConcepto`:

```csharp
        /// <summary>
        /// Aplica el match a un movimiento ya existente. Pisa siempre ConceptoEstandar,
        /// pero respeta un ConceptoFinal que el usuario haya editado a mano en la grilla:
        /// solo lo sobrescribe si seguía pendiente.
        /// No hace nada si no hubo match.
        /// </summary>
        /// <param name="cuentasPorConcepto">
        /// Nombre del concepto → cuenta, para completar CuentaFinal en el mismo paso
        /// (HomologacionStorage.ObtenerCuentasPorConcepto). Null si el llamador no la tiene
        /// a mano: en ese caso CuentaFinal no se toca.
        /// </param>
        public static void AplicarA(MovimientoProcesado mov, bool esCodigo,
            IDictionary<string, string> dicHomologacion, IDictionary<string, string> cuentasPorConcepto = null)
        {
            string valorABuscar = mov.ConceptoOriginal;

            string homologado = Resolver(dicHomologacion, valorABuscar, esCodigo);
            if (homologado == null) return;

            EscribirConcepto(mov, homologado);

            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Completar CuentaFinal junto con el concepto
            if (cuentasPorConcepto != null && cuentasPorConcepto.TryGetValue(homologado, out string cuenta))
                EscribirCuenta(mov, cuenta);
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Autocompletar CuentaFinal sin pisar lo editado
        /// <summary>
        /// Escribe CuentaFinal sólo mientras está vacía. A diferencia de EscribirConcepto,
        /// no hay una "CuentaEstandar" que trackee el último valor resuelto por el sistema
        /// (el diseño no agrega esa columna), así que no se puede distinguir "el usuario la
        /// vació a propósito" de "nunca se tocó". La regla es más simple pero segura: una vez
        /// que tiene contenido, sólo cambia si el usuario la edita a mano en la grilla.
        /// </summary>
        /// <returns>True si se completó (estaba vacía y cambió).</returns>
        public static bool EscribirCuenta(MovimientoProcesado mov, string cuentaNueva)
        {
            if (!string.IsNullOrWhiteSpace(mov.CuentaFinal)) return false;

            string valor = cuentaNueva ?? "";
            if (mov.CuentaFinal == valor) return false;

            mov.CuentaFinal = valor;
            return true;
        }
```

- [ ] **Step 2: `ImportacionService`**

En `AgrupadorConceptos/Services/ImportacionService.cs`, en `ImportarArchivo`, agregar la obtención del diccionario de cuentas junto a la del diccionario de homologación:

```csharp
            progreso?.Invoke(new ProgresoImportacion(ProgresoImportacion.PasoHomologando));
            var dicHomologacion = HomologacionStorage.ObtenerDiccionario(perfil.Id);
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Cuenta de cada concepto, para CuentaFinal
            var cuentasPorConcepto = HomologacionStorage.ObtenerCuentasPorConcepto();

            swParseo.Start();
            var movimientos = Parsear(filePath, perfil, dicHomologacion, cuentasPorConcepto);
            swParseo.Stop();
```

Cambiar la firma de `Parsear` y el uso del diccionario dentro del loop:

```csharp
        private static List<MovimientoProcesado> Parsear(
            string filePath, PerfilBanco perfil, IDictionary<string, string> dicHomologacion,
            IDictionary<string, string> cuentasPorConcepto)
```

Reemplazar el bloque que arma el `MovimientoProcesado`:

```csharp
                string valorABuscar = perfil.EsCodigo ? concepto : descripcion;
                string conceptoEstandar =
                    HomologacionMatcher.Resolver(dicHomologacion, valorABuscar, perfil.EsCodigo)
                    ?? ConceptosBancarios.PendienteHomologar;

                // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Cuenta final por defecto del concepto
                string cuentaFinal = conceptoEstandar != ConceptosBancarios.PendienteHomologar &&
                                     cuentasPorConcepto.TryGetValue(conceptoEstandar, out string cta)
                    ? cta : "";

                movimientos.Add(new MovimientoProcesado
                {
                    ConceptoOriginal    = concepto,
                    DescripcionOriginal = descripcion,
                    Fecha               = fecha,
                    Debitos             = debitos,
                    Creditos            = creditos,
                    ConceptoEstandar    = conceptoEstandar,
                    ConceptoFinal       = conceptoEstandar == ConceptosBancarios.PendienteHomologar ? "" : conceptoEstandar,
                    CuentaFinal         = cuentaFinal
                });
```

- [ ] **Step 3: `SesionMovimientosService`**

En `AgrupadorConceptos/Services/SesionMovimientosService.cs`, en `RehomologarPendientes`:

```csharp
        public static List<MovimientoProcesado> RehomologarPendientes(int idArchivo, PerfilBanco perfil)
        {
            var movs = MovimientoStorage.ObtenerPorArchivo(idArchivo);
            var dicHomologacion = HomologacionStorage.ObtenerDiccionario(perfil.Id);
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Completar CuentaFinal al rehomologar
            var cuentasPorConcepto = HomologacionStorage.ObtenerCuentasPorConcepto();

            var rehomologados = new List<MovimientoProcesado>();
            foreach (var mov in movs)
            {
                if (mov.ConceptoEstandar != ConceptosBancarios.PendienteHomologar) continue;

                HomologacionMatcher.AplicarA(mov, perfil.EsCodigo, dicHomologacion, cuentasPorConcepto);
                rehomologados.Add(mov);
            }

            MovimientoStorage.ActualizarConceptos(rehomologados);
            return movs;
        }
```

En `RehomologarEnMemoria`:

```csharp
        public static ISet<MovimientoProcesado> RehomologarEnMemoria(
            List<MovimientoProcesado> movs, PerfilBanco perfil)
        {
            var cambiados = new HashSet<MovimientoProcesado>();
            if (movs == null || movs.Count == 0) return cambiados;

            var dicHomologacion = HomologacionStorage.ObtenerDiccionario(perfil.Id);
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Completar CuentaFinal al rehomologar
            var cuentasPorConcepto = HomologacionStorage.ObtenerCuentasPorConcepto();
            foreach (var mov in movs)
            {
                if (mov.ConceptoEstandar != ConceptosBancarios.PendienteHomologar) continue;

                HomologacionMatcher.AplicarA(mov, perfil.EsCodigo, dicHomologacion, cuentasPorConcepto);
                cambiados.Add(mov);
            }

            MovimientoStorage.ActualizarConceptos(cambiados);
            return cambiados;
        }
```

En `ReaplicarHomologacion`:

```csharp
        public static ISet<MovimientoProcesado> ReaplicarHomologacion(
            IEnumerable<MovimientoProcesado> movs, PerfilBanco perfil)
        {
            var cambiados = new HashSet<MovimientoProcesado>();
            if (movs == null) return cambiados;

            var dic = HomologacionStorage.ObtenerDiccionario(perfil.Id);
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Completar CuentaFinal al reaplicar
            var cuentasPorConcepto = HomologacionStorage.ObtenerCuentasPorConcepto();

            foreach (var mov in movs)
            {
                string concepto = HomologacionMatcher.Resolver(dic, mov.ConceptoOriginal, perfil.EsCodigo)
                                  ?? ConceptosBancarios.PendienteHomologar;

                bool cambioConcepto = HomologacionMatcher.EscribirConcepto(mov, concepto);
                bool cambioCuenta = cuentasPorConcepto.TryGetValue(concepto, out string cuenta) &&
                                     HomologacionMatcher.EscribirCuenta(mov, cuenta);

                if (cambioConcepto || cambioCuenta) cambiados.Add(mov);
            }

            MovimientoStorage.ActualizarConceptos(cambiados);
            return cambiados;
        }
```

- [ ] **Step 4: `HomologacionAdminService`**

En `AgrupadorConceptos/Services/HomologacionAdminService.cs`, reemplazar `AplicarBaja`, `Reapuntar` y `AplicarConcepto`:

```csharp
        public static void AplicarBaja(HomologacionListado regla, ImpactoHomologacion impacto, string conceptoDestino)
        {
            var cambiados = new List<MovimientoProcesado>();
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Completar CuentaFinal al propagar la baja
            var cuentasPorConcepto = HomologacionStorage.ObtenerCuentasPorConcepto();

            string paraAfectados = string.IsNullOrWhiteSpace(conceptoDestino)
                ? ConceptosBancarios.PendienteHomologar
                : conceptoDestino.Trim();

            foreach (var item in impacto.Afectados)
                if (AplicarConcepto(item.Movimiento, paraAfectados, cuentasPorConcepto)) cambiados.Add(item.Movimiento);

            foreach (var item in impacto.CubiertosPorOtraRegla)
                if (AplicarConcepto(item.Movimiento, item.ConceptoSinLaRegla, cuentasPorConcepto)) cambiados.Add(item.Movimiento);

            HomologacionStorage.EliminarYActualizarMovimientos(regla.Id, cambiados);
        }

        /// <summary>
        /// Reapunta la regla a otro concepto y arrastra los movimientos que hoy resuelve.
        /// </summary>
        /// <returns>Id del concepto estándar al que quedó apuntando.</returns>
        public static int Reapuntar(HomologacionListado regla, PerfilBanco perfil, string nombreConcepto)
        {
            var dic = HomologacionStorage.ObtenerDiccionario(perfil.Id);
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Completar CuentaFinal al reapuntar
            var cuentasPorConcepto = HomologacionStorage.ObtenerCuentasPorConcepto();
            var cambiados = new List<MovimientoProcesado>();

            foreach (var mov in MovimientoStorage.ObtenerPorPerfil(perfil.Id))
            {
                string clave = HomologacionMatcher.ResolverClave(dic, mov.ConceptoOriginal, perfil.EsCodigo);
                if (!string.Equals(clave, regla.ValorOriginal, StringComparison.OrdinalIgnoreCase)) continue;

                if (AplicarConcepto(mov, nombreConcepto, cuentasPorConcepto)) cambiados.Add(mov);
            }

            return HomologacionStorage.ReapuntarYActualizarMovimientos(regla.Id, nombreConcepto, cambiados);
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 4 - Tambien completa CuentaFinal, sin pisar lo editado
        /// <summary>
        /// Escribe el concepto y, si corresponde, la cuenta — respetando lo editado a mano.
        /// Ver <see cref="HomologacionMatcher.EscribirConcepto"/> y
        /// <see cref="HomologacionMatcher.EscribirCuenta"/>.
        /// </summary>
        private static bool AplicarConcepto(
            MovimientoProcesado mov, string conceptoNuevo, IDictionary<string, string> cuentasPorConcepto)
        {
            bool cambioConcepto = HomologacionMatcher.EscribirConcepto(mov, conceptoNuevo);
            bool cambioCuenta = cuentasPorConcepto.TryGetValue(conceptoNuevo, out string cuenta) &&
                                HomologacionMatcher.EscribirCuenta(mov, cuenta);
            return cambioConcepto || cambioCuenta;
        }
```

Agregar `using System.Collections.Generic;` si no está (ya está, línea 3 del archivo).

- [ ] **Step 5: Compilar**

```bash
dotnet build ConciliadorContable.slnx -v q --nologo
```

Esperado: `0 Errores`.

- [ ] **Step 6: Commit**

```bash
git add AgrupadorConceptos/Services/HomologacionMatcher.cs AgrupadorConceptos/Services/ImportacionService.cs AgrupadorConceptos/Services/SesionMovimientosService.cs AgrupadorConceptos/Services/HomologacionAdminService.cs
git commit -m "feat(agrupador): autocompletar CuentaFinal desde la cuenta del concepto"
```

---

### Task 9: Bitácora

**Files:**
- Modify: `~/.claude/TAREAS.md`
- Modify: `docs/Historial.md` (crear si no existe)

- [ ] **Step 1: Verificación manual end-to-end**

Contra una base real (no hay proyecto de tests):

1. Abrir "Cuentas Contables" desde el menú, importar un Excel de ejemplo con columnas Cuenta/Descripción/Centro de Costo. Verificar que la grilla se llena y que reimportar el mismo archivo no duplica filas (actualiza descripción si cambió).
2. Editar un perfil de banco, asignarle una cuenta contable, guardar, reabrir el perfil y verificar que quedó seleccionada.
3. Homologar un concepto nuevo desde el Procesador, asignándole una cuenta en el mismo diálogo. Verificar en "Conceptos Estándar" que la cuenta quedó guardada.
4. Desde "Conceptos Estándar", cambiar la cuenta de un concepto ya usado por varios movimientos (no importar de nuevo). Verificar que el conteo de "Movimientos" coincide con lo esperado.
5. Importar un archivo nuevo con un concepto que ya tiene cuenta asignada: verificar que `Cuenta Final` sale completa sola.
6. Editar a mano `Cuenta Final` de una fila en la grilla del Procesador, reimportar/rehomologar ese mismo movimiento (doble click → nueva homologación con otra cuenta): verificar que la edición manual **no** se pisa.
7. Un movimiento con `Cuenta Final` vacía (concepto sin cuenta asignada): asignarle cuenta al concepto después, re-homologar ese movimiento puntual y verificar que ahora sí se completa (porque seguía vacía).
8. Perfil con una cuenta asignada y su concepto con OTRA cuenta distinta: verificar que `Cuenta Final` sale la del concepto, nunca la del perfil (sin cascada).

- [ ] **Step 2: `~/.claude/TAREAS.md`**

Agregar la fila (reemplazando el comentario `<!-- Próxima tarea: 00021 -->` por `<!-- Próxima tarea: 00022 -->` al final del archivo):

```
| 00021 | 05/09/2026 | ConciliadorContable | 1, 2, 3, 4 | Catálogo de cuentas contables importado desde Excel/CSV del sistema legacy; asignación de cuenta contable (informativa) al perfil de banco y (efectiva) al concepto estándar, con una ventana nueva de mantenimiento (`GestionConceptosEstandarForm`) que no existía; `HomologarForm` pide la cuenta al homologar; columna `CuentaFinal` en la grilla del Procesador, editable a mano y autocompletada desde la cuenta del concepto sin pisar ediciones manuales. El ítem 5 (conciliación interna entre extractos) queda en un plan aparte bajo la misma TAREA. Detalle en `D:\Sistemas\ConciliadorContable\docs\Historial.md`. |
```

- [ ] **Step 3: `docs/Historial.md`**

Si el archivo no existe, crearlo con un encabezado mínimo (`# Historial`) antes de la entrada. Agregar al final:

```markdown
## TAREA 00021 — Cuentas contables (ítems 1 a 4)

**Pedido:**

> 1. Importar cuentas contables de sistema legacy, sería cuenta y descripción y centro de costo.
> 2. Asignar cuenta contable a perfil.
> 3. Asignar cuenta contable a concepto standard. Acá vamos a tener que crear una ventana para
>    esto ya que no hay un mantenimiento del concepto standard. Al momento de homologar ahora
>    solo pide una descripción y ahora tiene que pedir la cuenta. La misma puede estar vacía.
> 4. Crear una columna de cuenta final así como está la de concepto final. Por defecto lleva
>    la del concepto standard.

**Ejecutado:**

- Catálogo `bancos.CuentasContables`, con pantalla de import de Excel/CSV mapeando columnas
  (upsert por la clave natural Cuenta+CentroCosto, para poder reimportar sin duplicar).
- `PerfilesBanco.IdCuentaContable`: dato de referencia en el perfil, sin efecto en el cálculo
  de `CuentaFinal` (decisión explícita: sin cascada perfil → movimiento).
- `ConceptosEstandar.IdCuentaContable` con ventana nueva de mantenimiento
  (`GestionConceptosEstandarForm`, accesible desde Gestión de Homologaciones) y con
  `HomologarForm` pidiendo la cuenta en el mismo paso de homologar.
- `MovimientosArchivo.CuentaFinal`: mismo mecanismo que `ConceptoFinal` (editable inline,
  autocompletada desde la cuenta del concepto), con la salvedad documentada de que sólo se
  autocompleta mientras está vacía (no hay una "CuentaEstandar" que trackee el valor resuelto
  por el sistema, a diferencia de `ConceptoFinal`).
- Diseño completo en `docs/superpowers/specs/2026-09-05-cuentas-contables-perfil-concepto-conciliacion-interna-design.md`.
- El ítem 5 (conciliación interna entre extractos) se implementa en un plan aparte,
  `docs/superpowers/plans/2026-09-05-conciliacion-interna.md`, bajo la misma TAREA.
```

- [ ] **Step 4: Commit**

```bash
git add docs/Historial.md
git commit -m "docs: TAREA 00021 (items 1-4) en la bitacora"
```

(El commit de `~/.claude/TAREAS.md` es aparte porque vive fuera del repo — no entra en este `git add`.)
