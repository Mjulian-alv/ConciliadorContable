using System;
using System.Collections.Generic;
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
    /// Fase 4 + disparo de Fase 5: revisa los comprobantes seleccionados uno a uno (mismo
    /// patron que <see cref="FormProcesarSoloArca"/> para Octosis), permite completar los datos
    /// de PRESEA y redistribuir "Otros Tributos" (ARCA) entre los campos de percepcion/impuesto
    /// del layout (por defecto, 100% a Percepcion IIBB). Al confirmar el ultimo comprobante,
    /// genera el TXT delimitado por '|' para PRESEA y registra los exportados en la memoria
    /// anti-duplicado.
    /// </summary>
    public partial class FormCompletarPresea : Telerik.WinControls.UI.RadForm
    {
        private readonly ConfigPresea _cfg;
        private readonly string _perfilId;
        private readonly List<PreseaLineaExport> _lineas;
        private readonly List<PreseaLineaExport> _confirmadas = new();

        private int _currentIndex;
        private int _omitidos;
        private bool _actualizandoGrid;

        public FormCompletarPresea(List<ItemConciliacion> items, PerfilOffline perfil)
        {
            InitializeComponent();
            Icon = AppIcons.Arca;

            _cfg = perfil?.ConfigPresea ?? new ConfigPresea();
            _perfilId = perfil?.Id.ToString() ?? string.Empty;

            ConfigurarColumnasGrid();
            dgvPercepciones.CellValueChanged += DgvPercepciones_CellValueChanged;

            ConfigurarColumnasDetalleIva();

            _lineas = PreseaExportResolver.Resolver(items ?? new List<ItemConciliacion>(), _cfg);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_lineas.Count == 0)
            {
                MessageBox.Show("No hay comprobantes para completar.", "Sin pendientes",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            MostrarItem(0);
        }

        // ── Configuracion de la grilla de percepciones ──────────────────────────────

        private void ConfigurarColumnasGrid()
        {
            dgvPercepciones.Columns.Add(new GridViewTextBoxColumn
            {
                Name = "Concepto",
                HeaderText = "Concepto",
                MinWidth = 150,
                ReadOnly = true
            });

            var colCampo = new GridViewComboBoxColumn
            {
                Name = "CampoDestino",
                HeaderText = "Campo PRESEA",
                MinWidth = 180,
                DisplayMember = "Descripcion",
                ValueMember = "Codigo",
                DropDownStyle = RadDropDownStyle.DropDownList,
                DataSource = PreseaCalculos.CamposPercepcion
                    .Select(c => new { c.Codigo, c.Descripcion })
                    .ToList(),
            };
            dgvPercepciones.Columns.Add(colCampo);

            dgvPercepciones.Columns.Add(new GridViewDecimalColumn
            {
                Name = "Importe",
                HeaderText = "Importe",
                MinWidth = 110,
                FormatString = "{0:N2}",
                TextAlignment = ContentAlignment.MiddleRight,
                DataType = typeof(decimal)
            });
        }

        // ── Detalle de IVA / Exento / No Gravado (solo lectura, para comprobacion) ─────

        private void ConfigurarColumnasDetalleIva()
        {
            dgvDetalleIva.Columns.Add(new GridViewTextBoxColumn
            {
                Name = "Concepto",
                HeaderText = "Concepto (segun ARCA)",
                MinWidth = 150,
                ReadOnly = true
            });
            dgvDetalleIva.Columns.Add(new GridViewDecimalColumn
            {
                Name = "Neto",
                HeaderText = "Neto",
                MinWidth = 100,
                ReadOnly = true,
                FormatString = "{0:N2}",
                TextAlignment = ContentAlignment.MiddleRight,
                DataType = typeof(decimal)
            });
            dgvDetalleIva.Columns.Add(new GridViewDecimalColumn
            {
                Name = "Iva",
                HeaderText = "IVA",
                MinWidth = 100,
                ReadOnly = true,
                FormatString = "{0:N2}",
                TextAlignment = ContentAlignment.MiddleRight,
                DataType = typeof(decimal)
            });
            dgvDetalleIva.Columns.Add(new GridViewTextBoxColumn
            {
                Name = "Slot",
                HeaderText = "Mapeo a PRESEA",
                MinWidth = 220,
                ReadOnly = true
            });
        }

        private void CargarDetalleIva(PreseaLineaExport l)
        {
            dgvDetalleIva.Rows.Clear();
            var detalle = PreseaCalculos.DetalleIva(l.Csv);
            if (detalle.Count == 0)
            {
                dgvDetalleIva.Rows.Add("Sin IVA / Exento / No Gravado informado por ARCA", 0m, 0m, string.Empty);
                return;
            }
            foreach (var d in detalle)
                dgvDetalleIva.Rows.Add(d.Concepto, d.Neto, d.Iva, d.Slot);
        }

        // ── Navegacion ───────────────────────────────────────────────────────────────

        private void MostrarItem(int index)
        {
            _currentIndex = index;

            if (index >= _lineas.Count)
            {
                MostrarResumen();
                return;
            }

            var l = _lineas[index];

            lblProgreso.Text = $"Comprobante {index + 1} de {_lineas.Count}";
            lblComprobante.Text = $"Comprobante: {l.Comprobante}";
            lblEmisor.Text = $"Emisor:      {l.Emisor}";
            lblImporte.Text = $"Importe:     $ {l.Importe}";

            lblAvisoProveedor.Text = l.ProveedorEnMapa ? string.Empty
                : "Proveedor no encontrado en el mapa de PRESEA - revise los datos abajo.";
            lblAvisoProveedor.Visible = !l.ProveedorEnMapa;

            if (l.OtrosTributosOriginal != 0m)
            {
                lblOtrosTributos.Text = $"Otros tributos informados por ARCA: $ {l.OtrosTributosOriginal:N2}  (a distribuir abajo)";
                lblOtrosTributos.Visible = true;
                grpImpuestos.Text = $"Percepciones / Otros Tributos (ARCA)  -  Total a distribuir: $ {l.OtrosTributosOriginal:N2}";
            }
            else
            {
                lblOtrosTributos.Visible = false;
                grpImpuestos.Text = "Percepciones / Otros Tributos (ARCA)  -  Sin otros tributos informados por ARCA";
            }

            txtCodigoProveedor.Text = l.CodigoProveedor;
            txtCuentaProveedor.Text = l.CuentaProveedor;
            txtCuentaDebe.Text = l.CuentaDebe;
            txtCentro.Text = l.Centro;
            txtVencimientoCai.Text = l.VencimientoCai;
            txtObservacion.Text = l.Observacion;

            CargarDetalleIva(l);
            CargarGridPercepciones(l);

            SetEstado(string.Empty, Color.Black);
        }

        private void CargarGridPercepciones(PreseaLineaExport l)
        {
            _actualizandoGrid = true;
            dgvPercepciones.Rows.Clear();
            foreach (var p in l.Percepciones)
                dgvPercepciones.Rows.Add(p.Concepto, p.CampoDestino, p.Importe);
            _actualizandoGrid = false;
        }

        // ── Redistribucion automatica (fila "Diferencia / Restante") ───────────────────

        private void DgvPercepciones_CellValueChanged(object sender, GridViewCellEventArgs e)
        {
            if (_actualizandoGrid) return;
            if (e.Column.Name != "Importe" && e.Column.Name != "CampoDestino") return;

            var linea = _lineas[_currentIndex];
            decimal objetivo = linea.OtrosTributosOriginal;

            decimal suma = 0m;
            GridViewRowInfo filaDiferencia = null;
            foreach (var row in dgvPercepciones.Rows)
            {
                if (row.Cells["Concepto"].Value?.ToString() == PreseaPercepcionLinea.DiferenciaConcepto)
                {
                    filaDiferencia = row;
                    continue;
                }
                suma += Convert.ToDecimal(row.Cells["Importe"].Value ?? 0m);
            }

            decimal diff = objetivo - suma;

            _actualizandoGrid = true;
            if (diff != 0m)
            {
                if (filaDiferencia != null)
                    filaDiferencia.Cells["Importe"].Value = diff;
                else
                    dgvPercepciones.Rows.Add(PreseaPercepcionLinea.DiferenciaConcepto, string.Empty, diff);
            }
            else if (filaDiferencia != null)
            {
                dgvPercepciones.Rows.Remove(filaDiferencia);
            }
            _actualizandoGrid = false;
        }

        // ── Confirmar / Omitir / Cancelar ───────────────────────────────────────────

        private void btnConfirmar_Click(object sender, EventArgs e)
        {
            dgvPercepciones.EndEdit();

            var faltantes = new List<string>();
            if (string.IsNullOrWhiteSpace(txtCodigoProveedor.Text)) faltantes.Add("Falta el codigo de proveedor.");
            if (string.IsNullOrWhiteSpace(txtCuentaProveedor.Text)) faltantes.Add("Falta la cuenta de proveedor.");
            if (string.IsNullOrWhiteSpace(txtCuentaDebe.Text)) faltantes.Add("Falta la cuenta del debe.");
            if (string.IsNullOrWhiteSpace(_cfg.CuentaIVA))
                faltantes.Add("Falta la Cuenta IVA en la configuracion general de PRESEA.");

            foreach (var row in dgvPercepciones.Rows)
            {
                if (string.IsNullOrWhiteSpace(row.Cells["CampoDestino"].Value?.ToString()))
                    faltantes.Add($"Falta elegir el campo PRESEA para \"{row.Cells["Concepto"].Value}\".");
            }

            if (faltantes.Count > 0)
            {
                MessageBox.Show(
                    "No se puede confirmar este comprobante. Corrija:\n\n  - " + string.Join("\n  - ", faltantes),
                    "Datos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var l = _lineas[_currentIndex];
            l.CodigoProveedor = txtCodigoProveedor.Text.Trim();
            l.CuentaProveedor = txtCuentaProveedor.Text.Trim();
            l.CuentaDebe = txtCuentaDebe.Text.Trim();
            l.Centro = txtCentro.Text.Trim();
            l.VencimientoCai = txtVencimientoCai.Text.Trim();
            l.Observacion = txtObservacion.Text.Trim();

            var percepciones = new List<PreseaPercepcionLinea>();
            foreach (var row in dgvPercepciones.Rows)
            {
                percepciones.Add(new PreseaPercepcionLinea
                {
                    Concepto = row.Cells["Concepto"].Value?.ToString() ?? string.Empty,
                    CampoDestino = row.Cells["CampoDestino"].Value?.ToString() ?? string.Empty,
                    Importe = Convert.ToDecimal(row.Cells["Importe"].Value ?? 0m),
                });
            }
            l.Percepciones = percepciones;

            _confirmadas.Add(l);
            MostrarItem(_currentIndex + 1);
        }

        private void btnOmitir_Click(object sender, EventArgs e)
        {
            _omitidos++;
            MostrarItem(_currentIndex + 1);
        }

        private void btnCancelarTodo_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                    "Cancelar el proceso? Los comprobantes ya confirmados no se exportan.",
                    "Confirmar cancelacion",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        // ── Resumen final / generacion del TXT ──────────────────────────────────────

        private void MostrarResumen()
        {
            grpInfo.Visible = false;
            grpDatos.Visible = false;
            grpDetalleIva.Visible = false;
            grpImpuestos.Visible = false;
            pnlBotones.Visible = false;
            grpResumen.Visible = true;
            lblProgreso.Text = "Revision finalizada";

            lblResumen.Text = $"Confirmados: {_confirmadas.Count}    Omitidos: {_omitidos}\n" +
                (_confirmadas.Count > 0
                    ? "Presione \"Generar TXT\" para exportar los comprobantes confirmados."
                    : "No hay comprobantes confirmados para exportar.");

            btnGenerarTxt.Enabled = _confirmadas.Count > 0;
        }

        private void btnGenerarTxt_Click(object sender, EventArgs e)
        {
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
                int n = PreseaTxtExportador.Exportar(_confirmadas, dlg.FileName, _cfg, _perfilId);

                SetEstado($"Generado: {Path.GetFileName(dlg.FileName)}  ({n} comprobante(s))", Color.DarkGreen);

                if (MessageBox.Show(
                        $"Se generaron {n} comprobante(s) en:\n{dlg.FileName}\n\nAbrir la carpeta?",
                        "Exportacion PRESEA completa", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer.exe", $"/select,\"{dlg.FileName}\""));

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                SetEstado("Error al generar.", Color.DarkRed);
                MessageBox.Show($"Error al generar el TXT:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCerrarSinGenerar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        // ── Helpers ──────────────────────────────────────────────────────────────────

        private void SetEstado(string texto, Color color)
        {
            lblEstado.Text = texto;
            lblEstado.ForeColor = color;
        }


    }
}
