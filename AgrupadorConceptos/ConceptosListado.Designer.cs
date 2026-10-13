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
            pnlFiltros = new FlowLayoutPanel();
            dtpDesde = new DateTimePicker();
            dtpHasta = new DateTimePicker();
            btnConsultar = new Button();
            btnExportar = new Button();
            pnlConceptos = new Panel();
            clbConceptos = new CheckedListBox();
            btnTodos = new Button();
            btnNinguno = new Button();
            dgvMovimientos = new RadGridView();
            lblEstado = new Label();
            var lblDesde = new Label();
            var lblHasta = new Label();
            var lblConceptos = new Label();
            var lblFechas = new Label();
            var pnlSeleccion = new FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvMovimientos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvMovimientos.MasterTemplate).BeginInit();
            SuspendLayout();

            pnlFiltros.Name = "pnlFiltros";
            pnlFiltros.Dock = DockStyle.Top;
            pnlFiltros.Height = 54;
            pnlFiltros.Padding = new Padding(10);
            pnlFiltros.WrapContents = false;
            lblDesde.Text = "Desde:";
            lblDesde.AutoSize = true;
            lblDesde.Margin = new Padding(0, 7, 5, 0);
            dtpDesde.Name = "dtpDesde";
            dtpDesde.Format = DateTimePickerFormat.Custom;
            dtpDesde.CustomFormat = "dd/MM/yyyy";
            dtpDesde.Width = 120;
            lblHasta.Text = "Hasta:";
            lblHasta.AutoSize = true;
            lblHasta.Margin = new Padding(14, 7, 5, 0);
            dtpHasta.Name = "dtpHasta";
            dtpHasta.Format = DateTimePickerFormat.Custom;
            dtpHasta.CustomFormat = "dd/MM/yyyy";
            dtpHasta.Width = 120;
            btnConsultar.Name = "btnConsultar";
            btnConsultar.Text = "Consultar";
            btnConsultar.Size = new Size(110, 30);
            btnConsultar.Margin = new Padding(20, 0, 6, 0);
            btnExportar.Name = "btnExportar";
            btnExportar.Text = "Exportar a Excel";
            btnExportar.Size = new Size(140, 30);
            btnExportar.Enabled = false;
            btnExportar.Margin = new Padding(0);
            pnlFiltros.Controls.AddRange(new Control[] { lblDesde, dtpDesde, lblHasta, dtpHasta, btnConsultar, btnExportar });

            pnlConceptos.Name = "pnlConceptos";
            pnlConceptos.Dock = DockStyle.Left;
            pnlConceptos.Width = 280;
            pnlConceptos.Padding = new Padding(10, 0, 10, 10);
            lblConceptos.Text = "Conceptos finales (selección múltiple)";
            lblConceptos.Dock = DockStyle.Top;
            lblConceptos.Height = 30;
            lblConceptos.TextAlign = ContentAlignment.MiddleLeft;
            clbConceptos.Name = "clbConceptos";
            clbConceptos.Dock = DockStyle.Fill;
            clbConceptos.CheckOnClick = true;
            clbConceptos.IntegralHeight = false;
            clbConceptos.HorizontalScrollbar = true;
            pnlSeleccion.Dock = DockStyle.Bottom;
            pnlSeleccion.Height = 38;
            btnTodos.Text = "Marcar todos";
            btnTodos.Size = new Size(122, 30);
            btnNinguno.Text = "Desmarcar todos";
            btnNinguno.Size = new Size(122, 30);
            pnlSeleccion.Controls.AddRange(new Control[] { btnTodos, btnNinguno });
            pnlConceptos.Controls.Add(clbConceptos);
            pnlConceptos.Controls.Add(lblConceptos);
            pnlConceptos.Controls.Add(pnlSeleccion);

            dgvMovimientos.Name = "dgvMovimientos";
            dgvMovimientos.Dock = DockStyle.Fill;
            dgvMovimientos.ReadOnly = true;
            dgvMovimientos.MasterTemplate.AllowAddNewRow = false;
            dgvMovimientos.MasterTemplate.AllowDeleteRow = false;
            dgvMovimientos.MasterTemplate.AllowEditRow = false;
            dgvMovimientos.EnableGrouping = false;
            dgvMovimientos.EnableFiltering = false;
            dgvMovimientos.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.None;
            dgvMovimientos.ShowGroupPanel = false;
            lblEstado.Name = "lblEstado";
            lblEstado.Dock = DockStyle.Bottom;
            lblEstado.Height = 34;
            lblEstado.Padding = new Padding(10, 0, 0, 0);
            lblEstado.TextAlign = ContentAlignment.MiddleLeft;
            // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 3 - Con Fecha date NOT NULL no hay registros sin fecha válida, así que se retira el aviso del contrato anterior.
            lblFechas.Text = "Rango inclusivo por fecha del movimiento.";
            lblFechas.Dock = DockStyle.Bottom;
            lblFechas.Height = 28;
            lblFechas.Padding = new Padding(10, 0, 0, 0);
            lblFechas.ForeColor = Color.DimGray;

            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Font = new Font("Segoe UI", 9F);
            ClientSize = new Size(1250, 650);
            MinimumSize = new Size(1000, 500);
            Name = "ConceptosListado";
            Text = "Consulta de movimientos por conceptos finales";
            StartPosition = FormStartPosition.CenterParent;
            Controls.Add(dgvMovimientos);
            Controls.Add(pnlConceptos);
            Controls.Add(pnlFiltros);
            Controls.Add(lblFechas);
            Controls.Add(lblEstado);
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
    }
}
