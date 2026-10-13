// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Operar sobre días tipados evita reinterpretaciones culturales.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;

namespace AgrupadorConceptos
{
    /// <summary>
    /// Cuando hay múltiples movimientos del extracto que coinciden con un ítem externo,
    /// este diálogo permite al usuario elegir a cuál asignarlo.
    /// </summary>
    public class SeleccionCandidatoDialog : Form
    {
        private DataGridView dgv;
        private Button btnAsignar;
        private Button btnOmitir;

        public MovimientoProcesado MovimientoSeleccionado { get; private set; }

        public SeleccionCandidatoDialog(ConciliacionItemExterno externo,
                                        List<MovimientoProcesado> candidatos)
        {
            Text            = "Seleccionar movimiento a conciliar";
            ClientSize      = new Size(700, 380);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;

            var lblInfo = new Label
            {
                Text      = $"El ítem externo  [{externo.Fecha:dd/MM/yyyy}  ${externo.Importe:N2}  {externo.Detalle}]  " +
                            $"tiene {candidatos.Count} movimientos candidatos en el extracto.\n" +
                            "Seleccione a cuál asignarlo (o Omitir para dejarlo pendiente).",
                Location  = new Point(12, 10),
                Size      = new Size(676, 46),
                Font      = new Font("Segoe UI", 9F)
            };

            dgv = new DataGridView
            {
                Location             = new Point(12, 65),
                Size                 = new Size(676, 255),
                ReadOnly             = true,
                AllowUserToAddRows   = false,
                SelectionMode        = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns  = true,
                AutoSizeColumnsMode  = DataGridViewAutoSizeColumnsMode.AllCells,
                RowHeadersVisible    = false,
                BackgroundColor      = Color.White
            };

            var display = candidatos.ConvertAll(m => new
            {
                m.Id,
                m.Fecha,
                Importe = ComparadorConciliacion.ImporteEfectivo(m),
                m.ConceptoFinal
            });
            // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Mantener el formato de fechas tipadas al renovar los datos.
            dgv.CellFormatting += (sender, e) => { if(dgv.Columns[e.ColumnIndex].Name=="Fecha" && e.Value is DateTime fecha) {e.Value=fecha.ToString("dd/MM/yyyy",System.Globalization.CultureInfo.InvariantCulture);e.FormattingApplied=true;} };
            dgv.DataSource = display;
            dgv.CellDoubleClick += (s, e) => AsignarSeleccionado(candidatos);

            btnAsignar = new Button
            { Text = "Asignar", Location = new Point(510, 333), Size = new Size(85, 28) };
            btnAsignar.Click += (s, e) => AsignarSeleccionado(candidatos);

            btnOmitir = new Button
            { Text = "Omitir", Location = new Point(603, 333), Size = new Size(85, 28),
              DialogResult = DialogResult.Cancel };

            Controls.AddRange(new Control[] { lblInfo, dgv, btnAsignar, btnOmitir });
            CancelButton = btnOmitir;
        }

        private void AsignarSeleccionado(List<MovimientoProcesado> candidatos)
        {
            if (dgv.SelectedRows.Count == 0) return;
            int id = (int)dgv.SelectedRows[0].Cells["Id"].Value;
            MovimientoSeleccionado = candidatos.Find(m => m.Id == id);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
