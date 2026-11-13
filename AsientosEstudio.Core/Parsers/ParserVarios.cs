// Fecha: 12/11/2026 - TAREA: 00180 - Linea: 3 - Parser VARIOS (§4.3): hojas por circuito (VENTAS, COMPRAS, CAJAS, CHEQUES…), varios meses por hoja uno debajo de otro.
// - Marcador de mes con dos formatos: «05-2026» (VENTAS) o «MAYO 2026» (el resto).
// - Encabezado de asiento: texto solo en A SEGUIDO de la fila «Código…» o directamente de una línea con código. Así se distingue de las notas
//   («Por operaciones de extracash…», «USD 15.000,00 | Tipo de cambio…»), que no tienen eso detrás y se ignoran.
// - Concepto = el texto del encabezado sin el prefijo «ASIENTO n — » si lo tiene.
// - Cierre: «TOTAL ASIENTO» (en A, VENTAS) o «TOTAL DEL ASIENTO» (en B, el resto). No es una línea.
// - Las columnas F a N (cálculos auxiliares, comentarios, tablas de cheques) no se leen: solo A a D.
// - El bloque «Asiendo del ejercicio anterior … ASIENTO QUE NO SE REGISTRA» se excluye (A10).
using System.Text.RegularExpressions;

namespace AsientosEstudio.Core;

public sealed class ParserVarios : IAsientoParser
{
    public TipoPlanilla Tipo => TipoPlanilla.Varios;

    // «ASIENTO 1 — VENTAS, COMISIONES E IMPUESTOS»: raya, guion medio o dos puntos después del número.
    private static readonly Regex PrefijoAsiento = new(@"^\s*ASIENTO\s+(\d+)\s*[—–\-:]\s*(.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public IReadOnlyList<HojaLeida> Leer(LibroCrudo libro) => libro.Hojas.Select(LeerHoja).ToList();

    private static HojaLeida LeerHoja(HojaCruda h)
    {
        var asientos = new List<AsientoLeido>();
        Mes? mes = null;
        BorradorAsiento? actual = null;
        var bloqueExcluido = false; // el asiento abierto es el del ejercicio anterior

        void Cerrar()
        {
            if (actual is not null) asientos.Add(actual.Construir(h.Nombre));
            actual = null;
            bloqueExcluido = false;
        }

        for (var fila = 1; fila <= h.UltimaFila; fila++)
        {
            if (!Filas.TieneContenido(h, fila)) continue;

            var a = h.Texto(fila, 1);
            var b = h.Texto(fila, 2);
            var hayImporte = h.TieneImporte(fila, 3) || h.TieneImporte(fila, 4);
            var normA = Texto.Normalizar(a);
            var soloTextoEnA = Filas.EsSoloTextoEnA(h, fila);

            // Marcador de mes: texto solo en A que se lee como mes, en cualquiera de los dos formatos.
            if (soloTextoEnA && Meses.TryParse(a, out var m))
            {
                Cerrar();
                mes = m;
                continue;
            }

            // Cierre del asiento.
            if (normA.StartsWith("TOTAL", StringComparison.Ordinal) || Texto.Normalizar(b).StartsWith("TOTAL", StringComparison.Ordinal))
            {
                Cerrar();
                continue;
            }

            // El bloque del ejercicio anterior: empieza en «Asiendo del ejercicio anterior» (con el error de tipeo del original) y termina en
            // «ASIENTO QUE NO SE REGISTRA». Se lee entero como un asiento excluido para poder listarlo (A10), no se descarta en silencio.
            if (soloTextoEnA && normA.Contains("EJERCICIO ANTERIOR", StringComparison.Ordinal))
            {
                Cerrar();
                actual = new BorradorAsiento("", a, "", fila, mes, noSeRegistra: true, estadoSiNoSeRegistra: EstadoAsiento.NoSeRegistra);
                bloqueExcluido = true;
                continue;
            }
            if (soloTextoEnA && normA.Contains("NO SE REGISTRA", StringComparison.Ordinal) && bloqueExcluido)
            {
                Cerrar();
                continue;
            }

            if (Filas.EsEncabezadoPorContexto(h, fila))
            {
                Cerrar();
                var (numero, concepto) = SepararPrefijo(a);
                var noSeRegistra = normA.Contains("NO SE REGISTRA", StringComparison.Ordinal);
                var informativo = normA.Contains("INFORMATIVO", StringComparison.Ordinal);
                actual = new BorradorAsiento(numero, a, concepto, fila, mes, noSeRegistra || informativo,
                    noSeRegistra ? EstadoAsiento.NoSeRegistra : EstadoAsiento.InformativoExcluido);
                bloqueExcluido = noSeRegistra;
                continue;
            }

            // Fuera de un asiento: el título de la hoja, las notas de debajo de un asiento.
            if (actual is null) continue;
            if (Filas.EsTitulos(h, fila)) continue;

            // Una nota pegada al asiento: texto en A, sin nombre ni importes.
            if (soloTextoEnA && !Texto.PareceCodigo(a)) continue;

            actual.Lineas.Add(Filas.Linea(h, fila, a, b));
        }

        Cerrar();
        return new HojaLeida { Nombre = h.Nombre, Orden = h.Orden, Asientos = asientos };
    }

    /// <summary>«ASIENTO 1 — VENTAS, COMISIONES E IMPUESTOS» → («1», «VENTAS, COMISIONES E IMPUESTOS»). Sin prefijo, el texto entero es el concepto.</summary>
    private static (string Numero, string Concepto) SepararPrefijo(string encabezado)
    {
        var m = PrefijoAsiento.Match(encabezado);
        return m.Success ? (m.Groups[1].Value, m.Groups[2].Value.Trim()) : ("", encabezado.Trim());
    }
}
