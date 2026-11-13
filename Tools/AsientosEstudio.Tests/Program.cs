// Fecha: 12/11/2026 - TAREA: 00180 - Linea: 1 - Arnés mínimo, el mismo estilo que AgrupadorWeb.Tests.
// Las pruebas de aceptación con los Excel reales leen la carpeta de la variable ASIENTOS_MUESTRAS y se saltan, avisando, si no está:
// los Excel son datos contables reales y no se commitean (decisión D3 de la guía).
using System.Globalization;

var cultura = Environment.GetEnvironmentVariable("ASIENTOS_CULTURA") ?? "es-AR"; // la lectura no debe depender de la cultura de la máquina
CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultura);
CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultura);
var passed = 0; var failed = 0; var skipped = 0;
void Test(string name, Action action)
{
    try { action(); Console.WriteLine("OK " + name); passed++; }
    catch (Saltada s) { Console.WriteLine("SALTADA " + name + ": " + s.Message); skipped++; }
    catch (Exception ex) { failed++; Console.Error.WriteLine("ERROR " + name + ": " + ex.Message); }
}
Console.WriteLine("Cultura de la prueba: " + CultureInfo.CurrentCulture.Name);
TextoTests.Run(Test);
LectorLibroTests.Run(Test);
ParserBancosTests.Run(Test);
ParserDevengamientosTests.Run(Test); // Fecha: 12/11/2026 - TAREA: 00180 - Linea: 2
ParserVariosTests.Run(Test); // Fecha: 12/11/2026 - TAREA: 00180 - Linea: 3
MuestrasTests.Run(Test);
Console.WriteLine($"{passed} pruebas correctas, {failed} con error, {skipped} saltadas.");
return failed == 0 ? 0 : 1;

/// <summary>Una prueba que no puede correr en esta máquina (falta una muestra). No es un error, pero se avisa.</summary>
class Saltada : Exception { public Saltada(string m) : base(m) { } }

static class Afirmar
{
    public static void Igual<T>(T esperado, T actual, string que = "")
    {
        if (!EqualityComparer<T>.Default.Equals(esperado, actual)) throw new Exception($"{que} esperado «{esperado}», obtenido «{actual}»".Trim());
    }
    public static void Verdad(bool condicion, string que) { if (!condicion) throw new Exception(que); }
}
