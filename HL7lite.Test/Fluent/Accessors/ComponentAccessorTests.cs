using System;
using Xunit;
using HL7lite;
using HL7lite.Fluent;
using HL7lite.Fluent.Accessors;

namespace HL7lite.Test.Fluent.Accessors
{
    public class ComponentAccessorTests
    {
        private readonly string PatientNameField = "Smith^John^M^Jr^Dr^MD^L";
        private readonly string SimpleComponentField = "Value1^Value2^Value3";
        private readonly string ComplexComponentWithSubcomponents = "Part1&Sub1&Sub2^Part2&Sub3&Sub4";
        
        private Message CreateTestMessage()
        {
            var builder = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment($"PID|||123456||{PatientNameField}||||||||{SimpleComponentField}||{ComplexComponentWithSubcomponents}")
                .Build();
            return builder;
        }

        [Fact]
        public void Constructor_WithValidSegmentAndField_ShouldInitialize()
        {
            // Arrange
            var message = CreateTestMessage();
            var fluent = new FluentMessage(message);
            var segment = fluent.PID;
            var field = segment[5];
            
            // Act
            var component = new ComponentAccessor(message, "PID", 5, 1);
            
            // Assert
            Assert.NotNull(component);
        }

        [Fact]
        public void Value_WhenComponentExists_ShouldReturnComponentValue()
        {
            // Arrange
            var message = CreateTestMessage();
            var component = new ComponentAccessor(message, "PID", 5, 1);
            
            // Act
            var value = component.Value;
            
            // Assert
            Assert.Equal("Smith", value);
        }

        [Fact]
        public void Value_WhenComponentIndexIsTwo_ShouldReturnSecondComponent()
        {
            // Arrange
            var message = CreateTestMessage();
            var component = new ComponentAccessor(message, "PID", 5, 2);
            
            // Act
            var value = component.Value;
            
            // Assert
            Assert.Equal("John", value);
        }

        [Fact]
        public void Value_WhenComponentDoesNotExist_ShouldReturnEmptyString()
        {
            // Arrange
            var message = CreateTestMessage();
            var component = new ComponentAccessor(message, "PID", 5, 99);
            
            // Act
            var value = component.Value;
            
            // Assert
            Assert.Equal("", value);
        }

        [Fact]
        public void Value_WhenFieldDoesNotExist_ShouldReturnEmptyString()
        {
            // Arrange
            var message = CreateTestMessage();
            var component = new ComponentAccessor(message, "PID", 99, 1);
            
            // Act
            var value = component.Value;
            
            // Assert
            Assert.Equal("", value);
        }

        [Fact]
        public void Value_WhenSegmentDoesNotExist_ShouldReturnEmptyString()
        {
            // Arrange
            var message = CreateTestMessage();
            var component = new ComponentAccessor(message, "ZZZ", 1, 1);
            
            // Act
            var value = component.Value;
            
            // Assert
            Assert.Equal("", value);
        }


        [Fact]
        public void Exists_WhenComponentPresent_ShouldReturnTrue()
        {
            // Arrange
            var message = CreateTestMessage();
            var component = new ComponentAccessor(message, "PID", 5, 1);
            
            // Act
            var exists = component.Exists;
            
            // Assert
            Assert.True(exists);
        }

        [Fact]
        public void Exists_WhenComponentNotPresent_ShouldReturnFalse()
        {
            // Arrange
            var message = CreateTestMessage();
            var component = new ComponentAccessor(message, "PID", 5, 99);
            
            // Act
            var exists = component.Exists;
            
            // Assert
            Assert.False(exists);
        }

        [Fact]
        public void IsNull_WhenComponentIsHL7Null_ShouldReturnTrue()
        {
            // Arrange
            var builder = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||123456||\"\"^John")
                .Build();
            var component = new ComponentAccessor(builder, "PID", 5, 1);
            
            // Act
            var isNull = component.IsNull;
            
            // Assert
            Assert.True(isNull);
        }

        [Fact]
        public void IsNull_WhenComponentHasValue_ShouldReturnFalse()
        {
            // Arrange
            var message = CreateTestMessage();
            var component = new ComponentAccessor(message, "PID", 5, 1);
            
            // Act
            var isNull = component.IsNull;
            
            // Assert
            Assert.False(isNull);
        }

        [Fact]
        public void IsEmpty_WhenComponentIsEmpty_ShouldReturnTrue()
        {
            // Arrange
            var builder = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||123456||^John")
                .Build();
            var component = new ComponentAccessor(builder, "PID", 5, 1);
            
            // Act
            var isEmpty = component.IsEmpty;
            
            // Assert
            Assert.True(isEmpty);
        }

        [Fact]
        public void IsEmpty_WhenComponentHasValue_ShouldReturnFalse()
        {
            // Arrange
            var message = CreateTestMessage();
            var component = new ComponentAccessor(message, "PID", 5, 1);
            
            // Act
            var isEmpty = component.IsEmpty;
            
            // Assert
            Assert.False(isEmpty);
        }

        [Fact]
        public void HasValue_WhenComponentHasActualValue_ShouldReturnTrue()
        {
            // Arrange
            var message = CreateTestMessage();
            var component = new ComponentAccessor(message, "PID", 5, 1);
            
            // Act
            var hasValue = component.HasValue;
            
            // Assert
            Assert.True(hasValue);
        }

        [Fact]
        public void HasValue_WhenComponentIsEmpty_ShouldReturnFalse()
        {
            // Arrange
            var builder = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||123456||^John")
                .Build();
            var component = new ComponentAccessor(builder, "PID", 5, 1);
            
            // Act
            var hasValue = component.HasValue;
            
            // Assert
            Assert.False(hasValue);
        }

        [Fact]
        public void Indexer_WhenSubComponentExists_ShouldReturnSubComponentAccessor()
        {
            // Arrange
            var message = CreateTestMessage();
            var component = new ComponentAccessor(message, "PID", 15, 1);  // Part1&Sub1&Sub2
            
            // Act
            var subComponent = component[1];
            
            // Assert
            Assert.NotNull(subComponent);
            Assert.IsType<SubComponentAccessor>(subComponent);
        }

        [Fact]
        public void SubComponent_WhenSubComponentExists_ShouldReturnSubComponentAccessor()
        {
            // Arrange
            var message = CreateTestMessage();
            var component = new ComponentAccessor(message, "PID", 15, 1);
            
            // Act
            var subComponent = component.SubComponent(2);
            
            // Assert
            Assert.NotNull(subComponent);
            Assert.IsType<SubComponentAccessor>(subComponent);
        }

        [Fact]
        public void SubComponent_AccessingValue_ShouldReturnCorrectValue()
        {
            // Arrange
            var message = CreateTestMessage();
            var component = new ComponentAccessor(message, "PID", 15, 1);  // Part1&Sub1&Sub2
            
            // Act
            var value1 = component[1].Value;
            var value2 = component[2].Value;
            var value3 = component[3].Value;
            
            // Assert
            Assert.Equal("Part1", value1);
            Assert.Equal("Sub1", value2);
            Assert.Equal("Sub2", value3);
        }

        [Fact]
        public void ComponentAccessor_ShouldCacheInstances()
        {
            // Arrange
            var message = CreateTestMessage();
            var component = new ComponentAccessor(message, "PID", 5, 1);
            
            // Act
            var sub1 = component[1];
            var sub2 = component[1];
            
            // Assert
            Assert.Same(sub1, sub2);
        }

        [Fact]
        public void Value_WhenComponentHasSubcomponentsWithoutEncoding_ShouldReturnFullValue()
        {
            // Arrange
            var builder = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||123456||Smith&Jones")
                .Build();
            var component = new ComponentAccessor(builder, "PID", 5, 1);
            
            // Act
            var value = component.Value;
            
            // Assert
            Assert.Equal("Smith&Jones", value);  // Should return full value when not properly encoded
        }

        [Theory]
        [InlineData(0, "")]  // Invalid index
        [InlineData(-1, "")]  // Negative index
        [InlineData(100, "")]  // Way out of bounds
        public void Value_WithInvalidIndices_ShouldReturnEmptyString(int componentIndex, string expected)
        {
            // Arrange
            var message = CreateTestMessage();
            var component = new ComponentAccessor(message, "PID", 5, componentIndex);
            
            // Act
            var value = component.Value;
            
            // Assert
            Assert.Equal(expected, value);
        }

        [Fact]
        public void AllComponents_ShouldAccessMultipleComponentsCorrectly()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Act & Assert - Access all name components
            Assert.Equal("Smith", new ComponentAccessor(message, "PID", 5, 1).Value);
            Assert.Equal("John", new ComponentAccessor(message, "PID", 5, 2).Value);
            Assert.Equal("M", new ComponentAccessor(message, "PID", 5, 3).Value);
            Assert.Equal("Jr", new ComponentAccessor(message, "PID", 5, 4).Value);
            Assert.Equal("Dr", new ComponentAccessor(message, "PID", 5, 5).Value);
            Assert.Equal("MD", new ComponentAccessor(message, "PID", 5, 6).Value);
            Assert.Equal("L", new ComponentAccessor(message, "PID", 5, 7).Value);
        }

        [Fact]
        public void Set_ShouldReturnComponentMutator()
        {
            // Arrange
            var message = CreateTestMessage();
            var accessor = new ComponentAccessor(message, "PID", 5, 2);
            
            // Act
            var mutator = accessor.Set();
            
            // Assert
            Assert.NotNull(mutator);
            Assert.IsType<HL7lite.Fluent.Mutators.ComponentMutator>(mutator);
        }

        [Fact]
        public void Set_ShouldAllowMutationThroughMutator()
        {
            // Arrange
            var message = CreateTestMessage();
            var accessor = new ComponentAccessor(message, "PID", 5, 2);
            
            // Act
            accessor.Set("Jane");
            
            // Assert
            Assert.Equal("Jane", accessor.Value);
        }
    }
}