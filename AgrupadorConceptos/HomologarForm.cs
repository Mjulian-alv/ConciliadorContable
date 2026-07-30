using System;
using System.Linq;
using System.Windows.Forms;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Data;

namespace AgrupadorConceptos
{
    public partial class HomologarForm : Form
    {
        private int _idPerfilBanco;
        private string _valorOriginal;

        public bool HomologacionExitosa { get; private set; } = false;
        public string sConcepto { get; private set; } = "";
        public HomologarForm(int idPerfilBanco, string valorOriginal)
        {
            InitializeComponent();
            this.Icon = AppIcon.GetIcon();
            _idPerfilBanco = idPerfilBanco;
            _valorOriginal = valorOriginal;
            
            txtOriginal.Text = _valorOriginal;
            CargarConceptosEstandar();
        }

        private void CargarConceptosEstandar()
        {
            cmbEstandar.DataSource = HomologacionStorage.ObtenerConceptosEstandar();
            cmbEstandar.DisplayMember = "Nombre";
            cmbEstandar.ValueMember = "Id";
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            string conceptoEstandarTexto = cmbEstandar.Text.Trim();

            if (string.IsNullOrEmpty(conceptoEstandarTexto))
            {
                MessageBox.Show("Debe ingresar o seleccionar un Concepto Estándar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Si el valor es texto largo, el usuario pudo haber editado txtOriginal
                // para dejar solo la palabra clave que se va a buscar por substring.
                string valorClave = txtOriginal.Text.Trim();

                HomologacionStorage.Guardar(_idPerfilBanco, valorClave, conceptoEstandarTexto);

                HomologacionExitosa = true;
                sConcepto = conceptoEstandarTexto;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar homologación: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}