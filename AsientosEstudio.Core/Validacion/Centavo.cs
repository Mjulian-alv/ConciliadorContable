// Fecha: 13/11/2026 - TAREA: 00180 - Linea: 5 - Redondeo y descuadres de centavo (§6, A5 y A6). Todo en decimal, nunca en double.
//
// Cada línea se redondea half-up a 2 decimales. Si después de redondear el asiento queda descuadrado, SE INFORMA y NO SE TOCA:
// el dueño del pedido decidió (13/11/2026) que la herramienta no hace arreglos de centavos. Los importes salen del TXT tal como quedan al redondear.
//   A5  La suma SIN redondear cuadra y tras redondear queda 0,01: es un artefacto del redondeo.
//   A6  El Excel ya viene descuadrado, o la diferencia es mayor a 0,01.
// (La primera versión de esta etapa ajustaba el centavo en una línea; se quitó por esa decisión.)
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
        /// <summary>Cuadra tras redondear. No hay nada que informar.</summary>
        Cuadra,
        /// <summary>A5: cuadra sin redondear y tras redondear queda 0,01. Se informa.</summary>
        DescuadrePorRedondeo,
        /// <summary>A6: el descuadre ya viene del origen, o es mayor a 0,01.</summary>
        DescuadreDeOrigen,
    }

    /// <param name="Diferencia">Debe − Haber ya redondeado por línea. Positivo: sobra Debe.</param>
    public sealed record Resultado(Veredicto Veredicto, decimal Diferencia, bool CuadraEnOrigen);

    /// <summary>
    /// Evalúa un asiento. <paramref name="debe"/> y <paramref name="haber"/> son los importes SIN redondear de cada línea, en el mismo orden.
    /// </summary>
    public static Resultado Evaluar(IReadOnlyList<decimal> debe, IReadOnlyList<decimal> haber)
    {
        if (debe.Count != haber.Count) throw new ArgumentException("debe y haber deben tener la misma cantidad de líneas");

        var cuadraEnOrigen = Math.Abs(debe.Sum() - haber.Sum()) <= ToleranciaOrigen;
        var dif = debe.Sum(Redondear) - haber.Sum(Redondear);

        if (dif == 0m) return new Resultado(Veredicto.Cuadra, 0m, cuadraEnOrigen);
        if (cuadraEnOrigen && Math.Abs(dif) == Paso) return new Resultado(Veredicto.DescuadrePorRedondeo, dif, true);
        return new Resultado(Veredicto.DescuadreDeOrigen, dif, cuadraEnOrigen);
    }

    /// <summary>«+0,01» o «−0,01» (con el signo menos tipográfico, como en el prompt), sin depender de la cultura.</summary>
    public static string TextoConSigno(decimal v) => (v > 0 ? "+" : v < 0 ? "−" : "") + Math.Abs(v).ToString("0.00", CultureInfo.InvariantCulture).Replace('.', ',');
}
