<h1 align="center">
  <img src="assets/hl7lite-logo.png" alt="HL7lite" width="200">
</h1>
<h3 align="center">Simple, Lightweight HL7 v2.x Parsing and Manipulation for .NET</h3>

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

## Key Features

### Modern Fluent API ![NEW](https://img.shields.io/badge/NEW-brightgreen?style=flat-square)
*New in v2.0.0-rc.1 - A complete reimplementation of the HL7 parsing experience*

- **Safe-by-Default Encoding** - Set() methods automatically encode HL7 delimiters, SetRaw() for structured data
- **Raw vs Display Separation** - Clear distinction between Raw (HL7 data) and ToString() (human-readable)
- **Fluent Navigation** - Navigate and modify data across all hierarchy levels with method chaining  
- **Safe Data Access** - No more null reference exceptions - missing elements return empty values  
- **Auto-creation** - Missing segments, fields, and components are created automatically when needed  
- **LINQ Collections** - Full LINQ support for segments, field repetitions, and segment groups  
- **Enhanced Path API** - Complete repetition support with intuitive `DG1[2].3[1].2` syntax  
- **Segment Repetitions** - Direct access to repeating segments like DG1, OBX with collection support  
- **Segment Groups** - Query consecutive segments of the same type separated by gaps  

### Core Engine
- **Lightning Fast** - Parses HL7 messages without the overhead of schema validation  
- **Lightweight** - Minimal dependencies keep your application lean  
- **Battle-tested** - Powers integrations at Belgium's largest hospital group ([ZAS](https://www.zas.be))  
- **Backwards Compatible** - Existing code using the 1.x API continues to work unchanged  
- **Smart Encoding** - Automatic handling and validation of HL7 delimiter characters  
- **Universal .NET** - Compatibility across .NET Framework, .NET Core, and .NET 5+
- **Bug Fixes** - Important [core API fixes](#core-hl7lite-fixes) for field repetitions and segment copying


## Quick Start

### Installation


> ⚠️ **PRE-RELEASE**: The fluent API (v2.0.0-rc.1) is in Release Candidate status and could benefit from real-world testing before the stable v2.0 release. 
> 
> **Use with caution in production** - while thoroughly tested, minor API refinements may occur based on community feedback.
> 
> **For maximum stability**, the legacy API (v1.x) remains rock-solid and fully supported with important [core fixes](#core-hl7lite-fixes).


```bash
# .NET CLI - Fluent API (RC), including legacy API
dotnet add package HL7lite --version 2.0.0-rc.1

# .NET CLI - Legacy API only (stable)
dotnet add package HL7lite

# Package Manager - Fluent API (RC), including legacy API
Install-Package HL7lite -Version 2.0.0-rc.1

# Package Manager - Legacy API only (stable)
Install-Package HL7lite

# PackageReference - Fluent API (RC), including legacy API
<PackageReference Include="HL7lite" Version="2.0.0-rc.1" />

# PackageReference - Legacy API only (stable)
<PackageReference Include="HL7lite" Version="1.*" />
```

### Getting Data

```csharp
using HL7lite;
using HL7lite.Fluent;

// Parse HL7 message (safe, returns result)
var result = hl7String.TryParse();
if (!result.IsSuccess)
{
    Console.WriteLine($"Parse failed: {result.ErrorMessage}");
    return;
}
var message = result.Message;

// Get patient information
string patientId = message.PID[3].ToString();      // Single field value
string lastName = message.PID[5][1].ToString();    // First component 
string firstName = message.PID[5][2].ToString();   // Second component 
string fullName = message.PID[5].ToString();       // Entire field: "Smith John M Jr"
DateTime? dateOfBirth = message.PID[7].AsDate();   // DateTime support

// .ToString() returns human-readable display format:
// - Structural delimiters (^) become spaces: "Smith John M"
// - Encoded delimiters (\T\) are decoded to literal characters: "Johnson & Associates"
// - Perfect for display, logging, and most business logic

// .Raw returns raw HL7 data (use when working with HL7 structure):
string structuralData = message.PID[5].Raw;        // "Smith^John^M^Jr" (with HL7 delimiters)
// - Use when copying between messages or parsing components manually
// - Contains structural delimiters and encoded characters
// - Equivalent to legacy API's message.GetValue("PID.5")

// Parse dates and timestamps
DateTime? birthDate = message.PID[7].AsDate();           // Parse "19850315" -> DateTime
DateTime? timestamp = message.EVN[2].AsDateTime();       // Parse "20240624143022" -> DateTime
DateTime? timestampWithTz = message.EVN[2].AsDateTime(out TimeSpan offset); // Include timezone

// Access with safe navigation - never throws
string gender = message.PID[8].ToString(); // Safe access to gender field
string missing = message.PID[99].ToString(); // Non-existing field returns empty string, doesn't throw

// Use path-based access
string ssn = message.Path("PID.19").ToString();
string phone = message.Path("PID.13[1].1").ToString();

// Check field repetitions
if (message.PID[3].HasRepetitions) {
    var ids = message.PID[3].Repetitions
        .Select(r => r[1].ToString()) // first component of field
        .ToList();
}

// Query multiple segments
var diagnoses = message.Segments("DG1")
    .Select(dg1 => new {
        Code = dg1[3][1].ToString(),
        Description = dg1[4].ToString(),
        Type = dg1[6].ToString()
    })
    .ToList();

// Work with segment groups (consecutive segments separated by gaps)
var diagnosisGroups = message.Segments("DG1").Groups();
foreach (var group in diagnosisGroups)
{
    Console.WriteLine($"Diagnosis group {group.Count} diagnoses:");
    var primaryDiagnosis = group.First[3][1].ToString();  
    var relatedDiagnoses = group.Skip(1)
        .Select(dg1 => dg1[3][1].ToString())
        .ToList();
        Console.WriteLine($"Primary Diagnosis: {primaryDiagnosis}");
    Console.WriteLine("Related Diagnoses:");
    foreach (var diagnosis in relatedDiagnoses)
    {
        Console.WriteLine($"- {diagnosis}");
    }        
}
```

### Data Manipulation with Navigation and Setters

HL7lite uses a **Navigation Pattern** that separates navigation from setting operations for clear intent. With Field(), Component and SubComponent() methods you can navigate down and up the hierarchy.

```csharp
// NAVIGATION: Navigate first, then set
message.PID[5].Set("Smith")              // Set current field
    .Field(7).Set("19850315")           // Navigate to field 7, then set
    .Field(8).Set("M")                  // Navigate to field 8, then set
    .Field(11).Component(3).Set("Springfield");  // Navigate to field 11, component 3, then set

// Cross-level navigation reads like natural language
message.PID[5][1][1].Set("LastName")     // Set current subcomponent
    .SubComponent(2).Set("FirstName")   // Navigate to subcomponent 2, then set
    .Component(2).Set("MiddleName")     // Navigate to component 2, then set
    .Field(7).Set("19850315");          // Navigate to field 7, then set

// Fluent creation of new segments
message.Segments("OBX").Add()
    .Field(1).Set("1")                  // Navigate to field 1, set sequence ID
    .Field(3).Component(1).Set("GLUCOSE")       // Navigate to observation identifier
    .Field(5).Set("95")                 // Navigate to observation value
    .Field(14).Component(1).Set("20240101120000");  // Navigate to timestamp
```

**Features:**
- **Full Navigation Matrix**: Navigate anywhere from any mutator type
- **Type Safety**: Return types clearly indicate current navigation context

```csharp
// Complete patient demographics in one powerful method chain
message.PID[3].Set("12345")
    .Field(5).SetComponents("Smith", "John", "M", "Jr", "Dr")
    .Field(7).SetDate(new DateTime(1985, 3, 15))
    .Field(8).Set("M")
    .Field(11).SetComponents("123 Main St", "Apt 4B", "Springfield", "IL", "62701", "USA")
    .Field(13).SetComponents("555", "123-4567")        // Home phone
    .Field(14).SetComponents("555", "098-7654");       // Business phone

// DateTime chaining for multiple timestamps
message.EVN[2].SetDateTime(DateTime.Now)          // Event occurred
    .Field(6).SetDateTime(DateTime.Now);               // Event entered

// Auto-creation: Set() never throws - creates missing elements automatically
message["ZZ1"][99][3].Set("CustomValue");  // Creates entire structure
message.Path("ZZ2.99.99").Set("Value");    // Creates segment, field, component

// Add field repetitions with fluent chaining
message.PID[3].Set("ID001")
    .AddRepetition("MRN001")              // Adds repetition, stays in fluent chain
    .AddRepetition("ENC123")              // Add another repetition
    .Field(5).SetComponents("Smith", "John");  // Continue with other fields

// Add a segment & add fields and components fluently
message.Segments("DG1").Add()
    .Field(1).Set("1")                  // Navigate to field 1, set ID
    .Field(3).SetComponents("I10", "250.00", "Diabetes mellitus type 2")  // Set components
    .Field(6).Set("F");                 // Navigate to field 6, set type

// Full navigation and setting with clear intent
message.Segments("OBX").Add()
    .Field(1).Set("1")                  // Navigate to field 1, set ID
    .Field(2).Set("ST")                 // Navigate to field 2, set value type
    .Field(3).SetComponents("GLUCOSE", "Glucose Level")  // Set observation components
    .Field(5).Set("95")                 // Navigate to field 5, set result
    .Field(6).Set("mg/dL")              // Navigate to field 6, set units
    .Field(7).Set("70-100")             // Navigate to field 7, set reference range
    .Field(8).Set("N")                  // Navigate to field 8, set abnormal flag
    .Field(14).Component(1).Set("20231215120000");  // Navigate to observation date/time

// Set subcomponents 
message.PID[11].SetComponents("123 Main St", "Apt 4B", "Springfield", "IL", "62701", "USA")
    .Component(1).SetSubComponents("123 Main St", "Building A", "Suite 100");
```

<details>
<summary><b>Message Construction and Copying</b></summary>

### Creating New Messages

```csharp
// Create a new message from scratch
var message = FluentMessage.Create();

// Build MSH segment fluently
message.CreateMSH
    .Sender("SENDING_APP", "FACILITY_A")
    .Receiver("RECEIVING_APP", "FACILITY_B")
    .MessageType("ADT^A01")
    .ControlId("12345")
    .ProcessingId("P")
    .Version("2.5")
    .Build();

// Or use convenient defaults
message.CreateMSH
    .Sender("APP", "FAC")
    .Receiver("DEST", "FAC2") 
    .MessageType("ORU^R01")
    .Production()              // Sets ProcessingId to "P" (production)
    .AutoControlId()           // Generates unique control ID automatically
    .Build();                  // MessageTime is automatically set to current timestamp

// Even simpler - minimal required fields only
message.CreateMSH
    .Sender("APP", "FAC")
    .Receiver("DEST", "FAC2")
    .MessageType("ADT^A08")
    .AutoControlId()           // Auto-generates unique ID like "20250623120000123"
    .Build();                  // Uses defaults: Version="2.5", ProcessingId="P", MessageTime=now

// Build complete patient segment with powerful method chaining
message.Segments("PID").Add()
    .Field(1).Set("1")
    .Field(3).Set("PAT001")
    .Field(5).SetComponents("Doe", "John", "Middle")
    .Field(7).Set("19800101")
    .Field(8).Set("M")
    .Field(11).SetComponents("456 Oak Ave", "", "Boston", "MA", "02101");

// Create admission info with method chaining
message.Segments("PV1").Add()
    .Field(1).Set("1")                         // Set ID
    .Field(2).Set("I")                         // Inpatient
    .Field(3).SetComponents("ICU", "001", "A")     // Location
    .Field(7).SetComponents("1234", "Smith", "John", "Dr")  // Attending doctor
    .Field(44).Set("20231215080000");          // Admit date/time
// Power of chaining: Complete ADT message in one flow
var message = FluentMessage.Create();
message.CreateMSH
    .Sender("HIS", "HOSPITAL")
    .Receiver("LAB", "LABORATORY")
    .MessageType("ADT^A01")
    .AutoControlId()
    .Build();

// Add multiple segments in single chained operations
message.Segments("EVN").Add()
    .Field(1).Set("A01")
    .Field(2).SetDateTime(DateTime.Now)
    .Field(6).SetDateTime(DateTime.Now);

message.Segments("PID").Add()
    .Field(1).Set("1")
    .Field(3).Set("MRN12345")
        .AddRepetition()
            .SetComponents("MRN", "12345", "HOSPITAL")
        .AddRepetition("SSN987654321")
    .Field(5).SetComponents("Smith", "John", "Michael", "Jr")
    .Field(7).SetDate(new DateTime(1985, 3, 15))
    .Field(8).Set("M")
    .Field(11).SetComponents("123 Main St", "Apt 4B", "Springfield", "IL", "62701", "USA")
    .Field(13).SetComponents("555", "123-4567")
    .Field(14).SetComponents("555", "098-7654");

message.Segments("NK1").Add()
    .Field(1).Set("1")
    .Field(2).SetComponents("Smith", "Jane", "Marie")
    .Field(3).SetComponents("SPO", "Spouse");

message.Segments("PV1").Add()
    .Field(1).Set("1")
    .Field(2).Set("I")
    .Field(3).SetComponents("ICU", "001", "A", "HOSPITAL")
    .Field(7).SetComponents("1234", "Johnson", "Robert", "Dr")
    .Field(10).Set("MED")
    .Field(19).Set("V" + DateTime.Now.Ticks)
    .Field(44).SetDateTime(DateTime.Now);
```

### Copying Messages and Segments

```csharp
// Deep copy entire message with fluent API
var result = hl7String.TryParse();
if (!result.IsSuccess) return;
var original = result.Message;
var copy = original.Copy();

// Modify the copy without affecting the original
copy.PID[3].Set("NEW_ID");
copy.PID[5].SetComponents("NewLastName", "NewFirstName");

// Copy specific segments between messages
var sourceResult = sourceHL7.TryParse();
if (!sourceResult.IsSuccess) return;
var source = sourceResult.Message;
var target = FluentMessage.Create();

// Copy all DG1 segments from source to target using fluent collections
foreach (var sourceDG1 in source.Segments("DG1")) {
    target.Segments("DG1").AddCopy(sourceDG1);
}

// Copy and modify a segment (if it exists)
if (source.Segments("PID").Any()) {
    var sourcePID = source.Segments("PID")[0];
    var pidAccessor = target.Segments("PID").AddCopy(sourcePID);
    pidAccessor[3].Set("MODIFIED_ID");
}

// Copy with selective field updates
if (source.Segments("OBX").Any()) {
    var sourceOBX = source.Segments("OBX")[0];
    var obxAccessor = target.Segments("OBX").AddCopy(sourceOBX);
    obxAccessor[5].Set("Updated Result");
    obxAccessor[14][1].SetDateTime(DateTime.Now);
}
```

</details>

<details>
<summary><b>Path API</b></summary>

The Path API provides string-based access to message elements with enhanced syntax supporting both segment and field repetitions.

**Important**: Path.Set() behaves like the other setters in the Fluent API - it never throws exceptions and creates missing elements automatically.

### Basic Path Syntax

```csharp
// Basic path access
string patientName = message.Path("PID.5.1").ToString();
message.Path("PID.5.1").Set("NewLastName");

// Full path syntax: SEGMENT[rep].FIELD[rep].COMPONENT.SUBCOMPONENT
// Repetition indexers [n] are optional and default to [1]
string value = message.Path("DG1[2].3.2").ToString();  // 2nd DG1 segment, field 3, component 2
```

### Repetition Support

```csharp
// Field repetitions - [n] is optional, defaults to [1]
string firstId = message.Path("PID.3").ToString();      // Same as PID.3[1]
string secondId = message.Path("PID.3[2]").ToString();  // Second repetition

// Segment repetitions - access multiple instances of the same segment
message.Path("DG1.3.1").Set("I10");        // First diagnosis (same as DG1[1].3.1)
message.Path("DG1[2].3.1").Set("E11.9");   // Second diagnosis segment
message.Path("DG1[3].3.1").Set("N18.3");   // Third diagnosis segment

// Combined segment and field repetitions
message.Path("OBX[2].5").Set("95");        // 2nd OBX, field 5 (first repetition)
message.Path("OBX[2].5[2]").Set("mg/dL");  // 2nd OBX, field 5 second repetition
```

### Complex Paths

```csharp
// Create complex paths with full addressing
message.Path("PV1.7.1").Set("1234");       // First attending doctor ID
message.Path("PV1.7.2").Set("Smith");      // Last name  
message.Path("PV1.7.3").Set("John");       // First name
message.Path("PV1.7[2].1").Set("5678");    // Second attending doctor ID

// Access deeply nested structures
message.Path("PID.11.1.1").Set("123 Main St");     // Street address subcomponent
message.Path("PID.11[1].1.2").Set("Apt 4B");       // Additional locator

// Alternative: Use structured data for clarity
message.PV1[7].SetComponents("1234", "Smith", "John", "Dr");
```

### Auto-Creation

```csharp
// Auto-creation: Set() never throws exceptions
message.Path("ZZ1.5.3").Set("CustomValue");      // Creates entire path if missing
message.Path("ZZ2[2].99.99").Set("Value");       // Creates 2nd instance of ZZ2 segment
message.Path("OBX[5].5[3].2").Set("Result");     // Creates OBX segments 1-5 if needed
```

### Conditional Operations

```csharp
// Check if path exists
bool hasAllergies = message.Path("AL1.3").Exists;
bool hasSecondDiagnosis = message.Path("DG1[2].3.1").HasValue;

// Conditional operations
message.Path("PID.6.1").SetIf("MAIDEN", patient.HasMaidenName);
message.Path("DG1[2].6").SetIf("F", diagnosis.IsFinal);

// Null handling
message.Path("PID.8").SetNull();  // Sets HL7 null value ("")
bool isNull = message.Path("PID.8").IsNull;  // Checks for HL7 null
```

### Enhanced Features vs Legacy API

The Path API in HL7lite extends the legacy GetValue/SetValue behavior with:
- **Segment repetition syntax**: `DG1[2]` accesses the 2nd DG1 segment (optional, defaults to [1])
- **Field repetition syntax**: `PID.3[2]` accesses the 2nd repetition of field 3 (optional, defaults to [1])
- **Auto-creation**: Missing segments are automatically created with required fields
- **Safe access**: Never throws exceptions for missing elements
- **Null safety**: Properly handles HL7 null values ("")

</details>

<details>
<summary><b>Encoding</b></summary>

HL7lite automatically handles encoding and decoding of delimiter characters to ensure message integrity.

### Safe-by-Default Encoding

The fluent API automatically encodes HL7 delimiter characters (`|`, `^`, `~`, `\`, `&`) to ensure message integrity. Set() methods encode by default, while SetRaw() methods work with pre-structured HL7 data:

```csharp
// Set() automatically encodes delimiter characters for safety
message.PID[5][1].Set("Smith & Jones");  // Automatically becomes "Smith \T\ Jones"
message.OBX[5].Set("Glucose: 95 mg/dL | Normal: 70-100");  // | safely encoded

// SetRaw() for pre-structured HL7 data with validation
message.PID[5].SetRaw("Smith^John^M");  // Direct HL7 structure

// Best practice: Use structured data for complex values
message.PID[5].SetComponents("Smith-Jones", "Mary");  // Clear component structure
```

### Real-World Examples

```csharp
// URLs with query parameters - Set() encodes automatically
message.OBX[5].Set("https://lab.hospital.com/results?id=123&type=CBC");

// Medical notes with special characters - safe by default
message.NTE[3].Set("Blood pressure: 120/80 | Temp: 98.6°F");

// Complex addresses using structured data (preferred)
message.PID[11].SetComponents("123 Main St", "Suite A&B", "Boston", "MA", "02101");
message.PID[11][2].SetSubComponents("Suite A&B", "Building 5", "East Wing");

// Lab results with ranges - use structured components when possible
message.OBX[5].SetComponents("95", "mg/dL");
message.OBX[7].Set("70-100");  // Reference range in separate field

// File paths - automatic encoding handles backslashes
message.OBX[5].Set("\\\\server\\lab\\results\\patient123.pdf");

// Working with pre-structured HL7 data
message.PID[5].SetRaw("Smith^John^M^Jr");  // Direct HL7 component structure
```

### Automatic Decoding

When reading values, encoded delimiters are automatically decoded:

```csharp
// Set value with delimiters (automatically encoded)
message.PID[5][1].Set("Smith & Jones");

// Read value - automatically decoded
string name = message.PID[5][1].ToString();  // Returns: "Smith & Jones" (human-readable)
string raw = message.PID[5][1].Raw;          // Returns: "Smith \T\ Jones" (encoded HL7 data)
```

</details>

<details>
<summary><b>Indexing</b></summary>

HL7lite uses consistent indexing conventions:

### HL7 Elements (1-based)
- **Fields**: `PID[3]` - Third field (per HL7 standard)
- **Components**: `PID[5][1]` - First component
- **Subcomponents**: `PID[5][1][2]` - Second subcomponent  
- **Path notation**: `"PID.5.1"` - All indices are 1-based

### Collections (0-based)
Collections use 0-based indexing for LINQ compatibility:

```csharp
// Segment collection access
var segments = message.Segments("DG1");
var firstDiagnosis = segments[0];          // 0-based for LINQ
var filtered = segments.Where((s, i) => i > 0);

// But methods remain 1-based
message.PID[3].Repetitions.RemoveRepetition(1);  // Removes first repetition
segments.RemoveSegment(1);                       // Removes first segment
```

</details>

<details>
<summary><b>Legacy API</b></summary>

The Pre-2.0 API remains fully supported with backward compatibility guaranteed. For complete legacy documentation, see [README.Legacy.md](README.Legacy.md).

### Basic Usage

```csharp
// Create and parse message
var message = new Message(hl7String);
message.ParseMessage();

// Get values
string patientId = message.GetValue("PID.3");
string lastName = message.GetValue("PID.5.1");

// Set values
message.SetValue("PID.3", "12345");
message.SetValue("PID.5.1", "Smith");

// Auto-create missing elements (but NOT segments)
message.PutValue("PID.99.3", "CustomValue");  // Creates field/component if PID exists

// Check existence
bool hasValue = message.ValueExists("PID.7");

// Add segments manually
var segment = new Segment("PV1", message.Encoding);
segment.AddNewField("1", 1);
segment.AddNewField("O", 2);
message.AddNewSegment(segment);
```

### Key Differences from Fluent API

The legacy API has several limitations compared to the Fluent API:
- **No segment auto-creation**: PutValue("ZZ1.2.3", "value") throws "Segment name not available" if ZZ1 doesn't exist
- **No segment repetition support in paths**: Cannot use `DG1[2].3` syntax
- **Limited field repetition access**: Cannot access field repetitions via path strings
- **Exception-based**: Missing elements often throw exceptions instead of returning empty values
- **Manual segment creation**: Must create segments manually before setting values

### Migration to Fluent API

The Fluent API wraps the legacy API, so you can migrate incrementally:

```csharp
// Legacy code continues to work
var legacyMessage = new Message(hl7String);
legacyMessage.ParseMessage();
string id = legacyMessage.GetValue("PID.3");

// Add fluent wrapper when convenient
var fluentWrapper = new FluentMessage(legacyMessage);
string name = fluentWrapper.PID[5][1].ToString();

// Both APIs work on the same message
legacyMessage.SetValue("PID.7", "19850315");
var dob = fluentWrapper.PID[7].ToString();  // "19850315"

// Fluent API auto-creates segments
fluentWrapper.Path("ZZ1.2.3").Set("value");  // Creates ZZ1 segment automatically
```

</details>

<details>
<summary><b>What's New</b></summary>

### v2.0.0-rc.1 (July 2025)
- **Pure Navigation API** - Crystal clear navigation with natural language-like syntax
- **Modern Fluent API** - Complete rewrite with intuitive, chainable interface
- **Enhanced Path API** - Full segment and field repetition support (`DG1[2].3[1].2` syntax)
- **Raw vs ToString()** - Clear separation between HL7 data (Raw) and display format (ToString())
- **Safe-by-Default Encoding** - Set() methods automatically encode delimiters, SetRaw() for structured data
- **Auto-creation** - Automatic segment creation for paths (e.g., `Path("ZZ1.5").Set()` creates ZZ1)
- **Enhanced Collections** - Full LINQ support for segments and repetitions
- **Full Compatibility** - Legacy API unchanged and fully supported
- **Core Bug Fixes** - Important [fixes](#core-hl7lite-fixes) for field repetitions, segment copying, and more

### Previous Versions
- v1.2.0 - Optional validation, improved error handling
- v1.1.x - Bug fixes and stability improvements
- v1.0.0 - Initial stable release

</details>

## Core HL7lite Fixes

Important bug fixes in v2.0.0 that benefit all users (including legacy API):

- **Field Repetition Fixes** - Fixed data loss in `RemoveRepetitions()` and state preservation in `AddRepetition()`
- **Segment Copy Fix** - `DeepCopy()` now properly copies segments with individually set fields  
- **Position Validation** - Added validation to prevent invalid field/component positions
- **Architecture** - Unified collections using generic `ElementCollection<T>` for consistency

For detailed information about these fixes, see [README.CoreFixes_v2.md](README.CoreFixes_v2.md).

## HL7lite API Reference

### FluentMessage

The main entry point for the fluent API. Wraps a legacy `Message` object.

```csharp
var message = FluentMessage.Create();        // Create empty message
var fluentWrapper = new FluentMessage(message);    // Wrap existing message
var result = hl7String.TryParse();         // Parse from string
```

**Static Methods:**
- `Create()` - Create new empty FluentMessage

**Properties:**
- `MSH`, `PID`, `PV1`, etc. - Direct segment accessors (37 common segments)
- `UnderlyingMessage` - Access to wrapped Message object
- `CreateMSH` - MSH segment builder

**Instance Methods:**
- `Segments(string code)` - Get collection of segments by code
- `Path(string path)` - Path-based accessor
- `Copy()` - Deep copy the message
- `GetAck()` / `GetNack(code, text)` - Generate acknowledgments
- `RemoveTrailingDelimiters()` - Clean up message
- `Serialize()` - Get serialization builder
- `this[string code]` - Segment accessor by code

### Accessors (Read Operations)

#### SegmentAccessor
Access to segment-level data.

```csharp
var pid = message.PID;
var custom = message["ZZ1"];
```

**Properties:**
- `Exists` - True if segment exists
- `Count` - Number of instances (for repeating segments)
- `HasMultiple` - True if segment repeats
- `IsSingle` - True if only one instance

**Methods:**
- `Field(int index)` / `this[int index]` - Get field accessor
- `Instance(int instance)` - Get specific segment instance

#### FieldAccessor
Access to field-level data with repetition support.

```csharp
var field = message.PID[3];
var component = message.PID[5][1];
```

**Properties:**
- `Raw` - Raw field value with structural delimiters and encoded delimiter characters ("" for HL7 nulls)
- `Exists` - True if field exists in message
- `HasValue` - True if field has actual data (false for empty, null, or HL7 null "")
- `IsNull` - True for HL7 null values ("")
- `IsEmpty` - True for empty or null
- `HasRepetitions` - True if field has repetitions
- `RepetitionCount` - Number of repetitions
- `Repetitions` - Collection of repetitions

**Methods:**
- `Component(int index)` / `this[int index]` - Get component accessor
- `Repetition(int index)` - Get specific repetition (1-based)
- `ToString()` - Get human-readable format (decoded, with spaces)
- `Set()` - Get field mutator
- `Set(string value)` - Set field value (automatically encodes delimiters) and return mutator
- `SetRaw(string value)` - Set raw HL7 value with validation and return mutator
- `SetComponents(params string[] values)` - Set multiple components and return mutator
- `SetNull()` - Set HL7 null value and return mutator
- `SetIf(string value, bool condition)` - Conditional set and return mutator
- `SetDate(DateTime date)` - Set date (YYYYMMDD) and return mutator
- `SetDateTime(DateTime dateTime)` - Set date/time (YYYYMMDDHHMMSS) and return mutator
- `AsDate(bool throwOnError = false)` - Parse as date
- `AsDateTime()` - Parse as date/time

#### ComponentAccessor
Access to component-level data.

```csharp
var component = message.PID[5][1];  // Last name
```

**Properties:**
- Same as FieldAccessor (Raw, Exists, etc.)

**Methods:**
- `SubComponent(int index)` / `this[int index]` - Get subcomponent
- `ToString()` - Get human-readable format (decoded, with spaces)
- `Set()` - Get component mutator
- `Set(string value)` - Set component value (automatically encodes delimiters) and return mutator
- `SetRaw(string value)` - Set raw HL7 value with validation and return mutator
- `SetSubComponents(params string[] values)` - Set multiple subcomponents and return mutator
- `SetNull()` - Set HL7 null value and return mutator
- `SetIf(string value, bool condition)` - Conditional set and return mutator

#### SubComponentAccessor
Access to subcomponent-level data.

```csharp
var subcomp = message.PID[5][1][2];
```

**Properties:**
- Same as FieldAccessor (Raw, Exists, etc.)

**Methods:**
- `ToString()` - Get human-readable format (decoded)
- `Set()` - Get subcomponent mutator
- `Set(string value)` - Set subcomponent value (automatically encodes delimiters) and return mutator
- `SetRaw(string value)` - Set raw HL7 value with validation and return mutator
- `SetNull()` - Set HL7 null value and return mutator
- `SetIf(string value, bool condition)` - Conditional set and return mutator

### Mutators (Write Operations)

All mutators support method chaining and auto-create missing elements.

#### FieldMutator
Modify field values with pure navigation pattern.

```csharp
message.PID[3].Set("12345")
    .Field(5).Set("Smith^John")        // Navigate to field 5, then set
    .Field(7).Set("19850315");         // Navigate to field 7, then set

// Add field repetitions fluently
message.PID[3].Set("FirstID")
    .AddRepetition("MRN001")           // Add repetition, stay in chain
    .AddRepetition()                   // Add empty repetition
        .SetComponents("ENC", "123", "VISIT") // Set components on new repetition
    .Field(7).Set("19850315");         // Continue with other fields
```

**Setting Methods:**
- `Set(string value)` - Set field value (automatically encodes delimiters)
- `SetRaw(string value)` - Set raw HL7 value with validation
- `SetNull()` - Set HL7 null value ("")
- `Clear()` - Clear field (empty string)
- `SetComponents(params string[] values)` - Set multiple components
- `SetIf(string value, bool condition)` - Conditional set
- `SetDate(DateTime date)` - Set date (YYYYMMDD)
- `SetDateTime(DateTime dateTime)` - Set date/time (YYYYMMDDHHMMSS)
- `AddRepetition(string value)` - Add field repetition with value
- `AddRepetition()` - Add empty field repetition for component setting

**Navigation Methods:**
- `Field(int index)` - Navigate to different field (returns FieldMutator)
- `Component(int index)` - Navigate to component (returns ComponentMutator)
- `SubComponent(int componentIndex, int subComponentIndex)` - Navigate to subcomponent (returns SubComponentMutator)

#### ComponentMutator
Modify component values with pure navigation pattern.

```csharp
message.PID[5][1].Set("Smith")
    .Component(2).Set("John")          // Navigate to component 2, then set
    .Field(7).Set("19850315");         // Navigate to field 7, then set
```

**Setting Methods:**
- `Set(string value)` - Set component value (automatically encodes delimiters)
- `SetRaw(string value)` - Set raw HL7 value with validation
- `SetNull()` - Set HL7 null value
- `Clear()` - Clear component
- `SetSubComponents(params string[] values)` - Set subcomponents
- `SetIf(string value, bool condition)` - Conditional set

**Navigation Methods:**
- `Field(int index)` - Navigate to different field (returns FieldMutator)
- `Component(int index)` - Navigate to different component (returns ComponentMutator)
- `SubComponent(int index)` - Navigate to subcomponent (returns SubComponentMutator)

#### SubComponentMutator
Modify subcomponent values with pure navigation pattern.

```csharp
message.PID[5][1][1].Set("Smith")
    .SubComponent(2).Set("Jr")         // Navigate to subcomponent 2, then set
    .Component(2).Set("John");         // Navigate to component 2, then set
```

**Setting Methods:**
- `Set(string value)` - Set subcomponent value (automatically encodes delimiters)
- `SetRaw(string value)` - Set raw HL7 value with validation
- `SetNull()` - Set HL7 null value
- `Clear()` - Clear subcomponent
- `SetIf(string value, bool condition)` - Conditional set

**Navigation Methods:**
- `Field(int index)` - Navigate to different field (returns FieldMutator)
- `Component(int index)` - Navigate to different component (returns ComponentMutator)
- `SubComponent(int index)` - Navigate to different subcomponent (returns SubComponentMutator)

### Collections

#### SegmentCollection
LINQ-compatible collection of segments.

```csharp
var diagnoses = message.Segments("DG1");
var primary = diagnoses[0];  // 0-based indexer
var count = diagnoses.Count;

// Access segment groups
var groups = diagnoses.Groups();  // Get consecutive segment groups
```

**Properties:**
- `Count` - Number of segments
- `HasGroups` - Whether segments have groups (gaps between them)
- `GroupCount` - Number of consecutive segment groups

**Methods:**
- `Add()` - Add new segment, returns accessor
- `AddCopy(Segment segment)` - Add copy of segment, returns accessor
- `AddCopy(SegmentAccessor segmentAccessor)` - Add copy from accessor, returns accessor
- `Clear()` - Remove all segments
- `Segment(int index)` - Get by 1-based index
- `RemoveSegment(int index)` - Remove by 1-based index
- `this[int index]` - Get by 0-based index
- `Groups()` - Get segment groups collection
- `Group(int groupNumber)` - Get specific group by 1-based index
- LINQ methods (Where, Select, etc.)

#### FieldRepetitionCollection
Collection of field repetitions.

```csharp
var ids = message.PID[3].Repetitions;
ids.Add("MRN001");
ids.Add("SSN123");
```

**Properties:**
- `Count` - Number of repetitions

**Methods:**
- `Add(string value)` - Add new repetition
- `Add()` - Add empty repetition
- `Clear()` - Remove all repetitions
- `Repetition(int index)` - Get by 1-based index
- `RemoveRepetition(int index)` - Remove by 1-based index
- `this[int index]` - Get by 0-based index
- LINQ methods

#### Segment Groups
Intelligent grouping of consecutive segments of the same type, automatically detecting gaps created by other segment types.

```csharp
// Example message structure:
// DG1|1|I9|^Diabetes|
// DG1|2|I9|^Hypertension|  
// OBX|1|ST|^Lab Result|     <- Creates gap
// DG1|3|I9|^Asthma|
// DG1|4|I9|^COPD|

var groups = message.Segments("DG1").Groups();
// Returns 2 groups: [DG1,DG1] and [DG1,DG1]

// Access groups
var firstGroup = groups[0];          // 0-based indexer
var secondGroup = groups.Group(2);   // 1-based Group() method

// LINQ operations on groups
var largeGroups = groups.Where(g => g.Count >= 2).ToList();

// Access segments within groups
foreach (var group in groups)
{
    foreach (var segment in group)
    {
        var diagnosis = segment[3][2].ToString(); // Diagnosis description
    }
}
```

**SegmentGroupCollection Properties:**
- `Count` - Number of groups
- `HasGroups` - Whether any groups exist
- `GroupCount` - Same as Count

**SegmentGroupCollection Methods:**
- `Group(int groupNumber)` - Get group by 1-based index
- `this[int index]` - Get group by 0-based index
- LINQ methods (Where, Select, etc.)

**SegmentGroup Properties:**
- `Count` - Number of segments in group
- `First` - First segment in group
- `Last` - Last segment in group
- `IsEmpty` - Whether group is empty

**SegmentGroup Methods:**
- `this[int index]` - Get segment by 0-based index
- LINQ methods for processing segments

### Path API

String-based access using enhanced HL7 path notation with full repetition support.

```csharp
var path = message.Path("PID.5.1");              // Basic path
var path2 = message.Path("DG1[2].3.1");          // With segment repetition
var path3 = message.Path("PID.3[2]");            // With field repetition
var path4 = message.Path("OBX[3].5[2].1");       // Combined repetitions
```

**Path Syntax:**
- `SEGMENT[segRep].FIELD[fieldRep].COMPONENT.SUBCOMPONENT`
- Repetition brackets `[n]` are optional and default to `[1]`
- All indices are 1-based per HL7 standard

**Properties:**
- `Value` - Get raw value at path with structural delimiters and encoded characters ("" for HL7 nulls)
- `Exists` - True if path exists
- `HasValue` - True if has actual data (false for empty, null, or HL7 null "")
- `IsNull` - True for HL7 nulls ("")

**Methods:**
- `ToString()` - Get human-readable format (decoded, with spaces)
- `Set(string value)` - Set value (auto-creates segments)
- `SetIf(string value, bool condition)` - Conditional set
- `SetNull()` - Set HL7 null ("")

### Builders

#### MSHBuilder
Fluent builder for MSH segments with intelligent defaults.

```csharp
message.CreateMSH
    .Sender("APP", "FACILITY")
    .Receiver("DEST", "FACILITY2")
    .MessageType("ADT^A01")
    .AutoControlId()
    .Production()
    .Build();
```

**Methods:**
- `Sender(string app, string facility)` - Set sending application and facility
- `Receiver(string app, string facility)` - Set receiving application and facility
- `MessageType(string messageType)` - Set message type (e.g., "ADT^A01")
- `ControlId(string id)` - Set specific control ID
- `AutoControlId()` - Generate unique control ID (timestamp-based)
- `ProcessingId(string id)` - Set processing ID ("P", "T", "D")
- `Production()` - Set processing ID to "P" (production)
- `Test()` - Set processing ID to "T" (test)
- `Debug()` - Set processing ID to "D" (debug)
- `Version(string version)` - Set HL7 version (default "2.5")
- `Security(string security)` - Set security field
- `AutoTimestamp()` - Set current timestamp (auto-applied)
- `Build()` - Create the MSH segment and apply to message

**Defaults applied automatically:**
- Message timestamp: Current date/time
- HL7 version: "2.5"
- Processing ID: "P" if not specified
- Field separators: Standard HL7 encoding (`|^~\&`)

#### SerializationBuilder
Fluent builder for message serialization.

```csharp
string hl7 = message.Serialize()
    .WithoutTrailingDelimiters()
    .WithValidation()
    .ToString();
```

**Methods:**
- `WithoutTrailingDelimiters()` - Remove trailing delimiters
- `WithValidation()` - Validate before serializing
- `WithEncoding(Encoding)` - Set text encoding
- `ToString()` - Serialize to string
- `ToBytes()` - Serialize to byte array
- `ToFile(string path)` - Write to file
- `ToStream(Stream)` - Write to stream
- `TrySerialize(out string result, out string error)` - Safe serialize

### Extension Methods

```csharp
// String to FluentMessage
var result = hl7String.TryParse();
if (result.IsSuccess) { var message = result.Message; }

```

### Raw vs ToString()

Understanding the difference between `Raw` and `ToString()` is crucial for proper HL7 data handling:

**Raw Property** - Returns raw HL7 data for reliable data operations:
```csharp
// Raw values preserve HL7 structure and encoding
field.Raw        // "Smith^John^M" (components separated by ^)
field.Raw        // "Johnson \\T\\ Associates" (encoded & as \T\)
field.Raw        // "Lab\\S\\Path^Result" (encoded ^ as \S\)

// Use Raw for:
// - Copying data between messages
// - Storing in databases
// - Sending to other systems
// - Any data processing operations
message.PID[5].SetRaw(otherMessage.PID[5].Raw);  // Safe copy
```

**ToString() Method** - Returns human-readable display format:
```csharp
// ToString() converts to readable text
field.ToString()   // "Smith John M" (^ becomes space)
field.ToString()   // "Johnson & Associates" (\T\ decoded to &)
field.ToString()   // "Lab^Path Result" (\S\ decoded to ^)

// Special handling:
nullField.ToString()   // "<null>" (HL7 nulls shown clearly)
emptyField.ToString()  // "" (empty remains empty)

// Use ToString() for:
// - Displaying to users
// - Generating reports
// - Logging human-readable data
Console.WriteLine($"Patient: {message.PID[5].ToString()}");
```

**Key Differences:**
- **Raw**: Preserves exact HL7 representation with encoded delimiters
- **ToString()**: Decodes for human readability, replaces structural delimiters with spaces
- **HL7 Nulls**: Raw returns `""`, ToString() returns `"<null>"`
- **Consistency**: All accessor types (Field, Component, SubComponent, Path) follow these rules

### Utility Methods

- `MessageHelper.GetMLLP(string hl7, Encoding encoding)` - Add MLLP wrapper

<details>
<summary><b>Contributing</b></summary>

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

## Credits

Based on [HL7-dotnetcore](https://github.com/Efferent-Health/HL7-dotnetcore) and Jayant Singh's original HL7 parser.

---

</details>

<p align="center">
  Made with ❤️ for the healthcare developer community
</p>
