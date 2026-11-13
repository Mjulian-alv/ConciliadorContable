// Fecha: 13/11/2026 - TAREA: 00180 - Linea: 5 - Redondeo half-up con decimal y regla del centavo (A5 contra A6).
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
            Afirmar.Igual(Centavo.Veredicto.Cuadra, r.Veredicto); Afirmar.Igual(0m, r.Diferencia); Afirmar.Verdad(r.IndiceAjuste is null, "sin ajuste");
        });
        test("Centavo: el ruido de punto flotante de Excel (1,5e-6) no es un descuadre", () =>
        {
            var r = Ev(new[] { 7402333564.176948m }, new[] { 7402333564.1769495m });
            Afirmar.Igual(Centavo.Veredicto.Cuadra, r.Veredicto); Afirmar.Verdad(r.CuadraEnOrigen, "cuadra en origen");
        });
        test("Centavo A5: cuadra sin redondear y tras redondear queda 0,01 → se sube el Debe con mayor residuo", () =>
        {
            // Debe 100,0049 + 200,0044 = 300,0093 → 300,00; Haber 150,0054 + 150,0039 = 300,0093 → 150,01 + 150,00 = 300,01.
            var r = Ev(new[] { 100.0049m, 200.0044m, 0m, 0m }, new[] { 0m, 0m, 150.0054m, 150.0039m });
            Afirmar.Igual(Centavo.Veredicto.AjustePorRedondeo, r.Veredicto); Afirmar.Igual(-0.01m, r.Diferencia);
            Afirmar.Igual(0, r.IndiceAjuste!.Value); Afirmar.Igual(0.01m, r.Ajuste); Afirmar.Verdad(r.AjustaDebe, "en el Debe");
        });
        test("Centavo A5: el ajuste se hace del lado del Debe aunque una línea del Haber tenga mejor residuo (así lo hizo el estudio)", () =>
        {
            // El Haber 150,0050 se redondeó a 150,01 (residuo −0,0050): bajarlo sería el ajuste más fiel. Pero el proceso manual ajusta el Debe (caso VENTAS 07/2026:
            // DEUDORES POR VENTAS +0,01 con residuo 0,0031, y no «A IVA DF 21 −0,01» con −0,0046), y el total del archivo ya importado depende de eso.
            // Debe 100,0049 + 200,0044 = 300,0093 → 300,00; Haber 150,0050 + 150,0043 = 300,0093 → 150,01 + 150,00 = 300,01. El Debe 100,0049 (residuo 0,0049) gana en el Debe.
            var r = Ev(new[] { 100.0049m, 200.0044m, 0m, 0m }, new[] { 0m, 0m, 150.0050m, 150.0043m });
            Afirmar.Igual(Centavo.Veredicto.AjustePorRedondeo, r.Veredicto); Afirmar.Igual(-0.01m, r.Diferencia);
            Afirmar.Igual(0, r.IndiceAjuste!.Value); Afirmar.Igual(0.01m, r.Ajuste); Afirmar.Verdad(r.AjustaDebe, "en el Debe");
        });
        test("Centavo A5: sobra Debe → se baja un Debe redondeado para arriba o se sube un Haber redondeado para abajo", () =>
        {
            // Debe 100,0050 + 200,0043 = 300,0093 → 100,01 + 200,00 = 300,01; Haber 150,0049 + 150,0044 = 300,0093 → 300,00. Sobra 0,01 de Debe.
            // El Debe 100,0050 se redondeó para arriba (residuo −0,0050): bajarlo gana (0,0050) contra subir el Haber 150,0049 (0,0049).
            var r = Ev(new[] { 100.0050m, 200.0043m, 0m, 0m }, new[] { 0m, 0m, 150.0049m, 150.0044m });
            Afirmar.Igual(Centavo.Veredicto.AjustePorRedondeo, r.Veredicto); Afirmar.Igual(0.01m, r.Diferencia);
            Afirmar.Igual(0, r.IndiceAjuste!.Value); Afirmar.Igual(-0.01m, r.Ajuste); Afirmar.Verdad(r.AjustaDebe, "baja el Debe 0");
        });
        test("Centavo A6: un asiento que ya viene descuadrado del origen no se ajusta solo", () =>
        {
            var r = Ev(new[] { 1169596185.03m, 0m }, new[] { 0m, 1169596185.04m });
            Afirmar.Igual(Centavo.Veredicto.DescuadreDeOrigen, r.Veredicto); Afirmar.Igual(-0.01m, r.Diferencia);
            Afirmar.Verdad(!r.CuadraEnOrigen, "no cuadra en origen"); Afirmar.Verdad(r.IndiceAjuste is null, "sin ajuste automático");
        });
        test("Centavo A6: una diferencia de redondeo mayor a 0,01 tampoco se ajusta sola", () =>
        {
            var d = Enumerable.Repeat(0.0049m, 10).ToArray(); var h = new decimal[10]; h[0] = 0.049m; // cuadra sin redondear (0,049) y tras redondear 0,00 contra 0,05
            var r = Ev(d, h);
            Afirmar.Igual(Centavo.Veredicto.DescuadreDeOrigen, r.Veredicto); Afirmar.Igual(-0.05m, r.Diferencia); Afirmar.Verdad(r.CuadraEnOrigen, "cuadra sin redondear");
        });
        test("Centavo: el ajuste manual sube o baja 0,01 del lado donde la línea tiene importe, solo para una diferencia de 0,01", () =>
        {
            Afirmar.Igual((0.01m, false), Centavo.AjusteManual(0.01m, 0m, 100m)!.Value);      // sobra Debe: sube un Haber
            Afirmar.Igual((-0.01m, false), Centavo.AjusteManual(-0.01m, 0m, 100m)!.Value);    // sobra Haber: baja un Haber
            Afirmar.Igual((-0.01m, true), Centavo.AjusteManual(0.01m, 100m, 0m)!.Value);      // sobra Debe: baja un Debe
            Afirmar.Igual((0.01m, true), Centavo.AjusteManual(-0.01m, 100m, 0m)!.Value);      // sobra Haber: sube un Debe
            Afirmar.Verdad(Centavo.AjusteManual(0.02m, 100m, 0m) is null && Centavo.AjusteManual(0.01m, 0m, 0m) is null && Centavo.AjusteManual(0.01m, 5m, 5m) is null, "casos sin ajuste");
        });
        test("Centavo: el texto del ajuste no depende de la cultura", () =>
        {
            Afirmar.Igual("+0,01", Centavo.TextoDelta(0.01m)); Afirmar.Igual("−0,01", Centavo.TextoDelta(-0.01m));
            Afirmar.Igual("488.708.350,94", Validador.Importe(488708350.94m)); Afirmar.Igual("0,01", Validador.Importe(0.01m));
        });
    }
}
