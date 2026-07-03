using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Telerik.WinControls.UI;
using LiquidacionesAuditar.Data;
using LiquidacionesAuditar.Models;
using LiquidacionesAuditar.Services;

namespace LiquidacionesAuditar
{
    /// <summary>
    /// CRUD de Marcas/Procesadores y sus columnas CSV esperadas.
    /// También permite importar columnas directamente desde un archivo CSV.
    /// </summary>
    public partial class FormColumnasCSV : RadForm
    {
        private MarcaTarjeta _marcaActual;

        public FormColumnasCSV()
        {
            InitializeComponent();
            CargarMarcas();
        }

        // ── Marcas ────────────────────────────────────────────────────────

        private void CargarMarcas()
        {
            var marcas = Repositorio.GetMarcas();
            lstMarcas.DataSource = null;
            lstMarcas.DataSource = marcas;
            lstMarcas.DisplayMember = "Nombre";
            lstMarcas.ValueMember = "Id";
        }

        private void lstMarcas_SelectedIndexChanged(object sender, EventArgs e)
        {
            _marcaActual = lstMarcas.SelectedItem as MarcaTarjeta;
            if (_marcaActual == null) return;
            txtMarcaId.Text = _marcaActual.Id;
            txtMarcaNombre.Text = _marcaActual.Nombre;
            nudFilaEncabezado.Value = _marcaActual.FilaEncabezado < 1 ? 1 : _marcaActual.FilaEncabezado;
            txtColumnaLiquidacion.Text = _marcaActual.ColumnaLiquidacion ?? "";
            txtSepDec.Text = _marcaActual.SeparadorDecimales ?? "";
            txtSepMil.Text = _marcaActual.SeparadorMiles ?? "";

            // Filtro de liquidación
            if (!string.IsNullOrWhiteSpace(_marcaActual.TipoDato))
                cmbTipoDato.SelectedValue = _marcaActual.TipoDato;

            CargarColumnas();
            CargarColumnasParaFiltro();
        }

        private void btnNuevaMarca_Click(object sender, EventArgs e)
        {
            txtMarcaId.Text = "";
            txtMarcaNombre.Text = "";
            txtMarcaId.Focus();
        }

        private void btnGuardarMarca_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMarcaId.Text) || string.IsNullOrWhiteSpace(txtMarcaNombre.Text))
            { MessageBox.Show("Ingrese ID y Nombre de la marca.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            Repositorio.UpsertMarca(new MarcaTarjeta
            {
                Id = txtMarcaId.Text.Trim(),
                Nombre = txtMarcaNombre.Text.Trim(),
                FilaEncabezado = (int)nudFilaEncabezado.Value,
                ColumnaLiquidacion = txtColumnaLiquidacion.Text.Trim(),
                SeparadorDecimales = txtSepDec.Text.Trim(),
                SeparadorMiles = txtSepMil.Text.Trim(),
                TipoDato = cmbTipoDato.SelectedItem?.ToString() ?? "",
                ColumnaFiltro = cb_Columna.SelectedItem?.ToString() ?? ""
            });
            CargarMarcas();
            SeleccionarMarca(txtMarcaId.Text.Trim());
        }

        private void btnEliminarMarca_Click(object sender, EventArgs e)
        {
            if (_marcaActual == null) return;
            if (MessageBox.Show($"¿Eliminar la marca '{_marcaActual.Nombre}' y todas sus columnas?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            Repositorio.DeleteMarca(_marcaActual.Id);
            CargarMarcas();
        }

        private void SeleccionarMarca(string id)
        {
            foreach (MarcaTarjeta m in lstMarcas.Items)
                if (m.Id == id) { lstMarcas.SelectedItem = m; return; }
        }

        // ── Columnas CSV ──────────────────────────────────────────────────

        private void CargarColumnas()
        {
            if (_marcaActual == null) return;
            var cols = Repositorio.GetColumnasCSV(_marcaActual.Id);
            gridColumnas.DataSource = null;
            gridColumnas.DataSource = cols;
            gridColumnas.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;
            //BeginInvoke(new Action(() => { try { gridColumnas.BestFitColumns(); } catch { } }));
        }

        private void CargarColumnasParaFiltro()
        {
            if (_marcaActual == null) return;
            var cols = Repositorio.GetColumnasCSV(_marcaActual.Id)
                                  .Select(c => c.IdColumnaArchivo)
                                  .ToList();
            cb_Columna.DataSource = null;
            cb_Columna.DataSource = cols;

            if (!string.IsNullOrWhiteSpace(_marcaActual.ColumnaFiltro))
                cb_Columna.SelectedValue = _marcaActual.ColumnaFiltro;
        }

        private void btnAgregarColumna_Click(object sender, EventArgs e)
        {
            if (_marcaActual == null) { MessageBox.Show("Seleccione una marca primero."); return; }
            if (string.IsNullOrWhiteSpace(txtNuevaColumna.Text)) return;

            Repositorio.InsertColumnaCSV(new ColumnaCSV
            {
                IdMarcaTarjeta = _marcaActual.Id,
                IdColumnaArchivo = txtNuevaColumna.Text.Trim()
            });
            txtNuevaColumna.Text = "";
            CargarColumnas();
        }

        private void btnEliminarColumna_Click(object sender, EventArgs e)
        {
            if (gridColumnas.SelectedRows.Count == 0) return;
            var col = gridColumnas.SelectedRows[0].DataBoundItem as ColumnaCSV;
            if (col == null) return;
            if (MessageBox.Show($"¿Eliminar la columna '{col.IdColumnaArchivo}'?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            Repositorio.DeleteColumnaCSV(col.Id);
            CargarColumnas();
        }

        private void btnImportarDesdeCSV_Click(object sender, EventArgs e)
        {
            if (_marcaActual == null) { MessageBox.Show("Seleccione una marca primero."); return; }

            using var dlg = new OpenFileDialog
            {
                Filter = "Archivos CSV (*.csv)|*.csv|Todos (*.*)|*.*",
                Title = "Seleccionar archivo CSV"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            var colsArchivo = CsvReader.LeerColumnas(dlg.FileName);
            ImportarColumnas(colsArchivo);
        }

        private void btnImportarDesdeExcel_Click(object sender, EventArgs e)
        {
            if (_marcaActual == null) { MessageBox.Show("Seleccione una marca primero."); return; }

            using var dlg = new OpenFileDialog
            {
                Filter = "Archivos Excel (*.xlsx;*.xls)|*.xlsx;*.xls|Todos (*.*)|*.*",
                Title = "Seleccionar archivo Excel"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                var colsArchivo = ExcelReader.LeerColumnas(dlg.FileName, (int)nudFilaEncabezado.Value);
                ImportarColumnas(colsArchivo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al leer el Excel:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ImportarColumnas(List<string> colsArchivo)
        {
            var colsBD = Repositorio.GetColumnasCSV(_marcaActual.Id).Select(c => c.IdColumnaArchivo).ToHashSet(StringComparer.OrdinalIgnoreCase);
            int nuevas = 0;
            foreach (var col in colsArchivo)
            {
                if (!colsBD.Contains(col))
                {
                    Repositorio.InsertColumnaCSV(new ColumnaCSV { IdMarcaTarjeta = _marcaActual.Id, IdColumnaArchivo = col });
                    nuevas++;
                }
            }
            CargarColumnas();
            MessageBox.Show($"Importación completa. {nuevas} columnas nuevas agregadas de {colsArchivo.Count} encontradas en el archivo.",
                "Importar columnas", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Validar columnas de un CSV contra la BD ───────────────────────

        private void btnValidarCSV_Click(object sender, EventArgs e)
        {
            if (_marcaActual == null) { MessageBox.Show("Seleccione una marca primero."); return; }

            using var dlg = new OpenFileDialog
            {
                Filter = "Archivos CSV (*.csv)|*.csv|Todos (*.*)|*.*",
                Title = "Seleccionar archivo CSV a validar"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            var colsArchivo = CsvReader.LeerColumnas(dlg.FileName).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var colsBD = Repositorio.GetColumnasCSV(_marcaActual.Id).Select(c => c.IdColumnaArchivo).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var soloEnArchivo = colsArchivo.Except(colsBD, StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();
            var soloEnBD = colsBD.Except(colsArchivo, StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();

            if (soloEnArchivo.Count == 0 && soloEnBD.Count == 0)
            {
                MessageBox.Show("✔ Las columnas del archivo coinciden exactamente con las de la base de datos.",
                    "Validación OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Mostrar diferencias en formulario dedicado
            using var frmDif = new FormDiferenciasColumnas(soloEnArchivo, soloEnBD, _marcaActual);
            frmDif.ShowDialog(this);
            CargarColumnas();
        }

        private void gridColumnas_CellEndEdit(object sender, GridViewCellEventArgs e)
        {
            if (gridColumnas.SelectedRows.Count == 0) return;
            var col = gridColumnas.SelectedRows[0].DataBoundItem as ColumnaCSV;
            if (col == null) return;
            Repositorio.UpdateColumnaCSV(col);
        }
    }
}
