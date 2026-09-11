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
            Telerik.WinControls.UI.TableViewDefinition tableViewDefinition1 = new Telerik.WinControls.UI.TableViewDefinition();
            Telerik.WinControls.UI.TableViewDefinition tableViewDefinition2 = new Telerik.WinControls.UI.TableViewDefinition();
            Telerik.WinControls.UI.TableViewDefinition tableViewDefinition3 = new Telerik.WinControls.UI.TableViewDefinition();
            lbSesiones = new System.Windows.Forms.ListBox();
            btnNuevaSesion = new System.Windows.Forms.Button();
            btnRetomar = new System.Windows.Forms.Button();
            btnEliminarSesion = new System.Windows.Forms.Button();
            lblSesionActiva = new System.Windows.Forms.Label();
            pnlConfigNueva = new System.Windows.Forms.Panel();
            lblRango = new System.Windows.Forms.Label();
            lblDesde = new System.Windows.Forms.Label();
            dtpDesde = new System.Windows.Forms.DateTimePicker();
            lblHasta = new System.Windows.Forms.Label();
            dtpHasta = new System.Windows.Forms.DateTimePicker();
            lblConceptos = new System.Windows.Forms.Label();
            clbConceptos = new System.Windows.Forms.CheckedListBox();
            btnConfirmarNueva = new System.Windows.Forms.Button();
            btnCancelarNueva = new System.Windows.Forms.Button();
            tabControl = new System.Windows.Forms.TabControl();
            tabPendientes = new System.Windows.Forms.TabPage();
            splitPendientes = new System.Windows.Forms.SplitContainer();
            dgvDebitos = new Telerik.WinControls.UI.RadGridView();
            lblDebitos = new System.Windows.Forms.Label();
            dgvCreditos = new Telerik.WinControls.UI.RadGridView();
            lblCreditos = new System.Windows.Forms.Label();
            tabConciliados = new System.Windows.Forms.TabPage();
            dgvConciliados = new Telerik.WinControls.UI.RadGridView();
            btnAutoConciliar = new System.Windows.Forms.Button();
            btnConciliarManual = new System.Windows.Forms.Button();
            btnDesconciliar = new System.Windows.Forms.Button();
            btnFinalizar = new System.Windows.Forms.Button();
            btnExportar = new System.Windows.Forms.Button();
            pnlConfigNueva.SuspendLayout();
            tabControl.SuspendLayout();
            tabPendientes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitPendientes).BeginInit();
            splitPendientes.Panel1.SuspendLayout();
            splitPendientes.Panel2.SuspendLayout();
            splitPendientes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDebitos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvDebitos.MasterTemplate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvCreditos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvCreditos.MasterTemplate).BeginInit();
            tabConciliados.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvConciliados).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvConciliados.MasterTemplate).BeginInit();
            SuspendLayout();
            // 
            // lbSesiones
            // 
            lbSesiones.ItemHeight = 15;
            lbSesiones.Location = new System.Drawing.Point(12, 12);
            lbSesiones.Name = "lbSesiones";
            lbSesiones.Size = new System.Drawing.Size(220, 154);
            lbSesiones.TabIndex = 0;
            lbSesiones.SelectedIndexChanged += lbSesiones_SelectedIndexChanged;
            // 
            // btnNuevaSesion
            // 
            btnNuevaSesion.Location = new System.Drawing.Point(12, 178);
            btnNuevaSesion.Name = "btnNuevaSesion";
            btnNuevaSesion.Size = new System.Drawing.Size(70, 26);
            btnNuevaSesion.TabIndex = 1;
            btnNuevaSesion.Text = "Nueva";
            btnNuevaSesion.UseVisualStyleBackColor = true;
            btnNuevaSesion.Click += btnNuevaSesion_Click;
            // 
            // btnRetomar
            // 
            btnRetomar.Location = new System.Drawing.Point(86, 178);
            btnRetomar.Name = "btnRetomar";
            btnRetomar.Size = new System.Drawing.Size(70, 26);
            btnRetomar.TabIndex = 2;
            btnRetomar.Text = "Retomar";
            btnRetomar.UseVisualStyleBackColor = true;
            btnRetomar.Click += btnRetomar_Click;
            // 
            // btnEliminarSesion
            // 
            btnEliminarSesion.Location = new System.Drawing.Point(160, 178);
            btnEliminarSesion.Name = "btnEliminarSesion";
            btnEliminarSesion.Size = new System.Drawing.Size(72, 26);
            btnEliminarSesion.TabIndex = 3;
            btnEliminarSesion.Text = "Eliminar";
            btnEliminarSesion.UseVisualStyleBackColor = true;
            btnEliminarSesion.Click += btnEliminarSesion_Click;
            // 
            // lblSesionActiva
            // 
            lblSesionActiva.AutoSize = true;
            lblSesionActiva.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblSesionActiva.Location = new System.Drawing.Point(246, 16);
            lblSesionActiva.Name = "lblSesionActiva";
            lblSesionActiva.Size = new System.Drawing.Size(97, 15);
            lblSesionActiva.TabIndex = 4;
            lblSesionActiva.Text = "Sin sesión activa";
            // 
            // pnlConfigNueva
            // 
            pnlConfigNueva.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pnlConfigNueva.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pnlConfigNueva.Controls.Add(lblRango);
            pnlConfigNueva.Controls.Add(lblDesde);
            pnlConfigNueva.Controls.Add(dtpDesde);
            pnlConfigNueva.Controls.Add(lblHasta);
            pnlConfigNueva.Controls.Add(dtpHasta);
            pnlConfigNueva.Controls.Add(lblConceptos);
            pnlConfigNueva.Controls.Add(clbConceptos);
            pnlConfigNueva.Controls.Add(btnConfirmarNueva);
            pnlConfigNueva.Controls.Add(btnCancelarNueva);
            pnlConfigNueva.Location = new System.Drawing.Point(12, 12);
            pnlConfigNueva.Name = "pnlConfigNueva";
            pnlConfigNueva.Size = new System.Drawing.Size(1160, 560);
            pnlConfigNueva.TabIndex = 6;
            pnlConfigNueva.Visible = false;
            // 
            // lblRango
            // 
            lblRango.AutoSize = true;
            lblRango.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblRango.Location = new System.Drawing.Point(12, 12);
            lblRango.Name = "lblRango";
            lblRango.Size = new System.Drawing.Size(156, 15);
            lblRango.TabIndex = 0;
            lblRango.Text = "Rango de fechas a conciliar";
            // 
            // lblDesde
            // 
            lblDesde.AutoSize = true;
            lblDesde.Location = new System.Drawing.Point(12, 42);
            lblDesde.Name = "lblDesde";
            lblDesde.Size = new System.Drawing.Size(42, 15);
            lblDesde.TabIndex = 1;
            lblDesde.Text = "Desde:";
            // 
            // dtpDesde
            // 
            dtpDesde.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            dtpDesde.Location = new System.Drawing.Point(70, 39);
            dtpDesde.Name = "dtpDesde";
            dtpDesde.Size = new System.Drawing.Size(120, 23);
            dtpDesde.TabIndex = 2;
            dtpDesde.ValueChanged += dtpRango_ValueChanged;
            // 
            // lblHasta
            // 
            lblHasta.AutoSize = true;
            lblHasta.Location = new System.Drawing.Point(200, 42);
            lblHasta.Name = "lblHasta";
            lblHasta.Size = new System.Drawing.Size(40, 15);
            lblHasta.TabIndex = 3;
            lblHasta.Text = "Hasta:";
            // 
            // dtpHasta
            // 
            dtpHasta.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            dtpHasta.Location = new System.Drawing.Point(250, 39);
            dtpHasta.Name = "dtpHasta";
            dtpHasta.Size = new System.Drawing.Size(120, 23);
            dtpHasta.TabIndex = 4;
            dtpHasta.ValueChanged += dtpRango_ValueChanged;
            // 
            // lblConceptos
            // 
            lblConceptos.AutoSize = true;
            lblConceptos.Location = new System.Drawing.Point(12, 80);
            lblConceptos.Name = "lblConceptos";
            lblConceptos.Size = new System.Drawing.Size(124, 15);
            lblConceptos.TabIndex = 5;
            lblConceptos.Text = "Conceptos a conciliar:";
            // 
            // clbConceptos
            // 
            clbConceptos.CheckOnClick = true;
            clbConceptos.Location = new System.Drawing.Point(12, 100);
            clbConceptos.Name = "clbConceptos";
            clbConceptos.Size = new System.Drawing.Size(748, 382);
            clbConceptos.TabIndex = 6;
            // 
            // btnConfirmarNueva
            // 
            btnConfirmarNueva.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnConfirmarNueva.Location = new System.Drawing.Point(636, 500);
            btnConfirmarNueva.Name = "btnConfirmarNueva";
            btnConfirmarNueva.Size = new System.Drawing.Size(124, 30);
            btnConfirmarNueva.TabIndex = 7;
            btnConfirmarNueva.Text = "Confirmar";
            btnConfirmarNueva.UseVisualStyleBackColor = true;
            btnConfirmarNueva.Click += btnConfirmarNueva_Click;
            // 
            // btnCancelarNueva
            // 
            btnCancelarNueva.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnCancelarNueva.Location = new System.Drawing.Point(504, 500);
            btnCancelarNueva.Name = "btnCancelarNueva";
            btnCancelarNueva.Size = new System.Drawing.Size(124, 30);
            btnCancelarNueva.TabIndex = 8;
            btnCancelarNueva.Text = "Cancelar";
            btnCancelarNueva.UseVisualStyleBackColor = true;
            btnCancelarNueva.Click += btnCancelarNueva_Click;
            // 
            // tabControl
            // 
            tabControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tabControl.Controls.Add(tabPendientes);
            tabControl.Controls.Add(tabConciliados);
            tabControl.Location = new System.Drawing.Point(12, 210);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new System.Drawing.Size(1160, 400);
            tabControl.TabIndex = 5;
            // 
            // tabPendientes
            // 
            tabPendientes.Controls.Add(splitPendientes);
            tabPendientes.Location = new System.Drawing.Point(4, 24);
            tabPendientes.Name = "tabPendientes";
            tabPendientes.Size = new System.Drawing.Size(1152, 372);
            tabPendientes.TabIndex = 0;
            tabPendientes.Text = "⏳ Pendientes";
            // 
            // splitPendientes
            // 
            splitPendientes.Dock = System.Windows.Forms.DockStyle.Fill;
            splitPendientes.Location = new System.Drawing.Point(0, 0);
            splitPendientes.Name = "splitPendientes";
            // 
            // splitPendientes.Panel1
            // 
            splitPendientes.Panel1.Controls.Add(dgvDebitos);
            splitPendientes.Panel1.Controls.Add(lblDebitos);
            // 
            // splitPendientes.Panel2
            // 
            splitPendientes.Panel2.Controls.Add(dgvCreditos);
            splitPendientes.Panel2.Controls.Add(lblCreditos);
            splitPendientes.Size = new System.Drawing.Size(1152, 372);
            splitPendientes.SplitterDistance = 548;
            splitPendientes.TabIndex = 0;
            // 
            // dgvDebitos
            // 
            dgvDebitos.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvDebitos.Location = new System.Drawing.Point(0, 22);
            // 
            // 
            // 
            dgvDebitos.MasterTemplate.AllowAddNewRow = false;
            dgvDebitos.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            dgvDebitos.MasterTemplate.ViewDefinition = tableViewDefinition1;
            dgvDebitos.Name = "dgvDebitos";
            dgvDebitos.ReadOnly = true;
            dgvDebitos.Size = new System.Drawing.Size(548, 350);
            dgvDebitos.TabIndex = 0;
            dgvDebitos.SelectionChanged += dgvDebitos_SelectionChanged;
            // 
            // lblDebitos
            // 
            lblDebitos.Dock = System.Windows.Forms.DockStyle.Top;
            lblDebitos.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblDebitos.Location = new System.Drawing.Point(0, 0);
            lblDebitos.Name = "lblDebitos";
            lblDebitos.Size = new System.Drawing.Size(548, 22);
            lblDebitos.TabIndex = 1;
            lblDebitos.Text = "Débitos";
            lblDebitos.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dgvCreditos
            // 
            dgvCreditos.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvCreditos.Location = new System.Drawing.Point(0, 22);
            // 
            // 
            // 
            dgvCreditos.MasterTemplate.AllowAddNewRow = false;
            dgvCreditos.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            dgvCreditos.MasterTemplate.ViewDefinition = tableViewDefinition2;
            dgvCreditos.Name = "dgvCreditos";
            dgvCreditos.ReadOnly = true;
            dgvCreditos.Size = new System.Drawing.Size(600, 350);
            dgvCreditos.TabIndex = 0;
            dgvCreditos.RowFormatting += dgvCreditos_RowFormatting;
            // 
            // lblCreditos
            // 
            lblCreditos.Dock = System.Windows.Forms.DockStyle.Top;
            lblCreditos.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblCreditos.Location = new System.Drawing.Point(0, 0);
            lblCreditos.Name = "lblCreditos";
            lblCreditos.Size = new System.Drawing.Size(600, 22);
            lblCreditos.TabIndex = 1;
            lblCreditos.Text = "Créditos";
            lblCreditos.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tabConciliados
            // 
            tabConciliados.Controls.Add(dgvConciliados);
            tabConciliados.Location = new System.Drawing.Point(4, 24);
            tabConciliados.Name = "tabConciliados";
            tabConciliados.Size = new System.Drawing.Size(1152, 372);
            tabConciliados.TabIndex = 1;
            tabConciliados.Text = "✅ Conciliados";
            // 
            // dgvConciliados
            // 
            dgvConciliados.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvConciliados.Location = new System.Drawing.Point(0, 0);
            // 
            // 
            // 
            dgvConciliados.MasterTemplate.AllowAddNewRow = false;
            dgvConciliados.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            dgvConciliados.MasterTemplate.ViewDefinition = tableViewDefinition3;
            dgvConciliados.Name = "dgvConciliados";
            dgvConciliados.ReadOnly = true;
            dgvConciliados.Size = new System.Drawing.Size(1152, 372);
            dgvConciliados.TabIndex = 0;
            // 
            // btnAutoConciliar
            // 
            btnAutoConciliar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnAutoConciliar.Location = new System.Drawing.Point(12, 616);
            btnAutoConciliar.Name = "btnAutoConciliar";
            btnAutoConciliar.Size = new System.Drawing.Size(140, 30);
            btnAutoConciliar.TabIndex = 7;
            btnAutoConciliar.Text = "Auto-conciliar";
            btnAutoConciliar.UseVisualStyleBackColor = true;
            btnAutoConciliar.Click += btnAutoConciliar_Click;
            // 
            // btnConciliarManual
            // 
            btnConciliarManual.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnConciliarManual.Location = new System.Drawing.Point(158, 616);
            btnConciliarManual.Name = "btnConciliarManual";
            btnConciliarManual.Size = new System.Drawing.Size(140, 30);
            btnConciliarManual.TabIndex = 8;
            btnConciliarManual.Text = "Conciliar manual";
            btnConciliarManual.UseVisualStyleBackColor = true;
            btnConciliarManual.Click += btnConciliarManual_Click;
            // 
            // btnDesconciliar
            // 
            btnDesconciliar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnDesconciliar.Location = new System.Drawing.Point(304, 616);
            btnDesconciliar.Name = "btnDesconciliar";
            btnDesconciliar.Size = new System.Drawing.Size(120, 30);
            btnDesconciliar.TabIndex = 9;
            btnDesconciliar.Text = "Desconciliar";
            btnDesconciliar.UseVisualStyleBackColor = true;
            btnDesconciliar.Click += btnDesconciliar_Click;
            // 
            // btnFinalizar
            // 
            btnFinalizar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnFinalizar.Location = new System.Drawing.Point(940, 616);
            btnFinalizar.Name = "btnFinalizar";
            btnFinalizar.Size = new System.Drawing.Size(110, 30);
            btnFinalizar.TabIndex = 10;
            btnFinalizar.Text = "Finalizar";
            btnFinalizar.UseVisualStyleBackColor = true;
            btnFinalizar.Click += btnFinalizar_Click;
            // 
            // btnExportar
            // 
            btnExportar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnExportar.Location = new System.Drawing.Point(1062, 616);
            btnExportar.Name = "btnExportar";
            btnExportar.Size = new System.Drawing.Size(110, 30);
            btnExportar.TabIndex = 11;
            btnExportar.Text = "Exportar";
            btnExportar.UseVisualStyleBackColor = true;
            btnExportar.Click += btnExportar_Click;
            // 
            // ConciliacionInternaForm
            // 
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
            MinimumSize = new System.Drawing.Size(1000, 500);
            Name = "ConciliacionInternaForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Conciliación Interna — Transferencias entre cuentas propias";
            pnlConfigNueva.ResumeLayout(false);
            pnlConfigNueva.PerformLayout();
            tabControl.ResumeLayout(false);
            tabPendientes.ResumeLayout(false);
            splitPendientes.Panel1.ResumeLayout(false);
            splitPendientes.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitPendientes).EndInit();
            splitPendientes.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvDebitos.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvDebitos).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvCreditos.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvCreditos).EndInit();
            tabConciliados.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvConciliados.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvConciliados).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.ListBox lbSesiones;
        private System.Windows.Forms.Button btnNuevaSesion;
        private System.Windows.Forms.Button btnRetomar;
        private System.Windows.Forms.Button btnEliminarSesion;
        private System.Windows.Forms.Label lblSesionActiva;

        private System.Windows.Forms.Panel pnlConfigNueva;
        private System.Windows.Forms.Label lblRango;
        private System.Windows.Forms.Label lblDesde;
        private System.Windows.Forms.DateTimePicker dtpDesde;
        private System.Windows.Forms.Label lblHasta;
        private System.Windows.Forms.DateTimePicker dtpHasta;
        private System.Windows.Forms.Label lblConceptos;
        private System.Windows.Forms.CheckedListBox clbConceptos;
        private System.Windows.Forms.Button btnConfirmarNueva;
        private System.Windows.Forms.Button btnCancelarNueva;

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPendientes;
        private System.Windows.Forms.SplitContainer splitPendientes;
        private System.Windows.Forms.Label lblDebitos;
        private Telerik.WinControls.UI.RadGridView dgvDebitos;
        private System.Windows.Forms.Label lblCreditos;
        private Telerik.WinControls.UI.RadGridView dgvCreditos;
        private System.Windows.Forms.TabPage tabConciliados;
        private Telerik.WinControls.UI.RadGridView dgvConciliados;

        private System.Windows.Forms.Button btnAutoConciliar;
        private System.Windows.Forms.Button btnConciliarManual;
        private System.Windows.Forms.Button btnDesconciliar;
        private System.Windows.Forms.Button btnFinalizar;
        private System.Windows.Forms.Button btnExportar;
    }
}
