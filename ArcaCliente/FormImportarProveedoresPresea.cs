using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ArcaCliente.Models;
using ArcaCliente.Services;
using Telerik.WinControls.UI;

namespace ArcaCliente
{
    /// <summary>
    /// Importa el maestro de proveedores de PRESEA desde un archivo CSV/TXT/Excel.
    /// El usuario elige el archivo, se leen sus columnas y se mapea cada campo de la
    /// entidad <see cref="ConfigPreseaProveedor"/> a una columna. El mapeo se recuerda
    /// entre sesiones (parametrizable). Los datos se guardan en la tabla local
    /// PreseaProveedores (upsert por CUIT).
    /// </summary>
    public partial class FormImportarProveedoresPresea : Telerik.WinControls.UI.RadForm
    {
        private const string Entidad = PreseaProveedorImporter.Entidad;

        private readonly Dictionary<string, RadDropDownList> _combos = new();
        private TablaLeida _tabla;
        private MapeoColumnasArchivo _mapeoGuardado;

        private RadTextBox      txtArchivo;
        private RadDropDownList cmbSeparador;
        private RadDropDownList cmbEncoding;
        private RadSpinEditor   spnFila;
        private RadDropDownList cmbSepDec;
        private RadTextBox      txtHoja;
        private RadGridView     gridPreview;
        private RadLabel        lblEstado;

        public FormImportarProveedoresPresea()
        {
            InitializeComponent();
            Icon = AppIcons.Arca;
            BuildUi();
            CargarMapeoGuardado();
        }

        // ── Construccion de la UI ───────────────────────────────────────────────────

        private void BuildUi()
        {
            // grpArchivo
            var grpArchivo = new GroupBox { Text = "Archivo", Location = new Point(12, 8), Size = new Size(736, 118), TabStop = false };

            grpArchivo.Controls.Add(Lbl("Archivo:", 12, 22));
            txtArchivo = Build(new RadTextBox(), c => { c.Location = new Point(12, 42); c.Size = new Size(516, 20); });
            grpArchivo.Controls.Add(txtArchivo);

            grpArchivo.Controls.Add(Build(new RadButton(), b =>
            {
                b.Location = new Point(536, 40); b.Size = new Size(90, 26); b.Text = "Examinar...";
                b.Click += BtnExaminar_Click;
            }));
            grpArchivo.Controls.Add(Build(new RadButton(), b =>
            {
                b.Location = new Point(632, 40); b.Size = new Size(92, 26); b.Text = "Leer columnas";
                b.Click += BtnLeer_Click;
            }));

            grpArchivo.Controls.Add(Lbl("Separador", 12, 74, 100));
            cmbSeparador = ComboValores(12, 90, 110, new[]
            {
                ("; (punto y coma)", ";"), (", (coma)", ","), ("Tab", "\t"), ("| (pipe)", "|")
            });
            grpArchivo.Controls.Add(cmbSeparador);

            grpArchivo.Controls.Add(Lbl("Encoding", 130, 74, 100));
            cmbEncoding = ComboValores(130, 90, 100, new[] { ("UTF-8", "UTF-8"), ("Latin-1", "Latin-1") });
            grpArchivo.Controls.Add(cmbEncoding);

            grpArchivo.Controls.Add(Lbl("Fila encab.", 238, 74, 70));
            spnFila = Build(new RadSpinEditor(), s =>
            {
                s.Location = new Point(238, 90); s.Size = new Size(60, 20);
                s.Minimum = 1; s.Maximum = 100; s.Value = 1;
            });
            grpArchivo.Controls.Add(spnFila);

            grpArchivo.Controls.Add(Lbl("Sep. decimal", 306, 74, 100));
            cmbSepDec = ComboValores(306, 90, 100, new[] { (". (punto)", "."), (", (coma)", ",") });
            grpArchivo.Controls.Add(cmbSepDec);

            grpArchivo.Controls.Add(Lbl("Hoja Excel", 414, 74, 100));
            txtHoja = Build(new RadTextBox(), c => { c.Location = new Point(414, 90); c.Size = new Size(150, 20); });
            grpArchivo.Controls.Add(txtHoja);

            Controls.Add(grpArchivo);

            // grpMapeo
            var grpMapeo = new GroupBox
            {
                Text = "Mapeo: campo de PRESEA  ->  columna del archivo",
                Location = new Point(12, 132), Size = new Size(736, 300), TabStop = false
            };

            var campos = PreseaProveedorImporter.Campos;
            for (int i = 0; i < campos.Length; i++)
            {
                int col = i % 2;
                int fila = i / 2;
                int x = 14 + col * 360;
                int y = 26 + fila * 52;

                grpMapeo.Controls.Add(Lbl(campos[i].Etiqueta, x, y, 340));

                var combo = Build(new RadDropDownList(), c =>
                {
                    c.Location = new Point(x, y + 18); c.Size = new Size(340, 24);
                    c.DropDownStyle = Telerik.WinControls.RadDropDownStyle.DropDownList;
                });
                grpMapeo.Controls.Add(combo);
                _combos[campos[i].Campo] = combo;
            }
            Controls.Add(grpMapeo);

            // grpPreview
            var grpPreview = new GroupBox { Text = "Vista previa", Location = new Point(12, 438), Size = new Size(736, 150), TabStop = false };
            gridPreview = Build(new RadGridView(), g =>
            {
                g.Location = new Point(8, 20); g.Size = new Size(720, 122);
                g.ReadOnly = true;
                g.AllowAddNewRow = false;
                g.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;
            });
            grpPreview.Controls.Add(gridPreview);
            Controls.Add(grpPreview);

            // Panel inferior
            var pnl = Build(new RadPanel(), p => { p.Dock = DockStyle.Bottom; p.Size = new Size(760, 44); });

            lblEstado = Build(new RadLabel(), l => { l.Location = new Point(12, 14); l.Text = string.Empty; });
            pnl.Controls.Add(lblEstado);

            pnl.Controls.Add(Build(new RadButton(), b =>
            {
                b.Location = new Point(440, 8); b.Size = new Size(110, 28); b.Text = "Previsualizar";
                b.Click += BtnPrevisualizar_Click;
            }));
            pnl.Controls.Add(Build(new RadButton(), b =>
            {
                b.Location = new Point(556, 8); b.Size = new Size(96, 28); b.Text = "Importar";
                b.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                b.Click += BtnImportar_Click;
            }));
            pnl.Controls.Add(Build(new RadButton(), b =>
            {
                b.Location = new Point(658, 8); b.Size = new Size(90, 28); b.Text = "Cerrar";
                b.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            }));

            Controls.Add(pnl);
        }

        // ── Carga del mapeo guardado ─────────────────────────────────────────────────

        private void CargarMapeoGuardado()
        {
            _mapeoGuardado = MapeoColumnasStorage.Load(Entidad) ?? new MapeoColumnasArchivo { Entidad = Entidad };

            SeleccionarValor(cmbSeparador, _mapeoGuardado.Separador ?? ";");
            SeleccionarValor(cmbEncoding,  _mapeoGuardado.Encoding  ?? "UTF-8");
            SeleccionarValor(cmbSepDec,    _mapeoGuardado.SeparadorDecimal ?? ".");
            spnFila.Value = _mapeoGuardado.FilaEncabezado < 1 ? 1 : _mapeoGuardado.FilaEncabezado;
            txtHoja.Text  = _mapeoGuardado.HojaExcel ?? string.Empty;
        }

        // ── Eventos ──────────────────────────────────────────────────────────────────

        private void BtnExaminar_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Seleccionar archivo de proveedores de PRESEA",
                Filter = "Archivos soportados (*.csv;*.txt;*.xlsx)|*.csv;*.txt;*.xlsx|Todos (*.*)|*.*"
            };
            if (!string.IsNullOrWhiteSpace(txtArchivo.Text))
                dlg.InitialDirectory = Path.GetDirectoryName(txtArchivo.Text);
            if (dlg.ShowDialog(this) == DialogResult.OK)
                txtArchivo.Text = dlg.FileName;
        }

        private void BtnLeer_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtArchivo.Text) || !File.Exists(txtArchivo.Text))
            {
                MessageBox.Show("Seleccione un archivo valido.", "Validacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var cfg = LeerConfigUi();
                _tabla = TabularFileReader.Leer(txtArchivo.Text, cfg.Separador, cfg.Encoding, cfg.FilaEncabezado, cfg.HojaExcel);

                if (_tabla.Columnas.Count == 0)
                {
                    SetEstado("No se detectaron columnas. Revise el separador o la fila de encabezado.", Color.DarkOrange);
                    return;
                }

                PoblarCombos(_tabla.Columnas);
                SetEstado($"{_tabla.Columnas.Count} columnas y {_tabla.Filas.Count} filas leidas. Revise el mapeo.", Color.DarkGreen);
            }
            catch (Exception ex)
            {
                SetEstado("Error al leer el archivo.", Color.DarkRed);
                MessageBox.Show($"Error al leer el archivo:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PoblarCombos(List<string> columnas)
        {
            foreach (var (campo, combo) in _combos)
            {
                combo.Items.Clear();
                combo.Items.Add(new RadListDataItem("(ninguno)", string.Empty));
                foreach (var col in columnas)
                    combo.Items.Add(new RadListDataItem(col, col));

                // Preseleccion: mapeo guardado, si no heuristica por nombre.
                string elegido = null;
                if (_mapeoGuardado.Mapeo.TryGetValue(campo, out var guardado) && columnas.Contains(guardado, StringComparer.OrdinalIgnoreCase))
                    elegido = guardado;
                else
                    elegido = SugerirColumna(campo, columnas);

                SeleccionarValor(combo, elegido ?? string.Empty);
            }
        }

        private static string SugerirColumna(string campo, List<string> columnas)
        {
            string[] claves = campo switch
            {
                "Cuit"                    => new[] { "cuit" },
                "CodigoProveedor"         => new[] { "codigo", "cod prov", "cod.prov", "nro prov" },
                "CuentaContableProveedor" => new[] { "cuenta", "cta" },
                "CuentaDebe"              => new[] { "debe", "gasto" },
                "Centro"                  => new[] { "centro" },
                "Provincia"               => new[] { "provincia" },
                "Condicion"               => new[] { "condic" },
                "Descuento"               => new[] { "descuento", "desc" },
                "Fiscal"                  => new[] { "fiscal" },
                "Nombre"                  => new[] { "nombre", "razon", "denomin" },
                _                         => Array.Empty<string>()
            };

            foreach (var clave in claves)
            {
                var match = columnas.FirstOrDefault(c => c.ToLowerInvariant().Contains(clave));
                if (match != null) return match;
            }
            return null;
        }

        private void BtnPrevisualizar_Click(object sender, EventArgs e)
        {
            if (!ValidarYParsear(out var lista, out var mapeo)) return;
            gridPreview.DataSource = null;
            gridPreview.DataSource = lista;
            SetEstado($"Vista previa: {lista.Count} proveedor(es).", lista.Count > 0 ? Color.DarkGreen : Color.DarkOrange);
        }

        private void BtnImportar_Click(object sender, EventArgs e)
        {
            if (!ValidarYParsear(out var lista, out var mapeo)) return;

            if (lista.Count == 0)
            {
                MessageBox.Show("No hay proveedores para importar.", "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                AppServices.SavePreseaProveedores(lista);
                MapeoColumnasStorage.Save(mapeo);

                gridPreview.DataSource = null;
                gridPreview.DataSource = lista;
                SetEstado($"Importados {lista.Count} proveedor(es). Mapeo guardado.", Color.DarkGreen);
                MessageBox.Show($"Se importaron/actualizaron {lista.Count} proveedor(es) en PRESEA.",
                    "Importacion completa", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                SetEstado("Error al guardar.", Color.DarkRed);
                MessageBox.Show($"Error al guardar los proveedores:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Helpers de logica ────────────────────────────────────────────────────────

        private bool ValidarYParsear(out List<ConfigPreseaProveedor> lista, out MapeoColumnasArchivo mapeo)
        {
            lista = new List<ConfigPreseaProveedor>();
            var cfg = LeerConfigUi();
            mapeo = cfg;

            if (_tabla == null)
            {
                MessageBox.Show("Lea un archivo primero (boton 'Leer columnas').", "Validacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            var faltantes = PreseaProveedorImporter.Campos
                .Where(c => c.Requerido && !cfg.Mapeo.ContainsKey(c.Campo))
                .Select(c => c.Etiqueta)
                .ToList();

            if (faltantes.Count > 0)
            {
                MessageBox.Show("Falta mapear los campos obligatorios:\n\n  - " + string.Join("\n  - ", faltantes),
                    "Validacion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            lista = PreseaProveedorImporter.Parsear(_tabla, cfg, out var errores);
            if (errores.Count > 0)
                SetEstado(string.Join("  |  ", errores), Color.DarkOrange);
            return true;
        }

        private MapeoColumnasArchivo LeerConfigUi()
        {
            var m = new MapeoColumnasArchivo
            {
                Entidad          = Entidad,
                Separador        = ValorCombo(cmbSeparador, ";"),
                Encoding         = ValorCombo(cmbEncoding, "UTF-8"),
                SeparadorDecimal = ValorCombo(cmbSepDec, "."),
                FilaEncabezado   = (int)spnFila.Value,
                HojaExcel        = string.IsNullOrWhiteSpace(txtHoja.Text) ? null : txtHoja.Text.Trim(),
            };

            foreach (var (campo, combo) in _combos)
            {
                var col = combo.SelectedItem?.Value?.ToString();
                if (!string.IsNullOrEmpty(col))
                    m.Mapeo[campo] = col;
            }
            return m;
        }

        // ── Helpers de UI ────────────────────────────────────────────────────────────

        private static T Build<T>(T ctrl, Action<T> cfg) where T : Control
        {
            if (ctrl is ISupportInitialize si) si.BeginInit();
            cfg(ctrl);
            if (ctrl is ISupportInitialize si2) si2.EndInit();
            return ctrl;
        }

        private static RadLabel Lbl(string text, int x, int y, int w = 200) =>
            Build(new RadLabel(), l => { l.Text = text; l.Location = new Point(x, y); l.Size = new Size(w, 16); });

        private RadDropDownList ComboValores(int x, int y, int w, (string Texto, string Valor)[] items)
        {
            var cmb = Build(new RadDropDownList(), c =>
            {
                c.Location = new Point(x, y); c.Size = new Size(w, 24);
                c.DropDownStyle = Telerik.WinControls.RadDropDownStyle.DropDownList;
            });
            foreach (var (texto, valor) in items)
                cmb.Items.Add(new RadListDataItem(texto, valor));
            if (cmb.Items.Count > 0) cmb.SelectedIndex = 0;
            return cmb;
        }

        private static void SeleccionarValor(RadDropDownList cmb, string valor)
        {
            foreach (RadListDataItem item in cmb.Items)
                if (string.Equals(item.Value?.ToString(), valor, StringComparison.OrdinalIgnoreCase))
                { cmb.SelectedItem = item; return; }
            if (cmb.Items.Count > 0) cmb.SelectedIndex = 0;
        }

        private static string ValorCombo(RadDropDownList cmb, string fallback) =>
            cmb.SelectedItem?.Value?.ToString() ?? fallback;

        private void SetEstado(string texto, Color color)
        {
            lblEstado.Text = texto;
            lblEstado.ForeColor = color;
        }
    }
}
