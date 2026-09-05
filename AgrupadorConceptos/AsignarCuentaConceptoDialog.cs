// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 3 - Asignar/cambiar/quitar la cuenta de un concepto
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;

namespace AgrupadorConceptos
{
    public partial class AsignarCuentaConceptoDialog : Form
    {
        private const int SinAsignar = 0;

        /// <summary>Cuenta elegida, o null si quedó "(sin asignar)".</summary>
        public int? IdCuentaContable { get; private set; }

        public AsignarCuentaConceptoDialog(ConceptoEstandarListado concepto)
        {
            InitializeComponent();
            Icon = AppIcon.GetIcon();

            lblConcepto.Text = $"Concepto: {concepto.Nombre}";

            var opciones = new List<CuentaContable>
            {
                new CuentaContable { Id = SinAsignar, Descripcion = "(sin asignar)" }
            };
            opciones.AddRange(CuentaContableStorage.ObtenerTodas());

            cmbCuenta.DataSource = opciones;
            cmbCuenta.DisplayMember = "DisplayName";
            cmbCuenta.ValueMember = "Id";
            cmbCuenta.SelectedValue = concepto.IdCuentaContable ?? SinAsignar;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            int seleccion = (int)cmbCuenta.SelectedValue;
            IdCuentaContable = seleccion == SinAsignar ? (int?)null : seleccion;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
