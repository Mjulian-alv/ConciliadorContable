using System;
using System.Security.Cryptography;
using System.Text;
using ConciliadorContable.Data;
using ConciliadorContable.Models;
using Microsoft.Data.Sqlite;

namespace ConciliadorContable.Auth
{
    public static class AuthService
    {
        public static Usuario? UsuarioActual { get; private set; }

        public static bool Login(string username, string password)
        {
            string hash = HashPassword(password);

            using var cn = DatabaseHelper.GetConnection();
            cn.Open();

            using var cmd = cn.CreateCommand();
            cmd.CommandText = @"
                SELECT Id, Username, PasswordHash, Nombre, Activo, Rol, PermisosJson
                FROM Usuarios
                WHERE Username = @u AND PasswordHash = @p AND Activo = 1";
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", hash);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                UsuarioActual = new Usuario
                {
                    Id           = reader.GetInt32(0),
                    Username     = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    Nombre       = reader.GetString(3),
                    Activo       = reader.GetInt32(4) == 1,
                    Rol          = reader.IsDBNull(5) ? "Usuario" : reader.GetString(5),
                    PermisosJson = reader.IsDBNull(6) ? "[]"      : reader.GetString(6),
                };
                return true;
            }

            return false;
        }

        public static void Logout() => UsuarioActual = null;

        public static string HashPassword(string password)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
