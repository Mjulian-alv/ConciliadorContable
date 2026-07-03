using System.Drawing;
using System.Windows.Forms;
using Telerik.WinControls.UI;

namespace ConciliadorContable.Forms
{
    partial class FormLogin
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            pnlFondo = new Panel();
            pnlCard = new Panel();
            lblTitulo = new RadLabel();
            lblSubtitulo = new RadLabel();
            lblUserLbl = new RadLabel();
            txtUsuario = new RadTextBox();
            lblPassLbl = new RadLabel();
            txtPassword = new RadTextBox();
            btnIngresar = new RadButton();
            lblError = new RadLabel();
            lblVersion = new RadLabel();
            pnlFondo.SuspendLayout();
            pnlCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lblTitulo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblSubtitulo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblUserLbl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtUsuario).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblPassLbl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtPassword).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnIngresar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblError).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblVersion).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            SuspendLayout();
            // 
            // pnlFondo
            // 
            pnlFondo.BackColor = Color.FromArgb(30, 40, 55);
            pnlFondo.Controls.Add(pnlCard);
            pnlFondo.Controls.Add(lblVersion);
            pnlFondo.Dock = DockStyle.Fill;
            pnlFondo.Location = new Point(0, 0);
            pnlFondo.Name = "pnlFondo";
            pnlFondo.Size = new Size(405, 384);
            pnlFondo.TabIndex = 0;
            // 
            // pnlCard
            // 
            pnlCard.BackColor = Color.White;
            pnlCard.Controls.Add(lblTitulo);
            pnlCard.Controls.Add(lblSubtitulo);
            pnlCard.Controls.Add(lblUserLbl);
            pnlCard.Controls.Add(txtUsuario);
            pnlCard.Controls.Add(lblPassLbl);
            pnlCard.Controls.Add(txtPassword);
            pnlCard.Controls.Add(btnIngresar);
            pnlCard.Controls.Add(lblError);
            pnlCard.Location = new Point(50, 50);
            pnlCard.Name = "pnlCard";
            pnlCard.Size = new Size(320, 280);
            pnlCard.TabIndex = 0;
            // 
            // lblTitulo
            // 
            lblTitulo.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(30, 40, 55);
            lblTitulo.Location = new Point(20, 22);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(198, 29);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Conciliador Contable";
            // 
            // lblSubtitulo
            // 
            lblSubtitulo.Font = new Font("Segoe UI", 8.5F);
            lblSubtitulo.ForeColor = Color.Gray;
            lblSubtitulo.Location = new Point(20, 52);
            lblSubtitulo.Name = "lblSubtitulo";
            lblSubtitulo.Size = new Size(208, 18);
            lblSubtitulo.TabIndex = 1;
            lblSubtitulo.Text = "Ingrese sus credenciales para continuar";
            // 
            // lblUserLbl
            // 
            lblUserLbl.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblUserLbl.ForeColor = Color.FromArgb(50, 60, 75);
            lblUserLbl.Location = new Point(20, 88);
            lblUserLbl.Name = "lblUserLbl";
            lblUserLbl.Size = new Size(51, 19);
            lblUserLbl.TabIndex = 2;
            lblUserLbl.Text = "Usuario";
            // 
            // txtUsuario
            // 
            txtUsuario.Location = new Point(20, 108);
            txtUsuario.Name = "txtUsuario";
            txtUsuario.Size = new Size(280, 24);
            txtUsuario.TabIndex = 3;
            // 
            // lblPassLbl
            // 
            lblPassLbl.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPassLbl.ForeColor = Color.FromArgb(50, 60, 75);
            lblPassLbl.Location = new Point(20, 148);
            lblPassLbl.Name = "lblPassLbl";
            lblPassLbl.Size = new Size(72, 19);
            lblPassLbl.TabIndex = 4;
            lblPassLbl.Text = "Contraseña";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(20, 168);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '●';
            txtPassword.Size = new Size(280, 24);
            txtPassword.TabIndex = 5;
            // 
            // btnIngresar
            // 
            btnIngresar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnIngresar.Location = new Point(20, 212);
            btnIngresar.Name = "btnIngresar";
            btnIngresar.Size = new Size(280, 36);
            btnIngresar.TabIndex = 6;
            btnIngresar.Text = "Ingresar";
            btnIngresar.Click += BtnIngresar_Click;
            // 
            // lblError
            // 
            lblError.Font = new Font("Segoe UI", 8.5F);
            lblError.ForeColor = Color.Crimson;
            lblError.Location = new Point(20, 255);
            lblError.Name = "lblError";
            lblError.Size = new Size(2, 2);
            lblError.TabIndex = 7;
            lblError.Visible = false;
            // 
            // lblVersion
            // 
            lblVersion.Font = new Font("Segoe UI", 7.5F);
            lblVersion.ForeColor = Color.FromArgb(120, 140, 160);
            lblVersion.Location = new Point(110, 345);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(164, 17);
            lblVersion.TabIndex = 1;
            lblVersion.Text = "v1.0 · © 2026 Conciliador Contable";
            // 
            // FormLogin
            // 
            BackColor = Color.FromArgb(30, 40, 55);
            ClientSize = new Size(405, 384);
            Controls.Add(pnlFondo);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormLogin";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Conciliador Contable · Acceso";
            pnlFondo.ResumeLayout(false);
            pnlFondo.PerformLayout();
            pnlCard.ResumeLayout(false);
            pnlCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)lblTitulo).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblSubtitulo).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblUserLbl).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtUsuario).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblPassLbl).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtPassword).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnIngresar).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblError).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblVersion).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        private Panel     pnlFondo;
        private Panel     pnlCard;
        private RadLabel  lblTitulo;
        private RadLabel  lblSubtitulo;
        private RadLabel  lblUserLbl;
        private RadTextBox txtUsuario;
        private RadLabel  lblPassLbl;
        private RadTextBox txtPassword;
        private RadButton btnIngresar;
        private RadLabel  lblError;
        private RadLabel  lblVersion;
    }
}
