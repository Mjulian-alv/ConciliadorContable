// Fecha: 21/09/2026 - TAREA: 00041 - Linea: 2 - Detalle de perfil PyR con la grilla de cuentas de percepción/retención
// Las cuentas se editan sobre una copia: si se cancela el formulario, el perfil queda intacto.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ArcaCliente.Models;
using Telerik.WinControls.UI;

namespace ArcaCliente
{
    public partial class FormPerfilPyRDetalle : Telerik.WinControls.UI.RadForm
    {
        private readonly PerfilOfflinePyR _perfil;
        private readonly BindingList<CuentaPyR> _cuentas;

        // Códigos repetidos detectados al guardar: se pintan en rojo en la grilla.
        private HashSet<string> _codigosRepetidos = new(StringComparer.OrdinalIgnoreCase);

        public PerfilOfflinePyR Perfil => _perfil;

        public FormPerfilPyRDetalle(PerfilOfflinePyR perfil = null)
        {
            InitializeComponent();
            Icon = AppIcons.Arca;

            bool esNuevo = perfil == null;
            _perfil  = perfil ?? new PerfilOfflinePyR();
            _cuentas = new BindingList<CuentaPyR>(_perfil.Cuentas.Select(c => c.Clonar()).ToList());

            CargarCombos();
            ConfigurarGrillaCuentas();
            CargarDatos();

            Text = esNuevo ? "Perfil PyR — Nuevo" : $"Perfil PyR — {_perfil.Nombre}";
        }

        // ── Init ─────────────────────────────────────────────────────────────────

        private void CargarCombos()
        {
            cmbTipoArchivo.Items.Add(new RadListDataItem("Excel (.xlsx / .xls)", "xlsx"));
            cmbTipoArchivo.SelectedIndex = 0;

            foreach (var f in new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "MM/dd/yyyy" })
                cmbFormatoFecha.Items.Add(new RadListDataItem(f, f));

            cmbSeparadorDecimal.Items.Add(new RadListDataItem(". (punto)", "."));
            cmbSeparadorDecimal.Items.Add(new RadListDataItem(", (coma)",  ","));
        }

        private void ConfigurarGrillaCuentas()
        {
            gridCuentas.AutoGenerateColumns = false;
            gridCuentas.Columns.Add(new GridViewTextBoxColumn(nameof(CuentaPyR.Codigo))        { HeaderText = "Código",   Width = 90 });
            gridCuentas.Columns.Add(new GridViewTextBoxColumn(nameof(CuentaPyR.Nombre))        { HeaderText = "Nombre",   Width = 260 });
            gridCuentas.Columns.Add(new GridViewTextBoxColumn(nameof(CuentaPyR.TipoTexto))     { HeaderText = "Tipo",     Width = 110 });
            gridCuentas.Columns.Add(new GridViewTextBoxColumn(nameof(CuentaPyR.ImpuestoTexto)) { HeaderText = "Impuesto", Width = 100 });
            gridCuentas.DataSource = _cuentas;
            _cuentas.ListChanged += (s, e) => ActualizarEstadoCuentas();
        }

        private void CargarDatos()
        {
            txtNombre.Text    = _perfil.Nombre ?? string.Empty;
            txtHojaExcel.Text = _perfil.HojaExcel ?? string.Empty;
            chkTieneCabecera.Checked = _perfil.TieneCabecera;

            SeleccionarCombo(cmbFormatoFecha,     _perfil.FormatoFecha ?? "dd/MM/yyyy");
            SeleccionarCombo(cmbSeparadorDecimal, _perfil.SeparadorDecimal ?? ".");

            txtColFecha.Text    = _perfil.ColFecha    ?? string.Empty;
            txtColAsiento.Text  = _perfil.ColAsiento  ?? string.Empty;
            txtColConcepto.Text = _perfil.ColConcepto ?? string.Empty;
            txtColDebe.Text     = _perfil.ColDebe     ?? string.Empty;
            txtColHaber.Text    = _perfil.ColHaber    ?? string.Empty;

            ActualizarTituloColumnas();
            ActualizarEstadoCuentas();
        }

        private static void SeleccionarCombo(RadDropDownList cmb, string valor)
        {
            foreach (RadListDataItem item in cmb.Items)
                if (string.Equals(item.Value?.ToString(), valor, StringComparison.OrdinalIgnoreCase))
                { cmb.SelectedItem = item; return; }
            if (cmb.Items.Count > 0) cmb.SelectedIndex = 0;
        }

        // ── Estado de la pantalla ────────────────────────────────────────────────

        // Sin cabecera, los mismos campos se interpretan como número de columna (1, 2, …).
        private void ActualizarTituloColumnas() =>
            grpColumnas.Text = chkTieneCabecera.Checked
                ? "Columnas del mayor (nombre del encabezado)"
                : "Columnas del mayor (número de columna: 1, 2, 3…)";

        private void ActualizarEstadoCuentas()
        {
            bool hay = _cuentas.Count > 0;
            lblSinCuentas.Visible    = !hay;
            gridCuentas.Visible      = hay;
            btnEditarCuenta.Enabled  = hay;
            btnQuitarCuenta.Enabled  = hay;
        }

        private void ChkTieneCabecera_ToggleStateChanged(object sender, StateChangedEventArgs args)
            => ActualizarTituloColumnas();

        // ── Cuentas ──────────────────────────────────────────────────────────────

        private void BtnAgregarCuenta_Click(object sender, EventArgs e)
        {
            using var form = new FormCuentaPyR();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                _cuentas.Add(form.Cuenta);
                LimpiarRepetidos();
            }
        }

        private void BtnEditarCuenta_Click(object sender, EventArgs e)
        {
            if (gridCuentas.CurrentRow?.DataBoundItem is not CuentaPyR cuenta) return;

            using var form = new FormCuentaPyR(cuenta);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                int i = _cuentas.IndexOf(cuenta);
                if (i >= 0) _cuentas[i] = form.Cuenta;
                LimpiarRepetidos();
            }
        }

        private void GridCuentas_CellDoubleClick(object sender, GridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) BtnEditarCuenta_Click(sender, e);
        }

        private void BtnQuitarCuenta_Click(object sender, EventArgs e)
        {
            if (gridCuentas.CurrentRow?.DataBoundItem is not CuentaPyR cuenta) return;

            if (MessageBox.Show($"¿Quitar la cuenta {cuenta}?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _cuentas.Remove(cuenta);
                LimpiarRepetidos();
            }
        }

        private void LimpiarRepetidos()
        {
            if (_codigosRepetidos.Count == 0) return;
            _codigosRepetidos.Clear();
            gridCuentas.TableElement.Update(GridUINotifyAction.StateChanged);
        }

        private void GridCuentas_RowFormatting(object sender, RowFormattingEventArgs e)
        {
            bool repetido = e.RowElement.RowInfo.DataBoundItem is CuentaPyR c
                            && _codigosRepetidos.Contains(c.Codigo?.Trim() ?? string.Empty);
            if (repetido)
            {
                e.RowElement.DrawFill  = true;
                e.RowElement.GradientStyle = Telerik.WinControls.GradientStyles.Solid;
                e.RowElement.BackColor = Color.FromArgb(253, 226, 226);
                e.RowElement.ForeColor = Color.FromArgb(139, 26, 26);
            }
            else
            {
                e.RowElement.ResetValue(Telerik.WinControls.UI.LightVisualElement.DrawFillProperty, Telerik.WinControls.ValueResetFlags.Local);
                e.RowElement.ResetValue(Telerik.WinControls.UI.LightVisualElement.GradientStyleProperty, Telerik.WinControls.ValueResetFlags.Local);
                e.RowElement.ResetValue(Telerik.WinControls.VisualElement.BackColorProperty, Telerik.WinControls.ValueResetFlags.Local);
                e.RowElement.ResetValue(Telerik.WinControls.VisualElement.ForeColorProperty, Telerik.WinControls.ValueResetFlags.Local);
            }
        }

        // ── Guardar / Cancelar ───────────────────────────────────────────────────

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (!Validar()) return;
            GuardarDatos();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool Validar()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
                return Error("Ingresá un nombre para el perfil.", txtNombre);

            var columnas = new (string Nombre, RadTextBox Txt)[]
            {
                ("Fecha", txtColFecha), ("Asiento", txtColAsiento), ("Concepto", txtColConcepto),
                ("Debe", txtColDebe), ("Haber", txtColHaber)
            };
            foreach (var (nombre, txt) in columnas)
            {
                if (string.IsNullOrWhiteSpace(txt.Text))
                    return Error($"Completá la columna \"{nombre}\" del mayor.", txt);
                if (!chkTieneCabecera.Checked && (!int.TryParse(txt.Text.Trim(), out int n) || n < 1))
                    return Error($"Sin cabecera, la columna \"{nombre}\" tiene que ser un número de columna (1, 2, 3…).", txt);
            }

            // Mensaje de la maqueta 01-perfil-pyr-detalle-error: se nombran las cuentas en conflicto.
            var repetidos = _cuentas
                .GroupBy(c => c.Codigo?.Trim() ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .ToList();
            if (repetidos.Count > 0)
            {
                _codigosRepetidos = new HashSet<string>(repetidos.Select(g => g.Key), StringComparer.OrdinalIgnoreCase);
                gridCuentas.TableElement.Update(GridUINotifyAction.StateChanged);

                var detalle = string.Join("\n\n", repetidos.Select(g =>
                    $"El código de cuenta {g.Key} está repetido:\n" + string.Join("\n", g.Select(c => "· " + c.Nombre))));
                MessageBox.Show($"No se puede guardar el perfil.\n\n{detalle}\n\nCada cuenta del perfil tiene que tener un código distinto.",
                    "Perfil PyR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private bool Error(string mensaje, Control foco)
        {
            MessageBox.Show(mensaje, "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            foco.Focus();
            return false;
        }

        private void GuardarDatos()
        {
            _perfil.Nombre           = txtNombre.Text.Trim();
            _perfil.HojaExcel        = string.IsNullOrWhiteSpace(txtHojaExcel.Text) ? null : txtHojaExcel.Text.Trim();
            _perfil.TieneCabecera    = chkTieneCabecera.Checked;
            _perfil.FormatoFecha     = cmbFormatoFecha.SelectedItem?.Value?.ToString() ?? "dd/MM/yyyy";
            _perfil.SeparadorDecimal = cmbSeparadorDecimal.SelectedItem?.Value?.ToString() ?? ".";

            _perfil.ColFecha    = txtColFecha.Text.Trim();
            _perfil.ColAsiento  = txtColAsiento.Text.Trim();
            _perfil.ColConcepto = txtColConcepto.Text.Trim();
            _perfil.ColDebe     = txtColDebe.Text.Trim();
            _perfil.ColHaber    = txtColHaber.Text.Trim();

            _perfil.Cuentas = _cuentas.ToList();
        }
    }
}
