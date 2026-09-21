// Fecha: 21/09/2026 - TAREA: 00041 - Linea: 6 - Lista ordenada de directivas PyR (maquetas 05-directivas-pyr*)
// Mismo comportamiento que FormDirectivasConciliacion de Offline: la primera es fija (no se edita,
// no se borra ni se mueve) y cada una trabaja sobre lo que las anteriores no emparejaron.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ArcaCliente.Models;
using Telerik.WinControls.UI;

namespace ArcaCliente
{
    public class FormDirectivasPyR : Telerik.WinControls.UI.RadForm
    {
        private readonly List<DirectivaPyR> _directivas;
        private readonly RadGridView gridDirectivas = new();
        private readonly RadButton btnEditar = new(), btnEliminar = new(), btnSubir = new(), btnBajar = new();

        public List<DirectivaPyR> Directivas => _directivas;

        private class Fila
        {
            public int    Orden       { get; set; }
            public string Descripcion { get; set; }
            public string Campos      { get; set; }
        }

        public FormDirectivasPyR(string nombrePerfil, IEnumerable<DirectivaPyR> actuales)
        {
            _directivas = actuales.Select(d => d.Clonar()).ToList();
            if (_directivas.Count == 0) _directivas = DirectivaPyR.CrearPredeterminadas();

            Text            = $"Directivas de conciliación — {nombrePerfil}";
            Icon            = AppIcons.Arca;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;
            ClientSize      = new Size(720, 330);

            Controls.Add(new RadLabel
            {
                Location  = new Point(12, 10),
                ForeColor = Color.DimGray,
                Text      = "Se aplican en orden: cada una trabaja sobre lo que las anteriores no emparejaron. La primera es fija."
            });

            gridDirectivas.Location = new Point(12, 34);
            gridDirectivas.Size     = new Size(696, 200);
            gridDirectivas.ShowGroupPanel = false;
            gridDirectivas.MasterTemplate.AllowAddNewRow = false;
            gridDirectivas.MasterTemplate.AllowDeleteRow = false;
            gridDirectivas.MasterTemplate.AllowEditRow   = false;
            gridDirectivas.MasterTemplate.ShowRowHeaderColumn = false;
            gridDirectivas.MasterTemplate.AutoGenerateColumns = false;
            gridDirectivas.MasterTemplate.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;
            gridDirectivas.Columns.Add(new GridViewTextBoxColumn(nameof(Fila.Orden))       { HeaderText = "#", Width = 30 });
            gridDirectivas.Columns.Add(new GridViewTextBoxColumn(nameof(Fila.Descripcion)) { HeaderText = "Descripción", Width = 300 });
            gridDirectivas.Columns.Add(new GridViewTextBoxColumn(nameof(Fila.Campos))      { HeaderText = "Campos", Width = 300 });
            gridDirectivas.RowFormatting   += GridDirectivas_RowFormatting;
            gridDirectivas.SelectionChanged += (s, e) => ActualizarBotones();
            gridDirectivas.CellDoubleClick += (s, e) => { if (e.RowIndex > 0) Editar(); };
            Controls.Add(gridDirectivas);

            int x = 12;
            RadButton Boton(RadButton b, string texto, int ancho, EventHandler click)
            {
                b.Text = texto; b.Location = new Point(x, 244); b.Size = new Size(ancho, 28); b.Click += click;
                Controls.Add(b); x += ancho + 8; return b;
            }
            Boton(new RadButton(), "Agregar...", 96, (s, e) => Agregar());
            Boton(btnEditar,   "Editar...", 96, (s, e) => Editar());
            Boton(btnEliminar, "Eliminar",  96, (s, e) => Eliminar());
            Boton(btnSubir,    "▲ Subir",   86, (s, e) => Mover(-1));
            Boton(btnBajar,    "▼ Bajar",   86, (s, e) => Mover(+1));
            Boton(new RadButton(), "Restablecer predeterminadas", 200, (s, e) => Restablecer());

            var btnAceptar  = new RadButton { Text = "Aceptar",  Location = new Point(500, 290), Size = new Size(100, 30), DialogResult = DialogResult.OK };
            var btnCancelar = new RadButton { Text = "Cancelar", Location = new Point(608, 290), Size = new Size(100, 30), DialogResult = DialogResult.Cancel };
            btnAceptar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            Controls.AddRange(new Control[] { btnAceptar, btnCancelar });
            CancelButton = btnCancelar;

            Refrescar(0);
        }

        private int Seleccion => gridDirectivas.CurrentRow?.Index ?? -1;

        private void Refrescar(int seleccionar)
        {
            gridDirectivas.DataSource = _directivas
                .Select((d, i) => new Fila { Orden = i + 1, Descripcion = d.Descripcion, Campos = d.ResumenCampos })
                .ToList();
            if (seleccionar >= 0 && seleccionar < gridDirectivas.Rows.Count)
                gridDirectivas.CurrentRow = gridDirectivas.Rows[seleccionar];
            ActualizarBotones();
        }

        private void ActualizarBotones()
        {
            int i = Seleccion;
            bool editable = i > 0;   // la primera es fija
            btnEditar.Enabled   = editable;
            btnEliminar.Enabled = editable;
            btnSubir.Enabled    = editable && i > 1;
            btnBajar.Enabled    = editable && i < _directivas.Count - 1;
        }

        private void GridDirectivas_RowFormatting(object sender, RowFormattingEventArgs e)
        {
            if (e.RowElement.RowInfo.Index == 0)
            {
                e.RowElement.DrawFill      = true;
                e.RowElement.GradientStyle = Telerik.WinControls.GradientStyles.Solid;
                e.RowElement.BackColor     = Color.FromArgb(220, 235, 255);
                e.RowElement.ForeColor     = Color.DarkBlue;
            }
            else
            {
                e.RowElement.ResetValue(LightVisualElement.DrawFillProperty, Telerik.WinControls.ValueResetFlags.Local);
                e.RowElement.ResetValue(LightVisualElement.GradientStyleProperty, Telerik.WinControls.ValueResetFlags.Local);
                e.RowElement.ResetValue(Telerik.WinControls.VisualElement.BackColorProperty, Telerik.WinControls.ValueResetFlags.Local);
                e.RowElement.ResetValue(Telerik.WinControls.VisualElement.ForeColorProperty, Telerik.WinControls.ValueResetFlags.Local);
            }
        }

        private void Agregar()
        {
            using var form = new FormDirectivaPyRDetalle();
            if (form.ShowDialog(this) != DialogResult.OK) return;
            _directivas.Add(form.Directiva);
            Refrescar(_directivas.Count - 1);
        }

        private void Editar()
        {
            int i = Seleccion;
            if (i <= 0) return;
            using var form = new FormDirectivaPyRDetalle(_directivas[i]);
            if (form.ShowDialog(this) != DialogResult.OK) return;
            _directivas[i] = form.Directiva;
            Refrescar(i);
        }

        private void Eliminar()
        {
            int i = Seleccion;
            if (i <= 0) return;
            if (MessageBox.Show($"¿Eliminar la directiva \"{_directivas[i].Descripcion}\"?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            _directivas.RemoveAt(i);
            Refrescar(Math.Min(i, _directivas.Count - 1));
        }

        private void Mover(int delta)
        {
            int i = Seleccion, j = i + delta;
            if (i <= 0 || j <= 0 || j >= _directivas.Count) return;   // nunca por encima de la fija
            (_directivas[i], _directivas[j]) = (_directivas[j], _directivas[i]);
            Refrescar(j);
        }

        private void Restablecer()
        {
            if (MessageBox.Show("¿Reemplazar las directivas por las 3 predeterminadas?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            _directivas.Clear();
            _directivas.AddRange(DirectivaPyR.CrearPredeterminadas());
            Refrescar(0);
        }
    }
}
