using System.Drawing;
using System.Windows.Forms;
using Telerik.WinControls.UI;

namespace ConciliadorContable.Forms
{
    partial class FormMenuPrincipal
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            // Header
            pnlHeader      = new RadPanel();
            lblTitulo      = new RadLabel();
            lblUsuario     = new RadLabel();
            btnSalir       = new RadButton();

            // Contenido
            pnlContenido   = new Panel();

            // Tarjeta ARCA
            pnlArca        = new Panel();
            lblArcaTitulo  = new RadLabel();
            lblArcaDesc    = new RadLabel();
            btnArcaOffline = new RadButton();
            btnArcaPerfiles    = new RadButton();
            btnArcaEquivalencias = new RadButton();
            // Fecha: 21/09/2026 - TAREA: 00041 - Linea: 3, 4 - Botones de la conciliacion de percepciones y retenciones
            btnArcaPyR         = new RadButton();
            btnArcaPerfilesPyR = new RadButton();

            // Tarjeta Agrupador
            pnlAgrupador       = new Panel();
            lblAgrTitulo       = new RadLabel();
            lblAgrDesc         = new RadLabel();
            btnAgrProcesador    = new RadButton();
            btnAgrHomologaciones = new RadButton();
            btnAgrConciliacion   = new RadButton();
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 1 - Boton nuevo: catalogo de cuentas contables
            btnAgrCuentasContables = new RadButton();
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Boton nuevo: conciliacion interna
            btnAgrConciliacionInterna = new RadButton();

            // Admin
            btnAdminUsuarios = new RadButton();

            ((System.ComponentModel.ISupportInitialize)pnlHeader).BeginInit();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lblTitulo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblUsuario).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnSalir).BeginInit();
            pnlContenido.SuspendLayout();
            pnlArca.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lblArcaTitulo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblArcaDesc).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnArcaOffline).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnArcaPerfiles).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnArcaEquivalencias).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnArcaPyR).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnArcaPerfilesPyR).BeginInit();
            pnlAgrupador.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lblAgrTitulo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblAgrDesc).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnAgrProcesador).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnAgrHomologaciones).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnAgrConciliacion).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnAgrCuentasContables).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnAgrConciliacionInterna).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnAdminUsuarios).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            SuspendLayout();

            // ── FormMenuPrincipal ────────────────────────────────────────
            Text        = "Conciliador Contable";
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 1 - Ventana mas alta para la 4ta fila de botones del Agrupador
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Ventana mas alta para la 5ta fila de botones del Agrupador
            ClientSize  = new Size(780, 620);
            MinimumSize = new Size(780, 620);
            StartPosition = FormStartPosition.CenterScreen;

            // ── pnlHeader ─────────────────────────────────────────────────
            pnlHeader.Dock      = DockStyle.Top;
            pnlHeader.Size      = new Size(780, 56);
            pnlHeader.BackColor = Color.FromArgb(30, 40, 55);
            pnlHeader.Controls.AddRange(new System.Windows.Forms.Control[] { lblTitulo, lblUsuario, btnSalir });

            lblTitulo.Text      = "🏢  Conciliador Contable";
            lblTitulo.Font      = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location  = new System.Drawing.Point(16, 14);
            lblTitulo.AutoSize  = true;

            lblUsuario.Font      = new Font("Segoe UI", 9F);
            lblUsuario.ForeColor = Color.FromArgb(180, 200, 220);
            lblUsuario.Location  = new System.Drawing.Point(310, 18);
            lblUsuario.AutoSize  = true;

            btnSalir.Text     = "Salir";
            btnSalir.Font     = new Font("Segoe UI", 9F);
            btnSalir.Location = new System.Drawing.Point(680, 13);
            btnSalir.Size     = new Size(80, 30);
            btnSalir.Click   += BtnSalir_Click;

            btnAdminUsuarios.Text     = "👥 Usuarios";
            btnAdminUsuarios.Font     = new Font("Segoe UI", 9F);
            btnAdminUsuarios.Location = new System.Drawing.Point(564, 13);
            btnAdminUsuarios.Size     = new Size(110, 30);
            btnAdminUsuarios.Visible  = false;
            btnAdminUsuarios.Click   += BtnAdminUsuarios_Click;

            pnlHeader.Controls.AddRange(new System.Windows.Forms.Control[] { lblTitulo, lblUsuario, btnAdminUsuarios, btnSalir });

            // ── pnlContenido ──────────────────────────────────────────────
            pnlContenido.Dock      = DockStyle.Fill;
            pnlContenido.BackColor = Color.FromArgb(240, 244, 248);
            pnlContenido.Padding   = new Padding(24);
            pnlContenido.Controls.AddRange(new System.Windows.Forms.Control[] { pnlArca, pnlAgrupador });

            // ── pnlArca ───────────────────────────────────────────────────
            pnlArca.BackColor   = Color.White;
            pnlArca.BorderStyle = BorderStyle.FixedSingle;
            pnlArca.Location    = new System.Drawing.Point(24, 24);
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 1 - Mas alto para que las dos tarjetas queden parejas con la de Agrupador
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Mas alto de nuevo para seguir parejas con la de Agrupador
            pnlArca.Size        = new Size(340, 490);
            pnlArca.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                lblArcaTitulo, lblArcaDesc,
                btnArcaOffline, btnArcaPerfiles, btnArcaEquivalencias, btnArcaPyR, btnArcaPerfilesPyR
            });

            lblArcaTitulo.Text      = "📋  ARCA Cliente";
            lblArcaTitulo.Font      = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblArcaTitulo.ForeColor = Color.FromArgb(30, 40, 55);
            lblArcaTitulo.Location  = new System.Drawing.Point(16, 18);
            lblArcaTitulo.AutoSize  = true;

            lblArcaDesc.Text      = "Gestión de comprobantes fiscales\noffline y conciliación con ARCA.";
            lblArcaDesc.Font      = new Font("Segoe UI", 8.5F);
            lblArcaDesc.ForeColor = Color.Gray;
            lblArcaDesc.Location  = new System.Drawing.Point(16, 52);
            lblArcaDesc.AutoSize  = true;

            ConfigurarBotonModulo(btnArcaOffline,       "Comprobantes Offline",    new System.Drawing.Point(16, 100), BtnArcaOffline_Click);
            ConfigurarBotonModulo(btnArcaPerfiles,      "Perfiles Offline",        new System.Drawing.Point(16, 160), BtnArcaPerfiles_Click);
            ConfigurarBotonModulo(btnArcaEquivalencias, "Equivalencias",           new System.Drawing.Point(16, 220), BtnArcaEquivalencias_Click);
            // Fecha: 21/09/2026 - TAREA: 00041 - Linea: 3, 4 - Conciliacion de percepciones y retenciones y sus perfiles
            ConfigurarBotonModulo(btnArcaPyR,           "Percepciones y Retenciones", new System.Drawing.Point(16, 280), BtnArcaPyR_Click);
            ConfigurarBotonModulo(btnArcaPerfilesPyR,   "Perfiles PyR",            new System.Drawing.Point(16, 340), BtnArcaPerfilesPyR_Click);

            // ── pnlAgrupador
            pnlAgrupador.BackColor   = Color.White;
            pnlAgrupador.BorderStyle = BorderStyle.FixedSingle;
            pnlAgrupador.Location    = new System.Drawing.Point(392, 24);
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 1 - Mas alto para la 4ta fila de botones (Cuentas Contables)
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Mas alto para la 5ta fila de botones (Conciliacion Interna)
            pnlAgrupador.Size        = new Size(340, 490);
            pnlAgrupador.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                lblAgrTitulo, lblAgrDesc, btnAgrProcesador, btnAgrHomologaciones, btnAgrConciliacion, btnAgrCuentasContables, btnAgrConciliacionInterna
            });

            lblAgrTitulo.Text      = "🔗  Agrupador de Conceptos";
            lblAgrTitulo.Font      = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblAgrTitulo.ForeColor = Color.FromArgb(30, 40, 55);
            lblAgrTitulo.Location  = new System.Drawing.Point(16, 18);
            lblAgrTitulo.AutoSize  = true;

            lblAgrDesc.Text      = "Procesamiento y homologación\nde movimientos bancarios.";
            lblAgrDesc.Font      = new Font("Segoe UI", 8.5F);
            lblAgrDesc.ForeColor = Color.Gray;
            lblAgrDesc.Location  = new System.Drawing.Point(16, 52);
            lblAgrDesc.AutoSize  = true;

            ConfigurarBotonModulo(btnAgrProcesador,     "Procesador de Archivos",     new System.Drawing.Point(16, 100), BtnAgrupadorProcesador_Click);
            ConfigurarBotonModulo(btnAgrHomologaciones, "Gestión de Homologaciones",  new System.Drawing.Point(16, 160), BtnAgrupadorHomologaciones_Click);
            ConfigurarBotonModulo(btnAgrConciliacion,   "Conciliación con Externo",   new System.Drawing.Point(16, 220), BtnAgrupadorConciliacion_Click);
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 1 - Fila nueva del boton de Cuentas Contables
            ConfigurarBotonModulo(btnAgrCuentasContables, "Cuentas Contables",        new System.Drawing.Point(16, 280), BtnAgrupadorCuentasContables_Click);
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Fila nueva del boton de Conciliacion Interna
            ConfigurarBotonModulo(btnAgrConciliacionInterna, "Conciliación Interna",  new System.Drawing.Point(16, 340), BtnAgrupadorConciliacionInterna_Click);

            // ── Ensamblar ────────────────────────────────────────────────
            Controls.Add(pnlContenido);
            Controls.Add(pnlHeader);

            ((System.ComponentModel.ISupportInitialize)pnlHeader).EndInit();
            pnlHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)lblTitulo).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblUsuario).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnSalir).EndInit();
            pnlContenido.ResumeLayout(false);
            pnlArca.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)lblArcaTitulo).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblArcaDesc).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnArcaOffline).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnArcaPerfiles).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnArcaEquivalencias).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnArcaPyR).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnArcaPerfilesPyR).EndInit();
            pnlAgrupador.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)lblAgrTitulo).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblAgrDesc).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnAgrProcesador).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnAgrHomologaciones).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnAgrConciliacion).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnAgrCuentasContables).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnAgrConciliacionInterna).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnAdminUsuarios).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        private static void ConfigurarBotonModulo(RadButton btn, string texto,
            System.Drawing.Point location, System.EventHandler handler)
        {
            btn.Text     = texto;
            btn.Font     = new Font("Segoe UI", 9.5F);
            btn.Location = location;
            btn.Size     = new Size(308, 44);
            btn.Click   += handler;
        }

        private RadPanel  pnlHeader;
        private RadLabel  lblTitulo;
        private RadLabel  lblUsuario;
        private RadButton btnSalir;
        private Panel     pnlContenido;

        private Panel     pnlArca;
        private RadLabel  lblArcaTitulo;
        private RadLabel  lblArcaDesc;
        private RadButton btnArcaOffline;
        private RadButton btnArcaPerfiles;
        private RadButton btnArcaEquivalencias;
        private RadButton btnArcaPyR;
        private RadButton btnArcaPerfilesPyR;

        private Panel     pnlAgrupador;
        private RadLabel  lblAgrTitulo;
        private RadLabel  lblAgrDesc;
        private RadButton btnAgrProcesador;
        private RadButton btnAgrHomologaciones;
        private RadButton btnAgrConciliacion;
        private RadButton btnAgrCuentasContables;
        private RadButton btnAgrConciliacionInterna;
        private RadButton btnAdminUsuarios;
    }
}
