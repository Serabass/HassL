# Build script for HassLanguage project using Docker

Write-Host "Building HassLanguage project..." -ForegroundColor Green

# Check if Docker is available
$dockerAvailable = Get-Command docker -ErrorAction SilentlyContinue
if (-not $dockerAvailable) {
    Write-Host "Docker is not available. Please install Docker Desktop." -ForegroundColor Red
    exit 1
}

# Build using Docker
docker run --rm -v "${PWD}:/workspace" -w /workspace mcr.microsoft.com/dotnet/sdk:8.0 `
    dotnet restore HassLanguage.sln

docker run --rm -v "${PWD}:/workspace" -w /workspace mcr.microsoft.com/dotnet/sdk:8.0 `
    dotnet build HassLanguage.sln --no-restore -c Release

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build completed successfully!" -ForegroundColor Green
} else {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}
