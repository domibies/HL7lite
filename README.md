<h1 align="center">
  <br>
  🏥 HL7lite
  <br>
</h1>

<h4 align="center">A lightweight, high-performance HL7 2.x parser for .NET</h4>

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

<p align="center">
  <a href="#-key-features">Key Features</a> •
  <a href="#-installation">Installation</a> •
  <a href="#-quick-start">Quick Start</a> •
  <a href="#-usage">Usage</a> •
  <a href="#-whats-new">What's New</a> •
  <a href="#-contributing">Contributing</a> •
  <a href="#-credits">Credits</a>
</p>

## 🚀 Key Features

- ⚡ **Lightning Fast** - Parse HL7 messages without schema validation overhead
- 🎯 **Simple API** - Intuitive methods for message manipulation
- 🔧 **Flexible** - Auto-create segments, fields, and components as needed
- 📦 **Lightweight** - Minimal dependencies, small footprint
- ✅ **Battle-tested** - High code coverage and real-world usage
- 🌐 **.NET Standard** - Compatible with .NET Framework, .NET Core, and .NET 5+
- 🏗️ **No Schema Required** - Works with any HL7 2.x message format

## 📦 Installation

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

## 🏃 Quick Start

```csharp
using HL7lite;

// Parse an HL7 message
var message = new Message(hl7String);
message.ParseMessage();

// Access patient information
var patientName = message.GetValue("PID.5.1");
var dateOfBirth = message.GetValue("PID.7");

// Modify values
message.SetValue("PID.5.1", "SMITH");
message.SetValue("PID.5.2", "JOHN");

// Create a new segment
var newSegment = new Segment("ZIC", message.Encoding);
newSegment.AddNewField(new Field("1.556", message.Encoding), 3);
message.AddNewSegment(newSegment);

// Generate ACK
var ack = message.GetACK();
```

## 📖 Usage

<details>
<summary><b>🏗️ Message Construction</b></summary>

### Create a new message
```csharp
var message = new Message();
message.AddSegmentMSH("LAB400", "LAB", 
                      "EPD", "NEUROLOGY",
                      "", "ADT^A01", 
                      "84768948", "P", "2.3");
```

### Parse existing message
```csharp
Message message = new Message(strMsg);
try 
{
    message.ParseMessage();
}
catch(HL7Exception ex)
{
    // Handle parse errors
}
```

### Extract from MLLP frame
```csharp
var messages = MessageHelper.ExtractMessages(mlllpBuffer);
foreach (var strMsg in messages)
{
    var message = new Message(strMsg);
    message.ParseMessage();
}
```
</details>

<details>
<summary><b>📊 Accessing Data</b></summary>

### Get field values
```csharp
// Multiple ways to access the same field
string sendingFacility = message.GetValue("MSH.4");
sendingFacility = message.DefaultSegment("MSH").Fields(4).Value;
sendingFacility = message.Segments("MSH")[0].Fields(4).Value;
```

### Work with repeating fields
```csharp
// Check if field has repetitions
bool hasRepetitions = message.HasRepetitions("PID.3");

// Get all repetitions
List<Field> patientIds = message.Segments("PID")[0].Fields(3).Repetitions();

// Access specific repetition
string secondId = message.GetValue("PID.3[2]");
```

### Handle components
```csharp
// Access components
string familyName = message.GetValue("PID.5.1");
string givenName = message.GetValue("PID.5.2");

// Check if componentized
bool isComponentized = message.IsComponentized("PID.5");
```
</details>

<details>
<summary><b>✏️ Modifying Messages</b></summary>

### Update values
```csharp
// Simple field update
message.SetValue("PV1.2", "I");

// Update component
message.SetValue("PID.5.1", "SMITH");

// Create missing elements automatically
message.PutValue("ZZ1.2.4", "SYSTEM59");

// Check existence before updating
if (message.ValueExists("ZZ1.2"))
    message.PutValue("ZZ1.2.4", "SYSTEM59");
```

### Add new segments
```csharp
// Create a custom segment
var newSegment = new Segment("ZIM", message.Encoding);
newSegment.AddNewField("1.57884", 3);

// Add component to field
newSegment.Fields(3).AddNewComponent(new Component("MM", message.Encoding), 2);

// Add to message
message.AddNewSegment(segment);
```

### Remove segments
```csharp
// Remove first occurrence
message.RemoveSegment("NK1");

// Remove specific occurrence (0-based)
message.RemoveSegment("NK1", 1);
```

### Clean up messages
```csharp
// Remove trailing delimiters
message.RemoveTrailingDelimiters(RemoveDelimitersOptions.All);
```
</details>

<details>
<summary><b>🔄 ACK/NACK Generation</b></summary>

```csharp
// Generate ACK
Message ack = message.GetACK();

// Generate NACK with error
Message nack = message.GetNACK("AR", "Invalid patient ID");

// Customize ACK fields
ack.SetValue("MSH.3", "MyApplication");
ack.SetValue("MSH.4", "MyFacility");
```
</details>

<details>
<summary><b>🔧 Advanced Features</b></summary>

### Encoded content
```csharp
var obx = new Segment("OBX", new HL7Encoding());

// Encode special characters
obx.AddNewField(obx.Encoding.Encode("domain.com/resource.html?Action=1&ID=2"));
```

### Deep copy segments
```csharp
Segment pidCopy = originalMessage.DefaultSegment("PID").DeepCopy();
newMessage.AddNewSegment(pidCopy);
```

### Date handling
```csharp
// Parse HL7 date/time
string hl7DateTime = "20151231234500.1234+2358";
TimeSpan offset;
DateTime? dt = MessageHelper.ParseDateTime(hl7DateTime, out offset);

// Without timezone
DateTime? dt2 = MessageHelper.ParseDateTime("20151231234500");
```

### Null elements
```csharp
// Null elements are represented as ""
var nullValue = message.GetValue("EVN.4"); // Returns null if field contains ""
```
</details>

## 📊 What's New

### v1.2.0 (July 2024)
- ✨ Optional validation skip in ParseMessage
- 🐛 Improved error handling with proper HL7Exceptions
- 🆕 Parameterless AddSegmentMSH() for minimal segments
- 📦 Updated to latest .NET test frameworks

### v1.1.6 (July 2022)
- 🐛 Fixed GetValue() exception for removed segments

### v1.1.5 (November 2021)
- ✨ Support for custom segment names ending with '0' (e.g., 'ZZ0')

<details>
<summary><b>📜 Older Versions</b></summary>

### v1.1.3 (November 2021)
- 🐛 Fixed HasRepetitions() method

### v1.1.2 (May 2021)
- ✨ Added RemoveTrailingDelimiters() functionality

### v1.1.1 (April 2021)
- ✨ Added PutValue() for auto-creating elements
- ✨ Added ValueExists() for checking element existence
- ✨ Added Ensure methods for element creation
- ✨ Added SwapFields() for field reordering
</details>

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

## 🙏 Credits

This library is based on a fork of [HL7-dotnetcore](https://github.com/Efferent-Health/HL7-dotnetcore), which itself evolved from Jayant Singh's original HL7 parser.

### Original Projects
- [HL7-dotnetcore](https://github.com/Efferent-Health/HL7-dotnetcore)
- [hl7-cSharp-parser](https://github.com/j4jayant/hl7-cSharp-parser)
- [Original article](http://j4jayant.com/articles/hl7/31-hl7-parsing-lib)

Field encoding/decoding methods based on [hl7inspector](https://github.com/elomagic/hl7inspector).

## ⚠️ Breaking Changes

### Since v1.0
- `ParseMessage()` now throws exceptions on failure instead of returning boolean
- Use `ParseMessage(false)` to skip serialization checks

### Since v2.9 (from upstream)
- MSH segment includes field separator at position 1 (per HL7 standard)
- All MSH field indices should be incremented by one
- Lowercase methods removed in favor of uppercase equivalents
- `GetValue()` now automatically decodes returned content

---

<p align="center">
  Made with ❤️ for the HL7 community
</p>