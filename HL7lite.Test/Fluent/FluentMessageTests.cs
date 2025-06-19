using System;
using HL7lite.Fluent;
using Xunit;

namespace HL7lite.Test.Fluent
{
    public class FluentMessageTests
    {
        // Helper methods for common assertion patterns (following existing test conventions)
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
        public void Constructor_WithValidMessage_ShouldWrapMessage()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();

            // Act
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Assert
            Assert.NotNull(fluentMessage);
            Assert.Same(message, fluentMessage.UnderlyingMessage);
        }

        [Fact]
        public void Constructor_WithNullMessage_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new HL7lite.Fluent.FluentMessage(null));
        }

        [Fact]
        public void Indexer_WithValidSegmentName_ShouldReturnSegmentAccessor()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var pidAccessor = fluentMessage["PID"];

            // Assert
            Assert.NotNull(pidAccessor);
            Assert.IsType<HL7lite.Fluent.Accessors.SegmentAccessor>(pidAccessor);
        }

        [Fact]
        public void Indexer_WithNonExistentSegment_ShouldReturnNonExistentAccessor()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var nonExistentAccessor = fluentMessage["ZZZ"];

            // Assert
            Assert.NotNull(nonExistentAccessor);
            Assert.False(nonExistentAccessor.Exists);
        }

        [Fact]
        public void Indexer_CalledMultipleTimes_ShouldReturnCachedAccessor()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var accessor1 = fluentMessage["PID"];
            var accessor2 = fluentMessage["PID"];

            // Assert
            Assert.Same(accessor1, accessor2);
        }

        [Fact]
        public void MSH_Property_ShouldReturnMSHAccessor()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var mshAccessor = fluentMessage.MSH;

            // Assert
            Assert.NotNull(mshAccessor);
            Assert.True(mshAccessor.Exists);
            Assert.Same(fluentMessage["MSH"], mshAccessor);
        }

        [Fact]
        public void PID_Property_ShouldReturnPIDAccessor()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var pidAccessor = fluentMessage.PID;

            // Assert
            Assert.NotNull(pidAccessor);
            Assert.True(pidAccessor.Exists);
            Assert.Same(fluentMessage["PID"], pidAccessor);
        }

        [Fact]
        public void PV1_Property_WhenSegmentExists_ShouldReturnAccessor()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .WithPV1()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var pv1Accessor = fluentMessage.PV1;

            // Assert
            Assert.NotNull(pv1Accessor);
            Assert.True(pv1Accessor.Exists);
        }

        [Fact]
        public void PV1_Property_WhenSegmentDoesNotExist_ShouldReturnNonExistentAccessor()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var pv1Accessor = fluentMessage.PV1;

            // Assert
            Assert.NotNull(pv1Accessor);
            Assert.False(pv1Accessor.Exists);
        }

        [Fact]
        public void CommonNamedSegments_ShouldReturnValidAccessors()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("EVN||20240101120000")
                .WithPID()
                .WithSegment("NK1|1|DOE^JANE^M|SPO|123 MAIN ST")
                .WithSegment("AL1|1|DA|PENICILLIN|SV|RASH")
                .WithSegment("ORC|NW|12345")
                .WithSegment("NTE|1|L|Patient note")
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act & Assert - Test existence checks
            Assert.True(fluentMessage.EVN.Exists);
            Assert.True(fluentMessage.NK1.Exists);
            Assert.True(fluentMessage.AL1.Exists);
            Assert.True(fluentMessage.ORC.Exists);
            Assert.True(fluentMessage.NTE.Exists);

            // Test non-existent segments
            Assert.False(fluentMessage.PV2.Exists);
            Assert.False(fluentMessage.GT1.Exists);
            Assert.False(fluentMessage.IN2.Exists);
        }

        [Fact]
        public void PharmacySegments_ShouldReturnValidAccessors()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("RXE|^Q24H&0600^^20240101^20240107|ASPIRIN 325MG TAB")
                .WithSegment("RXA|0^ASPIRIN^325MG")
                .WithSegment("RXR|PO^BY MOUTH")
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act & Assert
            Assert.True(fluentMessage.RXE.Exists);
            Assert.True(fluentMessage.RXA.Exists);
            Assert.True(fluentMessage.RXR.Exists);
            Assert.False(fluentMessage.RXD.Exists);
            Assert.False(fluentMessage.RXG.Exists);
            Assert.False(fluentMessage.RXO.Exists);
        }

        [Fact] 
        public void FinancialSegments_ShouldReturnValidAccessors()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("GT1|1|GUARANTOR^JOHN^M")
                .WithSegment("IN1|1|PLAN001^INSURANCE CO")
                .WithSegment("FT1|1|ST|20240101|20240101|CG|12345^PROCEDURE")
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act & Assert
            Assert.True(fluentMessage.GT1.Exists);
            Assert.True(fluentMessage.FT1.Exists);
            Assert.False(fluentMessage.IN2.Exists);
            Assert.False(fluentMessage.IN3.Exists);
        }

        [Fact]
        public void SchedulingSegments_ShouldReturnValidAccessors()
        {
            // Arrange  
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("SCH|12345|APPOINTMENT")
                .WithSegment("AIS|1|CONSULTATION")
                .WithSegment("AIL|1|ROOM 101")
                .WithSegment("AIP|1|DR^SMITH^JOHN")
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act & Assert
            Assert.True(fluentMessage.SCH.Exists);
            Assert.True(fluentMessage.AIS.Exists);
            Assert.True(fluentMessage.AIL.Exists);
            Assert.True(fluentMessage.AIP.Exists);
            Assert.False(fluentMessage.AIG.Exists);
        }

        [Fact]
        public void AllNamedSegments_ShouldReturnValidAccessorsWhenNotPresent()
        {
            // Arrange - Message with only MSH and PID (minimum viable message)
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act & Assert - All named segments should return non-null accessors
            // even when segments don't exist
            Assert.NotNull(fluentMessage.MSH);
            Assert.NotNull(fluentMessage.EVN);
            Assert.NotNull(fluentMessage.PID);
            Assert.NotNull(fluentMessage.PV1);
            Assert.NotNull(fluentMessage.PV2);
            Assert.NotNull(fluentMessage.OBR);
            Assert.NotNull(fluentMessage.OBX);
            Assert.NotNull(fluentMessage.DG1);
            Assert.NotNull(fluentMessage.IN1);
            Assert.NotNull(fluentMessage.NK1);
            Assert.NotNull(fluentMessage.AL1);
            Assert.NotNull(fluentMessage.PRB);
            Assert.NotNull(fluentMessage.ROL);
            Assert.NotNull(fluentMessage.GT1);
            Assert.NotNull(fluentMessage.IN2);
            Assert.NotNull(fluentMessage.IN3);
            Assert.NotNull(fluentMessage.MRG);
            Assert.NotNull(fluentMessage.IAM);
            Assert.NotNull(fluentMessage.ACC);
            Assert.NotNull(fluentMessage.UB1);
            Assert.NotNull(fluentMessage.UB2);
            Assert.NotNull(fluentMessage.PR1);
            Assert.NotNull(fluentMessage.NTE);
            Assert.NotNull(fluentMessage.ORC);
            Assert.NotNull(fluentMessage.RXE);
            Assert.NotNull(fluentMessage.RXR);
            Assert.NotNull(fluentMessage.RXA);
            Assert.NotNull(fluentMessage.RXD);
            Assert.NotNull(fluentMessage.RXG);
            Assert.NotNull(fluentMessage.RXO);
            Assert.NotNull(fluentMessage.SCH);
            Assert.NotNull(fluentMessage.TXA);
            Assert.NotNull(fluentMessage.FT1);
            Assert.NotNull(fluentMessage.AIS);
            Assert.NotNull(fluentMessage.AIG);
            Assert.NotNull(fluentMessage.AIL);
            Assert.NotNull(fluentMessage.AIP);

            // Only MSH and PID should exist
            Assert.True(fluentMessage.MSH.Exists);
            Assert.True(fluentMessage.PID.Exists);
            Assert.False(fluentMessage.EVN.Exists);
        }

        [Fact]
        public void RemoveTrailingDelimiters_WithOptions_ShouldCallUnderlyingMessage()
        {
            // Arrange
            var hl7String = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||Z99^Z01|12345|P|2.3||
PID|1||12345|||DOE^JOHN^M||||||||||||||||||||||||";
            
            var message = new Message(hl7String);
            message.ParseMessage();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);
            var originalLength = message.SerializeMessage(false).Length;
            
            // Act
            fluentMessage.RemoveTrailingDelimiters(new MessageElement.RemoveDelimitersOptions { Fields = true });
            
            // Assert - Check that trailing delimiters were removed (message should be shorter)
            var newLength = message.SerializeMessage(false).Length;
            Assert.True(newLength < originalLength, "Message should be shorter after removing trailing delimiters");
        }

        [Fact]
        public void RemoveTrailingDelimiters_WithoutOptions_ShouldCallUnderlyingMessageWithAllOptions()
        {
            // Arrange
            var hl7String = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||Z99^Z01|12345|P|2.3||
PID|1||12345|||DOE^JOHN^M||||||||||||||||||||||||";
            
            var message = new Message(hl7String);
            message.ParseMessage();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);
            var originalLength = message.SerializeMessage(false).Length;
            
            // Act
            fluentMessage.RemoveTrailingDelimiters();
            
            // Assert - Check that trailing delimiters were removed (message should be shorter)
            var newLength = message.SerializeMessage(false).Length;
            Assert.True(newLength < originalLength, "Message should be shorter after removing trailing delimiters");
            
            // Should not contain trailing pipes
            var serialized = message.SerializeMessage(false);
            Assert.DoesNotContain("||||||||||||||||||||||||", serialized);
        }

        [Fact]
        public void GetAck_WithValidMessage_ShouldReturnAckFluentMessage()
        {
            // Arrange
            const string hl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ADT^A01|12345|P|2.3||
PID|1||12345^^^MRN||DOE^JOHN^M||19800101|M|||123 MAIN ST^^CITY^ST^12345||5551234567|||||||||||||||||";
            
            var fluentMessage = hl7.ToFluentMessage();

            // Act
            var ackMessage = fluentMessage.GetAck();

            // Assert
            Assert.NotNull(ackMessage);
            Assert.True(ackMessage.MSH.Exists);
            Assert.True(ackMessage["MSA"].Exists);
            
            // Verify swapped sender/receiver
            Assert.Equal("RECEIVER", ackMessage.MSH[3].Value); // Original receiver becomes sender
            Assert.Equal("RFACILITY", ackMessage.MSH[4].Value);
            Assert.Equal("SENDER", ackMessage.MSH[5].Value); // Original sender becomes receiver
            Assert.Equal("SFACILITY", ackMessage.MSH[6].Value);
            
            // Verify MSA segment
            Assert.Equal("AA", ackMessage["MSA"][1].Value); // Application Accept
            Assert.Equal("12345", ackMessage["MSA"][2].Value); // Original message control ID
        }

        [Fact]
        public void GetNack_WithValidMessage_ShouldReturnNackFluentMessage()
        {
            // Arrange
            const string hl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ADT^A01|12345|P|2.3||
PID|1||12345^^^MRN||DOE^JOHN^M||19800101|M|||123 MAIN ST^^CITY^ST^12345||5551234567|||||||||||||||||";
            
            var fluentMessage = hl7.ToFluentMessage();

            // Act
            var nackMessage = fluentMessage.GetNack("AR", "Invalid patient ID");

            // Assert
            Assert.NotNull(nackMessage);
            Assert.True(nackMessage.MSH.Exists);
            Assert.True(nackMessage["MSA"].Exists);
            
            // Verify swapped sender/receiver
            Assert.Equal("RECEIVER", nackMessage.MSH[3].Value);
            Assert.Equal("RFACILITY", nackMessage.MSH[4].Value);
            Assert.Equal("SENDER", nackMessage.MSH[5].Value);
            Assert.Equal("SFACILITY", nackMessage.MSH[6].Value);
            
            // Verify MSA segment
            Assert.Equal("AR", nackMessage["MSA"][1].Value); // Application Reject
            Assert.Equal("12345", nackMessage["MSA"][2].Value); // Original message control ID
            Assert.Equal("Invalid patient ID", nackMessage["MSA"][3].Value); // Error message
        }

        [Fact]
        public void GetAck_WithExistingAckMessage_ShouldReturnNull()
        {
            // Arrange - Create an ACK message first
            const string originalHl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ADT^A01|12345|P|2.3||
PID|1||12345^^^MRN||DOE^JOHN^M||19800101|M|||123 MAIN ST^^CITY^ST^12345||5551234567|||||||||||||||||";
            
            var originalMessage = originalHl7.ToFluentMessage();
            var ackMessage = originalMessage.GetAck();

            // Act - Try to get ACK from an existing ACK
            var doubleAck = ackMessage.GetAck();

            // Assert
            Assert.Null(doubleAck);
        }

        [Fact]
        public void GetNack_WithExistingAckMessage_ShouldReturnNull()
        {
            // Arrange - Create an ACK message first
            const string originalHl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ADT^A01|12345|P|2.3||
PID|1||12345^^^MRN||DOE^JOHN^M||19800101|M|||123 MAIN ST^^CITY^ST^12345||5551234567|||||||||||||||||";
            
            var originalMessage = originalHl7.ToFluentMessage();
            var ackMessage = originalMessage.GetAck();

            // Act - Try to get NACK from an existing ACK
            var nackFromAck = ackMessage.GetNack("AE", "Error");

            // Assert
            Assert.Null(nackFromAck);
        }

        [Fact]
        public void GetAck_MessageType_ShouldBeACK()
        {
            // Arrange
            const string hl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ORU^R01|12345|P|2.3||
OBR|1|ORDER123|RESULT456|CBC^COMPLETE BLOOD COUNT^L|||20210330110000|||||||||||||||||||F||
OBX|1|NM|WBC^WHITE BLOOD COUNT^L||7.5|10*3/uL|4.0-11.0|N|||F||";
            
            var fluentMessage = hl7.ToFluentMessage();

            // Act
            var ackMessage = fluentMessage.GetAck();

            // Assert
            Assert.NotNull(ackMessage);
            Assert.Equal("ACK", ackMessage.MSH[9].Value);
        }

        [Fact]
        public void GetNack_WithDifferentErrorCodes_ShouldSetCorrectCode()
        {
            // Arrange
            const string hl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ADT^A01|12345|P|2.3||
PID|1||12345^^^MRN||DOE^JOHN^M||19800101|M|||123 MAIN ST^^CITY^ST^12345||5551234567|||||||||||||||||";
            
            var fluentMessage = hl7.ToFluentMessage();

            // Act
            var arNack = fluentMessage.GetNack("AR", "Application Reject");
            var aeNack = fluentMessage.GetNack("AE", "Application Error");

            // Assert
            Assert.Equal("AR", arNack["MSA"][1].Value);
            Assert.Equal("Application Reject", arNack["MSA"][3].Value);
            
            Assert.Equal("AE", aeNack["MSA"][1].Value);
            Assert.Equal("Application Error", aeNack["MSA"][3].Value);
        }
    }
}