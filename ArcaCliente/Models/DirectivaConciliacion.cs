using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ArcaCliente.Models
{
    public enum CampoConciliacion
    {
        Cuit,
        TipoComprobante,
        PuntoVenta,
        Numero,
        Fecha,
        Total
    }

    public class DirectivaConciliacion
    {
        public Guid   Id          { get; set; } = Guid.NewGuid();
        public string Descripcion { get; set; }
        public List<CampoConciliacion> Campos { get; set; } = new();

        /// <summary>Resumen legible de los campos incluidos. No se serializa.</summary>
        [JsonIgnore]
        public string ResumenCampos => Campos.Count == 0
            ? "(sin campos)"
            : string.Join(" + ", Campos.Select(NombreCampo));

        public static string NombreCampo(CampoConciliacion campo) => campo switch
        {
            CampoConciliacion.Cuit            => "CUIT",
            CampoConciliacion.TipoComprobante => "Tipo",
            CampoConciliacion.PuntoVenta      => "Pto. Venta",
            CampoConciliacion.Numero          => "Número",
            CampoConciliacion.Fecha           => "Fecha",
            CampoConciliacion.Total           => "Importe",
            _                                 => campo.ToString()
        };

        /// <summary>Crea la directiva primaria por defecto: CUIT + Tipo + Pto. Venta + Número.</summary>
        public static DirectivaConciliacion CrearPrimaria() => new()
        {
            Descripcion = "Coincidencia exacta (CUIT + Tipo + Pto. Venta + Número)",
            Campos = new List<CampoConciliacion>
            {
                CampoConciliacion.Cuit,
                CampoConciliacion.TipoComprobante,
                CampoConciliacion.PuntoVenta,
                CampoConciliacion.Numero
            }
        };
        public static DirectivaConciliacion CrearUltima() => new()
        {
            Descripcion = "Ultima Oportunidad (CUIT + Tipo + Importe)",
            Campos = new List<CampoConciliacion>
            {
                CampoConciliacion.Cuit,
                CampoConciliacion.TipoComprobante,
                CampoConciliacion.Total
            }
        };

    }
}
