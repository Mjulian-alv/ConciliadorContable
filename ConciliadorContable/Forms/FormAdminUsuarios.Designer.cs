using System.Drawing;
using System.Windows.Forms;
using ConciliadorContable.Models;
using Telerik.WinControls.UI;

namespace ConciliadorContable.Forms
{
    partial class FormAdminUsuarios
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) { if (disposing) components?.Dispose(); base.Dispose(disposing); }

        private void InitializeComponent()
        {
            splitMain = new SplitContainer();
            pnlLista = new Panel();
            lbUsuarios = new ListBox();
            lblListaTitulo = new Label();
            btnNuevo = new Button();
            pnlForm = new Panel();
            lblFormTitulo = new Label();
            lblUsername = new Label();
            txtUsername = new TextBox();
            lblNombre = new Label();
            txtNombre = new TextBox();
            lblPassword = new Label();
            txtPassword = new TextBox();
            lblRol = new Label();
            cmbRol = new ComboBox();
            chkActivo = new CheckBox();
            lblPermisos = new Label();
            lblPermisosTodos = new Label();
            pnlPermisos = new Panel();
            clbPermisos = new CheckedListBox();
            pnlBotones = new Panel();
            btnGuardar = new Button();
            btnEliminar = new Button();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            pnlLista.SuspendLayout();
            pnlForm.SuspendLayout();
            pnlPermisos.SuspendLayout();
            pnlBotones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            SuspendLayout();
            // 
            // splitMain
            // 
            splitMain.Dock = DockStyle.Fill;
            splitMain.Location = new Point(0, 0);
            splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(pnlLista);
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(pnlForm);
            splitMain.Size = new Size(900, 580);
            splitMain.SplitterDistance = 260;
            splitMain.TabIndex = 0;
            // 
            // pnlLista
            // 
            pnlLista.BackColor = Color.FromArgb(240, 244, 248);
            pnlLista.Controls.Add(lbUsuarios);
            pnlLista.Controls.Add(lblListaTitulo);
            pnlLista.Controls.Add(btnNuevo);
            pnlLista.Dock = DockStyle.Fill;
            pnlLista.Location = new Point(0, 0);
            pnlLista.Name = "pnlLista";
            pnlLista.Padding = new Padding(10);
            pnlLista.Size = new Size(851, 547);
            pnlLista.TabIndex = 0;
            // 
            // lbUsuarios
            // 
            lbUsuarios.DisplayMember = "DisplayName";
            lbUsuarios.Dock = DockStyle.Fill;
            lbUsuarios.Location = new Point(10, 38);
            lbUsuarios.Name = "lbUsuarios";
            lbUsuarios.Size = new Size(831, 467);
            lbUsuarios.TabIndex = 0;
            lbUsuarios.SelectedIndexChanged += lbUsuarios_SelectedIndexChanged;
            // 
            // lblListaTitulo
            // 
            lblListaTitulo.Dock = DockStyle.Top;
            lblListaTitulo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblListaTitulo.Location = new Point(10, 10);
            lblListaTitulo.Name = "lblListaTitulo";
            lblListaTitulo.Size = new Size(831, 28);
            lblListaTitulo.TabIndex = 1;
            lblListaTitulo.Text = "Usuarios";
            // 
            // btnNuevo
            // 
            btnNuevo.BackColor = Color.FromArgb(40, 120, 200);
            btnNuevo.Dock = DockStyle.Bottom;
            btnNuevo.FlatStyle = FlatStyle.Flat;
            btnNuevo.ForeColor = Color.White;
            btnNuevo.Location = new Point(10, 505);
            btnNuevo.Name = "btnNuevo";
            btnNuevo.Size = new Size(831, 32);
            btnNuevo.TabIndex = 2;
            btnNuevo.Text = "➕ Nuevo usuario";
            btnNuevo.UseVisualStyleBackColor = false;
            btnNuevo.Click += btnNuevo_Click;
            // 
            // pnlForm
            // 
            pnlForm.Controls.Add(lblFormTitulo);
            pnlForm.Controls.Add(lblUsername);
            pnlForm.Controls.Add(txtUsername);
            pnlForm.Controls.Add(lblNombre);
            pnlForm.Controls.Add(txtNombre);
            pnlForm.Controls.Add(lblPassword);
            pnlForm.Controls.Add(txtPassword);
            pnlForm.Controls.Add(lblRol);
            pnlForm.Controls.Add(cmbRol);
            pnlForm.Controls.Add(chkActivo);
            pnlForm.Controls.Add(lblPermisos);
            pnlForm.Controls.Add(lblPermisosTodos);
            pnlForm.Controls.Add(pnlPermisos);
            pnlForm.Controls.Add(pnlBotones);
            pnlForm.Dock = DockStyle.Fill;
            pnlForm.Location = new Point(0, 0);
            pnlForm.Name = "pnlForm";
            pnlForm.Padding = new Padding(16, 10, 16, 10);
            pnlForm.Size = new Size(201, 547);
            pnlForm.TabIndex = 0;
            // 
            // lblFormTitulo
            // 
            lblFormTitulo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblFormTitulo.Location = new Point(0, 0);
            lblFormTitulo.Name = "lblFormTitulo";
            lblFormTitulo.Size = new Size(300, 26);
            lblFormTitulo.TabIndex = 0;
            lblFormTitulo.Text = "Datos del usuario";
            // 
            // lblUsername
            // 
            lblUsername.Location = new Point(0, 34);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(145, 20);
            lblUsername.TabIndex = 1;
            lblUsername.Text = "Usuario:";
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(150, 31);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(430, 23);
            txtUsername.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            txtUsername.TabIndex = 2;
            // 
            // lblNombre
            // 
            lblNombre.Location = new Point(0, 64);
            lblNombre.Name = "lblNombre";
            lblNombre.Size = new Size(115, 20);
            lblNombre.TabIndex = 3;
            lblNombre.Text = "Nombre completo:";
            // 
            // txtNombre
            // 
            txtNombre.Location = new Point(150, 61);
            txtNombre.Name = "txtNombre";
            txtNombre.Size = new Size(430, 23);
            txtNombre.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            txtNombre.TabIndex = 4;
            // 
            // lblPassword
            // 
            lblPassword.Location = new Point(0, 94);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(145, 20);
            lblPassword.TabIndex = 5;
            lblPassword.Text = "Contraseña:";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(150, 91);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(280, 23);
            txtPassword.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            txtPassword.TabIndex = 6;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // lblRol
            // 
            lblRol.Location = new Point(0, 124);
            lblRol.Name = "lblRol";
            lblRol.Size = new Size(145, 20);
            lblRol.TabIndex = 7;
            lblRol.Text = "Rol:";
            // 
            // cmbRol
            // 
            cmbRol.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRol.Items.AddRange(new object[] { "Admin", "Usuario" });
            cmbRol.Location = new Point(150, 121);
            cmbRol.Name = "cmbRol";
            cmbRol.Size = new Size(150, 23);
            cmbRol.TabIndex = 8;
            cmbRol.SelectedIndexChanged += cmbRol_SelectedIndexChanged;
            // 
            // chkActivo
            // 
            chkActivo.AutoSize = true;
            chkActivo.Checked = true;
            chkActivo.CheckState = CheckState.Checked;
            chkActivo.Location = new Point(320, 123);
            chkActivo.Name = "chkActivo";
            chkActivo.Size = new Size(60, 19);
            chkActivo.TabIndex = 9;
            chkActivo.Text = "Activo";
            // 
            // lblPermisos
            // 
            lblPermisos.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPermisos.Location = new Point(0, 156);
            lblPermisos.Name = "lblPermisos";
            lblPermisos.Size = new Size(200, 20);
            lblPermisos.TabIndex = 10;
            lblPermisos.Text = "Permisos de acceso:";
            // 
            // lblPermisosTodos
            // 
            lblPermisosTodos.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            lblPermisosTodos.ForeColor = Color.DarkGreen;
            lblPermisosTodos.Location = new Point(0, 180);
            lblPermisosTodos.Name = "lblPermisosTodos";
            lblPermisosTodos.Size = new Size(300, 20);
            lblPermisosTodos.TabIndex = 11;
            lblPermisosTodos.Text = "✔ Acceso total (Administrador)";
            lblPermisosTodos.Visible = false;
            // 
            // pnlPermisos
            // 
            pnlPermisos.Controls.Add(clbPermisos);
            pnlPermisos.Location = new Point(0, 180);
            pnlPermisos.Name = "pnlPermisos";
            pnlPermisos.Size = new Size(580, 200);
            pnlPermisos.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            pnlPermisos.TabIndex = 12;
            // 
            // clbPermisos
            // 
            clbPermisos.CheckOnClick = true;
            clbPermisos.Dock = DockStyle.Fill;
            clbPermisos.Location = new Point(0, 0);
            clbPermisos.Name = "clbPermisos";
            clbPermisos.Size = new Size(560, 210);
            clbPermisos.TabIndex = 0;
            // 
            // pnlBotones
            // 
            pnlBotones.Controls.Add(btnGuardar);
            pnlBotones.Controls.Add(btnEliminar);
            pnlBotones.Location = new Point(0, 395);
            pnlBotones.Name = "pnlBotones";
            pnlBotones.Size = new Size(580, 44);
            pnlBotones.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            pnlBotones.TabIndex = 13;
            // 
            // btnGuardar
            // 
            btnGuardar.BackColor = Color.FromArgb(40, 160, 80);
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.ForeColor = Color.White;
            btnGuardar.Location = new Point(0, 0);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(120, 34);
            btnGuardar.TabIndex = 0;
            btnGuardar.Text = "💾 Guardar";
            btnGuardar.UseVisualStyleBackColor = false;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // btnEliminar
            // 
            btnEliminar.BackColor = Color.MistyRose;
            btnEliminar.FlatStyle = FlatStyle.Flat;
            btnEliminar.Location = new Point(130, 0);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new Size(120, 34);
            btnEliminar.TabIndex = 1;
            btnEliminar.Text = "🗑 Eliminar";
            btnEliminar.UseVisualStyleBackColor = false;
            btnEliminar.Click += btnEliminar_Click;
            // 
            // FormAdminUsuarios
            // 
            ClientSize = new Size(900, 580);
            Controls.Add(splitMain);
            MinimumSize = new Size(900, 580);
            Name = "FormAdminUsuarios";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Administración de Usuarios";
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            pnlLista.ResumeLayout(false);
            pnlForm.ResumeLayout(false);
            pnlForm.PerformLayout();
            pnlPermisos.ResumeLayout(false);
            pnlBotones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        // ── Campos ───────────────────────────────────────────────────────
        private SplitContainer  splitMain;
        private Panel           pnlLista;
        private Label           lblListaTitulo;
        private ListBox         lbUsuarios;
        private Button          btnNuevo;

        private Panel           pnlForm;
        private Label           lblFormTitulo;
        private Label           lblUsername;
        private TextBox         txtUsername;
        private Label           lblNombre;
        private TextBox         txtNombre;
        private Label           lblPassword;
        private TextBox         txtPassword;
        private Label           lblRol;
        private ComboBox        cmbRol;
        private CheckBox        chkActivo;
        private Label           lblPermisos;
        private Label           lblPermisosTodos;
        private Panel           pnlPermisos;
        private CheckedListBox  clbPermisos;
        private Panel           pnlBotones;
        private Button          btnGuardar;
        private Button          btnEliminar;
    }
}
