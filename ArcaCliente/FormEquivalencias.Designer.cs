namespace ArcaCliente
{
    partial class FormEquivalencias
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            Telerik.WinControls.UI.TableViewDefinition tableViewDefinition1 = new Telerik.WinControls.UI.TableViewDefinition();
            pnlBotones = new Telerik.WinControls.UI.RadPanel();
            btnGuardar = new Telerik.WinControls.UI.RadButton();
            btnCancelar = new Telerik.WinControls.UI.RadButton();
            gridDatos = new Telerik.WinControls.UI.RadGridView();
            ((System.ComponentModel.ISupportInitialize)pnlBotones).BeginInit();
            pnlBotones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)btnGuardar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnCancelar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridDatos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridDatos.MasterTemplate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            SuspendLayout();
            // 
            // pnlBotones
            // 
            pnlBotones.Controls.Add(btnGuardar);
            pnlBotones.Controls.Add(btnCancelar);
            pnlBotones.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlBotones.Location = new System.Drawing.Point(0, 395);
            pnlBotones.Name = "pnlBotones";
            pnlBotones.Size = new System.Drawing.Size(601, 71);
            pnlBotones.TabIndex = 0;
            pnlBotones.Paint += pnlBotones_Paint;
            // 
            // btnGuardar
            // 
            btnGuardar.Location = new System.Drawing.Point(344, 21);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new System.Drawing.Size(110, 24);
            btnGuardar.TabIndex = 0;
            btnGuardar.Text = "Guardar";
            btnGuardar.Click += btnGuardar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.Location = new System.Drawing.Point(470, 21);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new System.Drawing.Size(110, 24);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text = "Cancelar";
            btnCancelar.Click += btnCancelar_Click;
            // 
            // gridDatos
            // 
            gridDatos.Dock = System.Windows.Forms.DockStyle.Fill;
            gridDatos.Location = new System.Drawing.Point(0, 0);
            // 
            // 
            // 
            gridDatos.MasterTemplate.ViewDefinition = tableViewDefinition1;
            gridDatos.Name = "gridDatos";
            gridDatos.Size = new System.Drawing.Size(601, 395);
            gridDatos.TabIndex = 1;
            // 
            // FormEquivalencias
            // 
            AutoScaleBaseSize = new System.Drawing.Size(7, 15);
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(601, 466);
            Controls.Add(gridDatos);
            Controls.Add(pnlBotones);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormEquivalencias";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Equivalencias de Tipos de Comprobante";
            ((System.ComponentModel.ISupportInitialize)pnlBotones).EndInit();
            pnlBotones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)btnGuardar).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnCancelar).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridDatos.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridDatos).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        private Telerik.WinControls.UI.RadPanel pnlBotones;
        private Telerik.WinControls.UI.RadButton btnGuardar;
        private Telerik.WinControls.UI.RadButton btnCancelar;
        private Telerik.WinControls.UI.RadGridView gridDatos;
    }
}