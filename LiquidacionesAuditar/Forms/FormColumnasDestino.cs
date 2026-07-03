using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Telerik.WinControls.UI;
using LiquidacionesAuditar.Data;
using LiquidacionesAuditar.Models;

namespace LiquidacionesAuditar
{
    public partial class FormColumnasDestino : RadForm
    {
        private LiqCol _colActual;

        public FormColumnasDestino()
        {
            InitializeComponent();
            CargarMarcasCombo();
            CargarGrid();
        }

        private void CargarMarcasCombo()
        {
            var marcas = Repositorio.GetMarcas();
            cmbMarcaFiltro.DataSource = null;
            cmbMarcaFiltro.DataSource = marcas;
            cmbMarcaFiltro.DisplayMember = "Nombre";
            cmbMarcaFiltro.ValueMember = "Id";
        }

        private string MarcaActual =>
            cmbMarcaFiltro.SelectedValue?.ToString() ?? "";

        private void CargarGrid()
        {
            var cols = Repositorio.GetLiqCols(MarcaActual);
            gridDest.DataSource = null;
            gridDest.DataSource = cols;
            //gridDest.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.None;
            //try { gridDest.BestFitColumns(); } catch { }
            //BeginInvoke(new Action(() => { try { gridDest.BestFitColumns(); } catch { } }));
        }

        private void gridDest_SelectionChanged(object sender, EventArgs e)
        {
            if (gridDest.SelectedRows.Count == 0) { LimpiarFormulario(); return; }
            _colActual = gridDest.SelectedRows[0].DataBoundItem as LiqCol;
            if (_colActual == null) return;
            CargarFormulario(_colActual);
            CargarRelaciones();
        }

        private void CargarFormulario(LiqCol c)
        {
            txtIdColumna.Text = c.IdColumna;
            cmbTipoRegistro.SelectedValue = c.TipoRegistro;
            if (cmbTipoRegistro.SelectedIndex < 0)
                cmbTipoRegistro.Text = c.TipoRegistro;
            cmbTipoDato.SelectedValue = c.TipoDato;
            if (cmbTipoDato.SelectedIndex < 0)
                cmbTipoDato.Text = c.TipoDato;
            nudOrden.Value = c.Orden;
            txtValorFijo.Text = c.ValorFijo;
            txtCondicion.Text = c.Condicion;
            txtCondicionSigno.Text = c.CondicionSigno;
            ch_filtro.Checked = c.esFiltro;
        }

        private void LimpiarFormulario()
        {
            _colActual = null;
            txtIdColumna.Text = "";
            nudOrden.Value = 0;
            txtValorFijo.Text = "";
            txtCondicion.Text = "";
            txtCondicionSigno.Text = "";
            clbRelaciones.Items.Clear();
        }

        private void CargarRelaciones()
        {
            clbRelaciones.Items.Clear();
            if (_colActual == null || string.IsNullOrEmpty(MarcaActual)) return;

            var colsCSV = Repositorio.GetColumnasCSV(MarcaActual);
            var relacionadas = Repositorio.GetRelaciones(_colActual.Id)
                               .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var col in colsCSV)
                clbRelaciones.Items.Add(col.IdColumnaArchivo, relacionadas.Contains(col.IdColumnaArchivo));
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            _colActual = null;
            LimpiarFormulario();
            txtIdColumna.Focus();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIdColumna.Text))
            { MessageBox.Show("Ingrese el nombre de la columna.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (string.IsNullOrEmpty(MarcaActual))
            { MessageBox.Show("Seleccione una marca antes de guardar.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var col = new LiqCol
            {
                IdMarcaTarjeta = MarcaActual,
                IdColumna      = txtIdColumna.Text.Trim(),
                TipoRegistro   = cmbTipoRegistro.SelectedItem?.ToString() ?? "",
                TipoDato       = cmbTipoDato.SelectedItem?.ToString() ?? "",
                Orden          = (int)nudOrden.Value,
                ValorFijo      = txtValorFijo.Text?.Trim() ?? null,
                Condicion      = txtCondicion.Text?.Trim() ?? null,
                CondicionSigno = txtCondicionSigno.Text?.Trim() ?? null,
                tieneSigno = !string.IsNullOrEmpty(txtCondicionSigno.Text?.Trim()),
                esFiltro = ch_filtro.Checked
            };

            if (_colActual == null)
            {
                Repositorio.InsertLiqCol(col);
            }
            else
            {
                col.Id = _colActual.Id;
                Repositorio.UpdateLiqCol(col);
            }

            Repositorio.SetRelaciones(col.Id, clbRelaciones.Items.Cast<string>()
                .Where((item, index) => clbRelaciones.GetItemChecked(index)).ToList());

            CargarGrid();
            MessageBox.Show("Columna guardada correctamente.", "Guardar", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (_colActual == null) return;
            if (MessageBox.Show($"¿Eliminar la columna '{_colActual.IdColumna}' ({_colActual.TipoRegistro})?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            Repositorio.DeleteLiqCol(_colActual.Id);
            LimpiarFormulario();
            CargarGrid();
        }

        private void cmbMarcaFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            LimpiarFormulario();
            CargarGrid();
        }

        private void btnClonar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(MarcaActual))
            {
                MessageBox.Show("Seleccione la marca origen antes de clonar.", "Clonar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var marcas = Repositorio.GetMarcas()
                         .Where(m => m.Id != MarcaActual)
                         .ToList();

            if (marcas.Count == 0)
            {
                MessageBox.Show("No hay otras marcas disponibles.", "Clonar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dlg = new FormSeleccionarMarca(marcas);
            if (dlg.ShowDialog(this) != DialogResult.OK || dlg.MarcaSeleccionada == null) return;

            var destino = dlg.MarcaSeleccionada;
            var marcaOrigenNombre = Repositorio.GetMarcas().FirstOrDefault(m => m.Id == MarcaActual)?.Nombre ?? MarcaActual;

            if (MessageBox.Show(
                $"Se clonarán todas las columnas destino de '{marcaOrigenNombre}' a '{destino.Nombre}'.\n" +
                "Las columnas existentes en la marca destino serán reemplazadas.\n¿Continuar?",
                "Confirmar clonación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            Repositorio.ClonarLiqCols(MarcaActual, destino.Id);
            MessageBox.Show($"Columnas clonadas a '{destino.Nombre}' correctamente.", "Clonar", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
