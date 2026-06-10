# tests-build-distribution-assets Specification

## Purpose
TBD - created by archiving change rewrite-avalonia-dotnet10. Update Purpose after archive.
## Requirements
### Requirement: .NET 10 solution topology
The rewrite SHALL provide an SDK-style .NET 10 solution with separated Core, Infrastructure, Avalonia, and test projects.

#### Scenario: Required projects restore
- **WHEN** `dotnet restore` runs on a clean checkout
- **THEN** projects for Core, Infrastructure, Avalonia, and tests SHALL restore successfully

#### Scenario: Core has no UI dependency
- **WHEN** project references are inspected
- **THEN** Core SHALL NOT reference Avalonia, WinForms, System.Drawing UI APIs, or Windows-only platform APIs

### Requirement: xUnit test migration
The rewrite SHALL migrate and strengthen existing MSTest coverage into .NET 10 tests.

#### Scenario: Existing parser tests are preserved
- **WHEN** tests are migrated
- **THEN** equivalent assertions SHALL exist for Expression, ToolKits, OGM, WebVTT, CUE, MPLS, IFO, MP4, and SharpDvdInfo behavior

#### Scenario: MP4 reader tests cover managed adapter behavior
- **WHEN** MP4 import tests run
- **THEN** they SHALL cover successful reader output, reader exception diagnostics, unsupported or malformed metadata, Unicode chapter names, and empty chapter output without requiring `libmp4v2` or a real installed MP4 command-line tool

#### Scenario: MKVToolNix discovery tests avoid machine installation dependencies
- **WHEN** mkvextract discovery tests run
- **THEN** they SHALL cover configured path, PATH search, Windows registry installation data, macOS app bundle discovery, and missing-tool diagnostics using fake filesystem/platform probes rather than requiring a real MKVToolNix installation

#### Scenario: External process encoding tests preserve non-ASCII output
- **WHEN** process runner tests exercise redirected stdout and stderr
- **THEN** they SHALL include non-ASCII text and verify it is decoded without platform terminal mojibake

#### Scenario: Core remains free of platform implementation details
- **WHEN** Core tests verify project and type dependencies
- **THEN** Core SHALL NOT reference registry access, filesystem discovery, MKVToolNix app-bundle probing, or process encoding implementation types

#### Scenario: Tests are split by responsibility
- **WHEN** tests are organized
- **THEN** Core behavior SHALL live in Core tests, process/native/filesystem behavior SHALL live in Infrastructure tests, and ViewModel commands SHALL live in Avalonia or ViewModel tests

### Requirement: Fixture preservation
The rewrite SHALL preserve existing sample fixtures as deterministic test assets.

#### Scenario: Fixtures are working-directory independent
- **WHEN** tests run from CLI, IDE, or CI
- **THEN** VTT, OGM, CUE, MPLS, IFO, MP4, and expression fixtures SHALL be resolved without fragile current-directory assumptions

#### Scenario: Non-ASCII fixture names work
- **WHEN** the Japanese CUE sample is checked out and tested
- **THEN** filename and content SHALL remain usable

### Requirement: CI build test publish
The rewrite SHALL use .NET CLI based CI.

#### Scenario: CI runs tests
- **WHEN** CI runs on push or pull request
- **THEN** it SHALL execute restore, build, and test, and any test failure SHALL fail the workflow

#### Scenario: Publish artifacts are explicit
- **WHEN** a release workflow publishes artifacts
- **THEN** artifacts SHALL include declared runtime files, assets, licenses, and native dependencies according to packaging decisions

### Requirement: Packaging, assets, licenses, and versioning
The rewrite SHALL account for installer strategy, native dependencies, assets, licenses, and a unified version source.

#### Scenario: Legacy bundling tools are replaced or justified
- **WHEN** the .NET 10 app is packaged
- **THEN** Fody/Costura SHALL NOT be required unless a new design decision explicitly justifies them

#### Scenario: Version source is unified
- **WHEN** assembly, package, installer, or release metadata is generated
- **THEN** all versions SHALL derive from one source and SHALL NOT diverge

#### Scenario: Assets are migrated or retired
- **WHEN** packaging is implemented
- **THEN** icons, UI images, native DLLs, and third-party license files SHALL be migrated, replaced, or explicitly retired with rationale

#### Scenario: MP4 managed dependency is documented
- **WHEN** release artifacts or packaging documentation are produced
- **THEN** they SHALL document ATL.NET as the managed MP4 chapter reader dependency and explain that no separate MP4 CLI installation is required for the default path

#### Scenario: Legacy libmp4v2 binaries are not bundled by default
- **WHEN** the Avalonia app is published
- **THEN** publish output SHALL NOT include `Time_Shift/mp4v2` legacy native DLLs unless a later design explicitly restores a maintained native MP4 backend

