# Ruta completa del proyecto
$projectPath = "C:\Users\x\source\repos\Paquete NuGet\HelpersNuGET\HelpersNuGET.csproj"

# Configuración
$configuration = "Release"

# Empaquetar el proyecto
Write-Host "Empaquetando el proyecto..."
dotnet pack $projectPath --configuration $configuration --no-build

# Ruta del paquete generado
$packageFolder = "C:\Users\x\source\repos\Paquete NuGet\HelpersNuGET\bin\$configuration"
$nupkgPath = Get-ChildItem $packageFolder -Filter *.nupkg | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if (-not $nupkgPath) {
    Write-Host " No se encontró ningún archivo .nupkg en $packageFolder"
    exit 1
}

# Tu API Key (reemplaza con la tuya)
$apiKey = ""

# Publicar el paquete
Write-Host "Publicando $($nupkgPath.Name)..."
dotnet nuget push $nupkgPath.FullName --api-key $apiKey --source https://api.nuget.org/v3/index.json --skip-duplicate

Write-Host "Publicación completada."
