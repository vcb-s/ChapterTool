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
The rewrite SHALL migrate and strengthen existing MSTest coverage into .NET 10 tests that run on xUnit v3.

#### Scenario: Test projects reference xUnit v3
- **WHEN** test project package references are inspected
- **THEN** Core, Infrastructure, and Avalonia test projects SHALL use xUnit v3 framework packages and a compatible xUnit v3 Visual Studio runner
- **AND** they SHALL NOT retain xUnit v2 framework package references

#### Scenario: Existing parser tests are preserved
- **WHEN** tests are migrated
- **THEN** equivalent assertions SHALL exist for Expression, ToolKits, OGM, WebVTT, CUE, MPLS, IFO, media chapter import, and SharpDvdInfo behavior

#### Scenario: FFprobe media reader tests cover adapter behavior
- **WHEN** media import tests run
- **THEN** they SHALL cover successful ffprobe JSON output, missing ffprobe diagnostics, cannot-start diagnostics, process failure diagnostics, timeout/cancellation handling, malformed JSON, unsupported or chapterless media, Unicode chapter names, timestamp fallback from `time_base`, cannot-invoke fallback routing to legacy importers, multi-edition grouping by EDITION_UID, single-edition fallback without EDITION_UID, mixed EDITION_UID presence, and empty chapter output without requiring a real installed command-line tool

#### Scenario: Fallback routing tests verify automatic degradation
- **WHEN** importer registry/load service tests run
- **THEN** they SHALL cover ffprobe-cannot-invoke-to-ATL fallback for `.mp4`/`.m4a`/`.m4v`, mkvextract-cannot-invoke-to-ffprobe fallback for Matroska-family files, fallback informational diagnostics, invoked-ffprobe-failure-without-ATL-fallback, invoked-mkvextract-failure-without-ffprobe-fallback, and no-fallback diagnostic for extensions without legacy importer support

#### Scenario: External tool discovery tests avoid machine installation dependencies
- **WHEN** ffprobe discovery tests run
- **THEN** they SHALL cover configured executable path, configured FFmpeg directory path, PATH/search-directory discovery, platform executable naming, and missing-tool diagnostics using fake filesystem/platform probes rather than requiring a real FFmpeg installation

#### Scenario: External process encoding tests preserve non-ASCII output
- **WHEN** process runner tests exercise redirected stdout and stderr
- **THEN** they SHALL include non-ASCII text and verify it is decoded without platform terminal mojibake

#### Scenario: Core remains free of platform implementation details
- **WHEN** Core tests verify project and type dependencies
- **THEN** Core SHALL NOT reference registry access, filesystem discovery, FFmpeg probing, MKVToolNix app-bundle probing, or process encoding implementation types

#### Scenario: Tests are split by responsibility
- **WHEN** tests are organized
- **THEN** Core behavior SHALL live in Core tests, process/native/filesystem behavior SHALL live in Infrastructure tests, and ViewModel commands SHALL live in Avalonia or ViewModel tests

#### Scenario: xUnit v3 test suite runs through dotnet test
- **WHEN** `dotnet test ChapterTool.Avalonia.slnx --no-restore` runs after restore
- **THEN** Core, Infrastructure, and Avalonia test assemblies SHALL execute under xUnit v3 without framework discovery failures

### Requirement: Avalonia Headless UI test coverage
The Avalonia test project SHALL provide a headless Avalonia runtime for rendered UI tests that run in CI without launching the desktop application.

#### Scenario: Headless runtime initializes without desktop lifetime
- **WHEN** Avalonia UI tests start
- **THEN** they SHALL initialize an Avalonia Headless AppBuilder compatible with the app's Avalonia major version
- **AND** they SHALL NOT call `StartWithClassicDesktopLifetime` or require a platform desktop session

#### Scenario: Main window renders under headless backend
- **WHEN** a headless test constructs the main window with test services
- **THEN** the window SHALL load compiled XAML, apply localization resources, bind to the supplied ViewModel, and complete layout without throwing

#### Scenario: XML edition switching displays chapter names
- **WHEN** a headless main-window test loads an XML source with multiple edition options and selects a non-default edition through the visible clip/edition selector
- **THEN** the rendered chapter grid SHALL display the selected edition's chapter names in the name column
- **AND** the names SHALL NOT be blank, stale from the previous edition, or hidden by layout/binding failure

#### Scenario: IFO edition switching displays chapter names
- **WHEN** a headless main-window test loads an IFO source with multiple program-chain or title options and selects a non-default option through the visible clip/edition selector
- **THEN** the rendered chapter grid SHALL display the selected option's chapter names in the name column
- **AND** the names SHALL NOT be blank, stale from the previous option, or hidden by layout/binding failure

#### Scenario: MPLS clip switching displays chapter names
- **WHEN** a headless main-window test loads an MPLS source with multiple playlist or clip options and selects another option through the visible clip selector
- **THEN** the rendered chapter grid SHALL display the selected option's chapter names in the name column
- **AND** the names SHALL NOT be blank, stale from the previous option, or hidden by layout/binding failure

#### Scenario: Headless UI tests stay deterministic
- **WHEN** headless UI tests exercise XML, IFO, or MPLS switching
- **THEN** they SHALL use in-process deterministic fixtures or fake load services
- **AND** they SHALL NOT require external media tools, installed desktop applications, registry state, or machine-specific file paths

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

#### Scenario: FFprobe dependency is documented
- **WHEN** release artifacts or packaging documentation are produced
- **THEN** they SHALL document ffprobe/FFmpeg as the primary multimedia chapter reader dependency for MP4-family and other non-Matroska containers
- **AND** MKVToolNix/mkvextract SHALL be documented as the primary dependency for Matroska-family files, with ffprobe as fallback only when mkvextract cannot be invoked

#### Scenario: Legacy dependencies retained with appropriate role
- **WHEN** the Avalonia app is published
- **THEN** ATL.NET SHALL be retained as `.mp4`/`.m4a`/`.m4v` fallback (activated only when ffprobe cannot be invoked)
- **AND** MKVToolNix/mkvextract SHALL be retained as Matroska-family primary (unchanged from current)
- **AND** ffprobe/FFmpeg SHALL serve as Matroska-family fallback only when mkvextract cannot be invoked

