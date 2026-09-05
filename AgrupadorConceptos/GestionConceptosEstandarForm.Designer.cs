namespace AgrupadorConceptos
{
    partial class GestionConceptosEstandarForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Layout de la grilla y el boton de asignacion
        private void InitializeComponent()
        {
            Telerik.WinControls.UI.TableViewDefinition tableViewDefinition1 = new Telerik.WinControls.UI.TableViewDefinition();
            dgvConceptos = new Telerik.WinControls.UI.RadGridView();
            btnAsignarCuenta = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)dgvConceptos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvConceptos.MasterTemplate).BeginInit();
            SuspendLayout();
            //
            // dgvConceptos
            //
            dgvConceptos.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dgvConceptos.Location = new System.Drawing.Point(12, 12);
            dgvConceptos.MasterTemplate.AllowAddNewRow = false;
            dgvConceptos.MasterTemplate.AllowDeleteRow = false;
            dgvConceptos.MasterTemplate.AllowEditRow = false;
            dgvConceptos.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            dgvConceptos.MasterTemplate.ViewDefinition = tableViewDefinition1;
            dgvConceptos.Name = "dgvConceptos";
            dgvConceptos.ReadOnly = true;
            dgvConceptos.Size = new System.Drawing.Size(600, 300);
            dgvConceptos.TabIndex = 0;
            dgvConceptos.DoubleClick += dgvConceptos_DoubleClick;
            //
            // btnAsignarCuenta
            //
            btnAsignarCuenta.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnAsignarCuenta.Location = new System.Drawing.Point(12, 320);
            btnAsignarCuenta.Size = new System.Drawing.Size(200, 30);
            btnAsignarCuenta.Text = "Asignar cuenta...";
            btnAsignarCuenta.UseVisualStyleBackColor = true;
            btnAsignarCuenta.Click += btnAsignarCuenta_Click;
            //
            // GestionConceptosEstandarForm
            //
            ClientSize = new System.Drawing.Size(624, 362);
            Controls.Add(btnAsignarCuenta);
            Controls.Add(dgvConceptos);
            MinimumSize = new System.Drawing.Size(640, 400);
            Name = "GestionConceptosEstandarForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Gestión de Conceptos Estándar";
            ((System.ComponentModel.ISupportInitialize)dgvConceptos.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvConceptos).EndInit();
            ResumeLayout(false);
        }

        private Telerik.WinControls.UI.RadGridView dgvConceptos;
        private System.Windows.Forms.Button btnAsignarCuenta;
    }
}
