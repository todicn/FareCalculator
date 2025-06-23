# Build and Generate Documentation Script for FareCalculator

Write-Host "Building FareCalculator Documentation..." -ForegroundColor Green

# Build the main project to generate XML documentation
Write-Host "Building main project..." -ForegroundColor Yellow
dotnet build src/FareCalculator/FareCalculator.csproj --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to build main project" -ForegroundColor Red
    exit 1
}

# Build the test project to generate test documentation
Write-Host "Building test project..." -ForegroundColor Yellow
dotnet build tests/FareCalculator.Tests/FareCalculator.Tests.csproj --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to build test project" -ForegroundColor Red
    exit 1
}

# Generate documentation with DocFX
Write-Host "Generating documentation with DocFX..." -ForegroundColor Yellow
docfx build docfx.json
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to generate documentation" -ForegroundColor Red
    exit 1
}

Write-Host "Documentation generated successfully!" -ForegroundColor Green
Write-Host "Files available in: _site/" -ForegroundColor Cyan
Write-Host ""
Write-Host "To serve the documentation locally, run:" -ForegroundColor White
Write-Host "  docfx serve _site" -ForegroundColor Gray
Write-Host ""
Write-Host "Or to build and serve in one command:" -ForegroundColor White
Write-Host "  docfx docfx.json --serve" -ForegroundColor Gray 