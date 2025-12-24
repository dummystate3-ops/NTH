
param(
    [switch]$Full = $false
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "   NovaToolsHub Release Builder" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# 1. Clean previous build
$publishDir = ".\publish_self_contained"
$zipFile = ".\deploy.zip"

if (Test-Path $publishDir) {
    Write-Host "Cleaning previous build..."
    Remove-Item $publishDir -Recurse -Force
}
if (Test-Path $zipFile) {
    Remove-Item $zipFile -Force
}

# 2. Publish Self-Contained
Write-Host "Building project (Self-Contained)..." -ForegroundColor Yellow
dotnet publish -c Release -r win-x64 --self-contained true -o $publishDir

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build Failed!"
    exit
}

# 3. Handle App_Data (Exclude by default)
if (-not $Full) {
    Write-Host "Excluding App_Data from deployment (Code-Only Update)..." -ForegroundColor Yellow
    # We remove it from the publish output before zipping so it doesn't get into the zip
    if (Test-Path "$publishDir\App_Data") {
        Remove-Item "$publishDir\App_Data" -Recurse -Force
    }
}
else {
    Write-Host "Including App_Data (Full Update)..." -ForegroundColor Magenta
    # Copy App_Data from source to publish directory if it exists
    if (Test-Path ".\App_Data") {
        Write-Host "Copying App_Data folder to publish directory..." -ForegroundColor Yellow
        Copy-Item -Path ".\App_Data" -Destination "$publishDir\App_Data" -Recurse -Force
    }
}

# 4. Create Zip
Write-Host "Creating $zipFile..." -ForegroundColor Yellow
Compress-Archive -Path "$publishDir\*" -DestinationPath $zipFile -Force

$zipSize = (Get-Item $zipFile).Length / 1MB
Write-Host "==========================================" -ForegroundColor Green
Write-Host "   SUCCESS!" -ForegroundColor Green
Write-Host "   Zip Created: $zipFile" -ForegroundColor Green
Write-Host "   Size: $("1:N2" -f $zipSize) MB" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green

if ($zipSize -gt 100 -and -not $Full) {
    Write-Warning "The zip file is unexpectedly large. Did you forget to exclude something?"
}
