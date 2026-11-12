// Fecha: 12/11/2026 - TAREA: 00180 - Linea: 1,3 - Marcadores de mes (los dos formatos), último día del mes y normalización de texto.
using AsientosEstudio.Core;

static class TextoTests
{
    static Mes Parsear(string t) => Meses.TryParse(t, out var m) ? m : throw new Exception($"«{t}» no se leyó como mes");

    public static void Run(Action<string, Action> test)
    {
        test("Meses: «MES 05/2026», «05-2026» y «5/2026» son el mismo mes", () =>
        {
            Afirmar.Igual(new Mes(2026, 5), Parsear("MES 05/2026"));
            Afirmar.Igual(new Mes(2026, 5), Parsear("05-2026"));
            Afirmar.Igual(new Mes(2026, 5), Parsear(" 5 / 2026 "));
        });
        test("Meses: «MAYO 2026» en cualquier mayúscula, con tilde o con «de»", () =>
        {
            Afirmar.Igual(new Mes(2026, 5), Parsear("MAYO 2026"));
            Afirmar.Igual(new Mes(2026, 9), Parsear("Septiembre 2026"));
            Afirmar.Igual(new Mes(2026, 9), Parsear("setiembre de 2026"));
            Afirmar.Igual(new Mes(2026, 12), Parsear("Diciembre 2026"));
        });
        test("Meses: lo que no es un mes no se lee como mes", () =>
        {
            foreach (var t in new[] { "", "ASIENTO 1", "13/2026", "00/2026", "Compras", "Por egresos de caja", "BANCO BBVA", "2.01.01.003", "MAYO", "ABRIL DOS MIL" })
                Afirmar.Verdad(!Meses.TryParse(t, out _), $"«{t}» no es un mes");
        });
        test("Meses: el último día es el real, con febrero bisiesto y no bisiesto", () =>
        {
            Afirmar.Igual(new DateTime(2026, 5, 31), new Mes(2026, 5).UltimoDia);
            Afirmar.Igual(new DateTime(2026, 6, 30), new Mes(2026, 6).UltimoDia);
            Afirmar.Igual(new DateTime(2028, 2, 29), new Mes(2028, 2).UltimoDia);
            Afirmar.Igual(new DateTime(2026, 2, 28), new Mes(2026, 2).UltimoDia);
            Afirmar.Igual(new DateTime(2100, 2, 28), new Mes(2100, 2).UltimoDia); // 2100 no es bisiesto
            Afirmar.Igual(new DateTime(2000, 2, 29), new Mes(2000, 2).UltimoDia);
        });
        test("Meses: los textos del mes no dependen de la cultura", () =>
        {
            Afirmar.Igual("05/2026", new Mes(2026, 5).Texto);
            Afirmar.Igual("05-2026", new Mes(2026, 5).TextoGuion);
            Afirmar.Verdad(new Mes(2026, 5).CompareTo(new Mes(2026, 8)) < 0 && new Mes(2027, 1).CompareTo(new Mes(2026, 12)) > 0, "orden de meses");
        });
        test("Texto: normaliza tildes, mayúsculas y espacios", () =>
        {
            Afirmar.Igual("CODIGO", Texto.Normalizar("  Código "));
            Afirmar.Igual("A PRESTAMO NBSF", Texto.Normalizar("A PRÉSTAMO   NBSF "));
            Afirmar.Igual("", Texto.Normalizar(null));
        });
        test("Texto: un código es dígitos con puntos o con comas (la coma es el error A2)", () =>
        {
            Afirmar.Verdad(Texto.PareceCodigo("2.01.01.003"), "2.01.01.003");
            Afirmar.Verdad(Texto.PareceCodigo("4,01,05,050"), "4,01,05,050");
            Afirmar.Verdad(!Texto.PareceCodigo("Código"), "Código");
            Afirmar.Verdad(!Texto.PareceCodigo("ASIENTO 1"), "ASIENTO 1");
            Afirmar.Verdad(!Texto.PareceCodigo(""), "vacío");
        });
    }
}
