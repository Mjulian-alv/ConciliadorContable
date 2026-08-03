using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using ArcaCliente.Models;
using ArcaCliente.Services;
using Telerik.WinControls.UI;

namespace ArcaCliente
{
    /// <summary>
    /// Formulario de conciliación offline: los comprobantes ARCA se leen desde
    /// una carpeta de CSVs locales y los comprobantes del sistema se leen desde
    /// un archivo configurado en el <see cref="PerfilOffline"/>.
    /// </summary>
    public partial class FormComprobantesOffline : Telerik.WinControls.UI.RadForm
    {
        private readonly PerfilOffline _perfil;
        private List<ComprobanteCsv> _comprobantesArca = new();
        private List<ComprobanteLocal> _comprobantesLocales = new();
        private Telerik.WinControls.UI.RadButton btnConciliarXCuit;


        public FormComprobantesOffline(PerfilOffline perfil)
        {
            ArcaStorageConfig.Initialize();
            InitializeComponent();
            Icon = AppIcons.Arca;

            _perfil = perfil;

            dtpFechaInicio.Format = DateTimePickerFormat.Custom;
            dtpFechaInicio.CustomFormat = "dd/MM/yyyy";
            dtpFechaInicio.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            dtpFechaFin.Format = DateTimePickerFormat.Custom;
            dtpFechaFin.CustomFormat = "dd/MM/yyyy";
            dtpFechaFin.Value = DateTime.Now;

            gridComprobantes.MasterTemplate.AllowAddNewRow = false;
            gridComprobantes.MasterTemplate.AllowDeleteRow = false;
            gridComprobantes.MasterTemplate.AllowEditRow = false;

            gridConciliacion.MasterTemplate.AllowAddNewRow = false;
            gridConciliacion.MasterTemplate.AllowDeleteRow = false;
            gridConciliacion.MasterTemplate.AllowEditRow = false;
            gridConciliacion.RowFormatting   += GridConciliacion_RowFormatting;
            gridConciliacion.CellDoubleClick += GridConciliacion_CellDoubleClick;
            gridComprobantes.RowFormatting   += GridComprobantes_RowFormatting;
            gridComprobantes.CellDoubleClick += GridComprobantes_CellDoubleClick;

            lblPerfilActivo.Text = $"{perfil.Nombre}  •  {perfil.TipoArchivo}";
            grpLocal.Text = $"Sistema local ({perfil.TipoArchivo})";

            if (!string.IsNullOrWhiteSpace(perfil.CarpetaCsvArca))
                txtCarpeta.Text = perfil.CarpetaCsvArca;

            InicializarBotonConciliarXCuit();
            InicializarBotonExportarSistema();
            InicializarBotonExportarPreseaQr();
        }

        // ?? Fuente ARCA (CSV) ?????????????????????????????????????????????????????

        private void BtnBrowseCarpeta_Click(object sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog
            {
                Description = "Seleccionar la carpeta con los archivos CSV de ARCA",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = false
            };
            if (!string.IsNullOrWhiteSpace(txtCarpeta.Text))
                dlg.InitialDirectory = txtCarpeta.Text;

            if (dlg.ShowDialog(this) == DialogResult.OK)
                txtCarpeta.Text = dlg.SelectedPath;
        }

        private void BtnCargarCsv_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCarpeta.Text))
            {
                MessageBox.Show("Seleccioná una carpeta con archivos CSV.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                _comprobantesArca = OfflineCsvImporter.ImportarDesdeCarpeta(txtCarpeta.Text);

                var archivos = System.IO.Directory.GetFiles(
                    txtCarpeta.Text, "*.csv",
                    System.IO.SearchOption.TopDirectoryOnly).Length;

                var colorCsv = _comprobantesArca.Count > 0 ? Color.DarkGreen : Color.DarkOrange;
                lblConteoCsv.Text = $"{IconoEstado(colorCsv)} {archivos} archivo(s)  •  {_comprobantesArca.Count} registros";
                lblConteoCsv.ForeColor = colorCsv;

                CargarGridArca(_comprobantesArca);
                MostrarEstado($"CSV cargados: {_comprobantesArca.Count} comprobantes.", Color.DarkGreen);
                ActualizarBotonExportarSistema();

                _perfil.CarpetaCsvArca = txtCarpeta.Text;
                GuardarPerfil();
            }
            catch (Exception ex)
            {
                lblConteoCsv.Text = $"{IconoEstado(Color.DarkRed)} Error al cargar";
                lblConteoCsv.ForeColor = Color.DarkRed;
                MostrarEstado("Error al cargar CSV.", Color.DarkRed);
                MessageBox.Show($"Error al leer los archivos CSV:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ── Fuente local ─────────────────────────────────────────────────────────

        private void BtnBrowseArchivo_Click(object sender, EventArgs e)
        {
            var filter = _perfil.TipoArchivo switch
            {
                TipoArchivoOffline.Xlsx => "Archivos Excel (*.xlsx)|*.xlsx",
                TipoArchivoOffline.Csv => "Archivos CSV (*.csv)|*.csv",
                TipoArchivoOffline.Txt => "Archivos de texto (*.txt)|*.txt",
                _ => "Todos los archivos (*.*)|*.*"
            };

            using var dlg = new OpenFileDialog
            {
                Title = "Seleccionar archivo del sistema local",
                Filter = filter + "|Todos los archivos (*.*)|*.*",
                FilterIndex = 1
            };
            if (!string.IsNullOrWhiteSpace(txtArchivo.Text))
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(txtArchivo.Text);

            if (dlg.ShowDialog(this) == DialogResult.OK)
                txtArchivo.Text = dlg.FileName;
        }

        private void BtnCargarLocal_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtArchivo.Text))
            {
                MessageBox.Show("Seleccioná el archivo del sistema local.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                _comprobantesLocales = LocalFileImporter.Importar(txtArchivo.Text, _perfil);

                dtpFechaInicio.Value = _comprobantesLocales.Min(c => c.Fecha);
                dtpFechaFin.Value = _comprobantesLocales.Max(c => c.Fecha);

                var colorLocal = _comprobantesLocales.Count > 0 ? Color.DarkGreen : Color.DarkOrange;
                lblConteoLocal.Text = $"{IconoEstado(colorLocal)} {_comprobantesLocales.Count} registros cargados";
                lblConteoLocal.ForeColor = colorLocal;

                MostrarEstado($"Archivo local cargado: {_comprobantesLocales.Count} registros.", Color.DarkGreen);
            }
            catch (Exception ex)
            {
                lblConteoLocal.Text = $"{IconoEstado(Color.DarkRed)} Error al cargar";
                lblConteoLocal.ForeColor = Color.DarkRed;
                MostrarEstado("Error al cargar el archivo local.", Color.DarkRed);
                MessageBox.Show($"Error al leer el archivo:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ── Conciliación ─────────────────────────────────────────────────────────

        private void BtnConciliar_Click(object sender, EventArgs e)
        {
            if (_comprobantesArca.Count == 0 && _comprobantesLocales.Count == 0)
            {
                MessageBox.Show(
                    "Cargá al menos los CSV de ARCA o el archivo del sistema local.",
                    "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (dtpFechaFin.Value.Date < dtpFechaInicio.Value.Date)
            {
                MessageBox.Show("La fecha de fin no puede ser anterior a la de inicio.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            EjecutarConciliacion();
        }

        private void EjecutarConciliacion() => EjecutarConciliacion(null, null);

        private void EjecutarConciliacion(
            List<ComprobanteCsv> arcaFiltrados,
            List<ComprobanteLocal> localFiltrados,
            string tituloCuit = null)
        {
            var desde = dtpFechaInicio.Value.Date;
            var hasta = dtpFechaFin.Value.Date;

            arcaFiltrados  ??= FiltrarArcaPorFecha(_comprobantesArca, desde, hasta);
            localFiltrados ??= _comprobantesLocales
                .Where(c => c.Fecha.Date >= desde && c.Fecha.Date <= hasta)
                .ToList();

            try
            {
                var service = new ConciliacionService(
                    () => throw new InvalidOperationException("Modo offline"),
                    string.Empty);

                var directivas = _perfil.DirectivasConciliacion?.Count > 0
                    ? _perfil.DirectivasConciliacion
                    : (IReadOnlyList<ArcaCliente.Models.DirectivaConciliacion>)
                      new[] { ArcaCliente.Models.DirectivaConciliacion.CrearPrimaria() };

                var items = service.Conciliar(arcaFiltrados, localFiltrados, directivas);

                gridConciliacion.DataSource = null;
                gridConciliacion.DataSource = items;
                ConfigurarGridConciliacion();

                lblConciliacion.Text = tituloCuit != null
                    ? $"Conciliación — CUIT {tituloCuit}"
                    : "Conciliación";

                int conciliados = items.Count(x => x.Estado == EstadoConciliacion.Conciliado);
                int diferencias = items.Count(x => x.Estado == EstadoConciliacion.DiferenciaImporte);
                int soloArca = items.Count(x => x.Estado == EstadoConciliacion.SoloARCA);
                int soloSistema = items.Count(x => x.Estado == EstadoConciliacion.SoloSistema);
                bool hayInc = diferencias > 0 || soloArca > 0 || soloSistema > 0;

                btnExportarConciliacion.Visible = items.Count > 0;

                MostrarEstadoConciliacion(
                    $"{conciliados} ok  •  {diferencias} dif.  •  {soloArca} solo ARCA  •  {soloSistema} solo sist.",
                    hayInc ? Color.DarkOrange : Color.DarkGreen);
            }
            catch (Exception ex)
            {
                MostrarEstadoConciliacion($"Error: {ex.Message}", Color.DarkRed);
                MessageBox.Show($"Error en conciliación:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static List<ComprobanteCsv> FiltrarArcaPorFecha(
            List<ComprobanteCsv> lista, DateTime desde, DateTime hasta)
        {
            return lista.Where(c =>
            {
                if (DateTime.TryParseExact(c.FechaEmision,
                        new[] { "yyyy-MM-dd", "dd/MM/yyyy" },
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
                    return fecha.Date >= desde && fecha.Date <= hasta;
                return false; // fecha no parseable ? excluir
            }).ToList();
        }

        private void BtnConciliarXCuit_Click(object sender, EventArgs e)
        {
            if (_comprobantesArca.Count == 0 && _comprobantesLocales.Count == 0)
            {
                MessageBox.Show(
                    "Cargá al menos los CSV de ARCA o el archivo del sistema local.",
                    "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (dtpFechaFin.Value.Date < dtpFechaInicio.Value.Date)
            {
                MessageBox.Show("La fecha de fin no puede ser anterior a la de inicio.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var desde = dtpFechaInicio.Value.Date;
            var hasta = dtpFechaFin.Value.Date;

            var arcaFiltrados  = FiltrarArcaPorFecha(_comprobantesArca, desde, hasta);
            var localFiltrados = _comprobantesLocales
                .Where(c => c.Fecha.Date >= desde && c.Fecha.Date <= hasta)
                .ToList();

            var resumen = ConciliacionService.ConciliarPorTotalesXCuit(arcaFiltrados, localFiltrados);

            if (resumen.Count == 0)
            {
                MessageBox.Show(
                    "No hay comprobantes en el rango de fechas seleccionado.",
                    "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MostrarResumenXCuit(resumen);
        }

        private void EjecutarConciliacionPorCuit(string cuit)
        {
            var cuitNorm = new string(cuit.Where(char.IsDigit).ToArray());
            var desde    = dtpFechaInicio.Value.Date;
            var hasta    = dtpFechaFin.Value.Date;

            var arcaFiltrados = FiltrarArcaPorFecha(_comprobantesArca, desde, hasta)
                .Where(a => new string((a.NroDocEmisor ?? string.Empty).Where(char.IsDigit).ToArray()) == cuitNorm)
                .ToList();
            var localFiltrados = _comprobantesLocales
                .Where(c => c.Fecha.Date >= desde && c.Fecha.Date <= hasta
                         && new string((c.Cuit ?? string.Empty).Where(char.IsDigit).ToArray()) == cuitNorm)
                .ToList();

            EjecutarConciliacion(arcaFiltrados, localFiltrados, cuit);
        }

        // ?? Grids ?????????????????????????????????????????????????????????????????

        private void CargarGridArca(List<ComprobanteCsv> comprobantes)
        {
            foreach (var c in comprobantes)
                c.DescripcionTipoComprobante = TablaComprobanteDescripcionMapper.ObtenerDescripcion(c.TipoComprobante);
            gridComprobantes.DataSource = null;
            gridComprobantes.DataSource = comprobantes;
            ConfigurarColumnasArca();
            lblTituloArca.Text = "Comprobantes ARCA";
        }

        private void ConfigurarColumnasArca()
        {
            var columnas = new Dictionary<string, string>
            {
                { "FechaEmision",               "Fecha"       },
                { "TipoComprobante",            "Tipo"        },
                { "DescripcionTipoComprobante", "Descripción"},
                { "PuntoVenta",                 "Pto. Vta."   },
                { "NumeroDesde",                "Número"      },
                { "CodAutorizacion",            "CAE / CAI"   },
                { "NroDocEmisor",               "Doc. Emisor" },
                { "DenominacionEmisor",         "Emisor"      },
                { "Moneda",                     "Moneda"      },
                { "ImpTotal",                   "Total"       },
            };
            var visibles = new HashSet<string>(columnas.Keys);
            foreach (var col in gridComprobantes.Columns.OfType<GridViewDataColumn>())
            {
                if (columnas.TryGetValue(col.FieldName, out var h)) col.HeaderText = h;
                col.IsVisible = visibles.Contains(col.FieldName);
            }
            gridComprobantes.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;
        }

        private void MostrarResumenXCuit(List<ItemConciliacionXCuit> resumen)
        {
            gridComprobantes.DataSource = null;
            gridComprobantes.DataSource = resumen;
            ConfigurarColumnasXCuit();
            lblTituloArca.Text = "Resumen por CUIT";
            int conDif = resumen.Count(x => x.TieneDiferencia);
            int ok     = resumen.Count - conDif;
            MostrarEstado(
                $"{conDif} con diferencia  •  {ok} OK  •  Doble clic en un CUIT para ver el detalle",
                conDif > 0 ? Color.DarkOrange : Color.DarkGreen);
        }

        private void ConfigurarColumnasXCuit()
        {
            var columnas = new Dictionary<string, string>
            {
                { "EstadoTexto",     "Estado"      },
                { "CuitProveedor",   "CUIT"        },
                { "NombreProveedor", "Proveedor"   },
                { "CantARCA",        "Cant. ARCA"  },
                { "CantSistema",     "Cant. Sist." },
                { "TotalARCA",       "Total ARCA"  },
                { "TotalSistema",    "Total Sist." },
                { "Diferencia",      "Diferencia"  },
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
                { "DescripcionTipoComprobante", "Descripción"     },
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

            e.RowElement.DrawFill = true;
            e.RowElement.GradientStyle = Telerik.WinControls.GradientStyles.Solid;
            e.RowElement.BackColor = EstadoConciliacionColores.Rgb.TryGetValue(item.Estado, out var rgb)
                ? Color.FromArgb(rgb.R, rgb.G, rgb.B)
                : Color.White;
        }

        private void GridComprobantes_RowFormatting(object sender, RowFormattingEventArgs e)
        {
            if (e.RowElement.RowInfo is not GridViewDataRowInfo dataRow ||
                dataRow.DataBoundItem is not ItemConciliacionXCuit item)
            {
                e.RowElement.DrawFill = false;
                return;
            }
            e.RowElement.DrawFill      = true;
            e.RowElement.GradientStyle = Telerik.WinControls.GradientStyles.Solid;
            e.RowElement.BackColor     = item.TieneDiferencia
                ? Color.FromArgb(255, 220, 150)
                : Color.FromArgb(200, 240, 200);
        }

        private void GridComprobantes_CellDoubleClick(object sender, GridViewCellEventArgs e)
        {
            if (e.Row is not GridViewDataRowInfo dataRow ||
                dataRow.DataBoundItem is not ItemConciliacionXCuit item)
                return;
            EjecutarConciliacionPorCuit(item.CuitProveedor);
        }

        private void GridConciliacion_CellDoubleClick(object sender, GridViewCellEventArgs e)
        {
            if (e.Row is not GridViewDataRowInfo dataRow ||
                dataRow.DataBoundItem is not ItemConciliacion item)
                return;
            new FormDetalleItemConciliacion(item).Show(this);
        }

        // ── Enriquecimiento Solo ARCA ────────────────────────────────────────────

        private static void EnriquecerSoloArca(List<ItemConciliacion> items)
        {
            var desconocidos = SoloArcaEnricher.Enriquecer(items);

            if (desconocidos.Count > 0)
            {
                MessageBox.Show(
                    "Se encontraron tipos de comprobante no reconocidos por el mapper:\n\n" +
                    string.Join("\n", desconocidos.Select(d => $"  • \"{d}\"")) +
                    "\n\nEstos documentos no podrán darse de alta automáticamente.\n" +
                    "Informar al equipo de desarrollo para actualizar el diccionario.",
                    "Tipos de comprobante desconocidos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void MostrarEstado(string texto, Color color)
        {
            lblEstado.Text = $"{IconoEstado(color)} {texto}";
            lblEstado.ForeColor = color;
        }

        private void MostrarEstadoConciliacion(string texto, Color color)
        {
            lblEstadoConciliacion.Text = $"{IconoEstado(color)} {texto}";
            lblEstadoConciliacion.ForeColor = color;
        }

        /// <summary>
        /// Ícono redundante al color, para que el estado no dependa solo de distinguir
        /// verde/naranja/rojo (accesibilidad para usuarios con daltonismo).
        /// </summary>
        private static string IconoEstado(Color color)
        {
            if (color == Color.DarkGreen) return "✓";  // ✓
            if (color == Color.DarkRed) return "✗";    // ✗
            if (color == Color.DarkOrange) return "⚠"; // ⚠
            return "•";                                 // •
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
                Title = "Guardar conciliaci\u00f3n como...",
                Filter = "Archivos Excel (*.xlsx)|*.xlsx",
                FileName = $"Conciliacion_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
                DefaultExt = "xlsx",
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

        // ── Exportación a sistema externo ───────────────────────────────────────

        private void InicializarBotonConciliarXCuit()
        {
            btnConciliarXCuit = new Telerik.WinControls.UI.RadButton();
            ((System.ComponentModel.ISupportInitialize)btnConciliarXCuit).BeginInit();
            btnConciliarXCuit.Location = new System.Drawing.Point(12, 428);
            btnConciliarXCuit.Size     = new System.Drawing.Size(248, 28);
            btnConciliarXCuit.TabIndex = 13;
            btnConciliarXCuit.Text     = "Conciliar por totales x CUIT";
            btnConciliarXCuit.Click   += BtnConciliarXCuit_Click;
            ((System.ComponentModel.ISupportInitialize)btnConciliarXCuit).EndInit();
            pnlLeft.Controls.Add(btnConciliarXCuit);
        }

        private void InicializarBotonExportarSistema()
        {
            btnExportarSistema = new Telerik.WinControls.UI.RadButton();
            ((System.ComponentModel.ISupportInitialize)btnExportarSistema).BeginInit();
            btnExportarSistema.Location = new System.Drawing.Point(12, 496);
            btnExportarSistema.Size = new System.Drawing.Size(248, 28);
            btnExportarSistema.TabIndex = 11;
            btnExportarSistema.Visible = false;
            btnExportarSistema.Click += BtnExportarSistema_Click;
            ((System.ComponentModel.ISupportInitialize)btnExportarSistema).EndInit();
            pnlLeft.Controls.Add(btnExportarSistema);

            // Mover lblEstado hacia abajo para dejar espacio al nuevo botón
            lblEstado.Location = new System.Drawing.Point(12, 530);

            ActualizarBotonExportarSistema();
        }

        private void ActualizarBotonExportarSistema()
        {
            if (_perfil.SistemaExportacion == SistemaExportacionOffline.Ninguno)
            {
                btnExportarSistema.Visible = false;
                return;
            }

            var nombre = ExportadorSistemaFactory.ObtenerNombre(_perfil.SistemaExportacion);
            btnExportarSistema.Text = $"Exportar para {nombre}...";
            btnExportarSistema.Visible = _comprobantesArca.Count > 0;
        }

        private void BtnExportarSistema_Click(object sender, EventArgs e)
        {
            var exportador = ExportadorSistemaFactory.ObtenerExportador(_perfil);
            if (exportador == null) return;

            var desde = dtpFechaInicio.Value.Date;
            var hasta = dtpFechaFin.Value.Date;
            var filtrados = FiltrarArcaPorFecha(_comprobantesArca, desde, hasta);

            if (filtrados.Count == 0)
            {
                MessageBox.Show(
                    "No hay comprobantes ARCA en el rango de fechas seleccionado.",
                    "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dlg = new SaveFileDialog
            {
                Title = $"Exportar para {exportador.NombreSistema}...",
                Filter = "Archivos Excel (*.xlsx)|*.xlsx",
                FileName = $"{exportador.NombreSistema}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
                DefaultExt = "xlsx",
                OverwritePrompt = true
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                exportador.Exportar(filtrados, dlg.FileName);
                MostrarEstado(
                    $"Exportado para {exportador.NombreSistema}: {System.IO.Path.GetFileName(dlg.FileName)}",
                    Color.DarkGreen);

                if (MessageBox.Show("¿Abrir el archivo?", "Exportado",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    System.Diagnostics.Process.Start(
                        new System.Diagnostics.ProcessStartInfo(dlg.FileName)
                        { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar para {exportador.NombreSistema}:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Exportación SOLO ARCA a PRESEA por QR (Fase 3) ──────────────────────

        private Telerik.WinControls.UI.RadButton btnExportarPreseaQr;

        private void InicializarBotonExportarPreseaQr()
        {
            // Bajar el label de estado para hacer lugar al boton (evita pisarse con
            // btnExportarConciliacion / btnExportarSistema que ocupan 462..524).
            lblEstado.Location = new System.Drawing.Point(12, 566);

            btnExportarPreseaQr = new Telerik.WinControls.UI.RadButton();
            ((System.ComponentModel.ISupportInitialize)btnExportarPreseaQr).BeginInit();
            btnExportarPreseaQr.Location = new System.Drawing.Point(12, 532);
            btnExportarPreseaQr.Size     = new System.Drawing.Size(248, 28);
            btnExportarPreseaQr.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnExportarPreseaQr.Text     = "Exportar SOLO ARCA a PRESEA (QR)...";
            btnExportarPreseaQr.Visible  = true;
            btnExportarPreseaQr.Click   += BtnExportarPreseaQr_Click;
            ((System.ComponentModel.ISupportInitialize)btnExportarPreseaQr).EndInit();
            pnlLeft.Controls.Add(btnExportarPreseaQr);
            btnExportarPreseaQr.BringToFront();
        }

        private void BtnExportarPreseaQr_Click(object sender, EventArgs e)
        {
            if (gridConciliacion.DataSource is not List<ItemConciliacion> items || items.Count == 0)
            {
                MessageBox.Show("Ejecutá la conciliación primero para obtener los comprobantes SOLO ARCA.",
                    "Sin conciliación", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var soloArca = items.Where(x => x.Estado == EstadoConciliacion.SoloARCA).ToList();
            if (soloArca.Count == 0)
            {
                MessageBox.Show("No hay comprobantes SOLO ARCA en la conciliación actual.",
                    "Sin pendientes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var form = new FormExportarPreseaQr(soloArca);
            if (form.ShowDialog(this) != DialogResult.OK) return;

            var seleccionados = form.ItemsSeleccionados;
            if (seleccionados.Count == 0) return;

            using var completar = new FormCompletarPresea(seleccionados, _perfil);
            completar.ShowDialog(this);
        }

        private void GuardarPerfil()
        {
            var todos = AppServices.PerfilesOffline;
            var idx = todos.FindIndex(p => p.Id == _perfil.Id);
            if (idx >= 0) todos[idx] = _perfil;
            else todos.Add(_perfil);
            AppServices.SavePerfilesOffline(todos);
        }


    }
}
