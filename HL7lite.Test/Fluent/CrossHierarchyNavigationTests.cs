using System;
using HL7lite.Fluent;
using Xunit;

namespace HL7lite.Test.Fluent
{
    /// <summary>
    /// Tests cross-hierarchy navigation in mutators to ensure segment instance indices are correctly passed
    /// across all navigation patterns (Field→Component→Field, SubComponent→Component→Field, etc.)
    /// </summary>
    public class CrossHierarchyNavigationTests
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
            
            return message;
        }

        #region Field→Component→Field Navigation

        [Fact]
        public void FieldToComponentToField_WithMultipleSegments_ShouldTargetCorrectSegment()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Create multiple DG1 segments
            message.Segments("DG1").Add()[3].Set("FirstDiagnosis");
            var dg1Second = message.Segments("DG1").Add();
            dg1Second[3].Set("SecondDiagnosis");
            
            // Act - Navigate: Field[3] → Component[1] → Field[4] on second segment
            dg1Second[3][1].Set("ModifiedComponent")
                .Field(4).Set("RelatedField");
            
            // Assert - First segment unchanged
            Assert.Equal("FirstDiagnosis", message.Segments("DG1")[0][3].Raw);
            Assert.Equal("", message.Segments("DG1")[0][4].Raw);
            
            // Second segment modified correctly
            Assert.Equal("ModifiedComponent", message.Segments("DG1")[1][3].Raw);
            Assert.Equal("RelatedField", message.Segments("DG1")[1][4].Raw);
        }

        [Fact]
        public void FieldToComponentToComponent_WithMultipleSegments_ShouldTargetCorrectSegment()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Create multiple DG1 segments
            message.Segments("DG1").Add()[5].SetComponents("First", "Component1", "Component2");
            var dg1Second = message.Segments("DG1").Add();
            dg1Second[5].SetComponents("Second", "Component1", "Component2");
            
            // Act - Navigate: Field[5] → Component[1] → Component[3] on second segment
            dg1Second[5][1].Set("ModifiedFirst")
                .Component(3).Set("ModifiedThird");
            
            // Assert - First segment unchanged
            Assert.Equal("First^Component1^Component2", message.Segments("DG1")[0][5].Raw);
            
            // Second segment modified correctly
            Assert.Equal("ModifiedFirst", message.Segments("DG1")[1][5][1].Raw);
            Assert.Equal("ModifiedThird", message.Segments("DG1")[1][5][3].Raw);
            Assert.Equal("Component1", message.Segments("DG1")[1][5][2].Raw); // Unchanged
        }

        #endregion

        #region Component→Field→Component Navigation

        [Fact]
        public void ComponentToFieldToComponent_WithMultipleSegments_ShouldTargetCorrectSegment()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Create multiple OBX segments
            message.Segments("OBX").Add()[3].SetComponents("CE", "CODE1", "TEXT1");
            var obxSecond = message.Segments("OBX").Add();
            obxSecond[3].SetComponents("CE", "CODE2", "TEXT2");
            
            // Act - Navigate: Component[2] → Field[5] → Component[1] on second segment
            obxSecond[3][2].Set("MODIFIED_CODE")
                .Field(5).Component(1).Set("VALUE1");
            
            // Assert - First segment unchanged
            Assert.Equal("CE^CODE1^TEXT1", message.Segments("OBX")[0][3].Raw);
            Assert.Equal("", message.Segments("OBX")[0][5].Raw);
            
            // Second segment modified correctly
            Assert.Equal("CE^MODIFIED_CODE^TEXT2", message.Segments("OBX")[1][3].Raw);
            Assert.Equal("VALUE1", message.Segments("OBX")[1][5][1].Raw);
        }

        #endregion

        #region SubComponent→Component→Field Navigation

        [Fact]
        public void SubComponentToComponentToField_WithMultipleSegments_ShouldTargetCorrectSegment()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Create multiple PID segments with complex name fields
            message.Segments("PID").Add()[5].SetRaw("Smith&Sr^John&Jr^M");
            var pidSecond = message.Segments("PID").Add();
            pidSecond[5].SetRaw("Jones&Sr^Jane&Jr^F");
            
            // Act - Navigate: SubComponent[1][1] → Component[2] → Field[7] on second segment
            pidSecond[5][1][1].Set("ModifiedJones")
                .Component(2).Set("ModifiedJane")
                .Field(7).Set("19850315");
            
            // Assert - First segment unchanged
            Assert.Equal("Smith&Sr^John&Jr^M", message.Segments("PID")[0][5].Raw);
            Assert.Equal("", message.Segments("PID")[0][7].Raw);
            
            // Second segment modified correctly
            Assert.Equal("ModifiedJones&Sr", message.Segments("PID")[1][5][1].Raw);
            Assert.Equal("ModifiedJane", message.Segments("PID")[1][5][2].Raw);
            Assert.Equal("19850315", message.Segments("PID")[1][7].Raw);
        }

        [Fact]
        public void SubComponentToSubComponent_WithMultipleSegments_ShouldTargetCorrectSegment()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Create multiple segments with subcomponents
            message.Segments("PID").Add()[5][1].SetSubComponents("Smith", "Sr", "III");
            var pidSecond = message.Segments("PID").Add();
            pidSecond[5][1].SetSubComponents("Jones", "Jr", "II");
            
            // Act - Navigate: SubComponent[1] → SubComponent[3] on second segment
            pidSecond[5][1][1].Set("ModifiedJones")
                .SubComponent(3).Set("IV");
            
            // Assert - First segment unchanged
            Assert.Equal("Smith&Sr&III", message.Segments("PID")[0][5][1].Raw);
            
            // Second segment modified correctly
            Assert.Equal("ModifiedJones", message.Segments("PID")[1][5][1][1].Raw);
            Assert.Equal("Jr", message.Segments("PID")[1][5][1][2].Raw); // Unchanged
            Assert.Equal("IV", message.Segments("PID")[1][5][1][3].Raw);
        }

        #endregion

        #region Complex Multi-Level Navigation

        [Fact]
        public void ComplexNavigation_SubComponentToFieldToComponentToSubComponent_ShouldWork()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Create multiple complex segments
            message.Segments("DG1").Add()[3].SetRaw("ICD10^A00.0&Primary^Description");
            var dg1Second = message.Segments("DG1").Add();
            dg1Second[3].SetRaw("ICD10^B00.0&Secondary^Other");
            
            // Act - Complex navigation: SubComponent[2][2] → Field[4] → Component[1] → SubComponent[2]
            dg1Second[3][2][2].Set("Modified")
                .Field(4).Component(1).SubComponent(2).Set("Result");
            
            // Assert - First segment unchanged
            Assert.Equal("ICD10^A00.0&Primary^Description", message.Segments("DG1")[0][3].Raw);
            Assert.Equal("", message.Segments("DG1")[0][4].Raw);
            
            // Second segment modified correctly
            Assert.Equal("ICD10^B00.0&Modified^Other", message.Segments("DG1")[1][3].Raw);
            Assert.Equal("&Result", message.Segments("DG1")[1][4].Raw); // Empty first subcomponent, set second subcomponent
        }

        [Fact]
        public void BackwardNavigation_FieldToComponentBackToField_ShouldMaintainSegmentContext()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Create three segments to test navigation doesn't "drift"
            message.Segments("OBX").Add()[3].Set("First");
            message.Segments("OBX").Add()[3].Set("Second");
            var obxThird = message.Segments("OBX").Add();
            obxThird[3].Set("Third");
            
            // Act - Navigate forward and back: Field[3] → Component[1] → Field[5] → Field[3] again
            obxThird[3][1].Set("ModifiedThird")
                .Field(5).Set("RelatedField")
                .Field(3).Set("BackToThird");
            
            // Assert - First two segments unchanged
            Assert.Equal("First", message.Segments("OBX")[0][3].Raw);
            Assert.Equal("Second", message.Segments("OBX")[1][3].Raw);
            Assert.Equal("", message.Segments("OBX")[0][5].Raw);
            Assert.Equal("", message.Segments("OBX")[1][5].Raw);
            
            // Third segment modified in all expected ways
            Assert.Equal("BackToThird", message.Segments("OBX")[2][3].Raw);
            Assert.Equal("RelatedField", message.Segments("OBX")[2][5].Raw);
        }

        #endregion

        #region Integration with AddRepetition

        [Fact]
        public void AddRepetitionNavigation_WithMultipleSegments_ShouldTargetCorrectSegment()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Create multiple segments
            message.Segments("DG1").Add()[3].Set("FirstDiagnosis");
            var dg1Second = message.Segments("DG1").Add();
            
            // Act - Add repetition and navigate: AddRepetition → Component → Field on second segment
            dg1Second[3].Set().AddRepetition("SecondDiagnosis")
                .AddRepetition()  // Add a second repetition for components
                .SetComponents("ICD10", "A00.0", "Primary")
                .Field(4).Set("Severity");
            
            // Assert - First segment unchanged
            Assert.Equal("FirstDiagnosis", message.Segments("DG1")[0][3].Raw);
            Assert.False(message.Segments("DG1")[0][3].HasRepetitions);
            Assert.Equal("", message.Segments("DG1")[0][4].Raw);
            
            // Second segment has repetition and related field set
            Assert.True(message.Segments("DG1")[1][3].HasRepetitions);
            Assert.Equal("SecondDiagnosis", message.Segments("DG1")[1][3].Repetition(1).Raw);
            Assert.Equal("ICD10^A00.0^Primary", message.Segments("DG1")[1][3].Repetition(2).Raw);
            Assert.Equal("Severity", message.Segments("DG1")[1][4].Raw);
        }

        [Fact]
        public void AddRepetitionThenCrossNavigation_ShouldMaintainContext()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Create segments
            var dg1First = message.Segments("DG1").Add();
            var dg1Second = message.Segments("DG1").Add();
            
            // Act - Add repetition on first, then navigate to field on same segment
            dg1First[3].Set().AddRepetition("FirstRep")
                .Field(4).Set("FirstRelated");
                
            // Add repetition on second, then navigate on same segment  
            dg1Second[3].Set().AddRepetition("SecondRep")
                .Field(4).Set("SecondRelated");
            
            // Assert - Each segment modified independently
            Assert.Equal("FirstRep", message.Segments("DG1")[0][3].Raw);
            Assert.Equal("FirstRelated", message.Segments("DG1")[0][4].Raw);
            
            Assert.Equal("SecondRep", message.Segments("DG1")[1][3].Raw);
            Assert.Equal("SecondRelated", message.Segments("DG1")[1][4].Raw);
        }

        #endregion

        #region Error Cases and Edge Cases

        [Fact]
        public void NavigationFromNonExistentSegment_ShouldCreateSegmentAndWork()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Verify no DG1 segments exist
            Assert.Equal(0, message.Segments("DG1").Count);
            
            // Act - Navigate on non-existent segment (should auto-create)
            var newDG1 = message.Segments("DG1").Add();
            newDG1[3][1].Set("Component1")
                .Field(4).Set("RelatedField");
            
            // Assert - Segment created and navigation worked
            Assert.Equal(1, message.Segments("DG1").Count);
            Assert.Equal("Component1", message.Segments("DG1")[0][3].Raw);
            Assert.Equal("RelatedField", message.Segments("DG1")[0][4].Raw);
        }

        [Fact]
        public void LongNavigationChain_ShouldMaintainSegmentContext()
        {
            // Arrange
            var message = CreateTestMessage();
            
            // Create multiple segments for stress testing
            message.Segments("OBX").Add()[3].Set("First");
            message.Segments("OBX").Add()[3].Set("Second");
            var obxThird = message.Segments("OBX").Add();
            
            // Act - Very long navigation chain on third segment
            obxThird[3][1][1].Set("DeepValue")
                .SubComponent(2).Set("SubValue")
                .Component(2).Set("CompValue")
                .Field(5).Component(1).SubComponent(1).Set("FinalValue");
            
            // Assert - Only third segment affected
            Assert.Equal("First", message.Segments("OBX")[0][3].Raw);
            Assert.Equal("Second", message.Segments("OBX")[1][3].Raw);
            
            // Third segment has complex modifications
            Assert.Equal("DeepValue&SubValue", message.Segments("OBX")[2][3][1].Raw);
            Assert.Equal("CompValue", message.Segments("OBX")[2][3][2].Raw);
            Assert.Equal("FinalValue", message.Segments("OBX")[2][5][1][1].Raw);
        }

        #endregion
    }
}