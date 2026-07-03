using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using ArcaCliente.Models;
using ArcaCliente.Services;
using Telerik.WinControls.UI;

namespace ArcaCliente
{
    public partial class FormPerfilesOffline : Telerik.WinControls.UI.RadForm
    {
        private BindingList<PerfilOffline> _perfiles;

        /// <summary>Perfil seleccionado al hacer clic en "Seleccionar" o doble clic.</summary>
        public PerfilOffline PerfilSeleccionado { get; private set; }

        public FormPerfilesOffline()
        {
            InitializeComponent();
            Icon = AppIcons.Arca;
            CargarPerfiles();
        }

        private void CargarPerfiles()
        {
            _perfiles = new BindingList<PerfilOffline>(AppServices.PerfilesOffline);
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
                    "Nombre"        => true,
                    "TipoArchivo"   => true,
                    "TieneCabecera" => true,
                    _               => false
                };
            }

            SetHeader("Nombre",        "Nombre");
            SetHeader("TipoArchivo",   "Tipo de archivo");
            SetHeader("TieneCabecera", "Tiene cabecera");
            gridPerfiles.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;
        }

        private void SetHeader(string field, string header)
        {
            if (gridPerfiles.Columns[field] is GridViewDataColumn col)
                col.HeaderText = header;
        }

        // ?? CRUD ?????????????????????????????????????????????????????????????????

        private void BtnNuevo_Click(object sender, EventArgs e)
        {
            using var form = new FormPerfilOfflineDetalle();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                _perfiles.Add(form.Perfil);
                Guardar();
            }
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            if (gridPerfiles.CurrentRow?.DataBoundItem is not PerfilOffline perfil)
            {
                MessageBox.Show("Seleccioná un perfil para editar.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var form = new FormPerfilOfflineDetalle(perfil);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                var idx = _perfiles.IndexOf(perfil);
                if (idx >= 0) _perfiles[idx] = form.Perfil;
                Guardar();
                gridPerfiles.DataSource = null;
                gridPerfiles.DataSource = _perfiles;
                ConfigurarColumnas();
            }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (gridPerfiles.CurrentRow?.DataBoundItem is not PerfilOffline perfil)
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

        private void BtnDirectivas_Click(object sender, EventArgs e)
        {
            if (gridPerfiles.CurrentRow?.DataBoundItem is not PerfilOffline perfil)
            {
                MessageBox.Show("Seleccioná un perfil para configurar sus directivas.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (perfil.DirectivasConciliacion.Count == 0)
                perfil.DirectivasConciliacion.Add(DirectivaConciliacion.CrearPrimaria());

            using var form = new FormDirectivasConciliacion(perfil.Nombre, perfil.DirectivasConciliacion);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                perfil.DirectivasConciliacion = form.Directivas;
                Guardar();
            }
        }

        // ?? Selección ?????????????????????????????????????????????????????????????

        private void BtnSeleccionar_Click(object sender, EventArgs e)
        {
            if (gridPerfiles.CurrentRow?.DataBoundItem is not PerfilOffline perfil)
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

        // ?? Persistencia ??????????????????????????????????????????????????????????

        private void Guardar() =>
            AppServices.SavePerfilesOffline(new List<PerfilOffline>(_perfiles));
    }
}
