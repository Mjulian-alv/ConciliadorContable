// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 3 - Regresiones de la revisión independiente y compatibilidad con fechas actuales.
using ArcaCliente.Web.Data;
using ArcaCliente.Web.Models;
using ArcaCliente.Web.Services;
using Conciliador.Web.Services;
using Conciliador.Web.Data;
using Conciliador.Web.Security;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text;
static class ReviewRegressionTests
{
    static void Check(bool condition){if(!condition)throw new Exception("Falló regresión de revisión");}
    sealed class Auth : AuthenticationStateProvider { public System.Security.Claims.ClaimsPrincipal Principal=new WebUser("demo","Demo","Admin",new()).Principal(true);public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(Principal)); }
    static Workspace Workspace(IWebConfigurationStore store){var auth=new Auth();var registry=new SessionRegistry();registry.Register(auth.Principal);return new(store,new AccessGuard(auth,new UserStorage(""),new WebMode(true),registry));}
    sealed class FailingStore : IWebConfigurationStore {
        public DemoConfigurationStorage Inner=new();public bool Fail;
        public Task<List<WebProfile>> Profiles()=>Inner.Profiles();public Task<ProviderConfiguration> Provider(string c)=>Inner.Provider(c);
        public Task<WebProfile> Save(WebProfile p)=>Fail?throw new IOException("Error simulado"):Inner.Save(p);
        public Task<ProviderConfiguration> Save(ProviderConfiguration p)=>Fail?throw new IOException("Error simulado"):Inner.Save(p);
    }
    public static void Run(Action<string,Action> test){
        test("Tiques FC, NC y ND conservan familia fiscal y signo",()=>{
            foreach(var code in new[]{"081","082","111","112","113","114","115","116","117"}) {
                var file=new UploadedDocument("t.csv",Encoding.UTF8.GetBytes("Tipo;Fecha;PuntoVenta;Numero;Importe;CUIT\n"+code+";05/09/2026;5;1234;100,00;30123456781"));
                var p=ProviderImporter.Preview(file,new FileFormat());Check(p.Ready&&p.ExcludedCount==0&&p.Included.Count==1);
                Check(FiscalCatalog.Sign(code)==(new[]{"112","113","114"}.Contains(code)?-1:1));
            }
        });
        test("FC ambiguo no sugiere una letra fiscal",()=>{Check(FiscalCatalog.Suggest("FC")=="");Check(FiscalCatalog.Suggest("FC C")=="011");});
        test("Equivalencias se corrigen aunque cambien mayúsculas",()=>{
            var store=new DemoConfigurationStorage();store.Save(new ProviderConfiguration{Cuit="30123456781",Types=new(){["FA"]="001"}}).GetAwaiter().GetResult();
            var work=Workspace(store);work.Select(store.Profiles().Result.First(x=>x.Source==ReconciliationSource.Provider).Profile.Id,"30123456781").GetAwaiter().GetResult();
            work.MapType("fa","006").GetAwaiter().GetResult();var saved=store.Provider("30123456781").Result;Check(saved.Types.Count==1&&saved.Types.Values.Single()=="006");
        });
        test("ARCA admite fecha ISO actual además de fecha histórica",()=>{
            var original=Encoding.UTF8.GetString(DemoFiles.Arca.Content);
            var iso=original.Replace("05/09/2026","2026-09-05");
            Check(ReconciliationRunner.Arca(new("arca.csv",Encoding.UTF8.GetBytes(iso))).Ready);
        });
        // Fecha: 06/10/2026 - TAREA: 00067 - Linea: 1 - Los separadores vacíos sobrantes no alteran columnas fiscales ni relajan la cuenta corriente del proveedor.
        test("ARCA conserva mapeo del escritorio con columnas sobrantes y avisa",()=>{
            var lines=Encoding.UTF8.GetString(DemoFiles.Arca.Content).Split('\n');lines[0]+=";";lines[1]+=";;";
            Check(ReconciliationRunner.Arca(new("historico.csv",Encoding.UTF8.GetBytes(string.Join('\n',lines)))).Ready);
            var extra=Encoding.UTF8.GetString(DemoFiles.Arca.Content).Split('\n');extra[1]+=";valor";
            var compatible=ReconciliationRunner.Arca(new("extra.csv",Encoding.UTF8.GetBytes(string.Join('\n',extra))));
            Check(compatible.Ready&&compatible.Warnings.Count==1&&compatible.Included.First().Total==100000m&&compatible.Rows.First().RawCells.Last()=="valor");
            extra[1]="05/09/2026;001;5;1234";Check(!ReconciliationRunner.Arca(new("incompleto.csv",Encoding.UTF8.GetBytes(string.Join('\n',extra)))).Ready);
            var system=Encoding.UTF8.GetString(DemoFiles.System.Content).Split('\n');system[0]+=";";
            Check(!ProviderImporter.Preview(new("proveedor.csv",Encoding.UTF8.GetBytes(string.Join('\n',system))),new FileFormat()).Ready);
        });
        // Fecha: 06/10/2026 - TAREA: 00067 - Linea: 2 - El archivo del sistema determina el período, aunque ARCA contenga otros meses.
        test("Período inicial proviene del archivo del sistema",()=>{
            var store=new DemoConfigurationStorage();var work=Workspace(store);var id=store.Profiles().Result.First(x=>x.Source==ReconciliationSource.Arca).Profile.Id;work.Select(id).GetAwaiter().GetResult();
            Check(work.From==null&&work.To==null);
            work.UploadSystem(DemoFiles.System).GetAwaiter().GetResult();
            var dates=work.System!.Included.Select(x=>x.Fecha.Date).ToList();
            Check(work.From==dates.Min()&&work.To==dates.Max());
            work.From=new DateTime(2026,9,1);work.To=new DateTime(2026,9,30);
            work.UploadExternal(new(){DemoFiles.Arca}).GetAwaiter().GetResult();
            Check(work.From==new DateTime(2026,9,1)&&work.To==new DateTime(2026,9,30));
        });
        // Fecha: 06/10/2026 - TAREA: 00067 - Linea: 1 - Una carpeta ARCA no se limita a diez archivos ni incluye subcarpetas o archivos ajenos.
        test("Carpeta ARCA reúne todos los CSV y descarta el lote anterior al fallar",()=>{
            var store=new DemoConfigurationStorage();var work=Workspace(store);
            work.Select(store.Profiles().Result.First(x=>x.Source==ReconciliationSource.Arca).Profile.Id).GetAwaiter().GetResult();
            var method=typeof(Workspace).GetMethod("LoadArcaFolder")??throw new Exception("Falta lectura de carpeta ARCA");
            var folder=Path.Combine(Path.GetTempPath(),"arca-test-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);
            try{
                for(var i=0;i<12;i++)File.WriteAllBytes(Path.Combine(folder,$"{i:D2}.csv"),DemoFiles.Arca.Content);
                File.WriteAllText(Path.Combine(folder,"ignorar.txt"),"No es ARCA");Directory.CreateDirectory(Path.Combine(folder,"subcarpeta"));File.WriteAllBytes(Path.Combine(folder,"subcarpeta","ignorar.csv"),DemoFiles.Arca.Content);
                ((Task)method.Invoke(work,new object[]{folder})!).GetAwaiter().GetResult();
                Check(work.ExternalFiles.Count==12&&work.External!.Ready);
                Check(work.External.Rows.Count==12*ReconciliationRunner.Arca(DemoFiles.Arca).Rows.Count);
                work.UploadSystem(DemoFiles.System).GetAwaiter().GetResult();work.Reconcile().GetAwaiter().GetResult();Check(work.Result!=null);
                try{((Task)method.Invoke(work,new object[]{Path.Combine(folder,"ausente")})!).GetAwaiter().GetResult();throw new Exception("Aceptó carpeta inexistente");}catch(ImportException){}
                Check(work.External==null&&work.ExternalFiles.Count==0&&work.Result==null);
            }finally{Directory.Delete(folder,true);}
        });
        // Fecha: 06/10/2026 - TAREA: 00067 - Linea: 1-2 - Verificación opcional de los archivos compartidos, sin conexión ni escrituras SQL.
        var actualFolder=Environment.GetEnvironmentVariable("ARCA_TEST_FOLDER");var actualSystem=Environment.GetEnvironmentVariable("ARCA_TEST_SYSTEM");
        if(!string.IsNullOrWhiteSpace(actualFolder)&&!string.IsNullOrWhiteSpace(actualSystem))test("Carpeta real ARCA completa y rango real PRESEA",()=>{
            var store=new DemoConfigurationStorage();var work=Workspace(store);work.Select(store.Profiles().Result.First(x=>x.Source==ReconciliationSource.Arca).Profile.Id).GetAwaiter().GetResult();
            work.Profile!.SystemFormat=new(){Date="fecha",Amount="importe",Cuit="cuit",Type="codigoafip",Point="ptovta",Number="numero",Name="nombre",DecimalSeparator="."};work.Profile.ExcludedSystemTypes=new(){"000"};
            work.LoadArcaFolder(actualFolder).GetAwaiter().GetResult();
            Check(work.ExternalFiles.Count==Directory.GetFiles(actualFolder,"*.csv").Length);
            if(!work.External!.Ready){
                foreach(var file in work.ExternalFiles){var preview=ReconciliationRunner.Arca(file);foreach(var row in preview.Rows.Where(r=>r.Errors.Count>0).Take(3))Console.WriteLine($"Diagnóstico {file.Name[..7]} línea {row.Line}: {row.RawCells.Length} columnas; {string.Join("; ",row.Errors)}; cola vacía: {row.RawCells.LastOrDefault()==""}");}
                throw new Exception(string.Join("; ",work.External.Errors.Take(3))+" errores de filas: "+string.Join("; ",work.External.Rows.SelectMany(r=>r.Errors).Distinct()));
            }
            work.UploadSystem(new(Path.GetFileName(actualSystem),File.ReadAllBytes(actualSystem))).GetAwaiter().GetResult();
            if(!work.System!.Ready)throw new Exception("PRESEA no está listo: "+string.Join("; ",work.System.Errors));
            var systemDates=work.System.Included.Select(x=>x.Fecha.Date).ToArray();
            if(work.From!=systemDates.Min()||work.To!=systemDates.Max())throw new Exception($"Período incorrecto: {work.From:dd/MM/yyyy} a {work.To:dd/MM/yyyy}");
            work.Reconcile().GetAwaiter().GetResult();
            Check(work.Result!.ExternalOmitted>0&&work.Result.SystemOmitted==0);
            Console.WriteLine($"Carpeta real: {work.ExternalFiles.Count} CSV, {work.External.Rows.Count} filas; período {work.From:dd/MM/yyyy} a {work.To:dd/MM/yyyy}; {work.Result.ExternalOmitted} ARCA fuera del filtro.");
        });
        test("Un formato no guardado no vuelve a habilitarse al subir otro archivo",()=>{
            var store=new FailingStore();var config=new ProviderConfiguration{Cuit="30123456781",Format=new(){NumberMode=NumberMode.Separated},Types=new(){["FA"]="001",["NCA"]="003",["NDA"]="002",["REC"]="EXCLUIR",["PAGO"]="EXCLUIR"}};
            store.Save(config).GetAwaiter().GetResult();var work=Workspace(store);work.Select(store.Profiles().Result.First(x=>x.Source==ReconciliationSource.Provider).Profile.Id,config.Cuit).GetAwaiter().GetResult();
            work.UploadExternal(new(){DemoFiles.Provider}).GetAwaiter().GetResult();work.UploadSystem(DemoFiles.System).GetAwaiter().GetResult();Check(work.External!.Ready);
            work.Provider!.Format.DecimalSeparator=".";store.Fail=true;try{work.SaveProviderFormat().GetAwaiter().GetResult();throw new Exception("No falló guardado");}catch(IOException){}
            var changed=new UploadedDocument("nuevo.csv",Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(DemoFiles.Provider.Content).Replace(",00",".00")));
            work.UploadExternal(new(){changed}).GetAwaiter().GetResult();Check(work.External?.Ready!=true);
            store.Fail=false;work.SaveProviderFormat().GetAwaiter().GetResult();Check(work.External!.Ready);
        });
    }
}
