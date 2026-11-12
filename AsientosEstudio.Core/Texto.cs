// Fecha: 12/11/2026 - TAREA: 00180 - Linea: 1,3 - Utilidades de texto: normalización para comparar y lectura de los dos formatos de mes.
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AsientosEstudio.Core;

public static class Texto
{
    /// <summary>
    /// Mayúsculas, sin tildes y con los espacios colapsados. Las planillas no son rígidas («Codigo» y «Código»,
    /// «INFORMATIVO» con cualquier mayúscula), así que todo lo que se reconoce por texto se compara normalizado.
    /// </summary>
    public static string Normalizar(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        var d = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(d.Length);
        foreach (var c in d)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
        return Regex.Replace(sb.ToString().Normalize(NormalizationForm.FormC).ToUpperInvariant(), @"\s+", " ").Trim();
    }

    // Un código de cuenta: dígitos con separadores («2.01.01.003»). Acepta la coma («4,01,05,050»), que es el error A2.
    private static readonly Regex Codigo = new(@"^\d+([.,]\d+)*$", RegexOptions.Compiled);

    public static bool PareceCodigo(string? s) => !string.IsNullOrEmpty(s) && Codigo.IsMatch(s.Trim());
}

public static class Meses
{
    private static readonly string[] Nombres =
        { "ENERO", "FEBRERO", "MARZO", "ABRIL", "MAYO", "JUNIO", "JULIO", "AGOSTO", "SEPTIEMBRE", "OCTUBRE", "NOVIEMBRE", "DICIEMBRE" };

    // «05/2026», «5-2026», «MES 05/2026».
    private static readonly Regex Numerico = new(@"^(?:MES\s+)?(\d{1,2})\s*[/-]\s*(\d{4})$", RegexOptions.Compiled);
    // «MAYO 2026», «Mayo de 2026», «MES MAYO 2026».
    private static readonly Regex Nombre = new(@"^(?:MES\s+)?([A-Z]+)\s+(?:DE\s+)?(\d{4})$", RegexOptions.Compiled);

    /// <summary>Lee un marcador de mes en cualquiera de sus formatos. Devuelve false si el texto no es un mes.</summary>
    public static bool TryParse(string? texto, out Mes mes)
    {
        mes = default;
        var t = Texto.Normalizar(texto);
        if (t.Length == 0) return false;

        var m = Numerico.Match(t);
        if (m.Success)
        {
            var n = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
            var a = int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
            return Crear(a, n, out mes);
        }

        m = Nombre.Match(t);
        if (m.Success)
        {
            var palabra = m.Groups[1].Value == "SETIEMBRE" ? "SEPTIEMBRE" : m.Groups[1].Value;
            var idx = Array.IndexOf(Nombres, palabra);
            if (idx < 0) return false;
            return Crear(int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture), idx + 1, out mes);
        }

        return false;
    }

    private static bool Crear(int anio, int numero, out Mes mes)
    {
        mes = default;
        if (numero < 1 || numero > 12 || anio < 1900 || anio > 2999) return false;
        mes = new Mes(anio, numero);
        return true;
    }
}
