namespace AgrupadorConceptos
{
    partial class CuentasContablesForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            Telerik.WinControls.UI.TableViewDefinition tableViewDefinition1 = new Telerik.WinControls.UI.TableViewDefinition();
            dgvCuentas = new Telerik.WinControls.UI.RadGridView();
            btnImportar = new System.Windows.Forms.Button();
            pnlImportar = new System.Windows.Forms.Panel();
            lblArchivo = new System.Windows.Forms.Label();
            txtArchivo = new System.Windows.Forms.TextBox();
            btnSeleccionarArchivo = new System.Windows.Forms.Button();
            lblColCuenta = new System.Windows.Forms.Label();
            cmbColCuenta = new System.Windows.Forms.ComboBox();
            lblColDescripcion = new System.Windows.Forms.Label();
            cmbColDescripcion = new System.Windows.Forms.ComboBox();
            lblColCentroCosto = new System.Windows.Forms.Label();
            cmbColCentroCosto = new System.Windows.Forms.ComboBox();
            dgvPreview = new Telerik.WinControls.UI.RadGridView();
            btnConfirmarImportar = new System.Windows.Forms.Button();
            btnCancelarImportar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)dgvCuentas).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvCuentas.MasterTemplate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvPreview).BeginInit();
            SuspendLayout();
            //
            // dgvCuentas
            //
            dgvCuentas.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dgvCuentas.Location = new System.Drawing.Point(12, 12);
            dgvCuentas.MasterTemplate.AllowAddNewRow = false;
            dgvCuentas.MasterTemplate.AllowDeleteRow = false;
            dgvCuentas.MasterTemplate.AllowEditRow = false;
            dgvCuentas.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            dgvCuentas.MasterTemplate.ViewDefinition = tableViewDefinition1;
            dgvCuentas.Name = "dgvCuentas";
            dgvCuentas.ReadOnly = true;
            dgvCuentas.Size = new System.Drawing.Size(660, 300);
            dgvCuentas.TabIndex = 0;
            //
            // btnImportar
            //
            btnImportar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnImportar.Location = new System.Drawing.Point(12, 320);
            btnImportar.Name = "btnImportar";
            btnImportar.Size = new System.Drawing.Size(200, 30);
            btnImportar.TabIndex = 1;
            btnImportar.Text = "Importar desde Excel/CSV...";
            btnImportar.UseVisualStyleBackColor = true;
            btnImportar.Click += btnImportar_Click;
            //
            // pnlImportar
            //
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 2 - Alto ajustado a 358: con el ClientSize
            // original del plan (362) el panel se pasaba 8px del borde inferior de la ventana.
            pnlImportar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pnlImportar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pnlImportar.Location = new System.Drawing.Point(12, 12);
            pnlImportar.Size = new System.Drawing.Size(660, 358);
            pnlImportar.Visible = false;
            pnlImportar.Controls.Add(lblArchivo);
            pnlImportar.Controls.Add(txtArchivo);
            pnlImportar.Controls.Add(btnSeleccionarArchivo);
            pnlImportar.Controls.Add(lblColCuenta);
            pnlImportar.Controls.Add(cmbColCuenta);
            pnlImportar.Controls.Add(lblColDescripcion);
            pnlImportar.Controls.Add(cmbColDescripcion);
            pnlImportar.Controls.Add(lblColCentroCosto);
            pnlImportar.Controls.Add(cmbColCentroCosto);
            pnlImportar.Controls.Add(dgvPreview);
            pnlImportar.Controls.Add(btnConfirmarImportar);
            pnlImportar.Controls.Add(btnCancelarImportar);
            //
            // lblArchivo
            //
            lblArchivo.AutoSize = true;
            lblArchivo.Location = new System.Drawing.Point(12, 15);
            lblArchivo.Text = "Archivo:";
            //
            // txtArchivo
            //
            txtArchivo.Location = new System.Drawing.Point(90, 12);
            txtArchivo.ReadOnly = true;
            txtArchivo.Size = new System.Drawing.Size(430, 23);
            //
            // btnSeleccionarArchivo
            //
            btnSeleccionarArchivo.Location = new System.Drawing.Point(526, 11);
            btnSeleccionarArchivo.Size = new System.Drawing.Size(120, 25);
            btnSeleccionarArchivo.Text = "Examinar...";
            btnSeleccionarArchivo.UseVisualStyleBackColor = true;
            btnSeleccionarArchivo.Click += btnSeleccionarArchivo_Click;
            //
            // lblColCuenta
            //
            lblColCuenta.AutoSize = true;
            lblColCuenta.Location = new System.Drawing.Point(12, 50);
            lblColCuenta.Text = "Columna → Cuenta:";
            //
            // cmbColCuenta
            //
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 2 - Combos corridos a x=230: con el x=150
            // del plan, la etiqueta de "Columna → Descripción:" (la más larga de las tres) se
            // solapaba con el combo.
            cmbColCuenta.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbColCuenta.Location = new System.Drawing.Point(230, 47);
            cmbColCuenta.Size = new System.Drawing.Size(200, 23);
            //
            // lblColDescripcion
            //
            lblColDescripcion.AutoSize = true;
            lblColDescripcion.Location = new System.Drawing.Point(12, 80);
            lblColDescripcion.Text = "Columna → Descripción:";
            //
            // cmbColDescripcion
            //
            cmbColDescripcion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbColDescripcion.Location = new System.Drawing.Point(230, 77);
            cmbColDescripcion.Size = new System.Drawing.Size(200, 23);
            //
            // lblColCentroCosto
            //
            lblColCentroCosto.AutoSize = true;
            lblColCentroCosto.Location = new System.Drawing.Point(12, 110);
            lblColCentroCosto.Text = "Columna → Centro de costo:";
            //
            // cmbColCentroCosto
            //
            cmbColCentroCosto.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbColCentroCosto.Location = new System.Drawing.Point(230, 107);
            cmbColCentroCosto.Size = new System.Drawing.Size(200, 23);
            //
            // dgvPreview
            //
            dgvPreview.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dgvPreview.Location = new System.Drawing.Point(12, 140);
            dgvPreview.MasterTemplate.AllowAddNewRow = false;
            dgvPreview.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            dgvPreview.Name = "dgvPreview";
            dgvPreview.ReadOnly = true;
            dgvPreview.Size = new System.Drawing.Size(634, 168);
            //
            // btnConfirmarImportar
            //
            btnConfirmarImportar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnConfirmarImportar.Location = new System.Drawing.Point(480, 318);
            btnConfirmarImportar.Size = new System.Drawing.Size(166, 30);
            btnConfirmarImportar.Text = "Confirmar importación";
            btnConfirmarImportar.UseVisualStyleBackColor = true;
            btnConfirmarImportar.Click += btnConfirmarImportar_Click;
            //
            // btnCancelarImportar
            //
            btnCancelarImportar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnCancelarImportar.Location = new System.Drawing.Point(374, 318);
            btnCancelarImportar.Size = new System.Drawing.Size(100, 30);
            btnCancelarImportar.Text = "Cancelar";
            btnCancelarImportar.UseVisualStyleBackColor = true;
            btnCancelarImportar.Click += btnCancelarImportar_Click;
            //
            // CuentasContablesForm
            //
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 2 - ClientSize a 684x370 (era 684x362 en el
            // plan): pnlImportar (Location 12,12 + Size 660x358) llega hasta y=370, y con 362 de
            // alto la ventana lo recortaba 8px. MinimumSize sube en la misma medida.
            ClientSize = new System.Drawing.Size(684, 370);
            Controls.Add(pnlImportar);
            Controls.Add(btnImportar);
            Controls.Add(dgvCuentas);
            MinimumSize = new System.Drawing.Size(700, 408);
            Name = "CuentasContablesForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Cuentas Contables";
            ((System.ComponentModel.ISupportInitialize)dgvCuentas.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvCuentas).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvPreview).EndInit();
            ResumeLayout(false);
        }

        private Telerik.WinControls.UI.RadGridView dgvCuentas;
        private System.Windows.Forms.Button btnImportar;
        private System.Windows.Forms.Panel pnlImportar;
        private System.Windows.Forms.Label lblArchivo;
        private System.Windows.Forms.TextBox txtArchivo;
        private System.Windows.Forms.Button btnSeleccionarArchivo;
        private System.Windows.Forms.Label lblColCuenta;
        private System.Windows.Forms.ComboBox cmbColCuenta;
        private System.Windows.Forms.Label lblColDescripcion;
        private System.Windows.Forms.ComboBox cmbColDescripcion;
        private System.Windows.Forms.Label lblColCentroCosto;
        private System.Windows.Forms.ComboBox cmbColCentroCosto;
        private Telerik.WinControls.UI.RadGridView dgvPreview;
        private System.Windows.Forms.Button btnConfirmarImportar;
        private System.Windows.Forms.Button btnCancelarImportar;
    }
}
