namespace LiquidacionesAuditar
{
    partial class FormFiltroLiquidacion
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            _tlpMain = new System.Windows.Forms.TableLayoutPanel();
            _lblColumna = new System.Windows.Forms.Label();
            _lblColumnaVal = new System.Windows.Forms.Label();
            _lblTipo = new System.Windows.Forms.Label();
            _lblTipoVal = new System.Windows.Forms.Label();
            _pnlCampos = new System.Windows.Forms.Panel();
            _flpBotones = new System.Windows.Forms.FlowLayoutPanel();
            _btnAceptar = new System.Windows.Forms.Button();
            _btnLimpiar = new System.Windows.Forms.Button();
            _btnCancelar = new System.Windows.Forms.Button();
            _tlpMain.SuspendLayout();
            _flpBotones.SuspendLayout();
            SuspendLayout();
            // 
            // _tlpMain
            // 
            _tlpMain.ColumnCount = 2;
            _tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            _tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            _tlpMain.Controls.Add(_lblColumna, 0, 0);
            _tlpMain.Controls.Add(_lblColumnaVal, 1, 0);
            _tlpMain.Controls.Add(_lblTipo, 0, 1);
            _tlpMain.Controls.Add(_lblTipoVal, 1, 1);
            _tlpMain.Controls.Add(_pnlCampos, 0, 2);
            _tlpMain.Controls.Add(_flpBotones, 0, 3);
            _tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            _tlpMain.Location = new System.Drawing.Point(10, 10);
            _tlpMain.Name = "_tlpMain";
            _tlpMain.Padding = new System.Windows.Forms.Padding(4);
            _tlpMain.RowCount = 4;
            _tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            _tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            _tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            _tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            _tlpMain.SetColumnSpan(_pnlCampos, 2);
            _tlpMain.SetColumnSpan(_flpBotones, 2);
            _tlpMain.TabIndex = 0;
            // 
            // _lblColumna
            // 
            _lblColumna.AutoSize = true;
            _lblColumna.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            _lblColumna.Location = new System.Drawing.Point(7, 7);
            _lblColumna.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
            _lblColumna.Name = "_lblColumna";
            _lblColumna.Size = new System.Drawing.Size(60, 15);
            _lblColumna.TabIndex = 0;
            _lblColumna.Text = "Columna:";
            // 
            // _lblColumnaVal
            // 
            _lblColumnaVal.AutoSize = true;
            _lblColumnaVal.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _lblColumnaVal.Location = new System.Drawing.Point(76, 7);
            _lblColumnaVal.Name = "_lblColumnaVal";
            _lblColumnaVal.Size = new System.Drawing.Size(200, 15);
            _lblColumnaVal.TabIndex = 1;
            _lblColumnaVal.Text = "";
            // 
            // _lblTipo
            // 
            _lblTipo.AutoSize = true;
            _lblTipo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            _lblTipo.Location = new System.Drawing.Point(7, 30);
            _lblTipo.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
            _lblTipo.Name = "_lblTipo";
            _lblTipo.Size = new System.Drawing.Size(53, 15);
            _lblTipo.TabIndex = 2;
            _lblTipo.Text = "Tipo:";
            // 
            // _lblTipoVal
            // 
            _lblTipoVal.AutoSize = true;
            _lblTipoVal.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _lblTipoVal.Location = new System.Drawing.Point(76, 30);
            _lblTipoVal.Name = "_lblTipoVal";
            _lblTipoVal.Size = new System.Drawing.Size(200, 15);
            _lblTipoVal.TabIndex = 3;
            _lblTipoVal.Text = "";
            // 
            // _pnlCampos
            // 
            _pnlCampos.Dock = System.Windows.Forms.DockStyle.Fill;
            _pnlCampos.Location = new System.Drawing.Point(7, 58);
            _pnlCampos.Name = "_pnlCampos";
            _pnlCampos.Padding = new System.Windows.Forms.Padding(0, 6, 0, 6);
            _pnlCampos.Size = new System.Drawing.Size(350, 80);
            _pnlCampos.TabIndex = 4;
            // 
            // _flpBotones
            // 
            _flpBotones.Controls.Add(_btnCancelar);
            _flpBotones.Controls.Add(_btnLimpiar);
            _flpBotones.Controls.Add(_btnAceptar);
            _flpBotones.Dock = System.Windows.Forms.DockStyle.Fill;
            _flpBotones.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            _flpBotones.Location = new System.Drawing.Point(7, 147);
            _flpBotones.Name = "_flpBotones";
            _flpBotones.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            _flpBotones.Size = new System.Drawing.Size(350, 36);
            _flpBotones.TabIndex = 5;
            _flpBotones.WrapContents = false;
            // 
            // _btnAceptar
            // 
            _btnAceptar.DialogResult = System.Windows.Forms.DialogResult.OK;
            _btnAceptar.Location = new System.Drawing.Point(270, 4);
            _btnAceptar.Name = "_btnAceptar";
            _btnAceptar.Size = new System.Drawing.Size(80, 28);
            _btnAceptar.TabIndex = 0;
            _btnAceptar.Text = "Aplicar";
            _btnAceptar.UseVisualStyleBackColor = true;
            _btnAceptar.Click += BtnAceptar_Click;
            // 
            // _btnLimpiar
            // 
            _btnLimpiar.Location = new System.Drawing.Point(184, 4);
            _btnLimpiar.Name = "_btnLimpiar";
            _btnLimpiar.Size = new System.Drawing.Size(80, 28);
            _btnLimpiar.TabIndex = 1;
            _btnLimpiar.Text = "Limpiar";
            _btnLimpiar.UseVisualStyleBackColor = true;
            _btnLimpiar.Click += BtnLimpiar_Click;
            // 
            // _btnCancelar
            // 
            _btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            _btnCancelar.Location = new System.Drawing.Point(98, 4);
            _btnCancelar.Name = "_btnCancelar";
            _btnCancelar.Size = new System.Drawing.Size(80, 28);
            _btnCancelar.TabIndex = 2;
            _btnCancelar.Text = "Cancelar";
            _btnCancelar.UseVisualStyleBackColor = true;
            // 
            // FormFiltroLiquidacion
            // 
            AcceptButton = _btnAceptar;
            CancelButton = _btnCancelar;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(380, 210);
            Controls.Add(_tlpMain);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormFiltroLiquidacion";
            Padding = new System.Windows.Forms.Padding(10);
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Definir filtro de liquidación";
            _tlpMain.ResumeLayout(false);
            _tlpMain.PerformLayout();
            _flpBotones.ResumeLayout(false);
            ResumeLayout(false);
        }

        private System.Windows.Forms.TableLayoutPanel _tlpMain;
        private System.Windows.Forms.Label _lblColumna;
        private System.Windows.Forms.Label _lblColumnaVal;
        private System.Windows.Forms.Label _lblTipo;
        private System.Windows.Forms.Label _lblTipoVal;
        private System.Windows.Forms.Panel _pnlCampos;
        private System.Windows.Forms.FlowLayoutPanel _flpBotones;
        private System.Windows.Forms.Button _btnAceptar;
        private System.Windows.Forms.Button _btnLimpiar;
        private System.Windows.Forms.Button _btnCancelar;
    }
}
