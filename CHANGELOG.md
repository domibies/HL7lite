# Changelog

## [2.0.0-rc.1] - 2025-07-23

### Added
- **Complete Fluent API** - Modern, intuitive API for HL7 message manipulation
  - Safe-by-default encoding: `Set()` automatically encodes HL7 delimiters
  - Clear data separation: `Raw` property for HL7 data, `ToString()` for display
  - `SetRaw()` methods with hierarchical delimiter validation
  - Pure navigation pattern for clear, readable code
  - Complete abstraction with `FluentMessage.Create()`
  - Field repetitions with fluent `AddRepetition()` methods
  - LINQ-compatible collections for segments and repetitions
  - Path-based access with encoding support
  - Segment groups for analyzing message structure
  - MSHBuilder for creating MSH segments
  - DateTime utilities for HL7 date/time formats
  - Deep copy functionality
  - Safe parsing with `TryParse()` result pattern

### Changed
- Fluent API uses safe-by-default encoding (Set methods encode delimiters)
- Clear separation between Raw (HL7 data) and ToString() (human display)
- All fluent API methods use consistent Set() naming

### Fixed
- Segment.DeepCopy() now preserves field content for fluent-created segments
- Component/SubComponent mutations properly update parent field values
- Field repetitions correctly target specific segment instances

### Metrics
- 924 tests (up from 55 in v1.2.0)
- almost 90% line coverage, and 85% branch coverage
- 40 source files with 7,369 lines of code
- 54 commits implementing the fluent API
- Zero breaking changes to existing API