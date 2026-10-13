using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;
using ClosedXML.Excel;
using Telerik.WinControls.UI;
using Telerik.WinControls;

namespace AgrupadorConceptos
{
    public partial class ConciliacionExternForm : Form
    {
        private ConciliacionSesion _sesionActiva;
        // Fecha: 08/10/2026 - TAREA: 00076 - Linea: 4 - La selección persiste aunque el rango oculte un archivo.
        private readonly HashSet<int> _idsArchivosSeleccionados = new();
        private List<ConciliacionItemExterno> _itemsExternosLeidos;
        private List<ExtractoCandidato> _candidatosVisibles = new();

        public ConciliacionExternForm()
        {
            InitializeComponent();
            // Fecha: 09/10/2026 - TAREA: 00082 - Linea: 1 - El Designer concentra la composición visual y las suscripciones a eventos.
            Icon = AppIcon.GetIcon();
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

        // Fecha: 09/10/2026 - TAREA: 00082 - Linea: 1 - Los eventos conservan la selección y el resumen sin construir controles fuera del Designer.
        private void grillaArchivos_CellValueChanged(object sender, GridViewCellEventArgs e)
        {
            if (e.Row?.DataBoundItem is not ExtractoCandidato candidato || e.Column?.Name != "Seleccionado") return;
            if (candidato.Seleccionado) _idsArchivosSeleccionados.Add(candidato.IdArchivo);
            else _idsArchivosSeleccionados.Remove(candidato.IdArchivo);
            CargarConceptosPorArchivos();
            ActualizarEstadoArchivos();
        }

        private void FiltroArchivos_Changed(object sender, EventArgs e)
        {
            RefrescarArchivosPorRango();
        }

        private void SeleccionExtracto_Changed(object sender, EventArgs e)
        {
            ActualizarResumenSeleccion();
        }

        private void tabsPendExtracto_SelectedIndexChanged(object sender, EventArgs e)
        {
            dgvExtractoPendiente.ClearSelection();
            dgvExtractoSinConcepto.ClearSelection();
            ActualizarResumenSeleccion();
        }

        private RadGridView GrillaExtractoActiva =>
            tabsPendExtracto.SelectedTab == tabSinConcepto ? dgvExtractoSinConcepto : dgvExtractoPendiente;

        private static IEnumerable<GridViewRowInfo> FilasDatosVisibles(IEnumerable<GridViewRowInfo> filas)
        {
            foreach (var fila in filas)
            {
                if (fila is GridViewGroupRowInfo grupo)
                {
                    foreach (var hija in FilasDatosVisibles(grupo.ChildRows)) yield return hija;
                }
                else if (fila is GridViewDataRowInfo) yield return fila;
            }
        }

        private List<MovimientoProcesado> MovimientosSeleccionados() =>
            FilasDatosVisibles(GrillaExtractoActiva.ChildRows)
                .Where(f => f.IsSelected && f.DataBoundItem is MovimientoProcesado)
                .Select(f => (MovimientoProcesado)f.DataBoundItem)
                .DistinctBy(m => m.Id).ToList();

        private void ActualizarResumenSeleccion()
        {
            var movimientos = MovimientosSeleccionados();
            decimal total = movimientos.Sum(ComparadorConciliacion.ImporteEfectivo);
            decimal externo = Math.Abs(_externoSeleccionado?.Importe ?? 0m);
            _resumenSeleccion.Text = $"Seleccionados: {movimientos.Count}  ·  Total seleccionado: {total.ToString("N2", CulturaArgentina)}  ·  Importe externo: {externo.ToString("N2", CulturaArgentina)}  ·  Diferencia: {(externo - total).ToString("N2", CulturaArgentina)}";
            btnConciliarManual.Enabled = _sesionActiva?.Estado == "EnProceso" && _externoSeleccionado != null &&
                movimientos.Count > 0 && total == externo;
        }

        private static readonly System.Globalization.CultureInfo CulturaArgentina = new("es-AR");

        private void FormatearCeldaConciliacion(object sender, CellFormattingEventArgs e)
        {
            string nombre = e.CellElement?.ColumnInfo?.Name;
            if (nombre == null || e.CellElement.Value == null) return;
            if (sender == dgvConciliados && nombre is "FechaExterno" or "ImporteExterno" or "DetalleExterno")
            {
                var par = e.Row.DataBoundItem as ConciliacionPar;
                var primera = FilasDatosVisibles(dgvConciliados.ChildRows)
                    .Select(f => f.DataBoundItem as ConciliacionPar)
                    .FirstOrDefault(p => p?.IdSesion == par?.IdSesion && p?.IdItemExterno == par?.IdItemExterno);
                if (primera?.Id != par?.Id) { e.CellElement.Text = ""; return; }
            }
            if (nombre is "Fecha" or "FechaExterno" or "FechaExtracto")
            {
                // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Operar sobre días tipados evita reinterpretaciones culturales.
                if(e.CellElement.Value is DateTime fecha) e.CellElement.Text = fecha.ToString("dd/MM/yyyy", CulturaArgentina);
            }
            else if (nombre is "Importe" or "ImporteExterno" or "ImporteExtracto" or "Debitos" or "Creditos")
            {
                if (e.CellElement.Value is decimal numero)
                    e.CellElement.Text = numero.ToString("N2", CulturaArgentina);
            }
        }

        private void CargarArchivosEnPanel()
        {
            _archivosDisponibles = ArchivoImportadoStorage.ObtenerTodos();
            _idsArchivosSeleccionados.Clear();
            clbConceptos.Items.Clear();
            RefrescarArchivosPorRango();
        }

        private void RefrescarArchivosPorRango()
        {
            if (_desdeArchivos.Value.Date > _hastaArchivos.Value.Date)
            {
                _estadoArchivos.Text = "Rango inválido: Desde es posterior a Hasta.";
                return;
            }
            _candidatosVisibles = SelectorExtractosService.Obtener(_desdeArchivos.Value, _hastaArchivos.Value, _mostrarTodos.Checked);
            foreach (var candidato in _candidatosVisibles)
                candidato.Seleccionado = _idsArchivosSeleccionados.Contains(candidato.IdArchivo);
            _grillaArchivos.DataSource = _candidatosVisibles;
            // Fecha: 09/10/2026 - TAREA: 00082 - Linea: 1 - Las columnas se definen en el Designer y se conservan al renovar los datos.
            ActualizarEstadoArchivos();
        }

        private void ActualizarEstadoArchivos()
        {
            int ocultos = _idsArchivosSeleccionados.Count(id => _candidatosVisibles.All(c => c.IdArchivo != id));
            _estadoArchivos.Text = $"{_idsArchivosSeleccionados.Count} seleccionados, {ocultos} fuera del rango";
        }

        private void CargarConceptosPorArchivos()
        {
            var marcados = clbConceptos.CheckedItems.Cast<string>().ToHashSet(StringComparer.OrdinalIgnoreCase);
            clbConceptos.Items.Clear();
            var seleccionados = ArchivosSeleccionados;
            if (seleccionados.Count == 0) return;

            var ids = seleccionados.Select(a => a.Id).ToList();
            var conceptos = MovimientoStorage.ObtenerConceptosFinalesDistintos(ids);

            foreach (var c in conceptos)
                clbConceptos.Items.Add(c, marcados.Contains(c));
        }

        private List<ArchivoImportado> ArchivosSeleccionados =>
            _archivosDisponibles.Where(a => _idsArchivosSeleccionados.Contains(a.Id)).ToList();

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
            _itemsExternosLeidos = null;
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

            var items = _itemsExternosLeidos;
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

        private void btnCargarExterno_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Excel / CSV|*.xlsx;*.xls;*.csv",
                Title  = "Seleccionar archivo externo"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            var items = LeerArchivoExterno(dlg.FileName);
            if (items == null) return;
            _itemsExternosLeidos = items;
            txtArchivoExterno.Text = dlg.FileName;
            var sugerencia = SelectorExtractosService.SugerirRango(items);
            if (sugerencia.Desde.HasValue && sugerencia.Hasta.HasValue)
            {
                _desdeArchivos.Value = sugerencia.Desde.Value;
                _hastaArchivos.Value = sugerencia.Hasta.Value;
                _mostrarTodos.Checked = false;
            }
            else
            {
                _mostrarTodos.Checked = true;
                MessageBox.Show("El archivo externo no tiene fechas interpretables; se muestran todos los extractos.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            RefrescarArchivosPorRango();
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
            dgvExtractoPendiente.ClearSelection();
            dgvExtractoSinConcepto.ClearSelection();
            ActualizarResumenSeleccion();
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

            var movimientos = MovimientosSeleccionados();
            if (movimientos.Count == 0)
            { MessageBox.Show("Seleccione movimientos visibles del extracto.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            try
            {
                ConciliacionExternService.ConciliarSeleccion(
                    _sesionActiva.Id, _externoSeleccionado.Id, movimientos.Select(m => m.Id).ToArray(), TipoMatch.Manual);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo conciliar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

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

            // Fecha: 08/10/2026 - TAREA: 00076 - Linea: 2, 3 - Exportar grupos y valores tipados sin repetir importes externos.
            ConciliacionExcelExporter.Exportar(pares, pendExt, pendExtr, dlg.FileName);
            MessageBox.Show("Exportado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Helpers de UI ─────────────────────────────────────────────────────────

        // Fecha: 09/10/2026 - TAREA: 00082 - Linea: 1 - El refresco actualiza datos; el ajuste fijo de columnas pertenece al Designer.
        private void RefrescarGrillas()
        {
            if (_sesionActiva == null) { LimpiarGrillas(); return; }

            // Tab pendientes — externo
            dgvExternoPendiente.DataSource = ConciliacionExternService.ObtenerItemsPendientes(_sesionActiva.Id);
            OcultarIdentificadores(dgvExternoPendiente);
            // Tab pendientes — extracto
            dgvExtractoPendiente.DataSource = ConciliacionExternService.ObtenerMovimientosPendientes(_sesionActiva.Id);
            OcultarIdentificadores(dgvExtractoPendiente);

            // Pendientes sin concepto (restante del extracto)
            dgvExtractoSinConcepto.DataSource = ConciliacionExternService.ObtenerMovimientosSinConcepto(_sesionActiva.Id);
            OcultarIdentificadores(dgvExtractoSinConcepto);

            // Tab conciliados
            dgvConciliados.DataSource = ConciliacionExternService.ObtenerPares(_sesionActiva.Id);
            OcultarIdentificadores(dgvConciliados);

            ActualizarContadores();
            ActualizarResumenSeleccion();
        }

        // Fecha: 08/10/2026 - TAREA: 00076 - Linea: 2, 3 - Las claves de persistencia son operativas y no ayudan a comparar importes o fechas.
        private static void OcultarIdentificadores(RadGridView grilla)
        {
            foreach (var columna in grilla.Columns)
                if (columna.Name is "Id" or "IdSesion" or "IdArchivo" or "IdItemExterno" or "IdMovimientoProcesado")
                    columna.IsVisible = false;
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
            // Fecha: 08/10/2026 - TAREA: 00076 - Linea: 2 - Una fila por movimiento no implica un externo distinto por fila.
            int externos = dgvConciliados.Rows.Select(r => r.DataBoundItem as ConciliacionPar)
                .Where(p => p != null).Select(p => p.IdItemExterno).Distinct().Count();
            lblContadorConcil.Text   = $"{externos} externos / {concil} movimientos";
        }

        private void ActualizarEstadoSesion()
        {
            bool activa = _sesionActiva != null;
            lblSesionActiva.Text    = activa ? $"Sesión: {_sesionActiva.Nombre} [{_sesionActiva.Estado}]" : "Sin sesión activa";
            btnAutoConciliar.Enabled  = activa && _sesionActiva.Estado == "EnProceso";
            btnConciliarManual.Enabled = false;
            btnDesconciliar.Enabled    = activa && _sesionActiva.Estado == "EnProceso";
            btnFinalizar.Enabled       = activa && _sesionActiva.Estado == "EnProceso";
            btnExportar.Enabled        = activa;
            ActualizarResumenSeleccion();
        }

        // ── Lectura del archivo externo ───────────────────────────────────────────

        private List<ConciliacionItemExterno> LeerArchivoExterno(string path)
        {
            try
            {
                return ArchivoExternoReader.Leer(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al leer el archivo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}
