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
