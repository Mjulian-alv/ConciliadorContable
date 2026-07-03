using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Telerik.WinControls.UI;
using LiquidacionesAuditar.Models;

namespace LiquidacionesAuditar
{
    /// <summary>
    /// Diálogo simple para elegir una marca destino al clonar columnas.
    /// </summary>
    public class FormSeleccionarMarca : RadForm
    {
        public MarcaTarjeta MarcaSeleccionada { get; private set; }

        private RadDropDownList _cmb;
        private RadButton _btnOk;
        private RadButton _btnCancelar;

        public FormSeleccionarMarca(List<MarcaTarjeta> marcas)
        {
            Text = "Seleccionar marca destino";
            ClientSize = new System.Drawing.Size(360, 120);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            var lbl = new Label
            {
                Text = "Marca destino:",
                AutoSize = true,
                Location = new System.Drawing.Point(12, 16)
            };

            _cmb = new RadDropDownList
            {
                Location = new System.Drawing.Point(12, 36),
                Size = new System.Drawing.Size(320, 24),
                DataSource = marcas,
                DisplayMember = "Nombre",
                ValueMember = "Id"
            };

            _btnOk = new RadButton
            {
                Text = "Aceptar",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(160, 76),
                Size = new System.Drawing.Size(80, 28)
            };
            _btnOk.Click += (s, e) =>
            {
                if (_cmb.SelectedIndex >= 0 && _cmb.DataSource is List<MarcaTarjeta> lst)
                    MarcaSeleccionada = lst[_cmb.SelectedIndex];
                DialogResult = DialogResult.OK;
                Close();
            };

            _btnCancelar = new RadButton
            {
                Text = "Cancelar",
                DialogResult = DialogResult.Cancel,
                Location = new System.Drawing.Point(248, 76),
                Size = new System.Drawing.Size(80, 28)
            };
            _btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            AcceptButton = _btnOk;
            CancelButton = _btnCancelar;

            Controls.AddRange(new Control[] { lbl, _cmb, _btnOk, _btnCancelar });
        }
    }
}
