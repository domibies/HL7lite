using System;
using HL7lite.Fluent;
using HL7lite.Fluent.Querying;
using Xunit;

namespace HL7lite.Test.Fluent.Querying
{
    public class PathAccessorTests
    {
        private const string SampleMessage = @"MSH|^~\&|SendingApp|SendingFacility|ReceivingApp|ReceivingFacility|20230101120000||ADT^A01|12345|P|2.5||
PID|1||12345^^^MRN~67890^^^SSN||Doe^John^M^Jr||19800101|M|||123 Main St^^City^ST^12345||5551234567~5559876543|||||||||||||||||
PV1||O|NWSLED^^^NYULHLI^^^^^LI NW SLEEP DISORDER^^DEPID||||1447312459^DOE^MICHAEL^^^^^^EPIC^^^^PNPI~DOEM06^DOE^MICHAEL^^^^^^KID^^^^KID|1447312459^DOE^MICHAEL^^^^^^EPIC^^^^PNPI~DOEM06^DOE^MICHAEL^^^^^^KID^^^^KID|||||||||||496779945|||||||||||||||||||||||||20191107|||||||V";

        private FluentMessage CreateTestMessage()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            return new FluentMessage(message);
        }

        private (FluentMessage fluent, Message message) CreateTestMessagePair()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            return (new FluentMessage(message), message);
        }

        [Fact]
        public void Path_WithValidFieldPath_ReturnsCorrectValue()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();

            // Act
            var result = fluent.Path("PID.5.1").Value;

            // Assert
            Assert.Equal("Doe", result);
        }

        [Fact]
        public void Path_WithValidComponentPath_ReturnsCorrectValue()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();

            // Act
            var familyName = fluent.Path("PID.5.1").Value;
            var givenName = fluent.Path("PID.5.2").Value;
            var middleName = fluent.Path("PID.5.3").Value;

            // Assert
            Assert.Equal("Doe", familyName);
            Assert.Equal("John", givenName);
            Assert.Equal("M", middleName);
        }

        [Fact]
        public void Path_WithValidFieldOnlyPath_ReturnsCompleteFieldValue()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();

            // Act
            var fullName = fluent.Path("PID.5").Value;

            // Assert
            Assert.Equal("Doe^John^M^Jr", fullName);
        }

        [Fact]
        public void Path_WithRepetitionSyntax_ReturnsCorrectValue()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();

            // Act
            var firstId = fluent.Path("PID.3[1]").Value;
            var secondId = fluent.Path("PID.3[2]").Value;

            // Assert
            Assert.Equal("12345^^^MRN", firstId);
            Assert.Equal("67890^^^SSN", secondId);
        }

        [Fact]
        public void Path_WithRepetitionAndComponent_ReturnsCorrectValue()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();

            // Act
            var firstPhone = fluent.Path("PID.13[1]").Value;
            var secondPhone = fluent.Path("PID.13[2]").Value;

            // Assert
            Assert.Equal("5551234567", firstPhone);
            Assert.Equal("5559876543", secondPhone);
        }

        [Fact]
        public void Path_WithComplexRepetitionPath_ReturnsCorrectValue()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();

            // Act
            var doctorId = fluent.Path("PV1.7[1].1").Value;
            var doctorName = fluent.Path("PV1.7[2].1").Value;

            // Assert
            Assert.Equal("1447312459", doctorId);
            Assert.Equal("DOEM06", doctorName);
        }

        [Fact]
        public void Path_WithNonExistentPath_ReturnsEmptyString()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();

            // Act
            var result = fluent.Path("ZZZ.999").Value;

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void Path_WithNonExistentField_ReturnsEmptyString()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();

            // Act
            var result = fluent.Path("PID.99").Value;

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void Exists_WithValidPath_ReturnsTrue()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();

            // Act & Assert
            Assert.True(fluent.Path("PID.5.1").Exists);
            Assert.True(fluent.Path("PID.5").Exists);
            Assert.True(fluent.Path("MSH.9").Exists);
        }

        [Fact]
        public void Exists_WithNonExistentPath_ReturnsFalse()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();

            // Act & Assert
            Assert.False(fluent.Path("ZZZ.999").Exists);
            Assert.False(fluent.Path("PID.99").Exists);
            Assert.False(fluent.Path("PID.5.99").Exists);
        }

        [Fact]
        public void HasValue_WithNonEmptyField_ReturnsTrue()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();

            // Act & Assert
            Assert.True(fluent.Path("PID.5.1").HasValue);
            Assert.True(fluent.Path("MSH.9").HasValue);
        }

        [Fact]
        public void HasValue_WithEmptyField_ReturnsFalse()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();

            // Act & Assert
            Assert.False(fluent.Path("PID.99").HasValue);
            Assert.False(fluent.Path("ZZZ.999").HasValue);
        }

        [Fact]
        public void Set_WithValidPath_UpdatesValue()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            var originalValue = fluent.Path("PID.5.1").Value;
            Assert.Equal("Doe", originalValue);

            // Act
            fluent.Path("PID.5.1").Set("Smith");

            // Assert
            Assert.Equal("Smith", fluent.Path("PID.5.1").Value);
        }

        [Fact]
        public void Set_ReturnsFluentMessage_AllowsChaining()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();

            // Act
            var result = fluent.Path("PID.5.1").Set("Smith")
                              .Path("PID.5.2").Set("Jane");

            // Assert
            Assert.Same(fluent, result);
            Assert.Equal("Smith", fluent.Path("PID.5.1").Value);
            Assert.Equal("Jane", fluent.Path("PID.5.2").Value);
        }

        [Fact]
        public void Set_WithNewPath_CreatesElement()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            Assert.False(fluent.Path("PID.39").Exists);

            // Act - Should not throw, creates missing elements
            fluent.Path("PID.39").Set("NewValue");

            // Assert
            Assert.True(fluent.Path("PID.39").Exists);
            Assert.Equal("NewValue", fluent.Path("PID.39").Value);
        }

        [Fact]
        public void Set_WithExistingPath_UpdatesValue()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            var originalValue = fluent.Path("PID.5.1").Value;
            Assert.Equal("Doe", originalValue);

            // Act
            fluent.Path("PID.5.1").Set("UpdatedValue");

            // Assert
            Assert.Equal("UpdatedValue", fluent.Path("PID.5.1").Value);
        }

        [Fact]
        public void SetIf_WithTrueCondition_UpdatesValue()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();

            // Act
            fluent.Path("PID.5.1").SetIf("ConditionalValue", true);

            // Assert
            Assert.Equal("ConditionalValue", fluent.Path("PID.5.1").Value);
        }

        [Fact]
        public void SetIf_WithFalseCondition_DoesNotUpdateValue()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            var originalValue = fluent.Path("PID.5.1").Value;

            // Act
            fluent.Path("PID.5.1").SetIf("ConditionalValue", false);

            // Assert
            Assert.Equal(originalValue, fluent.Path("PID.5.1").Value);
        }

        [Fact]
        public void SetIf_WithTrueCondition_CreatesElement()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            Assert.False(fluent.Path("PID.40").Exists);

            // Act
            fluent.Path("PID.40").SetIf("ConditionalValue", true);

            // Assert
            Assert.True(fluent.Path("PID.40").Exists);
            Assert.Equal("ConditionalValue", fluent.Path("PID.40").Value);
        }

        [Fact]
        public void SetIf_WithFalseCondition_DoesNotCreateElement()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            Assert.False(fluent.Path("PID.41").Exists);

            // Act
            fluent.Path("PID.41").SetIf("ConditionalValue", false);

            // Assert
            Assert.False(fluent.Path("PID.41").Exists);
        }

        [Fact]
        public void MultiplePathOperations_CanBeChained()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();

            // Act
            fluent.Path("PID.5.1").Set("Smith")
                  .Path("PID.5.2").Set("Jane")
                  .Path("PID.7").Set("19900315")
                  .Path("PID.43").Set("CustomValue");

            // Assert
            Assert.Equal("Smith", fluent.Path("PID.5.1").Value);
            Assert.Equal("Jane", fluent.Path("PID.5.2").Value);
            Assert.Equal("19900315", fluent.Path("PID.7").Value);
            Assert.Equal("CustomValue", fluent.Path("PID.43").Value);
        }

        [Fact]
        public void Path_WithNullPath_ThrowsArgumentNullException()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => fluent.Path(null));
        }

        [Fact]
        public void ToString_ReturnsPathString()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            var pathAccessor = fluent.Path("PID.5.1");

            // Act
            var result = pathAccessor.ToString();

            // Assert
            Assert.Equal("PID.5.1", result);
        }

        [Fact]
        public void Path_WithComplexMessage_HandlesAllSyntaxVariations()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();

            // Act & Assert - Basic field access
            Assert.Equal("ADT^A01", fluent.Path("MSH.9").Value);
            
            // Act & Assert - Component access
            Assert.Equal("ADT", fluent.Path("MSH.9.1").Value);
            Assert.Equal("A01", fluent.Path("MSH.9.2").Value);
            
            // Act & Assert - Field with repetitions
            Assert.Equal("12345^^^MRN", fluent.Path("PID.3[1]").Value);
            Assert.Equal("67890^^^SSN", fluent.Path("PID.3[2]").Value);
            
            // Act & Assert - Component within repetition  
            Assert.Equal("12345", fluent.Path("PID.3[1].1").Value);
            Assert.Equal("67890", fluent.Path("PID.3[2].1").Value);
        }

        [Fact]
        public void SetNull_SetsHL7NullValue()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();

            // Act
            fluent.Path("PID.5.1").SetNull();

            // Assert
            Assert.True(fluent.Path("PID.5.1").IsNull);
        }

        [Fact]
        public void SetNull_CreatesHL7NullValue()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();

            // Act
            fluent.Path("PID.42").SetNull();

            // Assert
            Assert.True(fluent.Path("PID.42").Exists);
            Assert.True(fluent.Path("PID.42").IsNull);
        }

        [Fact]
        public void IsNull_WithHL7NullValue_ReturnsTrue()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            fluent.Path("PID.5.1").SetNull();

            // Act & Assert
            Assert.True(fluent.Path("PID.5.1").IsNull);
        }

        [Fact]
        public void IsNull_WithNormalValue_ReturnsFalse()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();

            // Act & Assert
            Assert.False(fluent.Path("PID.5.1").IsNull);
        }

        [Fact]
        public void IsNull_WithNonExistentPath_ReturnsFalse()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();

            // Act & Assert
            Assert.False(fluent.Path("ZZZ.999").IsNull);
        }

        #region SetEncoded Tests

        [Fact]
        public void SetEncoded_WithDelimiterCharacters_ShouldEncodeCorrectly()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            var valueWithDelimiters = "Smith|John^Middle~Name\\Test&Co";
            
            // Act
            fluent.Path("PID.5.1").SetEncoded(valueWithDelimiters);
            
            // Assert
            // GetValue() automatically decodes, so check decoded value matches
            var decodedValue = message.GetValue("PID.5.1");
            Assert.Equal(valueWithDelimiters, decodedValue);
            
            // Check that raw value is encoded
            var pidSegment = message.DefaultSegment("PID");
            var field5 = pidSegment.Fields(5);
            var component1 = field5.Components(1);
            var rawValue = component1.Value;
            
            Assert.Contains("\\F\\", rawValue); // | encoded
            Assert.Contains("\\S\\", rawValue); // ^ encoded
            Assert.Contains("\\R\\", rawValue); // ~ encoded
            Assert.Contains("\\E\\", rawValue); // \ encoded
            Assert.Contains("\\T\\", rawValue); // & encoded
        }

        [Fact]
        public void SetEncoded_WithFieldSeparator_ShouldEncodeCorrectly()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            var valueWithFieldSeparator = "Test|Field|Separator";
            
            // Act
            fluent.Path("PID.5.1").SetEncoded(valueWithFieldSeparator);
            
            // Assert
            var decodedValue = message.GetValue("PID.5.1");
            Assert.Equal(valueWithFieldSeparator, decodedValue);
            
            // Check encoding in raw value
            var pidSegment = message.DefaultSegment("PID");
            var field5 = pidSegment.Fields(5);
            var component1 = field5.Components(1);
            var rawValue = component1.Value;
            
            Assert.Contains("\\F\\", rawValue);
            Assert.DoesNotContain("|", rawValue);
        }

        [Fact]
        public void SetEncoded_WithComponentSeparator_ShouldEncodeCorrectly()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            var valueWithComponentSeparator = "Test^Component^Separator";
            
            // Act
            fluent.Path("PID.5.1").SetEncoded(valueWithComponentSeparator);
            
            // Assert
            var decodedValue = message.GetValue("PID.5.1");
            Assert.Equal(valueWithComponentSeparator, decodedValue);
            
            // Check encoding in raw value
            var pidSegment = message.DefaultSegment("PID");
            var field5 = pidSegment.Fields(5);
            var component1 = field5.Components(1);
            var rawValue = component1.Value;
            
            Assert.Contains("\\S\\", rawValue);
            Assert.DoesNotContain("^", rawValue);
        }

        [Fact]
        public void SetEncoded_WithRepetitionSeparator_ShouldEncodeCorrectly()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            var valueWithRepetitionSeparator = "Test~Repetition~Separator";
            
            // Act
            fluent.Path("PID.5.1").SetEncoded(valueWithRepetitionSeparator);
            
            // Assert
            var decodedValue = message.GetValue("PID.5.1");
            Assert.Equal(valueWithRepetitionSeparator, decodedValue);
            
            // Check encoding in raw value
            var pidSegment = message.DefaultSegment("PID");
            var field5 = pidSegment.Fields(5);
            var component1 = field5.Components(1);
            var rawValue = component1.Value;
            
            Assert.Contains("\\R\\", rawValue);
            Assert.DoesNotContain("~", rawValue);
        }

        [Fact]
        public void SetEncoded_WithEscapeCharacter_ShouldEncodeCorrectly()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            var valueWithEscapeCharacter = "Test\\Escape\\Character";
            
            // Act
            fluent.Path("PID.5.1").SetEncoded(valueWithEscapeCharacter);
            
            // Assert
            var decodedValue = message.GetValue("PID.5.1");
            Assert.Equal(valueWithEscapeCharacter, decodedValue);
            
            // Check encoding in raw value
            var pidSegment = message.DefaultSegment("PID");
            var field5 = pidSegment.Fields(5);
            var component1 = field5.Components(1);
            var rawValue = component1.Value;
            
            Assert.Contains("\\E\\", rawValue);
        }

        [Fact]
        public void SetEncoded_WithSubComponentSeparator_ShouldEncodeCorrectly()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            var valueWithSubComponentSeparator = "Test&SubComponent&Separator";
            
            // Act
            fluent.Path("PID.5.1").SetEncoded(valueWithSubComponentSeparator);
            
            // Assert
            var decodedValue = message.GetValue("PID.5.1");
            Assert.Equal(valueWithSubComponentSeparator, decodedValue);
            
            // Check encoding in raw value
            var pidSegment = message.DefaultSegment("PID");
            var field5 = pidSegment.Fields(5);
            var component1 = field5.Components(1);
            var rawValue = component1.Value;
            
            Assert.Contains("\\T\\", rawValue);
            Assert.DoesNotContain("&", rawValue);
        }

        [Fact]
        public void SetEncoded_WithNullValue_ShouldSetEmptyValue()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            
            // Act
            fluent.Path("PID.5.1").SetEncoded(null);
            
            // Assert
            var decodedValue = message.GetValue("PID.5.1");
            Assert.Equal("", decodedValue);
        }

        [Fact]
        public void SetEncoded_WithEmptyString_ShouldSetEmptyString()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            
            // Act
            fluent.Path("PID.5.1").SetEncoded("");
            
            // Assert
            var decodedValue = message.GetValue("PID.5.1");
            Assert.Equal("", decodedValue);
        }

        [Fact]
        public void SetEncoded_WithNormalText_ShouldNotChangeValue()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            var normalText = "Smith John Middle";
            
            // Act
            fluent.Path("PID.5.1").SetEncoded(normalText);
            
            // Assert
            var decodedValue = message.GetValue("PID.5.1");
            Assert.Equal(normalText, decodedValue);
        }

        [Fact]
        public void SetEncoded_ShouldReturnFluentMessageForChaining()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            
            // Act
            var result = fluent.Path("PID.5.1").SetEncoded("Test|Value");
            
            // Assert
            Assert.Same(fluent, result);
        }

        [Fact]
        public void SetEncoded_CanBeChainedWithOtherOperations()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            
            // Act
            fluent.Path("PID.5.1").SetEncoded("Smith|John^Middle")
                  .Path("PID.7").Set("19850315")
                  .Path("PID.8").Set("M");
            
            // Assert
            var nameValue = message.GetValue("PID.5.1");
            Assert.Equal("Smith|John^Middle", nameValue);
            Assert.Equal("19850315", message.GetValue("PID.7"));
            Assert.Equal("M", message.GetValue("PID.8"));
        }

        [Fact]
        public void SetEncoded_WithComplexURLLikeValue_ShouldEncodeCorrectly()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            var complexValue = "http://domain.com/resource?Action=1&ID=2|Special^Value~Test\\Path&More";
            
            // Act
            fluent.Path("PID.5.1").SetEncoded(complexValue);
            
            // Assert
            var decodedValue = message.GetValue("PID.5.1");
            Assert.Equal(complexValue, decodedValue);
            
            // Check that raw value contains encoded delimiters
            var pidSegment = message.DefaultSegment("PID");
            var field5 = pidSegment.Fields(5);
            var component1 = field5.Components(1);
            var rawValue = component1.Value;
            
            Assert.Contains("\\F\\", rawValue); // |
            Assert.Contains("\\S\\", rawValue); // ^
            Assert.Contains("\\R\\", rawValue); // ~
            Assert.Contains("\\E\\", rawValue); // \
            Assert.Contains("\\T\\", rawValue); // &
            
            // Should not contain raw delimiters
            Assert.DoesNotContain("|", rawValue);
            Assert.DoesNotContain("^", rawValue);
            Assert.DoesNotContain("~", rawValue);
            Assert.DoesNotContain("&", rawValue);
        }

        [Fact]
        public void SetEncodedIf_WithTrueCondition_ShouldSetEncodedValue()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            var valueWithDelimiters = "Test|Value^With~Delimiters";
            
            // Act
            fluent.Path("PID.5.1").SetEncodedIf(valueWithDelimiters, true);
            
            // Assert
            var decodedValue = message.GetValue("PID.5.1");
            Assert.Equal(valueWithDelimiters, decodedValue);
        }

        [Fact]
        public void SetEncodedIf_WithFalseCondition_ShouldNotSetValue()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            var originalValue = message.GetValue("PID.5.1");
            var valueWithDelimiters = "Test|Value^With~Delimiters";
            
            // Act
            fluent.Path("PID.5.1").SetEncodedIf(valueWithDelimiters, false);
            
            // Assert
            var currentValue = message.GetValue("PID.5.1");
            Assert.Equal(originalValue, currentValue);
        }

        [Fact]
        public void SetEncodedIf_ShouldReturnFluentMessageForChaining()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            
            // Act
            var result = fluent.Path("PID.5.1").SetEncodedIf("Test|Value", true);
            
            // Assert
            Assert.Same(fluent, result);
        }

        [Fact]
        public void SetEncoded_CreatesPathIfNotExists()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            var valueWithDelimiters = "New|Field^Value~With\\Delimiters&More";
            
            // Act - Set value on non-existent path
            fluent.Path("PID.99").SetEncoded(valueWithDelimiters);
            
            // Assert
            var decodedValue = message.GetValue("PID.99");
            Assert.Equal(valueWithDelimiters, decodedValue);
            
            // Verify path exists
            Assert.True(fluent.Path("PID.99").Exists);
        }

        #endregion
    }
}