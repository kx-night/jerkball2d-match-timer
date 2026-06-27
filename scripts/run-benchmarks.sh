#!/usr/bin/env bash
set -euo pipefail

main() {
  local script_dir repo_root target_dir bench_project
  script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
  repo_root="$(cd "${script_dir}/.." && pwd)"
  target_dir="${repo_root}/src/Diagnostics/Jerkball2D.MatchTimer.Benchmarks"
  bench_project="${target_dir}/Jerkball2D.MatchTimer.Benchmarks.csproj"

  if ! command -v dotnet >/dev/null 2>&1; then
    echo "❌ dotnet SDK not found on PATH. Please install .NET and try again." >&2
    exit 1
  fi

  if [[ ! -d "${target_dir}" ]]; then
    echo "❌ Benchmarks directory not found: ${target_dir}" >&2
    exit 1
  fi

  if [[ ! -f "${bench_project}" ]]; then
    echo "❌ Benchmarks project file not found: ${bench_project}" >&2
    exit 1
  fi

  echo "⚙️  Stepping into diagnostics context..."
  cd "${target_dir}"

  echo "🚀 Restoring dependencies..."
  dotnet restore "${bench_project}"

  echo "🔥 Launching BenchmarkDotNet Execution Suite..."
  dotnet run -c Release -p "${bench_project}" -- \
    --exporters json md csv \
    --filter '*'

  echo "✅ Execution complete! Performance artifacts generated in:"
  echo "   ${target_dir}/BenchmarkDotNet.Artifacts/results/"
}

main "$@"
