namespace AgrupadorConceptos
{
    partial class BajaHomologacionDialog
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

        private void InitializeComponent()
        {
            this.lblResumen = new System.Windows.Forms.Label();
            this.lblPregunta = new System.Windows.Forms.Label();
            this.rbPendientes = new System.Windows.Forms.RadioButton();
            this.rbReasignar = new System.Windows.Forms.RadioButton();
            this.cmbConcepto = new System.Windows.Forms.ComboBox();
            this.lblAviso = new System.Windows.Forms.Label();
            this.btnAceptar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // lblResumen
            //
            this.lblResumen.Location = new System.Drawing.Point(16, 16);
            this.lblResumen.Name = "lblResumen";
            this.lblResumen.Size = new System.Drawing.Size(520, 60);
            this.lblResumen.TabIndex = 0;
            //
            // lblPregunta
            //
            this.lblPregunta.Location = new System.Drawing.Point(16, 86);
            this.lblPregunta.Name = "lblPregunta";
            this.lblPregunta.Size = new System.Drawing.Size(520, 20);
            this.lblPregunta.TabIndex = 1;
            this.lblPregunta.Text = "¿Qué se hace con los movimientos que quedan sin regla?";
            //
            // rbPendientes
            //
            this.rbPendientes.Location = new System.Drawing.Point(24, 112);
            this.rbPendientes.Name = "rbPendientes";
            this.rbPendientes.Size = new System.Drawing.Size(510, 22);
            this.rbPendientes.TabIndex = 2;
            this.rbPendientes.Text = "Dejarlos pendientes de homologar";
            this.rbPendientes.UseVisualStyleBackColor = true;
            //
            // rbReasignar
            //
            this.rbReasignar.Location = new System.Drawing.Point(24, 140);
            this.rbReasignar.Name = "rbReasignar";
            this.rbReasignar.Size = new System.Drawing.Size(150, 22);
            this.rbReasignar.TabIndex = 3;
            this.rbReasignar.Text = "Reasignarlos a:";
            this.rbReasignar.UseVisualStyleBackColor = true;
            //
            // cmbConcepto
            //
            this.cmbConcepto.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cmbConcepto.FormattingEnabled = true;
            this.cmbConcepto.Location = new System.Drawing.Point(180, 140);
            this.cmbConcepto.Name = "cmbConcepto";
            this.cmbConcepto.Size = new System.Drawing.Size(356, 23);
            this.cmbConcepto.TabIndex = 4;
            //
            // lblAviso
            //
            this.lblAviso.ForeColor = System.Drawing.Color.Gray;
            this.lblAviso.Location = new System.Drawing.Point(24, 170);
            this.lblAviso.Name = "lblAviso";
            this.lblAviso.Size = new System.Drawing.Size(512, 34);
            this.lblAviso.TabIndex = 5;
            this.lblAviso.Text = "La homologación se borra igual: una importación futura de ese mismo valor va a quedar pendiente.";
            //
            // btnAceptar
            //
            this.btnAceptar.Location = new System.Drawing.Point(340, 214);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new System.Drawing.Size(95, 30);
            this.btnAceptar.TabIndex = 6;
            this.btnAceptar.Text = "Aceptar";
            this.btnAceptar.UseVisualStyleBackColor = true;
            this.btnAceptar.Click += new System.EventHandler(this.btnAceptar_Click);
            //
            // btnCancelar
            //
            this.btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelar.Location = new System.Drawing.Point(441, 214);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(95, 30);
            this.btnCancelar.TabIndex = 7;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            //
            // BajaHomologacionDialog
            //
            this.AcceptButton = this.btnAceptar;
            this.CancelButton = this.btnCancelar;
            this.ClientSize = new System.Drawing.Size(552, 258);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnAceptar);
            this.Controls.Add(this.lblAviso);
            this.Controls.Add(this.cmbConcepto);
            this.Controls.Add(this.rbReasignar);
            this.Controls.Add(this.rbPendientes);
            this.Controls.Add(this.lblPregunta);
            this.Controls.Add(this.lblResumen);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BajaHomologacionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Eliminar homologación";
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label lblResumen;
        private System.Windows.Forms.Label lblPregunta;
        private System.Windows.Forms.RadioButton rbPendientes;
        private System.Windows.Forms.RadioButton rbReasignar;
        private System.Windows.Forms.ComboBox cmbConcepto;
        private System.Windows.Forms.Label lblAviso;
        private System.Windows.Forms.Button btnAceptar;
        private System.Windows.Forms.Button btnCancelar;
    }
}
