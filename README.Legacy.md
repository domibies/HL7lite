# HL7lite Legacy API Documentation

> **Note**: This documentation is for the legacy HL7lite API (v1.x). For the modern fluent API with intuitive, chainable methods, see the [main README](README.md).

## 🔧 Core API Improvements in v2.0.0

Even if you're using the legacy API, you benefit from these important bug fixes:
- **Field Repetition Fixes** - Fixed data loss in `RemoveRepetitions()` and state preservation in `AddRepetition()`
- **Segment Copy Fix** - `DeepCopy()` now properly copies segments with individually set fields
- **Position Validation** - Added validation to prevent invalid field/component positions
- **Architecture** - Unified collections using generic `ElementCollection<T>` for consistency

[View detailed fixes](README.CoreFixes_v2.md)

## 📦 Installation

> **Note**: You can install the latest version of HL7lite and continue using the legacy API - all existing functionality remains fully supported and backward compatible.

### .NET CLI
```bash
# Latest version (recommended - includes both legacy and fluent APIs)
dotnet add package HL7lite

# Or pin to legacy version 1.2.0
dotnet add package HL7lite --version 1.2.0
```

### Package Manager
```powershell
# Latest version (recommended)
Install-Package HL7lite

# Or pin to legacy version 1.2.0
Install-Package HL7lite -Version 1.2.0
```

### PackageReference
```xml
<!-- Latest version (recommended) -->
<PackageReference Include="HL7lite" Version="*" />

<!-- Or pin to legacy version 1.2.0 -->
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

## 📊 Version History

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

## 📄 License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

---

## 🔄 Upgrading to Modern Fluent API

Consider upgrading to the modern fluent API for a better development experience:

```csharp
// Legacy API
var patientName = message.GetValue("PID.5.1");
message.SetValue("PID.5.1", "SMITH");

// Modern Fluent API
var fluent = message.ToFluentMessage();
var patientName = fluent.PID[5][1].Value;
fluent.PID[5][1].Set("SMITH");
```

See the [main README](README.md) for complete fluent API documentation.