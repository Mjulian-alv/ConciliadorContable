using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ArcaCliente.Models;
using ArcaCliente.Services;
using Telerik.WinControls.UI;

namespace ArcaCliente
{
    /// <summary>
    /// Formulario de procesamiento uno-a-uno de comprobantes "Solo ARCA".
    /// Se abre desde <see cref="FormComprobantes"/> cuando hay ítems con
    /// <see cref="EstadoConciliacion.SoloARCA"/> tras la conciliación.
    /// <para>
    /// PASO 5 del roadmap (§12 del análisis).
    /// </para>
    /// Flujo interno por comprobante:
    /// <list type="number">
    ///   <item>Buscar proveedor por CUIT (<see cref="OctosisProveedorService"/>). §7.6</item>
    ///   <item>Desagregar impuestos (<see cref="TaxDisaggregationService"/>). §7.7 / PASO 6</item>
    ///   <item>Validar tipo de cambio si el comprobante es en moneda extranjera. PASO 3</item>
    ///   <item>Construir <see cref="OctosisDocumento"/> y llamar <see cref="OctosisDocumentoService.AltaAsync"/>. PASO 3b</item>
    /// </list>
    /// </summary>
    public partial class FormProcesarSoloArca : RadForm
    {
        // ?? Servicios ?????????????????????????????????????????????????????????
        private readonly OctosisProveedorService    _proveedorService;
        private readonly OctosisImpuestosService    _impuestosService;
        private readonly TaxDisaggregationService   _taxService;
        private readonly OctosisDocumentoService    _docService;
        private readonly OctosisImputacionesService _imputacionesService;

        // ?? Estado ????????????????????????????????????????????????????????????
        private readonly List<ItemConciliacion>     _items;
        private readonly PerfilFiscal               _perfil;
        private int            _currentIndex;
        private ProveedorItem? _proveedorResuelto;
        private int            _altasOk;
        private int            _omitidos;
        private int            _errores;
        private List<OctosisTasa> _todasLasTasas = new();

        /// <summary>Número de documentos dados de alta correctamente al cerrar el formulario.</summary>
        public int AltasRealizadas => _altasOk;

        /// <param name="items">
        /// Lista de <see cref="ItemConciliacion"/> producida por la conciliación.
        /// Solo se procesan los que tienen <see cref="EstadoConciliacion.SoloARCA"/>.
        /// </param>
        public FormProcesarSoloArca(List<ItemConciliacion> items, PerfilFiscal perfil = null)
        {
            InitializeComponent();
            Icon = AppIcons.Arca;

            // Configurar columnas de dgvTasas
            dgvTasas.Columns.Add(new GridViewTextBoxColumn { Name = "Descripcion", HeaderText = "Descripción ARCA", MinWidth = 150, MaxWidth = 200, ReadOnly = true });

            var colTasa = new GridViewComboBoxColumn 
            { 
                Name = "LineaSistema", 
                HeaderText = "Tasa del Sistema", 
                MinWidth = 200, 
                MaxWidth = 300,
                DisplayMember = "DescripcionCombo",
                ValueMember = "NumeroLinea",
                DropDownStyle = Telerik.WinControls.RadDropDownStyle.DropDownList
            };
            dgvTasas.Columns.Add(colTasa);

            dgvTasas.Columns.Add(new GridViewDecimalColumn { Name = "Porcentaje", HeaderText = "Tasa %", MinWidth = 60, MaxWidth = 80, FormatString = "{0:N2}", ReadOnly = true, DataType = typeof(decimal) });
            dgvTasas.Columns.Add(new GridViewDecimalColumn { Name = "Neto", HeaderText = "Neto Gravado", MinWidth = 100, MaxWidth = 150, FormatString = "{0:N2}", TextAlignment = ContentAlignment.MiddleRight, ReadOnly = false, DataType = typeof(decimal) });
            dgvTasas.Columns.Add(new GridViewDecimalColumn { Name = "Importe", HeaderText = "IVA / Impuesto", MinWidth = 100, MaxWidth = 150, FormatString = "{0:N2}", TextAlignment = ContentAlignment.MiddleRight, ReadOnly = false, DataType = typeof(decimal) });

            dgvTasas.AllowEditRow = true;
            dgvTasas.ReadOnly = false;
            dgvTasas.CellEditorInitialized += DgvTasas_CellEditorInitialized;
            dgvTasas.CellValueChanged += DgvTasas_CellValueChanged;

            _perfil = perfil;
            _items = items?
                .Where(x => x.Estado == EstadoConciliacion.SoloARCA)
                .ToList()
                ?? throw new ArgumentNullException(nameof(items));

            _impuestosService    = new OctosisImpuestosService();
            _proveedorService    = new OctosisProveedorService();
            _taxService          = new TaxDisaggregationService(_impuestosService);
            _docService          = new OctosisDocumentoService();
            _imputacionesService = new OctosisImputacionesService(_impuestosService);
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                using var conn = OctosisConfig.GetConnectionFactory(_perfil)();
                conn.Open();
                _todasLasTasas = await _impuestosService.ObtenerTodasParaProveedoresAsync(conn);

                var colTasa = (GridViewComboBoxColumn)dgvTasas.Columns["LineaSistema"];
                colTasa.DataSource = _todasLasTasas
                    .Select(t => new { NumeroLinea = t.NumeroLinea.Trim(), DescripcionCombo = $"{t.NumeroLinea.Trim()} - {t.Descripcion.Trim()}" })
                    .ToList();
            }
            catch (Exception ex)
            {
                SetEstado($"Error al cargar tasas: {ex.Message}", Color.DarkRed);
            }

            if (_items.Count == 0)
            {
                MessageBox.Show("No hay comprobantes Solo ARCA para procesar.", "Sin pendientes",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            await MostrarItemAsync(0);
        }

        // ?? Navegación ????????????????????????????????????????????????????????

        private async Task MostrarItemAsync(int index)
        {
            _currentIndex      = index;
            _proveedorResuelto = null;

            if (index >= _items.Count)
            {
                MostrarResumen();
                return;
            }

            var item = _items[index];
            var csv  = item.SourceArca;

            // Progreso
            lblProgreso.Text = $"Comprobante {index + 1} de {_items.Count}";

            // Habilitar controles de navegación
            grpInfo.Visible       = true;
            grpResolucion.Visible = true;
            grpTasas.Visible      = true;
            btnDarDeAlta.Visible  = true;
            btnOmitir.Visible     = true;

            LimpiarEstado();
            dgvTasas.Rows.Clear();

            if (csv is null)
            {
                SetEstado("Error: el ítem no tiene datos del comprobante ARCA.", Color.DarkRed);
                btnDarDeAlta.Enabled = false;
                return;
            }

            // Info del comprobante
            lblEmisor.Text = $"Emisor:     {csv.DenominacionEmisor}  (CUIT: {FormatearCuit(csv.NroDocEmisor)})";
            lblTipo.Text   = $"Tipo:       {csv.TipoComprobante}";
            lblNumero.Text = $"Número:     {(csv.PuntoVenta ?? "").PadLeft(4, '0')}-{(csv.NumeroDesde ?? "").PadLeft(8, '0')}";
            lblFecha.Text  = $"Fecha:      {csv.FechaEmision}";
            lblTotal.Text  = $"Total ARCA: $ {TaxDisaggregationService.ParseDecimal(csv.ImpTotal):N2}";

            // Moneda
            var moneda = item.Moneda ?? MonedaResolver.Resolver(csv);
            pnlTipoCambio.Visible = moneda.EsMonedaExtranjera;
            if (moneda.EsMonedaExtranjera)
            {
                lblMonedaInfo.Text = $"Moneda extranjera: {moneda.DescripcionMoneda}";
                txtTipoCambio.Text = moneda.TipoCambioConfirmado && moneda.TipoCambio > 0m
                    ? moneda.TipoCambio.ToString("F4", CultureInfo.InvariantCulture)
                    : string.Empty;
            }
            else
            {
                lblMonedaInfo.Text = string.Empty;
            }

            // Tipo de comprobante reconocido
            if (string.IsNullOrEmpty(item.TipoOctosis))
            {
                SetEstado($"Tipo de comprobante no reconocido: \"{csv.TipoComprobante}\". No se puede dar de alta.",
                    Color.DarkRed);
                btnDarDeAlta.Enabled = false;
                return;
            }

            // Auto-resolver proveedor (y luego impuestos si ok)
            await ResolverProveedorAsync(csv.NroDocEmisor);
        }

        // ?? Resolución de proveedor (PASO 4) ?????????????????????????????????

        private async Task ResolverProveedorAsync(string cuit)
        {
            lblProveedorNombre.Text = "Buscando...";
            btnDarDeAlta.Enabled    = false;

            try
            {
                using var conn = OctosisConfig.GetConnectionFactory(_perfil)();
                conn.Open();
                var result = await _proveedorService.BuscarPorCuitAsync(cuit, conn);

                switch (result.Estado)
                {
                    case EstadoBusquedaProveedor.NoEncontrado:
                        lblProveedorNombre.Text = "? No encontrado";
                        SetEstado($"CUIT {cuit} no existe en Octosis. No se puede dar de alta.", Color.DarkOrange);
                        return;

                    case EstadoBusquedaProveedor.Unico:
                        _proveedorResuelto      = result.Proveedores[0];
                        lblProveedorNombre.Text = $"{_proveedorResuelto.Nombre}  ({_proveedorResuelto.Numero})";
                        break;

                    case EstadoBusquedaProveedor.Multiples:
                        using (var dlg = new FormSeleccionProveedor(cuit, result.Proveedores))
                        {
                            if (dlg.ShowDialog(this) == DialogResult.OK &&
                                dlg.ProveedorSeleccionado != null)
                            {
                                _proveedorResuelto      = dlg.ProveedorSeleccionado;
                                lblProveedorNombre.Text = $"{_proveedorResuelto.Nombre}  ({_proveedorResuelto.Numero})";
                            }
                            else
                            {
                                lblProveedorNombre.Text = "? Sin proveedor seleccionado";
                                SetEstado("Seleccioná un proveedor para continuar.", Color.DarkOrange);
                                return;
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                lblProveedorNombre.Text = "? Error al buscar";
                SetEstado($"Error al buscar proveedor: {ex.Message}", Color.DarkRed);
                return;
            }

            // Proveedor resuelto ? desagregar impuestos
            var item   = _items[_currentIndex];
            var moneda = item.Moneda ?? MonedaResolver.Resolver(item.SourceArca!);
            await ResolverTasasAsync(item.SourceArca!, moneda, item.TipoOctosis!);
        }

        // ?? Desagregación de impuestos (PASO 6) ??????????????????????????????

        private async Task ResolverTasasAsync(ComprobanteCsv csv, MonedaInfo moneda, string tipoOctosis)
        {
            dgvTasas.Rows.Clear();
            SetEstado("Analizando impuestos...", Color.DarkBlue);

            try
            {
                using var conn = OctosisConfig.GetConnectionFactory(_perfil)();
                conn.Open();
                var res = await _taxService.DesagregarAsync(csv, moneda, tipoOctosis, OctosisConfig.Sucursal, conn);

                SetEstado("", Color.Black);

                if (res.Tasas.Count == 0 && res.TasasFaltantes.Count == 0)
                {
                    dgvTasas.Rows.Add("Sin impuestos desagregados", null, 0m, 0m, 0m);
                }
                else
                {
                    foreach (var t in res.Tasas)
                    {
                        string desc = !string.IsNullOrEmpty(t.Descripcion) ? t.Descripcion : $"IVA {t.Porcentaje}%";
                        dgvTasas.Rows.Add(desc, t.Linea?.Trim(), t.Porcentaje, t.Neto, t.Importe);
                    }
                }

                if (res.TasasFaltantes.Count > 0)
                {
                    foreach (var f in res.TasasFaltantes)
                        dgvTasas.Rows.Add($"FALTANTE: {f}", null, 0m, 0m, 0m);
                    SetEstado("Faltan tasas parametrizadas. Verificar módulo Impuestos (par_habmpv='S').", Color.DarkRed);
                    btnDarDeAlta.Enabled = false;
                    return;
                }

                foreach (var av in res.Avisos)
                    SetEstado($"Aviso: {av}", Color.DarkOrange);

                btnDarDeAlta.Enabled = true;
            }
            catch (Exception ex)
            {
                dgvTasas.Rows.Clear();
                SetEstado($"Error al analizar impuestos: {ex.Message}", Color.DarkRed);
                btnDarDeAlta.Enabled = false;
            }
        }

        // ?? Eventos de Grilla (Edición y Filtrado) ????????????????????????????

        private void DgvTasas_CellEditorInitialized(object sender, GridViewCellEventArgs e)
        {
            if (e.Column.Name == "LineaSistema" && e.ActiveEditor is RadDropDownListEditor combo)
            {
                var el = (RadDropDownListEditorElement)combo.EditorElement;
                decimal porc = Convert.ToDecimal(e.Row.Cells["Porcentaje"].Value ?? 0m);

                // Filtrar las tasas por el porcentaje de la fila (0% para Otros Tributos u Operaciones Exentas)
                var filtradas = _todasLasTasas
                    .Where(t => t.Porcentaje == porc)
                    .Select(t => new { NumeroLinea = t.NumeroLinea.Trim(), DescripcionCombo = $"{t.NumeroLinea.Trim()} - {t.Descripcion.Trim()}" })
                    .OrderBy(t => t.NumeroLinea)
                    .ToList();

                el.DataSource = filtradas;
                el.ValueMember = "NumeroLinea";
                el.DisplayMember = "DescripcionCombo";
            }
        }

        private bool _isUpdatingGrid = false;

        private void DgvTasas_CellValueChanged(object sender, GridViewCellEventArgs e)
        {
            if (_isUpdatingGrid) return;

            if (e.Column.Name == "Importe" || e.Column.Name == "Neto")
            {
                var csv = _items[_currentIndex].SourceArca;
                if (csv == null) return;

                decimal totalDoc = TaxDisaggregationService.ParseDecimal(csv.ImpTotal);
                decimal sumaActual = 0m;
                foreach (var row in dgvTasas.Rows)
                {
                    sumaActual += Convert.ToDecimal(row.Cells["Neto"].Value ?? 0m);
                    sumaActual += Convert.ToDecimal(row.Cells["Importe"].Value ?? 0m);
                }

                if (sumaActual < totalDoc)
                {
                    decimal diff = totalDoc - sumaActual;
                    _isUpdatingGrid = true;
                    dgvTasas.Rows.Add("Diferencia / Restante", "70", 0m, 0m, diff);
                    _isUpdatingGrid = false;
                }
            }
        }

        // ?? Acción: Dar de alta ???????????????????????????????????????????????

        private async void btnDarDeAlta_Click(object sender, EventArgs e)
        {
            var item   = _items[_currentIndex];
            var csv    = item.SourceArca!;
            var moneda = item.Moneda ?? MonedaResolver.Resolver(csv);

            // Validar tipo de cambio si moneda extranjera y no confirmado
            if (moneda.EsMonedaExtranjera && !moneda.TipoCambioConfirmado)
            {
                string tcTexto = txtTipoCambio.Text.Trim().Replace(',', '.');
                if (!decimal.TryParse(tcTexto, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out decimal tc) || tc <= 0m)
                {
                    MessageBox.Show("Ingresá el tipo de cambio (mayor que 0).", "Validación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTipoCambio.Focus();
                    return;
                }
                moneda = new MonedaInfo
                {
                    EsMonedaExtranjera   = true,
                    CodigoMoneda         = moneda.CodigoMoneda,
                    DescripcionMoneda    = moneda.DescripcionMoneda,
                    TipoCambio           = tc,
                    TipoCambioConfirmado = true,
                };
            }

            SetCargando(true);
            try
            {
                using var conn = OctosisConfig.GetConnectionFactory(_perfil)();
                conn.Open();

                // Re-desagregar para recuperar tasas faltantes si las hay, pero usaremos la grilla para los montos y lineas finales.
                var tasasOriginales = await _taxService.DesagregarAsync(
                    csv, moneda, item.TipoOctosis!, OctosisConfig.Sucursal, conn);

                var tasas = new TaxDisaggregationResult();
                tasas.TasasFaltantes.AddRange(tasasOriginales.TasasFaltantes);
                tasas.Avisos.AddRange(tasasOriginales.Avisos);

                // Armar las tasas basándonos 100% en lo que dice la grilla
                foreach (var row in dgvTasas.Rows)
                {
                    string lineaElegida = row.Cells["LineaSistema"].Value?.ToString() ?? string.Empty;
                    decimal porc = Convert.ToDecimal(row.Cells["Porcentaje"].Value ?? 0m);
                    decimal neto = Convert.ToDecimal(row.Cells["Neto"].Value ?? 0m);
                    decimal importe = Convert.ToDecimal(row.Cells["Importe"].Value ?? 0m);

                    if (string.IsNullOrEmpty(lineaElegida))
                    {
                        MessageBox.Show("Falta seleccionar la 'Tasa del Sistema' en la fila: " + row.Cells["Descripcion"].Value, "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        SetCargando(false);
                        return;
                    }

                    var tMaster = _todasLasTasas.FirstOrDefault(t => t.NumeroLinea.Trim() == lineaElegida.Trim());
                    if (tMaster == null)
                    {
                        tasas.TasasFaltantes.Add($"Tasa inválida (Línea: {lineaElegida}).");
                        continue;
                    }

                    var docTasa = new OctosisDocumentoTasa
                    {
                        TipoDocumento      = item.TipoOctosis!,
                        NumeroSucursal     = OctosisConfig.Sucursal,
                        Linea              = tMaster.NumeroLinea,
                        Porcentaje         = porc,
                        Neto               = neto,
                        Importe            = importe,
                        Descripcion        = tMaster.Descripcion,
                        ColumnaLibroIva    = tMaster.ColumnaLibroIva,
                        Resta              = tMaster.Resta,
                        TotalizaSeparado   = tMaster.TotalizaSeparado,
                        EsExento           = !tMaster.EsIvaGeneral && porc == 0m,
                        DocumentoEnDolares = moneda.EsMonedaExtranjera,
                        TipoCambio         = moneda.TipoCambio,
                    };

                    if (moneda.EsMonedaExtranjera && moneda.TipoCambio > 0m)
                    {
                        docTasa.ImporteDolares = moneda.ConvertirDesdePesos(importe);
                        docTasa.NetoDolares    = moneda.ConvertirDesdePesos(neto);
                    }

                    tasas.Tasas.Add(docTasa);
                }

                if (tasas.TasasFaltantes.Count > 0)
                {
                    MessageBox.Show(
                        "No se puede dar de alta el documento.\n\nFaltan tasas parametrizadas:\n\n" +
                        string.Join("\n", tasas.TasasFaltantes),
                        "Tasas faltantes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var doc = ConstruirDocumento(item, csv, moneda, tasas);

                // Verificar duplicado antes de insertar
                bool existe = await _docService.ExisteAsync(
                    doc.CodProveedor, doc.TipoDocumento, doc.NroSuc, doc.NroDoc, conn);

                if (existe)
                {
                    MessageBox.Show(
                        $"El comprobante {doc.NroSuc.PadLeft(5, '0')}-{doc.NroDoc.PadLeft(8, '0')} " +
                        "ya existe en Octosis para este proveedor.\nSe omitirá.",
                        "Duplicado detectado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _omitidos++;
                    await MostrarItemAsync(_currentIndex + 1);
                    return;
                }

                await _docService.AltaAsync(doc, conn, _imputacionesService);

                _altasOk++;
                SetEstado($"? Alta exitosa — NroInterno: {doc.NroInterno}", Color.DarkGreen);

                // Breve pausa para que el usuario vea la confirmación
                await Task.Delay(800);
                await MostrarItemAsync(_currentIndex + 1);
            }
            catch (Exception ex)
            {
                _errores++;
                SetEstado($"? Error: {ex.Message}", Color.DarkRed);
                MessageBox.Show(
                    $"Error al dar de alta el comprobante:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetCargando(false);
            }
        }

        // ?? Acción: Omitir ????????????????????????????????????????????????????

        private async void btnOmitir_Click(object sender, EventArgs e)
        {
            _omitidos++;
            await MostrarItemAsync(_currentIndex + 1);
        }

        // ?? Acción: Cancelar todo ?????????????????????????????????????????????

        private void btnCancelarTodo_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                    "¿Cancelar el procesamiento?\n\nLos documentos ya dados de alta no se revierten.",
                    "Confirmar cancelación",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        // ?? Construcción del OctosisDocumento ?????????????????????????????????

        private OctosisDocumento ConstruirDocumento(
            ItemConciliacion item,
            ComprobanteCsv csv,
            MonedaInfo moneda,
            TaxDisaggregationResult tasas)
        {
            DateTime fecha = DateTime.TryParseExact(
                csv.FechaEmision,
                new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" },
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var f)
                ? f : DateTime.Today;

            string nroSuc = (csv.PuntoVenta  ?? string.Empty).Trim().PadLeft(4, '0');
            string nroDoc = (csv.NumeroDesde ?? string.Empty).Trim().PadLeft(8, '0');
            decimal total = TaxDisaggregationService.ParseDecimal(csv.ImpTotal);

            return new OctosisDocumento
            {
                TipoDocumento      = item.TipoOctosis!,
                CodProveedor       = _proveedorResuelto!.Numero,
                Letra              = item.Letra ?? string.Empty,
                NroSuc             = nroSuc,
                NroDoc             = nroDoc,
                NumeroSucursal     = OctosisConfig.Sucursal,
                FecEmision         = fecha,
                FecRecepcion       = fecha,
                FecVto             = fecha,
                Importe            = total,
                Saldo              = total,
                DocumentoEnDolares = moneda.EsMonedaExtranjera,
                TipoCambio         = moneda.TipoCambio,
                ImporteDolares     = moneda.EsMonedaExtranjera ? moneda.ConvertirDesdePesos(total) : 0m,
                SaldoDolares       = moneda.EsMonedaExtranjera ? moneda.ConvertirDesdePesos(total) : 0m,
                Tasas              = tasas.Tasas,
            };
        }

        // ?? Resumen final ?????????????????????????????????????????????????????

        private void MostrarResumen()
        {
            grpInfo.Visible       = false;
            grpResolucion.Visible = false;
            grpTasas.Visible      = false;
            btnDarDeAlta.Visible  = false;
            btnOmitir.Visible     = false;
            lblProgreso.Text      = "Procesamiento finalizado";

            MessageBox.Show(
                $"Procesamiento completado.\n\n" +
                $"? Dados de alta:  {_altasOk}\n" +
                $"—  Omitidos:       {_omitidos}\n" +
                $"? Con errores:    {_errores}",
                "Procesamiento finalizado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
            Close();
        }

        // ?? Helpers UI ????????????????????????????????????????????????????????

        private void SetCargando(bool cargando)
        {
            btnDarDeAlta.Enabled = !cargando;
            btnOmitir.Enabled    = !cargando;
            Cursor = cargando ? Cursors.WaitCursor : Cursors.Default;
        }

        private void SetEstado(string texto, Color color)
        {
            lblEstado.Text      = texto;
            lblEstado.ForeColor = color;
        }

        private void LimpiarEstado()
        {
            lblEstado.Text          = string.Empty;
            lblProveedorNombre.Text = string.Empty;
            btnDarDeAlta.Enabled    = false;
            btnOmitir.Enabled       = true;
        }

        private static string FormatearCuit(string? cuit)
        {
            if (string.IsNullOrWhiteSpace(cuit) || cuit.Length != 11)
                return cuit ?? string.Empty;
            return $"{cuit[..2]}-{cuit[2..10]}-{cuit[10]}";
        }
    }
}
