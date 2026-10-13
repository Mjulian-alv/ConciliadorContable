namespace FechasBancarias.Tests;
// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1, 2 - Separar pruebas de entrada y migración evita tocar la base operativa.
internal static class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            if(args.Length==3 && args[0]=="--ensayo") { EnsayoReal.Run(args[1],args[2]); return 0; }
            if(args.SequenceEqual(new[]{"--inicializar"}))
            {
                Conciliador.Comun.SqlDb.ConnectionString=Environment.GetEnvironmentVariable("FECHAS_TEST_DB") ?? throw new Exception("Falta base de prueba.");
                AgrupadorConceptos.Data.DatabaseHelper.InitializeDatabase(); return 0;
            }
            string grupo = args.Length == 2 && args[0] == "--grupo" ? args[1] : "todos";
            if (grupo is not ("parser" or "migracion" or "importacion" or "presentacion" or "todos")) throw new ArgumentException("Grupo desconocido.");
            if (grupo is "todos" or "parser") ParserTests.Run();
            if (grupo is "todos" or "migracion") MigracionTests.Run();
            if (grupo is "todos" or "importacion") ImportacionTests.Run();
            if (grupo is "todos" or "presentacion") PresentacionTests.Run();
            Console.WriteLine("Pruebas correctas: " + grupo);
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
    internal static void Check(bool ok, string mensaje) { if (!ok) throw new Exception(mensaje); }
}
