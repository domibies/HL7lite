using System;
using HL7lite.Fluent;
using HL7lite.Fluent.Builders;
using Xunit;

namespace HL7lite.Test.Fluent
{
    public class MSHBuilderTests
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
        public void CreateMSH_WithAllRequiredFields_ShouldCreateValidMSHSegment()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);

            // Act
            fluent.CreateMSH
                .Sender("MyApp", "MyFacility")
                .Receiver("TheirApp", "TheirFacility")
                .MessageType("ADT^A01")
                .ControlId("12345")
                .Build();

            // Assert
            Assert.True(fluent.MSH.Exists);
            Assert.Equal("MyApp", fluent.MSH[3].Raw);
            Assert.Equal("MyFacility", fluent.MSH[4].Raw);
            Assert.Equal("TheirApp", fluent.MSH[5].Raw);
            Assert.Equal("TheirFacility", fluent.MSH[6].Raw);
            Assert.Equal("ADT^A01", fluent.MSH[9].Raw);
            Assert.Equal("12345", fluent.MSH[10].Raw);
            Assert.Equal("P", fluent.MSH[11].Raw); // Default processing ID
            Assert.Equal("2.5", fluent.MSH[12].Raw); // Default version
        }

        [Fact]
        public void CreateMSH_WithAutoControlId_ShouldGenerateUniqueControlId()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);

            // Act
            fluent.CreateMSH
                .Sender("MyApp", "MyFacility")
                .Receiver("TheirApp", "TheirFacility")
                .MessageType("ADT^A01")
                .AutoControlId()
                .Build();

            // Assert
            Assert.True(fluent.MSH.Exists);
            var controlId = fluent.MSH[10].Raw;
            Assert.NotNull(controlId);
            Assert.NotEmpty(controlId);
            Assert.True(controlId.Length >= 14); // Should be timestamp (14) + random (4)
        }

        [Fact]
        public void CreateMSH_WithProcessingIdMethods_ShouldSetCorrectProcessingId()
        {
            // Arrange
            var message1 = new Message();
            var message2 = new Message();
            var message3 = new Message();
            var fluent1 = new FluentMessage(message1);
            var fluent2 = new FluentMessage(message2);
            var fluent3 = new FluentMessage(message3);

            // Act & Assert - Production
            fluent1.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ADT^A01")
                .ControlId("123")
                .Production()
                .Build();
            Assert.Equal("P", fluent1.MSH[11].Raw);

            // Act & Assert - Test
            fluent2.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ADT^A01")
                .ControlId("123")
                .Test()
                .Build();
            Assert.Equal("T", fluent2.MSH[11].Raw);

            // Act & Assert - Debug
            fluent3.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ADT^A01")
                .ControlId("123")
                .Debug()
                .Build();
            Assert.Equal("D", fluent3.MSH[11].Raw);
        }

        [Fact]
        public void CreateMSH_WithCustomProcessingId_ShouldSetCustomValue()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);

            // Act
            fluent.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ADT^A01")
                .ControlId("123")
                .ProcessingId("X")
                .Build();

            // Assert
            Assert.Equal("X", fluent.MSH[11].Raw);
        }

        [Fact]
        public void CreateMSH_WithCustomVersion_ShouldSetCustomVersion()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);

            // Act
            fluent.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ORU^R01")
                .ControlId("123")
                .Version("2.8")
                .Build();

            // Assert
            Assert.Equal("2.8", fluent.MSH[12].Raw);
        }

        [Fact]
        public void CreateMSH_WithSecurity_ShouldSetSecurityField()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);

            // Act
            fluent.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ADT^A01")
                .ControlId("123")
                .Security("SECRET")
                .Build();

            // Assert
            Assert.Equal("SECRET", fluent.MSH[8].Raw);
        }

        [Fact]
        public void CreateMSH_WithoutSender_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                fluent.CreateMSH
                    .Receiver("App2", "Fac2")
                    .MessageType("ADT^A01")
                    .ControlId("123")
                    .Build());
        }

        [Fact]
        public void CreateMSH_WithoutReceiver_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                fluent.CreateMSH
                    .Sender("App", "Fac")
                    .MessageType("ADT^A01")
                    .ControlId("123")
                    .Build());
        }

        [Fact]
        public void CreateMSH_WithoutMessageType_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                fluent.CreateMSH
                    .Sender("App", "Fac")
                    .Receiver("App2", "Fac2")
                    .ControlId("123")
                    .Build());
        }

        [Fact]
        public void CreateMSH_WithoutControlIdOrAutoControlId_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                fluent.CreateMSH
                    .Sender("App", "Fac")
                    .Receiver("App2", "Fac2")
                    .MessageType("ADT^A01")
                    .Build());
        }

        [Fact]
        public void CreateMSH_WithNullSenderApplication_ShouldThrowArgumentNullException()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                fluent.CreateMSH.Sender(null, "Facility"));
        }

        [Fact]
        public void CreateMSH_WithNullSenderFacility_ShouldThrowArgumentNullException()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                fluent.CreateMSH.Sender("App", null));
        }

        [Fact]
        public void CreateMSH_WithNullMessageType_ShouldThrowArgumentNullException()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                fluent.CreateMSH.MessageType(null));
        }

        [Fact]
        public void CreateMSH_GeneratedTimestamp_ShouldBeRecentTimestamp()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);
            var beforeTime = DateTime.Now.AddSeconds(-1);

            // Act
            fluent.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ADT^A01")
                .ControlId("123")
                .AutoTimestamp()
                .Build();

            var afterTime = DateTime.Now.AddSeconds(1);

            // Assert
            var timestampStr = fluent.MSH[7].Raw;
            Assert.NotNull(timestampStr);
            Assert.NotEmpty(timestampStr);

            // Parse the timestamp and verify it's recent
            var timestamp = MessageHelper.ParseDateTime(timestampStr);
            Assert.NotNull(timestamp);
            Assert.True(timestamp >= beforeTime, "Timestamp should be after the before time");
            Assert.True(timestamp <= afterTime, "Timestamp should be before the after time");
        }

        [Fact]
        public void CreateMSH_ChainedMethods_ShouldAllowAnyOrder()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);

            // Act - Test different order of method calls
            fluent.CreateMSH
                .Version("2.8")
                .MessageType("ORU^R01")
                .Debug()
                .AutoControlId()
                .Security("TEST")
                .Receiver("Lab", "MainLab")
                .Sender("HIS", "Hospital")
                .Build();

            // Assert
            Assert.True(fluent.MSH.Exists);
            Assert.Equal("HIS", fluent.MSH[3].Raw);
            Assert.Equal("Hospital", fluent.MSH[4].Raw);
            Assert.Equal("Lab", fluent.MSH[5].Raw);
            Assert.Equal("MainLab", fluent.MSH[6].Raw);
            Assert.Equal("TEST", fluent.MSH[8].Raw);
            Assert.Equal("ORU^R01", fluent.MSH[9].Raw);
            Assert.Equal("D", fluent.MSH[11].Raw);
            Assert.Equal("2.8", fluent.MSH[12].Raw);
        }

        [Fact]
        public void CreateMSH_MultipleControlIdCalls_LastCallShouldWin()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);

            // Act
            fluent.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ADT^A01")
                .AutoControlId()
                .ControlId("MANUAL123")  // This should override AutoControlId
                .Build();

            // Assert
            Assert.Equal("MANUAL123", fluent.MSH[10].Raw);
        }

        [Fact]
        public void CreateMSH_ControlIdAfterAutoControlId_ShouldUseManualId()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);

            // Act
            fluent.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ADT^A01")
                .ControlId("FIRST")
                .AutoControlId()  // This should enable auto generation
                .Build();

            // Assert
            var controlId = fluent.MSH[10].Raw;
            Assert.NotEqual("FIRST", controlId);
            Assert.True(controlId.Length >= 14); // Should be auto-generated
        }

        [Fact]
        public void AutoControlId_MultipleRapidCalls_ShouldGenerateSequentialIds()
        {
            // Arrange
            var controlIds = new string[5];

            // Act - Generate multiple messages rapidly to test sequential behavior
            for (int i = 0; i < 5; i++)
            {
                var message = new Message();
                var fluent = new FluentMessage(message);
                
                fluent.CreateMSH
                    .Sender("App", "Fac")
                    .Receiver("App2", "Fac2")
                    .MessageType("ADT^A01")
                    .AutoControlId()
                    .Build();

                controlIds[i] = fluent.MSH[10].Raw;
            }

            // Assert - Control IDs should be sequential (when generated within same second)
            for (int i = 1; i < controlIds.Length; i++)
            {
                var prev = controlIds[i - 1];
                var curr = controlIds[i];
                
                // Both should have same format (18 characters: 14 timestamp + 4 counter)
                Assert.Equal(18, prev.Length);
                Assert.Equal(18, curr.Length);
                
                // Extract timestamp and counter parts
                var prevTimestamp = prev.Substring(0, 14);
                var currTimestamp = curr.Substring(0, 14);
                var prevCounter = int.Parse(prev.Substring(14, 4));
                var currCounter = int.Parse(curr.Substring(14, 4));
                
                // If timestamps are the same (generated within same second), counters should be sequential
                if (prevTimestamp == currTimestamp)
                {
                    // Counter should increment by 1, or wrap around from 9999 to 1
                    var expectedNext = (prevCounter % 9999) + 1;
                    Assert.Equal(expectedNext, currCounter);
                }
                
                // All IDs should be unique
                Assert.NotEqual(prev, curr);
            }
        }
    }
}