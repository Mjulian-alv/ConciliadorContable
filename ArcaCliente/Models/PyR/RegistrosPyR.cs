// Fecha: 21/09/2026 - TAREA: 00041 - Linea: 3, 4 - Filas normalizadas de ARCA y de PRESEA para la conciliación PyR
// Los dos lados dejan el número de comprobante con la misma forma (sólo dígitos, sin ceros a la
// izquierda) porque así lo escribe PRESEA en el concepto: "Factura A000200118618" de ARCA y
// "SEGUN FACTURA A 200118618 de ..." de PRESEA tienen que dar el mismo 200118618.
using System;
using System.Collections.Generic;

namespace ArcaCliente.Models
{
    /// <summary>Un certificado de retención/percepción leído de la carpeta de ARCA.</summary>
    public class RegistroArcaPyR
    {
        public ImpuestoPyR      Impuesto          { get; set; }
        public TipoOperacionPyR Operacion         { get; set; }
        public DateTime?        Fecha             { get; set; }
        public string           Cuit              { get; set; }
        public string           Denominacion      { get; set; }
        public string           TipoComprobante   { get; set; }
        public string           Letra             { get; set; }
        public string           Numero            { get; set; }
        public string           NumeroCertificado { get; set; }
        public decimal          Importe           { get; set; }
        public string           ArchivoOrigen     { get; set; }

        public string ImpuestoTexto  => CuentaPyR.NombreImpuesto(Impuesto);
        public string OperacionTexto => CuentaPyR.NombreTipo(Operacion);
    }

    /// <summary>Una fila del mayor de PRESEA de una cuenta.</summary>
    public class RegistroPreseaPyR
    {
        public CuentaPyR Cuenta           { get; set; }
        public DateTime? Fecha            { get; set; }
        public string    Asiento          { get; set; }
        public string    ConceptoOriginal { get; set; }
        public string    TipoComprobante  { get; set; }
        public string    Numero           { get; set; }
        public string    Proveedor        { get; set; }
        public bool      EsAnulacion      { get; set; }
        /// <summary>El concepto no respeta el patrón (ej. "Según MINUTA FINANCIERA Nº ...").</summary>
        public bool      SinComprobante   { get; set; }
        /// <summary>Debe − haber, con signo.</summary>
        public decimal   Importe          { get; set; }
        public string    ArchivoOrigen    { get; set; }

        public string CuentaNombre => Cuenta?.Nombre;
        public string AnulacionTexto => EsAnulacion ? "Sí" : string.Empty;
    }

    /// <summary>Un mayor de PRESEA cargado en la pantalla, atado a una cuenta del perfil.</summary>
    public class MayorPreseaPyR
    {
        public string                  Archivo   { get; set; }
        public string                  Ruta      { get; set; }
        public CuentaPyR               Cuenta    { get; set; }
        public List<RegistroPreseaPyR> Registros { get; set; } = new();

        public string CuentaTexto        => Cuenta?.ToString();
        public int    Filas              => Registros.Count;
        public int    FilasSinComprobante => Registros.FindAll(r => r.SinComprobante).Count;
    }

    /// <summary>Resultado de leer la carpeta de ARCA: lo leído y lo que no se pudo reconocer.</summary>
    public class CargaArcaPyR
    {
        public List<RegistroArcaPyR> Registros            { get; } = new();
        public List<string>          ArchivosLeidos       { get; } = new();
        /// <summary>Nombre de archivo → motivo.</summary>
        public List<(string Archivo, string Motivo)> ArchivosNoReconocidos { get; } = new();
        /// <summary>Archivo repetido → archivo idéntico que sí se leyó.</summary>
        public List<(string Archivo, string IgualA)> ArchivosRepetidos { get; } = new();
        public int                   FilasNoReconocidas   { get; set; }
    }
}
