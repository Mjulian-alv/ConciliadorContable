using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using ArcaCliente.Models;
using ArcaCliente.Services;
using Telerik.WinControls.UI;

namespace ArcaCliente
{
    public partial class FormPerfiles : Telerik.WinControls.UI.RadForm
    {
        private BindingList<PerfilFiscal> _perfiles;

        public FormPerfiles()
        {
            InitializeComponent();
            Icon = AppIcons.Arca;
            CargarPerfiles();
        }

        private void CargarPerfiles()
        {
            _perfiles = new BindingList<PerfilFiscal>(PerfilStorage.Load());
            gridPerfiles.DataSource = _perfiles;
            ConfigurarColumnas();
        }

        private void ConfigurarColumnas()
        {
            if (gridPerfiles.Columns.Count == 0) return;

            // Ocultar columnas de configuración avanzada
            foreach (GridViewColumn col in gridPerfiles.Columns)
            {
                col.IsVisible = col.FieldName switch
                {
                    "Nombre" => true,
                    "Username" => true,
                    "Cuit" => true,
                    "IntegracionHabilitada" => true,
                    "Sistema" => true,
                    _ => false
                };
            }

            SetHeader("Nombre", "Nombre / Empresa");
            SetHeader("Username", "CUIL");
            SetHeader("Cuit", "CUIT Representada");
            SetHeader("IntegracionHabilitada", "Integración");
            SetHeader("Sistema", "Sistema");
        }

        private void SetHeader(string field, string header)
        {
            if (gridPerfiles.Columns[field] is GridViewDataColumn col)
                col.HeaderText = header;
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            using var form = new FormPerfilDetalle();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                _perfiles.Add(form.Perfil);
            }
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            if (gridPerfiles.CurrentRow?.DataBoundItem is not PerfilFiscal perfil) 
            {
                MessageBox.Show("Seleccioná un perfil para editar.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var form = new FormPerfilDetalle(perfil);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                // Refrescar el grid
                gridPerfiles.DataSource = null;
                gridPerfiles.DataSource = _perfiles;
                ConfigurarColumnas();
            }
        }

        private void btnDirectivas_Click(object sender, EventArgs e)
        {
            if (gridPerfiles.CurrentRow?.DataBoundItem is not PerfilFiscal perfil)
            {
                MessageBox.Show("Seleccioná un perfil para configurar sus directivas.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (perfil.DirectivasConciliacion == null)
                perfil.DirectivasConciliacion = new List<DirectivaConciliacion>();
            if (perfil.DirectivasConciliacion.Count == 0)
                perfil.DirectivasConciliacion.Add(DirectivaConciliacion.CrearPrimaria());

            using var form = new FormDirectivasConciliacion(perfil.Nombre, perfil.DirectivasConciliacion);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                perfil.DirectivasConciliacion = form.Directivas;
                AppServices.SavePerfiles(new List<PerfilFiscal>(_perfiles));
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (gridPerfiles.CurrentRow?.DataBoundItem is not PerfilFiscal perfil) 
            {
                MessageBox.Show("Seleccioná un perfil para eliminar.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show($"¿Eliminar el perfil \"{perfil.Nombre}\"?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                _perfiles.Remove(perfil);
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            AppServices.SavePerfiles(new List<PerfilFiscal>(_perfiles));
            MessageBox.Show("Perfiles guardados correctamente.", "Éxito",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Cerrar sin guardar los cambios?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void btnEquivalencias_Click(object sender, EventArgs e)
        {
            using var form = new FormEquivalencias();
            form.ShowDialog(this);
        }

        private void gridPerfiles_CellDoubleClick(object sender, GridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                btnEditar_Click(sender, e);
        }
    }
}
