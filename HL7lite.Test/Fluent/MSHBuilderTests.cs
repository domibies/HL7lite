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
            Assert.Equal("MyApp", fluent.MSH[3].Value);
            Assert.Equal("MyFacility", fluent.MSH[4].Value);
            Assert.Equal("TheirApp", fluent.MSH[5].Value);
            Assert.Equal("TheirFacility", fluent.MSH[6].Value);
            Assert.Equal("ADT^A01", fluent.MSH[9].Value);
            Assert.Equal("12345", fluent.MSH[10].Value);
            Assert.Equal("P", fluent.MSH[11].Value); // Default processing ID
            Assert.Equal("2.5", fluent.MSH[12].Value); // Default version
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
            var controlId = fluent.MSH[10].Value;
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
            Assert.Equal("P", fluent1.MSH[11].Value);

            // Act & Assert - Test
            fluent2.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ADT^A01")
                .ControlId("123")
                .Test()
                .Build();
            Assert.Equal("T", fluent2.MSH[11].Value);

            // Act & Assert - Debug
            fluent3.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ADT^A01")
                .ControlId("123")
                .Debug()
                .Build();
            Assert.Equal("D", fluent3.MSH[11].Value);
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
            Assert.Equal("X", fluent.MSH[11].Value);
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
            Assert.Equal("2.8", fluent.MSH[12].Value);
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
            Assert.Equal("SECRET", fluent.MSH[8].Value);
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
            var timestampStr = fluent.MSH[7].Value;
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
            Assert.Equal("HIS", fluent.MSH[3].Value);
            Assert.Equal("Hospital", fluent.MSH[4].Value);
            Assert.Equal("Lab", fluent.MSH[5].Value);
            Assert.Equal("MainLab", fluent.MSH[6].Value);
            Assert.Equal("TEST", fluent.MSH[8].Value);
            Assert.Equal("ORU^R01", fluent.MSH[9].Value);
            Assert.Equal("D", fluent.MSH[11].Value);
            Assert.Equal("2.8", fluent.MSH[12].Value);
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
            Assert.Equal("MANUAL123", fluent.MSH[10].Value);
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
            var controlId = fluent.MSH[10].Value;
            Assert.NotEqual("FIRST", controlId);
            Assert.True(controlId.Length >= 14); // Should be auto-generated
        }
    }
}