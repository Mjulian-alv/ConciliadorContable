// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 4 - Un espacio por circuito evita compartir archivos y descarta resultados al cambiar entradas.
using ArcaCliente.Web.Data;
using ArcaCliente.Web.Models;
using ArcaCliente.Web.Services;
using Conciliador.Web.Security;
namespace Conciliador.Web.Services;
public sealed class Workspace(IWebConfigurationStore store,AccessGuard access)
{
    public WebProfile? Profile{get;private set;}
    public ProviderConfiguration? Provider{get;private set;}
    public List<UploadedDocument> ExternalFiles{get;private set;}=new();
    public UploadedDocument? SystemFile{get;private set;}
    public ImportPreview? External{get;private set;}
    public ImportPreview? System{get;private set;}
    public ReconciliationResult? Result{get;private set;}
    public DateTime? From{get;set;} public DateTime? To{get;set;}
    public bool PendingConfirmed{get;set;}
    public string Saved{get;private set;}="";
    bool providerFormatPending,systemFormatPending;
    public void Invalidate(){Result=null;Saved="";}
    public void BeginExternalUpload(){Invalidate();External=null;ExternalFiles.Clear();PendingConfirmed=false;}
    public void BeginSystemUpload(){Invalidate();System=null;SystemFile=null;}
    public async Task Select(Guid id,string cuit="") {
        await access.Require("ArcaOffline");Clear();
        var selected=(await store.Profiles()).Single(x=>x.Profile.Id==id);
        ProviderConfiguration? provider=null;
        if(selected.Source==ReconciliationSource.Provider)provider=await store.Provider(CuitListImporter.Normalize(cuit));
        Profile=selected;Provider=provider;
    }
    public void Clear(){Profile=null;Provider=null;ExternalFiles.Clear();SystemFile=null;External=null;System=null;Result=null;PendingConfirmed=false;providerFormatPending=false;systemFormatPending=false;var today=DateTime.Today;From=new DateTime(today.Year,today.Month,1);To=today;}
    public async Task UploadExternal(List<UploadedDocument> files){await access.Require("ArcaOffline");Invalidate();External=null;ExternalFiles.Clear();
        if(Profile==null)throw new ImportException("Seleccioná un perfil.");
        if(files.Count==0||files.Count>10||files.Sum(f=>(long)f.Content.Length)>TabularReader.MaxBytes)throw new ImportException("Máximo 10 archivos y 20 MB en total.");
        if(Provider!=null&&files.Count!=1)throw new ImportException("El proveedor se importa desde un archivo por conciliación.");
        ExternalFiles=files;Refresh();
    }
    public async Task UploadSystem(UploadedDocument file){await access.Require("ArcaOffline");Invalidate();System=null;SystemFile=null;
        if(file.Content.Length>TabularReader.MaxBytes)throw new ImportException("Máximo 20 MB.");SystemFile=file;Refresh();}
    public async Task SaveProviderFormat(){providerFormatPending=true;Invalidate();External=null;await access.Require("ArcaOffline");if(Provider==null)return;
        var current=await store.Provider(Provider.Cuit);if(current.Version!=Provider.Version)throw new ConfigurationConflictException();
        Provider.Types=current.Types;Provider=await store.Save(Provider);providerFormatPending=false;Refresh();Saved="Formato guardado automáticamente";}
    public async Task MapType(string raw,string target){await access.Require("ArcaEquivalencias");EnsureSaved();Invalidate();External=null;if(Provider==null)return;
        var copy=global::System.Text.Json.JsonSerializer.Deserialize<ProviderConfiguration>(global::System.Text.Json.JsonSerializer.Serialize(Provider))!;
        copy.SetMapping(raw,target);Provider=await store.Save(copy);Refresh();Saved="Equivalencia guardada automáticamente";}
    public async Task SaveSystemFormat(){systemFormatPending=true;Invalidate();System=null;await access.Require("ArcaPerfiles");if(Profile==null)return;Profile=await store.Save(Profile);systemFormatPending=false;Refresh();Saved="Formato del sistema guardado automáticamente";}
    public void Refresh(){
        Invalidate();
        if(Profile==null)return;
        if(SystemFile!=null&&!systemFormatPending)System=ProviderImporter.Preview(SystemFile,Profile.SystemFormat);
        if(ExternalFiles.Count>0&&!providerFormatPending){
            External=new();
            foreach(var file in ExternalFiles){var p=Provider==null?ReconciliationRunner.Arca(file):ProviderImporter.Preview(file,Provider);External.Rows.AddRange(p.Rows);External.Errors.AddRange(p.Errors.Select(x=>file.Name+": "+x));External.UnknownTypes.AddRange(p.UnknownTypes);}
            if(External.Rows.Count>TabularReader.MaxRows)External.Errors.Add("El conjunto supera 50.000 filas.");External.UnknownTypes=External.UnknownTypes.Distinct().ToList();
        }
    }
    void EnsureSaved(){if(providerFormatPending||systemFormatPending)throw new ImportException("Hay cambios de formato sin guardar. Reintentá el guardado o recargá el perfil antes de continuar.");}
    async Task CheckVersions(){
        EnsureSaved();
        if(Profile==null)throw new ImportException("Seleccioná un perfil.");
        var current=(await store.Profiles()).SingleOrDefault(p=>p.Profile.Id==Profile.Profile.Id);
        if(current?.Version!=Profile.Version){Invalidate();throw new ConfigurationConflictException();}
        if(Provider!=null && (await store.Provider(Provider.Cuit)).Version!=Provider.Version){Invalidate();throw new ConfigurationConflictException();}
    }
    public async Task Reconcile(){await access.Require("ArcaOffline");Invalidate();await CheckVersions();
        if(!From.HasValue||!To.HasValue)throw new ImportException("Indicá las fechas desde y hasta para conciliar.");
        if(Provider!=null&&!PendingConfirmed)throw new ImportException("Confirmá que el archivo del proveedor contiene los comprobantes pendientes de pago.");
        if(External==null||System==null)throw new ImportException("Cargá los dos archivos.");
        Result=ReconciliationRunner.Run(Profile!,External,System,Provider?.Cuit,From,To);
    }
    public async Task<byte[]> Export(){await access.Require("ArcaOffline");await CheckVersions();if(Result==null)throw new ImportException("Volvé a conciliar antes de exportar.");return ResultExporter.Export(Result);}
}
