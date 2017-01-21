# HL7-dotnetcore

This is a fork from Jayant Singh's HL7 parser. For more information read:
- https://github.com/j4jayant/hl7-cSharp-parser
- http://j4jayant.com/articles/hl7/31-hl7-parsing-lib

This library treats every message in same manner while parsing HL7 messages. After successfull parsing it provides all the components of HL7 message like segments, fields (with repetitions), components, subcomponents in easily accessible way.

### Create a Message object and pass raw HL7 message in text format

````cSharp
Message message = new Message(strMsg);

Parse this message

bool isParsed = false;
try
{
    isParsed = message.ParseMessage();
}
catch(Exception ex)
{
    // Handle the exception
}
`````

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
String SendingFacility = message.getValue("MSH.4");

// OR

String SendingFacility = message.DefaultSegment("MSH").Fields(4).Value;

// OR

String SendingFacility = message.Segments("MSH")[0].Fields(4).Value;
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

bool isRepeated = message.HasRepeatitions("PID.3");
````

### Get list of repeated fields

````cSharp
List<Field> repList = message.Segments("PID")[0].Fields(3).Repetitions();
````

### Get particular repetition i.e 2nd repetition of PID.3

````cSharp
Field PID3_R2 = message.Segments("PID")[0].Fields(3).Repetitions(2);
````

### Update value of any field i.e. to update PV1.2 – patient class

````cSharp
message.setValue("PV1.2", "I");

// OR

message.Segments("PV1"[0];).Fields(2).Value = "I";
````

### Access some of the required MSH fields with properties

````cSharp
String version = message.Version;
String msgControlID = message.MessageControlID;
String messageStructure = message.MessageStructure;
````

## Accessing Components

### Access particular component i.e. PID.5.1 – Patient Family Name

````cSharp
String PatName1 = message.getValue("PID.5.1");

// OR

String PatName1 = message.Segments("PID")[0].Fields(5).Components(1).Value;
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

message.setValue("PID.5.1", "Jayant");
````

### Adding new Segment

````cSharp
//Create a Segment with name ZIB
Segment newSeg = new Segment("ZIB");
 
//Create Field ZIB_1
Field ZIB_1 = new Field("ZIB1");
//Create Field ZIB_5
Field ZIB_5 = new Field("ZIB5");
 
//Create Component ZIB.5.2
Component com1 = new Component("ZIB.5.2");
 
//Add Component ZIB.5.2 to Field ZIB_5
//2nd parameter here specifies the component position, for inserting segment on particular position
//If we don’t provide 2nd parameter, component will be inserted to next position (if field has 2 components this will be 3rd, 
//if field is empty this will be 1st component
ZIB_5.AddNewComponent(com1, 2);
 
//Add Field ZIB_1 to segment ZIB, this will add a new filed to next field location, in this case first field
newSeg.AddNewField(ZIB_1);
 
//Add Field ZIB_5 to segment ZIB, this will add a new filed as 5th field of segment
newSeg.AddNewField(ZIB_5, 5);
 
//add segment ZIB to message
message.AddNewSegment(newSeg);
````

New Segment would look like this:

````text
ZIB|ZIB1||||^ZIB.5.2
````

After evaluated and modified required values, the message can be obtained again in text format

````cSharp
string strUpdatedMsg = message.SerializeMessage();
````

### Copying a segment

The DeepCopy method allows to perform a clone of a segment when building new messages. Countersense, if a segment is referenced directly when adding segments to a message, a change in the segment will affect both the origin and new messages.

````cSharp
Segment pid = ormMessage.DefaultSegment("PID").DeepCopy();
oru.AddNewSegment(pid);
````

### Generate ACKs

AA ACK

````cSharp
string ackMsg = message.getACK();
````

To generate negative ACK message with error message


````cSharp
string ackMsg = message.getNACK("AR", "Invalid Processing ID");
````
