using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using ExcelDataReader;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Services;
using Telerik.WinControls.UI;
using System.Drawing;
using ClosedXML.Excel;
using System.Threading;

namespace AgrupadorConceptos
{
    public partial class ProcesadorForm : Form
    {
        public ProcesadorForm()
        {
            InitializeComponent();
            this.Icon = AppIcon.GetIcon();
            this.Load += ProcesadorForm_Load;
            this.dgvDatos.CellFormatting += DgvDatos_CellFormatting;
            this.dgvDatos.CellDoubleClick += DgvDatos_CellDoubleClick;
            this.dgvDatos.CellValueChanged += DgvDatos_CellValueChanged;
            this.cboPerfiles.SelectedIndexChanged += CboPerfiles_SelectedIndexChanged;
        }

        private void DgvDatos_CellFormatting(object sender, CellFormattingEventArgs e)
        {
            if (e.CellElement is GridDataCellElement dataCell && e.CellElement.ColumnInfo.Name == "Debitos")
            {
                if (dataCell.Value != null && dataCell.Value is decimal debitos)
                {
                    if (debitos < 0)
                    {
                        e.CellElement.ForeColor = Color.Red;
                    }
                    else
                    {
                        e.CellElement.ResetValue(LightVisualElement.ForeColorProperty, Telerik.WinControls.ValueResetFlags.Local);
                    }
                }
            }
            else
            {
                e.CellElement.ResetValue(LightVisualElement.ForeColorProperty, Telerik.WinControls.ValueResetFlags.Local);
            }
        }

        private void ProcesadorForm_Load(object sender, EventArgs e)
        {
            CargarPerfiles();

            // Si no hay perfiles, abrimos la creacin obligatoriamente
            if (cboPerfiles.Items.Count == 0)
            {
                MessageBox.Show("No hay perfiles configurados en la base de datos. Por favor, cree al menos uno para comenzar.", "Bienvenido", MessageBoxButtons.OK, MessageBoxIcon.Information);
                AbrirNuevoPerfil();
            }
            
            CargarArchivosDelPerfil();
        }

        private void CargarPerfiles()
        {
            try
            {
                var perfiles = PerfilBancoStorage.ObtenerTodos();

                var selectedId = cboPerfiles.SelectedValue;
                cboPerfiles.SelectedIndexChanged -= CboPerfiles_SelectedIndexChanged;

                cboPerfiles.DataSource = perfiles;
                cboPerfiles.DisplayMember = "NombreBanco";
                cboPerfiles.ValueMember = "Id";

                if (selectedId != null && perfiles.Any(p => p.Id == (int)selectedId))
                {
                    cboPerfiles.SelectedValue = selectedId;
                }

                cboPerfiles.SelectedIndexChanged += CboPerfiles_SelectedIndexChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar perfiles: {ex.Message}");
            }
        }

        private void CboPerfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarArchivosDelPerfil();
            dgvDatos.DataSource = null; // Limpiamos la grilla al cambiar el perfil
        }

        private void CargarArchivosDelPerfil()
        {
            if (cboPerfiles.SelectedItem is PerfilBanco perfil)
            {
                var archivos = ArchivoImportadoStorage.ObtenerPorPerfil(perfil.Id);

                var selectedId = cmbArchivos.SelectedValue;
                cmbArchivos.DataSource = archivos;
                cmbArchivos.DisplayMember = "DisplayName";
                cmbArchivos.ValueMember = "Id";

                if (selectedId != null && archivos.Any(a => a.Id == (int)selectedId))
                {
                    cmbArchivos.SelectedValue = selectedId;
                }
            }
            else
            {
                cmbArchivos.DataSource = null;
            }
        }

        private void btnNuevoPerfil_Click(object sender, EventArgs e)
        {
            AbrirNuevoPerfil();
        }

        private void btnEditarPerfil_Click(object sender, EventArgs e)
        {
            if (cboPerfiles.SelectedItem is PerfilBanco perfil)
            {
                AbrirNuevoPerfil(perfil.Id);
            }
            else
            {
                MessageBox.Show("Por favor, seleccione un perfil para editar.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AbrirNuevoPerfil(int? idPerfil = null)
        {
            Form frmPerfil = new MainForm(idPerfil);
            frmPerfil.ShowDialog();
            // Al volver de crear/editar un perfil, recargamos el combo
            CargarPerfiles();
        }

        private void btnCargarSesion_Click(object sender, EventArgs e)
        {
            if (cmbArchivos.SelectedItem is ArchivoImportado archivo && cboPerfiles.SelectedItem is PerfilBanco perfil)
            {
                // Aplicar mapeos nuevamente por si algo cambió en la configuración de homologaciones
                var movs = MovimientoStorage.ObtenerPorArchivo(archivo.Id);
                var dicHomologacion = HomologacionStorage.ObtenerDiccionario(perfil.Id);

                var rehomologados = new List<MovimientoProcesado>();
                foreach (var mov in movs)
                {
                    if (mov.ConceptoEstandar == ConceptosBancarios.PendienteHomologar)
                    {
                        HomologacionMatcher.AplicarA(mov, perfil.EsCodigo, dicHomologacion);
                        rehomologados.Add(mov);
                    }
                }
                MovimientoStorage.ActualizarConceptos(rehomologados);

                dgvDatos.DataSource = null;
                dgvDatos.DataSource = movs;
                ConfigurarGrilla();

                ActualizarResumen(movs);
            }
        }
        private void marcarComoPendiente(string conceptoStandard)
        {
            if (cmbArchivos.SelectedItem is ArchivoImportado archivo && cboPerfiles.SelectedItem is PerfilBanco)
            {
                var movs = MovimientoStorage.ObtenerPorArchivo(archivo.Id);

                var vueltosAPendiente = new List<MovimientoProcesado>();
                foreach (var mov in movs)
                {
                    if (mov.ConceptoEstandar == conceptoStandard)
                    {
                        mov.ConceptoEstandar = ConceptosBancarios.PendienteHomologar;
                        mov.ConceptoFinal = ConceptosBancarios.PendienteHomologar;
                        vueltosAPendiente.Add(mov);
                    }
                }
                MovimientoStorage.ActualizarConceptos(vueltosAPendiente);
            }
        }
        private void btnBorrarSesion_Click(object sender, EventArgs e)
        {
            if (cmbArchivos.SelectedItem is ArchivoImportado archivo)
            {
                if (MessageBox.Show($"¿Desea borrar el archivo {archivo.NombreArchivo} y todos sus movimientos?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        ArchivoImportadoStorage.Eliminar(archivo.Id);
                    }
                    catch (Microsoft.Data.SqlClient.SqlException)
                    {
                        MessageBox.Show("No se puede borrar: el archivo tiene sesiones de conciliación asociadas.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    CargarArchivosDelPerfil();
                    dgvDatos.DataSource = null;
                }
            }
        }

        private void btnGestionarHomologaciones_Click(object sender, EventArgs e)
        {
            var frm = new GestionHomologacionesForm();
            frm.ShowDialog();
        }

        private void btnHomologacionMasiva_Click(object sender, EventArgs e)
        {
            var movimientos = dgvDatos.DataSource as List<MovimientoProcesado>;
            if (movimientos == null || movimientos.Count == 0)
            {
                MessageBox.Show("No hay datos cargados en la sesión.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (cboPerfiles.SelectedItem is PerfilBanco perfil)
            {
                var frm = new HomologacionMasivaForm(movimientos, perfil.Id, perfil.EsCodigo);
                frm.ShowDialog();

                if (frm.HuboCambios)
                {
                    btnCargarSesion_Click(null, null); // Recargar la sesión para aplicar mapeos masivos
                }
            }
        }

        private void DgvDatos_CellDoubleClick(object sender, GridViewCellEventArgs e)
        {
            btnHomologar_Click(sender, e);
        }

        private void DgvDatos_CellValueChanged(object sender, GridViewCellEventArgs e)
        {
            if (e.Column.Name == "ConceptoFinal" && e.Row.DataBoundItem is MovimientoProcesado mov)
            {
                MovimientoStorage.ActualizarConceptoFinal(mov.Id, mov.ConceptoFinal);
            }
        }

        private void ConfigurarGrilla()
        {
            foreach (var col in dgvDatos.Columns)
            {
                col.ReadOnly = col.Name != "ConceptoFinal";
                col.IsVisible = col.Name != "Id" && col.Name !="IdArchivo";
            }
        }

        private void ActualizarResumen(List<MovimientoProcesado> movs)
        {
            if (movs == null) return;
            int total = movs.Count;
            int homologados = movs.Count(m => m.ConceptoEstandar != ConceptosBancarios.PendienteHomologar);
            int pendientes = total - homologados;
            lblTotalRegistros.Text = $"Registros leídos: {total}   |   Homologados: {homologados}   |   Pendientes: {pendientes}";
        }

        private void btnExportarConsolidado_Click(object sender, EventArgs e)
        {
            var movimientos = dgvDatos.DataSource as List<MovimientoProcesado>;
            if (movimientos == null || movimientos.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var pendientes = movimientos.Count(m => ConceptosBancarios.EstaPendiente(m.ConceptoFinal));
            if (pendientes > 0)
            {
                var r = MessageBox.Show($"Hay {pendientes} movimientos sin homologar. ¿Desea continuar con la exportación ignorando estos registros?", "Advertencia", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (r != DialogResult.Yes) return;
            }

            var consolidado = movimientos
                .Where(m => !ConceptosBancarios.EstaPendiente(m.ConceptoFinal))
                .GroupBy(m => m.ConceptoFinal)
                .Select(g => new {
                    Concepto = g.Key,
                    Debitos = Math.Abs(Math.Round(g.Sum(x => x.Debitos), 2)),
                    Creditos = Math.Round(g.Sum(x => x.Creditos), 2),
                    Saldo = Math.Round(g.Sum(x => x.Creditos)- Math.Abs(g.Sum(x => x.Debitos)), 2)
                })
                .OrderBy(x => x.Concepto)
                .ToList();

            using (SaveFileDialog sfd = new SaveFileDialog { Filter = "Archivos de Excel (*.xlsx)|*.xlsx", FileName = "Consolidado.xlsx" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using var wb = new XLWorkbook();
                        var ws = wb.Worksheets.Add("Consolidado");

                        // Título y fecha (opcional, como en arcacliente)
                        ws.Cell(1, 1).Value = $"Consolidado Bancario - {DateTime.Now:dd/MM/yyyy HH:mm} - {cmbArchivos.SelectedText}";
                        ws.Cell(1, 1).Style.Font.Bold = true;
                        ws.Cell(1, 1).Style.Font.FontSize = 12;
                        ws.Range(1, 1, 1, 4).Merge();

                        // Encabezados
                        const int headerRow = 2;
                        ws.Cell(headerRow, 1).Value = "Concepto Final";
                        ws.Cell(headerRow, 2).Value = "Débitos";
                        ws.Cell(headerRow, 3).Value = "Créditos";
                        ws.Cell(headerRow, 4).Value = "Saldo";

                        var hr = ws.Range(headerRow, 1, headerRow, 4);
                        hr.Style.Font.Bold = true;
                        hr.Style.Font.FontColor = XLColor.White;
                        hr.Style.Fill.BackgroundColor = XLColor.FromArgb(50, 50, 50);
                        hr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        hr.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                        hr.Style.Border.BottomBorderColor = XLColor.Black;

                        // Datos
                        int row = headerRow + 1;
                        foreach (var item in consolidado)
                        {
                            ws.Cell(row, 1).Value = item.Concepto;
                            ws.Cell(row, 2).Value = item.Debitos;
                            ws.Cell(row, 3).Value = item.Creditos;
                            ws.Cell(row, 4).Value = item.Saldo;

                            ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00";
                            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
                            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";

                            row++;
                        }

                        // Formato final
                        ws.Columns().AdjustToContents(1, row);
                        ws.SheetView.Freeze(headerRow, 0);

                        var dataRange = ws.Range(1, 1, row - 1, 4);
                        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        dataRange.Style.Border.OutsideBorderColor = XLColor.Gray;

                        wb.SaveAs(sfd.FileName);
                        MessageBox.Show("Consolidado exportado a Excel exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnCargarArchivo_Click(object sender, EventArgs e)
        {
            if (cboPerfiles.SelectedItem == null)
            {
                MessageBox.Show("Por favor, seleccione un perfil bancario.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var perfil = (PerfilBanco)cboPerfiles.SelectedItem;

            using var ofd = new OpenFileDialog() { Filter = "Archivos Excel/CSV|*.xls;*.xlsx;*.csv" };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            string filePath = ofd.FileName;

            // Mostrar panel de progreso
            MostrarProgreso(true);

            // Procesar en hilo background para no bloquear la UI
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    ProcesarArchivoExcel(filePath, perfil, true);
                }
                finally
                {
                    this.Invoke(() => MostrarProgreso(false));
                }
            });
        }

        private void MostrarProgreso(bool visible)
        {
            pnlProgreso.Visible = visible;
            btnCargarArchivo.Enabled = !visible;
            if (visible)
            {
                // Configurar steps al inicio de cada importación
                SPB_Importar.Steps.Clear();
                SPB_Importar.Steps.Add(new StepProgressItem { FirstHeader = "Leyendo archivo",   Progress = 0 });
                SPB_Importar.Steps.Add(new StepProgressItem { FirstHeader = "Homologando",       Progress = 0 });
                SPB_Importar.Steps.Add(new StepProgressItem { FirstHeader = "Guardando en DB",   Progress = 0 });
                SPB_Importar.Steps.Add(new StepProgressItem { FirstHeader = "Mostrando datos",   Progress = 0 });
            }
        }

        private void btnHomologar_Click(object sender, EventArgs e)
        {
            if (dgvDatos.CurrentRow == null)
            {
                MessageBox.Show("Por favor seleccione un movimiento de la grilla que desee homologar.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var movInfo = (MovimientoProcesado)dgvDatos.CurrentRow.DataBoundItem;
            bool reemplazapornuevo = false;
            string conceptopareemplazar = "";
            if (movInfo.ConceptoEstandar != ConceptosBancarios.PendienteHomologar)
            {
                var diag = MessageBox.Show($"El movimiento ya se encuentra homologado como '{movInfo.ConceptoEstandar}'. ¿Desea crear una nueva homologación para la descripción/concepto '{movInfo.ConceptoOriginal}'?", "Aviso", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (diag != DialogResult.Yes) return;
                reemplazapornuevo = true;
                conceptopareemplazar = movInfo.ConceptoEstandar;

            }

            var perfil = (PerfilBanco)cboPerfiles.SelectedItem;
            string valorParaHomologar = perfil.EsCodigo ? movInfo.ConceptoOriginal : movInfo.DescripcionOriginal;

            HomologarForm frmHomologar = new HomologarForm(perfil.Id, valorParaHomologar);
            frmHomologar.ShowDialog();

            if (frmHomologar.HomologacionExitosa)
            {
                if (reemplazapornuevo)
                {
                    marcarComoPendiente(conceptopareemplazar);
                }
                btnCargarSesion_Click(null, null); // Recargar la sesión desde DB
                

            }
        }

        private decimal ParsearDecimal(object valor)
        {
            if (valor == null) return 0m;
            if (valor is double d) return (decimal)d;
            if (valor is decimal dec) return dec;
            if (valor is int i) return (decimal)i;
            
            string strValor = valor.ToString().Trim();
            if (string.IsNullOrEmpty(strValor)) return 0m;

            if (decimal.TryParse(strValor, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out decimal res1)) return res1;
            if (decimal.TryParse(strValor, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal res2)) return res2;
            
            return 0m;
        }

        private void AvanzarStep(int stepIndex)
        {
            this.Invoke(() =>
            {
                // Completar el step anterior si existe
                if (stepIndex > 0)
                    SPB_Importar.Steps[stepIndex - 1].Progress = 100;
                // Activar el step actual
                if (stepIndex < SPB_Importar.Steps.Count)
                    SPB_Importar.Steps[stepIndex].Progress = 50;
            });
        }

        private void ProcesarArchivoExcel(string filePath, PerfilBanco perfil, bool mostrarMensajeExito)
        {
            var movimientos = new List<MovimientoProcesado>();
            var swTotal = Stopwatch.StartNew();
            var swParseo = new Stopwatch();
            var swGuardado = new Stopwatch();
            try
            {
                // Step 0: Leyendo Excel
                AvanzarStep(0);
                this.Invoke(() => PB_Importar.Value1 = 10);

                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);

                // Insertar registro del archivo
                string nombreArchivo = Path.GetFileName(filePath);
                int idArchivo = ArchivoImportadoStorage.Insertar(perfil.Id, nombreArchivo, DateTime.Now);

                // Step 1: Homologando
                AvanzarStep(1);
                this.Invoke(() => PB_Importar.Value1 = 30);

                var dicHomologacion = HomologacionStorage.ObtenerDiccionario(perfil.Id);

                swParseo.Start();
                string ext = Path.GetExtension(filePath).ToLowerInvariant();
                using var reader = ext == ".csv"
                    ? ExcelReaderFactory.CreateCsvReader(stream)
                    : ExcelReaderFactory.CreateReader(stream);

                // Avanzar hasta la fila de encabezado
                for (int i = 1; i < perfil.FilaEncabezado; i++)
                    reader.Read();

                if (reader.Read())
                {
                    var headers = new List<string>();
                    for (int i = 0; i < reader.FieldCount; i++)
                        headers.Add(reader.GetValue(i)?.ToString()?.Trim() ?? "");

                    int idxConcepto     = headers.IndexOf(perfil.ColumnaConcepto ?? "");
                    int idxDescripcion  = string.IsNullOrEmpty(perfil.ColumnaDescripcion) ? -1 : headers.IndexOf(perfil.ColumnaDescripcion);
                    int idxFecha        = string.IsNullOrEmpty(perfil.ColumnaFecha) ? -1 : headers.IndexOf(perfil.ColumnaFecha);
                    int idxImporteUnico = perfil.TipoImporte == 1 ? headers.IndexOf(perfil.ColumnaImporteUnico ?? "") : -1;
                    int idxDebe         = perfil.TipoImporte == 2 ? headers.IndexOf(perfil.ColumnaDebe ?? "") : -1;
                    int idxHaber        = perfil.TipoImporte == 2 ? headers.IndexOf(perfil.ColumnaHaber ?? "") : -1;

                    if (idxConcepto == -1)
                    {
                        this.Invoke(() => MessageBox.Show("No se encontró la columna del concepto en la fila especificada (verifique el perfil y el archivo)."));
                        return;
                    }

                    while (reader.Read())
                    {
                        string concepto    = reader.GetValue(idxConcepto)?.ToString() ?? "";
                        string descripcion = idxDescripcion != -1 ? (reader.GetValue(idxDescripcion)?.ToString() ?? "") : concepto;
                        string fecha       = idxFecha != -1 ? (reader.GetValue(idxFecha)?.ToString() ?? "") : "";

                        if (string.IsNullOrWhiteSpace(concepto)) continue;

                        decimal debitos = 0, creditos = 0;
                        if (perfil.TipoImporte == 1 && idxImporteUnico != -1)
                        {
                            decimal importeUnico = ParsearDecimal(reader.GetValue(idxImporteUnico));
                            if (importeUnico < 0) debitos = Math.Abs(importeUnico);
                            else creditos = Math.Abs(importeUnico);
                        }
                        else if (perfil.TipoImporte == 2)
                        {
                            decimal debe  = idxDebe  != -1 ? ParsearDecimal(reader.GetValue(idxDebe))  : 0m;
                            decimal haber = idxHaber != -1 ? ParsearDecimal(reader.GetValue(idxHaber)) : 0m;
                            debitos  = debe > 0 ? -debe : debe;
                            creditos = haber;
                        }

                        string valorABuscar = perfil.EsCodigo ? concepto : descripcion;
                        string conceptoEstandar =
                            HomologacionMatcher.Resolver(dicHomologacion, valorABuscar, perfil.EsCodigo)
                            ?? ConceptosBancarios.PendienteHomologar;

                        movimientos.Add(new MovimientoProcesado
                        {
                            ConceptoOriginal    = concepto,
                            DescripcionOriginal = descripcion,
                            Fecha               = fecha,
                            Debitos             = debitos,
                            Creditos            = creditos,
                            ConceptoEstandar    = conceptoEstandar,
                            ConceptoFinal       = conceptoEstandar == ConceptosBancarios.PendienteHomologar ? "" : conceptoEstandar,
                            IdArchivo           = idArchivo
                        });
                    }
                }

                swParseo.Stop();

                // Step 2: Guardando en DB
                AvanzarStep(2);
                swGuardado.Start();
                int total = movimientos.Count;
                this.Invoke(() =>
                {
                    PB_Importar.Value1 = 65;
                    SPB_Importar.Steps[2].SecondHeader = $"0 / {total}";
                });

                MovimientoStorage.InsertarLote(movimientos, (guardados, cantidad) =>
                {
                    // Actualizar cada 500 registros (o el último) para no saturar la UI
                    if (guardados % 500 != 0 && guardados != cantidad) return;

                    int pbValue = 65 + (int)(20.0 * guardados / cantidad); // de 65 a 85
                    this.Invoke(() =>
                    {
                        PB_Importar.Value1 = pbValue;
                        SPB_Importar.Steps[2].SecondHeader = $"{guardados} / {cantidad}";
                    });
                });
                swGuardado.Stop();
                Debug.WriteLine($"[Importar] {total} movs | parseo {swParseo.ElapsedMilliseconds} ms | guardado {swGuardado.ElapsedMilliseconds} ms | total {swTotal.ElapsedMilliseconds} ms");

                // Step 3: Mostrando datos
                AvanzarStep(3);
                this.Invoke(() => PB_Importar.Value1 = 90);

                this.Invoke(() =>
                {
                    CargarArchivosDelPerfil();
                    dgvDatos.DataSource = null;
                    dgvDatos.DataSource = movimientos;
                    ConfigurarGrilla();
                    ActualizarResumen(movimientos);
                    PB_Importar.Value1 = 100;
                    // Completar último step
                    SPB_Importar.Steps[SPB_Importar.Steps.Count - 1].Progress = 100;
                });

                if (mostrarMensajeExito)
                    this.Invoke(() => MessageBox.Show("Archivo leído exitosamente.", "Proceso Finalizado", MessageBoxButtons.OK, MessageBoxIcon.Information));
            }
            catch (Exception ex)
            {
                this.Invoke(() => MessageBox.Show($"Ocurrió un error al procesar el archivo Excel: {ex.Message}", "Error de Lectura", MessageBoxButtons.OK, MessageBoxIcon.Error));
            }
        }
    }
}