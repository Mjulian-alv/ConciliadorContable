using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ArcaCliente.Models;
using ArcaCliente.Services;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace ArcaCliente
{
    /// <summary>
    /// Fase 3: seleccion de comprobantes SOLO ARCA para exportar a PRESEA.
    /// El usuario escanea el QR de cada comprobante con un lector USB (keyboard-wedge):
    /// el lector "tipea" la URL del QR + Enter en <c>txtScan</c>. Se parsea offline
    /// (<see cref="ArcaQrParser"/>), se matchea por <see cref="ComprobanteClave"/> y se
    /// marca la fila. Tambien se puede tildar manualmente. La memoria anti-duplicado
    /// indica que comprobantes ya fueron exportados.
    /// </summary>
    public partial class FormExportarPreseaQr : Telerik.WinControls.UI.RadForm
    {
        private readonly List<ItemConciliacion> _origen;
        private BindingList<PreseaExportRow> _rows = new();
        private readonly Dictionary<string, PreseaExportRow> _porClave = new();

        private RadTextBox  txtScan;
        private RadLabel    lblScanInfo;
        private RadLabel    lblContadores;
        private RadGridView gridSeleccion;

        /// <summary>Items seleccionados al presionar Continuar (DialogResult.OK).</summary>
        public List<ItemConciliacion> ItemsSeleccionados { get; private set; } = new();

        public FormExportarPreseaQr(List<ItemConciliacion> soloArca)
        {
            InitializeComponent();
            Icon = AppIcons.Arca;

            _origen = (soloArca ?? new List<ItemConciliacion>())
                .Where(x => x.Estado == EstadoConciliacion.SoloARCA)
                .ToList();

            BuildUi();
            ConstruirFilas();
            ActualizarContadores();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            txtScan.Focus();
        }

        // ── UI ───────────────────────────────────────────────────────────────────────

        private void BuildUi()
        {
            var grpScan = new GroupBox { Text = "Escaneo", Location = new Point(12, 8), Size = new Size(796, 80), TabStop = false };
            grpScan.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            grpScan.Controls.Add(Build(new RadLabel(), l =>
            {
                l.Location = new Point(12, 20);
                l.Text = "Escanee el QR del comprobante (o tilde manualmente en la grilla):";
                l.Size = new Size(500, 16);
            }));

            txtScan = Build(new RadTextBox(), c => { c.Location = new Point(12, 44); c.Size = new Size(480, 22); });
            txtScan.KeyDown += TxtScan_KeyDown;
            grpScan.Controls.Add(txtScan);

            lblScanInfo = Build(new RadLabel(), l => { l.Location = new Point(504, 46); l.Size = new Size(280, 18); l.Text = string.Empty; });
            grpScan.Controls.Add(lblScanInfo);

            Controls.Add(grpScan);

            gridSeleccion = Build(new RadGridView(), g =>
            {
                g.Location = new Point(12, 96);
                g.Size = new Size(796, 360);
                g.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                g.ReadOnly = false;
                g.MasterTemplate.AllowAddNewRow = false;
                g.MasterTemplate.AllowDeleteRow = false;
                g.AutoGenerateColumns = false;
            });
            ConfigurarColumnas();
            gridSeleccion.CellValueChanged += (s, e) => ActualizarContadores();
            gridSeleccion.RowFormatting += GridSeleccion_RowFormatting;
            gridSeleccion.CellFormatting += GridSeleccion_CellFormatting;
            Controls.Add(gridSeleccion);

            lblContadores = Build(new RadLabel(), l =>
            {
                l.Location = new Point(12, 466);
                l.Size = new Size(360, 18);
                l.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            });
            Controls.Add(lblContadores);

            Controls.Add(Build(new RadButton(), b =>
            {
                b.Location = new Point(12, 484); b.Size = new Size(180, 28);
                b.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
                b.Text = "Seleccionar nuevos";
                b.Click += (s, e) => { MarcarTodos(soloNuevos: true); };
            }));
            Controls.Add(Build(new RadButton(), b =>
            {
                b.Location = new Point(200, 484); b.Size = new Size(140, 28);
                b.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
                b.Text = "Quitar seleccion";
                b.Click += (s, e) => { MarcarTodos(soloNuevos: false, marcar: false); };
            }));
            Controls.Add(Build(new RadButton(), b =>
            {
                b.Location = new Point(596, 484); b.Size = new Size(120, 28);
                b.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                b.Text = "Continuar";
                b.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                b.Click += BtnContinuar_Click;
            }));
            Controls.Add(Build(new RadButton(), b =>
            {
                b.Location = new Point(722, 484); b.Size = new Size(86, 28);
                b.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                b.Text = "Cancelar";
                b.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            }));
        }

        private void ConfigurarColumnas()
        {
            gridSeleccion.Columns.Add(new GridViewCheckBoxColumn { Name = "Seleccionado", FieldName = "Seleccionado", HeaderText = "Exportar", Width = 70 });
            gridSeleccion.Columns.Add(new GridViewTextBoxColumn { FieldName = "EstadoExport", HeaderText = "Estado", Width = 90,  ReadOnly = true });
            gridSeleccion.Columns.Add(new GridViewTextBoxColumn { FieldName = "Emisor",       HeaderText = "Emisor", Width = 210, ReadOnly = true });
            gridSeleccion.Columns.Add(new GridViewTextBoxColumn { FieldName = "Cuit",         HeaderText = "CUIT",   Width = 120, ReadOnly = true });
            gridSeleccion.Columns.Add(new GridViewTextBoxColumn { FieldName = "Tipo",         HeaderText = "Tipo",   Width = 170, ReadOnly = true });
            gridSeleccion.Columns.Add(new GridViewTextBoxColumn { FieldName = "Numero",       HeaderText = "Numero", Width = 120, ReadOnly = true });
            gridSeleccion.Columns.Add(new GridViewTextBoxColumn { FieldName = "Fecha",        HeaderText = "Fecha",  Width = 90,  ReadOnly = true });
            gridSeleccion.Columns.Add(new GridViewTextBoxColumn { FieldName = "Total",        HeaderText = "Total",  Width = 110, ReadOnly = true, TextAlignment = ContentAlignment.MiddleRight });
        }

        private void GridSeleccion_RowFormatting(object sender, RowFormattingEventArgs e)
        {
            if (e.RowElement.RowInfo?.DataBoundItem is PreseaExportRow r && r.YaExportado)
            {
                e.RowElement.DrawFill = true;
                e.RowElement.GradientStyle = Telerik.WinControls.GradientStyles.Solid;
                e.RowElement.BackColor = Color.FromArgb(235, 235, 235);
                e.RowElement.ForeColor = Color.Black;
            }
            else
            {
                e.RowElement.ResetValue(LightVisualElement.DrawFillProperty, ValueResetFlags.Local);
                e.RowElement.ResetValue(LightVisualElement.BackColorProperty, ValueResetFlags.Local);
                e.RowElement.ResetValue(LightVisualElement.ForeColorProperty, ValueResetFlags.Local);
            }
        }

        private void GridSeleccion_CellFormatting(object sender, CellFormattingEventArgs e)
        {
            if (e.CellElement.RowInfo?.DataBoundItem is PreseaExportRow r && r.YaExportado)
            {
                e.CellElement.DrawFill = true;
                e.CellElement.GradientStyle = Telerik.WinControls.GradientStyles.Solid;
                e.CellElement.BackColor = Color.FromArgb(235, 235, 235);
                e.CellElement.ForeColor = Color.Black;
            }
            else
            {
                e.CellElement.ResetValue(LightVisualElement.DrawFillProperty, ValueResetFlags.Local);
                e.CellElement.ResetValue(LightVisualElement.BackColorProperty, ValueResetFlags.Local);
                e.CellElement.ResetValue(LightVisualElement.ForeColorProperty, ValueResetFlags.Local);
            }
        }

        // ── Construccion de filas ────────────────────────────────────────────────────

        private void ConstruirFilas()
        {
            var exportadas = PreseaExportMemoryStorage.ClavesExportadas();
            _rows = new BindingList<PreseaExportRow>();
            _porClave.Clear();

            foreach (var item in _origen)
            {
                var csv = item.SourceArca;

                string cuit = !string.IsNullOrWhiteSpace(csv?.NroDocEmisor) ? csv.NroDocEmisor : item.CuitProveedor;
                string tipo = !string.IsNullOrWhiteSpace(csv?.TipoComprobante) ? csv.TipoComprobante : item.TipoComprobante;
                string pv   = !string.IsNullOrWhiteSpace(csv?.PuntoVenta) ? csv.PuntoVenta : item.PuntoVenta;
                string nro  = !string.IsNullOrWhiteSpace(csv?.NumeroDesde) ? csv.NumeroDesde : item.Numero;

                string clave   = ComprobanteClave.Generar(cuit, tipo, pv, nro);
                bool   yaExp   = exportadas.Contains(clave);
                string descTipo = !string.IsNullOrWhiteSpace(item.DescripcionTipoComprobante)
                    ? item.DescripcionTipoComprobante
                    : TablaComprobanteDescripcionMapper.ObtenerDescripcion(tipo);

                var row = new PreseaExportRow
                {
                    Seleccionado = false,
                    YaExportado  = yaExp,
                    EstadoExport = yaExp ? "Ya exportado" : "Nuevo",
                    Emisor       = item.Emisor ?? csv?.DenominacionEmisor ?? string.Empty,
                    Cuit         = Services.CuitFormatter.Formatear(cuit),
                    Tipo         = string.IsNullOrWhiteSpace(descTipo) ? tipo : $"{tipo} - {descTipo}",
                    Numero       = $"{(pv ?? string.Empty).PadLeft(4, '0')}-{(nro ?? string.Empty).PadLeft(8, '0')}",
                    Fecha        = item.Fecha ?? csv?.FechaEmision ?? string.Empty,
                    Total        = item.TotalARCA ?? csv?.ImpTotal ?? string.Empty,
                    Clave        = clave,
                    Item         = item,
                };

                _rows.Add(row);
                _porClave[clave] = row;   // ultimo gana si hubiera claves repetidas
            }

            gridSeleccion.DataSource = _rows;
        }

        // ── Escaneo ──────────────────────────────────────────────────────────────────

        private void TxtScan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            e.Handled = true;

            string raw = txtScan.Text;
            txtScan.Clear();
            ProcesarScan(raw);
            txtScan.Focus();
        }

        private void ProcesarScan(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return;

            if (!ArcaQrParser.TryParse(raw, out var data, out var error))
            {
                SetScanInfo("QR invalido: " + error, Color.DarkRed);
                return;
            }

            if (_porClave.TryGetValue(data.Clave, out var row))
            {
                row.Seleccionado = true;
                SeleccionarFila(row);
                ActualizarContadores();

                if (row.YaExportado)
                    SetScanInfo($"YA EXPORTADO antes: {row.Numero}. Se marca igual.", Color.DarkOrange);
                else
                    SetScanInfo($"OK: {row.Emisor}  {row.Numero}", Color.DarkGreen);
            }
            else
            {
                SetScanInfo($"No esta en SOLO ARCA: {data.PtoVta:0000}-{data.NroCmp:00000000} (CUIT {data.Cuit}).", Color.DarkOrange);
            }
        }

        private void SeleccionarFila(PreseaExportRow row)
        {
            foreach (var r in gridSeleccion.Rows)
            {
                if (!ReferenceEquals(r.DataBoundItem, row)) continue;
                gridSeleccion.CurrentRow = r;
                r.IsSelected = true;
                try { gridSeleccion.TableElement.ScrollToRow(r); } catch { /* no critico */ }
                break;
            }
        }

        // ── Acciones masivas ─────────────────────────────────────────────────────────

        private void MarcarTodos(bool soloNuevos, bool marcar = true)
        {
            foreach (var r in _rows)
            {
                if (marcar && soloNuevos && r.YaExportado) continue;
                r.Seleccionado = marcar;
            }
            ActualizarContadores();
        }

        private void BtnContinuar_Click(object sender, EventArgs e)
        {
            ItemsSeleccionados = _rows.Where(r => r.Seleccionado).Select(r => r.Item).ToList();
            if (ItemsSeleccionados.Count == 0)
            {
                MessageBox.Show("Seleccione al menos un comprobante (escaneando o tildando).",
                    "Sin seleccion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int yaExp = _rows.Count(r => r.Seleccionado && r.YaExportado);
            if (yaExp > 0 &&
                MessageBox.Show(
                    $"{yaExp} de los seleccionados ya figuran como exportados.\n\nContinuar de todos modos?",
                    "Comprobantes ya exportados", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            DialogResult = DialogResult.OK;
            Close();
        }

        // ── Helpers ──────────────────────────────────────────────────────────────────

        private void ActualizarContadores()
        {
            int sel = _rows.Count(r => r.Seleccionado);
            int exp = _rows.Count(r => r.YaExportado);
            lblContadores.Text = $"Seleccionados: {sel} / {_rows.Count}     Ya exportados: {exp}";
        }

        private void SetScanInfo(string texto, Color color)
        {
            lblScanInfo.Text = texto;
            lblScanInfo.ForeColor = color;
        }

        private static T Build<T>(T ctrl, Action<T> cfg) where T : Control
        {
            if (ctrl is ISupportInitialize si) si.BeginInit();
            cfg(ctrl);
            if (ctrl is ISupportInitialize si2) si2.EndInit();
            return ctrl;
        }
    }
}
