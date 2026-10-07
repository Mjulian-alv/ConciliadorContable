// Fecha: 06/10/2026 - TAREA: 00072 - Linea: 2 - La versión pertenece al host web sin alterar el contrato de los formularios.
using ArcaCliente.Models;
using ArcaCliente.Web.Services;
using System.Text.Json;
namespace ArcaCliente.Web.Models;

public sealed class WebPyRProfile
{
    public PerfilOfflinePyR Profile { get; set; } = new() { Nombre = "Nuevo perfil PyR" };
    public string Version { get; set; } = "";
    public WebPyRProfile Copy() => JsonSerializer.Deserialize<WebPyRProfile>(JsonSerializer.Serialize(this))!;
}

public static class PyRProfileValidator
{
    public static void Validate(WebPyRProfile value)
    {
        var p = value.Profile;
        if (p.Id == Guid.Empty || string.IsNullOrWhiteSpace(p.Nombre) || p.Nombre.Length > 200)
            throw new ImportException("Indicá un nombre de perfil de hasta 200 caracteres.");
        if ((p.CarpetaArca?.Length ?? 0) > 500 || (p.HojaExcel?.Length ?? 0) > 200)
            throw new ImportException("La carpeta o la hoja supera la longitud permitida.");
        foreach (var (name, column, optional) in new[] {
            ("Fecha",p.ColFecha,false),("Concepto",p.ColConcepto,false),("Debe",p.ColDebe,false),
            ("Haber",p.ColHaber,false),("Operador",p.ColOperador,false),("Asiento",p.ColAsiento,true) })
        {
            if (optional && string.IsNullOrWhiteSpace(column)) continue;
            if (string.IsNullOrWhiteSpace(column) || column.Length > 100 ||
                !p.TieneCabecera && (!int.TryParse(column,out var position) || position < 1 || position > 256))
                throw new ImportException($"Revisá la columna {name}: indicá un encabezado o una posición desde 1, según el formato.");
        }
        if (string.IsNullOrWhiteSpace(p.FormatoFecha) || p.FormatoFecha.Length > 30 || p.SeparadorDecimal is not ("." or ","))
            throw new ImportException("Revisá el formato de fecha y el separador decimal.");
        try { _ = DateTime.Today.ToString(p.FormatoFecha); } catch (FormatException) { throw new ImportException("El formato de fecha no es válido."); }
        if (p.Cuentas == null || p.Cuentas.Any(c => c.Id == Guid.Empty || string.IsNullOrWhiteSpace(c.Codigo) || string.IsNullOrWhiteSpace(c.Nombre)
            || !Enum.IsDefined(c.Tipo) || !Enum.IsDefined(c.Impuesto)) || p.Cuentas.Select(c=>c.Id).Distinct().Count()!=p.Cuentas.Count
            || p.Cuentas.Select(c=>c.Codigo.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count()!=p.Cuentas.Count)
            throw new ImportException("Completá código y nombre de cada cuenta, sin códigos repetidos.");
        ValidateDirectives(p.DirectivasConciliacion);
    }
    public static void ValidateDirectives(IReadOnlyList<DirectivaPyR> directives)
    {
        if (directives == null || directives.Count == 0 || directives[0].Campos == null ||
            !directives[0].Campos.SequenceEqual(new[]{CampoPyR.Numero}))
            throw new ImportException("La primera directiva debe conservar Número completo.");
        if (directives.Any(d=>string.IsNullOrWhiteSpace(d.Descripcion) || d.Campos == null || d.Campos.Count == 0 || d.Campos.Any(c=>!Enum.IsDefined(c))))
            throw new ImportException("Cada directiva necesita una descripción y al menos un campo.");
    }
}
