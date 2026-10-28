// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 3 - Lee usuarios sin modificar la base ni el login estático del escritorio.
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Security.Claims;
namespace Conciliador.Web.Data;
// Fecha: 28/10/2026 - TAREA: 00140 - Linea: 2,3 - Resultado de entrar con una cuenta de Microsoft a través de AppHub: User nulo = usuario desactivado.
public sealed record IngresoMicrosoft(WebUser? User,bool Creado);
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
    // Fecha: 28/10/2026 - TAREA: 00140 - Linea: 2,3 - La web deja de ser sólo lectura, con lo mínimo: crear usuarios de Microsoft (sin permisos, sin contraseña utilizable) y guardar su id de Microsoft. No toca contraseñas ni roles de usuarios que ya existen (decisión de la 00140 sobre la 00064).
    bool columns;
    static async Task<SqliteConnection> OpenWrite(string path){var c=new SqliteConnection(new SqliteConnectionStringBuilder{DataSource=path,Mode=SqliteOpenMode.ReadWrite}.ToString());await c.OpenAsync();return c;}
    async Task EnsureColumns(SqliteConnection c)
    {
        if(columns)return;
        foreach(var col in new[]{"Correo","OidMicrosoft"})
        {
            using var q=c.CreateCommand();q.CommandText="SELECT COUNT(*) FROM pragma_table_info('Usuarios') WHERE name=@n";q.Parameters.AddWithValue("@n",col);
            if(Convert.ToInt64(await q.ExecuteScalarAsync())==0){using var a=c.CreateCommand();a.CommandText=$"ALTER TABLE Usuarios ADD COLUMN {col} TEXT NULL";try{await a.ExecuteNonQueryAsync();}catch(SqliteException){/* otro proceso la agregó primero */}}
        }
        using var i=c.CreateCommand();i.CommandText="CREATE UNIQUE INDEX IF NOT EXISTS UX_Usuarios_Oid ON Usuarios(OidMicrosoft) WHERE OidMicrosoft IS NOT NULL;CREATE UNIQUE INDEX IF NOT EXISTS UX_Usuarios_Correo ON Usuarios(Correo COLLATE NOCASE) WHERE Correo IS NOT NULL AND Correo <> ''";await i.ExecuteNonQueryAsync();
        columns=true;
    }
    static WebUser Row(SqliteDataReader r){List<string> p;try{p=JsonSerializer.Deserialize<List<string>>(r.GetString(4))??new();}catch(JsonException){p=new();}return new(r.GetString(1),r.GetString(2),r.GetString(3),p);}
    /// <summary>Busca por id de Microsoft y, si no, por correo (sólo si ese usuario todavía no tiene id). Si no existe lo crea: administrador si está en la lista inicial, si no usuario sin permisos. Nunca vincula por parecido de nombre.</summary>
    public async Task<IngresoMicrosoft> IngresarMicrosoftAsync(string? oid,string correo,string nombre,bool adminInicial)
    {
        if(string.IsNullOrWhiteSpace(path)||!File.Exists(path))throw new InvalidOperationException("No hay base de usuarios.");
        await using var c=await OpenWrite(path);await EnsureColumns(c);
        for(var intento=0;;intento++)
        {
            await using var tx=(SqliteTransaction)await c.BeginTransactionAsync();
            long id=0;var activo=false;var oidActual="";WebUser? user=null;
            const string cols="SELECT Id,Username,Nombre,Rol,PermisosJson,Activo,COALESCE(OidMicrosoft,'') FROM Usuarios ";
            async Task<bool> Buscar(string where,string param,string valor)
            {
                using var q=c.CreateCommand();q.Transaction=tx;q.CommandText=cols+where;q.Parameters.AddWithValue(param,valor);
                await using var rd=await q.ExecuteReaderAsync();if(!await rd.ReadAsync())return false;
                id=rd.GetInt64(0);activo=rd.GetInt64(5)==1;oidActual=rd.GetString(6);user=Row(rd);return true;
            }
            var hallado=!string.IsNullOrWhiteSpace(oid)&&await Buscar("WHERE OidMicrosoft=@o","@o",oid!);
            if(!hallado)hallado=await Buscar("WHERE Correo=@c COLLATE NOCASE AND (OidMicrosoft IS NULL OR OidMicrosoft='')","@c",correo);
            if(hallado)
            {
                if(!activo)return new(null,false);
                if(oidActual.Length==0&&!string.IsNullOrWhiteSpace(oid)){using var u=c.CreateCommand();u.Transaction=tx;u.CommandText="UPDATE Usuarios SET OidMicrosoft=@o WHERE Id=@i";u.Parameters.AddWithValue("@o",oid);u.Parameters.AddWithValue("@i",id);await u.ExecuteNonQueryAsync();}
                await tx.CommitAsync();return new(user,false);
            }
            var baseName=System.Text.RegularExpressions.Regex.Replace(correo[..correo.IndexOf('@')].ToLowerInvariant(),"[^a-z0-9._-]","");if(baseName.Length==0)baseName="usuario";
            var username=baseName;
            for(var n=2;;n++){using var e=c.CreateCommand();e.Transaction=tx;e.CommandText="SELECT COUNT(*) FROM Usuarios WHERE Username=@u";e.Parameters.AddWithValue("@u",username);if(Convert.ToInt64(await e.ExecuteScalarAsync())==0)break;username=baseName+n;}
            var rol=adminInicial?"Admin":"Usuario";var display=string.IsNullOrWhiteSpace(nombre)?correo:nombre;
            try
            {
                using var ins=c.CreateCommand();ins.Transaction=tx;
                ins.CommandText="INSERT INTO Usuarios(Username,PasswordHash,Nombre,Activo,Rol,PermisosJson,Correo,OidMicrosoft) VALUES(@u,'!',@n,1,@r,'[]',@c,@o)";
                ins.Parameters.AddWithValue("@u",username);ins.Parameters.AddWithValue("@n",display);ins.Parameters.AddWithValue("@r",rol);ins.Parameters.AddWithValue("@c",correo);ins.Parameters.AddWithValue("@o",string.IsNullOrWhiteSpace(oid)?DBNull.Value:oid);
                await ins.ExecuteNonQueryAsync();await tx.CommitAsync();
            }
            catch(SqliteException e)when(e.SqliteErrorCode==19)
            {
                await tx.RollbackAsync();
                // Otro pedido creó al mismo usuario justo ahora: se reintenta y lo encuentra. Si el correo ya es de otra cuenta de Microsoft, no se vincula.
                if(intento==0)continue;throw new InvalidOperationException("El correo ya está asociado a otra cuenta.");
            }
            return new(new WebUser(username,display,rol,new()),true);
        }
    }
}
