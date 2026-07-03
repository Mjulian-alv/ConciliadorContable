using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Telerik.WinControls.UI;
using LiquidacionesAuditar.Data;
using LiquidacionesAuditar.Models;

namespace LiquidacionesAuditar
{
    public partial class FormLineasCON : RadForm
    {
        private LineaCON _lineaActual;

        public FormLineasCON()
        {
            InitializeComponent();
            CargarMarcas();
            CargarLineas();
            CargarColsCSV();
            CargarValoresFijos();
        }

        private void CargarMarcas()
        {
            var marcas = Repositorio.GetMarcas();
            cmbMarca.DataSource = null;
            cmbMarca.DataSource = marcas;
            cmbMarca.DisplayMember = "Nombre";
            cmbMarca.ValueMember = "Id";
        }

        private void cmbMarca_SelectedIndexChanged(object sender, EventArgs e)
            => CargarLineas();

        private void CargarLineas()
        {
            var idMarca = cmbMarca.SelectedValue?.ToString() ?? "";
            var lineas = Repositorio.GetLineasCON(idMarca);
            lstLineas.DataSource = null;
            lstLineas.DataSource = lineas;
            lstLineas.DisplayMember = "Descripcion";
            _lineaActual = null;
            LimpiarDetalle();
        }

        private void lstLineas_SelectedIndexChanged(object sender, EventArgs e)
        {
            _lineaActual = lstLineas.SelectedItem as LineaCON;
            if (_lineaActual == null) { LimpiarDetalle(); return; }
            txtDescripcion.Text = _lineaActual.Descripcion;
            nudOrden.Value = _lineaActual.Orden;
            txtCondicionSigno.Text = _lineaActual.CondicionSigno;
            CargarColsCSV();
            CargarValoresFijos();
        }

        private void LimpiarDetalle()
        {
            txtDescripcion.Text = "";
            nudOrden.Value = 0;
            txtCondicionSigno.Text = "";
            clbColsCSV.Items.Clear();
            gridValoresFijos.DataSource = null;
        }

        private void CargarColsCSV()
        {
            clbColsCSV.Items.Clear();
            var idMarca = cmbMarca.SelectedValue?.ToString() ?? "";
            var colsCSV = Repositorio.GetColumnasCSV(idMarca);
            var seleccionadas = _lineaActual != null
                ? Repositorio.GetCONCols(_lineaActual.Id).ToHashSet(StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>();

            foreach (var col in colsCSV)
                clbColsCSV.Items.Add(col.IdColumnaArchivo, seleccionadas.Contains(col.IdColumnaArchivo));
        }

        private void CargarValoresFijos()
        {
            if (_lineaActual == null) { gridValoresFijos.DataSource = null; return; }
            var vals = Repositorio.GetCONValoresFijos(_lineaActual.Id);
            gridValoresFijos.DataSource = null;
            gridValoresFijos.DataSource = vals;
            gridValoresFijos.BestFitColumns();
        }

        private void btnNuevaLinea_Click(object sender, EventArgs e)
        {
            _lineaActual = null;
            LimpiarDetalle();
            txtDescripcion.Focus();
        }

        private void btnGuardarLinea_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDescripcion.Text))
            { MessageBox.Show("Ingrese una descripción.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var idMarca = cmbMarca.SelectedValue?.ToString() ?? "";
            var linea = new LineaCON
            {
                IdMarcaTarjeta = idMarca,
                Descripcion = txtDescripcion.Text.Trim(),
                Orden = (int)nudOrden.Value,
                CondicionSigno = txtCondicionSigno.Text.Trim()
            };

            int idLinea;
            if (_lineaActual == null)
            {
                idLinea = Repositorio.InsertLineaCON(linea);
            }
            else
            {
                linea.Id = _lineaActual.Id;
                Repositorio.UpdateLineaCON(linea);
                idLinea = _lineaActual.Id;
            }

            // Guardar columnas CSV seleccionadas
            var colsSel = new List<string>();
            for (int i = 0; i < clbColsCSV.Items.Count; i++)
                if (clbColsCSV.GetItemChecked(i))
                    colsSel.Add(clbColsCSV.Items[i].ToString());
            Repositorio.SetCONCols(idLinea, colsSel);

            // Guardar valores fijos desde el grid
            var vals = gridValoresFijos.DataSource as List<CONValorFijo>;
            if (vals != null)
                Repositorio.SetCONValoresFijos(idLinea, vals);

            CargarLineas();
            MessageBox.Show("Línea CON guardada correctamente.", "Guardar", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnEliminarLinea_Click(object sender, EventArgs e)
        {
            if (_lineaActual == null) return;
            if (MessageBox.Show($"¿Eliminar la línea CON '{_lineaActual.Descripcion}'?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            Repositorio.DeleteLineaCON(_lineaActual.Id);
            CargarLineas();
        }

        private void btnAgregarValorFijo_Click(object sender, EventArgs e)
        {
            var lista = (gridValoresFijos.DataSource as List<CONValorFijo>) ?? new List<CONValorFijo>();
            lista.Add(new CONValorFijo { Posicion = lista.Count + 1, Valor = "" });
            gridValoresFijos.DataSource = null;
            gridValoresFijos.DataSource = lista;
        }

        private void btnQuitarValorFijo_Click(object sender, EventArgs e)
        {
            if (gridValoresFijos.SelectedRows.Count == 0) return;
            var v = gridValoresFijos.SelectedRows[0].DataBoundItem as CONValorFijo;
            var lista = (gridValoresFijos.DataSource as List<CONValorFijo>) ?? new List<CONValorFijo>();
            lista.Remove(v);
            gridValoresFijos.DataSource = null;
            gridValoresFijos.DataSource = lista;
        }
    }
}
