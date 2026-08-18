using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;
using Telerik.WinControls.UI;

namespace AgrupadorConceptos
{
    public partial class HomologacionMasivaForm : Form
    {
        private readonly List<MovimientoProcesado> _movimientos;
        private readonly PerfilBanco _perfil;

        public bool HuboCambios { get; private set; } = false;

        /// <param name="movimientos">
        /// La misma lista que muestra la grilla del llamador. Se modifica in situ: al volver,
        /// el llamador sólo tiene que refrescar la vista, no recargar de la base.
        /// </param>
        public HomologacionMasivaForm(List<MovimientoProcesado> movimientos, PerfilBanco perfil)
        {
            InitializeComponent();
            this.Icon = AppIcon.GetIcon();
            _movimientos = movimientos;
            _perfil = perfil;

            this.Load += HomologacionMasivaForm_Load;
            this.dgvPendientes.CellDoubleClick += DgvPendientes_CellDoubleClick;
        }

        private void HomologacionMasivaForm_Load(object sender, EventArgs e)
        {
            CargarDatos();
        }

        private void CargarDatos()
        {
            var pendientesAgrupados = _movimientos
                .Where(m => m.ConceptoEstandar == ConceptosBancarios.PendienteHomologar)
                .GroupBy(m => new { m.ConceptoOriginal, m.DescripcionOriginal })
                .Select(g => new
                {
                    ConceptoOriginal = g.Key.ConceptoOriginal,
                    DescripcionOriginal = g.Key.DescripcionOriginal,
                    Cantidad = g.Count(),
                    TotalDebitos = g.Sum(x => x.Debitos),
                    TotalCreditos = g.Sum(x => x.Creditos)
                })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            dgvPendientes.DataSource = null;
            dgvPendientes.DataSource = pendientesAgrupados;

            foreach (var col in dgvPendientes.Columns)
            {
                if (col.Name == "TotalDebitos" || col.Name == "TotalCreditos")
                {
                    col.FormatString = "{0:N2}";
                }
            }
        }

        private void DgvPendientes_CellDoubleClick(object sender, GridViewCellEventArgs e)
        {
            if (e.Row?.DataBoundItem != null)
            {
                dynamic data = e.Row.DataBoundItem;
                string valorParaHomologar = _perfil.EsCodigo ? (string)data.ConceptoOriginal : (string)data.DescripcionOriginal;
                if (string.IsNullOrEmpty(valorParaHomologar))
                    valorParaHomologar = (string)data.ConceptoOriginal;

                var frmHomologar = new HomologarForm(_perfil.Id, valorParaHomologar);
                frmHomologar.ShowDialog();

                if (frmHomologar.HomologacionExitosa)
                {
                    // Aplicamos y persistimos sobre la misma lista del llamador, así los
                    // pendientes ya resueltos desaparecen de esta ventana y la grilla de
                    // atrás queda al día sin tener que releerse.
                    HuboCambios = true;
                    SesionMovimientosService.RehomologarEnMemoria(_movimientos, _perfil);
                    CargarDatos();
                }
            }
        }
    }
}
