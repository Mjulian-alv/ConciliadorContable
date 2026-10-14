// Fecha: 14/10/2026 - TAREA: 00097 - Linea: 1 - Arnés mínimo, con el mismo estilo que ArcaWeb.Tests.
using System.Globalization;
CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US"); // el formato de importes no debe depender de la cultura del servidor
var passed = 0; var failed = 0;
void Test(string name, Action action) { try { action(); Console.WriteLine("OK " + name); passed++; } catch (Exception ex) { failed++; Console.Error.WriteLine("ERROR " + name + ": " + ex.Message); } }
FormatoImporteTests.Run(Test);
MenuTests.Run(Test);
CuentasTests.Run(Test);
LoginTests.Run(Test);
Console.WriteLine($"{passed} pruebas correctas, {failed} con error.");
return failed == 0 ? 0 : 1;
