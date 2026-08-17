namespace ArcaCliente
{
    partial class FormExportarPreseaQr
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            grpScan = new System.Windows.Forms.GroupBox();
            lblScanCaption = new Telerik.WinControls.UI.RadLabel();
            txtScan = new Telerik.WinControls.UI.RadTextBox();
            lblScanInfo = new Telerik.WinControls.UI.RadLabel();
            gridSeleccion = new Telerik.WinControls.UI.RadGridView();
            lblContadores = new Telerik.WinControls.UI.RadLabel();
            btnSeleccionarNuevos = new Telerik.WinControls.UI.RadButton();
            btnQuitarSeleccion = new Telerik.WinControls.UI.RadButton();
            btnContinuar = new Telerik.WinControls.UI.RadButton();
            btnCancelar = new Telerik.WinControls.UI.RadButton();
            grpScan.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lblScanCaption).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtScan).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblScanInfo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridSeleccion).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridSeleccion.MasterTemplate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblContadores).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnSeleccionarNuevos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnQuitarSeleccion).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnContinuar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnCancelar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            SuspendLayout();
            //
            // grpScan
            //
            grpScan.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            grpScan.Controls.Add(lblScanCaption);
            grpScan.Controls.Add(txtScan);
            grpScan.Controls.Add(lblScanInfo);
            grpScan.Location = new System.Drawing.Point(12, 8);
            grpScan.Name = "grpScan";
            grpScan.Size = new System.Drawing.Size(796, 80);
            grpScan.TabIndex = 0;
            grpScan.TabStop = false;
            grpScan.Text = "Escaneo";
            //
            // lblScanCaption
            //
            lblScanCaption.AutoSize = false;
            lblScanCaption.Location = new System.Drawing.Point(12, 20);
            lblScanCaption.Name = "lblScanCaption";
            lblScanCaption.Size = new System.Drawing.Size(500, 16);
            lblScanCaption.TabIndex = 0;
            lblScanCaption.Text = "Escanee el QR del comprobante (o tilde manualmente en la grilla):";
            //
            // txtScan
            //
            txtScan.Location = new System.Drawing.Point(12, 44);
            txtScan.Name = "txtScan";
            txtScan.Size = new System.Drawing.Size(480, 22);
            txtScan.TabIndex = 1;
            txtScan.KeyDown += TxtScan_KeyDown;
            //
            // lblScanInfo
            //
            lblScanInfo.AutoSize = false;
            lblScanInfo.Location = new System.Drawing.Point(504, 46);
            lblScanInfo.Name = "lblScanInfo";
            lblScanInfo.Size = new System.Drawing.Size(280, 18);
            lblScanInfo.TabIndex = 2;
            //
            // gridSeleccion
            //
            gridSeleccion.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gridSeleccion.AutoGenerateColumns = false;
            gridSeleccion.Location = new System.Drawing.Point(12, 96);
            gridSeleccion.MasterTemplate.AllowAddNewRow = false;
            gridSeleccion.MasterTemplate.AllowDeleteRow = false;
            gridSeleccion.Name = "gridSeleccion";
            gridSeleccion.ReadOnly = false;
            gridSeleccion.Size = new System.Drawing.Size(796, 360);
            gridSeleccion.TabIndex = 1;
            //
            // lblContadores
            //
            lblContadores.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblContadores.AutoSize = false;
            lblContadores.Location = new System.Drawing.Point(12, 466);
            lblContadores.Name = "lblContadores";
            lblContadores.Size = new System.Drawing.Size(360, 18);
            lblContadores.TabIndex = 2;
            //
            // btnSeleccionarNuevos
            //
            btnSeleccionarNuevos.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnSeleccionarNuevos.Location = new System.Drawing.Point(12, 484);
            btnSeleccionarNuevos.Name = "btnSeleccionarNuevos";
            btnSeleccionarNuevos.Size = new System.Drawing.Size(180, 28);
            btnSeleccionarNuevos.TabIndex = 3;
            btnSeleccionarNuevos.Text = "Seleccionar nuevos";
            btnSeleccionarNuevos.Click += btnSeleccionarNuevos_Click;
            //
            // btnQuitarSeleccion
            //
            btnQuitarSeleccion.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnQuitarSeleccion.Location = new System.Drawing.Point(200, 484);
            btnQuitarSeleccion.Name = "btnQuitarSeleccion";
            btnQuitarSeleccion.Size = new System.Drawing.Size(140, 28);
            btnQuitarSeleccion.TabIndex = 4;
            btnQuitarSeleccion.Text = "Quitar seleccion";
            btnQuitarSeleccion.Click += btnQuitarSeleccion_Click;
            //
            // btnContinuar
            //
            btnContinuar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnContinuar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnContinuar.Location = new System.Drawing.Point(596, 484);
            btnContinuar.Name = "btnContinuar";
            btnContinuar.Size = new System.Drawing.Size(120, 28);
            btnContinuar.TabIndex = 5;
            btnContinuar.Text = "Continuar";
            btnContinuar.Click += BtnContinuar_Click;
            //
            // btnCancelar
            //
            btnCancelar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnCancelar.Location = new System.Drawing.Point(722, 484);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new System.Drawing.Size(86, 28);
            btnCancelar.TabIndex = 6;
            btnCancelar.Text = "Cancelar";
            btnCancelar.Click += btnCancelar_Click;
            //
            // FormExportarPreseaQr
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(820, 520);
            Controls.Add(grpScan);
            Controls.Add(gridSeleccion);
            Controls.Add(lblContadores);
            Controls.Add(btnSeleccionarNuevos);
            Controls.Add(btnQuitarSeleccion);
            Controls.Add(btnContinuar);
            Controls.Add(btnCancelar);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            MinimizeBox = false;
            Name = "FormExportarPreseaQr";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Exportar SOLO ARCA a PRESEA  -  Seleccion por QR";
            grpScan.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)lblScanCaption).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtScan).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblScanInfo).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridSeleccion.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridSeleccion).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblContadores).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnSeleccionarNuevos).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnQuitarSeleccion).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnContinuar).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnCancelar).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        private System.Windows.Forms.GroupBox      grpScan;
        private Telerik.WinControls.UI.RadLabel     lblScanCaption;
        private Telerik.WinControls.UI.RadTextBox   txtScan;
        private Telerik.WinControls.UI.RadLabel     lblScanInfo;
        private Telerik.WinControls.UI.RadGridView  gridSeleccion;
        private Telerik.WinControls.UI.RadLabel     lblContadores;
        private Telerik.WinControls.UI.RadButton    btnSeleccionarNuevos;
        private Telerik.WinControls.UI.RadButton    btnQuitarSeleccion;
        private Telerik.WinControls.UI.RadButton    btnContinuar;
        private Telerik.WinControls.UI.RadButton    btnCancelar;
    }
}
