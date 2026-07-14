# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Integrated standalone `actions/cache@v4` caching layer inside the continuous integration workflow.
- Established a `markdownlint` configuration file to ensure repository documentation quality.

### Changed

- Standardized action step naming conventions across both `ci.yml` and `bench.yml` pipelines.
- Configured multi-targeted .NET SDK environment specs (`8.0.x` and `10.0.x`) within the benchmarking runtime execution step.

### Fixed

- Resolved `.slnx` parsing structural syntax errors by correcting required edge slashes on virtual folders.

### Removed

- Removed absolute/unslashed solution folder paths to adhere to strict `.slnx` validation engine specifications.
