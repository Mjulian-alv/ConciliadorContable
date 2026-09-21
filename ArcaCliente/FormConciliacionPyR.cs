// Fecha: 21/09/2026 - TAREA: 00041 - Linea: 3, 4 - Conciliación de percepciones y retenciones: carga de ARCA y de los mayores de PRESEA
// Esta etapa sólo levanta los dos lados y los muestra; CONCILIAR queda deshabilitado hasta que
// lleguen las directivas. Todo lo leído vive en memoria mientras la pantalla está abierta,
// igual que en la Conciliación Offline.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ArcaCliente.Models;
using ArcaCliente.Services;
using ExcelDataReader.Exceptions;
using Telerik.WinControls.UI;

namespace ArcaCliente
{
    public partial class FormConciliacionPyR : Telerik.WinControls.UI.RadForm
    {
        private static readonly CultureInfo Ar = CultureInfo.GetCultureInfo("es-AR");
        private static readonly Color Amarillo = Color.FromArgb(255, 243, 196);

        private PerfilOfflinePyR _perfil;
        private List<RegistroArcaPyR> _registrosArca = new();
        private readonly BindingList<MayorPreseaPyR> _mayores = new();
        private bool _cambiandoPerfil;

        public FormConciliacionPyR(PerfilOfflinePyR perfil)
        {
            ArcaStorageConfig.Initialize();
            InitializeComponent();
            Icon = AppIcons.Arca;

            ConfigurarColumnas();
            gridMayores.DataSource = _mayores;
            gridPresea.RowFormatting += GridPresea_RowFormatting;

            CargarPerfiles(perfil);
            AplicarPerfil(perfil);
            cmbPerfil.SelectedIndexChanged += CmbPerfil_SelectedIndexChanged;
        }

        // ── Perfil ───────────────────────────────────────────────────────────────

        private void CargarPerfiles(PerfilOfflinePyR actual)
        {
            cmbPerfil.Items.Clear();
            foreach (var p in AppServices.PerfilesPyR)
            {
                var item = new RadListDataItem(p.Nombre, p);
                cmbPerfil.Items.Add(item);
                if (p.Id == actual.Id) cmbPerfil.SelectedItem = item;
            }
        }

        private void AplicarPerfil(PerfilOfflinePyR perfil)
        {
            _perfil = perfil;
            txtCarpeta.Text = perfil.CarpetaArca ?? string.Empty;
            _registrosArca = new List<RegistroArcaPyR>();
            _mayores.Clear();
            MostrarArca(null);
            RefrescarPresea();
            MostrarEstadoPresea(null, Color.Empty);
        }

        // Cambiar de perfil cambia las cuentas: lo cargado deja de tener sentido y se descarta.
        private void CmbPerfil_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {
            if (_cambiandoPerfil || cmbPerfil.SelectedItem?.Value is not PerfilOfflinePyR nuevo || nuevo.Id == _perfil.Id)
                return;

            bool hayDatos = _registrosArca.Count > 0 || _mayores.Count > 0;
            if (hayDatos && MessageBox.Show("Al cambiar de perfil se descarta lo que está cargado. ¿Continuar?",
                    "Cambiar perfil", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                _cambiandoPerfil = true;
                foreach (RadListDataItem item in cmbPerfil.Items)
                    if (item.Value is PerfilOfflinePyR p && p.Id == _perfil.Id) cmbPerfil.SelectedItem = item;
                _cambiandoPerfil = false;
                return;
            }

            AplicarPerfil(nuevo);
        }

        private void GuardarPerfil()
        {
            var todos = AppServices.PerfilesPyR;
            var idx = todos.FindIndex(p => p.Id == _perfil.Id);
            if (idx >= 0) todos[idx] = _perfil;
            else todos.Add(_perfil);
            AppServices.SavePerfilesPyR(todos);
        }

        // ── ARCA (Línea 3) ───────────────────────────────────────────────────────

        private void BtnBrowseCarpeta_Click(object sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog
            {
                Description            = "Seleccionar la carpeta con los archivos de ARCA (percepciones y retenciones)",
                UseDescriptionForTitle = true,
                ShowNewFolderButton    = false
            };
            if (Directory.Exists(txtCarpeta.Text))
                dlg.InitialDirectory = txtCarpeta.Text;

            if (dlg.ShowDialog(this) == DialogResult.OK)
                txtCarpeta.Text = dlg.SelectedPath;
        }

        private void BtnCargarCarpeta_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCarpeta.Text))
            {
                MessageBox.Show("Seleccioná la carpeta con los archivos de ARCA.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            CargaArcaPyR carga;
            try
            {
                Cursor = Cursors.WaitCursor;
                carga = ArcaPyRImporter.ImportarDesdeCarpeta(txtCarpeta.Text.Trim());
            }
            catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException)
            {
                // Carpeta inexistente o sin ningún archivo reconocible: no se carga nada.
                _registrosArca = new List<RegistroArcaPyR>();
                MostrarArca(null);
                lblEstadoArca.Text      = "✗ " + PrimeraLinea(ex.Message);
                lblEstadoArca.ForeColor = Color.DarkRed;
                MessageBox.Show(ex.Message, "Carpeta de ARCA", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            _registrosArca = carga.Registros;
            MostrarArca(carga);

            _perfil.CarpetaArca = txtCarpeta.Text.Trim();
            GuardarPerfil();
        }

        private void MostrarArca(CargaArcaPyR carga)
        {
            gridArca.DataSource = null;
            gridArca.DataSource = _registrosArca;
            bool hay = _registrosArca.Count > 0;
            gridArca.Visible     = hay;
            lblVacioArca.Visible = !hay;
            pageArca.Text = hay ? $"Registros ARCA ({N(_registrosArca.Count)})" : "Registros ARCA";

            if (carga == null)
            {
                lblEstadoArca.Text        = "Sin cargar";
                lblEstadoArca.ForeColor   = Color.Gray;
                lblEstadoArca.LabelElement.ToolTipText = null;
                lblResumenArca.Text       = string.Empty;
                return;
            }

            var avisos = new List<string>();
            if (carga.ArchivosNoReconocidos.Count > 0)
                avisos.Add($"{carga.ArchivosNoReconocidos.Count} no reconocido(s): " +
                           string.Join(", ", carga.ArchivosNoReconocidos.Select(a => a.Archivo)));
            // Fecha: 21/09/2026 - TAREA: 00041 - Linea: 3 - Un repetido no es "no reconocido": se informa aparte
            if (carga.ArchivosRepetidos.Count > 0)
                avisos.Add($"{carga.ArchivosRepetidos.Count} repetido(s), se salteó: " +
                           string.Join(", ", carga.ArchivosRepetidos.Select(a => a.Archivo)));
            if (carga.FilasNoReconocidas > 0)
                avisos.Add($"{N(carga.FilasNoReconocidas)} fila(s) con impuesto u operación no reconocidos");

            if (avisos.Count == 0)
            {
                lblEstadoArca.Text      = $"✓ {carga.ArchivosLeidos.Count} archivo(s)  ·  {N(_registrosArca.Count)} registros";
                lblEstadoArca.ForeColor = Color.DarkGreen;
            }
            else
            {
                lblEstadoArca.Text      = $"⚠ {carga.ArchivosLeidos.Count} archivo(s) cargado(s)  ·  " + string.Join("  ·  ", avisos);
                lblEstadoArca.ForeColor = Color.DarkOrange;
            }
            // El motivo de cada archivo no reconocido no entra en la línea de estado: va al tooltip.
            var detalle = carga.ArchivosNoReconocidos.Select(a => $"{a.Archivo}: {a.Motivo}")
                .Concat(carga.ArchivosRepetidos.Select(a => $"{a.Archivo}: idéntico a {a.IgualA}"))
                .ToList();
            lblEstadoArca.LabelElement.ToolTipText = detalle.Count == 0 ? null : string.Join("\n", detalle);

            // Texto plano: el formato HTML de Telerik se come el espacio antes de <b>.
            lblResumenArca.Text = string.Join("      ", _registrosArca
                .GroupBy(r => (r.Impuesto, r.Operacion))
                .OrderBy(g => g.Key.Impuesto).ThenBy(g => g.Key.Operacion)
                .Select(g => $"{CuentaPyR.NombreImpuesto(g.Key.Impuesto)} · {CuentaPyR.NombreTipo(g.Key.Operacion)}: {N(g.Count())}"));
        }

        // ── PRESEA (Línea 4) ─────────────────────────────────────────────────────

        private void BtnAgregarMayor_Click(object sender, EventArgs e)
        {
            if (_perfil.Cuentas.Count == 0) return;   // el botón ya está deshabilitado; defensa extra

            using var dlgArchivo = new OpenFileDialog
            {
                Title  = "Seleccionar el mayor de PRESEA",
                Filter = "Archivos Excel (*.xlsx;*.xls)|*.xlsx;*.xls"
            };
            if (dlgArchivo.ShowDialog(this) != DialogResult.OK) return;

            string archivo = Path.GetFileName(dlgArchivo.FileName);
            // Fecha: 21/09/2026 - TAREA: 00041 - Linea: 5 - Pasar los mayores ya cargados para que el diálogo arranque en una cuenta libre
            using var dlgCuenta = new FormElegirCuentaPyR(archivo, _perfil.Cuentas,
                _mayores.ToDictionary(m => m.Cuenta.Id, m => m.Archivo));
            if (dlgCuenta.ShowDialog(this) != DialogResult.OK || dlgCuenta.CuentaElegida == null) return;
            var cuenta = dlgCuenta.CuentaElegida;

            var existente = _mayores.FirstOrDefault(m => m.Cuenta.Id == cuenta.Id);
            if (existente != null && MessageBox.Show(
                    $"La cuenta {cuenta} ya tiene cargado el mayor \"{existente.Archivo}\".\n\n¿Reemplazarlo por \"{archivo}\"?",
                    "Reemplazar mayor", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            MayorPreseaPyR mayor;
            try
            {
                Cursor = Cursors.WaitCursor;
                mayor = PreseaPyRImporter.Importar(dlgArchivo.FileName, _perfil, cuenta);
            }
            catch (ColumnasFaltantesException ex)
            {
                MostrarEstadoPresea($"✗ {archivo} no se agregó: faltan las columnas {string.Join(", ", ex.Faltantes)}", Color.DarkRed);
                return;
            }
            catch (Exception ex) when (ex is IOException or InvalidDataException or ExcelReaderException or UnauthorizedAccessException)
            {
                MostrarEstadoPresea($"✗ {archivo} no se agregó: {PrimeraLinea(ex.Message)}", Color.DarkRed);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            if (existente != null) _mayores[_mayores.IndexOf(existente)] = mayor;
            else _mayores.Add(mayor);

            RefrescarPresea();
            MostrarEstadoPresea(null, Color.Empty);
        }

        private void BtnQuitarMayor_Click(object sender, EventArgs e)
        {
            if (gridMayores.CurrentRow?.DataBoundItem is not MayorPreseaPyR mayor) return;
            _mayores.Remove(mayor);
            RefrescarPresea();
            MostrarEstadoPresea(null, Color.Empty);
        }

        private void RefrescarPresea()
        {
            var filas = _mayores.SelectMany(m => m.Registros).ToList();
            var sinComprobante = filas.Where(r => r.SinComprobante).ToList();

            gridPresea.DataSource = null;
            gridPresea.DataSource = filas;
            gridSinComprobante.DataSource = null;
            gridSinComprobante.DataSource = sinComprobante;

            bool hayMayores = _mayores.Count > 0;
            gridMayores.Visible    = hayMayores;
            lblSinMayores.Visible  = !hayMayores;
            btnQuitarMayor.Enabled = hayMayores;
            gridPresea.Visible     = filas.Count > 0;
            lblVacioPresea.Visible = filas.Count == 0;

            pagePresea.Text         = filas.Count > 0 ? $"Mayor PRESEA ({N(filas.Count)})" : "Mayor PRESEA";
            pageSinComprobante.Text = $"Sin comprobante ({N(sinComprobante.Count)})";

            bool hayCuentas = _perfil.Cuentas.Count > 0;
            btnAgregarMayor.Enabled = hayCuentas;
        }

        /// <summary>
        /// Con <paramref name="texto"/> null muestra el estado general (conteos o "Sin cargar");
        /// con texto, un mensaje puntual (por ejemplo, el rechazo de un mayor).
        /// </summary>
        private void MostrarEstadoPresea(string texto, Color color)
        {
            if (texto != null)
            {
                lblEstadoPresea.Text      = texto;
                lblEstadoPresea.ForeColor = color;
                return;
            }

            if (_perfil.Cuentas.Count == 0)
            {
                lblEstadoPresea.Text      = "⚠ El perfil no tiene cuentas configuradas";
                lblEstadoPresea.ForeColor = Color.DarkOrange;
            }
            else if (_mayores.Count == 0)
            {
                lblEstadoPresea.Text      = "Sin cargar";
                lblEstadoPresea.ForeColor = Color.Gray;
            }
            else
            {
                int filas = _mayores.Sum(m => m.Filas), sin = _mayores.Sum(m => m.FilasSinComprobante);
                lblEstadoPresea.Text      = $"✓ {_mayores.Count} mayor(es)  ·  {N(filas)} filas  ·  {N(sin)} sin comprobante";
                lblEstadoPresea.ForeColor = Color.DarkGreen;
            }
        }

        // Las filas sin comprobante se resaltan en amarillo, como en la maqueta.
        private void GridPresea_RowFormatting(object sender, RowFormattingEventArgs e)
        {
            if (e.RowElement.RowInfo.DataBoundItem is RegistroPreseaPyR { SinComprobante: true })
            {
                e.RowElement.DrawFill      = true;
                e.RowElement.GradientStyle = Telerik.WinControls.GradientStyles.Solid;
                e.RowElement.BackColor     = Amarillo;
            }
            else
            {
                e.RowElement.ResetValue(LightVisualElement.DrawFillProperty, Telerik.WinControls.ValueResetFlags.Local);
                e.RowElement.ResetValue(LightVisualElement.GradientStyleProperty, Telerik.WinControls.ValueResetFlags.Local);
                e.RowElement.ResetValue(Telerik.WinControls.VisualElement.BackColorProperty, Telerik.WinControls.ValueResetFlags.Local);
            }
        }

        // ── Columnas ─────────────────────────────────────────────────────────────

        private void ConfigurarColumnas()
        {
            gridArca.Columns.AddRange(
                Texto(nameof(RegistroArcaPyR.ImpuestoTexto),     "Impuesto",     70),
                Texto(nameof(RegistroArcaPyR.OperacionTexto),    "Operación",    85),
                Fecha(nameof(RegistroArcaPyR.Fecha)),
                Texto(nameof(RegistroArcaPyR.Cuit),              "CUIT",         95),
                Texto(nameof(RegistroArcaPyR.Denominacion),      "Denominación", 230),
                Texto(nameof(RegistroArcaPyR.TipoComprobante),   "Tipo",         140),
                Texto(nameof(RegistroArcaPyR.Letra),             "Letra",        45),
                Texto(nameof(RegistroArcaPyR.Numero),            "Número",       120),
                Importe(nameof(RegistroArcaPyR.Importe)),
                Texto(nameof(RegistroArcaPyR.ArchivoOrigen),     "Archivo",      220));

            foreach (var grid in new[] { gridPresea, gridSinComprobante })
                grid.Columns.AddRange(
                    Texto(nameof(RegistroPreseaPyR.CuentaNombre),     "Cuenta",            150),
                    Fecha(nameof(RegistroPreseaPyR.Fecha)),
                    Texto(nameof(RegistroPreseaPyR.Asiento),          "Asiento",           85),
                    Texto(nameof(RegistroPreseaPyR.TipoComprobante),  "Tipo",              130),
                    Texto(nameof(RegistroPreseaPyR.Numero),           "Número",            110),
                    Texto(nameof(RegistroPreseaPyR.Proveedor),        "Proveedor",         150),
                    Texto(nameof(RegistroPreseaPyR.AnulacionTexto),   "Anul.",             45),
                    Importe(nameof(RegistroPreseaPyR.Importe)),
                    Texto(nameof(RegistroPreseaPyR.ConceptoOriginal), "Concepto original", 330));

            gridMayores.AutoGenerateColumns = false;
            gridMayores.Columns.AddRange(
                Texto(nameof(MayorPreseaPyR.Archivo),     "Archivo", 170),
                Texto(nameof(MayorPreseaPyR.CuentaTexto), "Cuenta",  220),
                new GridViewDecimalColumn(nameof(MayorPreseaPyR.Filas))               { HeaderText = "Filas",           Width = 60,  FormatString = "{0:N0}", DecimalPlaces = 0 },
                new GridViewDecimalColumn(nameof(MayorPreseaPyR.FilasSinComprobante)) { HeaderText = "Sin comprobante", Width = 100, FormatString = "{0:N0}", DecimalPlaces = 0 });
        }

        private static GridViewTextBoxColumn Texto(string campo, string titulo, int ancho) =>
            new(campo) { HeaderText = titulo, Width = ancho };

        private static GridViewDateTimeColumn Fecha(string campo) =>
            new(campo) { HeaderText = "Fecha", Width = 80, FormatString = "{0:dd/MM/yyyy}" };

        private static GridViewDecimalColumn Importe(string campo) =>
            new(campo) { HeaderText = "Importe", Width = 100, FormatString = "{0:N2}", FormatInfo = Ar, TextAlignment = ContentAlignment.MiddleRight };

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static string N(int n) => n.ToString("N0", Ar);

        private static string PrimeraLinea(string texto) =>
            (texto ?? string.Empty).Split('\n')[0].Trim();
    }
}
