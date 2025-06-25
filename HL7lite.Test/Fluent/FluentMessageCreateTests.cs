using System;
using HL7lite.Fluent;
using Xunit;

namespace HL7lite.Test.Fluent
{
    public class FluentMessageCreateTests
    {
        [Fact]
        public void Create_ShouldReturnNewEmptyFluentMessage()
        {
            // Act
            var fluent = FluentMessage.Create();

            // Assert
            Assert.NotNull(fluent);
            Assert.NotNull(fluent.UnderlyingMessage);
            Assert.Empty(fluent.UnderlyingMessage.Segments());
        }

        [Fact]
        public void Create_ShouldCreateMessageReadyForMSHBuilder()
        {
            // Act
            var fluent = FluentMessage.Create();
            fluent.CreateMSH
                .Sender("APP", "FACILITY")
                .Receiver("DEST", "FACILITY2")
                .MessageType("ADT^A01")
                .AutoControlId()
                .Build();

            // Assert
            Assert.True(fluent.MSH.Exists);
            Assert.Equal("APP", fluent.MSH[3].Value);
            Assert.Equal("FACILITY", fluent.MSH[4].Value);
            Assert.Equal("DEST", fluent.MSH[5].Value);
            Assert.Equal("FACILITY2", fluent.MSH[6].Value);
            Assert.Equal("ADT^A01", fluent.MSH[9].Value);
        }

        [Fact]
        public void Create_ShouldAllowBuildingCompleteMessage()
        {
            // Act
            var fluent = FluentMessage.Create();
            
            // Build MSH
            fluent.CreateMSH
                .Sender("SENDING_APP", "FACILITY_A")
                .Receiver("RECEIVING_APP", "FACILITY_B")
                .MessageType("ADT^A01")
                .ControlId("12345")
                .Production()
                .Build();

            // Add PID segment
            var pid = fluent.Segments("PID").Add();
            pid[1].Set("1");
            pid[3].Set("PAT001");
            pid[5].Set().SetComponents("Doe", "John", "M");
            pid[7].Set("19800101");
            pid[8].Set("M");

            // Add PV1 segment
            var pv1 = fluent.Segments("PV1").Add();
            pv1[1].Set("1");
            pv1[2].Set("I");
            pv1[3].Set().SetComponents("ICU", "001", "A");

            // Assert
            Assert.Equal(3, fluent.UnderlyingMessage.Segments().Count);
            Assert.Equal("PAT001", fluent.PID[3].Value);
            Assert.Equal("Doe^John^M", fluent.PID[5].Value);
            Assert.Equal("19800101", fluent.PID[7].Value);
            Assert.Equal("I", fluent.PV1[2].Value);
            Assert.Equal("ICU^001^A", fluent.PV1[3].Value);
        }

        [Fact]
        public void Create_ShouldBehaveSameAsNewMessageConstructor()
        {
            // Arrange & Act
            var fluent1 = FluentMessage.Create();
            var fluent2 = new FluentMessage(new Message());

            // Both should be empty
            Assert.Empty(fluent1.UnderlyingMessage.Segments());
            Assert.Empty(fluent2.UnderlyingMessage.Segments());

            // Both should allow same operations
            fluent1.PID[3].Set("ID1");
            fluent2.PID[3].Set("ID1");

            Assert.Equal(fluent1.PID[3].Value, fluent2.PID[3].Value);
        }

        [Fact]
        public void Create_MultipleCallsShouldReturnDifferentInstances()
        {
            // Act
            var fluent1 = FluentMessage.Create();
            var fluent2 = FluentMessage.Create();

            // Assert - different instances
            Assert.NotSame(fluent1, fluent2);
            Assert.NotSame(fluent1.UnderlyingMessage, fluent2.UnderlyingMessage);

            // Modifications to one should not affect the other
            fluent1.PID[3].Set("ID1");
            fluent2.PID[3].Set("ID2");

            Assert.Equal("ID1", fluent1.PID[3].Value);
            Assert.Equal("ID2", fluent2.PID[3].Value);
        }

        [Fact]
        public void Create_ShouldAllowSerializationOfBuiltMessage()
        {
            // Arrange
            var fluent = FluentMessage.Create();
            fluent.CreateMSH
                .Sender("APP", "FAC")
                .Receiver("DEST", "FAC2")
                .MessageType("ORU^R01")
                .AutoControlId()
                .Build();

            fluent.PID[3].Set("12345");
            fluent.PID[5].Set().SetComponents("Smith", "John");

            // Act
            var serialized = fluent.Serialize().ToString();

            // Assert
            Assert.NotNull(serialized);
            Assert.Contains("MSH", serialized);
            Assert.Contains("PID", serialized);
            Assert.Contains("12345", serialized);
            Assert.Contains("Smith^John", serialized);
        }

        [Fact]
        public void Create_DocumentationExample_ShouldWork()
        {
            // Example from documentation
            var fluent = FluentMessage.Create();
            fluent.CreateMSH
                .Sender("APP", "FACILITY")
                .Receiver("DEST", "FACILITY2")
                .MessageType("ADT^A01")
                .AutoControlId()
                .Build();

            // Assert
            Assert.True(fluent.MSH.Exists);
            Assert.Equal("ADT^A01", fluent.MSH[9].Value);
        }
    }
}