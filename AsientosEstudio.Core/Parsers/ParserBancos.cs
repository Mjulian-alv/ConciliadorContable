// Fecha: 12/11/2026 - TAREA: 00180 - Linea: 1 - Parser BANCOS (§4.1): una hoja por banco, marcador «MES mm/aaaa», encabezado «ASIENTO n»,
// fila de títulos «Código | Cuenta / concepto | Debe | Haber», cierre «SUMATORIA DEL ASIENTO» (se ignora).
// Columnas: A = código, B = nombre de la cuenta, C = Debe, D = Haber. El resto de las columnas (la hoja NBSF tiene 11) no se mira.
// Se detecta por patrones de texto, nunca por número de fila.
using System.Text.RegularExpressions;

namespace AsientosEstudio.Core;

public sealed class ParserBancos : IAsientoParser
{
    public TipoPlanilla Tipo => TipoPlanilla.Bancos;

    private static readonly Regex EncabezadoAsiento = new(@"^ASIENTO\s+(\d+)\b(.*)$", RegexOptions.Compiled);

    public IReadOnlyList<HojaLeida> Leer(LibroCrudo libro) => libro.Hojas.Select(LeerHoja).ToList();

    private static HojaLeida LeerHoja(HojaCruda h)
    {
        var asientos = new List<AsientoLeido>();
        Mes? mes = null;
        Borrador? actual = null;

        void Cerrar()
        {
            if (actual is not null) asientos.Add(actual.Construir(h.Nombre));
            actual = null;
        }

        for (var fila = 1; fila <= h.UltimaFila; fila++)
        {
            var a = h.Texto(fila, 1);
            var b = h.Texto(fila, 2);
            var hayImporte = h.TieneImporte(fila, 3) || h.TieneImporte(fila, 4);
            var normA = Texto.Normalizar(a);

            // «MES 05/2026» en la columna A, solo en la fila.
            if (b.Length == 0 && !hayImporte && Meses.TryParse(a, out var m))
            {
                Cerrar();
                mes = m;
                continue;
            }

            var enc = EncabezadoAsiento.Match(normA);
            if (enc.Success)
            {
                Cerrar();
                var resto = enc.Groups[2].Value;
                var informativo = resto.Contains("INFORMATIVO", StringComparison.Ordinal) || resto.Contains("NO SE REGISTRA", StringComparison.Ordinal);
                actual = new Borrador(enc.Groups[1].Value, a, fila, mes, informativo);
                continue;
            }

            // Lo que está fuera de un asiento (el título de la hoja, las notas del pie) no es un movimiento.
            if (actual is null) continue;

            if (normA == "CODIGO" && Texto.Normalizar(b).StartsWith("CUENTA", StringComparison.Ordinal)) continue;

            if (Texto.Normalizar(b) == "SUMATORIA DEL ASIENTO")
            {
                Cerrar();
                continue;
            }

            if (a.Length == 0 && b.Length == 0 && !hayImporte) continue; // fila en blanco

            // Una nota suelta dentro del asiento: texto en A, sin nombre ni importes.
            if (a.Length > 0 && !Texto.PareceCodigo(a) && b.Length == 0 && !hayImporte) continue;

            // Una línea. Sin código es A1; sin código y sin nombre es la fila de totales suelta (A3): se conserva para avisarla.
            actual.Lineas.Add(new LineaLeida(fila, a, b, h.Importe(fila, 3), h.Importe(fila, 4)));
        }

        Cerrar();
        return new HojaLeida { Nombre = h.Nombre, Orden = h.Orden, Asientos = asientos };
    }

    private sealed class Borrador
    {
        private readonly string _numero, _titulo;
        private readonly int _filaEncabezado;
        private readonly Mes? _mes;
        private readonly bool _informativo;

        public Borrador(string numero, string titulo, int filaEncabezado, Mes? mes, bool informativo)
        {
            _numero = numero; _titulo = titulo; _filaEncabezado = filaEncabezado; _mes = mes; _informativo = informativo;
        }

        public List<LineaLeida> Lineas { get; } = new();

        public AsientoLeido Construir(string hoja)
        {
            var estado = _informativo ? EstadoAsiento.InformativoExcluido
                : Lineas.All(l => l.EnCero) ? EstadoAsiento.VacioOmitido
                : EstadoAsiento.Registrable;
            return new AsientoLeido
            {
                Hoja = hoja,
                Mes = _mes,
                NumeroOrigen = _numero,
                Titulo = _titulo,
                // Concepto de BANCOS: «MM-AAAA-NOMBRE DE LA HOJA» (§4.1), con el nombre de la hoja tal cual.
                Concepto = _mes is { } m ? m.TextoGuion + "-" + hoja : "",
                Estado = estado,
                FilaEncabezado = _filaEncabezado,
                Lineas = Lineas,
            };
        }
    }
}
