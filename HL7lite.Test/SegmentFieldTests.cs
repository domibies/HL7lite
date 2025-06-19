using System;
using Xunit;

namespace HL7lite.Test
{
    public class SegmentFieldTests
    {
        private HL7Encoding _encoding;

        public SegmentFieldTests()
        {
            _encoding = new HL7Encoding();
        }

        [Fact]
        public void Segment_AddNewField_WithValidPosition_ShouldAddFieldAtCorrectIndex()
        {
            // Arrange
            var segment = new Segment("TST", _encoding);

            // Act
            segment.AddNewField("Field1", 1); // Should go to index 0
            segment.AddNewField("Field2", 2); // Should go to index 1
            segment.AddNewField("Field3", 1); // Should replace field at index 0

            // Assert
            Assert.Equal(2, segment.GetAllFields().Count);
            Assert.Equal("Field3", segment.Fields(1).Value);
            Assert.Equal("Field2", segment.Fields(2).Value);
        }

        [Fact]
        public void Segment_AddNewField_WithPositionZero_ShouldThrowHL7Exception()
        {
            // Arrange
            var segment = new Segment("TST", _encoding);

            // Act & Assert
            var exception = Assert.Throws<HL7Exception>(() => segment.AddNewField("TestField", 0));
            Assert.Contains("Element position must be greater than or equal to 1", exception.Message);
        }

        [Fact]
        public void Segment_AddNewField_WithNegativePosition_ShouldAppendToEnd()
        {
            // Arrange
            var segment = new Segment("TST", _encoding);
            segment.AddNewField("Field1");
            segment.AddNewField("Field2");

            // Act - -1 is special case for append
            segment.AddNewField("Field3", -1);

            // Assert
            var fields = segment.GetAllFields();
            Assert.Equal(3, fields.Count);
            Assert.Equal("Field3", fields[2].Value);
        }

        [Fact]
        public void Segment_AddNewField_WithPositionBeyondCount_ShouldFillGapsWithEmptyFields()
        {
            // Arrange
            var segment = new Segment("TST", _encoding);

            // Act
            segment.AddNewField("TestField", 5); // Should add to position 5 (index 4)

            // Assert
            var fields = segment.GetAllFields();
            Assert.Equal(5, fields.Count);
            Assert.Equal(string.Empty, fields[0].Value); // Gap field
            Assert.Equal(string.Empty, fields[1].Value); // Gap field
            Assert.Equal(string.Empty, fields[2].Value); // Gap field
            Assert.Equal(string.Empty, fields[3].Value); // Gap field
            Assert.Equal("TestField", fields[4].Value);  // Our field
        }

        [Fact]
        public void Segment_AddNewField_WithoutPosition_ShouldAppendToEnd()
        {
            // Arrange
            var segment = new Segment("TST", _encoding);

            // Act
            segment.AddNewField("Field1");
            segment.AddNewField("Field2");

            // Assert
            var fields = segment.GetAllFields();
            Assert.Equal(2, fields.Count);
            Assert.Equal("Field1", fields[0].Value);
            Assert.Equal("Field2", fields[1].Value);
        }

        [Fact]
        public void Segment_AddNewField_WithPosition_ShouldUseOneBasedIndexing()
        {
            // Arrange
            var segment = new Segment("TST", _encoding);

            // Act
            segment.AddNewField("Field1", 1);
            segment.AddNewField("Field2", 3); // Should create gap at position 2
            segment.AddNewField("Field3", 2); // Should fill the gap

            // Assert
            var fields = segment.GetAllFields();
            Assert.Equal(3, fields.Count);
            Assert.Equal("Field1", fields[0].Value);
            Assert.Equal("Field3", fields[1].Value);
            Assert.Equal("Field2", fields[2].Value);
        }

        [Fact]
        public void Segment_Fields_Method_UsesOneBasedIndexing()
        {
            // Arrange
            var segment = new Segment("TST", _encoding);
            segment.AddNewField("Field1", 1);
            segment.AddNewField("Field2", 2);
            segment.AddNewField("Field3", 3);

            // Act & Assert
            Assert.Equal("Field1", segment.Fields(1).Value);
            Assert.Equal("Field2", segment.Fields(2).Value);
            Assert.Equal("Field3", segment.Fields(3).Value);
        }

        [Fact]
        public void Segment_AddNewField_ComplexScenario_ShouldHandleMultipleOperations()
        {
            // Arrange
            var segment = new Segment("TST", _encoding);

            // Act - Add fields in various positions
            segment.AddNewField("First", 1);
            segment.AddNewField("Fifth", 5);    // Creates gaps
            segment.AddNewField("Third", 3);    // Fills one gap
            segment.AddNewField("Second", 2);   // Fills another gap
            segment.AddNewField("NewFirst", 1); // Replaces first

            // Assert
            var fields = segment.GetAllFields();
            Assert.Equal(5, fields.Count);
            Assert.Equal("NewFirst", segment.Fields(1).Value);
            Assert.Equal("Second", segment.Fields(2).Value);
            Assert.Equal("Third", segment.Fields(3).Value);
            Assert.Equal(string.Empty, segment.Fields(4).Value); // Gap field
            Assert.Equal("Fifth", segment.Fields(5).Value);
        }

    }
}