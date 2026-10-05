# Fecha: 05/10/2026 - TAREA: 00065 - Linea: 2 - Reutiliza la configuración del escritorio sin copiar secretos al repositorio ni a la línea de comandos.
param([Parameter(Mandatory=$true)][string]$DesktopDirectory)
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
$names=@('ConnectionStrings__Arca','Web__UsersDatabasePath','Web__Demo','ASPNETCORE_ENVIRONMENT','ASPNETCORE_URLS')
$previous=@{};foreach($name in $names){$previous[$name]=[Environment]::GetEnvironmentVariable($name,'Process')}
try{
    $env:ConnectionStrings__Arca=$connection
    $env:Web__UsersDatabasePath=$usersPath
    $env:Web__Demo='false'
    $env:ASPNETCORE_ENVIRONMENT='Production'
    $env:ASPNETCORE_URLS='https://localhost:5085;http://localhost:5084'
    Write-Host 'ArcaCliente web con configuración del escritorio: https://localhost:5085'
    & dotnet run --project (Join-Path $PSScriptRoot '../Conciliador.Web') --no-build --no-launch-profile
    if($LASTEXITCODE -ne 0){throw 'El servidor web no pudo iniciar. Revisá el registro de arranque.'}
}finally{foreach($name in $names){[Environment]::SetEnvironmentVariable($name,$previous[$name],'Process')}}
