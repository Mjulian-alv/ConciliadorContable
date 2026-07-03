namespace LiquidacionesAuditar
{
    partial class FormLineasCON
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            Telerik.WinControls.UI.TableViewDefinition tableViewDefinition1 = new Telerik.WinControls.UI.TableViewDefinition();
            splitMain = new System.Windows.Forms.SplitContainer();
            grpIzq = new System.Windows.Forms.GroupBox();
            lblMarca = new System.Windows.Forms.Label();
            cmbMarca = new Telerik.WinControls.UI.RadDropDownList();
            lstLineas = new System.Windows.Forms.ListBox();
            panelBtnLineas = new System.Windows.Forms.Panel();
            btnNuevaLinea = new Telerik.WinControls.UI.RadButton();
            btnEliminarLinea = new Telerik.WinControls.UI.RadButton();
            splitDer = new System.Windows.Forms.SplitContainer();
            grpDatos = new System.Windows.Forms.GroupBox();
            lblDescripcion = new System.Windows.Forms.Label();
            txtDescripcion = new Telerik.WinControls.UI.RadTextBox();
            lblOrden = new System.Windows.Forms.Label();
            nudOrden = new System.Windows.Forms.NumericUpDown();
            lblColsCSV = new System.Windows.Forms.Label();
            clbColsCSV = new System.Windows.Forms.CheckedListBox();
            grpValores = new System.Windows.Forms.GroupBox();
            gridValoresFijos = new Telerik.WinControls.UI.RadGridView();
            panelBtnVals = new System.Windows.Forms.Panel();
            btnAgregarValorFijo = new Telerik.WinControls.UI.RadButton();
            btnQuitarValorFijo = new Telerik.WinControls.UI.RadButton();
            panelGuardar = new System.Windows.Forms.Panel();
            btnGuardarLinea = new Telerik.WinControls.UI.RadButton();
            label1 = new System.Windows.Forms.Label();
            txtCondicionSigno = new Telerik.WinControls.UI.RadTextBox();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            grpIzq.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)cmbMarca).BeginInit();
            panelBtnLineas.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)btnNuevaLinea).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnEliminarLinea).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitDer).BeginInit();
            splitDer.Panel1.SuspendLayout();
            splitDer.Panel2.SuspendLayout();
            splitDer.SuspendLayout();
            grpDatos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtDescripcion).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudOrden).BeginInit();
            grpValores.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridValoresFijos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridValoresFijos.MasterTemplate).BeginInit();
            panelBtnVals.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)btnAgregarValorFijo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnQuitarValorFijo).BeginInit();
            panelGuardar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)btnGuardarLinea).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtCondicionSigno).BeginInit();
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
            splitMain.Panel1.Controls.Add(grpIzq);
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(splitDer);
            splitMain.Panel2.Controls.Add(panelGuardar);
            splitMain.Size = new System.Drawing.Size(1060, 620);
            splitMain.SplitterDistance = 263;
            splitMain.TabIndex = 0;
            // 
            // grpIzq
            // 
            grpIzq.Controls.Add(lblMarca);
            grpIzq.Controls.Add(cmbMarca);
            grpIzq.Controls.Add(lstLineas);
            grpIzq.Controls.Add(panelBtnLineas);
            grpIzq.Dock = System.Windows.Forms.DockStyle.Fill;
            grpIzq.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            grpIzq.Location = new System.Drawing.Point(0, 0);
            grpIzq.Name = "grpIzq";
            grpIzq.Padding = new System.Windows.Forms.Padding(6);
            grpIzq.Size = new System.Drawing.Size(263, 620);
            grpIzq.TabIndex = 0;
            grpIzq.TabStop = false;
            grpIzq.Text = "Líneas CON";
            // 
            // lblMarca
            // 
            lblMarca.AutoSize = true;
            lblMarca.Location = new System.Drawing.Point(8, 20);
            lblMarca.Name = "lblMarca";
            lblMarca.Size = new System.Drawing.Size(44, 15);
            lblMarca.TabIndex = 0;
            lblMarca.Text = "Marca:";
            // 
            // cmbMarca
            // 
            cmbMarca.Location = new System.Drawing.Point(8, 38);
            cmbMarca.Name = "cmbMarca";
            cmbMarca.Size = new System.Drawing.Size(210, 24);
            cmbMarca.TabIndex = 1;
            cmbMarca.SelectedIndexChanged += cmbMarca_SelectedIndexChanged;
            // 
            // lstLineas
            // 
            lstLineas.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            lstLineas.ItemHeight = 17;
            lstLineas.Location = new System.Drawing.Point(8, 70);
            lstLineas.Name = "lstLineas";
            lstLineas.Size = new System.Drawing.Size(220, 412);
            lstLineas.TabIndex = 2;
            lstLineas.SelectedIndexChanged += lstLineas_SelectedIndexChanged;
            // 
            // panelBtnLineas
            // 
            panelBtnLineas.Controls.Add(btnNuevaLinea);
            panelBtnLineas.Controls.Add(btnEliminarLinea);
            panelBtnLineas.Location = new System.Drawing.Point(8, 504);
            panelBtnLineas.Name = "panelBtnLineas";
            panelBtnLineas.Size = new System.Drawing.Size(220, 36);
            panelBtnLineas.TabIndex = 3;
            // 
            // btnNuevaLinea
            // 
            btnNuevaLinea.Location = new System.Drawing.Point(0, 4);
            btnNuevaLinea.Name = "btnNuevaLinea";
            btnNuevaLinea.Size = new System.Drawing.Size(106, 28);
            btnNuevaLinea.TabIndex = 0;
            btnNuevaLinea.Text = "+ Nueva";
            btnNuevaLinea.Click += btnNuevaLinea_Click;
            // 
            // btnEliminarLinea
            // 
            btnEliminarLinea.Location = new System.Drawing.Point(112, 4);
            btnEliminarLinea.Name = "btnEliminarLinea";
            btnEliminarLinea.Size = new System.Drawing.Size(106, 28);
            btnEliminarLinea.TabIndex = 1;
            btnEliminarLinea.Text = "✕ Eliminar";
            btnEliminarLinea.Click += btnEliminarLinea_Click;
            // 
            // splitDer
            // 
            splitDer.Dock = System.Windows.Forms.DockStyle.Fill;
            splitDer.Location = new System.Drawing.Point(0, 0);
            splitDer.Name = "splitDer";
            // 
            // splitDer.Panel1
            // 
            splitDer.Panel1.Controls.Add(grpDatos);
            // 
            // splitDer.Panel2
            // 
            splitDer.Panel2.Controls.Add(grpValores);
            splitDer.Size = new System.Drawing.Size(793, 578);
            splitDer.SplitterDistance = 419;
            splitDer.TabIndex = 0;
            // 
            // grpDatos
            // 
            grpDatos.Controls.Add(label1);
            grpDatos.Controls.Add(txtCondicionSigno);
            grpDatos.Controls.Add(lblDescripcion);
            grpDatos.Controls.Add(txtDescripcion);
            grpDatos.Controls.Add(lblOrden);
            grpDatos.Controls.Add(nudOrden);
            grpDatos.Controls.Add(lblColsCSV);
            grpDatos.Controls.Add(clbColsCSV);
            grpDatos.Dock = System.Windows.Forms.DockStyle.Fill;
            grpDatos.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            grpDatos.Location = new System.Drawing.Point(0, 0);
            grpDatos.Name = "grpDatos";
            grpDatos.Padding = new System.Windows.Forms.Padding(6);
            grpDatos.Size = new System.Drawing.Size(419, 578);
            grpDatos.TabIndex = 0;
            grpDatos.TabStop = false;
            grpDatos.Text = "Datos de la línea CON";
            // 
            // lblDescripcion
            // 
            lblDescripcion.AutoSize = true;
            lblDescripcion.Location = new System.Drawing.Point(8, 20);
            lblDescripcion.Name = "lblDescripcion";
            lblDescripcion.Size = new System.Drawing.Size(132, 15);
            lblDescripcion.TabIndex = 0;
            lblDescripcion.Text = "Descripción / Etiqueta:";
            // 
            // txtDescripcion
            // 
            txtDescripcion.Location = new System.Drawing.Point(8, 38);
            txtDescripcion.Name = "txtDescripcion";
            txtDescripcion.NullText = "Ej: Total Bruto, Total Retenciones...";
            txtDescripcion.Size = new System.Drawing.Size(360, 24);
            txtDescripcion.TabIndex = 1;
            // 
            // lblOrden
            // 
            lblOrden.AutoSize = true;
            lblOrden.Location = new System.Drawing.Point(8, 70);
            lblOrden.Name = "lblOrden";
            lblOrden.Size = new System.Drawing.Size(115, 15);
            lblOrden.TabIndex = 2;
            lblOrden.Text = "Orden de aparición:";
            // 
            // nudOrden
            // 
            nudOrden.Location = new System.Drawing.Point(8, 88);
            nudOrden.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            nudOrden.Name = "nudOrden";
            nudOrden.Size = new System.Drawing.Size(80, 23);
            nudOrden.TabIndex = 3;
            // 
            // lblColsCSV
            // 
            lblColsCSV.AutoSize = true;
            lblColsCSV.Location = new System.Drawing.Point(8, 120);
            lblColsCSV.Name = "lblColsCSV";
            lblColsCSV.Size = new System.Drawing.Size(238, 15);
            lblColsCSV.TabIndex = 4;
            lblColsCSV.Text = "Columnas CSV que se suman en esta línea:";
            // 
            // clbColsCSV
            // 
            clbColsCSV.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            clbColsCSV.Location = new System.Drawing.Point(8, 138);
            clbColsCSV.Name = "clbColsCSV";
            clbColsCSV.Size = new System.Drawing.Size(360, 327);
            clbColsCSV.TabIndex = 5;
            // 
            // grpValores
            // 
            grpValores.Controls.Add(gridValoresFijos);
            grpValores.Controls.Add(panelBtnVals);
            grpValores.Dock = System.Windows.Forms.DockStyle.Fill;
            grpValores.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            grpValores.Location = new System.Drawing.Point(0, 0);
            grpValores.Name = "grpValores";
            grpValores.Padding = new System.Windows.Forms.Padding(6);
            grpValores.Size = new System.Drawing.Size(370, 578);
            grpValores.TabIndex = 0;
            grpValores.TabStop = false;
            grpValores.Text = "Valores fijos en el TXT de salida";
            // 
            // gridValoresFijos
            // 
            gridValoresFijos.Dock = System.Windows.Forms.DockStyle.Fill;
            gridValoresFijos.Location = new System.Drawing.Point(6, 22);
            // 
            // 
            // 
            gridValoresFijos.MasterTemplate.ViewDefinition = tableViewDefinition1;
            gridValoresFijos.Name = "gridValoresFijos";
            gridValoresFijos.Size = new System.Drawing.Size(358, 514);
            gridValoresFijos.TabIndex = 0;
            // 
            // panelBtnVals
            // 
            panelBtnVals.Controls.Add(btnAgregarValorFijo);
            panelBtnVals.Controls.Add(btnQuitarValorFijo);
            panelBtnVals.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelBtnVals.Location = new System.Drawing.Point(6, 536);
            panelBtnVals.Name = "panelBtnVals";
            panelBtnVals.Size = new System.Drawing.Size(358, 36);
            panelBtnVals.TabIndex = 1;
            // 
            // btnAgregarValorFijo
            // 
            btnAgregarValorFijo.Location = new System.Drawing.Point(6, 4);
            btnAgregarValorFijo.Name = "btnAgregarValorFijo";
            btnAgregarValorFijo.Size = new System.Drawing.Size(120, 28);
            btnAgregarValorFijo.TabIndex = 0;
            btnAgregarValorFijo.Text = "Agregar fila";
            btnAgregarValorFijo.Click += btnAgregarValorFijo_Click;
            // 
            // btnQuitarValorFijo
            // 
            btnQuitarValorFijo.Location = new System.Drawing.Point(132, 4);
            btnQuitarValorFijo.Name = "btnQuitarValorFijo";
            btnQuitarValorFijo.Size = new System.Drawing.Size(120, 28);
            btnQuitarValorFijo.TabIndex = 1;
            btnQuitarValorFijo.Text = "Quitar fila";
            btnQuitarValorFijo.Click += btnQuitarValorFijo_Click;
            // 
            // panelGuardar
            // 
            panelGuardar.BackColor = System.Drawing.Color.FromArgb(224, 229, 236);
            panelGuardar.Controls.Add(btnGuardarLinea);
            panelGuardar.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelGuardar.Location = new System.Drawing.Point(0, 578);
            panelGuardar.Name = "panelGuardar";
            panelGuardar.Padding = new System.Windows.Forms.Padding(6, 5, 6, 5);
            panelGuardar.Size = new System.Drawing.Size(793, 42);
            panelGuardar.TabIndex = 1;
            // 
            // btnGuardarLinea
            // 
            btnGuardarLinea.Dock = System.Windows.Forms.DockStyle.Right;
            btnGuardarLinea.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnGuardarLinea.Location = new System.Drawing.Point(587, 5);
            btnGuardarLinea.Name = "btnGuardarLinea";
            btnGuardarLinea.Size = new System.Drawing.Size(200, 32);
            btnGuardarLinea.TabIndex = 0;
            btnGuardarLinea.Text = "Guardar línea CON";
            btnGuardarLinea.Click += btnGuardarLinea_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(8, 486);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(183, 15);
            label1.TabIndex = 16;
            label1.Text = "Condición SIGNO para Negativo";
            // 
            // txtCondicionSigno
            // 
            txtCondicionSigno.Location = new System.Drawing.Point(8, 504);
            txtCondicionSigno.Name = "txtCondicionSigno";
            txtCondicionSigno.NullText = "Ej: SIGNO M.B==-|-|+";
            txtCondicionSigno.Size = new System.Drawing.Size(391, 24);
            txtCondicionSigno.TabIndex = 17;
            // 
            // FormLineasCON
            // 
            AutoScaleBaseSize = new System.Drawing.Size(7, 15);
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1060, 620);
            Controls.Add(splitMain);
            MinimumSize = new System.Drawing.Size(900, 560);
            Name = "FormLineasCON";
            Text = "Líneas Totalizadoras (CON)";
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            grpIzq.ResumeLayout(false);
            grpIzq.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)cmbMarca).EndInit();
            panelBtnLineas.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)btnNuevaLinea).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnEliminarLinea).EndInit();
            splitDer.Panel1.ResumeLayout(false);
            splitDer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitDer).EndInit();
            splitDer.ResumeLayout(false);
            grpDatos.ResumeLayout(false);
            grpDatos.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtDescripcion).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudOrden).EndInit();
            grpValores.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridValoresFijos.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridValoresFijos).EndInit();
            panelBtnVals.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)btnAgregarValorFijo).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnQuitarValorFijo).EndInit();
            panelGuardar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)btnGuardarLinea).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtCondicionSigno).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.GroupBox grpIzq;
        private System.Windows.Forms.Label lblMarca;
        private Telerik.WinControls.UI.RadDropDownList cmbMarca;
        private System.Windows.Forms.ListBox lstLineas;
        private System.Windows.Forms.Panel panelBtnLineas;
        private Telerik.WinControls.UI.RadButton btnNuevaLinea;
        private Telerik.WinControls.UI.RadButton btnEliminarLinea;
        private System.Windows.Forms.SplitContainer splitDer;
        private System.Windows.Forms.GroupBox grpDatos;
        private System.Windows.Forms.Label lblDescripcion;
        private Telerik.WinControls.UI.RadTextBox txtDescripcion;
        private System.Windows.Forms.Label lblOrden;
        private System.Windows.Forms.NumericUpDown nudOrden;
        private System.Windows.Forms.Label lblColsCSV;
        private System.Windows.Forms.CheckedListBox clbColsCSV;
        private System.Windows.Forms.GroupBox grpValores;
        private Telerik.WinControls.UI.RadGridView gridValoresFijos;
        private System.Windows.Forms.Panel panelBtnVals;
        private Telerik.WinControls.UI.RadButton btnAgregarValorFijo;
        private Telerik.WinControls.UI.RadButton btnQuitarValorFijo;
        private System.Windows.Forms.Panel panelGuardar;
        private Telerik.WinControls.UI.RadButton btnGuardarLinea;
        private System.Windows.Forms.Label label1;
        private Telerik.WinControls.UI.RadTextBox txtCondicionSigno;
    }
}
