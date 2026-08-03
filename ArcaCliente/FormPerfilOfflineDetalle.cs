using System;
using System.Windows.Forms;
using ArcaCliente.Models;
using Telerik.WinControls.UI;

namespace ArcaCliente
{
    public partial class FormPerfilOfflineDetalle : Telerik.WinControls.UI.RadForm
    {
        private PerfilOffline _perfil;
        private readonly bool _esNuevo;

        public PerfilOffline Perfil => _perfil;

        public FormPerfilOfflineDetalle(PerfilOffline perfil = null)
        {
            InitializeComponent();
            Icon = AppIcons.Arca;

            _esNuevo = perfil == null;
            _perfil  = perfil ?? new PerfilOffline { Nombre = "Nuevo perfil offline" };

            CargarCombos();
            CargarDatos();
            ActualizarVisibilidadTipoArchivo();
            ActualizarVisibilidadCabecera();

            Text = _esNuevo ? "Nuevo Perfil Offline" : $"Editar: {_perfil.Nombre}";

            InicializarCamposProveedor();
            InicializarSistemaExportacion();
        }

        // ?? Init ??????????????????????????????????????????????????????????????????

        private void CargarCombos()
        {
            cmbSeparador.Items.Clear();
            cmbSeparador.Items.Add(new RadListDataItem("; (punto y coma)",  ";"));
            cmbSeparador.Items.Add(new RadListDataItem(", (coma)",          ","));
            cmbSeparador.Items.Add(new RadListDataItem("Tab",               "\t"));
            cmbSeparador.Items.Add(new RadListDataItem("| (pipe)",          "|"));

            cmbEncoding.Items.Clear();
            cmbEncoding.Items.Add(new RadListDataItem("UTF-8",    "UTF-8"));
            cmbEncoding.Items.Add(new RadListDataItem("Latin-1",  "Latin-1"));

            cmbFormatoFecha.Items.Clear();
            cmbFormatoFecha.Items.Add(new RadListDataItem("dd/MM/yyyy",  "dd/MM/yyyy"));
            cmbFormatoFecha.Items.Add(new RadListDataItem("MM/dd/yyyy",  "MM/dd/yyyy"));
            cmbFormatoFecha.Items.Add(new RadListDataItem("yyyy-MM-dd",  "yyyy-MM-dd"));
            cmbFormatoFecha.Items.Add(new RadListDataItem("dd-MM-yyyy",  "dd-MM-yyyy"));
            cmbFormatoFecha.Items.Add(new RadListDataItem("d/M/yyyy",    "d/M/yyyy"));

            cmbSeparadorDecimal.Items.Clear();
            cmbSeparadorDecimal.Items.Add(new RadListDataItem(". (punto)", "."));
            cmbSeparadorDecimal.Items.Add(new RadListDataItem(", (coma)",  ","));
        }

        private void CargarDatos()
        {
            txtNombre.Text = _perfil.Nombre;

            radXlsx.IsChecked = _perfil.TipoArchivo == TipoArchivoOffline.Xlsx;
            radCsv.IsChecked  = _perfil.TipoArchivo == TipoArchivoOffline.Csv;
            radTxt.IsChecked  = _perfil.TipoArchivo == TipoArchivoOffline.Txt;

            SeleccionarCombo(cmbSeparador,   _perfil.Separador   ?? ";");
            SeleccionarCombo(cmbEncoding,    _perfil.Encoding    ?? "UTF-8");
            SeleccionarCombo(cmbFormatoFecha, _perfil.FormatoFecha ?? "dd/MM/yyyy");

            txtHojaExcel.Text = _perfil.HojaExcel ?? string.Empty;

            chkTieneCabecera.Checked = _perfil.TieneCabecera;

            // Nombres de columna
            txtColFecha.Text           = _perfil.ColFecha           ?? string.Empty;
            txtColPuntoVenta.Text      = _perfil.ColPuntoVenta      ?? string.Empty;
            txtColNumero.Text          = _perfil.ColNumero           ?? string.Empty;
            txtColTipoComprobante.Text = _perfil.ColTipoComprobante ?? string.Empty;
            txtColCuit.Text            = _perfil.ColCuit             ?? string.Empty;
            txtColTotal.Text           = _perfil.ColTotal            ?? string.Empty;

            // Posiciones
            spnPosFecha.Value           = _perfil.PosFecha;
            spnPosPuntoVenta.Value      = _perfil.PosPuntoVenta;
            spnPosNumero.Value          = _perfil.PosNumero;
            spnPosTipoComprobante.Value = _perfil.PosTipoComprobante;
            spnPosCuit.Value            = _perfil.PosCuit;
            spnPosTotal.Value           = _perfil.PosTotal;

            SeleccionarCombo(cmbSeparadorDecimal, _perfil.SeparadorDecimal ?? ".");
        }

        private static void SeleccionarCombo(RadDropDownList cmb, string valor)
        {
            foreach (RadListDataItem item in cmb.Items)
                if (string.Equals(item.Value?.ToString(), valor, StringComparison.OrdinalIgnoreCase))
                { cmb.SelectedItem = item; return; }
            if (cmb.Items.Count > 0) cmb.SelectedIndex = 0;
        }

        // ?? Visibilidad dinámica ??????????????????????????????????????????????????

        private void ActualizarVisibilidadTipoArchivo()
        {
            bool esCsvTxt = radCsv.IsChecked || radTxt.IsChecked;
            pnlCsvConfig.Visible  = esCsvTxt;
            pnlXlsxConfig.Visible = radXlsx.IsChecked;
        }

        private void ActualizarVisibilidadCabecera()
        {
            pnlCabecera.Visible    = chkTieneCabecera.Checked;
            pnlPosiciones.Visible  = !chkTieneCabecera.Checked;
        }

        // ?? Eventos ???????????????????????????????????????????????????????????????

        private void RadTipoArchivo_ToggleStateChanged(object sender,
            Telerik.WinControls.UI.StateChangedEventArgs args)
            => ActualizarVisibilidadTipoArchivo();

        private void ChkTieneCabecera_CheckedChanged(object sender, EventArgs e)
            => ActualizarVisibilidadCabecera();

        // ?? Guardar / Cancelar ????????????????????????????????????????????????????

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (!Validar()) return;
            GuardarDatos();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool Validar()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Ingresá un nombre para el perfil.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return false;
            }
            return true;
        }

        private void GuardarDatos()
        {
            _perfil.Nombre = txtNombre.Text.Trim();

            _perfil.TipoArchivo = radXlsx.IsChecked ? TipoArchivoOffline.Xlsx
                                : radCsv.IsChecked  ? TipoArchivoOffline.Csv
                                :                     TipoArchivoOffline.Txt;

            _perfil.Separador    = cmbSeparador.SelectedItem?.Value?.ToString()    ?? ";";
            _perfil.Encoding     = cmbEncoding.SelectedItem?.Value?.ToString()     ?? "UTF-8";
            _perfil.FormatoFecha = cmbFormatoFecha.SelectedItem?.Value?.ToString() ?? "dd/MM/yyyy";
            _perfil.HojaExcel    = string.IsNullOrWhiteSpace(txtHojaExcel.Text)
                ? null : txtHojaExcel.Text.Trim();

            _perfil.TieneCabecera = chkTieneCabecera.Checked;

            _perfil.ColFecha           = txtColFecha.Text.Trim();
            _perfil.ColPuntoVenta      = txtColPuntoVenta.Text.Trim();
            _perfil.ColNumero          = txtColNumero.Text.Trim();
            _perfil.ColTipoComprobante = txtColTipoComprobante.Text.Trim();
            _perfil.ColCuit            = txtColCuit.Text.Trim();
            _perfil.ColTotal           = txtColTotal.Text.Trim();

            _perfil.PosFecha           = (int)spnPosFecha.Value;
            _perfil.PosPuntoVenta      = (int)spnPosPuntoVenta.Value;
            _perfil.PosNumero          = (int)spnPosNumero.Value;
            _perfil.PosTipoComprobante = (int)spnPosTipoComprobante.Value;
            _perfil.PosCuit            = (int)spnPosCuit.Value;
            _perfil.PosTotal           = (int)spnPosTotal.Value;

            _perfil.SeparadorDecimal   = cmbSeparadorDecimal.SelectedItem?.Value?.ToString() ?? ".";

            _perfil.ColNombreProveedor = txtColNombreProveedor?.Text.Trim() ?? string.Empty;
            _perfil.PosNombreProveedor = (int)(spnPosNombreProveedor?.Value ?? 6m);

            _perfil.SistemaExportacion = ObtenerSistemaSeleccionado();
        }

        // ?? Campos adicionales: nombre del proveedor ??????????????????????????

        private System.Windows.Forms.GroupBox          grpCamposProveedor;
        private Telerik.WinControls.UI.RadLabel        lblColNombreProveedor;
        private Telerik.WinControls.UI.RadTextBox      txtColNombreProveedor;
        private Telerik.WinControls.UI.RadLabel        lblPosNombreProveedor;
        private Telerik.WinControls.UI.RadSpinEditor   spnPosNombreProveedor;

        private void InicializarCamposProveedor()
        {
            grpCamposProveedor    = new System.Windows.Forms.GroupBox();
            lblColNombreProveedor = new Telerik.WinControls.UI.RadLabel();
            txtColNombreProveedor = new Telerik.WinControls.UI.RadTextBox();
            lblPosNombreProveedor = new Telerik.WinControls.UI.RadLabel();
            spnPosNombreProveedor = new Telerik.WinControls.UI.RadSpinEditor();

            ((System.ComponentModel.ISupportInitialize)lblColNombreProveedor).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtColNombreProveedor).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lblPosNombreProveedor).BeginInit();
            ((System.ComponentModel.ISupportInitialize)spnPosNombreProveedor).BeginInit();

            lblColNombreProveedor.Location = new System.Drawing.Point(8, 18);
            lblColNombreProveedor.Name     = "lblColNombreProveedor";
            lblColNombreProveedor.Text     = "Columna nombre proveedor:";

            txtColNombreProveedor.Location = new System.Drawing.Point(8, 34);
            txtColNombreProveedor.Name     = "txtColNombreProveedor";
            txtColNombreProveedor.Size     = new System.Drawing.Size(244, 20);
            txtColNombreProveedor.TabIndex = 0;
            txtColNombreProveedor.Text     = _perfil.ColNombreProveedor ?? string.Empty;

            lblPosNombreProveedor.Location = new System.Drawing.Point(260, 18);
            lblPosNombreProveedor.Name     = "lblPosNombreProveedor";
            lblPosNombreProveedor.Text     = "Nombre proveedor (col Nº):";

            spnPosNombreProveedor.Location = new System.Drawing.Point(260, 34);
            spnPosNombreProveedor.Name     = "spnPosNombreProveedor";
            spnPosNombreProveedor.Size     = new System.Drawing.Size(80, 20);
            spnPosNombreProveedor.TabIndex = 1;
            spnPosNombreProveedor.Minimum  = 1;
            spnPosNombreProveedor.Maximum  = 50;
            spnPosNombreProveedor.Value    = _perfil.PosNombreProveedor;

            ((System.ComponentModel.ISupportInitialize)lblColNombreProveedor).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtColNombreProveedor).EndInit();
            ((System.ComponentModel.ISupportInitialize)lblPosNombreProveedor).EndInit();
            ((System.ComponentModel.ISupportInitialize)spnPosNombreProveedor).EndInit();

            grpCamposProveedor.Controls.Add(lblColNombreProveedor);
            grpCamposProveedor.Controls.Add(txtColNombreProveedor);
            grpCamposProveedor.Controls.Add(lblPosNombreProveedor);
            grpCamposProveedor.Controls.Add(spnPosNombreProveedor);
            grpCamposProveedor.Location = new System.Drawing.Point(12, 532);
            grpCamposProveedor.Name     = "grpCamposProveedor";
            grpCamposProveedor.Size     = new System.Drawing.Size(528, 72);
            grpCamposProveedor.TabStop  = false;
            grpCamposProveedor.Text     = "Proveedor";

            Controls.Add(grpCamposProveedor);

            chkTieneCabecera.ToggleStateChanged += (s, a) => ActualizarVisibilidadCamposProveedor();
            ActualizarVisibilidadCamposProveedor();
        }

        private void ActualizarVisibilidadCamposProveedor()
        {
            bool tieneHeader = chkTieneCabecera.Checked;
            lblColNombreProveedor.Visible = tieneHeader;
            txtColNombreProveedor.Visible = tieneHeader;
            lblPosNombreProveedor.Visible = !tieneHeader;
            spnPosNombreProveedor.Visible = !tieneHeader;
        }

        // ?? Exportación a sistema externo ?????????????????????????????????????

        private System.Windows.Forms.GroupBox          grpExportacion;
        private Telerik.WinControls.UI.RadLabel        lblSistemaExportacion;
        private Telerik.WinControls.UI.RadDropDownList cmbSistemaExportacion;
        private Telerik.WinControls.UI.RadButton       btnConfigurarSistema;

        private void InicializarSistemaExportacion()
        {
            grpExportacion        = new System.Windows.Forms.GroupBox();
            lblSistemaExportacion = new Telerik.WinControls.UI.RadLabel();
            cmbSistemaExportacion = new Telerik.WinControls.UI.RadDropDownList();
            btnConfigurarSistema  = new Telerik.WinControls.UI.RadButton();

            ((System.ComponentModel.ISupportInitialize)lblSistemaExportacion).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbSistemaExportacion).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnConfigurarSistema).BeginInit();

            cmbSistemaExportacion.Items.Add(new Telerik.WinControls.UI.RadListDataItem("Ninguno", (int)ArcaCliente.Models.SistemaExportacionOffline.Ninguno));
            cmbSistemaExportacion.Items.Add(new Telerik.WinControls.UI.RadListDataItem("PRESEA",  (int)ArcaCliente.Models.SistemaExportacionOffline.Presea));
            cmbSistemaExportacion.Location             = new System.Drawing.Point(8, 34);
            cmbSistemaExportacion.Name                 = "cmbSistemaExportacion";
            cmbSistemaExportacion.Size                 = new System.Drawing.Size(180, 24);
            cmbSistemaExportacion.TabIndex             = 0;
            cmbSistemaExportacion.SelectedIndexChanged += CmbSistemaExportacion_SelectedIndexChanged;

            lblSistemaExportacion.Location = new System.Drawing.Point(8, 18);
            lblSistemaExportacion.Name     = "lblSistemaExportacion";
            lblSistemaExportacion.Text     = "Sistema de exportación:";

            btnConfigurarSistema.Location = new System.Drawing.Point(198, 31);
            btnConfigurarSistema.Name     = "btnConfigurarSistema";
            btnConfigurarSistema.Size     = new System.Drawing.Size(150, 28);
            btnConfigurarSistema.TabIndex = 1;
            btnConfigurarSistema.Visible  = false;
            btnConfigurarSistema.Click   += BtnConfigurarSistema_Click;

            ((System.ComponentModel.ISupportInitialize)lblSistemaExportacion).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbSistemaExportacion).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnConfigurarSistema).EndInit();

            grpExportacion.Controls.Add(lblSistemaExportacion);
            grpExportacion.Controls.Add(cmbSistemaExportacion);
            grpExportacion.Controls.Add(btnConfigurarSistema);
            grpExportacion.Location = new System.Drawing.Point(12, 622);
            grpExportacion.Name     = "grpExportacion";
            grpExportacion.Size     = new System.Drawing.Size(528, 76);
            grpExportacion.TabStop  = false;
            grpExportacion.Text     = "Exportación a sistema externo";

            Controls.Add(grpExportacion);
            ClientSize = new System.Drawing.Size(ClientSize.Width, 762);

            // Cargar el valor guardado en el perfil
            foreach (Telerik.WinControls.UI.RadListDataItem item in cmbSistemaExportacion.Items)
                if (item.Value is int v && v == (int)_perfil.SistemaExportacion)
                { cmbSistemaExportacion.SelectedItem = item; break; }
            if (cmbSistemaExportacion.SelectedIndex < 0 && cmbSistemaExportacion.Items.Count > 0)
                cmbSistemaExportacion.SelectedIndex = 0;

            ActualizarBotonConfigurar();
        }

        private void CmbSistemaExportacion_SelectedIndexChanged(
            object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
            => ActualizarBotonConfigurar();

        private void ActualizarBotonConfigurar()
        {
            var sistema = ObtenerSistemaSeleccionado();
            btnConfigurarSistema.Visible = sistema != ArcaCliente.Models.SistemaExportacionOffline.Ninguno;
            if (sistema != ArcaCliente.Models.SistemaExportacionOffline.Ninguno)
                btnConfigurarSistema.Text = $"Configurar {ArcaCliente.Services.ExportadorSistemaFactory.ObtenerNombre(sistema)}...";
        }

        private ArcaCliente.Models.SistemaExportacionOffline ObtenerSistemaSeleccionado() =>
            cmbSistemaExportacion?.SelectedItem?.Value is int v
                ? (ArcaCliente.Models.SistemaExportacionOffline)v
                : ArcaCliente.Models.SistemaExportacionOffline.Ninguno;

        private void BtnConfigurarSistema_Click(object sender, EventArgs e)
        {
            if (ObtenerSistemaSeleccionado() != ArcaCliente.Models.SistemaExportacionOffline.Presea)
                return;

            using var form = new FormConfigPresea(_perfil.ConfigPresea);
            if (form.ShowDialog(this) == DialogResult.OK)
                _perfil.ConfigPresea = form.Config;
        }
    }
}
