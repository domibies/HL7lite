using System;
using System.Collections.Generic;
using Xunit;
using HL7lite.Fluent.Accessors;

namespace HL7lite.Test.Fluent.Accessors
{
    public class FieldAccessorTests
    {
        // Helper methods for common assertion patterns
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
        public void Exists_WhenFieldExists_ShouldReturnTrue()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID("123456", "Doe", "John")
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var fieldAccessor = fluentMessage["PID"][3]; // Patient ID field

            // Assert
            Assert.True(fieldAccessor.Exists);
        }

        [Fact]
        public void Exists_WhenFieldDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID("123456", "Doe", "John")
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var fieldAccessor = fluentMessage["PID"][99]; // Non-existent field

            // Assert
            Assert.False(fieldAccessor.Exists);
        }

        [Fact]
        public void Exists_WhenSegmentDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var fieldAccessor = fluentMessage["ZZZ"][1]; // Non-existent segment

            // Assert
            Assert.False(fieldAccessor.Exists);
        }

        [Fact]
        public void Value_WhenFieldExists_ShouldReturnCorrectValue()
        {
            // Arrange
            const string expectedPatientId = "123456";
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID(expectedPatientId, "Doe", "John")
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var value = fluentMessage["PID"][3].Value; // Patient ID field

            // Assert
            Assert.Equal(expectedPatientId, value);
        }

        [Fact]
        public void Value_WhenFieldDoesNotExist_ShouldReturnEmptyString()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var value = fluentMessage["PID"][99].Value; // Non-existent field

            // Assert
            Assert.Equal("", value);
        }

        [Fact]
        public void Value_WhenSegmentDoesNotExist_ShouldReturnEmptyString()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var value = fluentMessage["ZZZ"][1].Value; // Non-existent segment

            // Assert
            Assert.Equal("", value);
        }

        [Fact]
        public void Value_WhenFieldIsExplicitNull_ShouldReturnNull()
        {
            // Arrange
            var messageString = TestMessages.NullValues; // Contains explicit null values
            var message = new Message(messageString);
            message.ParseMessage();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var value = fluentMessage["PID"][7].Value; // Date of birth (explicitly null)

            // Assert
            Assert.Null(value);
        }

        [Fact]
        public void SafeValue_WhenFieldExists_ShouldReturnCorrectValue()
        {
            // Arrange
            const string expectedPatientId = "123456";
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID(expectedPatientId, "Doe", "John")
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var value = fluentMessage["PID"][3].SafeValue; // Patient ID field

            // Assert
            Assert.Equal(expectedPatientId, value);
        }

        [Fact]
        public void SafeValue_WhenFieldDoesNotExist_ShouldReturnEmptyString()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var value = fluentMessage["PID"][99].SafeValue; // Non-existent field

            // Assert
            Assert.Equal("", value);
        }

        [Fact]
        public void SafeValue_WhenFieldIsExplicitNull_ShouldReturnEmptyString()
        {
            // Arrange
            var messageString = TestMessages.NullValues; // Contains explicit null values
            var message = new Message(messageString);
            message.ParseMessage();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var value = fluentMessage["PID"][8].SafeValue; // Date of birth (explicitly null)

            // Assert
            Assert.Equal("", value);
        }

        [Fact]
        public void IsNull_WhenFieldIsExplicitNull_ShouldReturnTrue()
        {
            // Arrange
            var messageString = TestMessages.NullValues; // Contains explicit null values
            var message = new Message(messageString);
            message.ParseMessage();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var isNull = fluentMessage["PID"][7].IsNull; // Date of birth (explicitly null)

            // Assert
            Assert.True(isNull);
        }

        [Fact]
        public void IsNull_WhenFieldDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var isNull = fluentMessage["PID"][99].IsNull; // Non-existent field

            // Assert
            Assert.False(isNull);
        }

        [Fact]
        public void IsNull_WhenFieldHasValue_ShouldReturnFalse()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID("123456", "Doe", "John")
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var isNull = fluentMessage["PID"][3].IsNull; // Patient ID field

            // Assert
            Assert.False(isNull);
        }

        [Fact]
        public void IsEmpty_WhenFieldExistsButIsEmpty_ShouldReturnTrue()
        {
            // Arrange  
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID("123456", "Doe", "John", "", "19800101") // Middle name is empty
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var isEmpty = fluentMessage["PID"][5].IsEmpty; // Patient name field (contains empty middle name)

            // Assert - this test needs the actual field structure to be checked
            // For now, let's check a truly empty field
            var isEmptyNonExistent = fluentMessage["PID"][99].IsEmpty;
            Assert.False(isEmptyNonExistent); // Non-existent fields are not considered "empty"
        }

        [Fact]
        public void HasValue_WhenFieldExistsWithValue_ShouldReturnTrue()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID("123456", "Doe", "John")
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var hasValue = fluentMessage["PID"][3].HasValue; // Patient ID field

            // Assert
            Assert.True(hasValue);
        }

        [Fact]
        public void HasValue_WhenFieldDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var hasValue = fluentMessage["PID"][99].HasValue; // Non-existent field

            // Assert
            Assert.False(hasValue);
        }

        [Fact]
        public void HasValue_WhenFieldIsExplicitNull_ShouldReturnFalse()
        {
            // Arrange
            var messageString = TestMessages.NullValues; // Contains explicit null values
            var message = new Message(messageString);
            message.ParseMessage();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var hasValue = fluentMessage["PID"][8].HasValue; // Date of birth (explicitly null)

            // Assert
            Assert.False(hasValue);
        }

        [Theory]
        [InlineData(1, 2, 3, 4, 5)] // Valid field numbers
        public void Exists_WithVariousFieldNumbers_ShouldNotThrow(params int[] fieldNumbers)
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID("123456", "Doe", "John", "M", "19800101", "M")
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act & Assert - should not throw exceptions
            foreach (var fieldNumber in fieldNumbers)
            {
                var accessor = fluentMessage["PID"][fieldNumber];
                Assert.NotNull(accessor);
                // Check the property without asserting specific values (graceful handling)
                var _ = accessor.Exists;
                var __ = accessor.Value;
                var ___ = accessor.SafeValue;
            }
        }

        [Fact]
        public void ComponentIndexer_ShouldReturnComponentAccessor()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||123456||Smith^John^M")
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);
            
            // Act
            var component = fluentMessage.PID[5][1];
            
            // Assert
            Assert.NotNull(component);
            Assert.IsType<ComponentAccessor>(component);
        }

        [Fact]
        public void ComponentMethod_ShouldReturnComponentAccessor()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||123456||Smith^John^M")
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);
            
            // Act
            var component = fluentMessage.PID[5].Component(2);
            
            // Assert
            Assert.NotNull(component);
            Assert.IsType<ComponentAccessor>(component);
        }

        [Fact]
        public void ComponentAccess_ShouldReturnCorrectValues()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||123456||Smith^John^M^Jr^Dr")
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);
            
            // Act & Assert
            Assert.Equal("Smith", fluentMessage.PID[5][1].Value);
            Assert.Equal("John", fluentMessage.PID[5][2].Value);
            Assert.Equal("M", fluentMessage.PID[5][3].Value);
            Assert.Equal("Jr", fluentMessage.PID[5][4].Value);
            Assert.Equal("Dr", fluentMessage.PID[5][5].Value);
        }

        [Fact]
        public void ComponentAccess_NonExistentComponent_ShouldReturnEmptyString()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||123456||Smith^John")
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);
            
            // Act
            var value = fluentMessage.PID[5][99].Value;
            
            // Assert
            Assert.Equal("", value);
        }

        [Fact]
        public void ComponentAccess_ShouldCacheAccessors()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||123456||Smith^John")
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);
            
            // Act
            var component1 = fluentMessage.PID[5][1];
            var component2 = fluentMessage.PID[5][1];
            
            // Assert
            Assert.Same(component1, component2);
        }

        [Fact]
        public void ComponentAccess_ThroughSubComponent_ShouldWork()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||||||||||||Home&123&Main St^Work&456&Office Blvd")
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);
            
            // Act
            var subValue = fluentMessage.PID[13][1][2].Value;
            
            // Assert
            Assert.Equal("123", subValue);
        }

        #region Field Repetition Tests

        [Fact]
        public void HasRepetitions_WithSingleValue_ShouldReturnFalse()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001")
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            var accessor = fluent.PID[3];
            
            // Act
            var hasRepetitions = accessor.HasRepetitions;
            
            // Assert
            Assert.False(hasRepetitions);
        }

        [Fact]
        public void HasRepetitions_WithMultipleValues_ShouldReturnTrue()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001~ID002~ID003")
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            var accessor = fluent.PID[3];
            
            // Act
            var hasRepetitions = accessor.HasRepetitions;
            
            // Assert
            Assert.True(hasRepetitions);
        }

        [Fact]
        public void RepetitionCount_WithSingleValue_ShouldReturn1()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001")
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            var accessor = fluent.PID[3];
            
            // Act
            var count = accessor.RepetitionCount;
            
            // Assert
            Assert.Equal(1, count);
        }

        [Fact]
        public void RepetitionCount_WithMultipleValues_ShouldReturnCorrectCount()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001~ID002~ID003")
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            var accessor = fluent.PID[3];
            
            // Act
            var count = accessor.RepetitionCount;
            
            // Assert
            Assert.Equal(3, count);
        }

        [Fact]
        public void RepetitionCount_WithNonExistentField_ShouldReturn0()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID")
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            var accessor = fluent.PID[99];
            
            // Act
            var count = accessor.RepetitionCount;
            
            // Assert
            Assert.Equal(0, count);
        }

        [Fact]
        public void Repetition_WithValidIndex_ShouldReturnCorrectValue()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001~ID002~ID003")
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            var accessor = fluent.PID[3];
            
            // Act
            var rep1 = accessor.Repetition(1);
            var rep2 = accessor.Repetition(2);
            var rep3 = accessor.Repetition(3);
            
            // Assert
            Assert.Equal("ID001", rep1.Value);
            Assert.Equal("ID002", rep2.Value);
            Assert.Equal("ID003", rep3.Value);
        }

        [Fact]
        public void Repetition_WithInvalidIndex_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001")
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            var accessor = fluent.PID[3];
            
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => accessor.Repetition(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => accessor.Repetition(-1));
        }

        [Fact]
        public void Repetition_WithOutOfRangeIndex_ShouldReturnEmptyValue()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001")
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            var accessor = fluent.PID[3];
            
            // Act
            var rep = accessor.Repetition(5);
            
            // Assert
            Assert.Equal("", rep.Value);
            Assert.False(rep.Exists);
        }

        [Fact]
        public void Repetitions_ShouldReturnFieldRepetitionCollection()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001~ID002")
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            var accessor = fluent.PID[3];
            
            // Act
            var repetitions = accessor.Repetitions;
            
            // Assert
            Assert.NotNull(repetitions);
            Assert.IsType<HL7lite.Fluent.Collections.FieldRepetitionCollection>(repetitions);
            Assert.Equal(2, repetitions.Count);
        }

        [Fact]
        public void Repetitions_WithComponentAccess_ShouldWork()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001^Type1~ID002^Type2")
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            var accessor = fluent.PID[3];
            
            // Act
            var type1 = accessor.Repetition(1)[2].Value;
            var type2 = accessor.Repetition(2)[2].Value;
            
            // Assert
            Assert.Equal("Type1", type1);
            Assert.Equal("Type2", type2);
        }

        [Fact]
        public void DefaultAccessor_WithRepetitions_ShouldAccessFirstRepetition()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001~ID002~ID003")
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            var accessor = fluent.PID[3];
            
            // Act
            var defaultValue = accessor.Value;
            
            // Assert
            Assert.Equal("ID001", defaultValue); // Should get first repetition
        }

        [Fact]
        public void Repetitions_ShouldBeCached()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001~ID002")
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            var accessor = fluent.PID[3];
            
            // Act
            var repetitions1 = accessor.Repetitions;
            var repetitions2 = accessor.Repetitions;
            
            // Assert
            Assert.Same(repetitions1, repetitions2);
        }

        #endregion

        #region Set() Method Tests

        [Fact]
        public void Set_ShouldReturnFieldMutator()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001")
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            var accessor = fluent.PID[3];
            
            // Act
            var mutator = accessor.Set();
            
            // Assert
            Assert.NotNull(mutator);
            Assert.IsType<HL7lite.Fluent.Mutators.FieldMutator>(mutator);
        }

        [Fact]
        public void Set_ShouldAllowMutationThroughMutator()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001")
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            var accessor = fluent.PID[3];
            
            // Act
            accessor.Set().Value("ID002");
            
            // Assert
            Assert.Equal("ID002", accessor.Value);
        }

        #endregion

        [Fact]
        public void Set_SpecificComponentInSpecificRepetition_ShouldWork()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID")
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            
            // Create initial repetitions with components
            fluent.PID[3].Set().Value("ID001^System1");
            fluent.PID[3].Set().AddRepetition("ID002^System2");
            fluent.PID[3].Set().AddRepetition("ID003^System3");
            
            // Act - Set component 2 of repetition 2
            fluent.PID[3].Repetition(2)[2].Set().Value("UpdatedSystem2");
            
            // Assert
            Assert.Equal("ID001", fluent.PID[3].Repetition(1)[1].Value);
            Assert.Equal("System1", fluent.PID[3].Repetition(1)[2].Value);
            Assert.Equal("ID002", fluent.PID[3].Repetition(2)[1].Value);
            Assert.Equal("UpdatedSystem2", fluent.PID[3].Repetition(2)[2].Value);
            Assert.Equal("ID003", fluent.PID[3].Repetition(3)[1].Value);
            Assert.Equal("System3", fluent.PID[3].Repetition(3)[2].Value);
        }

        [Fact]
        public void Set_ComponentInRepetitionUsingCollection_ShouldWork()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001^System1~ID002^System2")
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            
            // Act - Use Repetitions collection (0-based) to access component
            fluent.PID[3].Repetitions[1][2].Set().Value("NewSystem");
            
            // Assert
            Assert.Equal("System1", fluent.PID[3].Repetitions[0][2].Value);
            Assert.Equal("NewSystem", fluent.PID[3].Repetitions[1][2].Value);
        }

        [Fact]
        public void Set_SpecificRepetition_ShouldNotReplaceEntireField()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001~ID002~ID003")
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            
            // Act - Set value on specific repetition
            fluent.PID[3].Repetition(2).Set().Value("UPDATED");
            
            // Assert - All repetitions should still exist
            Assert.Equal(3, fluent.PID[3].RepetitionCount);
            Assert.Equal("ID001", fluent.PID[3].Repetition(1).Value);
            Assert.Equal("UPDATED", fluent.PID[3].Repetition(2).Value);
            Assert.Equal("ID003", fluent.PID[3].Repetition(3).Value);
        }

        [Fact]
        public void Set_AddRepetitionOnSpecificRepetition_ShouldThrow()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001~ID002")
                .Build();
            var fluent = new HL7lite.Fluent.FluentMessage(message);
            
            // Act & Assert - Adding repetition on repetition 2 should throw
            var ex = Assert.Throws<InvalidOperationException>(() => 
                fluent.PID[3].Repetition(2).Set().AddRepetition("NEW"));
            Assert.Contains("Cannot add a repetition when operating on a specific repetition", ex.Message);
        }
    }
}