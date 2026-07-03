using System.Drawing;
using System.Windows.Forms;
using Telerik.WinControls.UI;

namespace AgrupadorConceptos
{
    partial class ConciliacionExternForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) { if (disposing) components?.Dispose(); base.Dispose(disposing); }

        private void InitializeComponent()
        {
            pnlSesiones = new Panel();
            lblSesionesTitle = new Label();
            lbSesiones = new ListBox();
            btnNuevaSesion = new Button();
            btnRetomar = new Button();
            btnEliminarSesion = new Button();
            lblSesionActiva = new Label();
            pnlConfigNueva = new Panel();
            lblArchivo = new Label();
            clbArchivos = new CheckedListBox();
            lblConceptos = new Label();
            clbConceptos = new CheckedListBox();
            lblArchivoExterno = new Label();
            txtArchivoExterno = new TextBox();
            btnCargarExterno = new Button();
            btnConfirmarNueva = new Button();
            btnCancelarNueva = new Button();
            pnlAcciones = new Panel();
            btnAutoConciliar = new Button();
            btnConciliarManual = new Button();
            btnDesconciliar = new Button();
            btnFinalizar = new Button();
            btnExportar = new Button();
            lblContadorPendExt = new Label();
            lblContadorPendExtr = new Label();
            lblContadorConcil = new Label();
            tabControl = new TabControl();
            tabPendientes = new TabPage();
            splitPendientes = new SplitContainer();
            pnlPendExt = new Panel();
            dgvExternoPendiente = new RadGridView();
            lblPendExt = new Label();
            tabsPendExtracto = new TabControl();
            tabConConcepto = new TabPage();
            dgvExtractoPendiente = new RadGridView();
            tabSinConcepto = new TabPage();
            dgvExtractoSinConcepto = new RadGridView();
            tabConciliados = new TabPage();
            dgvConciliados = new RadGridView();
            pnlSesiones.SuspendLayout();
            pnlConfigNueva.SuspendLayout();
            pnlAcciones.SuspendLayout();
            tabControl.SuspendLayout();
            tabPendientes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitPendientes).BeginInit();
            splitPendientes.Panel1.SuspendLayout();
            splitPendientes.Panel2.SuspendLayout();
            splitPendientes.SuspendLayout();
            pnlPendExt.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvExternoPendiente).BeginInit();
            tabsPendExtracto.SuspendLayout();
            tabConConcepto.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvExtractoPendiente).BeginInit();
            tabSinConcepto.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvExtractoSinConcepto).BeginInit();
            tabConciliados.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvConciliados).BeginInit();
            SuspendLayout();
            // 
            // pnlSesiones
            // 
            pnlSesiones.BackColor = Color.FromArgb(240, 244, 248);
            pnlSesiones.Controls.Add(lblSesionesTitle);
            pnlSesiones.Controls.Add(lbSesiones);
            pnlSesiones.Controls.Add(btnNuevaSesion);
            pnlSesiones.Controls.Add(btnRetomar);
            pnlSesiones.Controls.Add(btnEliminarSesion);
            pnlSesiones.Controls.Add(lblSesionActiva);
            pnlSesiones.Dock = DockStyle.Top;
            pnlSesiones.Location = new Point(0, 0);
            pnlSesiones.Name = "pnlSesiones";
            pnlSesiones.Padding = new Padding(10, 8, 10, 4);
            pnlSesiones.Size = new Size(1200, 120);
            pnlSesiones.TabIndex = 3;
            // 
            // lblSesionesTitle
            // 
            lblSesionesTitle.AutoSize = true;
            lblSesionesTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblSesionesTitle.Location = new Point(10, 10);
            lblSesionesTitle.Name = "lblSesionesTitle";
            lblSesionesTitle.Size = new Size(58, 15);
            lblSesionesTitle.TabIndex = 0;
            lblSesionesTitle.Text = "Sesiones:";
            // 
            // lbSesiones
            // 
            lbSesiones.DisplayMember = "DisplayName";
            lbSesiones.ItemHeight = 15;
            lbSesiones.Location = new Point(100, 7);
            lbSesiones.Name = "lbSesiones";
            lbSesiones.Size = new Size(700, 64);
            lbSesiones.TabIndex = 1;
            lbSesiones.ValueMember = "Id";
            lbSesiones.SelectedIndexChanged += lbSesiones_SelectedIndexChanged;
            // 
            // btnNuevaSesion
            // 
            btnNuevaSesion.Location = new Point(815, 7);
            btnNuevaSesion.Name = "btnNuevaSesion";
            btnNuevaSesion.Size = new Size(100, 28);
            btnNuevaSesion.TabIndex = 2;
            btnNuevaSesion.Text = "➕ Nueva";
            btnNuevaSesion.Click += btnNuevaSesion_Click;
            // 
            // btnRetomar
            // 
            btnRetomar.Enabled = false;
            btnRetomar.Location = new Point(815, 40);
            btnRetomar.Name = "btnRetomar";
            btnRetomar.Size = new Size(100, 28);
            btnRetomar.TabIndex = 3;
            btnRetomar.Text = "▶ Retomar";
            btnRetomar.Click += btnRetomar_Click;
            // 
            // btnEliminarSesion
            // 
            btnEliminarSesion.BackColor = Color.MistyRose;
            btnEliminarSesion.Enabled = false;
            btnEliminarSesion.Location = new Point(815, 73);
            btnEliminarSesion.Name = "btnEliminarSesion";
            btnEliminarSesion.Size = new Size(100, 28);
            btnEliminarSesion.TabIndex = 4;
            btnEliminarSesion.Text = "🗑 Eliminar";
            btnEliminarSesion.UseVisualStyleBackColor = false;
            btnEliminarSesion.Click += btnEliminarSesion_Click;
            // 
            // lblSesionActiva
            // 
            lblSesionActiva.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblSesionActiva.ForeColor = Color.DarkSlateBlue;
            lblSesionActiva.Location = new Point(930, 10);
            lblSesionActiva.Name = "lblSesionActiva";
            lblSesionActiva.Size = new Size(250, 40);
            lblSesionActiva.TabIndex = 5;
            lblSesionActiva.Text = "Sin sesión activa";
            // 
            // pnlConfigNueva
            // 
            pnlConfigNueva.BackColor = Color.FromArgb(255, 252, 230);
            pnlConfigNueva.Controls.Add(lblArchivo);
            pnlConfigNueva.Controls.Add(clbArchivos);
            pnlConfigNueva.Controls.Add(lblConceptos);
            pnlConfigNueva.Controls.Add(clbConceptos);
            pnlConfigNueva.Controls.Add(lblArchivoExterno);
            pnlConfigNueva.Controls.Add(txtArchivoExterno);
            pnlConfigNueva.Controls.Add(btnCargarExterno);
            pnlConfigNueva.Controls.Add(btnConfirmarNueva);
            pnlConfigNueva.Controls.Add(btnCancelarNueva);
            pnlConfigNueva.Dock = DockStyle.Top;
            pnlConfigNueva.Location = new Point(0, 120);
            pnlConfigNueva.Name = "pnlConfigNueva";
            pnlConfigNueva.Padding = new Padding(10, 8, 10, 4);
            pnlConfigNueva.Size = new Size(1200, 165);
            pnlConfigNueva.TabIndex = 2;
            pnlConfigNueva.Visible = false;
            // 
            // lblArchivo
            // 
            lblArchivo.AutoSize = true;
            lblArchivo.Location = new Point(10, 10);
            lblArchivo.Name = "lblArchivo";
            lblArchivo.Size = new Size(120, 15);
            lblArchivo.TabIndex = 0;
            lblArchivo.Text = "Archivos importados:";
            // 
            // clbArchivos
            // 
            clbArchivos.CheckOnClick = true;
            clbArchivos.Location = new Point(160, 7);
            clbArchivos.Name = "clbArchivos";
            clbArchivos.Size = new Size(380, 76);
            clbArchivos.TabIndex = 1;
            clbArchivos.ItemCheck += clbArchivos_ItemCheck;
            // 
            // lblConceptos
            // 
            lblConceptos.AutoSize = true;
            lblConceptos.Location = new Point(10, 98);
            lblConceptos.Name = "lblConceptos";
            lblConceptos.Size = new Size(67, 15);
            lblConceptos.TabIndex = 2;
            lblConceptos.Text = "Conceptos:";
            // 
            // clbConceptos
            // 
            clbConceptos.CheckOnClick = true;
            clbConceptos.Location = new Point(160, 95);
            clbConceptos.Name = "clbConceptos";
            clbConceptos.Size = new Size(380, 58);
            clbConceptos.TabIndex = 3;
            // 
            // lblArchivoExterno
            // 
            lblArchivoExterno.AutoSize = true;
            lblArchivoExterno.Location = new Point(560, 10);
            lblArchivoExterno.Name = "lblArchivoExterno";
            lblArchivoExterno.Size = new Size(93, 15);
            lblArchivoExterno.TabIndex = 4;
            lblArchivoExterno.Text = "Archivo externo:";
            // 
            // txtArchivoExterno
            // 
            txtArchivoExterno.Location = new Point(680, 7);
            txtArchivoExterno.Name = "txtArchivoExterno";
            txtArchivoExterno.ReadOnly = true;
            txtArchivoExterno.Size = new Size(360, 23);
            txtArchivoExterno.TabIndex = 5;
            // 
            // btnCargarExterno
            // 
            btnCargarExterno.Location = new Point(1048, 6);
            btnCargarExterno.Name = "btnCargarExterno";
            btnCargarExterno.Size = new Size(36, 23);
            btnCargarExterno.TabIndex = 6;
            btnCargarExterno.Text = "...";
            btnCargarExterno.Click += btnCargarExterno_Click;
            // 
            // btnConfirmarNueva
            // 
            btnConfirmarNueva.BackColor = Color.LightGreen;
            btnConfirmarNueva.Location = new Point(680, 40);
            btnConfirmarNueva.Name = "btnConfirmarNueva";
            btnConfirmarNueva.Size = new Size(110, 28);
            btnConfirmarNueva.TabIndex = 7;
            btnConfirmarNueva.Text = "✔ Confirmar";
            btnConfirmarNueva.UseVisualStyleBackColor = false;
            btnConfirmarNueva.Click += btnConfirmarNueva_Click;
            // 
            // btnCancelarNueva
            // 
            btnCancelarNueva.BackColor = Color.MistyRose;
            btnCancelarNueva.Location = new Point(800, 40);
            btnCancelarNueva.Name = "btnCancelarNueva";
            btnCancelarNueva.Size = new Size(110, 28);
            btnCancelarNueva.TabIndex = 8;
            btnCancelarNueva.Text = "✖ Cancelar";
            btnCancelarNueva.UseVisualStyleBackColor = false;
            btnCancelarNueva.Click += btnCancelarNueva_Click;
            // 
            // pnlAcciones
            // 
            pnlAcciones.BackColor = Color.FromArgb(230, 235, 240);
            pnlAcciones.Controls.Add(btnAutoConciliar);
            pnlAcciones.Controls.Add(btnConciliarManual);
            pnlAcciones.Controls.Add(btnDesconciliar);
            pnlAcciones.Controls.Add(btnFinalizar);
            pnlAcciones.Controls.Add(btnExportar);
            pnlAcciones.Controls.Add(lblContadorPendExt);
            pnlAcciones.Controls.Add(lblContadorPendExtr);
            pnlAcciones.Controls.Add(lblContadorConcil);
            pnlAcciones.Dock = DockStyle.Bottom;
            pnlAcciones.Location = new Point(0, 708);
            pnlAcciones.Name = "pnlAcciones";
            pnlAcciones.Padding = new Padding(6, 6, 6, 0);
            pnlAcciones.Size = new Size(1200, 42);
            pnlAcciones.TabIndex = 1;
            // 
            // btnAutoConciliar
            // 
            btnAutoConciliar.Enabled = false;
            btnAutoConciliar.Location = new Point(6, 6);
            btnAutoConciliar.Name = "btnAutoConciliar";
            btnAutoConciliar.Size = new Size(155, 28);
            btnAutoConciliar.TabIndex = 0;
            btnAutoConciliar.Text = "⚡ Auto-conciliar";
            btnAutoConciliar.Click += btnAutoConciliar_Click;
            // 
            // btnConciliarManual
            // 
            btnConciliarManual.Enabled = false;
            btnConciliarManual.Location = new Point(166, 6);
            btnConciliarManual.Name = "btnConciliarManual";
            btnConciliarManual.Size = new Size(155, 28);
            btnConciliarManual.TabIndex = 1;
            btnConciliarManual.Text = "✔ Conciliar manual";
            btnConciliarManual.Click += btnConciliarManual_Click;
            // 
            // btnDesconciliar
            // 
            btnDesconciliar.Enabled = false;
            btnDesconciliar.Location = new Point(326, 6);
            btnDesconciliar.Name = "btnDesconciliar";
            btnDesconciliar.Size = new Size(155, 28);
            btnDesconciliar.TabIndex = 2;
            btnDesconciliar.Text = "↩ Desconciliar";
            btnDesconciliar.Click += btnDesconciliar_Click;
            // 
            // btnFinalizar
            // 
            btnFinalizar.Enabled = false;
            btnFinalizar.Location = new Point(486, 6);
            btnFinalizar.Name = "btnFinalizar";
            btnFinalizar.Size = new Size(155, 28);
            btnFinalizar.TabIndex = 3;
            btnFinalizar.Text = "🏁 Finalizar";
            btnFinalizar.Click += btnFinalizar_Click;
            // 
            // btnExportar
            // 
            btnExportar.Enabled = false;
            btnExportar.Location = new Point(646, 6);
            btnExportar.Name = "btnExportar";
            btnExportar.Size = new Size(155, 28);
            btnExportar.TabIndex = 4;
            btnExportar.Text = "📤 Exportar Excel";
            btnExportar.Click += btnExportar_Click;
            // 
            // lblContadorPendExt
            // 
            lblContadorPendExt.AutoSize = true;
            lblContadorPendExt.ForeColor = Color.DarkRed;
            lblContadorPendExt.Location = new Point(860, 10);
            lblContadorPendExt.Name = "lblContadorPendExt";
            lblContadorPendExt.Size = new Size(0, 15);
            lblContadorPendExt.TabIndex = 5;
            // 
            // lblContadorPendExtr
            // 
            lblContadorPendExtr.AutoSize = true;
            lblContadorPendExtr.ForeColor = Color.DarkOrange;
            lblContadorPendExtr.Location = new Point(1000, 10);
            lblContadorPendExtr.Name = "lblContadorPendExtr";
            lblContadorPendExtr.Size = new Size(0, 15);
            lblContadorPendExtr.TabIndex = 6;
            // 
            // lblContadorConcil
            // 
            lblContadorConcil.AutoSize = true;
            lblContadorConcil.ForeColor = Color.DarkGreen;
            lblContadorConcil.Location = new Point(1130, 10);
            lblContadorConcil.Name = "lblContadorConcil";
            lblContadorConcil.Size = new Size(0, 15);
            lblContadorConcil.TabIndex = 7;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabPendientes);
            tabControl.Controls.Add(tabConciliados);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 285);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(1200, 423);
            tabControl.TabIndex = 0;
            // 
            // tabPendientes
            // 
            tabPendientes.Controls.Add(splitPendientes);
            tabPendientes.Location = new Point(4, 24);
            tabPendientes.Name = "tabPendientes";
            tabPendientes.Size = new Size(1192, 395);
            tabPendientes.TabIndex = 0;
            tabPendientes.Text = "⏳ Pendientes";
            // 
            // splitPendientes
            // 
            splitPendientes.Dock = DockStyle.Fill;
            splitPendientes.Location = new Point(0, 0);
            splitPendientes.Name = "splitPendientes";
            // 
            // splitPendientes.Panel1
            // 
            splitPendientes.Panel1.Controls.Add(pnlPendExt);
            // 
            // splitPendientes.Panel2
            // 
            splitPendientes.Panel2.Controls.Add(tabsPendExtracto);
            splitPendientes.Size = new Size(1192, 395);
            splitPendientes.SplitterDistance = 961;
            splitPendientes.TabIndex = 0;
            // 
            // pnlPendExt
            // 
            pnlPendExt.Controls.Add(dgvExternoPendiente);
            pnlPendExt.Controls.Add(lblPendExt);
            pnlPendExt.Dock = DockStyle.Fill;
            pnlPendExt.Location = new Point(0, 0);
            pnlPendExt.Name = "pnlPendExt";
            pnlPendExt.Size = new Size(961, 395);
            pnlPendExt.TabIndex = 0;
            // 
            // dgvExternoPendiente
            // 
            dgvExternoPendiente.Dock = DockStyle.Fill;
            dgvExternoPendiente.Location = new Point(0, 20);
            dgvExternoPendiente.Name = "dgvExternoPendiente";
            dgvExternoPendiente.Size = new Size(961, 375);
            dgvExternoPendiente.TabIndex = 0;
            dgvExternoPendiente.ReadOnly = true;
            dgvExternoPendiente.MultiSelect = false;
            dgvExternoPendiente.SelectionChanged += dgvExternoPendiente_SelectionChanged;
            // 
            // lblPendExt
            // 
            lblPendExt.Dock = DockStyle.Top;
            lblPendExt.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPendExt.ForeColor = Color.DarkSlateBlue;
            lblPendExt.Location = new Point(0, 0);
            lblPendExt.Name = "lblPendExt";
            lblPendExt.Size = new Size(961, 20);
            lblPendExt.TabIndex = 1;
            lblPendExt.Text = "Archivo externo — pendientes";
            // 
            // tabsPendExtracto
            // 
            tabsPendExtracto.Controls.Add(tabConConcepto);
            tabsPendExtracto.Controls.Add(tabSinConcepto);
            tabsPendExtracto.Dock = DockStyle.Fill;
            tabsPendExtracto.Location = new Point(0, 0);
            tabsPendExtracto.Name = "tabsPendExtracto";
            tabsPendExtracto.SelectedIndex = 0;
            tabsPendExtracto.Size = new Size(227, 395);
            tabsPendExtracto.TabIndex = 0;
            // 
            // tabConConcepto
            // 
            tabConConcepto.Controls.Add(dgvExtractoPendiente);
            tabConConcepto.Location = new Point(4, 24);
            tabConConcepto.Name = "tabConConcepto";
            tabConConcepto.Size = new Size(219, 367);
            tabConConcepto.TabIndex = 0;
            tabConConcepto.Text = "Con concepto seleccionado";
            // 
            // dgvExtractoPendiente
            // 
            dgvExtractoPendiente.Dock = DockStyle.Fill;
            dgvExtractoPendiente.Location = new Point(0, 0);
            dgvExtractoPendiente.Name = "dgvExtractoPendiente";
            dgvExtractoPendiente.Size = new Size(219, 367);
            dgvExtractoPendiente.TabIndex = 0;
            dgvExtractoPendiente.ReadOnly = true;
            dgvExtractoPendiente.MultiSelect = false;
            // 
            // tabSinConcepto
            // 
            tabSinConcepto.Controls.Add(dgvExtractoSinConcepto);
            tabSinConcepto.Location = new Point(4, 24);
            tabSinConcepto.Name = "tabSinConcepto";
            tabSinConcepto.Size = new Size(219, 367);
            tabSinConcepto.TabIndex = 1;
            tabSinConcepto.Text = "Resto del extracto";
            // 
            // dgvExtractoSinConcepto
            // 
            dgvExtractoSinConcepto.Dock = DockStyle.Fill;
            dgvExtractoSinConcepto.Location = new Point(0, 0);
            dgvExtractoSinConcepto.Name = "dgvExtractoSinConcepto";
            dgvExtractoSinConcepto.Size = new Size(219, 367);
            dgvExtractoSinConcepto.TabIndex = 0;
            dgvExtractoSinConcepto.ReadOnly = true;
            dgvExtractoSinConcepto.MultiSelect = false;
            // 
            // tabConciliados
            // 
            tabConciliados.Controls.Add(dgvConciliados);
            tabConciliados.Location = new Point(4, 24);
            tabConciliados.Name = "tabConciliados";
            tabConciliados.Size = new Size(1192, 395);
            tabConciliados.TabIndex = 1;
            tabConciliados.Text = "✅ Conciliados";
            // 
            // dgvConciliados
            // 
            dgvConciliados.Dock = DockStyle.Fill;
            dgvConciliados.Location = new Point(0, 0);
            dgvConciliados.Name = "dgvConciliados";
            dgvConciliados.Size = new Size(1192, 395);
            dgvConciliados.TabIndex = 0;
            dgvConciliados.ReadOnly = true;
            dgvConciliados.MultiSelect = false;
            // 
            // ConciliacionExternForm
            // 
            ClientSize = new Size(1200, 750);
            Controls.Add(tabControl);
            Controls.Add(pnlAcciones);
            Controls.Add(pnlConfigNueva);
            Controls.Add(pnlSesiones);
            MinimumSize = new Size(1000, 650);
            Name = "ConciliacionExternForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Conciliación con Archivo Externo";
            pnlSesiones.ResumeLayout(false);
            pnlSesiones.PerformLayout();
            pnlConfigNueva.ResumeLayout(false);
            pnlConfigNueva.PerformLayout();
            pnlAcciones.ResumeLayout(false);
            pnlAcciones.PerformLayout();
            tabControl.ResumeLayout(false);
            tabPendientes.ResumeLayout(false);
            splitPendientes.Panel1.ResumeLayout(false);
            splitPendientes.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitPendientes).EndInit();
            splitPendientes.ResumeLayout(false);
            pnlPendExt.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvExternoPendiente).EndInit();
            tabsPendExtracto.ResumeLayout(false);
            tabConConcepto.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvExtractoPendiente).EndInit();
            tabSinConcepto.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvExtractoSinConcepto).EndInit();
            tabConciliados.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvConciliados).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvExternoPendiente.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvExtractoPendiente.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvExtractoSinConcepto.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvConciliados.MasterTemplate).EndInit();
            ResumeLayout(false);
        }

        private static void ConfigBtn(Button btn, string texto, Point loc, System.EventHandler handler)
        {
            btn.Text     = texto;
            btn.Location = loc;
            btn.Size     = new Size(155, 28);
            btn.Click   += handler;
        }

        private static void ConfigGrilla(RadGridView dgv)
        {
            dgv.ReadOnly    = true;
            dgv.MultiSelect = false;
            dgv.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
        }

        // ── Campos ───────────────────────────────────────────────────────────────
        private Panel           pnlSesiones;
        private Label           lblSesionesTitle;
        private ListBox         lbSesiones;
        private Button          btnNuevaSesion;
        private Button          btnRetomar;
        private Button          btnEliminarSesion;
        private Label           lblSesionActiva;

        private Panel           pnlConfigNueva;
        private Label           lblArchivo;
        private CheckedListBox  clbArchivos;
        private Label           lblConceptos;
        private CheckedListBox  clbConceptos;
        private Label           lblArchivoExterno;
        private TextBox         txtArchivoExterno;
        private Button          btnCargarExterno;
        private Button          btnConfirmarNueva;
        private Button          btnCancelarNueva;

        private Panel           pnlAcciones;
        private Button          btnAutoConciliar;
        private Button          btnConciliarManual;
        private Button          btnDesconciliar;
        private Button          btnFinalizar;
        private Button          btnExportar;
        private Label           lblContadorPendExt;
        private Label           lblContadorPendExtr;
        private Label           lblContadorConcil;

        private TabControl      tabControl;
        private TabPage         tabPendientes;
        private TabPage         tabConciliados;

        private SplitContainer  splitPendientes;
        private Panel           pnlPendExt;
        private Label           lblPendExt;

        private RadGridView     dgvExternoPendiente;
        private TabControl      tabsPendExtracto;
        private TabPage         tabConConcepto;
        private TabPage         tabSinConcepto;
        private RadGridView     dgvExtractoPendiente;
        private RadGridView     dgvExtractoSinConcepto;
        private RadGridView     dgvConciliados;
    }
}
