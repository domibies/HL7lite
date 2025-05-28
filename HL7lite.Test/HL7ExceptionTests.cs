using System;
using Xunit;

namespace HL7lite.Test
{
    public class HL7ExceptionTests
    {
        [Fact]
        public void Constructor_WithMessage_ShouldSetMessageProperty()
        {
            // Arrange
            const string expectedMessage = "Test error message";

            // Act
            var exception = new HL7Exception(expectedMessage);

            // Assert
            Assert.Equal(expectedMessage, exception.Message);
            Assert.Null(exception.ErrorCode);
        }

        [Fact]
        public void Constructor_WithMessageAndCode_ShouldSetBothProperties()
        {
            // Arrange
            const string expectedMessage = "Test error message";
            const string expectedCode = "TEST_ERROR_CODE";

            // Act
            var exception = new HL7Exception(expectedMessage, expectedCode);

            // Assert
            Assert.Equal(expectedMessage, exception.Message);
            Assert.Equal(expectedCode, exception.ErrorCode);
        }

        [Fact]
        public void ErrorCode_ShouldBeSettableAndGettable()
        {
            // Arrange
            var exception = new HL7Exception("Test message");
            const string expectedCode = "NEW_ERROR_CODE";

            // Act
            exception.ErrorCode = expectedCode;

            // Assert
            Assert.Equal(expectedCode, exception.ErrorCode);
        }

        [Fact]
        public void ToString_WithErrorCode_ShouldReturnFormattedString()
        {
            // Arrange
            const string message = "Test error message";
            const string errorCode = "TEST_ERROR";
            var exception = new HL7Exception(message, errorCode);
            const string expectedOutput = "TEST_ERROR : Test error message";

            // Act
            var result = exception.ToString();

            // Assert
            Assert.Equal(expectedOutput, result);
        }

        [Fact]
        public void ToString_WithoutErrorCode_ShouldReturnNullPlusMessage()
        {
            // Arrange
            const string message = "Test error message";
            var exception = new HL7Exception(message);
            const string expectedOutput = " : Test error message";

            // Act
            var result = exception.ToString();

            // Assert
            Assert.Equal(expectedOutput, result);
        }

        [Theory]
        [InlineData(HL7Exception.REQUIRED_FIELD_MISSING)]
        [InlineData(HL7Exception.UNSUPPORTED_MESSAGE_TYPE)]
        [InlineData(HL7Exception.BAD_MESSAGE)]
        [InlineData(HL7Exception.PARSING_ERROR)]
        [InlineData(HL7Exception.SERIALIZATION_ERROR)]
        [InlineData(HL7Exception.SEGMENT_TOO_SHORT)]
        [InlineData(HL7Exception.INVALID_REQUEST)]
        public void PredefinedConstants_ShouldHaveExpectedValues(string constantValue)
        {
            // Assert
            Assert.NotNull(constantValue);
            Assert.NotEmpty(constantValue);
            Assert.Contains("Error", constantValue);
        }

        [Fact]
        public void HL7Exception_ShouldInheritFromException()
        {
            // Arrange & Act
            var exception = new HL7Exception("Test");

            // Assert
            Assert.IsAssignableFrom<Exception>(exception);
        }

        [Fact]
        public void HL7Exception_ShouldBeThrowable()
        {
            // Arrange
            const string message = "Test exception";
            const string errorCode = "TEST_CODE";
            HL7Exception caughtException = null;

            // Act
            try
            {
                throw new HL7Exception(message, errorCode);
            }
            catch (HL7Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.Equal(message, caughtException.Message);
            Assert.Equal(errorCode, caughtException.ErrorCode);
        }

        [Fact]
        public void HL7Exception_ShouldWorkWithExceptionHandling()
        {
            // Arrange
            const string expectedMessage = "Parsing failed";
            const string expectedCode = HL7Exception.PARSING_ERROR;

            try
            {
                // Act
                throw new HL7Exception(expectedMessage, expectedCode);
            }
            catch (HL7Exception ex)
            {
                // Assert
                Assert.Equal(expectedMessage, ex.Message);
                Assert.Equal(expectedCode, ex.ErrorCode);
                Assert.Equal($"{expectedCode} : {expectedMessage}", ex.ToString());
            }
            catch (Exception)
            {
                // This should not be reached
                Assert.True(false, "Expected HL7Exception but caught different exception type");
            }
        }
    }
}