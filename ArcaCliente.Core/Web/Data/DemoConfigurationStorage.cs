// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 2 - Demostración explícita en memoria, sin conexiones ni credenciales productivas.
using ArcaCliente.Web.Models;
using ArcaCliente.Web.Services;
using System.Text.Json;
namespace ArcaCliente.Web.Data;
public sealed class DemoConfigurationStorage : IWebConfigurationStore
{
    readonly object gate=new();readonly Dictionary<Guid,WebProfile> profiles=new();readonly Dictionary<string,ProviderConfiguration> providers=new();
    public DemoConfigurationStorage() {
        var arca=new WebProfile();arca.Profile.Nombre="ARCA · comprobantes recibidos";arca.Version="1";
        var provider=new WebProfile {Source=ReconciliationSource.Provider};provider.Profile.Nombre="Proveedores · pendientes de pago";provider.Version="1";
        profiles[arca.Profile.Id]=arca;profiles[provider.Profile.Id]=provider;
    }
    static T Copy<T>(T x)=>JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(x))!;
    public Task<List<WebProfile>> Profiles(){lock(gate)return Task.FromResult(profiles.Values.Select(Copy).OrderBy(x=>x.Profile.Nombre).ToList());}
    public Task<WebProfile> Save(WebProfile x){ ConfigurationValidator.Profile(x);lock(gate){
        var current=profiles.GetValueOrDefault(x.Profile.Id);if((current?.Version??"")!=x.Version)throw new ConfigurationConflictException();
        var saved=Copy(x);saved.Version=Guid.NewGuid().ToString();profiles[x.Profile.Id]=saved;return Task.FromResult(Copy(saved));
    }}
    public Task<ProviderConfiguration> Provider(string cuit){cuit=CuitListImporter.Normalize(cuit);lock(gate)return Task.FromResult(providers.TryGetValue(cuit,out var p)?Copy(p):new ProviderConfiguration{Cuit=cuit});}
    public Task<ProviderConfiguration> Save(ProviderConfiguration x){ ConfigurationValidator.Provider(x);lock(gate){
        var current=providers.GetValueOrDefault(x.Cuit);if((current?.Version??"")!=x.Version)throw new ConfigurationConflictException();
        var saved=Copy(x);saved.Version=Guid.NewGuid().ToString();providers[x.Cuit]=saved;return Task.FromResult(Copy(saved));
    }}
}
public static class ConfigurationValidator
{
    public static void Provider(ProviderConfiguration p) {
        p.Cuit=CuitListImporter.Normalize(p.Cuit);TabularReader.ValidateFormat(p.Format);
        if(p.Types.Any(x=>string.IsNullOrWhiteSpace(x.Key)||x.Value!="EXCLUIR"&&!FiscalCatalog.All.ContainsKey(x.Value)))throw new ImportException("Equivalencia inválida.");
        if(p.Types.Keys.Distinct(StringComparer.OrdinalIgnoreCase).Count()!=p.Types.Count)throw new ImportException("Hay tipos repetidos con distintas mayúsculas.");
    }
    public static void Profile(WebProfile p) {
        if(string.IsNullOrWhiteSpace(p.Profile.Nombre)||p.Profile.Nombre.Length>200)throw new ImportException("Ingresá un nombre de hasta 200 caracteres.");
        TabularReader.ValidateFormat(p.SystemFormat);
        p.Cuits=p.Cuits.Select(CuitListImporter.Normalize).Distinct().Order().ToList();
        if(p.Source==ReconciliationSource.Arca && p.FilterCuits && p.Cuits.Count==0)throw new ImportException("Cargá una lista de CUIT antes de activar el filtro.");
        var directives=p.Profile.DirectivasConciliacion;
        var first=ArcaCliente.Models.DirectivaConciliacion.CrearPrimaria();
        if(directives.Count==0||!directives[0].Campos.SequenceEqual(first.Campos))throw new ImportException("La directiva principal no puede modificarse.");
        if(directives.Any(d=>d.Campos.Count==0||d.Campos.Distinct().Count()!=d.Campos.Count||d.Campos.Any(c=>!Enum.IsDefined(c))))throw new ImportException("Revisá los campos de las directivas.");
    }
}
