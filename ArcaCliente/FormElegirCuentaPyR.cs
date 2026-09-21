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

        // Fecha: 21/09/2026 - TAREA: 00041 - Linea: 5 - mayoresCargados: cuenta → archivo ya cargado en la pantalla
        public FormElegirCuentaPyR(string archivo, IEnumerable<CuentaPyR> cuentas,
            IReadOnlyDictionary<System.Guid, string> mayoresCargados = null)
        {
            Text            = "Agregar mayor de PRESEA";
            Icon            = AppIcons.Arca;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;
            // Fecha: 21/09/2026 - TAREA: 00041 - Linea: 5 - Más ancho para el "(ya cargada: archivo)" del combo
            ClientSize      = new Size(660, 168);

            var lblArchivo = new RadLabel { Text = "Archivo:", Location = new Point(14, 18) };
            var lblNombre  = new RadLabel { Text = archivo, Location = new Point(150, 18), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            var lblCuenta  = new RadLabel { Text = "¿De qué cuenta es?", Location = new Point(14, 52) };

            cmbCuenta.Location      = new Point(150, 49);
            cmbCuenta.Size          = new Size(494, 24);
            cmbCuenta.DropDownStyle = Telerik.WinControls.RadDropDownStyle.DropDownList;
            // Fecha: 21/09/2026 - TAREA: 00041 - Linea: 5 - Arrancar en la primera cuenta sin mayor cargado
            // Antes el combo arrancaba siempre en la primera cuenta: al agregar el segundo mayor sin
            // tocarlo, iba a la misma cuenta y pedía reemplazar el primero (lo reportó el usuario).
            // Las cuentas que ya tienen archivo lo muestran, para que el reemplazo sea a propósito.
            RadListDataItem primeraLibre = null;
            foreach (var c in cuentas)
            {
                bool cargada = mayoresCargados != null && mayoresCargados.TryGetValue(c.Id, out _);
                var item = new RadListDataItem(
                    cargada ? $"{c.Descripcion}  (ya cargada: {mayoresCargados[c.Id]})" : c.Descripcion, c);
                cmbCuenta.Items.Add(item);
                if (!cargada && primeraLibre == null) primeraLibre = item;
            }
            if (primeraLibre != null) cmbCuenta.SelectedItem = primeraLibre;
            else if (cmbCuenta.Items.Count > 0) cmbCuenta.SelectedIndex = 0;

            var ayuda = new RadLabel
            {
                Text      = "Si la cuenta ya tiene un mayor cargado, se pregunta si se reemplaza.",
                Location  = new Point(150, 82),
                ForeColor = Color.DimGray
            };

            var btnAceptar  = new RadButton { Text = "Aceptar",  Location = new Point(436, 124), Size = new Size(100, 28), DialogResult = DialogResult.OK };
            var btnCancelar = new RadButton { Text = "Cancelar", Location = new Point(544, 124), Size = new Size(100, 28), DialogResult = DialogResult.Cancel };
            btnAceptar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            Controls.AddRange(new Control[] { lblArchivo, lblNombre, lblCuenta, cmbCuenta, ayuda, btnAceptar, btnCancelar });
            AcceptButton = btnAceptar;
            CancelButton = btnCancelar;
        }
    }
}
