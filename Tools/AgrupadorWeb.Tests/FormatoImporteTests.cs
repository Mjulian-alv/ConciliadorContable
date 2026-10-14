// Fecha: 14/10/2026 - TAREA: 00097 - Linea: 7 - La matriz es la de la maqueta 07-importes-formato: miles y decimales, absoluto, cero y nulo.
using AgrupadorConceptos.Services;
static class FormatoImporteTests
{
    static void Equal(string esperado, string actual) { if (esperado != actual) throw new Exception($"Esperado «{esperado}», obtenido «{actual}»"); }
    public static void Run(Action<string, Action> test)
    {
        test("Importes: miles con punto y dos decimales con coma, sin depender de la cultura", () =>
        {
            Equal("1.234.567,89", FormatoImporte.Absoluto(1234567.89m));
            Equal("58.049,41", FormatoImporte.Absoluto(58049.41m));
            Equal("0,50", FormatoImporte.Absoluto(0.5m));
            Equal("25.614.706.918,23", FormatoImporte.Absoluto(25614706918.23m));
        });
        test("Importes: siempre en valor absoluto, también los datos históricos con signo", () =>
        {
            Equal("1.234,56", FormatoImporte.Absoluto(-1234.56m));
            Equal("5.000.000,00", FormatoImporte.Absoluto(-5000000m));
        });
        test("Importes: cero con decimales y nulo en blanco", () =>
        {
            Equal("0,00", FormatoImporte.Absoluto(0m));
            Equal("", FormatoImporte.Absoluto(null));
            if (!FormatoImporte.EsCero(0m) || FormatoImporte.EsCero(0.01m) || FormatoImporte.EsCero(null)) throw new Exception("EsCero");
        });
        test("Importes: redondeo a dos decimales, mitad hacia arriba", () =>
        {
            Equal("0,01", FormatoImporte.Absoluto(0.005m));
            Equal("1,00", FormatoImporte.Absoluto(0.995m));
            Equal("2,34", FormatoImporte.Absoluto(2.3449m));
        });
        test("Importes: la diferencia conserva el signo y los miles", () =>
        {
            Equal("+1.951.317,66", FormatoImporte.Diferencia(1951317.66m));
            Equal("−3.100,00", FormatoImporte.Diferencia(-3100m));
            Equal("0,00", FormatoImporte.Diferencia(0m));
        });
    }
}
