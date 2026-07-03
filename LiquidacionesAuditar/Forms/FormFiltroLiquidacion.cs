using System;
using System.Globalization;
using System.Windows.Forms;
using LiquidacionesAuditar.Models;

namespace LiquidacionesAuditar
{
    /// <summary>
    /// Formulario chico para definir el rango de filtro de liquidación.
    /// Genera dinámicamente los controles según el TipoFiltro de la marca.
    /// </summary>
    public partial class FormFiltroLiquidacion : Form
    {
        private readonly TipoFiltro _tipo;

        // Controles generados dinámicamente
        private TextBox _txtTexto;
        private TextBox _txtNumDesde;
        private TextBox _txtNumHasta;
        private DateTimePicker _dtpDesde;
        private DateTimePicker _dtpHasta;
        private CheckBox _chkDesdeActivo;
        private CheckBox _chkHastaActivo;

        /// <summary>Filtro resultante al cerrar con OK. Null si se limpió o canceló.</summary>
        public FiltroLiquidacion Resultado { get; private set; }

        public FormFiltroLiquidacion(string columna, TipoFiltro tipo, FiltroLiquidacion filtroActual = null)
        {
            InitializeComponent();

            _tipo = tipo;
            _lblColumnaVal.Text = columna;
            _lblTipoVal.Text = tipo.ToString().ToUpper();

            CrearCamposDinamicos(tipo, filtroActual);
        }

        private void CrearCamposDinamicos(TipoFiltro tipo, FiltroLiquidacion filtroActual)
        {
            _pnlCampos.Controls.Clear();

            switch (tipo)
            {
                case TipoFiltro.String:
                case TipoFiltro.Int when tipo == TipoFiltro.String:
                    CrearCampoTexto(filtroActual?.TextoContiene);
                    break;

                case TipoFiltro.Int:
                case TipoFiltro.Decimal:
                    CrearCamposNumericos(filtroActual);
                    break;

                case TipoFiltro.Date:
                    CrearCamposFecha(filtroActual);
                    break;

                default:
                    CrearCampoTexto(filtroActual?.TextoContiene);
                    break;
            }
        }

        private void CrearCampoTexto(string valorActual)
        {
            // Ajustar alto del formulario para campo simple
            ClientSize = new System.Drawing.Size(380, 175);

            var lbl = new Label
            {
                Text = "Contiene el texto:",
                AutoSize = true,
                Location = new System.Drawing.Point(0, 10)
            };
            _txtTexto = new TextBox
            {
                Location = new System.Drawing.Point(0, 30),
                Width = _pnlCampos.Width - 4,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Text = valorActual ?? ""
            };
            _pnlCampos.Controls.Add(lbl);
            _pnlCampos.Controls.Add(_txtTexto);
            _txtTexto.Focus();
        }

        private void CrearCamposNumericos(FiltroLiquidacion filtroActual)
        {
            ClientSize = new System.Drawing.Size(380, 210);

            var lblDesde = new Label { Text = "Desde:", AutoSize = true, Location = new System.Drawing.Point(0, 8) };
            _txtNumDesde = new TextBox
            {
                Location = new System.Drawing.Point(60, 5),
                Width = 120,
                Text = filtroActual?.NumDesde?.ToString(CultureInfo.InvariantCulture) ?? ""
            };

            var lblHasta = new Label { Text = "Hasta:", AutoSize = true, Location = new System.Drawing.Point(0, 42) };
            _txtNumHasta = new TextBox
            {
                Location = new System.Drawing.Point(60, 39),
                Width = 120,
                Text = filtroActual?.NumHasta?.ToString(CultureInfo.InvariantCulture) ?? ""
            };

            _pnlCampos.Controls.AddRange(new Control[] { lblDesde, _txtNumDesde, lblHasta, _txtNumHasta });
            _txtNumDesde.Focus();
        }

        private void CrearCamposFecha(FiltroLiquidacion filtroActual)
        {
            ClientSize = new System.Drawing.Size(380, 230);

            _chkDesdeActivo = new CheckBox
            {
                Text = "Desde:",
                AutoSize = true,
                Location = new System.Drawing.Point(0, 8),
                Checked = filtroActual?.FechaDesde.HasValue ?? false
            };
            _dtpDesde = new DateTimePicker
            {
                Location = new System.Drawing.Point(80, 5),
                Width = 160,
                Format = DateTimePickerFormat.Short,
                Value = filtroActual?.FechaDesde ?? DateTime.Today,
                Enabled = _chkDesdeActivo.Checked
            };
            _chkDesdeActivo.CheckedChanged += (s, e) => _dtpDesde.Enabled = _chkDesdeActivo.Checked;

            _chkHastaActivo = new CheckBox
            {
                Text = "Hasta:",
                AutoSize = true,
                Location = new System.Drawing.Point(0, 42),
                Checked = filtroActual?.FechaHasta.HasValue ?? false
            };
            _dtpHasta = new DateTimePicker
            {
                Location = new System.Drawing.Point(80, 39),
                Width = 160,
                Format = DateTimePickerFormat.Short,
                Value = filtroActual?.FechaHasta ?? DateTime.Today,
                Enabled = _chkHastaActivo.Checked
            };
            _chkHastaActivo.CheckedChanged += (s, e) => _dtpHasta.Enabled = _chkHastaActivo.Checked;

            _pnlCampos.Controls.AddRange(new Control[]
            {
                _chkDesdeActivo, _dtpDesde, _chkHastaActivo, _dtpHasta
            });
        }

        private void BtnAceptar_Click(object? sender, EventArgs e)
        {
            if (!TryBuildFiltro(out var filtro, out var error))
            {
                MessageBox.Show(error, "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }
            Resultado = filtro;
        }

        private void BtnLimpiar_Click(object? sender, EventArgs e)
        {
            Resultado = null;
            DialogResult = DialogResult.OK;
        }

        private bool TryBuildFiltro(out FiltroLiquidacion filtro, out string error)
        {
            filtro = null;
            error = null;

            var columna = _lblColumnaVal.Text;
            filtro = new FiltroLiquidacion { Columna = columna, Tipo = _tipo };

            switch (_tipo)
            {
                case TipoFiltro.String:
                    filtro.TextoContiene = _txtTexto?.Text?.Trim() ?? "";
                    break;

                case TipoFiltro.Int:
                case TipoFiltro.Decimal:
                    if (!string.IsNullOrWhiteSpace(_txtNumDesde?.Text))
                    {
                        if (!decimal.TryParse(_txtNumDesde.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var nd))
                        { error = $"El valor 'Desde' no es un número válido: {_txtNumDesde.Text}"; return false; }
                        filtro.NumDesde = nd;
                    }
                    if (!string.IsNullOrWhiteSpace(_txtNumHasta?.Text))
                    {
                        if (!decimal.TryParse(_txtNumHasta.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var nh))
                        { error = $"El valor 'Hasta' no es un número válido: {_txtNumHasta.Text}"; return false; }
                        filtro.NumHasta = nh;
                    }
                    if (filtro.NumDesde.HasValue && filtro.NumHasta.HasValue && filtro.NumDesde > filtro.NumHasta)
                    { error = "El valor 'Desde' no puede ser mayor que 'Hasta'."; return false; }
                    break;

                case TipoFiltro.Date:
                    if (_chkDesdeActivo?.Checked == true)
                        filtro.FechaDesde = _dtpDesde.Value.Date;
                    if (_chkHastaActivo?.Checked == true)
                        filtro.FechaHasta = _dtpHasta.Value.Date;
                    if (filtro.FechaDesde.HasValue && filtro.FechaHasta.HasValue && filtro.FechaDesde > filtro.FechaHasta)
                    { error = "La fecha 'Desde' no puede ser posterior a 'Hasta'."; return false; }
                    break;
            }

            return true;
        }
    }
}
