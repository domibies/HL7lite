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
            Assert.Equal("MRN001", message.PID[3].Raw);
            Assert.False(message.PID[3].HasRepetitions); // Single value, not yet repetitions
            Assert.Equal(1, message.PID[3].RepetitionCount);
            Assert.Equal("MRN001", result.Raw);
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
            Assert.Equal("MRN001", message.PID[3].Repetition(1).Raw);
            Assert.Equal("ENC123", message.PID[3].Repetition(2).Raw);
            Assert.Equal("ENC123", result.Raw);
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
            Assert.Equal("MRN001", message.PID[3].Repetition(1).Raw);
            Assert.Equal("ENC123", message.PID[3].Repetition(2).Raw);
            Assert.Equal("SSN456", message.PID[3].Repetition(3).Raw);
            Assert.Equal("SSN456", result.Raw);
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
            Assert.Equal("", message.PID[3].Raw);
            Assert.Equal("", result.Raw);
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
            Assert.Equal("MRN001", message.PID[3].Repetition(1).Raw);
            Assert.Equal("", message.PID[3].Repetition(2).Raw);
            Assert.Equal("", result.Raw);
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
            Assert.Equal("MRN001", message.PID[3].Repetition(1).Raw);
            Assert.Equal("ENC123", message.PID[3].Repetition(2).Raw);
            Assert.Equal("SSN456", message.PID[3].Repetition(3).Raw);
            
            // Verify returned accessors
            Assert.Equal("MRN001", result1.Raw);
            Assert.Equal("ENC123", result2.Raw);
            Assert.Equal("SSN456", result3.Raw);
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
            Assert.Equal("MRN^001^HOSPITAL", emptyRepetition.Raw);
            Assert.Equal("MRN^001^HOSPITAL", message.PID[3].Raw);
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
            Assert.Equal("", message.PID[3].Raw);
            Assert.Equal("", result.Raw);
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
            Assert.Equal("MRN001", message.PID[3].Raw);
            Assert.Equal("MRN001", result.Raw);
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
            Assert.Equal("MRN^001^HOSP", message.PID[3].Repetition(1).Raw);
            Assert.Equal("ENC^123^VISIT", message.PID[3].Repetition(2).Raw);
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
            Assert.Equal("1", message.Segments("DG1")[0][1].Raw);
            Assert.Equal("MRN^001", message.PID[3].Repetitions[0].Raw);
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
            Assert.Equal("MRN^001^HOSP", rep1.Raw);
            Assert.Equal("ENC^123^VISIT", rep2.Raw);
        }

        [Fact]
        public void Add_CacheClearing_ShouldWorkCorrectly()
        {
            // Arrange
            var message = CreateTestMessage();
            message.PID[3].Set("Initial");
            
            // Access to populate cache
            var initialAccessor = message.PID[3].Repetitions[0];
            Assert.Equal("Initial", initialAccessor.Raw);
            
            // Act - Add repetition should clear cache
            var newRepetition = message.PID[3].Repetitions.Add("Second");
            
            // Assert - Cache should be cleared, new access should reflect current state
            Assert.True(message.PID[3].HasRepetitions);
            Assert.Equal(2, message.PID[3].RepetitionCount);
            Assert.Equal("Initial", message.PID[3].Repetitions[0].Raw);
            Assert.Equal("Second", message.PID[3].Repetitions[1].Raw);
        }

        #region Segment Instance Tests

        [Fact]
        public void Add_ToSecondSegmentInstance_ShouldTargetCorrectSegment()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Create multiple DG1 segments
            var dg1First = message.Segments("DG1").Add();
            dg1First[3].Set("FirstDiagnosis");
            
            var dg1Second = message.Segments("DG1").Add(); 
            dg1Second[3].Set("SecondDiagnosis");
            
            // Act - Add repetition to second DG1 segment's field
            var newRepetition = dg1Second[3].Repetitions.Add("AdditionalDiagnosis");
            
            // Assert - First segment should remain unchanged
            Assert.Equal("FirstDiagnosis", message.Segments("DG1")[0][3].Raw);
            Assert.False(message.Segments("DG1")[0][3].HasRepetitions);
            
            // Second segment should have repetitions
            Assert.True(message.Segments("DG1")[1][3].HasRepetitions);
            Assert.Equal(2, message.Segments("DG1")[1][3].RepetitionCount);
            Assert.Equal("SecondDiagnosis", message.Segments("DG1")[1][3].Repetition(1).Raw);
            Assert.Equal("AdditionalDiagnosis", message.Segments("DG1")[1][3].Repetition(2).Raw);
            Assert.Equal("AdditionalDiagnosis", newRepetition.Raw);
        }

        [Fact]
        public void Add_ToThirdSegmentInstance_ShouldTargetCorrectSegment()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Create three DG1 segments
            message.Segments("DG1").Add()[3].Set("First");
            message.Segments("DG1").Add()[3].Set("Second");
            var dg1Third = message.Segments("DG1").Add();
            dg1Third[3].Set("Third");
            
            // Act - Add repetition to third segment
            var newRepetition = dg1Third[3].Repetitions.Add("ThirdAdditional");
            
            // Assert - First two segments unchanged
            Assert.Equal("First", message.Segments("DG1")[0][3].Raw);
            Assert.False(message.Segments("DG1")[0][3].HasRepetitions);
            Assert.Equal("Second", message.Segments("DG1")[1][3].Raw);
            Assert.False(message.Segments("DG1")[1][3].HasRepetitions);
            
            // Third segment should have repetitions
            Assert.True(message.Segments("DG1")[2][3].HasRepetitions);
            Assert.Equal(2, message.Segments("DG1")[2][3].RepetitionCount);
            Assert.Equal("Third", message.Segments("DG1")[2][3].Repetition(1).Raw);
            Assert.Equal("ThirdAdditional", message.Segments("DG1")[2][3].Repetition(2).Raw);
        }

        [Fact]
        public void Add_MultipleRepetitionsToMultipleSegments_ShouldTargetCorrectly()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Create two DG1 segments
            var dg1First = message.Segments("DG1").Add();
            var dg1Second = message.Segments("DG1").Add();
            
            // Act - Add repetitions to both segments
            dg1First[3].Repetitions.Add("First-1");
            dg1First[3].Repetitions.Add("First-2");
            
            dg1Second[3].Repetitions.Add("Second-1");
            dg1Second[3].Repetitions.Add("Second-2");
            dg1Second[3].Repetitions.Add("Second-3");
            
            // Assert - First segment has 2 repetitions
            Assert.True(message.Segments("DG1")[0][3].HasRepetitions);
            Assert.Equal(2, message.Segments("DG1")[0][3].RepetitionCount);
            Assert.Equal("First-1", message.Segments("DG1")[0][3].Repetition(1).Raw);
            Assert.Equal("First-2", message.Segments("DG1")[0][3].Repetition(2).Raw);
            
            // Second segment has 3 repetitions
            Assert.True(message.Segments("DG1")[1][3].HasRepetitions);
            Assert.Equal(3, message.Segments("DG1")[1][3].RepetitionCount);
            Assert.Equal("Second-1", message.Segments("DG1")[1][3].Repetition(1).Raw);
            Assert.Equal("Second-2", message.Segments("DG1")[1][3].Repetition(2).Raw);
            Assert.Equal("Second-3", message.Segments("DG1")[1][3].Repetition(3).Raw);
        }

        [Fact]
        public void Add_NavigationFromAddedSegment_ShouldWorkCorrectly()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Act - Test the navigation pattern that was failing before the fix
            var newDG1 = message.Segments("DG1").Add();
            var addedRepetition = newDG1[3].Repetitions.Add("DiagnosisCode");
            
            // Verify the navigation worked correctly by checking the value through different access paths
            Assert.Equal("DiagnosisCode", newDG1[3].Raw);
            Assert.Equal("DiagnosisCode", message.Segments("DG1")[0][3].Raw);
            Assert.Equal("DiagnosisCode", addedRepetition.Raw);
        }

        [Fact]
        public void Add_ConvertSingleToRepetitionsOnSpecificSegment_ShouldWorkCorrectly()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Create two DG1 segments with single values
            message.Segments("DG1").Add()[3].Set("FirstSingle");
            var dg1Second = message.Segments("DG1").Add();
            dg1Second[3].Set("SecondSingle");
            
            // Act - Convert second segment's single field to repetitions
            var newRepetition = dg1Second[3].Repetitions.Add("SecondAdditional");
            
            // Assert - First segment remains single
            Assert.Equal("FirstSingle", message.Segments("DG1")[0][3].Raw);
            Assert.False(message.Segments("DG1")[0][3].HasRepetitions);
            Assert.Equal(1, message.Segments("DG1")[0][3].RepetitionCount);
            
            // Second segment converted to repetitions
            Assert.True(message.Segments("DG1")[1][3].HasRepetitions);
            Assert.Equal(2, message.Segments("DG1")[1][3].RepetitionCount);
            Assert.Equal("SecondSingle", message.Segments("DG1")[1][3].Repetition(1).Raw);
            Assert.Equal("SecondAdditional", message.Segments("DG1")[1][3].Repetition(2).Raw);
            Assert.Equal("SecondAdditional", newRepetition.Raw);
        }

        #endregion
    }
}