$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot  = Split-Path -Parent $ScriptDir
$TargetDir = Join-Path $RepoRoot "src\Diagnostics\Jerkball2D.MatchTimer.Benchmarks"

Write-Host "⚙️  Stepping into diagnostics context..."
Set-Location $TargetDir

Write-Host "🚀 Restoring dependencies..."
dotnet restore

Write-Host "🔥 Launching BenchmarkDotNet Execution Suite..."
dotnet run -c Release -- --exporters json md csv --filter "*"

Write-Host "✅ Execution complete! Performance artifacts generated in:"
Write-Host "   $TargetDir\BenchmarkDotNet.Artifacts\results\"
