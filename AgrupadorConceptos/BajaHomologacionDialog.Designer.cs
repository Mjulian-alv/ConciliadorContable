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
            Telerik.WinControls.UI.TableViewDefinition tableViewDefinition1 = new Telerik.WinControls.UI.TableViewDefinition();
            this.lblResumen = new System.Windows.Forms.Label();
            this.lblDetalle = new System.Windows.Forms.Label();
            this.dgvPorArchivo = new Telerik.WinControls.UI.RadGridView();
            this.lblPregunta = new System.Windows.Forms.Label();
            this.rbPendientes = new System.Windows.Forms.RadioButton();
            this.rbReasignar = new System.Windows.Forms.RadioButton();
            this.cmbConcepto = new System.Windows.Forms.ComboBox();
            this.lblAviso = new System.Windows.Forms.Label();
            this.btnAceptar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)this.dgvPorArchivo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.dgvPorArchivo.MasterTemplate).BeginInit();
            this.SuspendLayout();
            //
            // lblResumen
            //
            this.lblResumen.Location = new System.Drawing.Point(16, 16);
            this.lblResumen.Name = "lblResumen";
            this.lblResumen.Size = new System.Drawing.Size(608, 60);
            this.lblResumen.TabIndex = 0;
            //
            // lblDetalle
            //
            this.lblDetalle.AutoSize = true;
            this.lblDetalle.Location = new System.Drawing.Point(16, 84);
            this.lblDetalle.Name = "lblDetalle";
            this.lblDetalle.Size = new System.Drawing.Size(200, 15);
            this.lblDetalle.TabIndex = 1;
            this.lblDetalle.Text = "Sobre qué archivos importados cae:";
            //
            // dgvPorArchivo
            //
            this.dgvPorArchivo.Location = new System.Drawing.Point(16, 104);
            //
            //
            //
            this.dgvPorArchivo.MasterTemplate.AllowAddNewRow = false;
            this.dgvPorArchivo.MasterTemplate.AllowDeleteRow = false;
            this.dgvPorArchivo.MasterTemplate.AllowEditRow = false;
            this.dgvPorArchivo.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            this.dgvPorArchivo.MasterTemplate.EnableFiltering = false;
            this.dgvPorArchivo.MasterTemplate.ViewDefinition = tableViewDefinition1;
            this.dgvPorArchivo.Name = "dgvPorArchivo";
            this.dgvPorArchivo.ReadOnly = true;
            this.dgvPorArchivo.ShowGroupPanel = false;
            this.dgvPorArchivo.Size = new System.Drawing.Size(608, 150);
            this.dgvPorArchivo.TabIndex = 2;
            //
            // lblPregunta
            //
            this.lblPregunta.Location = new System.Drawing.Point(16, 266);
            this.lblPregunta.Name = "lblPregunta";
            this.lblPregunta.Size = new System.Drawing.Size(608, 20);
            this.lblPregunta.TabIndex = 3;
            this.lblPregunta.Text = "¿Qué se hace con los movimientos que quedan sin regla?";
            //
            // rbPendientes
            //
            this.rbPendientes.Location = new System.Drawing.Point(24, 292);
            this.rbPendientes.Name = "rbPendientes";
            this.rbPendientes.Size = new System.Drawing.Size(598, 22);
            this.rbPendientes.TabIndex = 4;
            this.rbPendientes.Text = "Dejarlos pendientes de homologar";
            this.rbPendientes.UseVisualStyleBackColor = true;
            //
            // rbReasignar
            //
            this.rbReasignar.Location = new System.Drawing.Point(24, 320);
            this.rbReasignar.Name = "rbReasignar";
            this.rbReasignar.Size = new System.Drawing.Size(150, 22);
            this.rbReasignar.TabIndex = 5;
            this.rbReasignar.Text = "Reasignarlos a:";
            this.rbReasignar.UseVisualStyleBackColor = true;
            //
            // cmbConcepto
            //
            this.cmbConcepto.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cmbConcepto.FormattingEnabled = true;
            this.cmbConcepto.Location = new System.Drawing.Point(180, 320);
            this.cmbConcepto.Name = "cmbConcepto";
            this.cmbConcepto.Size = new System.Drawing.Size(444, 23);
            this.cmbConcepto.TabIndex = 6;
            //
            // lblAviso
            //
            this.lblAviso.ForeColor = System.Drawing.Color.Gray;
            this.lblAviso.Location = new System.Drawing.Point(24, 350);
            this.lblAviso.Name = "lblAviso";
            this.lblAviso.Size = new System.Drawing.Size(600, 34);
            this.lblAviso.TabIndex = 7;
            this.lblAviso.Text = "La homologación se borra igual: una importación futura de ese mismo valor va a quedar pendiente.";
            //
            // btnAceptar
            //
            this.btnAceptar.Location = new System.Drawing.Point(428, 392);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new System.Drawing.Size(95, 30);
            this.btnAceptar.TabIndex = 8;
            this.btnAceptar.Text = "Aceptar";
            this.btnAceptar.UseVisualStyleBackColor = true;
            this.btnAceptar.Click += new System.EventHandler(this.btnAceptar_Click);
            //
            // btnCancelar
            //
            this.btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelar.Location = new System.Drawing.Point(529, 392);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(95, 30);
            this.btnCancelar.TabIndex = 9;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            //
            // BajaHomologacionDialog
            //
            this.AcceptButton = this.btnAceptar;
            this.CancelButton = this.btnCancelar;
            this.ClientSize = new System.Drawing.Size(640, 436);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnAceptar);
            this.Controls.Add(this.lblAviso);
            this.Controls.Add(this.cmbConcepto);
            this.Controls.Add(this.rbReasignar);
            this.Controls.Add(this.rbPendientes);
            this.Controls.Add(this.lblPregunta);
            this.Controls.Add(this.dgvPorArchivo);
            this.Controls.Add(this.lblDetalle);
            this.Controls.Add(this.lblResumen);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BajaHomologacionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Eliminar homologación";
            ((System.ComponentModel.ISupportInitialize)this.dgvPorArchivo.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.dgvPorArchivo).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblResumen;
        private System.Windows.Forms.Label lblDetalle;
        private Telerik.WinControls.UI.RadGridView dgvPorArchivo;
        private System.Windows.Forms.Label lblPregunta;
        private System.Windows.Forms.RadioButton rbPendientes;
        private System.Windows.Forms.RadioButton rbReasignar;
        private System.Windows.Forms.ComboBox cmbConcepto;
        private System.Windows.Forms.Label lblAviso;
        private System.Windows.Forms.Button btnAceptar;
        private System.Windows.Forms.Button btnCancelar;
    }
}
