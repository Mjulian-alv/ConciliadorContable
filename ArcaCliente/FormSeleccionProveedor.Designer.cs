namespace ArcaCliente
{
    partial class FormSeleccionProveedor
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
            this.lblMensaje      = new Telerik.WinControls.UI.RadLabel();
            this.gridProveedores = new Telerik.WinControls.UI.RadGridView();
            this.pnlBotones      = new Telerik.WinControls.UI.RadPanel();
            this.btnSeleccionar  = new Telerik.WinControls.UI.RadButton();
            this.btnCancelar     = new Telerik.WinControls.UI.RadButton();

            ((System.ComponentModel.ISupportInitialize)(this.gridProveedores)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlBotones)).BeginInit();
            this.pnlBotones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();

            // ?? lblMensaje ????????????????????????????????????????????????????
            this.lblMensaje.Dock      = System.Windows.Forms.DockStyle.Top;
            this.lblMensaje.Name      = "lblMensaje";
            this.lblMensaje.Padding   = new System.Windows.Forms.Padding(8, 10, 8, 10);
            this.lblMensaje.Size      = new System.Drawing.Size(540, 48);
            this.lblMensaje.TabIndex  = 0;

            // ?? gridProveedores ???????????????????????????????????????????????
            this.gridProveedores.Dock     = System.Windows.Forms.DockStyle.Fill;
            this.gridProveedores.Name     = "gridProveedores";
            this.gridProveedores.TabIndex = 1;

            // ?? pnlBotones ????????????????????????????????????????????????????
            this.pnlBotones.Controls.Add(this.btnSeleccionar);
            this.pnlBotones.Controls.Add(this.btnCancelar);
            this.pnlBotones.Dock     = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBotones.Name     = "pnlBotones";
            this.pnlBotones.Size     = new System.Drawing.Size(540, 48);
            this.pnlBotones.TabIndex = 2;

            this.btnSeleccionar.Location  = new System.Drawing.Point(310, 10);
            this.btnSeleccionar.Name      = "btnSeleccionar";
            this.btnSeleccionar.Size      = new System.Drawing.Size(100, 28);
            this.btnSeleccionar.Text      = "Seleccionar";
            this.btnSeleccionar.TabIndex  = 0;
            this.btnSeleccionar.Click    += new System.EventHandler(this.btnSeleccionar_Click);

            this.btnCancelar.Location  = new System.Drawing.Point(418, 10);
            this.btnCancelar.Name      = "btnCancelar";
            this.btnCancelar.Size      = new System.Drawing.Size(100, 28);
            this.btnCancelar.Text      = "Cancelar";
            this.btnCancelar.TabIndex  = 1;
            this.btnCancelar.Click    += new System.EventHandler(this.btnCancelar_Click);

            // ?? FormSeleccionProveedor ????????????????????????????????????????
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(540, 360);
            this.Controls.Add(this.gridProveedores);
            this.Controls.Add(this.lblMensaje);
            this.Controls.Add(this.pnlBotones);
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox         = false;
            this.MinimizeBox         = false;
            this.Name                = "FormSeleccionProveedor";
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text                = "Seleccionar proveedor";

            ((System.ComponentModel.ISupportInitialize)(this.gridProveedores)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlBotones)).EndInit();
            this.pnlBotones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);
        }

        private Telerik.WinControls.UI.RadLabel    lblMensaje;
        private Telerik.WinControls.UI.RadGridView gridProveedores;
        private Telerik.WinControls.UI.RadPanel    pnlBotones;
        private Telerik.WinControls.UI.RadButton   btnSeleccionar;
        private Telerik.WinControls.UI.RadButton   btnCancelar;
    }
}
