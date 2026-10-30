// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 4 - Un espacio por circuito evita compartir archivos y descarta resultados al cambiar entradas.
using ArcaCliente.Web.Data;
using ArcaCliente.Web.Models;
using ArcaCliente.Web.Services;
using Conciliador.Web.Security;
namespace Conciliador.Web.Services;
// Fecha: 06/10/2026 - TAREA: 00067 - Linea: 1 - La carpeta del perfil tiene prioridad sobre la ruta configurada para este host.
public sealed record ArcaFolderOptions(string Directory);
public sealed class Workspace(IWebConfigurationStore store,AccessGuard access,ArcaFolderOptions? folderOptions=null)
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
    // Fecha: 06/10/2026 - TAREA: 00067 - Linea: 1 - La fuente ARCA es una carpeta completa, no un lote de archivos elegidos en el navegador.
    public string ArcaDirectory{get;set;}="";
    bool externalFromFolder;
    public void Invalidate(){Result=null;Saved="";}
    // Fecha: 06/10/2026 - TAREA: 00067 - Linea: 1 - Una carga fallida debe dejar sin efecto las entradas y el período anteriores.
    public void BeginExternalUpload(){Invalidate();External=null;ExternalFiles.Clear();PendingConfirmed=false;externalFromFolder=false;}
    public void BeginSystemUpload(){Invalidate();System=null;SystemFile=null;From=null;To=null;}
    public async Task Select(Guid id,string cuit="") {
        await access.Require("ArcaOffline");Clear();
        var selected=(await store.Profiles()).Single(x=>x.Profile.Id==id);
        ProviderConfiguration? provider=null;
        if(selected.Source==ReconciliationSource.Provider)provider=await store.Provider(CuitListImporter.Normalize(cuit));
        Profile=selected;Provider=provider;
        // Fecha: 06/10/2026 - TAREA: 00067 - Linea: 1 - Reutiliza la ubicación ARCA vigente en el escritorio.
        ArcaDirectory=string.IsNullOrWhiteSpace(selected.Profile.CarpetaCsvArca)?folderOptions?.Directory??"":selected.Profile.CarpetaCsvArca;
    }
    // Fecha: 06/10/2026 - TAREA: 00067 - Linea: 2 - No propone el mes actual cuando aún no se conoce el archivo del sistema.
    public void Clear(){Profile=null;Provider=null;ExternalFiles.Clear();SystemFile=null;External=null;System=null;Result=null;PendingConfirmed=false;providerFormatPending=false;systemFormatPending=false;externalFromFolder=false;ArcaDirectory="";From=null;To=null;}
    // Fecha: 06/10/2026 - TAREA: 00067 - Linea: 1 - Replica TopDirectoryOnly del escritorio y publica el lote sólo después de leer toda la carpeta.
    public async Task LoadArcaFolder(string directory){
        await access.Require("ArcaOffline");BeginExternalUpload();
        if(Profile==null||Provider!=null)throw new ImportException("Seleccioná un perfil de ARCA para leer una carpeta.");
        try{
            if(string.IsNullOrWhiteSpace(directory)||!Directory.Exists(directory))throw new ImportException("La carpeta ARCA no existe o no está accesible desde el servidor web.");
            var files=await Task.Run(()=>Directory.GetFiles(directory,"*.csv",SearchOption.TopDirectoryOnly).Order(StringComparer.OrdinalIgnoreCase).ToArray());
            if(files.Length==0)throw new ImportException("La carpeta ARCA no contiene archivos CSV.");
            var documents=new List<UploadedDocument>();
            foreach(var path in files){
                if(new FileInfo(path).Length>TabularReader.MaxBytes)throw new ImportException($"El archivo {Path.GetFileName(path)} supera 20 MB.");
                documents.Add(new(Path.GetFileName(path),await File.ReadAllBytesAsync(path)));
            }
            ArcaDirectory=directory;ExternalFiles=documents;externalFromFolder=true;Refresh();
        }catch(Exception e) when(e is IOException or UnauthorizedAccessException){BeginExternalUpload();throw new ImportException("No se pudo leer toda la carpeta ARCA. Revisá su acceso y volvé a cargarla.");}
    }
    public async Task UploadExternal(List<UploadedDocument> files){await access.Require("ArcaOffline");Invalidate();External=null;ExternalFiles.Clear();
        if(Profile==null)throw new ImportException("Seleccioná un perfil.");
        if(files.Count==0||files.Count>10||files.Sum(f=>(long)f.Content.Length)>TabularReader.MaxBytes)throw new ImportException("Máximo 10 archivos y 20 MB en total.");
        if(Provider!=null&&files.Count!=1)throw new ImportException("El proveedor se importa desde un archivo por conciliación.");
        ExternalFiles=files;
        // Fecha: 30/10/2026 - TAREA: 00150 - Linea: 1 - Un listado SAP precarga el formato del proveedor y lo guarda: no se pisa si ya está aplicado, para respetar ajustes posteriores.
        if(Provider!=null&&SapListReader.IsSapList(files[0].Name)&&!(Provider.Format.NumberOrder==NumberOrder.PointLetterNumber&&Provider.Format.NumberMode==NumberMode.Fixed&&Provider.Format.Number=="Referencia")){
            SapListReader.Apply(files[0],Provider.Format);await SaveProviderFormat();Saved="Se aplicó el formato del listado SAP";
        } else Refresh();
    }
    public async Task UploadSystem(UploadedDocument file){await access.Require("ArcaOffline");Invalidate();System=null;SystemFile=null;
        // Fecha: 06/10/2026 - TAREA: 00067 - Linea: 2 - El rango se calcula al cambiar el archivo, sin pisar ajustes manuales al recargar ARCA.
        BeginSystemUpload();if(file.Content.Length>TabularReader.MaxBytes)throw new ImportException("Máximo 20 MB.");SystemFile=file;Refresh();SetSystemPeriod();}
    public async Task SaveProviderFormat(){providerFormatPending=true;Invalidate();External=null;await access.Require("ArcaOffline");if(Provider==null)return;
        var current=await store.Provider(Provider.Cuit);if(current.Version!=Provider.Version)throw new ConfigurationConflictException();
        Provider.Types=current.Types;Provider=await store.Save(Provider);providerFormatPending=false;Refresh();Saved="Formato guardado automáticamente";}
    public async Task MapType(string raw,string target){await access.Require("ArcaEquivalencias");EnsureSaved();Invalidate();External=null;if(Provider==null)return;
        var copy=global::System.Text.Json.JsonSerializer.Deserialize<ProviderConfiguration>(global::System.Text.Json.JsonSerializer.Serialize(Provider))!;
        copy.SetMapping(raw,target);Provider=await store.Save(copy);Refresh();Saved="Equivalencia guardada automáticamente";}
    // Fecha: 06/10/2026 - TAREA: 00067 - Linea: 2 - Una corrección de formato vuelve a derivar fechas del archivo interpretado correctamente.
    public async Task SaveSystemFormat(){systemFormatPending=true;Invalidate();System=null;From=null;To=null;await access.Require("ArcaPerfiles");if(Profile==null)return;Profile=await store.Save(Profile);systemFormatPending=false;Refresh();SetSystemPeriod();Saved="Formato del sistema guardado automáticamente";}
    void SetSystemPeriod(){From=null;To=null;if(System?.Ready!=true)return;var dates=System.Included.Select(x=>x.Fecha.Date);From=dates.Min();To=dates.Max();}
    public void Refresh(){
        Invalidate();
        if(Profile==null)return;
        // Fecha: 05/10/2026 - TAREA: 00065 - Linea: 1 - Aplica el perfil compartido y sus exclusiones sin reinterpretar las columnas del sistema.
        if(SystemFile!=null&&!systemFormatPending)System=SystemFileImporter.Preview(SystemFile,Profile.SystemFormat,Profile.ExcludedSystemTypes);
        if(ExternalFiles.Count>0&&!providerFormatPending){
            External=new();
            // Fecha: 06/10/2026 - TAREA: 00067 - Linea: 1 - Cada aviso identifica su CSV para revisar compatibilidad sin perder los datos originales.
            foreach(var file in ExternalFiles){var p=Provider==null?ReconciliationRunner.Arca(file):ProviderImporter.Preview(file,Provider);External.Rows.AddRange(p.Rows);External.Errors.AddRange(p.Errors.Select(x=>file.Name+": "+x));External.Warnings.AddRange(p.Warnings.Select(x=>file.Name+": "+x));External.UnknownTypes.AddRange(p.UnknownTypes);}
            // Fecha: 06/10/2026 - TAREA: 00067 - Linea: 1 - El histórico de carpeta puede superar el límite de un lote subido; cada CSV conserva su validación.
            if(!externalFromFolder&&External.Rows.Count>TabularReader.MaxRows)External.Errors.Add("El conjunto supera 50.000 filas.");External.UnknownTypes=External.UnknownTypes.Distinct().ToList();
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
