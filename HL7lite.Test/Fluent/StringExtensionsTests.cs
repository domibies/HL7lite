using System;
using HL7lite.Fluent;
using Xunit;

namespace HL7lite.Test.Fluent
{
    public class StringExtensionsTests
    {
        [Fact]
        public void TryParse_WithValidHL7String_ShouldReturnSuccessResult()
        {
            // Arrange
            const string hl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ADT^A01|12345|P|2.3||
PID|1||12345^^^MRN||DOE^JOHN^M||19800101|M|||123 MAIN ST^^CITY^ST^12345||5551234567|||||||||||||||||";

            // Act
            var result = hl7.TryParse();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Message);
            Assert.Null(result.ErrorMessage);
            Assert.Null(result.ErrorCode);
            Assert.True(result.Message.MSH.Exists);
            Assert.True(result.Message.PID.Exists);
            Assert.Equal("DOE^JOHN^M", result.Message.PID[5].Raw);
        }

        [Fact]
        public void TryParse_WithValidHL7StringAndValidationDisabled_ShouldReturnSuccessResult()
        {
            // Arrange
            const string hl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ADT^A01|12345|P|2.3||
PID|1||12345^^^MRN||DOE^JOHN^M||19800101|M|||123 MAIN ST^^CITY^ST^12345||5551234567|||||||||||||||||";

            // Act
            var result = hl7.TryParse(validate: false);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Message);
            Assert.Null(result.ErrorMessage);
            Assert.True(result.Message.MSH.Exists);
            Assert.True(result.Message.PID.Exists);
            Assert.Equal("DOE^JOHN^M", result.Message.PID[5].Raw);
        }

        [Fact]
        public void TryParse_WithNullString_ShouldReturnFailureResult()
        {
            // Arrange
            string hl7 = null;

            // Act
            var result = hl7.TryParse();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.Message);
            Assert.NotNull(result.ErrorMessage);
            Assert.Equal("HL7 message cannot be null", result.ErrorMessage);
        }

        [Fact]
        public void TryParse_WithEmptyString_ShouldReturnFailureResult()
        {
            // Arrange
            const string emptyHl7 = "";

            // Act
            var result = emptyHl7.TryParse();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.Message);
            Assert.NotNull(result.ErrorMessage);
            Assert.Equal("HL7 message cannot be empty", result.ErrorMessage);
        }

        [Fact]
        public void TryParse_WithWhitespaceString_ShouldReturnFailureResult()
        {
            // Arrange
            const string whitespaceHl7 = "   \t\n  ";

            // Act
            var result = whitespaceHl7.TryParse();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.Message);
            Assert.NotNull(result.ErrorMessage);
            Assert.Equal("HL7 message cannot be empty", result.ErrorMessage);
        }

        [Fact]
        public void TryParse_WithInvalidHL7String_ShouldReturnFailureResult()
        {
            // Arrange
            const string invalidHl7 = "This is not a valid HL7 message";

            // Act
            var result = invalidHl7.TryParse();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.Message);
            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public void TryParse_WithComplexMessage_ShouldParseCorrectly()
        {
            // Arrange
            const string hl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ORU^R01|12345|P|2.3||
PID|1||12345^^^MRN||DOE^JOHN^M||19800101|M|||123 MAIN ST^^CITY^ST^12345||5551234567|||||||||||||||||
OBR|1|ORDER123|RESULT456|CBC^COMPLETE BLOOD COUNT^L|||20210330110000|||||||||||||||||||F||
OBX|1|NM|WBC^WHITE BLOOD COUNT^L||7.5|10*3/uL|4.0-11.0|N|||F||
OBX|2|NM|RBC^RED BLOOD COUNT^L||4.2|10*6/uL|4.2-5.8|N|||F||";

            // Act
            var result = hl7.TryParse();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Message);
            var message = result.Message;
            Assert.True(message.MSH.Exists);
            Assert.True(message.PID.Exists);
            Assert.True(message.OBR.Exists);
            Assert.True(message.OBX.Exists);
            
            // Verify specific field values
            Assert.Equal("DOE^JOHN^M", message.PID[5].Raw);
            Assert.Equal("CBC^COMPLETE BLOOD COUNT^L", message.OBR[4].Raw);
            Assert.Equal("7.5", message.OBX[5].Raw);
            
            // Verify segments collection works
            var obxSegments = message.Segments("OBX");
            Assert.Equal(2, obxSegments.Count);
            Assert.Equal("WBC^WHITE BLOOD COUNT^L", obxSegments[0][3].Raw);
            Assert.Equal("RBC^RED BLOOD COUNT^L", obxSegments[1][3].Raw);
        }

        [Fact]
        public void TryParse_WithSpecialCharacters_ShouldHandleEncoding()
        {
            // Arrange - Message with special HL7 characters (repetition separator ~)
            const string hl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ADT^A01|12345|P|2.3||
PID|1||12345^^^MRN||DOE^JOHN^M~JOHNNY^J^M||19800101|M|||123 MAIN ST\S\APARTMENT 2^^CITY^ST^12345||5551234567|||||||||||||||||";

            // Act
            var result = hl7.TryParse();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Message);
            var message = result.Message;
            // First repetition should be the first part
            Assert.Equal("DOE^JOHN^M", message.PID[5].Repetition(1).Raw);
            Assert.Contains("APARTMENT 2", message.PID[11].Raw);
        }

        [Fact]
        public void TryParse_ResultCanBeManipulated_ShouldWorkCorrectly()
        {
            // Arrange
            const string hl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ADT^A01|12345|P|2.3||
PID|1||12345^^^MRN||DOE^JOHN^M||19800101|M|||123 MAIN ST^^CITY^ST^12345||5551234567|||||||||||||||||";

            // Act
            var result = hl7.TryParse();
            Assert.True(result.IsSuccess);
            var message = result.Message;
            
            // Manipulate the message
            message.PID[5].SetRaw("SMITH^JANE^F");
            message.PID[8].SetRaw("F");

            // Assert
            Assert.Equal("SMITH^JANE^F", message.PID[5].Raw);
            Assert.Equal("F", message.PID[8].Raw);
        }

        [Fact]
        public void TryParse_CanAddSegmentsAndFields_ShouldWorkCorrectly()
        {
            // Arrange
            const string hl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ADT^A01|12345|P|2.3||
PID|1||12345^^^MRN||DOE^JOHN^M||19800101|M|||123 MAIN ST^^CITY^ST^12345||5551234567|||||||||||||||||";

            // Act
            var result = hl7.TryParse();
            Assert.True(result.IsSuccess);
            var message = result.Message;
            
            // Add new segment via fluent API
            message.Segments("OBX").Add();
            message.OBX[1].Set("1");
            message.OBX[2].Set("NM");
            message.OBX[3].Set("GLUCOSE");

            // Assert
            Assert.True(message.OBX.Exists);
            Assert.Equal("1", message.OBX[1].Raw);
            Assert.Equal("NM", message.OBX[2].Raw);
            Assert.Equal("GLUCOSE", message.OBX[3].Raw);
        }

    }
}