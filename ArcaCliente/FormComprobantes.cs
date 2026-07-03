using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ArcaCliente.Models;
using ArcaCliente.Services;
using Telerik.WinControls.UI;

namespace ArcaCliente
{
    public partial class FormComprobantes : Telerik.WinControls.UI.RadForm
    {
        private CancellationTokenSource            _cts;
        private List<ItemConciliacion>             _itemsSoloArca = new();
        private List<ComprobanteCsv>               _comprobantesArca;
        private ArcaCliente.Models.PerfilFiscal    _perfilActivo;

        public FormComprobantes()
        {
            InitializeComponent();
            Icon = AppIcons.Arca;

            txtClaveAfip.TextBoxElement.PasswordChar = '*';

            dtpFechaInicio.Format       = DateTimePickerFormat.Custom;
            dtpFechaInicio.CustomFormat = "dd/MM/yyyy";
            dtpFechaInicio.Value        = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            dtpFechaFin.Format          = DateTimePickerFormat.Custom;
            dtpFechaFin.CustomFormat    = "dd/MM/yyyy";
            dtpFechaFin.Value           = DateTime.Now;

            gridComprobantes.MasterTemplate.AllowAddNewRow  = false;
            gridComprobantes.MasterTemplate.AllowDeleteRow  = false;
            gridComprobantes.MasterTemplate.AllowEditRow    = false;

            gridConciliacion.MasterTemplate.AllowAddNewRow  = false;
            gridConciliacion.MasterTemplate.AllowDeleteRow  = false;
            gridConciliacion.MasterTemplate.AllowEditRow    = false;
            gridConciliacion.RowFormatting += GridConciliacion_RowFormatting;

            CargarComboPerfiles();
            CargarUsuario();
        }

        private void CargarUsuario()
        {
            var token = AppServices.CurrentToken;
            if (token != null)
                lblUsuario.Text = token.Email;
        }

        private void CargarComboPerfiles()
        {
            var perfiles = AppServices.Perfiles;
            cmbPerfiles.DataSource     = null;
            cmbPerfiles.DataSource     = perfiles;
            cmbPerfiles.DisplayMember  = "Nombre";
            cmbPerfiles.NullText       = "-- Seleccionar perfil --";
        }

        private async void CmbPerfiles_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {
            if (cmbPerfiles.SelectedItem?.DataBoundItem is not ArcaCliente.Models.PerfilFiscal perfil)
                return;

            _perfilActivo            = perfil;
            txtCuil.Text             = perfil.Username;
            txtClaveAfip.Text        = perfil.Password;
            txtCuitRepresentada.Text = perfil.Cuit;

            await InicializarIntegracionAsync(perfil);
        }

        /// <summary>
        /// Inicializa <see cref="OctosisConfig"/> si el perfil tiene la integración habilitada.
        /// Muestra el estado en <c>lblIntegracion</c> y controla la visibilidad del botón Solo ARCA.
        /// Si la conexión falla, ofrece corregir la configuración del perfil y reintentar.
        /// </summary>
        private async Task InicializarIntegracionAsync(ArcaCliente.Models.PerfilFiscal perfil)
        {
            OctosisConfig.Reset();
            lblIntegracion.Text         = string.Empty;
            btnProcesarSoloArca.Visible = false;

            if (!perfil.IntegracionHabilitada || perfil.Sistema != ArcaCliente.Models.SistemaIntegracion.Octosis)
                return;

            while (true)
            {
                lblIntegracion.Text      = "Conectando a Octosis...";
                lblIntegracion.ForeColor = Color.DarkBlue;

                try
                {
                    await OctosisConfig.InicializarAsync(perfil);

                    lblIntegracion.Text      = $" Octosis · Suc. {OctosisConfig.Sucursal}";
                    lblIntegracion.ForeColor = Color.DarkGreen;
                    return;
                }
                catch (Exception ex)
                {
                    lblIntegracion.Text      = "? Octosis: error de conexión";
                    lblIntegracion.ForeColor = Color.DarkRed;

                    var respuesta = MessageBox.Show(
                        $"No se pudo conectar a la base de datos de Octosis para el perfil \"{perfil.Nombre}\".\n\n" +
                        $"Error: {ex.Message}\n\n" +
                        $"¿Desea corregir la configuración del perfil y reintentar?",
                        "Error de conexión — Octosis",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button1);

                    if (respuesta != DialogResult.Yes)
                        return;

                    using var formDetalle = new FormPerfilDetalle(perfil);
                    if (formDetalle.ShowDialog(this) != DialogResult.OK)
                        return;

                    // Persistir los cambios del perfil en disco
                    var todosPerfiles = AppServices.Perfiles;
                    var idx = todosPerfiles.FindIndex(p => p.Id == perfil.Id);
                    if (idx >= 0)
                        todosPerfiles[idx] = perfil;
                    AppServices.SavePerfiles(todosPerfiles);
                    CargarComboPerfiles();
                }
            }
        }

        private void BtnGestionarPerfiles_Click(object sender, EventArgs e)
        {
            using var form = new FormPerfiles();
            form.ShowDialog(this);
            CargarComboPerfiles();
        }

        private void BtnOffline_Click(object sender, EventArgs e)
        {
            using var selector = new FormPerfilesOffline();
            if (selector.ShowDialog(this) != DialogResult.OK || selector.PerfilSeleccionado == null)
                return;
            var form = new FormComprobantesOffline(selector.PerfilSeleccionado);
            form.Show();
        }

        private async void btnExportar_Click(object sender, EventArgs e)
        {
            if (!ValidarFiltros()) return;

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            SetLoading(true, "Consultando portal ARCA... (puede demorar hasta 90 segundos)");
            try
            {
                var request = new ExportarRequest
                {
                    Username    = txtCuil.Text.Trim(),
                    Password    = txtClaveAfip.Text,
                    Cuit        = txtCuitRepresentada.Text.Trim(),
                    FechaInicio = dtpFechaInicio.Value.ToString("dd/MM/yyyy"),
                    FechaFin    = dtpFechaFin.Value.ToString("dd/MM/yyyy")
                };

                var perfilActivo = cmbPerfiles.SelectedItem?.DataBoundItem as ArcaCliente.Models.PerfilFiscal;
                var api = AppServices.GetApiForPerfil(perfilActivo);
                var result = await api.ExportarComprobantesAsync(request, _cts.Token);

                _comprobantesArca = result.Comprobantes;
                CargarGrid(result.Comprobantes);
                MostrarEstado($" {result.TotalRegistros} comprobantes encontrados.", Color.DarkGreen);

                await EjecutarConciliacionAsync(result.Comprobantes, perfilActivo);
            }
            catch (OperationCanceledException)
            {
                MostrarEstado("Operación cancelada.", Color.DarkOrange);
            }
            catch (ArcaApiException ex)
            {
                MostrarEstado($"? {ex.UserFriendlyMessage}", Color.DarkRed);

                var msg = ex.UserFriendlyMessage;
                if (ex.Screenshots?.Count > 0)
                    msg += $"\n\nScreenshots del servidor:\n• {string.Join("\n• ", ex.Screenshots)}";

                if (ex.StatusCode == 401)
                {
                    MessageBox.Show(msg + "\n\nSe cerrará la sesión.", "Sesión expirada",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    CerrarSesion();
                    return;
                }

                MessageBox.Show(msg, "Error al exportar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MostrarEstado("? Error inesperado.", Color.DarkRed);
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetLoading(false, string.Empty);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e) => _cts?.Cancel();

        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Cerrar sesión?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                CerrarSesion();
        }

        private void CerrarSesion()
        {
            AppServices.Logout();
            Close();
        }

        private void CargarGrid(List<ComprobanteCsv> comprobantes)
        {
            foreach (var c in comprobantes)
                c.DescripcionTipoComprobante = TablaComprobanteDescripcionMapper.ObtenerDescripcion(c.TipoComprobante);
            gridComprobantes.DataSource = null;
            gridComprobantes.DataSource = comprobantes;
            ConfigurarColumnas();
        }

        private void ConfigurarColumnas()
        {
            var columnas = new Dictionary<string, string>
            {
                { "FechaEmision",               "Fecha"        },
                { "TipoComprobante",            "Tipo"         },
                { "DescripcionTipoComprobante", "Descripción"  },
                { "PuntoVenta",                 "Pto. Vta."    },
                { "NumeroDesde",                "Número"       },
                { "CodAutorizacion",            "CAE / CAI"    },
                { "NroDocEmisor",               "Doc. Emisor"  },
                { "DenominacionEmisor",         "Emisor"       },
                { "Moneda",                     "Moneda"       },
                { "ImpNetoGravadoIVA105",       "Neto 10,5%"   },
                { "ImpNetoGravadoIVA21",        "Neto 21%"     },
                { "TotalIVA",             "IVA"          },
                { "ImpTotal",             "Total"        },
            };

            var visibles = new HashSet<string>(columnas.Keys);

            foreach (var col in gridComprobantes.Columns.OfType<GridViewDataColumn>())
            {
                if (columnas.TryGetValue(col.FieldName, out var header))
                    col.HeaderText = header;

                col.IsVisible = visibles.Contains(col.FieldName);
            }
            gridComprobantes.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;
        }

        private bool ValidarFiltros()
        {
            if (string.IsNullOrWhiteSpace(txtCuil.Text))
            {
                MessageBox.Show("Ingresá el CUIL/CUIT del contribuyente (sin guiones).", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtCuil.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtClaveAfip.Text))
            {
                MessageBox.Show("Ingresá la clave fiscal ARCA.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtClaveAfip.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtCuitRepresentada.Text))
            {
                MessageBox.Show("Ingresá el CUIT de la empresa representada (con guiones, ej: 20-12345678-9).",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtCuitRepresentada.Focus();
                return false;
            }
            if (dtpFechaFin.Value.Date < dtpFechaInicio.Value.Date)
            {
                MessageBox.Show("La fecha de fin no puede ser anterior a la fecha de inicio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }

        private void SetLoading(bool loading, string mensaje)
        {
            btnExportar.Enabled  = !loading;
            btnCancelar.Visible  = loading;
            Cursor = loading ? Cursors.WaitCursor : Cursors.Default;
            if (loading)
                MostrarEstado(mensaje, Color.DarkBlue);
        }

        private void MostrarEstado(string texto, Color color)
        {
            lblEstado.Text      = texto;
            lblEstado.ForeColor = color;
        }

        private async Task EjecutarConciliacionAsync(List<ComprobanteCsv> comprobantesArca, ArcaCliente.Models.PerfilFiscal perfil = null)
        {
            var (_, effectiveFactory, effectiveQuery) = ConciliacionConfig.GetEffective(perfil);

            if (effectiveFactory == null)
            {
                MostrarEstadoConciliacion(
                    "Configure ConciliacionConfig.ConnectionFactory para habilitar la conciliación.",
                    Color.Gray);
                return;
            }

            MostrarEstadoConciliacion("Consultando base de datos local...", Color.DarkBlue);
            try
            {
                var service = new ConciliacionService(
                    effectiveFactory,
                    effectiveQuery,
                    usarEquivalenciaOnline: true);

                var locales = await service.ObtenerComprobantesLocalesAsync(
                    dtpFechaInicio.Value.Date,
                    dtpFechaFin.Value.Date);

                var items = service.Conciliar(comprobantesArca, locales, perfil?.DirectivasConciliacion);

                // Enriquecer items SoloARCA con el tipo/letra Octosis parseado
                EnriquecerSoloArca(items);

                // Guardar los Solo ARCA para el botón de procesamiento (PASO 5)
                _itemsSoloArca                   = items.Where(x => x.Estado == EstadoConciliacion.SoloARCA).ToList();
                btnProcesarSoloArca.Visible      = _itemsSoloArca.Count > 0 && OctosisConfig.Inicializado;
                btnProcesarSoloArca.Text         = _itemsSoloArca.Count > 0
                    ? $"Procesar Solo ARCA ({_itemsSoloArca.Count})..."
                    : "Procesar Solo ARCA...";

                gridConciliacion.DataSource = null;
                gridConciliacion.DataSource = items;
                ConfigurarGridConciliacion();

                int conciliados  = items.Count(x => x.Estado == EstadoConciliacion.Conciliado);
                int diferencias  = items.Count(x => x.Estado == EstadoConciliacion.DiferenciaImporte);
                int soloArca     = items.Count(x => x.Estado == EstadoConciliacion.SoloARCA);
                int soloSistema  = items.Count(x => x.Estado == EstadoConciliacion.SoloSistema);
                bool hayIncidencias = diferencias > 0 || soloArca > 0 || soloSistema > 0;

                btnExportarConciliacion.Visible = items.Count > 0;

                MostrarEstadoConciliacion(
                    $"? {conciliados} ok  ·  ? {diferencias} dif.  ·  ? {soloArca} solo ARCA  ·  ? {soloSistema} solo sist.",
                    hayIncidencias ? Color.DarkOrange : Color.DarkGreen);
            }
            catch (Exception ex)
            {
                MostrarEstadoConciliacion($"Error al consultar BD local: {ex.Message}", Color.DarkRed);
                MessageBox.Show($"Error en conciliación:\n\n{ex.Message}",
                    "Error de base de datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ConfigurarGridConciliacion()
        {
            var columnas = new Dictionary<string, string>
            {
                { "EstadoTexto",                "Estado"           },
                { "DescripcionDirectiva",       "Directiva"        },
                { "DetalleMatcheo",             "Clave matcheo"    },
                { "FalloDirectivas",            "Dif. con sist."  },
                { "Fecha",                      "Fecha"            },
                { "TipoComprobante",            "Tipo"             },
                { "DescripcionTipoComprobante", "Descripción"      },
                { "PuntoVenta",                 "Pto. Vta."        },
                { "Numero",                     "Número"           },
                { "CuitProveedor",              "CUIT"             },
                { "NombreProveedor",            "Nombre proveedor" },
                { "TotalARCA",                  "Total ARCA"       },
                { "TotalSistema",               "Total Sist."      },
                { "Diferencia",                 "Diferencia"       },
            };

            var visibles = new HashSet<string>(columnas.Keys);
            foreach (var col in gridConciliacion.Columns.OfType<GridViewDataColumn>())
            {
                if (columnas.TryGetValue(col.FieldName, out var header))
                    col.HeaderText = header;
                col.IsVisible = visibles.Contains(col.FieldName);
            }
            gridConciliacion.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;
        }

        private void GridConciliacion_RowFormatting(object sender, RowFormattingEventArgs e)
        {
            if (e.RowElement.RowInfo is not GridViewDataRowInfo dataRow ||
                dataRow.DataBoundItem is not ItemConciliacion item)
            {
                e.RowElement.DrawFill = false;
                return;
            }

            e.RowElement.DrawFill      = true;
            e.RowElement.GradientStyle = Telerik.WinControls.GradientStyles.Solid;
            e.RowElement.BackColor     = item.Estado switch
            {
                EstadoConciliacion.Conciliado        => Color.FromArgb(170, 240, 209),
                EstadoConciliacion.DiferenciaImporte => Color.FromArgb(254, 240, 186),
                EstadoConciliacion.SoloARCA          => Color.FromArgb(255, 205, 210),
                EstadoConciliacion.SoloSistema       => Color.FromArgb(206, 212, 237),
                _                                    => Color.White
            };
        }

        private static void EnriquecerSoloArca(System.Collections.Generic.List<ItemConciliacion> items)
        {
            var soloArca = items.Where(x => x.Estado == EstadoConciliacion.SoloARCA).ToList();
            foreach (var item in soloArca)
            {
                // PASO 2 — tipo de comprobante
                var parseado     = TipoComprobanteMapper.Parse(item.TipoComprobante.PadLeft(3,Char.Parse("0")));
                item.TipoOctosis = parseado.TipoOctosis;
                item.Letra       = parseado.Letra;

                // PASO 3 — moneda y tipo de cambio
                if (item.SourceArca != null)
                    item.Moneda = MonedaResolver.Resolver(item.SourceArca);
            }

            var desconocidos = TipoComprobanteMapper
                .ObtenerDesconocidos(soloArca.Select(x => x.TipoComprobante.PadLeft(3, Char.Parse("0"))))
                .ToList();

            if (desconocidos.Count > 0)
            {
                MessageBox.Show(
                    "Se encontraron tipos de comprobante no reconocidos por el mapper:\n\n" +
                    string.Join("\n", desconocidos.Select(d => $"  · \"{d}\"")) +
                    "\n\nEstos documentos no podrán darse de alta automáticamente.\n" +
                    "Informar al equipo de desarrollo para actualizar el diccionario.",
                    "Tipos de comprobante desconocidos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void MostrarEstadoConciliacion(string texto, Color color)
        {
            lblEstadoConciliacion.Text      = texto;
            lblEstadoConciliacion.ForeColor = color;
        }

        private void BtnExportarConciliacion_Click(object sender, EventArgs e)
        {
            if (gridConciliacion.DataSource is not List<ItemConciliacion> items || items.Count == 0)
            {
                MessageBox.Show("No hay datos de conciliaci\u00f3n para exportar.", "Sin datos",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dlg = new SaveFileDialog
            {
                Title           = "Guardar conciliaci\u00f3n como...",
                Filter          = "Archivos Excel (*.xlsx)|*.xlsx",
                FileName        = $"Conciliacion_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
                DefaultExt      = "xlsx",
                OverwritePrompt = true
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                ConciliacionExcelExporter.Exportar(items, dlg.FileName);
                MostrarEstadoConciliacion(
                    $"Exportado: {System.IO.Path.GetFileName(dlg.FileName)}", Color.DarkGreen);

                if (MessageBox.Show("\u00bfAbrir el archivo?", "Exportado",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    System.Diagnostics.Process.Start(
                        new System.Diagnostics.ProcessStartInfo(dlg.FileName)
                        { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar:\n\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnProcesarSoloArca_Click(object sender, EventArgs e)
        {
            if (_itemsSoloArca == null || _itemsSoloArca.Count == 0)
                return;

            using var form = new FormProcesarSoloArca(_itemsSoloArca, _perfilActivo);
            form.ShowDialog(this);

            // Si se dieron de alta documentos, re-ejecutar la conciliación para reflejar los cambios
            if (form.AltasRealizadas > 0 && _comprobantesArca != null)
                await EjecutarConciliacionAsync(_comprobantesArca, _perfilActivo);
        }
    }
}
