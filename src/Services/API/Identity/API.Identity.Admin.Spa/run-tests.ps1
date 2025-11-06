# Identity Admin SPA Test Runner (PowerShell)
# This script runs all tests for the Identity Admin SPA project

param(
    [Parameter(Position=0)]
    [ValidateSet('angular', 'dotnet', 'docker', 'all')]
    [string]$TestType = 'all'
)

$ErrorActionPreference = "Stop"

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "Identity Admin SPA - Test Runner" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""

# Get the directory where the script is located
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

function Print-Status {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Green
}

function Print-Warning {
    param([string]$Message)
    Write-Host "[WARN] $Message" -ForegroundColor Yellow
}

function Print-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Run-AngularTests {
    Print-Status "Running Angular unit tests..."
    Push-Location "$ScriptDir\Client"

    if (-not (Test-Path "node_modules")) {
        Print-Warning "node_modules not found. Installing dependencies..."
        npm install
    }

    npm test -- --watch=false --browsers=ChromeHeadless

    if ($LASTEXITCODE -eq 0) {
        Print-Status "Angular tests passed ✓"
    } else {
        Print-Error "Angular tests failed ✗"
        Pop-Location
        throw "Angular tests failed"
    }

    Pop-Location
}

function Run-DotnetTests {
    Print-Status "Running ASP.NET Core integration tests..."
    Push-Location "$ScriptDir\..\API.Identity.Admin.Spa.Tests"

    dotnet test --verbosity normal

    if ($LASTEXITCODE -eq 0) {
        Print-Status "ASP.NET Core tests passed ✓"
    } else {
        Print-Error "ASP.NET Core tests failed ✗"
        Pop-Location
        throw "ASP.NET Core tests failed"
    }

    Pop-Location
}

function Build-DockerImage {
    Print-Status "Building Docker image..."

    # Navigate to repo root (7 levels up from script directory)
    $RepoRoot = Split-Path (Split-Path (Split-Path (Split-Path (Split-Path (Split-Path (Split-Path $ScriptDir))))))
    Push-Location $RepoRoot

    docker build -f src/Services/API/Identity/API.Identity.Admin.Spa/Dockerfile `
        -t exdrums/hbg-admin-spa:test `
        .

    if ($LASTEXITCODE -eq 0) {
        Print-Status "Docker image built successfully ✓"
    } else {
        Print-Error "Docker image build failed ✗"
        Pop-Location
        throw "Docker image build failed"
    }

    Pop-Location
}

function Test-DockerHealth {
    Print-Status "Testing Docker container health check..."

    # Start container
    docker run -d --name hbg-admin-spa-test `
        -p 8080:80 `
        -e ASPNETCORE_ENVIRONMENT=Development `
        exdrums/hbg-admin-spa:test

    # Wait for container to be ready
    Start-Sleep -Seconds 10

    # Test health endpoint
    try {
        $Response = Invoke-WebRequest -Uri "http://localhost:8080/health" -UseBasicParsing
        $HealthStatus = $Response.StatusCode
    } catch {
        $HealthStatus = 0
    }

    # Cleanup
    docker stop hbg-admin-spa-test
    docker rm hbg-admin-spa-test

    if ($HealthStatus -eq 200) {
        Print-Status "Docker health check passed ✓"
    } else {
        Print-Error "Docker health check failed (HTTP $HealthStatus) ✗"
        throw "Docker health check failed"
    }
}

# Main execution
try {
    switch ($TestType) {
        'angular' {
            Run-AngularTests
        }
        'dotnet' {
            Run-DotnetTests
        }
        'docker' {
            Build-DockerImage
            Test-DockerHealth
        }
        'all' {
            Run-AngularTests
            Run-DotnetTests
            Build-DockerImage
            Test-DockerHealth
        }
    }

    Write-Host ""
    Print-Status "All tests completed successfully! ✓"
    exit 0
} catch {
    Write-Host ""
    Print-Error "Some tests failed ✗"
    Print-Error $_.Exception.Message
    exit 1
}
