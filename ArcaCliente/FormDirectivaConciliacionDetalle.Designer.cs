namespace ArcaCliente
{
    partial class FormDirectivaConciliacionDetalle
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
            pnlBotones      = new Telerik.WinControls.UI.RadPanel();
            btnGuardar      = new Telerik.WinControls.UI.RadButton();
            btnCancelar     = new Telerik.WinControls.UI.RadButton();
            grpDescripcion  = new System.Windows.Forms.GroupBox();
            lblDescripcion  = new Telerik.WinControls.UI.RadLabel();
            txtDescripcion  = new Telerik.WinControls.UI.RadTextBox();
            grpCampos       = new System.Windows.Forms.GroupBox();
            lblHint         = new Telerik.WinControls.UI.RadLabel();
            lstCampos       = new System.Windows.Forms.CheckedListBox();
            btnSubir        = new Telerik.WinControls.UI.RadButton();
            btnBajar        = new Telerik.WinControls.UI.RadButton();

            ((System.ComponentModel.ISupportInitialize)pnlBotones).BeginInit();
            pnlBotones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)btnGuardar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnCancelar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblDescripcion).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtDescripcion).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblHint).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnSubir).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnBajar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            SuspendLayout();

            // ?? pnlBotones ????????????????????????????????????????????????????????
            pnlBotones.Controls.Add(btnGuardar);
            pnlBotones.Controls.Add(btnCancelar);
            pnlBotones.Dock     = System.Windows.Forms.DockStyle.Bottom;
            pnlBotones.Name     = "pnlBotones";
            pnlBotones.Size     = new System.Drawing.Size(420, 46);
            pnlBotones.TabIndex = 0;

            btnGuardar.Location = new System.Drawing.Point(214, 10);
            btnGuardar.Name     = "btnGuardar";
            btnGuardar.Size     = new System.Drawing.Size(90, 26);
            btnGuardar.TabIndex = 0;
            btnGuardar.Text     = "Guardar";
            btnGuardar.Click   += BtnGuardar_Click;

            btnCancelar.Location = new System.Drawing.Point(312, 10);
            btnCancelar.Name     = "btnCancelar";
            btnCancelar.Size     = new System.Drawing.Size(90, 26);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text     = "Cancelar";
            btnCancelar.Click   += BtnCancelar_Click;

            // ?? grpDescripcion ????????????????????????????????????????????????????
            lblDescripcion.Location = new System.Drawing.Point(8, 20);
            lblDescripcion.Name     = "lblDescripcion";
            lblDescripcion.Text     = "Descripción:";

            txtDescripcion.Location = new System.Drawing.Point(8, 38);
            txtDescripcion.Name     = "txtDescripcion";
            txtDescripcion.Size     = new System.Drawing.Size(392, 24);
            txtDescripcion.TabIndex = 0;

            grpDescripcion.Controls.Add(lblDescripcion);
            grpDescripcion.Controls.Add(txtDescripcion);
            grpDescripcion.Location = new System.Drawing.Point(12, 12);
            grpDescripcion.Name     = "grpDescripcion";
            grpDescripcion.Size     = new System.Drawing.Size(412, 72);
            grpDescripcion.TabStop  = false;
            grpDescripcion.Text     = "Descripción";

            // ?? grpCampos ?????????????????????????????????????????????????????????
            lblHint.Location  = new System.Drawing.Point(8, 20);
            lblHint.Name      = "lblHint";
            lblHint.Size      = new System.Drawing.Size(320, 16);
            lblHint.Text      = "Marque los campos a usar y ordénelos con ? / ?:";

            lstCampos.CheckOnClick   = true;
            lstCampos.FormattingEnabled = true;
            lstCampos.Location       = new System.Drawing.Point(8, 40);
            lstCampos.Name           = "lstCampos";
            lstCampos.Size           = new System.Drawing.Size(280, 110);
            lstCampos.TabIndex       = 0;

            btnSubir.Location  = new System.Drawing.Point(298, 40);
            btnSubir.Name      = "btnSubir";
            btnSubir.Size      = new System.Drawing.Size(102, 26);
            btnSubir.TabIndex  = 1;
            btnSubir.Text      = "?  Subir";
            btnSubir.Click    += BtnSubir_Click;

            btnBajar.Location  = new System.Drawing.Point(298, 74);
            btnBajar.Name      = "btnBajar";
            btnBajar.Size      = new System.Drawing.Size(102, 26);
            btnBajar.TabIndex  = 2;
            btnBajar.Text      = "?  Bajar";
            btnBajar.Click    += BtnBajar_Click;

            grpCampos.Controls.Add(lblHint);
            grpCampos.Controls.Add(lstCampos);
            grpCampos.Controls.Add(btnSubir);
            grpCampos.Controls.Add(btnBajar);
            grpCampos.Location = new System.Drawing.Point(12, 92);
            grpCampos.Name     = "grpCampos";
            grpCampos.Size     = new System.Drawing.Size(412, 164);
            grpCampos.TabStop  = false;
            grpCampos.Text     = "Campos de matching";

            // ?? Form ??????????????????????????????????????????????????????????????
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize          = new System.Drawing.Size(436, 314);
            Controls.Add(grpDescripcion);
            Controls.Add(grpCampos);
            Controls.Add(pnlBotones);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            Name            = "FormDirectivaConciliacionDetalle";
            StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            Text            = "Directiva de conciliación";

            ((System.ComponentModel.ISupportInitialize)pnlBotones).EndInit();
            pnlBotones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)btnGuardar).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnCancelar).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblDescripcion).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtDescripcion).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblHint).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnSubir).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnBajar).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        private Telerik.WinControls.UI.RadPanel      pnlBotones;
        private Telerik.WinControls.UI.RadButton     btnGuardar;
        private Telerik.WinControls.UI.RadButton     btnCancelar;
        private System.Windows.Forms.GroupBox        grpDescripcion;
        private Telerik.WinControls.UI.RadLabel      lblDescripcion;
        private Telerik.WinControls.UI.RadTextBox    txtDescripcion;
        private System.Windows.Forms.GroupBox        grpCampos;
        private Telerik.WinControls.UI.RadLabel      lblHint;
        private System.Windows.Forms.CheckedListBox  lstCampos;
        private Telerik.WinControls.UI.RadButton     btnSubir;
        private Telerik.WinControls.UI.RadButton     btnBajar;
    }
}
