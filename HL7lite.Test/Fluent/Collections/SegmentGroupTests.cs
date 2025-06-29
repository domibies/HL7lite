using HL7lite.Fluent;
using HL7lite.Fluent.Collections;
using System;
using System.Linq;
using Xunit;

namespace HL7lite.Test.Fluent.Collections
{
    public class SegmentGroupTests
    {
        private readonly FluentMessage _testMessage;

        public SegmentGroupTests()
        {
            // Create test message with multiple DG1 segments in two groups
            var message = new Message(TestMessages.SimpleADT);
            message.ParseMessage(serializeCheck: false);
            
            // Create segments using the fluent API to ensure proper structure
            var fluent = new FluentMessage(message);
            
            // Add DG1 segments using fluent API
            fluent.Segments("DG1").Add()[1].Set("1");  // Set ID
            fluent.Segments("DG1")[0][2].Set("I9");    // Diagnosis coding method
            fluent.Segments("DG1")[0][3].SetComponents("", "Diabetes");  // Diagnosis code and description
            
            fluent.Segments("DG1").Add()[1].Set("2");  // Set ID
            fluent.Segments("DG1")[1][2].Set("I9");    // Diagnosis coding method
            fluent.Segments("DG1")[1][3].SetComponents("", "Hypertension");  // Diagnosis code and description
            
            // Add OBX segment to create gap
            fluent.Segments("OBX").Add()[1].Set("1");  // Set ID
            fluent.Segments("OBX")[0][2].Set("ST");    // Value type
            fluent.Segments("OBX")[0][3].SetComponents("", "Test");  // Observation identifier
            
            // Add more DG1 segments (second group)
            fluent.Segments("DG1").Add()[1].Set("3");  // Set ID
            fluent.Segments("DG1")[2][2].Set("I9");    // Diagnosis coding method
            fluent.Segments("DG1")[2][3].SetComponents("", "Asthma");  // Diagnosis code and description
            
            fluent.Segments("DG1").Add()[1].Set("4");  // Set ID
            fluent.Segments("DG1")[3][2].Set("I9");    // Diagnosis coding method
            fluent.Segments("DG1")[3][3].SetComponents("", "COPD");  // Diagnosis code and description
            
            _testMessage = new FluentMessage(message);
        }

        [Fact]
        public void SegmentGroup_Constructor_WithNullSegments_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SegmentGroup(null, "DG1"));
        }

        [Fact]
        public void SegmentGroup_Constructor_WithNullSegmentName_ThrowsArgumentNullException()
        {
            // Arrange
            var segments = _testMessage.Segments("DG1").Take(2);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SegmentGroup(segments, null));
        }

        [Fact]
        public void SegmentGroup_Count_ReturnsCorrectCount()
        {
            // Arrange
            var segments = _testMessage.Segments("DG1").Take(2);
            var group = new SegmentGroup(segments, "DG1");

            // Act & Assert
            Assert.Equal(2, group.Count);
        }

        [Fact]
        public void SegmentGroup_IsEmpty_WithNoSegments_ReturnsTrue()
        {
            // Arrange
            var emptyGroup = new SegmentGroup(Enumerable.Empty<HL7lite.Fluent.Accessors.SegmentAccessor>(), "DG1");

            // Act & Assert
            Assert.True(emptyGroup.IsEmpty);
            Assert.Equal(0, emptyGroup.Count);
        }

        [Fact]
        public void SegmentGroup_IsEmpty_WithSegments_ReturnsFalse()
        {
            // Arrange
            var segments = _testMessage.Segments("DG1").Take(1);
            var group = new SegmentGroup(segments, "DG1");

            // Act & Assert
            Assert.False(group.IsEmpty);
        }

        [Fact]
        public void SegmentGroup_First_WithSegments_ReturnsFirstSegment()
        {
            // Arrange
            var segments = _testMessage.Segments("DG1").Take(2);
            var group = new SegmentGroup(segments, "DG1");

            // Act
            var first = group.First;

            // Assert
            Assert.NotNull(first);
            Assert.Equal("1", first[1].Value);  // DG1.1 = Set ID
        }

        [Fact]
        public void SegmentGroup_Last_WithSegments_ReturnsLastSegment()
        {
            // Arrange
            var segments = _testMessage.Segments("DG1").Take(2);
            var group = new SegmentGroup(segments, "DG1");

            // Act
            var last = group.Last;

            // Assert
            Assert.NotNull(last);
            Assert.Equal("2", last[1].Value);  // DG1.1 = Set ID
        }

        [Fact]
        public void SegmentGroup_First_WithEmptyGroup_ReturnsNull()
        {
            // Arrange
            var emptyGroup = new SegmentGroup(Enumerable.Empty<HL7lite.Fluent.Accessors.SegmentAccessor>(), "DG1");

            // Act & Assert
            Assert.Null(emptyGroup.First);
        }

        [Fact]
        public void SegmentGroup_Last_WithEmptyGroup_ReturnsNull()
        {
            // Arrange
            var emptyGroup = new SegmentGroup(Enumerable.Empty<HL7lite.Fluent.Accessors.SegmentAccessor>(), "DG1");

            // Act & Assert
            Assert.Null(emptyGroup.Last);
        }

        [Fact]
        public void SegmentGroup_SegmentName_ReturnsCorrectName()
        {
            // Arrange
            var segments = _testMessage.Segments("DG1").Take(1);
            var group = new SegmentGroup(segments, "DG1");

            // Act & Assert
            Assert.Equal("DG1", group.SegmentName);
        }

        [Fact]
        public void SegmentGroup_Indexer_ValidIndex_ReturnsCorrectSegment()
        {
            // Arrange
            var segments = _testMessage.Segments("DG1").Take(2);
            var group = new SegmentGroup(segments, "DG1");

            // Act
            var firstSegment = group[0];
            var secondSegment = group[1];

            // Assert
            Assert.NotNull(firstSegment);
            Assert.NotNull(secondSegment);
            Assert.Equal("1", firstSegment[1].Value);
            Assert.Equal("2", secondSegment[1].Value);
        }

        [Fact]
        public void SegmentGroup_Indexer_NegativeIndex_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var segments = _testMessage.Segments("DG1").Take(2);
            var group = new SegmentGroup(segments, "DG1");

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => group[-1]);
        }

        [Fact]
        public void SegmentGroup_Indexer_IndexTooHigh_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var segments = _testMessage.Segments("DG1").Take(2);
            var group = new SegmentGroup(segments, "DG1");

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => group[2]);
        }

        [Fact]
        public void SegmentGroup_GetEnumerator_CanIterateSegments()
        {
            // Arrange
            var segments = _testMessage.Segments("DG1").Take(2);
            var group = new SegmentGroup(segments, "DG1");

            // Act
            var segmentIds = group.Select(s => s[1].Value).ToList();

            // Assert
            Assert.Equal(2, segmentIds.Count);
            Assert.Equal("1", segmentIds[0]);
            Assert.Equal("2", segmentIds[1]);
        }

        [Fact]
        public void SegmentGroup_LinqOperations_WorkCorrectly()
        {
            // Arrange
            var segments = _testMessage.Segments("DG1").Take(3);
            var group = new SegmentGroup(segments, "DG1");

            // Act
            var count = group.Count();
            var hasAny = group.Any();
            var first = group.First();
            var diabetesSegment = group.FirstOrDefault(s => s[3][2].Value.Contains("Diabetes"));

            // Assert
            Assert.Equal(3, count);
            Assert.True(hasAny);
            Assert.NotNull(first);
            Assert.NotNull(diabetesSegment);
            Assert.Equal("1", diabetesSegment[1].Value);
        }
    }
}