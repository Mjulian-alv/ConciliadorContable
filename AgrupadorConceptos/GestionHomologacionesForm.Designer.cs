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
            lblPerfil = new System.Windows.Forms.Label();
            cboPerfil = new System.Windows.Forms.ComboBox();
            dgvHomologaciones = new Telerik.WinControls.UI.RadGridView();
            btnNueva = new System.Windows.Forms.Button();
            btnEditar = new System.Windows.Forms.Button();
            btnEliminar = new System.Windows.Forms.Button();
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Entrada al mantenimiento de conceptos
            btnConceptosEstandar = new System.Windows.Forms.Button();
            // Fecha: 07/10/2026 - TAREA: 00073 - Linea: 1 - Quinta acción solicitada, sin agregar otro acceso en el menú principal.
            btnConsultarMovimientos = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)dgvHomologaciones).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvHomologaciones.MasterTemplate).BeginInit();
            SuspendLayout();
            //
            // lblPerfil
            //
            lblPerfil.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            lblPerfil.AutoSize = true;
            lblPerfil.Location = new System.Drawing.Point(12, 17);
            lblPerfil.Name = "lblPerfil";
            lblPerfil.Size = new System.Drawing.Size(40, 15);
            lblPerfil.TabIndex = 0;
            lblPerfil.Text = "Perfil:";
            //
            // cboPerfil
            //
            cboPerfil.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cboPerfil.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboPerfil.FormattingEnabled = true;
            cboPerfil.Location = new System.Drawing.Point(70, 14);
            cboPerfil.Name = "cboPerfil";
            // Fecha: 07/10/2026 - TAREA: 00073 - Linea: 1 - Aprovechar el ancho necesario para los cinco botones.
            cboPerfil.Size = new System.Drawing.Size(802, 23);
            cboPerfil.TabIndex = 1;
            //
            // dgvHomologaciones
            //
            dgvHomologaciones.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dgvHomologaciones.EnableCustomFiltering = true;
            dgvHomologaciones.Location = new System.Drawing.Point(12, 48);
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
            // Fecha: 07/10/2026 - TAREA: 00073 - Linea: 1 - Mantener la grilla alineada con el nuevo ancho de la ventana.
            dgvHomologaciones.Size = new System.Drawing.Size(860, 300);
            dgvHomologaciones.TabIndex = 2;
            //
            // btnNueva
            //
            btnNueva.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnNueva.Location = new System.Drawing.Point(12, 356);
            btnNueva.Name = "btnNueva";
            btnNueva.Size = new System.Drawing.Size(150, 30);
            btnNueva.TabIndex = 3;
            btnNueva.Text = "Nueva";
            btnNueva.UseVisualStyleBackColor = true;
            btnNueva.Click += btnNueva_Click;
            //
            // btnEditar
            //
            btnEditar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnEditar.Location = new System.Drawing.Point(168, 356);
            btnEditar.Name = "btnEditar";
            btnEditar.Size = new System.Drawing.Size(150, 30);
            btnEditar.TabIndex = 4;
            btnEditar.Text = "Editar";
            btnEditar.UseVisualStyleBackColor = true;
            btnEditar.Click += btnEditar_Click;
            //
            // btnEliminar
            //
            btnEliminar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnEliminar.Location = new System.Drawing.Point(324, 356);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new System.Drawing.Size(150, 30);
            btnEliminar.TabIndex = 5;
            btnEliminar.Text = "Eliminar";
            btnEliminar.UseVisualStyleBackColor = true;
            btnEliminar.Click += btnEliminar_Click;
            //
            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Entrada al mantenimiento de conceptos
            // btnConceptosEstandar
            //
            // Fecha: 07/10/2026 - TAREA: 00073 - Linea: 1 - Conservar la cuarta posición al redimensionar.
            btnConceptosEstandar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnConceptosEstandar.Location = new System.Drawing.Point(480, 356);
            btnConceptosEstandar.Name = "btnConceptosEstandar";
            btnConceptosEstandar.Size = new System.Drawing.Size(186, 30);
            btnConceptosEstandar.TabIndex = 6;
            btnConceptosEstandar.Text = "Conceptos Estándar...";
            btnConceptosEstandar.UseVisualStyleBackColor = true;
            btnConceptosEstandar.Click += btnConceptosEstandar_Click;
            //
            // GestionHomologacionesForm
            //
            // Fecha: 07/10/2026 - TAREA: 00073 - Linea: 1 - El quinto botón queda después de Conceptos Estándar y no se superpone al reducir la ventana.
            btnConsultarMovimientos.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnConsultarMovimientos.Location = new System.Drawing.Point(672, 356);
            btnConsultarMovimientos.Name = "btnConsultarMovimientos";
            btnConsultarMovimientos.Size = new System.Drawing.Size(200, 30);
            btnConsultarMovimientos.TabIndex = 7;
            btnConsultarMovimientos.Text = "Consultar movimientos...";
            btnConsultarMovimientos.UseVisualStyleBackColor = true;
            btnConsultarMovimientos.Click += btnConsultarMovimientos_Click;
            ClientSize = new System.Drawing.Size(884, 397);
            Controls.Add(btnConsultarMovimientos);
            Controls.Add(btnConceptosEstandar);
            Controls.Add(btnEliminar);
            Controls.Add(btnEditar);
            Controls.Add(btnNueva);
            Controls.Add(dgvHomologaciones);
            Controls.Add(cboPerfil);
            Controls.Add(lblPerfil);
            // Fecha: 07/10/2026 - TAREA: 00073 - Linea: 1 - Reservar espacio para las cinco acciones.
            MinimumSize = new System.Drawing.Size(900, 436);
            Name = "GestionHomologacionesForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Gestión de Homologaciones";
            ((System.ComponentModel.ISupportInitialize)dgvHomologaciones.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvHomologaciones).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.Label lblPerfil;
        private System.Windows.Forms.ComboBox cboPerfil;
        private Telerik.WinControls.UI.RadGridView dgvHomologaciones;
        private System.Windows.Forms.Button btnNueva;
        private System.Windows.Forms.Button btnEditar;
        private System.Windows.Forms.Button btnEliminar;
        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Entrada al mantenimiento de conceptos
        private System.Windows.Forms.Button btnConceptosEstandar;
        // Fecha: 07/10/2026 - TAREA: 00073 - Linea: 1 - Acceso a la consulta recuperada.
        private System.Windows.Forms.Button btnConsultarMovimientos;
    }
}
