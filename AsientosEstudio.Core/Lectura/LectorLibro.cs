// Fecha: 12/11/2026 - TAREA: 00180 - Linea: 1 - Lee un .xlsx con ClosedXML y lo vuelca a la grilla cruda.
// Regla central (§4): los importes con fórmula se leen del VALOR CALCULADO GUARDADO en el archivo; no se recalcula. Si una celda con
// fórmula no tiene valor guardado, queda marcada (SinValorCalculado) para que A11 la señale en vez de leerla como cero.
using System.Globalization;
using System.Security.Cryptography;
using ClosedXML.Excel;

namespace AsientosEstudio.Core;

public static class LectorLibro
{
    public static LibroCrudo Leer(string ruta)
    {
        using var fs = File.OpenRead(ruta);
        return Leer(fs, Path.GetFileName(ruta));
    }

    public static LibroCrudo Leer(Stream contenido, string nombre)
    {
        // Se copia a memoria: hace falta el hash de los bytes originales y ClosedXML necesita un stream que se pueda releer.
        using var copia = new MemoryStream();
        contenido.CopyTo(copia);
        var bytes = copia.ToArray();
        var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var hojas = new List<HojaCruda>();
        var orden = 0;
        foreach (var ws in wb.Worksheets)
        {
            var hoja = new HojaCruda(ws.Name, orden++);
            foreach (var celda in ws.CellsUsed())
                hoja.Poner(celda.Address.RowNumber, celda.Address.ColumnNumber, Convertir(celda));
            hojas.Add(hoja);
        }
        return new LibroCrudo { Nombre = nombre, HashExcel = hash, Hojas = hojas };
    }

    private static CeldaCruda Convertir(IXLCell celda)
    {
        if (!celda.HasFormula) return Desde(celda.Value, null);

        // CachedValue es lo que el archivo guardó. Value podría disparar un recálculo; acá eso es justo lo que no se quiere.
        var c = Desde(celda.CachedValue, celda.FormulaA1);
        var vacio = c.Texto is null && c.Numero is null && c.Fecha is null && c.Error is null;
        return vacio ? c with { SinValorCalculado = true } : c;
    }

    private static CeldaCruda Desde(XLCellValue v, string? formula)
    {
        var baseCelda = new CeldaCruda { TieneFormula = formula is not null, Formula = formula };
        if (v.IsBlank) return baseCelda;
        if (v.IsError) return baseCelda with { Error = TextoDeError(v.GetError()) };
        if (v.IsNumber) return baseCelda with { Numero = ADecimal(v.GetNumber()) };
        if (v.IsDateTime) return baseCelda with { Fecha = v.GetDateTime() };
        if (v.IsBoolean) return baseCelda with { Texto = v.GetBoolean() ? "TRUE" : "FALSE" };
        if (v.IsText) return baseCelda with { Texto = v.GetText() };
        return baseCelda;
    }

    /// <summary>
    /// double → decimal por la representación más corta que vuelve al mismo double (la que Excel muestra), en cultura invariante.
    /// El cast directo redondea a 15 cifras significativas, y un importe de 4 a 6 decimales como los de VENTAS lo notaría.
    /// </summary>
    public static decimal ADecimal(double d)
    {
        var s = d.ToString("R", CultureInfo.InvariantCulture);
        return decimal.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture);
    }

    private static string TextoDeError(XLError e) => e switch
    {
        XLError.CellReference => "#REF!",
        XLError.DivisionByZero => "#DIV/0!",
        XLError.IncompatibleValue => "#VALUE!",
        XLError.NameNotRecognized => "#NAME?",
        XLError.NoValueAvailable => "#N/A",
        XLError.NullValue => "#NULL!",
        XLError.NumberInvalid => "#NUM!",
        _ => "#ERROR!",
    };
}
