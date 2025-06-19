<div align="center">

# HL7lite

A lightweight, high-performance HL7 2.x parser for .NET with modern Fluent API support.

</div>

<p align="center">
  <a href="https://github.com/domibies/HL7lite/actions/workflows/dotnet.yml">
    <img src="https://github.com/domibies/HL7lite/actions/workflows/dotnet.yml/badge.svg?branch=master" alt=".NET">
  </a>
  <a href="https://codecov.io/gh/domibies/HL7lite">
    <img src="https://codecov.io/gh/domibies/HL7lite/graph/badge.svg" alt="codecov">
  </a>
  <a href="https://www.nuget.org/packages/HL7lite/">
    <img src="https://img.shields.io/nuget/v/HL7lite.svg" alt="NuGet">
  </a>
  <a href="https://www.nuget.org/packages/HL7lite/">
    <img src="https://img.shields.io/nuget/dt/HL7lite.svg" alt="NuGet Downloads">
  </a>
  <a href="https://github.com/domibies/HL7lite/blob/master/LICENSE.txt">
    <img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="License">
  </a>
</p>

## Table of Contents

- [Key Features](#key-features)
- [Installation](#installation)
- [Fluent API](#fluent-api)
- [Path API](#path-api)
- [Indexing Principles](#indexing-principles)
- [Migration Guide](#migration-guide)
- [Legacy API](#legacy-api)
- [What's New](#whats-new)
- [Contributing](#contributing)

## Key Features

- **High Performance** - Parse HL7 messages without schema validation overhead
- **Modern Fluent API** - Intuitive, chainable methods for message access and mutation
- **Path-based Access** - String-based path syntax wrapping legacy GetValue/SetValue/PutValue
- **Legacy Compatibility** - Full backward compatibility with existing code
- **Auto-creation** - Automatically create missing segments, fields, and components
- **Lightweight** - Minimal dependencies, small footprint
- **Comprehensive Testing** - High code coverage and production usage
- **.NET Standard** - Compatible with .NET Framework, .NET Core, and .NET 5+
- **Schema-free** - Works with any HL7 2.x message format

## Installation

### .NET CLI
```bash
dotnet add package HL7lite
```

### Package Manager
```powershell
Install-Package HL7lite
```

### PackageReference
```xml
<PackageReference Include="HL7lite" Version="1.2.0" />
```

## Fluent API

HL7lite provides a modern Fluent API for intuitive HL7 message manipulation. The Fluent API offers type-safe, chainable methods for accessing and modifying HL7 messages while maintaining full compatibility with the legacy API.

### Basic Usage

```csharp
using HL7lite;
using HL7lite.Fluent;

// Parse an HL7 message
var message = new Message(hl7String);
message.ParseMessage();

// Create fluent wrapper
var fluent = new FluentMessage(message);
```

### Data Access Patterns

#### Field Access
```csharp
// Get field values
string patientId = fluent.PID[3].Value;
string familyName = fluent.PID[5][1].Value;
string givenName = fluent.PID[5][2].Value;
string middleName = fluent.PID[5][3].Value;

// Safe access with null handling
string dateOfBirth = fluent.PID[7].SafeValue; // Never returns null
bool hasValue = fluent.PID[8].HasValue;
bool isNull = fluent.PID[8].IsNull; // true for HL7 "" values
```

#### Component and Subcomponent Access
```csharp
// Access nested components
string addressLine1 = fluent.PID[11][1][1].Value;
string city = fluent.PID[11][3].Value;
string zipCode = fluent.PID[11][5].Value;

// Check component existence
bool hasMiddleName = fluent.PID[5][3].Exists;
```

#### Field Repetitions
```csharp
// Access field repetitions
var patientIds = fluent.PID[3].Repetitions;
string firstId = patientIds[0].Value;
string secondId = patientIds[1].Value;

// Get repetition count
int idCount = fluent.PID[3].RepetitionCount;
bool hasMultipleIds = fluent.PID[3].HasRepetitions;

// Access specific repetition
string thirdId = fluent.PID[3].Repetition(3).Value; // 1-based
```

#### Multiple Segments
```csharp
// Check for multiple segments
bool hasMultipleDiagnoses = fluent.DG1.HasMultiple;
int diagnosisCount = fluent.DG1.Count;

// Access specific instance
string firstDiagnosis = fluent.DG1.Instance(1)[3][1].Value; // 1-based

// Use segment collections with LINQ
var diagnoses = fluent.Segments("DG1");
var severeDiagnoses = diagnoses
    .Where(d => d[4].Value.Contains("Severe"))
    .Select(d => d[3][1].Value)
    .ToList();
```

### Mutation Patterns

#### Field Mutations
```csharp
// Set field values
fluent.PID[5].Set().Value("Smith^John^Michael");
fluent.PID[7].Set().Value("19850315");

// Set individual components
fluent.PID[5].Set().Components("Smith", "John", "Michael");

// Clear fields
fluent.PID[8].Set().Clear();
fluent.PID[9].Set().Null(); // Sets HL7 null ("")
```

#### Component Mutations
```csharp
// Set component values
fluent.PID[5][1].Set().Value("Johnson");
fluent.PID[5][2].Set().Value("Robert");

// Set subcomponents
fluent.PID[11][1].Set().SubComponents("123 Main St", "Apt 4B");
```

#### Conditional Mutations
```csharp
// Conditional updates
fluent.PID[5].Set().ValueIf("Unknown^Patient", string.IsNullOrEmpty(currentName));

// Method chaining
fluent.PID[5].Set()
    .Clear()
    .Components("Brown", "Alice")
    .ValueIf("Default^Name", someCondition);

// Multi-field setting in one chain
fluent.PID[1].Set()
    .Value("1")
    .Field(5, "Smith^John^M")
    .Field(7, "19850315")
    .Field(8, "M");
```

#### Collection Mutations
```csharp
// Add field repetitions
fluent.PID[3].Set().AddRepetition("ALT123456");

// Remove repetitions
fluent.PID[3].Repetitions.RemoveRepetition(1); // Remove first repetition
fluent.PID[3].Repetitions.RemoveRepetition(2); // Remove second repetition
fluent.PID[3].Repetitions.Clear(); // Remove all

// Segment operations
var diagnosisSegments = fluent.Segments("DG1");
diagnosisSegments.Add(); // Add new DG1 segment
diagnosisSegments.RemoveSegment(1); // Remove first segment (1-based)

// Add multiple segments with field values
var obs1 = fluent.Segments("OBX").Add();
obs1[1].Set()
    .Value("1")
    .Field(2, "NM")
    .Field(3, "GLUCOSE")
    .Field(5, "120");

var obs2 = fluent.Segments("OBX").Add();
obs2[1].Set()
    .Value("2")
    .Field(2, "ST")
    .Field(3, "COMMENTS")
    .Field(5, "Normal range");
```

### Advanced Features

#### Message Creation and Building
```csharp
// One-step parsing from string
var fluent = hl7String.ToFluentMessage();

// Fluent MSH creation with intelligent defaults
var message = new FluentMessage(new Message());
message.CreateMSH
    .Sender("MyApp", "MyFacility")
    .Receiver("TheirApp", "TheirFacility")
    .MessageType("ADT^A01")
    .AutoControlId()      // Generates unique control ID
    .Production()         // Sets processing ID to "P"
    .Build();

// Environment-specific builders
message.CreateMSH
    .Sender("TestApp", "TestFac")
    .Receiver("TargetApp", "TargetFac")
    .MessageType("ORU^R01")
    .ControlId("12345")
    .Test()               // Sets processing ID to "T"
    .Build();
```

#### Deep Copy and Message Manipulation
```csharp
// Create independent copy of entire message
var copy = fluent.Copy();
copy.PID[5].Set().Value("NewName"); // Original unchanged

// ACK/NACK generation
var ack = fluent.GetAck();
var nack = fluent.GetNack("AR", "Invalid patient ID");

// Message cleanup
fluent.RemoveTrailingDelimiters();
fluent.RemoveTrailingDelimiters(MessageElement.RemoveDelimitersOptions.Fields);
```

#### DateTime Utilities
```csharp
// Set HL7 datetime fields
fluent.OBX[14].Set().DateTime(DateTime.Now);
fluent.OBX[14].Set().DateTimeNow();
fluent.MSH[7].Set().Date(DateTime.Today);
fluent.MSH[7].Set().DateToday();

// Parse HL7 datetime fields
DateTime? timestamp = fluent.OBX[14].AsDateTime();
DateTime? dateOnly = fluent.MSH[7].AsDate();

// Parse with timezone information
TimeSpan offset;
DateTime? timestampWithTz = fluent.OBX[14].AsDateTime(out offset);
```

#### Null Safety
```csharp
// Non-existent fields return empty values, no exceptions
string nonExistent = fluent["ZZZ"][999].Value; // Returns ""
string safeValue = fluent["ZZZ"][999].SafeValue; // Returns ""

// HL7 null handling
if (fluent.PID[8].IsNull) {
    // Field contains HL7 null ("")
    var value = fluent.PID[8].Value; // Returns null
}
```

#### Dynamic Segment Access
```csharp
// Access segments by name
var customSegment = fluent["ZIC"];
string customValue = fluent["ZIC"][3][1].Value;

// Generic field access
string anyField = fluent[segmentName][fieldIndex].Value;
```

#### LINQ Integration
```csharp
// Complex queries with LINQ
var criticalDiagnoses = fluent.Segments("DG1")
    .Where(diag => diag[4].Value.Contains("Critical"))
    .Select(diag => new {
        Code = diag[3][1].Value,
        Description = diag[3][2].Value,
        Severity = diag[4].Value
    })
    .ToList();

// Filter repetitions
var validPatientIds = fluent.PID[3].Repetitions
    .Where(id => !string.IsNullOrEmpty(id.Value))
    .Select(id => id.Value)
    .ToList();
```

## Path API

HL7lite provides a powerful path-based API that wraps the legacy `GetValue`/`SetValue`/`PutValue` methods with a modern, fluent interface. The Path API supports all existing HL7 path syntax while providing a foundation for future enhanced path features.

### Basic Path Syntax

The Path API supports the complete legacy path syntax:

```csharp
var fluent = new FluentMessage(message);

// Basic field access
string patientId = fluent.Path("PID.3").Value;
string familyName = fluent.Path("PID.5.1").Value;
string givenName = fluent.Path("PID.5.2").Value;

// Component and subcomponent access
string messageType = fluent.Path("MSH.9.1").Value;    // ADT
string triggerEvent = fluent.Path("MSH.9.2").Value;   // A01

// Field repetitions
string firstId = fluent.Path("PID.3[1]").Value;       // First patient ID
string secondId = fluent.Path("PID.3[2]").Value;      // Second patient ID

// Complex repetition paths
string doctorId = fluent.Path("PV1.7[1].1").Value;    // First attending doctor ID
string doctorName = fluent.Path("PV1.7[2].3").Value;  // Second attending doctor name
```

### Path Element Properties

Each path accessor provides comprehensive information about the element:

```csharp
var pathAccessor = fluent.Path("PID.5.1");

// Value access
string value = pathAccessor.Value;        // Get the value
bool exists = pathAccessor.Exists;        // Check if element exists
bool hasValue = pathAccessor.HasValue;    // Check if has non-empty value
bool isNull = pathAccessor.IsNull;        // Check for HL7 null ("")

// Path information
string pathString = pathAccessor.ToString(); // Returns "PID.5.1"
```

### Path Mutation Methods

The Path API provides two distinct mutation approaches that preserve the exact behavior of the legacy API:

#### Set() - Update Existing Elements Only
```csharp
// Set() wraps SetValue() - throws exception if path doesn't exist
fluent.Path("PID.5.1").Set("NewValue");    // Updates existing field
fluent.Path("PID.99").Set("Value");        // ❌ Throws HL7Exception

// Conditional updates
fluent.Path("PID.5.1").SetIf("ConditionalValue", someCondition);

// HL7 null values
fluent.Path("PID.5.1").SetNull();          // Sets HL7 null ("")
```

#### Put() - Create or Update Elements
```csharp
// Put() wraps PutValue() - creates missing elements automatically
fluent.Path("PID.99").Put("NewValue");     // ✅ Creates field and sets value
fluent.Path("ZZ1.2.3").Put("CustomValue"); // Creates entire path structure

// Conditional creation/updates
fluent.Path("PID.100").PutIf("ConditionalValue", someCondition);

// HL7 null values with creation
fluent.Path("PID.101").PutNull();          // Creates field with HL7 null
```

### Method Chaining

All path operations return the `FluentMessage` for seamless chaining:

```csharp
// Chain multiple path operations
fluent.Path("PID.5.1").Set("Smith")
      .Path("PID.5.2").Set("John")
      .Path("PID.7").Put("19850315")
      .Path("MSH.10").Set("12345");

// Mix path operations with other fluent methods
fluent.Path("PID.5.1").Set("Johnson")
      .PID[8].Set().Value("M")
      .Path("PV1.2").Put("I");
```

### Error Handling

The Path API preserves the exact error handling behavior of the legacy API:

```csharp
// Non-existent paths return empty values (no exceptions)
string nonExistent = fluent.Path("ZZZ.999").Value;     // Returns ""
bool exists = fluent.Path("ZZZ.999").Exists;           // Returns false

// Set() throws exceptions for non-existent paths (like SetValue)
try {
    fluent.Path("PID.999").Set("Value");
} catch (HL7Exception ex) {
    // "Field not available - PID.999 Error: ..."
}

// Put() never throws for valid paths (like PutValue)
fluent.Path("PID.999").Put("Value");  // Always succeeds
```

### Path vs Fluent API Comparison

Both approaches provide equivalent functionality - choose based on your preference:

```csharp
// Path API - String-based paths
string name = fluent.Path("PID.5.1").Value;
fluent.Path("PID.5.1").Set("Smith");

// Fluent API - Indexed access
string name = fluent.PID[5][1].Value;
fluent.PID[5][1].Set().Value("Smith");

// Path API - Repetitions
string firstId = fluent.Path("PID.3[1]").Value;
fluent.Path("PID.3[2]").Put("NewId");

// Fluent API - Repetitions
string firstId = fluent.PID[3].Repetition(1).Value;
fluent.PID[3].Repetitions.Add("NewId");
```

### Legacy API Compatibility

The Path API is a pure wrapper - it calls the exact same underlying methods:

```csharp
// These are equivalent:
message.GetValue("PID.5.1");           ↔ fluent.Path("PID.5.1").Value;
message.SetValue("PID.5.1", "Smith");  ↔ fluent.Path("PID.5.1").Set("Smith");
message.PutValue("PID.99", "Value");   ↔ fluent.Path("PID.99").Put("Value");
message.ValueExists("PID.5.1");       ↔ fluent.Path("PID.5.1").Exists;
```

## Indexing Principles

HL7lite follows consistent indexing conventions throughout the API:

### 1-Based Indexing (HL7 Standard)
All HL7 element access and manipulation methods use 1-based indexing to align with HL7 standards:

- **Fields**: `PID[3]` accesses the third field
- **Components**: `PID[5][1]` accesses the first component  
- **Repetitions**: `.Repetition(1)` accesses the first repetition
- **Segments**: `.Instance(1)` accesses the first segment occurrence
- **Path Notation**: `"PID.5.1"` uses 1-based indices throughout

### Collection Access (0-Based for LINQ)
Collections themselves use 0-based indexing to maintain compatibility with LINQ and standard .NET conventions:

```csharp
// Direct collection access is 0-based (for LINQ compatibility)
var segments = fluent.Segments("DG1");
var firstSegment = segments[0];                  // 0-based indexer
var filtered = segments.Where((s, i) => i > 0); // 0-based LINQ

// But all methods use 1-based numbering
segments.RemoveSegment(1);                       // Removes first segment
fluent.PID[3].Repetitions.RemoveRepetition(1);  // Removes first repetition
```

### Key Points
- **Methods are 1-based**: `RemoveSegment(1)`, `RemoveRepetition(1)`, `Instance(1)`, `Repetition(1)`
- **Collection indexers are 0-based**: `collection[0]` for LINQ compatibility
- **No 0-based methods**: We don't provide `RemoveAt(0)` to avoid confusion
- **Path strings are 1-based**: `"PID.3"` refers to the third field

This design ensures HL7 standard compliance while maintaining natural LINQ integration.

## Migration Guide

The Fluent API is designed for easy adoption alongside existing code. You can migrate incrementally without breaking changes.

### Getting Started

```csharp
// Existing code continues to work
var message = new Message(hl7String);
message.ParseMessage();

// Add fluent wrapper when ready
var fluent = new FluentMessage(message);
```

### Field Access Migration

**Before (Legacy API):**
```csharp
// Verbose field access
string patientId = message.GetValue("PID.3");
string familyName = message.GetValue("PID.5.1");
string givenName = message.GetValue("PID.5.2");

// Manual null checking
string dateOfBirth = message.GetValue("PID.7");
if (string.IsNullOrEmpty(dateOfBirth)) {
    dateOfBirth = "Unknown";
}

// Complex repetition handling
bool hasReps = message.HasRepetitions("PID.3");
if (hasReps) {
    var field = message.DefaultSegment("PID").Fields(3);
    var reps = field.Repetitions();
    string secondId = reps.Count > 1 ? reps[1].Value : "";
}
```

**After (Fluent API):**
```csharp
var fluent = new FluentMessage(message);

// Clean, intuitive access
string patientId = fluent.PID[3].Value;
string familyName = fluent.PID[5][1].Value;
string givenName = fluent.PID[5][2].Value;

// Built-in null safety
string dateOfBirth = fluent.PID[7].SafeValue;
if (string.IsNullOrEmpty(dateOfBirth)) {
    dateOfBirth = "Unknown";
}

// Simple repetition access
string secondId = fluent.PID[3].Repetitions.Count > 1 
    ? fluent.PID[3].Repetitions[1].Value 
    : "";
```

### Field Updates Migration

**Before (Legacy API):**
```csharp
// Basic field updates
message.SetValue("PID.5.1", "Smith");
message.SetValue("PID.5.2", "John");

// Auto-creation requires PutValue
if (!message.ValueExists("ZZ1.2.3")) {
    message.PutValue("ZZ1.2.3", "CustomValue");
}

// Manual component building
message.SetValue("PID.5", "Smith^John^Michael");
```

**After (Fluent API):**
```csharp
var fluent = new FluentMessage(message);

// Chainable mutations
fluent.PID[5][1].Set().Value("Smith");
fluent.PID[5][2].Set().Value("John");

// Auto-creation built-in
fluent["ZZ1"][2][3].Set().Value("CustomValue");

// Component helpers
fluent.PID[5].Set().Components("Smith", "John", "Michael");
```

### Segment Operations Migration

**Before (Legacy API):**
```csharp
// Check for multiple segments
var segments = message.Segments("DG1");
bool hasMultiple = segments.Count > 1;

// Access specific segment
if (segments.Count > 0) {
    string diagnosis = segments[0].Fields(3).Components(1).Value;
}

// Add new segment
var newSeg = new Segment("DG1", message.Encoding);
newSeg.AddNewField("1", 1);
newSeg.AddNewField("Primary", 3);
message.AddNewSegment(newSeg);
```

**After (Fluent API):**
```csharp
var fluent = new FluentMessage(message);

// Intuitive segment checking
bool hasMultiple = fluent.DG1.HasMultiple;

// Clean segment access
string diagnosis = fluent.DG1[3][1].Value;

// Simple segment addition
fluent.Segments("DG1").Add()[1].Set().Value("1");
fluent.Segments("DG1").Last()[3].Set().Value("Primary");
```

### LINQ Integration Migration

**Before (Legacy API):**
```csharp
// Manual iteration and filtering
var diagnosisCodes = new List<string>();
var dg1Segments = message.Segments("DG1");
foreach (var segment in dg1Segments) {
    string severity = segment.Fields(4).Value;
    if (severity.Contains("Critical")) {
        string code = segment.Fields(3).Components(1).Value;
        if (!string.IsNullOrEmpty(code)) {
            diagnosisCodes.Add(code);
        }
    }
}
```

**After (Fluent API):**
```csharp
var fluent = new FluentMessage(message);

// LINQ-enabled operations
var diagnosisCodes = fluent.Segments("DG1")
    .Where(d => d[4].Value.Contains("Critical"))
    .Select(d => d[3][1].Value)
    .Where(code => !string.IsNullOrEmpty(code))
    .ToList();
```

### Incremental Migration Strategy

1. **Start Small**: Wrap existing `Message` instances with `FluentMessage`
2. **Focus on Reads**: Migrate data access code first (lowest risk)
3. **Add Mutations**: Replace `SetValue`/`PutValue` calls with fluent mutations
4. **Leverage LINQ**: Replace manual loops with LINQ operations
5. **Keep Legacy**: Maintain legacy API usage where fluent doesn't add value

```csharp
// Mixed usage example
public void ProcessMessage(string hl7String) {
    // Parse with legacy API
    var message = new Message(hl7String);
    message.ParseMessage();
    
    // Use fluent for complex operations
    var fluent = new FluentMessage(message);
    
    // LINQ for data extraction
    var diagnoses = fluent.Segments("DG1")
        .Select(d => d[3][1].Value)
        .ToList();
    
    // Legacy for compatibility
    string sendingApp = message.GetValue("MSH.3");
    
    // Fluent for updates
    fluent.PID[5].Set().Components("Updated", "Name");
}
```

## Legacy API

The legacy API remains fully supported and continues to work exactly as before. This section documents the original API for reference and backward compatibility.

### Message Construction

```csharp
// Create a new message
var message = new Message();
message.AddSegmentMSH("LAB400", "LAB", 
                      "EPD", "NEUROLOGY",
                      "", "ADT^A01", 
                      "84768948", "P", "2.3");

// Parse existing message
Message message = new Message(strMsg);
try {
    message.ParseMessage();
} catch(HL7Exception ex) {
    // Handle parse errors
}

// Extract from MLLP frame
var messages = MessageHelper.ExtractMessages(mlllpBuffer);
foreach (var strMsg in messages) {
    var message = new Message(strMsg);
    message.ParseMessage();
}
```

### Data Access

```csharp
// Get field values
string sendingFacility = message.GetValue("MSH.4");
sendingFacility = message.DefaultSegment("MSH").Fields(4).Value;
sendingFacility = message.Segments("MSH")[0].Fields(4).Value;

// Work with repeating fields
bool hasRepetitions = message.HasRepetitions("PID.3");
List<Field> patientIds = message.Segments("PID")[0].Fields(3).Repetitions();
string secondId = message.GetValue("PID.3[2]");

// Handle components
string familyName = message.GetValue("PID.5.1");
string givenName = message.GetValue("PID.5.2");
bool isComponentized = message.IsComponentized("PID.5");
```

### Message Modification

```csharp
// Update values
message.SetValue("PV1.2", "I");
message.SetValue("PID.5.1", "SMITH");

// Create missing elements automatically
message.PutValue("ZZ1.2.4", "SYSTEM59");

// Check existence before updating
if (message.ValueExists("ZZ1.2"))
    message.PutValue("ZZ1.2.4", "SYSTEM59");

// Add new segments
var newSegment = new Segment("ZIM", message.Encoding);
newSegment.AddNewField("1.57884", 3);
newSegment.Fields(3).AddNewComponent(new Component("MM", message.Encoding), 2);
message.AddNewSegment(segment);

// Remove segments
message.RemoveSegment("NK1");
message.RemoveSegment("NK1", 1); // Remove specific occurrence (0-based)

// Clean up messages
message.RemoveTrailingDelimiters(RemoveDelimitersOptions.All);
```

### ACK/NACK Generation

```csharp
// Generate ACK
Message ack = message.GetACK();

// Generate NACK with error
Message nack = message.GetNACK("AR", "Invalid patient ID");

// Customize ACK fields
ack.SetValue("MSH.3", "MyApplication");
ack.SetValue("MSH.4", "MyFacility");
```

### Advanced Features

```csharp
// Encoded content
var obx = new Segment("OBX", new HL7Encoding());
obx.AddNewField(obx.Encoding.Encode("domain.com/resource.html?Action=1&ID=2"));

// Deep copy segments
Segment pidCopy = originalMessage.DefaultSegment("PID").DeepCopy();
newMessage.AddNewSegment(pidCopy);

// Date handling
string hl7DateTime = "20151231234500.1234+2358";
TimeSpan offset;
DateTime? dt = MessageHelper.ParseDateTime(hl7DateTime, out offset);
DateTime? dt2 = MessageHelper.ParseDateTime("20151231234500");

// Null elements are represented as ""
var nullValue = message.GetValue("EVN.4"); // Returns null if field contains ""
```

## What's New

### v1.3.0 (December 2024)
- **New Fluent API** - Modern, chainable interface for HL7 message manipulation
- **Path API** - String-based path access wrapping legacy GetValue/SetValue/PutValue methods
- **LINQ Support** - Query segments and repetitions using LINQ expressions
- **Type-safe Access** - Fluent accessors with built-in null safety
- **Collection Operations** - Advanced mutation methods for repetitions and segments
- **MSH Builder** - Fluent MSH creation with grouped parameters and auto-generation
- **DateTime Utilities** - HL7-specific datetime parsing and formatting methods
- **Deep Copy** - Create independent copies of entire messages
- **ACK/NACK Generation** - Fluent wrappers for acknowledgment message creation
- **Message Cleanup** - RemoveTrailingDelimiters support in fluent API
- **One-step Parsing** - String extension method for direct fluent message creation
- **Bug Fixes** - Fixed field repetition value preservation issue
- **Full Compatibility** - Legacy API remains unchanged and fully supported

### v1.2.0 (July 2024)
- Optional validation skip in ParseMessage
- Improved error handling with proper HL7Exceptions
- Parameterless AddSegmentMSH() for minimal segments
- Updated to latest .NET test frameworks

### v1.1.6 (July 2022)
- Fixed GetValue() exception for removed segments

### v1.1.5 (November 2021)
- Support for custom segment names ending with '0' (e.g., 'ZZ0')

<details>
<summary><b>Older Versions</b></summary>

### v1.1.3 (November 2021)
- Fixed HasRepetitions() method

### v1.1.2 (May 2021)
- Added RemoveTrailingDelimiters() functionality

### v1.1.1 (April 2021)
- Added PutValue() for auto-creating elements
- Added ValueExists() for checking element existence
- Added Ensure methods for element creation
- Added SwapFields() for field reordering
</details>

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

## Credits

This library is based on a fork of [HL7-dotnetcore](https://github.com/Efferent-Health/HL7-dotnetcore), which itself evolved from Jayant Singh's original HL7 parser.

### Original Projects
- [HL7-dotnetcore](https://github.com/Efferent-Health/HL7-dotnetcore)
- [hl7-cSharp-parser](https://github.com/j4jayant/hl7-cSharp-parser)
- [Original article](http://j4jayant.com/articles/hl7/31-hl7-parsing-lib)

Field encoding/decoding methods based on [hl7inspector](https://github.com/elomagic/hl7inspector).

## Breaking Changes

### Since v1.0
- `ParseMessage()` now throws exceptions on failure instead of returning boolean
- Use `ParseMessage(false)` to skip serialization checks

### Since v2.9 (from upstream)
- MSH segment includes field separator at position 1 (per HL7 standard)
- All MSH field indices should be incremented by one
- Lowercase methods removed in favor of uppercase equivalents
- `GetValue()` now automatically decodes returned content