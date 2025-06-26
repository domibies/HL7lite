using System;
using HL7lite.Fluent;
using HL7lite.Fluent.Collections;
using Xunit;

namespace HL7lite.Test.Fluent.Collections
{
    public class FieldRepetitionCollectionAddTests
    {
        private FluentMessage CreateTestMessage()
        {
            var message = new FluentMessage(new Message());
            message.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ADT^A01")
                .AutoControlId()
                .Build();
            
            message.Segments("PID").Add();
            return message;
        }

        [Fact]
        public void Add_WithValue_ToEmptyField_ShouldCreateFirstRepetition()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Act
            var result = message.PID[3].Repetitions.Add("MRN001");
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("MRN001", message.PID[3].Value);
            Assert.False(message.PID[3].HasRepetitions); // Single value, not yet repetitions
            Assert.Equal(1, message.PID[3].RepetitionCount);
            Assert.Equal("MRN001", result.Value);
        }

        [Fact]
        public void Add_WithValue_ToExistingSingleField_ShouldCreateRepetitions()
        {
            // Arrange
            var message = CreateTestMessage();
            message.PID[3].Set("MRN001");
            
            // Act
            var result = message.PID[3].Repetitions.Add("ENC123");
            
            // Assert
            Assert.NotNull(result);
            Assert.True(message.PID[3].HasRepetitions);
            Assert.Equal(2, message.PID[3].RepetitionCount);
            Assert.Equal("MRN001", message.PID[3].Repetition(1).Value);
            Assert.Equal("ENC123", message.PID[3].Repetition(2).Value);
            Assert.Equal("ENC123", result.Value);
        }

        [Fact]
        public void Add_WithValue_ToExistingRepetitions_ShouldAddToEnd()
        {
            // Arrange
            var message = CreateTestMessage();
            message.PID[3].Repetitions.Add("MRN001");
            message.PID[3].Repetitions.Add("ENC123");
            
            // Act
            var result = message.PID[3].Repetitions.Add("SSN456");
            
            // Assert
            Assert.NotNull(result);
            Assert.True(message.PID[3].HasRepetitions);
            Assert.Equal(3, message.PID[3].RepetitionCount);
            Assert.Equal("MRN001", message.PID[3].Repetition(1).Value);
            Assert.Equal("ENC123", message.PID[3].Repetition(2).Value);
            Assert.Equal("SSN456", message.PID[3].Repetition(3).Value);
            Assert.Equal("SSN456", result.Value);
        }

        [Fact]
        public void Add_WithoutValue_ToEmptyField_ShouldCreateEmptyRepetition()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Act
            var result = message.PID[3].Repetitions.Add();
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("", message.PID[3].Value);
            Assert.Equal("", result.Value);
            Assert.False(message.PID[3].HasRepetitions); // Single empty value
            Assert.Equal(1, message.PID[3].RepetitionCount);
        }

        [Fact]
        public void Add_WithoutValue_ToExistingField_ShouldCreateEmptyRepetition()
        {
            // Arrange
            var message = CreateTestMessage();
            message.PID[3].Set("MRN001");
            
            // Act
            var result = message.PID[3].Repetitions.Add();
            
            // Assert
            Assert.NotNull(result);
            Assert.True(message.PID[3].HasRepetitions);
            Assert.Equal(2, message.PID[3].RepetitionCount);
            Assert.Equal("MRN001", message.PID[3].Repetition(1).Value);
            Assert.Equal("", message.PID[3].Repetition(2).Value);
            Assert.Equal("", result.Value);
        }

        [Fact]
        public void Add_MultipleValues_ShouldCreateCorrectSequence()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Act - Add multiple values using the new consistent pattern
            var result1 = message.PID[3].Repetitions.Add("MRN001");
            var result2 = message.PID[3].Repetitions.Add("ENC123");
            var result3 = message.PID[3].Repetitions.Add("SSN456");
            
            // Assert
            Assert.True(message.PID[3].HasRepetitions);
            Assert.Equal(3, message.PID[3].RepetitionCount);
            Assert.Equal("MRN001", message.PID[3].Repetition(1).Value);
            Assert.Equal("ENC123", message.PID[3].Repetition(2).Value);
            Assert.Equal("SSN456", message.PID[3].Repetition(3).Value);
            
            // Verify returned accessors
            Assert.Equal("MRN001", result1.Value);
            Assert.Equal("ENC123", result2.Value);
            Assert.Equal("SSN456", result3.Value);
        }

        [Fact]
        public void Add_ThenModifyReturnedAccessor_ShouldWork()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Act
            var emptyRepetition = message.PID[3].Repetitions.Add();
            emptyRepetition.SetComponents("MRN", "001", "HOSPITAL");
            
            // Assert
            Assert.Equal("MRN^001^HOSPITAL", emptyRepetition.Value);
            Assert.Equal("MRN^001^HOSPITAL", message.PID[3].Value);
        }

        [Fact]
        public void Add_WithNullValue_ShouldHandleGracefully()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Act
            var result = message.PID[3].Repetitions.Add(null);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("", message.PID[3].Value);
            Assert.Equal("", result.Value);
        }

        [Fact]
        public void Add_ToNonExistentSegment_ShouldCreateSegmentAndField()
        {
            // Arrange
            var message = new FluentMessage(new Message());
            message.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ADT^A01")
                .AutoControlId()
                .Build();
            
            // Verify PID doesn't exist initially
            Assert.False(message.PID.Exists);
            
            // Act - Add to PID field without creating PID segment first
            var result = message.PID[3].Repetitions.Add("MRN001");
            
            // Assert
            Assert.NotNull(result);
            // Note: The segment should be created by our Add() method
            var segmentExists = message.UnderlyingMessage.Segments("PID").Count > 0;
            Assert.True(segmentExists, "PID segment should have been created by Add() method");
            Assert.Equal("MRN001", message.PID[3].Value);
            Assert.Equal("MRN001", result.Value);
        }

        [Fact]
        public void Add_ChainedOperations_ShouldWorkCorrectly()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Act - Chain operations on the returned accessor
            message.PID[3].Repetitions.Add("MRN001").SetComponents("MRN", "001", "HOSP");
            var result2 = message.PID[3].Repetitions.Add("ENC123");
            result2.SetComponents("ENC", "123", "VISIT");
            
            // Assert
            Assert.True(message.PID[3].HasRepetitions);
            Assert.Equal(2, message.PID[3].RepetitionCount);
            Assert.Equal("MRN^001^HOSP", message.PID[3].Repetition(1).Value);
            Assert.Equal("ENC^123^VISIT", message.PID[3].Repetition(2).Value);
        }

        [Fact]
        public void Add_VerifyConsistentWithSegmentPattern()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Act - Show that both segments and repetitions follow the same pattern
            
            // Segments: Collection.Add() returns accessor
            var newDG1 = message.Segments("DG1").Add();
            newDG1[1].Set("1");
            
            // Repetitions: Collection.Add() returns accessor  
            var newRepetition = message.PID[3].Repetitions.Add("MRN001");
            newRepetition.SetComponents("MRN", "001");
            
            // Assert - Both patterns work the same way
            Assert.Equal("1", message.Segments("DG1")[0][1].Value);
            Assert.Equal("MRN^001", message.PID[3].Repetitions[0].Value);
        }

        [Fact]
        public void Add_WithComplexValues_ShouldPreserveData()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Act - Use values without repetition delimiter (~) to avoid confusion
            var rep1 = message.PID[3].Repetitions.Add("MRN^001^HOSP");
            var rep2 = message.PID[3].Repetitions.Add("ENC^123^VISIT");
            
            // Assert
            Assert.True(message.PID[3].HasRepetitions);
            Assert.Equal(2, message.PID[3].RepetitionCount);
            Assert.Equal("MRN^001^HOSP", rep1.Value);
            Assert.Equal("ENC^123^VISIT", rep2.Value);
        }

        [Fact]
        public void Add_CacheClearing_ShouldWorkCorrectly()
        {
            // Arrange
            var message = CreateTestMessage();
            message.PID[3].Set("Initial");
            
            // Access to populate cache
            var initialAccessor = message.PID[3].Repetitions[0];
            Assert.Equal("Initial", initialAccessor.Value);
            
            // Act - Add repetition should clear cache
            var newRepetition = message.PID[3].Repetitions.Add("Second");
            
            // Assert - Cache should be cleared, new access should reflect current state
            Assert.True(message.PID[3].HasRepetitions);
            Assert.Equal(2, message.PID[3].RepetitionCount);
            Assert.Equal("Initial", message.PID[3].Repetitions[0].Value);
            Assert.Equal("Second", message.PID[3].Repetitions[1].Value);
        }
    }
}