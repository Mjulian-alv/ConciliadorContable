using System;
using System.Drawing;
using System.Windows.Forms;
using ConciliadorContable.Auth;
using Telerik.WinControls.UI;

namespace ConciliadorContable.Forms
{
    public partial class FormLogin : RadForm
    {
        public FormLogin()
        {
            InitializeComponent();
            txtPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnIngresar_Click(s, e); };
            txtUsuario.KeyDown  += (s, e) => { if (e.KeyCode == Keys.Enter) txtPassword.Focus(); };
        }

        private void BtnIngresar_Click(object sender, EventArgs e)
        {
            string user = txtUsuario.Text.Trim();
            string pass = txtPassword.Text;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                lblError.Text    = "Ingrese usuario y contraseña.";
                lblError.Visible = true;
                return;
            }

            if (AuthService.Login(user, pass))
            {
                DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();
            }
            else
            {
                lblError.Text    = "Usuario o contraseña incorrectos.";
                lblError.Visible = true;
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }
    }
}
