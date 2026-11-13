// Fecha: 13/11/2026 - TAREA: 00180 - Linea: 5 - Redondeo y regla del centavo (§6, A5 vs A6). Todo en decimal, nunca en double.
//
// Cada línea se redondea half-up a 2 decimales. Si después de redondear el asiento queda descuadrado, hay dos casos que NO se tratan igual:
//   A5  La suma SIN redondear cuadra y tras redondear la diferencia es exactamente 0,01: es un artefacto del redondeo. Se ajusta solo el centavo,
//       en la línea cuyo residuo de redondeo sea el mayor en la dirección correcta, y se muestra cuál.
//   A6  El Excel ya viene descuadrado (o la diferencia es mayor a 0,01): nunca se ajusta en silencio. Lo decide el usuario.
using System.Globalization;

namespace AsientosEstudio.Core;

public static class Centavo
{
    public const decimal Paso = 0.01m;

    /// <summary>
    /// Dos sumas «cuadran sin redondear» si difieren menos que esto. Los importes con fórmula salen de Excel como double y sus sumas arrastran ruido de
    /// punto flotante (p. ej. 7402333564.176948 contra 7402333564.1769495): eso no es un descuadre. Un descuadre real es de 0,01 o más.
    /// </summary>
    public const decimal ToleranciaOrigen = 0.001m;

    public static decimal Redondear(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

    /// <summary>El importe tiene más de 2 decimales «de verdad» (4,0000 no cuenta; 4,0049 sí). Es la alerta informativa A12.</summary>
    public static bool TieneMasDeDosDecimales(decimal v) => Redondear(v) != v;

    public enum Veredicto
    {
        /// <summary>Cuadra tras redondear. No hay nada que hacer.</summary>
        Cuadra,
        /// <summary>A5: cuadra sin redondear y la diferencia tras redondear es de 0,01. Se ajusta una línea.</summary>
        AjustePorRedondeo,
        /// <summary>A6: descuadre que ya viene del origen, o mayor a 0,01. Lo decide el usuario.</summary>
        DescuadreDeOrigen,
    }

    public sealed record Resultado(Veredicto Veredicto, decimal Diferencia, bool CuadraEnOrigen, int? IndiceAjuste, decimal Ajuste, bool AjustaDebe);

    /// <summary>
    /// Evalúa un asiento. <paramref name="debe"/> y <paramref name="haber"/> son los importes SIN redondear de cada línea, en el mismo orden.
    /// <see cref="Resultado.Diferencia"/> es Debe − Haber ya redondeado por línea (positivo: sobra Debe).
    /// </summary>
    public static Resultado Evaluar(IReadOnlyList<decimal> debe, IReadOnlyList<decimal> haber)
    {
        if (debe.Count != haber.Count) throw new ArgumentException("debe y haber deben tener la misma cantidad de líneas");

        var sumaD = debe.Sum(); var sumaH = haber.Sum();
        var cuadraEnOrigen = Math.Abs(sumaD - sumaH) <= ToleranciaOrigen;
        var dif = debe.Sum(Redondear) - haber.Sum(Redondear);

        if (dif == 0m) return new Resultado(Veredicto.Cuadra, 0m, cuadraEnOrigen, null, 0m, false);
        if (!cuadraEnOrigen || Math.Abs(dif) != Paso) return new Resultado(Veredicto.DescuadreDeOrigen, dif, cuadraEnOrigen, null, 0m, false);

        // A5. dif > 0: sobra Debe → hay que bajar un Debe (o subir un Haber). dif < 0: falta Debe → subir un Debe (o bajar un Haber).
        //
        // DECISIÓN (13/11/2026): el ajuste se hace del lado del DEBE. Así lo hizo el proceso manual del estudio en los cuatro casos reales
        // (RICARDO +0,01 ×2, IMP A LOS DEB Y CRED COMP −0,01, DEUDORES POR VENTAS +0,01) y es lo que ya está importado: ajustar del lado del Haber en
        // alguno de esos asientos movería el total del archivo en un centavo (71.914.422.835,05 pasaba a ,04) y no coincidiría con lo que ya recibió el
        // estudio. El Haber queda solo como respaldo, si el asiento no tuviera ninguna línea de Debe.
        //
        // Dentro del lado se elige la línea con el mayor residuo de redondeo en la dirección correcta (§6 A5): subir una que se redondeó para abajo
        // (residuo > 0) o bajar una que se redondeó para arriba (residuo < 0). Ante un empate, la primera.
        var delta = dif > 0 ? -Paso : Paso;                              // sentido del ajuste sobre una línea del Debe
        var elegido = Elegir(debe, delta, ladoDebe: true) ?? Elegir(haber, -delta, ladoDebe: false);
        return elegido is { } e
            ? new Resultado(Veredicto.AjustePorRedondeo, dif, cuadraEnOrigen, e.Indice, e.Delta, e.EnDebe)
            : new Resultado(Veredicto.DescuadreDeOrigen, dif, cuadraEnOrigen, null, 0m, false); // ninguna línea admite el ajuste: que decida el usuario
    }

    /// <summary>La línea de un lado con el mayor residuo de redondeo en el sentido del ajuste, o null si ese lado no tiene líneas con importe.</summary>
    private static (int Indice, decimal Delta, bool EnDebe)? Elegir(IReadOnlyList<decimal> importes, decimal delta, bool ladoDebe)
    {
        int? mejor = null; var mejorGanancia = decimal.MinValue;
        for (var i = 0; i < importes.Count; i++)
        {
            if (importes[i] == 0m) continue;                              // solo líneas que ya tienen importe de ese lado
            var redondeado = Redondear(importes[i]);
            if (redondeado + delta < 0m) continue;                        // un ajuste no deja un importe negativo
            var ganancia = (importes[i] - redondeado) * (delta > 0 ? 1 : -1);
            if (ganancia > mejorGanancia) { mejor = i; mejorGanancia = ganancia; }
        }
        return mejor is { } m ? (m, delta, ladoDebe) : null;
    }

    /// <summary>
    /// El ajuste manual de A6 (salida b): 0,01 en la línea que eligió el usuario, del lado que ya tiene importe, en la dirección que cuadra el asiento.
    /// Solo existe para una diferencia de exactamente 0,01: una mayor solo se arregla corrigiendo el Excel.
    /// </summary>
    public static (decimal Delta, bool EnDebe)? AjusteManual(decimal diferencia, decimal debeLinea, decimal haberLinea)
    {
        if (Math.Abs(diferencia) != Paso) return null;
        if (debeLinea != 0m && haberLinea == 0m) return (diferencia > 0 ? -Paso : Paso, true);
        if (haberLinea != 0m && debeLinea == 0m) return (diferencia > 0 ? Paso : -Paso, false);
        return null;
    }

    /// <summary>«+0,01» o «−0,01» (con el signo menos tipográfico, como en el prompt), sin depender de la cultura.</summary>
    public static string TextoDelta(decimal delta) => (delta > 0 ? "+" : "−") + Math.Abs(delta).ToString("0.00", CultureInfo.InvariantCulture).Replace('.', ',');
}
