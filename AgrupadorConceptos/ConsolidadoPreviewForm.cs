using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AgrupadorConceptos.Services;
using Telerik.WinControls.UI;

namespace AgrupadorConceptos
{
    /// <summary>
    /// Vista previa del consolidado antes de mandarlo a Excel: muestra exactamente las
    /// filas que va a tener el archivo, más una fila de totales para controlar la
    /// cuadratura. Los totales son sólo de la vista: el Excel se exporta sin ellos.
    ///
    /// La exportación se dispara desde acá a propósito. Si falla el guardado, la ventana
    /// queda abierta para reintentar o elegir otra ruta, en vez de perder el consolidado
    /// y obligar a rehacer todo el recorrido.
    /// </summary>
    public class ConsolidadoPreviewForm : Form
    {
        private readonly List<LineaConsolidado> _lineas;
        private readonly string _titulo;

        public ConsolidadoPreviewForm(List<LineaConsolidado> lineas, string titulo)
        {
            _lineas = lineas;
            _titulo = titulo;

            Text = "Vista previa del consolidado";
            Icon = AppIcon.GetIcon();
            ClientSize = new Size(780, 470);
            MinimumSize = new Size(560, 340);
            StartPosition = FormStartPosition.CenterParent;

            // El Fill va primero: WinForms dockea de mayor a menor índice, así que el que
            // se agrega primero es el último en dockear y se queda con lo que sobra.
            Controls.Add(ConstruirGrilla());
            Controls.Add(ConstruirBotonera());
            Controls.Add(ConstruirEncabezado());
        }

        private Control ConstruirEncabezado() => new Label
        {
            Text = _titulo,
            Dock = DockStyle.Top,
            Height = 36,
            Padding = new Padding(12, 0, 12, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            AutoEllipsis = true
        };

        private Control ConstruirGrilla()
        {
            var grilla = new RadGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoScroll = true,
                ShowGroupPanel = false,
                ShowRowHeaderColumn = false
            };

            grilla.MasterTemplate.AllowAddNewRow = false;
            grilla.MasterTemplate.AllowDeleteRow = false;
            grilla.MasterTemplate.AllowEditRow = false;
            grilla.MasterTemplate.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;

            // Las columnas no existen hasta que el binding termina: la grilla todavía no
            // está en el form, así que recorrerlas justo después de asignar el DataSource
            // no encuentra ninguna y el formato se pierde.
            grilla.DataBindingComplete += (s, e) => FormatearImportes(grilla);
            grilla.DataSource = _lineas;

            AgregarFilaDeTotales(grilla);
            return grilla;
        }

        private static void FormatearImportes(RadGridView grilla)
        {
            foreach (var col in grilla.Columns)
            {
                if (col.Name == "Concepto") continue;

                col.FormatString = "{0:N2}";
                col.TextAlignment = ContentAlignment.MiddleRight;
            }
        }

        /// <summary>
        /// Fila de totales al pie, con el mecanismo nativo de la grilla. No se agrega una
        /// fila más a la lista: así no hay riesgo de que los totales se cuelen en el Excel.
        /// </summary>
        private static void AgregarFilaDeTotales(RadGridView grilla)
        {
            var fila = new GridViewSummaryRowItem(new[]
            {
                // Count y no None: con None la celda no se dibuja y la etiqueta se pierde.
                // El FormatString no tiene placeholder, así que el conteo no llega a verse.
                new GridViewSummaryItem("Concepto", "Totales", GridAggregateFunction.Count),
                new GridViewSummaryItem("Debitos",  "{0:N2}",  GridAggregateFunction.Sum),
                new GridViewSummaryItem("Creditos", "{0:N2}",  GridAggregateFunction.Sum),
                new GridViewSummaryItem("Saldo",    "{0:N2}",  GridAggregateFunction.Sum)
            });

            grilla.SummaryRowsBottom.Add(fila);
        }

        private Control ConstruirBotonera()
        {
            var btnCerrar = new Button
            {
                Text = "Cerrar",
                Size = new Size(110, 30),
                Margin = new Padding(8, 0, 0, 0)
            };
            btnCerrar.Click += (s, e) => Close();

            var btnExportar = new Button
            {
                Text = "Exportar a Excel",
                Size = new Size(150, 30),
                Margin = new Padding(0)
            };
            btnExportar.Click += BtnExportar_Click;

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 52,
                Padding = new Padding(12, 11, 12, 11)
            };
            panel.Controls.Add(btnCerrar);
            panel.Controls.Add(btnExportar);

            AcceptButton = btnExportar;
            CancelButton = btnCerrar;
            return panel;
        }

        private void BtnExportar_Click(object sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog { Filter = "Archivos de Excel (*.xlsx)|*.xlsx", FileName = "Consolidado.xlsx" };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                ConsolidadoExporter.ExportarAExcel(_lineas, _titulo, sfd.FileName);
                MessageBox.Show("Consolidado exportado a Excel exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
