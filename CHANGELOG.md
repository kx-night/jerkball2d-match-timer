# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2026-07-19

### Added

- `MatchTimer` core timer class (`Jerkball2D`) with `Play`, `Pause`, `Restart`, `ResetTo`, and `DigitalClock` formatted output, designed to run unmodified in Unity, Godot 4, raylib, MonoGame, Stride, or any custom .NET game loop.
- `Jerkball2D.TimerExtensions` F# functional wrapper providing pure `tick` state transitions, `Result`-based validation via `tryResetTo`, active patterns for state matching, and `onFinished` event subscription with `IDisposable` cleanup.
- Engine integration examples for Unity, Godot 4, raylib, and Stride.
- Integrated standalone `actions/cache@v4` caching layer inside the continuous integration workflow.
- Established a `markdownlint` configuration file to ensure repository documentation quality.
- `scripts/run-benchmarks.sh` and `scripts/run-benchmarks.ps1` wrapper scripts for running the BenchmarkDotNet suite by framework and filter.

### Changed

- Standardized action step naming conventions across both `ci.yml` and `bench.yml` pipelines.
- Configured multi-targeted .NET SDK environment specs (`8.0.x` and `10.0.x`) within the benchmarking runtime execution step.
- Corrected `README.md` and `CONTRIBUTING.md` to state the actual supported target frameworks (`net8.0`, `net9.0`, `net10.0`, as set in `Directory.Build.props`), replacing an earlier inaccurate `.NET Standard 2.0+ / .NET 6.0+` claim.
- Updated `CONTRIBUTING.md` project structure section to match the real solution layout (`src/Core`, `src/Demo`, `src/Extensions`, `src/Diagnostics`).

### Fixed

- Resolved `.slnx` parsing structural syntax errors by correcting required edge slashes on virtual folders.

### Removed

- Removed absolute/unslashed solution folder paths to adhere to strict `.slnx` validation engine specifications.

[Unreleased]: https://github.com/kx-night/jerkball2d-match-timer/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/kx-night/jerkball2d-match-timer/releases/tag/v1.0.0
