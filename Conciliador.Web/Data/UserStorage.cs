// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 3 - Lee usuarios sin modificar la base ni el login estático del escritorio.
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Security.Claims;
namespace Conciliador.Web.Data;
public sealed record WebUser(string Username,string Name,string Role,List<string> Permissions)
{
    public bool Allows(string permission)=>Role=="Admin"||Permissions.Contains(permission);
    public ClaimsPrincipal Principal(bool demo=false)=>new(new ClaimsIdentity(new[]{new Claim(ClaimTypes.Name,Username),new Claim("display_name",Name),new Claim(ClaimTypes.Role,Role),new Claim("demo",demo.ToString()),new Claim("session",Guid.NewGuid().ToString()),new Claim("expires",DateTimeOffset.UtcNow.AddHours(8).ToUnixTimeSeconds().ToString())}.Concat(Permissions.Select(x=>new Claim("permission",x))),"Cookies"));
}
public sealed class UserStorage(string path)
{
    async Task<(WebUser? User,string Hash)> Read(string name) {
        if(string.IsNullOrWhiteSpace(path)||!File.Exists(path))return(null,"");
        await using var c=new SqliteConnection(new SqliteConnectionStringBuilder{DataSource=path,Mode=SqliteOpenMode.ReadOnly}.ToString());await c.OpenAsync();
        using var cmd=c.CreateCommand();cmd.CommandText="SELECT Username,Nombre,Rol,PermisosJson,PasswordHash FROM Usuarios WHERE Username=@Name AND Activo=1";cmd.Parameters.AddWithValue("@Name",name);
        using var r=await cmd.ExecuteReaderAsync();if(!await r.ReadAsync())return(null,"");
        List<string> permissions;try{permissions=JsonSerializer.Deserialize<List<string>>(r.GetString(3))??new();}catch(JsonException){permissions=new();}
        return(new(r.GetString(0),r.GetString(1),r.GetString(2),permissions),r.GetString(4));
    }
    public async Task<WebUser?> Find(string name)=>(await Read(name)).User;
    public async Task<WebUser?> ValidateAsync(string name,string password) {
        var row=await Read(name);var hash=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password))).ToLowerInvariant();
        return row.User!=null&&CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(hash),Encoding.ASCII.GetBytes(row.Hash.ToLowerInvariant()))?row.User:null;
    }
}

