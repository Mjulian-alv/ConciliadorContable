using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ArcaCliente.Models;
using ArcaCliente.Services;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace ArcaCliente
{
    /// <summary>
    /// Fase 4 + disparo de Fase 5: muestra los comprobantes seleccionados con sus datos
    /// resueltos (config general + mapa por proveedor), permite editar lo necesario y genera
    /// el TXT delimitado por '|' para PRESEA. Al generar, registra los comprobantes en la
    /// memoria anti-duplicado.
    /// </summary>
    public partial class FormCompletarPresea : Telerik.WinControls.UI.RadForm
    {
        private readonly ConfigPresea _cfg;
        private readonly string _perfilId;
        private BindingList<PreseaLineaExport> _lineas;

        private RadGridView gridLineas;
        private RadLabel    lblEstado;

        public FormCompletarPresea(List<ItemConciliacion> items, PerfilOffline perfil)
        {
            InitializeComponent();
            Icon = AppIcons.Arca;

            _cfg      = perfil?.ConfigPresea ?? new ConfigPresea();
            _perfilId = perfil?.Id.ToString() ?? string.Empty;

            BuildUi();

            var lista = PreseaExportResolver.Resolver(items ?? new List<ItemConciliacion>(), _cfg);
            _lineas = new BindingList<PreseaLineaExport>(lista);
            gridLineas.DataSource = _lineas;

            ActualizarEstado();
        }

        // ── UI ───────────────────────────────────────────────────────────────────────

        private void BuildUi()
        {
            gridLineas = Build(new RadGridView(), g =>
            {
                g.Location = new Point(12, 12);
                g.Size = new Size(936, 470);
                g.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                g.ReadOnly = false;
                g.MasterTemplate.AllowAddNewRow = false;
                g.MasterTemplate.AllowDeleteRow = false;
                g.AutoGenerateColumns = false;
            });
            ConfigurarColumnas();
            gridLineas.RowFormatting  += GridLineas_RowFormatting;
            gridLineas.CellFormatting += GridLineas_CellFormatting;
            Controls.Add(gridLineas);

            lblEstado = Build(new RadLabel(), l =>
            {
                l.Location = new Point(12, 492);
                l.Size = new Size(560, 18);
                l.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            });
            Controls.Add(lblEstado);

            Controls.Add(Build(new RadButton(), b =>
            {
                b.Location = new Point(740, 500); b.Size = new Size(120, 28);
                b.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                b.Text = "Generar TXT";
                b.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                b.Click += BtnGenerar_Click;
            }));
            Controls.Add(Build(new RadButton(), b =>
            {
                b.Location = new Point(866, 500); b.Size = new Size(86, 28);
                b.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                b.Text = "Cancelar";
                b.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            }));
        }

        private void ConfigurarColumnas()
        {
            gridLineas.Columns.Add(new GridViewTextBoxColumn { FieldName = "Comprobante",     HeaderText = "Comprobante",       Width = 210, ReadOnly = true });
            gridLineas.Columns.Add(new GridViewTextBoxColumn { FieldName = "Emisor",          HeaderText = "Emisor",            Width = 170, ReadOnly = true });
            gridLineas.Columns.Add(new GridViewTextBoxColumn { FieldName = "Importe",         HeaderText = "Importe",           Width = 90,  ReadOnly = true, TextAlignment = ContentAlignment.MiddleRight });
            gridLineas.Columns.Add(new GridViewTextBoxColumn { FieldName = "CodigoProveedor", HeaderText = "Cod. prov.",        Width = 80 });
            gridLineas.Columns.Add(new GridViewTextBoxColumn { FieldName = "CuentaProveedor", HeaderText = "Cta. proveedor",    Width = 110 });
            gridLineas.Columns.Add(new GridViewTextBoxColumn { FieldName = "CuentaDebe",      HeaderText = "Cta. debe",         Width = 110 });
            gridLineas.Columns.Add(new GridViewTextBoxColumn { FieldName = "Centro",          HeaderText = "Centro",            Width = 80 });
            gridLineas.Columns.Add(new GridViewTextBoxColumn { FieldName = "VencimientoCai",  HeaderText = "Vto. CAI",          Width = 90 });
            gridLineas.Columns.Add(new GridViewTextBoxColumn { FieldName = "Observacion",     HeaderText = "Observacion",       Width = 140 });
            gridLineas.Columns.Add(new GridViewTextBoxColumn { FieldName = "Estado",          HeaderText = "Estado",            Width = 120, ReadOnly = true });
        }

        private void GridLineas_RowFormatting(object sender, RowFormattingEventArgs e)
        {
            if (e.RowElement.RowInfo?.DataBoundItem is PreseaLineaExport l && !l.ProveedorEnMapa)
            {
                e.RowElement.DrawFill = true;
                e.RowElement.GradientStyle = GradientStyles.Solid;
                e.RowElement.BackColor = Color.FromArgb(136, 207, 148);
                e.RowElement.ForeColor = Color.Black;
            }
            else
            {
                e.RowElement.ResetValue(LightVisualElement.DrawFillProperty,  ValueResetFlags.Local);
                e.RowElement.ResetValue(LightVisualElement.BackColorProperty, ValueResetFlags.Local);
                e.RowElement.ResetValue(LightVisualElement.ForeColorProperty, ValueResetFlags.Local);
            }
        }

        private void GridLineas_CellFormatting(object sender, CellFormattingEventArgs e)
        {
            if (e.CellElement.RowInfo?.DataBoundItem is PreseaLineaExport l && !l.ProveedorEnMapa)
            {
                e.CellElement.DrawFill = true;
                e.CellElement.GradientStyle = GradientStyles.Solid;
                e.CellElement.BackColor = Color.FromArgb(136, 207, 148);
                e.CellElement.ForeColor = Color.Black;
            }
            else
            {
                e.CellElement.ResetValue(LightVisualElement.DrawFillProperty, ValueResetFlags.Local);
                e.CellElement.ResetValue(LightVisualElement.BackColorProperty, ValueResetFlags.Local);
                e.CellElement.ResetValue(LightVisualElement.ForeColorProperty, ValueResetFlags.Local);
            }
        }

        // ── Generar

        private void BtnGenerar_Click(object sender, EventArgs e)
        {
            gridLineas.EndEdit();

            var faltantes = new List<string>();
            foreach (var l in _lineas)
            {
                if (string.IsNullOrWhiteSpace(l.CodigoProveedor)) faltantes.Add($"{l.Comprobante}: falta codigo de proveedor.");
                if (string.IsNullOrWhiteSpace(l.CuentaProveedor)) faltantes.Add($"{l.Comprobante}: falta cuenta de proveedor.");
                if (string.IsNullOrWhiteSpace(l.CuentaDebe))      faltantes.Add($"{l.Comprobante}: falta cuenta del debe.");
            }
            if (string.IsNullOrWhiteSpace(_cfg.CuentaIVA))
                faltantes.Add("Falta la Cuenta IVA en la configuracion general de PRESEA.");

            if (faltantes.Count > 0)
            {
                MessageBox.Show(
                    "No se puede generar el TXT. Corrija:\n\n  - " + string.Join("\n  - ", faltantes.Take(15)) +
                    (faltantes.Count > 15 ? $"\n  ... y {faltantes.Count - 15} mas." : string.Empty),
                    "Datos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dlg = new SaveFileDialog
            {
                Title = "Guardar archivo de importacion PRESEA",
                Filter = "Archivo de texto (*.txt)|*.txt",
                FileName = $"PRESEA_{DateTime.Now:yyyyMMdd_HHmm}.txt",
                DefaultExt = "txt",
                OverwritePrompt = true
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                int n = PreseaTxtExportador.Exportar(_lineas, dlg.FileName, _cfg, _perfilId);

                lblEstado.Text = $"Generado: {Path.GetFileName(dlg.FileName)}  ({n} comprobante(s))";
                lblEstado.ForeColor = Color.DarkGreen;

                if (MessageBox.Show(
                        $"Se generaron {n} comprobante(s) en:\n{dlg.FileName}\n\nAbrir la carpeta?",
                        "Exportacion PRESEA completa", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer.exe", $"/select,\"{dlg.FileName}\""));

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                lblEstado.Text = "Error al generar.";
                lblEstado.ForeColor = Color.DarkRed;
                MessageBox.Show($"Error al generar el TXT:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────────────

        private void ActualizarEstado()
        {
            int sinProv = _lineas.Count(l => !l.ProveedorEnMapa);
            lblEstado.Text = sinProv > 0
                ? $"{_lineas.Count} comprobante(s).  {sinProv} sin proveedor en el mapa (fila resaltada) - complete los datos."
                : $"{_lineas.Count} comprobante(s) listos para generar.";
            lblEstado.ForeColor = sinProv > 0 ? Color.DarkOrange : Color.DarkGreen;
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
