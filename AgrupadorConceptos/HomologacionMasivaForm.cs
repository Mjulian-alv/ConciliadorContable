using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Services;
using Telerik.WinControls.UI;

namespace AgrupadorConceptos
{
    public partial class HomologacionMasivaForm : Form
    {
        private readonly List<MovimientoProcesado> _movimientos;
        private readonly int _idPerfil;
        private readonly bool _esCodigo;

        public bool HuboCambios { get; private set; } = false;

        public HomologacionMasivaForm(List<MovimientoProcesado> movimientos, int idPerfil, bool esCodigo)
        {
            InitializeComponent();
            this.Icon = AppIcon.GetIcon();
            _movimientos = movimientos;
            _idPerfil = idPerfil;
            _esCodigo = esCodigo;

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
                string valorParaHomologar = _esCodigo ? (string)data.ConceptoOriginal : (string)data.DescripcionOriginal;
                if (string.IsNullOrEmpty(valorParaHomologar))
                    valorParaHomologar = (string)data.ConceptoOriginal;

                var frmHomologar = new HomologarForm(_idPerfil, valorParaHomologar);
                frmHomologar.ShowDialog();

                if (frmHomologar.HomologacionExitosa)
                {
                    // Aplicar el cambio a la lista en memoria para poder refrescar la vista
                    HuboCambios = true;
                    AplicarMapeos();
                    CargarDatos();
                }
            }
        }

        private void AplicarMapeos()
        {
            var dicHomologacion = HomologacionStorage.ObtenerDiccionario(_idPerfil);

            foreach (var mov in _movimientos)
            {
                if (mov.ConceptoEstandar == ConceptosBancarios.PendienteHomologar)
                    HomologacionMatcher.AplicarA(mov, _esCodigo, dicHomologacion);
            }
        }
    }
}