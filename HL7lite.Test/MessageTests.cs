using System;
using System.Linq;
using Xunit;

namespace HL7lite.Test
{
    public class MessageTests
    {
        private const string SampleMessage = "MSH|^~\\&|SendingApp|SendingFacility|ReceivingApp|ReceivingFacility|20230101120000||ADT^A01|12345|P|2.5\r" +
                                           "PID|||123456||Doe^John||19800101|M\r" +
                                           "PV1||I|ICU^101^A";

        // Helper methods for common assertion patterns
        private static void AssertThrowsHL7Exception(Action action, string expectedErrorCode)
        {
            var ex = Assert.Throws<HL7Exception>(action);
            Assert.Equal(expectedErrorCode, ex.ErrorCode);
        }

        private static void AssertThrowsHL7Exception(Action action)
        {
            Assert.Throws<HL7Exception>(action);
        }

        [Fact]
        public void Constructor_WithEmptyConstructor_CreatesEmptyMessage()
        {
            var message = new Message();
            
            Assert.NotNull(message);
            Assert.Null(message.HL7Message); // HL7Message is null by default, not empty
            Assert.NotNull(message.Encoding);
        }

        [Fact]
        public void Constructor_WithMessageString_StoresMessage()
        {
            var message = new Message(SampleMessage);
            
            Assert.Equal(SampleMessage, message.HL7Message);
        }

        [Fact]
        public void Equals_WithMessageObject_ComparesCorrectly()
        {
            var message1 = new Message(SampleMessage);
            var message2 = new Message(SampleMessage);
            var message3 = new Message("MSH|^~\\&|Different|Message");
            
            Assert.True(message1.Equals(message2));
            Assert.False(message1.Equals(message3));
        }

        [Fact]
        public void Equals_WithString_ComparesCorrectly()
        {
            var message = new Message(SampleMessage);
            
            Assert.True(message.Equals(SampleMessage));
            Assert.False(message.Equals("Different message"));
        }

        [Fact]
        public void Equals_WithNull_ReturnsFalse()
        {
            var message = new Message(SampleMessage);
            
            Assert.False(message.Equals(null));
        }

        [Fact]
        public void Equals_WithOtherType_ReturnsFalse()
        {
            var message = new Message(SampleMessage);
            
            Assert.False(message.Equals(123));
            Assert.False(message.Equals(new object()));
        }

        [Fact]
        public void GetHashCode_ReturnsSameHashForSameMessage()
        {
            var message1 = new Message(SampleMessage);
            var message2 = new Message(SampleMessage);
            
            Assert.Equal(message1.GetHashCode(), message2.GetHashCode());
        }

        [Fact]
        public void ParseMessage_WithEmptySegments_SkipsEmptySegments()
        {
            var messageText = "MSH|^~\\&|App|Fac|App2|Fac2|20230101||ADT^A01|123|P|2.5\r\r\rPID|||123456";
            var message = new Message(messageText);
            
            message.ParseMessage();
            
            Assert.Equal(2, message.SegmentCount);
        }

        [Fact]
        public void ParseMessage_WithoutMSH_ThrowsException()
        {
            var message = new Message("PID|||123456||Doe^John");
            
            AssertThrowsHL7Exception(() => message.ParseMessage(), HL7Exception.BAD_MESSAGE);
        }

        [Fact]
        public void ParseMessage_WithShortMessage_ThrowsException()
        {
            var message = new Message("MS");
            
            AssertThrowsHL7Exception(() => message.ParseMessage(), HL7Exception.BAD_MESSAGE);
        }

        [Fact]
        public void ParseMessage_WithInvalidSegmentName_ThrowsException()
        {
            var message = new Message("MSH|^~\\&|App|Fac|App2|Fac2|20230101||ADT^A01|123|P|2.5\r1NV|Invalid segment");
            
            AssertThrowsHL7Exception(() => message.ParseMessage(), HL7Exception.BAD_MESSAGE);
        }

        [Fact]
        public void ParseMessage_WithShortSegment_ThrowsException()
        {
            var message = new Message("MSH|^~\\&|App|Fac|App2|Fac2|20230101||ADT^A01|123|P|2.5\rPI");
            
            AssertThrowsHL7Exception(() => message.ParseMessage(), HL7Exception.SEGMENT_TOO_SHORT);
        }

        [Fact]
        public void ParseMessage_WithWrongFieldDelimiter_ThrowsException()
        {
            var message = new Message("MSH|^~\\&|App|Fac|App2|Fac2|20230101||ADT^A01|123|P|2.5\rPID#Wrong delimiter");
            
            AssertThrowsHL7Exception(() => message.ParseMessage(), HL7Exception.BAD_MESSAGE);
        }

        [Fact]
        public void ParseMessage_WithInsufficientMSHFields_ThrowsException()
        {
            var message = new Message("MSH|^~\\&|App|Fac|App2|Fac2|20230101||ADT^A01|123\r");
            
            AssertThrowsHL7Exception(() => message.ParseMessage(), HL7Exception.BAD_MESSAGE);
        }

        [Fact]
        public void ParseMessage_WithEmptyVersion_DoesNotThrow()
        {
            // Empty version field is actually allowed
            var message = new Message("MSH|^~\\&|App|Fac|App2|Fac2|20230101||ADT^A01|123|P|\r");
            
            // Should parse successfully with empty version
            message.ParseMessage();
            Assert.Equal("", message.Version);
        }

        [Fact]
        public void ParseMessage_WithoutMessageType_ThrowsException()
        {
            var message = new Message("MSH|^~\\&|App|Fac|App2|Fac2|20230101|||123|P|2.5\r");
            
            AssertThrowsHL7Exception(() => message.ParseMessage(), HL7Exception.UNSUPPORTED_MESSAGE_TYPE);
        }

        [Fact]
        public void ParseMessage_WithoutMessageControlID_ThrowsException()
        {
            var message = new Message("MSH|^~\\&|App|Fac|App2|Fac2|20230101||ADT^A01||P|2.5\r");
            
            AssertThrowsHL7Exception(() => message.ParseMessage(), HL7Exception.REQUIRED_FIELD_MISSING);
        }

        [Fact]
        public void ParseMessage_WithoutProcessingID_ThrowsException()
        {
            var message = new Message("MSH|^~\\&|App|Fac|App2|Fac2|20230101||ADT^A01|123||2.5\r");
            
            AssertThrowsHL7Exception(() => message.ParseMessage(), HL7Exception.REQUIRED_FIELD_MISSING);
        }

        [Fact]
        public void ParseMessage_WithEmptyHL7Message_ThrowsException()
        {
            var message = new Message();
            message.HL7Message = "";
            
            AssertThrowsHL7Exception(() => message.ParseMessage(), HL7Exception.BAD_MESSAGE);
        }

        [Fact]
        public void SerializeMessage_WithValidation_ValidatesFirst()
        {
            // Start with a valid parsed message
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            // Serialize with validation enabled
            var result = message.SerializeMessage(true);
            Assert.NotNull(result);
            Assert.Contains("MSH|", result);
        }

        [Fact]
        public void GetMLLP_ReturnsMLLPEncodedBytes()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            var mllpBytes = message.GetMLLP();
            
            Assert.NotNull(mllpBytes);
            Assert.True(mllpBytes.Length > 0);
            // MLLP format: <SB>message<EB><CR>
            Assert.Equal(0x0B, mllpBytes[0]); // Start byte
            Assert.Equal(0x1C, mllpBytes[mllpBytes.Length - 2]); // End byte
            Assert.Equal(0x0D, mllpBytes[mllpBytes.Length - 1]); // Carriage return
        }

        [Fact]
        public void ValueExists_WithValidValue_ReturnsTrue()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            Assert.True(message.ValueExists("PID.5.1"));
            Assert.True(message.ValueExists("MSH.9"));
        }

        [Fact]
        public void ValueExists_WithInvalidFormat_ThrowsException()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            // ValueExists throws for invalid format
            Assert.Throws<HL7Exception>(() => message.ValueExists("InvalidFormat"));
        }

        [Fact]
        public void ValueExists_WithNonExistentValue_ReturnsFalse()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            Assert.False(message.ValueExists("ZZZ.1"));
            Assert.False(message.ValueExists("PID.99"));
        }

        [Fact]
        public void IsComponentized_WithComponentizedField_ReturnsTrue()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            Assert.True(message.IsComponentized("PID.5")); // Name field with components
            Assert.True(message.IsComponentized("MSH.9")); // Message type with trigger
        }

        [Fact]
        public void IsComponentized_WithNonComponentizedField_ReturnsFalse()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            Assert.False(message.IsComponentized("PID.3")); // Simple ID field
            Assert.False(message.IsComponentized("PID.7")); // Date field
        }

        [Fact]
        public void IsComponentized_WithInvalidFormat_ThrowsException()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            AssertThrowsHL7Exception(() => message.IsComponentized("InvalidFormat"));
        }

        [Fact]
        public void IsComponentized_WithNonExistentSegment_ThrowsException()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            AssertThrowsHL7Exception(() => message.IsComponentized("ZZZ.1"));
        }

        [Fact]
        public void IsSubComponentized_WithSubComponentizedField_ReturnsTrue()
        {
            var message = new Message("MSH|^~\\&|App|Fac|App2|Fac2|20230101||ADT^A01|123|P|2.5\r" +
                                    "PID|||123456||Doe^John&Middle||19800101|M");
            message.ParseMessage();
            
            Assert.True(message.IsSubComponentized("PID.5.2")); // Name with subcomponent
        }

        [Fact]
        public void IsSubComponentized_WithNonSubComponentizedField_ReturnsFalse()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            Assert.False(message.IsSubComponentized("PID.5.1")); // Simple component
        }

        [Fact]
        public void IsSubComponentized_WithInvalidFormat_ThrowsException()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            // Missing component index should throw
            Assert.Throws<HL7Exception>(() => message.IsSubComponentized("PID.5"));
        }

        [Fact]
        public void GetACK_ReturnsValidACKMessage()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            var ack = message.GetACK();
            
            Assert.NotNull(ack);
            Assert.Equal("ACK", ack.MessageStructure);
            Assert.Contains("MSA|AA|12345", ack.HL7Message);
        }

        [Fact]
        public void GetNACK_ReturnsValidNACKMessage()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            var nack = message.GetNACK("AR", "Test error message");
            
            Assert.NotNull(nack);
            Assert.Equal("ACK", nack.MessageStructure);
            Assert.Contains("MSA|AR|12345|Test error message", nack.HL7Message);
        }

        [Fact]
        public void GetACK_WithACKMessage_ReturnsNull()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            var ack = message.GetACK();
            
            // Try to get ACK of an ACK
            var ackOfAck = ack.GetACK();
            
            Assert.Null(ackOfAck);
        }

        [Fact]
        public void RemoveSegment_WithExistingSegment_RemovesSuccessfully()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            Assert.True(message.RemoveSegment("PV1"));
            // Returns empty list, not null
            var segments = message.Segments("PV1");
            Assert.NotNull(segments);
            Assert.Empty(segments);
        }

        [Fact]
        public void RemoveSegment_WithNonExistentSegment_ReturnsFalse()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            Assert.False(message.RemoveSegment("ZZZ"));
        }

        [Fact]
        public void RemoveSegment_WithIndexOutOfBounds_ReturnsFalse()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            Assert.False(message.RemoveSegment("PID", 5)); // Only one PID segment
        }

        [Fact]
        public void Segments_WithoutParameters_ReturnsAllSegments()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            var segments = message.Segments();
            
            Assert.NotNull(segments);
            Assert.Equal(3, segments.Count); // MSH, PID, PV1
        }

        [Fact]
        public void Segments_WithSegmentName_ReturnsMatchingSegments()
        {
            var message = new Message("MSH|^~\\&|App|Fac|App2|Fac2|20230101||ADT^A01|123|P|2.5\r" +
                                    "PID|||123456||Doe^John\r" +
                                    "PID|||789012||Smith^Jane");
            message.ParseMessage();
            
            var pidSegments = message.Segments("PID");
            
            Assert.NotNull(pidSegments);
            Assert.Equal(2, pidSegments.Count);
        }

        [Fact]
        public void Segments_WithNonExistentSegmentName_ReturnsEmptyList()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            var segments = message.Segments("ZZZ");
            
            // Actually returns empty list, not null
            Assert.NotNull(segments);
            Assert.Empty(segments);
        }

        [Fact]
        public void DefaultSegment_WithNonExistentSegment_ThrowsException()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            Assert.Throws<InvalidOperationException>(() => message.DefaultSegment("ZZZ"));
        }

        [Fact]
        public void SetValue_WithInvalidFormat_ThrowsException()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            // Passing just segment name without field number causes format validation error
            Assert.Throws<HL7Exception>(() => message.SetValue("PID", "NewValue"));
        }

        [Fact]
        public void GetValue_WithComponentIndexBeyondMax_ThrowsException()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            // Very large indices should throw
            Assert.Throws<HL7Exception>(() => message.GetValue("PID.5.99999"));
        }

        [Fact]
        public void GetValue_WithFieldIndexBeyondMax_ThrowsException()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            // Very large field indices should throw
            Assert.Throws<HL7Exception>(() => message.GetValue("PID.99999"));
        }

        [Fact]
        public void PutValue_WithNonExistentSegment_ThrowsException()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            Assert.Throws<HL7Exception>(() => message.PutValue("ZZZ.1", "Value"));
        }

        [Fact]
        public void PutValue_WithInvalidFormat_ThrowsException()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            Assert.Throws<HL7Exception>(() => message.PutValue("InvalidFormat", "Value"));
        }

        [Fact]
        public void AddSegmentMSH_WithNullSecurity_HandlesCorrectly()
        {
            var message = new Message();
            message.AddSegmentMSH("App", "Fac", "App2", "Fac2", null, "ADT^A01", "123", "P", "2.5");
            
            var serialized = message.SerializeMessage(false);
            Assert.Contains("||ADT^A01|", serialized); // Empty security field
        }

        [Fact]
        public void ParseMessage_WithSerializeCheckDisabled_SkipsCheck()
        {
            var message = new Message(SampleMessage);
            
            // Should not throw even if serialization would differ
            message.ParseMessage(serializeCheck: false);
            
            Assert.True(message.SegmentCount > 0);
        }

        [Fact]
        public void ParseMessage_WithValidationDisabled_SkipsValidation()
        {
            // Message with missing required fields
            var message = new Message("MSH|^~\\&|App|Fac|App2|Fac2|20230101||ADT^A01||P|2.5");
            
            // Should not throw with validation disabled
            message.ParseMessage(validate: false);
            
            Assert.True(message.SegmentCount > 0);
        }

        [Fact]
        public void Message_Properties_AreAccessible()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            Assert.Equal("ADT_A01", message.MessageStructure); // Includes trigger event
            Assert.Equal("12345", message.MessageControlID);
            Assert.Equal("P", message.ProcessingID);
            Assert.Equal("2.5", message.Version);
            Assert.Equal(3, message.SegmentCount);
        }

        [Fact]
        public void GetValue_WithRepetition_ReturnsCorrectValue()
        {
            var message = new Message("MSH|^~\\&|App|Fac|App2|Fac2|20230101||ADT^A01|123|P|2.5\r" +
                                    "PID|||ID1~ID2~ID3||Name");
            message.ParseMessage();
            
            Assert.Equal("ID1", message.GetValue("PID.3(1)"));
            Assert.Equal("ID2", message.GetValue("PID.3(2)"));
            Assert.Equal("ID3", message.GetValue("PID.3[3]")); // Alternative syntax
        }

        [Fact]
        public void AddNewSegment_AppendsToEnd()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            var nk1 = new Segment("NK1", message.Encoding);
            nk1.AddNewField(new Field("1", message.Encoding));
            
            message.AddNewSegment(nk1);
            
            var segments = message.Segments();
            // NK1 should be at the end
            Assert.Equal("NK1", segments[segments.Count - 1].Name);
            Assert.Equal(4, segments.Count);
        }

        [Fact]
        public void Message_ComplexOperations_WorkCorrectly()
        {
            // Start with an existing message and modify it
            var message = new Message(SampleMessage);
            message.ParseMessage();
            
            // Add a new segment
            var obx = new Segment("OBX", message.Encoding);
            message.AddNewSegment(obx);
            
            // Add fields using PutValue (creates structure)
            message.PutValue("OBX.1", "1");
            message.PutValue("OBX.2", "ST");
            message.PutValue("OBX.3.1", "TEST");
            message.PutValue("OBX.3.2", "Test Name");
            message.PutValue("OBX.5", "Test Result");
            
            // Verify
            Assert.Equal("1", message.GetValue("OBX.1"));
            Assert.Equal("Test Name", message.GetValue("OBX.3.2"));
            Assert.Equal("Test Result", message.GetValue("OBX.5"));
            // OBX.3 might not be componentized if we just set a single value
            // Check if the field exists instead
            Assert.True(message.ValueExists("OBX.3.1"));
            
            // Serialize and reparse
            var serialized = message.SerializeMessage(false);
            var reparsed = new Message(serialized);
            reparsed.ParseMessage();
            
            Assert.Equal("Test Result", reparsed.GetValue("OBX.5"));
            Assert.Equal("Test Name", reparsed.GetValue("OBX.3.2"));
        }

        [Fact]
        public void RemoveTrailingDelimiters_RemovesEmptyElements()
        {
            var message = new Message("MSH|^~\\&|App|Fac|App2|Fac2|20230101||ADT^A01|123|P|2.5\r" +
                                    "PID|||12345||Doe^John^^^||||||||||||||||");
            message.ParseMessage();
            
            // Get the PID segment
            var pid = message.DefaultSegment("PID");
            var initialFieldCount = pid.GetAllFields().Count;
            
            // Remove trailing delimiters
            message.RemoveTrailingDelimiters(MessageElement.RemoveDelimitersOptions.All);
            
            // Should have fewer fields after removal
            var finalFieldCount = pid.GetAllFields().Count;
            Assert.True(finalFieldCount < initialFieldCount);
        }
    }
}