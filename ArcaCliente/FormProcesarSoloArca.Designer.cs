namespace ArcaCliente
{
    partial class FormProcesarSoloArca
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
            Telerik.WinControls.UI.TableViewDefinition tableViewDefinition1 = new Telerik.WinControls.UI.TableViewDefinition();
            pnlTop = new Telerik.WinControls.UI.RadPanel();
            lblProgreso = new Telerik.WinControls.UI.RadLabel();
            grpInfo = new System.Windows.Forms.GroupBox();
            lblTotal = new Telerik.WinControls.UI.RadLabel();
            lblFecha = new Telerik.WinControls.UI.RadLabel();
            lblNumero = new Telerik.WinControls.UI.RadLabel();
            lblTipo = new Telerik.WinControls.UI.RadLabel();
            lblEmisor = new Telerik.WinControls.UI.RadLabel();
            grpResolucion = new System.Windows.Forms.GroupBox();
            lblMonedaInfo = new Telerik.WinControls.UI.RadLabel();
            pnlTipoCambio = new Telerik.WinControls.UI.RadPanel();
            txtTipoCambio = new Telerik.WinControls.UI.RadTextBox();
            lblTcCaption = new Telerik.WinControls.UI.RadLabel();
            lblProveedorNombre = new Telerik.WinControls.UI.RadLabel();
            lblProvCaption = new Telerik.WinControls.UI.RadLabel();
            grpTasas = new System.Windows.Forms.GroupBox();
            dgvTasas = new Telerik.WinControls.UI.RadGridView();
            lblEstado = new Telerik.WinControls.UI.RadLabel();
            pnlBotones = new Telerik.WinControls.UI.RadPanel();
            btnCancelarTodo = new Telerik.WinControls.UI.RadButton();
            btnOmitir = new Telerik.WinControls.UI.RadButton();
            btnDarDeAlta = new Telerik.WinControls.UI.RadButton();
            ((System.ComponentModel.ISupportInitialize)pnlTop).BeginInit();
            pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lblProgreso).BeginInit();
            grpInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lblTotal).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblFecha).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblNumero).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblTipo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblEmisor).BeginInit();
            grpResolucion.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lblMonedaInfo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlTipoCambio).BeginInit();
            pnlTipoCambio.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtTipoCambio).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblTcCaption).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblProveedorNombre).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblProvCaption).BeginInit();
            grpTasas.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTasas).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvTasas.MasterTemplate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblEstado).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlBotones).BeginInit();
            pnlBotones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)btnCancelarTodo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnOmitir).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnDarDeAlta).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            SuspendLayout();
            // 
            // pnlTop
            // 
            pnlTop.Controls.Add(lblProgreso);
            pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            pnlTop.Location = new System.Drawing.Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new System.Drawing.Size(733, 32);
            pnlTop.TabIndex = 0;
            // 
            // lblProgreso
            // 
            lblProgreso.Dock = System.Windows.Forms.DockStyle.Fill;
            lblProgreso.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblProgreso.Location = new System.Drawing.Point(0, 0);
            lblProgreso.Name = "lblProgreso";
            lblProgreso.Padding = new System.Windows.Forms.Padding(10, 7, 0, 0);
            lblProgreso.Size = new System.Drawing.Size(152, 28);
            lblProgreso.TabIndex = 0;
            lblProgreso.Text = "Comprobante 1 de N";
            // 
            // grpInfo
            // 
            grpInfo.Controls.Add(lblTotal);
            grpInfo.Controls.Add(lblFecha);
            grpInfo.Controls.Add(lblNumero);
            grpInfo.Controls.Add(lblTipo);
            grpInfo.Controls.Add(lblEmisor);
            grpInfo.Dock = System.Windows.Forms.DockStyle.Top;
            grpInfo.Location = new System.Drawing.Point(0, 32);
            grpInfo.Name = "grpInfo";
            grpInfo.Size = new System.Drawing.Size(733, 150);
            grpInfo.TabIndex = 1;
            grpInfo.TabStop = false;
            grpInfo.Text = "Comprobante ARCA";
            // 
            // lblTotal
            // 
            lblTotal.AutoSize = false;
            lblTotal.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblTotal.Location = new System.Drawing.Point(12, 118);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new System.Drawing.Size(530, 22);
            lblTotal.TabIndex = 0;
            lblTotal.Text = "Total ARCA: ...";
            // 
            // lblFecha
            // 
            lblFecha.AutoSize = false;
            lblFecha.Location = new System.Drawing.Point(12, 94);
            lblFecha.Name = "lblFecha";
            lblFecha.Size = new System.Drawing.Size(530, 22);
            lblFecha.TabIndex = 1;
            lblFecha.Text = "Fecha: ...";
            // 
            // lblNumero
            // 
            lblNumero.AutoSize = false;
            lblNumero.Location = new System.Drawing.Point(12, 70);
            lblNumero.Name = "lblNumero";
            lblNumero.Size = new System.Drawing.Size(530, 22);
            lblNumero.TabIndex = 2;
            lblNumero.Text = "Número: ...";
            // 
            // lblTipo
            // 
            lblTipo.AutoSize = false;
            lblTipo.Location = new System.Drawing.Point(12, 46);
            lblTipo.Name = "lblTipo";
            lblTipo.Size = new System.Drawing.Size(530, 22);
            lblTipo.TabIndex = 3;
            lblTipo.Text = "Tipo: ...";
            // 
            // lblEmisor
            // 
            lblEmisor.AutoSize = false;
            lblEmisor.Location = new System.Drawing.Point(12, 22);
            lblEmisor.Name = "lblEmisor";
            lblEmisor.Size = new System.Drawing.Size(530, 22);
            lblEmisor.TabIndex = 4;
            lblEmisor.Text = "Emisor: ...";
            // 
            // grpResolucion
            // 
            grpResolucion.Controls.Add(lblMonedaInfo);
            grpResolucion.Controls.Add(pnlTipoCambio);
            grpResolucion.Controls.Add(lblProveedorNombre);
            grpResolucion.Controls.Add(lblProvCaption);
            grpResolucion.Dock = System.Windows.Forms.DockStyle.Top;
            grpResolucion.Location = new System.Drawing.Point(0, 182);
            grpResolucion.Name = "grpResolucion";
            grpResolucion.Size = new System.Drawing.Size(733, 120);
            grpResolucion.TabIndex = 2;
            grpResolucion.TabStop = false;
            grpResolucion.Text = "Datos para el alta";
            // 
            // lblMonedaInfo
            // 
            lblMonedaInfo.AutoSize = false;
            lblMonedaInfo.ForeColor = System.Drawing.Color.DarkOrange;
            lblMonedaInfo.Location = new System.Drawing.Point(12, 84);
            lblMonedaInfo.Name = "lblMonedaInfo";
            lblMonedaInfo.Size = new System.Drawing.Size(530, 22);
            lblMonedaInfo.TabIndex = 0;
            // 
            // pnlTipoCambio
            // 
            pnlTipoCambio.Controls.Add(txtTipoCambio);
            pnlTipoCambio.Controls.Add(lblTcCaption);
            pnlTipoCambio.Location = new System.Drawing.Point(12, 52);
            pnlTipoCambio.Name = "pnlTipoCambio";
            pnlTipoCambio.Size = new System.Drawing.Size(530, 26);
            pnlTipoCambio.TabIndex = 0;
            pnlTipoCambio.Visible = false;
            // 
            // txtTipoCambio
            // 
            txtTipoCambio.Location = new System.Drawing.Point(155, 1);
            txtTipoCambio.Name = "txtTipoCambio";
            txtTipoCambio.Size = new System.Drawing.Size(110, 24);
            txtTipoCambio.TabIndex = 0;
            // 
            // lblTcCaption
            // 
            lblTcCaption.Location = new System.Drawing.Point(0, 3);
            lblTcCaption.Name = "lblTcCaption";
            lblTcCaption.Size = new System.Drawing.Size(86, 18);
            lblTcCaption.TabIndex = 1;
            lblTcCaption.Text = "Tipo de cambio:";
            // 
            // lblProveedorNombre
            // 
            lblProveedorNombre.AutoSize = false;
            lblProveedorNombre.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblProveedorNombre.Location = new System.Drawing.Point(165, 24);
            lblProveedorNombre.Name = "lblProveedorNombre";
            lblProveedorNombre.Size = new System.Drawing.Size(375, 22);
            lblProveedorNombre.TabIndex = 1;
            // 
            // lblProvCaption
            // 
            lblProvCaption.AutoSize = false;
            lblProvCaption.Location = new System.Drawing.Point(12, 24);
            lblProvCaption.Name = "lblProvCaption";
            lblProvCaption.Size = new System.Drawing.Size(150, 22);
            lblProvCaption.TabIndex = 2;
            lblProvCaption.Text = "Proveedor Octosis:";
            // 
            // grpTasas
            // 
            grpTasas.Controls.Add(dgvTasas);
            grpTasas.Dock = System.Windows.Forms.DockStyle.Fill;
            grpTasas.Location = new System.Drawing.Point(0, 302);
            grpTasas.Name = "grpTasas";
            grpTasas.Padding = new System.Windows.Forms.Padding(4, 6, 4, 4);
            grpTasas.Size = new System.Drawing.Size(733, 268);
            grpTasas.TabIndex = 3;
            grpTasas.TabStop = false;
            grpTasas.Text = "Impuestos detectados";
            // 
            // dgvTasas
            // 
            dgvTasas.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvTasas.Location = new System.Drawing.Point(4, 22);
            // 
            // 
            // 
            dgvTasas.MasterTemplate.AllowAddNewRow = false;
            dgvTasas.MasterTemplate.AllowDeleteRow = false;
            dgvTasas.MasterTemplate.AllowEditRow = false;
            dgvTasas.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            dgvTasas.MasterTemplate.ViewDefinition = tableViewDefinition1;
            dgvTasas.Name = "dgvTasas";
            dgvTasas.ReadOnly = true;
            dgvTasas.ShowGroupPanel = false;
            dgvTasas.Size = new System.Drawing.Size(725, 242);
            dgvTasas.TabIndex = 0;
            // 
            // lblEstado
            // 
            lblEstado.AutoSize = false;
            lblEstado.Dock = System.Windows.Forms.DockStyle.Bottom;
            lblEstado.Location = new System.Drawing.Point(0, 570);
            lblEstado.Name = "lblEstado";
            lblEstado.Padding = new System.Windows.Forms.Padding(8, 0, 0, 2);
            lblEstado.Size = new System.Drawing.Size(733, 22);
            lblEstado.TabIndex = 4;
            // 
            // pnlBotones
            // 
            pnlBotones.Controls.Add(btnCancelarTodo);
            pnlBotones.Controls.Add(btnOmitir);
            pnlBotones.Controls.Add(btnDarDeAlta);
            pnlBotones.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlBotones.Location = new System.Drawing.Point(0, 592);
            pnlBotones.Name = "pnlBotones";
            pnlBotones.Size = new System.Drawing.Size(733, 48);
            pnlBotones.TabIndex = 4;
            // 
            // btnCancelarTodo
            // 
            btnCancelarTodo.Location = new System.Drawing.Point(611, 10);
            btnCancelarTodo.Name = "btnCancelarTodo";
            btnCancelarTodo.Size = new System.Drawing.Size(110, 30);
            btnCancelarTodo.TabIndex = 2;
            btnCancelarTodo.Text = "Cancelar todo";
            btnCancelarTodo.Click += btnCancelarTodo_Click;
            // 
            // btnOmitir
            // 
            btnOmitir.Location = new System.Drawing.Point(172, 10);
            btnOmitir.Name = "btnOmitir";
            btnOmitir.Size = new System.Drawing.Size(110, 30);
            btnOmitir.TabIndex = 1;
            btnOmitir.Text = "Omitir";
            btnOmitir.Click += btnOmitir_Click;
            // 
            // btnDarDeAlta
            // 
            btnDarDeAlta.Location = new System.Drawing.Point(12, 10);
            btnDarDeAlta.Name = "btnDarDeAlta";
            btnDarDeAlta.Size = new System.Drawing.Size(150, 30);
            btnDarDeAlta.TabIndex = 0;
            btnDarDeAlta.Text = "? Dar de alta";
            btnDarDeAlta.Click += btnDarDeAlta_Click;
            // 
            // FormProcesarSoloArca
            // 
            AutoScaleBaseSize = new System.Drawing.Size(7, 15);
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(733, 640);
            Controls.Add(grpTasas);
            Controls.Add(lblEstado);
            Controls.Add(pnlBotones);
            Controls.Add(grpResolucion);
            Controls.Add(grpInfo);
            Controls.Add(pnlTop);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormProcesarSoloArca";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Procesar comprobantes Solo ARCA";
            ((System.ComponentModel.ISupportInitialize)pnlTop).EndInit();
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)lblProgreso).EndInit();
            grpInfo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)lblTotal).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblFecha).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblNumero).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblTipo).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblEmisor).EndInit();
            grpResolucion.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)lblMonedaInfo).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlTipoCambio).EndInit();
            pnlTipoCambio.ResumeLayout(false);
            pnlTipoCambio.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtTipoCambio).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblTcCaption).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblProveedorNombre).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblProvCaption).EndInit();
            grpTasas.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvTasas.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvTasas).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblEstado).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlBotones).EndInit();
            pnlBotones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)btnCancelarTodo).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnOmitir).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnDarDeAlta).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        private Telerik.WinControls.UI.RadPanel   pnlTop;
        private Telerik.WinControls.UI.RadLabel   lblProgreso;
        private System.Windows.Forms.GroupBox     grpInfo;
        private Telerik.WinControls.UI.RadLabel   lblEmisor;
        private Telerik.WinControls.UI.RadLabel   lblTipo;
        private Telerik.WinControls.UI.RadLabel   lblNumero;
        private Telerik.WinControls.UI.RadLabel   lblFecha;
        private Telerik.WinControls.UI.RadLabel   lblTotal;
        private System.Windows.Forms.GroupBox     grpResolucion;
        private Telerik.WinControls.UI.RadLabel   lblProvCaption;
        private Telerik.WinControls.UI.RadLabel   lblProveedorNombre;
        private Telerik.WinControls.UI.RadPanel   pnlTipoCambio;
        private Telerik.WinControls.UI.RadLabel   lblTcCaption;
        private Telerik.WinControls.UI.RadTextBox txtTipoCambio;
        private Telerik.WinControls.UI.RadLabel   lblMonedaInfo;
        private System.Windows.Forms.GroupBox     grpTasas;
        private Telerik.WinControls.UI.RadGridView dgvTasas;
        private Telerik.WinControls.UI.RadLabel   lblEstado;
        private Telerik.WinControls.UI.RadPanel   pnlBotones;
        private Telerik.WinControls.UI.RadButton  btnDarDeAlta;
        private Telerik.WinControls.UI.RadButton  btnOmitir;
        private Telerik.WinControls.UI.RadButton  btnCancelarTodo;
    }
}
