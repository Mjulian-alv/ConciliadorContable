namespace AgrupadorConceptos
{
    partial class AsignarCuentaConceptoDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Layout del dialogo de asignacion de cuenta
        private void InitializeComponent()
        {
            lblConcepto = new System.Windows.Forms.Label();
            lblCuenta = new System.Windows.Forms.Label();
            cmbCuenta = new System.Windows.Forms.ComboBox();
            btnGuardar = new System.Windows.Forms.Button();
            btnCancelar = new System.Windows.Forms.Button();
            SuspendLayout();
            //
            // lblConcepto
            //
            lblConcepto.AutoSize = true;
            lblConcepto.Location = new System.Drawing.Point(16, 16);
            lblConcepto.Size = new System.Drawing.Size(400, 20);
            //
            // lblCuenta
            //
            lblCuenta.AutoSize = true;
            lblCuenta.Location = new System.Drawing.Point(16, 50);
            lblCuenta.Text = "Cuenta contable:";
            //
            // cmbCuenta
            //
            cmbCuenta.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbCuenta.Location = new System.Drawing.Point(140, 47);
            cmbCuenta.Size = new System.Drawing.Size(340, 23);
            //
            // btnGuardar
            //
            btnGuardar.Location = new System.Drawing.Point(300, 90);
            btnGuardar.Size = new System.Drawing.Size(90, 30);
            btnGuardar.Text = "Guardar";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Click += btnGuardar_Click;
            //
            // btnCancelar
            //
            btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancelar.Location = new System.Drawing.Point(396, 90);
            btnCancelar.Size = new System.Drawing.Size(90, 30);
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            //
            // AsignarCuentaConceptoDialog
            //
            AcceptButton = btnGuardar;
            CancelButton = btnCancelar;
            ClientSize = new System.Drawing.Size(500, 136);
            Controls.Add(btnCancelar);
            Controls.Add(btnGuardar);
            Controls.Add(cmbCuenta);
            Controls.Add(lblCuenta);
            Controls.Add(lblConcepto);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AsignarCuentaConceptoDialog";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Asignar cuenta contable";
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.Label lblConcepto;
        private System.Windows.Forms.Label lblCuenta;
        private System.Windows.Forms.ComboBox cmbCuenta;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnCancelar;
    }
}
