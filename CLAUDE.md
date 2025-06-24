# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build --configuration Release

# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Run a specific test
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"

# Pack NuGet package
dotnet pack src/HL7lite.csproj --configuration Release

# Generate coverage report locally
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
reportgenerator -reports:coverage/**/coverage.cobertura.xml -targetdir:coverage/report -reporttypes:"Cobertura;HtmlSummary;Badges"
```


## Architecture Overview

HL7lite is a lightweight HL7 2.x message parser following a hierarchical structure:

```
Message
├── Segment (MSH, PID, OBR, etc.)
│   ├── Field
│   │   ├── Component
│   │   │   └── SubComponent
│   │   └── Repetitions (for repeated fields)
│   └── Repetitions (for repeated segments)
```

### Core Components

**Message**: Root container for HL7 message
- Contains segments in order
- Provides encoding rules for delimiters
- Original API: `GetValue()`, `SetValue()`, `PutValue()`

**Segment**: Individual HL7 segments (MSH, PID, etc.)
- Contains fields
- Can have multiple instances (repeating segments)
- Original API: `FieldList` property

**Field**: Individual data fields within segments
- Can contain components or simple values
- Support repetitions (multiple values)
- 1-based indexing in HL7 standard

**Component**: Sub-parts of complex fields
- Separated by ^ (component separator)
- Can contain subcomponents
- 1-based indexing

**SubComponent**: Finest granularity level
- Separated by & (subcomponent separator)
- Contains actual values
- 1-based indexing

## Fluent API Architecture

The Fluent API provides a modern, intuitive interface built on top of the existing Message API:

### Key Design Principles

1. **Non-breaking**: All existing Message API functionality preserved
2. **Null-safe**: Invalid access returns empty values instead of exceptions
3. **Fluent**: Method chaining for intuitive operations
4. **1-based indexing**: Follows HL7 standard (fields, components, subcomponents)
5. **0-based collections**: LINQ-compatible collections use 0-based indexing
6. **Cached accessors**: Performance optimization through caching

### Fluent API Components

**FluentMessage**: Main entry point
```csharp
var fluent = new FluentMessage(message);
// or
var fluent = hl7String.ToFluentMessage();
```

**Accessors** (Read-only operations):
- `SegmentAccessor`: Access to segments (`fluent.PID`, `fluent["MSH"]`)
- `FieldAccessor`: Access to fields (`fluent.PID[3]`)
- `ComponentAccessor`: Access to components (`fluent.PID[5][1]`)
- `SubComponentAccessor`: Access to subcomponents (`fluent.PID[5][1][1]`)

**Mutators** (Write operations):
- `FieldMutator`: Field mutations (`fluent.PID[3].Set()`)
- `ComponentMutator`: Component mutations (`fluent.PID[5][1].Set()`)
- `SubComponentMutator`: Subcomponent mutations (`fluent.PID[5][1][1].Set()`)

**Collections** (LINQ support):
- `SegmentCollection`: Multiple segments (`fluent.Segments("DG1")`)
- `FieldRepetitionCollection`: Field repetitions (`fluent.PID[3].Repetitions`)

**Special Features**:
- `PathAccessor`: String-based access (`fluent.Path("PID.5.1")`)
- `MSHBuilder`: Fluent MSH header creation
- Encoding support for HL7 delimiter characters
- DateTime utilities for HL7 date/time formats

### Error Handling Philosophy

**Accessors** (reading):
- Invalid segment/field/component access returns empty accessor
- `.Value` returns `null` for HL7 null values (`""`)
- `.Exists` checks if element actually exists
- Never throws exceptions for invalid access

**Mutators** (writing):
- Create missing segments/fields/components automatically
- Validate indices (negative indices throw ArgumentException)
- Use message encoding for null values
- Support method chaining

### Indexing Strategy

**1-based** (HL7 Standard):
- Field indices: `fluent.PID[3]` (3rd field)
- Component indices: `fluent.PID[5][1]` (1st component)  
- SubComponent indices: `fluent.PID[5][1][1]` (1st subcomponent)
- Collection methods: `.Repetition(1)`, `.Segment(1)`

**0-based** (LINQ Compatibility):
- Collection indexers: `fluent.Segments("DG1")[0]`
- LINQ operations: `.Where()`, `.Select()`, `.First()`

### HL7 Encoding Support

All mutators support automatic encoding of HL7 delimiter characters:

**Standard Delimiters**:
- `|` (field separator) → `\F\`
- `^` (component separator) → `\S\`  
- `~` (repetition separator) → `\R\`
- `\` (escape character) → `\E\`
- `&` (subcomponent separator) → `\T\`

**Usage**:
```csharp
// Automatic encoding
fluent.PID[5].Set().EncodedValue("Smith|John^Medical^Center");
fluent.PID[5][1].Set().EncodedValue("Name|With^Delimiters");
fluent.PID[5][1][1].Set().EncodedValue("Value|With^Special&Characters");
fluent.Path("PID.5.1").SetEncoded("Complex|Value^With~Delimiters");
```

### Performance Considerations

**Caching**:
- Accessors are cached per FluentMessage instance
- Reduces object allocation for repeated access
- Thread-safe caching implementation

**Lazy Evaluation**:
- Segments/fields/components created only when accessed
- Collections built on-demand
- Minimal memory overhead

**Direct API Usage**:
- FieldAccessor uses `Message.GetValue()` for reliability
- Mutations use direct FieldList manipulation for performance
- Encoding/decoding handled by existing Message API

### Test Coverage Strategy

**TDD Approach**:
- Tests written before implementation
- Edge cases covered (null values, missing segments, invalid indices)
- Both positive and negative test cases

**Coverage Targets**:
- Line coverage: >85%
- Branch coverage: >80%
- All public API methods tested
- Error conditions validated

**Test Organization**:
- Separate test classes per component
- Descriptive test method names
- Arrange-Act-Assert pattern
- Comprehensive edge case coverage

**Important Testing Notes**:
- **Always create new test files within existing test projects** - standalone test files are not automatically included in the test discovery
- Add new test classes to `/HL7lite.Test/` with proper namespace `HL7lite.Test.*`
- Use descriptive test class names ending in `Tests` (e.g. `FieldSetVsAddRepetitionTests`)
- Test files must be in the same project structure to be discovered by `dotnet test`

### Integration Points

**Legacy API Compatibility**:
- All existing `Message` methods work unchanged
- `GetValue()`, `SetValue()`, `PutValue()` preserved
- Field/Component/Segment classes unchanged
- No breaking changes to public API

**Fluent API Extensions**:
- `ToFluentMessage()` extension method
- `GetAck()` / `GetNack()` fluent wrappers
- `RemoveTrailingDelimiters()` utility
- Deep copy functionality

### Common Usage Patterns

**Reading Values**:
```csharp
var fluent = message.ToFluentMessage();
string patientId = fluent.PID[3].Value;
string lastName = fluent.PID[5][1].Value;
string suffix = fluent.PID[5][1][2].Value ?? "";
```

**Setting Values**:
```csharp
fluent.PID[3].Set().Value("12345");
fluent.PID[5].Set().Components("Smith", "John", "M");
fluent.PID[5][1][2].Set().Value("Jr");
```

**Method Chaining**:
```csharp
fluent.PID[5].Set()
    .Components("Smith", "John")
    .Field(7, "19850315")
    .Field(8, "M");
```

**LINQ Operations**:
```csharp
var diagnoses = fluent.Segments("DG1")
    .Where(dg1 => dg1[3][1].Value.Contains("Diabetes"))
    .Select(dg1 => dg1[3][1].Value)
    .ToList();
```

**Path-based Access**:
```csharp
fluent.Path("PID.5.1").Set("Smith");
fluent.Path("PID.5.2").Set("John");
string name = fluent.Path("PID.5").Value;
```

**Field Repetitions - Consistent API Pattern**:
```csharp
// CONSISTENT PATTERN: Use collection.Add() (matches Segments pattern)
fluent.PID[3].Repetitions.Add("MRN001");
fluent.PID[3].Repetitions.Add("ENC123");
fluent.PID[3].Repetitions.Add("SSN456");

// Chain operations on returned accessor
fluent.PID[3].Repetitions.Add("MRN001").Set().Components("MRN", "001", "HOSPITAL");
fluent.PID[3].Repetitions.Add("ENC123").Set().Components("ENC", "123", "VISIT");

// IMPORTANT: Set().Value() resets the entire field, losing all repetitions
fluent.PID[3].Repetitions.Add("FirstID");
fluent.PID[3].Repetitions.Add("SecondID"); // Now has 2 repetitions
fluent.PID[3].Set().Value("NewID"); // ❌ Resets field, loses all repetitions!

// CORRECT: Use only the collection approach for repetitions
fluent.PID[3].Repetitions.Add("MRN001");
fluent.PID[3].Repetitions.Add("ENC123");
fluent.PID[3].Repetitions.Add("SSN456");
```