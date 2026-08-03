namespace ArcaCliente
{
    partial class FormComprobantesOffline
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            Telerik.WinControls.UI.TableViewDefinition tableViewDefinition3 = new Telerik.WinControls.UI.TableViewDefinition();
            Telerik.WinControls.UI.TableViewDefinition tableViewDefinition4 = new Telerik.WinControls.UI.TableViewDefinition();
            pnlTop = new Telerik.WinControls.UI.RadPanel();
            lblTitulo = new Telerik.WinControls.UI.RadLabel();
            pnlLeft = new Telerik.WinControls.UI.RadPanel();
            grpArca = new System.Windows.Forms.GroupBox();
            lblCarpeta = new Telerik.WinControls.UI.RadLabel();
            txtCarpeta = new Telerik.WinControls.UI.RadTextBox();
            btnBrowseCarpeta = new Telerik.WinControls.UI.RadButton();
            btnCargarCsv = new Telerik.WinControls.UI.RadButton();
            lblConteoCsv = new Telerik.WinControls.UI.RadLabel();
            grpLocal = new System.Windows.Forms.GroupBox();
            lblArchivo = new Telerik.WinControls.UI.RadLabel();
            txtArchivo = new Telerik.WinControls.UI.RadTextBox();
            btnBrowseArchivo = new Telerik.WinControls.UI.RadButton();
            btnCargarLocal = new Telerik.WinControls.UI.RadButton();
            lblConteoLocal = new Telerik.WinControls.UI.RadLabel();
            lblPerfilActivo = new Telerik.WinControls.UI.RadLabel();
            lblFechaInicio = new Telerik.WinControls.UI.RadLabel();
            dtpFechaInicio = new Telerik.WinControls.UI.RadDateTimePicker();
            lblFechaFin = new Telerik.WinControls.UI.RadLabel();
            dtpFechaFin = new Telerik.WinControls.UI.RadDateTimePicker();
            btnConciliar = new Telerik.WinControls.UI.RadButton();
            btnExportarConciliacion = new Telerik.WinControls.UI.RadButton();
            lblEstado = new Telerik.WinControls.UI.RadLabel();
            splitMain = new System.Windows.Forms.SplitContainer();
            gridComprobantes = new Telerik.WinControls.UI.RadGridView();
            lblTituloArca = new Telerik.WinControls.UI.RadLabel();
            gridConciliacion = new Telerik.WinControls.UI.RadGridView();
            lblEstadoConciliacion = new Telerik.WinControls.UI.RadLabel();
            lblConciliacion = new Telerik.WinControls.UI.RadLabel();
            ((System.ComponentModel.ISupportInitialize)pnlTop).BeginInit();
            pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lblTitulo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlLeft).BeginInit();
            pnlLeft.SuspendLayout();
            grpArca.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lblCarpeta).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtCarpeta).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnBrowseCarpeta).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnCargarCsv).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblConteoCsv).BeginInit();
            grpLocal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lblArchivo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtArchivo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnBrowseArchivo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnCargarLocal).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblConteoLocal).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblPerfilActivo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblFechaInicio).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dtpFechaInicio).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblFechaFin).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dtpFechaFin).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnConciliar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnExportarConciliacion).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblEstado).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridComprobantes).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridComprobantes.MasterTemplate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblTituloArca).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridConciliacion).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridConciliacion.MasterTemplate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblEstadoConciliacion).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblConciliacion).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            SuspendLayout();
            // 
            // pnlTop
            // 
            pnlTop.Controls.Add(lblTitulo);
            pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            pnlTop.Location = new System.Drawing.Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new System.Drawing.Size(1150, 50);
            pnlTop.TabIndex = 0;
            // 
            // lblTitulo
            // 
            lblTitulo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            lblTitulo.Location = new System.Drawing.Point(12, 13);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new System.Drawing.Size(267, 25);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Conciliación Offline (CSV + Excel)";
            // 
            // pnlLeft
            // 
            pnlLeft.Controls.Add(grpArca);
            pnlLeft.Controls.Add(grpLocal);
            pnlLeft.Controls.Add(lblPerfilActivo);
            pnlLeft.Controls.Add(lblFechaInicio);
            pnlLeft.Controls.Add(dtpFechaInicio);
            pnlLeft.Controls.Add(lblFechaFin);
            pnlLeft.Controls.Add(dtpFechaFin);
            pnlLeft.Controls.Add(btnConciliar);
            pnlLeft.Controls.Add(btnExportarConciliacion);
            pnlLeft.Controls.Add(lblEstado);
            pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            pnlLeft.Location = new System.Drawing.Point(0, 50);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Size = new System.Drawing.Size(280, 650);
            pnlLeft.TabIndex = 1;
            // 
            // grpArca
            // 
            grpArca.Controls.Add(lblCarpeta);
            grpArca.Controls.Add(txtCarpeta);
            grpArca.Controls.Add(btnBrowseCarpeta);
            grpArca.Controls.Add(btnCargarCsv);
            grpArca.Controls.Add(lblConteoCsv);
            grpArca.Location = new System.Drawing.Point(12, 8);
            grpArca.Name = "grpArca";
            grpArca.Size = new System.Drawing.Size(256, 118);
            grpArca.TabIndex = 0;
            grpArca.TabStop = false;
            grpArca.Text = "Comprobantes ARCA (CSV)";
            // 
            // lblCarpeta
            // 
            lblCarpeta.Location = new System.Drawing.Point(8, 20);
            lblCarpeta.Name = "lblCarpeta";
            lblCarpeta.Size = new System.Drawing.Size(48, 18);
            lblCarpeta.TabIndex = 0;
            lblCarpeta.Text = "Carpeta:";
            // 
            // txtCarpeta
            // 
            txtCarpeta.Location = new System.Drawing.Point(8, 40);
            txtCarpeta.Name = "txtCarpeta";
            txtCarpeta.ReadOnly = true;
            txtCarpeta.Size = new System.Drawing.Size(200, 24);
            txtCarpeta.TabIndex = 0;
            // 
            // btnBrowseCarpeta
            // 
            btnBrowseCarpeta.Location = new System.Drawing.Point(212, 40);
            btnBrowseCarpeta.Name = "btnBrowseCarpeta";
            btnBrowseCarpeta.Size = new System.Drawing.Size(36, 24);
            btnBrowseCarpeta.TabIndex = 1;
            btnBrowseCarpeta.Text = "...";
            btnBrowseCarpeta.Click += BtnBrowseCarpeta_Click;
            // 
            // btnCargarCsv
            // 
            btnCargarCsv.Location = new System.Drawing.Point(8, 68);
            btnCargarCsv.Name = "btnCargarCsv";
            btnCargarCsv.Size = new System.Drawing.Size(240, 26);
            btnCargarCsv.TabIndex = 2;
            btnCargarCsv.Text = "Cargar archivos CSV";
            btnCargarCsv.Click += BtnCargarCsv_Click;
            // 
            // lblConteoCsv
            // 
            lblConteoCsv.AutoSize = false;
            lblConteoCsv.Location = new System.Drawing.Point(8, 98);
            lblConteoCsv.Name = "lblConteoCsv";
            lblConteoCsv.Size = new System.Drawing.Size(240, 15);
            lblConteoCsv.TabIndex = 3;
            lblConteoCsv.Text = "Sin cargar";
            // 
            // grpLocal
            // 
            grpLocal.Controls.Add(lblArchivo);
            grpLocal.Controls.Add(txtArchivo);
            grpLocal.Controls.Add(btnBrowseArchivo);
            grpLocal.Controls.Add(btnCargarLocal);
            grpLocal.Controls.Add(lblConteoLocal);
            grpLocal.Location = new System.Drawing.Point(12, 132);
            grpLocal.Name = "grpLocal";
            grpLocal.Size = new System.Drawing.Size(256, 118);
            grpLocal.TabIndex = 1;
            grpLocal.TabStop = false;
            grpLocal.Text = "Sistema local (Excel .xlsx)";
            // 
            // lblArchivo
            // 
            lblArchivo.Location = new System.Drawing.Point(8, 20);
            lblArchivo.Name = "lblArchivo";
            lblArchivo.Size = new System.Drawing.Size(46, 18);
            lblArchivo.TabIndex = 0;
            lblArchivo.Text = "Archivo:";
            // 
            // txtArchivo
            // 
            txtArchivo.Location = new System.Drawing.Point(8, 40);
            txtArchivo.Name = "txtArchivo";
            txtArchivo.ReadOnly = true;
            txtArchivo.Size = new System.Drawing.Size(200, 24);
            txtArchivo.TabIndex = 0;
            // 
            // btnBrowseArchivo
            // 
            btnBrowseArchivo.Location = new System.Drawing.Point(212, 40);
            btnBrowseArchivo.Name = "btnBrowseArchivo";
            btnBrowseArchivo.Size = new System.Drawing.Size(36, 24);
            btnBrowseArchivo.TabIndex = 1;
            btnBrowseArchivo.Text = "...";
            btnBrowseArchivo.Click += BtnBrowseArchivo_Click;
            // 
            // btnCargarLocal
            // 
            btnCargarLocal.Location = new System.Drawing.Point(8, 68);
            btnCargarLocal.Name = "btnCargarLocal";
            btnCargarLocal.Size = new System.Drawing.Size(240, 26);
            btnCargarLocal.TabIndex = 2;
            btnCargarLocal.Text = "Cargar archivo local";
            btnCargarLocal.Click += BtnCargarLocal_Click;
            // 
            // lblConteoLocal
            // 
            lblConteoLocal.AutoSize = false;
            lblConteoLocal.Location = new System.Drawing.Point(8, 98);
            lblConteoLocal.Name = "lblConteoLocal";
            lblConteoLocal.Size = new System.Drawing.Size(240, 15);
            lblConteoLocal.TabIndex = 3;
            lblConteoLocal.Text = "Sin cargar";
            // 
            // lblPerfilActivo
            // 
            lblPerfilActivo.AutoSize = false;
            lblPerfilActivo.Location = new System.Drawing.Point(12, 258);
            lblPerfilActivo.Name = "lblPerfilActivo";
            lblPerfilActivo.Size = new System.Drawing.Size(248, 18);
            lblPerfilActivo.TabIndex = 2;
            // 
            // lblFechaInicio
            // 
            lblFechaInicio.Location = new System.Drawing.Point(12, 284);
            lblFechaInicio.Name = "lblFechaInicio";
            lblFechaInicio.Size = new System.Drawing.Size(67, 18);
            lblFechaInicio.TabIndex = 5;
            lblFechaInicio.Text = "Fecha inicio:";
            // 
            // dtpFechaInicio
            // 
            dtpFechaInicio.Location = new System.Drawing.Point(12, 302);
            dtpFechaInicio.Name = "dtpFechaInicio";
            dtpFechaInicio.Size = new System.Drawing.Size(248, 24);
            dtpFechaInicio.TabIndex = 6;
            dtpFechaInicio.TabStop = true;
            dtpFechaInicio.Text = "viernes, 27 de marzo de 2026";
            dtpFechaInicio.Value = new System.DateTime(2026, 3, 27, 8, 50, 38, 132);
            // 
            // lblFechaFin
            // 
            lblFechaFin.Location = new System.Drawing.Point(12, 332);
            lblFechaFin.Name = "lblFechaFin";
            lblFechaFin.Size = new System.Drawing.Size(53, 18);
            lblFechaFin.TabIndex = 7;
            lblFechaFin.Text = "Fecha fin:";
            // 
            // dtpFechaFin
            // 
            dtpFechaFin.Location = new System.Drawing.Point(12, 350);
            dtpFechaFin.Name = "dtpFechaFin";
            dtpFechaFin.Size = new System.Drawing.Size(248, 24);
            dtpFechaFin.TabIndex = 8;
            dtpFechaFin.TabStop = true;
            dtpFechaFin.Text = "viernes, 27 de marzo de 2026";
            dtpFechaFin.Value = new System.DateTime(2026, 3, 27, 8, 50, 38, 157);
            // 
            // btnConciliar
            // 
            btnConciliar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnConciliar.Location = new System.Drawing.Point(12, 386);
            btnConciliar.Name = "btnConciliar";
            btnConciliar.Size = new System.Drawing.Size(248, 36);
            btnConciliar.TabIndex = 9;
            btnConciliar.Text = "CONCILIAR";
            btnConciliar.Click += BtnConciliar_Click;
            // 
            // btnExportarConciliacion
            // 
            btnExportarConciliacion.Location = new System.Drawing.Point(12, 462);
            btnExportarConciliacion.Name = "btnExportarConciliacion";
            btnExportarConciliacion.Size = new System.Drawing.Size(248, 28);
            btnExportarConciliacion.TabIndex = 10;
            btnExportarConciliacion.Text = "Exportar conciliación...";
            btnExportarConciliacion.Visible = false;
            btnExportarConciliacion.Click += BtnExportarConciliacion_Click;
            // 
            // lblEstado
            // 
            lblEstado.AutoSize = false;
            lblEstado.Location = new System.Drawing.Point(12, 530);
            lblEstado.Name = "lblEstado";
            lblEstado.Size = new System.Drawing.Size(248, 40);
            lblEstado.TabIndex = 10;
            // 
            // splitMain
            // 
            splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitMain.Location = new System.Drawing.Point(280, 50);
            splitMain.Name = "splitMain";
            splitMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(gridComprobantes);
            splitMain.Panel1.Controls.Add(lblTituloArca);
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(gridConciliacion);
            splitMain.Panel2.Controls.Add(lblEstadoConciliacion);
            splitMain.Panel2.Controls.Add(lblConciliacion);
            splitMain.Size = new System.Drawing.Size(870, 650);
            splitMain.SplitterDistance = 320;
            splitMain.TabIndex = 2;
            // 
            // gridComprobantes
            // 
            gridComprobantes.Dock = System.Windows.Forms.DockStyle.Fill;
            gridComprobantes.Location = new System.Drawing.Point(0, 24);
            // 
            // 
            // 
            gridComprobantes.MasterTemplate.EnableFiltering = true;
            gridComprobantes.MasterTemplate.ViewDefinition = tableViewDefinition3;
            gridComprobantes.Name = "gridComprobantes";
            gridComprobantes.Size = new System.Drawing.Size(870, 296);
            gridComprobantes.TabIndex = 0;
            // 
            // lblTituloArca
            // 
            lblTituloArca.AutoSize = false;
            lblTituloArca.Dock = System.Windows.Forms.DockStyle.Top;
            lblTituloArca.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblTituloArca.Location = new System.Drawing.Point(0, 0);
            lblTituloArca.Name = "lblTituloArca";
            lblTituloArca.Padding = new System.Windows.Forms.Padding(4, 2, 0, 0);
            lblTituloArca.Size = new System.Drawing.Size(870, 24);
            lblTituloArca.TabIndex = 1;
            lblTituloArca.Text = "Comprobantes ARCA";
            // 
            // gridConciliacion
            // 
            gridConciliacion.Dock = System.Windows.Forms.DockStyle.Fill;
            gridConciliacion.Location = new System.Drawing.Point(0, 24);
            // 
            // 
            // 
            gridConciliacion.MasterTemplate.EnableFiltering = true;
            gridConciliacion.MasterTemplate.ViewDefinition = tableViewDefinition4;
            gridConciliacion.Name = "gridConciliacion";
            gridConciliacion.Size = new System.Drawing.Size(870, 282);
            gridConciliacion.TabIndex = 0;
            // 
            // lblEstadoConciliacion
            // 
            lblEstadoConciliacion.AutoSize = false;
            lblEstadoConciliacion.Dock = System.Windows.Forms.DockStyle.Bottom;
            lblEstadoConciliacion.Location = new System.Drawing.Point(0, 306);
            lblEstadoConciliacion.Name = "lblEstadoConciliacion";
            lblEstadoConciliacion.Padding = new System.Windows.Forms.Padding(4, 0, 0, 2);
            lblEstadoConciliacion.Size = new System.Drawing.Size(870, 20);
            lblEstadoConciliacion.TabIndex = 1;
            // 
            // lblConciliacion
            // 
            lblConciliacion.AutoSize = false;
            lblConciliacion.Dock = System.Windows.Forms.DockStyle.Top;
            lblConciliacion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblConciliacion.Location = new System.Drawing.Point(0, 0);
            lblConciliacion.Name = "lblConciliacion";
            lblConciliacion.Padding = new System.Windows.Forms.Padding(4, 2, 0, 0);
            lblConciliacion.Size = new System.Drawing.Size(870, 24);
            lblConciliacion.TabIndex = 2;
            lblConciliacion.Text = "Conciliación";
            // 
            // FormComprobantesOffline
            // 
            AutoScaleBaseSize = new System.Drawing.Size(7, 15);
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1150, 700);
            Controls.Add(splitMain);
            Controls.Add(pnlLeft);
            Controls.Add(pnlTop);
            MinimumSize = new System.Drawing.Size(900, 600);
            Name = "FormComprobantesOffline";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "ARCA Cliente — Conciliación Offline";
            ((System.ComponentModel.ISupportInitialize)pnlTop).EndInit();
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)lblTitulo).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlLeft).EndInit();
            pnlLeft.ResumeLayout(false);
            pnlLeft.PerformLayout();
            grpArca.ResumeLayout(false);
            grpArca.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)lblCarpeta).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtCarpeta).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnBrowseCarpeta).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnCargarCsv).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblConteoCsv).EndInit();
            grpLocal.ResumeLayout(false);
            grpLocal.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)lblArchivo).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtArchivo).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnBrowseArchivo).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnCargarLocal).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblConteoLocal).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblPerfilActivo).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblFechaInicio).EndInit();
            ((System.ComponentModel.ISupportInitialize)dtpFechaInicio).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblFechaFin).EndInit();
            ((System.ComponentModel.ISupportInitialize)dtpFechaFin).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnConciliar).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnExportarConciliacion).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblEstado).EndInit();
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridComprobantes.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridComprobantes).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblTituloArca).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridConciliacion.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridConciliacion).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblEstadoConciliacion).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblConciliacion).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        // ?? Field declarations ????????????????????????????????????????????????????
        private Telerik.WinControls.UI.RadPanel         pnlTop;
        private Telerik.WinControls.UI.RadLabel         lblTitulo;
        private Telerik.WinControls.UI.RadPanel         pnlLeft;
        private System.Windows.Forms.GroupBox           grpArca;
        private Telerik.WinControls.UI.RadLabel         lblCarpeta;
        private Telerik.WinControls.UI.RadTextBox       txtCarpeta;
        private Telerik.WinControls.UI.RadButton        btnBrowseCarpeta;
        private Telerik.WinControls.UI.RadButton        btnCargarCsv;
        private Telerik.WinControls.UI.RadLabel         lblConteoCsv;
        private System.Windows.Forms.GroupBox           grpLocal;
        private Telerik.WinControls.UI.RadLabel         lblArchivo;
        private Telerik.WinControls.UI.RadTextBox       txtArchivo;
        private Telerik.WinControls.UI.RadButton        btnBrowseArchivo;
        private Telerik.WinControls.UI.RadButton        btnCargarLocal;
        private Telerik.WinControls.UI.RadLabel         lblConteoLocal;
        private Telerik.WinControls.UI.RadLabel         lblPerfilActivo;
        private Telerik.WinControls.UI.RadLabel         lblFechaInicio;
        private Telerik.WinControls.UI.RadDateTimePicker dtpFechaInicio;
        private Telerik.WinControls.UI.RadLabel         lblFechaFin;
        private Telerik.WinControls.UI.RadDateTimePicker dtpFechaFin;
        private Telerik.WinControls.UI.RadButton        btnConciliar;
        private Telerik.WinControls.UI.RadButton        btnExportarConciliacion;
        private Telerik.WinControls.UI.RadButton        btnExportarSistema;
        private Telerik.WinControls.UI.RadLabel         lblEstado;
        private System.Windows.Forms.SplitContainer     splitMain;
        private Telerik.WinControls.UI.RadLabel         lblTituloArca;
        private Telerik.WinControls.UI.RadGridView      gridComprobantes;
        private Telerik.WinControls.UI.RadLabel         lblConciliacion;
        private Telerik.WinControls.UI.RadGridView      gridConciliacion;
        private Telerik.WinControls.UI.RadLabel         lblEstadoConciliacion;
    }
}
