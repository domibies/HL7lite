using System;
using System.Linq;
using Xunit;
using HL7lite.Fluent;

namespace HL7lite.Test.Fluent.Collections
{
    public class FieldRepetitionCollectionRemovalTests
    {

        [Fact]
        public void Clear_ShouldRemoveAllRepetitions()
        {
            // Arrange
            var builder = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||123456~789012~345678")
                .Build();
            var fluentMessage = new FluentMessage(builder);
            var repetitions = fluentMessage.PID[3].Repetitions;
            
            // Act
            repetitions.Clear();
            
            // Assert
            Assert.Equal(0, repetitions.Count);
            Assert.Equal("", fluentMessage.PID[3].Value);
        }

        [Fact]
        public void Clear_WithSingleRepetition_ShouldClearField()
        {
            // Arrange
            var builder = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||123456")
                .Build();
            var fluentMessage = new FluentMessage(builder);
            var repetitions = fluentMessage.PID[3].Repetitions;
            
            // Act
            repetitions.Clear();
            
            // Assert
            Assert.Equal(0, repetitions.Count);
            Assert.Equal("", fluentMessage.PID[3].Value);
        }

        [Fact]
        public void Clear_WithNoField_ShouldNotThrow()
        {
            // Arrange
            var builder = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID")
                .Build();
            var fluentMessage = new FluentMessage(builder);
            var repetitions = fluentMessage.PID[99].Repetitions; // Non-existent field
            
            // Act & Assert
            repetitions.Clear(); // Should not throw
            Assert.Equal(0, repetitions.Count);
        }






        [Fact]
        public void RemoveRepetition_ShouldRemoveSpecificRepetition_OneBased()
        {
            // Arrange
            var builder = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||123456~789012~345678")
                .Build();
            var fluentMessage = new FluentMessage(builder);
            var repetitions = fluentMessage.PID[3].Repetitions;
            
            // Act
            repetitions.RemoveRepetition(2); // Remove "789012" (1-based number)
            
            // Assert
            Assert.Equal(2, repetitions.Count);
            Assert.Equal("123456", repetitions[0].Value);
            Assert.Equal("345678", repetitions[1].Value);
        }

        [Fact]
        public void RemoveRepetition_FirstRepetition_ShouldUpdateFieldValue()
        {
            // Arrange
            var builder = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||123456~789012~345678")
                .Build();
            var fluentMessage = new FluentMessage(builder);
            var repetitions = fluentMessage.PID[3].Repetitions;
            
            // Act
            repetitions.RemoveRepetition(1); // Remove first repetition (1-based)
            
            // Assert
            Assert.Equal(2, repetitions.Count);
            Assert.Equal("789012", repetitions[0].Value);
            Assert.Equal("345678", repetitions[1].Value);
            Assert.Equal("789012", fluentMessage.PID[3].Value);
        }

        [Fact]
        public void RemoveRepetition_InvalidNumber_ShouldThrow()
        {
            // Arrange
            var builder = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||123456~789012")
                .Build();
            var fluentMessage = new FluentMessage(builder);
            var repetitions = fluentMessage.PID[3].Repetitions;
            
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => repetitions.RemoveRepetition(0)); // 1-based
            Assert.Throws<ArgumentOutOfRangeException>(() => repetitions.RemoveRepetition(3));
        }

        [Fact]
        public void RemoveRepetition_FromTwoRepetitions_ShouldConvertToSingleField()
        {
            // Arrange
            var builder = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||123456~789012")
                .Build();
            var fluentMessage = new FluentMessage(builder);
            var repetitions = fluentMessage.PID[3].Repetitions;
            
            // Act
            repetitions.RemoveRepetition(2); // Remove second repetition
            
            // Assert
            Assert.Equal(1, repetitions.Count);
            Assert.Equal("123456", repetitions[0].Value);
            Assert.Equal("123456", fluentMessage.PID[3].Value);
            Assert.False(fluentMessage.PID[3].HasRepetitions); // Should no longer have repetitions
        }

    }
}