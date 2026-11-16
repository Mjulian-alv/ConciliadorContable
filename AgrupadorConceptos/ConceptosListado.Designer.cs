using System.Drawing;
using System.Windows.Forms;
using Telerik.WinControls.UI;

namespace AgrupadorConceptos
{
    partial class ConceptosListado
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing) components?.Dispose();
            base.Dispose(disposing);
        }

        // Fecha: 07/10/2026 - TAREA: 00073 - Linea: 1, 2, 3 - Completar la ventana existente con selección múltiple y una grilla de consulta sin edición.
        private void InitializeComponent()
        {
            TableViewDefinition tableViewDefinition1 = new TableViewDefinition();
            pnlFiltros = new FlowLayoutPanel();
            lblDesde = new Label();
            dtpDesde = new DateTimePicker();
            lblHasta = new Label();
            dtpHasta = new DateTimePicker();
            btnConsultar = new Button();
            btnExportar = new Button();
            pnlConceptos = new Panel();
            clbConceptos = new CheckedListBox();
            lblConceptos = new Label();
            pnlSeleccion = new FlowLayoutPanel();
            btnTodos = new Button();
            btnNinguno = new Button();
            dgvMovimientos = new RadGridView();
            lblEstado = new Label();
            lblFechas = new Label();
            pnlFiltros.SuspendLayout();
            pnlConceptos.SuspendLayout();
            pnlSeleccion.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMovimientos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvMovimientos.MasterTemplate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            SuspendLayout();
            // 
            // pnlFiltros
            // 
            pnlFiltros.Controls.Add(lblDesde);
            pnlFiltros.Controls.Add(dtpDesde);
            pnlFiltros.Controls.Add(lblHasta);
            pnlFiltros.Controls.Add(dtpHasta);
            pnlFiltros.Controls.Add(btnConsultar);
            pnlFiltros.Controls.Add(btnExportar);
            pnlFiltros.Dock = DockStyle.Top;
            pnlFiltros.Location = new Point(0, 0);
            pnlFiltros.Name = "pnlFiltros";
            pnlFiltros.Padding = new Padding(10);
            pnlFiltros.Size = new Size(1250, 54);
            pnlFiltros.TabIndex = 2;
            pnlFiltros.WrapContents = false;
            // 
            // lblDesde
            // 
            lblDesde.AutoSize = true;
            lblDesde.Location = new Point(10, 17);
            lblDesde.Margin = new Padding(0, 7, 5, 0);
            lblDesde.Name = "lblDesde";
            lblDesde.Size = new Size(42, 15);
            lblDesde.TabIndex = 0;
            lblDesde.Text = "Desde:";
            // 
            // dtpDesde
            // 
            dtpDesde.CustomFormat = "dd/MM/yyyy";
            dtpDesde.Format = DateTimePickerFormat.Custom;
            dtpDesde.Location = new Point(60, 13);
            dtpDesde.Name = "dtpDesde";
            dtpDesde.Size = new Size(120, 23);
            dtpDesde.TabIndex = 1;
            // 
            // lblHasta
            // 
            lblHasta.AutoSize = true;
            lblHasta.Location = new Point(197, 17);
            lblHasta.Margin = new Padding(14, 7, 5, 0);
            lblHasta.Name = "lblHasta";
            lblHasta.Size = new Size(40, 15);
            lblHasta.TabIndex = 2;
            lblHasta.Text = "Hasta:";
            // 
            // dtpHasta
            // 
            dtpHasta.CustomFormat = "dd/MM/yyyy";
            dtpHasta.Format = DateTimePickerFormat.Custom;
            dtpHasta.Location = new Point(245, 13);
            dtpHasta.Name = "dtpHasta";
            dtpHasta.Size = new Size(120, 23);
            dtpHasta.TabIndex = 3;
            // 
            // btnConsultar
            // 
            btnConsultar.Location = new Point(388, 10);
            btnConsultar.Margin = new Padding(20, 0, 6, 0);
            btnConsultar.Name = "btnConsultar";
            btnConsultar.Size = new Size(110, 30);
            btnConsultar.TabIndex = 4;
            btnConsultar.Text = "Consultar";
            // 
            // btnExportar
            // 
            btnExportar.Enabled = false;
            btnExportar.Location = new Point(504, 10);
            btnExportar.Margin = new Padding(0);
            btnExportar.Name = "btnExportar";
            btnExportar.Size = new Size(140, 30);
            btnExportar.TabIndex = 5;
            btnExportar.Text = "Exportar a Excel";
            // 
            // pnlConceptos
            // 
            pnlConceptos.Controls.Add(clbConceptos);
            pnlConceptos.Controls.Add(lblConceptos);
            pnlConceptos.Controls.Add(pnlSeleccion);
            pnlConceptos.Dock = DockStyle.Left;
            pnlConceptos.Location = new Point(0, 54);
            pnlConceptos.Name = "pnlConceptos";
            pnlConceptos.Padding = new Padding(10, 0, 10, 10);
            pnlConceptos.Size = new Size(280, 534);
            pnlConceptos.TabIndex = 1;
            // 
            // clbConceptos
            // 
            clbConceptos.CheckOnClick = true;
            clbConceptos.Dock = DockStyle.Fill;
            clbConceptos.HorizontalScrollbar = true;
            clbConceptos.IntegralHeight = false;
            clbConceptos.Location = new Point(10, 30);
            clbConceptos.Name = "clbConceptos";
            clbConceptos.Size = new Size(260, 456);
            clbConceptos.TabIndex = 0;
            // 
            // lblConceptos
            // 
            lblConceptos.Dock = DockStyle.Top;
            lblConceptos.Location = new Point(10, 0);
            lblConceptos.Name = "lblConceptos";
            lblConceptos.Size = new Size(260, 30);
            lblConceptos.TabIndex = 1;
            lblConceptos.Text = "Conceptos finales (selección múltiple)";
            lblConceptos.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlSeleccion
            // 
            pnlSeleccion.Controls.Add(btnTodos);
            pnlSeleccion.Controls.Add(btnNinguno);
            pnlSeleccion.Dock = DockStyle.Bottom;
            pnlSeleccion.Location = new Point(10, 486);
            pnlSeleccion.Name = "pnlSeleccion";
            pnlSeleccion.Size = new Size(260, 38);
            pnlSeleccion.TabIndex = 2;
            // 
            // btnTodos
            // 
            btnTodos.Location = new Point(3, 3);
            btnTodos.Name = "btnTodos";
            btnTodos.Size = new Size(122, 30);
            btnTodos.TabIndex = 0;
            btnTodos.Text = "Marcar todos";
            // 
            // btnNinguno
            // 
            btnNinguno.Location = new Point(131, 3);
            btnNinguno.Name = "btnNinguno";
            btnNinguno.Size = new Size(122, 30);
            btnNinguno.TabIndex = 1;
            btnNinguno.Text = "Desmarcar todos";
            // 
            // dgvMovimientos
            // 
            dgvMovimientos.Dock = DockStyle.Fill;
            dgvMovimientos.Location = new Point(280, 54);
            // 
            // 
            // 
            dgvMovimientos.MasterTemplate.AllowAddNewRow = false;
            dgvMovimientos.MasterTemplate.AllowDeleteRow = false;
            dgvMovimientos.MasterTemplate.AllowEditRow = false;
            dgvMovimientos.MasterTemplate.EnableGrouping = false;
            dgvMovimientos.MasterTemplate.ViewDefinition = tableViewDefinition1;
            dgvMovimientos.Name = "dgvMovimientos";
            dgvMovimientos.ReadOnly = true;
            dgvMovimientos.ShowGroupPanel = false;
            dgvMovimientos.Size = new Size(970, 534);
            dgvMovimientos.TabIndex = 0;
            // 
            // lblEstado
            // 
            lblEstado.Dock = DockStyle.Bottom;
            lblEstado.Location = new Point(0, 616);
            lblEstado.Name = "lblEstado";
            lblEstado.Padding = new Padding(10, 0, 0, 0);
            lblEstado.Size = new Size(1250, 34);
            lblEstado.TabIndex = 4;
            lblEstado.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblFechas
            // 
            lblFechas.Dock = DockStyle.Bottom;
            lblFechas.ForeColor = Color.DimGray;
            lblFechas.Location = new Point(0, 588);
            lblFechas.Name = "lblFechas";
            lblFechas.Padding = new Padding(10, 0, 0, 0);
            lblFechas.Size = new Size(1250, 28);
            lblFechas.TabIndex = 3;
            lblFechas.Text = "Rango inclusivo por fecha del movimiento.";
            // 
            // ConceptosListado
            // 
            AutoScaleBaseSize = new Size(7, 15);
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1250, 650);
            Controls.Add(dgvMovimientos);
            Controls.Add(pnlConceptos);
            Controls.Add(pnlFiltros);
            Controls.Add(lblFechas);
            Controls.Add(lblEstado);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(1000, 500);
            Name = "ConceptosListado";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Consulta de movimientos por conceptos finales";
            pnlFiltros.ResumeLayout(false);
            pnlFiltros.PerformLayout();
            pnlConceptos.ResumeLayout(false);
            pnlSeleccion.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvMovimientos.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvMovimientos).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        private FlowLayoutPanel pnlFiltros;
        private DateTimePicker dtpDesde;
        private DateTimePicker dtpHasta;
        private Button btnConsultar;
        private Button btnExportar;
        private Panel pnlConceptos;
        private CheckedListBox clbConceptos;
        private Button btnTodos;
        private Button btnNinguno;
        private RadGridView dgvMovimientos;
        private Label lblEstado;
        private Label lblDesde;
        private Label lblHasta;
        private Label lblConceptos;
        private FlowLayoutPanel pnlSeleccion;
        private Label lblFechas;
    }
}
