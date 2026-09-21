// Fecha: 21/09/2026 - TAREA: 00041 - Linea: 2 - Lista de perfiles PyR: alta, edición, baja y selección
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using ArcaCliente.Models;
using ArcaCliente.Services;
using Telerik.WinControls.UI;

namespace ArcaCliente
{
    public partial class FormPerfilesPyR : Telerik.WinControls.UI.RadForm
    {
        private BindingList<PerfilOfflinePyR> _perfiles;

        /// <summary>Perfil elegido con "Usar perfil" o doble clic.</summary>
        public PerfilOfflinePyR PerfilSeleccionado { get; private set; }

        public FormPerfilesPyR()
        {
            ArcaStorageConfig.Initialize();
            InitializeComponent();
            Icon = AppIcons.Arca;
            CargarPerfiles();
        }

        private void CargarPerfiles()
        {
            _perfiles = new BindingList<PerfilOfflinePyR>(AppServices.PerfilesPyR);
            gridPerfiles.DataSource = _perfiles;
            ConfigurarColumnas();
        }

        private void ConfigurarColumnas()
        {
            if (gridPerfiles.Columns.Count == 0) return;

            foreach (GridViewColumn col in gridPerfiles.Columns)
            {
                col.IsVisible = col.FieldName switch
                {
                    nameof(PerfilOfflinePyR.Nombre)          => true,
                    nameof(PerfilOfflinePyR.CantidadCuentas) => true,
                    nameof(PerfilOfflinePyR.CarpetaArca)     => true,
                    _                                        => false
                };
            }

            SetHeader(nameof(PerfilOfflinePyR.Nombre),          "Nombre");
            SetHeader(nameof(PerfilOfflinePyR.CantidadCuentas), "Cuentas");
            SetHeader(nameof(PerfilOfflinePyR.CarpetaArca),     "Carpeta ARCA");
            gridPerfiles.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;
        }

        private void SetHeader(string field, string header)
        {
            if (gridPerfiles.Columns[field] is GridViewDataColumn col)
                col.HeaderText = header;
        }

        // ── CRUD ─────────────────────────────────────────────────────────────────

        private void BtnNuevo_Click(object sender, EventArgs e)
        {
            using var form = new FormPerfilPyRDetalle();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                _perfiles.Add(form.Perfil);
                Guardar();
            }
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            if (gridPerfiles.CurrentRow?.DataBoundItem is not PerfilOfflinePyR perfil)
            {
                MessageBox.Show("Seleccioná un perfil para editar.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var form = new FormPerfilPyRDetalle(perfil);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                Guardar();
                RefrescarGrilla();
            }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (gridPerfiles.CurrentRow?.DataBoundItem is not PerfilOfflinePyR perfil)
            {
                MessageBox.Show("Seleccioná un perfil para eliminar.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show($"¿Eliminar el perfil \"{perfil.Nombre}\"?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _perfiles.Remove(perfil);
                Guardar();
            }
        }

        // Fecha: 21/09/2026 - TAREA: 00041 - Linea: 6 - Editar las directivas del perfil seleccionado
        private void BtnDirectivas_Click(object sender, EventArgs e)
        {
            if (gridPerfiles.CurrentRow?.DataBoundItem is not PerfilOfflinePyR perfil)
            {
                MessageBox.Show("Seleccioná un perfil para configurar sus directivas.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var form = new FormDirectivasPyR(perfil.Nombre, perfil.DirectivasConciliacion);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                perfil.DirectivasConciliacion = form.Directivas;
                Guardar();
            }
        }

        // ── Selección ────────────────────────────────────────────────────────────

        private void BtnSeleccionar_Click(object sender, EventArgs e)
        {
            if (gridPerfiles.CurrentRow?.DataBoundItem is not PerfilOfflinePyR perfil)
            {
                MessageBox.Show("Seleccioná un perfil para usar.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            PerfilSeleccionado = perfil;
            DialogResult       = DialogResult.OK;
            Close();
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void GridPerfiles_CellDoubleClick(object sender, GridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                BtnSeleccionar_Click(sender, e);
        }

        // ── Persistencia ─────────────────────────────────────────────────────────

        private void RefrescarGrilla()
        {
            gridPerfiles.DataSource = null;
            gridPerfiles.DataSource = _perfiles;
            ConfigurarColumnas();
        }

        private void Guardar() =>
            AppServices.SavePerfilesPyR(new List<PerfilOfflinePyR>(_perfiles));
    }
}
