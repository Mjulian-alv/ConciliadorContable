using System;
using System.Windows.Forms;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;

namespace AgrupadorConceptos
{
    public partial class GestionHomologacionesForm : Form
    {
        public GestionHomologacionesForm()
        {
            InitializeComponent();
            this.Icon = AppIcon.GetIcon();
            this.Load += (s, e) => CargarDatos();
        }

        private void CargarDatos()
        {
            dgvHomologaciones.DataSource = null;
            dgvHomologaciones.DataSource = HomologacionStorage.ObtenerListado();
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvHomologaciones.CurrentRow?.DataBoundItem is HomologacionListado fila)
            {
                HomologacionStorage.Eliminar(fila.Id);
                CargarDatos();
            }
            else
            {
                MessageBox.Show("Seleccione una homologación para eliminar.");
            }
        }
    }
}
