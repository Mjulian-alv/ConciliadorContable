// Fecha: 21/09/2026 - TAREA: 00041 - Linea: 6 - Detalle de una directiva PyR (maqueta 06-directiva-pyr-detalle)
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ArcaCliente.Models;
using Telerik.WinControls.UI;

namespace ArcaCliente
{
    public class FormDirectivaPyRDetalle : Telerik.WinControls.UI.RadForm
    {
        private readonly RadTextBox txtDescripcion = new();
        private readonly Dictionary<CampoPyR, RadCheckBox> _casillas = new();

        /// <summary>Directiva resultante (copia: la original no se toca si se cancela).</summary>
        public DirectivaPyR Directiva { get; private set; }

        public FormDirectivaPyRDetalle(DirectivaPyR directiva = null)
        {
            Directiva = directiva?.Clonar() ?? new DirectivaPyR { Descripcion = "Nueva directiva" };

            Text            = "Directiva de conciliación";
            Icon            = AppIcons.Arca;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;
            ClientSize      = new Size(470, 300);

            Controls.Add(new RadLabel { Text = "Descripción:", Location = new Point(14, 17) });
            txtDescripcion.Location = new Point(120, 14);
            txtDescripcion.Size     = new Size(334, 24);
            txtDescripcion.Text     = Directiva.Descripcion ?? string.Empty;

            var grp = new GroupBox { Text = "Campos que tienen que coincidir", Location = new Point(12, 48), Size = new Size(446, 200) };
            int y = 24;
            foreach (CampoPyR campo in Enum.GetValues(typeof(CampoPyR)))
            {
                var chk = new RadCheckBox
                {
                    Text     = DirectivaPyR.NombreCampoLargo(campo),
                    Location = new Point(14, y),
                    Checked  = Directiva.Campos.Contains(campo)
                };
                _casillas[campo] = chk;
                grp.Controls.Add(chk);
                y += 28;
            }
            grp.Controls.Add(new RadLabel
            {
                AutoSize  = false,
                Location  = new Point(14, y + 2),
                Size      = new Size(420, 34),
                ForeColor = Color.DimGray,
                Text      = "Si a una fila le falta alguno de estos valores (por ejemplo, una minuta no tiene número), esta directiva no la empareja."
            });

            var btnAceptar  = new RadButton { Text = "Aceptar",  Location = new Point(246, 260), Size = new Size(100, 28) };
            var btnCancelar = new RadButton { Text = "Cancelar", Location = new Point(354, 260), Size = new Size(100, 28), DialogResult = DialogResult.Cancel };
            btnAceptar.Font   = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnAceptar.Click += BtnAceptar_Click;

            Controls.AddRange(new Control[] { txtDescripcion, grp, btnAceptar, btnCancelar });
            CancelButton = btnCancelar;
        }

        private void BtnAceptar_Click(object sender, EventArgs e)
        {
            var campos = _casillas.Where(kv => kv.Value.Checked).Select(kv => kv.Key).ToList();
            if (campos.Count == 0)
            {
                MessageBox.Show("Elegí al menos un campo que tenga que coincidir.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtDescripcion.Text))
            {
                MessageBox.Show("Ingresá una descripción.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDescripcion.Focus();
                return;
            }

            Directiva.Descripcion = txtDescripcion.Text.Trim();
            Directiva.Campos      = campos;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
