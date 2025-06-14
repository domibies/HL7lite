using System;
using Xunit;
using HL7lite;
using HL7lite.Fluent;
using HL7lite.Fluent.Accessors;

namespace HL7lite.Test.Fluent.Accessors
{
    public class SubComponentAccessorTests
    {
        private readonly string ComplexField = "Part1&Sub1&Sub2^Part2&Sub3&Sub4^Part3";
        
        private Message CreateTestMessage()
        {
            var builder = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment($"PID|||{ComplexField}||||||||||||Simple&Test")
                .Build();
            return builder;
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldInitialize()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Act
            var subComponent = new SubComponentAccessor(message, "PID", 3, 1, 1);
            
            // Assert
            Assert.NotNull(subComponent);
        }

        [Fact]
        public void Value_WhenSubComponentExists_ShouldReturnValue()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Debug - let's see what's actually in the message
            var pidSegment = message.DefaultSegment("PID");
            var field3 = pidSegment.Fields(3);
            var actualFieldValue = field3.Value;
            
            var subComponent = new SubComponentAccessor(message, "PID", 3, 1, 1);
            
            // Act
            var value = subComponent.Value;
            
            // Assert
            Assert.Equal("Part1", value);
        }

        [Fact]
        public void Value_WhenAccessingSecondSubComponent_ShouldReturnCorrectValue()
        {
            // Arrange
            var message = CreateTestMessage();
            var subComponent = new SubComponentAccessor(message, "PID", 3, 1, 2);
            
            // Act
            var value = subComponent.Value;
            
            // Assert
            Assert.Equal("Sub1", value);
        }

        [Fact]
        public void Value_WhenAccessingThirdSubComponent_ShouldReturnCorrectValue()
        {
            // Arrange
            var message = CreateTestMessage();
            var subComponent = new SubComponentAccessor(message, "PID", 3, 1, 3);
            
            // Act
            var value = subComponent.Value;
            
            // Assert
            Assert.Equal("Sub2", value);
        }

        [Fact]
        public void Value_WhenSubComponentDoesNotExist_ShouldReturnEmptyString()
        {
            // Arrange
            var message = CreateTestMessage();
            var subComponent = new SubComponentAccessor(message, "PID", 3, 1, 99);
            
            // Act
            var value = subComponent.Value;
            
            // Assert
            Assert.Equal("", value);
        }

        [Fact]
        public void Value_WhenComponentDoesNotExist_ShouldReturnEmptyString()
        {
            // Arrange
            var message = CreateTestMessage();
            var subComponent = new SubComponentAccessor(message, "PID", 3, 99, 1);
            
            // Act
            var value = subComponent.Value;
            
            // Assert
            Assert.Equal("", value);
        }

        [Fact]
        public void SafeValue_WhenSubComponentExists_ShouldReturnValue()
        {
            // Arrange
            var message = CreateTestMessage();
            var subComponent = new SubComponentAccessor(message, "PID", 3, 1, 1);
            
            // Act
            var value = subComponent.SafeValue;
            
            // Assert
            Assert.Equal("Part1", value);
        }

        [Fact]
        public void SafeValue_WhenSubComponentIsNull_ShouldReturnEmptyString()
        {
            // Arrange
            var builder = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||\"\"&Sub1")
                .Build();
            var subComponent = new SubComponentAccessor(builder, "PID", 13, 1, 1);
            
            // Act
            var value = subComponent.SafeValue;
            
            // Assert
            Assert.Equal("", value);
        }

        [Fact]
        public void Exists_WhenSubComponentPresent_ShouldReturnTrue()
        {
            // Arrange
            var message = CreateTestMessage();
            var subComponent = new SubComponentAccessor(message, "PID", 3, 1, 2);
            
            // Act
            var exists = subComponent.Exists;
            
            // Assert
            Assert.True(exists);
        }

        [Fact]
        public void Exists_WhenSubComponentNotPresent_ShouldReturnFalse()
        {
            // Arrange
            var message = CreateTestMessage();
            var subComponent = new SubComponentAccessor(message, "PID", 3, 1, 99);
            
            // Act
            var exists = subComponent.Exists;
            
            // Assert
            Assert.False(exists);
        }

        [Fact]
        public void IsNull_WhenSubComponentIsHL7Null_ShouldReturnTrue()
        {
            // Arrange
            var builder = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||\"\"&Sub1")
                .Build();
            var subComponent = new SubComponentAccessor(builder, "PID", 3, 1, 1);
            
            // Act
            var isNull = subComponent.IsNull;
            
            // Assert
            Assert.True(isNull);
        }

        [Fact]
        public void IsNull_WhenSubComponentHasValue_ShouldReturnFalse()
        {
            // Arrange
            var message = CreateTestMessage();
            var subComponent = new SubComponentAccessor(message, "PID", 3, 1, 1);
            
            // Act
            var isNull = subComponent.IsNull;
            
            // Assert
            Assert.False(isNull);
        }

        [Fact]
        public void IsEmpty_WhenSubComponentIsEmpty_ShouldReturnTrue()
        {
            // Arrange
            var builder = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||&Sub1")
                .Build();
            var subComponent = new SubComponentAccessor(builder, "PID", 3, 1, 1);
            
            // Act
            var isEmpty = subComponent.IsEmpty;
            
            // Assert
            Assert.True(isEmpty);
        }

        [Fact]
        public void IsEmpty_WhenSubComponentHasValue_ShouldReturnFalse()
        {
            // Arrange
            var message = CreateTestMessage();
            var subComponent = new SubComponentAccessor(message, "PID", 3, 1, 1);
            
            // Act
            var isEmpty = subComponent.IsEmpty;
            
            // Assert
            Assert.False(isEmpty);
        }

        [Fact]
        public void HasValue_WhenSubComponentHasActualValue_ShouldReturnTrue()
        {
            // Arrange
            var message = CreateTestMessage();
            var subComponent = new SubComponentAccessor(message, "PID", 3, 1, 1);
            
            // Act
            var hasValue = subComponent.HasValue;
            
            // Assert
            Assert.True(hasValue);
        }

        [Fact]
        public void HasValue_WhenSubComponentIsEmpty_ShouldReturnFalse()
        {
            // Arrange
            var builder = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||&Sub1")
                .Build();
            var subComponent = new SubComponentAccessor(builder, "PID", 13, 1, 1);
            
            // Act
            var hasValue = subComponent.HasValue;
            
            // Assert
            Assert.False(hasValue);
        }

        [Theory]
        [InlineData(0, "")]  // Invalid index
        [InlineData(-1, "")]  // Negative index
        [InlineData(100, "")]  // Way out of bounds
        public void Value_WithInvalidIndices_ShouldReturnEmptyString(int subComponentIndex, string expected)
        {
            // Arrange
            var message = CreateTestMessage();
            var subComponent = new SubComponentAccessor(message, "PID", 13, 1, subComponentIndex);
            
            // Act
            var value = subComponent.Value;
            
            // Assert
            Assert.Equal(expected, value);
        }

        [Fact]
        public void AccessingDifferentComponents_ShouldReturnCorrectSubComponents()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Act & Assert - Component 2 subcomponents
            Assert.Equal("Part2", new SubComponentAccessor(message, "PID", 3, 2, 1).Value);
            Assert.Equal("Sub3", new SubComponentAccessor(message, "PID", 3, 2, 2).Value);
            Assert.Equal("Sub4", new SubComponentAccessor(message, "PID", 3, 2, 3).Value);
        }

        [Fact]
        public void ComponentWithoutSubcomponents_ShouldReturnFullValueForFirstSubComponent()
        {
            // Arrange
            var message = CreateTestMessage();
            var subComponent = new SubComponentAccessor(message, "PID", 3, 3, 1);  // Part3 has no subcomponents
            
            // Act
            var value = subComponent.Value;
            
            // Assert
            Assert.Equal("Part3", value);
        }

        [Fact]
        public void ComponentWithoutSubcomponents_ShouldReturnEmptyForOtherSubComponents()
        {
            // Arrange
            var message = CreateTestMessage();
            var subComponent = new SubComponentAccessor(message, "PID", 3, 3, 2);  // Part3 has no subcomponents
            
            // Act
            var value = subComponent.Value;
            
            // Assert
            Assert.Equal("", value);
        }
    }
}