// Fecha: 13/11/2026 - TAREA: 00180 - Linea: 5 - Redondeo half-up con decimal y descuadres de centavo (A5 y A6): se informan, NUNCA se ajustan.
using AsientosEstudio.Core;

static class CentavoTests
{
    static Centavo.Resultado Ev(decimal[] d, decimal[] h) => Centavo.Evaluar(d, h);

    public static void Run(Action<string, Action> test)
    {
        test("Centavo: el redondeo es half-up a 2 decimales con decimal (0,005 sube, también el negativo se aleja del cero)", () =>
        {
            Afirmar.Igual(0.01m, Centavo.Redondear(0.005m)); Afirmar.Igual(2.35m, Centavo.Redondear(2.345m)); Afirmar.Igual(1.99m, Centavo.Redondear(1.994999m));
            Afirmar.Igual(-0.01m, Centavo.Redondear(-0.005m)); Afirmar.Igual(10394283.15m, Centavo.Redondear(10394283.1476m)); Afirmar.Igual(21103544.57m, Centavo.Redondear(21103544.5724m));
            Afirmar.Igual(1398900661.6m, Centavo.Redondear(1398900661.6m));
        });
        test("Centavo: «más de 2 decimales» distingue 4,0000 (no) de 4,0049 (sí)", () =>
        {
            Afirmar.Verdad(!Centavo.TieneMasDeDosDecimales(4.0000m) && !Centavo.TieneMasDeDosDecimales(1398900661.6m), "no");
            Afirmar.Verdad(Centavo.TieneMasDeDosDecimales(4.0049m) && Centavo.TieneMasDeDosDecimales(10394283.1476m), "sí");
        });
        test("Centavo: un asiento que cuadra tras redondear no necesita nada", () =>
        {
            var r = Ev(new[] { 100.5m, 50m }, new[] { 0m, 150.5m });
            Afirmar.Igual(Centavo.Veredicto.Cuadra, r.Veredicto); Afirmar.Igual(0m, r.Diferencia);
        });
        test("Centavo: el ruido de punto flotante de Excel (1,5e-6) no es un descuadre", () =>
        {
            var r = Ev(new[] { 7402333564.176948m }, new[] { 7402333564.1769495m });
            Afirmar.Igual(Centavo.Veredicto.Cuadra, r.Veredicto); Afirmar.Verdad(r.CuadraEnOrigen, "cuadra en origen");
        });
        test("Centavo A5: cuadra sin redondear y tras redondear queda 0,01 → se informa como descuadre por redondeo, con el signo de la diferencia", () =>
        {
            // Debe 100,0049 + 200,0044 = 300,0093 → 300,00; Haber 150,0054 + 150,0039 = 300,0093 → 150,01 + 150,00 = 300,01. Falta 0,01 de Debe.
            var r = Ev(new[] { 100.0049m, 200.0044m, 0m, 0m }, new[] { 0m, 0m, 150.0054m, 150.0039m });
            Afirmar.Igual(Centavo.Veredicto.DescuadrePorRedondeo, r.Veredicto); Afirmar.Igual(-0.01m, r.Diferencia); Afirmar.Verdad(r.CuadraEnOrigen, "cuadra sin redondear");
            // Y al revés: sobra 0,01 de Debe.
            var r2 = Ev(new[] { 100.0050m, 200.0043m, 0m, 0m }, new[] { 0m, 0m, 150.0049m, 150.0044m });
            Afirmar.Igual(Centavo.Veredicto.DescuadrePorRedondeo, r2.Veredicto); Afirmar.Igual(0.01m, r2.Diferencia);
        });
        test("Centavo A6: un asiento que ya viene descuadrado del origen es A6", () =>
        {
            var r = Ev(new[] { 1169596185.03m, 0m }, new[] { 0m, 1169596185.04m });
            Afirmar.Igual(Centavo.Veredicto.DescuadreDeOrigen, r.Veredicto); Afirmar.Igual(-0.01m, r.Diferencia); Afirmar.Verdad(!r.CuadraEnOrigen, "no cuadra en origen");
        });
        test("Centavo A6: una diferencia de redondeo mayor a 0,01 también es A6", () =>
        {
            var d = Enumerable.Repeat(0.0049m, 10).ToArray(); var h = new decimal[10]; h[0] = 0.049m; // cuadra sin redondear (0,049) y tras redondear 0,00 contra 0,05
            var r = Ev(d, h);
            Afirmar.Igual(Centavo.Veredicto.DescuadreDeOrigen, r.Veredicto); Afirmar.Igual(-0.05m, r.Diferencia); Afirmar.Verdad(r.CuadraEnOrigen, "cuadra sin redondear");
        });
        test("Centavo: el texto con signo y los importes no dependen de la cultura", () =>
        {
            Afirmar.Igual("+0,01", Centavo.TextoConSigno(0.01m)); Afirmar.Igual("−0,01", Centavo.TextoConSigno(-0.01m)); Afirmar.Igual("0,00", Centavo.TextoConSigno(0m));
            Afirmar.Igual("488.708.350,94", Validador.Importe(488708350.94m)); Afirmar.Igual("0,01", Validador.Importe(0.01m));
        });
    }
}
