using System.Collections.Generic;
using System.Windows.Forms;
using ArcaCliente.Services;
using Telerik.WinControls.UI;

namespace ArcaCliente
{
    /// <summary>
    /// Diálogo de selección de proveedor cuando hay dos o más registros con el mismo CUIT en Octosis.
    /// Se muestra desde el formulario de procesamiento Solo ARCA (PASO 5) cuando
    /// <see cref="OctosisProveedorService.BuscarPorCuitAsync"/> retorna
    /// <see cref="EstadoBusquedaProveedor.Multiples"/>.
    /// </summary>
    public partial class FormSeleccionProveedor : RadForm
    {
        /// <summary>
        /// El proveedor elegido por el usuario.
        /// <c>null</c> si cerró el diálogo con Cancelar.
        /// </summary>
        public ProveedorItem ProveedorSeleccionado { get; private set; }

        /// <param name="cuit">CUIT del emisor ARCA, para mostrarlo en el mensaje.</param>
        /// <param name="proveedores">Lista de proveedores con ese CUIT.</param>
        public FormSeleccionProveedor(string cuit, IReadOnlyList<ProveedorItem> proveedores)
        {
            InitializeComponent();

            lblMensaje.Text = $"Se encontraron {proveedores.Count} proveedores con CUIT {cuit}.\n" +
                               "Seleccione el proveedor al que pertenece el comprobante:";

            gridProveedores.MasterTemplate.AllowAddNewRow = false;
            gridProveedores.MasterTemplate.AllowDeleteRow = false;
            gridProveedores.MasterTemplate.AllowEditRow   = false;

            gridProveedores.DataSource = proveedores;

            // Encabezados de columna (la grilla crea las columnas al asignar DataSource)
            gridProveedores.MasterTemplate.AutoGenerateColumns = true;
            if (gridProveedores.Columns["Numero"] is GridViewDataColumn colNum)
                colNum.HeaderText = "Número";
            if (gridProveedores.Columns["Nombre"] is GridViewDataColumn colNom)
            {
                colNom.HeaderText = "Nombre / Razón social";
                colNom.Width      = 320;
            }

            // Doble clic en una fila equivale a pulsar Seleccionar
            gridProveedores.CellDoubleClick += (_, _) => Seleccionar();
        }

        private void btnSeleccionar_Click(object sender, System.EventArgs e) => Seleccionar();

        private void btnCancelar_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void Seleccionar()
        {
            if (gridProveedores.CurrentRow?.DataBoundItem is ProveedorItem item)
            {
                ProveedorSeleccionado = item;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show(
                    "Seleccione un proveedor de la lista.",
                    "Selección requerida",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }
}
