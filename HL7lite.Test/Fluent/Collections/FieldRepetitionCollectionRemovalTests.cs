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
        public void RemoveAt_ShouldRemoveSpecificRepetition_ZeroBased()
        {
            // Arrange
            var builder = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||123456~789012~345678")
                .Build();
            var fluentMessage = new FluentMessage(builder);
            var repetitions = fluentMessage.PID[3].Repetitions;
            
            // Act
            repetitions.RemoveAt(1); // Remove "789012" (0-based index)
            
            // Assert
            Assert.Equal(2, repetitions.Count);
            Assert.Equal("123456", repetitions[0].Value);
            Assert.Equal("345678", repetitions[1].Value);
        }

        [Fact]
        public void RemoveAt_FirstItem_ShouldUpdateFirstRepetition()
        {
            // Arrange
            var builder = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||123456~789012~345678")
                .Build();
            var fluentMessage = new FluentMessage(builder);
            var repetitions = fluentMessage.PID[3].Repetitions;
            
            // Act
            repetitions.RemoveAt(0); // Remove "123456"
            
            // Assert
            Assert.Equal(2, repetitions.Count);
            Assert.Equal("789012", repetitions[0].Value);
            Assert.Equal("345678", repetitions[1].Value);
            Assert.Equal("789012", fluentMessage.PID[3].Value); // Field value should update
        }

        [Fact]
        public void RemoveAt_LastItem_ShouldRemoveLastRepetition()
        {
            // Arrange
            var builder = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||123456~789012~345678")
                .Build();
            var fluentMessage = new FluentMessage(builder);
            var repetitions = fluentMessage.PID[3].Repetitions;
            
            // Act
            repetitions.RemoveAt(2); // Remove "345678"
            
            // Assert
            Assert.Equal(2, repetitions.Count);
            Assert.Equal("123456", repetitions[0].Value);
            Assert.Equal("789012", repetitions[1].Value);
        }

        [Fact]
        public void RemoveAt_OnlyItem_ShouldClearField()
        {
            // Arrange
            var builder = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||123456")
                .Build();
            var fluentMessage = new FluentMessage(builder);
            var repetitions = fluentMessage.PID[3].Repetitions;
            
            // Act
            repetitions.RemoveAt(0);
            
            // Assert
            Assert.Equal(0, repetitions.Count);
            Assert.Equal("", fluentMessage.PID[3].Value);
        }

        [Fact]
        public void RemoveAt_InvalidIndex_ShouldThrow()
        {
            // Arrange
            var builder = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||123456~789012")
                .Build();
            var fluentMessage = new FluentMessage(builder);
            var repetitions = fluentMessage.PID[3].Repetitions;
            
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => repetitions.RemoveAt(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => repetitions.RemoveAt(2));
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
        public void RemoveAt_FromTwoRepetitions_ShouldConvertToSingleField()
        {
            // Arrange
            var builder = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||123456~789012")
                .Build();
            var fluentMessage = new FluentMessage(builder);
            var repetitions = fluentMessage.PID[3].Repetitions;
            
            // Act
            repetitions.RemoveAt(1); // Remove second repetition
            
            // Assert
            Assert.Equal(1, repetitions.Count);
            Assert.Equal("123456", repetitions[0].Value);
            Assert.Equal("123456", fluentMessage.PID[3].Value);
            Assert.False(fluentMessage.PID[3].HasRepetitions); // Should no longer have repetitions
        }

        [Fact]
        public void ConsistentWithSegmentCollection_NamingConvention()
        {
            // This test verifies our naming convention for FieldRepetitionCollection
            var builder = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||123~456~789")
                .Build();
            var fluentMessage = new FluentMessage(builder);
            
            var fieldReps = fluentMessage.PID[3].Repetitions;
            
            // Test 0-based and 1-based methods work consistently
            Assert.Equal(3, fieldReps.Count); // Initial: 123, 456, 789
            
            // 0-based method
            fieldReps.RemoveAt(0); // Remove first item (123)
            Assert.Equal(2, fieldReps.Count); // Should have: 456, 789
            
            // 1-based method  
            fieldReps.RemoveRepetition(1); // Remove first repetition (456)
            Assert.Equal(1, fieldReps.Count); // Should have: 789
            Assert.Equal("789", fieldReps[0].Value);
            
            // Verify naming convention: RemoveAt uses 0-based, RemoveRepetition uses 1-based
            Assert.False(fluentMessage.PID[3].HasRepetitions); // Should be converted to single field
        }
    }
}