using System;
using HL7lite.Fluent.Querying;
using Xunit;

namespace HL7lite.Test.Fluent.Querying
{
    public class PathParserTests
    {
        #region Segment-Level Parsing Tests
        
        [Fact]
        public void Parse_SimpleSegment_ParsesCorrectly()
        {
            // Arrange & Act
            var result = PathParser.Parse("PID");
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("PID", result.SegmentName);
            Assert.Equal(1, result.SegmentRepetition);
            Assert.Null(result.FieldNumber);
            Assert.Equal(1, result.FieldRepetition);
            Assert.Null(result.ComponentNumber);
            Assert.Null(result.SubComponentNumber);
            Assert.True(result.IsSegmentLevel);
            Assert.False(result.IsFieldLevel);
            Assert.False(result.IsComponentLevel);
            Assert.False(result.IsSubComponentLevel);
        }
        
        [Fact]
        public void Parse_SegmentWithRepetition_ParsesCorrectly()
        {
            // Arrange & Act
            var result = PathParser.Parse("DG1[3]");
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("DG1", result.SegmentName);
            Assert.Equal(3, result.SegmentRepetition);
            Assert.Null(result.FieldNumber);
            Assert.True(result.IsSegmentLevel);
        }
        
        [Theory]
        [InlineData("MSH")]
        [InlineData("PID")]
        [InlineData("PV1")]
        [InlineData("OBX")]
        [InlineData("DG1")]
        [InlineData("AL1")]
        [InlineData("ZZ1")]
        [InlineData("ZZ9")]
        public void Parse_ValidSegmentNames_ParsesCorrectly(string segmentName)
        {
            // Arrange & Act
            var result = PathParser.Parse(segmentName);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(segmentName, result.SegmentName);
        }
        
        [Theory]
        [InlineData("A")]      // Too short
        [InlineData("AB")]     // Too short
        [InlineData("ABCD")]   // Too long
        [InlineData("1BC")]    // Starts with number
        [InlineData("A1C")]    // Invalid pattern (2nd position must be A-Z)
        [InlineData("Z99")]    // Invalid pattern (2nd position must be A-Z)
        [InlineData("abc")]    // Lowercase
        public void Parse_InvalidSegmentNames_ReturnsNull(string segmentName)
        {
            // Arrange & Act
            var result = PathParser.Parse(segmentName);
            
            // Assert
            Assert.Null(result);
        }
        
        [Theory]
        [InlineData("PID[0]")]   // Zero repetition
        [InlineData("PID[-1]")]  // Negative repetition
        [InlineData("PID[abc]")] // Non-numeric repetition
        public void Parse_InvalidSegmentRepetitions_ReturnsNull(string path)
        {
            // Arrange & Act
            var result = PathParser.Parse(path);
            
            // Assert
            Assert.Null(result);
        }
        
        #endregion
        
        #region Field-Level Parsing Tests
        
        [Fact]
        public void Parse_SegmentWithField_ParsesCorrectly()
        {
            // Arrange & Act
            var result = PathParser.Parse("PID.5");
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("PID", result.SegmentName);
            Assert.Equal(1, result.SegmentRepetition);
            Assert.Equal(5, result.FieldNumber);
            Assert.Equal(1, result.FieldRepetition);
            Assert.Null(result.ComponentNumber);
            Assert.Null(result.SubComponentNumber);
            Assert.False(result.IsSegmentLevel);
            Assert.True(result.IsFieldLevel);
            Assert.False(result.IsComponentLevel);
            Assert.False(result.IsSubComponentLevel);
        }
        
        [Fact]
        public void Parse_SegmentWithFieldRepetition_ParsesCorrectly()
        {
            // Arrange & Act
            var result = PathParser.Parse("PID.3[2]");
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("PID", result.SegmentName);
            Assert.Equal(3, result.FieldNumber);
            Assert.Equal(2, result.FieldRepetition);
            Assert.True(result.IsFieldLevel);
        }
        
        [Fact]
        public void Parse_SegmentRepetitionWithFieldRepetition_ParsesCorrectly()
        {
            // Arrange & Act
            var result = PathParser.Parse("DG1[2].4[3]");
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("DG1", result.SegmentName);
            Assert.Equal(2, result.SegmentRepetition);
            Assert.Equal(4, result.FieldNumber);
            Assert.Equal(3, result.FieldRepetition);
            Assert.True(result.IsFieldLevel);
        }
        
        [Theory]
        [InlineData("PID.0")]    // Zero field
        [InlineData("PID.-1")]   // Negative field
        [InlineData("PID.abc")]  // Non-numeric field
        [InlineData("PID.5[0]")] // Zero field repetition
        [InlineData("PID.5[-1]")]// Negative field repetition
        public void Parse_InvalidFieldNumbers_ReturnsNull(string path)
        {
            // Arrange & Act
            var result = PathParser.Parse(path);
            
            // Assert
            Assert.Null(result);
        }
        
        #endregion
        
        #region Component-Level Parsing Tests
        
        [Fact]
        public void Parse_SegmentWithFieldAndComponent_ParsesCorrectly()
        {
            // Arrange & Act
            var result = PathParser.Parse("PID.5.1");
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("PID", result.SegmentName);
            Assert.Equal(5, result.FieldNumber);
            Assert.Equal(1, result.ComponentNumber);
            Assert.Null(result.SubComponentNumber);
            Assert.False(result.IsSegmentLevel);
            Assert.False(result.IsFieldLevel);
            Assert.True(result.IsComponentLevel);
            Assert.False(result.IsSubComponentLevel);
        }
        
        [Fact]
        public void Parse_ComplexComponentPath_ParsesCorrectly()
        {
            // Arrange & Act
            var result = PathParser.Parse("DG1[2].3[1].4");
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("DG1", result.SegmentName);
            Assert.Equal(2, result.SegmentRepetition);
            Assert.Equal(3, result.FieldNumber);
            Assert.Equal(1, result.FieldRepetition);
            Assert.Equal(4, result.ComponentNumber);
            Assert.True(result.IsComponentLevel);
        }
        
        [Theory]
        [InlineData("PID.5.0")]   // Zero component
        [InlineData("PID.5.-1")]  // Negative component
        [InlineData("PID.5.abc")] // Non-numeric component
        public void Parse_InvalidComponentNumbers_ReturnsNull(string path)
        {
            // Arrange & Act
            var result = PathParser.Parse(path);
            
            // Assert
            Assert.Null(result);
        }
        
        #endregion
        
        #region SubComponent-Level Parsing Tests
        
        [Fact]
        public void Parse_FullPath_ParsesCorrectly()
        {
            // Arrange & Act
            var result = PathParser.Parse("PID.5.1.2");
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("PID", result.SegmentName);
            Assert.Equal(1, result.SegmentRepetition);
            Assert.Equal(5, result.FieldNumber);
            Assert.Equal(1, result.FieldRepetition);
            Assert.Equal(1, result.ComponentNumber);
            Assert.Equal(2, result.SubComponentNumber);
            Assert.False(result.IsSegmentLevel);
            Assert.False(result.IsFieldLevel);
            Assert.False(result.IsComponentLevel);
            Assert.True(result.IsSubComponentLevel);
        }
        
        [Fact]
        public void Parse_ComplexFullPath_ParsesCorrectly()
        {
            // Arrange & Act
            var result = PathParser.Parse("DG1[3].4[2].1.3");
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("DG1", result.SegmentName);
            Assert.Equal(3, result.SegmentRepetition);
            Assert.Equal(4, result.FieldNumber);
            Assert.Equal(2, result.FieldRepetition);
            Assert.Equal(1, result.ComponentNumber);
            Assert.Equal(3, result.SubComponentNumber);
            Assert.True(result.IsSubComponentLevel);
        }
        
        [Theory]
        [InlineData("PID.5.1.0")]   // Zero subcomponent
        [InlineData("PID.5.1.-1")]  // Negative subcomponent
        [InlineData("PID.5.1.abc")] // Non-numeric subcomponent
        public void Parse_InvalidSubComponentNumbers_ReturnsNull(string path)
        {
            // Arrange & Act
            var result = PathParser.Parse(path);
            
            // Assert
            Assert.Null(result);
        }
        
        #endregion
        
        #region Edge Cases and Error Handling
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Parse_NullOrEmptyPath_ReturnsNull(string path)
        {
            // Arrange & Act
            var result = PathParser.Parse(path);
            
            // Assert
            Assert.Null(result);
        }
        
        [Theory]
        [InlineData("PID.5.1.2.3")]    // Too many parts
        [InlineData("PID.5.1.2.3.4")]  // Way too many parts
        public void Parse_TooManyParts_ReturnsNull(string path)
        {
            // Arrange & Act
            var result = PathParser.Parse(path);
            
            // Assert
            Assert.Null(result);
        }
        
        [Theory]
        [InlineData("PID..5")]     // Empty field
        [InlineData("PID.5..1")]   // Empty component
        [InlineData("PID.5.1.")]   // Trailing dot
        [InlineData(".PID.5")]     // Leading dot
        public void Parse_MalformedPaths_ReturnsNull(string path)
        {
            // Arrange & Act
            var result = PathParser.Parse(path);
            
            // Assert
            Assert.Null(result);
        }
        
        #endregion
        
        #region IsValid Method Tests
        
        [Theory]
        [InlineData("PID", true)]
        [InlineData("PID[2]", true)]
        [InlineData("PID.5", true)]
        [InlineData("PID.3[2]", true)]
        [InlineData("PID.5.1", true)]
        [InlineData("DG1[3].4[2].1.3", true)]
        [InlineData("", false)]
        [InlineData("INVALID", false)]
        [InlineData("PID.0", false)]
        [InlineData("PID[0]", false)]
        [InlineData("PID.5.1.2.3", false)]
        public void IsValid_VariousPaths_ReturnsExpectedResult(string path, bool expected)
        {
            // Arrange & Act
            var result = PathParser.IsValid(path);
            
            // Assert
            Assert.Equal(expected, result);
        }
        
        #endregion
        
        #region TryParse Method Tests
        
        [Fact]
        public void TryParse_ValidPath_ReturnsTrue()
        {
            // Arrange & Act
            var success = PathParser.TryParse("PID.5.1", out var result);
            
            // Assert
            Assert.True(success);
            Assert.NotNull(result);
            Assert.Equal("PID", result.SegmentName);
            Assert.Equal(5, result.FieldNumber);
            Assert.Equal(1, result.ComponentNumber);
        }
        
        [Fact]
        public void TryParse_InvalidPath_ReturnsFalse()
        {
            // Arrange & Act
            var success = PathParser.TryParse("INVALID", out var result);
            
            // Assert
            Assert.False(success);
            Assert.Null(result);
        }
        
        #endregion
        
        #region ToString Method Tests
        
        [Theory]
        [InlineData("PID")]
        [InlineData("PID[2]")]
        [InlineData("PID.5")]
        [InlineData("PID.3[2]")]
        [InlineData("PID.5.1")]
        [InlineData("PID.5.1.2")]
        [InlineData("DG1[3].4[2].1.3")]
        public void ToString_ParsedPath_ReconstructsOriginalPath(string originalPath)
        {
            // Arrange
            var parsed = PathParser.Parse(originalPath);
            Assert.NotNull(parsed);
            
            // Act
            var reconstructed = parsed.ToString();
            
            // Assert
            Assert.Equal(originalPath, reconstructed);
        }
        
        #endregion
        
        #region PathParser GetValue and PathExists Tests
        
        [Fact]
        public void GetValue_WithExistingPath_ReturnsCorrectValue()
        {
            // Arrange
            const string hl7 = @"MSH|^~\&|SendingApp|SendingFacility|ReceivingApp|ReceivingFacility|20230101120000||ADT^A01|12345|P|2.5||
PID|1||12345^^^MRN~67890^^^SSN||Doe^John^M^Jr||19800101|M|||123 Main St^^City^ST^12345||5551234567~5559876543|||||||||||||||||";
            var message = new Message(hl7);
            message.ParseMessage();
            
            // Act & Assert
            Assert.Equal("Doe", PathParser.GetValue(message, "PID.5.1"));
            Assert.Equal("John", PathParser.GetValue(message, "PID.5.2"));
            Assert.Equal("M", PathParser.GetValue(message, "PID.5.3"));
            Assert.Equal("Jr", PathParser.GetValue(message, "PID.5.4"));
            Assert.Equal("12345^^^MRN", PathParser.GetValue(message, "PID.3[1]"));
            Assert.Equal("67890^^^SSN", PathParser.GetValue(message, "PID.3[2]"));
        }
        
        [Fact]
        public void GetValue_WithNonExistentPath_ReturnsEmptyString()
        {
            // Arrange
            const string hl7 = @"MSH|^~\&|SendingApp|SendingFacility|ReceivingApp|ReceivingFacility|20230101120000||ADT^A01|12345|P|2.5||
PID|1||12345|||Doe^John||19800101|M||||||||||||||||||||||||||";
            var message = new Message(hl7);
            message.ParseMessage();
            
            // Act & Assert
            Assert.Equal("", PathParser.GetValue(message, "ZZZ.1"));
            Assert.Equal("", PathParser.GetValue(message, "PID.99"));
            Assert.Equal("", PathParser.GetValue(message, "PID.5.99"));
            Assert.Equal("", PathParser.GetValue(message, "PID[2].5.1")); // Second PID doesn't exist
        }
        
        [Fact]
        public void PathExists_WithExistingPath_ReturnsTrue()
        {
            // Arrange
            const string hl7 = @"MSH|^~\&|SendingApp|SendingFacility|ReceivingApp|ReceivingFacility|20230101120000||ADT^A01|12345|P|2.5||
PID|1||12345^^^MRN~67890^^^SSN||Doe^John^M^Jr||19800101|M|||123 Main St^^City^ST^12345||5551234567~5559876543|||||||||||||||||";
            var message = new Message(hl7);
            message.ParseMessage();
            
            // Act & Assert
            Assert.True(PathParser.PathExists(message, "PID"));
            Assert.True(PathParser.PathExists(message, "PID.5"));
            Assert.True(PathParser.PathExists(message, "PID.5.1"));
            Assert.True(PathParser.PathExists(message, "PID.3[1]"));
            Assert.True(PathParser.PathExists(message, "PID.3[2]"));
        }
        
        [Fact]
        public void PathExists_WithNonExistentPath_ReturnsFalse()
        {
            // Arrange
            const string hl7 = @"MSH|^~\&|SendingApp|SendingFacility|ReceivingApp|ReceivingFacility|20230101120000||ADT^A01|12345|P|2.5||
PID|1||12345|||Doe^John||19800101|M||||||||||||||||||||||||||";
            var message = new Message(hl7);
            message.ParseMessage();
            
            // Act & Assert
            Assert.False(PathParser.PathExists(message, "ZZZ"));
            Assert.False(PathParser.PathExists(message, "PID.99"));
            Assert.False(PathParser.PathExists(message, "PID.5.99"));
            Assert.False(PathParser.PathExists(message, "PID[2]")); // Second PID doesn't exist
        }
        
        [Fact]
        public void GetValue_WithSegmentRepetitions_WorksCorrectly()
        {
            // Arrange - Create message with multiple DG1 segments
            const string hl7 = @"MSH|^~\&|SendingApp|SendingFacility|ReceivingApp|ReceivingFacility|20230101120000||ADT^A01|12345|P|2.5||
PID|1||12345|||Doe^John||19800101|M||||||||||||||||||||||||||
DG1|1||I10^250.00^Diabetes mellitus type 2||F
DG1|2||I11^401.9^Essential hypertension||F";
            var message = new Message(hl7);
            message.ParseMessage();
            
            // Act & Assert
            Assert.Equal("I10", PathParser.GetValue(message, "DG1[1].3.1"));
            Assert.Equal("I11", PathParser.GetValue(message, "DG1[2].3.1"));
            Assert.Equal("250.00", PathParser.GetValue(message, "DG1[1].3.2"));
            Assert.Equal("401.9", PathParser.GetValue(message, "DG1[2].3.2"));
        }
        
        #endregion
    }
}