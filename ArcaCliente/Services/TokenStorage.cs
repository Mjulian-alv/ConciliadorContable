using System;
using System.IO;
using System.Text.Json;
using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    internal static class TokenStorage
    {
        private static readonly string StoragePath = Path.Combine(
            AppContext.BaseDirectory,
            "token.json");

        private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

        public static void Save(TokenData data)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(StoragePath)!);
            File.WriteAllText(StoragePath, JsonSerializer.Serialize(data, WriteOptions));
        }

        public static TokenData Load()
        {
            if (!File.Exists(StoragePath))
                return null;
            try
            {
                return JsonSerializer.Deserialize<TokenData>(File.ReadAllText(StoragePath));
            }
            catch
            {
                return null;
            }
        }

        public static void Delete()
        {
            if (File.Exists(StoragePath))
                File.Delete(StoragePath);
        }

        public static bool HasToken() => Load() != null;
    }
}
