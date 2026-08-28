using System;
using System.Linq;
using System.Windows.Forms;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Data;

namespace AgrupadorConceptos
{
    public partial class HomologarForm : Form
    {
        private int _idPerfilBanco;
        private string _valorOriginal;

        public bool HomologacionExitosa { get; private set; } = false;
        public string sConcepto { get; private set; } = "";

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 2 - Valor con el que se guardo
        /// <summary>Valor original efectivamente usado (el usuario pudo acortarlo).</summary>
        public string sValorOriginal { get; private set; } = "";

        private bool _bloquearValorOriginal;

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 2 - Modo edicion: la clave no se toca
        /// <summary>
        /// Deja el valor del banco en sólo lectura. La edición desde la gestión no puede
        /// cambiar la clave: Guardar pisa la homologación previa borrando por
        /// (IdPerfilBanco, ValorOriginal), así que cambiarla dejaría viva la regla vieja.
        /// Para cambiar la clave hay que eliminar y dar de alta de nuevo.
        /// </summary>
        public bool BloquearValorOriginal
        {
            get => _bloquearValorOriginal;
            set
            {
                _bloquearValorOriginal = value;
                txtOriginal.ReadOnly = value;
                lblAyuda.Visible = !value;
            }
        }

        // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 2 - Devolver el concepto sin persistir
        /// <summary>
        /// El form no guarda: sólo devuelve el concepto elegido en <see cref="sConcepto"/>.
        /// Lo usa la edición, que necesita guardar la regla y arrastrar los movimientos en
        /// una sola transacción y no puede dejar que este form commitee por su cuenta.
        /// </summary>
        public bool SoloSeleccionar { get; set; }

        public HomologarForm(int idPerfilBanco, string valorOriginal)
        {
            InitializeComponent();
            this.Icon = AppIcon.GetIcon();
            _idPerfilBanco = idPerfilBanco;
            _valorOriginal = valorOriginal;
            
            txtOriginal.Text = _valorOriginal;
            CargarConceptosEstandar();
        }

        private void CargarConceptosEstandar()
        {
            cmbEstandar.DataSource = HomologacionStorage.ObtenerConceptosEstandar();
            cmbEstandar.DisplayMember = "Nombre";
            cmbEstandar.ValueMember = "Id";
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            string conceptoEstandarTexto = cmbEstandar.Text.Trim();

            if (string.IsNullOrEmpty(conceptoEstandarTexto))
            {
                MessageBox.Show("Debe ingresar o seleccionar un Concepto Estándar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Si el valor es texto largo, el usuario pudo haber editado txtOriginal
                // para dejar solo la palabra clave que se va a buscar por substring.
                string valorClave = txtOriginal.Text.Trim();

                // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 2 - Validar el valor del banco
                // El boton "Nueva" de la gestion abre este form en blanco. Un ValorOriginal
                // vacio entraria a la base y, al buscarse por substring, haria match con todo.
                if (string.IsNullOrEmpty(valorClave))
                {
                    MessageBox.Show("Debe indicar el concepto o la palabra clave del banco.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Fecha: 28/08/2026 - TAREA: 00003 - Linea: 2 - En modo seleccion no persiste
                if (!SoloSeleccionar)
                    HomologacionStorage.Guardar(_idPerfilBanco, valorClave, conceptoEstandarTexto);

                HomologacionExitosa = true;
                sConcepto = conceptoEstandarTexto;
                sValorOriginal = valorClave;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar homologación: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}