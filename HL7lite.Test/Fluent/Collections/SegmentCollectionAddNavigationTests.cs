using System;
using HL7lite.Fluent;
using Xunit;

namespace HL7lite.Test.Fluent.Collections
{
    /// <summary>
    /// Tests for the bug where Segments().Add().Field().Component() and similar patterns
    /// target the wrong segment instance instead of the newly added segment.
    /// </summary>
    public class SegmentCollectionAddNavigationTests
    {
        [Fact]
        public void SegmentsAdd_WithFieldNavigation_ShouldTargetNewlyAddedSegment()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);
            fluent.CreateMSH.Sender("App", "Fac").Receiver("App2", "Fac2").MessageType("ADT^A01").AutoControlId().Build();

            // Act - These should each target the newly added segment
            fluent["ZZ1"][2][3].Set("CustomValue");  // Creates entire structure
            fluent.Segments("ZZ1").Add().Field(4).Set("CustomValue1"); // This sets field 4 in the newly added segment
            fluent.Segments("ZZ1").Add().Field(4).Component(3).Set("CustomValue2"); // BUG: This sets the component in the first existing ZZ1 segment
            fluent.Segments("ZZ1").Add().Field(4).Component(3).SubComponent(2).Set("CustomValue3"); // BUG: This sets the subcomponent in the first existing ZZ1 segment

            // Assert
            var segments = fluent.Segments("ZZ1");
            Assert.Equal(4, segments.Count); // Should have 4 ZZ1 segments

            // First segment (created by fluent["ZZ1"][2][3].Set("CustomValue"))
            Assert.Equal("CustomValue", segments[0][2][3].Value);

            // Second segment (created by .Add().Field(4).Set("CustomValue1"))
            Assert.Equal("CustomValue1", segments[1][4].Value);

            // Third segment (created by .Add().Field(4).Component(3).Set("CustomValue2"))
            // BUG: This should be in segment 2 (index 2), not segment 0
            Assert.Equal("CustomValue2", segments[2][4][3].Value);
            // The bug would put it in segments[0][4][3] instead

            // Fourth segment (created by .Add().Field(4).Component(3).SubComponent(2).Set("CustomValue3"))
            // BUG: This should be in segment 3 (index 3), not segment 0
            Assert.Equal("CustomValue3", segments[3][4][3][2].Value);
            // The bug would put it in segments[0][4][3][2] instead
        }

        [Fact]
        public void SegmentsAdd_BugDemonstration_ShowsWrongTargeting()
        {
            // Arrange - Create a test that demonstrates the bug clearly
            var message = new Message();
            var fluent = new FluentMessage(message);
            fluent.CreateMSH.Sender("App", "Fac").Receiver("App2", "Fac2").MessageType("ADT^A01").AutoControlId().Build();

            // Create first segment with a marker value
            fluent.Segments("ZZ1").Add().Field(4).Component(3).Set("FIRST_SEGMENT");

            // Act - Add a second segment and try to set a different value
            fluent.Segments("ZZ1").Add().Field(4).Component(3).Set("SECOND_SEGMENT");

            // Assert - Check where the values actually ended up
            var segments = fluent.Segments("ZZ1");
            Assert.Equal(2, segments.Count);

            // If the bug exists, both values will be in the first segment
            var firstSegmentValue = segments[0][4][3].Value;
            var secondSegmentValue = segments[1][4][3].Value;

            // Expected behavior: each segment should have its own value
            // Buggy behavior: second value overwrites first segment
            Assert.Equal("FIRST_SEGMENT", firstSegmentValue);
            Assert.Equal("SECOND_SEGMENT", secondSegmentValue);

            // This test will fail if the bug exists, showing both values in first segment
        }

        [Fact]
        public void SegmentsAdd_WithSubComponentNavigation_ShouldTargetCorrectSegment()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);
            fluent.CreateMSH.Sender("App", "Fac").Receiver("App2", "Fac2").MessageType("ADT^A01").AutoControlId().Build();

            // Act - Test subcomponent navigation specifically
            fluent.Segments("DG1").Add().Field(3).Component(1).SubComponent(1).Set("Diagnosis1_Sub1");
            fluent.Segments("DG1").Add().Field(3).Component(1).SubComponent(1).Set("Diagnosis2_Sub1");
            fluent.Segments("DG1").Add().Field(3).Component(1).SubComponent(2).Set("Diagnosis3_Sub2");

            // Assert
            var segments = fluent.Segments("DG1");
            Assert.Equal(3, segments.Count);

            // Each segment should have its own value
            Assert.Equal("Diagnosis1_Sub1", segments[0][3][1][1].Value);
            Assert.Equal("Diagnosis2_Sub1", segments[1][3][1][1].Value);
            Assert.Equal("Diagnosis3_Sub2", segments[2][3][1][2].Value);
        }

        [Fact]
        public void SegmentsAdd_CompareWithDirectAccess_ShouldMatchBehavior()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);
            fluent.CreateMSH.Sender("App", "Fac").Receiver("App2", "Fac2").MessageType("ADT^A01").AutoControlId().Build();

            // Act - Compare direct access vs Add() chaining
            
            // Method 1: Direct access to specific segment (should work correctly)
            fluent.Segments("OBX").Add(); // Add first segment
            fluent.Segments("OBX").Add(); // Add second segment
            fluent.Segments("OBX")[1][5][1].Set("DirectMethod"); // Set in second segment directly

            // Method 2: Using Add() with chaining (potentially buggy)
            fluent.Segments("OBX").Add().Field(5).Component(1).Set("ChainedMethod");

            // Assert
            var segments = fluent.Segments("OBX");
            Assert.Equal(3, segments.Count);

            // Check that direct method worked
            Assert.Equal("DirectMethod", segments[1][5][1].Value);

            // Check that chained method targeted the correct (third) segment
            Assert.Equal("ChainedMethod", segments[2][5][1].Value);
            
            // If bug exists, ChainedMethod might be in segments[0] or segments[1] instead
        }

        [Fact]
        public void SegmentsAdd_WithComplexChaining_ShouldTargetNewSegment()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);
            fluent.CreateMSH.Sender("App", "Fac").Receiver("App2", "Fac2").MessageType("ADT^A01").AutoControlId().Build();

            // Act - Test complex field chaining after Add()
            var result = fluent.Segments("OBX").Add()
                .Field(1).Set("1")
                .Field(2).Set("ST")
                .Field(3).SetComponents("GLUCOSE", "Glucose Level")
                .Field(5).Set("95")
                .Field(6).Set("mg/dL");

            // Assert - Verify all values are in the newly added segment
            var segments = fluent.Segments("OBX");
            Assert.Equal(1, segments.Count);

            var newSegment = segments[0];
            Assert.Equal("1", newSegment[1].Value);
            Assert.Equal("ST", newSegment[2].Value);
            Assert.Equal("GLUCOSE", newSegment[3][1].Value);
            Assert.Equal("Glucose Level", newSegment[3][2].Value);
            Assert.Equal("95", newSegment[5].Value);
            Assert.Equal("mg/dL", newSegment[6].Value);
        }

        [Fact]
        public void SegmentsAdd_DebugStepByStep_ShouldShowCorrectTargeting()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);
            fluent.CreateMSH.Sender("App", "Fac").Receiver("App2", "Fac2").MessageType("ADT^A01").AutoControlId().Build();

            // Act - Debug step by step
            var collection = fluent.Segments("ZZ1");
            var countBefore = collection.Count;
            
            var firstSegmentAccessor = collection.Add();
            var countAfterFirst = collection.Count;
            
            // Check the instance index of the returned accessor
            var firstField = firstSegmentAccessor.Field(4);
            var firstComponent = firstField.Component(3);
            firstComponent.Set("FIRST_SEGMENT");
            
            var secondSegmentAccessor = collection.Add();
            var countAfterSecond = collection.Count;
            
            var secondField = secondSegmentAccessor.Field(4);
            var secondComponent = secondField.Component(3);
            secondComponent.Set("SECOND_SEGMENT");

            // Assert - Check step by step
            Assert.Equal(0, countBefore);
            Assert.Equal(1, countAfterFirst);
            Assert.Equal(2, countAfterSecond);
            
            var segments = fluent.Segments("ZZ1");
            var firstValue = segments[0][4][3].Value;
            var secondValue = segments[1][4][3].Value;
            
            // Debug output - this will show in test output
            var rawMessage = message.SerializeMessage(false);
            
            // This test is specifically for field-level targeting, which should work
            var testFieldFirst = segments[0][4].Value;
            var testFieldSecond = segments[1][4].Value;
            
            Assert.Equal("FIRST_SEGMENT", firstValue);
            Assert.Equal("SECOND_SEGMENT", secondValue);
        }

        [Fact]
        public void SegmentsAdd_FieldLevelOnly_ShouldTargetCorrectSegment()
        {
            // Test only field-level access to isolate the issue
            var message = new Message();
            var fluent = new FluentMessage(message);
            fluent.CreateMSH.Sender("App", "Fac").Receiver("App2", "Fac2").MessageType("ADT^A01").AutoControlId().Build();

            // Test field-level access only
            fluent.Segments("ZZ1").Add().Field(4).Set("FIRST_FIELD");
            fluent.Segments("ZZ1").Add().Field(4).Set("SECOND_FIELD");

            var segments = fluent.Segments("ZZ1");
            Assert.Equal(2, segments.Count);
            Assert.Equal("FIRST_FIELD", segments[0][4].Value);
            Assert.Equal("SECOND_FIELD", segments[1][4].Value);
        }

        [Fact]
        public void SegmentsAdd_ComponentLevel_DebugSegmentTargeting()
        {
            // Debug component targeting to see exactly what's happening
            var message = new Message();
            var fluent = new FluentMessage(message);
            fluent.CreateMSH.Sender("App", "Fac").Receiver("App2", "Fac2").MessageType("ADT^A01").AutoControlId().Build();

            // First add a segment and set a component
            var firstSegmentAccessor = fluent.Segments("ZZ1").Add();
            var firstComponentMutator = firstSegmentAccessor.Field(4).Component(3);
            firstComponentMutator.Set("FIRST_COMPONENT");
            
            // Check message state after first
            var segments1 = fluent.Segments("ZZ1");
            var count1 = segments1.Count;
            var value1 = segments1[0][4][3].Value;
            
            // Add second segment and set component
            var secondSegmentAccessor = fluent.Segments("ZZ1").Add();
            var secondComponentMutator = secondSegmentAccessor.Field(4).Component(3);
            secondComponentMutator.Set("SECOND_COMPONENT");
            
            // Check final state
            var segments2 = fluent.Segments("ZZ1");
            var count2 = segments2.Count;
            var finalValue1 = segments2[0][4][3].Value;
            var finalValue2 = segments2[1][4][3].Value;
            
            // Assert step by step
            Assert.Equal(1, count1);
            Assert.Equal("FIRST_COMPONENT", value1);
            Assert.Equal(2, count2);
            Assert.Equal("FIRST_COMPONENT", finalValue1); // This should NOT change
            Assert.Equal("SECOND_COMPONENT", finalValue2); // This should be the new value
        }
    }
}