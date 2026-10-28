using System.Collections.Generic;
using System.Text.Json;

namespace ConciliadorContable.Models
{
    public class Usuario
    {
        public int    Id           { get; set; }
        public string Username     { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Nombre       { get; set; } = string.Empty;
        public bool   Activo       { get; set; } = true;
        public string Rol          { get; set; } = "Usuario"; // "Admin" | "Usuario"
        public string PermisosJson { get; set; } = "[]";

        // Fecha: 28/10/2026 - TAREA: 00140 - Linea: 3,4 - Correo de Microsoft e id de Microsoft (lo guarda la web al primer ingreso por AppHub).
        public string Correo       { get; set; } = string.Empty;
        public string OidMicrosoft { get; set; } = string.Empty;
        /// <summary>Marcador de contraseña de los usuarios de Microsoft: nunca coincide con un hash SHA-256.</summary>
        public const string SinClave = "!";

        public List<string> Permisos
        {
            get
            {
                try { return JsonSerializer.Deserialize<List<string>>(PermisosJson) ?? new(); }
                catch { return new(); }
            }
            set => PermisosJson = JsonSerializer.Serialize(value);
        }

        public bool TienePermiso(string modulo) =>
            Rol == "Admin" || Permisos.Contains(modulo);

        // Fecha: 28/10/2026 - TAREA: 00140 - Linea: 3,4 - La lista marca a quien entra con Microsoft y a quien todavía no tiene permisos.
        public string DisplayName => $"{Nombre} ({Username}){(Activo ? "" : " [Inactivo]")}{(Rol == "Admin" ? " ⭐" : "")}{MarcaMicrosoft}";

        private string MarcaMicrosoft => string.IsNullOrEmpty(Correo) ? "" : (Rol != "Admin" && Permisos.Count == 0 ? " [Microsoft · sin permisos]" : " [Microsoft]");

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 1 - Modulo nuevo: catalogo de cuentas contables
        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Modulo nuevo: conciliacion interna
        public static readonly string[] TodosLosModulos =
        {
            "ArcaOffline", "ArcaPerfiles", "ArcaEquivalencias",
            "AgrProcesador", "AgrHomologaciones", "AgrConciliacion", "AgrCuentasContables",
            "AgrConciliacionInterna",
            "AgrClaveMaestra", // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 3 - Definir y cambiar la clave maestra del blindaje
            // Fecha: 21/09/2026 - TAREA: 00041 - Linea: 3, 4 - Modulo nuevo: conciliacion de percepciones y retenciones
            "ArcaPyR"
        };

        public static readonly Dictionary<string, string> NombresModulos = new()
        {
            ["ArcaOffline"]         = "ARCA - Comprobantes Offline",
            ["ArcaPerfiles"]        = "ARCA - Perfiles Offline",
            ["ArcaEquivalencias"]   = "ARCA - Equivalencias",
            ["AgrProcesador"]       = "Agrupador - Procesador",
            ["AgrHomologaciones"]   = "Agrupador - Homologaciones",
            ["AgrConciliacion"]     = "Agrupador - Conciliación Externa",
            ["AgrCuentasContables"] = "Agrupador - Cuentas Contables",
            ["AgrConciliacionInterna"] = "Agrupador - Conciliación Interna",
            ["AgrClaveMaestra"]     = "Agrupador - Clave maestra del blindaje", // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 3
            ["ArcaPyR"]             = "ARCA - Percepciones y Retenciones",
        };
    }
}
