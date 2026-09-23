// Fecha: 21/09/2026 - TAREA: 00041 - Linea: 2 - Cuenta contable de percepciones/retenciones del perfil PyR
// El mayor de PRESEA es de una sola cuenta y no la trae en el archivo: la cuenta se elige al
// levantarlo, y de ella salen el tipo (percepción/retención) y el impuesto contra los que se
// va a conciliar con ARCA.
using System;
using System.Text.Json.Serialization;

namespace ArcaCliente.Models
{
    public enum TipoOperacionPyR
    {
        Percepcion = 0,
        Retencion  = 1
    }

    public enum ImpuestoPyR
    {
        Iva       = 0,
        Iibb      = 1,
        Ganancias = 2
    }

    public class CuentaPyR
    {
        public Guid             Id       { get; set; } = Guid.NewGuid();
        public string           Codigo   { get; set; }
        public string           Nombre   { get; set; }
        public TipoOperacionPyR Tipo     { get; set; } = TipoOperacionPyR.Percepcion;
        public ImpuestoPyR      Impuesto { get; set; } = ImpuestoPyR.Iva;

        public CuentaPyR Clonar() => (CuentaPyR)MemberwiseClone();

        /// <summary>"114105 — Percepciones IVA (Percepción · IVA)", como en el combo de elegir cuenta.</summary>
        [JsonIgnore]
        public string Descripcion => $"{Codigo} — {Nombre} ({NombreTipo(Tipo)} · {NombreImpuesto(Impuesto)})";

        [JsonIgnore] public string TipoTexto     => NombreTipo(Tipo);
        [JsonIgnore] public string ImpuestoTexto => NombreImpuesto(Impuesto);

        public override string ToString() => $"{Codigo} — {Nombre}";

        public static string NombreTipo(TipoOperacionPyR tipo) =>
            tipo == TipoOperacionPyR.Retencion ? "Retención" : "Percepción";

        public static string NombreImpuesto(ImpuestoPyR impuesto) => impuesto switch
        {
            ImpuestoPyR.Iibb      => "IIBB",
            ImpuestoPyR.Ganancias => "Ganancias",
            _                     => "IVA"
        };
    }
}
