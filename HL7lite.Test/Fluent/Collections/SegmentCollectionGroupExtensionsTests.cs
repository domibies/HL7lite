using HL7lite.Fluent;
using System;
using System.Linq;
using Xunit;

namespace HL7lite.Test.Fluent.Collections
{
    public class SegmentCollectionGroupExtensionsTests
    {
        private readonly FluentMessage _testMessage;
        private readonly FluentMessage _singleGroupMessage;
        private readonly FluentMessage _emptyMessage;

        public SegmentCollectionGroupExtensionsTests()
        {
            // Create test message with multiple DG1 segments in two groups
            var message = new Message(TestMessages.SimpleADT);
            message.ParseMessage(serializeCheck: false);
            
            var dg1_1 = new Segment("DG1", message.Encoding) { Value = "DG1|1|I9|^Diabetes|" };
            var dg1_2 = new Segment("DG1", message.Encoding) { Value = "DG1|2|I9|^Hypertension|" };
            var obx_1 = new Segment("OBX", message.Encoding) { Value = "OBX|1|ST|^Test|" };
            var dg1_3 = new Segment("DG1", message.Encoding) { Value = "DG1|3|I9|^Asthma|" };
            var dg1_4 = new Segment("DG1", message.Encoding) { Value = "DG1|4|I9|^COPD|" };
            
            message.AddNewSegment(dg1_1);  // First group
            message.AddNewSegment(dg1_2);  // First group continues
            message.AddNewSegment(obx_1);  // Gap - different segment type
            message.AddNewSegment(dg1_3);  // Second group starts
            message.AddNewSegment(dg1_4);  // Second group continues
            
            _testMessage = new FluentMessage(message);

            // Create message with single group
            var singleMessage = new Message(TestMessages.SimpleADT);
            singleMessage.ParseMessage(serializeCheck: false);
            var single_dg1_1 = new Segment("DG1", singleMessage.Encoding) { Value = "DG1|1|I9|^Diabetes|" };
            var single_dg1_2 = new Segment("DG1", singleMessage.Encoding) { Value = "DG1|2|I9|^Hypertension|" };
            singleMessage.AddNewSegment(single_dg1_1);
            singleMessage.AddNewSegment(single_dg1_2);
            _singleGroupMessage = new FluentMessage(singleMessage);

            // Create empty message
            var emptyMessage = new Message(TestMessages.SimpleADT);
            emptyMessage.ParseMessage(serializeCheck: false);
            _emptyMessage = new FluentMessage(emptyMessage);
        }

        [Fact]
        public void SegmentCollection_Groups_WithMultipleGroups_ReturnsCorrectCollection()
        {
            // Arrange
            var segmentCollection = _testMessage.Segments("DG1");

            // Act
            var groups = segmentCollection.Groups();

            // Assert
            Assert.NotNull(groups);
            Assert.Equal(2, groups.Count);
            Assert.True(groups.HasGroups);
            Assert.Equal(2, groups.GroupCount);
        }

        [Fact]
        public void SegmentCollection_Groups_WithSingleGroup_ReturnsOneGroup()
        {
            // Arrange
            var segmentCollection = _singleGroupMessage.Segments("DG1");

            // Act
            var groups = segmentCollection.Groups();

            // Assert
            Assert.NotNull(groups);
            Assert.Equal(1, groups.Count);
            Assert.True(groups.HasGroups);
            Assert.Equal(1, groups.GroupCount);
        }

        [Fact]
        public void SegmentCollection_Groups_WithNoSegments_ReturnsEmptyCollection()
        {
            // Arrange
            var segmentCollection = _emptyMessage.Segments("DG1");

            // Act
            var groups = segmentCollection.Groups();

            // Assert
            Assert.NotNull(groups);
            Assert.Equal(0, groups.Count);
            Assert.False(groups.HasGroups);
            Assert.Equal(0, groups.GroupCount);
        }

        [Fact]
        public void SegmentCollection_Group_ValidGroupNumber_ReturnsCorrectGroup()
        {
            // Arrange
            var segmentCollection = _testMessage.Segments("DG1");

            // Act
            var firstGroup = segmentCollection.Group(1);
            var secondGroup = segmentCollection.Group(2);

            // Assert
            Assert.NotNull(firstGroup);
            Assert.NotNull(secondGroup);
            Assert.Equal(2, firstGroup.Count);
            Assert.Equal(2, secondGroup.Count);
            Assert.Equal("1", firstGroup.First[1].Raw);
            Assert.Equal("3", secondGroup.First[1].Raw);
        }

        [Fact]
        public void SegmentCollection_Group_ZeroGroupNumber_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var segmentCollection = _testMessage.Segments("DG1");

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => segmentCollection.Group(0));
        }

        [Fact]
        public void SegmentCollection_Group_NegativeGroupNumber_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var segmentCollection = _testMessage.Segments("DG1");

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => segmentCollection.Group(-1));
        }

        [Fact]
        public void SegmentCollection_Group_GroupNumberTooHigh_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var segmentCollection = _testMessage.Segments("DG1");

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => segmentCollection.Group(3));
        }

        [Fact]
        public void SegmentCollection_HasGroups_WithMultipleGroups_ReturnsTrue()
        {
            // Arrange
            var segmentCollection = _testMessage.Segments("DG1");

            // Act & Assert
            Assert.True(segmentCollection.HasGroups);
        }

        [Fact]
        public void SegmentCollection_HasGroups_WithSingleGroup_ReturnsTrue()
        {
            // Arrange
            var segmentCollection = _singleGroupMessage.Segments("DG1");

            // Act & Assert
            Assert.True(segmentCollection.HasGroups);
        }

        [Fact]
        public void SegmentCollection_HasGroups_WithNoSegments_ReturnsFalse()
        {
            // Arrange
            var segmentCollection = _emptyMessage.Segments("DG1");

            // Act & Assert
            Assert.False(segmentCollection.HasGroups);
        }

        [Fact]
        public void SegmentCollection_GroupCount_WithMultipleGroups_ReturnsCorrectCount()
        {
            // Arrange
            var segmentCollection = _testMessage.Segments("DG1");

            // Act & Assert
            Assert.Equal(2, segmentCollection.GroupCount);
        }

        [Fact]
        public void SegmentCollection_GroupCount_WithSingleGroup_ReturnsOne()
        {
            // Arrange
            var segmentCollection = _singleGroupMessage.Segments("DG1");

            // Act & Assert
            Assert.Equal(1, segmentCollection.GroupCount);
        }

        [Fact]
        public void SegmentCollection_GroupCount_WithNoSegments_ReturnsZero()
        {
            // Arrange
            var segmentCollection = _emptyMessage.Segments("DG1");

            // Act & Assert
            Assert.Equal(0, segmentCollection.GroupCount);
        }

        [Fact]
        public void SegmentCollection_GroupMethods_Integration_WorkCorrectly()
        {
            // This test verifies that all group methods work together correctly
            
            // Arrange
            var segmentCollection = _testMessage.Segments("DG1");

            // Act
            var hasGroups = segmentCollection.HasGroups;
            var groupCount = segmentCollection.GroupCount;
            var groups = segmentCollection.Groups();
            var firstGroup = segmentCollection.Group(1);
            var secondGroup = segmentCollection.Group(2);

            // Assert
            Assert.True(hasGroups);
            Assert.Equal(2, groupCount);
            Assert.Equal(2, groups.Count);
            Assert.Equal(groupCount, groups.Count);
            
            Assert.NotNull(firstGroup);
            Assert.NotNull(secondGroup);
            Assert.Equal(firstGroup.Count, groups[0].Count);
            Assert.Equal(secondGroup.Count, groups[1].Count);
            
            // Verify group content consistency
            Assert.Equal(firstGroup.First[1].Raw, groups[0].First[1].Raw);
            Assert.Equal(secondGroup.First[1].Raw, groups[1].First[1].Raw);
        }

        [Fact]
        public void SegmentCollection_GroupMethods_WithComplexMessage_HandlesCorrectly()
        {
            // Create a more complex message with mixed segment types
            var message = new Message(TestMessages.SimpleADT);
            message.ParseMessage(serializeCheck: false);
            
            var complex_dg1_1 = new Segment("DG1", message.Encoding) { Value = "DG1|1|I9|^Diabetes|" };
            var complex_dg1_2 = new Segment("DG1", message.Encoding) { Value = "DG1|2|I9|^Hypertension|" };
            var complex_obx_1 = new Segment("OBX", message.Encoding) { Value = "OBX|1|ST|^Test1|" };
            var complex_obx_2 = new Segment("OBX", message.Encoding) { Value = "OBX|2|ST|^Test2|" };
            var complex_dg1_3 = new Segment("DG1", message.Encoding) { Value = "DG1|3|I9|^Asthma|" };
            var complex_al1_1 = new Segment("AL1", message.Encoding) { Value = "AL1|1|DA|^Allergy|" };
            var complex_dg1_4 = new Segment("DG1", message.Encoding) { Value = "DG1|4|I9|^COPD|" };
            
            message.AddNewSegment(complex_dg1_1);     // Group 1
            message.AddNewSegment(complex_dg1_2);     // Group 1
            message.AddNewSegment(complex_obx_1);     // Gap
            message.AddNewSegment(complex_obx_2);     // Gap continues
            message.AddNewSegment(complex_dg1_3);     // Group 2
            message.AddNewSegment(complex_al1_1);     // Gap
            message.AddNewSegment(complex_dg1_4);     // Group 3 (single)
            
            var testMessage = new FluentMessage(message);
            var dg1Collection = testMessage.Segments("DG1");

            // Act
            var groups = dg1Collection.Groups();

            // Assert
            Assert.Equal(3, groups.Count);  // Three separate groups
            Assert.Equal(2, groups[0].Count);  // First group has 2 segments
            Assert.Equal(1, groups[1].Count);  // Second group has 1 segment
            Assert.Equal(1, groups[2].Count);  // Third group has 1 segment
            
            Assert.Equal("1", groups[0][0][1].Raw);  // First group: DG1.1 = "1"
            Assert.Equal("2", groups[0][1][1].Raw);  // First group: DG1.1 = "2"
            Assert.Equal("3", groups[1][0][1].Raw);  // Second group: DG1.1 = "3"
            Assert.Equal("4", groups[2][0][1].Raw);  // Third group: DG1.1 = "4"
        }
    }
}