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
    }
}