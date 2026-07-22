using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ExcelDataReader;
using Dapper;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Data;

namespace AgrupadorConceptos
{
    public partial class MainForm : Form
    {
        private int? _idPerfilEditar = null;

        public MainForm(int? idPerfil = null)
        {
            InitializeComponent();
            this.Icon = AppIcon.GetIcon();
            _idPerfilEditar = idPerfil;
            ConfigurarUI_Inicial();
            this.Load += MainForm_Load;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (_idPerfilEditar.HasValue)
            {
                CargarPerfilParaEdicion(_idPerfilEditar.Value);
            }
        }

        private void CargarPerfilParaEdicion(int idPerfil)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    var perfil = connection.QueryFirstOrDefault<PerfilBanco>("SELECT * FROM PerfilesBanco WHERE Id = @Id", new { Id = idPerfil });
                    if (perfil != null)
                    {
                        txtBanco.Text = perfil.NombreBanco;
                        numFilaEncabezado.Value = perfil.FilaEncabezado;
                        chkEsCodigo.Checked = perfil.EsCodigo;
                        radImporteUnico.Checked = perfil.TipoImporte == 1;
                        radDebeHaber.Checked = perfil.TipoImporte == 2;
                        
                        cmbColumnaConcepto.Items.Add(perfil.ColumnaConcepto);
                        cmbColumnaConcepto.SelectedItem = perfil.ColumnaConcepto;

                        if (!string.IsNullOrEmpty(perfil.ColumnaDescripcion))
                        {
                            cmbColumnaDescripcion.Items.Add(perfil.ColumnaDescripcion);
                            cmbColumnaDescripcion.SelectedItem = perfil.ColumnaDescripcion;
                        }

                        if (!string.IsNullOrEmpty(perfil.ColumnaImporteUnico))
                        {
                            cmbImporteUnico.Items.Add(perfil.ColumnaImporteUnico);
                            cmbImporteUnico.SelectedItem = perfil.ColumnaImporteUnico;
                        }

                        if (!string.IsNullOrEmpty(perfil.ColumnaDebe))
                        {
                            cmbColumnaDebe.Items.Add(perfil.ColumnaDebe);
                            cmbColumnaDebe.SelectedItem = perfil.ColumnaDebe;
                        }

                        if (!string.IsNullOrEmpty(perfil.ColumnaHaber))
                        {
                            cmbColumnaHaber.Items.Add(perfil.ColumnaHaber);
                            cmbColumnaHaber.SelectedItem = perfil.ColumnaHaber;
                        }

                        if (!string.IsNullOrEmpty(perfil.ColumnaFecha))
                        {
                            cmbColumnaFecha.Items.Add(perfil.ColumnaFecha);
                            cmbColumnaFecha.SelectedItem = perfil.ColumnaFecha;
                        }

                        lblArchivoExcel.Text = "Cargue un excel para ver todas las columnas.";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el perfil para editar: {ex.Message}");
            }
        }

        private void ConfigurarUI_Inicial()
        {
            radImporteUnico.Checked = true;
            ActualizarEstadoCombosImporte();
            radImporteUnico.CheckedChanged += (s, e) => ActualizarEstadoCombosImporte();
            radDebeHaber.CheckedChanged += (s, e) => ActualizarEstadoCombosImporte();
        }

        private void ActualizarEstadoCombosImporte()
        {
            cmbImporteUnico.Enabled = radImporteUnico.Checked;
            cmbColumnaDebe.Enabled = radDebeHaber.Checked;
            cmbColumnaHaber.Enabled = radDebeHaber.Checked;
        }

        private void btnCargarExcel_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "Archivos Excel/CSV|*.xls;*.xlsx;*.csv" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    lblArchivoExcel.Text = Path.GetFileName(ofd.FileName);
                    int filaSeleccionada = (int)numFilaEncabezado.Value;
                    CargarEncabezadosExcel(ofd.FileName, filaSeleccionada);
                }
            }
        }

        private void CargarEncabezadosExcel(string filePath, int filaEncabezado)
        {
            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    string ext = Path.GetExtension(filePath).ToLowerInvariant();

                    using var reader = ext == ".csv"
                    ? ExcelReaderFactory.CreateCsvReader(stream)
                    : ExcelReaderFactory.CreateReader(stream);
                    using (reader)
                    {
                        // Avanzamos hasta la fila indicada
                        for (int i = 1; i < filaEncabezado; i++)
                        {
                            reader.Read();
                        }

                        if (reader.Read()) // Lee la fila del encabezado
                        {
                            var headers = new System.Collections.Generic.List<string>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string colName = reader.GetValue(i)?.ToString()?.Trim() ?? $"Columna{i}";
                                headers.Add(colName);
                            }

                            LlenarComboBox(cmbColumnaConcepto, headers);
                            LlenarComboBox(cmbColumnaDescripcion, headers);
                            LlenarComboBox(cmbImporteUnico, headers);
                            LlenarComboBox(cmbColumnaDebe, headers);
                            LlenarComboBox(cmbColumnaHaber, headers);
                            LlenarComboBox(cmbColumnaFecha, headers);

                            MessageBox.Show("Encabezados cargados correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("El archivo Excel está vacío.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al leer el Excel: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LlenarComboBox(ComboBox cmb, System.Collections.Generic.List<string> items)
        {
            cmb.Items.Clear();
            cmb.Items.AddRange(items.ToArray());
            if (cmb.Items.Count > 0) cmb.SelectedIndex = 0;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBanco.Text))
            {
                MessageBox.Show("Debe ingresar un nombre de banco.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var perfil = new PerfilBanco
            {
                Id = _idPerfilEditar ?? 0,
                NombreBanco = txtBanco.Text.Trim(),
                ColumnaConcepto = cmbColumnaConcepto.SelectedItem?.ToString() ?? "",
                ColumnaDescripcion = cmbColumnaDescripcion.SelectedItem?.ToString() ?? "",
                EsCodigo = chkEsCodigo.Checked,
                FilaEncabezado = (int)numFilaEncabezado.Value,
                TipoImporte = radImporteUnico.Checked ? 1 : 2,
                ColumnaImporteUnico = radImporteUnico.Checked ? (cmbImporteUnico.SelectedItem?.ToString() ?? "") : null,
                ColumnaDebe = radDebeHaber.Checked ? (cmbColumnaDebe.SelectedItem?.ToString() ?? "") : null,
                ColumnaHaber = radDebeHaber.Checked ? (cmbColumnaHaber.SelectedItem?.ToString() ?? "") : null,
                ColumnaFecha = cmbColumnaFecha.SelectedItem?.ToString() ?? ""
            };

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    
                    if (_idPerfilEditar.HasValue)
                    {
                        string sql = @"
                            UPDATE PerfilesBanco 
                            SET NombreBanco = @NombreBanco, ColumnaConcepto = @ColumnaConcepto, 
                                ColumnaDescripcion = @ColumnaDescripcion, EsCodigo = @EsCodigo, 
                                FilaEncabezado = @FilaEncabezado, TipoImporte = @TipoImporte, 
                                ColumnaImporteUnico = @ColumnaImporteUnico, ColumnaDebe = @ColumnaDebe, 
                                ColumnaHaber = @ColumnaHaber, ColumnaFecha = @ColumnaFecha
                            WHERE Id = @Id";
                        connection.Execute(sql, perfil);
                    }
                    else
                    {
                        string sql = @"
                            INSERT INTO PerfilesBanco 
                            (NombreBanco, ColumnaConcepto, ColumnaDescripcion, EsCodigo, FilaEncabezado, TipoImporte, ColumnaImporteUnico, ColumnaDebe, ColumnaHaber, ColumnaFecha) 
                            VALUES 
                            (@NombreBanco, @ColumnaConcepto, @ColumnaDescripcion, @EsCodigo, @FilaEncabezado, @TipoImporte, @ColumnaImporteUnico, @ColumnaDebe, @ColumnaHaber, @ColumnaFecha)";
                        
                        connection.Execute(sql, perfil);
                    }
                }

                MessageBox.Show("Perfil guardado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close(); // Cerramos al guardar para volver al menú principal
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar en base de datos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarFomulario()
        {
            txtBanco.Clear();
            lblArchivoExcel.Text = "Ningún archivo seleccionado";
            cmbColumnaConcepto.Items.Clear();
            cmbColumnaDescripcion.Items.Clear();
            cmbImporteUnico.Items.Clear();
            cmbColumnaDebe.Items.Clear();
            cmbColumnaHaber.Items.Clear();
            cmbColumnaFecha.Items.Clear();
            chkEsCodigo.Checked = true;
            radImporteUnico.Checked = true;
        }
    }
}