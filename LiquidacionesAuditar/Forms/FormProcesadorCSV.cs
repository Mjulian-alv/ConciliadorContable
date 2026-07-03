using LiquidacionesAuditar.Data;
using LiquidacionesAuditar.Models;
using LiquidacionesAuditar.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Windows.Forms;
using Telerik.WinControls.UI;

namespace LiquidacionesAuditar
{
    public partial class FormProcesadorCSV : RadForm
    {
        private List<Dictionary<string, string>> _filasCargadas;
        private List<string> _columnasCargadas;
        private List<MarcaTarjeta> _marcas;
        private MarcaTarjeta _marcaActual;
        private FiltroLiquidacion _filtroActivo;
        public FormProcesadorCSV()
        {
            InitializeComponent();
            CargarMarcas();
            setSepDecMil();
        }

        private void CargarMarcas()
        {
            _marcas = Repositorio.GetMarcas();
            cmbMarca.DataSource = null;
            cmbMarca.DataSource = _marcas;
            cmbMarca.DisplayMember = "Nombre";
            cmbMarca.ValueMember = "Id";
        }

        private void btnSeleccionarCSV_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Archivos soportados (*.csv;*.xlsx;*.xls)|*.csv;*.xlsx;*.xls|CSV (*.csv)|*.csv|Excel (*.xlsx;*.xls)|*.xlsx;*.xls|Todos (*.*)|*.*",
                Title = "Seleccionar archivo CSV o Excel"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            txtRutaCSV.Text = dlg.FileName;

            try
            {
                var ext = System.IO.Path.GetExtension(dlg.FileName).ToLowerInvariant();
                List<string> cols;
                List<Dictionary<string, string>> filas;

                if (ext == ".xls" || ext == ".xlsx")
                {
                    var marca = _marcas.FirstOrDefault(
                        m => m.Id == (cmbMarca.SelectedValue?.ToString() ?? ""));
                    int filaEnc = marca?.FilaEncabezado ?? 1;
                    (cols, filas) = ExcelReader.Leer(dlg.FileName, filaEnc);
                }
                else
                {
                    (cols, filas) = CsvReader.Leer(dlg.FileName);
                }
                _columnasCargadas = cols;
                _filasCargadas = filas;

                lblEstado.Text = $"✔ Archivo cargado: {filas.Count} filas, {cols.Count} columnas.";
                lblEstado.ForeColor = System.Drawing.Color.DarkGreen;

                // Mostrar preview en grid
                var dt = new System.Data.DataTable();
                foreach (var col in cols.Take(20)) dt.Columns.Add(col);
                foreach (var fila in filas.Take(50))
                {
                    var row = dt.NewRow();
                    foreach (var col in cols.Take(20))
                        row[col] = fila.ContainsKey(col) ? fila[col] : "";
                    dt.Rows.Add(row);
                }
                gridPreview.DataSource = dt;
                gridPreview.BestFitColumns();

                // Validar columnas contra BD
                ValidarColumnas(cols);
            }
            catch (Exception ex)
            {
                lblEstado.Text = $"❌ Error al leer el archivo: {ex.Message}";
                lblEstado.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void ValidarColumnas(List<string> colsArchivo)
        {
            var idMarca = cmbMarca.SelectedValue?.ToString() ?? "";
            if (string.IsNullOrWhiteSpace(idMarca)) return;

            var colsBD = Repositorio.GetColumnasCSV(idMarca)
                         .Select(c => c.IdColumnaArchivo)
                         .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var soloEnArchivo = colsArchivo.Where(c => !colsBD.Contains(c)).ToList();
            var soloEnBD = colsBD.Where(c => !colsArchivo.Contains(c, StringComparer.OrdinalIgnoreCase)).OrderBy(x => x).ToList();

            if (soloEnArchivo.Count == 0 && soloEnBD.Count == 0)
            {
                lblValidacion.Text = "✔ Columnas OK — sin diferencias.";
                lblValidacion.ForeColor = System.Drawing.Color.DarkGreen;
                btnVerDiferencias.Visible = false;
            }
            else
            {
                lblValidacion.Text = $"⚠ Diferencias detectadas: {soloEnArchivo.Count} nuevas en archivo, {soloEnBD.Count} ausentes.";
                lblValidacion.ForeColor = System.Drawing.Color.OrangeRed;
                btnVerDiferencias.Tag = new object[] { soloEnArchivo, soloEnBD };
                btnVerDiferencias.Visible = true;
            }
        }

        private void btnVerDiferencias_Click(object sender, EventArgs e)
        {
            var idMarca = cmbMarca.SelectedValue?.ToString() ?? "";
            var marca = Repositorio.GetMarcas().FirstOrDefault(m => m.Id == idMarca);
            if (marca == null || btnVerDiferencias.Tag == null) return;
            var arr = (object[])btnVerDiferencias.Tag;
            using var frm = new FormDiferenciasColumnas((List<string>)arr[0], (List<string>)arr[1], marca);
            frm.ShowDialog(this);
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            if (_filasCargadas == null || _filasCargadas.Count == 0)
            { MessageBox.Show("Primero cargue un archivo CSV.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var idMarca = cmbMarca.SelectedValue?.ToString() ?? "";
            if (string.IsNullOrWhiteSpace(idMarca))
            { MessageBox.Show("Seleccione una marca/procesador.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var nroBase = txtNroBase.Text.Trim();
            if (string.IsNullOrWhiteSpace(nroBase)) nroBase = "1000";

            var marcaObj = _marcas.FirstOrDefault(m => m.Id == idMarca);
            var columnaLiq = marcaObj?.ColumnaLiquidacion ?? "";

            char sepDec = string.IsNullOrWhiteSpace(txtSepDec.Text)
                          ? System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]
                          : txtSepDec.Text[0];
            char sepMil = string.IsNullOrWhiteSpace(txtSepMil.Text)
                          ? System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator[0]
                          : txtSepMil.Text[0];

            try
            {
                var filasFiltradas = AplicarFiltro(_filasCargadas);
                if (filasFiltradas.Count == 0)
                {
                    MessageBox.Show("El filtro activo no dejó ninguna fila. Revisá el rango configurado.",
                        "Filtro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (filasFiltradas.Count < _filasCargadas.Count)
                    lblEstado.Text = $"✔ Filtro activo: {filasFiltradas.Count} de {_filasCargadas.Count} filas incluidas.";

                var contenido = ExportadorLiquidacion.Generar(idMarca, filasFiltradas, nroBase, columnaLiq, sepDec, sepMil);
                txtPreviewSalida.Text = contenido;

              
            }
            catch (Exception ex)
            {
                lblEstado.Text = $"❌ Error al exportar: {ex.Message}";
                lblEstado.ForeColor = System.Drawing.Color.Red;
                MessageBox.Show($"Error al generar el archivo:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_Ayuda_Click(object? sender, EventArgs e)
        {
            using var frm = new FormAyudaProcesadorCSV();
            frm.ShowDialog(this);
        }

        private void btn_Filtros_Click(object? sender, EventArgs e)
        {
            if (_marcaActual == null)
            {
                MessageBox.Show("Seleccione una marca/procesador primero.", "Filtro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(_marcaActual.ColumnaFiltro))
            {
                MessageBox.Show(
                    "La marca no tiene una columna de filtro configurada.\nDefinila en Columnas CSV → Filtro de Liquidacion.",
                    "Filtro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var tipo = _marcaActual.TipoDato?.ToUpperInvariant() switch
            {
                "INT"     => TipoFiltro.Int,
                "DECIMAL" => TipoFiltro.Decimal,
                "DATE"    => TipoFiltro.Date,
                _         => TipoFiltro.String
            };

            using var frm = new FormFiltroLiquidacion(_marcaActual.ColumnaFiltro, tipo, _filtroActivo);
            if (frm.ShowDialog(this) != DialogResult.OK) return;

            _filtroActivo = frm.Resultado;
            ActualizarIndicadorFiltro();
        }

        private void ActualizarIndicadorFiltro()
        {
            if (_filtroActivo?.EstaActivo == true)
            {
                btn_Filtros.Text = $"Filtro: {_marcaActual?.ColumnaFiltro} ✔";
                btn_Filtros.ForeColor = System.Drawing.Color.DarkGreen;
            }
            else
            {
                btn_Filtros.Text = "Definir Filtros";
                btn_Filtros.ForeColor = System.Drawing.Color.Empty;
            }
        }

        /// <summary>Filtra _filasCargadas según el filtro activo y devuelve la lista resultante.</summary>
        private List<Dictionary<string, string>> AplicarFiltro(List<Dictionary<string, string>> filas)
        {
            if (_filtroActivo == null || !_filtroActivo.EstaActivo) return filas;

            var col = _filtroActivo.Columna;
            return _filtroActivo.Tipo switch
            {
                TipoFiltro.String => filas.Where(f =>
                    f.TryGetValue(col, out var v) &&
                    v.Contains(_filtroActivo.TextoContiene, StringComparison.OrdinalIgnoreCase)).ToList(),

                TipoFiltro.Int or TipoFiltro.Decimal => filas.Where(f =>
                {
                    if (!f.TryGetValue(col, out var v)) return false;
                    if (!decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var n))
                        decimal.TryParse(v.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out n);
                    if (_filtroActivo.NumDesde.HasValue && n < _filtroActivo.NumDesde.Value) return false;
                    if (_filtroActivo.NumHasta.HasValue && n > _filtroActivo.NumHasta.Value) return false;
                    return true;
                }).ToList(),

                TipoFiltro.Date => filas.Where(f =>
                {
                    if (!f.TryGetValue(col, out var v)) return false;
                    if (!DateTime.TryParse(v, out var fecha)) return false;
                    if (_filtroActivo.FechaDesde.HasValue && fecha.Date < _filtroActivo.FechaDesde.Value) return false;
                    if (_filtroActivo.FechaHasta.HasValue && fecha.Date > _filtroActivo.FechaHasta.Value) return false;
                    return true;
                }).ToList(),

                _ => filas
            };
        }

        private void cmbMarca_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {
            setSepDecMil();
            // Al cambiar la marca se invalida el filtro anterior
            _filtroActivo = null;
            ActualizarIndicadorFiltro();
        }
        private void setSepDecMil()
        {
            _marcaActual = _marcas.FirstOrDefault(m => m.Id == cmbMarca.SelectedValue?.ToString());
            if (_marcaActual == null) return;

            txtSepDec.Text = _marcaActual.SeparadorDecimales ?? "";
            txtSepMil.Text = _marcaActual.SeparadorMiles ?? "";

        }

        private void radEportar_Click(object sender, EventArgs e)
        {
            var nroBase = txtNroBase.Text.Trim();
            if (string.IsNullOrWhiteSpace(nroBase)) nroBase = "1000";

            using var dlg = new SaveFileDialog
            {
                Filter = "Archivos TXT (*.txt)|*.txt|Todos (*.*)|*.*",
                Title = "Guardar archivo de liquidación",
                FileName = $"Liquidacion_{_marcaActual?.Nombre.Trim()}_{nroBase}_{DateTime.Today:ddMMyyyy}.txt"
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(dlg.FileName, txtPreviewSalida.Text, System.Text.Encoding.UTF8);
                lblEstado.Text = $"✔ Archivo exportado: {dlg.FileName}";
                lblEstado.ForeColor = System.Drawing.Color.DarkGreen;
                MessageBox.Show("Archivo exportado correctamente.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
