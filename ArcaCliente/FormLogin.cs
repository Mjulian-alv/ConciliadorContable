using System;
using System.Windows.Forms;
using ArcaCliente.Models;
using ArcaCliente.Services;

namespace ArcaCliente
{
    public partial class FormLogin : Telerik.WinControls.UI.RadForm
    {
        public FormLogin()
        {
            InitializeComponent();
            Icon = AppIcons.Arca;
            txtLoginPassword.TextBoxElement.PasswordChar = '*';
            txtRegPassword.TextBoxElement.PasswordChar = '*';
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            if (!ValidarLogin()) return;
            SetLoading(true);
            try
            {
                var request = new LoginRequest
                {
                    Email = txtLoginEmail.Text.Trim(),
                    Password = txtLoginPassword.Text,
                    RequestsLimit = (int)spnLoginLimite.Value > 0
                        ? (int?)(int)spnLoginLimite.Value
                        : null
                };
                var response = await AppServices.Api.LoginAsync(request);
                AppServices.SaveToken(response, request.Email);
                AbrirMenu();
            }
            catch (ArcaApiException ex)
            {
                MessageBox.Show(ex.UserFriendlyMessage, "Error al iniciar sesión",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetLoading(false);
            }
        }

        private async void btnRegistrar_Click(object sender, EventArgs e)
        {
            if (!ValidarRegistro()) return;
            SetLoading(true);
            try
            {
                var request = new RegisterRequest
                {
                    Email = txtRegEmail.Text.Trim(),
                    Password = txtRegPassword.Text,
                    CompanyName = txtRegEmpresa.Text.Trim(),
                    RequestsLimit = (int)spnRegLimite.Value > 0
                        ? (int?)(int)spnRegLimite.Value
                        : null
                };
                var response = await AppServices.Api.RegisterAsync(request);
                AppServices.SaveToken(response, request.Email);
                AbrirMenu();
            }
            catch (ArcaApiException ex)
            {
                MessageBox.Show(ex.UserFriendlyMessage, "Error al registrarse",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetLoading(false);
            }
        }

        private bool ValidarLogin()
        {
            if (string.IsNullOrWhiteSpace(txtLoginEmail.Text))
            {
                MessageBox.Show("Ingresá tu email.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtLoginEmail.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtLoginPassword.Text))
            {
                MessageBox.Show("Ingresá tu contraseña.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtLoginPassword.Focus();
                return false;
            }
            return true;
        }

        private bool ValidarRegistro()
        {
            if (string.IsNullOrWhiteSpace(txtRegEmail.Text))
            {
                MessageBox.Show("Ingresá tu email.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtRegEmail.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtRegPassword.Text))
            {
                MessageBox.Show("Ingresá una contraseña.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtRegPassword.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtRegEmpresa.Text))
            {
                MessageBox.Show("Ingresá el nombre de la empresa.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtRegEmpresa.Focus();
                return false;
            }
            return true;
        }

        private void SetLoading(bool loading)
        {
            btnLogin.Enabled = !loading;
            btnRegistrar.Enabled = !loading;
            pageViewLogin.Enabled = !loading;
            Cursor = loading ? Cursors.WaitCursor : Cursors.Default;
        }

        private void AbrirMenu()
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
