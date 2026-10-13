namespace AgrupadorConceptos
{
    partial class HomologarForm
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
            this.lblOriginal = new System.Windows.Forms.Label();
            this.txtOriginal = new System.Windows.Forms.TextBox();
            this.lblEstandar = new System.Windows.Forms.Label();
            this.cmbEstandar = new System.Windows.Forms.ComboBox();
            this.lblCuenta = new System.Windows.Forms.Label();
            this.cmbCuenta = new System.Windows.Forms.ComboBox();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.lblAyuda = new System.Windows.Forms.Label();
            this.SuspendLayout();
            
            // lblOriginal
            this.lblOriginal.AutoSize = true;
            this.lblOriginal.Location = new System.Drawing.Point(20, 20);
            this.lblOriginal.Name = "lblOriginal";
            this.lblOriginal.Size = new System.Drawing.Size(126, 15);
            this.lblOriginal.Text = "Concepto de Banco:";
            
            // txtOriginal
            this.txtOriginal.Location = new System.Drawing.Point(160, 17);
            this.txtOriginal.Name = "txtOriginal";
            this.txtOriginal.Size = new System.Drawing.Size(400, 23);
            
            // lblEstandar
            this.lblEstandar.AutoSize = true;
            this.lblEstandar.Location = new System.Drawing.Point(20, 100);
            this.lblEstandar.Name = "lblEstandar";
            this.lblEstandar.Size = new System.Drawing.Size(126, 15);
            // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 3 - Se restauran los acentos que el archivo había perdido por un guardado con codificación equivocada (se veían como signo de reemplazo).
            this.lblEstandar.Text = "Concepto Estándar:";
            
            // cmbEstandar
            this.cmbEstandar.FormattingEnabled = true;
            this.cmbEstandar.Location = new System.Drawing.Point(160, 97);
            this.cmbEstandar.Name = "cmbEstandar";
            this.cmbEstandar.Size = new System.Drawing.Size(300, 23);
            
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Cuenta contable del concepto al homologar
            // lblCuenta
            this.lblCuenta.AutoSize = true;
            this.lblCuenta.Location = new System.Drawing.Point(20, 130);
            this.lblCuenta.Name = "lblCuenta";
            this.lblCuenta.Size = new System.Drawing.Size(126, 15);
            this.lblCuenta.Text = "Cuenta Contable:";

            // cmbCuenta
            this.cmbCuenta.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCuenta.FormattingEnabled = true;
            this.cmbCuenta.Location = new System.Drawing.Point(160, 127);
            this.cmbCuenta.Name = "cmbCuenta";
            this.cmbCuenta.Size = new System.Drawing.Size(300, 23);

            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Baja el boton para dejar lugar al combo de cuenta
            // btnGuardar
            this.btnGuardar.Location = new System.Drawing.Point(160, 180);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(150, 30);
            // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 3 - Se restauran los acentos que el archivo había perdido por un guardado con codificación equivocada (se veían como signo de reemplazo).
            this.btnGuardar.Text = "Guardar Homologación";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);

            // lblAyuda
            this.lblAyuda.AutoSize = true;
            this.lblAyuda.Location = new System.Drawing.Point(160, 48);
            this.lblAyuda.Name = "lblAyuda";
            this.lblAyuda.Size = new System.Drawing.Size(390, 30);
            // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 3 - Se restauran los acentos que el archivo había perdido por un guardado con codificación equivocada (se veían como signo de reemplazo).
            this.lblAyuda.Text = "Si el campo es una descripción larga, puedes acortar el texto para \r\nque busque esta palabra clave (Ej. 'TRANSF').";
            this.lblAyuda.ForeColor = System.Drawing.Color.Gray;
            
            // HomologarForm
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - El form crece para que entre el combo de cuenta
            this.ClientSize = new System.Drawing.Size(584, 231);
            this.Controls.Add(this.lblAyuda);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.cmbCuenta);
            this.Controls.Add(this.lblCuenta);
            this.Controls.Add(this.cmbEstandar);
            this.Controls.Add(this.lblEstandar);
            this.Controls.Add(this.txtOriginal);
            this.Controls.Add(this.lblOriginal);
            this.Name = "HomologarForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 3 - Se restauran los acentos que el archivo había perdido por un guardado con codificación equivocada (se veían como signo de reemplazo).
            this.Text = "Crear Homologación de Conceptos";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblOriginal;
        private System.Windows.Forms.TextBox txtOriginal;
        private System.Windows.Forms.Label lblEstandar;
        private System.Windows.Forms.ComboBox cmbEstandar;
        private System.Windows.Forms.Label lblCuenta;
        private System.Windows.Forms.ComboBox cmbCuenta;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Label lblAyuda;
    }
}