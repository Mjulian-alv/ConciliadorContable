using System.Drawing;
using System.Windows.Forms;
using Telerik.WinControls.UI;

namespace AgrupadorConceptos
{
    partial class ConciliacionExternForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) { if (disposing) components?.Dispose(); base.Dispose(disposing); }

        // Fecha: 09/10/2026 - TAREA: 00082 - Linea: 1 - La composición queda serializable para poder editar el formulario desde el Designer.
        private void InitializeComponent()
        {
            GridViewCheckBoxColumn gridViewCheckBoxColumn1 = new GridViewCheckBoxColumn();
            GridViewTextBoxColumn gridViewTextBoxColumn1 = new GridViewTextBoxColumn();
            GridViewTextBoxColumn gridViewTextBoxColumn2 = new GridViewTextBoxColumn();
            // Fecha: 09/10/2026 - TAREA: 00084 - Linea: 1 - Las columnas de perfil quedan editables en el diseñador de ambas pestañas del extracto.
            // Fecha: 09/10/2026 - TAREA: 00085 - Linea: 1 - Declarar el conjunto completo evita que la columna Perfil suprima la generación del resto del extracto.
            GridViewDateTimeColumn columnaFechaPendiente = new GridViewDateTimeColumn();
            GridViewTextBoxColumn columnaConceptoOriginalPendiente = new GridViewTextBoxColumn();
            GridViewTextBoxColumn columnaDescripcionOriginalPendiente = new GridViewTextBoxColumn();
            GridViewTextBoxColumn columnaConceptoEstandarPendiente = new GridViewTextBoxColumn();
            GridViewDecimalColumn columnaDebitosPendiente = new GridViewDecimalColumn();
            GridViewDecimalColumn columnaCreditosPendiente = new GridViewDecimalColumn();
            GridViewTextBoxColumn columnaConceptoFinalPendiente = new GridViewTextBoxColumn();
            GridViewTextBoxColumn columnaCuentaFinalPendiente = new GridViewTextBoxColumn();
            GridViewDateTimeColumn columnaFechaResto = new GridViewDateTimeColumn();
            GridViewTextBoxColumn columnaConceptoOriginalResto = new GridViewTextBoxColumn();
            GridViewTextBoxColumn columnaDescripcionOriginalResto = new GridViewTextBoxColumn();
            GridViewTextBoxColumn columnaConceptoEstandarResto = new GridViewTextBoxColumn();
            GridViewDecimalColumn columnaDebitosResto = new GridViewDecimalColumn();
            GridViewDecimalColumn columnaCreditosResto = new GridViewDecimalColumn();
            GridViewTextBoxColumn columnaConceptoFinalResto = new GridViewTextBoxColumn();
            GridViewTextBoxColumn columnaCuentaFinalResto = new GridViewTextBoxColumn();
            GridViewTextBoxColumn columnaPerfilPendiente = new GridViewTextBoxColumn();
            GridViewTextBoxColumn columnaPerfilResto = new GridViewTextBoxColumn();
            GridViewDateTimeColumn gridViewDateTimeColumn1 = new GridViewDateTimeColumn();
            GridViewDateTimeColumn gridViewDateTimeColumn2 = new GridViewDateTimeColumn();
            GridViewDecimalColumn gridViewDecimalColumn1 = new GridViewDecimalColumn();
            TableViewDefinition tableViewDefinition1 = new TableViewDefinition();
            TableViewDefinition tableViewDefinition2 = new TableViewDefinition();
            TableViewDefinition tableViewDefinition3 = new TableViewDefinition();
            TableViewDefinition tableViewDefinition4 = new TableViewDefinition();
            TableViewDefinition tableViewDefinition5 = new TableViewDefinition();
            pnlSesiones = new Panel();
            lblSesionesTitle = new Label();
            lbSesiones = new ListBox();
            btnNuevaSesion = new Button();
            btnRetomar = new Button();
            btnEliminarSesion = new Button();
            lblSesionActiva = new Label();
            pnlConfigNueva = new Panel();
            lblArchivo = new Label();
            lblDesdeArchivos = new Label();
            lblHastaArchivos = new Label();
            _desdeArchivos = new DateTimePicker();
            _hastaArchivos = new DateTimePicker();
            _mostrarTodos = new CheckBox();
            _estadoArchivos = new Label();
            _avisoRango = new Label();
            pnlSelectorArchivos = new Panel();
            _splitArchivos = new SplitContainer();
            _grillaArchivos = new RadGridView();
            lblConceptos = new Label();
            clbConceptos = new CheckedListBox();
            lblArchivoExterno = new Label();
            txtArchivoExterno = new TextBox();
            btnCargarExterno = new Button();
            btnConfirmarNueva = new Button();
            btnCancelarNueva = new Button();
            _resumenSeleccion = new Label();
            pnlAcciones = new Panel();
            btnAutoConciliar = new Button();
            btnConciliarManual = new Button();
            btnDesconciliar = new Button();
            btnFinalizar = new Button();
            btnExportar = new Button();
            lblContadorPendExt = new Label();
            lblContadorPendExtr = new Label();
            lblContadorConcil = new Label();
            tabControl = new TabControl();
            tabPendientes = new TabPage();
            splitPendientes = new SplitContainer();
            pnlPendExt = new Panel();
            dgvExternoPendiente = new RadGridView();
            lblPendExt = new Label();
            tabsPendExtracto = new TabControl();
            tabConConcepto = new TabPage();
            dgvExtractoPendiente = new RadGridView();
            tabSinConcepto = new TabPage();
            dgvExtractoSinConcepto = new RadGridView();
            tabConciliados = new TabPage();
            dgvConciliados = new RadGridView();
            pnlSesiones.SuspendLayout();
            pnlConfigNueva.SuspendLayout();
            pnlSelectorArchivos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_splitArchivos).BeginInit();
            _splitArchivos.Panel1.SuspendLayout();
            _splitArchivos.Panel2.SuspendLayout();
            _splitArchivos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_grillaArchivos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_grillaArchivos.MasterTemplate).BeginInit();
            pnlAcciones.SuspendLayout();
            tabControl.SuspendLayout();
            tabPendientes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitPendientes).BeginInit();
            splitPendientes.Panel1.SuspendLayout();
            splitPendientes.Panel2.SuspendLayout();
            splitPendientes.SuspendLayout();
            pnlPendExt.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvExternoPendiente).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvExternoPendiente.MasterTemplate).BeginInit();
            tabsPendExtracto.SuspendLayout();
            tabConConcepto.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvExtractoPendiente).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvExtractoPendiente.MasterTemplate).BeginInit();
            tabSinConcepto.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvExtractoSinConcepto).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvExtractoSinConcepto.MasterTemplate).BeginInit();
            tabConciliados.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvConciliados).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvConciliados.MasterTemplate).BeginInit();
            SuspendLayout();
            // 
            // pnlSesiones
            // 
            pnlSesiones.BackColor = Color.FromArgb(240, 244, 248);
            pnlSesiones.Controls.Add(lblSesionesTitle);
            pnlSesiones.Controls.Add(lbSesiones);
            pnlSesiones.Controls.Add(btnNuevaSesion);
            pnlSesiones.Controls.Add(btnRetomar);
            pnlSesiones.Controls.Add(btnEliminarSesion);
            pnlSesiones.Controls.Add(lblSesionActiva);
            pnlSesiones.Dock = DockStyle.Top;
            pnlSesiones.Location = new Point(0, 0);
            pnlSesiones.Name = "pnlSesiones";
            pnlSesiones.Padding = new Padding(10, 8, 10, 4);
            pnlSesiones.Size = new Size(1200, 120);
            pnlSesiones.TabIndex = 3;
            // 
            // lblSesionesTitle
            // 
            lblSesionesTitle.AutoSize = true;
            lblSesionesTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblSesionesTitle.Location = new Point(10, 10);
            lblSesionesTitle.Name = "lblSesionesTitle";
            lblSesionesTitle.Size = new Size(58, 15);
            lblSesionesTitle.TabIndex = 0;
            lblSesionesTitle.Text = "Sesiones:";
            // 
            // lbSesiones
            // 
            lbSesiones.DisplayMember = "DisplayName";
            lbSesiones.ItemHeight = 15;
            lbSesiones.Location = new Point(100, 7);
            lbSesiones.Name = "lbSesiones";
            lbSesiones.Size = new Size(700, 64);
            lbSesiones.TabIndex = 1;
            lbSesiones.ValueMember = "Id";
            lbSesiones.SelectedIndexChanged += lbSesiones_SelectedIndexChanged;
            // 
            // btnNuevaSesion
            // 
            btnNuevaSesion.Location = new Point(815, 7);
            btnNuevaSesion.Name = "btnNuevaSesion";
            btnNuevaSesion.Size = new Size(100, 28);
            btnNuevaSesion.TabIndex = 2;
            btnNuevaSesion.Text = "➕ Nueva";
            btnNuevaSesion.Click += btnNuevaSesion_Click;
            // 
            // btnRetomar
            // 
            btnRetomar.Enabled = false;
            btnRetomar.Location = new Point(815, 40);
            btnRetomar.Name = "btnRetomar";
            btnRetomar.Size = new Size(100, 28);
            btnRetomar.TabIndex = 3;
            btnRetomar.Text = "▶ Retomar";
            btnRetomar.Click += btnRetomar_Click;
            // 
            // btnEliminarSesion
            // 
            btnEliminarSesion.BackColor = Color.MistyRose;
            btnEliminarSesion.Enabled = false;
            btnEliminarSesion.Location = new Point(815, 73);
            btnEliminarSesion.Name = "btnEliminarSesion";
            btnEliminarSesion.Size = new Size(100, 28);
            btnEliminarSesion.TabIndex = 4;
            btnEliminarSesion.Text = "🗑 Eliminar";
            btnEliminarSesion.UseVisualStyleBackColor = false;
            btnEliminarSesion.Click += btnEliminarSesion_Click;
            // 
            // lblSesionActiva
            // 
            lblSesionActiva.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblSesionActiva.ForeColor = Color.DarkSlateBlue;
            lblSesionActiva.Location = new Point(930, 10);
            lblSesionActiva.Name = "lblSesionActiva";
            lblSesionActiva.Size = new Size(250, 40);
            lblSesionActiva.TabIndex = 5;
            lblSesionActiva.Text = "Sin sesión activa";
            // 
            // pnlConfigNueva
            // 
            pnlConfigNueva.BackColor = Color.FromArgb(255, 252, 230);
            pnlConfigNueva.Controls.Add(lblArchivo);
            pnlConfigNueva.Controls.Add(lblDesdeArchivos);
            pnlConfigNueva.Controls.Add(lblHastaArchivos);
            pnlConfigNueva.Controls.Add(_desdeArchivos);
            pnlConfigNueva.Controls.Add(_hastaArchivos);
            pnlConfigNueva.Controls.Add(_mostrarTodos);
            pnlConfigNueva.Controls.Add(_estadoArchivos);
            pnlConfigNueva.Controls.Add(_avisoRango);
            pnlConfigNueva.Controls.Add(pnlSelectorArchivos);
            pnlConfigNueva.Controls.Add(lblArchivoExterno);
            pnlConfigNueva.Controls.Add(txtArchivoExterno);
            pnlConfigNueva.Controls.Add(btnCargarExterno);
            pnlConfigNueva.Controls.Add(btnConfirmarNueva);
            pnlConfigNueva.Controls.Add(btnCancelarNueva);
            pnlConfigNueva.Dock = DockStyle.Top;
            pnlConfigNueva.Location = new Point(0, 120);
            pnlConfigNueva.Name = "pnlConfigNueva";
            pnlConfigNueva.Padding = new Padding(10, 8, 10, 4);
            pnlConfigNueva.Size = new Size(1200, 335);
            pnlConfigNueva.TabIndex = 2;
            pnlConfigNueva.Visible = false;
            // 
            // lblArchivo
            // 
            lblArchivo.AutoSize = true;
            lblArchivo.Location = new Point(10, 10);
            lblArchivo.Name = "lblArchivo";
            lblArchivo.Size = new Size(57, 15);
            lblArchivo.TabIndex = 0;
            lblArchivo.Text = "Extractos:";
            // 
            // lblDesdeArchivos
            // 
            lblDesdeArchivos.AutoSize = true;
            lblDesdeArchivos.Location = new Point(10, 44);
            lblDesdeArchivos.Name = "lblDesdeArchivos";
            lblDesdeArchivos.Size = new Size(42, 15);
            lblDesdeArchivos.TabIndex = 9;
            lblDesdeArchivos.Text = "Desde:";
            // 
            // lblHastaArchivos
            // 
            lblHastaArchivos.AutoSize = true;
            lblHastaArchivos.Location = new Point(250, 44);
            lblHastaArchivos.Name = "lblHastaArchivos";
            lblHastaArchivos.Size = new Size(40, 15);
            lblHastaArchivos.TabIndex = 11;
            lblHastaArchivos.Text = "Hasta:";
            // 
            // _desdeArchivos
            // 
            _desdeArchivos.CustomFormat = "dd/MM/yyyy";
            _desdeArchivos.Format = DateTimePickerFormat.Custom;
            _desdeArchivos.Location = new Point(110, 40);
            _desdeArchivos.Name = "_desdeArchivos";
            _desdeArchivos.Size = new Size(130, 23);
            _desdeArchivos.TabIndex = 10;
            _desdeArchivos.ValueChanged += FiltroArchivos_Changed;
            // 
            // _hastaArchivos
            // 
            _hastaArchivos.CustomFormat = "dd/MM/yyyy";
            _hastaArchivos.Format = DateTimePickerFormat.Custom;
            _hastaArchivos.Location = new Point(300, 40);
            _hastaArchivos.Name = "_hastaArchivos";
            _hastaArchivos.Size = new Size(130, 23);
            _hastaArchivos.TabIndex = 12;
            _hastaArchivos.ValueChanged += FiltroArchivos_Changed;
            // 
            // _mostrarTodos
            // 
            _mostrarTodos.AutoSize = true;
            _mostrarTodos.Location = new Point(460, 42);
            _mostrarTodos.Name = "_mostrarTodos";
            _mostrarTodos.Size = new Size(100, 19);
            _mostrarTodos.TabIndex = 13;
            _mostrarTodos.Text = "Mostrar todos";
            _mostrarTodos.CheckedChanged += FiltroArchivos_Changed;
            // 
            // _estadoArchivos
            // 
            _estadoArchivos.AutoSize = true;
            _estadoArchivos.Location = new Point(600, 43);
            _estadoArchivos.Name = "_estadoArchivos";
            _estadoArchivos.Size = new Size(0, 15);
            _estadoArchivos.TabIndex = 14;
            // 
            // _avisoRango
            // 
            _avisoRango.AutoSize = true;
            _avisoRango.Location = new Point(10, 70);
            _avisoRango.Name = "_avisoRango";
            _avisoRango.Size = new Size(472, 15);
            _avisoRango.TabIndex = 15;
            _avisoRango.Text = "El rango filtra la lista de archivos; la sesión incluye los archivos seleccionados completos.";
            // 
            // pnlSelectorArchivos
            // 
            pnlSelectorArchivos.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlSelectorArchivos.Controls.Add(_splitArchivos);
            pnlSelectorArchivos.Location = new Point(10, 95);
            pnlSelectorArchivos.Name = "pnlSelectorArchivos";
            pnlSelectorArchivos.Size = new Size(1170, 195);
            pnlSelectorArchivos.TabIndex = 16;
            // 
            // _splitArchivos
            // 
            _splitArchivos.Dock = DockStyle.Fill;
            _splitArchivos.Location = new Point(0, 0);
            _splitArchivos.Name = "_splitArchivos";
            // 
            // _splitArchivos.Panel1
            // 
            _splitArchivos.Panel1.Controls.Add(_grillaArchivos);
            // 
            // _splitArchivos.Panel2
            // 
            _splitArchivos.Panel2.Controls.Add(lblConceptos);
            _splitArchivos.Panel2.Controls.Add(clbConceptos);
            _splitArchivos.Size = new Size(1170, 195);
            _splitArchivos.SplitterDistance = 790;
            _splitArchivos.TabIndex = 0;
            // 
            // _grillaArchivos
            // 
            _grillaArchivos.Dock = DockStyle.Fill;
            _grillaArchivos.Location = new Point(0, 0);
            // 
            // 
            // 
            _grillaArchivos.MasterTemplate.AllowAddNewRow = false;
            _grillaArchivos.MasterTemplate.AllowDeleteRow = false;
            _grillaArchivos.MasterTemplate.AutoGenerateColumns = false;
            _grillaArchivos.MasterTemplate.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;
            gridViewCheckBoxColumn1.FieldName = "Seleccionado";
            gridViewCheckBoxColumn1.HeaderText = "Elegir";
            gridViewCheckBoxColumn1.Name = "Seleccionado";
            gridViewCheckBoxColumn1.Width = 109;
            gridViewTextBoxColumn1.FieldName = "Perfil";
            gridViewTextBoxColumn1.HeaderText = "Perfil";
            gridViewTextBoxColumn1.Name = "Perfil";
            gridViewTextBoxColumn1.ReadOnly = true;
            gridViewTextBoxColumn1.Width = 109;
            gridViewTextBoxColumn2.FieldName = "Archivo";
            gridViewTextBoxColumn2.HeaderText = "Archivo";
            gridViewTextBoxColumn2.Name = "Archivo";
            gridViewTextBoxColumn2.ReadOnly = true;
            gridViewTextBoxColumn2.Width = 109;
            gridViewDateTimeColumn1.FieldName = "Desde";
            gridViewDateTimeColumn1.FormatString = "{0:dd/MM/yyyy}";
            gridViewDateTimeColumn1.HeaderText = "Desde";
            gridViewDateTimeColumn1.Name = "Desde";
            gridViewDateTimeColumn1.ReadOnly = true;
            gridViewDateTimeColumn1.Width = 109;
            gridViewDateTimeColumn2.FieldName = "Hasta";
            gridViewDateTimeColumn2.FormatString = "{0:dd/MM/yyyy}";
            gridViewDateTimeColumn2.HeaderText = "Hasta";
            gridViewDateTimeColumn2.Name = "Hasta";
            gridViewDateTimeColumn2.ReadOnly = true;
            gridViewDateTimeColumn2.Width = 109;
            gridViewDecimalColumn1.DataType = typeof(int);
            gridViewDecimalColumn1.DecimalPlaces = 0;
            gridViewDecimalColumn1.FieldName = "Cantidad";
            gridViewDecimalColumn1.HeaderText = "Cantidad";
            gridViewDecimalColumn1.Name = "Cantidad";
            gridViewDecimalColumn1.ReadOnly = true;
            gridViewDecimalColumn1.Width = 109;
            _grillaArchivos.MasterTemplate.Columns.AddRange(new GridViewDataColumn[] { gridViewCheckBoxColumn1, gridViewTextBoxColumn1, gridViewTextBoxColumn2, gridViewDateTimeColumn1, gridViewDateTimeColumn2, gridViewDecimalColumn1 });
            _grillaArchivos.MasterTemplate.ViewDefinition = tableViewDefinition1;
            _grillaArchivos.Name = "_grillaArchivos";
            _grillaArchivos.ShowGroupPanel = false;
            _grillaArchivos.Size = new Size(790, 195);
            _grillaArchivos.TabIndex = 0;
            _grillaArchivos.CellValueChanged += grillaArchivos_CellValueChanged;
            // 
            // lblConceptos
            // 
            lblConceptos.AutoSize = true;
            lblConceptos.Location = new Point(8, 4);
            lblConceptos.Name = "lblConceptos";
            lblConceptos.Size = new Size(67, 15);
            lblConceptos.TabIndex = 2;
            lblConceptos.Text = "Conceptos:";
            // 
            // clbConceptos
            // 
            clbConceptos.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            clbConceptos.CheckOnClick = true;
            clbConceptos.Location = new Point(8, 28);
            clbConceptos.Name = "clbConceptos";
            clbConceptos.Size = new Size(340, 148);
            clbConceptos.TabIndex = 3;
            // 
            // lblArchivoExterno
            // 
            lblArchivoExterno.AutoSize = true;
            lblArchivoExterno.Location = new Point(560, 10);
            lblArchivoExterno.Name = "lblArchivoExterno";
            lblArchivoExterno.Size = new Size(93, 15);
            lblArchivoExterno.TabIndex = 4;
            lblArchivoExterno.Text = "Archivo externo:";
            // 
            // txtArchivoExterno
            // 
            txtArchivoExterno.Location = new Point(680, 7);
            txtArchivoExterno.Name = "txtArchivoExterno";
            txtArchivoExterno.ReadOnly = true;
            txtArchivoExterno.Size = new Size(360, 23);
            txtArchivoExterno.TabIndex = 5;
            // 
            // btnCargarExterno
            // 
            btnCargarExterno.Location = new Point(1048, 6);
            btnCargarExterno.Name = "btnCargarExterno";
            btnCargarExterno.Size = new Size(36, 23);
            btnCargarExterno.TabIndex = 6;
            btnCargarExterno.Text = "...";
            btnCargarExterno.Click += btnCargarExterno_Click;
            // 
            // btnConfirmarNueva
            // 
            btnConfirmarNueva.BackColor = Color.LightGreen;
            btnConfirmarNueva.Location = new Point(920, 296);
            btnConfirmarNueva.Name = "btnConfirmarNueva";
            btnConfirmarNueva.Size = new Size(110, 28);
            btnConfirmarNueva.TabIndex = 7;
            btnConfirmarNueva.Text = "✔ Confirmar";
            btnConfirmarNueva.UseVisualStyleBackColor = false;
            btnConfirmarNueva.Click += btnConfirmarNueva_Click;
            // 
            // btnCancelarNueva
            // 
            btnCancelarNueva.BackColor = Color.MistyRose;
            btnCancelarNueva.Location = new Point(1040, 296);
            btnCancelarNueva.Name = "btnCancelarNueva";
            btnCancelarNueva.Size = new Size(110, 28);
            btnCancelarNueva.TabIndex = 8;
            btnCancelarNueva.Text = "✖ Cancelar";
            btnCancelarNueva.UseVisualStyleBackColor = false;
            btnCancelarNueva.Click += btnCancelarNueva_Click;
            // 
            // _resumenSeleccion
            // 
            _resumenSeleccion.Dock = DockStyle.Bottom;
            _resumenSeleccion.Location = new Point(6, 44);
            _resumenSeleccion.Name = "_resumenSeleccion";
            _resumenSeleccion.Size = new Size(1188, 26);
            _resumenSeleccion.TabIndex = 8;
            _resumenSeleccion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlAcciones
            // 
            pnlAcciones.BackColor = Color.FromArgb(230, 235, 240);
            pnlAcciones.Controls.Add(_resumenSeleccion);
            pnlAcciones.Controls.Add(btnAutoConciliar);
            pnlAcciones.Controls.Add(btnConciliarManual);
            pnlAcciones.Controls.Add(btnDesconciliar);
            pnlAcciones.Controls.Add(btnFinalizar);
            pnlAcciones.Controls.Add(btnExportar);
            pnlAcciones.Controls.Add(lblContadorPendExt);
            pnlAcciones.Controls.Add(lblContadorPendExtr);
            pnlAcciones.Controls.Add(lblContadorConcil);
            pnlAcciones.Dock = DockStyle.Bottom;
            pnlAcciones.Location = new Point(0, 680);
            pnlAcciones.Name = "pnlAcciones";
            pnlAcciones.Padding = new Padding(6, 6, 6, 0);
            pnlAcciones.Size = new Size(1200, 70);
            pnlAcciones.TabIndex = 1;
            // 
            // btnAutoConciliar
            // 
            btnAutoConciliar.Enabled = false;
            btnAutoConciliar.Location = new Point(6, 6);
            btnAutoConciliar.Name = "btnAutoConciliar";
            btnAutoConciliar.Size = new Size(155, 28);
            btnAutoConciliar.TabIndex = 0;
            btnAutoConciliar.Text = "⚡ Auto-conciliar";
            btnAutoConciliar.Click += btnAutoConciliar_Click;
            // 
            // btnConciliarManual
            // 
            btnConciliarManual.Enabled = false;
            btnConciliarManual.Location = new Point(166, 6);
            btnConciliarManual.Name = "btnConciliarManual";
            btnConciliarManual.Size = new Size(155, 28);
            btnConciliarManual.TabIndex = 1;
            btnConciliarManual.Text = "Conciliar selección";
            btnConciliarManual.Click += btnConciliarManual_Click;
            // 
            // btnDesconciliar
            // 
            btnDesconciliar.Enabled = false;
            btnDesconciliar.Location = new Point(326, 6);
            btnDesconciliar.Name = "btnDesconciliar";
            btnDesconciliar.Size = new Size(155, 28);
            btnDesconciliar.TabIndex = 2;
            btnDesconciliar.Text = "↩ Desconciliar";
            btnDesconciliar.Click += btnDesconciliar_Click;
            // 
            // btnFinalizar
            // 
            btnFinalizar.Enabled = false;
            btnFinalizar.Location = new Point(486, 6);
            btnFinalizar.Name = "btnFinalizar";
            btnFinalizar.Size = new Size(155, 28);
            btnFinalizar.TabIndex = 3;
            btnFinalizar.Text = "🏁 Finalizar";
            btnFinalizar.Click += btnFinalizar_Click;
            // 
            // btnExportar
            // 
            btnExportar.Enabled = false;
            btnExportar.Location = new Point(646, 6);
            btnExportar.Name = "btnExportar";
            btnExportar.Size = new Size(155, 28);
            btnExportar.TabIndex = 4;
            btnExportar.Text = "📤 Exportar Excel";
            btnExportar.Click += btnExportar_Click;
            // 
            // lblContadorPendExt
            // 
            lblContadorPendExt.AutoSize = true;
            lblContadorPendExt.ForeColor = Color.DarkRed;
            lblContadorPendExt.Location = new Point(860, 10);
            lblContadorPendExt.Name = "lblContadorPendExt";
            lblContadorPendExt.Size = new Size(0, 15);
            lblContadorPendExt.TabIndex = 5;
            // 
            // lblContadorPendExtr
            // 
            lblContadorPendExtr.AutoSize = true;
            lblContadorPendExtr.ForeColor = Color.DarkOrange;
            lblContadorPendExtr.Location = new Point(1000, 10);
            lblContadorPendExtr.Name = "lblContadorPendExtr";
            lblContadorPendExtr.Size = new Size(0, 15);
            lblContadorPendExtr.TabIndex = 6;
            // 
            // lblContadorConcil
            // 
            lblContadorConcil.AutoSize = true;
            lblContadorConcil.ForeColor = Color.DarkGreen;
            lblContadorConcil.Location = new Point(1130, 10);
            lblContadorConcil.Name = "lblContadorConcil";
            lblContadorConcil.Size = new Size(0, 15);
            lblContadorConcil.TabIndex = 7;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabPendientes);
            tabControl.Controls.Add(tabConciliados);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 455);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(1200, 225);
            tabControl.TabIndex = 0;
            // 
            // tabPendientes
            // 
            tabPendientes.Controls.Add(splitPendientes);
            tabPendientes.Location = new Point(4, 24);
            tabPendientes.Name = "tabPendientes";
            tabPendientes.Size = new Size(1192, 197);
            tabPendientes.TabIndex = 0;
            tabPendientes.Text = "⏳ Pendientes";
            // 
            // splitPendientes
            // 
            splitPendientes.Dock = DockStyle.Fill;
            splitPendientes.Location = new Point(0, 0);
            splitPendientes.Name = "splitPendientes";
            // 
            // splitPendientes.Panel1
            // 
            splitPendientes.Panel1.Controls.Add(pnlPendExt);
            // 
            // splitPendientes.Panel2
            // 
            splitPendientes.Panel2.Controls.Add(tabsPendExtracto);
            splitPendientes.Size = new Size(1192, 197);
            splitPendientes.SplitterDistance = 531;
            splitPendientes.TabIndex = 0;
            // 
            // pnlPendExt
            // 
            pnlPendExt.Controls.Add(dgvExternoPendiente);
            pnlPendExt.Controls.Add(lblPendExt);
            pnlPendExt.Dock = DockStyle.Fill;
            pnlPendExt.Location = new Point(0, 0);
            pnlPendExt.Name = "pnlPendExt";
            pnlPendExt.Size = new Size(531, 197);
            pnlPendExt.TabIndex = 0;
            // 
            // dgvExternoPendiente
            // 
            dgvExternoPendiente.Dock = DockStyle.Fill;
            dgvExternoPendiente.Location = new Point(0, 20);
            // 
            // 
            // 
            dgvExternoPendiente.MasterTemplate.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;
            dgvExternoPendiente.MasterTemplate.ViewDefinition = tableViewDefinition2;
            dgvExternoPendiente.Name = "dgvExternoPendiente";
            // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Mantener el formato de fechas tipadas al renovar los datos.
            dgvExternoPendiente.DataBindingComplete += Services.FechasPresentacion.AlEnlazar;
            dgvExternoPendiente.ReadOnly = true;
            dgvExternoPendiente.Size = new Size(531, 177);
            dgvExternoPendiente.TabIndex = 0;
            dgvExternoPendiente.CellFormatting += FormatearCeldaConciliacion;
            dgvExternoPendiente.SelectionChanged += dgvExternoPendiente_SelectionChanged;
            // 
            // lblPendExt
            // 
            lblPendExt.Dock = DockStyle.Top;
            lblPendExt.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPendExt.ForeColor = Color.DarkSlateBlue;
            lblPendExt.Location = new Point(0, 0);
            lblPendExt.Name = "lblPendExt";
            lblPendExt.Size = new Size(531, 20);
            lblPendExt.TabIndex = 1;
            lblPendExt.Text = "Archivo externo — pendientes";
            // 
            // tabsPendExtracto
            // 
            tabsPendExtracto.Controls.Add(tabConConcepto);
            tabsPendExtracto.Controls.Add(tabSinConcepto);
            tabsPendExtracto.Dock = DockStyle.Fill;
            tabsPendExtracto.Location = new Point(0, 0);
            tabsPendExtracto.Name = "tabsPendExtracto";
            tabsPendExtracto.SelectedIndex = 0;
            tabsPendExtracto.Size = new Size(657, 197);
            tabsPendExtracto.TabIndex = 0;
            tabsPendExtracto.SelectedIndexChanged += tabsPendExtracto_SelectedIndexChanged;
            // 
            // tabConConcepto
            // 
            tabConConcepto.Controls.Add(dgvExtractoPendiente);
            tabConConcepto.Location = new Point(4, 24);
            tabConConcepto.Name = "tabConConcepto";
            tabConConcepto.Size = new Size(649, 169);
            tabConConcepto.TabIndex = 0;
            tabConConcepto.Text = "Con concepto seleccionado";
            // 
            // dgvExtractoPendiente
            // 
            dgvExtractoPendiente.Dock = DockStyle.Fill;
            dgvExtractoPendiente.Location = new Point(0, 0);
            // 
            // 
            // 
            dgvExtractoPendiente.MasterTemplate.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;
            // Fecha: 09/10/2026 - TAREA: 00084 - Linea: 1 - Vincular el nombre del perfil identifica el origen de cada movimiento pendiente.
            columnaPerfilPendiente.FieldName = "NombreBanco";
            columnaPerfilPendiente.HeaderText = "Perfil";
            columnaPerfilPendiente.Name = "NombreBanco";
            columnaPerfilPendiente.ReadOnly = true;
            columnaPerfilPendiente.Width = 120;
            // Fecha: 09/10/2026 - TAREA: 00085 - Linea: 1 - Declarar el conjunto completo evita que la columna Perfil suprima la generación del resto del extracto.
            dgvExtractoPendiente.MasterTemplate.AutoGenerateColumns = false;
            columnaFechaPendiente.FieldName = "Fecha";
            columnaFechaPendiente.HeaderText = "Fecha";
            columnaFechaPendiente.Name = "Fecha";
            // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Operar sobre días tipados evita reinterpretaciones culturales.
            columnaFechaPendiente.FormatString = "{0:dd/MM/yyyy}";
            columnaFechaPendiente.ReadOnly = true;
            columnaFechaPendiente.Width = 100;
            columnaConceptoOriginalPendiente.FieldName = "ConceptoOriginal";
            columnaConceptoOriginalPendiente.HeaderText = "Concepto original";
            columnaConceptoOriginalPendiente.Name = "ConceptoOriginal";
            columnaConceptoOriginalPendiente.ReadOnly = true;
            columnaConceptoOriginalPendiente.Width = 100;
            columnaDescripcionOriginalPendiente.FieldName = "DescripcionOriginal";
            columnaDescripcionOriginalPendiente.HeaderText = "Descripción";
            columnaDescripcionOriginalPendiente.Name = "DescripcionOriginal";
            columnaDescripcionOriginalPendiente.ReadOnly = true;
            columnaDescripcionOriginalPendiente.Width = 100;
            columnaConceptoEstandarPendiente.FieldName = "ConceptoEstandar";
            columnaConceptoEstandarPendiente.HeaderText = "Concepto estándar";
            columnaConceptoEstandarPendiente.Name = "ConceptoEstandar";
            columnaConceptoEstandarPendiente.ReadOnly = true;
            columnaConceptoEstandarPendiente.Width = 100;
            columnaDebitosPendiente.FieldName = "Debitos";
            columnaDebitosPendiente.HeaderText = "Débitos";
            columnaDebitosPendiente.Name = "Debitos";
            columnaDebitosPendiente.ReadOnly = true;
            columnaDebitosPendiente.Width = 100;
            columnaDebitosPendiente.DataType = typeof(decimal);
            columnaDebitosPendiente.FormatString = "{0:N2}";
            columnaCreditosPendiente.FieldName = "Creditos";
            columnaCreditosPendiente.HeaderText = "Créditos";
            columnaCreditosPendiente.Name = "Creditos";
            columnaCreditosPendiente.ReadOnly = true;
            columnaCreditosPendiente.Width = 100;
            columnaCreditosPendiente.DataType = typeof(decimal);
            columnaCreditosPendiente.FormatString = "{0:N2}";
            columnaConceptoFinalPendiente.FieldName = "ConceptoFinal";
            columnaConceptoFinalPendiente.HeaderText = "Concepto final";
            columnaConceptoFinalPendiente.Name = "ConceptoFinal";
            columnaConceptoFinalPendiente.ReadOnly = true;
            columnaConceptoFinalPendiente.Width = 100;
            columnaCuentaFinalPendiente.FieldName = "CuentaFinal";
            columnaCuentaFinalPendiente.HeaderText = "Cuenta final";
            columnaCuentaFinalPendiente.Name = "CuentaFinal";
            columnaCuentaFinalPendiente.ReadOnly = true;
            columnaCuentaFinalPendiente.Width = 100;
            dgvExtractoPendiente.MasterTemplate.Columns.AddRange(new GridViewDataColumn[] { columnaFechaPendiente, columnaConceptoOriginalPendiente, columnaDescripcionOriginalPendiente, columnaConceptoEstandarPendiente, columnaDebitosPendiente, columnaCreditosPendiente, columnaConceptoFinalPendiente, columnaCuentaFinalPendiente, columnaPerfilPendiente });
            dgvExtractoPendiente.MasterTemplate.MultiSelect = true;
            dgvExtractoPendiente.MasterTemplate.ViewDefinition = tableViewDefinition3;
            dgvExtractoPendiente.Name = "dgvExtractoPendiente";
            dgvExtractoPendiente.ReadOnly = true;
            dgvExtractoPendiente.Size = new Size(649, 169);
            dgvExtractoPendiente.TabIndex = 0;
            dgvExtractoPendiente.CellFormatting += FormatearCeldaConciliacion;
            dgvExtractoPendiente.SelectionChanged += SeleccionExtracto_Changed;
            // 
            // tabSinConcepto
            // 
            tabSinConcepto.Controls.Add(dgvExtractoSinConcepto);
            tabSinConcepto.Location = new Point(4, 24);
            tabSinConcepto.Name = "tabSinConcepto";
            tabSinConcepto.Size = new Size(649, 367);
            tabSinConcepto.TabIndex = 1;
            tabSinConcepto.Text = "Resto del extracto";
            // 
            // dgvExtractoSinConcepto
            // 
            dgvExtractoSinConcepto.Dock = DockStyle.Fill;
            dgvExtractoSinConcepto.Location = new Point(0, 0);
            // 
            // 
            // 
            dgvExtractoSinConcepto.MasterTemplate.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;
            // Fecha: 09/10/2026 - TAREA: 00084 - Linea: 1 - El resto del extracto debe mostrar el mismo origen que los pendientes por concepto.
            columnaPerfilResto.FieldName = "NombreBanco";
            columnaPerfilResto.HeaderText = "Perfil";
            columnaPerfilResto.Name = "NombreBanco";
            columnaPerfilResto.ReadOnly = true;
            columnaPerfilResto.Width = 120;
            // Fecha: 09/10/2026 - TAREA: 00085 - Linea: 1 - Declarar el conjunto completo evita que la columna Perfil suprima la generación del resto del extracto.
            dgvExtractoSinConcepto.MasterTemplate.AutoGenerateColumns = false;
            columnaFechaResto.FieldName = "Fecha";
            columnaFechaResto.HeaderText = "Fecha";
            columnaFechaResto.Name = "Fecha";
            // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Operar sobre días tipados evita reinterpretaciones culturales.
            columnaFechaResto.FormatString = "{0:dd/MM/yyyy}";
            columnaFechaResto.ReadOnly = true;
            columnaFechaResto.Width = 100;
            columnaConceptoOriginalResto.FieldName = "ConceptoOriginal";
            columnaConceptoOriginalResto.HeaderText = "Concepto original";
            columnaConceptoOriginalResto.Name = "ConceptoOriginal";
            columnaConceptoOriginalResto.ReadOnly = true;
            columnaConceptoOriginalResto.Width = 100;
            columnaDescripcionOriginalResto.FieldName = "DescripcionOriginal";
            columnaDescripcionOriginalResto.HeaderText = "Descripción";
            columnaDescripcionOriginalResto.Name = "DescripcionOriginal";
            columnaDescripcionOriginalResto.ReadOnly = true;
            columnaDescripcionOriginalResto.Width = 100;
            columnaConceptoEstandarResto.FieldName = "ConceptoEstandar";
            columnaConceptoEstandarResto.HeaderText = "Concepto estándar";
            columnaConceptoEstandarResto.Name = "ConceptoEstandar";
            columnaConceptoEstandarResto.ReadOnly = true;
            columnaConceptoEstandarResto.Width = 100;
            columnaDebitosResto.FieldName = "Debitos";
            columnaDebitosResto.HeaderText = "Débitos";
            columnaDebitosResto.Name = "Debitos";
            columnaDebitosResto.ReadOnly = true;
            columnaDebitosResto.Width = 100;
            columnaDebitosResto.DataType = typeof(decimal);
            columnaDebitosResto.FormatString = "{0:N2}";
            columnaCreditosResto.FieldName = "Creditos";
            columnaCreditosResto.HeaderText = "Créditos";
            columnaCreditosResto.Name = "Creditos";
            columnaCreditosResto.ReadOnly = true;
            columnaCreditosResto.Width = 100;
            columnaCreditosResto.DataType = typeof(decimal);
            columnaCreditosResto.FormatString = "{0:N2}";
            columnaConceptoFinalResto.FieldName = "ConceptoFinal";
            columnaConceptoFinalResto.HeaderText = "Concepto final";
            columnaConceptoFinalResto.Name = "ConceptoFinal";
            columnaConceptoFinalResto.ReadOnly = true;
            columnaConceptoFinalResto.Width = 100;
            columnaCuentaFinalResto.FieldName = "CuentaFinal";
            columnaCuentaFinalResto.HeaderText = "Cuenta final";
            columnaCuentaFinalResto.Name = "CuentaFinal";
            columnaCuentaFinalResto.ReadOnly = true;
            columnaCuentaFinalResto.Width = 100;
            dgvExtractoSinConcepto.MasterTemplate.Columns.AddRange(new GridViewDataColumn[] { columnaFechaResto, columnaConceptoOriginalResto, columnaDescripcionOriginalResto, columnaConceptoEstandarResto, columnaDebitosResto, columnaCreditosResto, columnaConceptoFinalResto, columnaCuentaFinalResto, columnaPerfilResto });
            dgvExtractoSinConcepto.MasterTemplate.MultiSelect = true;
            dgvExtractoSinConcepto.MasterTemplate.ViewDefinition = tableViewDefinition4;
            dgvExtractoSinConcepto.Name = "dgvExtractoSinConcepto";
            dgvExtractoSinConcepto.ReadOnly = true;
            dgvExtractoSinConcepto.Size = new Size(649, 367);
            dgvExtractoSinConcepto.TabIndex = 0;
            dgvExtractoSinConcepto.CellFormatting += FormatearCeldaConciliacion;
            dgvExtractoSinConcepto.SelectionChanged += SeleccionExtracto_Changed;
            // 
            // tabConciliados
            // 
            tabConciliados.Controls.Add(dgvConciliados);
            tabConciliados.Location = new Point(4, 24);
            tabConciliados.Name = "tabConciliados";
            tabConciliados.Size = new Size(1192, 395);
            tabConciliados.TabIndex = 1;
            tabConciliados.Text = "✅ Conciliados";
            // 
            // dgvConciliados
            // 
            dgvConciliados.Dock = DockStyle.Fill;
            dgvConciliados.Location = new Point(0, 0);
            // 
            // 
            // 
            dgvConciliados.MasterTemplate.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;
            dgvConciliados.MasterTemplate.ViewDefinition = tableViewDefinition5;
            dgvConciliados.Name = "dgvConciliados";
            // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Mantener el formato de fechas tipadas al renovar los datos.
            dgvConciliados.DataBindingComplete += Services.FechasPresentacion.AlEnlazar;
            dgvConciliados.ReadOnly = true;
            dgvConciliados.Size = new Size(1192, 395);
            dgvConciliados.TabIndex = 0;
            dgvConciliados.CellFormatting += FormatearCeldaConciliacion;
            // 
            // ConciliacionExternForm
            // 
            ClientSize = new Size(1200, 750);
            Controls.Add(tabControl);
            Controls.Add(pnlAcciones);
            Controls.Add(pnlConfigNueva);
            Controls.Add(pnlSesiones);
            MinimumSize = new Size(1000, 650);
            Name = "ConciliacionExternForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Conciliación con Archivo Externo";
            Load += OnLoad;
            pnlSesiones.ResumeLayout(false);
            pnlSesiones.PerformLayout();
            pnlConfigNueva.ResumeLayout(false);
            pnlConfigNueva.PerformLayout();
            pnlSelectorArchivos.ResumeLayout(false);
            _splitArchivos.Panel1.ResumeLayout(false);
            _splitArchivos.Panel2.ResumeLayout(false);
            _splitArchivos.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_splitArchivos).EndInit();
            _splitArchivos.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_grillaArchivos.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)_grillaArchivos).EndInit();
            pnlAcciones.ResumeLayout(false);
            pnlAcciones.PerformLayout();
            tabControl.ResumeLayout(false);
            tabPendientes.ResumeLayout(false);
            splitPendientes.Panel1.ResumeLayout(false);
            splitPendientes.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitPendientes).EndInit();
            splitPendientes.ResumeLayout(false);
            pnlPendExt.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvExternoPendiente.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvExternoPendiente).EndInit();
            tabsPendExtracto.ResumeLayout(false);
            tabConConcepto.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvExtractoPendiente.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvExtractoPendiente).EndInit();
            tabSinConcepto.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvExtractoSinConcepto.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvExtractoSinConcepto).EndInit();
            tabConciliados.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvConciliados.MasterTemplate).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvConciliados).EndInit();
            ResumeLayout(false);
        }

        // ── Campos ───────────────────────────────────────────────────────────────
        private Panel           pnlSesiones;
        private Label           lblSesionesTitle;
        private ListBox         lbSesiones;
        private Button          btnNuevaSesion;
        private Button          btnRetomar;
        private Button          btnEliminarSesion;
        private Label           lblSesionActiva;

        private Panel           pnlConfigNueva;
        private Label           lblArchivo;
        // Fecha: 09/10/2026 - TAREA: 00082 - Linea: 1 - La composición queda serializable para poder editar el formulario desde el Designer.
        private RadGridView _grillaArchivos;
        private DateTimePicker _desdeArchivos;
        private DateTimePicker _hastaArchivos;
        private CheckBox _mostrarTodos;
        private Label _estadoArchivos;
        private Label _avisoRango;
        private SplitContainer _splitArchivos;
        private Panel pnlSelectorArchivos;
        private Label lblDesdeArchivos;
        private Label lblHastaArchivos;
        private Label _resumenSeleccion;
        private Label           lblConceptos;
        private CheckedListBox  clbConceptos;
        private Label           lblArchivoExterno;
        private TextBox         txtArchivoExterno;
        private Button          btnCargarExterno;
        private Button          btnConfirmarNueva;
        private Button          btnCancelarNueva;

        private Panel           pnlAcciones;
        private Button          btnAutoConciliar;
        private Button          btnConciliarManual;
        private Button          btnDesconciliar;
        private Button          btnFinalizar;
        private Button          btnExportar;
        private Label           lblContadorPendExt;
        private Label           lblContadorPendExtr;
        private Label           lblContadorConcil;

        private TabControl      tabControl;
        private TabPage         tabPendientes;
        private TabPage         tabConciliados;

        private SplitContainer  splitPendientes;
        private Panel           pnlPendExt;
        private Label           lblPendExt;

        private RadGridView     dgvExternoPendiente;
        private TabControl      tabsPendExtracto;
        private TabPage         tabConConcepto;
        private TabPage         tabSinConcepto;
        private RadGridView     dgvExtractoPendiente;
        private RadGridView     dgvExtractoSinConcepto;
        private RadGridView     dgvConciliados;
    }
}
