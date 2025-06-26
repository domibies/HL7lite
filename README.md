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

### 🎯 Modern Fluent API ![NEW](https://img.shields.io/badge/NEW-brightgreen?style=flat-square)
*Complete rewrite with intuitive, modern interface - available in v2.0.0-rc.1*

- 🧭 **Pure Navigation** ![NEW](https://img.shields.io/badge/NEW-brightgreen?style=flat-square) - Crystal clear navigation with natural language-like syntax: `Field(7).Component(1).Set("text")`
- ⛓️ **Method Chaining** ![NEW](https://img.shields.io/badge/NEW-brightgreen?style=flat-square) - Fluent operations across all hierarchy levels with intuitive return types
- 🛡️ **Safe Data Access** ![NEW](https://img.shields.io/badge/NEW-brightgreen?style=flat-square) - Returns empty values instead of throwing exceptions
- 🔧 **Auto-creation** ![NEW](https://img.shields.io/badge/NEW-brightgreen?style=flat-square) - Automatically creates missing segments, fields, and components
- 🗂️ **LINQ Collections** ![NEW](https://img.shields.io/badge/NEW-brightgreen?style=flat-square) - Full LINQ support for segments and field repetitions
- 🛤️ **Path API** ![NEW](https://img.shields.io/badge/NEW-brightgreen?style=flat-square) - String-based access wrapping legacy GetValue/SetValue methods
- 🔐 **Encoding Support** ![NEW](https://img.shields.io/badge/NEW-brightgreen?style=flat-square) - Automatic HL7 delimiter character encoding/decoding

### 🏗️ Core Engine
- ⚡ **Lightning Fast** - Parse HL7 messages without schema validation overhead  
- 📦 **Lightweight** - Minimal dependencies, small footprint
- ✅ **Battle-tested** - Key integration component in Belgium's largest hospital group ([ZAS](https://www.zas.be))
- 🔄 **Always Compatible** - Full backward compatibility with [legacy API](README.Legacy.md)
- 🌐 **.NET Standard** - Compatible with .NET Framework, .NET Core, and .NET 5+

## Quick Start

### Installation

> **Note**: The fluent API is currently in Release Candidate. Installing without specifying a version will install the latest stable 1.2.0 version (legacy API only).

```bash
# .NET CLI - Fluent API (RC)
dotnet add package HL7lite --version 2.0.0-rc.1

# .NET CLI - Legacy API only (stable)
dotnet add package HL7lite

# Package Manager - Fluent API (RC)
Install-Package HL7lite -Version 2.0.0-rc.1

# Package Manager - Legacy API only (stable)
Install-Package HL7lite

# PackageReference - Fluent API (RC)
<PackageReference Include="HL7lite" Version="2.0.0-rc.1" />

# PackageReference - Legacy API only (stable)
<PackageReference Include="HL7lite" Version="1.*" />
```

### Getting Data

```csharp
using HL7lite;
using HL7lite.Fluent;

// Parse an HL7 message
var message = new Message(hl7String);
message.ParseMessage();

// Create fluent wrapper
var fluent = new FluentMessage(message);

// Get patient information
string patientId = fluent.PID[3].Value;        // Single field value
string lastName = fluent.PID[5][1].Value;      // First component only
string firstName = fluent.PID[5][2].Value;     // Second component only
string fullName = fluent.PID[5].Value;         // Entire field: "Smith^John^M^Jr"
string dateOfBirth = fluent.PID[7].Value;

// .Value returns entire structure with HL7 separators:
// - Field.Value includes all components: "Smith^John^M"
// - Field.Value with repetitions: "ID1~ID2~ID3" 
// - Component.Value includes subcomponents: "Home&555-1234"

// Parse dates and timestamps
DateTime? birthDate = fluent.PID[7].AsDate();           // Parse "19850315" -> DateTime
DateTime? timestamp = fluent.EVN[2].AsDateTime();       // Parse "20240624143022" -> DateTime
DateTime? timestampWithTz = fluent.EVN[2].AsDateTime(out TimeSpan offset); // Include timezone

// Access with safe navigation - never throws
string gender = fluent.PID[8].Value ?? ""; // Handle null with null-coalescing
string missing = fluent.PID[99].Value;     // Non-existing field returns "", doesn't throw

// Use path-based access
string ssn = fluent.Path("PID.19").Value;
string phone = fluent.Path("PID.13[1].1").Value;

// Check field repetitions
if (fluent.PID[3].HasRepetitions) {
    var ids = fluent.PID[3].Repetitions
        .Select(r => r.Value)
        .ToList();
}

// Query multiple segments
var diagnoses = fluent.Segments("DG1")
    .Select(dg1 => new {
        Code = dg1[3][1].Value,
        Description = dg1[3][2].Value,
        Type = dg1[6].Value
    })
    .ToList();
```

### Pure Navigation Pattern

HL7lite uses a **Pure Navigation Pattern** that separates navigation from setting operations for crystal-clear intent:

```csharp
// ✅ PURE NAVIGATION: Navigate first, then set
fluent.PID[5].Set()
    .Value("Smith")                     // Set current field
    .Field(7).Set("19850315")           // Navigate to field 7, then set
    .Field(8).Set("M")                  // Navigate to field 8, then set
    .Field(11).Component(3).Set("Springfield");  // Navigate to field 11, component 3, then set

// ✅ Cross-level navigation reads like natural language
fluent.PID[5][1][1].Set()
    .Value("LastName")                  // Set current subcomponent
    .SubComponent(2).Set("FirstName")   // Navigate to subcomponent 2, then set
    .Component(2).Set("MiddleName")     // Navigate to component 2, then set
    .Field(7).Set("19850315");          // Navigate to field 7, then set

// ✅ Complex navigation with clear intent
fluent.Segments("OBX").Add()
    .Field(1).Set("1")                  // Navigate to field 1, set sequence ID
    .Field(3).Component(1).Set("GLUCOSE")       // Navigate to observation identifier
    .Field(5).Set("95")                 // Navigate to observation value
    .Field(14).Component(1).Set("20240101120000");  // Navigate to timestamp
```

**Key Benefits:**
- **Crystal Clear Intent**: `Field(11).Component(1).Set("text")` is completely unambiguous
- **Natural Language**: Code reads like step-by-step navigation instructions  
- **No Parameter Confusion**: Single-parameter methods eliminate ambiguity
- **Full Navigation Matrix**: Navigate anywhere from any mutator type
- **Type Safety**: Return types clearly indicate current navigation context

### Manipulating Data

```csharp
// Complete patient demographics in one powerful method chain
fluent.PID[3].Set("12345")
    .Field(5).SetComponents("Smith", "John", "M", "Jr", "Dr")
    .Field(7).SetDate(new DateTime(1985, 3, 15))
    .Field(8).Set("M")
    .Field(11).SetComponents("123 Main St", "Apt 4B", "Springfield", "IL", "62701", "USA")
    .Field(13).SetComponents("555", "123-4567")        // Home phone
    .Field(14).SetComponents("555", "098-7654");       // Business phone

// DateTime chaining for multiple timestamps
fluent.EVN[2].SetDateTime(DateTime.Now)          // Event occurred
    .Field(6).SetDateTime(DateTime.Now);               // Event entered

// Pure Navigation - Crystal clear navigation and setting
fluent.PID[5].SetComponents("Johnson", "Mary", "Elizabeth")
    .Field(7).Set("19901225")           // Navigate to field 7, set date of birth
    .Field(8).Set("F")                  // Navigate to field 8, set gender
    .Field(11).Set("123 Main St")       // Navigate to field 11, set address
    .Field(11).Component(3).Set("Springfield")  // Navigate to component 3, set city
    .Field(11).Component(4).Set("IL")           // Navigate to component 4, set state
    .Field(11).Component(5).Set("62701");       // Navigate to component 5, set zip

// Auto-creation: Set() never throws - creates missing elements automatically
fluent["Z01"][99][3].Set("CustomValue");  // Creates entire structure
fluent.Path("Z02.99.99").Set("Value");    // Creates segment, field, component

// Add field repetitions with fluent chaining
fluent.PID[3].Set("ID001")
    .AddRepetition("MRN001")              // Adds repetition, stays in fluent chain
    .AddRepetition("ENC123")              // Add another repetition
    .Field(5).SetComponents("Smith", "John");  // Continue with other fields

// Or add repetitions with components
fluent.PID[3].Set("SimpleID")
    .AddRepetition()                      // Add empty repetition
        .SetComponents("MRN", "001", "HOSPITAL")  // Set complex components
    .AddRepetition()                      // Add another empty repetition
        .SetComponents("ENC", "123", "VISIT");    // Different components

// Work with multiple segments using pure navigation
fluent.Segments("DG1").Add()
    .Field(1).Set("1")                  // Navigate to field 1, set ID
    .Field(3).SetComponents("I10", "250.00", "Diabetes mellitus type 2")  // Set components
    .Field(6).Set("F");                 // Navigate to field 6, set type

// Complex navigation with clear intent
fluent.Segments("OBX").Add()
    .Field(1).Set("1")                  // Navigate to field 1, set ID
    .Field(2).Set("ST")                 // Navigate to field 2, set value type
    .Field(3).SetComponents("GLUCOSE", "Glucose Level")  // Set observation components
    .Field(5).Set("95")                 // Navigate to field 5, set result
    .Field(6).Set("mg/dL")              // Navigate to field 6, set units
    .Field(7).Set("70-100")             // Navigate to field 7, set reference range
    .Field(8).Set("N")                  // Navigate to field 8, set abnormal flag
    .Field(14).Component(1).Set("20231215120000");  // Navigate to observation date/time

// Set subcomponents for complex fields
fluent.PID[11].SetComponents("123 Main St", "Apt 4B", "Springfield", "IL", "62701", "USA")
    .Component(1).SetSubComponents("123 Main St", "Building A", "Suite 100");
```

<details>
<summary><b>Message Construction and Copying</b></summary>

### Creating New Messages

```csharp
// Create a new message from scratch
var fluent = FluentMessage.Create();

// Build MSH segment fluently
fluent.CreateMSH
    .Sender("SENDING_APP", "FACILITY_A")
    .Receiver("RECEIVING_APP", "FACILITY_B")
    .MessageType("ADT^A01")
    .ControlId("12345")
    .ProcessingId("P")
    .Version("2.5")
    .Build();

// Or use convenient defaults
fluent.CreateMSH
    .Sender("APP", "FAC")
    .Receiver("DEST", "FAC2") 
    .MessageType("ORU^R01")
    .Production()              // Sets ProcessingId to "P" (production)
    .AutoControlId()           // Generates unique control ID automatically
    .Build();                  // MessageTime is automatically set to current timestamp

// Even simpler - minimal required fields only
fluent.CreateMSH
    .Sender("APP", "FAC")
    .Receiver("DEST", "FAC2")
    .MessageType("ADT^A08")
    .AutoControlId()           // Auto-generates unique ID like "20250623120000123"
    .Build();                  // Uses defaults: Version="2.5", ProcessingId="P", MessageTime=now

// Build complete patient segment with powerful method chaining
fluent.Segments("PID").Add()
    .Field(1).Set("1")
    .Field(3).Set("PAT001")
    .Field(5).SetComponents("Doe", "John", "Middle")
    .Field(7).Set("19800101")
    .Field(8).Set("M")
    .Field(11).SetComponents("456 Oak Ave", "", "Boston", "MA", "02101");

// Create admission info with method chaining
fluent.Segments("PV1").Add()
    .Field(1).Set("1")                         // Set ID
    .Field(2).Set("I")                         // Inpatient
    .Field(3).SetComponents("ICU", "001", "A")     // Location
    .Field(7).SetComponents("1234", "Smith", "John", "Dr")  // Attending doctor
    .Field(44).Set("20231215080000");          // Admit date/time
// Power of chaining: Complete ADT message in one flow
var fluent = FluentMessage.Create();
fluent.CreateMSH
    .Sender("HIS", "HOSPITAL")
    .Receiver("LAB", "LABORATORY")
    .MessageType("ADT^A01")
    .AutoControlId()
    .Build();

// Add all segments in a single chained operation
fluent.Segments("EVN").Add()
    .Field(1).Set("A01")
    .Field(2).SetDateTime(DateTime.Now)
    .Field(6).SetDateTime(DateTime.Now);

fluent.Segments("PID").Add()
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

fluent.Segments("NK1").Add()
    .Field(1).Set("1")
    .Field(2).SetComponents("Smith", "Jane", "Marie")
    .Field(3).SetComponents("SPO", "Spouse");

fluent.Segments("PV1").Add()
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
var original = hl7String.ToFluentMessage();
var copy = original.Copy();

// Modify the copy without affecting the original
copy.PID[3].Set("NEW_ID");
copy.PID[5].SetComponents("NewLastName", "NewFirstName");

// Copy specific segments between messages
var source = sourceHL7.ToFluentMessage();
var target = FluentMessage.Create();

// Copy all DG1 segments from source to target (using AddCopy for independence)
var sourceDG1Segments = source.UnderlyingMessage.GetSegments("DG1");
foreach (var segment in sourceDG1Segments) {
    target.Segments("DG1").AddCopy(segment);
}

// Copy and modify a segment
var sourcePIDSegment = source.UnderlyingMessage.GetSegments("PID")[0];
var pidAccessor = target.Segments("PID").AddCopy(sourcePIDSegment);
pidAccessor[3].Set("MODIFIED_ID");

// Copy with selective field updates
var sourceOBXSegment = source.UnderlyingMessage.GetSegments("OBX")[0];
var obxAccessor = target.Segments("OBX").AddCopy(sourceOBXSegment);
obxAccessor[5].Set("Updated Result");
obxAccessor[14][1].Set(DateTime.Now.ToString("yyyyMMddHHmmss"));
```

</details>

<details>
<summary><b>Path API</b></summary>

The Path API provides string-based access to message elements, wrapping the legacy GetValue/SetValue methods.

**Important**: Unlike the legacy API, Path.Set() behaves like PutValue() - it never throws exceptions and creates missing elements automatically.

```csharp
// Basic path access
string patientName = fluent.Path("PID.5.1").Value;
fluent.Path("PID.5.1").Set("NewLastName");

// Access repetitions using array notation
string firstId = fluent.Path("PID.3[1]").Value;
string secondId = fluent.Path("PID.3[2]").Value;

// Create complex paths
fluent.Path("PV1.7[1].1").Set("1234");     // First attending doctor ID
fluent.Path("PV1.7[1].2").Set("Smith");    // Last name
fluent.Path("PV1.7[1].3").Set("John");     // First name

// Or better, use structured data
fluent.PV1[7].Repetition(1).SetComponents("1234", "Smith", "John", "Dr");

// Check if path exists
bool hasAllergies = fluent.Path("AL1.3").Exists;

// Conditional operations
fluent.Path("PID.6.1").SetIf("MAIDEN", patient.HasMaidenName);

// Auto-creation: Set() never throws exceptions
fluent.Path("ZZ1.5.3").Set("CustomValue"); // Creates entire path if missing
fluent.Path("NEW.99.99").Set("Value");     // Creates segment, field, component
```

</details>

<details>
<summary><b>Encoding</b></summary>

HL7lite automatically handles encoding and decoding of delimiter characters to ensure message integrity.

### Understanding HL7 Encoding

When field values contain HL7 delimiter characters (`|`, `^`, `~`, `\`, `&`), they must be escaped to prevent message corruption. Setting encoded values ensures these characters are properly escaped:

```csharp
// WITHOUT encoding - corrupts the message structure
fluent.PID[5].SetComponents("Smith|Jones", "Mary");  // ❌ The | breaks field separation

// WITH encoding - properly escaped
fluent.PID[5][1].SetEncoded("Smith|Jones");  // ✅ Becomes "Smith\F\Jones"

// Best practice: Use structured data when possible
fluent.PID[5].SetComponents("Smith-Jones", "Mary");  // ✅ No delimiters needed
```

### Real-World Examples

```csharp
// URLs with query parameters
fluent.OBX[5].SetEncoded("https://lab.hospital.com/results?id=123&type=CBC");

// Medical notes with special characters
fluent.NTE[3].SetEncoded("Blood pressure: 120/80 | Temp: 98.6°F");

// Complex addresses using structured data (preferred)
fluent.PID[11].SetComponents("123 Main St", "Suite A&B", "Boston", "MA", "02101");
fluent.PID[11][2].SetSubComponents("Suite A&B", "Building 5", "East Wing");

// Lab results with ranges - use structured components when possible
fluent.OBX[5].SetComponents("95", "mg/dL");
fluent.OBX[7].Set("70-100");  // Reference range in separate field

// File paths
fluent.OBX[5].SetEncoded("\\\\server\\lab\\results\\patient123.pdf");
```

### Automatic Decoding

When reading values, delimiters are automatically decoded:

```csharp
// Set encoded value
fluent.PID[5][1].SetEncoded("Smith|Jones");

// Read value - automatically decoded
string name = fluent.PID[5][1].Value;  // Returns: "Smith|Jones"
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
var segments = fluent.Segments("DG1");
var firstDiagnosis = segments[0];          // 0-based for LINQ
var filtered = segments.Where((s, i) => i > 0);

// But methods remain 1-based
fluent.PID[3].Repetitions.RemoveRepetition(1);  // Removes first repetition
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

// Auto-create missing elements
message.PutValue("ZZ1.2.3", "CustomValue");

// Check existence
bool hasValue = message.ValueExists("PID.7");

// Add segments
var segment = new Segment("PV1", message.Encoding);
segment.AddNewField("1", 1);
segment.AddNewField("O", 2);
message.AddNewSegment(segment);
```

### Migration to Fluent API

The Fluent API wraps the legacy API, so you can migrate incrementally:

```csharp
// Legacy code continues to work
var message = new Message(hl7String);
message.ParseMessage();
string id = message.GetValue("PID.3");

// Add fluent wrapper when convenient
var fluent = new FluentMessage(message);
string name = fluent.PID[5][1].Value;

// Both APIs work on the same message
message.SetValue("PID.7", "19850315");
var dob = fluent.PID[7].Value;  // "19850315"
```

</details>

<details>
<summary><b>What's New</b></summary>

### v2.0.0-rc.1 (June 2025)
- **Pure Navigation API** - Crystal clear navigation with natural language-like syntax
- **Modern Fluent API** - Complete rewrite with intuitive, chainable interface
- **Path API** - String-based paths wrapping legacy methods
- **Enhanced Collections** - Full LINQ support for segments and repetitions
- **Better Encoding** - Improved EncodedValue methods for delimiter handling
- **Full Compatibility** - Legacy API unchanged and fully supported

### Previous Versions
- v1.2.0 - Optional validation, improved error handling
- v1.1.x - Bug fixes and stability improvements
- v1.0.0 - Initial stable release

</details>

## HL7lite API Reference

### FluentMessage

The main entry point for the fluent API. Wraps a legacy `Message` object.

```csharp
var fluent = FluentMessage.Create();        // Create empty message
var fluent = new FluentMessage(message);    // Wrap existing message
var fluent = hl7String.ToFluentMessage();  // Parse from string
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
var pid = fluent.PID;
var custom = fluent["ZZ1"];
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
var field = fluent.PID[3];
var component = fluent.PID[5][1];
```

**Properties:**
- `Value` - Field value (null for HL7 nulls "")
- `Exists` - True if field exists in message
- `HasValue` - True if not empty/null
- `IsNull` - True for HL7 null values ("")
- `IsEmpty` - True for empty or null
- `HasRepetitions` - True if field has repetitions
- `RepetitionCount` - Number of repetitions
- `Repetitions` - Collection of repetitions

**Methods:**
- `Component(int index)` / `this[int index]` - Get component accessor
- `Repetition(int index)` - Get specific repetition (1-based)
- `Set()` - Get field mutator
- `Set(string value)` - Set field value and return mutator
- `AsDate(bool throwOnError = false)` - Parse as date
- `AsDateTime` - Parse as date/time

#### ComponentAccessor
Access to component-level data.

```csharp
var component = fluent.PID[5][1];  // Last name
```

**Properties:**
- Same as FieldAccessor (Value, Exists, etc.)

**Methods:**
- `SubComponent(int index)` / `this[int index]` - Get subcomponent
- `Set()` - Get component mutator
- `Set(string value)` - Set component value and return mutator

#### SubComponentAccessor
Access to subcomponent-level data.

```csharp
var subcomp = fluent.PID[5][1][2];
```

**Properties:**
- Same as FieldAccessor (Value, Exists, etc.)

**Methods:**
- `Set()` - Get subcomponent mutator
- `Set(string value)` - Set subcomponent value and return mutator

### Mutators (Write Operations)

All mutators support method chaining and auto-create missing elements.

#### FieldMutator
Modify field values with pure navigation pattern.

```csharp
fluent.PID[3].Set("12345")
    .Field(5).Set("Smith^John")        // Navigate to field 5, then set
    .Field(7).Set("19850315");         // Navigate to field 7, then set

// Add field repetitions fluently
fluent.PID[3].Set("FirstID")
    .AddRepetition("MRN001")           // Add repetition, stay in chain
    .AddRepetition()                   // Add empty repetition
        .SetComponents("ENC", "123", "VISIT") // Set components on new repetition
    .Field(7).Set("19850315");         // Continue with other fields
```

**Setting Methods:**
- `Set(string value)` - Set field value
- `SetNull()` - Set HL7 null value ("")
- `Clear()` - Clear field (empty string)
- `SetComponents(params string[] values)` - Set multiple components
- `SetEncoded(string value)` - Set with delimiter encoding
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
fluent.PID[5][1].Set()
    .Value("Smith")
    .Component(2).Set("John")          // Navigate to component 2, then set
    .Field(7).Set("19850315");         // Navigate to field 7, then set
```

**Setting Methods:**
- `Set(string value)` - Set component value
- `SetNull()` - Set HL7 null value
- `Clear()` - Clear component
- `SetSubComponents(params string[] values)` - Set subcomponents
- `SetEncoded(string value)` - Set with encoding
- `SetIf(string value, bool condition)` - Conditional set

**Navigation Methods:**
- `Field(int index)` - Navigate to different field (returns FieldMutator)
- `Component(int index)` - Navigate to different component (returns ComponentMutator)
- `SubComponent(int index)` - Navigate to subcomponent (returns SubComponentMutator)

#### SubComponentMutator
Modify subcomponent values with pure navigation pattern.

```csharp
fluent.PID[5][1][1].Set()
    .Value("Smith")
    .SubComponent(2).Set("Jr")         // Navigate to subcomponent 2, then set
    .Component(2).Set("John");         // Navigate to component 2, then set
```

**Setting Methods:**
- `Set(string value)` - Set subcomponent value
- `SetNull()` - Set HL7 null value
- `Clear()` - Clear subcomponent
- `SetEncoded(string value)` - Set with encoding
- `SetIf(string value, bool condition)` - Conditional set

**Navigation Methods:**
- `Field(int index)` - Navigate to different field (returns FieldMutator)
- `Component(int index)` - Navigate to different component (returns ComponentMutator)
- `SubComponent(int index)` - Navigate to different subcomponent (returns SubComponentMutator)

### Collections

#### SegmentCollection
LINQ-compatible collection of segments.

```csharp
var diagnoses = fluent.Segments("DG1");
var primary = diagnoses[0];  // 0-based indexer
var count = diagnoses.Count;
```

**Properties:**
- `Count` - Number of segments

**Methods:**
- `Add()` - Add new segment, returns accessor
- `AddCopy(Segment segment)` - Add copy of segment
- `Clear()` - Remove all segments
- `Segment(int index)` - Get by 1-based index
- `RemoveSegment(int index)` - Remove by 1-based index
- `this[int index]` - Get by 0-based index
- LINQ methods (Where, Select, etc.)

#### FieldRepetitionCollection
Collection of field repetitions.

```csharp
var ids = fluent.PID[3].Repetitions;
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

### Path API

String-based access using HL7 path notation.

```csharp
var path = fluent.Path("PID.5.1");
```

**Properties:**
- `Value` - Get value at path
- `Exists` - True if path exists
- `HasValue` - True if not empty
- `IsNull` - True for HL7 nulls

**Methods:**
- `Set(string value)` - Set value (auto-creates)
- `SetIf(string value, bool condition)` - Conditional set
- `SetNull()` - Set HL7 null
- `SetEncoded(string value)` - Set with encoding

### Builders

#### MSHBuilder
Fluent builder for MSH segments with intelligent defaults.

```csharp
fluent.CreateMSH
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
string hl7 = fluent.Serialize()
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
var fluent = hl7String.ToFluentMessage();

// Legacy Message to FluentMessage
var fluent = message.ToFluentMessage();
```

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
