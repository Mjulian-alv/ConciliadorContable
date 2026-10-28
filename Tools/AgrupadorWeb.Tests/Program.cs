// Fecha: 14/10/2026 - TAREA: 00097 - Linea: 1 - Arnés mínimo, con el mismo estilo que ArcaWeb.Tests.
using System.Globalization;
CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US"); // el formato de importes no debe depender de la cultura del servidor
var passed = 0; var failed = 0;
void Test(string name, Action action) { try { action(); Console.WriteLine("OK " + name); passed++; } catch (Exception ex) { failed++; Console.Error.WriteLine("ERROR " + name + ": " + ex.Message); } }
FormatoImporteTests.Run(Test);
MenuTests.Run(Test);
CuentasTests.Run(Test);
ExcelTests.Run(Test);
AsientoTests.Run(Test); // Fecha: 23/10/2026 - TAREA: 00126 - Linea: 1 - Hoja Asiento del consolidado
ImportacionTests.Run(Test);
ConciliacionInternaTests.Run(Test);
ConciliacionExternaTests.Run(Test);
BlindajeTests.Run(Test); // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 1, 2, 3 - Concepto final protegido, historial y blindaje
LoginTests.Run(Test);
IdentidadAppHubTests.Run(Test); // Fecha: 28/10/2026 - TAREA: 00140 - Linea: 2,3 - Identidad de AppHub: token, usuarios de Microsoft y middleware
Console.WriteLine($"{passed} pruebas correctas, {failed} con error.");
return failed == 0 ? 0 : 1;
