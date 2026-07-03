namespace LiquidacionesAuditar
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMenuPrincipal));
            btnColumnasCSV = new Telerik.WinControls.UI.RadButton();
            btnColumnasDestino = new Telerik.WinControls.UI.RadButton();
            btnLineasCON = new Telerik.WinControls.UI.RadButton();
            btnProcesador = new Telerik.WinControls.UI.RadButton();
            btnAbrirCarpetaDB = new Telerik.WinControls.UI.RadButton();
            panelHeader = new System.Windows.Forms.Panel();
            lblTitulo = new System.Windows.Forms.Label();
            lblSubtitulo = new System.Windows.Forms.Label();
            radPictureBox1 = new Telerik.WinControls.UI.RadPictureBox();
            panelCards = new System.Windows.Forms.Panel();
            panelFooter = new System.Windows.Forms.Panel();
            lblDBPath = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)btnColumnasCSV).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnColumnasDestino).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnLineasCON).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnProcesador).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnAbrirCarpetaDB).BeginInit();
            panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)radPictureBox1).BeginInit();
            panelCards.SuspendLayout();
            panelFooter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            SuspendLayout();
            // 
            // btnColumnasCSV
            // 
            btnColumnasCSV.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            btnColumnasCSV.Location = new System.Drawing.Point(55, 29);
            btnColumnasCSV.Name = "btnColumnasCSV";
            btnColumnasCSV.Size = new System.Drawing.Size(230, 90);
            btnColumnasCSV.TabIndex = 0;
            btnColumnasCSV.Text = "Columnas CSV\r\n(Origen)";
            btnColumnasCSV.Click += btnColumnasCSV_Click;
            // 
            // btnColumnasDestino
            // 
            btnColumnasDestino.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            btnColumnasDestino.Location = new System.Drawing.Point(305, 29);
            btnColumnasDestino.Name = "btnColumnasDestino";
            btnColumnasDestino.Size = new System.Drawing.Size(230, 90);
            btnColumnasDestino.TabIndex = 1;
            btnColumnasDestino.Text = "Columnas Destino\r\n(CAB / DET / CON)";
            btnColumnasDestino.Click += btnColumnasDestino_Click;
            // 
            // btnLineasCON
            // 
            btnLineasCON.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            btnLineasCON.Location = new System.Drawing.Point(55, 135);
            btnLineasCON.Name = "btnLineasCON";
            btnLineasCON.Size = new System.Drawing.Size(230, 90);
            btnLineasCON.TabIndex = 2;
            btnLineasCON.Text = "Lineas Totalizadoras\r\n(CON)";
            btnLineasCON.Click += btnLineasCON_Click;
            // 
            // btnProcesador
            // 
            btnProcesador.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            btnProcesador.Location = new System.Drawing.Point(305, 135);
            btnProcesador.Name = "btnProcesador";
            btnProcesador.Size = new System.Drawing.Size(230, 90);
            btnProcesador.TabIndex = 3;
            btnProcesador.Text = "Procesar / Exportar\r\nArchivo CSV";
            btnProcesador.Click += btnProcesador_Click;
            // 
            // btnAbrirCarpetaDB
            // 
            btnAbrirCarpetaDB.Dock = System.Windows.Forms.DockStyle.Right;
            btnAbrirCarpetaDB.Font = new System.Drawing.Font("Segoe UI", 8F);
            btnAbrirCarpetaDB.Location = new System.Drawing.Point(467, 0);
            btnAbrirCarpetaDB.Name = "btnAbrirCarpetaDB";
            btnAbrirCarpetaDB.Size = new System.Drawing.Size(130, 36);
            btnAbrirCarpetaDB.TabIndex = 1;
            btnAbrirCarpetaDB.Text = "Abrir carpeta BD";
            btnAbrirCarpetaDB.Click += btnAbrirCarpetaDB_Click;
            // 
            // panelHeader
            // 
            panelHeader.BackColor = System.Drawing.Color.DarkGray;
            panelHeader.Controls.Add(lblTitulo);
            panelHeader.Controls.Add(lblSubtitulo);
            panelHeader.Controls.Add(radPictureBox1);
            panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            panelHeader.Location = new System.Drawing.Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new System.Drawing.Size(597, 157);
            panelHeader.TabIndex = 1;
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            lblTitulo.ForeColor = System.Drawing.Color.White;
            lblTitulo.Location = new System.Drawing.Point(3, 12);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new System.Drawing.Size(264, 32);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Liquidaciones Auditar";
            // 
            // lblSubtitulo
            // 
            lblSubtitulo.AutoSize = true;
            lblSubtitulo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lblSubtitulo.ForeColor = System.Drawing.Color.Black;
            lblSubtitulo.Location = new System.Drawing.Point(11, 58);
            lblSubtitulo.Name = "lblSubtitulo";
            lblSubtitulo.Size = new System.Drawing.Size(330, 15);
            lblSubtitulo.TabIndex = 1;
            lblSubtitulo.Text = "Sistema de generacion y control de archivos de liquidacion";
            // 
            // radPictureBox1
            // 
            radPictureBox1.Image = Properties.Resources.AlvearSistemas_Logo;
            radPictureBox1.ImageAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            radPictureBox1.ImageLayout = Telerik.WinControls.UI.RadImageLayout.Stretch;
            radPictureBox1.Location = new System.Drawing.Point(360, 12);
            radPictureBox1.Name = "radPictureBox1";
            radPictureBox1.Size = new System.Drawing.Size(211, 132);
            radPictureBox1.TabIndex = 2;
            // 
            // panelCards
            // 
            panelCards.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            panelCards.Controls.Add(btnColumnasCSV);
            panelCards.Controls.Add(btnColumnasDestino);
            panelCards.Controls.Add(btnLineasCON);
            panelCards.Controls.Add(btnProcesador);
            panelCards.Dock = System.Windows.Forms.DockStyle.Fill;
            panelCards.Location = new System.Drawing.Point(0, 157);
            panelCards.Name = "panelCards";
            panelCards.Size = new System.Drawing.Size(597, 295);
            panelCards.TabIndex = 0;
            // 
            // panelFooter
            // 
            panelFooter.BackColor = System.Drawing.Color.FromArgb(224, 229, 236);
            panelFooter.Controls.Add(lblDBPath);
            panelFooter.Controls.Add(btnAbrirCarpetaDB);
            panelFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelFooter.Location = new System.Drawing.Point(0, 452);
            panelFooter.Name = "panelFooter";
            panelFooter.Size = new System.Drawing.Size(597, 36);
            panelFooter.TabIndex = 2;
            // 
            // lblDBPath
            // 
            lblDBPath.Dock = System.Windows.Forms.DockStyle.Fill;
            lblDBPath.Font = new System.Drawing.Font("Consolas", 8F);
            lblDBPath.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
            lblDBPath.Location = new System.Drawing.Point(0, 0);
            lblDBPath.Name = "lblDBPath";
            lblDBPath.Size = new System.Drawing.Size(467, 36);
            lblDBPath.TabIndex = 0;
            lblDBPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FormMenuPrincipal
            // 
            AutoScaleBaseSize = new System.Drawing.Size(7, 15);
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(597, 488);
            Controls.Add(panelCards);
            Controls.Add(panelHeader);
            Controls.Add(panelFooter);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MinimumSize = new System.Drawing.Size(560, 380);
            Name = "FormMenuPrincipal";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Liquidaciones Auditar";
            ((System.ComponentModel.ISupportInitialize)btnColumnasCSV).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnColumnasDestino).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnLineasCON).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnProcesador).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnAbrirCarpetaDB).EndInit();
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)radPictureBox1).EndInit();
            panelCards.ResumeLayout(false);
            panelFooter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        private Telerik.WinControls.UI.RadButton btnColumnasCSV;
        private Telerik.WinControls.UI.RadButton btnColumnasDestino;
        private Telerik.WinControls.UI.RadButton btnLineasCON;
        private Telerik.WinControls.UI.RadButton btnProcesador;
        private Telerik.WinControls.UI.RadButton btnAbrirCarpetaDB;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Panel panelCards;
        private System.Windows.Forms.Panel panelFooter;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblSubtitulo;
        private System.Windows.Forms.Label lblDBPath;
        private Telerik.WinControls.UI.RadPictureBox radPictureBox1;
    }
}
