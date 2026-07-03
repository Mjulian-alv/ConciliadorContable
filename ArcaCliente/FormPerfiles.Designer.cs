namespace ArcaCliente
{
    partial class FormPerfiles
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
            this.pnlBotones    = new Telerik.WinControls.UI.RadPanel();
            this.btnAgregar    = new Telerik.WinControls.UI.RadButton();
            this.btnEditar     = new Telerik.WinControls.UI.RadButton();
            this.btnEliminar   = new Telerik.WinControls.UI.RadButton();
            this.btnDirectivas = new Telerik.WinControls.UI.RadButton();
            this.btnGuardar    = new Telerik.WinControls.UI.RadButton();
            this.btnEquivalencias = new Telerik.WinControls.UI.RadButton();
            this.btnCerrar     = new Telerik.WinControls.UI.RadButton();
            this.gridPerfiles  = new Telerik.WinControls.UI.RadGridView();

            ((System.ComponentModel.ISupportInitialize)(this.pnlBotones)).BeginInit();
            this.pnlBotones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridPerfiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();

            // ?? pnlBotones ???????????????????????????????????????????????????????
            this.pnlBotones.Controls.Add(this.btnAgregar);
            this.pnlBotones.Controls.Add(this.btnEditar);
            this.pnlBotones.Controls.Add(this.btnEliminar);
            this.pnlBotones.Controls.Add(this.btnDirectivas);
            this.pnlBotones.Controls.Add(this.btnEquivalencias);
            this.pnlBotones.Controls.Add(this.btnGuardar);
            this.pnlBotones.Controls.Add(this.btnCerrar);
            this.pnlBotones.Dock     = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBotones.Name     = "pnlBotones";
            this.pnlBotones.Size     = new System.Drawing.Size(800, 48);
            this.pnlBotones.TabIndex = 0;

            this.btnAgregar.Location  = new System.Drawing.Point(12, 10);
            this.btnAgregar.Name      = "btnAgregar";
            this.btnAgregar.Size      = new System.Drawing.Size(110, 28);
            this.btnAgregar.Text      = "+ Nuevo";
            this.btnAgregar.TabIndex  = 0;
            this.btnAgregar.Click    += new System.EventHandler(this.btnAgregar_Click);

            this.btnEditar.Location   = new System.Drawing.Point(130, 10);
            this.btnEditar.Name       = "btnEditar";
            this.btnEditar.Size       = new System.Drawing.Size(110, 28);
            this.btnEditar.Text       = "Editar...";
            this.btnEditar.TabIndex   = 1;
            this.btnEditar.Click     += new System.EventHandler(this.btnEditar_Click);

            this.btnEliminar.Location = new System.Drawing.Point(248, 10);
            this.btnEliminar.Name     = "btnEliminar";
            this.btnEliminar.Size     = new System.Drawing.Size(110, 28);
            this.btnEliminar.Text     = "Eliminar";
            this.btnEliminar.TabIndex = 2;
            this.btnEliminar.Click   += new System.EventHandler(this.btnEliminar_Click);

            this.btnDirectivas.Location = new System.Drawing.Point(366, 10);
            this.btnDirectivas.Name     = "btnDirectivas";
            this.btnDirectivas.Size     = new System.Drawing.Size(110, 28);
            this.btnDirectivas.Text     = "Directivas...";
            this.btnDirectivas.TabIndex = 5;
            this.btnDirectivas.Click   += new System.EventHandler(this.btnDirectivas_Click);

            this.btnEquivalencias.Location = new System.Drawing.Point(484, 10);
            this.btnEquivalencias.Name     = "btnEquivalencias";
            this.btnEquivalencias.Size     = new System.Drawing.Size(110, 28);
            this.btnEquivalencias.Text     = "Equivalencias...";
            this.btnEquivalencias.TabIndex = 6;
            this.btnEquivalencias.Click   += new System.EventHandler(this.btnEquivalencias_Click);

            this.btnGuardar.Location  = new System.Drawing.Point(602, 10);
            this.btnGuardar.Name      = "btnGuardar";
            this.btnGuardar.Size      = new System.Drawing.Size(110, 28);
            this.btnGuardar.Text      = "Guardar";
            this.btnGuardar.TabIndex  = 3;
            this.btnGuardar.Click    += new System.EventHandler(this.btnGuardar_Click);

            this.btnCerrar.Location   = new System.Drawing.Point(678, 10);
            this.btnCerrar.Name       = "btnCerrar";
            this.btnCerrar.Size       = new System.Drawing.Size(110, 28);
            this.btnCerrar.Text       = "Cancelar";
            this.btnCerrar.TabIndex   = 4;
            this.btnCerrar.Click     += new System.EventHandler(this.btnCerrar_Click);

            // ?? gridPerfiles ?????????????????????????????????????????????????????
            this.gridPerfiles.Dock     = System.Windows.Forms.DockStyle.Fill;
            this.gridPerfiles.Name     = "gridPerfiles";
            this.gridPerfiles.TabIndex = 5;
            this.gridPerfiles.CellDoubleClick += new Telerik.WinControls.UI.GridViewCellEventHandler(this.gridPerfiles_CellDoubleClick);

            // ?? FormPerfiles ?????????????????????????????????????????????????????
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(800, 500);
            this.Controls.Add(this.gridPerfiles);
            this.Controls.Add(this.pnlBotones);
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox         = false;
            this.MinimizeBox         = false;
            this.Name                = "FormPerfiles";
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text                = "Perfiles Fiscales";

            ((System.ComponentModel.ISupportInitialize)(this.pnlBotones)).EndInit();
            this.pnlBotones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridPerfiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);
        }

        private Telerik.WinControls.UI.RadPanel    pnlBotones;
        private Telerik.WinControls.UI.RadButton   btnAgregar;
        private Telerik.WinControls.UI.RadButton   btnEditar;
        private Telerik.WinControls.UI.RadButton   btnEliminar;
        private Telerik.WinControls.UI.RadButton   btnDirectivas;
        private Telerik.WinControls.UI.RadButton   btnGuardar;
        private Telerik.WinControls.UI.RadButton   btnEquivalencias;
        private Telerik.WinControls.UI.RadButton   btnCerrar;
        private Telerik.WinControls.UI.RadGridView gridPerfiles;
    }
}
