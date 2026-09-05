using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;
using Telerik.WinControls.Data;

namespace AgrupadorConceptos
{
    /// <summary>
    /// Alta, baja y modificación de las homologaciones de un perfil. La baja no es un DELETE
    /// suelto: el concepto homologado quedó escrito como texto en cada movimiento, así que
    /// hay que decidir qué pasa con ellos (ver HomologacionAdminService).
    /// </summary>
    public partial class GestionHomologacionesForm : Form
    {
        // Entrada centinela del combo: evita un flag aparte para "no hay perfil elegido".
        private const int IdTodosLosPerfiles = 0;

        private readonly PerfilBanco _perfilInicial;

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 4 - Avisar al llamador que refresque
        /// <summary>True si se tocó alguna homologación: el llamador tiene que refrescar.</summary>
        public bool HuboCambios { get; private set; }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 4 - Abrir filtrada por el perfil que la invoca
        /// <param name="perfilInicial">
        /// Perfil con el que arranca filtrada. Null abre en "(Todos los perfiles)".
        /// </param>
        public GestionHomologacionesForm(PerfilBanco perfilInicial = null)
        {
            InitializeComponent();
            this.Icon = AppIcon.GetIcon();
            _perfilInicial = perfilInicial;

            this.Load += (s, e) =>
            {
                CargarPerfiles();
                CargarDatos();
            };

            dgvHomologaciones.CurrentRowChanged += (s, e) => ActualizarBotones();
        }

        /// <summary>Perfil elegido, o null cuando está en "(Todos los perfiles)".</summary>
        private PerfilBanco PerfilSeleccionado
        {
            get
            {
                var perfil = cboPerfil.SelectedItem as PerfilBanco;
                return perfil != null && perfil.Id != IdTodosLosPerfiles ? perfil : null;
            }
        }

        private HomologacionListado FilaSeleccionada =>
            dgvHomologaciones.CurrentRow?.DataBoundItem as HomologacionListado;

        /// <summary>
        /// Perfil sobre el que opera una fila. Con "(Todos los perfiles)" el combo no lo sabe,
        /// pero la fila trae su IdPerfilBanco.
        /// </summary>
        private PerfilBanco PerfilDeLaFila(HomologacionListado fila) =>
            PerfilSeleccionado ?? PerfilBancoStorage.ObtenerPorId(fila.IdPerfilBanco);

        private void CargarPerfiles()
        {
            var perfiles = PerfilBancoStorage.ObtenerTodos()
                .OrderBy(p => p.NombreBanco)
                .ToList();

            perfiles.Insert(0, new PerfilBanco
            {
                Id = IdTodosLosPerfiles,
                NombreBanco = "(Todos los perfiles)"
            });

            cboPerfil.DataSource = perfiles;
            cboPerfil.DisplayMember = "NombreBanco";
            cboPerfil.ValueMember = "Id";
            cboPerfil.SelectedValue = _perfilInicial?.Id ?? IdTodosLosPerfiles;

            // El handler se engancha despues de fijar la seleccion inicial: si no,
            // el Load dispararia CargarDatos dos veces.
            cboPerfil.SelectedIndexChanged += (s, e) => CargarDatos();
        }

        private void CargarDatos()
        {
            var perfil = PerfilSeleccionado;

            Cursor = Cursors.WaitCursor;
            try
            {
                var filas = HomologacionStorage.ObtenerListado(perfil?.Id);

                // El conteo de uso no sale del SQL: que regla resuelve cada movimiento lo
                // decide el matcher, no una FK. Se calcula perfil por perfil.
                foreach (var grupo in filas.GroupBy(f => f.IdPerfilBanco))
                {
                    var perfilDelGrupo = perfil ?? PerfilBancoStorage.ObtenerPorId(grupo.Key);
                    if (perfilDelGrupo == null) continue;

                    var uso = HomologacionAdminService.ContarUsoPorRegla(perfilDelGrupo);
                    foreach (var fila in grupo)
                        fila.Movimientos = uso.TryGetValue(fila.ValorOriginal, out int n) ? n : 0;
                }

                dgvHomologaciones.DataSource = null;
                dgvHomologaciones.DataSource = filas;

                ConfigurarGrilla(agrupar: perfil == null);
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            ActualizarBotones();
        }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 3 - Agrupar por perfil y ordenar por clave
        // El orden por ValorOriginal descendente es el mismo del diccionario del matcher:
        // la grilla tiene que leerse como la directiva de precedencia que efectivamente rige.
        private void ConfigurarGrilla(bool agrupar)
        {
            OcultarColumna("Id");
            OcultarColumna("IdPerfilBanco");
            OcultarColumna("IdConceptoEstandar");

            RenombrarColumna("Banco", "Perfil / Banco");
            RenombrarColumna("ValorOriginal", "Valor del banco");
            RenombrarColumna("ConceptoEstandar", "Concepto estándar");
            RenombrarColumna("Movimientos", "Movimientos");

            var colMovimientos = dgvHomologaciones.Columns["Movimientos"];
            if (colMovimientos != null)
            {
                colMovimientos.TextAlignment = ContentAlignment.MiddleRight;
                colMovimientos.MaxWidth = 120;
            }

            dgvHomologaciones.SortDescriptors.Clear();
            dgvHomologaciones.SortDescriptors.Add(
                new SortDescriptor("ValorOriginal", ListSortDirection.Descending));

            dgvHomologaciones.GroupDescriptors.Clear();
            if (agrupar)
            {
                var porBanco = new GroupDescriptor();
                porBanco.GroupNames.Add("Banco", ListSortDirection.Ascending);
                dgvHomologaciones.GroupDescriptors.Add(porBanco);
            }
        }

        private void OcultarColumna(string nombre)
        {
            var col = dgvHomologaciones.Columns[nombre];
            if (col != null) col.IsVisible = false;
        }

        private void RenombrarColumna(string nombre, string titulo)
        {
            var col = dgvHomologaciones.Columns[nombre];
            if (col != null) col.HeaderText = titulo;
        }

        private void ActualizarBotones()
        {
            // El alta necesita un perfil concreto: con "(Todos los perfiles)" no hay
            // a cual dar de alta.
            btnNueva.Enabled = PerfilSeleccionado != null;

            bool haySeleccion = FilaSeleccionada != null;
            btnEditar.Enabled = haySeleccion;
            btnEliminar.Enabled = haySeleccion;
        }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 2 - Alta desde la gestion
        private void btnNueva_Click(object sender, EventArgs e)
        {
            var perfil = PerfilSeleccionado;
            if (perfil == null)
            {
                MessageBox.Show("Elija un perfil para dar de alta una homologación.",
                    "Atención", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var frm = new HomologarForm(perfil.Id, "");
            frm.ShowDialog(this);

            if (!frm.HomologacionExitosa) return;

            HuboCambios = true;
            CargarDatos();
        }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 2 - Edicion: reapuntar y arrastrar
        private void btnEditar_Click(object sender, EventArgs e)
        {
            var fila = FilaSeleccionada;
            if (fila == null)
            {
                MessageBox.Show("Seleccione una homologación para editar.",
                    "Atención", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var perfil = PerfilDeLaFila(fila);
            if (perfil == null) return;

            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Preseleccionar el concepto de la fila
            // Sin ConceptoInicial, cmbEstandar arrancaba en el primero de la lista (no en el
            // concepto de esta regla): el usuario podia ver la cuenta de otro concepto y, al
            // aceptar, ActualizarCuentaConcepto (mas abajo) pisaba en silencio la cuenta de un
            // concepto que no tenia nada que ver.
            var frm = new HomologarForm(perfil.Id, fila.ValorOriginal)
            {
                BloquearValorOriginal = true,
                SoloSeleccionar = true,
                ConceptoInicial = fila.ConceptoEstandar,
                Text = "Editar homologación"
            };
            frm.ShowDialog(this);

            if (!frm.HomologacionExitosa) return;

            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - La cuenta se persiste aunque el
            // concepto no cambie. Antes, si el usuario solo queria corregir la cuenta contable
            // (el caso mas comun al editar), el return de abajo por "concepto sin cambios" hacia
            // que ActualizarCuentaConcepto nunca se llamara y la cuenta elegida en el combo se
            // perdiera en silencio. Si el concepto no cambio no hace falta Reapuntar (no hay
            // movimientos que arrastrar): se usa directamente el id de concepto de la fila.
            bool conceptoCambio = !string.Equals(frm.sConcepto, fila.ConceptoEstandar, StringComparison.OrdinalIgnoreCase);

            Cursor = Cursors.WaitCursor;
            try
            {
                int idConcepto = conceptoCambio
                    ? HomologacionAdminService.Reapuntar(fila, perfil, frm.sConcepto)
                    : fila.IdConceptoEstandar;

                HomologacionStorage.ActualizarCuentaConcepto(idConcepto, frm.sIdCuentaContable);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al editar la homologación: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            HuboCambios = true;
            CargarDatos();
        }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 1 - Baja preguntando por los ya homologados
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            var fila = FilaSeleccionada;
            if (fila == null)
            {
                MessageBox.Show("Seleccione una homologación para eliminar.",
                    "Atención", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var perfil = PerfilDeLaFila(fila);
            if (perfil == null) return;

            ImpactoHomologacion impacto;
            Cursor = Cursors.WaitCursor;
            try
            {
                impacto = HomologacionAdminService.CalcularImpactoBaja(fila, perfil);
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            string conceptoDestino = null;

            if (impacto.Total == 0)
            {
                // Sin movimientos que dependan de la regla no hay nada que preguntar.
                var confirma = MessageBox.Show(
                    $"¿Eliminar la homologación '{fila.ValorOriginal}' → '{fila.ConceptoEstandar}'?",
                    "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirma != DialogResult.Yes) return;
            }
            else
            {
                using var dlg = new BajaHomologacionDialog(fila, impacto);
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                conceptoDestino = dlg.ConceptoDestino;
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                HomologacionAdminService.AplicarBaja(fila, impacto, conceptoDestino);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar la homologación: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            HuboCambios = true;
            CargarDatos();
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Entrada al mantenimiento de conceptos
        private void btnConceptosEstandar_Click(object sender, EventArgs e)
        {
            using var frm = new GestionConceptosEstandarForm();
            frm.ShowDialog(this);
        }
    }
}
