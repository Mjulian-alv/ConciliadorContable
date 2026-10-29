// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 2 - Lectura acotada y estricta: ningún error al final del archivo queda fuera de la validación.
using ArcaCliente.Web.Models;
using ClosedXML.Excel;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using System.IO.Compression;
using System.Text;
namespace ArcaCliente.Web.Services;
public static class TabularReader
{
    public const int MaxBytes=20*1024*1024, MaxRows=50000, MaxColumns=256;
    // Fecha: 06/10/2026 - TAREA: 00067 - Linea: 1 - ARCA histórico puede incluir columnas sin nombre; el resto de importaciones sigue estricto.
    public static TabularDocument Read(UploadedDocument file, FileFormat format,bool arcaHeaders=false)
    {
        ValidateFormat(format);
        if(file.Content.Length==0 || file.Content.Length>MaxBytes) throw new ImportException("Archivo vacío o mayor a 20 MB.");
        var rows=new List<TabularRow>();
        var ext=Path.GetExtension(file.Name).ToLowerInvariant();
        try {
            if(ext==".xlsx") {
                using var stream=new MemoryStream(file.Content);
                using(var zip=new ZipArchive(stream,ZipArchiveMode.Read,true))
                    if(zip.Entries.Sum(e=>e.Length)>100L*1024*1024) throw new ImportException("El Excel expandido supera 100 MB.");
                // Fecha: 29/10/2026 - TAREA: 00142 - Linea: 1 - Los pivotes no aportan filas: ClosedXML falla al abrir cachés de Excel con campos agrupados y el libro entero se rechazaba.
                // Se descartan las partes de tablas dinámicas antes de abrir y la hoja elegida se lee igual.
                using var clean=WithoutPivots(file.Content);
                using var book=new XLWorkbook(clean);
                var sheet=string.IsNullOrWhiteSpace(format.Sheet)?book.Worksheets.First():book.Worksheet(format.Sheet);
                int last=sheet.LastRowUsed()?.RowNumber()??0, columns=sheet.LastColumnUsed()?.ColumnNumber()??0;
                if(last>MaxRows || columns>MaxColumns) throw new ImportException("Máximo 50.000 filas y 256 columnas.");
                for(int r=format.FirstRow;r<=last;r++) {
                    var cells=new string[columns];
                    for(int c=1;c<=columns;c++) {
                        var cell=sheet.Cell(r,c);
                        if(cell.HasFormula) throw new ImportException($"Fila {r}: exportá las fórmulas como valores antes de importar.");
                        cells[c-1]=cell.DataType switch {
                            XLDataType.DateTime=>cell.GetDateTime().ToString(format.DatePattern,CultureInfo.InvariantCulture),
                            XLDataType.Number=>cell.GetDouble().ToString("0.################",CultureInfo.InvariantCulture).Replace(".",format.DecimalSeparator),
                            _=>cell.GetString()
                        };
                    }
                    if(cells.Any(x=>!string.IsNullOrWhiteSpace(x))) rows.Add(new(r,cells));
                }
            } else if(ext is ".csv" or ".txt") {
                Encoding encoding=format.Encoding=="Latin-1"?Encoding.Latin1:new UTF8Encoding(false,true);
                var content=encoding.GetString(file.Content).TrimStart('\uFEFF');
                using var parser=new TextFieldParser(new StringReader(content)) { HasFieldsEnclosedInQuotes=true, TrimWhiteSpace=false };
                parser.SetDelimiters(format.Separator);
                while(!parser.EndOfData) {
                    int line=checked((int)parser.LineNumber); var cells=parser.ReadFields()??Array.Empty<string>();
                    if(line<format.FirstRow) continue;
                    if(rows.Count>=MaxRows || cells.Length>MaxColumns) throw new ImportException("Máximo 50.000 filas y 256 columnas.");
                    if(cells.Any(x=>!string.IsNullOrWhiteSpace(x))) rows.Add(new(line,cells));
                }
            } else throw new ImportException("Seleccioná un archivo XLSX, CSV o TXT delimitado.");
        } catch(ImportException) { throw; } catch(Exception ex) when(ex is not OutOfMemoryException) {
            throw new ImportException("No se pudo leer el archivo. Revisá el formato, la hoja, las comillas y la codificación.");
        }
        if(rows.Count==0) throw new ImportException("No hay filas para importar.");
        string[] headers=format.Header?rows[0].Cells.Select(x=>x.Trim()).ToArray():Enumerable.Range(1,rows.Max(r=>r.Cells.Length)).Select(x=>x.ToString()).ToArray();
        // Fecha: 06/10/2026 - TAREA: 00067 - Linea: 1 - Asigna identificadores sólo a columnas vacías sin modificar ni desplazar sus valores originales.
        if(arcaHeaders)headers=headers.Select((h,i)=>string.IsNullOrWhiteSpace(h)?$"__ARCA_SIN_CABECERA_{i}":h).ToArray();
        if(headers.Any(string.IsNullOrWhiteSpace) || headers.Distinct(StringComparer.OrdinalIgnoreCase).Count()!=headers.Length)
            throw new ImportException("La cabecera contiene columnas vacías o repetidas.");
        if(format.Header) rows.RemoveAt(0);
        return new(headers,rows);
    }
    // Fecha: 29/10/2026 - TAREA: 00142 - Linea: 1 - Reescribe el .xlsx sin tablas dinámicas: quita sus partes y las referencias desde workbook.xml y los .rels que las apuntan.
    static MemoryStream WithoutPivots(byte[] content)
    {
        static bool IsPivot(string name)=>name.Replace('\\','/').StartsWith("xl/pivot",StringComparison.OrdinalIgnoreCase);
        using var source=new ZipArchive(new MemoryStream(content),ZipArchiveMode.Read);
        var output=new MemoryStream();
        if(!source.Entries.Any(e=>IsPivot(e.FullName))) { output.Write(content);output.Position=0;return output; }
        using(var target=new ZipArchive(output,ZipArchiveMode.Create,true))
            foreach(var entry in source.Entries) {
                if(IsPivot(entry.FullName)) continue;
                var path=entry.FullName.Replace('\\','/');
                var copy=target.CreateEntry(path,CompressionLevel.Fastest);
                using var read=entry.Open(); using var write=copy.Open();
                if(path.Equals("xl/workbook.xml",StringComparison.OrdinalIgnoreCase)) {
                    var doc=System.Xml.Linq.XDocument.Load(read);
                    doc.Descendants().Where(x=>x.Name.LocalName=="pivotCaches").ToList().ForEach(x=>x.Remove());
                    doc.Save(write);
                } else if(path.EndsWith(".rels",StringComparison.OrdinalIgnoreCase)) {
                    var doc=System.Xml.Linq.XDocument.Load(read);
                    doc.Descendants().Where(x=>x.Name.LocalName=="Relationship"&&((string?)x.Attribute("Type")??"").Contains("pivot",StringComparison.OrdinalIgnoreCase)).ToList().ForEach(x=>x.Remove());
                    doc.Save(write);
                } else read.CopyTo(write);
            }
        output.Position=0;return output;
    }
    public static void ValidateFormat(FileFormat f)
    {
        if(f.Separator.Length!=1 || f.Separator[0] is '\r' or '\n' or '"') throw new ImportException("El separador debe ser un carácter válido.");
        if(f.Encoding is not ("UTF-8" or "Latin-1")) throw new ImportException("Codificación no admitida.");
        if(f.FirstRow<1 || f.FirstRow>MaxRows) throw new ImportException("La primera fila debe estar entre 1 y 50.000.");
        if(f.DecimalSeparator is not ("," or ".")) throw new ImportException("Separador decimal inválido.");
        if(f.DatePattern is not ("dd/MM/yyyy" or "d/M/yyyy" or "yyyy-MM-dd" or "dd-MM-yyyy" or "yyyyMMdd")) throw new ImportException("Seleccioná un formato de fecha admitido.");
        if(f.NumberMode==NumberMode.Fixed && (f.PointLength<1||f.PointLength>5)) throw new ImportException("El punto de venta ocupa entre 1 y 5 posiciones.");
        if(f.NumberMode==NumberMode.Separated && string.IsNullOrEmpty(f.NumberSeparator)) throw new ImportException("Indicá el separador de punto y número.");
    }
}
