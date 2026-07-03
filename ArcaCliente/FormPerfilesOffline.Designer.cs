namespace ArcaCliente
{
    partial class FormPerfilesOffline
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
            pnlBotones     = new Telerik.WinControls.UI.RadPanel();
            btnNuevo       = new Telerik.WinControls.UI.RadButton();
            btnEditar      = new Telerik.WinControls.UI.RadButton();
            btnEliminar    = new Telerik.WinControls.UI.RadButton();
            btnDirectivas  = new Telerik.WinControls.UI.RadButton();
            btnSeleccionar = new Telerik.WinControls.UI.RadButton();
            btnCancelar    = new Telerik.WinControls.UI.RadButton();
            gridPerfiles   = new Telerik.WinControls.UI.RadGridView();

            ((System.ComponentModel.ISupportInitialize)pnlBotones).BeginInit();
            pnlBotones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)btnNuevo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnEditar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnEliminar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnDirectivas).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnSeleccionar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnCancelar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridPerfiles).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            SuspendLayout();

            // ?? pnlBotones ????????????????????????????????????????????????????????
            pnlBotones.Controls.Add(btnNuevo);
            pnlBotones.Controls.Add(btnEditar);
            pnlBotones.Controls.Add(btnEliminar);
            pnlBotones.Controls.Add(btnDirectivas);
            pnlBotones.Controls.Add(btnSeleccionar);
            pnlBotones.Controls.Add(btnCancelar);
            pnlBotones.Dock     = System.Windows.Forms.DockStyle.Bottom;
            pnlBotones.Name     = "pnlBotones";
            pnlBotones.Size     = new System.Drawing.Size(700, 48);
            pnlBotones.TabIndex = 0;

            btnNuevo.Location  = new System.Drawing.Point(12, 10);
            btnNuevo.Name      = "btnNuevo";
            btnNuevo.Size      = new System.Drawing.Size(100, 28);
            btnNuevo.TabIndex  = 0;
            btnNuevo.Text      = "+ Nuevo";
            btnNuevo.Click    += BtnNuevo_Click;

            btnEditar.Location = new System.Drawing.Point(120, 10);
            btnEditar.Name     = "btnEditar";
            btnEditar.Size     = new System.Drawing.Size(100, 28);
            btnEditar.TabIndex = 1;
            btnEditar.Text     = "Editar...";
            btnEditar.Click   += BtnEditar_Click;

            btnEliminar.Location = new System.Drawing.Point(228, 10);
            btnEliminar.Name     = "btnEliminar";
            btnEliminar.Size     = new System.Drawing.Size(100, 28);
            btnEliminar.TabIndex = 2;
            btnEliminar.Text     = "Eliminar";
            btnEliminar.Click   += BtnEliminar_Click;

            btnDirectivas.Location = new System.Drawing.Point(336, 10);
            btnDirectivas.Name     = "btnDirectivas";
            btnDirectivas.Size     = new System.Drawing.Size(116, 28);
            btnDirectivas.TabIndex = 3;
            btnDirectivas.Text     = "Directivas...";
            btnDirectivas.Click   += BtnDirectivas_Click;

            btnSeleccionar.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnSeleccionar.Location = new System.Drawing.Point(460, 10);
            btnSeleccionar.Name     = "btnSeleccionar";
            btnSeleccionar.Size     = new System.Drawing.Size(120, 28);
            btnSeleccionar.TabIndex = 4;
            btnSeleccionar.Text     = "Usar perfil ?";
            btnSeleccionar.Click   += BtnSeleccionar_Click;

            btnCancelar.Location = new System.Drawing.Point(588, 10);
            btnCancelar.Name     = "btnCancelar";
            btnCancelar.Size     = new System.Drawing.Size(100, 28);
            btnCancelar.TabIndex = 5;
            btnCancelar.Text     = "Cancelar";
            btnCancelar.Click   += BtnCancelar_Click;

            // ?? gridPerfiles ??????????????????????????????????????????????????????
            gridPerfiles.Dock     = System.Windows.Forms.DockStyle.Fill;
            gridPerfiles.Name     = "gridPerfiles";
            gridPerfiles.TabIndex = 1;
            gridPerfiles.MasterTemplate.AllowAddNewRow = false;
            gridPerfiles.MasterTemplate.AllowDeleteRow = false;
            gridPerfiles.MasterTemplate.AllowEditRow   = false;
            gridPerfiles.CellDoubleClick += GridPerfiles_CellDoubleClick;

            // ?? Form ??????????????????????????????????????????????????????????????
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize          = new System.Drawing.Size(700, 400);
            Controls.Add(gridPerfiles);
            Controls.Add(pnlBotones);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            Name            = "FormPerfilesOffline";
            StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            Text            = "Perfiles Offline";

            ((System.ComponentModel.ISupportInitialize)pnlBotones).EndInit();
            pnlBotones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)btnNuevo).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnEditar).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnEliminar).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnDirectivas).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnSeleccionar).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnCancelar).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridPerfiles).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        private Telerik.WinControls.UI.RadPanel      pnlBotones;
        private Telerik.WinControls.UI.RadButton     btnNuevo;
        private Telerik.WinControls.UI.RadButton     btnEditar;
        private Telerik.WinControls.UI.RadButton     btnEliminar;
        private Telerik.WinControls.UI.RadButton     btnDirectivas;
        private Telerik.WinControls.UI.RadButton     btnSeleccionar;
        private Telerik.WinControls.UI.RadButton     btnCancelar;
        private Telerik.WinControls.UI.RadGridView   gridPerfiles;
    }
}
