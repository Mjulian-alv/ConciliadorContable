using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ConciliadorContable.Auth;
using ConciliadorContable.Data;
using ConciliadorContable.Models;
using Microsoft.Data.Sqlite;

namespace ConciliadorContable.Forms
{
    public partial class FormAdminUsuarios : Telerik.WinControls.UI.RadForm
    {
        private List<Usuario> _usuarios = new();
        private Usuario? _usuarioEditando;

        public FormAdminUsuarios()
        {
            InitializeComponent();
            foreach (var mod in Usuario.TodosLosModulos)
                clbPermisos.Items.Add(mod, false);
            Load += (_, _) => CargarUsuarios();
        }

        // ── Carga ─────────────────────────────────────────────────────────

        private void CargarUsuarios()
        {
            _usuarios = ObtenerTodos();
            lbUsuarios.DataSource    = null;
            lbUsuarios.DataSource    = _usuarios;
            lbUsuarios.DisplayMember = "DisplayName";
            LimpiarFormulario();
        }

        private static List<Usuario> ObtenerTodos()
        {
            var lista = new List<Usuario>();
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT Id, Username, PasswordHash, Nombre, Activo, Rol, PermisosJson FROM Usuarios ORDER BY Nombre";
            using var r = cmd.ExecuteReader();
            while (r.Read())
                lista.Add(new Usuario
                {
                    Id           = r.GetInt32(0),
                    Username     = r.GetString(1),
                    PasswordHash = r.GetString(2),
                    Nombre       = r.GetString(3),
                    Activo       = r.GetInt32(4) == 1,
                    Rol          = r.IsDBNull(5) ? "Usuario" : r.GetString(5),
                    PermisosJson = r.IsDBNull(6) ? "[]"      : r.GetString(6),
                });
            return lista;
        }

        // ── Selección de usuario ──────────────────────────────────────────

        private void lbUsuarios_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbUsuarios.SelectedItem is not Usuario u) { LimpiarFormulario(); return; }
            _usuarioEditando = u;
            txtUsername.Text  = u.Username;
            txtNombre.Text    = u.Nombre;
            txtPassword.Text  = "";
            chkActivo.Checked = u.Activo;
            cmbRol.SelectedItem = u.Rol;

            // Permisos
            var permisos = u.Permisos;
            foreach (int i in Enumerable.Range(0, clbPermisos.Items.Count))
            {
                string mod = (string)clbPermisos.Items[i];
                clbPermisos.SetItemChecked(i, permisos.Contains(mod));
            }

            // Ocultar permisos individuales si es Admin
            pnlPermisos.Enabled = u.Rol != "Admin";
            lblPermisosTodos.Visible = u.Rol == "Admin";
        }

        // ── Botones ───────────────────────────────────────────────────────

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            lbUsuarios.SelectedIndex = -1;
            _usuarioEditando = null;
            LimpiarFormulario();
            txtUsername.Focus();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string nombre   = txtNombre.Text.Trim();
            string password = txtPassword.Text;
            string rol      = cmbRol.SelectedItem?.ToString() ?? "Usuario";
            bool   activo   = chkActivo.Checked;

            if (string.IsNullOrEmpty(username))
            { MessageBox.Show("El nombre de usuario es obligatorio.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (string.IsNullOrEmpty(nombre))
            { MessageBox.Show("El nombre completo es obligatorio.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (_usuarioEditando == null && string.IsNullOrEmpty(password))
            { MessageBox.Show("La contraseña es obligatoria para usuarios nuevos.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var permisosSel = Enumerable.Range(0, clbPermisos.Items.Count)
                .Where(i => clbPermisos.GetItemChecked(i))
                .Select(i => (string)clbPermisos.Items[i])
                .ToList();
            string permisosJson = System.Text.Json.JsonSerializer.Serialize(permisosSel);

            using var cn = DatabaseHelper.GetConnection();
            cn.Open();

            if (_usuarioEditando == null)
            {
                // Nuevo usuario
                string hash = AuthService.HashPassword(password);
                using var cmd = cn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO Usuarios (Username, PasswordHash, Nombre, Activo, Rol, PermisosJson)
                    VALUES (@u, @p, @n, @a, @r, @pm)";
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", hash);
                cmd.Parameters.AddWithValue("@n", nombre);
                cmd.Parameters.AddWithValue("@a", activo ? 1 : 0);
                cmd.Parameters.AddWithValue("@r", rol);
                cmd.Parameters.AddWithValue("@pm", permisosJson);
                cmd.ExecuteNonQuery();
            }
            else
            {
                // Editar existente
                using var cmd = cn.CreateCommand();
                if (!string.IsNullOrEmpty(password))
                {
                    cmd.CommandText = @"
                        UPDATE Usuarios SET Username=@u, PasswordHash=@p, Nombre=@n, Activo=@a, Rol=@r, PermisosJson=@pm
                        WHERE Id=@id";
                    cmd.Parameters.AddWithValue("@p", AuthService.HashPassword(password));
                }
                else
                {
                    cmd.CommandText = @"
                        UPDATE Usuarios SET Username=@u, Nombre=@n, Activo=@a, Rol=@r, PermisosJson=@pm
                        WHERE Id=@id";
                }
                cmd.Parameters.AddWithValue("@u",  username);
                cmd.Parameters.AddWithValue("@n",  nombre);
                cmd.Parameters.AddWithValue("@a",  activo ? 1 : 0);
                cmd.Parameters.AddWithValue("@r",  rol);
                cmd.Parameters.AddWithValue("@pm", permisosJson);
                cmd.Parameters.AddWithValue("@id", _usuarioEditando.Id);
                cmd.ExecuteNonQuery();
            }

            CargarUsuarios();
            MessageBox.Show("Usuario guardado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (_usuarioEditando == null) return;
            if (_usuarioEditando.Username.Equals("admin", StringComparison.OrdinalIgnoreCase))
            { MessageBox.Show("No se puede eliminar el usuario admin.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (_usuarioEditando.Id == AuthService.UsuarioActual?.Id)
            { MessageBox.Show("No puede eliminar su propio usuario.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (MessageBox.Show($"¿Eliminar el usuario '{_usuarioEditando.Username}'?",
                    "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "DELETE FROM Usuarios WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", _usuarioEditando.Id);
            cmd.ExecuteNonQuery();

            CargarUsuarios();
        }

        private void cmbRol_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool esAdmin = cmbRol.SelectedItem?.ToString() == "Admin";
            pnlPermisos.Enabled      = !esAdmin;
            lblPermisosTodos.Visible = esAdmin;
        }

        private void LimpiarFormulario()
        {
            _usuarioEditando  = null;
            txtUsername.Text  = "";
            txtNombre.Text    = "";
            txtPassword.Text  = "";
            chkActivo.Checked = true;
            cmbRol.SelectedIndex = 1; // "Usuario"
            for (int i = 0; i < clbPermisos.Items.Count; i++)
                clbPermisos.SetItemChecked(i, false);
            pnlPermisos.Enabled      = true;
            lblPermisosTodos.Visible = false;
        }
    }
}
