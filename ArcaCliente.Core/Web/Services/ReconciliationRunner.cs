// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 2 - El adaptador limita el universo y delega el emparejamiento completo al motor vigente.
using ArcaCliente.Models;
using ArcaCliente.Services;
using ArcaCliente.Web.Models;
using System.Globalization;
using System.Text;
namespace ArcaCliente.Web.Services;
public record CuitSummary(string Cuit, decimal External, decimal System) { public decimal Difference=>External-System; }
public sealed record ReconciliationResult(List<ItemConciliacion> Items,List<CuitSummary> Summary,int ExternalOmitted,int SystemOmitted,string Source);
public static class ReconciliationRunner
{
    public static ReconciliationResult Run(WebProfile profile,ImportPreview external,ImportPreview system,string? providerCuit,DateTime? from=null,DateTime? to=null)
    {
        if(!external.Ready||!system.Ready)throw new ImportException("Corregí ambos archivos antes de conciliar. No se importan lotes parciales.");
        if(from>to)throw new ImportException("La fecha desde no puede ser posterior a hasta.");
        var cuits=new HashSet<string>();
        if(profile.Source==ReconciliationSource.Provider)cuits.Add(CuitListImporter.Normalize(providerCuit??""));
        else if(profile.FilterCuits) {
            if(profile.Cuits.Count==0)throw new ImportException("El perfil tiene filtro CUIT activado pero su lista está vacía.");
            foreach(var c in profile.Cuits)cuits.Add(CuitListImporter.Normalize(c));
        }
        bool Accept(ComprobanteLocal x)=>(cuits.Count==0||cuits.Contains(x.Cuit))&&(!from.HasValue||x.Fecha.Date>=from.Value.Date)&&(!to.HasValue||x.Fecha.Date<=to.Value.Date);
        var a=external.Included.Where(Accept).ToList();var b=system.Included.Where(Accept).ToList();
        var docs=a.Select(x=>new ComprobanteCsv { FechaEmision=x.Fecha.ToString("dd/MM/yyyy"),NroDocEmisor=x.Cuit,TipoComprobante=x.TipoComprobante,PuntoVenta=x.PuntoVenta,NumeroDesde=x.Numero,DenominacionEmisor=x.NombreProveedor,ImpTotal=x.Total.ToString("F2",CultureInfo.GetCultureInfo("es-AR")) }).ToList();
        var engine=new ConciliacionService(()=>throw new InvalidOperationException("La conciliación de archivos no consulta SQL."),"");
        var previous=CultureInfo.CurrentCulture;
        List<ItemConciliacion> items;
        try {CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo("es-AR");items=engine.Conciliar(docs,b,profile.Profile.DirectivasConciliacion);}finally{CultureInfo.CurrentCulture=previous;}
        var totalsA=a.GroupBy(x=>x.Cuit).ToDictionary(g=>g.Key,g=>g.Sum(x=>FiscalCatalog.Sign(x.TipoComprobante)*x.Total));
        var totalsB=b.GroupBy(x=>x.Cuit).ToDictionary(g=>g.Key,g=>g.Sum(x=>FiscalCatalog.Sign(x.TipoComprobante)*x.Total));
        var summary=totalsA.Keys.Union(totalsB.Keys).Order().Select(c=>new CuitSummary(c,totalsA.GetValueOrDefault(c),totalsB.GetValueOrDefault(c))).ToList();
        return new(items,summary,external.Included.Count-a.Count,system.Included.Count-b.Count,profile.Source==ReconciliationSource.Provider?"Proveedor":"ARCA");
    }
    public static ImportPreview Arca(UploadedDocument file)
    {
        try {
            var f=new FileFormat();
            if(Path.GetExtension(file.Name).ToLowerInvariant() is ".csv" or ".txt") {
                string line=Encoding.UTF8.GetString(file.Content).Split('\n')[0];f.Separator=line.Contains(';')?";":",";
                try{new UTF8Encoding(false,true).GetString(file.Content);}catch(DecoderFallbackException){f.Encoding="Latin-1";}
            }
            var table=TabularReader.Read(file,f);
            string Normalize(string s)=>new(s.Normalize(NormalizationForm.FormD).Where(c=>CharUnicodeInfo.GetUnicodeCategory(c)!=UnicodeCategory.NonSpacingMark&&char.IsLetterOrDigit(c)).Select(char.ToLowerInvariant).ToArray());
            string Find(params string[] aliases)=>table.Headers.FirstOrDefault(h=>aliases.Contains(Normalize(h)))??aliases[0];
            f.Date=Find("fechadeemision","fechaemision");f.Type=Find("tipodecomprobante");f.Point=Find("puntodeventa");f.Number=Find("numerodesde","nrodesde");
            f.Cuit=Find("nrodocemisor");f.Name=Find("denominacionemisor");f.Amount=Find("importetotal","imptotal");
            return ProviderImporter.Preview(file,f);
        }catch(ImportException e){return new ImportPreview{Errors=new(){e.Message}};}
    }
}
