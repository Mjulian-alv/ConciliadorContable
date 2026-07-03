namespace AgrupadorConceptos
{
    partial class ProcesadorForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            Telerik.WinControls.UI.TableViewDefinition tableViewDefinition1 = new Telerik.WinControls.UI.TableViewDefinition();
            pnlTop = new System.Windows.Forms.Panel();
            lblTitulo = new System.Windows.Forms.Label();
            lblPerfil = new System.Windows.Forms.Label();
            cboPerfiles = new System.Windows.Forms.ComboBox();
            btnNuevoPerfil = new System.Windows.Forms.Button();
            btnEditarPerfil = new System.Windows.Forms.Button();
            btnCargarArchivo = new System.Windows.Forms.Button();
            btnHomologar = new System.Windows.Forms.Button();
            btnExportarConsolidado = new System.Windows.Forms.Button();
            btnHomologacionMasiva = new System.Windows.Forms.Button();
            lblArchivos = new System.Windows.Forms.Label();
            cmbArchivos = new System.Windows.Forms.ComboBox();
            btnCargarSesion = new System.Windows.Forms.Button();
            btnBorrarSesion = new System.Windows.Forms.Button();
            btnGestionarHomologaciones = new System.Windows.Forms.Button();
            lblTotalRegistros = new System.Windows.Forms.Label();
            dgvDatos = new Telerik.WinControls.UI.RadGridView();
            SPB_Importar = new Telerik.WinControls.UI.RadStepProgressBar();
            PB_Importar = new Telerik.WinControls.UI.RadProgressBar();
            pnlProgreso = new System.Windows.Forms.Panel();
            pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDatos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvDatos.MasterTemplate).BeginInit();
            dgvDatos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)SPB_Importar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)PB_Importar).BeginInit();
            pnlProgreso.SuspendLayout();
            SuspendLayout();
            // 
            // pnlTop
            // 
            pnlTop.Controls.Add(lblTitulo);
            pnlTop.Controls.Add(lblPerfil);
            pnlTop.Controls.Add(cboPerfiles);
            pnlTop.Controls.Add(btnNuevoPerfil);
            pnlTop.Controls.Add(btnEditarPerfil);
            pnlTop.Controls.Add(btnCargarArchivo);
            pnlTop.Controls.Add(btnHomologar);
            pnlTop.Controls.Add(btnExportarConsolidado);
            pnlTop.Controls.Add(btnHomologacionMasiva);
            pnlTop.Controls.Add(lblArchivos);
            pnlTop.Controls.Add(cmbArchivos);
            pnlTop.Controls.Add(btnCargarSesion);
            pnlTop.Controls.Add(btnBorrarSesion);
            pnlTop.Controls.Add(btnGestionarHomologaciones);
            pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            pnlTop.Location = new System.Drawing.Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new System.Drawing.Size(1000, 160);
            pnlTop.TabIndex = 1;
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            lblTitulo.Location = new System.Drawing.Point(12, 9);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new System.Drawing.Size(372, 30);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Procesador de Movimientos Banco";
            // 
            // lblPerfil
            // 
            lblPerfil.AutoSize = true;
            lblPerfil.Location = new System.Drawing.Point(14, 55);
            lblPerfil.Name = "lblPerfil";
            lblPerfil.Size = new System.Drawing.Size(89, 15);
            lblPerfil.TabIndex = 1;
            lblPerfil.Text = "Perfil de Banco:";
            // 
            // cboPerfiles
            // 
            cboPerfiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboPerfiles.FormattingEnabled = true;
            cboPerfiles.Location = new System.Drawing.Point(110, 52);
            cboPerfiles.Name = "cboPerfiles";
            cboPerfiles.Size = new System.Drawing.Size(250, 23);
            cboPerfiles.TabIndex = 2;
            // 
            // btnNuevoPerfil
            // 
            btnNuevoPerfil.Location = new System.Drawing.Point(370, 51);
            btnNuevoPerfil.Name = "btnNuevoPerfil";
            btnNuevoPerfil.Size = new System.Drawing.Size(120, 25);
            btnNuevoPerfil.TabIndex = 3;
            btnNuevoPerfil.Text = "+ Nuevo Perfil";
            btnNuevoPerfil.UseVisualStyleBackColor = true;
            btnNuevoPerfil.Click += btnNuevoPerfil_Click;
            // 
            // btnEditarPerfil
            // 
            btnEditarPerfil.Location = new System.Drawing.Point(495, 51);
            btnEditarPerfil.Name = "btnEditarPerfil";
            btnEditarPerfil.Size = new System.Drawing.Size(120, 25);
            btnEditarPerfil.TabIndex = 4;
            btnEditarPerfil.Text = "Editar Perfil";
            btnEditarPerfil.UseVisualStyleBackColor = true;
            btnEditarPerfil.Click += btnEditarPerfil_Click;
            // 
            // btnCargarArchivo
            // 
            btnCargarArchivo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnCargarArchivo.Location = new System.Drawing.Point(17, 85);
            btnCargarArchivo.Name = "btnCargarArchivo";
            btnCargarArchivo.Size = new System.Drawing.Size(200, 30);
            btnCargarArchivo.TabIndex = 5;
            btnCargarArchivo.Text = "1. Cargar Archivo de Banco...";
            btnCargarArchivo.UseVisualStyleBackColor = true;
            btnCargarArchivo.Click += btnCargarArchivo_Click;
            // 
            // btnHomologar
            // 
            btnHomologar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnHomologar.Location = new System.Drawing.Point(230, 85);
            btnHomologar.Name = "btnHomologar";
            btnHomologar.Size = new System.Drawing.Size(250, 30);
            btnHomologar.TabIndex = 6;
            btnHomologar.Text = "2. Homologar Fila Seleccionada...";
            btnHomologar.UseVisualStyleBackColor = true;
            btnHomologar.Click += btnHomologar_Click;
            // 
            // btnExportarConsolidado
            // 
            btnExportarConsolidado.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnExportarConsolidado.Location = new System.Drawing.Point(490, 85);
            btnExportarConsolidado.Name = "btnExportarConsolidado";
            btnExportarConsolidado.Size = new System.Drawing.Size(200, 30);
            btnExportarConsolidado.TabIndex = 12;
            btnExportarConsolidado.Text = "3. Exportar Consolidado...";
            btnExportarConsolidado.UseVisualStyleBackColor = true;
            btnExportarConsolidado.Click += btnExportarConsolidado_Click;
            // 
            // btnHomologacionMasiva
            // 
            btnHomologacionMasiva.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnHomologacionMasiva.Location = new System.Drawing.Point(700, 121);
            btnHomologacionMasiva.Name = "btnHomologacionMasiva";
            btnHomologacionMasiva.Size = new System.Drawing.Size(200, 25);
            btnHomologacionMasiva.TabIndex = 13;
            btnHomologacionMasiva.Text = "Ver Pendientes Agrupados...";
            btnHomologacionMasiva.UseVisualStyleBackColor = true;
            btnHomologacionMasiva.Click += btnHomologacionMasiva_Click;
            // 
            // lblArchivos
            // 
            lblArchivos.AutoSize = true;
            lblArchivos.Location = new System.Drawing.Point(14, 125);
            lblArchivos.Name = "lblArchivos";
            lblArchivos.Size = new System.Drawing.Size(93, 15);
            lblArchivos.TabIndex = 7;
            lblArchivos.Text = "Archivos Leídos:";
            // 
            // cmbArchivos
            // 
            cmbArchivos.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbArchivos.FormattingEnabled = true;
            cmbArchivos.Location = new System.Drawing.Point(110, 122);
            cmbArchivos.Name = "cmbArchivos";
            cmbArchivos.Size = new System.Drawing.Size(370, 23);
            cmbArchivos.TabIndex = 8;
            // 
            // btnCargarSesion
            // 
            btnCargarSesion.Location = new System.Drawing.Point(490, 121);
            btnCargarSesion.Name = "btnCargarSesion";
            btnCargarSesion.Size = new System.Drawing.Size(100, 25);
            btnCargarSesion.TabIndex = 9;
            btnCargarSesion.Text = "Cargar Datos";
            btnCargarSesion.UseVisualStyleBackColor = true;
            btnCargarSesion.Click += btnCargarSesion_Click;
            // 
            // btnBorrarSesion
            // 
            btnBorrarSesion.Location = new System.Drawing.Point(600, 121);
            btnBorrarSesion.Name = "btnBorrarSesion";
            btnBorrarSesion.Size = new System.Drawing.Size(100, 25);
            btnBorrarSesion.TabIndex = 10;
            btnBorrarSesion.Text = "Borrar Archivo";
            btnBorrarSesion.UseVisualStyleBackColor = true;
            btnBorrarSesion.Click += btnBorrarSesion_Click;
            // 
            // btnGestionarHomologaciones
            // 
            btnGestionarHomologaciones.Location = new System.Drawing.Point(700, 85);
            btnGestionarHomologaciones.Name = "btnGestionarHomologaciones";
            btnGestionarHomologaciones.Size = new System.Drawing.Size(200, 30);
            btnGestionarHomologaciones.TabIndex = 11;
            btnGestionarHomologaciones.Text = "Gestionar Homologaciones";
            btnGestionarHomologaciones.UseVisualStyleBackColor = true;
            btnGestionarHomologaciones.Click += btnGestionarHomologaciones_Click;
            // 
            // lblTotalRegistros
            // 
            lblTotalRegistros.Dock = System.Windows.Forms.DockStyle.Bottom;
            lblTotalRegistros.Location = new System.Drawing.Point(0, 536);
            lblTotalRegistros.Name = "lblTotalRegistros";
            lblTotalRegistros.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            lblTotalRegistros.Size = new System.Drawing.Size(1000, 25);
            lblTotalRegistros.TabIndex = 2;
            lblTotalRegistros.Text = "Registros leídos: 0";
            lblTotalRegistros.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dgvDatos
            // 
            dgvDatos.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvDatos.Location = new System.Drawing.Point(0, 160);
            // 
            // 
            // 
            dgvDatos.MasterTemplate.AllowAddNewRow = false;
            dgvDatos.MasterTemplate.AllowDeleteRow = false;
            dgvDatos.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            dgvDatos.MasterTemplate.EnableFiltering = true;
            dgvDatos.MasterTemplate.ViewDefinition = tableViewDefinition1;
            dgvDatos.Name = "dgvDatos";
            dgvDatos.Size = new System.Drawing.Size(1000, 376);
            dgvDatos.TabIndex = 0;
            // 
            // pnlProgreso
            // 
            pnlProgreso.Controls.Add(SPB_Importar);
            pnlProgreso.Controls.Add(PB_Importar);
            pnlProgreso.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlProgreso.BackColor = System.Drawing.Color.FromArgb(245, 248, 252);
            pnlProgreso.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pnlProgreso.Height = 160;
            pnlProgreso.Name = "pnlProgreso";
            pnlProgreso.Visible = false;
            pnlProgreso.Padding = new System.Windows.Forms.Padding(8);
            // 
            // SPB_Importar
            // 
            SPB_Importar.Dock = System.Windows.Forms.DockStyle.Fill;
            SPB_Importar.Name = "SPB_Importar";
            SPB_Importar.TabIndex = 14;
            SPB_Importar.Text = "";
            // 
            // PB_Importar
            // 
            PB_Importar.Dock = System.Windows.Forms.DockStyle.Top;
            PB_Importar.Height = 28;
            PB_Importar.Name = "PB_Importar";
            PB_Importar.TabIndex = 15;
            PB_Importar.Text = "";
            // 
            // ProcesadorForm
            // 
            ClientSize = new System.Drawing.Size(1000, 561);
            Controls.Add(dgvDatos);
            Controls.Add(pnlProgreso);
            Controls.Add(pnlTop);
            Controls.Add(lblTotalRegistros);
            Name = "ProcesadorForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Agrupador Conceptos Bancarios";
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDatos.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvDatos).EndInit();
            dgvDatos.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)SPB_Importar).EndInit();
            ((System.ComponentModel.ISupportInitialize)PB_Importar).EndInit();
            pnlProgreso.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblPerfil;
        private System.Windows.Forms.ComboBox cboPerfiles;
        private System.Windows.Forms.Button btnNuevoPerfil;
        private System.Windows.Forms.Button btnEditarPerfil;
        private System.Windows.Forms.Button btnCargarArchivo;
        private System.Windows.Forms.Button btnHomologar;
        private System.Windows.Forms.Button btnExportarConsolidado;
        private System.Windows.Forms.Button btnHomologacionMasiva;
        private System.Windows.Forms.Label lblArchivos;
        private System.Windows.Forms.ComboBox cmbArchivos;
        private System.Windows.Forms.Button btnCargarSesion;
        private System.Windows.Forms.Button btnBorrarSesion;
        private System.Windows.Forms.Button btnGestionarHomologaciones;
        private System.Windows.Forms.Label lblTotalRegistros;
        private Telerik.WinControls.UI.RadGridView dgvDatos;
        private Telerik.WinControls.UI.RadStepProgressBar SPB_Importar;
        private Telerik.WinControls.UI.RadProgressBar PB_Importar;
        private System.Windows.Forms.Panel pnlProgreso;
    }
}