using System;
using Xunit;
using HL7lite.Fluent.Accessors;

namespace HL7lite.Test.Fluent.Accessors
{
    public class FieldAccessorOutOfBoundsTests
    {
        [Fact]
        public void Repetition_AccessingOutOfBoundsIndex_ShouldReturnEmptyValueNotThrow()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001~ID002") // Only 2 repetitions
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            
            // Act - Access repetition 5 when only 2 exist
            var rep5 = fluent.PID[3].Repetition(5);
            
            // Assert - Should return empty value, not throw
            Assert.Equal("", rep5.Value);
            Assert.False(rep5.Exists);
        }

        [Fact]
        public void Repetition_AccessingRepetitionOnSingleValueField_ShouldReturnEmptyForIndex2()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001||Smith^John") // Field 3 has no repetitions, field 5 has single value
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            
            // Act - Access repetition 2 on a field with only 1 value
            var rep2 = fluent.PID[5].Repetition(2);
            
            // Assert
            Assert.Equal("", rep2.Value);
            Assert.False(rep2.Exists);
        }

        [Fact]
        public void RepetitionCollection_AccessingOutOfBoundsIndex_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001~ID002") // Only 2 repetitions
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            var repetitions = fluent.PID[3].Repetitions;
            
            // Act & Assert - Collection indexer should throw
            Assert.Equal(2, repetitions.Count);
            Assert.Throws<ArgumentOutOfRangeException>(() => repetitions[5]);
        }

        [Fact]
        public void Repetition_OnNonExistentField_ShouldReturnEmptyValue()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID")
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            
            // Act - Access repetition on non-existent field
            var rep1 = fluent.PID[99].Repetition(1);
            
            // Assert
            Assert.Equal("", rep1.Value);
            Assert.False(rep1.Exists);
            Assert.Equal(0, fluent.PID[99].RepetitionCount);
        }

        [Fact]
        public void RepetitionBehaviorConsistency_ComparedToOtherAccessors()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001")
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            
            // Act - Test consistency with other accessors
            // Non-existent field
            var nonExistentField = fluent.PID[99];
            Assert.Equal("", nonExistentField.Value);
            Assert.False(nonExistentField.Exists);
            
            // Non-existent component
            var nonExistentComponent = fluent.PID[3][99];
            Assert.Equal("", nonExistentComponent.Value);
            Assert.False(nonExistentComponent.Exists);
            
            // Non-existent repetition - should behave the same way
            var nonExistentRepetition = fluent.PID[3].Repetition(99);
            Assert.Equal("", nonExistentRepetition.Value);
            Assert.False(nonExistentRepetition.Exists);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(10)]
        [InlineData(100)]
        public void Repetition_VariousOutOfBoundsIndexes_ShouldReturnEmptyValues(int repetitionIndex)
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001") // Only 1 repetition
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            
            // Act
            var rep = fluent.PID[3].Repetition(repetitionIndex);
            
            // Assert
            if (repetitionIndex == 1)
            {
                Assert.Equal("ID001", rep.Value);
                Assert.True(rep.Exists);
            }
            else
            {
                Assert.Equal("", rep.Value);
                Assert.False(rep.Exists);
            }
        }

        [Fact]
        public void Repetition_ZeroIndex_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001")
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => fluent.PID[3].Repetition(0));
        }

        [Fact]
        public void Repetition_NegativeIndex_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001")
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => fluent.PID[3].Repetition(-1));
        }
    }
}