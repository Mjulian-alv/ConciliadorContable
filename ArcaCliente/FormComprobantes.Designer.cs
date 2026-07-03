namespace ArcaCliente
{
    partial class FormComprobantes
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
            Telerik.WinControls.UI.TableViewDefinition tableViewDefinition1 = new Telerik.WinControls.UI.TableViewDefinition();
            Telerik.WinControls.UI.TableViewDefinition tableViewDefinition2 = new Telerik.WinControls.UI.TableViewDefinition();
            pnlTop = new Telerik.WinControls.UI.RadPanel();
            lblTitulo = new Telerik.WinControls.UI.RadLabel();
            lblUsuario = new Telerik.WinControls.UI.RadLabel();
            btnOffline = new Telerik.WinControls.UI.RadButton();
            btnLogout = new Telerik.WinControls.UI.RadButton();
            pnlFiltros = new Telerik.WinControls.UI.RadPanel();
            lblPerfil = new Telerik.WinControls.UI.RadLabel();
            cmbPerfiles = new Telerik.WinControls.UI.RadDropDownList();
            btnGestionarPerfiles = new Telerik.WinControls.UI.RadButton();
            lblCuil = new Telerik.WinControls.UI.RadLabel();
            txtCuil = new Telerik.WinControls.UI.RadTextBox();
            lblClaveAfip = new Telerik.WinControls.UI.RadLabel();
            txtClaveAfip = new Telerik.WinControls.UI.RadTextBox();
            lblCuitRepresentada = new Telerik.WinControls.UI.RadLabel();
            txtCuitRepresentada = new Telerik.WinControls.UI.RadTextBox();
            lblFechaInicio = new Telerik.WinControls.UI.RadLabel();
            dtpFechaInicio = new Telerik.WinControls.UI.RadDateTimePicker();
            lblFechaFin = new Telerik.WinControls.UI.RadLabel();
            dtpFechaFin = new Telerik.WinControls.UI.RadDateTimePicker();
            btnExportar = new Telerik.WinControls.UI.RadButton();
            btnCancelar = new Telerik.WinControls.UI.RadButton();
            lblEstado = new Telerik.WinControls.UI.RadLabel();
            btnProcesarSoloArca = new Telerik.WinControls.UI.RadButton();
            btnExportarConciliacion = new Telerik.WinControls.UI.RadButton();
            lblIntegracion = new Telerik.WinControls.UI.RadLabel();
            gridComprobantes = new Telerik.WinControls.UI.RadGridView();
            splitMain = new System.Windows.Forms.SplitContainer();
            gridConciliacion = new Telerik.WinControls.UI.RadGridView();
            lblEstadoConciliacion = new Telerik.WinControls.UI.RadLabel();
            lblConciliacion = new Telerik.WinControls.UI.RadLabel();
            ((System.ComponentModel.ISupportInitialize)pnlTop).BeginInit();
            pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lblTitulo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblUsuario).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnOffline).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnLogout).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlFiltros).BeginInit();
            pnlFiltros.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lblPerfil).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbPerfiles).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnGestionarPerfiles).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblCuil).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtCuil).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblClaveAfip).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtClaveAfip).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblCuitRepresentada).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtCuitRepresentada).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblFechaInicio).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dtpFechaInicio).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblFechaFin).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dtpFechaFin).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnExportar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnCancelar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblEstado).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnProcesarSoloArca).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnExportarConciliacion).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblIntegracion).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridComprobantes).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridComprobantes.MasterTemplate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
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
            pnlTop.Controls.Add(lblUsuario);
            pnlTop.Controls.Add(btnOffline);
            pnlTop.Controls.Add(btnLogout);
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
            lblTitulo.Size = new System.Drawing.Size(216, 25);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "ARCA – Mis Comprobantes";
            // 
            // lblUsuario
            // 
            lblUsuario.Location = new System.Drawing.Point(900, 16);
            lblUsuario.Name = "lblUsuario";
            lblUsuario.Size = new System.Drawing.Size(2, 2);
            lblUsuario.TabIndex = 1;
            // 
            // btnOffline
            // 
            btnOffline.Location = new System.Drawing.Point(745, 10);
            btnOffline.Name = "btnOffline";
            btnOffline.Size = new System.Drawing.Size(120, 28);
            btnOffline.TabIndex = 0;
            btnOffline.Text = "Modo Offline...";
            btnOffline.Click += BtnOffline_Click;
            // 
            // btnLogout
            // 
            btnLogout.Location = new System.Drawing.Point(1060, 12);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new System.Drawing.Size(78, 28);
            btnLogout.TabIndex = 0;
            btnLogout.Text = "Cerrar sesión";
            btnLogout.Click += btnLogout_Click;
            // 
            // pnlFiltros
            // 
            pnlFiltros.Controls.Add(lblPerfil);
            pnlFiltros.Controls.Add(cmbPerfiles);
            pnlFiltros.Controls.Add(btnGestionarPerfiles);
            pnlFiltros.Controls.Add(lblCuil);
            pnlFiltros.Controls.Add(txtCuil);
            pnlFiltros.Controls.Add(lblClaveAfip);
            pnlFiltros.Controls.Add(txtClaveAfip);
            pnlFiltros.Controls.Add(lblCuitRepresentada);
            pnlFiltros.Controls.Add(txtCuitRepresentada);
            pnlFiltros.Controls.Add(lblFechaInicio);
            pnlFiltros.Controls.Add(dtpFechaInicio);
            pnlFiltros.Controls.Add(lblFechaFin);
            pnlFiltros.Controls.Add(dtpFechaFin);
            pnlFiltros.Controls.Add(btnExportar);
            pnlFiltros.Controls.Add(btnCancelar);
            pnlFiltros.Controls.Add(lblEstado);
            pnlFiltros.Controls.Add(btnProcesarSoloArca);
            pnlFiltros.Controls.Add(btnExportarConciliacion);
            pnlFiltros.Controls.Add(lblIntegracion);
            pnlFiltros.Dock = System.Windows.Forms.DockStyle.Left;
            pnlFiltros.Location = new System.Drawing.Point(0, 50);
            pnlFiltros.Name = "pnlFiltros";
            pnlFiltros.Size = new System.Drawing.Size(270, 650);
            pnlFiltros.TabIndex = 1;
            // 
            // lblPerfil
            // 
            lblPerfil.Location = new System.Drawing.Point(12, 16);
            lblPerfil.Name = "lblPerfil";
            lblPerfil.Size = new System.Drawing.Size(62, 18);
            lblPerfil.TabIndex = 0;
            lblPerfil.Text = "Perfil fiscal:";
            // 
            // cmbPerfiles
            // 
            cmbPerfiles.Location = new System.Drawing.Point(12, 36);
            cmbPerfiles.Name = "cmbPerfiles";
            cmbPerfiles.NullText = "-- Seleccionar perfil --";
            cmbPerfiles.Size = new System.Drawing.Size(244, 24);
            cmbPerfiles.TabIndex = 0;
            cmbPerfiles.SelectedIndexChanged += CmbPerfiles_SelectedIndexChanged;
            // 
            // btnGestionarPerfiles
            // 
            btnGestionarPerfiles.Location = new System.Drawing.Point(12, 64);
            btnGestionarPerfiles.Name = "btnGestionarPerfiles";
            btnGestionarPerfiles.Size = new System.Drawing.Size(244, 24);
            btnGestionarPerfiles.TabIndex = 1;
            btnGestionarPerfiles.Text = "Gestionar perfiles...";
            btnGestionarPerfiles.Click += BtnGestionarPerfiles_Click;
            // 
            // lblCuil
            // 
            lblCuil.Location = new System.Drawing.Point(12, 96);
            lblCuil.Name = "lblCuil";
            lblCuil.Size = new System.Drawing.Size(132, 18);
            lblCuil.TabIndex = 2;
            lblCuil.Text = "CUIL/CUIT contribuyente:";
            // 
            // txtCuil
            // 
            txtCuil.Location = new System.Drawing.Point(12, 116);
            txtCuil.Name = "txtCuil";
            txtCuil.Size = new System.Drawing.Size(244, 24);
            txtCuil.TabIndex = 0;
            // 
            // lblClaveAfip
            // 
            lblClaveAfip.Location = new System.Drawing.Point(12, 154);
            lblClaveAfip.Name = "lblClaveAfip";
            lblClaveAfip.Size = new System.Drawing.Size(95, 18);
            lblClaveAfip.TabIndex = 3;
            lblClaveAfip.Text = "Clave fiscal ARCA:";
            // 
            // txtClaveAfip
            // 
            txtClaveAfip.Location = new System.Drawing.Point(12, 174);
            txtClaveAfip.Name = "txtClaveAfip";
            txtClaveAfip.Size = new System.Drawing.Size(244, 24);
            txtClaveAfip.TabIndex = 1;
            // 
            // lblCuitRepresentada
            // 
            lblCuitRepresentada.Location = new System.Drawing.Point(12, 212);
            lblCuitRepresentada.Name = "lblCuitRepresentada";
            lblCuitRepresentada.Size = new System.Drawing.Size(172, 18);
            lblCuitRepresentada.TabIndex = 4;
            lblCuitRepresentada.Text = "CUIT representada (con guiones):";
            // 
            // txtCuitRepresentada
            // 
            txtCuitRepresentada.Location = new System.Drawing.Point(12, 232);
            txtCuitRepresentada.Name = "txtCuitRepresentada";
            txtCuitRepresentada.Size = new System.Drawing.Size(244, 24);
            txtCuitRepresentada.TabIndex = 2;
            // 
            // lblFechaInicio
            // 
            lblFechaInicio.Location = new System.Drawing.Point(12, 270);
            lblFechaInicio.Name = "lblFechaInicio";
            lblFechaInicio.Size = new System.Drawing.Size(67, 18);
            lblFechaInicio.TabIndex = 5;
            lblFechaInicio.Text = "Fecha inicio:";
            // 
            // dtpFechaInicio
            // 
            dtpFechaInicio.Location = new System.Drawing.Point(12, 290);
            dtpFechaInicio.Name = "dtpFechaInicio";
            dtpFechaInicio.Size = new System.Drawing.Size(244, 24);
            dtpFechaInicio.TabIndex = 3;
            dtpFechaInicio.TabStop = false;
            dtpFechaInicio.Text = "jueves, 12 de marzo de 2026";
            dtpFechaInicio.Value = new System.DateTime(2026, 3, 12, 10, 3, 33, 332);
            // 
            // lblFechaFin
            // 
            lblFechaFin.Location = new System.Drawing.Point(12, 328);
            lblFechaFin.Name = "lblFechaFin";
            lblFechaFin.Size = new System.Drawing.Size(53, 18);
            lblFechaFin.TabIndex = 6;
            lblFechaFin.Text = "Fecha fin:";
            // 
            // dtpFechaFin
            // 
            dtpFechaFin.Location = new System.Drawing.Point(12, 348);
            dtpFechaFin.Name = "dtpFechaFin";
            dtpFechaFin.Size = new System.Drawing.Size(244, 24);
            dtpFechaFin.TabIndex = 4;
            dtpFechaFin.TabStop = false;
            dtpFechaFin.Text = "jueves, 12 de marzo de 2026";
            dtpFechaFin.Value = new System.DateTime(2026, 3, 12, 10, 3, 33, 364);
            // 
            // btnExportar
            // 
            btnExportar.Location = new System.Drawing.Point(12, 394);
            btnExportar.Name = "btnExportar";
            btnExportar.Size = new System.Drawing.Size(244, 38);
            btnExportar.TabIndex = 5;
            btnExportar.Text = "OBTENER COMPROBANTES";
            btnExportar.Click += btnExportar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.Location = new System.Drawing.Point(12, 440);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new System.Drawing.Size(244, 28);
            btnCancelar.TabIndex = 6;
            btnCancelar.Text = "Cancelar";
            btnCancelar.Visible = false;
            btnCancelar.Click += btnCancelar_Click;
            // 
            // lblEstado
            // 
            lblEstado.AutoSize = false;
            lblEstado.Location = new System.Drawing.Point(12, 482);
            lblEstado.Name = "lblEstado";
            lblEstado.Size = new System.Drawing.Size(244, 16);
            lblEstado.TabIndex = 7;
            // 
            // btnProcesarSoloArca
            // 
            btnProcesarSoloArca.Location = new System.Drawing.Point(12, 542);
            btnProcesarSoloArca.Name = "btnProcesarSoloArca";
            btnProcesarSoloArca.Size = new System.Drawing.Size(244, 32);
            btnProcesarSoloArca.TabIndex = 7;
            btnProcesarSoloArca.Text = "Procesar Solo ARCA...";
            btnProcesarSoloArca.Visible = false;
            btnProcesarSoloArca.Click += btnProcesarSoloArca_Click;
            // 
            // btnExportarConciliacion
            // 
            btnExportarConciliacion.Location = new System.Drawing.Point(12, 506);
            btnExportarConciliacion.Name = "btnExportarConciliacion";
            btnExportarConciliacion.Size = new System.Drawing.Size(244, 28);
            btnExportarConciliacion.TabIndex = 9;
            btnExportarConciliacion.Text = "Exportar conciliación...";
            btnExportarConciliacion.Visible = false;
            btnExportarConciliacion.Click += BtnExportarConciliacion_Click;
            // 
            // lblIntegracion
            // 
            lblIntegracion.AutoSize = false;
            lblIntegracion.Location = new System.Drawing.Point(12, 582);
            lblIntegracion.Name = "lblIntegracion";
            lblIntegracion.Size = new System.Drawing.Size(244, 36);
            lblIntegracion.TabIndex = 8;
            // 
            // gridComprobantes
            // 
            gridComprobantes.Dock = System.Windows.Forms.DockStyle.Fill;
            gridComprobantes.Location = new System.Drawing.Point(0, 0);
            // 
            // 
            // 
            gridComprobantes.MasterTemplate.ViewDefinition = tableViewDefinition1;
            gridComprobantes.Name = "gridComprobantes";
            gridComprobantes.Size = new System.Drawing.Size(880, 461);
            gridComprobantes.TabIndex = 0;
            // 
            // splitMain
            // 
            splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitMain.Location = new System.Drawing.Point(270, 50);
            splitMain.Name = "splitMain";
            splitMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(gridComprobantes);
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(gridConciliacion);
            splitMain.Panel2.Controls.Add(lblEstadoConciliacion);
            splitMain.Panel2.Controls.Add(lblConciliacion);
            splitMain.Size = new System.Drawing.Size(880, 650);
            splitMain.SplitterDistance = 461;
            splitMain.TabIndex = 2;
            // 
            // gridConciliacion
            // 
            gridConciliacion.Dock = System.Windows.Forms.DockStyle.Fill;
            gridConciliacion.Location = new System.Drawing.Point(0, 24);
            // 
            // 
            // 
            gridConciliacion.MasterTemplate.ViewDefinition = tableViewDefinition2;
            gridConciliacion.Name = "gridConciliacion";
            gridConciliacion.Size = new System.Drawing.Size(880, 141);
            gridConciliacion.TabIndex = 0;
            // 
            // lblEstadoConciliacion
            // 
            lblEstadoConciliacion.AutoSize = false;
            lblEstadoConciliacion.Dock = System.Windows.Forms.DockStyle.Bottom;
            lblEstadoConciliacion.Location = new System.Drawing.Point(0, 165);
            lblEstadoConciliacion.Name = "lblEstadoConciliacion";
            lblEstadoConciliacion.Padding = new System.Windows.Forms.Padding(4, 0, 0, 2);
            lblEstadoConciliacion.Size = new System.Drawing.Size(880, 20);
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
            lblConciliacion.Size = new System.Drawing.Size(880, 24);
            lblConciliacion.TabIndex = 2;
            lblConciliacion.Text = "Conciliación con sistema local";
            // 
            // FormComprobantes
            // 
            AutoScaleBaseSize = new System.Drawing.Size(7, 15);
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1150, 700);
            Controls.Add(splitMain);
            Controls.Add(pnlFiltros);
            Controls.Add(pnlTop);
            MinimumSize = new System.Drawing.Size(900, 600);
            Name = "FormComprobantes";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "ARCA Cliente";
            ((System.ComponentModel.ISupportInitialize)pnlTop).EndInit();
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)lblTitulo).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblUsuario).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnOffline).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnLogout).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlFiltros).EndInit();
            pnlFiltros.ResumeLayout(false);
            pnlFiltros.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)lblPerfil).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbPerfiles).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnGestionarPerfiles).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblCuil).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtCuil).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblClaveAfip).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtClaveAfip).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblCuitRepresentada).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtCuitRepresentada).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblFechaInicio).EndInit();
            ((System.ComponentModel.ISupportInitialize)dtpFechaInicio).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblFechaFin).EndInit();
            ((System.ComponentModel.ISupportInitialize)dtpFechaFin).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnExportar).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnCancelar).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblEstado).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnProcesarSoloArca).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnExportarConciliacion).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblIntegracion).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridComprobantes.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridComprobantes).EndInit();
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridConciliacion.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridConciliacion).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblEstadoConciliacion).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblConciliacion).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        private Telerik.WinControls.UI.RadPanel           pnlTop;
        private Telerik.WinControls.UI.RadLabel           lblTitulo;
        private Telerik.WinControls.UI.RadLabel           lblUsuario;
        private Telerik.WinControls.UI.RadButton          btnOffline;
        private Telerik.WinControls.UI.RadButton          btnLogout;
        private Telerik.WinControls.UI.RadPanel           pnlFiltros;
        private Telerik.WinControls.UI.RadLabel           lblCuil;
        private Telerik.WinControls.UI.RadTextBox         txtCuil;
        private Telerik.WinControls.UI.RadLabel           lblClaveAfip;
        private Telerik.WinControls.UI.RadTextBox         txtClaveAfip;
        private Telerik.WinControls.UI.RadLabel           lblCuitRepresentada;
        private Telerik.WinControls.UI.RadTextBox         txtCuitRepresentada;
        private Telerik.WinControls.UI.RadLabel           lblFechaInicio;
        private Telerik.WinControls.UI.RadDateTimePicker  dtpFechaInicio;
        private Telerik.WinControls.UI.RadLabel           lblFechaFin;
        private Telerik.WinControls.UI.RadDateTimePicker  dtpFechaFin;
        private Telerik.WinControls.UI.RadButton          btnExportar;
        private Telerik.WinControls.UI.RadButton          btnCancelar;
        private Telerik.WinControls.UI.RadLabel           lblEstado;
        private Telerik.WinControls.UI.RadLabel           lblPerfil;
        private Telerik.WinControls.UI.RadDropDownList    cmbPerfiles;
        private Telerik.WinControls.UI.RadButton          btnGestionarPerfiles;
        private Telerik.WinControls.UI.RadButton          btnProcesarSoloArca;
        private Telerik.WinControls.UI.RadButton          btnExportarConciliacion;
        private Telerik.WinControls.UI.RadLabel           lblIntegracion;
        private Telerik.WinControls.UI.RadGridView        gridComprobantes;
        private System.Windows.Forms.SplitContainer       splitMain;
        private Telerik.WinControls.UI.RadLabel           lblConciliacion;
        private Telerik.WinControls.UI.RadLabel           lblEstadoConciliacion;
        private Telerik.WinControls.UI.RadGridView        gridConciliacion;
    }
}
