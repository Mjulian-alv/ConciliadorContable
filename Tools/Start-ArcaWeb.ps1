# Fecha: 05/10/2026 - TAREA: 00065 - Linea: 2 - Reutiliza la configuración del escritorio sin copiar secretos al repositorio ni a la línea de comandos.
# Fecha: 06/10/2026 - TAREA: 00067 - Linea: 1 - La ruta compartida es el respaldo cuando el perfil no define carpeta ARCA.
param([Parameter(Mandatory=$true)][string]$DesktopDirectory,[string]$ArcaDirectory='Y:\ArcaCliente\ARCHIVOSARCA')
$ErrorActionPreference='Stop'
$desktopRoot=(Resolve-Path -LiteralPath $DesktopDirectory).Path
$appConfigPath=Join-Path $desktopRoot 'ConciliadorContable.dll.config'
[xml]$desktopConfig=Get-Content -LiteralPath $appConfigPath -Raw
$connection=($desktopConfig.configuration.appSettings.add | Where-Object key -eq 'ConciliadorSqlConnectionString').value
$externalFile=$desktopConfig.configuration.appSettings.file
if($externalFile){
    [xml]$externalConfig=Get-Content -LiteralPath (Join-Path $desktopRoot $externalFile) -Raw
    $externalConnection=($externalConfig.appSettings.add | Where-Object key -eq 'ConciliadorSqlConnectionString').value
    if($externalConnection){$connection=$externalConnection}
}
if([string]::IsNullOrWhiteSpace($connection)){throw 'No se encontró la conexión SQL configurada del escritorio.'}
$usersPath=Join-Path $desktopRoot 'conciliador.db'
if(!(Test-Path -LiteralPath $usersPath -PathType Leaf)){throw 'No se encontró la base de usuarios del escritorio.'}
# Fecha: 06/10/2026 - TAREA: 00067 - Linea: 1 - Restaura también la ubicación ARCA al terminar el lanzador.
$names=@('ConnectionStrings__Arca','Web__UsersDatabasePath','Web__Demo','Web__ArcaDirectory','ASPNETCORE_ENVIRONMENT','ASPNETCORE_URLS')
$previous=@{};foreach($name in $names){$previous[$name]=[Environment]::GetEnvironmentVariable($name,'Process')}
try{
    $env:ConnectionStrings__Arca=$connection
    $env:Web__UsersDatabasePath=$usersPath
    $env:Web__Demo='false'
    # Fecha: 06/10/2026 - TAREA: 00067 - Linea: 1 - El navegador no necesita subir uno por uno los CSV del histórico compartido.
    $env:Web__ArcaDirectory=$ArcaDirectory
    $env:ASPNETCORE_ENVIRONMENT='Production'
    $env:ASPNETCORE_URLS='https://localhost:5085;http://localhost:5084'
    # Fecha: 05/10/2026 - TAREA: 00065 - Linea: 2 - Production necesita la salida publicada: dotnet run dejaba los recursos Blazor y CSS fuera del directorio servido.
    $webProject=Join-Path $PSScriptRoot '../Conciliador.Web'
    $publishRoot=[IO.Path]::GetFullPath((Join-Path $webProject 'bin/LocalHost'))
    & dotnet publish $webProject --configuration Release --no-restore --output $publishRoot --verbosity quiet
    if($LASTEXITCODE -ne 0){throw 'No se pudo preparar la aplicación web y sus recursos.'}
    Write-Host 'ArcaCliente web con configuración del escritorio: https://localhost:5085'
    & dotnet (Join-Path $publishRoot 'Conciliador.Web.dll') --contentRoot $publishRoot
    if($LASTEXITCODE -ne 0){throw 'El servidor web no pudo iniciar. Revisá el registro de arranque.'}
}finally{foreach($name in $names){[Environment]::SetEnvironmentVariable($name,$previous[$name],'Process')}}
