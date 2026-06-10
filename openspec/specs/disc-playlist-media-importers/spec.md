# disc-playlist-media-importers Specification

## Purpose
TBD - created by archiving change rewrite-avalonia-dotnet10. Update Purpose after archive.
## Requirements
### Requirement: Disc and media importer contract
The system SHALL import `.mpls`, `.ifo`, `.xpl`, BDMV directories, `.mp4`, `.m4a`, and `.m4v` through UI-independent importers.

#### Scenario: Supported source is routed
- **WHEN** the import service receives a supported file or directory
- **THEN** it SHALL route to the matching importer and return a chapter set or chapter group with structured diagnostics

#### Scenario: Invalid source is rejected
- **WHEN** the source is missing, unsupported, or structurally invalid
- **THEN** import SHALL fail without overwriting accepted UI state

### Requirement: MPLS import
The system SHALL parse Blu-ray `.mpls` playlists into selectable chapter segments.

#### Scenario: Valid MPLS produces a group
- **WHEN** a valid MPLS file has one or more play items
- **THEN** each play item SHALL produce one `ChapterInfo` with source type `MPLS`

#### Scenario: Chapter marks convert from PTS
- **WHEN** matching chapter marks are found
- **THEN** timestamps SHALL convert from PTS using `pts / 45000` seconds

#### Scenario: MPLS can append
- **WHEN** another `.mpls` is appended to an existing MPLS group
- **THEN** new play items SHALL be added while preserving existing source context

### Requirement: DVD IFO import
The system SHALL import DVD `.ifo` chapter data.

#### Scenario: Valid IFO produces DVD group
- **WHEN** a valid `VTS_nn_0.IFO` is parsed
- **THEN** the importer SHALL return one or more DVD chapter sets with normalized chapter numbers

#### Scenario: PAL and NTSC times convert
- **WHEN** DVD playback time is converted
- **THEN** PAL SHALL use 25 fps and NTSC SHALL use `30000/1001`

### Requirement: XPL import
The system SHALL parse HD-DVD `.xpl` playlist XML.

#### Scenario: Valid XPL title imports
- **WHEN** an XPL file uses the documented namespace and contains title chapter lists
- **THEN** each title with chapters SHALL produce a chapter set with source type `HD-DVD`

#### Scenario: Invalid XPL is diagnosed
- **WHEN** namespace, duration, or timestamp structure is malformed
- **THEN** the importer SHALL return a parse diagnostic instead of a null-reference failure

### Requirement: BDMV eac3to import
The system SHALL import BDMV directories through an eac3to adapter.

#### Scenario: Valid BDMV delegates chapter text
- **WHEN** eac3to lists playlists and exports chapter text
- **THEN** the importer SHALL parse exported chapter text through the OGM parser

#### Scenario: Missing eac3to is recoverable
- **WHEN** no valid eac3to path is configured
- **THEN** import SHALL return a missing-dependency result and Core SHALL NOT prompt directly

### Requirement: MP4 import
The system SHALL import MP4-family chapters through a managed cross-platform MP4 reader adapter.

#### Scenario: Reader chapters become cumulative starts
- **WHEN** MP4 chapter entries are read as titles with durations
- **THEN** the importer SHALL convert them to cumulative chapter start times

#### Scenario: Reader chapter times normalize to durations
- **WHEN** the MP4 reader backend returns chapter start/end times instead of durations
- **THEN** the reader SHALL normalize them into ordered durations before returning `Mp4ChapterClip` entries

#### Scenario: Managed reader failures are diagnosed
- **WHEN** the managed MP4 reader cannot read chapter metadata because the file is invalid, unsupported, inaccessible, or lacks a readable chapter table
- **THEN** import SHALL return structured diagnostics without UI prompts

#### Scenario: Legacy native library is not required
- **WHEN** MP4 import runs on Windows, macOS, or Linux
- **THEN** it SHALL NOT require `libmp4v2` native library resolution for the default import path

#### Scenario: Empty MP4 chapter set is rejected
- **WHEN** the MP4 reader succeeds but returns no chapter entries
- **THEN** import SHALL fail with a no-chapters diagnostic

#### Scenario: MP4 reader is runtime-substitutable
- **WHEN** tests or composition replace the MP4 reader implementation
- **THEN** `.mp4`, `.m4a`, and `.m4v` imports SHALL use the replacement without changing UI code

#### Scenario: Unicode chapter names are preserved
- **WHEN** MP4 chapter metadata contains non-ASCII titles
- **THEN** the reader SHALL return the original Unicode strings without loss or mangling

#### Scenario: Placeholder reader is not wired in production
- **WHEN** the Avalonia application composes the MP4 import path
- **THEN** `MissingMp4ChapterReader` SHALL NOT be used as the production reader implementation

### Requirement: Segment selection and combine
The system SHALL preserve multi-segment selection, combine, and related-media behavior with structured models.

#### Scenario: Combine is limited to MPLS and IFO
- **WHEN** combine is requested for a non-MPLS/IFO source group
- **THEN** the command SHALL be disabled or return a validation diagnostic

#### Scenario: Combined segments offset times
- **WHEN** MPLS or IFO segments are combined
- **THEN** chapter times SHALL be offset by accumulated segment duration and names SHALL be renumbered

### Requirement: Runtime importer registry
Runtime chapter loading SHALL route supported sources through injected importer registrations or factories.

#### Scenario: Supported source dispatches through registry
- **WHEN** the load service receives `.mpls`, `.ifo`, `.xpl`, BDMV directory, `.mp4`, `.m4a`, `.m4v`, or other supported chapter sources
- **THEN** it SHALL select the matching importer through an injected registry or factory model and return the importer result

#### Scenario: Importer infrastructure is injected
- **WHEN** dependency-backed importers require external tool location, process execution, native dependency resolution, filesystem access, or parser adapters
- **THEN** those dependencies SHALL come from registered services rather than being constructed inside each load operation

#### Scenario: Importer dispatch is test-substitutable
- **WHEN** tests replace importer registrations, external tool locators, process runners, or native dependency services
- **THEN** runtime loading SHALL use the replacements without requiring changes to UI code or concrete runtime service constructors

#### Scenario: Dependencies are not recreated per load by default
- **WHEN** multiple load operations run in one application session
- **THEN** singleton or scoped infrastructure services SHALL follow their registered lifetimes instead of being recreated manually inside `LoadAsync`
