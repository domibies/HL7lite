using System;
using HL7lite.Fluent;
using Xunit;

namespace HL7lite.Test.Fluent
{
    public class SerializeMessageTests
    {
        [Fact]
        public void SerializeMessage_WithoutValidation_ReturnsHL7String()
        {
            // Arrange
            var testMessage = @"MSH|^~\&|SENDING|FACILITY|RECEIVING|FACILITY|20200101120000||ADT^A01|12345|P|2.5||
PID|1||123456^^^MRN||Doe^John^M||19800101|M|||123 Main St^^City^ST^12345||5551234567||||||||||||||||";
            
            var message = new Message(testMessage);
            message.ParseMessage();
            var fluent = new FluentMessage(message);

            // Act
            var serialized = fluent.SerializeMessage();

            // Assert
            Assert.NotNull(serialized);
            Assert.Contains("MSH|^~\\&|SENDING|FACILITY|RECEIVING|FACILITY", serialized);
            Assert.Contains("PID|1||123456^^^MRN||Doe^John^M", serialized);
        }

        [Fact]
        public void SerializeMessage_WithValidation_ReturnsHL7String()
        {
            // Arrange
            var testMessage = @"MSH|^~\&|SENDING|FACILITY|RECEIVING|FACILITY|20200101120000||ADT^A01|12345|P|2.5||
PID|1||123456^^^MRN||Doe^John^M||19800101|M|||123 Main St^^City^ST^12345||5551234567||||||||||||||||";
            
            var message = new Message(testMessage);
            message.ParseMessage();
            var fluent = new FluentMessage(message);

            // Act
            var serialized = fluent.SerializeMessage(validate: true);

            // Assert
            Assert.NotNull(serialized);
            Assert.Contains("MSH|^~\\&|SENDING|FACILITY|RECEIVING|FACILITY", serialized);
            Assert.Contains("PID|1||123456^^^MRN||Doe^John^M", serialized);
        }

        [Fact]
        public void SerializeMessage_AfterModifications_IncludesChanges()
        {
            // Arrange
            var testMessage = @"MSH|^~\&|SENDING|FACILITY|RECEIVING|FACILITY|20200101120000||ADT^A01|12345|P|2.5||
PID|1||123456^^^MRN||Doe^John^M||19800101|M|||123 Main St^^City^ST^12345||5551234567||||||||||||||||";
            
            var message = new Message(testMessage);
            message.ParseMessage();
            var fluent = new FluentMessage(message);

            // Act - Modify message
            fluent.PID[5].Set().Components("Smith", "Jane", "Marie");
            fluent.PID[7].Set().Value("19901231");
            
            var serialized = fluent.SerializeMessage();

            // Assert
            Assert.Contains("Smith^Jane^Marie", serialized);
            Assert.Contains("19901231", serialized);
            Assert.DoesNotContain("Doe^John^M", serialized);
            Assert.DoesNotContain("19800101", serialized);
        }

        [Fact]
        public void SerializeMessage_RoundTrip_PreservesData()
        {
            // Arrange
            var original = @"MSH|^~\&|SENDING|FACILITY|RECEIVING|FACILITY|20200101120000||ADT^A01|12345|P|2.5||
PID|1||123456^^^MRN||Doe^John^M||19800101|M|||123 Main St^^City^ST^12345||5551234567||||||||||||||||";
            
            var message = new Message(original);
            message.ParseMessage();
            var fluent = new FluentMessage(message);

            // Act
            var serialized = fluent.SerializeMessage();
            var reparsed = new Message(serialized);
            reparsed.ParseMessage();
            var refluentMessage = new FluentMessage(reparsed);

            // Assert
            Assert.Equal(fluent.PID[5][1].Value, refluentMessage.PID[5][1].Value);
            Assert.Equal(fluent.PID[5][2].Value, refluentMessage.PID[5][2].Value);
            Assert.Equal(fluent.PID[7].Value, refluentMessage.PID[7].Value);
            Assert.Equal(fluent.MSH[3].Value, refluentMessage.MSH[3].Value);
        }
    }
}