using System;
using System.Linq;
using System.Windows.Forms;
using Dapper;
using AgrupadorConceptos.Data;
using Telerik.WinControls.UI;

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
            using (var cn = DatabaseHelper.GetConnection())
            {
                var data = cn.Query(@"
                    SELECT h.Id, p.NombreBanco as Banco, h.ValorOriginal, c.Nombre as ConceptoEstandar
                    FROM HomologacionConceptos h
                    JOIN PerfilesBanco p ON h.IdPerfilBanco = p.Id
                    JOIN ConceptosEstandar c ON h.IdConceptoEstandar = c.Id
                ").ToList();
                dgvHomologaciones.DataSource = null;
                dgvHomologaciones.DataSource = data;
            }
        }
        
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvHomologaciones.CurrentRow != null)
            {
                var rowData = (dynamic)dgvHomologaciones.CurrentRow.DataBoundItem;
                int id = (int)rowData.Id;
                using (var cn = DatabaseHelper.GetConnection())
                {
                    cn.Execute("DELETE FROM HomologacionConceptos WHERE Id = @Id", new { Id = id });
                }
                CargarDatos();
            }
            else
            {
                MessageBox.Show("Seleccione una homologación para eliminar.");
            }
        }
    }
}