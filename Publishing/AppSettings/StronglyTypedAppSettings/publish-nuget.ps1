# publish-nuget.ps1
param(
    [Parameter(Mandatory=$true)]
    [string]$ApiKey,
    
    [Parameter(Mandatory=$false)]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false)]
    [bool]$PushToNuGet = $true,
    
    [Parameter(Mandatory=$false)]
    [string]$ProjectPath = "../../AppSettingsAccessor/AppSettingsAccessors.csproj"
)

# Set working directory to the script location
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

Write-Host "Starting NuGet packaging process..." -ForegroundColor Cyan

# Step 1: Clean and build the project
Write-Host "Building project in $Configuration configuration..." -ForegroundColor Green
dotnet clean $ProjectPath -c $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to clean the project!" -ForegroundColor Red
    exit 1
}

dotnet build $ProjectPath -c $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to build the project!" -ForegroundColor Red
    exit 1
}

# Step 2: Create the NuGet package
Write-Host "Creating NuGet package..." -ForegroundColor Green
dotnet pack $ProjectPath -c $Configuration --no-build
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to create the NuGet package!" -ForegroundColor Red
    exit 1
}

# Find the created package
$packageDir = Join-Path -Path (Split-Path -Parent $ProjectPath) -ChildPath "bin\$Configuration"
$packageFile = Get-ChildItem -Path $packageDir -Filter "*.nupkg" | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if (-not $packageFile) {
    Write-Host "Could not find the NuGet package!" -ForegroundColor Red
    exit 1
}

Write-Host "Created package: $($packageFile.FullName)" -ForegroundColor Green

# Step 3: Test the package locally (optional)
$localSource = Join-Path -Path $scriptPath -ChildPath "local-packages"
if (-not (Test-Path $localSource)) {
    New-Item -ItemType Directory -Path $localSource | Out-Null
}

Copy-Item -Path $packageFile.FullName -Destination $localSource
Write-Host "Package copied to local repository: $localSource" -ForegroundColor Green

# Step 4: Push to NuGet.org if requested
if ($PushToNuGet) {
    Write-Host "Publishing package to NuGet.org..." -ForegroundColor Green
    dotnet nuget push $packageFile.FullName --api-key $ApiKey --source https://api.nuget.org/v3/index.json
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to publish the package to NuGet.org!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Package successfully published to NuGet.org!" -ForegroundColor Green
} else {
    Write-Host "Skipping NuGet.org publishing as requested." -ForegroundColor Yellow
}

Write-Host "NuGet packaging process completed successfully!" -ForegroundColor Cyan
