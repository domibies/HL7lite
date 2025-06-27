# HL7lite

Simple, lightweight HL7 v2.x parsing and manipulation for .NET

## Quick Start

```csharp
using HL7lite;
using HL7lite.Fluent;

// Parse a message
var hl7String = @"MSH|^~\&|SENDER|FACILITY|RECEIVER|FACILITY|20240101120000||ADT^A01|123456|P|2.5
PID|||12345||Doe^John^Middle||19800315|M|||123 Main St^Apt 4B^City^ST^12345";
var result = hl7String.TryParse();
if (!result.IsSuccess) {
    Console.WriteLine($"Parse failed: {result.ErrorMessage}");
    return;
}
var message = result.Message;

// Access fields using the Fluent API
var patientName = message.PID[5][1].Value;        // "Doe"
var dateOfBirth = message.PID[7].Value;           // "19800315"

// Modify values with fluent chaining
message.PID[5].SetComponents("Smith", "Jane", "Marie")
    .Field(7).Set("19850315")
    .Field(8).Set("M")
    .Field(13).Repetitions.Add("555-1234")
                          .Add("555-5678");

// Create new segments with fluent building
var obx = message.Segments("OBX").Add()
    .Field(1).Set("1")
    .Field(2).Set("NM")
    .Field(3).Set("GLUCOSE")
    .Field(5).Set("120");
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
message.PID[3].Set("12345");

// Set with components
message.PID[5].SetComponents("Smith", "John", "M");

// Add field repetitions with chaining
message.PID[13].Repetitions.Add("555-1234")
                           .Add("555-5678");

// Pure navigation with method chaining
message.PID[5].SetComponents("Johnson", "Robert")
    .Field(7).Set("19850315")
    .Field(8).Set("M")
    .Field(11).SetComponents("123 Main St", "Boston", "MA");
```

### Working with Segments
```csharp
// Add segments with fluent building
var dg1 = message.Segments("DG1").Add()
    .Field(1).Set("1")
    .Field(3).SetComponents("I10", "Essential Hypertension")
    .Field(6).Set("F");

// Query segments with LINQ
var diagnoses = message.Segments("DG1")
    .Where(d => d[6].Value == "F")
    .Select(d => d[3][2].Value);

// Create complete message from scratch
var newMessage = FluentMessage.Create();
newMessage.CreateMSH
    .Sender("HIS", "HOSPITAL")
    .Receiver("LAB", "LABORATORY")
    .MessageType("ADT^A01")
    .AutoControlId()
    .Build();

newMessage.Segments("PID").Add()
    .Field(3).Set("12345")
    .Field(5).SetComponents("Smith", "John", "M")
    .Field(7).Set("19850315");
```

## Path-based Access
```csharp
// Get values using path syntax
string value = message.Path("PID.5.1").Value;

// Set values using paths
message.Path("PID.5.1").Set("Smith");
message.Path("OBX.5").Set("120");

// Handle special characters with encoding
message.Path("PID.5.1").SetEncoded("Smith&Jones");  // Name with ampersand
message.Path("OBX.5").SetEncoded("http://lab.com/result?id=123&type=CBC");

// Conditional path operations
message.Path("PID.6").SetIf("MAIDEN", hasMiddleName);
```

## Documentation & Support

- **Full Documentation**: https://github.com/domibies/HL7lite
- **API Reference**: https://github.com/domibies/HL7lite#api-reference
- **Issues & Feedback**: https://github.com/domibies/HL7lite/issues
- **Release Notes**: https://github.com/domibies/HL7lite/releases

## License

MIT License - see https://github.com/domibies/HL7lite/blob/master/LICENSE.txt