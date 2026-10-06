// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 2 - Normaliza sólo la representación; las filas originales permanecen visibles e inmutables.
using ArcaCliente.Models;
using ArcaCliente.Services;
using ArcaCliente.Web.Models;
using System.Globalization;
using System.Text.RegularExpressions;
namespace ArcaCliente.Web.Services;
public static class FiscalCatalog
{
    public static IReadOnlyDictionary<string,string> All { get; } = TablaComprobanteDescripcionMapper.ObtenerTodosLosCodigos()
        .Select(x=>new KeyValuePair<string,string>(x,TablaComprobanteDescripcionMapper.ObtenerDescripcion(x)))
        .Where(x=>x.Value.StartsWith("FACTURA")||x.Value.StartsWith("NOTA DE CREDITO")||x.Value.StartsWith("NOTA DE DEBITO")||x.Value.StartsWith("NOTAS DE CREDITO")||x.Value.StartsWith("NOTAS DE DEBITO")||x.Value.StartsWith("TIQUE FACTURA")||x.Value.StartsWith("TIQUE NOTA DE CREDITO")||x.Value.StartsWith("TIQUE NOTA DE DEBITO"))
        .ToDictionary(x=>x.Key,x=>x.Value);
    public static string Normalize(string s)=>int.TryParse(s.Trim(),out var n)?n.ToString("000"):s.Trim().ToUpperInvariant();
    public static int Sign(string s)=>All.TryGetValue(Normalize(s),out var d)&&d.Contains("CREDITO")?-1:1;
    public static string Suggest(string value)=>value.Trim().ToUpperInvariant() switch { "FA" or "FC A"=>"001", "FB" or "FC B"=>"006", "FC C"=>"011", "NCA"=>"003", "NCB"=>"008", "NCC"=>"013", "NDA"=>"002", "NDB"=>"007", "NDC"=>"012", _=>All.ContainsKey(Normalize(value))?Normalize(value):"" };
}
public static class ProviderImporter
{
    public static ImportPreview Preview(UploadedDocument file, ProviderConfiguration config) => Preview(file,config.Format,config.Cuit,config.Types,false);
    // Fecha: 05/10/2026 - TAREA: 00065 - Linea: 1 - Sólo se omiten códigos del sistema que el perfil excluye expresamente.
    public static ImportPreview Preview(UploadedDocument file, FileFormat f, string? assignedCuit=null, IReadOnlyDictionary<string,string>? types=null, bool canonical=true, bool arcaDates=false, IReadOnlyCollection<string>? excludedTypes=null)
    {
        var result=new ImportPreview();
        try {
            if(assignedCuit!=null) assignedCuit=CuitListImporter.Normalize(assignedCuit);
            // Fecha: 06/10/2026 - TAREA: 00067 - Linea: 1 - La tolerancia a columnas vacías corresponde sólo a archivos ARCA.
            var table=TabularReader.Read(file,f,arcaHeaders:arcaDates);
            int Column(string name,bool required=true) {
                int i=Array.FindIndex(table.Headers,x=>x.Equals(name.Trim(),StringComparison.OrdinalIgnoreCase));
                if(i<0 && required) throw new ImportException($"No se encontró la columna «{name}».");return i;
            }
            int type=Column(f.Type);
            // Los campos fiscales se requieren sólo si hay documentos incluidos; un recibo no los necesita.
            int date=Column(f.Date,false),point=Column(f.Point,false),number=Column(f.Number,false),amount=Column(f.Amount,false),cuit=Column(f.Cuit,false),name=Column(f.Name,false);
            foreach(var row in table.Rows) {
                string At(int index)=>index>=0 && index<row.Cells.Length?row.Cells[index].Trim():"";
                var raw=At(type);var p=new PreviewRow { Line=row.Line,RawType=raw,RawAmount=At(amount),RawCells=row.Cells }; result.Rows.Add(p);
                string mapped="";
                if(canonical) mapped=FiscalCatalog.Normalize(raw);
                else if(types!=null) { var entry=types.FirstOrDefault(x=>x.Key.Equals(raw,StringComparison.OrdinalIgnoreCase));mapped=entry.Value??""; }
                // Fecha: 05/10/2026 - TAREA: 00065 - Linea: 1 - La exclusión conserva la fila original para revisión y no se infiere del código cero.
                if(canonical&&excludedTypes?.Contains(mapped)==true){p.Excluded=true;continue;}
                if(mapped=="EXCLUIR") { p.Excluded=true;continue; }
                if(canonical && !FiscalCatalog.All.ContainsKey(mapped) && TablaComprobanteDescripcionMapper.ObtenerTodosLosCodigos().Contains(mapped)) { p.Excluded=true;continue; }
                if(!FiscalCatalog.All.ContainsKey(mapped)) { p.Errors.Add("Tipo sin equivalencia fiscal o exclusión confirmada.");if(!result.UnknownTypes.Contains(raw))result.UnknownTypes.Add(raw);continue; }
                // Fecha: 06/10/2026 - TAREA: 00067 - Linea: 1 - ARCA replica el mapeo por posición de WinForms: valida campos fiscales y conserva las columnas sobrantes para revisión.
                if(row.Cells.Length!=table.Headers.Length){
                    if(!arcaDates)p.Errors.Add("Cantidad de columnas distinta de la cabecera.");
                    else {
                        bool emptyTail=row.Cells.Length>table.Headers.Length
                            ?row.Cells.Skip(table.Headers.Length).All(string.IsNullOrWhiteSpace)
                            :table.Headers.Skip(row.Cells.Length).All(h=>h.StartsWith("__ARCA_SIN_CABECERA_",StringComparison.Ordinal));
                        if(!emptyTail)result.Warnings.Add($"Fila {row.Line}: cantidad de columnas distinta de la cabecera. Se aplicó el mapeo ARCA del escritorio; revisá la vista previa.");
                    }
                }
                var datePatterns=arcaDates?new[]{"yyyy-MM-dd","dd/MM/yyyy"}:new[]{f.DatePattern};
                if(!DateTime.TryParseExact(At(date),datePatterns,CultureInfo.InvariantCulture,DateTimeStyles.None,out var parsedDate)) p.Errors.Add("Fecha inválida para el formato elegido.");
                string parsedCuit=assignedCuit??"";
                if(assignedCuit==null) try{parsedCuit=CuitListImporter.Normalize(At(cuit));}catch(ImportException){p.Errors.Add("CUIT inválido.");}
                string pv=At(point),num=At(number);
                if(f.NumberMode==NumberMode.Separated) { var parts=num.Split(f.NumberSeparator,StringSplitOptions.None);if(parts.Length==2){pv=parts[0];num=parts[1];}else p.Errors.Add("El comprobante debe contener punto de venta y número."); }
                if(f.NumberMode==NumberMode.Fixed) { if(num.Length>f.PointLength){pv=num[..f.PointLength];num=num[f.PointLength..];}else p.Errors.Add("Número compuesto demasiado corto."); }
                if(!Regex.IsMatch(pv,@"^[0-9]{1,5}$"))p.Errors.Add("Punto de venta inválido (1 a 5 dígitos).");
                if(!Regex.IsMatch(num,@"^[0-9]{1,8}$")||!long.TryParse(num,out var sequence)||sequence==0)p.Errors.Add("Número inválido (1 a 8 dígitos, mayor a cero).");
                decimal total=0;try{total=ParseAmount(At(amount),f.DecimalSeparator);}catch(ImportException e){p.Errors.Add(e.Message);}
                if(p.Errors.Count==0) p.Document=new() { Fecha=parsedDate,TipoComprobante=mapped,Cuit=parsedCuit,PuntoVenta=pv.PadLeft(5,'0'),Numero=num.PadLeft(8,'0'),Total=total,NombreProveedor=At(name) };
            }
            if(result.Rows.Count==0)result.Errors.Add("El archivo no contiene comprobantes.");
        } catch(ImportException e) {result.Errors.Add(e.Message);}
        return result;
    }
    public static decimal ParseAmount(string value,string dec)
    {
        var raw=value.Trim(); bool parens=raw.StartsWith('(')&&raw.EndsWith(')');if(parens)raw=raw[1..^1];
        string group=dec==","?".":",";
        string pattern=@"^[+-]?(?:[0-9]+|[0-9]{1,3}(?:"+Regex.Escape(group)+@"[0-9]{3})+)(?:"+Regex.Escape(dec)+@"[0-9]{1,2})?$";
        if((parens&&(raw.StartsWith('-')||raw.StartsWith('+')))||!Regex.IsMatch(raw,pattern)||!decimal.TryParse(raw.Replace(group,"").Replace(dec,"."),NumberStyles.AllowLeadingSign|NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out var n)||Math.Abs(n)>9999999999999999.99m)
            throw new ImportException("Importe inválido: revisá decimales y separadores (máximo 2 decimales).");
        return decimal.Parse(Math.Abs(n).ToString("F2",CultureInfo.InvariantCulture),CultureInfo.InvariantCulture);
    }
}
