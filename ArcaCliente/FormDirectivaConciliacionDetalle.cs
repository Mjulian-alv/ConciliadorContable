using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ArcaCliente.Models;

namespace ArcaCliente
{
    public partial class FormDirectivaConciliacionDetalle : Telerik.WinControls.UI.RadForm
    {
        private DirectivaConciliacion _directiva;

        /// <summary>Directiva resultante tras hacer clic en Guardar.</summary>
        public DirectivaConciliacion Directiva => _directiva;

        // Orden de todos los campos disponibles
        private static readonly CampoConciliacion[] _todosCampos =
        {
            CampoConciliacion.Cuit,
            CampoConciliacion.TipoComprobante,
            CampoConciliacion.PuntoVenta,
            CampoConciliacion.Numero,
            CampoConciliacion.Fecha
        };

        public FormDirectivaConciliacionDetalle(DirectivaConciliacion directiva = null)
        {
            InitializeComponent();
            Icon = AppIcons.Arca;

            _directiva = directiva != null
                ? new DirectivaConciliacion
                {
                    Id          = directiva.Id,
                    Descripcion = directiva.Descripcion,
                    Campos      = new List<CampoConciliacion>(directiva.Campos)
                }
                : new DirectivaConciliacion();

            CargarCampos();
        }

        // ?? Inicialización ???????????????????????????????????????????????????????????

        private void CargarCampos()
        {
            txtDescripcion.Text = _directiva.Descripcion ?? string.Empty;

            lstCampos.Items.Clear();

            // Primero los campos que ya están en la directiva (respetando su orden)
            var seleccionados = _directiva.Campos
                .Where(c => _todosCampos.Contains(c))
                .ToList();
            var noSeleccionados = _todosCampos
                .Where(c => !seleccionados.Contains(c))
                .ToList();

            foreach (var campo in seleccionados)
                lstCampos.Items.Add(new CampoItem(campo), true);

            foreach (var campo in noSeleccionados)
                lstCampos.Items.Add(new CampoItem(campo), false);
        }

        // ?? Botones de ordenamiento ??????????????????????????????????????????????????

        private void BtnSubir_Click(object sender, EventArgs e) =>
            MoverItem(lstCampos.SelectedIndex, -1);

        private void BtnBajar_Click(object sender, EventArgs e) =>
            MoverItem(lstCampos.SelectedIndex, +1);

        private void MoverItem(int idx, int delta)
        {
            int destino = idx + delta;
            if (idx < 0 || destino < 0 || destino >= lstCampos.Items.Count) return;

            var item       = (CampoItem)lstCampos.Items[idx];
            var checkState = lstCampos.GetItemCheckState(idx);

            lstCampos.Items.RemoveAt(idx);
            lstCampos.Items.Insert(destino, item);
            lstCampos.SetItemCheckState(destino, checkState);
            lstCampos.SelectedIndex = destino;
        }

        // ?? Guardar / Cancelar ???????????????????????????????????????????????????????

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (!Validar()) return;
            GuardarDatos();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool Validar()
        {
            if (string.IsNullOrWhiteSpace(txtDescripcion.Text))
            {
                MessageBox.Show("Ingresá una descripción para la directiva.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDescripcion.Focus();
                return false;
            }

            var camposSeleccionados = lstCampos.CheckedItems
                .Cast<CampoItem>()
                .ToList();

            if (camposSeleccionados.Count == 0)
            {
                MessageBox.Show("Seleccioná al menos un campo de matching.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void GuardarDatos()
        {
            _directiva.Descripcion = txtDescripcion.Text.Trim();
            _directiva.Campos      = lstCampos.Items
                .Cast<CampoItem>()
                .Where((_, i) => lstCampos.GetItemChecked(i))
                .Select(item => item.Campo)
                .ToList();
        }

        // ?? Clase auxiliar ???????????????????????????????????????????????????????

        private sealed class CampoItem
        {
            public CampoConciliacion Campo { get; }

            public CampoItem(CampoConciliacion campo) { Campo = campo; }

            public override string ToString() => DirectivaConciliacion.NombreCampo(Campo);
        }
    }
}
