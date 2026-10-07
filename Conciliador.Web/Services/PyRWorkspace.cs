// Fecha: 06/10/2026 - TAREA: 00072 - Linea: 3,4 - Cada circuito conserva sus fuentes y rechaza resultados de un perfil modificado.
using ArcaCliente.Models;
using ArcaCliente.Services;
using ArcaCliente.Web.Data;
using ArcaCliente.Web.Models;
using ArcaCliente.Web.Services;
using Conciliador.Web.Security;
namespace Conciliador.Web.Services;
public sealed class PyRWorkspace(IPyRProfileStore store,AccessGuard access)
{
    public WebPyRProfile? Profile {get;private set;}
    public CargaArcaPyR? Arca {get;private set;}
    public List<MayorPreseaPyR> Majors {get;}=new();
    public ResultadoConciliacionPyR? Result {get;private set;}
    public string DirectoryPath {get;private set;}="";
    long generation;
    public void Invalidate(){Result=null;generation++;}
    public void Clear(){Invalidate();Profile=null;Arca=null;Majors.Clear();DirectoryPath="";}
    public void ChangeDirectory(string value){Invalidate();Arca=null;DirectoryPath=value;}
    public void BeginArca(){Invalidate();Arca=null;}
    public void RemoveMajor(Guid account){Invalidate();Majors.RemoveAll(m=>m.Cuenta.Id==account);}
    public async Task Select(Guid id)
    {
        await access.Require("ArcaPyR");Clear();
        Profile=(await store.Profiles()).SingleOrDefault(p=>p.Profile.Id==id)?.Copy()??throw new ImportException("El perfil ya no existe.");
        DirectoryPath=Profile.Profile.CarpetaArca??"";
    }
    public async Task LoadFolder()
    {
        await access.Require("ArcaPyR");BeginArca();await CheckVersion();var stamp=generation;var directory=DirectoryPath;
        if(string.IsNullOrWhiteSpace(directory)||!Directory.Exists(directory))throw new ImportException("La carpeta ARCA no está accesible desde el servidor web.");
        var read=await Task.Run(()=>ArcaPyRImporter.ImportarDesdeCarpeta(directory));
        if(stamp!=generation)return;
        // Fecha: 06/10/2026 - TAREA: 00072 - Linea: 3 - WinForms recuerda la última carpeta sólo después de una lectura correcta.
        if(Profile!.Profile.CarpetaArca!=directory){var edited=Profile.Copy();edited.Profile.CarpetaArca=directory;var saved=await store.Save(edited);if(stamp!=generation)return;Profile=saved;}
        else await CheckVersion();
        Arca=read;
    }
    public async Task LoadMajor(UploadedDocument file,Guid accountId)
    {
        await access.Require("ArcaPyR");RemoveMajor(accountId);await CheckVersion();
        var p=Profile!.Copy().Profile;var account=p.Cuentas.SingleOrDefault(c=>c.Id==accountId)??throw new ImportException("Elegí una cuenta del perfil.");
        var stamp=generation;var major=await Task.Run(()=>PyRFileAdapter.ImportMajor(file,p,account));
        if(stamp==generation)Majors.Add(major);
    }
    public async Task SaveDirectives(List<DirectivaPyR> directives)
    {
        await access.Require("ArcaPyR");Invalidate();await CheckVersion();
        var edited=Profile!.Copy();edited.Profile.DirectivasConciliacion=directives.Select(d=>d.Clonar()).ToList();
        Profile=await store.Save(edited);
    }
    public async Task Reconcile()
    {
        await access.Require("ArcaPyR");Invalidate();await CheckVersion();
        if(Arca?.Registros.Count is not >0 || Majors.Count==0)throw new ImportException("Cargá ARCA y al menos un mayor para conciliar.");
        PyRProfileValidator.ValidateDirectives(Profile!.Profile.DirectivasConciliacion);
        var stamp=generation;var arca=Arca.Registros.ToList();var majors=Majors.ToList();var directives=Profile.Profile.DirectivasConciliacion.Select(d=>d.Clonar()).ToList();
        var result=await Task.Run(()=>ConciliacionPyRService.Conciliar(arca,majors,directives));
        if(stamp==generation)Result=result;
    }
    public async Task<byte[]> Export()
    {
        await access.Require("ArcaPyR");await CheckVersion();
        if(Result==null)throw new ImportException("Volvé a conciliar antes de exportar.");
        return PyRFileAdapter.Export(Result,Profile!.Profile.Nombre);
    }
    async Task CheckVersion()
    {
        if(Profile==null)throw new ImportException("Elegí un perfil PyR.");
        var current=(await store.Profiles()).SingleOrDefault(p=>p.Profile.Id==Profile.Profile.Id);
        if(current?.Version!=Profile.Version){Invalidate();throw new ConfigurationConflictException();}
    }
    public async Task Example()
    {
        await access.Require("ArcaPyR");Invalidate();Arca=null;Majors.Clear();await CheckVersion();
        var account=Profile!.Profile.Cuentas.FirstOrDefault(c=>c.Tipo==TipoOperacionPyR.Percepcion&&c.Impuesto==ImpuestoPyR.Iva)
            ??throw new ImportException("El ejemplo necesita una cuenta de Percepciones IVA.");
        Arca=PyRExamples.Arca();Majors.Add(PyRFileAdapter.ImportMajor(PyRExamples.Major(),Profile.Profile,account));
    }
}
