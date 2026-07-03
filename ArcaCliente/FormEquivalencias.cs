using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using ArcaCliente.Services;
using Telerik.WinControls.UI;

namespace ArcaCliente
{
    public class EquivalenciaViewModel
    {
        public string CodigoAfip { get; set; }
        public string Descripcion { get; set; }
        public string TipoSistema { get; set; }
        public string Letra { get; set; }
    }

    public partial class FormEquivalencias : RadForm
    {
        private BindingList<EquivalenciaTipoComprobante> _lista;

        public FormEquivalencias()
        {
            InitializeComponent();
            Icon = AppIcons.Arca;

            CargarEquivalencias();
        }

        private void CargarEquivalencias()
        {
            var list = TipoComprobanteStorage.LoadAll().OrderBy(x => x.CodigoAfip).ToList();
            var descripciones = new BindingList<EquivalenciaViewModel>();
            
            foreach (var item in list)
            {
                var desc = TablaComprobanteDescripcionMapper.ObtenerDescripcion(item.CodigoAfip);
                descripciones.Add(new EquivalenciaViewModel
                {
                    CodigoAfip = item.CodigoAfip,
                    Descripcion = desc,
                    TipoSistema = item.TipoSistema,
                    Letra = item.Letra
                });
            }

            _lista = new BindingList<EquivalenciaTipoComprobante>(list);
            
            gridDatos.DataSource = null;
            gridDatos.DataSource = descripciones;
            
            gridDatos.Columns["CodigoAfip"].HeaderText = "Código AFIP";
            gridDatos.Columns["CodigoAfip"].ReadOnly = true;
            gridDatos.Columns["Descripcion"].HeaderText = "Descripción";
            gridDatos.Columns["Descripcion"].ReadOnly = true;
            gridDatos.Columns["TipoSistema"].HeaderText = "Tipo Sistema";
            gridDatos.Columns["Letra"].HeaderText = "Letra";

            gridDatos.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;
            gridDatos.MasterTemplate.AllowAddNewRow = true;
            gridDatos.MasterTemplate.AllowDeleteRow = true;
            gridDatos.MasterTemplate.AllowEditRow = true;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                var bindingList = (BindingList<EquivalenciaViewModel>)gridDatos.DataSource;
                _lista.Clear();
                foreach(var item in bindingList)
                    _lista.Add(new EquivalenciaTipoComprobante { CodigoAfip = item.CodigoAfip, TipoSistema = item.TipoSistema, Letra = item.Letra });

                TipoComprobanteStorage.SaveAll(_lista.ToList());
                TipoComprobanteMapper.Reload();
                MessageBox.Show("Equivalencias guardadas correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void pnlBotones_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}