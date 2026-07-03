namespace LiquidacionesAuditar
{
    partial class FormColumnasCSV
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            Telerik.WinControls.UI.RadListDataItem radListDataItem6 = new Telerik.WinControls.UI.RadListDataItem();
            Telerik.WinControls.UI.RadListDataItem radListDataItem7 = new Telerik.WinControls.UI.RadListDataItem();
            Telerik.WinControls.UI.RadListDataItem radListDataItem8 = new Telerik.WinControls.UI.RadListDataItem();
            Telerik.WinControls.UI.RadListDataItem radListDataItem9 = new Telerik.WinControls.UI.RadListDataItem();
            Telerik.WinControls.UI.RadListDataItem radListDataItem10 = new Telerik.WinControls.UI.RadListDataItem();
            Telerik.WinControls.UI.TableViewDefinition tableViewDefinition2 = new Telerik.WinControls.UI.TableViewDefinition();
            splitMain = new System.Windows.Forms.SplitContainer();
            grpMarca = new System.Windows.Forms.GroupBox();
            radGroupBox1 = new Telerik.WinControls.UI.RadGroupBox();
            cmbTipoDato = new Telerik.WinControls.UI.RadDropDownList();
            label1 = new System.Windows.Forms.Label();
            lblTipoDato = new System.Windows.Forms.Label();
            cb_Columna = new Telerik.WinControls.UI.RadDropDownList();
            lblSepDec = new System.Windows.Forms.Label();
            txtSepDec = new Telerik.WinControls.UI.RadTextBox();
            lblSepMil = new System.Windows.Forms.Label();
            txtSepMil = new Telerik.WinControls.UI.RadTextBox();
            lstMarcas = new System.Windows.Forms.ListBox();
            lblMarcaId = new System.Windows.Forms.Label();
            txtMarcaId = new Telerik.WinControls.UI.RadTextBox();
            lblMarcaNombre = new System.Windows.Forms.Label();
            txtMarcaNombre = new Telerik.WinControls.UI.RadTextBox();
            btnNuevaMarca = new Telerik.WinControls.UI.RadButton();
            btnGuardarMarca = new Telerik.WinControls.UI.RadButton();
            btnEliminarMarca = new Telerik.WinControls.UI.RadButton();
            lblFilaEncabezado = new System.Windows.Forms.Label();
            nudFilaEncabezado = new System.Windows.Forms.NumericUpDown();
            lblColumnaLiquidacion = new System.Windows.Forms.Label();
            txtColumnaLiquidacion = new Telerik.WinControls.UI.RadTextBox();
            grpColumnas = new System.Windows.Forms.GroupBox();
            gridColumnas = new Telerik.WinControls.UI.RadGridView();
            panelColBotones = new System.Windows.Forms.Panel();
            txtNuevaColumna = new Telerik.WinControls.UI.RadTextBox();
            btnAgregarColumna = new Telerik.WinControls.UI.RadButton();
            btnEliminarColumna = new Telerik.WinControls.UI.RadButton();
            btnImportarDesdeCSV = new Telerik.WinControls.UI.RadButton();
            btnValidarCSV = new Telerik.WinControls.UI.RadButton();
            btnImportarDesdeExcel = new Telerik.WinControls.UI.RadButton();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            grpMarca.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)radGroupBox1).BeginInit();
            radGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)cmbTipoDato).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cb_Columna).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtSepDec).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtSepMil).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtMarcaId).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtMarcaNombre).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnNuevaMarca).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnGuardarMarca).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnEliminarMarca).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudFilaEncabezado).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtColumnaLiquidacion).BeginInit();
            grpColumnas.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridColumnas).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridColumnas.MasterTemplate).BeginInit();
            panelColBotones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtNuevaColumna).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnAgregarColumna).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnEliminarColumna).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnImportarDesdeCSV).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnValidarCSV).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnImportarDesdeExcel).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            SuspendLayout();
            // 
            // splitMain
            // 
            splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitMain.Location = new System.Drawing.Point(0, 0);
            splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(grpMarca);
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(grpColumnas);
            splitMain.Size = new System.Drawing.Size(1065, 599);
            splitMain.SplitterDistance = 280;
            splitMain.TabIndex = 0;
            // 
            // grpMarca
            // 
            grpMarca.Controls.Add(radGroupBox1);
            grpMarca.Controls.Add(lblSepDec);
            grpMarca.Controls.Add(txtSepDec);
            grpMarca.Controls.Add(lblSepMil);
            grpMarca.Controls.Add(txtSepMil);
            grpMarca.Controls.Add(lstMarcas);
            grpMarca.Controls.Add(lblMarcaId);
            grpMarca.Controls.Add(txtMarcaId);
            grpMarca.Controls.Add(lblMarcaNombre);
            grpMarca.Controls.Add(txtMarcaNombre);
            grpMarca.Controls.Add(btnNuevaMarca);
            grpMarca.Controls.Add(btnGuardarMarca);
            grpMarca.Controls.Add(btnEliminarMarca);
            grpMarca.Controls.Add(lblFilaEncabezado);
            grpMarca.Controls.Add(nudFilaEncabezado);
            grpMarca.Controls.Add(lblColumnaLiquidacion);
            grpMarca.Controls.Add(txtColumnaLiquidacion);
            grpMarca.Dock = System.Windows.Forms.DockStyle.Fill;
            grpMarca.Location = new System.Drawing.Point(0, 0);
            grpMarca.Name = "grpMarca";
            grpMarca.Padding = new System.Windows.Forms.Padding(6);
            grpMarca.Size = new System.Drawing.Size(280, 599);
            grpMarca.TabIndex = 0;
            grpMarca.TabStop = false;
            grpMarca.Text = "Marcas / Procesadores";
            // 
            // radGroupBox1
            // 
            radGroupBox1.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
            radGroupBox1.Controls.Add(cmbTipoDato);
            radGroupBox1.Controls.Add(label1);
            radGroupBox1.Controls.Add(lblTipoDato);
            radGroupBox1.Controls.Add(cb_Columna);
            radGroupBox1.HeaderMargin = new System.Windows.Forms.Padding(1);
            radGroupBox1.HeaderText = "Filtro de Liquidacion";
            radGroupBox1.Location = new System.Drawing.Point(12, 479);
            radGroupBox1.Name = "radGroupBox1";
            radGroupBox1.Size = new System.Drawing.Size(259, 100);
            radGroupBox1.TabIndex = 20;
            radGroupBox1.Text = "Filtro de Liquidacion";
            // 
            // cmbTipoDato
            // 
            radListDataItem6.Text = "STRING";
            radListDataItem7.Text = "INT";
            radListDataItem8.Text = "DECIMAL";
            radListDataItem9.Text = "DATE";
            radListDataItem10.Text = "NVARCHAR";
            cmbTipoDato.Items.Add(radListDataItem6);
            cmbTipoDato.Items.Add(radListDataItem7);
            cmbTipoDato.Items.Add(radListDataItem8);
            cmbTipoDato.Items.Add(radListDataItem9);
            cmbTipoDato.Items.Add(radListDataItem10);
            cmbTipoDato.Location = new System.Drawing.Point(84, 21);
            cmbTipoDato.Name = "cmbTipoDato";
            cmbTipoDato.Size = new System.Drawing.Size(120, 20);
            cmbTipoDato.TabIndex = 16;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(2, 69);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(56, 15);
            label1.TabIndex = 19;
            label1.Text = "Columna";
            // 
            // lblTipoDato
            // 
            lblTipoDato.AutoSize = true;
            lblTipoDato.Location = new System.Drawing.Point(2, 26);
            lblTipoDato.Name = "lblTipoDato";
            lblTipoDato.Size = new System.Drawing.Size(62, 15);
            lblTipoDato.TabIndex = 17;
            lblTipoDato.Text = "Tipo Dato:";
            // 
            // cb_Columna
            // 
            cb_Columna.Location = new System.Drawing.Point(84, 65);
            cb_Columna.Name = "cb_Columna";
            cb_Columna.Size = new System.Drawing.Size(175, 20);
            cb_Columna.TabIndex = 18;
            // 
            // lblSepDec
            // 
            lblSepDec.AutoSize = true;
            lblSepDec.Location = new System.Drawing.Point(6, 444);
            lblSepDec.Name = "lblSepDec";
            lblSepDec.Size = new System.Drawing.Size(77, 15);
            lblSepDec.TabIndex = 12;
            lblSepDec.Text = "Sep. decimal:";
            // 
            // txtSepDec
            // 
            txtSepDec.Font = new System.Drawing.Font("Courier New", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            txtSepDec.Location = new System.Drawing.Point(96, 442);
            txtSepDec.MaxLength = 1;
            txtSepDec.Name = "txtSepDec";
            txtSepDec.NullText = ",";
            txtSepDec.Size = new System.Drawing.Size(23, 27);
            txtSepDec.TabIndex = 13;
            txtSepDec.Text = ",";
            // 
            // lblSepMil
            // 
            lblSepMil.AutoSize = true;
            lblSepMil.Location = new System.Drawing.Point(142, 444);
            lblSepMil.Name = "lblSepMil";
            lblSepMil.Size = new System.Drawing.Size(63, 15);
            lblSepMil.TabIndex = 14;
            lblSepMil.Text = "Sep. miles:";
            // 
            // txtSepMil
            // 
            txtSepMil.Font = new System.Drawing.Font("Courier New", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            txtSepMil.Location = new System.Drawing.Point(222, 442);
            txtSepMil.MaxLength = 1;
            txtSepMil.Name = "txtSepMil";
            txtSepMil.NullText = ".";
            txtSepMil.Size = new System.Drawing.Size(24, 27);
            txtSepMil.TabIndex = 15;
            txtSepMil.Text = ".";
            // 
            // lstMarcas
            // 
            lstMarcas.Dock = System.Windows.Forms.DockStyle.Top;
            lstMarcas.ItemHeight = 15;
            lstMarcas.Location = new System.Drawing.Point(6, 22);
            lstMarcas.Name = "lstMarcas";
            lstMarcas.Size = new System.Drawing.Size(268, 169);
            lstMarcas.TabIndex = 0;
            lstMarcas.SelectedIndexChanged += lstMarcas_SelectedIndexChanged;
            // 
            // lblMarcaId
            // 
            lblMarcaId.AutoSize = true;
            lblMarcaId.Location = new System.Drawing.Point(6, 195);
            lblMarcaId.Name = "lblMarcaId";
            lblMarcaId.Size = new System.Drawing.Size(21, 15);
            lblMarcaId.TabIndex = 1;
            lblMarcaId.Text = "ID:";
            // 
            // txtMarcaId
            // 
            txtMarcaId.Location = new System.Drawing.Point(6, 212);
            txtMarcaId.Name = "txtMarcaId";
            txtMarcaId.Size = new System.Drawing.Size(230, 20);
            txtMarcaId.TabIndex = 2;
            // 
            // lblMarcaNombre
            // 
            lblMarcaNombre.AutoSize = true;
            lblMarcaNombre.Location = new System.Drawing.Point(6, 242);
            lblMarcaNombre.Name = "lblMarcaNombre";
            lblMarcaNombre.Size = new System.Drawing.Size(54, 15);
            lblMarcaNombre.TabIndex = 3;
            lblMarcaNombre.Text = "Nombre:";
            // 
            // txtMarcaNombre
            // 
            txtMarcaNombre.Location = new System.Drawing.Point(6, 259);
            txtMarcaNombre.Name = "txtMarcaNombre";
            txtMarcaNombre.Size = new System.Drawing.Size(230, 20);
            txtMarcaNombre.TabIndex = 4;
            // 
            // btnNuevaMarca
            // 
            btnNuevaMarca.Location = new System.Drawing.Point(6, 295);
            btnNuevaMarca.Name = "btnNuevaMarca";
            btnNuevaMarca.Size = new System.Drawing.Size(70, 28);
            btnNuevaMarca.TabIndex = 5;
            btnNuevaMarca.Text = "Nuevo";
            btnNuevaMarca.Click += btnNuevaMarca_Click;
            // 
            // btnGuardarMarca
            // 
            btnGuardarMarca.Location = new System.Drawing.Point(82, 295);
            btnGuardarMarca.Name = "btnGuardarMarca";
            btnGuardarMarca.Size = new System.Drawing.Size(70, 28);
            btnGuardarMarca.TabIndex = 6;
            btnGuardarMarca.Text = "Guardar";
            btnGuardarMarca.Click += btnGuardarMarca_Click;
            // 
            // btnEliminarMarca
            // 
            btnEliminarMarca.Location = new System.Drawing.Point(158, 295);
            btnEliminarMarca.Name = "btnEliminarMarca";
            btnEliminarMarca.Size = new System.Drawing.Size(78, 28);
            btnEliminarMarca.TabIndex = 7;
            btnEliminarMarca.Text = "Eliminar";
            btnEliminarMarca.Click += btnEliminarMarca_Click;
            // 
            // lblFilaEncabezado
            // 
            lblFilaEncabezado.AutoSize = true;
            lblFilaEncabezado.Location = new System.Drawing.Point(6, 335);
            lblFilaEncabezado.Name = "lblFilaEncabezado";
            lblFilaEncabezado.Size = new System.Drawing.Size(131, 15);
            lblFilaEncabezado.TabIndex = 8;
            lblFilaEncabezado.Text = "Fila encabezado (Excel):";
            // 
            // nudFilaEncabezado
            // 
            nudFilaEncabezado.Location = new System.Drawing.Point(6, 353);
            nudFilaEncabezado.Maximum = new decimal(new int[] { 999, 0, 0, 0 });
            nudFilaEncabezado.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudFilaEncabezado.Name = "nudFilaEncabezado";
            nudFilaEncabezado.Size = new System.Drawing.Size(70, 23);
            nudFilaEncabezado.TabIndex = 9;
            nudFilaEncabezado.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblColumnaLiquidacion
            // 
            lblColumnaLiquidacion.AutoSize = true;
            lblColumnaLiquidacion.Location = new System.Drawing.Point(6, 386);
            lblColumnaLiquidacion.Name = "lblColumnaLiquidacion";
            lblColumnaLiquidacion.Size = new System.Drawing.Size(150, 15);
            lblColumnaLiquidacion.TabIndex = 10;
            lblColumnaLiquidacion.Text = "Columna Nro. Liquidación:";
            // 
            // txtColumnaLiquidacion
            // 
            txtColumnaLiquidacion.Location = new System.Drawing.Point(6, 404);
            txtColumnaLiquidacion.Name = "txtColumnaLiquidacion";
            txtColumnaLiquidacion.NullText = "(vacío = agrupar por fecha)";
            txtColumnaLiquidacion.Size = new System.Drawing.Size(230, 20);
            txtColumnaLiquidacion.TabIndex = 11;
            // 
            // grpColumnas
            // 
            grpColumnas.Controls.Add(gridColumnas);
            grpColumnas.Controls.Add(panelColBotones);
            grpColumnas.Dock = System.Windows.Forms.DockStyle.Fill;
            grpColumnas.Location = new System.Drawing.Point(0, 0);
            grpColumnas.Name = "grpColumnas";
            grpColumnas.Padding = new System.Windows.Forms.Padding(6);
            grpColumnas.Size = new System.Drawing.Size(781, 599);
            grpColumnas.TabIndex = 0;
            grpColumnas.TabStop = false;
            grpColumnas.Text = "Columnas CSV";
            // 
            // gridColumnas
            // 
            gridColumnas.Dock = System.Windows.Forms.DockStyle.Fill;
            gridColumnas.Location = new System.Drawing.Point(6, 22);
            // 
            // 
            // 
            gridColumnas.MasterTemplate.ViewDefinition = tableViewDefinition2;
            gridColumnas.Name = "gridColumnas";
            gridColumnas.Size = new System.Drawing.Size(769, 501);
            gridColumnas.TabIndex = 0;
            gridColumnas.CellEndEdit += gridColumnas_CellEndEdit;
            // 
            // panelColBotones
            // 
            panelColBotones.Controls.Add(txtNuevaColumna);
            panelColBotones.Controls.Add(btnAgregarColumna);
            panelColBotones.Controls.Add(btnEliminarColumna);
            panelColBotones.Controls.Add(btnImportarDesdeCSV);
            panelColBotones.Controls.Add(btnValidarCSV);
            panelColBotones.Controls.Add(btnImportarDesdeExcel);
            panelColBotones.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelColBotones.Location = new System.Drawing.Point(6, 523);
            panelColBotones.Name = "panelColBotones";
            panelColBotones.Size = new System.Drawing.Size(769, 70);
            panelColBotones.TabIndex = 1;
            // 
            // txtNuevaColumna
            // 
            txtNuevaColumna.Location = new System.Drawing.Point(6, 8);
            txtNuevaColumna.Name = "txtNuevaColumna";
            txtNuevaColumna.NullText = "Nombre de columna...";
            txtNuevaColumna.Size = new System.Drawing.Size(360, 20);
            txtNuevaColumna.TabIndex = 0;
            // 
            // btnAgregarColumna
            // 
            btnAgregarColumna.Location = new System.Drawing.Point(374, 6);
            btnAgregarColumna.Name = "btnAgregarColumna";
            btnAgregarColumna.Size = new System.Drawing.Size(90, 28);
            btnAgregarColumna.TabIndex = 1;
            btnAgregarColumna.Text = "Agregar";
            btnAgregarColumna.Click += btnAgregarColumna_Click;
            // 
            // btnEliminarColumna
            // 
            btnEliminarColumna.Location = new System.Drawing.Point(470, 6);
            btnEliminarColumna.Name = "btnEliminarColumna";
            btnEliminarColumna.Size = new System.Drawing.Size(90, 28);
            btnEliminarColumna.TabIndex = 2;
            btnEliminarColumna.Text = "Eliminar";
            btnEliminarColumna.Click += btnEliminarColumna_Click;
            // 
            // btnImportarDesdeCSV
            // 
            btnImportarDesdeCSV.Location = new System.Drawing.Point(6, 38);
            btnImportarDesdeCSV.Name = "btnImportarDesdeCSV";
            btnImportarDesdeCSV.Size = new System.Drawing.Size(170, 28);
            btnImportarDesdeCSV.TabIndex = 3;
            btnImportarDesdeCSV.Text = "Importar desde CSV";
            btnImportarDesdeCSV.Click += btnImportarDesdeCSV_Click;
            // 
            // btnValidarCSV
            // 
            btnValidarCSV.Location = new System.Drawing.Point(182, 38);
            btnValidarCSV.Name = "btnValidarCSV";
            btnValidarCSV.Size = new System.Drawing.Size(170, 28);
            btnValidarCSV.TabIndex = 4;
            btnValidarCSV.Text = "Validar contra CSV";
            btnValidarCSV.Click += btnValidarCSV_Click;
            // 
            // btnImportarDesdeExcel
            // 
            btnImportarDesdeExcel.Location = new System.Drawing.Point(358, 38);
            btnImportarDesdeExcel.Name = "btnImportarDesdeExcel";
            btnImportarDesdeExcel.Size = new System.Drawing.Size(170, 28);
            btnImportarDesdeExcel.TabIndex = 5;
            btnImportarDesdeExcel.Text = "Importar desde Excel";
            btnImportarDesdeExcel.Click += btnImportarDesdeExcel_Click;
            // 
            // FormColumnasCSV
            // 
            AutoScaleBaseSize = new System.Drawing.Size(7, 15);
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1065, 599);
            Controls.Add(splitMain);
            Name = "FormColumnasCSV";
            Text = "Columnas CSV (Origen)";
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            grpMarca.ResumeLayout(false);
            grpMarca.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)radGroupBox1).EndInit();
            radGroupBox1.ResumeLayout(false);
            radGroupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)cmbTipoDato).EndInit();
            ((System.ComponentModel.ISupportInitialize)cb_Columna).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtSepDec).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtSepMil).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtMarcaId).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtMarcaNombre).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnNuevaMarca).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnGuardarMarca).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnEliminarMarca).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudFilaEncabezado).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtColumnaLiquidacion).EndInit();
            grpColumnas.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridColumnas.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridColumnas).EndInit();
            panelColBotones.ResumeLayout(false);
            panelColBotones.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtNuevaColumna).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnAgregarColumna).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnEliminarColumna).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnImportarDesdeCSV).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnValidarCSV).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnImportarDesdeExcel).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.GroupBox grpMarca;
        private System.Windows.Forms.ListBox lstMarcas;
        private System.Windows.Forms.Label lblMarcaId;
        private Telerik.WinControls.UI.RadTextBox txtMarcaId;
        private System.Windows.Forms.Label lblMarcaNombre;
        private Telerik.WinControls.UI.RadTextBox txtMarcaNombre;
        private Telerik.WinControls.UI.RadButton btnNuevaMarca;
        private Telerik.WinControls.UI.RadButton btnGuardarMarca;
        private Telerik.WinControls.UI.RadButton btnEliminarMarca;
        private System.Windows.Forms.GroupBox grpColumnas;
        private Telerik.WinControls.UI.RadGridView gridColumnas;
        private System.Windows.Forms.Panel panelColBotones;
        private Telerik.WinControls.UI.RadTextBox txtNuevaColumna;
        private Telerik.WinControls.UI.RadButton btnAgregarColumna;
        private Telerik.WinControls.UI.RadButton btnEliminarColumna;
        private Telerik.WinControls.UI.RadButton btnImportarDesdeCSV;
        private Telerik.WinControls.UI.RadButton btnValidarCSV;
        private Telerik.WinControls.UI.RadButton btnImportarDesdeExcel;
        private System.Windows.Forms.Label lblFilaEncabezado;
        private System.Windows.Forms.NumericUpDown nudFilaEncabezado;
        private System.Windows.Forms.Label lblColumnaLiquidacion;
        private Telerik.WinControls.UI.RadTextBox txtColumnaLiquidacion;
        private System.Windows.Forms.Label lblSepDec;
        private Telerik.WinControls.UI.RadTextBox txtSepDec;
        private System.Windows.Forms.Label lblSepMil;
        private Telerik.WinControls.UI.RadTextBox txtSepMil;
        private Telerik.WinControls.UI.RadDropDownList cmbTipoDato;
        private System.Windows.Forms.Label label1;
        private Telerik.WinControls.UI.RadDropDownList cb_Columna;
        private System.Windows.Forms.Label lblTipoDato;
        private Telerik.WinControls.UI.RadGroupBox radGroupBox1;
    }
}
