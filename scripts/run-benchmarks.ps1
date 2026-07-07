$ErrorActionPreference = "Stop"

function Main {
    param(
        [ValidateSet("net8.0", "net9.0", "net10.0")]
        [string] $Framework = "net8.0",

        [string] $Filter = "*"
    )

    $ScriptDir = $PSScriptRoot
    if ([string]::IsNullOrWhiteSpace($ScriptDir)) {
        throw "Unable to determine script directory via PSScriptRoot."
    }

    $RepoRoot = Split-Path -Parent $ScriptDir
    $TargetDir = Join-Path $RepoRoot "src/Diagnostics/Jerkball2D.MatchTimer.Benchmarks"
    $BenchProj = Join-Path $TargetDir "Jerkball2D.MatchTimer.Benchmarks.csproj"

    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        throw "❌ dotnet SDK not found on PATH. Please install .NET and try again."
    }

    if (-not (Test-Path -LiteralPath $TargetDir -PathType Container)) {
        throw "❌ Benchmarks directory not found: $TargetDir"
    }

    if (-not (Test-Path -LiteralPath $BenchProj -PathType Leaf)) {
        throw "❌ Benchmarks project file not found: $BenchProj"
    }

    Push-Location $TargetDir
    try {
        Write-Host "⚙️  Stepping into diagnostics context..."
        Write-Host "🔥 Launching BenchmarkDotNet Execution Suite ($Framework, filter='$Filter')..."

        $dotnetArgs = @(
            'run',
            '-c', 'Release',
            '-f', $Framework,
            '--',
            '--exporters', 'json', 'markdown',
            "--filter=$Filter"
        )

        & dotnet @dotnetArgs

        Write-Host "✅ Execution complete! Artifacts generated at:"
        Write-Host "   $TargetDir/BenchmarkDotNet.Artifacts/results/"
    }
    finally {
        Pop-Location
    }
}

if ($args.Count -gt 0 -and ($args[0] -in @('-h', '--help'))) {
    Write-Host @"
Usage: pwsh ./scripts/run-benchmarks.ps1 [framework] [filter]

  framework   net8.0, net9.0, net10.0   Default: net8.0
  filter      BenchmarkDotNet glob      Default: *
"@
    exit 0
}

Main @args
