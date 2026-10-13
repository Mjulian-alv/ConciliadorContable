using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;
using Telerik.WinControls.UI;

namespace AgrupadorConceptos
{
    // Fecha: 07/10/2026 - TAREA: 00073 - Linea: 2 - Mantener la selección con casillas de la conciliación interna, pero consultar sólo a pedido.
    public partial class ConceptosListado : RadForm
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-AR");
        private bool _ocupado;
        private bool _resultadoVigente;

        public ConceptosListado()
        {
            InitializeComponent();
            Icon = AppIcon.GetIcon();
            dtpDesde.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dtpHasta.Value = DateTime.Today;
            ConfigurarColumnas();
            Shown += CargarConceptos;
            dtpDesde.ValueChanged += (s, e) => InvalidarResultado();
            dtpHasta.ValueChanged += (s, e) => InvalidarResultado();
            clbConceptos.ItemCheck += (s, e) => InvalidarResultado();
            btnTodos.Click += (s, e) => MarcarConceptos(true);
            btnNinguno.Click += (s, e) => MarcarConceptos(false);
            btnConsultar.Click += Consultar;
            btnExportar.Click += Exportar;
        }

        // Fecha: 07/10/2026 - TAREA: 00073 - Linea: 2 - Columnas explícitas impiden que un ID agregado al modelo termine visible por autogeneración.
        private void ConfigurarColumnas()
        {
            dgvMovimientos.AutoGenerateColumns = false;
            dgvMovimientos.Columns.Add(new GridViewDateTimeColumn(nameof(MovimientoConsulta.Fecha))
            {
                HeaderText = "Fecha", FieldName = nameof(MovimientoConsulta.Fecha), Width = 105,
                FormatString = "{0:dd/MM/yyyy}", FormatInfo = Cultura
            });
            AgregarTexto(nameof(MovimientoConsulta.ConceptoOriginal), "Concepto original", 170);
            AgregarTexto(nameof(MovimientoConsulta.DescripcionOriginal), "Descripción original", 240);
            foreach (var campo in new[] { nameof(MovimientoConsulta.Debitos), nameof(MovimientoConsulta.Creditos) })
                dgvMovimientos.Columns.Add(new GridViewDecimalColumn(campo)
                {
                    FieldName = campo, HeaderText = campo == nameof(MovimientoConsulta.Debitos) ? "Débitos" : "Créditos",
                    Width = 125, FormatString = "{0:N2}", FormatInfo = Cultura,
                    TextAlignment = System.Drawing.ContentAlignment.MiddleRight
                });
            AgregarTexto(nameof(MovimientoConsulta.ConceptoEstandar), "Concepto estándar", 180);
            AgregarTexto(nameof(MovimientoConsulta.ConceptoFinal), "Concepto final", 180);
            AgregarTexto(nameof(MovimientoConsulta.CuentaFinal), "Cuenta final", 120);
            AgregarTexto(nameof(MovimientoConsulta.NombrePerfil), "Perfil", 180);
        }

        private void AgregarTexto(string campo, string titulo, int ancho) =>
            dgvMovimientos.Columns.Add(new GridViewTextBoxColumn(campo)
            { FieldName = campo, HeaderText = titulo, Width = ancho });

        // Fecha: 07/10/2026 - TAREA: 00073 - Linea: 2 - Abrir sólo carga el catálogo; los cambios de filtros no ejecutan consultas de movimientos.
        private async void CargarConceptos(object sender, EventArgs e)
        {
            EstablecerOcupado(true);
            lblEstado.Text = "Cargando conceptos finales...";
            try
            {
                var conceptos = await Task.Run(ConsultaMovimientosService.ObtenerConceptos);
                if (IsDisposed) return;
                clbConceptos.Items.AddRange(conceptos.Cast<object>().ToArray());
                lblEstado.Text = conceptos.Count == 0 ? "No hay conceptos finales en los movimientos importados."
                    : "Seleccione fechas y conceptos y presione Consultar. Se incluyen todos los perfiles.";
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                {
                    lblEstado.Text = "No se pudieron cargar los conceptos. Cierre y vuelva a abrir la consulta para reintentar.";
                    MostrarError(ex);
                }
            }
            finally { if (!IsDisposed) EstablecerOcupado(false); }
        }

        private void InvalidarResultado()
        {
            // Fecha: 07/10/2026 - TAREA: 00073 - Linea: 2 - Impedir exportar filas de filtros anteriores como si correspondieran a la selección actual.
            _resultadoVigente = false;
            dgvMovimientos.DataSource = null;
            btnExportar.Enabled = false;
            lblEstado.Text = "Filtros pendientes. Presione Consultar para obtener los movimientos.";
        }

        private void MarcarConceptos(bool marcar)
        {
            clbConceptos.BeginUpdate();
            try
            {
                for (int i = 0; i < clbConceptos.Items.Count; i++) clbConceptos.SetItemChecked(i, marcar);
            }
            finally { clbConceptos.EndUpdate(); }
        }

        private async void Consultar(object sender, EventArgs e)
        {
            if (_ocupado) return;
            var desde = dtpDesde.Value.Date;
            var hasta = dtpHasta.Value.Date;
            var conceptos = clbConceptos.CheckedItems.Cast<string>().ToArray();
            try { ConsultaMovimientosService.ValidarFiltros(desde, hasta, conceptos); }
            catch (ArgumentException ex)
            {
                MessageBox.Show(this, ex.Message, "Revisar filtros", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            InvalidarResultado();
            EstablecerOcupado(true);
            lblEstado.Text = "Consultando movimientos...";
            try
            {
                var movimientos = await Task.Run(() => ConsultaMovimientosService.Consultar(desde, hasta, conceptos));
                if (IsDisposed) return;
                dgvMovimientos.DataSource = movimientos;
                _resultadoVigente = true;
                // Fecha: 07/10/2026 - TAREA: 00075 - Linea: 1 - Calcular el saldo sobre los mismos totales visibles y conservar su signo para distinguir débitos de créditos excedentes.
                var totalDebitos = movimientos.Sum(m => m.Debitos ?? 0);
                var totalCreditos = movimientos.Sum(m => m.Creditos ?? 0);
                lblEstado.Text = movimientos.Count == 0 ? "No se encontraron movimientos para los filtros seleccionados."
                    : string.Format(Cultura, "{0:N0} movimientos | Débitos: {1:N2} | Créditos: {2:N2} | Diferencia (C - D): {3:N2}",
                        movimientos.Count, totalDebitos, totalCreditos, totalCreditos - totalDebitos);
            }
            catch (Exception ex)
            {
                if (!IsDisposed) { lblEstado.Text = "La consulta no pudo completarse. Puede volver a consultar."; MostrarError(ex); }
            }
            finally { if (!IsDisposed) EstablecerOcupado(false); }
        }

        // Fecha: 07/10/2026 - TAREA: 00073 - Linea: 3 - Capturar las filas ordenadas antes de exportar evita acceder a controles desde el hilo de trabajo.
        private async void Exportar(object sender, EventArgs e)
        {
            if (_ocupado || !_resultadoVigente) return;
            var movimientos = dgvMovimientos.ChildRows.Select(r => r.DataBoundItem).OfType<MovimientoConsulta>().ToList();
            if (movimientos.Count == 0) return;
            using var dialogo = new SaveFileDialog
            {
                Filter = "Libro de Excel (*.xlsx)|*.xlsx", DefaultExt = "xlsx", AddExtension = true,
                FileName = $"Movimientos_{dtpDesde.Value:yyyyMMdd}_{dtpHasta.Value:yyyyMMdd}.xlsx"
            };
            if (dialogo.ShowDialog(this) != DialogResult.OK) return;
            EstablecerOcupado(true);
            try
            {
                await Task.Run(() => ConsultaMovimientosService.Exportar(movimientos, dialogo.FileName));
                if (!IsDisposed) MessageBox.Show(this, "El archivo Excel se exportó correctamente.", "Exportación",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { if (!IsDisposed) MostrarError(ex); }
            finally { if (!IsDisposed) EstablecerOcupado(false); }
        }

        private void EstablecerOcupado(bool ocupado)
        {
            _ocupado = ocupado;
            pnlFiltros.Enabled = !ocupado;
            pnlConceptos.Enabled = !ocupado;
            dgvMovimientos.Enabled = !ocupado;
            btnExportar.Enabled = !ocupado && _resultadoVigente && dgvMovimientos.Rows.Count > 0;
            UseWaitCursor = ocupado;
        }

        private void MostrarError(Exception ex) => MessageBox.Show(this, ex.Message, "Consulta de movimientos",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
