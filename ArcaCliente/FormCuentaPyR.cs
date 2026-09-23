// Fecha: 21/09/2026 - TAREA: 00041 - Linea: 2 - Alta/edición de una cuenta de percepción o retención (maqueta 02-cuenta-pyr)
// Diálogo chico: los controles se arman en código, sin archivo Designer, como las secciones
// agregadas a mano en FormPerfilOfflineDetalle.
using System;
using System.Drawing;
using System.Windows.Forms;
using ArcaCliente.Models;
using Telerik.WinControls.UI;

namespace ArcaCliente
{
    public class FormCuentaPyR : Telerik.WinControls.UI.RadForm
    {
        private readonly RadTextBox      txtCodigo   = new();
        private readonly RadTextBox      txtNombre   = new();
        private readonly RadRadioButton  radPercepcion = new();
        private readonly RadRadioButton  radRetencion  = new();
        private readonly RadDropDownList cmbImpuesto = new();

        /// <summary>Cuenta resultante (una copia: la original no se toca si se cancela).</summary>
        public CuentaPyR Cuenta { get; private set; }

        public FormCuentaPyR(CuentaPyR cuenta = null)
        {
            Cuenta = cuenta?.Clonar() ?? new CuentaPyR();

            Text            = cuenta == null ? "Nueva cuenta de percepción / retención" : "Cuenta de percepción / retención";
            Icon            = AppIcons.Arca;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;
            ClientSize      = new Size(430, 214);

            Etiqueta("Código de cuenta:", 16);
            txtCodigo.Location = new Point(130, 13);
            txtCodigo.Size     = new Size(140, 24);

            Etiqueta("Nombre:", 48);
            txtNombre.Location = new Point(130, 45);
            txtNombre.Size     = new Size(280, 24);

            Etiqueta("Tipo:", 80);
            radPercepcion.Text     = "Percepción";
            radPercepcion.Location = new Point(130, 80);
            radRetencion.Text      = "Retención";
            radRetencion.Location  = new Point(236, 80);

            Etiqueta("Impuesto:", 112);
            cmbImpuesto.Location      = new Point(130, 109);
            cmbImpuesto.Size          = new Size(140, 24);
            cmbImpuesto.DropDownStyle = Telerik.WinControls.RadDropDownStyle.DropDownList;
            foreach (ImpuestoPyR imp in Enum.GetValues(typeof(ImpuestoPyR)))
                cmbImpuesto.Items.Add(new RadListDataItem(CuentaPyR.NombreImpuesto(imp), imp));

            var ayuda = new RadLabel
            {
                Location  = new Point(14, 144),
                ForeColor = Color.DimGray,
                Text      = "El código es el de la cuenta contable en PRESEA. No puede repetirse dentro del perfil."
            };

            var btnAceptar  = new RadButton { Text = "Aceptar",  Location = new Point(206, 174), Size = new Size(100, 28) };
            var btnCancelar = new RadButton { Text = "Cancelar", Location = new Point(314, 174), Size = new Size(100, 28) };
            btnAceptar.Font   = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnAceptar.Click += BtnAceptar_Click;
            btnCancelar.DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[] { txtCodigo, txtNombre, radPercepcion, radRetencion, cmbImpuesto, ayuda, btnAceptar, btnCancelar });
            CancelButton = btnCancelar;

            txtCodigo.Text          = Cuenta.Codigo ?? string.Empty;
            txtNombre.Text          = Cuenta.Nombre ?? string.Empty;
            radPercepcion.IsChecked = Cuenta.Tipo == TipoOperacionPyR.Percepcion;
            radRetencion.IsChecked  = Cuenta.Tipo == TipoOperacionPyR.Retencion;
            foreach (RadListDataItem item in cmbImpuesto.Items)
                if (item.Value is ImpuestoPyR v && v == Cuenta.Impuesto) cmbImpuesto.SelectedItem = item;
        }

        private void Etiqueta(string texto, int y) =>
            Controls.Add(new RadLabel { Text = texto, Location = new Point(14, y) });

        private void BtnAceptar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCodigo.Text))
            {
                MessageBox.Show("Ingresá el código de la cuenta.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCodigo.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Ingresá el nombre de la cuenta.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return;
            }

            Cuenta.Codigo   = txtCodigo.Text.Trim();
            Cuenta.Nombre   = txtNombre.Text.Trim();
            Cuenta.Tipo     = radRetencion.IsChecked ? TipoOperacionPyR.Retencion : TipoOperacionPyR.Percepcion;
            Cuenta.Impuesto = cmbImpuesto.SelectedItem?.Value is ImpuestoPyR imp ? imp : ImpuestoPyR.Iva;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
