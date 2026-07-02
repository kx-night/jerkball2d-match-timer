$ErrorActionPreference = "Stop"

function Main {
    param(
        [ValidateSet("net8.0", "net9.0", "net10.0")]
        [string] $Framework = "net8.0",
        [string] $Filter = "*"
    )

    $ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $RepoRoot  = Split-Path -Parent $ScriptDir
    $TargetDir = Join-Path $RepoRoot "src/Diagnostics/Jerkball2D.MatchTimer.Benchmarks"
    $BenchProj = Join-Path $TargetDir "Jerkball2D.MatchTimer.Benchmarks.csproj"

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
        Push-Location $TargetDir

        $dotnetArgs = @(
            'run',
            '-c', 'Release',
            '-f', $Framework,
            '--',
            '--exporters', 'json', 'markdown',
            '--filter', "$Filter"
        )

        Write-Host "🔥 Launching BenchmarkDotNet Execution Suite ($Framework, filter='$Filter')..."
        dotnet @dotnetArgs

        Write-Host "✅ Execution complete! Performance artifacts generated in:"
        Write-Host "   $TargetDir/BenchmarkDotNet.Artifacts/results/"
    }
    finally {
        Pop-Location
    }
}

if ($args -contains '-h' -or $args -contains '--help') {
    Write-Host @"
Usage: $($MyInvocation.MyCommand.Name) [framework] [filter]

  framework   - net8.0, net9.0, net10.0 (default: net8.0)
  filter      - BenchmarkDotNet glob pattern (default: *)
"@
    exit 0
}

Main @args
