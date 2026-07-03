namespace ArcaCliente
{
    partial class FormConciliacionXCuit
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
            _lblResumen  = new Telerik.WinControls.UI.RadLabel();
            _gridResumen = new Telerik.WinControls.UI.RadGridView();
            _lblHint     = new Telerik.WinControls.UI.RadLabel();
            ((System.ComponentModel.ISupportInitialize)_lblResumen).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_gridResumen).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_gridResumen.MasterTemplate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_lblHint).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            SuspendLayout();
            // 
            // _lblResumen
            // 
            _lblResumen.AutoSize = false;
            _lblResumen.Dock     = System.Windows.Forms.DockStyle.Top;
            _lblResumen.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            _lblResumen.Name     = "_lblResumen";
            _lblResumen.Padding  = new System.Windows.Forms.Padding(6, 4, 0, 0);
            _lblResumen.Size     = new System.Drawing.Size(960, 28);
            _lblResumen.TabIndex = 0;
            // 
            // _gridResumen
            // 
            _gridResumen.Dock     = System.Windows.Forms.DockStyle.Fill;
            _gridResumen.Location = new System.Drawing.Point(0, 28);
            // 
            // 
            // 
            _gridResumen.MasterTemplate.AllowAddNewRow  = false;
            _gridResumen.MasterTemplate.AllowDeleteRow  = false;
            _gridResumen.MasterTemplate.AllowEditRow    = false;
            _gridResumen.MasterTemplate.EnableFiltering = true;
            _gridResumen.MasterTemplate.ViewDefinition  = tableViewDefinition1;
            _gridResumen.Name     = "_gridResumen";
            _gridResumen.Size     = new System.Drawing.Size(960, 510);
            _gridResumen.TabIndex = 1;
            _gridResumen.CellDoubleClick += GridResumen_CellDoubleClick;
            _gridResumen.RowFormatting   += GridResumen_RowFormatting;
            // 
            // _lblHint
            // 
            _lblHint.AutoSize  = false;
            _lblHint.Dock      = System.Windows.Forms.DockStyle.Bottom;
            _lblHint.ForeColor = System.Drawing.Color.Gray;
            _lblHint.Name      = "_lblHint";
            _lblHint.Padding   = new System.Windows.Forms.Padding(6, 2, 0, 0);
            _lblHint.Size      = new System.Drawing.Size(960, 22);
            _lblHint.TabIndex  = 2;
            _lblHint.Text      = "Doble clic en un CUIT para ver el detalle comprobante a comprobante";
            // 
            // FormConciliacionXCuit
            // 
            AutoScaleBaseSize   = new System.Drawing.Size(7, 15);
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize          = new System.Drawing.Size(960, 560);
            Controls.Add(_gridResumen);
            Controls.Add(_lblHint);
            Controls.Add(_lblResumen);
            MinimumSize   = new System.Drawing.Size(820, 460);
            Name          = "FormConciliacionXCuit";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text          = "Conciliaci\u00f3n por totales x CUIT";
            ((System.ComponentModel.ISupportInitialize)_lblResumen).EndInit();
            ((System.ComponentModel.ISupportInitialize)_gridResumen.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)_gridResumen).EndInit();
            ((System.ComponentModel.ISupportInitialize)_lblHint).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        // ?? Field declarations ????????????????????????????????????????????????
        private Telerik.WinControls.UI.RadLabel    _lblResumen;
        private Telerik.WinControls.UI.RadGridView _gridResumen;
        private Telerik.WinControls.UI.RadLabel    _lblHint;
    }
}
