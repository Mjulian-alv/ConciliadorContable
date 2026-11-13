// Fecha: 12/11/2026 - TAREA: 00180 - Linea: 2 - Parser DEVENGAMIENTOS (§4.2): una hoja por mes, nombrada «MM-AAAA». Cada asiento tiene una línea
// de encabezado de texto («IVA - Asiento principal»), la fila «Código de cuenta | Nombre de la cuenta | Debe | Haber» (a veces «Codigo»),
// sus líneas y una fila TOTAL. El concepto del TXT es el texto del encabezado. Un asiento sin ninguna línea de cuenta se omite (A8).
namespace AsientosEstudio.Core;

public sealed class ParserDevengamientos : IAsientoParser
{
    public TipoPlanilla Tipo => TipoPlanilla.Devengamientos;

    public IReadOnlyList<HojaLeida> Leer(LibroCrudo libro) => libro.Hojas.Select(LeerHoja).ToList();

    private static HojaLeida LeerHoja(HojaCruda h)
    {
        var asientos = new List<AsientoLeido>();
        // El mes es el nombre de la hoja. Si una hoja no se llama como un mes sus asientos quedan sin mes, y el validador lo avisa.
        Mes? mes = Meses.TryParse(h.Nombre, out var m) ? m : null;
        BorradorAsiento? actual = null;

        void Cerrar()
        {
            if (actual is not null) asientos.Add(actual.Construir(h.Nombre));
            actual = null;
        }

        for (var fila = 1; fila <= h.UltimaFila; fila++)
        {
            if (!Filas.TieneContenido(h, fila)) continue;

            // La fila TOTAL cierra el asiento y no es un movimiento. Puede estar en la columna A («TOTAL») o en la B. Se mira antes que
            // el encabezado: un TOTAL sin importes cargados es «solo texto en A» y, con otro asiento debajo, parecería uno.
            if (Texto.Normalizar(h.Texto(fila, 1)) == "TOTAL" || Texto.Normalizar(h.Texto(fila, 2)) == "TOTAL")
            {
                Cerrar();
                continue;
            }

            // Un texto solo en A seguido de los títulos de columnas (o de una línea con código) abre un asiento nuevo, aunque el anterior
            // no haya cerrado con TOTAL. «DEVENGAMIENTOS 05-2026», el título de la hoja, no tiene eso detrás y no abre nada.
            if (Filas.EsEncabezadoPorContexto(h, fila))
            {
                Cerrar();
                var titulo = h.Texto(fila, 1);
                actual = new BorradorAsiento("", titulo, titulo, fila, mes);
                continue;
            }

            if (actual is null) continue;
            if (Filas.EsTitulos(h, fila)) continue;

            var a = h.Texto(fila, 1);
            var b = h.Texto(fila, 2);
            var hayImporte = h.TieneImporte(fila, 3) || h.TieneImporte(fila, 4);

            // Una nota suelta: texto en A, sin nombre ni importes.
            if (a.Length > 0 && !Texto.PareceCodigo(a) && b.Length == 0 && !hayImporte) continue;

            actual.Lineas.Add(new LineaLeida(fila, a, b, h.Importe(fila, 3), h.Importe(fila, 4)));
        }

        Cerrar();
        return new HojaLeida { Nombre = h.Nombre, Orden = h.Orden, Asientos = asientos };
    }
}
