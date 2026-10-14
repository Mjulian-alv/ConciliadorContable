// Fecha: 14/10/2026 - TAREA: 00097 - Linea: 6 - Archivos que llegan del navegador: se validan antes de leerlos y se usan como flujo, nunca como ruta.
using Microsoft.AspNetCore.Components.Forms;
namespace Conciliador.Web.Services;
public sealed record ArchivoSubido(string Nombre, string Extension, byte[] Contenido)
{
    public long Tamano => Contenido.LongLength;
    /// <summary>Un flujo nuevo cada vez: se lee más de una vez (encabezados, vista previa, datos).</summary>
    public MemoryStream Abrir() => new(Contenido, false);
}
public static class ArchivosWeb
{
    public const long LimiteBytes = 25L * 1024 * 1024;
    public static readonly string[] Extensiones = { ".xls", ".xlsx", ".csv" };
    /// <summary>Motivo por el que no se acepta el archivo, o null si se puede leer.</summary>
    public static string? Motivo(string nombre, long tamano)
    {
        string ext = Path.GetExtension(nombre ?? "").ToLowerInvariant();
        if (!Extensiones.Contains(ext)) return "Sólo se aceptan archivos .xls, .xlsx o .csv.";
        if (tamano <= 0) return "El archivo está vacío.";
        if (tamano > LimiteBytes) return "El archivo supera los 25 MB.";
        return null;
    }
    public static async Task<ArchivoSubido> LeerAsync(IBrowserFile archivo)
    {
        var motivo = Motivo(archivo.Name, archivo.Size);
        if (motivo != null) throw new InvalidOperationException(motivo);
        using var ms = new MemoryStream();
        await using (var origen = archivo.OpenReadStream(LimiteBytes)) await origen.CopyToAsync(ms);
        return new ArchivoSubido(Path.GetFileName(archivo.Name), Path.GetExtension(archivo.Name).ToLowerInvariant(), ms.ToArray());
    }
}
