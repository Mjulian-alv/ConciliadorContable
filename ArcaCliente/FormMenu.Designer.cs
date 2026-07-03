namespace ArcaCliente
{
    partial class FormMenu
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
            pnlTop        = new Telerik.WinControls.UI.RadPanel();
            lblTitulo     = new Telerik.WinControls.UI.RadLabel();
            lblUsuario    = new Telerik.WinControls.UI.RadLabel();
            btnLogout     = new Telerik.WinControls.UI.RadButton();
            grpOnline     = new System.Windows.Forms.GroupBox();
            lblDescOnline = new Telerik.WinControls.UI.RadLabel();
            btnOnline     = new Telerik.WinControls.UI.RadButton();
            grpOffline    = new System.Windows.Forms.GroupBox();
            lblDescOffline = new Telerik.WinControls.UI.RadLabel();
            btnOffline    = new Telerik.WinControls.UI.RadButton();

            ((System.ComponentModel.ISupportInitialize)pnlTop).BeginInit();
            pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lblTitulo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblUsuario).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnLogout).BeginInit();
            grpOnline.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lblDescOnline).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnOnline).BeginInit();
            grpOffline.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lblDescOffline).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnOffline).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            SuspendLayout();

            // ?? pnlTop ???????????????????????????????????????????????????????????
            pnlTop.Controls.Add(lblTitulo);
            pnlTop.Controls.Add(lblUsuario);
            pnlTop.Controls.Add(btnLogout);
            pnlTop.Dock     = System.Windows.Forms.DockStyle.Top;
            pnlTop.Location = new System.Drawing.Point(0, 0);
            pnlTop.Name     = "pnlTop";
            pnlTop.Size     = new System.Drawing.Size(492, 50);
            pnlTop.TabIndex = 0;

            lblTitulo.Font     = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            lblTitulo.Location = new System.Drawing.Point(12, 13);
            lblTitulo.Name     = "lblTitulo";
            lblTitulo.Size     = new System.Drawing.Size(110, 25);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text     = "ARCA Cliente";

            lblUsuario.Location  = new System.Drawing.Point(140, 16);
            lblUsuario.Name      = "lblUsuario";
            lblUsuario.Size      = new System.Drawing.Size(230, 18);
            lblUsuario.TabIndex  = 1;
            lblUsuario.AutoSize  = false;

            btnLogout.Location = new System.Drawing.Point(386, 12);
            btnLogout.Name     = "btnLogout";
            btnLogout.Size     = new System.Drawing.Size(96, 28);
            btnLogout.TabIndex = 2;
            btnLogout.Text     = "Cerrar sesión";
            btnLogout.Click   += BtnLogout_Click;

            // ?? grpOnline ?????????????????????????????????????????????????????????
            grpOnline.Controls.Add(lblDescOnline);
            grpOnline.Controls.Add(btnOnline);
            grpOnline.Location = new System.Drawing.Point(14, 62);
            grpOnline.Name     = "grpOnline";
            grpOnline.Size     = new System.Drawing.Size(464, 110);
            grpOnline.TabIndex = 1;
            grpOnline.TabStop  = false;
            grpOnline.Text     = "Comprobantes Online (ARCA)";

            lblDescOnline.AutoSize = false;
            lblDescOnline.Location = new System.Drawing.Point(8, 22);
            lblDescOnline.Name     = "lblDescOnline";
            lblDescOnline.Size     = new System.Drawing.Size(360, 50);
            lblDescOnline.TabIndex = 0;
            lblDescOnline.Text     = "Descarga los comprobantes desde el portal AFIP y\nlos concilia con el sistema local (base de datos).";

            btnOnline.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnOnline.Location = new System.Drawing.Point(376, 74);
            btnOnline.Name     = "btnOnline";
            btnOnline.Size     = new System.Drawing.Size(78, 28);
            btnOnline.TabIndex = 1;
            btnOnline.Text     = "Abrir ?";
            btnOnline.Click   += BtnOnline_Click;

            // ?? grpOffline ????????????????????????????????????????????????????????
            grpOffline.Controls.Add(lblDescOffline);
            grpOffline.Controls.Add(btnOffline);
            grpOffline.Location = new System.Drawing.Point(14, 184);
            grpOffline.Name     = "grpOffline";
            grpOffline.Size     = new System.Drawing.Size(464, 110);
            grpOffline.TabIndex = 2;
            grpOffline.TabStop  = false;
            grpOffline.Text     = "Conciliación Offline (CSV + Excel)";

            lblDescOffline.AutoSize = false;
            lblDescOffline.Location = new System.Drawing.Point(8, 22);
            lblDescOffline.Name     = "lblDescOffline";
            lblDescOffline.Size     = new System.Drawing.Size(360, 50);
            lblDescOffline.TabIndex = 0;
            lblDescOffline.Text     = "Concilia archivos CSV exportados de ARCA con un\nExcel del sistema, sin conexión al portal.";

            btnOffline.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnOffline.Location = new System.Drawing.Point(376, 74);
            btnOffline.Name     = "btnOffline";
            btnOffline.Size     = new System.Drawing.Size(78, 28);
            btnOffline.TabIndex = 1;
            btnOffline.Text     = "Abrir ?";
            btnOffline.Click   += BtnOffline_Click;

            // ?? Form ??????????????????????????????????????????????????????????????
            AutoScaleBaseSize   = new System.Drawing.Size(7, 15);
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize          = new System.Drawing.Size(492, 312);
            Controls.Add(grpOffline);
            Controls.Add(grpOnline);
            Controls.Add(pnlTop);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            Name            = "FormMenu";
            StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text            = "ARCA Cliente";

            ((System.ComponentModel.ISupportInitialize)pnlTop).EndInit();
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)lblTitulo).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblUsuario).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnLogout).EndInit();
            grpOnline.ResumeLayout(false);
            grpOnline.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)lblDescOnline).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnOnline).EndInit();
            grpOffline.ResumeLayout(false);
            grpOffline.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)lblDescOffline).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnOffline).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        private Telerik.WinControls.UI.RadPanel   pnlTop;
        private Telerik.WinControls.UI.RadLabel   lblTitulo;
        private Telerik.WinControls.UI.RadLabel   lblUsuario;
        private Telerik.WinControls.UI.RadButton  btnLogout;
        private System.Windows.Forms.GroupBox     grpOnline;
        private Telerik.WinControls.UI.RadLabel   lblDescOnline;
        private Telerik.WinControls.UI.RadButton  btnOnline;
        private System.Windows.Forms.GroupBox     grpOffline;
        private Telerik.WinControls.UI.RadLabel   lblDescOffline;
        private Telerik.WinControls.UI.RadButton  btnOffline;
    }
}
