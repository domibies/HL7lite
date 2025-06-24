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

- ⚡ **Lightning Fast** - Parse HL7 messages without schema validation overhead
- 🎯 **Modern Fluent API** ![NEW](https://img.shields.io/badge/NEW-brightgreen?style=flat-square) - Intuitive, chainable methods for message manipulation
- 🛡️ **Safe Data Access** - Fluent API returns empty values instead of throwing exceptions
- 🔧 **Auto-creation** - Automatically create missing segments, fields, and components
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
string patientId = fluent.PID[3].Value;
string lastName = fluent.PID[5][1].Value;
string firstName = fluent.PID[5][2].Value;
string dateOfBirth = fluent.PID[7].Value;

// Access with safe navigation - never throws
string gender = fluent.PID[8].Value ?? ""; // Handle null with null-coalescing
string missing = fluent.ZZZ[99].Value;     // Returns null, doesn't throw

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

### Manipulating Data

```csharp
// Set simple fields
fluent.PID[3].Set("12345");
fluent.PID[5].Set().Components("Smith", "John", "M");
fluent.PID[7].Set("19850315");

// Cross Chaining - Set multiple fields in one statement
fluent.PID[5].Set()
    .Components("Johnson", "Mary", "Elizabeth")
    .Field(7, "19901225")              // Date of birth
    .Field(8, "F")                      // Gender
    .Field(11, "123 Main St")           // Address
    .Component(11, 3, "Springfield")    // City
    .Component(11, 4, "IL")            // State
    .Component(11, 5, "62701");        // Zip

// Auto-creation: Set() never throws - creates missing elements automatically
fluent["ZZ1"][99][3].Set("CustomValue");  // Creates entire structure
fluent.Path("NEW.99.99").Set("Value");    // Creates segment, field, component

// Add field repetitions (correct pattern)
fluent.PID[3].Set()
    .Value("MRN001")
    .AddRepetition("ENC123");

// Work with multiple segments using structured data
fluent.Segments("DG1").Add()
    .Field(1, "1")                      // Set ID
    .Field(3).Components("I10", "250.00", "Diabetes mellitus type 2")
    .Field(6, "F");                    // Type

// Complex cross-chaining with structured components
fluent.Segments("OBX").Add()
    .Field(1, "1")                      // Set ID
    .Field(2, "ST")                    // Value type
    .Field(3).Components("GLUCOSE", "Glucose Level")
    .Field(5, "95")                    // Result
    .Field(6, "mg/dL")                 // Units
    .Field(7, "70-100")                // Reference range
    .Field(8, "N")                     // Abnormal flag
    .Component(14, 1, "20231215120000"); // Observation date/time

// Set subcomponents for complex fields
fluent.PID[11].Set()
    .Components("123 Main St", "Apt 4B", "Springfield", "IL", "62701", "USA")
    .SubComponents(1, "123 Main St", "Building A", "Suite 100");
```

<details>
<summary><b>Message Construction and Copying</b></summary>

### Creating New Messages

```csharp
// Create a new message from scratch
var message = new Message();
var fluent = new FluentMessage(message);

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

// Add patient segment
fluent.Segments("PID").Add()[1].Set().Value("1");
fluent.PID[3].Set("PAT001");
fluent.PID[5].Set().Components("Doe", "John", "Middle");
fluent.PID[7].Set("19800101");
fluent.PID[8].Set("M");

// One-step segment creation with multiple fields
fluent.Segments("PV1").Add()
    .Field(1, "1")
    .Field(2, "I")                     // Patient class
    .Field(3).Components("ICU", "001", "A")
    .Field(7).Components("1234", "Smith", "John", "Dr")
    .Field(44, "20231215080000");     // Admit date/time
```

### Copying Messages and Segments

```csharp
// Deep copy entire message with fluent API
var original = hl7String.ToFluentMessage();
var copy = original.Copy();

// Modify the copy without affecting the original
copy.PID[3].Set("NEW_ID");
copy.PID[5].Set().Components("NewLastName", "NewFirstName");

// Copy specific segments between messages
var source = sourceHL7.ToFluentMessage();
var target = new FluentMessage(new Message());

// Copy all DG1 segments from source to target (using AddCopy for independence)
var sourceDG1Segments = source.UnderlyingMessage.GetSegments("DG1");
foreach (var segment in sourceDG1Segments) {
    target.Segments("DG1").AddCopy(segment);
}

// Copy and modify a segment
var sourcePIDSegment = source.UnderlyingMessage.GetSegments("PID")[0];
var pidAccessor = target.Segments("PID").AddCopy(sourcePIDSegment);
pidAccessor[3].Set().Value("MODIFIED_ID");

// Copy with selective field updates
var sourceOBXSegment = source.UnderlyingMessage.GetSegments("OBX")[0];
var obxAccessor = target.Segments("OBX").AddCopy(sourceOBXSegment);
obxAccessor[5].Set().Value("Updated Result");
obxAccessor[14][1].Set().Value(DateTime.Now.ToString("yyyyMMddHHmmss"));
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
fluent.PV1[7].Repetition(1).Set().Components("1234", "Smith", "John", "Dr");

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
fluent.PID[5].Set().Components("Smith|Jones", "Mary");  // ❌ The | breaks field separation

// WITH encoding - properly escaped
fluent.PID[5][1].Set().EncodedValue("Smith|Jones");  // ✅ Becomes "Smith\F\Jones"

// Best practice: Use structured data when possible
fluent.PID[5].Set().Components("Smith-Jones", "Mary");  // ✅ No delimiters needed
```

### Real-World Examples

```csharp
// URLs with query parameters
fluent.OBX[5].Set().EncodedValue("https://lab.hospital.com/results?id=123&type=CBC");

// Medical notes with special characters
fluent.NTE[3].Set().EncodedValue("Blood pressure: 120/80 | Temp: 98.6°F");

// Complex addresses using structured data (preferred)
fluent.PID[11].Set()
    .Components("123 Main St", "Suite A&B", "Boston", "MA", "02101")
    .SubComponents(2, "Suite A&B", "Building 5", "East Wing");

// Lab results with ranges - use structured components when possible
fluent.OBX[5].Set().Components("95", "mg/dL");
fluent.OBX[7].Set("70-100");  // Reference range in separate field

// File paths
fluent.OBX[5].Set().EncodedValue("\\\\server\\lab\\results\\patient123.pdf");
```

### Automatic Decoding

When reading values, delimiters are automatically decoded:

```csharp
// Set encoded value
fluent.PID[5][1].Set().EncodedValue("Smith|Jones");

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
- **Modern Fluent API** - Complete rewrite with intuitive, chainable interface
- **Cross Chaining** - Set multiple fields and components in one statement
- **Path API** - String-based paths wrapping legacy methods
- **Enhanced Collections** - Full LINQ support for segments and repetitions
- **Better Encoding** - Improved EncodedValue methods for delimiter handling
- **Full Compatibility** - Legacy API unchanged and fully supported

### Previous Versions
- v1.2.0 - Optional validation, improved error handling
- v1.1.x - Bug fixes and stability improvements
- v1.0.0 - Initial stable release

</details>

## API Reference {#api-reference}

### FluentMessage

The main entry point for the fluent API. Wraps a legacy `Message` object.

```csharp
var fluent = new FluentMessage(message);
var fluent = hl7String.ToFluentMessage();  // Extension method
```

**Properties:**
- `MSH`, `PID`, `PV1`, etc. - Direct segment accessors (37 common segments)
- `UnderlyingMessage` - Access to wrapped Message object
- `CreateMSH` - MSH segment builder

**Methods:**
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
Modify field values.

```csharp
fluent.PID[3].Set()
    .Value("12345")
    .Field(5, "Smith^John")
    .Field(7, "19850315");
```

**Methods:**
- `Value(string value)` - Set field value
- `Null()` - Set HL7 null value ("")
- `Clear()` - Clear field (empty string)
- `Components(params string[] values)` - Set multiple components
- `EncodedValue(string value)` - Set with delimiter encoding
- `ValueIf(string value, bool condition)` - Conditional set
- `Field(int index, string value)` - Set different field
- `Date(DateTime date)` - Set date (YYYYMMDD)
- `DateTime(DateTime dateTime)` - Set date/time (YYYYMMDDHHMMSS)
- `DateToday()` / `DateTimeNow()` - Set current date/time

#### ComponentMutator
Modify component values.

```csharp
fluent.PID[5][1].Set()
    .Value("Smith")
    .Component(2, "John")
    .Field(7, "19850315");
```

**Methods:**
- `Value(string value)` - Set component value
- `Null()` - Set HL7 null value
- `Clear()` - Clear component
- `SubComponents(params string[] values)` - Set subcomponents
- `EncodedValue(string value)` - Set with encoding
- `ValueIf(string value, bool condition)` - Conditional set
- `Component(int index, string value)` - Set different component
- `Field(int index, string value)` - Set field value

#### SubComponentMutator
Modify subcomponent values.

```csharp
fluent.PID[5][1][1].Set()
    .Value("Smith")
    .SubComponent(2, "Jr")
    .Component(2, "John");
```

**Methods:**
- Same as ComponentMutator but at subcomponent level

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
- `SetEncodedIf(string value, bool condition)` - Conditional encoded set

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
