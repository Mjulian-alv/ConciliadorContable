using System;
using System.Windows.Forms;
using ArcaCliente.Models;
using Telerik.WinControls.UI;

namespace ArcaCliente
{
    public partial class FormConfigPresea : Telerik.WinControls.UI.RadForm
    {
        private ConfigPresea _config;

        /// <summary>Configuraci�n resultante despu�s de Guardar (DialogResult.OK).</summary>
        public ConfigPresea Config => _config;

        public FormConfigPresea(ConfigPresea config = null)
        {
            InitializeComponent();

            // Trabajar sobre una copia para no mutar el original hasta confirmar
            _config = config is null ? new ConfigPresea() : new ConfigPresea
            {
                CodigoProveedor         = config.CodigoProveedor,
                CuentaContableProveedor = config.CuentaContableProveedor,
                CuentaIVA               = config.CuentaIVA,
                CuentaIVA2              = config.CuentaIVA2,
                CuentaDebe              = config.CuentaDebe,
                Centro                  = config.Centro,
                Provincia               = config.Provincia,
                Condicion               = config.Condicion,
                Fiscal                  = config.Fiscal,
                Imputa                  = config.Imputa,
                FormatoFecha            = config.FormatoFecha
            };

            CargarCombos();
            CargarDatos();
            InicializarBotonImportarProveedores();
        }

        /// <summary>Agrega al panel de botones un acceso para importar el maestro de proveedores PRESEA.</summary>
        private void InicializarBotonImportarProveedores()
        {
            var btn = new Telerik.WinControls.UI.RadButton();
            ((System.ComponentModel.ISupportInitialize)btn).BeginInit();
            btn.Location = new System.Drawing.Point(12, 12);
            btn.Size     = new System.Drawing.Size(216, 28);
            btn.Text     = "Importar proveedores (CSV)...";
            btn.Click   += (s, e) =>
            {
                using var f = new FormImportarProveedoresPresea();
                f.ShowDialog(this);
            };
            ((System.ComponentModel.ISupportInitialize)btn).EndInit();
            pnlBotones.Controls.Add(btn);
        }

        // ?? Init ?????????????????????????????????????????????????????????????

        private void CargarCombos()
        {
            cmbFiscal.Items.Clear();
            cmbFiscal.Items.Add(new RadListDataItem("N � No fiscal",  "N"));
            cmbFiscal.Items.Add(new RadListDataItem("S � Fiscal",     "S"));

            cmbImputa.Items.Clear();
            cmbImputa.Items.Add(new RadListDataItem("N � No imputa",                 "N"));
            cmbImputa.Items.Add(new RadListDataItem("D � Imputa despu�s",            "D"));
            cmbImputa.Items.Add(new RadListDataItem("P � Imputa contra recepciones", "P"));

            cmbFormatoFecha.Items.Clear();
            cmbFormatoFecha.Items.Add(new RadListDataItem("yyyyMMdd  (p.ej. 20250115)",  "yyyyMMdd"));
            cmbFormatoFecha.Items.Add(new RadListDataItem("ddMMyyyy  (p.ej. 15012025)",  "ddMMyyyy"));
            cmbFormatoFecha.Items.Add(new RadListDataItem("dd/MM/yyyy",                  "dd/MM/yyyy"));
        }

        private void CargarDatos()
        {
            txtCodigoProveedor.Text         = _config.CodigoProveedor;
            txtCuentaContableProveedor.Text = _config.CuentaContableProveedor;
            txtCuentaIVA.Text               = _config.CuentaIVA;
            txtCuentaIVA2.Text              = _config.CuentaIVA2;
            txtCuentaDebe.Text              = _config.CuentaDebe;
            txtCentro.Text                  = _config.Centro;
            txtProvincia.Text               = _config.Provincia;
            txtCondicion.Text               = _config.Condicion;
            SeleccionarCombo(cmbFiscal,       _config.Fiscal);
            SeleccionarCombo(cmbImputa,       _config.Imputa);
            SeleccionarCombo(cmbFormatoFecha, _config.FormatoFecha);
        }

        private static void SeleccionarCombo(RadDropDownList cmb, string valor)
        {
            foreach (RadListDataItem item in cmb.Items)
                if (item.Value?.ToString() == valor)
                { cmb.SelectedItem = item; return; }
            if (cmb.Items.Count > 0) cmb.SelectedIndex = 0;
        }

        // ?? Guardar / Cancelar ????????????????????????????????????????????????

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            GuardarDatos();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void GuardarDatos()
        {
            _config.CodigoProveedor         = txtCodigoProveedor.Text.Trim();
            _config.CuentaContableProveedor = txtCuentaContableProveedor.Text.Trim();
            _config.CuentaIVA               = txtCuentaIVA.Text.Trim();
            _config.CuentaIVA2              = txtCuentaIVA2.Text.Trim();
            _config.CuentaDebe              = txtCuentaDebe.Text.Trim();
            _config.Centro                  = txtCentro.Text.Trim();
            _config.Provincia               = txtProvincia.Text.Trim();
            _config.Condicion               = txtCondicion.Text.Trim();
            _config.Fiscal      = cmbFiscal.SelectedItem?.Value?.ToString()        ?? "N";
            _config.Imputa      = cmbImputa.SelectedItem?.Value?.ToString()        ?? "N";
            _config.FormatoFecha = cmbFormatoFecha.SelectedItem?.Value?.ToString() ?? "yyyyMMdd";
        }
    }
}
