using System;
using System.Data;
using System.Windows.Forms;
using ArcaCliente.Models;
using ArcaCliente.Services;
using Telerik.WinControls.UI;

namespace ArcaCliente
{
    public partial class FormPerfilDetalle : RadForm
    {
        private readonly PerfilFiscal _perfil;
        private readonly bool _esNuevo;

        public PerfilFiscal Perfil => _perfil;

        public FormPerfilDetalle(PerfilFiscal perfil = null)
        {
            InitializeComponent();
            Icon = AppIcons.Arca;

            _esNuevo = perfil == null;
            _perfil = perfil ?? new PerfilFiscal 
            { 
                Nombre = "Nuevo perfil",
                IntegracionHabilitada = false,
                Sistema = SistemaIntegracion.Ninguno
            };

            ConfigurarComboSistema();
            CargarDatos();

            Text = _esNuevo ? "Nuevo Perfil Fiscal" : $"Editar Perfil: {_perfil.Nombre}";
        }

        private void ConfigurarComboSistema()
        {
            cmbSistema.Items.Clear();
            cmbSistema.Items.Add(new Telerik.WinControls.UI.RadListDataItem("Ninguno", SistemaIntegracion.Ninguno));
            cmbSistema.Items.Add(new Telerik.WinControls.UI.RadListDataItem("Octosis", SistemaIntegracion.Octosis));
        }

        private void CargarDatos()
        {
            txtNombre.Text = _perfil.Nombre;
            txtUsername.Text = _perfil.Username;
            txtPassword.Text = _perfil.Password;
            txtCuit.Text = _perfil.Cuit;
            chkIntegracionHabilitada.Checked = _perfil.IntegracionHabilitada;
            
            cmbSistema.SelectedValue = _perfil.Sistema;
            
            txtArcaApiUrl.Text = _perfil.ArcaApiUrl;
            txtConciliacionConnectionString.Text = _perfil.ConciliacionConnectionString;
            txtConciliacionQuery.Text = _perfil.ConciliacionQuery;
            txtOctosisConnectionString.Text = _perfil.OctosisConnectionString;
        }

        private void GuardarDatos()
        {
            _perfil.Nombre = txtNombre.Text.Trim();
            _perfil.Username = txtUsername.Text.Trim();
            _perfil.Password = txtPassword.Text;
            _perfil.Cuit = txtCuit.Text.Trim();
            _perfil.IntegracionHabilitada = chkIntegracionHabilitada.Checked;
            _perfil.Sistema = (SistemaIntegracion)(cmbSistema.SelectedValue ?? SistemaIntegracion.Ninguno);
            
            _perfil.ArcaApiUrl = string.IsNullOrWhiteSpace(txtArcaApiUrl.Text) 
                ? null 
                : txtArcaApiUrl.Text.Trim();
                
            _perfil.ConciliacionConnectionString = string.IsNullOrWhiteSpace(txtConciliacionConnectionString.Text)
                ? null
                : txtConciliacionConnectionString.Text.Trim();
                
            _perfil.ConciliacionQuery = string.IsNullOrWhiteSpace(txtConciliacionQuery.Text)
                ? null
                : txtConciliacionQuery.Text.Trim();
                
            _perfil.OctosisConnectionString = string.IsNullOrWhiteSpace(txtOctosisConnectionString.Text)
                ? null
                : txtOctosisConnectionString.Text.Trim();
        }

        private bool Validar()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Ingresá un nombre para el perfil.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Ingresá el CUIL (sin guiones).", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Ingresá la clave fiscal.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtCuit.Text))
            {
                MessageBox.Show("Ingresá el CUIT representada (con guiones, ej: 20-12345678-9).", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCuit.Focus();
                return false;
            }

            return true;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!Validar())
                return;

            GuardarDatos();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void btnTestConciliacion_Click(object sender, EventArgs e)
        {
            var connString = txtConciliacionConnectionString.Text.Trim();
            if (string.IsNullOrWhiteSpace(connString))
            {
                MessageBox.Show("Ingresá un Connection String para probar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            btnTestConciliacion.Enabled = false;
            btnTestConciliacion.Text = "Probando...";
            Cursor = Cursors.WaitCursor;

            try
            {
                var resultado = await Services.ConnectionTester.ProbarConciliacion(connString);
                if (resultado.Exitoso)
                    MessageBox.Show("✓ Conexión exitosa", "Test de Conexión",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show($"✗ Error al conectar:\n\n{resultado.Mensaje}", "Test de Conexión",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTestConciliacion.Enabled = true;
                btnTestConciliacion.Text = "Probar Conexión";
                Cursor = Cursors.Default;
            }
        }

        private async void btnTestOctosis_Click(object sender, EventArgs e)
        {
            var connString = txtOctosisConnectionString.Text.Trim();
            if (string.IsNullOrWhiteSpace(connString))
            {
                MessageBox.Show("Ingresá un Connection String para probar, o dejalo vacío para usar la configuración de conciliación.", 
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            btnTestOctosis.Enabled = false;
            btnTestOctosis.Text = "Probando...";
            Cursor = Cursors.WaitCursor;

            try
            {
                var resultado = await Services.ConnectionTester.ProbarOctosis(connString);
                if (resultado.Exitoso)
                    MessageBox.Show("✓ Conexión exitosa a Octosis", "Test de Conexión",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show($"✗ Error al conectar:\n\n{resultado.Mensaje}", "Test de Conexión",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTestOctosis.Enabled = true;
                btnTestOctosis.Text = "Probar Conexión";
                Cursor = Cursors.Default;
            }
        }
    }
}
