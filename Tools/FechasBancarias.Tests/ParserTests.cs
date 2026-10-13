namespace FechasBancarias.Tests;
// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - La misma entrada debe representar el mismo día en cualquier equipo.
using System.Globalization;
using Conciliador.Comun.Services;
internal static class ParserTests
{
    public static void Run()
    {
        var previa = CultureInfo.CurrentCulture;
        try
        {
            foreach (string cultura in new[] { "es-AR", "en-US" })
            {
                CultureInfo.CurrentCulture = new CultureInfo(cultura);
                foreach (object valor in new object[] { "03/04/2026", "3-4-2026", "2026-04-03", "3/4/2026 5:06", "03-04-2026 05:06:07.123", "2026-04-03T00:15:00+14:00", "2026-04-03T23:59:59Z", "2026-04-03 12:30:45.1234567", new DateTime(2026,4,3,22,0,0,DateTimeKind.Utc) })
                {
                    Program.Check(FechaBancariaParser.TryParse(valor, out var fecha) && fecha == new DateTime(2026,4,3) && fecha.Kind == DateTimeKind.Unspecified, $"Fecha incorrecta para {valor} en {cultura}");
                }
                Program.Check(FechaBancariaParser.TryParse("29/02/2024", out var bisiesto) && bisiesto == new DateTime(2024,2,29), "Bisiesto");
                foreach (object valor in new object[] { "29/02/2026", "04/30/2026", "", "  ", null, DBNull.Value, 46000d, "3 abril 2026", "03/04/26", "20260403", "2026-04-03 basura" })
                    Program.Check(!FechaBancariaParser.TryParse(valor, out _), $"Aceptó entrada inválida: {valor}");
            }
        }
        finally { CultureInfo.CurrentCulture = previa; }
        Console.WriteLine("OK: parser independiente de cultura y zona horaria");
    }
}
