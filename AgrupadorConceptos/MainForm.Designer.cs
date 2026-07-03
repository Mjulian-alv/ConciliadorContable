namespace AgrupadorConceptos
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblTitulo = new System.Windows.Forms.Label();
            lblBanco = new System.Windows.Forms.Label();
            txtBanco = new System.Windows.Forms.TextBox();
            btnCargarExcel = new System.Windows.Forms.Button();
            lblArchivoExcel = new System.Windows.Forms.Label();
            grpMapeo = new System.Windows.Forms.GroupBox();
            chkEsCodigo = new System.Windows.Forms.CheckBox();
            cmbColumnaDescripcion = new System.Windows.Forms.ComboBox();
            lblColumnaDescripcion = new System.Windows.Forms.Label();
            cmbColumnaConcepto = new System.Windows.Forms.ComboBox();
            lblColumnaConcepto = new System.Windows.Forms.Label();
            cmbColumnaFecha = new System.Windows.Forms.ComboBox();
            lblColumnaFecha = new System.Windows.Forms.Label();
            grpImporte = new System.Windows.Forms.GroupBox();
            radImporteUnico = new System.Windows.Forms.RadioButton();
            radDebeHaber = new System.Windows.Forms.RadioButton();
            lblImporteUnico = new System.Windows.Forms.Label();
            cmbImporteUnico = new System.Windows.Forms.ComboBox();
            lblDebe = new System.Windows.Forms.Label();
            cmbColumnaDebe = new System.Windows.Forms.ComboBox();
            lblHaber = new System.Windows.Forms.Label();
            cmbColumnaHaber = new System.Windows.Forms.ComboBox();
            btnGuardar = new System.Windows.Forms.Button();
            lblFilaEncabezado = new System.Windows.Forms.Label();
            numFilaEncabezado = new System.Windows.Forms.NumericUpDown();
            grpMapeo.SuspendLayout();
            grpImporte.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numFilaEncabezado).BeginInit();
            SuspendLayout();
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            lblTitulo.Location = new System.Drawing.Point(12, 9);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new System.Drawing.Size(228, 25);
            lblTitulo.TabIndex = 8;
            lblTitulo.Text = "Creación Perfil de Banco";
            // 
            // lblBanco
            // 
            lblBanco.AutoSize = true;
            lblBanco.Location = new System.Drawing.Point(14, 55);
            lblBanco.Name = "lblBanco";
            lblBanco.Size = new System.Drawing.Size(109, 15);
            lblBanco.TabIndex = 7;
            lblBanco.Text = "Nombre del Banco:";
            // 
            // txtBanco
            // 
            txtBanco.Location = new System.Drawing.Point(130, 52);
            txtBanco.Name = "txtBanco";
            txtBanco.Size = new System.Drawing.Size(200, 23);
            txtBanco.TabIndex = 6;
            // 
            // btnCargarExcel
            // 
            btnCargarExcel.Location = new System.Drawing.Point(17, 90);
            btnCargarExcel.Name = "btnCargarExcel";
            btnCargarExcel.Size = new System.Drawing.Size(150, 30);
            btnCargarExcel.TabIndex = 5;
            btnCargarExcel.Text = "Cargar Ejemplo Excel...";
            btnCargarExcel.UseVisualStyleBackColor = true;
            btnCargarExcel.Click += btnCargarExcel_Click;
            // 
            // lblArchivoExcel
            // 
            lblArchivoExcel.AutoSize = true;
            lblArchivoExcel.Location = new System.Drawing.Point(180, 98);
            lblArchivoExcel.Name = "lblArchivoExcel";
            lblArchivoExcel.Size = new System.Drawing.Size(161, 15);
            lblArchivoExcel.TabIndex = 4;
            lblArchivoExcel.Text = "Ningún archivo seleccionado";
            // 
            // grpMapeo
            // 
            grpMapeo.Controls.Add(chkEsCodigo);
            grpMapeo.Controls.Add(cmbColumnaDescripcion);
            grpMapeo.Controls.Add(lblColumnaDescripcion);
            grpMapeo.Controls.Add(cmbColumnaConcepto);
            grpMapeo.Controls.Add(lblColumnaConcepto);
            grpMapeo.Controls.Add(cmbColumnaFecha);
            grpMapeo.Controls.Add(lblColumnaFecha);
            grpMapeo.Controls.Add(grpImporte);
            grpMapeo.Location = new System.Drawing.Point(17, 130);
            grpMapeo.Name = "grpMapeo";
            grpMapeo.Size = new System.Drawing.Size(500, 324);
            grpMapeo.TabIndex = 3;
            grpMapeo.TabStop = false;
            grpMapeo.Text = "Mapeo de Columnas";
            // 
            // chkEsCodigo
            // 
            chkEsCodigo.AutoSize = true;
            chkEsCodigo.Checked = true;
            chkEsCodigo.CheckState = System.Windows.Forms.CheckState.Checked;
            chkEsCodigo.Location = new System.Drawing.Point(160, 120);
            chkEsCodigo.Name = "chkEsCodigo";
            chkEsCodigo.Size = new System.Drawing.Size(256, 19);
            chkEsCodigo.TabIndex = 0;
            chkEsCodigo.Text = "¿Es Código Exacto? (Si no, es un texto largo)";
            chkEsCodigo.UseVisualStyleBackColor = true;
            // 
            // cmbColumnaDescripcion
            // 
            cmbColumnaDescripcion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbColumnaDescripcion.FormattingEnabled = true;
            cmbColumnaDescripcion.Location = new System.Drawing.Point(160, 87);
            cmbColumnaDescripcion.Name = "cmbColumnaDescripcion";
            cmbColumnaDescripcion.Size = new System.Drawing.Size(200, 23);
            cmbColumnaDescripcion.TabIndex = 1;
            // 
            // lblColumnaDescripcion
            // 
            lblColumnaDescripcion.AutoSize = true;
            lblColumnaDescripcion.Location = new System.Drawing.Point(20, 90);
            lblColumnaDescripcion.Name = "lblColumnaDescripcion";
            lblColumnaDescripcion.Size = new System.Drawing.Size(124, 15);
            lblColumnaDescripcion.TabIndex = 2;
            lblColumnaDescripcion.Text = "Columna Descripción:";
            // 
            // cmbColumnaConcepto
            // 
            cmbColumnaConcepto.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbColumnaConcepto.FormattingEnabled = true;
            cmbColumnaConcepto.Location = new System.Drawing.Point(160, 27);
            cmbColumnaConcepto.Name = "cmbColumnaConcepto";
            cmbColumnaConcepto.Size = new System.Drawing.Size(200, 23);
            cmbColumnaConcepto.TabIndex = 3;
            // 
            // lblColumnaConcepto
            // 
            lblColumnaConcepto.AutoSize = true;
            lblColumnaConcepto.Location = new System.Drawing.Point(20, 30);
            lblColumnaConcepto.Name = "lblColumnaConcepto";
            lblColumnaConcepto.Size = new System.Drawing.Size(129, 15);
            lblColumnaConcepto.TabIndex = 4;
            lblColumnaConcepto.Text = "Columna Concepto/Id:";
            // 
            // cmbColumnaFecha
            // 
            cmbColumnaFecha.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbColumnaFecha.FormattingEnabled = true;
            cmbColumnaFecha.Location = new System.Drawing.Point(160, 57);
            cmbColumnaFecha.Name = "cmbColumnaFecha";
            cmbColumnaFecha.Size = new System.Drawing.Size(200, 23);
            cmbColumnaFecha.TabIndex = 5;
            // 
            // lblColumnaFecha
            // 
            lblColumnaFecha.AutoSize = true;
            lblColumnaFecha.Location = new System.Drawing.Point(20, 60);
            lblColumnaFecha.Name = "lblColumnaFecha";
            lblColumnaFecha.Size = new System.Drawing.Size(93, 15);
            lblColumnaFecha.TabIndex = 6;
            lblColumnaFecha.Text = "Columna Fecha:";
            // 
            // grpImporte
            // 
            grpImporte.Controls.Add(radImporteUnico);
            grpImporte.Controls.Add(radDebeHaber);
            grpImporte.Controls.Add(lblImporteUnico);
            grpImporte.Controls.Add(cmbImporteUnico);
            grpImporte.Controls.Add(lblDebe);
            grpImporte.Controls.Add(cmbColumnaDebe);
            grpImporte.Controls.Add(lblHaber);
            grpImporte.Controls.Add(cmbColumnaHaber);
            grpImporte.Location = new System.Drawing.Point(20, 140);
            grpImporte.Name = "grpImporte";
            grpImporte.Size = new System.Drawing.Size(460, 178);
            grpImporte.TabIndex = 7;
            grpImporte.TabStop = false;
            grpImporte.Text = "Mapeo de Importes";
            // 
            // radImporteUnico
            // 
            radImporteUnico.AutoSize = true;
            radImporteUnico.Location = new System.Drawing.Point(20, 30);
            radImporteUnico.Name = "radImporteUnico";
            radImporteUnico.Size = new System.Drawing.Size(101, 19);
            radImporteUnico.TabIndex = 0;
            radImporteUnico.Text = "Importe Único";
            radImporteUnico.UseVisualStyleBackColor = true;
            // 
            // radDebeHaber
            // 
            radDebeHaber.AutoSize = true;
            radDebeHaber.Location = new System.Drawing.Point(150, 30);
            radDebeHaber.Name = "radDebeHaber";
            radDebeHaber.Size = new System.Drawing.Size(89, 19);
            radDebeHaber.TabIndex = 1;
            radDebeHaber.Text = "Debe/Haber";
            radDebeHaber.UseVisualStyleBackColor = true;
            // 
            // lblImporteUnico
            // 
            lblImporteUnico.AutoSize = true;
            lblImporteUnico.Location = new System.Drawing.Point(20, 70);
            lblImporteUnico.Name = "lblImporteUnico";
            lblImporteUnico.Size = new System.Drawing.Size(104, 15);
            lblImporteUnico.TabIndex = 2;
            lblImporteUnico.Text = "Columna Importe:";
            // 
            // cmbImporteUnico
            // 
            cmbImporteUnico.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbImporteUnico.FormattingEnabled = true;
            cmbImporteUnico.Location = new System.Drawing.Point(120, 67);
            cmbImporteUnico.Name = "cmbImporteUnico";
            cmbImporteUnico.Size = new System.Drawing.Size(200, 23);
            cmbImporteUnico.TabIndex = 3;
            // 
            // lblDebe
            // 
            lblDebe.AutoSize = true;
            lblDebe.Location = new System.Drawing.Point(20, 110);
            lblDebe.Name = "lblDebe";
            lblDebe.Size = new System.Drawing.Size(89, 15);
            lblDebe.TabIndex = 4;
            lblDebe.Text = "Columna Debe:";
            // 
            // cmbColumnaDebe
            // 
            cmbColumnaDebe.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbColumnaDebe.FormattingEnabled = true;
            cmbColumnaDebe.Location = new System.Drawing.Point(120, 107);
            cmbColumnaDebe.Name = "cmbColumnaDebe";
            cmbColumnaDebe.Size = new System.Drawing.Size(200, 23);
            cmbColumnaDebe.TabIndex = 5;
            // 
            // lblHaber
            // 
            lblHaber.AutoSize = true;
            lblHaber.Location = new System.Drawing.Point(20, 150);
            lblHaber.Name = "lblHaber";
            lblHaber.Size = new System.Drawing.Size(94, 15);
            lblHaber.TabIndex = 6;
            lblHaber.Text = "Columna Haber:";
            // 
            // cmbColumnaHaber
            // 
            cmbColumnaHaber.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbColumnaHaber.FormattingEnabled = true;
            cmbColumnaHaber.Location = new System.Drawing.Point(120, 147);
            cmbColumnaHaber.Name = "cmbColumnaHaber";
            cmbColumnaHaber.Size = new System.Drawing.Size(200, 23);
            cmbColumnaHaber.TabIndex = 7;
            // 
            // btnGuardar
            // 
            btnGuardar.Location = new System.Drawing.Point(17, 460);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new System.Drawing.Size(120, 40);
            btnGuardar.TabIndex = 2;
            btnGuardar.Text = "Guardar Perfil";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // lblFilaEncabezado
            // 
            lblFilaEncabezado.AutoSize = true;
            lblFilaEncabezado.Location = new System.Drawing.Point(340, 55);
            lblFilaEncabezado.Name = "lblFilaEncabezado";
            lblFilaEncabezado.Size = new System.Drawing.Size(115, 15);
            lblFilaEncabezado.TabIndex = 1;
            lblFilaEncabezado.Text = "Fila de Encabezados:";
            // 
            // numFilaEncabezado
            // 
            numFilaEncabezado.Location = new System.Drawing.Point(465, 52);
            numFilaEncabezado.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numFilaEncabezado.Name = "numFilaEncabezado";
            numFilaEncabezado.Size = new System.Drawing.Size(50, 23);
            numFilaEncabezado.TabIndex = 0;
            numFilaEncabezado.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // MainForm
            // 
            ClientSize = new System.Drawing.Size(550, 520);
            Controls.Add(numFilaEncabezado);
            Controls.Add(lblFilaEncabezado);
            Controls.Add(btnGuardar);
            Controls.Add(grpMapeo);
            Controls.Add(lblArchivoExcel);
            Controls.Add(btnCargarExcel);
            Controls.Add(txtBanco);
            Controls.Add(lblBanco);
            Controls.Add(lblTitulo);
            Name = "MainForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Agrupador de Conceptos - Perfiles Bancarios";
            grpMapeo.ResumeLayout(false);
            grpMapeo.PerformLayout();
            grpImporte.ResumeLayout(false);
            grpImporte.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numFilaEncabezado).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblBanco;
        private System.Windows.Forms.TextBox txtBanco;
        private System.Windows.Forms.Button btnCargarExcel;
        private System.Windows.Forms.Label lblArchivoExcel;
        private System.Windows.Forms.GroupBox grpMapeo;
        private System.Windows.Forms.Label lblColumnaConcepto;
        private System.Windows.Forms.ComboBox cmbColumnaConcepto;
        private System.Windows.Forms.Label lblColumnaDescripcion;
        private System.Windows.Forms.ComboBox cmbColumnaDescripcion;
        private System.Windows.Forms.CheckBox chkEsCodigo;
        private System.Windows.Forms.GroupBox grpImporte;
        private System.Windows.Forms.RadioButton radImporteUnico;
        private System.Windows.Forms.RadioButton radDebeHaber;
        private System.Windows.Forms.Label lblImporteUnico;
        private System.Windows.Forms.ComboBox cmbImporteUnico;
        private System.Windows.Forms.Label lblDebe;
        private System.Windows.Forms.ComboBox cmbColumnaDebe;
        private System.Windows.Forms.Label lblHaber;
        private System.Windows.Forms.ComboBox cmbColumnaHaber;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Label lblFilaEncabezado;
        private System.Windows.Forms.NumericUpDown numFilaEncabezado;
        private System.Windows.Forms.Label lblColumnaFecha;
        private System.Windows.Forms.ComboBox cmbColumnaFecha;
    }
}