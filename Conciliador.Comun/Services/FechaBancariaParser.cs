// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1, 2 - Una sola interpretación para importación y migración.
using System.Globalization;
namespace Conciliador.Comun.Services;
public static class FechaBancariaParser
{
    private static readonly string[] Formatos = CrearFormatos();
    private static string[] CrearFormatos()
    {
        var formatos = new List<string>();
        foreach (var dia in new[] { "d/M/yyyy", "d-M-yyyy", "yyyy-MM-dd" })
        {
            formatos.Add(dia);
            foreach (var hora in new[] { "H:mm", "H:mm:ss", "H:mm:ss.FFFFFFF" })
                foreach (var separador in dia == "yyyy-MM-dd" ? new[] { " ", "'T'" } : new[] { " " })
                {
                    formatos.Add(dia + separador + hora);
                    if (dia == "yyyy-MM-dd")
                    {
                        formatos.Add(dia + separador + hora + "zzz");
                        formatos.Add(dia + separador + hora + "'Z'");
                    }
                }
        }
        return formatos.ToArray();
    }
    public static bool TryParse(object valor, out DateTime fecha)
    {
        fecha = default;
        if (valor is DateTime tipada)
        {
            fecha = DateTime.SpecifyKind(tipada.Date, DateTimeKind.Unspecified);
            return true;
        }
        if (valor is not string texto || string.IsNullOrWhiteSpace(texto)) return false;
        // DateTimeOffset.Date conserva el día escrito incluso si el offset cruza medianoche en Argentina.
        if (!DateTimeOffset.TryParseExact(texto.Trim(), Formatos, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var interpretada)) return false;
        fecha = DateTime.SpecifyKind(interpretada.Date, DateTimeKind.Unspecified);
        return true;
    }
}
