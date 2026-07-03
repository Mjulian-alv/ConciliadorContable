using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public FormColumnasDestino()
        {
            InitializeComponent();
            CargarMarcasCombo();
            CargarGrid();
            ConfigurarGridRelaciones();
        }

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
            _relacionesVM = new BindingList<RelacionVM>();
            gridRelaciones.DataSource = _relacionesVM;
        }

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

            gridRelaciones.EndEdit();
            var relaciones = _relacionesVM
                .Where(v => v.Sel)
                .Select(v => new RelacionCol { IdColumnasCSV = v.Columna, Signo = v.Signo })
                .ToList();
            Repositorio.SetRelaciones(col.Id, relaciones);

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
