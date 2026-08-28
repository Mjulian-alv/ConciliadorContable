// Fecha: 28/08/2026 - TAREA: 00003 - Linea: 1 - Que hacer con los movimientos al dar de baja
using System;
using System.Windows.Forms;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;

namespace AgrupadorConceptos
{
    /// <summary>
    /// Pregunta qué hacer con los movimientos que la baja de una homologación deja sin
    /// regla. Sólo decide: no borra ni escribe nada — de eso se ocupa el llamador con
    /// el mismo objeto de impacto que se mostró acá.
    /// </summary>
    public partial class BajaHomologacionDialog : Form
    {
        /// <summary>
        /// Concepto al que hay que reasignar los movimientos afectados, o null para
        /// dejarlos pendientes de homologar.
        /// </summary>
        public string ConceptoDestino { get; private set; }

        public BajaHomologacionDialog(HomologacionListado regla, ImpactoHomologacion impacto)
        {
            InitializeComponent();
            this.Icon = AppIcon.GetIcon();

            lblResumen.Text =
                $"La homologación '{regla.ValorOriginal}' → '{regla.ConceptoEstandar}' del perfil " +
                $"'{regla.Banco}' resuelve hoy {impacto.Total} movimiento(s)." + Environment.NewLine +
                $"{impacto.Afectados.Count} quedan sin ninguna regla que los cubra. " +
                $"Los otros {impacto.CubiertosPorOtraRegla.Count} los sigue resolviendo otra " +
                $"homologación y no se tocan.";

            cmbConcepto.DataSource = HomologacionStorage.ObtenerConceptosEstandar();
            cmbConcepto.DisplayMember = "Nombre";
            cmbConcepto.ValueMember = "Id";
            cmbConcepto.SelectedIndex = -1;
            cmbConcepto.Enabled = false;

            rbPendientes.Checked = true;
            rbReasignar.CheckedChanged += (s, e) => cmbConcepto.Enabled = rbReasignar.Checked;
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            if (rbReasignar.Checked)
            {
                string concepto = cmbConcepto.Text.Trim();
                if (string.IsNullOrEmpty(concepto))
                {
                    MessageBox.Show("Elija o escriba el concepto al que se reasignan los movimientos.",
                        "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                ConceptoDestino = concepto;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
