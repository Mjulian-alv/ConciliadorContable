namespace ArcaCliente
{
    partial class FormDetalleItemConciliacion
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
            pnlEstado = new Telerik.WinControls.UI.RadPanel();
            lblEstadoItem = new Telerik.WinControls.UI.RadLabel();
            pnlAcciones = new System.Windows.Forms.Panel();
            btnCerrar = new Telerik.WinControls.UI.RadButton();
            pnlContenido = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)pnlEstado).BeginInit();
            pnlEstado.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lblEstadoItem).BeginInit();
            pnlAcciones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)btnCerrar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            SuspendLayout();
            // 
            // pnlEstado
            // 
            pnlEstado.Controls.Add(lblEstadoItem);
            pnlEstado.Dock = System.Windows.Forms.DockStyle.Top;
            pnlEstado.Location = new System.Drawing.Point(0, 0);
            pnlEstado.Name = "pnlEstado";
            pnlEstado.Size = new System.Drawing.Size(620, 34);
            pnlEstado.TabIndex = 0;
            // 
            // lblEstadoItem
            // 
            lblEstadoItem.Dock = System.Windows.Forms.DockStyle.Fill;
            lblEstadoItem.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            lblEstadoItem.Location = new System.Drawing.Point(0, 0);
            lblEstadoItem.Name = "lblEstadoItem";
            lblEstadoItem.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            lblEstadoItem.Size = new System.Drawing.Size(12, 2);
            lblEstadoItem.TabIndex = 0;
            // 
            // pnlAcciones
            // 
            pnlAcciones.Controls.Add(btnCerrar);
            pnlAcciones.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlAcciones.Location = new System.Drawing.Point(0, 494);
            pnlAcciones.Name = "pnlAcciones";
            pnlAcciones.Padding = new System.Windows.Forms.Padding(0, 0, 12, 0);
            pnlAcciones.Size = new System.Drawing.Size(620, 46);
            pnlAcciones.TabIndex = 2;
            // 
            // btnCerrar
            // 
            btnCerrar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnCerrar.Location = new System.Drawing.Point(926, 9);
            btnCerrar.Name = "btnCerrar";
            btnCerrar.Size = new System.Drawing.Size(90, 28);
            btnCerrar.TabIndex = 0;
            btnCerrar.Text = "Cerrar";
            btnCerrar.Click += BtnCerrar_Click;
            // 
            // pnlContenido
            // 
            pnlContenido.AutoScroll = true;
            pnlContenido.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlContenido.Location = new System.Drawing.Point(0, 34);
            pnlContenido.Name = "pnlContenido";
            pnlContenido.Padding = new System.Windows.Forms.Padding(8, 8, 8, 0);
            pnlContenido.Size = new System.Drawing.Size(620, 460);
            pnlContenido.TabIndex = 1;
            // 
            // FormDetalleItemConciliacion
            // 
            AutoScaleBaseSize = new System.Drawing.Size(7, 15);
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(620, 540);
            Controls.Add(pnlContenido);
            Controls.Add(pnlAcciones);
            Controls.Add(pnlEstado);
            MinimumSize = new System.Drawing.Size(520, 400);
            Name = "FormDetalleItemConciliacion";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Detalle de comprobante";
            ((System.ComponentModel.ISupportInitialize)pnlEstado).EndInit();
            pnlEstado.ResumeLayout(false);
            pnlEstado.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)lblEstadoItem).EndInit();
            pnlAcciones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)btnCerrar).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        // ── Field declarations ────────────────────────────────────────────────
        private Telerik.WinControls.UI.RadPanel  pnlEstado;
        private Telerik.WinControls.UI.RadLabel  lblEstadoItem;
        private System.Windows.Forms.Panel       pnlAcciones;
        private Telerik.WinControls.UI.RadButton btnCerrar;
        private System.Windows.Forms.Panel       pnlContenido;
    }
}
