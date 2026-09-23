// Fecha: 21/09/2026 - TAREA: 00041 - Linea: 6, 7 - Pestaña Conciliación de la pantalla PyR (maquetas 07-conciliacion-pyr-resultado*)
// Parte de FormConciliacionPyR separada para que la carga (Líneas 3 y 4) y la conciliación no
// se mezclen en un solo archivo. Los controles de la pestaña se arman en código.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ArcaCliente.Models;
using ArcaCliente.Services;
using Telerik.WinControls.UI;

namespace ArcaCliente
{
    public partial class FormConciliacionPyR
    {
        private RadPageViewPage   pageConciliacion;
        private TableLayoutPanel  tblConciliacion;
        private RadGridView       gridResumen;
        private Label             lblAlcance;
        private FlowLayoutPanel   pnlFiltro;
        private RadLabel          lblPorDirectiva;
        private RadGridView       gridResultado;
        private RadLabel          lblVacioConciliacion;

        private ResultadoConciliacionPyR _resultado;
        private EstadoConciliacionPyR?   _filtro;   // null = Todos

        private void InicializarPestanaConciliacion()
        {
            pageConciliacion = new RadPageViewPage { Text = "Conciliación" };
            pvPresea.Pages.Add(pageConciliacion);

            gridResumen = new RadGridView { Dock = DockStyle.Fill };
            ConfigurarGrillaSoloLectura(gridResumen);
            gridResumen.MasterTemplate.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;
            gridResumen.Columns.AddRange(
                Texto(nameof(ResumenCuentaPyR.CuentaTexto), "Cuenta", 260),
                Entero(nameof(ResumenCuentaPyR.Conciliados), "Conciliados"),
                Entero(nameof(ResumenCuentaPyR.Diferencias), "Diferencias"),
                Entero(nameof(ResumenCuentaPyR.SoloArca),    "Sólo ARCA"),
                Entero(nameof(ResumenCuentaPyR.SoloPresea),  "Sólo PRESEA"),
                Entero(nameof(ResumenCuentaPyR.Anuladas),    "Anuladas"),
                ImporteCol(nameof(ResumenCuentaPyR.TotalArca),   "Total ARCA"),
                ImporteCol(nameof(ResumenCuentaPyR.TotalPresea), "Total PRESEA"),
                ImporteCol(nameof(ResumenCuentaPyR.Diferencia),  "Diferencia"));

            // Label común: el RadLabel sin AutoSize dentro de la tabla no dibujaba el texto.
            lblAlcance = new Label
            {
                Dock = DockStyle.Fill, AutoSize = false, BackColor = Color.FromArgb(255, 248, 225),
                ForeColor = Color.FromArgb(122, 90, 0), BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(4, 0, 4, 0)
            };

            pnlFiltro = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Padding = new Padding(0, 2, 0, 0) };
            pnlFiltro.Controls.Add(new Label { Text = "Mostrar:", AutoSize = true, Margin = new Padding(0, 7, 6, 0) });
            AgregarFiltro("Todos", null, true);
            foreach (EstadoConciliacionPyR e in Enum.GetValues(typeof(EstadoConciliacionPyR)))
                AgregarFiltro(ItemConciliacionPyR.NombreEstado(e), e, false);
            lblPorDirectiva = new RadLabel { ForeColor = Color.Gray, Margin = new Padding(12, 7, 0, 0) };
            pnlFiltro.Controls.Add(lblPorDirectiva);

            gridResultado = new RadGridView { Dock = DockStyle.Fill };
            ConfigurarGrillaSoloLectura(gridResultado);
            gridResultado.Columns.AddRange(
                Texto(nameof(ItemConciliacionPyR.EstadoTexto),    "Estado",        130),
                Texto(nameof(ItemConciliacionPyR.CuentaNombre),   "Cuenta",        150),
                Texto(nameof(ItemConciliacionPyR.DirectivaTexto), "Dir.",          40),
                FechaCol(nameof(ItemConciliacionPyR.FechaArca),   "Fecha ARCA"),
                Texto(nameof(ItemConciliacionPyR.Cuit),           "CUIT",          95),
                Texto(nameof(ItemConciliacionPyR.Denominacion),   "Denominación",  200),
                Texto(nameof(ItemConciliacionPyR.Tipo),           "Tipo",          110),
                Texto(nameof(ItemConciliacionPyR.NumeroArca),     "Número ARCA",   110),
                ImporteCol(nameof(ItemConciliacionPyR.ImporteArca), "Importe ARCA"),
                FechaCol(nameof(ItemConciliacionPyR.FechaPresea), "Fecha PRESEA"),
                Texto(nameof(ItemConciliacionPyR.Asiento),        "Asiento",       80),
                Texto(nameof(ItemConciliacionPyR.NumeroPresea),   "Número PRESEA", 110),
                Texto(nameof(ItemConciliacionPyR.Proveedor),      "Proveedor",     150),
                ImporteCol(nameof(ItemConciliacionPyR.ImportePresea), "Importe PRESEA"),
                ImporteCol(nameof(ItemConciliacionPyR.Diferencia),    "Diferencia"));
            gridResultado.RowFormatting += GridResultado_RowFormatting;

            tblConciliacion = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            tblConciliacion.RowStyles.Add(new RowStyle(SizeType.Absolute, 84));
            tblConciliacion.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            tblConciliacion.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            tblConciliacion.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tblConciliacion.Controls.Add(gridResumen, 0, 0);
            tblConciliacion.Controls.Add(lblAlcance, 0, 1);
            tblConciliacion.Controls.Add(pnlFiltro, 0, 2);
            tblConciliacion.Controls.Add(gridResultado, 0, 3);

            lblVacioConciliacion = new RadLabel
            {
                Dock = DockStyle.Fill, AutoSize = false, TextAlignment = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic), ForeColor = Color.Gray,
                Text = "Todavía no se concilió. Tocá CONCILIAR para cruzar ARCA con los mayores cargados."
            };

            pageConciliacion.Controls.Add(tblConciliacion);
            pageConciliacion.Controls.Add(lblVacioConciliacion);
            MostrarResultado();

            // El resultado necesita alto: con la pestaña Conciliación a la vista, la grilla de ARCA
            // de arriba se achica; al volver a las otras pestañas, la pantalla vuelve a la mitad.
            pvPresea.SelectedPageChanged += (s, e) =>
                splitGrillas.SplitterDistance = pvPresea.SelectedPage == pageConciliacion
                    ? Math.Min(140, splitGrillas.Height / 2)
                    : splitGrillas.Height / 2;
        }

        private void AgregarFiltro(string texto, EstadoConciliacionPyR? estado, bool marcado)
        {
            // RadioButton con aspecto de botón: exclusivos entre sí sin código extra.
            var rb = new RadioButton
            {
                Text = texto, Appearance = Appearance.Button, AutoSize = true, Checked = marcado,
                FlatStyle = FlatStyle.System, Margin = new Padding(0, 2, 4, 0), Tag = estado
            };
            rb.CheckedChanged += (s, e) =>
            {
                if (!rb.Checked) return;
                _filtro = (EstadoConciliacionPyR?)rb.Tag;
                AplicarFiltro();
            };
            pnlFiltro.Controls.Add(rb);
        }

        // ── Acciones ─────────────────────────────────────────────────────────────

        private void BtnConciliar_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                _resultado = ConciliacionPyRService.Conciliar(_registrosArca, _mayores, _perfil.DirectivasConciliacion);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
            MostrarResultado();
            pvPresea.SelectedPage = pageConciliacion;
        }

        private void BtnDirectivas_Click(object sender, EventArgs e)
        {
            using var form = new FormDirectivasPyR(_perfil.Nombre, _perfil.DirectivasConciliacion);
            if (form.ShowDialog(this) != DialogResult.OK) return;
            _perfil.DirectivasConciliacion = form.Directivas;
            GuardarPerfil();
            DatosCambiaron("se cambiaron las directivas");
        }

        private void BtnExportar_Click(object sender, EventArgs e)
        {
            if (_resultado == null) return;
            using var dlg = new SaveFileDialog
            {
                Title    = "Exportar conciliación",
                Filter   = "Excel (*.xlsx)|*.xlsx",
                FileName = $"Conciliacion PyR {DateTime.Now:yyyy-MM-dd HHmm}.xlsx"
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                ConciliacionPyRExcelExporter.Exportar(_resultado, _perfil.Nombre, dlg.FileName);
            }
            catch (IOException ex)
            {
                // El caso típico: el mismo archivo abierto en Excel.
                MessageBox.Show($"No se pudo guardar el archivo:\n\n{ex.Message}", "Exportar",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
            MessageBox.Show($"Conciliación exportada a:\n{dlg.FileName}", "Exportar",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Llamado cada vez que cambia lo cargado. Un resultado ya mostrado caduca: nunca se deja
        /// en pantalla una conciliación que no corresponde a los datos actuales.
        /// </summary>
        private void DatosCambiaron(string motivo)
        {
            btnConciliar.Enabled = _registrosArca.Count > 0 && _mayores.Count > 0;
            if (pageConciliacion == null || _resultado == null) return;

            _resultado = null;
            MostrarResultado($"⚠ Los datos cambiaron ({motivo}): volvé a conciliar.");
        }

        // ── Mostrar ──────────────────────────────────────────────────────────────

        private void MostrarResultado(string avisoCaducado = null)
        {
            bool hay = _resultado != null;
            tblConciliacion.Visible      = hay;
            lblVacioConciliacion.Visible = !hay;
            btnExportar.Enabled          = hay;

            if (!hay)
            {
                pageConciliacion.Text = "Conciliación";
                lblVacioConciliacion.Text = avisoCaducado
                    ?? "Todavía no se concilió. Tocá CONCILIAR para cruzar ARCA con los mayores cargados.";
                lblVacioConciliacion.ForeColor = avisoCaducado != null ? Color.DarkOrange : Color.Gray;
                lblVacioConciliacion.Font = new Font("Segoe UI", 9F, avisoCaducado != null ? FontStyle.Bold : FontStyle.Italic);
                gridResultado.DataSource = null;
                gridResumen.DataSource   = null;
                return;
            }

            pageConciliacion.Text = $"Conciliación ({N(_resultado.Items.Count)})";
            gridResumen.DataSource = _resultado.Resumen;

            var avisos = _resultado.FueraDeAlcance
                .Select(kv => $"{N(kv.Value)} registros de ARCA fuera de alcance: {kv.Key}")
                .ToList();
            if (_resultado.ArcaImporteCero > 0)
                avisos.Add($"{N(_resultado.ArcaImporteCero)} registros de ARCA con importe 0 excluidos");
            lblAlcance.Text    = avisos.Count == 0 ? "ⓘ Toda la carpeta de ARCA quedó dentro del alcance." : "ⓘ " + string.Join("   ·   ", avisos);

            lblPorDirectiva.Text = "Emparejados por " + string.Join("   ",
                _resultado.PorDirectiva.Select(kv => $"Dir. {kv.Key}: {N(kv.Value)}"));

            AplicarFiltro();
        }

        private void AplicarFiltro()
        {
            if (_resultado == null) return;
            gridResultado.DataSource = _resultado.Items
                .Where(i => _filtro == null || i.Estado == _filtro)
                .OrderBy(i => i.Estado).ThenBy(i => i.Cuenta.Codigo)
                .ToList();
        }

        private void GridResultado_RowFormatting(object sender, RowFormattingEventArgs e)
        {
            if (e.RowElement.RowInfo.DataBoundItem is not ItemConciliacionPyR item) return;
            var (r, g, b) = EstadoConciliacionPyRColores.Rgb(item.Estado);
            e.RowElement.DrawFill      = true;
            e.RowElement.GradientStyle = Telerik.WinControls.GradientStyles.Solid;
            e.RowElement.BackColor     = Color.FromArgb(r, g, b);
        }

        private static GridViewDecimalColumn Entero(string campo, string titulo) =>
            new(campo) { HeaderText = titulo, Width = 80, FormatString = "{0:N0}", FormatInfo = Ar, DecimalPlaces = 0 };

        private static GridViewDecimalColumn ImporteCol(string campo, string titulo) =>
            new(campo) { HeaderText = titulo, Width = 105, FormatString = "{0:N2}", FormatInfo = Ar, TextAlignment = ContentAlignment.MiddleRight };

        private static GridViewDateTimeColumn FechaCol(string campo, string titulo) =>
            new(campo) { HeaderText = titulo, Width = 80, FormatString = "{0:dd/MM/yyyy}" };
    }
}
