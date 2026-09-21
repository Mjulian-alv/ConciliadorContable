// Fecha: 21/09/2026 - TAREA: 00041 - Linea: 4 - Elegir a qué cuenta del perfil corresponde un mayor de PRESEA (maqueta 03-elegir-cuenta)
// El mayor no dice de qué cuenta es: sin este paso no se sabe si sus filas son percepción o
// retención, ni de qué impuesto.
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ArcaCliente.Models;
using Telerik.WinControls.UI;

namespace ArcaCliente
{
    public class FormElegirCuentaPyR : Telerik.WinControls.UI.RadForm
    {
        private readonly RadDropDownList cmbCuenta = new();

        public CuentaPyR CuentaElegida => cmbCuenta.SelectedItem?.Value as CuentaPyR;

        public FormElegirCuentaPyR(string archivo, IEnumerable<CuentaPyR> cuentas)
        {
            Text            = "Agregar mayor de PRESEA";
            Icon            = AppIcons.Arca;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;
            ClientSize      = new Size(520, 168);

            var lblArchivo = new RadLabel { Text = "Archivo:", Location = new Point(14, 18) };
            var lblNombre  = new RadLabel { Text = archivo, Location = new Point(150, 18), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            var lblCuenta  = new RadLabel { Text = "¿De qué cuenta es?", Location = new Point(14, 52) };

            cmbCuenta.Location      = new Point(150, 49);
            cmbCuenta.Size          = new Size(354, 24);
            cmbCuenta.DropDownStyle = Telerik.WinControls.RadDropDownStyle.DropDownList;
            foreach (var c in cuentas)
                cmbCuenta.Items.Add(new RadListDataItem(c.Descripcion, c));
            if (cmbCuenta.Items.Count > 0) cmbCuenta.SelectedIndex = 0;

            var ayuda = new RadLabel
            {
                Text      = "Si la cuenta ya tiene un mayor cargado, se pregunta si se reemplaza.",
                Location  = new Point(150, 82),
                ForeColor = Color.DimGray
            };

            var btnAceptar  = new RadButton { Text = "Aceptar",  Location = new Point(296, 124), Size = new Size(100, 28), DialogResult = DialogResult.OK };
            var btnCancelar = new RadButton { Text = "Cancelar", Location = new Point(404, 124), Size = new Size(100, 28), DialogResult = DialogResult.Cancel };
            btnAceptar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            Controls.AddRange(new Control[] { lblArchivo, lblNombre, lblCuenta, cmbCuenta, ayuda, btnAceptar, btnCancelar });
            AcceptButton = btnAceptar;
            CancelButton = btnCancelar;
        }
    }
}
