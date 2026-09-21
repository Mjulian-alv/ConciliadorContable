// Fecha: 21/09/2026 - TAREA: 00041 - Linea: 6 - Directivas de conciliación de percepciones y retenciones
// Propias de PyR: la DirectivaConciliacion de Offline arma claves con CUIT y punto de venta, que
// el mayor de PRESEA no trae. Acá el número tiene dos variantes porque AFIP no siempre lo
// escribe como PRESEA (DREAMCO 166000442096 contra 16600442096; SARANDI 315520 sin punto de
// venta contra 10200315520): los últimos 8 dígitos los emparejan.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ArcaCliente.Models
{
    public enum CampoPyR
    {
        Numero         = 0,
        NumeroUltimos8 = 1,
        Importe        = 2,
        Fecha          = 3,
        Proveedor      = 4
    }

    public class DirectivaPyR
    {
        public Guid           Id          { get; set; } = Guid.NewGuid();
        public string         Descripcion { get; set; }
        public List<CampoPyR> Campos      { get; set; } = new();

        [JsonIgnore]
        public string ResumenCampos => Campos.Count == 0
            ? "(sin campos)"
            : string.Join(" + ", Campos.OrderBy(c => c).Select(NombreCampo));

        public DirectivaPyR Clonar() => new()
        {
            Id = Id, Descripcion = Descripcion, Campos = new List<CampoPyR>(Campos)
        };

        public static string NombreCampo(CampoPyR campo) => campo switch
        {
            CampoPyR.Numero         => "Número",
            CampoPyR.NumeroUltimos8 => "Número (últimos 8)",
            CampoPyR.Importe        => "Importe",
            CampoPyR.Fecha          => "Fecha",
            CampoPyR.Proveedor      => "Proveedor",
            _                       => campo.ToString()
        };

        /// <summary>Texto largo para las casillas del detalle.</summary>
        public static string NombreCampoLargo(CampoPyR campo) => campo switch
        {
            CampoPyR.Numero         => "Número (completo)",
            CampoPyR.NumeroUltimos8 => "Número (últimos 8 dígitos)",
            CampoPyR.Proveedor      => "Proveedor (primeras 6 letras)",
            _                       => NombreCampo(campo)
        };

        /// <summary>
        /// Las tres por defecto, en orden de confianza. La primera es fija (no se edita ni se
        /// borra); la tercera, la más floja, va última.
        /// </summary>
        public static List<DirectivaPyR> CrearPredeterminadas() => new()
        {
            new() { Descripcion = "Número completo",             Campos = { CampoPyR.Numero } },
            new() { Descripcion = "Últimos 8 dígitos + Importe", Campos = { CampoPyR.NumeroUltimos8, CampoPyR.Importe } },
            new() { Descripcion = "Importe + Fecha + Proveedor", Campos = { CampoPyR.Importe, CampoPyR.Fecha, CampoPyR.Proveedor } },
        };
    }
}
