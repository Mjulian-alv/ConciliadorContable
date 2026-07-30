using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;
using ClosedXML.Excel;
using Dapper;
using ExcelDataReader;
using Telerik.WinControls.UI;
using Telerik.WinControls;

namespace AgrupadorConceptos
{
    public partial class ConciliacionExternForm : Form
    {
        private ConciliacionSesion _sesionActiva;

        public ConciliacionExternForm()
        {
            InitializeComponent();
            Icon = AppIcon.GetIcon();
            Load += OnLoad;
        }

        // ── Carga inicial ─────────────────────────────────────────────────────────

        private void OnLoad(object sender, EventArgs e)
        {
            CargarSesiones();
            ActualizarEstadoSesion();
        }

        private void CargarSesiones()
        {
            var sesiones = ConciliacionExternService.ObtenerTodasSesiones();
            lbSesiones.DataSource    = sesiones;
            lbSesiones.DisplayMember = "DisplayName";
            lbSesiones.ValueMember   = "Id";
            btnRetomar.Enabled       = sesiones.Count > 0;
            btnEliminarSesion.Enabled = sesiones.Count > 0;
            lbSesiones.SelectedIndex = sesiones.Count > 0 ? 0 : -1;
        }

        private List<ArchivoImportado> _archivosDisponibles = new();

        private void CargarArchivosEnPanel()
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            _archivosDisponibles = cn.Query<ArchivoImportado>(
                "SELECT * FROM bancos.ArchivosImportados ORDER BY Fecha DESC").ToList();

            clbArchivos.Items.Clear();
            foreach (var a in _archivosDisponibles)
                clbArchivos.Items.Add(a.DisplayName, false);
        }

        private void CargarConceptosPorArchivos()
        {
            clbConceptos.Items.Clear();
            var seleccionados = ArchivosSeleccionados;
            if (seleccionados.Count == 0) return;

            var ids = seleccionados.Select(a => a.Id).ToList();
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            var conceptos = cn.Query<string>(
                $@"SELECT DISTINCT ConceptoFinal FROM bancos.MovimientosArchivo
                   WHERE IdArchivo IN ({string.Join(",", ids)})
                     AND ConceptoFinal IS NOT NULL AND ConceptoFinal <> ''
                   ORDER BY ConceptoFinal").ToList();

            foreach (var c in conceptos)
                clbConceptos.Items.Add(c, false);
        }

        private List<ArchivoImportado> ArchivosSeleccionados =>
            clbArchivos.CheckedIndices.Cast<int>()
                .Select(i => _archivosDisponibles[i])
                .ToList();

        // ── Selección en la lista de sesiones ────────────────────────────────────

        private void lbSesiones_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool haySel = lbSesiones.SelectedItem != null;
            btnRetomar.Enabled        = haySel;
            btnEliminarSesion.Enabled = haySel;
        }

        // ── Botones de sesión ─────────────────────────────────────────────────────

        private void btnNuevaSesion_Click(object sender, EventArgs e)
        {
            CargarArchivosEnPanel();
            txtArchivoExterno.Text = "";
            pnlConfigNueva.Visible = true;
            btnNuevaSesion.Enabled = false;
        }

        private void btnConfirmarNueva_Click(object sender, EventArgs e)
        {
            var archivosSeleccionados = ArchivosSeleccionados;
            if (archivosSeleccionados.Count == 0)
            { MessageBox.Show("Seleccione al menos un archivo importado.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var conceptosSeleccionados = clbConceptos.CheckedItems.Cast<string>().ToList();
            if (conceptosSeleccionados.Count == 0)
            { MessageBox.Show("Seleccione al menos un concepto.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (string.IsNullOrWhiteSpace(txtArchivoExterno.Text) || !File.Exists(txtArchivoExterno.Text))
            { MessageBox.Show("Cargue el archivo externo.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var items = LeerArchivoExterno(txtArchivoExterno.Text);
            if (items == null || items.Count == 0)
            { MessageBox.Show("No se encontraron registros en el archivo externo.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            string nombre = $"{string.Join(", ", archivosSeleccionados.Select(a => a.NombreArchivo))} - {DateTime.Now:dd/MM HH:mm}";
            using var dlgNombre = new NombreSesionDialog(nombre);
            if (dlgNombre.ShowDialog(this) != DialogResult.OK) return;

            _sesionActiva = ConciliacionExternService.CrearSesion(
                dlgNombre.NombreSesion, archivosSeleccionados.Select(a => a.Id).ToList(),
                conceptosSeleccionados, items);

            pnlConfigNueva.Visible = false;
            btnNuevaSesion.Enabled = true;
            CargarSesiones();
            ActualizarEstadoSesion();
            RefrescarGrillas();
        }

        private void btnCancelarNueva_Click(object sender, EventArgs e)
        {
            pnlConfigNueva.Visible = false;
            btnNuevaSesion.Enabled = true;
        }

        private void btnRetomar_Click(object sender, EventArgs e)
        {
            if (lbSesiones.SelectedItem is not ConciliacionSesion sesion) return;
            _sesionActiva = sesion;
            ActualizarEstadoSesion();
            RefrescarGrillas();
        }

        private void btnEliminarSesion_Click(object sender, EventArgs e)
        {
            if (lbSesiones.SelectedItem is not ConciliacionSesion sesion) return;

            if (MessageBox.Show($"¿Eliminar la sesión '{sesion.Nombre}' y todos sus datos?",
                    "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            ConciliacionExternService.EliminarSesion(sesion.Id);
            if (_sesionActiva?.Id == sesion.Id) { _sesionActiva = null; LimpiarGrillas(); }
            CargarSesiones();
            ActualizarEstadoSesion();
        }

        // ── Cargar archivo externo ────────────────────────────────────────────────

        private void clbArchivos_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            BeginInvoke(() => CargarConceptosPorArchivos());
        }

        private void btnCargarExterno_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Excel / CSV|*.xlsx;*.xls;*.csv",
                Title  = "Seleccionar archivo externo"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            txtArchivoExterno.Text = dlg.FileName;
        }

        // ── Auto-conciliación ─────────────────────────────────────────────────────

        private void btnAutoConciliar_Click(object sender, EventArgs e)
        {
            if (_sesionActiva == null) { MessageBox.Show("No hay sesión activa.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var (conciliados, duplicados) = ConciliacionExternService.AutoConciliar(_sesionActiva.Id);

            // Resolver duplicados uno a uno
            foreach (var (externo, candidatos) in duplicados)
            {
                using var dlg = new SeleccionCandidatoDialog(externo, candidatos);
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.MovimientoSeleccionado != null)
                {
                    ConciliacionExternService.ConciliarPar(
                        _sesionActiva.Id, externo.Id, dlg.MovimientoSeleccionado.Id, TipoMatch.SoloImporte);
                    conciliados++;
                }
            }

            RefrescarGrillas();
            MessageBox.Show($"Auto-conciliación completada: {conciliados} par(es) conciliado(s).",
                "Resultado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Conciliación manual ───────────────────────────────────────────────────

        private ConciliacionItemExterno _externoSeleccionado;

        private void dgvExternoPendiente_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvExternoPendiente.CurrentRow == null) return;
            _externoSeleccionado = dgvExternoPendiente.CurrentRow.DataBoundItem as ConciliacionItemExterno;
            ResaltarCandidatosEnExtracto();
        }

        private void ResaltarCandidatosEnExtracto()
        {
            if (_externoSeleccionado == null || _sesionActiva == null) return;

            // Guardamos referencia para usar en RowFormatting
            dgvExtractoPendiente.RowFormatting -= dgvExtractoPendiente_RowFormatting;
            dgvExtractoPendiente.RowFormatting += dgvExtractoPendiente_RowFormatting;
            dgvExtractoPendiente.TableElement.BeginUpdate();
            dgvExtractoPendiente.TableElement.EndUpdate();
        }

        private void dgvExtractoPendiente_RowFormatting(object sender, RowFormattingEventArgs e)
        {
            if (_externoSeleccionado == null || e.RowElement.RowInfo.DataBoundItem is not MovimientoProcesado m)
            {
                e.RowElement.ResetValue(LightVisualElement.BackColorProperty, Telerik.WinControls.ValueResetFlags.Local);
                return;
            }

            bool fechaMatch   = ComparadorConciliacion.FechasIguales(_externoSeleccionado.Fecha, m.Fecha);
            bool importeMatch = Math.Abs(_externoSeleccionado.Importe) == ComparadorConciliacion.ImporteEfectivo(m);

            if (fechaMatch && importeMatch)
                e.RowElement.BackColor = Color.LightGreen;
            else if (importeMatch)
                e.RowElement.BackColor = Color.LightYellow;
            else
                e.RowElement.ResetValue(LightVisualElement.BackColorProperty, Telerik.WinControls.ValueResetFlags.Local);
        }

        private void btnConciliarManual_Click(object sender, EventArgs e)
        {
            if (_sesionActiva == null || _externoSeleccionado == null)
            { MessageBox.Show("Seleccione un ítem del archivo externo.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (dgvExtractoPendiente.CurrentRow == null)
            { MessageBox.Show("Seleccione un movimiento del extracto.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var movimiento = dgvExtractoPendiente.CurrentRow.DataBoundItem as MovimientoProcesado;
            if (movimiento == null) return;

            bool fechaMatch   = ComparadorConciliacion.FechasIguales(_externoSeleccionado.Fecha, movimiento.Fecha);
            bool importeMatch = Math.Abs(_externoSeleccionado.Importe) == ComparadorConciliacion.ImporteEfectivo(movimiento);
            var tipoMatch = TipoMatch.Manual; //(fechaMatch && importeMatch) ? TipoMatch.FechaImporte : TipoMatch.SoloImporte;

            ConciliacionExternService.ConciliarPar(
                _sesionActiva.Id, _externoSeleccionado.Id, movimiento.Id, tipoMatch);

            _externoSeleccionado = null;
            RefrescarGrillas();
        }

        private void btnDesconciliar_Click(object sender, EventArgs e)
        {
            if (dgvConciliados.CurrentRow == null) return;
            var par = dgvConciliados.CurrentRow.DataBoundItem as ConciliacionPar;
            if (par == null) return;

            ConciliacionExternService.DesconciliarPar(par.Id);
            RefrescarGrillas();
        }

        private void btnFinalizar_Click(object sender, EventArgs e)
        {
            if (_sesionActiva == null) return;
            var pendExt = ConciliacionExternService.ObtenerItemsPendientes(_sesionActiva.Id);
            if (pendExt.Count > 0 &&
                MessageBox.Show($"Aún quedan {pendExt.Count} ítems externos sin conciliar. ¿Finalizar igual?",
                    "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            ConciliacionExternService.MarcarFinalizada(_sesionActiva.Id);
            _sesionActiva.Estado = "Finalizada";
            ActualizarEstadoSesion();
            CargarSesiones();
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            if (_sesionActiva == null) return;

            using var dlg = new SaveFileDialog
            { Filter = "Excel|*.xlsx", FileName = $"Conciliacion_{DateTime.Now:yyyyMMdd_HHmm}.xlsx" };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            var pares     = ConciliacionExternService.ObtenerPares(_sesionActiva.Id);
            var pendExt   = ConciliacionExternService.ObtenerItemsPendientes(_sesionActiva.Id);
            var pendExtr  = ConciliacionExternService.ObtenerMovimientosPendientes(_sesionActiva.Id);

            using var wb = new XLWorkbook();
            var wsCon = wb.Worksheets.Add("Conciliados");
            wsCon.Cell(1,1).Value = "Fecha Externo"; wsCon.Cell(1,2).Value = "Importe Externo";
            wsCon.Cell(1,3).Value = "Detalle";       wsCon.Cell(1,4).Value = "Fecha Extracto";
            wsCon.Cell(1,5).Value = "Importe Extracto"; wsCon.Cell(1,6).Value = "Concepto Final";
            wsCon.Cell(1,7).Value = "Tipo Match";
            for (int i = 0; i < pares.Count; i++)
            {
                var p = pares[i]; int r = i + 2;
                wsCon.Cell(r,1).Value = p.FechaExterno;   wsCon.Cell(r,2).Value = (double)p.ImporteExterno;
                wsCon.Cell(r,3).Value = p.DetalleExterno; wsCon.Cell(r,4).Value = p.FechaExtracto;
                wsCon.Cell(r,5).Value = (double)p.ImporteExtracto; wsCon.Cell(r,6).Value = p.ConceptoFinalExtracto;
                wsCon.Cell(r,7).Value = p.TipoMatch;
            }

            var wsPend = wb.Worksheets.Add("Pendientes");
            wsPend.Cell(1,1).Value = "Origen"; wsPend.Cell(1,2).Value = "Fecha";
            wsPend.Cell(1,3).Value = "Importe"; wsPend.Cell(1,4).Value = "Detalle / Concepto";
            int rp = 2;
            foreach (var x in pendExt)
            { wsPend.Cell(rp,1).Value="Externo"; wsPend.Cell(rp,2).Value=x.Fecha; wsPend.Cell(rp,3).Value=(double)x.Importe; wsPend.Cell(rp,4).Value=x.Detalle; rp++; }
            foreach (var m in pendExtr)
            { wsPend.Cell(rp,1).Value="Extracto"; wsPend.Cell(rp,2).Value=m.Fecha; wsPend.Cell(rp,3).Value=(double)ComparadorConciliacion.ImporteEfectivo(m); wsPend.Cell(rp,4).Value=m.ConceptoFinal; rp++; }

            wb.SaveAs(dlg.FileName);
            MessageBox.Show("Exportado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Helpers de UI ─────────────────────────────────────────────────────────

        private void RefrescarGrillas()
        {
            if (_sesionActiva == null) { LimpiarGrillas(); return; }

            // Tab pendientes — externo
            dgvExternoPendiente.DataSource = ConciliacionExternService.ObtenerItemsPendientes(_sesionActiva.Id);
            dgvExternoPendiente.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;
            // Tab pendientes — extracto
            dgvExtractoPendiente.DataSource = ConciliacionExternService.ObtenerMovimientosPendientes(_sesionActiva.Id);
            dgvExtractoPendiente.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;

            // Pendientes sin concepto (restante del extracto)
            dgvExtractoSinConcepto.DataSource = ConciliacionExternService.ObtenerMovimientosSinConcepto(_sesionActiva.Id);
            dgvExtractoSinConcepto.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;

            // Tab conciliados
            dgvConciliados.DataSource = ConciliacionExternService.ObtenerPares(_sesionActiva.Id);
            dgvConciliados.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;

            ActualizarContadores();
        }

        private void LimpiarGrillas()
        {
            dgvExternoPendiente.DataSource    = null;
            dgvExtractoPendiente.DataSource   = null;
            dgvExtractoSinConcepto.DataSource = null;
            dgvConciliados.DataSource         = null;
            lblContadorPendExt.Text  = "";
            lblContadorPendExtr.Text = "";
            lblContadorConcil.Text   = "";
        }

        private void ActualizarContadores()
        {
            if (_sesionActiva == null) return;
            int pendExt  = dgvExternoPendiente.RowCount;
            int pendExtr = dgvExtractoPendiente.RowCount;
            int concil   = dgvConciliados.RowCount;

            lblContadorPendExt.Text  = $"Externo: {pendExt} pendientes";
            lblContadorPendExtr.Text = $"Extracto: {pendExtr} pendientes";
            lblContadorConcil.Text   = $"{concil} pares conciliados";
        }

        private void ActualizarEstadoSesion()
        {
            bool activa = _sesionActiva != null;
            lblSesionActiva.Text    = activa ? $"Sesión: {_sesionActiva.Nombre} [{_sesionActiva.Estado}]" : "Sin sesión activa";
            btnAutoConciliar.Enabled  = activa;
            btnConciliarManual.Enabled = activa;
            btnDesconciliar.Enabled    = activa;
            btnFinalizar.Enabled       = activa;
            btnExportar.Enabled        = activa;
        }

        // ── Lectura del archivo externo ───────────────────────────────────────────

        private List<ConciliacionItemExterno> LeerArchivoExterno(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            try
            {
                return ext == ".csv" ? LeerCsv(path) : LeerExcel(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al leer el archivo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private List<ConciliacionItemExterno> LeerExcel(string path)
        {
            var result = new List<ConciliacionItemExterno>();
            using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var ds = reader.AsDataSet(new ExcelDataSetConfiguration
            { ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true } });

            var table = ds.Tables[0];
            int iF = BuscarColumna(table, "fecha");
            int iI = BuscarColumna(table, "importe");
            int iD = BuscarColumna(table, "concepto");

            if (iF < 0 || iI < 0 || iD < 0)
            { MessageBox.Show("El archivo debe tener columnas Fecha, Importe y Concepto.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return null; }

            foreach (System.Data.DataRow row in table.Rows)
            {
                if (row[iI] == DBNull.Value) continue;
                result.Add(new ConciliacionItemExterno
                {
                    Fecha   = row[iF]?.ToString()?.Trim(),
                    Importe = ParseImporte(row[iI]?.ToString()),
                    Detalle = row[iD]?.ToString()?.Trim()
                });
            }
            return result;
        }

        private List<ConciliacionItemExterno> LeerCsv(string path)
        {
            var result = new List<ConciliacionItemExterno>();
            var lines  = File.ReadAllLines(path);
            if (lines.Length < 2) return result;

            char sep = lines[0].Contains(';') ? ';' : ',';
            var headers = lines[0].Split(sep);
            int iF = BuscarColumnaArr(headers, "fecha");
            int iI = BuscarColumnaArr(headers, "importe");
            int iD = BuscarColumnaArr(headers, "concepto");

            if (iF < 0 || iI < 0 || iD < 0)
            { MessageBox.Show("El archivo CSV debe tener columnas Fecha, Importe y Concepto.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return null; }

            for (int i = 1; i < lines.Length; i++)
            {
                var cols = lines[i].Split(sep);
                if (cols.Length <= Math.Max(iF, Math.Max(iI, iD))) continue;
                result.Add(new ConciliacionItemExterno
                {
                    Fecha   = cols[iF].Trim(),
                    Importe = ParseImporte(cols[iI]),
                    Detalle = cols[iD].Trim()
                });
            }
            return result;
        }

        private static int BuscarColumna(System.Data.DataTable table, string nombre) =>
            Enumerable.Range(0, table.Columns.Count)
                .FirstOrDefault(i => table.Columns[i].ColumnName.Trim()
                    .IndexOf(nombre, StringComparison.OrdinalIgnoreCase) >= 0, -1);

        private static int BuscarColumnaArr(string[] headers, string nombre) =>
            Array.FindIndex(headers, h => h.Trim().IndexOf(nombre, StringComparison.OrdinalIgnoreCase) >= 0);

        private static decimal ParseImporte(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0;
            s = s.Replace("$", "").Replace(" ", "").Trim();
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)) return Math.Abs(v);
            if (decimal.TryParse(s, NumberStyles.Any, new CultureInfo("es-AR"), out v)) return Math.Abs(v);
            return 0;
        }
    }
}
