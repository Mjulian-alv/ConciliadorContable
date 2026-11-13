// Fecha: 13/11/2026 - TAREA: 00180 - Linea: 5 - Validador (§6): recorre el libro leído y arma las alertas A1–A13 y A15 (más M1), y calcula cada asiento tal
// como va al TXT (importes redondeados, ajuste de centavo). No escribe nada ni decide por el usuario: las decisiones (ajustar una línea, aceptar una
// alerta) entran como parámetro y quedan marcadas en la alerta con quién las tomó. Sin plan de cuentas: A14 queda fuera (ver Alertas.cs).
// Fecha: 13/11/2026 - TAREA: 00180 - Linea: 4 - La selección de meses (OpcionesValidacion.Meses) y el rango habitual (A9) entran por acá.
using System.Globalization;
using System.Text.RegularExpressions;

namespace AsientosEstudio.Core;

public sealed record OpcionesValidacion
{
    /// <summary>Los meses a incluir. null = todos los que haya. Un asiento de un mes no incluido no se valida ni sale en el TXT.</summary>
    public IReadOnlySet<Mes>? Meses { get; init; }

    /// <summary>El rango que se esperaba (p. ej. el que dice el nombre del archivo). Un mes fuera de él da A9. null = no se compara.</summary>
    public (Mes Desde, Mes Hasta)? RangoHabitual { get; init; }

    public IReadOnlyList<DecisionAjuste> Ajustes { get; init; } = Array.Empty<DecisionAjuste>();
    public IReadOnlyList<DecisionAceptar> Aceptaciones { get; init; } = Array.Empty<DecisionAceptar>();
}

public static class Validador
{
    // Un código de cuenta ya normalizado: dígitos, y si hay más de un nivel, separados por puntos.
    private static readonly Regex CodigoValido = new(@"^\d+(\.\d+)*$", RegexOptions.Compiled);

    public static ResultadoValidacion Validar(LibroLeido libro, OpcionesValidacion? opciones = null)
    {
        opciones ??= new OpcionesValidacion();
        var alertas = new List<Alerta>();
        var calculados = new List<AsientoCalculado>();
        var decimalesPorHoja = new Dictionary<string, (int Lineas, HashSet<ClaveAsiento> Asientos)>();
        var nombresPorCodigo = new Dictionary<string, (HashSet<string> Nombres, HashSet<string> Hojas)>();

        AlertarMesesFueraDeRango(libro, opciones, alertas);

        foreach (var hoja in libro.Hojas)
            foreach (var a in hoja.Asientos)
            {
                if (a.Mes is { } m && opciones.Meses is { } incluidos && !incluidos.Contains(m)) continue; // el usuario no pidió ese mes

                switch (a.Estado)
                {
                    case EstadoAsiento.InformativoExcluido:
                        alertas.Add(Nueva("A7", Severidad.Informativa, "Asiento informativo excluido", "«INFORMATIVO — NO SE REGISTRA» en el encabezado.",
                            "No se valida su balance: algunos solo tienen Debe, y es lo esperable.", a));
                        calculados.Add(Omitido(a));
                        break;
                    case EstadoAsiento.VacioOmitido:
                        alertas.Add(Nueva("A8", Severidad.Informativa, "Asiento vacío o en cero omitido", "No tiene líneas con importe.",
                            "No se registra y no consume número correlativo.", a));
                        calculados.Add(Omitido(a));
                        break;
                    case EstadoAsiento.NoSeRegistra:
                        alertas.Add(Nueva("A10", Severidad.Informativa, "Bloque «no se registra» o del ejercicio anterior", "El bloque está marcado como que no se registra.",
                            "Se excluye y se lista.", a));
                        calculados.Add(Omitido(a));
                        break;
                    default:
                        var (calc, locales) = ValidarRegistrable(a, opciones, decimalesPorHoja, nombresPorCodigo);
                        calculados.Add(calc);
                        alertas.AddRange(locales);
                        break;
                }
            }

        foreach (var (hoja, (lineas, asientos)) in decimalesPorHoja)
            alertas.Add(new Alerta
            {
                Codigo = "A12", Severidad = Severidad.Informativa, Titulo = "Importes con más de 2 decimales en el origen", Hoja = hoja,
                Causa = $"{lineas} líneas de {asientos.Count} asientos traen más de 2 decimales.",
                Accion = "Se redondearon a 2 decimales, mitad hacia arriba, línea por línea.", Detalle = $"{lineas} líneas",
            });

        foreach (var (codigo, (nombres, hojas)) in nombresPorCodigo.Where(kv => kv.Value.Nombres.Count > 1))
            alertas.Add(new Alerta
            {
                Codigo = "A15", Severidad = Severidad.Informativa, Titulo = "Código de cuenta repetido con nombres distintos", Cuenta = codigo,
                Causa = $"El código {codigo} se usa con {nombres.Count} nombres: {string.Join(", ", nombres.Take(6))}{(nombres.Count > 6 ? "…" : "")}.",
                Accion = "Es normal (p. ej. varios IVA crédito fiscal en la misma cuenta). Se muestra solo en el detalle.", Detalle = string.Join(" · ", hojas.Take(6)),
            });

        return new ResultadoValidacion { Asientos = calculados, Alertas = alertas };
    }

    // ------------------------------------------------------------------ un asiento registrable
    private static (AsientoCalculado, List<Alerta>) ValidarRegistrable(AsientoLeido a, OpcionesValidacion op,
        Dictionary<string, (int Lineas, HashSet<ClaveAsiento> Asientos)> decimalesPorHoja,
        Dictionary<string, (HashSet<string> Nombres, HashSet<string> Hojas)> nombresPorCodigo)
    {
        var locales = new List<Alerta>();
        var clave = new ClaveAsiento(a.Hoja, a.FilaEncabezado);
        var hayErrorDeLectura = false;

        if (a.Mes is null)
            locales.Add(Nueva("M1", Severidad.Bloqueante, "Asiento sin mes", "El asiento está antes de cualquier marcador de mes de su hoja, así que no tiene fecha.",
                "Poné el marcador de mes («MES 05/2026», «MAYO 2026») arriba del asiento en el Excel y volvé a cargarlo.", a));

        // Las filas sin código ni nombre no son movimientos (A3): se avisan y quedan afuera.
        foreach (var s in a.Lineas.Where(l => l.FilaSuelta))
            locales.Add(Nueva("A3", Severidad.Advertencia, "Fila de totales suelta",
                $"Fila sin código ni nombre con Debe {Importe(s.Debe)} y Haber {Importe(s.Haber)} cargados a mano.",
                "Se ignora: una fila sin código y sin nombre no es un movimiento. Conviene borrarla del Excel.", a, s, resolucion: ResolucionAlerta.Ignorada,
                detalle: $"Debe {Importe(s.Debe)} · Haber {Importe(s.Haber)}"));

        var efectivas = a.Lineas.Where(l => !l.FilaSuelta).ToList();
        var codigos = new string[efectivas.Count];

        for (var i = 0; i < efectivas.Count; i++)
        {
            var l = efectivas[i];
            var cuenta = l.Nombre.Length > 0 ? l.Nombre : l.Codigo;

            // A11: un importe que no se pudo leer. Se bloquea con la celda exacta; nunca se lee como cero en silencio.
            foreach (var (col, problema) in new[] { ("C", l.ProblemaDebe), ("D", l.ProblemaHaber) })
            {
                if (problema == ProblemaImporte.Ninguno) continue;
                hayErrorDeLectura = true;
                var causa = problema switch
                {
                    ProblemaImporte.FormulaSinValor => "La celda tiene una fórmula y el archivo no guardó su valor calculado (o es un vínculo a otro libro). La herramienta lee el valor guardado y no recalcula.",
                    ProblemaImporte.ErrorDeExcel => "La celda devuelve un error de Excel.",
                    _ => "La celda tiene texto donde se esperaba un importe.",
                };
                locales.Add(Nueva("A11", Severidad.Bloqueante, "Importe sin valor calculado", causa,
                    "Abrí el libro en Excel, recalculá (F9), corregí o reemplazá por el valor la celda, guardá y volvé a cargarlo.", a, l, detalle: $"celda {col}{l.Fila}"));
            }

            if (l.Debe < 0m || l.Haber < 0m)
                locales.Add(Nueva("A13", Severidad.Advertencia, "Importe negativo", "La línea tiene un importe negativo.",
                    "No se invierte el lado automáticamente: revisá el Excel.", a, l,
                    detalle: l.Debe < 0m ? $"Debe −{Importe(-l.Debe)}" : $"Haber −{Importe(-l.Haber)}"));

            if (l.Debe != 0m && l.Haber != 0m)
                locales.Add(Nueva("A4", Severidad.Bloqueante, "Línea con Debe y Haber a la vez", "La línea tiene importe en las dos columnas y no es una fila de totales (tiene código o nombre).",
                    "Dejá el importe en una sola columna en el Excel y volvé a cargarlo, o aceptá la alerta.", a, l, detalle: $"Debe {Importe(l.Debe)} · Haber {Importe(l.Haber)}"));

            codigos[i] = NormalizarCodigo(l, a, locales, cuenta);

            if (Centavo.TieneMasDeDosDecimales(l.Debe) || Centavo.TieneMasDeDosDecimales(l.Haber))
            {
                var (n, set) = decimalesPorHoja.GetValueOrDefault(a.Hoja, (0, new HashSet<ClaveAsiento>()));
                set.Add(clave);
                decimalesPorHoja[a.Hoja] = (n + 1, set);
            }

            if (codigos[i].Length > 0 && l.Nombre.Length > 0)
            {
                if (!nombresPorCodigo.TryGetValue(codigos[i], out var e)) nombresPorCodigo[codigos[i]] = e = (new HashSet<string>(StringComparer.Ordinal), new HashSet<string>(StringComparer.Ordinal));
                e.Nombres.Add(Texto.Normalizar(l.Nombre)); e.Hojas.Add(a.Hoja);
            }
        }

        // Importes: redondeo por línea y regla del centavo.
        var debe = efectivas.Select(l => l.Debe).ToList();
        var haber = efectivas.Select(l => l.Haber).ToList();
        var rd = debe.Select(Centavo.Redondear).ToArray();
        var rh = haber.Select(Centavo.Redondear).ToArray();
        var ajuste = new decimal[efectivas.Count];

        if (!hayErrorDeLectura && efectivas.Count > 0)
        {
            var r = Centavo.Evaluar(debe, haber);
            switch (r.Veredicto)
            {
                case Centavo.Veredicto.AjustePorRedondeo when r.IndiceAjuste is { } idx:
                    Aplicar(idx, r.Ajuste, r.AjustaDebe, rd, rh, ajuste);
                    locales.Add(Nueva("A5", Severidad.Advertencia, "Descuadre por redondeo",
                        $"La suma sin redondear cuadra y, al pasar a 2 decimales, queda {Importe(Math.Abs(r.Diferencia))} de diferencia.",
                        "Se ajustó el centavo en la línea con mayor residuo de redondeo en la dirección correcta.", a, efectivas[idx], resolucion: ResolucionAlerta.Ajustada,
                        detalle: $"{NombreLinea(efectivas[idx])} {Centavo.TextoDelta(r.Ajuste)}"));
                    break;

                case Centavo.Veredicto.DescuadreDeOrigen:
                    var pideDecision = Math.Abs(r.Diferencia) == Centavo.Paso;
                    var alerta = Nueva("A6", Severidad.Bloqueante, "Asiento descuadrado en el origen",
                        $"El asiento no cuadra: Debe {Importe(rd.Sum())} y Haber {Importe(rh.Sum())} (diferencia {Importe(Math.Abs(r.Diferencia))}, {(r.Diferencia > 0 ? "sobra Debe" : "sobra Haber")}).",
                        pideDecision
                            ? "No se ajusta en silencio. Corregí el Excel y recargá, ajustá el centavo en una línea que elijas, o aceptá generar igual."
                            : "La diferencia es mayor a 0,01: corregí el Excel y recargá.", a) with { Diferencia = r.Diferencia };

                    // Decisión del usuario: ajustar el centavo en la línea que eligió.
                    var d = op.Ajustes.FirstOrDefault(x => x.Asiento == clave);
                    if (d is not null && efectivas.FindIndex(l => l.Fila == d.FilaLinea) is var j and >= 0
                        && Centavo.AjusteManual(r.Diferencia, debe[j], haber[j]) is { } man)
                    {
                        Aplicar(j, man.Delta, man.EnDebe, rd, rh, ajuste);
                        alerta = alerta with { Resolucion = ResolucionAlerta.Ajustada, ResueltaPor = d.Usuario, Detalle = $"{NombreLinea(efectivas[j])} {Centavo.TextoDelta(man.Delta)}" };
                    }
                    locales.Add(alerta);
                    break;
            }
        }

        // Aceptaciones explícitas del usuario sobre alertas bloqueantes de este asiento.
        for (var k = 0; k < locales.Count; k++)
            if (locales[k].BloqueaGeneracion && op.Aceptaciones.FirstOrDefault(x => x.Codigo == locales[k].Codigo && x.Asiento == clave) is { } ac)
                locales[k] = locales[k] with { Resolucion = ResolucionAlerta.Aceptada, ResueltaPor = ac.Usuario };

        var lineas = new List<LineaCalculada>(efectivas.Count);
        for (var i = 0; i < efectivas.Count; i++) lineas.Add(new LineaCalculada(efectivas[i], codigos[i], rd[i], rh[i], ajuste[i]));

        var estado = locales.Any(x => x.BloqueaGeneracion) ? EstadoVista.Error
            : locales.Any(x => x.Severidad != Severidad.Informativa) ? EstadoVista.Advertencia
            : EstadoVista.Ok;
        return (new AsientoCalculado { Origen = a, Estado = estado, Lineas = lineas }, locales);
    }

    /// <summary>El ajuste se aplica sobre el lado donde la línea tiene importe: sube o baja 0,01 el importe ya redondeado.</summary>
    private static void Aplicar(int idx, decimal delta, bool enDebe, decimal[] rd, decimal[] rh, decimal[] ajuste)
    {
        if (enDebe) rd[idx] += delta; else rh[idx] += delta;
        ajuste[idx] = delta;
    }

    // ------------------------------------------------------------------ códigos (A1, A2)
    private static string NormalizarCodigo(LineaLeida l, AsientoLeido a, List<Alerta> locales, string cuenta)
    {
        var original = l.Codigo.Trim();
        if (original.Length == 0)
        {
            locales.Add(Nueva("A1", Severidad.Advertencia, "Línea sin código de cuenta", $"La línea «{cuenta}» no tiene código de cuenta.",
                "El TXT sale con la cuenta vacía en esa línea. Completá el código en el Excel si querés que salga con cuenta.", a, l));
            return "";
        }

        var normalizado = original.Replace(',', '.');
        if (!CodigoValido.IsMatch(normalizado))
        {
            locales.Add(Nueva("A2", Severidad.Bloqueante, "Código con formato inválido", $"El código «{original}» no tiene forma de código de cuenta (dígitos separados por puntos).",
                "Corregí el código en el Excel y volvé a cargar el archivo, o aceptá la alerta.", a, l));
            return original;
        }
        if (normalizado != original)
            locales.Add(Nueva("A2", Severidad.Advertencia, "Código con formato inválido", $"El código «{original}» usa comas en lugar de puntos.",
                $"Se normalizó a «{normalizado}». Corregilo en el origen para que no vuelva a avisar.", a, l, resolucion: ResolucionAlerta.Normalizada, detalle: $"{original} → {normalizado}"));
        return normalizado;
    }

    // ------------------------------------------------------------------ A9
    private static void AlertarMesesFueraDeRango(LibroLeido libro, OpcionesValidacion op, List<Alerta> alertas)
    {
        if (op.RangoHabitual is not { } rango) return;
        var fuera = libro.Hojas.SelectMany(h => h.Asientos.Where(x => x.Mes is not null).Select(x => (h.Nombre, Mes: x.Mes!.Value, x.Estado)))
            .Where(t => t.Mes.CompareTo(rango.Desde) < 0 || t.Mes.CompareTo(rango.Hasta) > 0)
            .GroupBy(t => t.Mes).OrderBy(g => g.Key);
        foreach (var g in fuera)
        {
            var hojas = g.Select(t => t.Nombre).Distinct().ToList();
            var registrables = g.Count(t => t.Estado == EstadoAsiento.Registrable);
            alertas.Add(new Alerta
            {
                Codigo = "A9", Severidad = Severidad.Informativa, Titulo = "Mes fuera de rango", Mes = g.Key,
                Causa = $"{g.Key.Texto} está fuera del rango esperado ({rango.Desde.Texto} a {rango.Hasta.Texto}): aparece en {hojas.Count} hojas ({string.Join(", ", hojas)}) con {registrables} asientos a registrar.",
                Accion = "Elegí si se incluye o no. Queda registrado en el historial.", Detalle = $"{registrables} asientos",
            });
        }
    }

    // ------------------------------------------------------------------ helpers
    private static AsientoCalculado Omitido(AsientoLeido a) => new() { Origen = a, Estado = EstadoVista.Omitido };

    private static string NombreLinea(LineaLeida l) => l.Nombre.Length > 0 ? l.Nombre : l.Codigo;

    private static Alerta Nueva(string codigo, Severidad sev, string titulo, string causa, string accion, AsientoLeido a, LineaLeida? l = null,
        ResolucionAlerta resolucion = ResolucionAlerta.Ninguna, string? detalle = null) => new()
    {
        Codigo = codigo, Severidad = sev, Titulo = titulo, Causa = causa, Accion = accion,
        Hoja = a.Hoja, Mes = a.Mes, Asiento = new ClaveAsiento(a.Hoja, a.FilaEncabezado),
        AsientoTexto = a.Concepto.Length > 0 ? a.Concepto : a.Titulo,
        Fila = l?.Fila ?? a.FilaEncabezado, Cuenta = l is null ? null : NombreLinea(l), Detalle = detalle, Resolucion = resolucion,
    };

    /// <summary>«488.708.350,94»: miles con punto y dos decimales con coma, sin depender de la cultura de la máquina.</summary>
    public static string Importe(decimal v) =>
        v.ToString("#,##0.00", CultureInfo.InvariantCulture).Replace(',', '\u0001').Replace('.', ',').Replace('\u0001', '.');
}
