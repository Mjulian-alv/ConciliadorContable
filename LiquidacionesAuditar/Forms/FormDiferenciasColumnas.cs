using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Telerik.WinControls.UI;
using LiquidacionesAuditar.Data;
using LiquidacionesAuditar.Models;

namespace LiquidacionesAuditar
{
    /// <summary>
    /// Muestra diferencias entre columnas del archivo CSV y las registradas en la BD.
    /// Permite agregar las nuevas y eliminar las que ya no existen.
    /// </summary>
    public class FormDiferenciasColumnas : RadForm
    {
        private readonly List<string> _soloEnArchivo;
        private readonly List<string> _soloEnBD;
        private readonly MarcaTarjeta _marca;

        public FormDiferenciasColumnas(List<string> soloEnArchivo, List<string> soloEnBD, MarcaTarjeta marca)
        {
            _soloEnArchivo = soloEnArchivo;
            _soloEnBD = soloEnBD;
            _marca = marca;
            BuildUI();
        }

        private CheckedListBox _lstNuevas;
        private CheckedListBox _lstEliminadas;

        private void BuildUI()
        {
            this.Text = "Diferencias de Columnas";
            this.Size = new Size(760, 520);
            this.StartPosition = FormStartPosition.CenterParent;

            var lblInfo = new Label
            {
                Text = $"Se detectaron diferencias para la marca: {_marca.Nombre}",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(12, 12),
                AutoSize = true
            };

            var grpNuevas = new GroupBox
            {
                Text = $"🟢 Columnas NUEVAS en el archivo ({_soloEnArchivo.Count})  — Marcar para AGREGAR a BD",
                Location = new Point(12, 40),
                Size = new Size(350, 380),
                ForeColor = Color.DarkGreen,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            _lstNuevas = new CheckedListBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9) };
            foreach (var c in _soloEnArchivo) _lstNuevas.Items.Add(c, true);
            grpNuevas.Controls.Add(_lstNuevas);

            var grpEliminadas = new GroupBox
            {
                Text = $"🔴 Columnas que YA NO están en el archivo ({_soloEnBD.Count})  — Marcar para ELIMINAR de BD",
                Location = new Point(378, 40),
                Size = new Size(356, 380),
                ForeColor = Color.DarkRed,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            _lstEliminadas = new CheckedListBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9) };
            foreach (var c in _soloEnBD) _lstEliminadas.Items.Add(c, false);
            grpEliminadas.Controls.Add(_lstEliminadas);

            var btnAplicar = new Button
            {
                Text = "Aplicar cambios seleccionados",
                Location = new Point(12, 435),
                Size = new Size(220, 32),
                Font = new Font("Segoe UI", 10)
            };
            btnAplicar.Click += BtnAplicar_Click;

            var btnCerrar = new Button
            {
                Text = "Cerrar sin cambios",
                Location = new Point(242, 435),
                Size = new Size(160, 32),
                Font = new Font("Segoe UI", 10)
            };
            btnCerrar.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblInfo, grpNuevas, grpEliminadas, btnAplicar, btnCerrar });
        }

        private void BtnAplicar_Click(object sender, EventArgs e)
        {
            int agregadas = 0, eliminadas = 0;

            // Agregar las marcadas en "nuevas"
            for (int i = 0; i < _lstNuevas.Items.Count; i++)
            {
                if (_lstNuevas.GetItemChecked(i))
                {
                    var nombre = _lstNuevas.Items[i].ToString();
                    Repositorio.InsertColumnaCSV(new ColumnaCSV
                    {
                        IdMarcaTarjeta = _marca.Id,
                        IdColumnaArchivo = nombre
                    });
                    agregadas++;
                }
            }

            // Eliminar las marcadas en "eliminadas"
            var colsBD = Repositorio.GetColumnasCSV(_marca.Id);
            for (int i = 0; i < _lstEliminadas.Items.Count; i++)
            {
                if (_lstEliminadas.GetItemChecked(i))
                {
                    var nombre = _lstEliminadas.Items[i].ToString();
                    var col = colsBD.Find(c => c.IdColumnaArchivo.Equals(nombre, StringComparison.OrdinalIgnoreCase));
                    if (col != null) { Repositorio.DeleteColumnaCSV(col.Id); eliminadas++; }
                }
            }

            MessageBox.Show($"Se aplicaron los cambios:\n• {agregadas} columnas agregadas\n• {eliminadas} columnas eliminadas",
                "Cambios aplicados", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}
