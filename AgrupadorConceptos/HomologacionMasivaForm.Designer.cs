namespace AgrupadorConceptos
{
    partial class HomologacionMasivaForm
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
            this.dgvPendientes = new Telerik.WinControls.UI.RadGridView();
            this.lblAyuda = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPendientes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPendientes.MasterTemplate)).BeginInit();
            this.SuspendLayout();
            
            // dgvPendientes
            this.dgvPendientes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvPendientes.Location = new System.Drawing.Point(0, 30);
            this.dgvPendientes.Name = "dgvPendientes";
            this.dgvPendientes.Size = new System.Drawing.Size(800, 420);
            this.dgvPendientes.ReadOnly = true;
            this.dgvPendientes.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            this.dgvPendientes.MasterTemplate.AllowAddNewRow = false;
            this.dgvPendientes.MasterTemplate.AllowDeleteRow = false;
            this.dgvPendientes.MasterTemplate.AllowEditRow = false;
            this.dgvPendientes.MasterTemplate.EnableFiltering = true;

            // lblAyuda
            this.lblAyuda.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblAyuda.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblAyuda.Location = new System.Drawing.Point(0, 0);
            this.lblAyuda.Name = "lblAyuda";
            this.lblAyuda.Padding = new System.Windows.Forms.Padding(10, 5, 0, 0);
            this.lblAyuda.Size = new System.Drawing.Size(800, 30);
            this.lblAyuda.Text = "Doble clic sobre una fila para homologar ese grupo de movimientos.";

            // HomologacionMasivaForm
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.dgvPendientes);
            this.Controls.Add(this.lblAyuda);
            this.Name = "HomologacionMasivaForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Homologación Masiva (Agrupados por Original)";
            ((System.ComponentModel.ISupportInitialize)(this.dgvPendientes.MasterTemplate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPendientes)).EndInit();
            this.ResumeLayout(false);
        }

        private Telerik.WinControls.UI.RadGridView dgvPendientes;
        private System.Windows.Forms.Label lblAyuda;
    }
}