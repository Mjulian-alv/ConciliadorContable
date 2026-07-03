using System;
using System.Drawing;
using System.Windows.Forms;

namespace AgrupadorConceptos
{
    /// <summary>Pide al usuario el nombre para una nueva sesión de conciliación.</summary>
    public class NombreSesionDialog : Form
    {
        private TextBox txtNombre;
        private Button  btnOk;
        private Button  btnCancel;

        public string NombreSesion => txtNombre.Text.Trim();

        public NombreSesionDialog(string nombreSugerido)
        {
            Text            = "Nueva sesión";
            ClientSize      = new Size(400, 110);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;

            var lbl = new Label { Text = "Nombre de la sesión:", Location = new Point(12, 15), AutoSize = true };

            txtNombre = new TextBox { Location = new Point(12, 35), Width = 374, Text = nombreSugerido };
            txtNombre.SelectAll();

            btnOk = new Button
            { Text = "Crear", Location = new Point(230, 70), Size = new Size(75, 26),
              DialogResult = DialogResult.OK };
            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtNombre.Text))
                { MessageBox.Show("Ingrese un nombre.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                DialogResult = DialogResult.OK;
                Close();
            };

            btnCancel = new Button
            { Text = "Cancelar", Location = new Point(311, 70), Size = new Size(75, 26),
              DialogResult = DialogResult.Cancel };

            AcceptButton = btnOk;
            CancelButton = btnCancel;
            Controls.AddRange(new Control[] { lbl, txtNombre, btnOk, btnCancel });
        }
    }
}
