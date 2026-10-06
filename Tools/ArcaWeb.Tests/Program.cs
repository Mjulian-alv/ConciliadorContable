// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 3 - Se caracteriza el motor original antes de compartirlo con el host web.
using ArcaCliente.Models;
using ArcaCliente.Services;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("es-AR");
var passed = 0;
var failed = 0;
void Test(string name, Action action) { try { action(); Console.WriteLine("OK " + name); passed++; } catch(Exception ex) { failed++;Console.Error.WriteLine("ERROR " + name + ": " + ex.Message); } }
void Equal<T>(T expected, T actual) { if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new Exception($"Esperado {expected}, obtenido {actual}"); }
var engine = new ConciliacionService(() => throw new Exception("La prueba no usa SQL"), "");
ComprobanteCsv External(string number, string total) => new() { NroDocEmisor = "30123456781", TipoComprobante = "001", PuntoVenta = "00005", NumeroDesde = number, ImpTotal = total, FechaEmision = "05/09/2026", DenominacionEmisor = "Ejemplo" };
ComprobanteLocal Local(string number, decimal total) => new() { Cuit = "30-12345678-1", TipoComprobante = "001", PuntoVenta = "5", Numero = number, Total = total, Fecha = new(2026,9,5), NombreProveedor = "Ejemplo" };
Test("Motor compartido conserva coincidencia, diferencias y remanentes", () => {
    var result = engine.Conciliar(new[] { External("1234", "1.000,00"), External("1235", "600,00"), External("9999", "10,00") }, new[] { Local("00001234", 1000), Local("00001235", 1000), Local("00008888", 100) });
    Equal(1, result.Count(x=>x.Estado==EstadoConciliacion.Conciliado));
    Equal(1, result.Count(x=>x.Estado==EstadoConciliacion.DiferenciaImporte));
    Equal(1, result.Count(x=>x.Estado==EstadoConciliacion.SoloARCA));
    Equal(1, result.Count(x=>x.Estado==EstadoConciliacion.SoloSistema));
});
Test("Directivas adicionales actúan sobre los pendientes", () => {
    var directives = new[] { DirectivaConciliacion.CrearPrimaria(), new DirectivaConciliacion { Descripcion="Por importe", Campos = new() { CampoConciliacion.Cuit, CampoConciliacion.TipoComprobante, CampoConciliacion.Total } } };
    var r=engine.Conciliar(new[]{External("1","100,00")},new[]{Local("2",100.00m)},directives);
    Equal(1,r.Count); Equal(2,r[0].DirectivaAplicada); Equal(EstadoConciliacion.Conciliado,r[0].Estado);
});
ImportTests.Run(Test);
IntegrationTests.Run(Test);
ReviewRegressionTests.Run(Test);
// Fecha: 05/10/2026 - TAREA: 00065 - Linea: 1 - Regresiones de lectura con perfiles compartidos del escritorio.
DesktopImportTests.Run(Test);
// Fecha: 05/10/2026 - TAREA: 00066 - Linea: 2 - Verifica paridad con el exportador compartido.
ExportCompatibilityTests.Run(Test);
Console.WriteLine($"{passed} pruebas correctas.");
if(failed>0){Console.Error.WriteLine($"{failed} pruebas fallidas.");Environment.ExitCode=1;}
