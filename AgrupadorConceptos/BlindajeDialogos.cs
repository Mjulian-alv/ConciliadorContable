// Fecha: 26/10/2026 - TAREA: 00135 - Linea: 2, 3 - Diálogos del escritorio para blindar, clave maestra, historial del movimiento y aviso de baja
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;

namespace AgrupadorConceptos
{
    /// <summary>
    /// Ventanas chicas del blindaje armadas por código: son modales, sin estado propio y no justifican un diseñador cada una.
    /// Las reglas viven en <see cref="BlindajeService"/>; acá sólo se pide la clave y se muestra el motivo si falla.
    /// </summary>
    internal static class BlindajeDialogos
    {
        public static string UsuarioActual => AuditoriaContexto.Usuario ?? Environment.UserName;

        private static Form NuevoForm(string titulo, int ancho, int alto) => new()
        {
            Text = titulo, StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false, MinimizeBox = false, ShowInTaskbar = false, ClientSize = new Size(ancho, alto),
            Font = new Font("Segoe UI", 9F)
        };

        private static Label Etiqueta(string texto, int x, int y, int ancho, int alto = 20) =>
            new() { Text = texto, Location = new Point(x, y), Size = new Size(ancho, alto) };

        /// <summary>Pide la clave maestra y el motivo. True si el archivo quedó desblindado.</summary>
        public static bool Desblindar(IWin32Window owner, ArchivoImportado archivo)
        {
            using var f = NuevoForm("Desblindar archivo", 460, 290);
            var txtClave = new TextBox { Location = new Point(16, 78), Size = new Size(428, 23), UseSystemPasswordChar = true };
            var txtMotivo = new TextBox { Location = new Point(16, 130), Size = new Size(428, 23), MaxLength = 500 };
            var lblError = new Label { Location = new Point(16, 165), Size = new Size(428, 50), ForeColor = Color.Firebrick };
            var btnOk = new Button { Text = "Desblindar", Location = new Point(264, 240), Size = new Size(90, 28) };
            var btnCancelar = new Button { Text = "Cancelar", Location = new Point(360, 240), Size = new Size(84, 28), DialogResult = DialogResult.Cancel };
            f.Controls.AddRange(new Control[]
            {
                Etiqueta(archivo.NombreArchivo, 16, 14, 428), Etiqueta("Clave maestra (no es la de ningún usuario)", 16, 56, 428), txtClave,
                Etiqueta("Motivo", 16, 108, 428), txtMotivo, lblError,
                Etiqueta("Queda registrado quién desblindó, cuándo y por qué.", 16, 215, 428), btnOk, btnCancelar
            });
            f.CancelButton = btnCancelar; f.AcceptButton = btnOk;
            btnOk.Click += (_, _) =>
            {
                lblError.Text = "";
                try
                {
                    BlindajeService.Desblindar(archivo.Id, txtClave.Text, txtMotivo.Text, UsuarioActual);
                    f.DialogResult = DialogResult.OK;
                }
                catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
                {
                    lblError.Text = ex.Message; txtClave.Clear();
                }
            };
            return f.ShowDialog(owner) == DialogResult.OK;
        }

        /// <summary>Define la clave maestra (primera vez) o la cambia pidiendo la actual. True si se guardó.</summary>
        public static bool ClaveMaestra(IWin32Window owner, bool existe)
        {
            using var f = NuevoForm(existe ? "Cambiar la clave maestra" : "Definir la clave maestra", 460, existe ? 340 : 285);
            int y = 14;
            TextBox Campo(string rotulo)
            {
                f.Controls.Add(Etiqueta(rotulo, 16, y, 428)); y += 22;
                var t = new TextBox { Location = new Point(16, y), Size = new Size(428, 23), UseSystemPasswordChar = true };
                f.Controls.Add(t); y += 38; return t;
            }
            var txtActual = existe ? Campo("Clave actual") : null;
            var txtNueva = Campo($"{(existe ? "Clave nueva" : "Clave")} (al menos {BlindajeService.LargoMinimoClave} caracteres)");
            var txtRepetida = Campo("Repetí la clave");
            var lblError = new Label { Location = new Point(16, y - 6), Size = new Size(428, 36), ForeColor = Color.Firebrick };
            var btnOk = new Button { Text = "Guardar clave", Location = new Point(240, f.ClientSize.Height - 40), Size = new Size(110, 28) };
            var btnCancelar = new Button { Text = "Cancelar", Location = new Point(360, f.ClientSize.Height - 40), Size = new Size(84, 28), DialogResult = DialogResult.Cancel };
            f.Controls.AddRange(new Control[] { lblError, btnOk, btnCancelar });
            f.CancelButton = btnCancelar; f.AcceptButton = btnOk;
            btnOk.Click += (_, _) =>
            {
                lblError.Text = "";
                if (txtNueva.Text != txtRepetida.Text) { lblError.Text = "Las dos claves nuevas no coinciden."; return; }
                try
                {
                    if (existe) BlindajeService.CambiarClaveMaestra(txtActual.Text, txtNueva.Text, UsuarioActual);
                    else BlindajeService.DefinirClaveMaestra(txtNueva.Text, UsuarioActual);
                    f.DialogResult = DialogResult.OK;
                }
                catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
                {
                    lblError.Text = ex.Message; txtActual?.Clear();
                }
            };
            return f.ShowDialog(owner) == DialogResult.OK;
        }

        /// <summary>Historial de cambios del movimiento: fecha, campo, valor anterior y nuevo, origen y usuario.</summary>
        public static void Historial(IWin32Window owner, MovimientoProcesado mov)
        {
            List<CambioMovimiento> cambios;
            try { cambios = MovimientoHistorialStorage.ObtenerPorMovimiento(mov.Id); }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo leer el historial: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            using var f = NuevoForm("Historial del movimiento", 860, 420);
            f.FormBorderStyle = FormBorderStyle.Sizable; f.MinimizeBox = false;
            f.Controls.Add(new Label { Text = $"{mov.Fecha:dd/MM/yyyy} · {mov.ConceptoOriginal} · movimiento {mov.Id}", Dock = DockStyle.Top, Height = 28, Padding = new Padding(8, 6, 0, 0) });
            if (cambios.Count == 0)
            {
                f.Controls.Add(new Label { Text = "Este movimiento no tiene cambios registrados.", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter });
            }
            else
            {
                var lista = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true };
                foreach (var (titulo, ancho) in new[] { ("Fecha", 120), ("Campo", 120), ("Valor anterior", 170), ("Valor nuevo", 170), ("Origen", 130), ("Usuario", 110) })
                    lista.Columns.Add(titulo, ancho);
                foreach (var c in cambios)
                    lista.Items.Add(new ListViewItem(new[] { c.Fecha.ToString("dd/MM/yyyy HH:mm"), NombreCampo(c.Campo), Valor(c.Campo, c.ValorAnterior), Valor(c.Campo, c.ValorNuevo), c.Origen, c.Usuario }));
                f.Controls.Add(lista);
                lista.BringToFront();
            }
            f.ShowDialog(owner);
        }

        private static string NombreCampo(string c) => c switch
        {
            "ConceptoEstandar" => "Concepto estándar", "ConceptoFinal" => "Concepto final", "Conciliado" => "Conciliado", "ConciliadoFecha" => "Fecha de conciliado", _ => c
        };

        // El trigger guarda todo como texto: el bit llega como 0/1 y la fecha como yyyy-MM-dd.
        private static string Valor(string campo, string v)
        {
            if (string.IsNullOrEmpty(v)) return "—";
            if (campo == "Conciliado") return v == "1" ? "Sí" : "No";
            if (campo == "ConciliadoFecha" && DateTime.TryParseExact(v, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var d)) return d.ToString("dd/MM/yyyy");
            return v;
        }

        /// <summary>Aviso previo a la baja: cuenta lo modificado en el archivo y pide confirmar.</summary>
        public static bool ConfirmarBaja(IWin32Window owner, ArchivoImportado archivo)
        {
            ModificacionesArchivo mod;
            try { mod = EliminacionMovimientosStorage.ContarModificaciones(archivo.Id); }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo revisar el archivo antes de darlo de baja: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            string texto = mod.HayModificaciones
                ? $"Este archivo tiene {mod.Total} movimiento(s) con modificaciones:\n" +
                  $"  · {mod.ConceptosFinalesEditados} concepto(s) final(es) editado(s)\n  · {mod.CuentasEditadas} cuenta(s) editada(s)\n  · {mod.Conciliados} conciliado(s)\n\n¿Dar de baja igual?"
                : $"¿Dar de baja el archivo '{archivo.NombreArchivo}'? No tiene modificaciones registradas.";
            texto += "\n\nLos movimientos se archivan dentro del archivo, que queda inactivo; el historial se conserva. " +
                     "Si algún movimiento está tomado por una conciliación, no se da de baja nada.";
            return MessageBox.Show(owner, texto, "Confirmar baja de " + archivo.NombreArchivo, MessageBoxButtons.YesNo,
                mod.HayModificaciones ? MessageBoxIcon.Warning : MessageBoxIcon.Question) == DialogResult.Yes;
        }
    }
}
