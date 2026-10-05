// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 2 - La configuración web acompaña al perfil original sin cambiar sus contratos de escritorio.
using ArcaCliente.Models;
namespace ArcaCliente.Web.Models;
public record UploadedDocument(string Name, byte[] Content);
public enum ReconciliationSource { Arca, Provider }
public enum NumberMode { Columns, Separated, Fixed }
public sealed class FileFormat
{
    public TipoArchivoOffline FileType { get; set; } = TipoArchivoOffline.Csv;
    public string Separator { get; set; } = ";";
    public string Encoding { get; set; } = "UTF-8";
    public string Sheet { get; set; } = "";
    public bool Header { get; set; } = true;
    public int FirstRow { get; set; } = 1;
    public string DatePattern { get; set; } = "dd/MM/yyyy";
    public string DecimalSeparator { get; set; } = ",";
    public string Date { get; set; } = "Fecha";
    public string Type { get; set; } = "Tipo";
    public string Point { get; set; } = "PuntoVenta";
    public string Number { get; set; } = "Numero";
    public string Amount { get; set; } = "Importe";
    public string Cuit { get; set; } = "CUIT";
    public string Name { get; set; } = "Proveedor";
    public NumberMode NumberMode { get; set; }
    public string NumberSeparator { get; set; } = "-";
    public int PointLength { get; set; } = 5;
    public static FileFormat From(PerfilOffline p) => new() {
        FileType=p.TipoArchivo,Separator=p.Separador,Encoding=p.Encoding,Sheet=p.HojaExcel??"",Header=p.TieneCabecera,
        DatePattern=p.FormatoFecha,DecimalSeparator=p.SeparadorDecimal,
        Date=p.TieneCabecera?p.ColFecha??"Fecha":p.PosFecha.ToString(),Type=p.TieneCabecera?p.ColTipoComprobante??"Tipo":p.PosTipoComprobante.ToString(),
        Point=p.TieneCabecera?p.ColPuntoVenta??"PuntoVenta":p.PosPuntoVenta.ToString(),Number=p.TieneCabecera?p.ColNumero??"Numero":p.PosNumero.ToString(),
        Amount=p.TieneCabecera?p.ColTotal??"Importe":p.PosTotal.ToString(),Cuit=p.TieneCabecera?p.ColCuit??"CUIT":p.PosCuit.ToString(),Name=p.TieneCabecera?p.ColNombreProveedor??"Proveedor":p.PosNombreProveedor.ToString()
    };
}
public sealed class WebProfile
{
    public PerfilOffline Profile { get; set; } = new() { Nombre="Nuevo perfil", DirectivasConciliacion=new(){DirectivaConciliacion.CrearPrimaria()} };
    public ReconciliationSource Source { get; set; }
    public bool FilterCuits { get; set; }
    public List<string> Cuits { get; set; } = new();
    public string Version { get; set; } = "";
    public FileFormat SystemFormat { get; set; } = new();
}
public sealed class ProviderConfiguration
{
    public string Cuit { get; set; } = "";
    public FileFormat Format { get; set; } = new();
    public Dictionary<string,string> Types { get; set; } = new();
    public string Version { get; set; } = "";
}
public sealed record TabularRow(int Line, string[] Cells);
public sealed record TabularDocument(string[] Headers, List<TabularRow> Rows);
public sealed class PreviewRow
{
    public int Line { get; set; }
    public string RawType { get; set; } = "";
    public string RawAmount { get; set; } = "";
    public string[] RawCells { get; set; } = Array.Empty<string>();
    public bool Excluded { get; set; }
    public List<string> Errors { get; set; } = new();
    public ComprobanteLocal? Document { get; set; }
}
public sealed class ImportPreview
{
    public List<PreviewRow> Rows { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public List<string> UnknownTypes { get; set; } = new();
    public List<ComprobanteLocal> Included => Rows.Where(r=>!r.Excluded && r.Errors.Count==0 && r.Document!=null).Select(r=>r.Document!).ToList();
    public int ExcludedCount => Rows.Count(r=>r.Excluded);
    public bool Ready => Errors.Count==0 && UnknownTypes.Count==0 && Rows.All(r=>r.Errors.Count==0) && Included.Count>0;
}
public sealed class ImportException(string message) : Exception(message);
public sealed class ConfigurationConflictException() : Exception("Otra sesión modificó esta configuración. Volvé a cargarla antes de guardar.");
