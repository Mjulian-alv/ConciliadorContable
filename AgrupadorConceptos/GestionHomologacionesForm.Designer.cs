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
            Telerik.WinControls.UI.TableViewDefinition tableViewDefinition1 = new Telerik.WinControls.UI.TableViewDefinition();
            dgvHomologaciones = new Telerik.WinControls.UI.RadGridView();
            btnEliminar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)dgvHomologaciones).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvHomologaciones.MasterTemplate).BeginInit();
            SuspendLayout();
            // 
            // dgvHomologaciones
            // 
            dgvHomologaciones.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dgvHomologaciones.EnableCustomFiltering = true;
            dgvHomologaciones.Location = new System.Drawing.Point(12, 12);
            // 
            // 
            // 
            dgvHomologaciones.MasterTemplate.AllowAddNewRow = false;
            dgvHomologaciones.MasterTemplate.AllowDeleteRow = false;
            dgvHomologaciones.MasterTemplate.AllowEditRow = false;
            dgvHomologaciones.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            dgvHomologaciones.MasterTemplate.EnableCustomFiltering = true;
            dgvHomologaciones.MasterTemplate.EnableFiltering = true;
            dgvHomologaciones.MasterTemplate.ViewDefinition = tableViewDefinition1;
            dgvHomologaciones.Name = "dgvHomologaciones";
            dgvHomologaciones.ReadOnly = true;
            dgvHomologaciones.Size = new System.Drawing.Size(660, 300);
            dgvHomologaciones.TabIndex = 1;
            // 
            // btnEliminar
            // 
            btnEliminar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnEliminar.Location = new System.Drawing.Point(12, 320);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new System.Drawing.Size(150, 30);
            btnEliminar.TabIndex = 0;
            btnEliminar.Text = "Eliminar Seleccionada";
            btnEliminar.UseVisualStyleBackColor = true;
            btnEliminar.Click += btnEliminar_Click;
            // 
            // GestionHomologacionesForm
            // 
            ClientSize = new System.Drawing.Size(684, 361);
            Controls.Add(btnEliminar);
            Controls.Add(dgvHomologaciones);
            Name = "GestionHomologacionesForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Gestión de Homologaciones";
            ((System.ComponentModel.ISupportInitialize)dgvHomologaciones.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvHomologaciones).EndInit();
            ResumeLayout(false);
        }

        private Telerik.WinControls.UI.RadGridView dgvHomologaciones;
        private System.Windows.Forms.Button btnEliminar;
    }
}