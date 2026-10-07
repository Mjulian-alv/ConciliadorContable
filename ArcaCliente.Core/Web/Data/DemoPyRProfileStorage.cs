// Fecha: 06/10/2026 - TAREA: 00072 - Linea: 2 - La demostración no consulta ni modifica los perfiles SQL.
using ArcaCliente.Models;
using ArcaCliente.Web.Models;
namespace ArcaCliente.Web.Data;
public sealed class DemoPyRProfileStorage : IPyRProfileStore
{
    readonly object gate = new();
    readonly List<WebPyRProfile> profiles = new() { new() { Version = Guid.NewGuid().ToString(), Profile = new() {
        Nombre="Percepciones y retenciones · ejemplo", ColAsiento="", Cuentas=new() {
            new() {Codigo="114105",Nombre="Percepciones IVA"},
            new() {Codigo="114106",Nombre="Percepciones IIBB",Impuesto=ImpuestoPyR.Iibb} } } } };
    public Task<List<WebPyRProfile>> Profiles() { lock(gate) return Task.FromResult(profiles.Select(p=>p.Copy()).ToList()); }
    public Task<WebPyRProfile> Save(WebPyRProfile value)
    {
        PyRProfileValidator.Validate(value);
        lock(gate) {
            var old=profiles.SingleOrDefault(p=>p.Profile.Id==value.Profile.Id);
            if ((old?.Version??"")!=value.Version) throw new ConfigurationConflictException();
            var saved=value.Copy();saved.Version=Guid.NewGuid().ToString();
            if(old!=null)profiles.Remove(old);profiles.Add(saved);return Task.FromResult(saved.Copy());
        }
    }
    public Task Delete(WebPyRProfile value) { lock(gate) {
        var old=profiles.SingleOrDefault(p=>p.Profile.Id==value.Profile.Id);
        if(old==null || old.Version!=value.Version)throw new ConfigurationConflictException();
        profiles.Remove(old);return Task.CompletedTask;
    } }
}
