# HL7lite

Simple, lightweight HL7 v2.x parsing and manipulation for .NET

## Quick Start

```csharp
using HL7lite;
using HL7lite.Fluent;

// Parse a message
var message = @"MSH|^~\&|SENDER|FACILITY|RECEIVER|FACILITY|20240101120000||ADT^A01|123456|P|2.5
PID|||12345||Doe^John^Middle||19800315|M|||123 Main St^Apt 4B^City^ST^12345".ToFluentMessage();

// Access fields using the Fluent API
var patientName = message.PID[5][1].Value;        // "Doe"
var dateOfBirth = message.PID[7].Value;           // "19800315"

// Modify values
message.PID[5].Set().Components("Smith", "Jane", "Marie");
message.PID[13].Repetitions.Add("555-1234");
message.PID[13].Repetitions.Add("555-5678");

// Create new segments
var obx = message.Segments("OBX").Add();
obx[1].Set().Value("1");
obx[2].Set().Value("NM");
obx[3].Set().Value("GLUCOSE");
obx[5].Set().Value("120");
```

## Key Features

- 🎯 **Modern Fluent API** - Intuitive, chainable methods for message manipulation
- 🛡️ **Never Throws** - Returns empty values instead of throwing exceptions
- 🔧 **Auto-creation** - Automatically creates missing segments and fields
- ⚡ **High Performance** - Efficient parsing without schema validation overhead
- 📦 **Zero Dependencies** - Lightweight with minimal footprint

## Fluent API Examples

### Reading Values
```csharp
// Simple field access
string patientId = message.PID[3].Value;

// Component access
string lastName = message.PID[5][1].Value;
string firstName = message.PID[5][2].Value;

// Safe access (never throws)
string value = message.ZZZ[999].Value;  // Returns empty string if segment doesn't exist

// Field repetitions
string firstId = message.PID[3].Repetition(1).Value;
string secondId = message.PID[3].Repetition(2).Value;
```

### Setting Values
```csharp
// Set field value
message.PID[3].Set().Value("12345");

// Set with components
message.PID[5].Set().Components("Smith", "John", "M");

// Add field repetitions
message.PID[13].Repetitions.Add("555-1234");
message.PID[13].Repetitions.Add("555-5678");

// Method chaining
message.PID[5].Set()
    .Components("Johnson", "Robert")
    .Field(7, "19850315")
    .Field(8, "M");
```

### Working with Segments
```csharp
// Add segments
var dg1 = message.Segments("DG1").Add();
dg1[3].Set().Value("I10^Essential Hypertension");

// Query segments with LINQ
var diagnoses = message.Segments("DG1")
    .Where(d => d[6].Value == "F")
    .Select(d => d[3][2].Value);
```

## Path-based Access
```csharp
// Get values using path syntax
string value = message.Path("PID.5.1").Value;

// Set values using paths
message.Path("PID.5.1").Set("Smith");
message.Path("OBX.5").Set("120");
```

## Documentation & Support

- **Full Documentation**: https://github.com/domibies/HL7lite
- **API Reference**: https://github.com/domibies/HL7lite#api-reference
- **Issues & Feedback**: https://github.com/domibies/HL7lite/issues
- **Release Notes**: https://github.com/domibies/HL7lite/releases

## License

MIT License - see https://github.com/domibies/HL7lite/blob/master/LICENSE.txt