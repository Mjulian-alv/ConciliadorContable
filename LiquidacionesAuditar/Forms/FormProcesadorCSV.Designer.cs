namespace LiquidacionesAuditar
{
    partial class FormProcesadorCSV
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            Telerik.WinControls.UI.TableViewDefinition tableViewDefinition1 = new Telerik.WinControls.UI.TableViewDefinition();
            panelTop = new System.Windows.Forms.Panel();
            btn_Ayuda = new Telerik.WinControls.UI.RadButton();
            btn_Filtros = new Telerik.WinControls.UI.RadButton();
            lblMarca2 = new System.Windows.Forms.Label();
            cmbMarca = new Telerik.WinControls.UI.RadDropDownList();
            lblNroBase = new System.Windows.Forms.Label();
            txtNroBase = new Telerik.WinControls.UI.RadTextBox();
            lblCSV = new System.Windows.Forms.Label();
            txtRutaCSV = new Telerik.WinControls.UI.RadTextBox();
            btnSeleccionarCSV = new Telerik.WinControls.UI.RadButton();
            lblSepDec = new System.Windows.Forms.Label();
            txtSepDec = new Telerik.WinControls.UI.RadTextBox();
            lblSepMil = new System.Windows.Forms.Label();
            txtSepMil = new Telerik.WinControls.UI.RadTextBox();
            lblEstado = new System.Windows.Forms.Label();
            lblValidacion = new System.Windows.Forms.Label();
            btnVerDiferencias = new Telerik.WinControls.UI.RadButton();
            btnExportar = new Telerik.WinControls.UI.RadButton();
            splitVista = new System.Windows.Forms.SplitContainer();
            grpPreview = new System.Windows.Forms.GroupBox();
            gridPreview = new Telerik.WinControls.UI.RadGridView();
            grpSalida = new System.Windows.Forms.GroupBox();
            radEportar = new Telerik.WinControls.UI.RadButton();
            txtPreviewSalida = new System.Windows.Forms.RichTextBox();
            panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)btn_Ayuda).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_Filtros).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbMarca).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtNroBase).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtRutaCSV).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnSeleccionarCSV).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtSepDec).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtSepMil).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnVerDiferencias).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnExportar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitVista).BeginInit();
            splitVista.Panel1.SuspendLayout();
            splitVista.Panel2.SuspendLayout();
            splitVista.SuspendLayout();
            grpPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridPreview).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridPreview.MasterTemplate).BeginInit();
            grpSalida.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)radEportar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            SuspendLayout();
            // 
            // panelTop
            // 
            panelTop.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            panelTop.Controls.Add(btn_Ayuda);
            panelTop.Controls.Add(btn_Filtros);
            panelTop.Controls.Add(lblMarca2);
            panelTop.Controls.Add(cmbMarca);
            panelTop.Controls.Add(lblNroBase);
            panelTop.Controls.Add(txtNroBase);
            panelTop.Controls.Add(lblCSV);
            panelTop.Controls.Add(txtRutaCSV);
            panelTop.Controls.Add(btnSeleccionarCSV);
            panelTop.Controls.Add(lblSepDec);
            panelTop.Controls.Add(txtSepDec);
            panelTop.Controls.Add(lblSepMil);
            panelTop.Controls.Add(txtSepMil);
            panelTop.Controls.Add(lblEstado);
            panelTop.Controls.Add(lblValidacion);
            panelTop.Controls.Add(btnVerDiferencias);
            panelTop.Controls.Add(btnExportar);
            panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            panelTop.Location = new System.Drawing.Point(0, 0);
            panelTop.Name = "panelTop";
            panelTop.Padding = new System.Windows.Forms.Padding(10, 8, 10, 6);
            panelTop.Size = new System.Drawing.Size(1200, 92);
            panelTop.TabIndex = 1;
            // 
            // btn_Ayuda
            // 
            btn_Ayuda.Font = new System.Drawing.Font("Segoe UI Symbol", 10F, System.Drawing.FontStyle.Bold);
            btn_Ayuda.Location = new System.Drawing.Point(1130, 8);
            btn_Ayuda.Name = "btn_Ayuda";
            btn_Ayuda.Size = new System.Drawing.Size(50, 26);
            btn_Ayuda.TabIndex = 16;
            btn_Ayuda.Text = "?";
            btn_Ayuda.AccessibleName = "Ayuda";
            btn_Ayuda.AccessibleDescription = "Abre la guía de uso de esta pantalla";
            btn_Ayuda.Click += btn_Ayuda_Click;
            // 
            // btn_Filtros
            //
            btn_Filtros.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btn_Filtros.Location = new System.Drawing.Point(760, 57);
            btn_Filtros.Name = "btn_Filtros";
            btn_Filtros.Size = new System.Drawing.Size(150, 26);
            btn_Filtros.TabIndex = 15;
            btn_Filtros.Text = "Definir Filtros";
            btn_Filtros.Click += btn_Filtros_Click;
            // 
            // lblMarca2
            // 
            lblMarca2.AutoSize = true;
            lblMarca2.Location = new System.Drawing.Point(10, 10);
            lblMarca2.Name = "lblMarca2";
            lblMarca2.Size = new System.Drawing.Size(107, 15);
            lblMarca2.TabIndex = 0;
            lblMarca2.Text = "Marca/Procesador:";
            // 
            // cmbMarca
            // 
            cmbMarca.Location = new System.Drawing.Point(148, 8);
            cmbMarca.Name = "cmbMarca";
            cmbMarca.Size = new System.Drawing.Size(240, 24);
            cmbMarca.TabIndex = 1;
            cmbMarca.SelectedIndexChanged += cmbMarca_SelectedIndexChanged;
            // 
            // lblNroBase
            // 
            lblNroBase.AutoSize = true;
            lblNroBase.Location = new System.Drawing.Point(400, 10);
            lblNroBase.Name = "lblNroBase";
            lblNroBase.Size = new System.Drawing.Size(122, 15);
            lblNroBase.TabIndex = 2;
            lblNroBase.Text = "Nro. base liquidación:";
            // 
            // txtNroBase
            // 
            txtNroBase.Location = new System.Drawing.Point(555, 8);
            txtNroBase.Name = "txtNroBase";
            txtNroBase.Size = new System.Drawing.Size(100, 24);
            txtNroBase.TabIndex = 3;
            txtNroBase.Text = "1000";
            // 
            // lblCSV
            // 
            lblCSV.AutoSize = true;
            lblCSV.Location = new System.Drawing.Point(10, 40);
            lblCSV.Name = "lblCSV";
            lblCSV.Size = new System.Drawing.Size(75, 15);
            lblCSV.TabIndex = 4;
            lblCSV.Text = "Archivo CSV:";
            // 
            // txtRutaCSV
            // 
            txtRutaCSV.Location = new System.Drawing.Point(108, 38);
            txtRutaCSV.Name = "txtRutaCSV";
            txtRutaCSV.NullText = "(ningún archivo seleccionado)";
            txtRutaCSV.ReadOnly = true;
            txtRutaCSV.Size = new System.Drawing.Size(500, 24);
            txtRutaCSV.TabIndex = 5;
            // 
            // btnSeleccionarCSV
            // 
            btnSeleccionarCSV.Location = new System.Drawing.Point(616, 36);
            btnSeleccionarCSV.Name = "btnSeleccionarCSV";
            btnSeleccionarCSV.Size = new System.Drawing.Size(130, 28);
            btnSeleccionarCSV.TabIndex = 6;
            btnSeleccionarCSV.Text = "Seleccionar CSV...";
            btnSeleccionarCSV.Click += btnSeleccionarCSV_Click;
            // 
            // lblSepDec
            // 
            lblSepDec.AutoSize = true;
            lblSepDec.Location = new System.Drawing.Point(670, 10);
            lblSepDec.Name = "lblSepDec";
            lblSepDec.Size = new System.Drawing.Size(77, 15);
            lblSepDec.TabIndex = 7;
            lblSepDec.Text = "Sep. decimal:";
            // 
            // txtSepDec
            // 
            txtSepDec.Font = new System.Drawing.Font("Courier New", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            txtSepDec.Location = new System.Drawing.Point(760, 8);
            txtSepDec.MaxLength = 1;
            txtSepDec.Name = "txtSepDec";
            txtSepDec.NullText = ",";
            txtSepDec.Size = new System.Drawing.Size(23, 31);
            txtSepDec.TabIndex = 8;
            txtSepDec.Text = ",";
            // 
            // lblSepMil
            // 
            lblSepMil.AutoSize = true;
            lblSepMil.Location = new System.Drawing.Point(806, 10);
            lblSepMil.Name = "lblSepMil";
            lblSepMil.Size = new System.Drawing.Size(63, 15);
            lblSepMil.TabIndex = 9;
            lblSepMil.Text = "Sep. miles:";
            // 
            // txtSepMil
            // 
            txtSepMil.Font = new System.Drawing.Font("Courier New", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            txtSepMil.Location = new System.Drawing.Point(886, 8);
            txtSepMil.MaxLength = 1;
            txtSepMil.Name = "txtSepMil";
            txtSepMil.NullText = ".";
            txtSepMil.Size = new System.Drawing.Size(24, 31);
            txtSepMil.TabIndex = 10;
            txtSepMil.Text = ".";
            // 
            // lblEstado
            // 
            lblEstado.Location = new System.Drawing.Point(10, 68);
            lblEstado.Name = "lblEstado";
            lblEstado.Size = new System.Drawing.Size(440, 18);
            lblEstado.TabIndex = 11;
            lblEstado.Text = "Sin archivo cargado.";
            // 
            // lblValidacion
            // 
            lblValidacion.Location = new System.Drawing.Point(10, 68);
            lblValidacion.Name = "lblValidacion";
            lblValidacion.Size = new System.Drawing.Size(440, 18);
            lblValidacion.TabIndex = 12;
            // 
            // btnVerDiferencias
            // 
            btnVerDiferencias.Location = new System.Drawing.Point(460, 62);
            btnVerDiferencias.Name = "btnVerDiferencias";
            btnVerDiferencias.Size = new System.Drawing.Size(150, 26);
            btnVerDiferencias.TabIndex = 13;
            btnVerDiferencias.Text = "Ver diferencias";
            btnVerDiferencias.Visible = false;
            btnVerDiferencias.Click += btnVerDiferencias_Click;
            // 
            // btnExportar
            // 
            btnExportar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnExportar.Location = new System.Drawing.Point(939, 57);
            btnExportar.Name = "btnExportar";
            btnExportar.Size = new System.Drawing.Size(150, 26);
            btnExportar.TabIndex = 14;
            btnExportar.Text = "Procesar Vista Previa";
            btnExportar.Click += btnExportar_Click;
            // 
            // splitVista
            // 
            splitVista.Dock = System.Windows.Forms.DockStyle.Fill;
            splitVista.Location = new System.Drawing.Point(0, 92);
            splitVista.Name = "splitVista";
            // 
            // splitVista.Panel1
            // 
            splitVista.Panel1.Controls.Add(grpPreview);
            // 
            // splitVista.Panel2
            // 
            splitVista.Panel2.Controls.Add(grpSalida);
            splitVista.Size = new System.Drawing.Size(1200, 608);
            splitVista.SplitterDistance = 908;
            splitVista.TabIndex = 0;
            // 
            // grpPreview
            // 
            grpPreview.Controls.Add(gridPreview);
            grpPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            grpPreview.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            grpPreview.Location = new System.Drawing.Point(0, 0);
            grpPreview.Name = "grpPreview";
            grpPreview.Size = new System.Drawing.Size(908, 608);
            grpPreview.TabIndex = 0;
            grpPreview.TabStop = false;
            grpPreview.Text = "Vista previa del CSV  (50 filas · 20 columnas)";
            // 
            // gridPreview
            // 
            gridPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            gridPreview.Location = new System.Drawing.Point(3, 19);
            // 
            // 
            // 
            gridPreview.MasterTemplate.ViewDefinition = tableViewDefinition1;
            gridPreview.Name = "gridPreview";
            gridPreview.ReadOnly = true;
            gridPreview.Size = new System.Drawing.Size(902, 586);
            gridPreview.TabIndex = 0;
            // 
            // grpSalida
            // 
            grpSalida.Controls.Add(radEportar);
            grpSalida.Controls.Add(txtPreviewSalida);
            grpSalida.Dock = System.Windows.Forms.DockStyle.Fill;
            grpSalida.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            grpSalida.Location = new System.Drawing.Point(0, 0);
            grpSalida.Name = "grpSalida";
            grpSalida.Size = new System.Drawing.Size(288, 608);
            grpSalida.TabIndex = 0;
            grpSalida.TabStop = false;
            grpSalida.Text = "Archivo de salida TXT generado";
            // 
            // radEportar
            // 
            radEportar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            radEportar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            radEportar.Location = new System.Drawing.Point(6, 556);
            radEportar.Name = "radEportar";
            radEportar.Size = new System.Drawing.Size(270, 40);
            radEportar.TabIndex = 15;
            radEportar.Text = "Generar / Exportar";
            radEportar.Click += radEportar_Click;
            // 
            // txtPreviewSalida
            // 
            txtPreviewSalida.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtPreviewSalida.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            txtPreviewSalida.Font = new System.Drawing.Font("Consolas", 9.5F);
            txtPreviewSalida.ForeColor = System.Drawing.Color.FromArgb(200, 220, 200);
            txtPreviewSalida.Location = new System.Drawing.Point(3, 19);
            txtPreviewSalida.Name = "txtPreviewSalida";
            txtPreviewSalida.ReadOnly = true;
            txtPreviewSalida.Size = new System.Drawing.Size(279, 531);
            txtPreviewSalida.TabIndex = 0;
            txtPreviewSalida.Text = "";
            txtPreviewSalida.WordWrap = false;
            // 
            // FormProcesadorCSV
            // 
            AutoScaleBaseSize = new System.Drawing.Size(7, 15);
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1200, 700);
            Controls.Add(splitVista);
            Controls.Add(panelTop);
            Name = "FormProcesadorCSV";
            Text = "Procesar / Exportar CSV";
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)btn_Ayuda).EndInit();
            ((System.ComponentModel.ISupportInitialize)btn_Filtros).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbMarca).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtNroBase).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtRutaCSV).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnSeleccionarCSV).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtSepDec).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtSepMil).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnVerDiferencias).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnExportar).EndInit();
            splitVista.Panel1.ResumeLayout(false);
            splitVista.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitVista).EndInit();
            splitVista.ResumeLayout(false);
            grpPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridPreview.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridPreview).EndInit();
            grpSalida.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)radEportar).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblMarca2;
        private Telerik.WinControls.UI.RadDropDownList cmbMarca;
        private System.Windows.Forms.Label lblCSV;
        private Telerik.WinControls.UI.RadTextBox txtRutaCSV;
        private Telerik.WinControls.UI.RadButton btnSeleccionarCSV;
        private System.Windows.Forms.Label lblNroBase;
        private Telerik.WinControls.UI.RadTextBox txtNroBase;
        private Telerik.WinControls.UI.RadButton btnExportar;
        private System.Windows.Forms.Label lblEstado;
        private System.Windows.Forms.Label lblValidacion;
        private Telerik.WinControls.UI.RadButton btnVerDiferencias;
        private System.Windows.Forms.Label lblSepDec;
        private Telerik.WinControls.UI.RadTextBox txtSepDec;
        private System.Windows.Forms.Label lblSepMil;
        private Telerik.WinControls.UI.RadTextBox txtSepMil;
        private System.Windows.Forms.SplitContainer splitVista;
        private System.Windows.Forms.GroupBox grpPreview;
        private Telerik.WinControls.UI.RadGridView gridPreview;
        private System.Windows.Forms.GroupBox grpSalida;
        private System.Windows.Forms.RichTextBox txtPreviewSalida;
        private Telerik.WinControls.UI.RadButton radEportar;
        private Telerik.WinControls.UI.RadButton btn_Ayuda;
        private Telerik.WinControls.UI.RadButton btn_Filtros;
    }
}
