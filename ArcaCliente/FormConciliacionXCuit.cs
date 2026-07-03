using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ArcaCliente.Models;
using Telerik.WinControls.UI;

namespace ArcaCliente
{
    /// <summary>
    /// Muestra el resumen de conciliación agrupado por CUIT (totales con NC en negativo).
    /// Doble clic en una fila abre la conciliación detallada filtrada por ese CUIT.
    /// </summary>
    internal partial class FormConciliacionXCuit : Telerik.WinControls.UI.RadForm
    {
        private readonly List<ItemConciliacionXCuit> _items;
        private readonly Action<string>              _onDrillDown;

        public FormConciliacionXCuit(List<ItemConciliacionXCuit> items, Action<string> onDrillDown)
        {
            _items       = items       ?? throw new ArgumentNullException(nameof(items));
            _onDrillDown = onDrillDown ?? throw new ArgumentNullException(nameof(onDrillDown));

            InitializeComponent();
            Icon = AppIcons.Arca;
            CargarGrid();
        }

        // ?? Datos ?????????????????????????????????????????????????????????????

        private void CargarGrid()
        {
            _gridResumen.DataSource = null;
            _gridResumen.DataSource = _items;
            ConfigurarColumnas();

            int total  = _items.Count;
            int conDif = _items.Count(x => x.TieneDiferencia);
            int ok     = total - conDif;

            _lblResumen.Text      = $"{total} proveedores  ·  ? {conDif} con diferencia  ·  ? {ok} OK";
            _lblResumen.ForeColor = conDif > 0 ? Color.DarkOrange : Color.DarkGreen;
        }

        private void ConfigurarColumnas()
        {
            var columnas = new Dictionary<string, string>
            {
                { "EstadoTexto",     "Estado"       },
                { "CuitProveedor",   "CUIT"         },
                { "NombreProveedor", "Proveedor"    },
                { "CantARCA",        "Cant. ARCA"   },
                { "CantSistema",     "Cant. Sist."  },
                { "TotalARCA",       "Total ARCA"   },
                { "TotalSistema",    "Total Sist."  },
                { "Diferencia",      "Diferencia"   },
            };
            var visibles = new HashSet<string>(columnas.Keys);
            foreach (var col in _gridResumen.Columns.OfType<GridViewDataColumn>())
            {
                if (columnas.TryGetValue(col.FieldName, out var header))
                    col.HeaderText = header;
                col.IsVisible = visibles.Contains(col.FieldName);
            }
            _gridResumen.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;
        }

        // ?? Eventos ???????????????????????????????????????????????????????????

        private void GridResumen_RowFormatting(object sender, RowFormattingEventArgs e)
        {
            if (e.RowElement.RowInfo is not GridViewDataRowInfo dataRow ||
                dataRow.DataBoundItem is not ItemConciliacionXCuit item)
            {
                e.RowElement.DrawFill = false;
                return;
            }

            e.RowElement.DrawFill      = true;
            e.RowElement.GradientStyle = Telerik.WinControls.GradientStyles.Solid;
            e.RowElement.BackColor     = item.TieneDiferencia
                ? Color.FromArgb(255, 220, 150)
                : Color.FromArgb(200, 240, 200);
        }

        private void GridResumen_CellDoubleClick(object sender, GridViewCellEventArgs e)
        {
            if (e.Row is not GridViewDataRowInfo dataRow ||
                dataRow.DataBoundItem is not ItemConciliacionXCuit item)
                return;

            _onDrillDown(item.CuitProveedor);
            Close();
        }
    }
}
