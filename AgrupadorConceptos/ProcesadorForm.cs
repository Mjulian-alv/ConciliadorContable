using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Services;
using Telerik.WinControls.UI;
using System.Drawing;
using System.Threading;

namespace AgrupadorConceptos
{
    public partial class ProcesadorForm : Form
    {
        // Archivo y perfil que la grilla esta mostrando de verdad. No alcanza con mirar
        // los combos: despues de importar, cmbArchivos puede quedar apuntando a otro
        // archivo, y toda operacion sobre "la sesion actual" terminaba yendo contra el
        // archivo equivocado.
        private ArchivoImportado _archivoEnGrilla;
        private PerfilBanco _perfilEnGrilla;

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
            LimpiarSesion(); // La sesion cargada era de otro perfil
        }

        /// <param name="idASeleccionar">
        /// Archivo que tiene que quedar seleccionado al terminar; si es null se conserva
        /// el que ya estaba. Al importar hay que pasarlo si o si: de lo contrario el combo
        /// se queda en el archivo anterior mientras la grilla ya muestra el nuevo.
        /// </param>
        private void CargarArchivosDelPerfil(int? idASeleccionar = null)
        {
            if (cboPerfiles.SelectedItem is not PerfilBanco perfil)
            {
                cmbArchivos.DataSource = null;
                return;
            }

            var archivos = ArchivoImportadoStorage.ObtenerPorPerfil(perfil.Id);
            int? idDeseado = idASeleccionar ?? cmbArchivos.SelectedValue as int?;

            cmbArchivos.DataSource = archivos;
            cmbArchivos.DisplayMember = "DisplayName";
            cmbArchivos.ValueMember = "Id";

            if (idDeseado != null && archivos.Any(a => a.Id == idDeseado.Value))
            {
                cmbArchivos.SelectedValue = idDeseado.Value;
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
            if (cmbArchivos.SelectedItem is not ArchivoImportado archivo ||
                cboPerfiles.SelectedItem is not PerfilBanco perfil) return;

            var movs = SesionMovimientosService.RehomologarPendientes(archivo.Id, perfil);
            MostrarSesion(archivo, perfil, movs);
        }

        /// <summary>
        /// Bindea la grilla y deja registrado que archivo/perfil es el que se esta trabajando.
        /// Es el unico lugar que puede rebindear dgvDatos: cualquier otro refresco tiene que
        /// ir por RefrescarGrillaConservandoPosicion para no mover al usuario de lugar.
        /// </summary>
        private void MostrarSesion(ArchivoImportado archivo, PerfilBanco perfil, List<MovimientoProcesado> movs)
        {
            _archivoEnGrilla = archivo;
            _perfilEnGrilla = perfil;

            dgvDatos.DataSource = null;
            dgvDatos.DataSource = movs;
            ConfigurarGrilla();

            ActualizarResumen(movs);
        }

        private void LimpiarSesion()
        {
            _archivoEnGrilla = null;
            _perfilEnGrilla = null;
            dgvDatos.DataSource = null;
            lblTotalRegistros.Text = "Registros leídos: 0";
        }

        /// <summary>
        /// Vuelve a leer los valores de la lista ya bindeada sin rebindear: la fila actual,
        /// la columna y el scroll quedan donde el usuario los dejo. Hace falta el Refresh
        /// explicito porque MovimientoProcesado no notifica cambios de propiedad.
        /// </summary>
        private void RefrescarGrillaConservandoPosicion()
        {
            int idFilaActual = (dgvDatos.CurrentRow?.DataBoundItem as MovimientoProcesado)?.Id ?? 0;
            var columnaActual = dgvDatos.CurrentColumn;

            dgvDatos.MasterTemplate.Refresh();

            if (idFilaActual == 0) return;

            var fila = dgvDatos.Rows.FirstOrDefault(
                r => r.DataBoundItem is MovimientoProcesado m && m.Id == idFilaActual);
            if (fila == null) return;

            dgvDatos.CurrentRow = fila;
            if (columnaActual != null) dgvDatos.CurrentColumn = columnaActual;
            fila.EnsureVisible();
        }

        /// <summary>
        /// Aplica las homologaciones nuevas sobre los movimientos que ya estan en la grilla
        /// y refresca en el lugar. No relee de la base ni rebindea: la grilla sigue mostrando
        /// el archivo que se esta trabajando y el cursor no se mueve.
        /// </summary>
        private void AplicarHomologacionesEnGrilla(string conceptoADespegar = null)
        {
            if (_perfilEnGrilla == null) return;
            if (dgvDatos.DataSource is not List<MovimientoProcesado> movs) return;

            SesionMovimientosService.RehomologarEnMemoria(movs, _perfilEnGrilla, conceptoADespegar);

            RefrescarGrillaConservandoPosicion();
            ActualizarResumen(movs);
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
                    if (_archivoEnGrilla?.Id == archivo.Id) LimpiarSesion();
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
            if (movimientos == null || movimientos.Count == 0 || _perfilEnGrilla == null)
            {
                MessageBox.Show("No hay datos cargados en la sesión.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var frm = new HomologacionMasivaForm(movimientos, _perfilEnGrilla);
            frm.ShowDialog();

            if (frm.HuboCambios)
            {
                // La ventana masiva aplico y persistio los mapeos sobre esta misma lista.
                RefrescarGrillaConservandoPosicion();
                ActualizarResumen(movimientos);
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

            var pendientes = ConsolidadoExporter.ContarPendientes(movimientos);
            if (pendientes > 0)
            {
                var r = MessageBox.Show($"Hay {pendientes} movimientos sin homologar. ¿Desea continuar con la exportación ignorando estos registros?", "Advertencia", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (r != DialogResult.Yes) return;
            }

            var consolidado = ConsolidadoExporter.Calcular(movimientos);

            using (SaveFileDialog sfd = new SaveFileDialog { Filter = "Archivos de Excel (*.xlsx)|*.xlsx", FileName = "Consolidado.xlsx" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string titulo = $"Consolidado Bancario - {DateTime.Now:dd/MM/yyyy HH:mm} - {_archivoEnGrilla?.DisplayName}";
                        ConsolidadoExporter.ExportarAExcel(consolidado, titulo, sfd.FileName);
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
            if (_perfilEnGrilla == null)
            {
                MessageBox.Show("Primero cargue la sesión de un archivo.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (dgvDatos.CurrentRow?.DataBoundItem is not MovimientoProcesado movInfo)
            {
                MessageBox.Show("Por favor seleccione un movimiento de la grilla que desee homologar.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Si ya estaba homologado, el concepto viejo tiene que volver a pendiente para
            // que la homologación nueva lo agarre.
            string conceptoADespegar = null;
            if (movInfo.ConceptoEstandar != ConceptosBancarios.PendienteHomologar)
            {
                var diag = MessageBox.Show($"El movimiento ya se encuentra homologado como '{movInfo.ConceptoEstandar}'. ¿Desea crear una nueva homologación para la descripción/concepto '{movInfo.ConceptoOriginal}'?", "Aviso", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (diag != DialogResult.Yes) return;
                conceptoADespegar = movInfo.ConceptoEstandar;
            }

            string valorParaHomologar = _perfilEnGrilla.EsCodigo ? movInfo.ConceptoOriginal : movInfo.DescripcionOriginal;

            HomologarForm frmHomologar = new HomologarForm(_perfilEnGrilla.Id, valorParaHomologar);
            frmHomologar.ShowDialog();

            if (frmHomologar.HomologacionExitosa)
            {
                AplicarHomologacionesEnGrilla(conceptoADespegar);
            }
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
            try
            {
                var movimientos = ImportacionService.ImportarArchivo(filePath, perfil, ReportarProgresoImportacion);

                // Step 3: Mostrando datos
                AvanzarStep(3);
                int idArchivo = movimientos.Count > 0 ? movimientos[0].IdArchivo : 0;
                this.Invoke(() =>
                {
                    PB_Importar.Value1 = 90;
                    // El combo tiene que quedar en el archivo recien importado: es el que va a
                    // mostrar la grilla y contra el que van a ir las homologaciones.
                    CargarArchivosDelPerfil(idArchivo);
                    var archivo = cmbArchivos.SelectedItem as ArchivoImportado;
                    MostrarSesion(archivo?.Id == idArchivo ? archivo : null, perfil, movimientos);
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

        /// <summary>
        /// Traduce el avance que reporta ImportacionService a los steps de Telerik.
        /// Corre en el hilo de background, así que todo toque de UI va por Invoke.
        /// </summary>
        private void ReportarProgresoImportacion(ProgresoImportacion p)
        {
            switch (p.Paso)
            {
                case ProgresoImportacion.PasoLeyendo:
                    AvanzarStep(0);
                    this.Invoke(() => PB_Importar.Value1 = 10);
                    break;

                case ProgresoImportacion.PasoHomologando:
                    AvanzarStep(1);
                    this.Invoke(() => PB_Importar.Value1 = 30);
                    break;

                case ProgresoImportacion.PasoGuardando:
                    if (p.Guardados == 0)
                    {
                        AvanzarStep(2);
                        this.Invoke(() =>
                        {
                            PB_Importar.Value1 = 65;
                            SPB_Importar.Steps[2].SecondHeader = $"0 / {p.Total}";
                        });
                        break;
                    }

                    // Actualizar cada 500 registros (o el último) para no saturar la UI
                    if (p.Guardados % 500 != 0 && p.Guardados != p.Total) break;

                    int pbValue = 65 + (int)(20.0 * p.Guardados / p.Total); // de 65 a 85
                    this.Invoke(() =>
                    {
                        PB_Importar.Value1 = pbValue;
                        SPB_Importar.Steps[2].SecondHeader = $"{p.Guardados} / {p.Total}";
                    });
                    break;
            }
        }
    }
}
