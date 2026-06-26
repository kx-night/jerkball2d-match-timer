#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
TARGET_DIR="$REPO_ROOT/src/Diagnostics/Jerkball2D.MatchTimer.Benchmarks"

echo "⚙️  Stepping into diagnostics context..."
cd "$TARGET_DIR"

echo "🚀 Restoring dependencies..."
dotnet restore

echo "🔥 Launching BenchmarkDotNet Execution Suite..."
dotnet run -c Release -- --exporters json md csv --filter '*'

echo "✅ Execution complete! Performance artifacts generated in:"
echo "   $TARGET_DIR/BenchmarkDotNet.Artifacts/results/"
