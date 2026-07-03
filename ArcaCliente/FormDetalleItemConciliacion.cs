using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ArcaCliente.Models;

namespace ArcaCliente
{
    /// <summary>
    /// Muestra el detalle completo de un <see cref="ItemConciliacion"/>:
    /// datos del comprobante, resultado de la conciliación y (para Solo ARCA)
    /// el desglose de importes del CSV de ARCA.
    /// </summary>
    internal partial class FormDetalleItemConciliacion : Telerik.WinControls.UI.RadForm
    {
        private readonly ItemConciliacion _item;

        public FormDetalleItemConciliacion(ItemConciliacion item)
        {
            _item = item ?? throw new ArgumentNullException(nameof(item));
            InitializeComponent();
            Icon = AppIcons.Arca;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            PoblarDatos();
        }

        // ?? Datos ?????????????????????????????????????????????????????????????

        private void PoblarDatos()
        {
            // Estado en el header
            lblEstadoItem.Text = _item.EstadoTexto;
            lblEstadoItem.ForeColor = _item.Estado switch
            {
                EstadoConciliacion.Conciliado        => Color.FromArgb(0, 130, 80),
                EstadoConciliacion.DiferenciaImporte => Color.FromArgb(160, 120, 0),
                EstadoConciliacion.SoloARCA          => Color.FromArgb(160, 60, 60),
                EstadoConciliacion.SoloSistema       => Color.FromArgb(30, 100, 160),
                _                                    => Color.Black
            };

            Text = string.IsNullOrWhiteSpace(_item.DescripcionTipoComprobante)
                ? $"Detalle — {_item.TipoComprobante} {_item.PuntoVenta}-{_item.Numero}"
                : $"Detalle — {_item.DescripcionTipoComprobante} {_item.PuntoVenta}-{_item.Numero}";

            int y = 0;

            // ?? Sección: Comprobante ????????????????????????????????????????
            string tipoDesc = string.IsNullOrWhiteSpace(_item.DescripcionTipoComprobante)
                ? _item.TipoComprobante
                : $"{_item.TipoComprobante}  —  {_item.DescripcionTipoComprobante}";

            y = AgregarGrupo(y, "Comprobante", new[]
            {
                ("Fecha",          _item.Fecha),
                ("Tipo",           tipoDesc),
                ("Punto de venta", _item.PuntoVenta),
                ("Número",         _item.Numero),
                ("CUIT emisor",    _item.CuitProveedor),
                ("Proveedor",      _item.NombreProveedor),
            });

            // ?? Sección: Conciliación ??????????????????????????????????????
            var filasConc = new List<(string, string)>
            {
                ("Total ARCA",    Dash(_item.TotalARCA)),
                ("Total sistema", Dash(_item.TotalSistema)),
                ("Diferencia",    Dash(_item.Diferencia)),
            };

            if (_item.DirectivaAplicada > 0)
            {
                filasConc.Add(("Directiva", $"{_item.DirectivaAplicada}  —  {_item.DescripcionDirectiva}"));
                if (!string.IsNullOrWhiteSpace(_item.DetalleMatcheo))
                    filasConc.Add(("Clave de matcheo", _item.DetalleMatcheo));
            }

            if (!string.IsNullOrWhiteSpace(_item.FalloDirectivas))
                filasConc.Add(("Directivas sin match", _item.FalloDirectivas));

            y = AgregarGrupo(y, "Conciliación", filasConc.ToArray());

            // ?? Sección: Detalle ARCA (solo para ítems SoloARCA) ??????????
            if (_item.SourceArca is { } src)
            {
                var filasArca = new List<(string, string)>
                {
                    ("CAE / CAI", Dash(src.CodAutorizacion)),
                };

                if (_item.Moneda != null)
                {
                    string descMoneda = string.IsNullOrWhiteSpace(_item.Moneda.DescripcionMoneda)
                        ? _item.Moneda.CodigoMoneda
                        : $"{_item.Moneda.CodigoMoneda}  —  {_item.Moneda.DescripcionMoneda}";
                    filasArca.Add(("Moneda", descMoneda));

                    if (_item.Moneda.EsMonedaExtranjera)
                        filasArca.Add(("Tipo de cambio",
                            _item.Moneda.TipoCambioConfirmado
                                ? _item.Moneda.TipoCambio.ToString("F4")
                                : "Pendiente de confirmar"));
                }

                void AgregarSiHayValor(string label, string valor)
                {
                    if (!string.IsNullOrWhiteSpace(valor) && valor != "0" && valor != "0,00")
                        filasArca.Add((label, valor));
                }

                AgregarSiHayValor("Neto gravado (IVA 0%)", src.ImpNetoGravadoIVA0);
                AgregarSiHayValor("Neto gravado (IVA 2,5%)", src.ImpNetoGravadoIVA25);
                AgregarSiHayValor("Neto gravado (IVA 5%)",   src.ImpNetoGravadoIVA5);
                AgregarSiHayValor("Neto gravado (IVA 10,5%)", src.ImpNetoGravadoIVA105);
                AgregarSiHayValor("Neto gravado (IVA 21%)",  src.ImpNetoGravadoIVA21);
                AgregarSiHayValor("Neto gravado (IVA 27%)",  src.ImpNetoGravadoIVA27);
                AgregarSiHayValor("Neto gravado total",      src.ImpNetoGravadoTotal);
                AgregarSiHayValor("IVA 2,5%",               src.Iva25);
                AgregarSiHayValor("IVA 5%",                 src.Iva5);
                AgregarSiHayValor("IVA 10,5%",              src.Iva105);
                AgregarSiHayValor("IVA 21%",                src.Iva21);
                AgregarSiHayValor("IVA 27%",                src.Iva27);
                AgregarSiHayValor("Total IVA",              src.TotalIVA);
                AgregarSiHayValor("No gravado",             src.ImpNetoNoGravado);
                AgregarSiHayValor("Op. exentas",            src.ImpOpExentas);
                AgregarSiHayValor("Otros tributos",         src.OtrosTributos);

                filasArca.Add(("Total", Dash(src.ImpTotal)));

                y = AgregarGrupo(y, "Detalle ARCA", filasArca.ToArray());
            }
        }

        // ?? Construcción de grupos ?????????????????????????????????????????

        private int AgregarGrupo(int y, string titulo, (string Label, string Valor)[] filas)
        {
            const int LabelWidth = 160;
            const int RowHeight  = 28;
            const int PadTop     = 20;
            const int PadBottom  = 10;

            int groupHeight = PadTop + filas.Length * RowHeight + PadBottom;

            var grp = new GroupBox
            {
                Text     = titulo,
                Location = new Point(0, y),
                Width    = pnlContenido.ClientSize.Width - 4,
                Height   = groupHeight,
                Font     = new Font("Segoe UI", 9f, FontStyle.Bold),
                Anchor   = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
            };
            pnlContenido.Resize += (s, e) => grp.Width = pnlContenido.ClientSize.Width - 4;

            var valueFont = new Font("Segoe UI", 9f);
            int rowY = PadTop;

            foreach (var (label, valor) in filas)
            {
                var lbl = new Label
                {
                    Text      = label + ":",
                    Location  = new Point(8, rowY),
                    Size      = new Size(LabelWidth, RowHeight),
                    TextAlign = ContentAlignment.MiddleRight,
                    Font      = valueFont,
                };

                var val = new Label
                {
                    Text      = valor ?? string.Empty,
                    Location  = new Point(8 + LabelWidth + 6, rowY),
                    Size      = new Size(grp.Width - LabelWidth - 30, RowHeight),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font      = valueFont,
                    AutoSize  = false,
                };
                grp.Resize += (s, e) => val.Width = grp.Width - LabelWidth - 30;

                grp.Controls.Add(lbl);
                grp.Controls.Add(val);
                rowY += RowHeight;
            }

            pnlContenido.Controls.Add(grp);
            return y + groupHeight + 8;
        }

        // ?? Eventos ???????????????????????????????????????????????????????????

        private void BtnCerrar_Click(object sender, EventArgs e) => Close();

        // ?? Helpers ???????????????????????????????????????????????????????????

        private static string Dash(string valor) =>
            string.IsNullOrWhiteSpace(valor) ? "—" : valor;
    }
}
