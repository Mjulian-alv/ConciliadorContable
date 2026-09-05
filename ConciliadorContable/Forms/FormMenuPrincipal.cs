using System;
using System.IO;
using System.Windows.Forms;
using ConciliadorContable.Auth;
using ConciliadorContable.Data;

namespace ConciliadorContable.Forms
{
    public partial class FormMenuPrincipal : Telerik.WinControls.UI.RadForm
    {
        public FormMenuPrincipal()
        {
            InitializeComponent();
            lblUsuario.Text = $"👤  {AuthService.UsuarioActual?.Nombre ?? AuthService.UsuarioActual?.Username}";

            AplicarPermisos();
        }

        private void AplicarPermisos()
        {
            var u = AuthService.UsuarioActual;
            if (u == null) return;

            btnAdminUsuarios.Visible       = u.Rol == "Admin";
            btnArcaOffline.Enabled         = u.TienePermiso("ArcaOffline");
            btnArcaPerfiles.Enabled        = u.TienePermiso("ArcaPerfiles");
            btnArcaEquivalencias.Enabled   = u.TienePermiso("ArcaEquivalencias");
            btnAgrProcesador.Enabled       = u.TienePermiso("AgrProcesador");
            btnAgrHomologaciones.Enabled   = u.TienePermiso("AgrHomologaciones");
            btnAgrConciliacion.Enabled     = u.TienePermiso("AgrConciliacion");
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 1 - Permiso del catalogo de cuentas contables
            btnAgrCuentasContables.Enabled = u.TienePermiso("AgrCuentasContables");
        }

        // ── ARCA Cliente ──────────────────────────────────────────────────

        private void BtnArcaOffline_Click(object sender, EventArgs e)
        {
            var selector = new ArcaCliente.FormPerfilesOffline();
            if (selector.ShowDialog(this) != DialogResult.OK || selector.PerfilSeleccionado == null)
                return;
            AbrirVentana(new ArcaCliente.FormComprobantesOffline(selector.PerfilSeleccionado));
        }

        private void BtnArcaPerfiles_Click(object sender, EventArgs e)
            => AbrirVentana(new ArcaCliente.FormPerfilesOffline());

        private void BtnArcaEquivalencias_Click(object sender, EventArgs e)
            => AbrirVentana(new ArcaCliente.FormEquivalencias());

        // ── Agrupador

        private void BtnAgrupadorProcesador_Click(object sender, EventArgs e)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            AgrupadorConceptos.Data.DatabaseHelper.InitializeDatabase();
            AbrirVentana(new AgrupadorConceptos.ProcesadorForm());
        }

        private void BtnAgrupadorHomologaciones_Click(object sender, EventArgs e)
        {
            AgrupadorConceptos.Data.DatabaseHelper.InitializeDatabase();
            AbrirVentana(new AgrupadorConceptos.GestionHomologacionesForm());
        }

        private void BtnAgrupadorConciliacion_Click(object sender, EventArgs e)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            AgrupadorConceptos.Data.DatabaseHelper.InitializeDatabase();
            AbrirVentana(new AgrupadorConceptos.ConciliacionExternForm());
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 1 - Abrir el catalogo de cuentas contables
        private void BtnAgrupadorCuentasContables_Click(object sender, EventArgs e)
        {
            AgrupadorConceptos.Data.DatabaseHelper.InitializeDatabase();
            AbrirVentana(new AgrupadorConceptos.CuentasContablesForm());
        }

        // ── Salir

        private void BtnAdminUsuarios_Click(object sender, EventArgs e)
            => AbrirVentana(new FormAdminUsuarios());

        private void BtnSalir_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Cerrar sesión y salir?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                AuthService.Logout();
                Application.Exit();
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private void AbrirVentana(Form form)
        {
            Hide();
            form.Show();
            form.FormClosed += (s, _) => Show();
        }
    }
}
