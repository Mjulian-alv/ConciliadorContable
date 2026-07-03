using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ArcaCliente.Models;
using Telerik.WinControls.UI;

namespace ArcaCliente
{
    public partial class FormDirectivasConciliacion : Telerik.WinControls.UI.RadForm
    {
        private readonly List<DirectivaConciliacion> _directivas;

        /// <summary>Lista actualizada de directivas para persistir en el perfil.</summary>
        public List<DirectivaConciliacion> Directivas => _directivas;

        public FormDirectivasConciliacion(string nombrePerfil,
            IEnumerable<DirectivaConciliacion> directivasActuales)
        {
            InitializeComponent();
            Icon = AppIcons.Arca;

            Text = $"Directivas de conciliación  —  {nombrePerfil}";

            _directivas = directivasActuales
                .Select(d => new DirectivaConciliacion
                {
                    Id          = d.Id,
                    Descripcion = d.Descripcion,
                    Campos      = new List<CampoConciliacion>(d.Campos)
                })
                .ToList();

            if (_directivas.Count == 0)
                _directivas.Add(DirectivaConciliacion.CrearPrimaria());

            if (_directivas.Count >= 1 && !_directivas[_directivas.Count-1].Campos.SequenceEqual(DirectivaConciliacion.CrearUltima().Campos))
                _directivas.Add(DirectivaConciliacion.CrearUltima());

            gridDirectivas.RowFormatting += GridDirectivas_RowFormatting;

            RefrescarGrid();
        }

        // ?? Grid ??????????????????????????????????????????????????????????????????

        private void RefrescarGrid()
        {
            int selIdx = gridDirectivas.CurrentRow?.Index ?? -1;

            gridDirectivas.DataSource = null;
            gridDirectivas.DataSource = _directivas
                .Select((d, i) => new DirectivaFila
                {
                    Orden       = i + 1,
                    Descripcion = d.Descripcion,
                    Campos      = d.ResumenCampos
                })
                .ToList();

            ConfigurarColumnas();

            if (selIdx >= 0 && selIdx < gridDirectivas.Rows.Count)
                gridDirectivas.Rows[selIdx].IsSelected = true;
            else if (gridDirectivas.Rows.Count > 0)
                gridDirectivas.Rows[0].IsSelected = true;

            ActualizarBotones();
        }

        private void ConfigurarColumnas()
        {
            if (gridDirectivas.Columns.Count == 0) return;

            foreach (GridViewColumn col in gridDirectivas.Columns)
            {
                col.IsVisible = col.FieldName switch
                {
                    "Orden"       => true,
                    "Descripcion" => true,
                    "Campos"      => true,
                    _             => false
                };
            }

            if (gridDirectivas.Columns["Orden"] is GridViewDataColumn cOrden)
            {
                cOrden.HeaderText = "#";
                cOrden.Width      = 40;
            }
            if (gridDirectivas.Columns["Descripcion"] is GridViewDataColumn cDesc)
                cDesc.HeaderText = "Descripción";
            if (gridDirectivas.Columns["Campos"] is GridViewDataColumn cCampos)
                cCampos.HeaderText = "Campos";

            gridDirectivas.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;
        }

        private void GridDirectivas_RowFormatting(object sender, RowFormattingEventArgs e)
        {
            if (e.RowElement is GridDataRowElement row && row.RowInfo?.Index == 0)
            {
                row.DrawFill        = true;
                row.BackColor       = Color.FromArgb(220, 235, 255);
                row.GradientStyle   = Telerik.WinControls.GradientStyles.Solid;
                row.ForeColor       = Color.DarkBlue;
                row.Font            = new Font(row.Font, FontStyle.Bold);
            }
        }

        private void GridDirectivas_SelectionChanged(object sender, EventArgs e) =>
            ActualizarBotones();

        private void GridDirectivas_CellDoubleClick(object sender, GridViewCellEventArgs e)
        {
            if (e.RowIndex > 0) BtnEditar_Click(sender, e);
        }

        private void ActualizarBotones()
        {
            int idx       = gridDirectivas.CurrentRow?.Index ?? -1;
            bool esPrimaria = idx == 0;
            bool haySel   = idx >= 0;

            btnEditar.Enabled   = haySel && !esPrimaria;
            btnEliminar.Enabled = haySel && !esPrimaria;
            btnSubir.Enabled    = haySel && !esPrimaria && idx > 1;
            btnBajar.Enabled    = haySel && !esPrimaria && idx < _directivas.Count - 1;
        }

        // ?? CRUD ??????????????????????????????????????????????????????????????????

        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            using var form = new FormDirectivaConciliacionDetalle();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                _directivas.Add(form.Directiva);
                RefrescarGrid();
            }
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            int idx = gridDirectivas.CurrentRow?.Index ?? -1;
            if (idx <= 0)
            {
                MessageBox.Show("Seleccioná una directiva para editar.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var form = new FormDirectivaConciliacionDetalle(_directivas[idx]);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                _directivas[idx] = form.Directiva;
                RefrescarGrid();
            }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            int idx = gridDirectivas.CurrentRow?.Index ?? -1;
            if (idx <= 0)
            {
                MessageBox.Show("Seleccioná una directiva para eliminar.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var nombre = _directivas[idx].Descripcion;
            if (MessageBox.Show($"¿Eliminar la directiva \"{nombre}\"?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _directivas.RemoveAt(idx);
                RefrescarGrid();
            }
        }

        // ?? Reordenamiento ????????????????????????????????????????????????????????

        private void BtnSubir_Click(object sender, EventArgs e)
        {
            int idx = gridDirectivas.CurrentRow?.Index ?? -1;
            if (idx <= 1 || idx >= _directivas.Count - 1) return; // 0 = primaria, 1 = primero movible

            (_directivas[idx], _directivas[idx - 1]) = (_directivas[idx - 1], _directivas[idx]);
            RefrescarGrid();
            if (idx - 1 < gridDirectivas.Rows.Count)
                gridDirectivas.Rows[idx - 1].IsSelected = true;
        }

        private void BtnBajar_Click(object sender, EventArgs e)
        {
            int idx = gridDirectivas.CurrentRow?.Index ?? -1;
            if (idx <= 0 || idx >= _directivas.Count - 1) return;

            (_directivas[idx], _directivas[idx + 1]) = (_directivas[idx + 1], _directivas[idx]);
            RefrescarGrid();
            if (idx + 1 < gridDirectivas.Rows.Count)
                gridDirectivas.Rows[idx + 1].IsSelected = true;
        }

        // ?? Guardar / Cancelar ????????????????????????????????????????????????????

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        // ?? DTO de display ????????????????????????????????????????????????????????

        private sealed class DirectivaFila
        {
            public int    Orden       { get; set; }
            public string Descripcion { get; set; }
            public string Campos      { get; set; }
        }
    }
}
