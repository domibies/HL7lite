using System;
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
    }
}