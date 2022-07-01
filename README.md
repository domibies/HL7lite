![Build status](https://dev.azure.com/dmdevcode/HL7lite/_apis/build/status/HL7lite-ASP.NET%20Core-CI)
# HL7lite

This is a lightweight HL7 2.X parsing library. It's main purpose is being able to swiftly parse & manipulate HL7 2.x messages without the overhead of a fully bloated schema centric parser, while adhering to the HL7 standard.

It is a based on fork of
https://github.com/Efferent-Health/HL7-dotnetcore

That library is of course excellent by itself, and well documented. But since I have the need to refactor and enhance the library (for use in a larger framework), I have created this seperately maintained fork.
For credits for the original codebase(s) check the end of this readme.

## New in 1.1.6 (1 july 2022)

Bugfix : GetValue() for data in a removed segment would throw a 'Sequence contains no elements' exception

## New in 1.1.5 (20 nov 2021)

Minor change to allow (custom) segment names that end with a '0' e.g. 'ZZ0'. The previous version only allowed a letter or '1-9' for the third character.

## New in 1.1.4 (20 nov 2021)

Github action for build & auto deploy nuget (no real changes)

## New in 1.1.3 (2 nov 2021)

Minor bugfix. Method message.HasRepetitions() works now as expected.

## New in 1.1.2 (27 may 2021)

## RemoveTrailingDelimiters()

You can now remove all trailing delimiters at once in a parsed message, as well as on each message element serperately....

````cSharp
var message = new Message(clutteredMessage);
message.ParseMessage();
message.RemoveTrailingDelimiters(RemoveDelimitersOptions.All);
`````


## New in 1.1.1 (29 apr 2021)

## PutValue(), ValueExists(), EnsureXXXX(), SwapFields()

Quite some refactoring, to make the new method PutValue() work for easier manipulation of values. The new method will autocreate fields/(sub)components that are needed to set values. You can also Check for existence of values.

````cSharp
if (message.ValueExists("ZZ1.2"))
    message.PutValue("ZZ1.2.4","SYSTEM59") // SetValue would throw an exception
`````

You can  also autocreate fields/components/... with various 'Ensure...' methods e.g. (check the tests for more examples)

````cSharp
message.DefaultSegment("ZZ1").EnsureField(5).Value = "X"; // Makes sure the 5h field exists and returns it
`````

And finally, you can swap 2 fields in a segment
````cSharp
message.DefaultSegment("ZZ1").SwapFields(4,5);
`````


Manipulation now also directly affects GetValue() (it used to return the original parsed values, not the new ones)


## Object construction

### Create a Message object and pass raw HL7 message in text format

````cSharp
Message message = new Message(strMsg);

// Parse this message

try
{
    message.ParseMessage();
}
catch(Exception ex)
{
    // Handle the exception
}
`````

### Adding a message header

For adding a header segment to a new message object, use the `AddSegmentMSH()` method, after constructing an empty message:

````cSharp
message.AddSegmentMSH(sendingApplication, sendingFacility, 
    receivingApplication, receivingFacility,
    security, messageType, 
    messageControlId, processingId, version);
````

### Message extraction

If the HL7 message is coming from a MLLP connection (see [the official documentation]( www.hl7.org/documentcenter/public/wg/inm/mllp_transport_specification.PDF)), the message needs to be cleared from the MLLP prefixes and suffixes. Also, consider there can be more than one message in a single MLLP frame.

For this purpose, there is an `ExtractMessages()` method, to be used as follows:

````cSharp
// extract the messages from a buffer containing a MLLP frame
var messages = MessageHelper.ExtractMessages(buffer);

// construct and process each message
foreach (var strMsg in messages)
{
    Message message = new Message(strMsg);
    message.ParseMessage(); // Required by most operations
    // do something with the message object
}
````

## Accessing Segments

### Get list of all segments

````cSharp
List<Segment> segList = message.Segments();
`````

### Get List of list of repeated segments by name 

For example if there are multiple IN1 segments

````cSharp
List<Segment> IN1List = message.Segments("IN1");
````

### Access a particular occurrence from multiple IN1s providing the index

Note index 1 will return the 2nd element from list

````cSharp
Segment IN1_2 = message.Segments("IN1")[1];
````

### Get count of IN1s

````cSharp
int countIN1 = message.Segments("IN1").Count;
````

### Access first occurrence of any segment

````cSharp
Segment IN1 = message.DefaultSegment("IN1");

// OR

Segment IN1 = message.Segments("IN1")[0];
````

## Accessing Fields

### Access field values

````cSharp
string SendingFacility = message.GetValue("MSH.4");

// OR

string SendingFacility = message.DefaultSegment("MSH").Fields(4).Value;

// OR

string SendingFacility = message.Segments("MSH")[0].Fields(4).Value;
`````

### Check if field is componentized

````cSharp
bool isComponentized = message.Segments("PID")[0].Fields(5).IsComponentized;

// OR

bool isComponentized = message.IsComponentized("PID.5");
`````

### Check if field has repetitions

````cSharp
bool isRepeated = message.Segments("PID")[0].Fields(3).HasRepetitions;

// OR

bool isRepeated = message.HasRepetitions("PID.3");
````

### Get list of repeated fields

````cSharp
List<Field> repList = message.Segments("PID")[0].Fields(3).Repetitions();
````

### Get particular repetition i.e 2nd repetition of PID.3

````cSharp
Field PID3_R2 = message.Segments("PID.3[2]");

// OR

Field PID3_R2 = message.Segments("PID.3(2)");
````

### Update value of any field i.e. to update PV1.2 – patient class

````cSharp
message.SetValue("PV1.2", "I");

// OR

message.Segments("PV1")[0].Fields(2).Value = "I";
````

### Access some of the required MSH fields with properties

````cSharp
string version = message.Version;
string msgControlID = message.MessageControlID;
string messageStructure = message.MessageStructure;
````

### Generate ACKs

To generate an ACK message

````cSharp
Message ack = message.GetACK();
````

To generate negative ACK (NACK) message with error message

````cSharp
Message nack = message.GetNACK("AR", "Invalid Processing ID");
````

It may be required to change the application and facility fields

````cSharp
Message ack = message.GetACK();
ack.SetValue("MSH.3", appName);
ack.SetValue("MSH.4", facility);

````

Take into account that a message shall be previously parsed before attempting to generate an ACK or NACK message.

## Accessing Components

### Access particular component i.e. PID.5.1 – Patient Family Name

````cSharp
string PatName1 = message.GetValue("PID.5.1");

// OR

string PatName1 = message.Segments("PID")[0].Fields(5).Components(1).Value;
````

### Check if component is sub componentized

````cSharp
bool isSubComponentized = message.Segments("PV1")[0].Fields(7).Components(1).IsSubComponentized;

// OR

bool isSubComponentized = message.IsSubComponentized("PV1.7.1");
````

### Update value of any component

````cSharp
message.Segments("PID")[0].Fields(5).Components(1).Value = "Jayant";

// OR

message.SetValue("PID.5.1", "Jayant");
````

### Adding new Segment

````cSharp
// Create a Segment with name ZIB
Segment newSeg = new Segment("ZIB");
 
// Create Field ZIB_1
Field ZIB_1 = new Field("ZIB1");
// Create Field ZIB_5
Field ZIB_5 = new Field("ZIB5");
 
// Create Component ZIB.5.2
Component com1 = new Component("ZIB.5.2");
 
// Add Component ZIB.5.2 to Field ZIB_5
// 2nd parameter here specifies the component position, for inserting segment on particular position
// If we don’t provide 2nd parameter, component will be inserted to next position (if field has 2 components this will be 3rd, 
// If field is empty this will be 1st component
ZIB_5.AddNewComponent(com1, 2);
 
// Add Field ZIB_1 to segment ZIB, this will add a new filed to next field location, in this case first field
newSeg.AddNewField(ZIB_1);
 
// Add Field ZIB_5 to segment ZIB, this will add a new filed as 5th field of segment
newSeg.AddNewField(ZIB_5, 5);
 
// Add segment ZIB to message
bool success = message.AddNewSegment(newSeg);
````

New Segment would look like this:

````text
ZIB|ZIB1||||ZIB5^ZIB.5.2
````

After evaluated and modified required values, the message can be obtained again in text format

````cSharp
string strUpdatedMsg = message.SerializeMessage();
````

### Remove Trailing Delimiters

You can remove all trailing delimiters at once in a parsed message, as well as on each message element serperately....

````cSharp
var message = new Message(clutteredMessage);
message.ParseMessage();
message.RemoveTrailingDelimiters(RemoveDelimitersOptions.All);
`````

for example

````text
ZZA|1||^^&~^^&~~||~~~^&&|||&&|^^
ZZB|2||^||~~~^&&||123^|^^&X~^^&~~|^Y^^|^||~~~^&&|||
````

would be simplified to

````text
ZZA|1
ZZB|2||||||123|^^&X|^Y
````

### Remove a Segment

Segments are removed individually, including the case where there are repeated segments with the same name

````cSharp
    // Remove the first segment with name NK1
    bool success = message.RemoveSegment("NK1") 

    // Remove the second segment with name NK1
    bool success = message.RemoveSegment("NK1", 1) 
````

### Encoded segments

Some contents may contain forbidden characters like pipes and ampersands. Whenever there is a possibility of having those characters, the content shall be encoded before calling the 'AddNew' methods, like in the following code:

````cSharp
var obx = new Segment("OBX", new HL7Encoding());

// Not encoded. Will be split into parts.
obx.AddNewField("70030^Radiologic Exam, Eye, Detection, FB^CDIRadCodes");  

// Encoded. Won't be parsed nor split.
obx.AddNewField(obx.Encoding.Encode("domain.com/resource.html?Action=1&ID=2"));  
````

### Copying a segment

The DeepCopy method allows to perform a clone of a segment when building new messages. Countersense, if a segment is referenced directly when adding segments to a message, a change in the segment will affect both the origin and new messages.

````cSharp
Segment pid = ormMessage.DefaultSegment("PID").DeepCopy();
oru.AddNewSegment(pid);
````

### Null elements

Null elements (fields, components or subcomponents), also referred to as Present But Null, are expressed in HL7 messages as double quotes, like (see last field):

````
EVN|A04|20110613083617||""
````

Whenever requested individually, those elements are returned as `null`, rather than double quotes:

````cSharp
var expectEmpty = evn.Fields(3).Value; // Will return an empty string
var expectNull = evn.Fields(4).Value; // Will return null
````

### Date Handling

A couple of date handling methods have been added, for parsing elements containing valid date/times, including time zones, as described in [the HL7 standard](http://www.hl7.eu/refactored/dtDTM.html). Examples:

````csharp
// With time zone
string value1 = "20151231234500.1234+2358";
TimeSpan offset;
DateTime? dt1 = MessageHelper.ParseDateTime(value1, out offset);

// Date/time only
string value2 = "20151231234500";
DateTime? dt2 = MessageHelper.ParseDateTime(value2);
````

`ParseDateTime` will catch exceptions by default and return `null` in case of invalid dates. For preventing this mechanism, add an extra argument as `true`, like:

````csharp
try
{
    var dt1 = MessageHelper.ParseDateTime(value1, out TimeSpan offse, true);
    var dt2 = MessageHelper.ParseDateTime(value2, true);
}
catch 
{
    // do something here
}
````

## Credits

This is a forl from HL7-dotnetcore
https://github.com/Efferent-Health/HL7-dotnetcore
Which is a nicely evolved fork from Jayant Singh's HL7 parser.

For more information about that original implementation read:
- https://github.com/j4jayant/hl7-cSharp-parser
- http://j4jayant.com/articles/hl7/31-hl7-parsing-lib

The field encoding and decoding methods have been based on https://github.com/elomagic/hl7inspector

## Breaking changes

Since HL7lite version 1.0, the interface of the Parse() method doens't return a boolean, it allways throws on failure. If you want to skip the 'serialization check' (which catches bad escape sequences) for some reason, you can pass 'false' for the serializeCheck parameter (default = true).

### Older breaking changes from the previous fork https://github.com/Efferent-Health/HL7-dotnetcore

Since version 2.9, the MSH segment will have an extra field at the beginning of the segment list, containing the field separator. This is according to [the HL7 standard]( https://www.hl7.org/documentcenter/public_temp_CACD15D9-1C23-BA17-0C050D19F5A35765/wg/conf/HL7MSH.htm), as mentioned in Issue #26. Every field index in that segment should be increased by one.

Since version 2.9, some previously deprecated methods starting with lowercase have been removed. The replacement methods starting with uppercase shall be used instead.

Since version 2.21, message.GetValue() will decode the returned content.
