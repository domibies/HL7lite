using System;
using Xunit;

namespace HL7lite.Test
{
    public class EncodingTests
    {
        // Helper method for common exception assertions
        private static void AssertThrowsHL7Exception(Action action, string expectedErrorCode)
        {
            var ex = Assert.Throws<HL7Exception>(action);
            Assert.Equal(expectedErrorCode, ex.ErrorCode);
        }
        [Fact]
        public void DefaultEncoding_ShouldHaveStandardDelimiters()
        {
            var encoding = new HL7Encoding();
            
            Assert.Equal('|', encoding.FieldDelimiter);
            Assert.Equal('^', encoding.ComponentDelimiter);
            Assert.Equal('~', encoding.RepeatDelimiter);
            Assert.Equal('\\', encoding.EscapeCharacter);
            Assert.Equal('&', encoding.SubComponentDelimiter);
            Assert.Equal("\r", encoding.SegmentDelimiter);
            Assert.Equal("\"\"", encoding.PresentButNull);
        }

        [Fact]
        public void EvaluateDelimiters_ShouldSetAllDelimitersCorrectly()
        {
            var encoding = new HL7Encoding();
            encoding.EvaluateDelimiters("|^~\\&");
            
            Assert.Equal('|', encoding.FieldDelimiter);
            Assert.Equal('^', encoding.ComponentDelimiter);
            Assert.Equal('~', encoding.RepeatDelimiter);
            Assert.Equal('\\', encoding.EscapeCharacter);
            Assert.Equal('&', encoding.SubComponentDelimiter);
        }

        [Theory]
        [InlineData("test\r\ndata", "\r\n")]
        [InlineData("test\n\rdata", "\n\r")]
        [InlineData("test\rdata", "\r")]
        [InlineData("test\ndata", "\n")]
        public void EvaluateSegmentDelimiter_ShouldDetectCorrectDelimiter(string message, string expectedDelimiter)
        {
            var encoding = new HL7Encoding();
            encoding.EvaluateSegmentDelimiter(message);
            
            Assert.Equal(expectedDelimiter, encoding.SegmentDelimiter);
        }

        [Fact]
        public void EvaluateSegmentDelimiter_WithNoDelimiter_ShouldThrowException()
        {
            var encoding = new HL7Encoding();
            
            AssertThrowsHL7Exception(() => encoding.EvaluateSegmentDelimiter("testdata"), HL7Exception.BAD_MESSAGE);
        }

        [Theory]
        [InlineData(null, "\"\"")]
        [InlineData("", "")]
        [InlineData("  ", "  ")]
        [InlineData("normal text", "normal text")]
        public void Encode_BasicValues_ShouldHandleCorrectly(string input, string expected)
        {
            var encoding = new HL7Encoding();
            var result = encoding.Encode(input);
            
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("field|delimiter", "field\\F\\delimiter")]
        [InlineData("component^delimiter", "component\\S\\delimiter")]
        [InlineData("repeat~delimiter", "repeat\\R\\delimiter")]
        [InlineData("escape\\character", "escape\\E\\character")]
        [InlineData("subcomponent&delimiter", "subcomponent\\T\\delimiter")]
        public void Encode_SpecialCharacters_ShouldEscapeCorrectly(string input, string expected)
        {
            var encoding = new HL7Encoding();
            var result = encoding.Encode(input);
            
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Encode_AllDelimitersInOneString_ShouldEscapeAll()
        {
            var encoding = new HL7Encoding();
            var input = "test|with^all~delimiters\\and&subcomponents";
            var expected = "test\\F\\with\\S\\all\\R\\delimiters\\E\\and\\T\\subcomponents";
            
            var result = encoding.Encode(input);
            
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("test\rdata", "test\\X0D\\data")]
        [InlineData("test\ndata", "test\\X0A\\data")]
        [InlineData("test\r\ndata", "test\\X0D\\\\X0A\\data")]
        public void Encode_NewlineCharacters_ShouldConvertToHex(string input, string expected)
        {
            var encoding = new HL7Encoding();
            var result = encoding.Encode(input);
            
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("<B>bold</B>", "\\H\\bold\\N\\")]
        [InlineData("before<B>bold</B>after", "before\\H\\bold\\N\\after")]
        [InlineData("line1<BR>line2", "line1\\.br\\line2")]
        public void Encode_HtmlTags_ShouldConvertCorrectly(string input, string expected)
        {
            var encoding = new HL7Encoding();
            var result = encoding.Encode(input);
            
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("  ", "  ")]
        [InlineData("normal text", "normal text")]
        [InlineData("\\F\\", "|")]
        [InlineData("\\S\\", "^")]
        [InlineData("\\R\\", "~")]
        [InlineData("\\E\\", "\\")]
        [InlineData("\\T\\", "&")]
        public void Decode_BasicEscapeSequences_ShouldDecodeCorrectly(string input, string expected)
        {
            var encoding = new HL7Encoding();
            var result = encoding.Decode(input);
            
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("\\H\\bold\\N\\", "<B>bold</B>")]
        [InlineData("\\.br\\", "<BR>")]
        [InlineData("\\X0D\\", "\r")]
        [InlineData("\\X0A\\", "\n")]
        [InlineData("\\X0D\\\\X0A\\", "\r\n")]
        public void Decode_SpecialSequences_ShouldDecodeCorrectly(string input, string expected)
        {
            var encoding = new HL7Encoding();
            var result = encoding.Decode(input);
            
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Decode_IncompleteEscapeSequence_ShouldHandleGracefully()
        {
            var encoding = new HL7Encoding();
            var input = "test\\incomplete";
            var expected = "test\\incomplete";
            
            var result = encoding.Decode(input);
            
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Decode_EmptyEscapeSequence_ShouldIgnore()
        {
            var encoding = new HL7Encoding();
            var input = "test\\\\data";
            var expected = "testdata";
            
            var result = encoding.Decode(input);
            
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("test|with^all~delimiters\\and&subcomponents")]
        [InlineData("<B>bold text</B> and <BR> and normal")]
        [InlineData("line1\r\nline2\nline3")]
        [InlineData("special\\unknown\\sequence")]
        public void EncodeDecode_RoundTrip_ShouldPreserveData(string original)
        {
            var encoding = new HL7Encoding();
            
            var encoded = encoding.Encode(original);
            var decoded = encoding.Decode(encoded);
            
            Assert.Equal(original, decoded);
        }

        [Fact]
        public void CustomEncoding_ShouldWorkWithDifferentDelimiters()
        {
            var encoding = new HL7Encoding
            {
                FieldDelimiter = '#',
                ComponentDelimiter = '@',
                RepeatDelimiter = '*',
                EscapeCharacter = '!',
                SubComponentDelimiter = '$'
            };

            var input = "field#with@component*repeat!escape$subcomponent";
            var expected = "field!F!with!S!component!R!repeat!E!escape!T!subcomponent";
            
            var result = encoding.Encode(input);
            
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Message_ParsedElements_ShareSameEncodingReference()
        {
            // Parse a complete message - all elements should share encoding reference
            var messageText = "MSH|^~\\&|SendingApp|SendingFacility|ReceivingApp|ReceivingFacility|20230101120000||ADT^A01|12345|P|2.5\r" +
                             "PID|||123456||Doe^John||19800101|M";
            
            var message = new Message(messageText);
            message.ParseMessage();
            
            var mshSegment = message.DefaultSegment("MSH");
            var pidSegment = message.DefaultSegment("PID");
            
            // All segments from parsed message share the same encoding reference
            Assert.Same(message.Encoding, mshSegment.Encoding);
            Assert.Same(message.Encoding, pidSegment.Encoding);
            
            // Fields also share the encoding
            var mshField = mshSegment.Fields(3); // Sending app field
            var pidField = pidSegment.Fields(5); // Name field
            Assert.Same(message.Encoding, mshField.Encoding);
            Assert.Same(message.Encoding, pidField.Encoding);
            
            // Components also share the encoding
            var msgTypeField = mshSegment.Fields(9); // Message type field (ADT^A01)
            if (msgTypeField.IsComponentized)
            {
                Assert.Same(message.Encoding, msgTypeField.Components(1).Encoding);
            }
        }

        [Fact]
        public void Message_ChangingEncodingAfterParsing_AffectsAllElements()
        {
            // Parse a message with standard encoding
            var messageText = "MSH|^~\\&|SendingApp|SendingFacility|ReceivingApp|ReceivingFacility|20230101120000||ADT^A01|12345|P|2.5\r";
            
            var message = new Message(messageText);
            message.ParseMessage();
            
            // Get references to existing elements
            var mshSegment = message.DefaultSegment("MSH");
            var appField = mshSegment.Fields(3);
            
            // Store original encoding values to verify they change
            var originalFieldDelimiter = mshSegment.Encoding.FieldDelimiter;
            var originalComponentDelimiter = appField.Encoding.ComponentDelimiter;
            
            // Verify original encoding is used
            Assert.Equal('|', originalFieldDelimiter);
            Assert.Equal('^', originalComponentDelimiter);
            
            // Change message encoding properties directly
            message.Encoding.FieldDelimiter = '#';
            message.Encoding.ComponentDelimiter = '@';
            
            // Check that message has new encoding
            Assert.Equal('#', message.Encoding.FieldDelimiter);
            
            // All elements share the same encoding reference, so they all change
            Assert.Equal('#', mshSegment.Encoding.FieldDelimiter);
            Assert.Equal('@', appField.Encoding.ComponentDelimiter);
            
            // This shows that all elements share the same encoding reference
            Assert.Same(message.Encoding, mshSegment.Encoding);
            Assert.Same(message.Encoding, appField.Encoding);
        }

        [Fact]
        public void Message_SerializeWithDifferentEncoding_ShouldUseOriginalElementEncoding()
        {
            // Create message with custom encoding
            var customEncoding = new HL7Encoding
            {
                FieldDelimiter = '#',
                ComponentDelimiter = '@',
                RepeatDelimiter = '*',
                EscapeCharacter = '!',
                SubComponentDelimiter = '$'
            };

            var message = new Message();
            message.Encoding = customEncoding;
            message.AddSegmentMSH("App", "Facility", "App2", "Facility2", 
                "123", "ADT@A01", "456", "P", "2.5");
            
            // Add a segment with special characters that need encoding
            var pid = new Segment("PID", customEncoding);
            pid.AddNewField(new Field("", customEncoding)); // PID-1
            pid.AddNewField(new Field("", customEncoding)); // PID-2
            pid.AddNewField(new Field("123456", customEncoding)); // PID-3
            pid.AddNewField(new Field("", customEncoding)); // PID-4
            
            // Add name with component delimiter
            var nameField = new Field(customEncoding);
            nameField.AddNewComponent(new Component("Doe", customEncoding));
            nameField.AddNewComponent(new Component("John@Middle", customEncoding)); // Contains delimiter
            pid.AddNewField(nameField); // PID-5
            
            message.AddNewSegment(pid);
            
            var serialized = message.SerializeMessage(false);
            
            // Should use custom delimiters
            Assert.Contains("MSH#@*!$#", serialized);
            Assert.Contains("PID###123456##Doe@John!S!Middle", serialized);
        }

        [Fact]
        public void Message_ParseAndReserializeWithCustomEncoding_ShouldPreserveContent()
        {
            // Create a message with custom encoding
            // The encoding will be automatically detected from the MSH segment
            var originalMessage = 
                "MSH#@*!$#SendApp#SendFac#RecApp#RecFac#20230101#SecField#ADT@A01#123#P#2.5\n" +
                "PID###12345##Doe@John@A*Smith@Jane@B##19800101#M";

            // Parse message - encoding is automatically detected from MSH
            var message = new Message(originalMessage);
            message.ParseMessage();
            
            // Verify encoding was properly detected
            Assert.Equal('#', message.Encoding.FieldDelimiter);
            Assert.Equal('@', message.Encoding.ComponentDelimiter);
            Assert.Equal('*', message.Encoding.RepeatDelimiter);
            Assert.Equal('!', message.Encoding.EscapeCharacter);
            Assert.Equal('$', message.Encoding.SubComponentDelimiter);
            
            // Verify parsing worked correctly
            Assert.Equal("12345", message.GetValue("PID.3"));
            Assert.Equal("Doe", message.GetValue("PID.5.1"));
            Assert.Equal("John", message.GetValue("PID.5.2"));
            Assert.Equal("Smith", message.GetValue("PID.5(2).1"));
            
            // Reserialize and compare
            var reserialized = message.SerializeMessage(false);
            // Compare after trimming trailing newlines
            Assert.Equal(originalMessage.TrimEnd('\r', '\n'), reserialized.TrimEnd('\r', '\n'));
        }

        [Fact]
        public void AllDelimiters_Property_ShouldReturnCorrectString()
        {
            var encoding = new HL7Encoding();
            Assert.Equal("|^~\\&", encoding.AllDelimiters);
            
            var customEncoding = new HL7Encoding
            {
                FieldDelimiter = '#',
                ComponentDelimiter = '@',
                RepeatDelimiter = '*',
                EscapeCharacter = '!',
                SubComponentDelimiter = '$'
            };
            Assert.Equal("#@*!$", customEncoding.AllDelimiters);
        }

        [Fact]
        public void MSH_FieldDelimiter_AutomaticallyUpdatesFromEncoding()
        {
            // Parse a standard message
            var message = new Message("MSH|^~\\&|App|Fac|App2|Fac2|20230101||ADT^A01|123|P|2.5\r");
            message.ParseMessage();
            
            // Verify MSH.1 contains the field delimiter
            Assert.Equal("|", message.GetValue("MSH.1"));
            
            // Change encoding field delimiter
            message.Encoding.FieldDelimiter = '#';
            
            // MSH.1 still returns the original value when accessed
            Assert.Equal("|", message.GetValue("MSH.1"));
            
            // But when serialized, it uses the new delimiter automatically
            var serialized = message.SerializeMessage(false);
            Assert.StartsWith("MSH#", serialized);
            
            // And the rest of the message uses the new delimiter
            Assert.Contains("#App#Fac#App2#Fac2#", serialized);
        }

        [Fact]
        public void Message_FullRoundTrip_StandardToCustomToStandard_PreservesContent()
        {
            // Start with a standard encoded HL7 message with various features
            var originalMessage = 
                "MSH|^~\\&|SendingApp|SendingFacility|ReceivingApp|ReceivingFacility|20230101120000||ADT^A01^ADT_A01|12345|P|2.5|123456||EN|AL|USA|ASCII|en^English^ISO639\r" +
                "EVN|A01|20230101120000||ADM|Smith^John^J^III^Dr^MD|20230101120000|University Hospital\r" +
                "PID|1||123456789^^^Hospital^MRN~987654321^^^State^DL||Doe^Jane^Marie^Jr^Miss|Smith|19800515|F||W^White^HL70005|123 Main St^Apt 4B^Anytown^NY^12345^USA^H~456 Work Ave^Suite 100^Worktown^NY^12346^USA^O||^PRN^PH^(555)123-4567~^NET^Internet^jane.doe@email.com|^WPN^PH^(555)987-6543|en^English^ISO639|M^Married^HL70002|CHR^Christian^HL70006|111223333|123-45-6789|||N^Not Hispanic^HL70189|USA^United States^ISO3166|N|1||||||||||||||||||\r" +
                "PV1|1|I|3N^301^A^University Hospital^^^^^3rd Floor North|R|||12345^Smith^Robert^R^Jr^Dr^MD~67890^Jones^Sarah^S^^Dr^DO|98765^White^William^W^Sr^Dr^MD|MED^Medicine^HL70069||||A|||12345^Smith^Robert^R^Jr^Dr^MD|INP^Inpatient^HL70007|SELF^Self Pay^HL70064||||||||||||||||||||||||||20230101120000|20230105|3|4|2||||||\r" +
                "NK1|1|Doe^John^A|SPO^Spouse^HL70063|123 Main St^Apt 4B^Anytown^NY^12345^USA^H|^PRN^PH^(555)123-9999|^WPN^PH^(555)321-9999|EC^Emergency Contact^HL70131|||||||||||||||||||||||||||\r" +
                "OBX|1|ST|1234-5^Test Name^LN|1|Normal Value|units|N||N|||F|||20230101120000|LAB^Laboratory^L";

            // Step 1: Parse the message with standard encoding
            var message1 = new Message(originalMessage);
            message1.ParseMessage();
            
            // Verify standard encoding was detected
            Assert.Equal('|', message1.Encoding.FieldDelimiter);
            Assert.Equal('^', message1.Encoding.ComponentDelimiter);
            Assert.Equal('~', message1.Encoding.RepeatDelimiter);
            Assert.Equal('\\', message1.Encoding.EscapeCharacter);
            Assert.Equal('&', message1.Encoding.SubComponentDelimiter);
            
            // Step 2: Change to custom encoding and update MSH.2 field
            message1.Encoding.FieldDelimiter = '#';
            message1.Encoding.ComponentDelimiter = '@';
            message1.Encoding.RepeatDelimiter = '*';
            message1.Encoding.EscapeCharacter = '!';
            message1.Encoding.SubComponentDelimiter = '$';
            
            // Update MSH.2 to reflect new encoding
            message1.SetValue("MSH.2", "@*!$");
            
            var customEncodedString = message1.SerializeMessage(false);
            
            // Verify the custom encoded string uses new delimiters
            Assert.Contains("MSH#@*!$#", customEncodedString);
            Assert.Contains("Doe@Jane@Marie@Jr@Miss", customEncodedString);
            Assert.Contains("123456789@@@Hospital@MRN*987654321@@@State@DL", customEncodedString);
            
            // Step 3: Parse the custom encoded message
            var message2 = new Message(customEncodedString);
            message2.ParseMessage();
            
            // Verify custom encoding was detected
            Assert.Equal('#', message2.Encoding.FieldDelimiter);
            Assert.Equal('@', message2.Encoding.ComponentDelimiter);
            Assert.Equal('*', message2.Encoding.RepeatDelimiter);
            Assert.Equal('!', message2.Encoding.EscapeCharacter);
            Assert.Equal('$', message2.Encoding.SubComponentDelimiter);
            
            // Verify content is preserved with custom encoding
            Assert.Equal("Jane", message2.GetValue("PID.5.2"));
            Assert.Equal("123456789", message2.GetValue("PID.3.1"));
            Assert.Equal("987654321", message2.GetValue("PID.3(2).1"));
            Assert.Equal("Normal Value", message2.GetValue("OBX.5"));
            
            // Step 4: Change back to standard encoding and update MSH.2
            message2.Encoding.FieldDelimiter = '|';
            message2.Encoding.ComponentDelimiter = '^';
            message2.Encoding.RepeatDelimiter = '~';
            message2.Encoding.EscapeCharacter = '\\';
            message2.Encoding.SubComponentDelimiter = '&';
            
            // Update MSH.2 to reflect standard encoding
            message2.SetValue("MSH.2", "^~\\&");
            
            var finalMessage = message2.SerializeMessage(false);
            
            // Step 5: Compare original and final messages (ignoring trailing segment delimiters)
            Assert.Equal(originalMessage.TrimEnd('\r', '\n'), finalMessage.TrimEnd('\r', '\n'));
            
            // Additional verification: Parse both and compare specific values
            var originalParsed = new Message(originalMessage);
            originalParsed.ParseMessage();
            var finalParsed = new Message(finalMessage);
            finalParsed.ParseMessage();
            
            // Compare various fields to ensure complete preservation
            Assert.Equal(originalParsed.GetValue("MSH.9.1"), finalParsed.GetValue("MSH.9.1"));
            Assert.Equal(originalParsed.GetValue("PID.5.1"), finalParsed.GetValue("PID.5.1"));
            Assert.Equal(originalParsed.GetValue("PID.5.2"), finalParsed.GetValue("PID.5.2"));
            Assert.Equal(originalParsed.GetValue("PID.3.1"), finalParsed.GetValue("PID.3.1"));
            Assert.Equal(originalParsed.GetValue("PID.3(2).1"), finalParsed.GetValue("PID.3(2).1"));
            Assert.Equal(originalParsed.GetValue("PID.11.1"), finalParsed.GetValue("PID.11.1"));
            Assert.Equal(originalParsed.GetValue("PID.11(2).1"), finalParsed.GetValue("PID.11(2).1"));
            Assert.Equal(originalParsed.GetValue("OBX.5"), finalParsed.GetValue("OBX.5"));
        }
    }
}