namespace ArcaCliente
{
    partial class FormLogin
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
            pageViewLogin    = new Telerik.WinControls.UI.RadPageView();
            pgLogin          = new Telerik.WinControls.UI.RadPageViewPage();
            lblLoginEmail    = new Telerik.WinControls.UI.RadLabel();
            txtLoginEmail    = new Telerik.WinControls.UI.RadTextBox();
            lblLoginPassword = new Telerik.WinControls.UI.RadLabel();
            txtLoginPassword = new Telerik.WinControls.UI.RadTextBox();
            lblLoginLimite   = new Telerik.WinControls.UI.RadLabel();
            spnLoginLimite   = new Telerik.WinControls.UI.RadSpinEditor();
            btnLogin         = new Telerik.WinControls.UI.RadButton();
            pgRegistro       = new Telerik.WinControls.UI.RadPageViewPage();
            lblRegEmail      = new Telerik.WinControls.UI.RadLabel();
            txtRegEmail      = new Telerik.WinControls.UI.RadTextBox();
            lblRegPassword   = new Telerik.WinControls.UI.RadLabel();
            txtRegPassword   = new Telerik.WinControls.UI.RadTextBox();
            lblRegEmpresa    = new Telerik.WinControls.UI.RadLabel();
            txtRegEmpresa    = new Telerik.WinControls.UI.RadTextBox();
            lblRegLimite     = new Telerik.WinControls.UI.RadLabel();
            spnRegLimite     = new Telerik.WinControls.UI.RadSpinEditor();
            btnRegistrar     = new Telerik.WinControls.UI.RadButton();
            ((System.ComponentModel.ISupportInitialize)pageViewLogin).BeginInit();
            pageViewLogin.SuspendLayout();
            pgLogin.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lblLoginEmail).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtLoginEmail).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblLoginPassword).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtLoginPassword).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblLoginLimite).BeginInit();
            ((System.ComponentModel.ISupportInitialize)spnLoginLimite).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnLogin).BeginInit();
            pgRegistro.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lblRegEmail).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtRegEmail).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblRegPassword).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtRegPassword).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblRegEmpresa).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtRegEmpresa).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblRegLimite).BeginInit();
            ((System.ComponentModel.ISupportInitialize)spnRegLimite).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnRegistrar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            SuspendLayout();
            // 
            // pageViewLogin
            // 
            pageViewLogin.Controls.Add(pgLogin);
            pageViewLogin.Controls.Add(pgRegistro);
            pageViewLogin.Dock = System.Windows.Forms.DockStyle.Fill;
            pageViewLogin.Location = new System.Drawing.Point(0, 0);
            pageViewLogin.Name = "pageViewLogin";
            pageViewLogin.SelectedPage = pgRegistro;
            pageViewLogin.Size = new System.Drawing.Size(420, 460);
            pageViewLogin.TabIndex = 0;
            // 
            // pgLogin
            // 
            pgLogin.Controls.Add(lblLoginEmail);
            pgLogin.Controls.Add(txtLoginEmail);
            pgLogin.Controls.Add(lblLoginPassword);
            pgLogin.Controls.Add(txtLoginPassword);
            pgLogin.Controls.Add(lblLoginLimite);
            pgLogin.Controls.Add(spnLoginLimite);
            pgLogin.Controls.Add(btnLogin);
            pgLogin.ItemSize = new System.Drawing.SizeF(86F, 29F);
            pgLogin.Location = new System.Drawing.Point(6, 36);
            pgLogin.Name = "pgLogin";
            pgLogin.Size = new System.Drawing.Size(408, 418);
            pgLogin.Text = "Iniciar Sesión";
            // 
            // lblLoginEmail
            // 
            lblLoginEmail.Location = new System.Drawing.Point(20, 30);
            lblLoginEmail.Name = "lblLoginEmail";
            lblLoginEmail.Size = new System.Drawing.Size(35, 18);
            lblLoginEmail.TabIndex = 0;
            lblLoginEmail.Text = "Email:";
            // 
            // txtLoginEmail
            // 
            txtLoginEmail.Location = new System.Drawing.Point(20, 50);
            txtLoginEmail.Name = "txtLoginEmail";
            txtLoginEmail.Size = new System.Drawing.Size(364, 24);
            txtLoginEmail.TabIndex = 0;
            // 
            // lblLoginPassword
            // 
            lblLoginPassword.Location = new System.Drawing.Point(20, 90);
            lblLoginPassword.Name = "lblLoginPassword";
            lblLoginPassword.Size = new System.Drawing.Size(65, 18);
            lblLoginPassword.TabIndex = 1;
            lblLoginPassword.Text = "Contraseña:";
            // 
            // txtLoginPassword
            // 
            txtLoginPassword.Location = new System.Drawing.Point(20, 110);
            txtLoginPassword.Name = "txtLoginPassword";
            txtLoginPassword.Size = new System.Drawing.Size(364, 24);
            txtLoginPassword.TabIndex = 1;
            // 
            // lblLoginLimite
            // 
            lblLoginLimite.Location = new System.Drawing.Point(20, 150);
            lblLoginLimite.Name = "lblLoginLimite";
            lblLoginLimite.Size = new System.Drawing.Size(174, 18);
            lblLoginLimite.TabIndex = 2;
            lblLoginLimite.Text = "Límite de requests (0 = ilimitado):";
            // 
            // spnLoginLimite
            // 
            spnLoginLimite.Location = new System.Drawing.Point(20, 170);
            spnLoginLimite.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            spnLoginLimite.Name = "spnLoginLimite";
            spnLoginLimite.Size = new System.Drawing.Size(364, 24);
            spnLoginLimite.TabIndex = 2;
            // 
            // btnLogin
            // 
            btnLogin.Location = new System.Drawing.Point(20, 220);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new System.Drawing.Size(364, 38);
            btnLogin.TabIndex = 3;
            btnLogin.Text = "INICIAR SESIÓN";
            btnLogin.Click += btnLogin_Click;
            // 
            // pgRegistro
            // 
            pgRegistro.Controls.Add(lblRegEmail);
            pgRegistro.Controls.Add(txtRegEmail);
            pgRegistro.Controls.Add(lblRegPassword);
            pgRegistro.Controls.Add(txtRegPassword);
            pgRegistro.Controls.Add(lblRegEmpresa);
            pgRegistro.Controls.Add(txtRegEmpresa);
            pgRegistro.Controls.Add(lblRegLimite);
            pgRegistro.Controls.Add(spnRegLimite);
            pgRegistro.Controls.Add(btnRegistrar);
            pgRegistro.ItemSize = new System.Drawing.SizeF(75F, 29F);
            pgRegistro.Location = new System.Drawing.Point(6, 36);
            pgRegistro.Name = "pgRegistro";
            pgRegistro.Size = new System.Drawing.Size(408, 418);
            pgRegistro.Text = "Registrarse";
            // 
            // lblRegEmail
            // 
            lblRegEmail.Location = new System.Drawing.Point(20, 20);
            lblRegEmail.Name = "lblRegEmail";
            lblRegEmail.Size = new System.Drawing.Size(35, 18);
            lblRegEmail.TabIndex = 0;
            lblRegEmail.Text = "Email:";
            // 
            // txtRegEmail
            // 
            txtRegEmail.Location = new System.Drawing.Point(20, 40);
            txtRegEmail.Name = "txtRegEmail";
            txtRegEmail.Size = new System.Drawing.Size(364, 24);
            txtRegEmail.TabIndex = 0;
            // 
            // lblRegPassword
            // 
            lblRegPassword.Location = new System.Drawing.Point(20, 80);
            lblRegPassword.Name = "lblRegPassword";
            lblRegPassword.Size = new System.Drawing.Size(65, 18);
            lblRegPassword.TabIndex = 1;
            lblRegPassword.Text = "Contraseña:";
            // 
            // txtRegPassword
            // 
            txtRegPassword.Location = new System.Drawing.Point(20, 100);
            txtRegPassword.Name = "txtRegPassword";
            txtRegPassword.Size = new System.Drawing.Size(364, 24);
            txtRegPassword.TabIndex = 1;
            // 
            // lblRegEmpresa
            // 
            lblRegEmpresa.Location = new System.Drawing.Point(20, 140);
            lblRegEmpresa.Name = "lblRegEmpresa";
            lblRegEmpresa.Size = new System.Drawing.Size(112, 18);
            lblRegEmpresa.TabIndex = 2;
            lblRegEmpresa.Text = "Nombre de empresa:";
            // 
            // txtRegEmpresa
            // 
            txtRegEmpresa.Location = new System.Drawing.Point(20, 160);
            txtRegEmpresa.Name = "txtRegEmpresa";
            txtRegEmpresa.Size = new System.Drawing.Size(364, 24);
            txtRegEmpresa.TabIndex = 2;
            // 
            // lblRegLimite
            // 
            lblRegLimite.Location = new System.Drawing.Point(20, 200);
            lblRegLimite.Name = "lblRegLimite";
            lblRegLimite.Size = new System.Drawing.Size(174, 18);
            lblRegLimite.TabIndex = 3;
            lblRegLimite.Text = "Límite de requests (0 = ilimitado):";
            // 
            // spnRegLimite
            // 
            spnRegLimite.Location = new System.Drawing.Point(20, 220);
            spnRegLimite.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            spnRegLimite.Name = "spnRegLimite";
            spnRegLimite.Size = new System.Drawing.Size(364, 24);
            spnRegLimite.TabIndex = 3;
            // 
            // btnRegistrar
            // 
            btnRegistrar.Location = new System.Drawing.Point(20, 270);
            btnRegistrar.Name = "btnRegistrar";
            btnRegistrar.Size = new System.Drawing.Size(364, 38);
            btnRegistrar.TabIndex = 4;
            btnRegistrar.Text = "CREAR CUENTA";
            btnRegistrar.Click += btnRegistrar_Click;
            // 
            // FormLogin
            //
            AutoScaleBaseSize = new System.Drawing.Size(7, 15);
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(420, 460);
            Controls.Add(pageViewLogin);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "FormLogin";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "ARCA Cliente - Acceso";
            ((System.ComponentModel.ISupportInitialize)pageViewLogin).EndInit();
            pageViewLogin.ResumeLayout(false);
            pgLogin.ResumeLayout(false);
            pgLogin.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)lblLoginEmail).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtLoginEmail).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblLoginPassword).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtLoginPassword).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblLoginLimite).EndInit();
            ((System.ComponentModel.ISupportInitialize)spnLoginLimite).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnLogin).EndInit();
            pgRegistro.ResumeLayout(false);
            pgRegistro.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)lblRegEmail).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtRegEmail).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblRegPassword).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtRegPassword).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblRegEmpresa).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtRegEmpresa).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblRegLimite).EndInit();
            ((System.ComponentModel.ISupportInitialize)spnRegLimite).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnRegistrar).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        private Telerik.WinControls.UI.RadPageView     pageViewLogin;
        private Telerik.WinControls.UI.RadPageViewPage pgLogin;
        private Telerik.WinControls.UI.RadPageViewPage pgRegistro;
        private Telerik.WinControls.UI.RadLabel        lblLoginEmail;
        private Telerik.WinControls.UI.RadTextBox      txtLoginEmail;
        private Telerik.WinControls.UI.RadLabel        lblLoginPassword;
        private Telerik.WinControls.UI.RadTextBox      txtLoginPassword;
        private Telerik.WinControls.UI.RadLabel        lblLoginLimite;
        private Telerik.WinControls.UI.RadSpinEditor   spnLoginLimite;
        private Telerik.WinControls.UI.RadButton       btnLogin;
        private Telerik.WinControls.UI.RadLabel        lblRegEmail;
        private Telerik.WinControls.UI.RadTextBox      txtRegEmail;
        private Telerik.WinControls.UI.RadLabel        lblRegPassword;
        private Telerik.WinControls.UI.RadTextBox      txtRegPassword;
        private Telerik.WinControls.UI.RadLabel        lblRegEmpresa;
        private Telerik.WinControls.UI.RadTextBox      txtRegEmpresa;
        private Telerik.WinControls.UI.RadLabel        lblRegLimite;
        private Telerik.WinControls.UI.RadSpinEditor   spnRegLimite;
        private Telerik.WinControls.UI.RadButton       btnRegistrar;
    }
}
