// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 1 - La web exige conexión explícita y no usa los valores productivos por defecto.
using Conciliador.Web.Components;
using Conciliador.Web.Data;
using Conciliador.Web.Security;
using Conciliador.Web.Services;
using ArcaCliente.Web.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.RateLimiting;
using System.Globalization;
var builder=WebApplication.CreateBuilder(args);
CultureInfo.DefaultThreadCurrentCulture=CultureInfo.GetCultureInfo("es-AR");CultureInfo.DefaultThreadCurrentUICulture=CultureInfo.GetCultureInfo("es-AR");
bool demo=builder.Configuration.GetValue<bool>("Web:Demo");
if(demo&&!builder.Environment.IsDevelopment())throw new InvalidOperationException("La demostración requiere Development y acceso local.");
builder.Services.AddSingleton(new WebMode(demo));builder.Services.AddSingleton<SessionRegistry>();
builder.Services.AddSingleton(new UserStorage(builder.Configuration["Web:UsersDatabasePath"]??""));
if(demo)builder.Services.AddSingleton<IWebConfigurationStore,DemoConfigurationStorage>();
else {
    string connection=builder.Configuration.GetConnectionString("Arca")??throw new InvalidOperationException("Configurá ConnectionStrings:Arca o activá Web:Demo en Development.");
    if(string.IsNullOrWhiteSpace(builder.Configuration["Web:UsersDatabasePath"]))throw new InvalidOperationException("Configurá Web:UsersDatabasePath.");
    builder.Services.AddSingleton<IWebConfigurationStore>(new SqlConfigurationStorage(connection));
}
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o=>{
    o.LoginPath="/account/login";o.AccessDeniedPath="/account/login";o.Cookie.Name="Conciliador.Web.Auth";o.Cookie.HttpOnly=true;o.Cookie.SameSite=SameSiteMode.Strict;o.Cookie.SecurePolicy=demo?CookieSecurePolicy.SameAsRequest:CookieSecurePolicy.Always;o.ExpireTimeSpan=TimeSpan.FromHours(8);o.SlidingExpiration=false;
    o.Events.OnValidatePrincipal=async context=>{if(!context.HttpContext.RequestServices.GetRequiredService<SessionRegistry>().IsActive(context.Principal!) || !demo && await context.HttpContext.RequestServices.GetRequiredService<UserStorage>().Find(context.Principal?.Identity?.Name??"")==null)context.RejectPrincipal();};
});
builder.Services.AddAuthorization(o=>{
    foreach(var permission in new[]{"ArcaOffline","ArcaPerfiles","ArcaEquivalencias"})o.AddPolicy(permission,p=>p.RequireAuthenticatedUser().RequireAssertion(c=>c.User.IsInRole("Admin")||c.User.HasClaim("permission",permission)));
});
builder.Services.AddCascadingAuthenticationState();builder.Services.AddScoped<AccessGuard>();builder.Services.AddScoped<Workspace>();
builder.Services.AddRazorComponents().AddInteractiveServerComponents(o=>{o.DisconnectedCircuitRetentionPeriod=TimeSpan.FromMinutes(2);});
builder.Services.AddRazorPages();
builder.Services.AddRateLimiter(o=>{o.RejectionStatusCode=429;o.AddPolicy("login",context=>RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString()??"unknown",_=>new FixedWindowRateLimiterOptions{PermitLimit=10,Window=TimeSpan.FromMinutes(1),QueueLimit=0}));});
var app=builder.Build();
if(!demo){app.UseExceptionHandler("/Error");app.UseHsts();app.UseHttpsRedirection();await ((SqlConfigurationStorage)app.Services.GetRequiredService<IWebConfigurationStore>()).Initialize();}
if(demo)app.Use(async(ctx,next)=>{if(ctx.Connection.RemoteIpAddress==null||!System.Net.IPAddress.IsLoopback(ctx.Connection.RemoteIpAddress)){ctx.Response.StatusCode=403;return;}await next();});
app.Use(async(ctx,next)=>{ctx.Response.Headers["X-Content-Type-Options"]="nosniff";ctx.Response.Headers["X-Frame-Options"]="DENY";await next();});
app.UseAuthentication();app.UseAuthorization();app.UseRateLimiter();app.UseAntiforgery();
app.MapStaticAssets();app.MapRazorPages().RequireRateLimiting("login");app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();

