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
