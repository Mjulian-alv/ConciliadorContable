namespace AgrupadorConceptos
{
    partial class ConciliacionInternaForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lbSesiones = new System.Windows.Forms.ListBox();
            btnNuevaSesion = new System.Windows.Forms.Button();
            btnRetomar = new System.Windows.Forms.Button();
            btnEliminarSesion = new System.Windows.Forms.Button();
            lblSesionActiva = new System.Windows.Forms.Label();

            pnlConfigNueva = new System.Windows.Forms.Panel();
            lblExtractoA = new System.Windows.Forms.Label();
            lblPerfilA = new System.Windows.Forms.Label();
            cmbPerfilA = new System.Windows.Forms.ComboBox();
            lblDesdeA = new System.Windows.Forms.Label();
            dtpDesdeA = new System.Windows.Forms.DateTimePicker();
            lblHastaA = new System.Windows.Forms.Label();
            dtpHastaA = new System.Windows.Forms.DateTimePicker();
            lblExtractoB = new System.Windows.Forms.Label();
            lblPerfilB = new System.Windows.Forms.Label();
            cmbPerfilB = new System.Windows.Forms.ComboBox();
            lblDesdeB = new System.Windows.Forms.Label();
            dtpDesdeB = new System.Windows.Forms.DateTimePicker();
            lblHastaB = new System.Windows.Forms.Label();
            dtpHastaB = new System.Windows.Forms.DateTimePicker();
            lblConceptos = new System.Windows.Forms.Label();
            clbConceptos = new System.Windows.Forms.CheckedListBox();
            btnConfirmarNueva = new System.Windows.Forms.Button();
            btnCancelarNueva = new System.Windows.Forms.Button();

            tabControl = new System.Windows.Forms.TabControl();
            tabPendientes = new System.Windows.Forms.TabPage();
            splitPendientes = new System.Windows.Forms.SplitContainer();
            dgvPendienteA = new Telerik.WinControls.UI.RadGridView();
            dgvPendienteB = new Telerik.WinControls.UI.RadGridView();
            tabConciliados = new System.Windows.Forms.TabPage();
            dgvConciliados = new Telerik.WinControls.UI.RadGridView();

            btnAutoConciliar = new System.Windows.Forms.Button();
            btnConciliarManual = new System.Windows.Forms.Button();
            btnDesconciliar = new System.Windows.Forms.Button();
            btnFinalizar = new System.Windows.Forms.Button();
            btnExportar = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)dgvPendienteA).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvPendienteA.MasterTemplate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvPendienteB).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvPendienteB.MasterTemplate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvConciliados).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvConciliados.MasterTemplate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitPendientes).BeginInit();
            splitPendientes.Panel1.SuspendLayout();
            splitPendientes.Panel2.SuspendLayout();
            SuspendLayout();

            // ── Panel de sesiones (izquierda) ────────────────────────────────
            lbSesiones.Location = new System.Drawing.Point(12, 12);
            lbSesiones.Size = new System.Drawing.Size(220, 160);
            lbSesiones.SelectedIndexChanged += lbSesiones_SelectedIndexChanged;

            btnNuevaSesion.Location = new System.Drawing.Point(12, 178);
            btnNuevaSesion.Size = new System.Drawing.Size(70, 26);
            btnNuevaSesion.Text = "Nueva";
            btnNuevaSesion.UseVisualStyleBackColor = true;
            btnNuevaSesion.Click += btnNuevaSesion_Click;

            btnRetomar.Location = new System.Drawing.Point(86, 178);
            btnRetomar.Size = new System.Drawing.Size(70, 26);
            btnRetomar.Text = "Retomar";
            btnRetomar.UseVisualStyleBackColor = true;
            btnRetomar.Click += btnRetomar_Click;

            btnEliminarSesion.Location = new System.Drawing.Point(160, 178);
            btnEliminarSesion.Size = new System.Drawing.Size(72, 26);
            btnEliminarSesion.Text = "Eliminar";
            btnEliminarSesion.UseVisualStyleBackColor = true;
            btnEliminarSesion.Click += btnEliminarSesion_Click;

            lblSesionActiva.AutoSize = true;
            lblSesionActiva.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblSesionActiva.Location = new System.Drawing.Point(246, 16);
            lblSesionActiva.Text = "Sin sesión activa";

            // ── Panel de alta de sesión ───────────────────────────────────────
            pnlConfigNueva.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pnlConfigNueva.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pnlConfigNueva.Location = new System.Drawing.Point(12, 12);
            pnlConfigNueva.Size = new System.Drawing.Size(1160, 560);
            pnlConfigNueva.Visible = false;
            pnlConfigNueva.Controls.Add(lblExtractoA);
            pnlConfigNueva.Controls.Add(lblPerfilA);
            pnlConfigNueva.Controls.Add(cmbPerfilA);
            pnlConfigNueva.Controls.Add(lblDesdeA);
            pnlConfigNueva.Controls.Add(dtpDesdeA);
            pnlConfigNueva.Controls.Add(lblHastaA);
            pnlConfigNueva.Controls.Add(dtpHastaA);
            pnlConfigNueva.Controls.Add(lblExtractoB);
            pnlConfigNueva.Controls.Add(lblPerfilB);
            pnlConfigNueva.Controls.Add(cmbPerfilB);
            pnlConfigNueva.Controls.Add(lblDesdeB);
            pnlConfigNueva.Controls.Add(dtpDesdeB);
            pnlConfigNueva.Controls.Add(lblHastaB);
            pnlConfigNueva.Controls.Add(dtpHastaB);
            pnlConfigNueva.Controls.Add(lblConceptos);
            pnlConfigNueva.Controls.Add(clbConceptos);
            pnlConfigNueva.Controls.Add(btnConfirmarNueva);
            pnlConfigNueva.Controls.Add(btnCancelarNueva);

            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Se elige perfil + rango de fechas por lado
            // No se tildan archivos importados: el usuario elige un perfil de banco y un rango,
            // y la sesion se arma con lo que caiga ahi (ver ConciliacionInternaService.CargarSinConciliar).
            lblExtractoA.AutoSize = true;
            lblExtractoA.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblExtractoA.Location = new System.Drawing.Point(12, 12);
            lblExtractoA.Text = "Extracto A";

            lblPerfilA.AutoSize = true;
            lblPerfilA.Location = new System.Drawing.Point(12, 42);
            lblPerfilA.Text = "Perfil:";

            cmbPerfilA.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbPerfilA.Location = new System.Drawing.Point(70, 39);
            cmbPerfilA.Size = new System.Drawing.Size(300, 23);
            cmbPerfilA.SelectedIndexChanged += cmbPerfil_SelectedIndexChanged;

            lblDesdeA.AutoSize = true;
            lblDesdeA.Location = new System.Drawing.Point(390, 42);
            lblDesdeA.Text = "Desde:";

            dtpDesdeA.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            dtpDesdeA.Location = new System.Drawing.Point(440, 39);
            dtpDesdeA.Size = new System.Drawing.Size(120, 23);
            dtpDesdeA.ValueChanged += dtpRango_ValueChanged;

            lblHastaA.AutoSize = true;
            lblHastaA.Location = new System.Drawing.Point(570, 42);
            lblHastaA.Text = "Hasta:";

            dtpHastaA.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            dtpHastaA.Location = new System.Drawing.Point(620, 39);
            dtpHastaA.Size = new System.Drawing.Size(120, 23);
            dtpHastaA.ValueChanged += dtpRango_ValueChanged;

            lblExtractoB.AutoSize = true;
            lblExtractoB.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblExtractoB.Location = new System.Drawing.Point(12, 80);
            lblExtractoB.Text = "Extracto B";

            lblPerfilB.AutoSize = true;
            lblPerfilB.Location = new System.Drawing.Point(12, 110);
            lblPerfilB.Text = "Perfil:";

            cmbPerfilB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbPerfilB.Location = new System.Drawing.Point(70, 107);
            cmbPerfilB.Size = new System.Drawing.Size(300, 23);
            cmbPerfilB.SelectedIndexChanged += cmbPerfil_SelectedIndexChanged;

            lblDesdeB.AutoSize = true;
            lblDesdeB.Location = new System.Drawing.Point(390, 110);
            lblDesdeB.Text = "Desde:";

            dtpDesdeB.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            dtpDesdeB.Location = new System.Drawing.Point(440, 107);
            dtpDesdeB.Size = new System.Drawing.Size(120, 23);
            dtpDesdeB.ValueChanged += dtpRango_ValueChanged;

            lblHastaB.AutoSize = true;
            lblHastaB.Location = new System.Drawing.Point(570, 110);
            lblHastaB.Text = "Hasta:";

            dtpHastaB.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            dtpHastaB.Location = new System.Drawing.Point(620, 107);
            dtpHastaB.Size = new System.Drawing.Size(120, 23);
            dtpHastaB.ValueChanged += dtpRango_ValueChanged;

            lblConceptos.AutoSize = true;
            lblConceptos.Location = new System.Drawing.Point(12, 148);
            lblConceptos.Text = "Conceptos a conciliar:";

            clbConceptos.CheckOnClick = true;
            clbConceptos.Location = new System.Drawing.Point(12, 168);
            clbConceptos.Size = new System.Drawing.Size(748, 320);

            btnConfirmarNueva.Location = new System.Drawing.Point(636, 500);
            btnConfirmarNueva.Size = new System.Drawing.Size(124, 30);
            btnConfirmarNueva.Text = "Confirmar";
            btnConfirmarNueva.UseVisualStyleBackColor = true;
            btnConfirmarNueva.Click += btnConfirmarNueva_Click;

            btnCancelarNueva.Location = new System.Drawing.Point(504, 500);
            btnCancelarNueva.Size = new System.Drawing.Size(124, 30);
            btnCancelarNueva.Text = "Cancelar";
            btnCancelarNueva.UseVisualStyleBackColor = true;
            btnCancelarNueva.Click += btnCancelarNueva_Click;

            // ── Tabs (pendientes / conciliados) ──────────────────────────────
            tabControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tabControl.Location = new System.Drawing.Point(12, 210);
            tabControl.Size = new System.Drawing.Size(1160, 400);
            tabControl.TabPages.Add(tabPendientes);
            tabControl.TabPages.Add(tabConciliados);

            tabPendientes.Text = "⏳ Pendientes";
            tabPendientes.Controls.Add(splitPendientes);

            splitPendientes.Dock = System.Windows.Forms.DockStyle.Fill;
            splitPendientes.Panel1.Controls.Add(dgvPendienteA);
            splitPendientes.Panel2.Controls.Add(dgvPendienteB);

            dgvPendienteA.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvPendienteA.MasterTemplate.AllowAddNewRow = false;
            dgvPendienteA.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            dgvPendienteA.Name = "dgvPendienteA";
            dgvPendienteA.ReadOnly = true;
            dgvPendienteA.MultiSelect = false;
            dgvPendienteA.SelectionChanged += dgvPendienteA_SelectionChanged;

            dgvPendienteB.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvPendienteB.MasterTemplate.AllowAddNewRow = false;
            dgvPendienteB.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            dgvPendienteB.Name = "dgvPendienteB";
            dgvPendienteB.ReadOnly = true;
            dgvPendienteB.MultiSelect = false;
            dgvPendienteB.RowFormatting += dgvPendienteB_RowFormatting;

            tabConciliados.Text = "✅ Conciliados";
            tabConciliados.Controls.Add(dgvConciliados);

            dgvConciliados.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvConciliados.MasterTemplate.AllowAddNewRow = false;
            dgvConciliados.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            dgvConciliados.Name = "dgvConciliados";
            dgvConciliados.ReadOnly = true;
            dgvConciliados.MultiSelect = false;

            // ── Botonera inferior ─────────────────────────────────────────────
            btnAutoConciliar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnAutoConciliar.Location = new System.Drawing.Point(12, 616);
            btnAutoConciliar.Size = new System.Drawing.Size(140, 30);
            btnAutoConciliar.Text = "Auto-conciliar";
            btnAutoConciliar.UseVisualStyleBackColor = true;
            btnAutoConciliar.Click += btnAutoConciliar_Click;

            btnConciliarManual.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnConciliarManual.Location = new System.Drawing.Point(158, 616);
            btnConciliarManual.Size = new System.Drawing.Size(140, 30);
            btnConciliarManual.Text = "Conciliar manual";
            btnConciliarManual.UseVisualStyleBackColor = true;
            btnConciliarManual.Click += btnConciliarManual_Click;

            btnDesconciliar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnDesconciliar.Location = new System.Drawing.Point(304, 616);
            btnDesconciliar.Size = new System.Drawing.Size(120, 30);
            btnDesconciliar.Text = "Desconciliar";
            btnDesconciliar.UseVisualStyleBackColor = true;
            btnDesconciliar.Click += btnDesconciliar_Click;

            btnFinalizar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnFinalizar.Location = new System.Drawing.Point(940, 616);
            btnFinalizar.Size = new System.Drawing.Size(110, 30);
            btnFinalizar.Text = "Finalizar";
            btnFinalizar.UseVisualStyleBackColor = true;
            btnFinalizar.Click += btnFinalizar_Click;

            btnExportar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnExportar.Location = new System.Drawing.Point(1062, 616);
            btnExportar.Size = new System.Drawing.Size(110, 30);
            btnExportar.Text = "Exportar";
            btnExportar.UseVisualStyleBackColor = true;
            btnExportar.Click += btnExportar_Click;

            // ── ConciliacionInternaForm ───────────────────────────────────────
            ClientSize = new System.Drawing.Size(1184, 660);
            Controls.Add(lbSesiones);
            Controls.Add(btnNuevaSesion);
            Controls.Add(btnRetomar);
            Controls.Add(btnEliminarSesion);
            Controls.Add(lblSesionActiva);
            Controls.Add(tabControl);
            Controls.Add(pnlConfigNueva);
            Controls.Add(btnAutoConciliar);
            Controls.Add(btnConciliarManual);
            Controls.Add(btnDesconciliar);
            Controls.Add(btnFinalizar);
            Controls.Add(btnExportar);
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Alto minimo ajustado para no recortar pnlConfigNueva
            // pnlConfigNueva esta anclado a los 4 bordes y sus hijos usan Location fijo (no Anchor):
            // con MinimumSize.Height = 500 el panel se achica a menos de lo que sus controles
            // necesitan (el boton Confirmar/Cancelar termina en Y=530 dentro del panel) y quedan
            // recortados fuera del area visible. 650 deja margen (igual que ConciliacionExternForm).
            MinimumSize = new System.Drawing.Size(1000, 650);
            Name = "ConciliacionInternaForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Conciliación Interna — Transferencias entre cuentas propias";

            ((System.ComponentModel.ISupportInitialize)dgvPendienteA.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvPendienteA).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvPendienteB.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvPendienteB).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvConciliados.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvConciliados).EndInit();
            splitPendientes.Panel1.ResumeLayout(false);
            splitPendientes.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitPendientes).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.ListBox lbSesiones;
        private System.Windows.Forms.Button btnNuevaSesion;
        private System.Windows.Forms.Button btnRetomar;
        private System.Windows.Forms.Button btnEliminarSesion;
        private System.Windows.Forms.Label lblSesionActiva;

        private System.Windows.Forms.Panel pnlConfigNueva;
        private System.Windows.Forms.Label lblExtractoA;
        private System.Windows.Forms.Label lblPerfilA;
        private System.Windows.Forms.ComboBox cmbPerfilA;
        private System.Windows.Forms.Label lblDesdeA;
        private System.Windows.Forms.DateTimePicker dtpDesdeA;
        private System.Windows.Forms.Label lblHastaA;
        private System.Windows.Forms.DateTimePicker dtpHastaA;
        private System.Windows.Forms.Label lblExtractoB;
        private System.Windows.Forms.Label lblPerfilB;
        private System.Windows.Forms.ComboBox cmbPerfilB;
        private System.Windows.Forms.Label lblDesdeB;
        private System.Windows.Forms.DateTimePicker dtpDesdeB;
        private System.Windows.Forms.Label lblHastaB;
        private System.Windows.Forms.DateTimePicker dtpHastaB;
        private System.Windows.Forms.Label lblConceptos;
        private System.Windows.Forms.CheckedListBox clbConceptos;
        private System.Windows.Forms.Button btnConfirmarNueva;
        private System.Windows.Forms.Button btnCancelarNueva;

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPendientes;
        private System.Windows.Forms.SplitContainer splitPendientes;
        private Telerik.WinControls.UI.RadGridView dgvPendienteA;
        private Telerik.WinControls.UI.RadGridView dgvPendienteB;
        private System.Windows.Forms.TabPage tabConciliados;
        private Telerik.WinControls.UI.RadGridView dgvConciliados;

        private System.Windows.Forms.Button btnAutoConciliar;
        private System.Windows.Forms.Button btnConciliarManual;
        private System.Windows.Forms.Button btnDesconciliar;
        private System.Windows.Forms.Button btnFinalizar;
        private System.Windows.Forms.Button btnExportar;
    }
}
