$ErrorActionPreference = "Stop"

function Main {
    param(
        [ValidateSet("net8.0", "net9.0", "net10.0")]
        [string] $Framework = "net8.0"
    )

    $ScriptDir   = Split-Path -Parent $MyInvocation.MyCommand.Path
    $RepoRoot    = Split-Path -Parent $ScriptDir
    $TargetDir   = Join-Path $RepoRoot "src/Diagnostics/Jerkball2D.MatchTimer.Benchmarks"
    $BenchProj   = Join-Path $TargetDir "Jerkball2D.MatchTimer.Benchmarks.csproj"
    $OriginalDir = (Get-Location).Path

    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        Write-Error "❌ dotnet SDK not found on PATH. Please install .NET and try again."
        exit 1
    }

    if (-not (Test-Path -Path $TargetDir -PathType Container)) {
        Write-Error "❌ Benchmarks directory not found: $TargetDir"
        exit 1
    }

    if (-not (Test-Path -Path $BenchProj -PathType Leaf)) {
        Write-Error "❌ Benchmarks project file not found: $BenchProj"
        exit 1
    }

    try {
        Write-Host "⚙️  Stepping into diagnostics context..."
        Set-Location $TargetDir

        Write-Host "🚀 Restoring dependencies..."
        dotnet restore $BenchProj

        # Note: -f selects the target framework; pass -Framework net9.0 or net10.0 to match your preferred SDK runtime.
        Write-Host "🔥 Launching BenchmarkDotNet Execution Suite ($Framework)..."
        dotnet run -c Release -f $Framework -- `
            --exporters json md csv

        Write-Host "✅ Execution complete! Performance artifacts generated in:"
        Write-Host "   $TargetDir/BenchmarkDotNet.Artifacts/results/"
    }
    finally {
        Set-Location $OriginalDir
    }
}

Main
