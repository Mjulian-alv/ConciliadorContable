// Fecha: 21/09/2026 - TAREA: 00041 - Linea: 2 - Lista de perfiles PyR (copia de FormPerfilesOffline sin el botón de directivas, que llegan en la etapa siguiente)
namespace ArcaCliente
{
    partial class FormPerfilesPyR
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
            pnlBotones     = new Telerik.WinControls.UI.RadPanel();
            btnNuevo       = new Telerik.WinControls.UI.RadButton();
            btnEditar      = new Telerik.WinControls.UI.RadButton();
            btnEliminar    = new Telerik.WinControls.UI.RadButton();
            btnSeleccionar = new Telerik.WinControls.UI.RadButton();
            btnCancelar    = new Telerik.WinControls.UI.RadButton();
            gridPerfiles   = new Telerik.WinControls.UI.RadGridView();

            ((System.ComponentModel.ISupportInitialize)pnlBotones).BeginInit();
            pnlBotones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)btnNuevo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnEditar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnEliminar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnSeleccionar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnCancelar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridPerfiles).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            SuspendLayout();

            // ── pnlBotones ───────────────────────────────────────────────────────
            pnlBotones.Controls.Add(btnNuevo);
            pnlBotones.Controls.Add(btnEditar);
            pnlBotones.Controls.Add(btnEliminar);
            pnlBotones.Controls.Add(btnSeleccionar);
            pnlBotones.Controls.Add(btnCancelar);
            pnlBotones.Dock     = System.Windows.Forms.DockStyle.Bottom;
            pnlBotones.Name     = "pnlBotones";
            pnlBotones.Size     = new System.Drawing.Size(584, 48);
            pnlBotones.TabIndex = 0;

            btnNuevo.Location  = new System.Drawing.Point(12, 10);
            btnNuevo.Name      = "btnNuevo";
            btnNuevo.Size      = new System.Drawing.Size(100, 28);
            btnNuevo.TabIndex  = 0;
            btnNuevo.Text      = "+ Nuevo";
            btnNuevo.Click    += BtnNuevo_Click;

            btnEditar.Location = new System.Drawing.Point(120, 10);
            btnEditar.Name     = "btnEditar";
            btnEditar.Size     = new System.Drawing.Size(100, 28);
            btnEditar.TabIndex = 1;
            btnEditar.Text     = "Editar...";
            btnEditar.Click   += BtnEditar_Click;

            btnEliminar.Location = new System.Drawing.Point(228, 10);
            btnEliminar.Name     = "btnEliminar";
            btnEliminar.Size     = new System.Drawing.Size(100, 28);
            btnEliminar.TabIndex = 2;
            btnEliminar.Text     = "Eliminar";
            btnEliminar.Click   += BtnEliminar_Click;

            btnSeleccionar.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnSeleccionar.Location = new System.Drawing.Point(344, 10);
            btnSeleccionar.Name     = "btnSeleccionar";
            btnSeleccionar.Size     = new System.Drawing.Size(120, 28);
            btnSeleccionar.TabIndex = 3;
            btnSeleccionar.Text     = "Usar perfil →";
            btnSeleccionar.Click   += BtnSeleccionar_Click;

            btnCancelar.Location = new System.Drawing.Point(472, 10);
            btnCancelar.Name     = "btnCancelar";
            btnCancelar.Size     = new System.Drawing.Size(100, 28);
            btnCancelar.TabIndex = 4;
            btnCancelar.Text     = "Cancelar";
            btnCancelar.Click   += BtnCancelar_Click;

            // ── gridPerfiles ─────────────────────────────────────────────────────
            gridPerfiles.Dock     = System.Windows.Forms.DockStyle.Fill;
            gridPerfiles.Name     = "gridPerfiles";
            gridPerfiles.TabIndex = 1;
            gridPerfiles.MasterTemplate.AllowAddNewRow = false;
            gridPerfiles.MasterTemplate.AllowDeleteRow = false;
            gridPerfiles.MasterTemplate.AllowEditRow   = false;
            gridPerfiles.CellDoubleClick += GridPerfiles_CellDoubleClick;

            // ── Form ─────────────────────────────────────────────────────────────
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize          = new System.Drawing.Size(584, 400);
            Controls.Add(gridPerfiles);
            Controls.Add(pnlBotones);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            Name            = "FormPerfilesPyR";
            StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            Text            = "Perfiles de Percepciones y Retenciones";

            ((System.ComponentModel.ISupportInitialize)pnlBotones).EndInit();
            pnlBotones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)btnNuevo).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnEditar).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnEliminar).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnSeleccionar).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnCancelar).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridPerfiles).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        private Telerik.WinControls.UI.RadPanel      pnlBotones;
        private Telerik.WinControls.UI.RadButton     btnNuevo;
        private Telerik.WinControls.UI.RadButton     btnEditar;
        private Telerik.WinControls.UI.RadButton     btnEliminar;
        private Telerik.WinControls.UI.RadButton     btnSeleccionar;
        private Telerik.WinControls.UI.RadButton     btnCancelar;
        private Telerik.WinControls.UI.RadGridView   gridPerfiles;
    }
}
