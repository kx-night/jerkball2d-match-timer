# Contributing to Jerkball2D.MatchTimer

Thanks for considering a contribution to `Jerkball2D.MatchTimer` — an engine-agnostic timing library for C# game loops. Community contributions are what keep it robust, efficient, and portable across engines.

This document covers everything you need to set up, build, and submit changes.

---

## Table of Contents

- [Before You Start](#before-you-start)
- [Development Environment](#development-environment)
- [Project Structure](#project-structure)
- [Architectural Principles](#architectural-principles)
- [Making a Change](#making-a-change)
- [Testing & Benchmarking](#testing--benchmarking)
- [Commit & PR Conventions](#commit--pr-conventions)
- [Licensing & File Headers](#licensing--file-headers)
- [Reporting Bugs & Requesting Features](#reporting-bugs--requesting-features)

---

## Before You Start

- Check open [issues](../../issues) and [pull requests](../../pulls) to avoid duplicate work.
- For anything larger than a small fix (new public API, breaking change, new engine sample), open an issue first to discuss the approach before writing code.
- Small fixes (typos, docs, obvious bugs, missing test cases) can go straight to a PR.

---

## Development Environment

| Requirement | Version |
|---|---|
| Target frameworks | `net8.0`, `net9.0`, `net10.0` (multi-targeted, set in `Directory.Build.props`) |
| C# language version | `LangVersion=latest` |
| F# language version | `F# 6.0+` |
| Nullable reference types | Enabled project-wide (`<Nullable>enable</Nullable>` in `Directory.Build.props`) — do not disable in new projects |
| Code style enforcement | `EnforceCodeStyleInBuild=true` — style violations fail the build, not just `dotnet format` |
| Analyzers | `AnalysisLevel=latest-recommended` |
| Test framework | xUnit |
| Benchmarks | BenchmarkDotNet |
| CI | GitHub Actions (`ci.yml`, `bench.yml`) |

> Note: `ImplicitUsings` is enabled for C# projects and disabled for F# projects — this is set per-language via a condition in `Directory.Build.props`, so you don't need to configure it per-project.

Clone and build:

```bash
git clone https://github.com/kx-night/jerkball2d-match-timer.git
cd jerkball2d-match-timer
dotnet restore
dotnet build
```

### Git hooks (Husky.NET) — not yet configured

This repo does not currently have a `.config/dotnet-tools.json` tool manifest or Husky hooks set up, so there is no automatic pre-commit tooling yet. If you want to set it up locally (or you're the one adding it to the repo):

```bash
dotnet new tool-manifest        # creates .config/dotnet-tools.json
dotnet tool install Husky.Net
dotnet tool restore
dotnet husky install            # creates .husky/ with hook scripts
dotnet husky add pre-commit -c "dotnet format --verify-no-changes"
```

Until `.config/dotnet-tools.json` and `.husky/` are committed to the repo, `dotnet husky install` will fail for other contributors with a "command not found" error — this is expected in the current state. If you add this setup, please commit both the manifest and `.husky/`, and update this section to remove the "not yet configured" note.

---

## Project Structure

Structure as defined in `Jerkball2D.MatchTimer.slnx`:

```
src/
├── Core/
│   └── Jerkball2D.MatchTimer/              Core timer logic (engine-agnostic, no external deps)
├── Demo/
│   └── Jerkball2D.MatchTimer.Demo/         Console demo / usage example
├── Extensions/
│   └── Jerkball2D.TimerExtensions/         F# functional wrapper (tick, Result-based validation, active patterns)
└── Diagnostics/
    ├── Jerkball2D.MatchTimer.Benchmarks/   BenchmarkDotNet performance suite
    └── Jerkball2D.MatchTimer.Tests/        xUnit test suite

scripts/
├── run-benchmarks.sh                       Benchmark runner (macOS/Linux)
└── run-benchmarks.ps1                      Benchmark runner (Windows/pwsh)
```

Engine integration snippets (Unity, Godot, raylib, Stride) live as documentation examples in the README rather than as buildable sample projects in the solution — see `README.md` if you're adding a new engine example.

---

## Architectural Principles

Contributions must respect the constraints that keep this library small and portable:

### 1. Engine Agnosticism

`src/Core/Jerkball2D.MatchTimer` must stay free of engine-specific code or external dependencies. It should run identically in Unity, Godot 4, raylib, MonoGame, Stride, or a plain console app.

### 2. State & Threading Model

`MatchTimer` holds mutable local state and is intentionally **not thread-safe**. Do not add internal locks, `Interlocked` calls, or async synchronization — the calling game loop owns thread safety.

### 3. Functional Interoperability

`Jerkball2D.TimerExtensions` (F#) provides pure functional transformations (`tick`), `Result`-based validation (`tryResetTo`), and active patterns. Changes to this module must:

- Preserve immutability of inputs (return new state, don't mutate).
- Remain clean to consume from C# — the README's C# usage example (`TimerController.tick`, `TimerController.play`, `resetResult.IsOk`) is the interop contract; don't break that shape without discussion.

### 4. API Ergonomics

Public methods that configure or advance timer state should return the timer instance to support fluent chaining, e.g. `timer.Play().Pause()`.

---

## Making a Change

1. **Fork & branch** from `main`:
   - `feature/<short-description>` for new functionality
   - `bugfix/<short-description>` for fixes
   - `docs/<short-description>` for documentation-only changes

2. **Write the code.** Style violations will fail the build (`EnforceCodeStyleInBuild=true`), so run `dotnet format` before committing if you're unsure.

3. **Add or update tests** in `src/Diagnostics/Jerkball2D.MatchTimer.Tests`. Changes affecting performance-sensitive paths (tick loop, state transitions) should include or update a case in `src/Diagnostics/Jerkball2D.MatchTimer.Benchmarks`.

4. **Update docs** — XML doc comments on public members (documentation file generation is enabled: `GenerateDocumentationFile=true`), and `README.md` if the public API surface changes.

---

## Testing & Benchmarking

Run the full test suite before opening a PR:

```bash
dotnet test src/Diagnostics/Jerkball2D.MatchTimer.Tests
```

Run benchmarks for anything touching the hot path (`Tick`, `Update`, state transitions) using the wrapper scripts in `scripts/` rather than calling `dotnet run` directly — they validate the SDK, the benchmarks project path, and the framework argument before launching:

```bash
# macOS / Linux
./scripts/run-benchmarks.sh [framework] [filter]

# Windows / cross-platform
pwsh ./scripts/run-benchmarks.ps1 [framework] [filter]
```

- `framework` — one of `net8.0`, `net9.0`, `net10.0` (default `net8.0`)
- `filter` — a BenchmarkDotNet glob filter (default `*`, i.e. all benchmarks)

Examples:

```bash
./scripts/run-benchmarks.sh                       # net8.0, all benchmarks
./scripts/run-benchmarks.sh net10.0                # net10.0, all benchmarks
./scripts/run-benchmarks.sh net9.0 "*Tick*"        # net9.0, only Tick-related benchmarks
```

Both scripts export `json` and `markdown` results to `src/Diagnostics/Jerkball2D.MatchTimer.Benchmarks/BenchmarkDotNet.Artifacts/results/`. Since the project multi-targets `net8.0`/`net9.0`/`net10.0`, run the script against more than one framework if your change touches anything runtime-version-sensitive.

Include before/after benchmark numbers (or the exported markdown) in your PR description if your change could affect throughput or allocations — CI runs `bench.yml` but doesn't currently gate merges on regressions automatically.

---

## Commit & PR Conventions

- Keep commits focused; avoid bundling unrelated changes.
- Prefix commit messages by type where practical: `fix:`, `feat:`, `docs:`, `test:`, `perf:`, `refactor:`.
- In the PR description, include:
  - **What** changed and **why**.
  - Any tracking issue number (`Closes #123`).
  - Whether it's a breaking change, and migration notes if so.
  - Benchmark results, if relevant.
- CI (`ci.yml` build/test, `bench.yml` benchmarks) must pass before review.

---

## Licensing & File Headers

This project is distributed under the **Mozilla Public License 2.0** (MPL-2.0). By submitting a contribution, you agree it will be licensed under these terms.

Every new source file (C# and F#) must start with:

```csharp
// SPDX-License-Identifier: MPL-2.0
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. See the LICENSE file in the repository root for more details.
//
// Copyright (c) 2026 kx-night
```

This matches the `Copyright` property already set in `Directory.Build.props`, so package metadata and source headers stay consistent.

---

## Reporting Bugs & Requesting Features

When filing an issue, please include:

- **Bugs:** minimal repro (ideally a small code snippet or failing test), expected vs. actual behavior, target framework (`net8.0`/`net9.0`/`net10.0`) and engine, and library version.
- **Features:** the use case driving the request, not just the API shape — this helps keep the core aligned with the engine-agnostic principle above.

Thanks again for contributing — every fix, test, and doc improvement makes this a better tool for the next person building a game loop on top of it.
