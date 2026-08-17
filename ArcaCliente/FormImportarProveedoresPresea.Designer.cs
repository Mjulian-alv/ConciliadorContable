namespace ArcaCliente
{
    partial class FormImportarProveedoresPresea
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
            grpArchivo = new System.Windows.Forms.GroupBox();
            lblArchivo = new Telerik.WinControls.UI.RadLabel();
            txtArchivo = new Telerik.WinControls.UI.RadTextBox();
            btnExaminar = new Telerik.WinControls.UI.RadButton();
            btnLeer = new Telerik.WinControls.UI.RadButton();
            lblSeparador = new Telerik.WinControls.UI.RadLabel();
            cmbSeparador = new Telerik.WinControls.UI.RadDropDownList();
            lblEncoding = new Telerik.WinControls.UI.RadLabel();
            cmbEncoding = new Telerik.WinControls.UI.RadDropDownList();
            lblFilaEncab = new Telerik.WinControls.UI.RadLabel();
            spnFila = new Telerik.WinControls.UI.RadSpinEditor();
            lblSepDecimal = new Telerik.WinControls.UI.RadLabel();
            cmbSepDec = new Telerik.WinControls.UI.RadDropDownList();
            lblHojaExcel = new Telerik.WinControls.UI.RadLabel();
            txtHoja = new Telerik.WinControls.UI.RadTextBox();
            grpMapeo = new System.Windows.Forms.GroupBox();
            grpPreview = new System.Windows.Forms.GroupBox();
            gridPreview = new Telerik.WinControls.UI.RadGridView();
            pnl = new Telerik.WinControls.UI.RadPanel();
            lblEstado = new Telerik.WinControls.UI.RadLabel();
            btnPrevisualizar = new Telerik.WinControls.UI.RadButton();
            btnImportar = new Telerik.WinControls.UI.RadButton();
            btnCerrar = new Telerik.WinControls.UI.RadButton();
            grpArchivo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lblArchivo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtArchivo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnExaminar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnLeer).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblSeparador).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbSeparador).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblEncoding).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbEncoding).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblFilaEncab).BeginInit();
            ((System.ComponentModel.ISupportInitialize)spnFila).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblSepDecimal).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbSepDec).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblHojaExcel).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtHoja).BeginInit();
            grpMapeo.SuspendLayout();
            grpPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridPreview).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridPreview.MasterTemplate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnl).BeginInit();
            pnl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lblEstado).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnPrevisualizar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnImportar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnCerrar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            SuspendLayout();
            //
            // grpArchivo
            //
            grpArchivo.Controls.Add(lblArchivo);
            grpArchivo.Controls.Add(txtArchivo);
            grpArchivo.Controls.Add(btnExaminar);
            grpArchivo.Controls.Add(btnLeer);
            grpArchivo.Controls.Add(lblSeparador);
            grpArchivo.Controls.Add(cmbSeparador);
            grpArchivo.Controls.Add(lblEncoding);
            grpArchivo.Controls.Add(cmbEncoding);
            grpArchivo.Controls.Add(lblFilaEncab);
            grpArchivo.Controls.Add(spnFila);
            grpArchivo.Controls.Add(lblSepDecimal);
            grpArchivo.Controls.Add(cmbSepDec);
            grpArchivo.Controls.Add(lblHojaExcel);
            grpArchivo.Controls.Add(txtHoja);
            grpArchivo.Location = new System.Drawing.Point(12, 8);
            grpArchivo.Name = "grpArchivo";
            grpArchivo.Size = new System.Drawing.Size(736, 118);
            grpArchivo.TabIndex = 0;
            grpArchivo.TabStop = false;
            grpArchivo.Text = "Archivo";
            //
            // lblArchivo
            //
            lblArchivo.AutoSize = false;
            lblArchivo.Location = new System.Drawing.Point(12, 22);
            lblArchivo.Name = "lblArchivo";
            lblArchivo.Size = new System.Drawing.Size(200, 16);
            lblArchivo.TabIndex = 0;
            lblArchivo.Text = "Archivo:";
            //
            // txtArchivo
            //
            txtArchivo.Location = new System.Drawing.Point(12, 42);
            txtArchivo.Name = "txtArchivo";
            txtArchivo.Size = new System.Drawing.Size(516, 20);
            txtArchivo.TabIndex = 1;
            //
            // btnExaminar
            //
            btnExaminar.Location = new System.Drawing.Point(536, 40);
            btnExaminar.Name = "btnExaminar";
            btnExaminar.Size = new System.Drawing.Size(90, 26);
            btnExaminar.TabIndex = 2;
            btnExaminar.Text = "Examinar...";
            btnExaminar.Click += BtnExaminar_Click;
            //
            // btnLeer
            //
            btnLeer.Location = new System.Drawing.Point(632, 40);
            btnLeer.Name = "btnLeer";
            btnLeer.Size = new System.Drawing.Size(92, 26);
            btnLeer.TabIndex = 3;
            btnLeer.Text = "Leer columnas";
            btnLeer.Click += BtnLeer_Click;
            //
            // lblSeparador
            //
            lblSeparador.AutoSize = false;
            lblSeparador.Location = new System.Drawing.Point(12, 74);
            lblSeparador.Name = "lblSeparador";
            lblSeparador.Size = new System.Drawing.Size(100, 16);
            lblSeparador.TabIndex = 4;
            lblSeparador.Text = "Separador";
            //
            // cmbSeparador
            //
            cmbSeparador.DropDownStyle = Telerik.WinControls.RadDropDownStyle.DropDownList;
            cmbSeparador.Location = new System.Drawing.Point(12, 90);
            cmbSeparador.Name = "cmbSeparador";
            cmbSeparador.Size = new System.Drawing.Size(110, 24);
            cmbSeparador.TabIndex = 5;
            //
            // lblEncoding
            //
            lblEncoding.AutoSize = false;
            lblEncoding.Location = new System.Drawing.Point(130, 74);
            lblEncoding.Name = "lblEncoding";
            lblEncoding.Size = new System.Drawing.Size(100, 16);
            lblEncoding.TabIndex = 6;
            lblEncoding.Text = "Encoding";
            //
            // cmbEncoding
            //
            cmbEncoding.DropDownStyle = Telerik.WinControls.RadDropDownStyle.DropDownList;
            cmbEncoding.Location = new System.Drawing.Point(130, 90);
            cmbEncoding.Name = "cmbEncoding";
            cmbEncoding.Size = new System.Drawing.Size(100, 24);
            cmbEncoding.TabIndex = 7;
            //
            // lblFilaEncab
            //
            lblFilaEncab.AutoSize = false;
            lblFilaEncab.Location = new System.Drawing.Point(238, 74);
            lblFilaEncab.Name = "lblFilaEncab";
            lblFilaEncab.Size = new System.Drawing.Size(70, 16);
            lblFilaEncab.TabIndex = 8;
            lblFilaEncab.Text = "Fila encab.";
            //
            // spnFila
            //
            spnFila.Location = new System.Drawing.Point(238, 90);
            spnFila.Maximum = 100M;
            spnFila.Minimum = 1M;
            spnFila.Name = "spnFila";
            spnFila.Size = new System.Drawing.Size(60, 20);
            spnFila.TabIndex = 9;
            spnFila.Value = 1M;
            //
            // lblSepDecimal
            //
            lblSepDecimal.AutoSize = false;
            lblSepDecimal.Location = new System.Drawing.Point(306, 74);
            lblSepDecimal.Name = "lblSepDecimal";
            lblSepDecimal.Size = new System.Drawing.Size(100, 16);
            lblSepDecimal.TabIndex = 10;
            lblSepDecimal.Text = "Sep. decimal";
            //
            // cmbSepDec
            //
            cmbSepDec.DropDownStyle = Telerik.WinControls.RadDropDownStyle.DropDownList;
            cmbSepDec.Location = new System.Drawing.Point(306, 90);
            cmbSepDec.Name = "cmbSepDec";
            cmbSepDec.Size = new System.Drawing.Size(100, 24);
            cmbSepDec.TabIndex = 11;
            //
            // lblHojaExcel
            //
            lblHojaExcel.AutoSize = false;
            lblHojaExcel.Location = new System.Drawing.Point(414, 74);
            lblHojaExcel.Name = "lblHojaExcel";
            lblHojaExcel.Size = new System.Drawing.Size(100, 16);
            lblHojaExcel.TabIndex = 12;
            lblHojaExcel.Text = "Hoja Excel";
            //
            // txtHoja
            //
            txtHoja.Location = new System.Drawing.Point(414, 90);
            txtHoja.Name = "txtHoja";
            txtHoja.Size = new System.Drawing.Size(150, 20);
            txtHoja.TabIndex = 13;
            //
            // grpMapeo
            //
            // Los campos de mapeo se generan en tiempo de ejecucion (uno por cada
            // PreseaProveedorImporter.Campos[i]) porque su cantidad depende de datos,
            // no de layout fijo. Ver FormImportarProveedoresPresea.PoblarMapeoDinamico().
            grpMapeo.Location = new System.Drawing.Point(12, 132);
            grpMapeo.Name = "grpMapeo";
            grpMapeo.Size = new System.Drawing.Size(736, 300);
            grpMapeo.TabIndex = 1;
            grpMapeo.TabStop = false;
            grpMapeo.Text = "Mapeo: campo de PRESEA  ->  columna del archivo";
            //
            // grpPreview
            //
            grpPreview.Controls.Add(gridPreview);
            grpPreview.Location = new System.Drawing.Point(12, 438);
            grpPreview.Name = "grpPreview";
            grpPreview.Size = new System.Drawing.Size(736, 150);
            grpPreview.TabIndex = 2;
            grpPreview.TabStop = false;
            grpPreview.Text = "Vista previa";
            //
            // gridPreview
            //
            gridPreview.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            gridPreview.Location = new System.Drawing.Point(8, 20);
            gridPreview.MasterTemplate.AllowAddNewRow = false;
            gridPreview.Name = "gridPreview";
            gridPreview.ReadOnly = true;
            gridPreview.Size = new System.Drawing.Size(720, 122);
            gridPreview.TabIndex = 0;
            //
            // pnl
            //
            pnl.Controls.Add(lblEstado);
            pnl.Controls.Add(btnPrevisualizar);
            pnl.Controls.Add(btnImportar);
            pnl.Controls.Add(btnCerrar);
            pnl.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnl.Location = new System.Drawing.Point(0, 616);
            pnl.Name = "pnl";
            pnl.Size = new System.Drawing.Size(760, 44);
            pnl.TabIndex = 3;
            //
            // lblEstado
            //
            lblEstado.AutoSize = true;
            lblEstado.Location = new System.Drawing.Point(12, 14);
            lblEstado.Name = "lblEstado";
            lblEstado.TabIndex = 0;
            //
            // btnPrevisualizar
            //
            btnPrevisualizar.Location = new System.Drawing.Point(440, 8);
            btnPrevisualizar.Name = "btnPrevisualizar";
            btnPrevisualizar.Size = new System.Drawing.Size(110, 28);
            btnPrevisualizar.TabIndex = 1;
            btnPrevisualizar.Text = "Previsualizar";
            btnPrevisualizar.Click += BtnPrevisualizar_Click;
            //
            // btnImportar
            //
            btnImportar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnImportar.Location = new System.Drawing.Point(556, 8);
            btnImportar.Name = "btnImportar";
            btnImportar.Size = new System.Drawing.Size(96, 28);
            btnImportar.TabIndex = 2;
            btnImportar.Text = "Importar";
            btnImportar.Click += BtnImportar_Click;
            //
            // btnCerrar
            //
            btnCerrar.Location = new System.Drawing.Point(658, 8);
            btnCerrar.Name = "btnCerrar";
            btnCerrar.Size = new System.Drawing.Size(90, 28);
            btnCerrar.TabIndex = 3;
            btnCerrar.Text = "Cerrar";
            btnCerrar.Click += btnCerrar_Click;
            //
            // FormImportarProveedoresPresea
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(760, 660);
            Controls.Add(grpMapeo);
            Controls.Add(grpPreview);
            Controls.Add(grpArchivo);
            Controls.Add(pnl);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormImportarProveedoresPresea";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Importar proveedores PRESEA (CSV / Excel)";
            grpArchivo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)lblArchivo).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtArchivo).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnExaminar).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnLeer).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblSeparador).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbSeparador).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblEncoding).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbEncoding).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblFilaEncab).EndInit();
            ((System.ComponentModel.ISupportInitialize)spnFila).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblSepDecimal).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbSepDec).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblHojaExcel).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtHoja).EndInit();
            grpMapeo.ResumeLayout(false);
            grpPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridPreview.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridPreview).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnl).EndInit();
            pnl.ResumeLayout(false);
            pnl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)lblEstado).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnPrevisualizar).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnImportar).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnCerrar).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        private System.Windows.Forms.GroupBox         grpArchivo;
        private Telerik.WinControls.UI.RadLabel        lblArchivo;
        private Telerik.WinControls.UI.RadTextBox       txtArchivo;
        private Telerik.WinControls.UI.RadButton        btnExaminar;
        private Telerik.WinControls.UI.RadButton        btnLeer;
        private Telerik.WinControls.UI.RadLabel         lblSeparador;
        private Telerik.WinControls.UI.RadDropDownList  cmbSeparador;
        private Telerik.WinControls.UI.RadLabel         lblEncoding;
        private Telerik.WinControls.UI.RadDropDownList  cmbEncoding;
        private Telerik.WinControls.UI.RadLabel         lblFilaEncab;
        private Telerik.WinControls.UI.RadSpinEditor    spnFila;
        private Telerik.WinControls.UI.RadLabel         lblSepDecimal;
        private Telerik.WinControls.UI.RadDropDownList  cmbSepDec;
        private Telerik.WinControls.UI.RadLabel         lblHojaExcel;
        private Telerik.WinControls.UI.RadTextBox       txtHoja;
        private System.Windows.Forms.GroupBox           grpMapeo;
        private System.Windows.Forms.GroupBox           grpPreview;
        private Telerik.WinControls.UI.RadGridView      gridPreview;
        private Telerik.WinControls.UI.RadPanel         pnl;
        private Telerik.WinControls.UI.RadLabel         lblEstado;
        private Telerik.WinControls.UI.RadButton        btnPrevisualizar;
        private Telerik.WinControls.UI.RadButton        btnImportar;
        private Telerik.WinControls.UI.RadButton        btnCerrar;
    }
}
