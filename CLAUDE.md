# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Documentation Principles

- **Keep XML documentation concise**: Focus on what the method does, not extensive examples
- **Avoid verbose examples in XML comments**: Save detailed examples for README/documentation files
- **One-line summary when possible**: Most methods should have a brief single-line summary
- **Parameters only when non-obvious**: Don't document obvious parameters like "value" in SetValue(value)
- **IntelliSense-friendly**: Write for developers using autocomplete, not reading source files

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
**Segment**: Individual HL7 segments (MSH, PID, etc.)
**Field**: Individual data fields within segments (1-based indexing)
**Component**: Sub-parts of complex fields (1-based indexing)
**SubComponent**: Finest granularity level (1-based indexing)

## Fluent API Architecture

The Fluent API provides a modern, intuitive interface built on top of the existing Message API:

### Key Design Principles

1. **Non-breaking**: All existing Message API functionality preserved
2. **Safe-by-default**: Set() methods automatically encode HL7 delimiters
3. **Clear data distinction**: Raw property for HL7 data, ToString() for display
4. **Null-safe**: Invalid access returns empty values instead of exceptions
5. **Fluent**: Method chaining for intuitive operations
6. **1-based indexing**: Follows HL7 standard (fields, components, subcomponents)
7. **0-based collections**: LINQ-compatible collections use 0-based indexing
8. **Cached accessors**: Performance optimization through caching

### Fluent API Components

**FluentMessage**: Main entry point
**Accessors** (Read-only): SegmentAccessor, FieldAccessor, ComponentAccessor, SubComponentAccessor
**Mutators** (Write operations): FieldMutator, ComponentMutator, SubComponentMutator
**Collections** (LINQ support): SegmentCollection, FieldRepetitionCollection
**Special Features**: PathAccessor, MSHBuilder, automatic encoding support

### Error Handling Philosophy

**Accessors** (reading):
- Invalid access returns empty accessor
- `.Raw` returns raw HL7 data with structural delimiters and encoded characters
- `.ToString()` returns human-readable format (decoded with spaces replacing delimiters)
- `.Exists` checks if element actually exists
- Never throws exceptions for invalid access

**Mutators** (writing):
- `Set()` methods automatically encode HL7 delimiter characters for safety
- `SetRaw()` methods accept raw values with validation against invalid delimiters
- Create missing segments/fields/components automatically
- Validate indices (negative indices throw ArgumentException)
- Support method chaining

### Polymorphism and Inheritance Principle

**CRITICAL: Always use virtual/override for polymorphic behavior**

When creating derived classes that need to override base class behavior:
- **Base class**: Mark properties and methods as `virtual`
- **Derived class**: Use `override` keyword, NOT `new`

The `new` keyword creates a new member that hides the base member, breaking polymorphism.

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

The fluent API provides safe-by-default encoding and explicit raw value support:

**Standard Delimiters**:
- `|` (field separator) → `\F\`
- `^` (component separator) → `\S\`  
- `~` (repetition separator) → `\R\`
- `\` (escape character) → `\E\`
- `&` (subcomponent separator) → `\T\`

**Safe Encoding (Recommended)**:
```csharp
// Set() methods automatically encode delimiters for safety in real medical data scenarios

// Lab results with vertical bars in medical notation
fluent.OBX[5].Set("Glucose: 95 mg/dL | Reference: 70-100 mg/dL");  // | used as separator in results
fluent.OBX[7].Set("Normal | <70 = Low | >100 = High");              // | used in reference ranges
fluent.NTE[3].Set("BP: 120/80 | Pulse: 72 | Temp: 98.6°F");       // | separating vital signs

// Medication instructions with pipes
fluent.RXE[5].Set("Take 2 tablets by mouth | Max 8 per day");      // | in dosing instructions
fluent.NTE[3].Set("1 tab q4-6h prn | Do not exceed 6/24h");        // | in medication instructions

// Names with ampersands (business/partnership names)
fluent.PID[5].Set("Smith & Jones");                                 // & in law firm name
fluent.PID[5].Set("Johnson & Johnson Medical");                     // & in company name
fluent.PV1[3].Set("Bed & Breakfast Wing");                         // & in location name

// File paths with backslashes
fluent.OBX[5].Set("\\\\server\\share\\results\\2024\\patient_12345.pdf");  // Network path
fluent.OBX[5].Set("C:\\PatientData\\Images\\scan_001.jpg");               // Windows file path

// URLs with ampersands in query strings
fluent.OBX[5].Set("https://lab.hospital.com/results?pid=12345&type=CBC&urgent=true");
fluent.NTE[3].Set("https://portal.health.org/referral?id=789&patient=12345&provider=567");

// Tilde in version numbers or ranges
fluent.MSH[12].Set("2.5.1~2.8");                                   // HL7 version range
fluent.OBX[5].Set("Result pending ~ 24-48 hours");                 // ~ as approximation symbol
```

**Raw Value Handling (Advanced)**:
```csharp
// SetRaw() methods for pre-structured HL7 data with validation
fluent.PID[5].SetRaw("Smith^John^M");           // Direct HL7 structure
fluent.PID[5][1].SetRaw("Smith");               // Component data (no ^ or | allowed)
fluent.PID[5][1][1].SetRaw("Smith");            // SubComponent data (no delimiters allowed)
fluent.Path("PID.5.1").SetRaw("Smith");         // Path-based raw setting

// SetRaw() validates against invalid delimiters for the hierarchy level:
fluent.PID[5].SetRaw("Value|Invalid");          // ❌ Throws: field cannot contain |
fluent.PID[5][1].SetRaw("Value^Invalid");       // ❌ Throws: component cannot contain ^ or |
fluent.PID[5][1][1].SetRaw("Value&Invalid");    // ❌ Throws: subcomponent cannot contain any delimiters
```

### Common Usage Patterns

**Reading Values**:
```csharp
var fluent = message.ToFluentMessage();
string patientId = fluent.PID[3].Raw;          // Raw HL7 data with structural delimiters
string lastName = fluent.PID[5][1].Raw;        // Raw component data
string suffix = fluent.PID[5][1][2].Raw ?? "";  // Raw subcomponent data
```

**Human-Readable Display**:
```csharp
// Use ToString() for display/logging purposes
string displayName = fluent.PID[5].ToString();         // "Smith John M" (decoded with spaces)
string displayId = fluent.PID[3].ToString();           // Clean display format
string displayNull = fluent.PID[6].ToString();         // "<null>" for HL7 nulls

// Comparison: Raw vs ToString()
fluent.PID[5].Raw;          // "Smith^John^M" (raw with structural delimiters)
fluent.PID[5].ToString();   // "Smith John M" (human-readable with spaces)

// With encoded delimiters in data (when using Set())
fluent.OBX[5].Set("Result: Positive | Confidence: 95%");
fluent.OBX[5].Raw;          // "Result: Positive \\F\\ Confidence: 95%" (raw encoded)  
fluent.OBX[5].ToString();   // "Result: Positive | Confidence: 95%" (decoded back to original)
```

**Setting Values (Safe Encoding)**:
```csharp
// Set() methods automatically encode HL7 delimiters (|, ^, ~, \, &)
fluent.PID[3].Set("12345");                    // Simple value
fluent.OBX[5].Set("Glucose: 95 | Normal: 70-100");   // | automatically encoded
fluent.PID[5].Set("Smith & Jones Law Firm");   // & automatically encoded
fluent.PID[11].Set("\\\\server\\patient\\records");  // \ automatically encoded

// For data that might contain user input with delimiters
string labResult = "Result: Positive | Confidence: 95%";
fluent.OBX[5].Set(labResult);                  // Safe - | encoded automatically

// Components assembled safely
fluent.PID[5].SetComponents("Smith & Jones", "John", "III");
```

**Setting Raw HL7 Structure (Advanced)**:
```csharp
// SetRaw() for pre-structured HL7 data
fluent.PID[5].SetRaw("Smith^John^M");          // Direct HL7 component structure
fluent.PID[3].SetRaw("ID001~ID002");           // Field repetitions with ~ delimiter
fluent.Path("PID.5").SetRaw("Smith^John^M^Jr^Dr"); // Path-based raw setting
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
    .Where(dg1 => dg1[3][1].Raw.Contains("Diabetes"))
    .Select(dg1 => dg1[3][1].Raw)
    .ToList();
```

**Path-based Access**:
```csharp
// Safe encoding
fluent.Path("OBX.5").Set("BP: 120/80 | Pulse: 72");    // | encoded automatically
fluent.Path("PID.5").Set("Smith & Jones");             // & encoded automatically
string result = fluent.Path("OBX.5").Raw;              // Raw HL7 data

// Raw structure
fluent.Path("PID.5").SetRaw("Smith^John^M");    // Direct HL7 structure
string displayName = fluent.Path("PID.5").ToString(); // "Smith John M" (human-readable)
```

**Field Repetitions - Fluent API Pattern**:
```csharp
// FLUENT PATTERN: AddRepetition maintains chain flow
fluent.PID[3].Set("FirstID")
    .AddRepetition("MRN001")                    // Adds repetition, returns mutator for new repetition
        .SetComponents("MRN", "001", "HOSPITAL") // Set components on new repetition
    .AddRepetition("ENC123")                    // Add another repetition
        .SetComponents("ENC", "123", "VISIT")    // Set components on this repetition
    .Field(7).Set("19850315");                  // Continue fluent chain to other fields

// IMPORTANT: Set() resets the entire field, losing all repetitions
fluent.PID[3].AddRepetition("FirstID");
fluent.PID[3].AddRepetition("SecondID");        // Now has 2 repetitions
fluent.PID[3].Set("NewID");                     // ❌ Resets field, loses all repetitions!

// CORRECT: Use AddRepetition to maintain existing repetitions
fluent.PID[3].Set("FirstID")
    .AddRepetition("SecondID")
    .AddRepetition("ThirdID");
```

### Testing Notes

**IMPORTANT Testing Notes**:
- **CRITICAL: Always create new test files within existing test projects** - standalone test files are not automatically included in the test discovery
- **DO NOT create separate .cs files for testing - they will not work with `dotnet test`**
- Add new test classes to `/HL7lite.Test/` with proper namespace `HL7lite.Test.*`
- Use descriptive test class names ending in `Tests` (e.g. `FieldSetVsAddRepetitionTests`)
- Test files must be in the same project structure to be discovered by `dotnet test`

## XML Documentation Guidelines
- Keep XML documentation comments concise and focused
- Avoid excessive examples in XML docs - one simple example is sufficient if needed
- Remove redundant "equivalent to verbose form" examples
- Focus on describing what the method does, not how to use it in detail