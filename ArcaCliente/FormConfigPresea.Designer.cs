namespace ArcaCliente
{
    partial class FormConfigPresea
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
            // ?? Declaraciones ?????????????????????????????????????????????????
            pnlBotones                 = new Telerik.WinControls.UI.RadPanel();
            btnCancelar                = new Telerik.WinControls.UI.RadButton();
            btnGuardar                 = new Telerik.WinControls.UI.RadButton();
            grpCuentas                 = new System.Windows.Forms.GroupBox();
            lblCodigoProveedor         = new Telerik.WinControls.UI.RadLabel();
            txtCodigoProveedor         = new Telerik.WinControls.UI.RadTextBox();
            lblCuentaContableProveedor = new Telerik.WinControls.UI.RadLabel();
            txtCuentaContableProveedor = new Telerik.WinControls.UI.RadTextBox();
            lblCuentaIVA               = new Telerik.WinControls.UI.RadLabel();
            txtCuentaIVA               = new Telerik.WinControls.UI.RadTextBox();
            lblCuentaIVA2              = new Telerik.WinControls.UI.RadLabel();
            txtCuentaIVA2              = new Telerik.WinControls.UI.RadTextBox();
            lblCuentaDebe              = new Telerik.WinControls.UI.RadLabel();
            txtCuentaDebe              = new Telerik.WinControls.UI.RadTextBox();
            lblCentro                  = new Telerik.WinControls.UI.RadLabel();
            txtCentro                  = new Telerik.WinControls.UI.RadTextBox();
            grpComprobante             = new System.Windows.Forms.GroupBox();
            lblProvincia               = new Telerik.WinControls.UI.RadLabel();
            txtProvincia               = new Telerik.WinControls.UI.RadTextBox();
            lblCondicion               = new Telerik.WinControls.UI.RadLabel();
            txtCondicion               = new Telerik.WinControls.UI.RadTextBox();
            lblFiscal                  = new Telerik.WinControls.UI.RadLabel();
            cmbFiscal                  = new Telerik.WinControls.UI.RadDropDownList();
            lblImputa                  = new Telerik.WinControls.UI.RadLabel();
            cmbImputa                  = new Telerik.WinControls.UI.RadDropDownList();
            grpFormato                 = new System.Windows.Forms.GroupBox();
            lblFormatoFecha            = new Telerik.WinControls.UI.RadLabel();
            cmbFormatoFecha            = new Telerik.WinControls.UI.RadDropDownList();

            // ?? BeginInit ?????????????????????????????????????????????????????
            ((System.ComponentModel.ISupportInitialize)pnlBotones).BeginInit();
            pnlBotones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)btnCancelar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnGuardar).BeginInit();
            grpCuentas.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lblCodigoProveedor).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtCodigoProveedor).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblCuentaContableProveedor).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtCuentaContableProveedor).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblCuentaIVA).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtCuentaIVA).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblCuentaIVA2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtCuentaIVA2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblCuentaDebe).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtCuentaDebe).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblCentro).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtCentro).BeginInit();
            grpComprobante.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lblProvincia).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtProvincia).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblCondicion).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtCondicion).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblFiscal).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbFiscal).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblImputa).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbImputa).BeginInit();
            grpFormato.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lblFormatoFecha).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbFormatoFecha).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            SuspendLayout();

            // ?? pnlBotones (Dock=Bottom) ??????????????????????????????????????
            pnlBotones.Controls.Add(btnCancelar);
            pnlBotones.Controls.Add(btnGuardar);
            pnlBotones.Dock     = System.Windows.Forms.DockStyle.Bottom;
            pnlBotones.Name     = "pnlBotones";
            pnlBotones.Size     = new System.Drawing.Size(490, 50);
            pnlBotones.TabIndex = 0;

            btnCancelar.Location = new System.Drawing.Point(282, 12);
            btnCancelar.Name     = "btnCancelar";
            btnCancelar.Size     = new System.Drawing.Size(96, 28);
            btnCancelar.TabIndex = 0;
            btnCancelar.Text     = "Cancelar";
            btnCancelar.Click   += BtnCancelar_Click;

            btnGuardar.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnGuardar.Location = new System.Drawing.Point(386, 12);
            btnGuardar.Name     = "btnGuardar";
            btnGuardar.Size     = new System.Drawing.Size(96, 28);
            btnGuardar.TabIndex = 1;
            btnGuardar.Text     = "Guardar";
            btnGuardar.Click   += BtnGuardar_Click;

            // ?? grpCuentas  y=8  h=174 ???????????????????????????????????????
            grpCuentas.Controls.Add(lblCodigoProveedor);
            grpCuentas.Controls.Add(txtCodigoProveedor);
            grpCuentas.Controls.Add(lblCuentaContableProveedor);
            grpCuentas.Controls.Add(txtCuentaContableProveedor);
            grpCuentas.Controls.Add(lblCuentaIVA);
            grpCuentas.Controls.Add(txtCuentaIVA);
            grpCuentas.Controls.Add(lblCuentaIVA2);
            grpCuentas.Controls.Add(txtCuentaIVA2);
            grpCuentas.Controls.Add(lblCuentaDebe);
            grpCuentas.Controls.Add(txtCuentaDebe);
            grpCuentas.Controls.Add(lblCentro);
            grpCuentas.Controls.Add(txtCentro);
            grpCuentas.Location = new System.Drawing.Point(12, 8);
            grpCuentas.Name     = "grpCuentas";
            grpCuentas.Size     = new System.Drawing.Size(462, 174);
            grpCuentas.TabStop  = false;
            grpCuentas.Text     = "Cuentas contables";

            // fila 0 ? Código proveedor / Cuenta ctble. proveedor
            lblCodigoProveedor.Location = new System.Drawing.Point(8, 18);
            lblCodigoProveedor.Name     = "lblCodigoProveedor";
            lblCodigoProveedor.Text     = "Código de proveedor:";

            txtCodigoProveedor.Location = new System.Drawing.Point(8, 34);
            txtCodigoProveedor.Name     = "txtCodigoProveedor";
            txtCodigoProveedor.Size     = new System.Drawing.Size(216, 20);
            txtCodigoProveedor.TabIndex = 0;

            lblCuentaContableProveedor.Location = new System.Drawing.Point(240, 18);
            lblCuentaContableProveedor.Name     = "lblCuentaContableProveedor";
            lblCuentaContableProveedor.Text     = "Cta. ctble. del proveedor:";

            txtCuentaContableProveedor.Location = new System.Drawing.Point(240, 34);
            txtCuentaContableProveedor.Name     = "txtCuentaContableProveedor";
            txtCuentaContableProveedor.Size     = new System.Drawing.Size(214, 20);
            txtCuentaContableProveedor.TabIndex = 1;

            // fila 1 ? Cuenta IVA slot 1 / slot 2
            lblCuentaIVA.Location = new System.Drawing.Point(8, 66);
            lblCuentaIVA.Name     = "lblCuentaIVA";
            lblCuentaIVA.Text     = "Cuenta IVA crédito (slot 1):";

            txtCuentaIVA.Location = new System.Drawing.Point(8, 82);
            txtCuentaIVA.Name     = "txtCuentaIVA";
            txtCuentaIVA.Size     = new System.Drawing.Size(216, 20);
            txtCuentaIVA.TabIndex = 2;

            lblCuentaIVA2.Location = new System.Drawing.Point(240, 66);
            lblCuentaIVA2.Name     = "lblCuentaIVA2";
            lblCuentaIVA2.Text     = "Cuenta IVA crédito (slot 2):";

            txtCuentaIVA2.Location = new System.Drawing.Point(240, 82);
            txtCuentaIVA2.Name     = "txtCuentaIVA2";
            txtCuentaIVA2.Size     = new System.Drawing.Size(214, 20);
            txtCuentaIVA2.TabIndex = 3;

            // fila 2 ? Cuenta debe / Centro
            lblCuentaDebe.Location = new System.Drawing.Point(8, 114);
            lblCuentaDebe.Name     = "lblCuentaDebe";
            lblCuentaDebe.Text     = "Cta. contable del debe:";

            txtCuentaDebe.Location = new System.Drawing.Point(8, 130);
            txtCuentaDebe.Name     = "txtCuentaDebe";
            txtCuentaDebe.Size     = new System.Drawing.Size(216, 20);
            txtCuentaDebe.TabIndex = 4;

            lblCentro.Location = new System.Drawing.Point(240, 114);
            lblCentro.Name     = "lblCentro";
            lblCentro.Text     = "Centro de costos (C/10):";

            txtCentro.Location = new System.Drawing.Point(240, 130);
            txtCentro.Name     = "txtCentro";
            txtCentro.Size     = new System.Drawing.Size(214, 20);
            txtCentro.TabIndex = 5;

            // ?? grpComprobante  y=190  h=90 ??????????????????????????????????
            grpComprobante.Controls.Add(lblProvincia);
            grpComprobante.Controls.Add(txtProvincia);
            grpComprobante.Controls.Add(lblCondicion);
            grpComprobante.Controls.Add(txtCondicion);
            grpComprobante.Controls.Add(lblFiscal);
            grpComprobante.Controls.Add(cmbFiscal);
            grpComprobante.Controls.Add(lblImputa);
            grpComprobante.Controls.Add(cmbImputa);
            grpComprobante.Location = new System.Drawing.Point(12, 190);
            grpComprobante.Name     = "grpComprobante";
            grpComprobante.Size     = new System.Drawing.Size(462, 90);
            grpComprobante.TabStop  = false;
            grpComprobante.Text     = "Opciones del comprobante";

            lblProvincia.Location = new System.Drawing.Point(8, 18);
            lblProvincia.Name     = "lblProvincia";
            lblProvincia.Text     = "Provincia (I/3):";

            txtProvincia.Location = new System.Drawing.Point(8, 34);
            txtProvincia.Name     = "txtProvincia";
            txtProvincia.Size     = new System.Drawing.Size(60, 20);
            txtProvincia.TabIndex = 0;

            lblCondicion.Location = new System.Drawing.Point(86, 18);
            lblCondicion.Name     = "lblCondicion";
            lblCondicion.Text     = "Condición (I/5):";

            txtCondicion.Location = new System.Drawing.Point(86, 34);
            txtCondicion.Name     = "txtCondicion";
            txtCondicion.Size     = new System.Drawing.Size(60, 20);
            txtCondicion.TabIndex = 1;

            lblFiscal.Location = new System.Drawing.Point(164, 18);
            lblFiscal.Name     = "lblFiscal";
            lblFiscal.Text     = "Fiscal:";

            cmbFiscal.Location = new System.Drawing.Point(164, 34);
            cmbFiscal.Name     = "cmbFiscal";
            cmbFiscal.Size     = new System.Drawing.Size(116, 24);
            cmbFiscal.TabIndex = 2;

            lblImputa.Location = new System.Drawing.Point(296, 18);
            lblImputa.Name     = "lblImputa";
            lblImputa.Text     = "Imputa:";

            cmbImputa.Location = new System.Drawing.Point(296, 34);
            cmbImputa.Name     = "cmbImputa";
            cmbImputa.Size     = new System.Drawing.Size(160, 24);
            cmbImputa.TabIndex = 3;

            // ?? grpFormato  y=288  h=62 ???????????????????????????????????????
            grpFormato.Controls.Add(lblFormatoFecha);
            grpFormato.Controls.Add(cmbFormatoFecha);
            grpFormato.Location = new System.Drawing.Point(12, 288);
            grpFormato.Name     = "grpFormato";
            grpFormato.Size     = new System.Drawing.Size(462, 62);
            grpFormato.TabStop  = false;
            grpFormato.Text     = "Formato de exportación";

            lblFormatoFecha.Location = new System.Drawing.Point(8, 18);
            lblFormatoFecha.Name     = "lblFormatoFecha";
            lblFormatoFecha.Text     = "Formato de fecha (campo D/8):";

            cmbFormatoFecha.Location = new System.Drawing.Point(8, 34);
            cmbFormatoFecha.Name     = "cmbFormatoFecha";
            cmbFormatoFecha.Size     = new System.Drawing.Size(220, 24);
            cmbFormatoFecha.TabIndex = 0;

            // ?? Form ?????????????????????????????????????????????????????????
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize          = new System.Drawing.Size(490, 412);
            Controls.Add(grpFormato);
            Controls.Add(grpComprobante);
            Controls.Add(grpCuentas);
            Controls.Add(pnlBotones);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            Name            = "FormConfigPresea";
            StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            Text            = "Configuración PRESEA";

            // ?? EndInit ???????????????????????????????????????????????????????
            ((System.ComponentModel.ISupportInitialize)pnlBotones).EndInit();
            pnlBotones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)btnCancelar).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnGuardar).EndInit();
            grpCuentas.ResumeLayout(false);
            grpCuentas.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)lblCodigoProveedor).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtCodigoProveedor).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblCuentaContableProveedor).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtCuentaContableProveedor).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblCuentaIVA).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtCuentaIVA).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblCuentaIVA2).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtCuentaIVA2).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblCuentaDebe).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtCuentaDebe).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblCentro).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtCentro).EndInit();
            grpComprobante.ResumeLayout(false);
            grpComprobante.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)lblProvincia).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtProvincia).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblCondicion).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtCondicion).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblFiscal).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbFiscal).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblImputa).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbImputa).EndInit();
            grpFormato.ResumeLayout(false);
            grpFormato.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)lblFormatoFecha).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbFormatoFecha).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        // ?? Field declarations ????????????????????????????????????????????????
        private Telerik.WinControls.UI.RadPanel        pnlBotones;
        private Telerik.WinControls.UI.RadButton       btnCancelar;
        private Telerik.WinControls.UI.RadButton       btnGuardar;
        private System.Windows.Forms.GroupBox          grpCuentas;
        private Telerik.WinControls.UI.RadLabel        lblCodigoProveedor;
        private Telerik.WinControls.UI.RadTextBox      txtCodigoProveedor;
        private Telerik.WinControls.UI.RadLabel        lblCuentaContableProveedor;
        private Telerik.WinControls.UI.RadTextBox      txtCuentaContableProveedor;
        private Telerik.WinControls.UI.RadLabel        lblCuentaIVA;
        private Telerik.WinControls.UI.RadTextBox      txtCuentaIVA;
        private Telerik.WinControls.UI.RadLabel        lblCuentaIVA2;
        private Telerik.WinControls.UI.RadTextBox      txtCuentaIVA2;
        private Telerik.WinControls.UI.RadLabel        lblCuentaDebe;
        private Telerik.WinControls.UI.RadTextBox      txtCuentaDebe;
        private Telerik.WinControls.UI.RadLabel        lblCentro;
        private Telerik.WinControls.UI.RadTextBox      txtCentro;
        private System.Windows.Forms.GroupBox          grpComprobante;
        private Telerik.WinControls.UI.RadLabel        lblProvincia;
        private Telerik.WinControls.UI.RadTextBox      txtProvincia;
        private Telerik.WinControls.UI.RadLabel        lblCondicion;
        private Telerik.WinControls.UI.RadTextBox      txtCondicion;
        private Telerik.WinControls.UI.RadLabel        lblFiscal;
        private Telerik.WinControls.UI.RadDropDownList cmbFiscal;
        private Telerik.WinControls.UI.RadLabel        lblImputa;
        private Telerik.WinControls.UI.RadDropDownList cmbImputa;
        private System.Windows.Forms.GroupBox          grpFormato;
        private Telerik.WinControls.UI.RadLabel        lblFormatoFecha;
        private Telerik.WinControls.UI.RadDropDownList cmbFormatoFecha;
    }
}
