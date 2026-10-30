// Fecha: 30/10/2026 - TAREA: 00150 - Linea: 1 - Lee la «Lista de partidas individuales de deudores» que SAP exporta como HTML y la convierte en la misma tabla que un CSV.
using ArcaCliente.Web.Models;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
namespace ArcaCliente.Web.Services;
public static class SapListReader
{
    // Cada celda de la salida de SAP es un <nobr id="l{fila:4}{columna:3}">: el id da la posición exacta, así que no se depende del ancho del texto.
    // Fecha: 30/10/2026 - TAREA: 00150 - Linea: 1 - Revisión de Codex (ronda 1): el HTML lo sube el usuario; un <nobr> sin cierre repetido haría cuadrática la búsqueda, así que la regex tiene tiempo máximo y la cantidad de celdas tiene tope.
    const int MaxCells=1_000_000;
    static readonly Regex Cell=new(@"<nobr\b[^>]*?\bid=l(\d{4})(\d{3})[^>]*>(.*?)</nobr>",RegexOptions.Singleline|RegexOptions.Compiled,TimeSpan.FromSeconds(5));
    static readonly Regex Tags=new("<[^>]+>",RegexOptions.Compiled);
    static readonly Regex SapDate=new(@"^\d{2}\.\d{2}\.\d{4}$",RegexOptions.Compiled);
    public static bool IsSapList(string fileName)=>Path.GetExtension(fileName).ToLowerInvariant() is ".htm" or ".html";
    public static TabularDocument Read(UploadedDocument file)
    {
        string html;
        try { html=new UTF8Encoding(false,true).GetString(file.Content); } catch(DecoderFallbackException) { html=Encoding.Latin1.GetString(file.Content); }
        var rows=new SortedDictionary<int,SortedDictionary<int,string>>();
        int cellCount=0;
        try {
        foreach(Match m in Cell.Matches(html)) {
            if(++cellCount>MaxCells) throw new ImportException("El listado SAP tiene demasiadas celdas.");
            var text=WebUtility.HtmlDecode(Tags.Replace(m.Groups[3].Value,"")).Replace(' ',' ').Trim();
            int line=int.Parse(m.Groups[1].Value);
            if(!rows.TryGetValue(line,out var cells)) rows[line]=cells=new();
            cells[int.Parse(m.Groups[2].Value)]=text;
        }
        } catch(RegexMatchTimeoutException) { throw new ImportException("No se pudo leer el listado SAP: el HTML está dañado."); }
        // La fila de títulos se repite en cada página del listado; alcanza con la primera.
        var titles=rows.Values.FirstOrDefault(r=>r.Values.Contains("Referencia")&&r.Values.Any(v=>v.StartsWith("Fecha doc",StringComparison.OrdinalIgnoreCase)));
        if(titles==null) throw new ImportException("No se reconoció el listado SAP: falta la fila de títulos (Referencia, Fecha doc., Importe).");
        var columns=titles.Where(t=>t.Value.Length>0).ToList();
        var headers=columns.Select(c=>c.Value).ToList();
        for(int i=0;i<headers.Count;i++) if(headers.IndexOf(headers[i])!=i) headers[i]=$"{headers[i]} ({columns[i].Key})";
        int dateColumn=columns.FindIndex(c=>c.Value.StartsWith("Fecha doc",StringComparison.OrdinalIgnoreCase));
        int amountColumn=columns.FindIndex(c=>c.Value.StartsWith("Importe",StringComparison.OrdinalIgnoreCase));
        int Slot(int offset){int slot=columns.FindLastIndex(c=>c.Key<=offset);return slot<0?0:slot;}
        var result=new List<TabularRow>();
        foreach(var (line,cells) in rows) {
            // Una partida es una fila con fecha de documento; cabeceras, subtotales (*, **, ***) y totales no la tienen.
            if(!cells.Any(c=>Slot(c.Key)==dateColumn&&SapDate.IsMatch(c.Value))) continue;
            if(result.Count>=TabularReader.MaxRows) throw new ImportException("Máximo 50.000 filas y 256 columnas.");
            var values=new string[headers.Count];Array.Fill(values,"");
            foreach(var (offset,text) in cells) {
                int slot=Slot(offset);
                values[slot]=slot==amountColumn?Amount(text):SapDate.IsMatch(text)?text.Replace('.','/'):text;
            }
            result.Add(new(line,values));
        }
        if(result.Count==0) throw new ImportException("No hay filas para importar.");
        return new(headers.ToArray(),result);
    }
    // «31 255,75-» pasa a «-31255,75»: SAP agrupa miles con espacio y pone el signo al final.
    static string Amount(string text)
    {
        var s=text.Replace(" ","");
        return s.EndsWith('-')?"-"+s[..^1]:s;
    }
    // Fecha: 30/10/2026 - TAREA: 00150 - Linea: 1 - Precarga el formato del proveedor con las columnas del listado, así quien lo carga no tiene que escribirlas.
    public static void Apply(UploadedDocument file,FileFormat f)
    {
        var headers=Read(file).Headers;
        string Find(Func<string,bool> match,string what)=>headers.FirstOrDefault(match)??throw new ImportException($"El listado SAP no trae la columna de {what}.");
        f.Header=true;f.FirstRow=1;f.Sheet="";f.DatePattern="dd/MM/yyyy";f.DecimalSeparator=",";
        f.Type=Find(h=>h.Equals("Cl",StringComparison.OrdinalIgnoreCase)||h.StartsWith("Clase",StringComparison.OrdinalIgnoreCase),"clase de documento");
        f.Number=Find(h=>h.Equals("Referencia",StringComparison.OrdinalIgnoreCase),"referencia");
        f.Date=Find(h=>h.StartsWith("Fecha doc",StringComparison.OrdinalIgnoreCase),"fecha de documento");
        f.Amount=Find(h=>h.StartsWith("Importe",StringComparison.OrdinalIgnoreCase),"importe");
        f.Letter="";f.Point="";f.NumberMode=NumberMode.Fixed;f.NumberOrder=NumberOrder.PointLetterNumber;f.PointLength=4;
    }
}
