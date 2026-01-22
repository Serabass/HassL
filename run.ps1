# PowerShell script to run HassLanguage with example file

param(
    [string]$File = "examples/simple-example.hass"
)

Write-Host "Building and running HassLanguage with file: $File" -ForegroundColor Green

# Build image
docker-compose -f docker-compose.run.yml build

# Run with specified file
docker-compose -f docker-compose.run.yml run --rm hass-language dotnet HassLanguage.dll $File
