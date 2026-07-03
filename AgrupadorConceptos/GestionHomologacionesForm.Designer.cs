namespace AgrupadorConceptos
{
    partial class GestionHomologacionesForm
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
            this.dgvHomologaciones = new Telerik.WinControls.UI.RadGridView();
            this.btnEliminar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHomologaciones)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHomologaciones.MasterTemplate)).BeginInit();
            this.SuspendLayout();
            
            // dgvHomologaciones
            this.dgvHomologaciones.Location = new System.Drawing.Point(12, 12);
            this.dgvHomologaciones.Name = "dgvHomologaciones";
            this.dgvHomologaciones.ReadOnly = true;
            this.dgvHomologaciones.Size = new System.Drawing.Size(660, 300);
            this.dgvHomologaciones.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            this.dgvHomologaciones.MasterTemplate.AllowAddNewRow = false;
            this.dgvHomologaciones.MasterTemplate.AllowDeleteRow = false;
            this.dgvHomologaciones.MasterTemplate.AllowEditRow = false;

            // btnEliminar
            this.btnEliminar.Location = new System.Drawing.Point(12, 320);
            this.btnEliminar.Name = "btnEliminar";
            this.btnEliminar.Size = new System.Drawing.Size(150, 30);
            this.btnEliminar.Text = "Eliminar Seleccionada";
            this.btnEliminar.UseVisualStyleBackColor = true;
            this.btnEliminar.Click += new System.EventHandler(this.btnEliminar_Click);

            // GestionHomologacionesForm
            this.ClientSize = new System.Drawing.Size(684, 361);
            this.Controls.Add(this.btnEliminar);
            this.Controls.Add(this.dgvHomologaciones);
            this.Name = "GestionHomologacionesForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Gestión de Homologaciones";
            ((System.ComponentModel.ISupportInitialize)(this.dgvHomologaciones.MasterTemplate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHomologaciones)).EndInit();
            this.ResumeLayout(false);
        }

        private Telerik.WinControls.UI.RadGridView dgvHomologaciones;
        private System.Windows.Forms.Button btnEliminar;
    }
}