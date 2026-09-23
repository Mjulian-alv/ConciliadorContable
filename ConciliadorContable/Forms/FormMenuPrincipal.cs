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
            // Fecha: 21/09/2026 - TAREA: 00041 - Linea: 3, 4 - Permiso de la conciliacion de percepciones y retenciones
            btnArcaPyR.Enabled             = u.TienePermiso("ArcaPyR");
            btnArcaPerfilesPyR.Enabled     = u.TienePermiso("ArcaPyR");
            btnAgrProcesador.Enabled       = u.TienePermiso("AgrProcesador");
            btnAgrHomologaciones.Enabled   = u.TienePermiso("AgrHomologaciones");
            btnAgrConciliacion.Enabled     = u.TienePermiso("AgrConciliacion");
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 1 - Permiso del catalogo de cuentas contables
            btnAgrCuentasContables.Enabled = u.TienePermiso("AgrCuentasContables");
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Permiso de la conciliacion interna
            btnAgrConciliacionInterna.Enabled = u.TienePermiso("AgrConciliacionInterna");
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

        // Fecha: 21/09/2026 - TAREA: 00041 - Linea: 3, 4 - Abrir la conciliacion de percepciones y retenciones (mismo circuito que Offline: elegir perfil y abrir)
        private void BtnArcaPyR_Click(object sender, EventArgs e)
        {
            var selector = new ArcaCliente.FormPerfilesPyR();
            if (selector.ShowDialog(this) != DialogResult.OK || selector.PerfilSeleccionado == null)
                return;
            AbrirVentana(new ArcaCliente.FormConciliacionPyR(selector.PerfilSeleccionado));
        }

        private void BtnArcaPerfilesPyR_Click(object sender, EventArgs e)
            => AbrirVentana(new ArcaCliente.FormPerfilesPyR());

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

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Abrir la conciliacion interna
        private void BtnAgrupadorConciliacionInterna_Click(object sender, EventArgs e)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            AgrupadorConceptos.Data.DatabaseHelper.InitializeDatabase();
            AbrirVentana(new AgrupadorConceptos.ConciliacionInternaForm());
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
