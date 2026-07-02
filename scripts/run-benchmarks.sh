#!/usr/bin/env bash
set -euo pipefail

main() {
  local script_dir repo_root target_dir bench_project framework filter

  script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
  repo_root="$(cd "${script_dir}/.." && pwd)"
  target_dir="${repo_root}/src/Diagnostics/Jerkball2D.MatchTimer.Benchmarks"
  bench_project="Jerkball2D.MatchTimer.Benchmarks.csproj"

  framework="${1:-net8.0}"
  filter="${2:-*}"

  case "${framework}" in
    net8.0|net9.0|net10.0) ;;
    *)
      echo "❌ Invalid framework '${framework}'. Allowed: net8.0, net9.0, net10.0." >&2
      exit 1
      ;;
  esac

  if ! command -v dotnet >/dev/null 2>&1; then
    echo "❌ dotnet SDK not found on PATH. Please install .NET and try again." >&2
    exit 1
  fi

  if [[ ! -d "${target_dir}" ]]; then
    echo "❌ Benchmarks directory not found: ${target_dir}" >&2
    exit 1
  fi

  if [[ ! -f "${target_dir}/${bench_project}" ]]; then
    echo "❌ Benchmarks project file not found: ${target_dir}/${bench_project}" >&2
    exit 1
  fi

  echo "⚙️  Stepping into diagnostics context..."
  pushd "${target_dir}" >/dev/null
  trap 'popd >/dev/null 2>&1 || true' EXIT

  echo "🔥 Launching BenchmarkDotNet Execution Suite (${framework}, filter='${filter}')..."
  dotnet run -c Release -f "${framework}" -- \
    --exporters json markdown \
    --filter "${filter}"

  echo "✅ Execution complete! Performance artifacts generated in:"
  echo "   ${target_dir}/BenchmarkDotNet.Artifacts/results/"
}

main "$@"
