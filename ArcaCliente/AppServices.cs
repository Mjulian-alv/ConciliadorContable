using System;
using System.Collections.Generic;
using ArcaCliente.Models;
using ArcaCliente.Services;

namespace ArcaCliente
{
    internal static class AppServices
    {
        /// <summary>
        /// URL por defecto del servicio web ARCA.
        /// Se usa cuando el perfil no tiene <see cref="PerfilFiscal.ArcaApiUrl"/> configurada.
        /// </summary>
        public const string DefaultArcaApiUrl = "https://localhost:7150/";

        private static ArcaApiService _api;
        private static string _currentApiUrl;

        public static ArcaApiService Api
        {
            get
            {
                if (_api == null)
                {
                    _api = new ArcaApiService(DefaultArcaApiUrl);
                    var saved = TokenStorage.Load();
                    if (saved != null)
                        _api.SetToken(saved.Token);
                    _currentApiUrl = DefaultArcaApiUrl;
                }
                return _api;
            }
        }

        /// <summary>
        /// Obtiene o crea una instancia de <see cref="ArcaApiService"/> usando la URL del perfil especificado.
        /// Si el perfil no tiene URL configurada, usa <see cref="DefaultArcaApiUrl"/>.
        /// </summary>
        /// <param name="perfil">Perfil fiscal con posible configuraci�n de <see cref="PerfilFiscal.ArcaApiUrl"/>.</param>
        public static ArcaApiService GetApiForPerfil(PerfilFiscal perfil)
        {
            string url = !string.IsNullOrWhiteSpace(perfil?.ArcaApiUrl)
                ? perfil.ArcaApiUrl
                : DefaultArcaApiUrl;

            // Si la URL cambi�, recrear el servicio
            if (_api == null || _currentApiUrl != url)
            {
                _api?.Dispose();
                _api = new ArcaApiService(url);
                var saved = TokenStorage.Load();
                if (saved != null)
                    _api.SetToken(saved.Token);
                _currentApiUrl = url;
            }

            return _api;
        }

        public static TokenData CurrentToken => TokenStorage.Load();

        /// <summary>
        /// Persiste el token tras un login o registro exitoso.
        /// </summary>
        public static void SaveToken(AuthResponse auth, string email)
        {
            TokenStorage.Save(new TokenData
            {
                Token = auth.Token,
                UserId = auth.UserId,
                Email = email,
                SavedAt = DateTime.Now
            });
            Api.SetToken(auth.Token);
        }

        /// <summary>
        /// Elimina el token local y limpia la sesi�n en memoria.
        /// </summary>
        public static void Logout()
        {
            TokenStorage.Delete();
            _api?.SetToken(null);
        }

        // ?? Perfiles fiscales (online) ????????????????????????????????????????????

        public static List<PerfilFiscal> Perfiles => PerfilStorage.Load();

        public static void SavePerfiles(List<PerfilFiscal> perfiles) =>
            PerfilStorage.Save(perfiles);

        // ?? Perfiles offline ??????????????????????????????????????????????????????

        public static List<PerfilOffline> PerfilesOffline => PerfilOfflineStorage.Load();

        public static void SavePerfilesOffline(List<PerfilOffline> perfiles) =>
            PerfilOfflineStorage.Save(perfiles);

        // ?? PRESEA: proveedores (mapa por CUIT) ???????????????????????????????????

        public static List<ConfigPreseaProveedor> PreseaProveedores =>
            PreseaProveedorStorage.Load();

        public static ConfigPreseaProveedor GetPreseaProveedor(string cuit) =>
            PreseaProveedorStorage.Get(cuit);

        public static void SavePreseaProveedor(ConfigPreseaProveedor proveedor) =>
            PreseaProveedorStorage.Upsert(proveedor);

        public static void SavePreseaProveedores(System.Collections.Generic.IEnumerable<ConfigPreseaProveedor> proveedores) =>
            PreseaProveedorStorage.UpsertRange(proveedores);

        // ?? PRESEA: memoria anti-duplicado de exportados ??????????????????????????

        public static bool PreseaYaExportado(string clave) =>
            PreseaExportMemoryStorage.YaExportado(clave);

        public static System.Collections.Generic.HashSet<string> PreseaClavesExportadas() =>
            PreseaExportMemoryStorage.ClavesExportadas();

        public static void RegistrarPreseaExportado(PreseaComprobanteExportado exportado) =>
            PreseaExportMemoryStorage.Registrar(exportado);
    }
}
