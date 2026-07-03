namespace ArcaCliente
{
    partial class FormDirectivasConciliacion
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
            pnlInfo       = new Telerik.WinControls.UI.RadPanel();
            lblInfo       = new Telerik.WinControls.UI.RadLabel();
            pnlBotones    = new Telerik.WinControls.UI.RadPanel();
            btnAgregar    = new Telerik.WinControls.UI.RadButton();
            btnEditar     = new Telerik.WinControls.UI.RadButton();
            btnEliminar   = new Telerik.WinControls.UI.RadButton();
            btnSubir      = new Telerik.WinControls.UI.RadButton();
            btnBajar      = new Telerik.WinControls.UI.RadButton();
            btnGuardar    = new Telerik.WinControls.UI.RadButton();
            btnCancelar   = new Telerik.WinControls.UI.RadButton();
            gridDirectivas = new Telerik.WinControls.UI.RadGridView();

            ((System.ComponentModel.ISupportInitialize)pnlInfo).BeginInit();
            pnlInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lblInfo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlBotones).BeginInit();
            pnlBotones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)btnAgregar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnEditar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnEliminar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnSubir).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnBajar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnGuardar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnCancelar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridDirectivas).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            SuspendLayout();

            // ?? pnlInfo ??????????????????????????????????????????????????????????
            lblInfo.AutoSize = false;
            lblInfo.Dock     = System.Windows.Forms.DockStyle.Fill;
            lblInfo.Name     = "lblInfo";
            lblInfo.Padding  = new System.Windows.Forms.Padding(8, 6, 8, 6);
            lblInfo.Text     = "La primera directiva es la principal y no puede modificarse. " +
                               "Las siguientes actúan sobre el residual de la anterior.";

            pnlInfo.Controls.Add(lblInfo);
            pnlInfo.Dock     = System.Windows.Forms.DockStyle.Top;
            pnlInfo.Name     = "pnlInfo";
            pnlInfo.Size     = new System.Drawing.Size(680, 38);
            pnlInfo.TabIndex = 0;

            // ?? pnlBotones ???????????????????????????????????????????????????????
            pnlBotones.Controls.Add(btnAgregar);
            pnlBotones.Controls.Add(btnEditar);
            pnlBotones.Controls.Add(btnEliminar);
            pnlBotones.Controls.Add(btnSubir);
            pnlBotones.Controls.Add(btnBajar);
            pnlBotones.Controls.Add(btnGuardar);
            pnlBotones.Controls.Add(btnCancelar);
            pnlBotones.Dock     = System.Windows.Forms.DockStyle.Bottom;
            pnlBotones.Name     = "pnlBotones";
            pnlBotones.Size     = new System.Drawing.Size(680, 48);
            pnlBotones.TabIndex = 1;

            btnAgregar.Location = new System.Drawing.Point(12, 10);
            btnAgregar.Name     = "btnAgregar";
            btnAgregar.Size     = new System.Drawing.Size(90, 28);
            btnAgregar.TabIndex = 0;
            btnAgregar.Text     = "+ Agregar";
            btnAgregar.Click   += BtnAgregar_Click;

            btnEditar.Location  = new System.Drawing.Point(110, 10);
            btnEditar.Name      = "btnEditar";
            btnEditar.Size      = new System.Drawing.Size(90, 28);
            btnEditar.TabIndex  = 1;
            btnEditar.Text      = "Editar...";
            btnEditar.Click    += BtnEditar_Click;

            btnEliminar.Location = new System.Drawing.Point(208, 10);
            btnEliminar.Name     = "btnEliminar";
            btnEliminar.Size     = new System.Drawing.Size(90, 28);
            btnEliminar.TabIndex = 2;
            btnEliminar.Text     = "Eliminar";
            btnEliminar.Click   += BtnEliminar_Click;

            btnSubir.Location   = new System.Drawing.Point(310, 10);
            btnSubir.Name       = "btnSubir";
            btnSubir.Size       = new System.Drawing.Size(80, 28);
            btnSubir.TabIndex   = 3;
            btnSubir.Text       = "?  Subir";
            btnSubir.Click     += BtnSubir_Click;

            btnBajar.Location   = new System.Drawing.Point(398, 10);
            btnBajar.Name       = "btnBajar";
            btnBajar.Size       = new System.Drawing.Size(80, 28);
            btnBajar.TabIndex   = 4;
            btnBajar.Text       = "?  Bajar";
            btnBajar.Click     += BtnBajar_Click;

            btnGuardar.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnGuardar.Location = new System.Drawing.Point(490, 10);
            btnGuardar.Name     = "btnGuardar";
            btnGuardar.Size     = new System.Drawing.Size(90, 28);
            btnGuardar.TabIndex = 5;
            btnGuardar.Text     = "Guardar";
            btnGuardar.Click   += BtnGuardar_Click;

            btnCancelar.Location = new System.Drawing.Point(588, 10);
            btnCancelar.Name     = "btnCancelar";
            btnCancelar.Size     = new System.Drawing.Size(80, 28);
            btnCancelar.TabIndex = 6;
            btnCancelar.Text     = "Cancelar";
            btnCancelar.Click   += BtnCancelar_Click;

            // ?? gridDirectivas ???????????????????????????????????????????????????
            gridDirectivas.Dock     = System.Windows.Forms.DockStyle.Fill;
            gridDirectivas.Name     = "gridDirectivas";
            gridDirectivas.TabIndex = 2;
            gridDirectivas.MasterTemplate.AllowAddNewRow = false;
            gridDirectivas.MasterTemplate.AllowDeleteRow = false;
            gridDirectivas.MasterTemplate.AllowEditRow   = false;
            gridDirectivas.CellDoubleClick += GridDirectivas_CellDoubleClick;
            gridDirectivas.SelectionChanged += GridDirectivas_SelectionChanged;

            // ?? Form ??????????????????????????????????????????????????????????????
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize          = new System.Drawing.Size(680, 360);
            Controls.Add(gridDirectivas);
            Controls.Add(pnlInfo);
            Controls.Add(pnlBotones);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            Name            = "FormDirectivasConciliacion";
            StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            Text            = "Directivas de conciliación";

            ((System.ComponentModel.ISupportInitialize)pnlInfo).EndInit();
            pnlInfo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)lblInfo).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlBotones).EndInit();
            pnlBotones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)btnAgregar).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnEditar).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnEliminar).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnSubir).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnBajar).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnGuardar).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnCancelar).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridDirectivas).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        private Telerik.WinControls.UI.RadPanel     pnlInfo;
        private Telerik.WinControls.UI.RadLabel     lblInfo;
        private Telerik.WinControls.UI.RadPanel     pnlBotones;
        private Telerik.WinControls.UI.RadButton    btnAgregar;
        private Telerik.WinControls.UI.RadButton    btnEditar;
        private Telerik.WinControls.UI.RadButton    btnEliminar;
        private Telerik.WinControls.UI.RadButton    btnSubir;
        private Telerik.WinControls.UI.RadButton    btnBajar;
        private Telerik.WinControls.UI.RadButton    btnGuardar;
        private Telerik.WinControls.UI.RadButton    btnCancelar;
        private Telerik.WinControls.UI.RadGridView  gridDirectivas;
    }
}
