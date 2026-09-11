// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Desempate manual en la conciliacion interna
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;

namespace AgrupadorConceptos
{
    /// <summary>
    /// Cuando hay múltiples movimientos del lado opuesto (crédito, si el origen es débito, o
    /// viceversa) que coinciden por importe opuesto con uno del origen, este diálogo permite
    /// elegir a cuál asignarlo.
    /// </summary>
    public class SeleccionCandidatoInternoDialog : Form
    {
        private DataGridView dgv;
        private Button btnAsignar;
        private Button btnOmitir;

        public MovimientoProcesado MovimientoSeleccionado { get; private set; }

        public SeleccionCandidatoInternoDialog(MovimientoProcesado a, List<MovimientoProcesado> candidatosB)
        {
            Text            = "Seleccionar movimiento a conciliar";
            ClientSize      = new Size(700, 380);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;

            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Mostrar el extracto de origen y de cada candidato
            // Los pools de Debitos/Creditos son globales: fecha + importe iguales ya no alcanzan
            // para que el usuario distinga a mano dos candidatos de bancos distintos.
            var extractos = ConciliacionInternaService.ObtenerNombresExtracto();
            string ExtractoDe(MovimientoProcesado m) =>
                extractos.TryGetValue(m.IdArchivo, out var nombre) ? nombre : $"Archivo #{m.IdArchivo}";

            var lblInfo = new Label
            {
                Text     = $"El movimiento  [{a.Fecha}  ${ComparadorConciliacion.ImporteEfectivo(a):N2}  {a.ConceptoFinal}]  " +
                           $"del extracto \"{ExtractoDe(a)}\"  tiene {candidatosB.Count} candidatos.\n" +
                           "Seleccione a cuál asignarlo (o Omitir para dejarlo pendiente).",
                Location = new Point(12, 10),
                Size     = new Size(676, 46),
                Font     = new Font("Segoe UI", 9F)
            };

            dgv = new DataGridView
            {
                Location            = new Point(12, 65),
                Size                = new Size(676, 255),
                ReadOnly            = true,
                AllowUserToAddRows  = false,
                SelectionMode       = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                RowHeadersVisible   = false,
                BackgroundColor     = Color.White
            };

            var display = candidatosB.ConvertAll(m => new
            {
                m.Id,
                m.Fecha,
                Importe = ComparadorConciliacion.ImporteEfectivo(m),
                m.ConceptoFinal,
                Extracto = ExtractoDe(m)
            });
            dgv.DataSource = display;
            dgv.CellDoubleClick += (s, e) => AsignarSeleccionado(candidatosB);

            btnAsignar = new Button
            { Text = "Asignar", Location = new Point(510, 333), Size = new Size(85, 28) };
            btnAsignar.Click += (s, e) => AsignarSeleccionado(candidatosB);

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
