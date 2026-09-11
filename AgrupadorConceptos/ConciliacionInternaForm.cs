// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Conciliacion interna entre extractos propios
// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Rediseño: sin seleccion de perfil, un solo rango
// de fechas y paneles Debitos/Creditos globales en vez de "Extracto A"/"Extracto B".
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;
using ClosedXML.Excel;
using Telerik.WinControls.UI;
using Telerik.WinControls;

namespace AgrupadorConceptos
{
    public partial class ConciliacionInternaForm : Form
    {
        private ConciliacionInternaSesion _sesionActiva;

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Mostrar el extracto de cada movimiento en las grillas
        // Los pools de Debitos/Creditos son globales (cualquier extracto), asi que fecha+importe
        // ya no alcanzan para que el usuario distinga a simple vista dos movimientos parecidos de
        // bancos distintos. Este wrapper solo agrega el nombre del extracto para mostrar en la
        // grilla; toda la logica de matching sigue operando sobre el MovimientoProcesado original.
        private class MovimientoConciliable
        {
            [Browsable(false)]
            public MovimientoProcesado Original { get; }
            public string Fecha => Original.Fecha;
            public decimal Importe => ComparadorConciliacion.ImporteEfectivo(Original);
            public string ConceptoFinal => Original.ConceptoFinal;
            public string Extracto { get; }

            public MovimientoConciliable(MovimientoProcesado original, string extracto)
            {
                Original = original;
                Extracto = extracto;
            }
        }

        public ConciliacionInternaForm()
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
            var sesiones = ConciliacionInternaService.ObtenerTodasSesiones();
            lbSesiones.DataSource     = sesiones;
            lbSesiones.DisplayMember  = "DisplayName";
            lbSesiones.ValueMember    = "Id";
            btnRetomar.Enabled        = sesiones.Count > 0;
            btnEliminarSesion.Enabled = sesiones.Count > 0;
            lbSesiones.SelectedIndex  = sesiones.Count > 0 ? 0 : -1;
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Rango por defecto del panel de alta, sin perfiles
        private void PrepararPanelNuevaSesion()
        {
            dtpDesde.Value = DateTime.Today.AddMonths(-1);
            dtpHasta.Value = DateTime.Today;
            CargarConceptosPorRango();
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Conceptos disponibles en cualquier extracto, segun el rango elegido
        private void CargarConceptosPorRango()
        {
            clbConceptos.Items.Clear();

            if (dtpDesde.Value.Date > dtpHasta.Value.Date) return;

            foreach (var c in ConciliacionInternaService.ObtenerConceptosDisponibles(dtpDesde.Value, dtpHasta.Value))
                clbConceptos.Items.Add(c, false);
        }

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
            PrepararPanelNuevaSesion();
            MostrarPanelNuevaSesion(true);
        }

        private void dtpRango_ValueChanged(object sender, EventArgs e) => CargarConceptosPorRango();

        private void btnConfirmarNueva_Click(object sender, EventArgs e)
        {
            if (dtpDesde.Value.Date > dtpHasta.Value.Date)
            { MessageBox.Show("La fecha 'Desde' no puede ser posterior a 'Hasta'.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var conceptos = clbConceptos.CheckedItems.Cast<string>().ToList();
            if (conceptos.Count == 0)
            { MessageBox.Show("Seleccione al menos un concepto.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            string nombreSugerido = $"{dtpDesde.Value:dd/MM} - {dtpHasta.Value:dd/MM} - {DateTime.Now:dd/MM HH:mm}";
            using var dlgNombre = new NombreSesionDialog(nombreSugerido);
            if (dlgNombre.ShowDialog(this) != DialogResult.OK) return;

            _sesionActiva = ConciliacionInternaService.CrearSesion(
                dlgNombre.NombreSesion, dtpDesde.Value, dtpHasta.Value, conceptos);

            MostrarPanelNuevaSesion(false);
            CargarSesiones();
            ActualizarEstadoSesion();
            RefrescarGrillas();
        }

        private void btnCancelarNueva_Click(object sender, EventArgs e)
        {
            MostrarPanelNuevaSesion(false);
        }

        // Fecha: 11/09/2026 - TAREA: 00021 - Linea: 5 - Ocultar todo lo demas mientras se arma la sesion nueva
        // pnlConfigNueva no cubre geometricamente ni la pestaña Pendientes/Conciliados (se pasa
        // ~40px por abajo del panel) ni la barra de botones inferior (Auto-conciliar, etc. estan
        // mas abajo todavia) — nunca lo hizo, en ninguna version. Ocultar explicitamente en vez de
        // confiar en que el panel las tape es lo unico que garantiza que, mientras se elige el
        // rango y se arma el filtro inicial de conceptos, no quede nada de la sesion anterior a
        // la vista (ni clickeable) por debajo.
        private void MostrarPanelNuevaSesion(bool nuevaSesion)
        {
            pnlConfigNueva.Visible = nuevaSesion;

            bool mostrarTrabajo = !nuevaSesion;
            lbSesiones.Visible         = mostrarTrabajo;
            btnNuevaSesion.Visible     = mostrarTrabajo;
            btnRetomar.Visible         = mostrarTrabajo;
            btnEliminarSesion.Visible  = mostrarTrabajo;
            lblSesionActiva.Visible    = mostrarTrabajo;
            tabControl.Visible         = mostrarTrabajo;
            btnAutoConciliar.Visible   = mostrarTrabajo;
            btnConciliarManual.Visible = mostrarTrabajo;
            btnDesconciliar.Visible    = mostrarTrabajo;
            btnFinalizar.Visible       = mostrarTrabajo;
            btnExportar.Visible        = mostrarTrabajo;
        }

        private void btnRetomar_Click(object sender, EventArgs e)
        {
            if (lbSesiones.SelectedItem is not ConciliacionInternaSesion sesion) return;
            _sesionActiva = sesion;
            ActualizarEstadoSesion();
            RefrescarGrillas();
        }

        private void btnEliminarSesion_Click(object sender, EventArgs e)
        {
            if (lbSesiones.SelectedItem is not ConciliacionInternaSesion sesion) return;

            if (MessageBox.Show($"¿Eliminar la sesión '{sesion.Nombre}' y todos sus datos?",
                    "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            ConciliacionInternaService.EliminarSesion(sesion.Id);
            if (_sesionActiva?.Id == sesion.Id) { _sesionActiva = null; LimpiarGrillas(); }
            CargarSesiones();
            ActualizarEstadoSesion();
        }

        // ── Auto-conciliación ─────────────────────────────────────────────────────

        private void btnAutoConciliar_Click(object sender, EventArgs e)
        {
            if (_sesionActiva == null) { MessageBox.Show("No hay sesión activa.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var (conciliados, duplicados) = ConciliacionInternaService.AutoConciliar(_sesionActiva.Id);

            // Fecha: 11/09/2026 - TAREA: 00021 - Linea: 5 - No volver a ofrecer un credito ya asignado en este mismo loop
            // "duplicados" trae, por cada debito con mas de un candidato, la foto de candidatos que
            // habia en el momento de AutoConciliar. Dos debitos distintos pueden compartir un
            // candidato (mismo importe, misma fecha): sin este filtro, el segundo dialogo seguia
            // ofreciendo un credito que el primero ya se acababa de llevar, y quedaba conciliado dos
            // veces (el indice unico de la base lo hubiera rechazado, pero mejor no ofrecerlo).
            var creditosAsignados = new HashSet<int>();
            int quedaronPendientes = 0;

            foreach (var (a, candidatosOriginales) in duplicados)
            {
                var candidatos = candidatosOriginales.Where(c => !creditosAsignados.Contains(c.Id)).ToList();
                if (candidatos.Count == 0) { quedaronPendientes++; continue; }

                using var dlg = new SeleccionCandidatoInternoDialog(a, candidatos);
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.MovimientoSeleccionado != null)
                {
                    string conflicto = ConciliacionInternaService.ConciliarPar(
                        _sesionActiva.Id, a.Id, dlg.MovimientoSeleccionado.Id, TipoMatch.SeleccionManual);
                    if (conflicto != null)
                        MessageBox.Show($"No se concilió: {conflicto}", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    else
                    {
                        creditosAsignados.Add(dlg.MovimientoSeleccionado.Id);
                        conciliados++;
                    }
                }
            }

            RefrescarGrillas();
            string resumen = $"Auto-conciliación completada: {conciliados} par(es) conciliado(s).";
            if (quedaronPendientes > 0)
                resumen += $" {quedaronPendientes} quedaron pendientes porque su único candidato ya se había asignado a otro movimiento.";
            MessageBox.Show(resumen, "Resultado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Conciliación manual ───────────────────────────────────────────────────

        private MovimientoProcesado _movimientoDebitoSeleccionado;

        private void dgvDebitos_SelectionChanged(object sender, EventArgs e)
        {
            _movimientoDebitoSeleccionado = (dgvDebitos.CurrentRow?.DataBoundItem as MovimientoConciliable)?.Original;
            dgvCreditos.TableElement.BeginUpdate();
            dgvCreditos.TableElement.EndUpdate();
        }

        private void dgvCreditos_RowFormatting(object sender, RowFormattingEventArgs e)
        {
            if (_movimientoDebitoSeleccionado == null || e.RowElement.RowInfo.DataBoundItem is not MovimientoConciliable wrapper)
            {
                e.RowElement.ResetValue(LightVisualElement.BackColorProperty, ValueResetFlags.Local);
                return;
            }

            var b = wrapper.Original;
            bool fechaMatch   = ComparadorConciliacion.FechasIguales(_movimientoDebitoSeleccionado.Fecha, b.Fecha);
            bool importeMatch = ComparadorConciliacionInterna.ImportesOpuestos(_movimientoDebitoSeleccionado, b);

            if (fechaMatch && importeMatch) e.RowElement.BackColor = Color.LightGreen;
            else if (importeMatch) e.RowElement.BackColor = Color.LightYellow;
            else e.RowElement.ResetValue(LightVisualElement.BackColorProperty, ValueResetFlags.Local);
        }

        private void btnConciliarManual_Click(object sender, EventArgs e)
        {
            if (_sesionActiva == null || _movimientoDebitoSeleccionado == null)
            { MessageBox.Show("Seleccione un movimiento del panel Débitos.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (dgvCreditos.CurrentRow?.DataBoundItem is not MovimientoConciliable wrapperB)
            { MessageBox.Show("Seleccione un movimiento del panel Créditos.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var b = wrapperB.Original;

            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Bloquear si alguno ya esta conciliado en otra sesion EnProceso
            string conflicto = ConciliacionInternaService.ConciliarPar(_sesionActiva.Id, _movimientoDebitoSeleccionado.Id, b.Id, TipoMatch.Manual);
            if (conflicto != null)
            {
                MessageBox.Show(conflicto, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _movimientoDebitoSeleccionado = null;
            RefrescarGrillas();
        }

        private void btnDesconciliar_Click(object sender, EventArgs e)
        {
            if (dgvConciliados.CurrentRow?.DataBoundItem is not ConciliacionInternaPar par) return;

            ConciliacionInternaService.DesconciliarPar(par.Id);
            RefrescarGrillas();
        }

        private void btnFinalizar_Click(object sender, EventArgs e)
        {
            if (_sesionActiva == null) return;
            var pendDebitos = ConciliacionInternaService.ObtenerPendientesDebitos(_sesionActiva.Id);
            if (pendDebitos.Count > 0 &&
                MessageBox.Show($"Aún quedan {pendDebitos.Count} movimientos sin conciliar. ¿Finalizar igual?",
                    "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Cierre pisa CuentaFinal con la del perfil opuesto, resuelta por par
            var resultado = ConciliacionInternaService.Finalizar(_sesionActiva.Id);
            if (!resultado.Exito)
            {
                MessageBox.Show(
                    "No se puede finalizar: hay pares cuyo perfil no tiene cuenta contable asignada. " +
                    "Asignale una cuenta a esos perfiles (Agrupador → Procesador → Editar Perfil) y reintentá.\n\n" +
                    string.Join("\n", resultado.ParesSinCuenta),
                    "Faltan cuentas contables", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _sesionActiva.Estado = "Finalizada";
            ActualizarEstadoSesion();
            CargarSesiones();
            RefrescarGrillas(); // Cuenta Final quedo actualizada en los pares conciliados
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            if (_sesionActiva == null) return;

            using var dlg = new SaveFileDialog
            { Filter = "Excel|*.xlsx", FileName = $"ConciliacionInterna_{DateTime.Now:yyyyMMdd_HHmm}.xlsx" };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            var pares = ConciliacionInternaService.ObtenerPares(_sesionActiva.Id);
            var pendDebitos = ConciliacionInternaService.ObtenerPendientesDebitos(_sesionActiva.Id);
            var pendCreditos = ConciliacionInternaService.ObtenerPendientesCreditos(_sesionActiva.Id);

            using var wb = new XLWorkbook();
            var wsCon = wb.Worksheets.Add("Conciliados");
            wsCon.Cell(1,1).Value = "Fecha Débito"; wsCon.Cell(1,2).Value = "Importe Débito"; wsCon.Cell(1,3).Value = "Concepto Débito";
            wsCon.Cell(1,4).Value = "Fecha Crédito"; wsCon.Cell(1,5).Value = "Importe Crédito"; wsCon.Cell(1,6).Value = "Concepto Crédito";
            wsCon.Cell(1,7).Value = "Tipo Match";
            for (int i = 0; i < pares.Count; i++)
            {
                var p = pares[i]; int r = i + 2;
                wsCon.Cell(r,1).Value = p.FechaA; wsCon.Cell(r,2).Value = (double)p.ImporteA; wsCon.Cell(r,3).Value = p.ConceptoFinalA;
                wsCon.Cell(r,4).Value = p.FechaB; wsCon.Cell(r,5).Value = (double)p.ImporteB; wsCon.Cell(r,6).Value = p.ConceptoFinalB;
                wsCon.Cell(r,7).Value = p.TipoMatch;
            }

            var wsPend = wb.Worksheets.Add("Pendientes");
            wsPend.Cell(1,1).Value = "Lado"; wsPend.Cell(1,2).Value = "Fecha";
            wsPend.Cell(1,3).Value = "Importe"; wsPend.Cell(1,4).Value = "Concepto";
            int rp = 2;
            foreach (var m in pendDebitos)
            { wsPend.Cell(rp,1).Value="Débito"; wsPend.Cell(rp,2).Value=m.Fecha; wsPend.Cell(rp,3).Value=(double)ComparadorConciliacion.ImporteEfectivo(m); wsPend.Cell(rp,4).Value=m.ConceptoFinal; rp++; }
            foreach (var m in pendCreditos)
            { wsPend.Cell(rp,1).Value="Crédito"; wsPend.Cell(rp,2).Value=m.Fecha; wsPend.Cell(rp,3).Value=(double)ComparadorConciliacion.ImporteEfectivo(m); wsPend.Cell(rp,4).Value=m.ConceptoFinal; rp++; }

            wb.SaveAs(dlg.FileName);
            MessageBox.Show("Exportado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Helpers de UI ─────────────────────────────────────────────────────────

        private void RefrescarGrillas()
        {
            if (_sesionActiva == null) { LimpiarGrillas(); return; }

            var extractos = ConciliacionInternaService.ObtenerNombresExtracto();
            string ExtractoDe(MovimientoProcesado m) =>
                extractos.TryGetValue(m.IdArchivo, out var nombre) ? nombre : $"Archivo #{m.IdArchivo}";

            dgvDebitos.DataSource = ConciliacionInternaService.ObtenerPendientesDebitos(_sesionActiva.Id)
                .Select(m => new MovimientoConciliable(m, ExtractoDe(m))).ToList();
            dgvDebitos.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;

            dgvCreditos.DataSource = ConciliacionInternaService.ObtenerPendientesCreditos(_sesionActiva.Id)
                .Select(m => new MovimientoConciliable(m, ExtractoDe(m))).ToList();
            dgvCreditos.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;

            dgvConciliados.DataSource = ConciliacionInternaService.ObtenerPares(_sesionActiva.Id);
            dgvConciliados.Columns["Id"].IsVisible = false;
            dgvConciliados.Columns["IdSesion"].IsVisible = false;
            dgvConciliados.Columns["IdMovimientoA"].IsVisible = false;
            dgvConciliados.Columns["IdMovimientoB"].IsVisible = false;
            dgvConciliados.Columns["IdArchivoA"].IsVisible = false;
            dgvConciliados.Columns["IdArchivoB"].IsVisible = false;

            dgvConciliados.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;
        }

        private void LimpiarGrillas()
        {
            dgvDebitos.DataSource = null;
            dgvCreditos.DataSource = null;
            dgvConciliados.DataSource = null;
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Una sesion Finalizada se retoma de solo lectura
        // Antes esto habilitaba los botones con sólo mirar si había sesión activa: una sesión ya
        // Finalizada se podía re-tocar (ganar pares nuevos, volver a Finalizar) y pisar en silencio
        // una CuentaFinal que el usuario ya había corregido a mano después del cierre. Ahora todo
        // lo que muta queda deshabilitado si el Estado es "Finalizada"; Exportar sigue disponible
        // porque revisar/exportar una sesión ya cerrada sigue siendo útil.
        private void ActualizarEstadoSesion()
        {
            bool activa = _sesionActiva != null;
            bool editable = activa && _sesionActiva.Estado != "Finalizada";
            lblSesionActiva.Text = activa ? $"Sesión: {_sesionActiva.Nombre} [{_sesionActiva.Estado}]" : "Sin sesión activa";
            btnAutoConciliar.Enabled   = editable;
            btnConciliarManual.Enabled = editable;
            btnDesconciliar.Enabled    = editable;
            btnFinalizar.Enabled       = editable;
            btnExportar.Enabled        = activa;
        }
    }
}
