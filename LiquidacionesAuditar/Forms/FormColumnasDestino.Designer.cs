namespace LiquidacionesAuditar
{
    partial class FormColumnasDestino
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            Telerik.WinControls.UI.TableViewDefinition tableViewDefinition2 = new Telerik.WinControls.UI.TableViewDefinition();
            Telerik.WinControls.UI.RadListDataItem radListDataItem9 = new Telerik.WinControls.UI.RadListDataItem();
            Telerik.WinControls.UI.RadListDataItem radListDataItem10 = new Telerik.WinControls.UI.RadListDataItem();
            Telerik.WinControls.UI.RadListDataItem radListDataItem11 = new Telerik.WinControls.UI.RadListDataItem();
            Telerik.WinControls.UI.RadListDataItem radListDataItem1 = new Telerik.WinControls.UI.RadListDataItem();
            Telerik.WinControls.UI.RadListDataItem radListDataItem2 = new Telerik.WinControls.UI.RadListDataItem();
            Telerik.WinControls.UI.RadListDataItem radListDataItem3 = new Telerik.WinControls.UI.RadListDataItem();
            Telerik.WinControls.UI.RadListDataItem radListDataItem12 = new Telerik.WinControls.UI.RadListDataItem();
            Telerik.WinControls.UI.RadListDataItem radListDataItem13 = new Telerik.WinControls.UI.RadListDataItem();
            panelMarca = new System.Windows.Forms.Panel();
            btnClonar = new Telerik.WinControls.UI.RadButton();
            cmbMarcaFiltro = new Telerik.WinControls.UI.RadDropDownList();
            lblMarcaFiltro = new System.Windows.Forms.Label();
            splitMain = new System.Windows.Forms.SplitContainer();
            gridDest = new Telerik.WinControls.UI.RadGridView();
            grpDetalle = new System.Windows.Forms.GroupBox();
            label1 = new System.Windows.Forms.Label();
            txtCondicionSigno = new Telerik.WinControls.UI.RadTextBox();
            lblIdColumna = new System.Windows.Forms.Label();
            txtIdColumna = new Telerik.WinControls.UI.RadTextBox();
            lblTipoReg = new System.Windows.Forms.Label();
            cmbTipoRegistro = new Telerik.WinControls.UI.RadDropDownList();
            lblTipoDato = new System.Windows.Forms.Label();
            cmbTipoDato = new Telerik.WinControls.UI.RadDropDownList();
            lblOrden = new System.Windows.Forms.Label();
            nudOrden = new System.Windows.Forms.NumericUpDown();
            lblValorFijo = new System.Windows.Forms.Label();
            txtValorFijo = new Telerik.WinControls.UI.RadTextBox();
            lblCondicion = new System.Windows.Forms.Label();
            txtCondicion = new Telerik.WinControls.UI.RadTextBox();
            lblRelaciones = new System.Windows.Forms.Label();
            clbRelaciones = new System.Windows.Forms.CheckedListBox();
            panelBotones = new System.Windows.Forms.Panel();
            btnNuevo = new Telerik.WinControls.UI.RadButton();
            btnGuardar = new Telerik.WinControls.UI.RadButton();
            btnEliminar = new Telerik.WinControls.UI.RadButton();
            ch_filtro = new Telerik.WinControls.UI.RadCheckBox();
            panelMarca.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)btnClonar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbMarcaFiltro).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridDest).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridDest.MasterTemplate).BeginInit();
            grpDetalle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtCondicionSigno).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtIdColumna).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbTipoRegistro).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbTipoDato).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudOrden).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtValorFijo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtCondicion).BeginInit();
            panelBotones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)btnNuevo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnGuardar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnEliminar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ch_filtro).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            SuspendLayout();
            // 
            // panelMarca
            // 
            panelMarca.BackColor = System.Drawing.Color.FromArgb(220, 230, 242);
            panelMarca.Controls.Add(btnClonar);
            panelMarca.Controls.Add(cmbMarcaFiltro);
            panelMarca.Controls.Add(lblMarcaFiltro);
            panelMarca.Dock = System.Windows.Forms.DockStyle.Top;
            panelMarca.Location = new System.Drawing.Point(0, 0);
            panelMarca.Name = "panelMarca";
            panelMarca.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            panelMarca.Size = new System.Drawing.Size(1100, 40);
            panelMarca.TabIndex = 2;
            // 
            // btnClonar
            // 
            btnClonar.Location = new System.Drawing.Point(436, 8);
            btnClonar.Name = "btnClonar";
            btnClonar.Size = new System.Drawing.Size(160, 24);
            btnClonar.TabIndex = 1;
            btnClonar.Text = "Clonar a otra marca...";
            btnClonar.Click += btnClonar_Click;
            // 
            // cmbMarcaFiltro
            // 
            cmbMarcaFiltro.Location = new System.Drawing.Point(140, 8);
            cmbMarcaFiltro.Name = "cmbMarcaFiltro";
            cmbMarcaFiltro.Size = new System.Drawing.Size(280, 24);
            cmbMarcaFiltro.TabIndex = 0;
            cmbMarcaFiltro.SelectedIndexChanged += cmbMarcaFiltro_SelectedIndexChanged;
            // 
            // lblMarcaFiltro
            // 
            lblMarcaFiltro.AutoSize = true;
            lblMarcaFiltro.Location = new System.Drawing.Point(8, 12);
            lblMarcaFiltro.Name = "lblMarcaFiltro";
            lblMarcaFiltro.Size = new System.Drawing.Size(107, 15);
            lblMarcaFiltro.TabIndex = 2;
            lblMarcaFiltro.Text = "Marca/Procesador:";
            // 
            // splitMain
            // 
            splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitMain.Location = new System.Drawing.Point(0, 40);
            splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(gridDest);
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(grpDetalle);
            splitMain.Panel2.Controls.Add(panelBotones);
            splitMain.Size = new System.Drawing.Size(1100, 614);
            splitMain.SplitterDistance = 557;
            splitMain.TabIndex = 1;
            // 
            // gridDest
            // 
            gridDest.Dock = System.Windows.Forms.DockStyle.Fill;
            gridDest.Location = new System.Drawing.Point(0, 0);
            // 
            // 
            // 
            gridDest.MasterTemplate.ViewDefinition = tableViewDefinition2;
            gridDest.Name = "gridDest";
            gridDest.ReadOnly = true;
            gridDest.Size = new System.Drawing.Size(557, 614);
            gridDest.TabIndex = 0;
            gridDest.SelectionChanged += gridDest_SelectionChanged;
            // 
            // grpDetalle
            // 
            grpDetalle.Controls.Add(ch_filtro);
            grpDetalle.Controls.Add(label1);
            grpDetalle.Controls.Add(txtCondicionSigno);
            grpDetalle.Controls.Add(lblIdColumna);
            grpDetalle.Controls.Add(txtIdColumna);
            grpDetalle.Controls.Add(lblTipoReg);
            grpDetalle.Controls.Add(cmbTipoRegistro);
            grpDetalle.Controls.Add(lblTipoDato);
            grpDetalle.Controls.Add(cmbTipoDato);
            grpDetalle.Controls.Add(lblOrden);
            grpDetalle.Controls.Add(nudOrden);
            grpDetalle.Controls.Add(lblValorFijo);
            grpDetalle.Controls.Add(txtValorFijo);
            grpDetalle.Controls.Add(lblCondicion);
            grpDetalle.Controls.Add(txtCondicion);
            grpDetalle.Controls.Add(lblRelaciones);
            grpDetalle.Controls.Add(clbRelaciones);
            grpDetalle.Dock = System.Windows.Forms.DockStyle.Fill;
            grpDetalle.Location = new System.Drawing.Point(0, 0);
            grpDetalle.Name = "grpDetalle";
            grpDetalle.Padding = new System.Windows.Forms.Padding(8);
            grpDetalle.Size = new System.Drawing.Size(539, 566);
            grpDetalle.TabIndex = 0;
            grpDetalle.TabStop = false;
            grpDetalle.Text = "Detalle columna destino";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(10, 312);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(177, 15);
            label1.TabIndex = 14;
            label1.Text = "Condición SIGNO para Negativo";
            // 
            // txtCondicionSigno
            // 
            txtCondicionSigno.Location = new System.Drawing.Point(10, 330);
            txtCondicionSigno.Name = "txtCondicionSigno";
            txtCondicionSigno.NullText = "Ej: SIGNO M.B==-|-|+";
            txtCondicionSigno.Size = new System.Drawing.Size(500, 24);
            txtCondicionSigno.TabIndex = 15;
            // 
            // lblIdColumna
            // 
            lblIdColumna.AutoSize = true;
            lblIdColumna.Location = new System.Drawing.Point(10, 20);
            lblIdColumna.Name = "lblIdColumna";
            lblIdColumna.Size = new System.Drawing.Size(104, 15);
            lblIdColumna.TabIndex = 0;
            lblIdColumna.Text = "Nombre columna:";
            // 
            // txtIdColumna
            // 
            txtIdColumna.Location = new System.Drawing.Point(10, 38);
            txtIdColumna.Name = "txtIdColumna";
            txtIdColumna.Size = new System.Drawing.Size(340, 24);
            txtIdColumna.TabIndex = 1;
            // 
            // lblTipoReg
            // 
            lblTipoReg.AutoSize = true;
            lblTipoReg.Location = new System.Drawing.Point(10, 68);
            lblTipoReg.Name = "lblTipoReg";
            lblTipoReg.Size = new System.Drawing.Size(80, 15);
            lblTipoReg.TabIndex = 2;
            lblTipoReg.Text = "Tipo Registro:";
            // 
            // cmbTipoRegistro
            // 
            radListDataItem9.Text = "CAB";
            radListDataItem10.Text = "DET";
            radListDataItem11.Text = "CON";
            cmbTipoRegistro.Items.Add(radListDataItem9);
            cmbTipoRegistro.Items.Add(radListDataItem10);
            cmbTipoRegistro.Items.Add(radListDataItem11);
            cmbTipoRegistro.Location = new System.Drawing.Point(10, 86);
            cmbTipoRegistro.Name = "cmbTipoRegistro";
            cmbTipoRegistro.Size = new System.Drawing.Size(120, 24);
            cmbTipoRegistro.TabIndex = 3;
            // 
            // lblTipoDato
            // 
            lblTipoDato.AutoSize = true;
            lblTipoDato.Location = new System.Drawing.Point(10, 116);
            lblTipoDato.Name = "lblTipoDato";
            lblTipoDato.Size = new System.Drawing.Size(62, 15);
            lblTipoDato.TabIndex = 4;
            lblTipoDato.Text = "Tipo Dato:";
            // 
            // cmbTipoDato
            // 
            radListDataItem1.Text = "STRING";
            radListDataItem2.Text = "INT";
            radListDataItem3.Text = "DECIMAL";
            radListDataItem12.Text = "DATE";
            radListDataItem13.Text = "NVARCHAR";
            cmbTipoDato.Items.Add(radListDataItem1);
            cmbTipoDato.Items.Add(radListDataItem2);
            cmbTipoDato.Items.Add(radListDataItem3);
            cmbTipoDato.Items.Add(radListDataItem12);
            cmbTipoDato.Items.Add(radListDataItem13);
            cmbTipoDato.Location = new System.Drawing.Point(10, 134);
            cmbTipoDato.Name = "cmbTipoDato";
            cmbTipoDato.Size = new System.Drawing.Size(120, 24);
            cmbTipoDato.TabIndex = 5;
            // 
            // lblOrden
            // 
            lblOrden.AutoSize = true;
            lblOrden.Location = new System.Drawing.Point(10, 164);
            lblOrden.Name = "lblOrden";
            lblOrden.Size = new System.Drawing.Size(43, 15);
            lblOrden.TabIndex = 6;
            lblOrden.Text = "Orden:";
            // 
            // nudOrden
            // 
            nudOrden.Location = new System.Drawing.Point(10, 182);
            nudOrden.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            nudOrden.Name = "nudOrden";
            nudOrden.Size = new System.Drawing.Size(80, 23);
            nudOrden.TabIndex = 7;
            // 
            // lblValorFijo
            // 
            lblValorFijo.AutoSize = true;
            lblValorFijo.Location = new System.Drawing.Point(10, 212);
            lblValorFijo.Name = "lblValorFijo";
            lblValorFijo.Size = new System.Drawing.Size(180, 15);
            lblValorFijo.TabIndex = 8;
            lblValorFijo.Text = "Valor fijo (vacío si viene del CSV):";
            // 
            // txtValorFijo
            // 
            txtValorFijo.Location = new System.Drawing.Point(10, 230);
            txtValorFijo.Name = "txtValorFijo";
            txtValorFijo.Size = new System.Drawing.Size(500, 24);
            txtValorFijo.TabIndex = 9;
            // 
            // lblCondicion
            // 
            lblCondicion.AutoSize = true;
            lblCondicion.Location = new System.Drawing.Point(10, 260);
            lblCondicion.Name = "lblCondicion";
            lblCondicion.Size = new System.Drawing.Size(387, 15);
            lblCondicion.TabIndex = 10;
            lblCondicion.Text = "Condición IF  (formato: COLUMNA==VALOR|resultado_si|resultado_no):";
            // 
            // txtCondicion
            // 
            txtCondicion.Location = new System.Drawing.Point(10, 278);
            txtCondicion.Name = "txtCondicion";
            txtCondicion.NullText = "Ej: MONEDA==ARS|Pesos|Dólares";
            txtCondicion.Size = new System.Drawing.Size(500, 24);
            txtCondicion.TabIndex = 11;
            // 
            // lblRelaciones
            // 
            lblRelaciones.AutoSize = true;
            lblRelaciones.Location = new System.Drawing.Point(10, 366);
            lblRelaciones.Name = "lblRelaciones";
            lblRelaciones.Size = new System.Drawing.Size(286, 15);
            lblRelaciones.TabIndex = 12;
            lblRelaciones.Text = "Columnas CSV relacionadas (marcar las que aplican):";
            // 
            // clbRelaciones
            // 
            clbRelaciones.Location = new System.Drawing.Point(10, 384);
            clbRelaciones.Name = "clbRelaciones";
            clbRelaciones.Size = new System.Drawing.Size(500, 166);
            clbRelaciones.TabIndex = 13;
            // 
            // panelBotones
            // 
            panelBotones.BackColor = System.Drawing.Color.FromArgb(235, 238, 242);
            panelBotones.Controls.Add(btnNuevo);
            panelBotones.Controls.Add(btnGuardar);
            panelBotones.Controls.Add(btnEliminar);
            panelBotones.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelBotones.Location = new System.Drawing.Point(0, 566);
            panelBotones.Name = "panelBotones";
            panelBotones.Size = new System.Drawing.Size(539, 48);
            panelBotones.TabIndex = 1;
            // 
            // btnNuevo
            // 
            btnNuevo.Location = new System.Drawing.Point(8, 10);
            btnNuevo.Name = "btnNuevo";
            btnNuevo.Size = new System.Drawing.Size(100, 28);
            btnNuevo.TabIndex = 0;
            btnNuevo.Text = "Nuevo";
            btnNuevo.Click += btnNuevo_Click;
            // 
            // btnGuardar
            // 
            btnGuardar.Location = new System.Drawing.Point(116, 10);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new System.Drawing.Size(120, 28);
            btnGuardar.TabIndex = 1;
            btnGuardar.Text = "Guardar cambios";
            btnGuardar.Click += btnGuardar_Click;
            // 
            // btnEliminar
            // 
            btnEliminar.Location = new System.Drawing.Point(244, 10);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new System.Drawing.Size(120, 28);
            btnEliminar.TabIndex = 2;
            btnEliminar.Text = "Eliminar columna";
            btnEliminar.Click += btnEliminar_Click;
            // 
            // ch_filtro
            // 
            ch_filtro.Location = new System.Drawing.Point(182, 89);
            ch_filtro.Name = "ch_filtro";
            ch_filtro.Size = new System.Drawing.Size(125, 18);
            ch_filtro.TabIndex = 16;
            ch_filtro.Text = "Filtro de Liquidación";
            // 
            // FormColumnasDestino
            // 
            AutoScaleBaseSize = new System.Drawing.Size(7, 15);
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1100, 654);
            Controls.Add(splitMain);
            Controls.Add(panelMarca);
            Name = "FormColumnasDestino";
            Text = "Columnas Archivo Destino";
            panelMarca.ResumeLayout(false);
            panelMarca.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)btnClonar).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbMarcaFiltro).EndInit();
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridDest.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridDest).EndInit();
            grpDetalle.ResumeLayout(false);
            grpDetalle.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtCondicionSigno).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtIdColumna).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbTipoRegistro).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbTipoDato).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudOrden).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtValorFijo).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtCondicion).EndInit();
            panelBotones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)btnNuevo).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnGuardar).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnEliminar).EndInit();
            ((System.ComponentModel.ISupportInitialize)ch_filtro).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        private System.Windows.Forms.Panel panelMarca;
        private System.Windows.Forms.Label lblMarcaFiltro;
        private Telerik.WinControls.UI.RadDropDownList cmbMarcaFiltro;
        private System.Windows.Forms.SplitContainer splitMain;
        private Telerik.WinControls.UI.RadGridView gridDest;
        private System.Windows.Forms.GroupBox grpDetalle;
        private System.Windows.Forms.Label lblIdColumna;
        private Telerik.WinControls.UI.RadTextBox txtIdColumna;
        private System.Windows.Forms.Label lblTipoReg;
        private Telerik.WinControls.UI.RadDropDownList cmbTipoRegistro;
        private System.Windows.Forms.Label lblTipoDato;
        private Telerik.WinControls.UI.RadDropDownList cmbTipoDato;
        private System.Windows.Forms.Label lblOrden;
        private System.Windows.Forms.NumericUpDown nudOrden;
        private System.Windows.Forms.Label lblValorFijo;
        private Telerik.WinControls.UI.RadTextBox txtValorFijo;
        private System.Windows.Forms.Label lblCondicion;
        private Telerik.WinControls.UI.RadTextBox txtCondicion;
        private System.Windows.Forms.Label lblRelaciones;
        private System.Windows.Forms.CheckedListBox clbRelaciones;
        private System.Windows.Forms.Panel panelBotones;
        private Telerik.WinControls.UI.RadButton btnNuevo;
        private Telerik.WinControls.UI.RadButton btnGuardar;
        private Telerik.WinControls.UI.RadButton btnEliminar;
        private Telerik.WinControls.UI.RadButton btnClonar;
        private System.Windows.Forms.Label label1;
        private Telerik.WinControls.UI.RadTextBox txtCondicionSigno;
        private Telerik.WinControls.UI.RadCheckBox ch_filtro;
    }
}
