using System;
using System.Data;
using System.Text.Json.Serialization;

namespace ArcaCliente.Models
{
    public class PerfilFiscal
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nombre { get; set; }
        public string Username { get; set; }  // CUIL sin guiones
        public string Password { get; set; }  // Clave fiscal
        public string Cuit { get; set; }      // CUIT con guiones (representada)

        // ?? Integración contable ?????????????????????????????????????????????????

        /// <summary>
        /// Indica si este perfil tiene activa la integración con un sistema contable.
        /// Cuando es false, ArcaCliente solo hace conciliación visual sin dar de alta documentos.
        /// </summary>
        public bool IntegracionHabilitada { get; set; } = false;

        /// <summary>
        /// Sistema contable destino para la integración.
        /// Solo aplica cuando <see cref="IntegracionHabilitada"/> es true.
        /// </summary>
        public SistemaIntegracion Sistema { get; set; } = SistemaIntegracion.Ninguno;

        // ?? Conciliación ??????????????????????????????????????????????????????

        /// <summary>
        /// Lista ordenada de directivas de conciliación.
        /// La primera es la directiva principal y no puede modificarse.
        /// Cada directiva sucesiva actúa sobre el residual (no emparejado) de la anterior.
        /// </summary>
        public System.Collections.Generic.List<DirectivaConciliacion> DirectivasConciliacion { get; set; } = new();

        /// <summary>
        /// Cadena de conexión específica del perfil para la conciliación.
        /// Si es null o vacía, se usa <see cref="Services.ConciliacionConfig.ConnectionString"/>.
        /// </summary>
        public string ConciliacionConnectionString { get; set; }

        /// <summary>
        /// Query específico del perfil para la conciliación.
        /// Si es null o vacío, se usa <see cref="Services.ConciliacionConfig.Query"/>.
        /// </summary>
        public string ConciliacionQuery { get; set; }

        /// <summary>
        /// Fábrica de conexión específica del perfil. No se persiste en JSON.
        /// Permite override programático del proveedor de base de datos (MySQL, Npgsql, etc.).
        /// Si es null, se deriva de <see cref="ConciliacionConnectionString"/> o del default.
        /// </summary>
        [JsonIgnore]
        public Func<IDbConnection> ConciliacionConnectionFactory { get; set; }

        // ?? Integración Octosis ??????????????????????????????????????????????

        /// <summary>
        /// Cadena de conexión específica del perfil para la integración con Octosis.
        /// Si es null o vacía, se usa <see cref="ConciliacionConnectionString"/> (misma base).
        /// </summary>
        public string OctosisConnectionString { get; set; }

        /// <summary>
        /// Fábrica de conexión específica del perfil para Octosis. No se persiste en JSON.
        /// Permite override programático del proveedor de base de datos.
        /// Si es null, se deriva de <see cref="OctosisConnectionString"/> o del default.
        /// </summary>
        [JsonIgnore]
        public Func<IDbConnection> OctosisConnectionFactory { get; set; }

        // ?? Servicio Web ARCA ?????????????????????????????????????????????????

        /// <summary>
        /// URL base del servicio web ARCA (API backend) para este perfil.
        /// Si es null o vacía, se usa <see cref="AppServices.DefaultArcaApiUrl"/>.
        /// Ejemplo: "https://localhost:7250/" o "https://api.arca.miempresa.com/"
        /// </summary>
        public string ArcaApiUrl { get; set; }

        public override string ToString() => Nombre ?? string.Empty;
    }
}
