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

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Cuenta elegida al homologar
        /// <summary>Cuenta contable con la que se guardó, o null si quedó sin asignar.</summary>
        public int? sIdCuentaContable { get; private set; }

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
            CargarCuentasContables();

            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Precargar la cuenta si el concepto ya existe
            // Sin esto, tipear/seleccionar un concepto ya homologado en otro perfil y guardar
            // borraria en silencio la cuenta que ya tenia asignada.
            cmbEstandar.TextChanged += (s, e) => PrecargarCuentaDelConcepto();
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Cache de conceptos para precargar la cuenta
        // PrecargarCuentaDelConcepto necesita buscar el concepto tipeado/elegido sin ir a la base
        // en cada tecla: se guarda la lista ya cargada por CargarConceptosEstandar.
        private System.Collections.Generic.List<ConceptoEstandar> _conceptosCache = new();

        private void CargarConceptosEstandar()
        {
            _conceptosCache = HomologacionStorage.ObtenerConceptosEstandar();
            cmbEstandar.DataSource = _conceptosCache;
            cmbEstandar.DisplayMember = "Nombre";
            cmbEstandar.ValueMember = "Id";
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Opcion "sin asignar" en el combo de cuentas
        private const int SinCuenta = 0;

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Carga el combo de cuentas contables
        private void CargarCuentasContables()
        {
            var opciones = new System.Collections.Generic.List<Models.CuentaContable>
            {
                new Models.CuentaContable { Id = SinCuenta, Descripcion = "(sin asignar)" }
            };
            opciones.AddRange(Data.CuentaContableStorage.ObtenerTodas());

            cmbCuenta.DataSource = opciones;
            cmbCuenta.DisplayMember = "DisplayName";
            cmbCuenta.ValueMember = "Id";
        }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Precarga la cuenta del concepto ya existente
        private void PrecargarCuentaDelConcepto()
        {
            var existente = _conceptosCache.Find(c =>
                string.Equals(c.Nombre, cmbEstandar.Text.Trim(), StringComparison.OrdinalIgnoreCase));

            cmbCuenta.SelectedValue = existente?.IdCuentaContable ?? SinCuenta;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            string conceptoEstandarTexto = cmbEstandar.Text.Trim();

            if (string.IsNullOrEmpty(conceptoEstandarTexto))
            {
                MessageBox.Show("Debe ingresar o seleccionar un Concepto Estándar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string valorClave = txtOriginal.Text.Trim();

            if (string.IsNullOrEmpty(valorClave))
            {
                MessageBox.Show("Debe indicar el concepto o la palabra clave del banco.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Cuenta elegida (puede quedar vacia)
            int seleccionCuenta = (int)(cmbCuenta.SelectedValue ?? SinCuenta);
            sIdCuentaContable = seleccionCuenta == SinCuenta ? (int?)null : seleccionCuenta;

            try
            {
                if (!SoloSeleccionar)
                {
                    int idConcepto = HomologacionStorage.Guardar(_idPerfilBanco, valorClave, conceptoEstandarTexto);
                    // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Persiste la cuenta elegida junto con el alta
                    HomologacionStorage.ActualizarCuentaConcepto(idConcepto, sIdCuentaContable);
                }

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