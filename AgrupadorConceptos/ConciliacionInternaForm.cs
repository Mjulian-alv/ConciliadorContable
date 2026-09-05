// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Conciliacion interna entre extractos propios
using System;
using System.Collections.Generic;
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

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Perfiles y rango por defecto del panel de alta
        private void CargarPerfilesEnPanel()
        {
            var perfiles = PerfilBancoStorage.ObtenerTodos();

            cmbPerfilA.DataSource = perfiles;
            cmbPerfilA.DisplayMember = "NombreBanco";
            cmbPerfilA.ValueMember = "Id";
            cmbPerfilA.SelectedIndex = -1;

            cmbPerfilB.DataSource = perfiles.ToList(); // lista aparte: no comparten SelectedItem
            cmbPerfilB.DisplayMember = "NombreBanco";
            cmbPerfilB.ValueMember = "Id";
            cmbPerfilB.SelectedIndex = -1;

            dtpDesdeA.Value = dtpDesdeB.Value = DateTime.Today.AddMonths(-1);
            dtpHastaA.Value = dtpHastaB.Value = DateTime.Today;

            clbConceptos.Items.Clear();
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Conceptos disponibles segun perfil + rango elegidos
        private void CargarConceptosPorRango()
        {
            clbConceptos.Items.Clear();

            if (cmbPerfilA.SelectedValue is not int idPerfilA || cmbPerfilB.SelectedValue is not int idPerfilB)
                return;

            var conceptosA = MovimientoStorage.ObtenerPorPerfil(idPerfilA)
                .Where(m => ComparadorConciliacionInterna.EstaEnRango(m.Fecha, dtpDesdeA.Value, dtpHastaA.Value))
                .Select(m => m.ConceptoFinal);

            var conceptosB = MovimientoStorage.ObtenerPorPerfil(idPerfilB)
                .Where(m => ComparadorConciliacionInterna.EstaEnRango(m.Fecha, dtpDesdeB.Value, dtpHastaB.Value))
                .Select(m => m.ConceptoFinal);

            foreach (var c in conceptosA.Concat(conceptosB)
                         .Where(c => !string.IsNullOrWhiteSpace(c))
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .OrderBy(c => c))
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
            CargarPerfilesEnPanel();
            pnlConfigNueva.Visible = true;
            btnNuevaSesion.Enabled = false;
        }

        private void cmbPerfil_SelectedIndexChanged(object sender, EventArgs e) => CargarConceptosPorRango();

        private void dtpRango_ValueChanged(object sender, EventArgs e) => CargarConceptosPorRango();

        private void btnConfirmarNueva_Click(object sender, EventArgs e)
        {
            if (cmbPerfilA.SelectedValue is not int idPerfilA || cmbPerfilB.SelectedValue is not int idPerfilB)
            { MessageBox.Show("Elija el perfil de cada extracto.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (dtpDesdeA.Value.Date > dtpHastaA.Value.Date || dtpDesdeB.Value.Date > dtpHastaB.Value.Date)
            { MessageBox.Show("La fecha 'Desde' no puede ser posterior a 'Hasta'.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var conceptos = clbConceptos.CheckedItems.Cast<string>().ToList();
            if (conceptos.Count == 0)
            { MessageBox.Show("Seleccione al menos un concepto.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var perfilA = (PerfilBanco)cmbPerfilA.SelectedItem;
            var perfilB = (PerfilBanco)cmbPerfilB.SelectedItem;
            string nombreSugerido = $"{perfilA.NombreBanco} ↔ {perfilB.NombreBanco} - {DateTime.Now:dd/MM HH:mm}";
            using var dlgNombre = new NombreSesionDialog(nombreSugerido);
            if (dlgNombre.ShowDialog(this) != DialogResult.OK) return;

            _sesionActiva = ConciliacionInternaService.CrearSesion(
                dlgNombre.NombreSesion, idPerfilA, dtpDesdeA.Value, dtpHastaA.Value,
                idPerfilB, dtpDesdeB.Value, dtpHastaB.Value, conceptos);

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

            foreach (var (a, candidatos) in duplicados)
            {
                using var dlg = new SeleccionCandidatoInternoDialog(a, candidatos);
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.MovimientoSeleccionado != null)
                {
                    ConciliacionInternaService.ConciliarPar(
                        _sesionActiva.Id, a.Id, dlg.MovimientoSeleccionado.Id, TipoMatch.SoloImporte);
                    conciliados++;
                }
            }

            RefrescarGrillas();
            MessageBox.Show($"Auto-conciliación completada: {conciliados} par(es) conciliado(s).",
                "Resultado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Conciliación manual ───────────────────────────────────────────────────

        private MovimientoProcesado _movimientoASeleccionado;

        private void dgvPendienteA_SelectionChanged(object sender, EventArgs e)
        {
            _movimientoASeleccionado = dgvPendienteA.CurrentRow?.DataBoundItem as MovimientoProcesado;
            dgvPendienteB.TableElement.BeginUpdate();
            dgvPendienteB.TableElement.EndUpdate();
        }

        private void dgvPendienteB_RowFormatting(object sender, RowFormattingEventArgs e)
        {
            if (_movimientoASeleccionado == null || e.RowElement.RowInfo.DataBoundItem is not MovimientoProcesado b)
            {
                e.RowElement.ResetValue(LightVisualElement.BackColorProperty, ValueResetFlags.Local);
                return;
            }

            bool fechaMatch   = ComparadorConciliacion.FechasIguales(_movimientoASeleccionado.Fecha, b.Fecha);
            bool importeMatch = ComparadorConciliacionInterna.ImportesOpuestos(_movimientoASeleccionado, b);

            if (fechaMatch && importeMatch) e.RowElement.BackColor = Color.LightGreen;
            else if (importeMatch) e.RowElement.BackColor = Color.LightYellow;
            else e.RowElement.ResetValue(LightVisualElement.BackColorProperty, ValueResetFlags.Local);
        }

        private void btnConciliarManual_Click(object sender, EventArgs e)
        {
            if (_sesionActiva == null || _movimientoASeleccionado == null)
            { MessageBox.Show("Seleccione un movimiento del extracto A.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (dgvPendienteB.CurrentRow?.DataBoundItem is not MovimientoProcesado b)
            { MessageBox.Show("Seleccione un movimiento del extracto B.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            ConciliacionInternaService.ConciliarPar(_sesionActiva.Id, _movimientoASeleccionado.Id, b.Id, TipoMatch.Manual);

            _movimientoASeleccionado = null;
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
            var pendA = ConciliacionInternaService.ObtenerPendientesA(_sesionActiva.Id);
            if (pendA.Count > 0 &&
                MessageBox.Show($"Aún quedan {pendA.Count} movimientos sin conciliar. ¿Finalizar igual?",
                    "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Cierre pisa CuentaFinal con la del perfil opuesto
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
            var pendA = ConciliacionInternaService.ObtenerPendientesA(_sesionActiva.Id);
            var pendB = ConciliacionInternaService.ObtenerPendientesB(_sesionActiva.Id);

            using var wb = new XLWorkbook();
            var wsCon = wb.Worksheets.Add("Conciliados");
            wsCon.Cell(1,1).Value = "Fecha A"; wsCon.Cell(1,2).Value = "Importe A"; wsCon.Cell(1,3).Value = "Concepto A";
            wsCon.Cell(1,4).Value = "Fecha B"; wsCon.Cell(1,5).Value = "Importe B"; wsCon.Cell(1,6).Value = "Concepto B";
            wsCon.Cell(1,7).Value = "Tipo Match";
            for (int i = 0; i < pares.Count; i++)
            {
                var p = pares[i]; int r = i + 2;
                wsCon.Cell(r,1).Value = p.FechaA; wsCon.Cell(r,2).Value = (double)p.ImporteA; wsCon.Cell(r,3).Value = p.ConceptoFinalA;
                wsCon.Cell(r,4).Value = p.FechaB; wsCon.Cell(r,5).Value = (double)p.ImporteB; wsCon.Cell(r,6).Value = p.ConceptoFinalB;
                wsCon.Cell(r,7).Value = p.TipoMatch;
            }

            var wsPend = wb.Worksheets.Add("Pendientes");
            wsPend.Cell(1,1).Value = "Extracto"; wsPend.Cell(1,2).Value = "Fecha";
            wsPend.Cell(1,3).Value = "Importe"; wsPend.Cell(1,4).Value = "Concepto";
            int rp = 2;
            foreach (var m in pendA)
            { wsPend.Cell(rp,1).Value="A"; wsPend.Cell(rp,2).Value=m.Fecha; wsPend.Cell(rp,3).Value=(double)ComparadorConciliacion.ImporteEfectivo(m); wsPend.Cell(rp,4).Value=m.ConceptoFinal; rp++; }
            foreach (var m in pendB)
            { wsPend.Cell(rp,1).Value="B"; wsPend.Cell(rp,2).Value=m.Fecha; wsPend.Cell(rp,3).Value=(double)ComparadorConciliacion.ImporteEfectivo(m); wsPend.Cell(rp,4).Value=m.ConceptoFinal; rp++; }

            wb.SaveAs(dlg.FileName);
            MessageBox.Show("Exportado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Helpers de UI ─────────────────────────────────────────────────────────

        private void RefrescarGrillas()
        {
            if (_sesionActiva == null) { LimpiarGrillas(); return; }

            dgvPendienteA.DataSource = ConciliacionInternaService.ObtenerPendientesA(_sesionActiva.Id);
            dgvPendienteA.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;

            dgvPendienteB.DataSource = ConciliacionInternaService.ObtenerPendientesB(_sesionActiva.Id);
            dgvPendienteB.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;

            dgvConciliados.DataSource = ConciliacionInternaService.ObtenerPares(_sesionActiva.Id);
            dgvConciliados.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;
        }

        private void LimpiarGrillas()
        {
            dgvPendienteA.DataSource = null;
            dgvPendienteB.DataSource = null;
            dgvConciliados.DataSource = null;
        }

        private void ActualizarEstadoSesion()
        {
            bool activa = _sesionActiva != null;
            lblSesionActiva.Text = activa ? $"Sesión: {_sesionActiva.Nombre} [{_sesionActiva.Estado}]" : "Sin sesión activa";
            btnAutoConciliar.Enabled   = activa;
            btnConciliarManual.Enabled = activa;
            btnDesconciliar.Enabled    = activa;
            btnFinalizar.Enabled       = activa;
            btnExportar.Enabled        = activa;
        }
    }
}
