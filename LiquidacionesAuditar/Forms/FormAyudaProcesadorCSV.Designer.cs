namespace LiquidacionesAuditar
{
    partial class FormAyudaProcesadorCSV
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            _rtb = new System.Windows.Forms.RichTextBox();
            _panelBotones = new System.Windows.Forms.Panel();
            _btnCerrar = new System.Windows.Forms.Button();
            _panelBotones.SuspendLayout();
            SuspendLayout();
            // 
            // _rtb
            // 
            _rtb.BackColor = System.Drawing.Color.FromArgb(250, 250, 252);
            _rtb.BorderStyle = System.Windows.Forms.BorderStyle.None;
            _rtb.Dock = System.Windows.Forms.DockStyle.Fill;
            _rtb.Font = new System.Drawing.Font("Segoe UI", 10F);
            _rtb.Location = new System.Drawing.Point(0, 0);
            _rtb.Name = "_rtb";
            _rtb.ReadOnly = true;
            _rtb.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            _rtb.Size = new System.Drawing.Size(760, 560);
            _rtb.TabIndex = 0;
            _rtb.TabStop = false;
            // 
            // _panelBotones
            // 
            _panelBotones.Controls.Add(_btnCerrar);
            _panelBotones.Dock = System.Windows.Forms.DockStyle.Bottom;
            _panelBotones.Location = new System.Drawing.Point(0, 560);
            _panelBotones.Name = "_panelBotones";
            _panelBotones.Padding = new System.Windows.Forms.Padding(0, 6, 12, 6);
            _panelBotones.Size = new System.Drawing.Size(760, 44);
            _panelBotones.TabIndex = 1;
            // 
            // _btnCerrar
            // 
            _btnCerrar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            _btnCerrar.DialogResult = System.Windows.Forms.DialogResult.OK;
            _btnCerrar.Location = new System.Drawing.Point(660, 6);
            _btnCerrar.Name = "_btnCerrar";
            _btnCerrar.Size = new System.Drawing.Size(88, 30);
            _btnCerrar.TabIndex = 0;
            _btnCerrar.Text = "Cerrar";
            _btnCerrar.UseVisualStyleBackColor = true;
            // 
            // FormAyudaProcesadorCSV
            // 
            AcceptButton = _btnCerrar;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(760, 604);
            Controls.Add(_rtb);
            Controls.Add(_panelBotones);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormAyudaProcesadorCSV";
            Padding = new System.Windows.Forms.Padding(12, 12, 12, 0);
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Guía de uso — Procesar / Exportar CSV";
            _panelBotones.ResumeLayout(false);
            ResumeLayout(false);
        }

        private System.Windows.Forms.RichTextBox _rtb;
        private System.Windows.Forms.Panel _panelBotones;
        private System.Windows.Forms.Button _btnCerrar;
    }
}
