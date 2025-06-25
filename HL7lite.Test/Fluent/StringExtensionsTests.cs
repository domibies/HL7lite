using System;
using HL7lite.Fluent;
using Xunit;

namespace HL7lite.Test.Fluent
{
    public class StringExtensionsTests
    {
        // Helper methods for common assertion patterns (following existing test conventions)
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
        public void ToFluentMessage_WithValidHL7String_ShouldReturnFluentMessage()
        {
            // Arrange
            const string hl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ADT^A01|12345|P|2.3||
PID|1||12345^^^MRN||DOE^JOHN^M||19800101|M|||123 MAIN ST^^CITY^ST^12345||5551234567|||||||||||||||||";

            // Act
            var fluentMessage = hl7.ToFluentMessage();

            // Assert
            Assert.NotNull(fluentMessage);
            Assert.NotNull(fluentMessage.UnderlyingMessage);
            Assert.True(fluentMessage.MSH.Exists);
            Assert.True(fluentMessage.PID.Exists);
            Assert.Equal("DOE^JOHN^M", fluentMessage.PID[5].Value);
        }

        [Fact]
        public void ToFluentMessage_WithValidHL7StringAndValidationDisabled_ShouldReturnFluentMessage()
        {
            // Arrange
            const string hl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ADT^A01|12345|P|2.3||
PID|1||12345^^^MRN||DOE^JOHN^M||19800101|M|||123 MAIN ST^^CITY^ST^12345||5551234567|||||||||||||||||";

            // Act
            var fluentMessage = hl7.ToFluentMessage(validate: false);

            // Assert
            Assert.NotNull(fluentMessage);
            Assert.NotNull(fluentMessage.UnderlyingMessage);
            Assert.True(fluentMessage.MSH.Exists);
            Assert.True(fluentMessage.PID.Exists);
            Assert.Equal("DOE^JOHN^M", fluentMessage.PID[5].Value);
        }

        [Fact]
        public void ToFluentMessage_WithNullString_ShouldThrowArgumentNullException()
        {
            // Arrange
            string hl7 = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => hl7.ToFluentMessage());
        }

        [Fact]
        public void ToFluentMessage_WithInvalidHL7String_ShouldThrowHL7Exception()
        {
            // Arrange
            const string invalidHl7 = "This is not a valid HL7 message";

            // Act & Assert
            AssertThrowsHL7Exception(() => invalidHl7.ToFluentMessage());
        }

        [Fact]
        public void ToFluentMessage_WithEmptyString_ShouldThrowHL7Exception()
        {
            // Arrange
            const string emptyHl7 = "";

            // Act & Assert
            AssertThrowsHL7Exception(() => emptyHl7.ToFluentMessage());
        }

        [Fact]
        public void ToFluentMessage_WithComplexMessage_ShouldParseCorrectly()
        {
            // Arrange
            const string hl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ORU^R01|12345|P|2.3||
PID|1||12345^^^MRN||DOE^JOHN^M||19800101|M|||123 MAIN ST^^CITY^ST^12345||5551234567|||||||||||||||||
OBR|1|ORDER123|RESULT456|CBC^COMPLETE BLOOD COUNT^L|||20210330110000|||||||||||||||||||F||
OBX|1|NM|WBC^WHITE BLOOD COUNT^L||7.5|10*3/uL|4.0-11.0|N|||F||
OBX|2|NM|RBC^RED BLOOD COUNT^L||4.2|10*6/uL|4.2-5.8|N|||F||";

            // Act
            var fluentMessage = hl7.ToFluentMessage();

            // Assert
            Assert.NotNull(fluentMessage);
            Assert.True(fluentMessage.MSH.Exists);
            Assert.True(fluentMessage.PID.Exists);
            Assert.True(fluentMessage.OBR.Exists);
            Assert.True(fluentMessage.OBX.Exists);
            
            // Verify specific field values
            Assert.Equal("DOE^JOHN^M", fluentMessage.PID[5].Value);
            Assert.Equal("CBC^COMPLETE BLOOD COUNT^L", fluentMessage.OBR[4].Value);
            Assert.Equal("7.5", fluentMessage.OBX[5].Value);
            
            // Verify segments collection works
            var obxSegments = fluentMessage.Segments("OBX");
            Assert.Equal(2, obxSegments.Count);
            Assert.Equal("WBC^WHITE BLOOD COUNT^L", obxSegments[0][3].Value);
            Assert.Equal("RBC^RED BLOOD COUNT^L", obxSegments[1][3].Value);
        }

        [Fact]
        public void ToFluentMessage_WithSpecialCharacters_ShouldHandleEncoding()
        {
            // Arrange - Message with special HL7 characters (repetition separator ~)
            const string hl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ADT^A01|12345|P|2.3||
PID|1||12345^^^MRN||DOE^JOHN^M~JOHNNY^J^M||19800101|M|||123 MAIN ST\S\APARTMENT 2^^CITY^ST^12345||5551234567|||||||||||||||||";

            // Act
            var fluentMessage = hl7.ToFluentMessage();

            // Assert
            Assert.NotNull(fluentMessage);
            // First repetition should be the first part
            Assert.Equal("DOE^JOHN^M", fluentMessage.PID[5].Repetition(1).Value);
            Assert.Contains("APARTMENT 2", fluentMessage.PID[11].Value);
        }

        [Fact]
        public void ToFluentMessage_ResultCanBeManipulated_ShouldWorkCorrectly()
        {
            // Arrange
            const string hl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ADT^A01|12345|P|2.3||
PID|1||12345^^^MRN||DOE^JOHN^M||19800101|M|||123 MAIN ST^^CITY^ST^12345||5551234567|||||||||||||||||";

            // Act
            var fluentMessage = hl7.ToFluentMessage();
            
            // Manipulate the message
            fluentMessage.PID[5].Set("SMITH^JANE^F");
            fluentMessage.PID[8].Set("F");

            // Assert
            Assert.Equal("SMITH^JANE^F", fluentMessage.PID[5].Value);
            Assert.Equal("F", fluentMessage.PID[8].Value);
        }

        [Fact]
        public void ToFluentMessage_CanAddSegmentsAndFields_ShouldWorkCorrectly()
        {
            // Arrange
            const string hl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ADT^A01|12345|P|2.3||
PID|1||12345^^^MRN||DOE^JOHN^M||19800101|M|||123 MAIN ST^^CITY^ST^12345||5551234567|||||||||||||||||";

            // Act
            var fluentMessage = hl7.ToFluentMessage();
            
            // Add new segment via fluent API
            fluentMessage.Segments("OBX").Add();
            fluentMessage.OBX[1].Set("1");
            fluentMessage.OBX[2].Set("NM");
            fluentMessage.OBX[3].Set("GLUCOSE");

            // Assert
            Assert.True(fluentMessage.OBX.Exists);
            Assert.Equal("1", fluentMessage.OBX[1].Value);
            Assert.Equal("NM", fluentMessage.OBX[2].Value);
            Assert.Equal("GLUCOSE", fluentMessage.OBX[3].Value);
        }
    }
}