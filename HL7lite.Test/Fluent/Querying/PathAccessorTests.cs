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

        #region Segment Creation Tests

        [Fact]
        public void Set_WithMissingSegment_CreatesSegment()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            
            // Verify segment doesn't exist initially
            Assert.False(fluent.Path("ZZ1.1").Exists);
            Assert.False(fluent["ZZ1"].Exists);
            
            // Act - This would normally throw "Segment name not available" 
            var result = fluent.Path("ZZ1.1").Set("TestValue");
            
            // Assert - result should be the fluent message
            Assert.Same(fluent, result);
            
            // Check if the value was set
            Assert.True(fluent.Path("ZZ1.1").Exists);
            Assert.Equal("TestValue", fluent.Path("ZZ1.1").Value);
            Assert.True(fluent["ZZ1"].Exists);
        }

        [Fact]
        public void Set_WithMissingSegmentAndComplexPath_CreatesSegmentAndPath()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            
            // Verify segment doesn't exist initially
            Assert.False(fluent.Path("ZZ2.99.5").Exists);
            Assert.False(fluent["ZZ2"].Exists);
            
            // Act - This would normally throw "Segment name not available"
            fluent.Path("ZZ2.99.5").Set("ComplexValue");
            
            // Assert
            Assert.True(fluent.Path("ZZ2.99.5").Exists);
            Assert.Equal("ComplexValue", fluent.Path("ZZ2.99.5").Value);
            Assert.True(fluent["ZZ2"].Exists);
        }

        [Fact]
        public void Set_WithMissingSegmentRepetition_CreatesSegmentRepetition()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            
            // Verify segment doesn't exist initially
            Assert.False(fluent.Path("DG1[3].1").Exists);
            
            // Act - This would normally throw "Segment name not available" for repetition 3
            fluent.Path("DG1[3].1").Set("ThirdDiagnosis");
            
            // Assert
            Assert.True(fluent.Path("DG1[3].1").Exists);
            Assert.Equal("ThirdDiagnosis", fluent.Path("DG1[3].1").Value);
            Assert.True(fluent.Segments("DG1").Count >= 3);
        }

        [Fact]
        public void Set_WithMissingSegmentAndRepetition_CreatesSegmentRepetitions()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            
            // Verify segment doesn't exist initially
            Assert.False(fluent.Path("ZZ3[2].5.1").Exists);
            Assert.False(fluent["ZZ3"].Exists);
            
            // Act - This would normally throw "Segment name not available"
            fluent.Path("ZZ3[2].5.1").Set("SecondRepetitionValue");
            
            // Assert
            Assert.True(fluent.Path("ZZ3[2].5.1").Exists);
            Assert.Equal("SecondRepetitionValue", fluent.Path("ZZ3[2].5.1").Value);
            Assert.True(fluent["ZZ3"].Exists);
            Assert.Equal(2, fluent.Segments("ZZ3").Count); // Should create 2 segments (repetitions 1 and 2)
            
            // First repetition should exist but be empty
            Assert.True(fluent.Path("ZZ3[1].1").Exists);
            Assert.Equal("", fluent.Path("ZZ3[1].1").Value);
        }

        [Fact]
        public void SetIf_WithMissingSegment_CreatesSegmentWhenConditionTrue()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            
            // Verify segment doesn't exist initially
            Assert.False(fluent.Path("ZZ4.10").Exists);
            Assert.False(fluent["ZZ4"].Exists);
            
            // Act - This would normally throw "Segment name not available"
            fluent.Path("ZZ4.10").SetIf("ConditionalValue", true);
            
            // Assert
            Assert.True(fluent.Path("ZZ4.10").Exists);
            Assert.Equal("ConditionalValue", fluent.Path("ZZ4.10").Value);
            Assert.True(fluent["ZZ4"].Exists);
        }

        [Fact]
        public void SetIf_WithMissingSegment_DoesNotCreateSegmentWhenConditionFalse()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            
            // Verify segment doesn't exist initially
            Assert.False(fluent.Path("ZZ5.10").Exists);
            Assert.False(fluent["ZZ5"].Exists);
            
            // Act 
            fluent.Path("ZZ5.10").SetIf("ConditionalValue", false);
            
            // Assert - Segment should not be created
            Assert.False(fluent.Path("ZZ5.10").Exists);
            Assert.False(fluent["ZZ5"].Exists);
        }

        [Fact]
        public void SetEncoded_WithMissingSegment_CreatesSegmentAndEncodesValue()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            var valueWithDelimiters = "Test|Value^With~Delimiters&More";
            
            // Verify segment doesn't exist initially
            Assert.False(fluent.Path("ZZ6.1").Exists);
            Assert.False(fluent["ZZ6"].Exists);
            
            // Act - This would normally throw "Segment name not available"
            fluent.Path("ZZ6.1").SetEncoded(valueWithDelimiters);
            
            // Assert
            Assert.True(fluent.Path("ZZ6.1").Exists);
            Assert.Equal(valueWithDelimiters, fluent.Path("ZZ6.1").Value);
            Assert.True(fluent["ZZ6"].Exists);
            
            // Verify encoding worked
            var decodedValue = message.GetValue("ZZ6.1");
            Assert.Equal(valueWithDelimiters, decodedValue);
        }

        [Fact]
        public void SetNull_WithMissingSegment_CreatesSegmentAndSetsNull()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            
            // Verify segment doesn't exist initially
            Assert.False(fluent.Path("ZZ7.2").Exists);
            Assert.False(fluent["ZZ7"].Exists);
            
            // Act - This would normally throw "Segment name not available"
            fluent.Path("ZZ7.2").SetNull();
            
            // Assert
            Assert.True(fluent.Path("ZZ7.2").Exists);
            Assert.True(fluent.Path("ZZ7.2").IsNull);
            Assert.True(fluent["ZZ7"].Exists);
        }

        [Fact]
        public void Set_WithMultipleMissingSegments_CreatesAllSegments()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            
            // Verify segments don't exist initially
            Assert.False(fluent.Path("AB1.1").Exists);
            Assert.False(fluent.Path("CD2.5").Exists);
            Assert.False(fluent.Path("EF3.10.2").Exists);
            
            // Act - Chain multiple operations that would normally throw
            fluent.Path("AB1.1").Set("FirstValue")
                  .Path("CD2.5").Set("SecondValue")
                  .Path("EF3.10.2").Set("ThirdValue");
            
            // Assert
            Assert.True(fluent.Path("AB1.1").Exists);
            Assert.Equal("FirstValue", fluent.Path("AB1.1").Value);
            Assert.True(fluent["AB1"].Exists);
            
            Assert.True(fluent.Path("CD2.5").Exists);
            Assert.Equal("SecondValue", fluent.Path("CD2.5").Value);
            Assert.True(fluent["CD2"].Exists);
            
            Assert.True(fluent.Path("EF3.10.2").Exists);
            Assert.Equal("ThirdValue", fluent.Path("EF3.10.2").Value);
            Assert.True(fluent["EF3"].Exists);
        }

        [Fact]
        public void Set_WithInvalidSegmentName_HandlesGracefully()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            
            // Act - Use invalid segment names (should not crash)
            fluent.Path("1234.1").Set("TestValue");  // Invalid: starts with number
            fluent.Path("A.1").Set("TestValue");     // Invalid: too short
            fluent.Path("ABCDE.1").Set("TestValue"); // Invalid: too long
            
            // Assert - Should not crash, should handle gracefully (consistent with fluent API)
            // The specific behavior depends on path parsing, but should not throw exceptions
            Assert.True(true); // Test passes if no exception is thrown
        }

        [Fact]
        public void Set_WithExistingSegmentButMissingField_WorksNormally()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            
            // Act - Set field that doesn't exist in existing segment (normal PutValue behavior)
            fluent.Path("PID.99").Set("NewFieldValue");
            
            // Assert
            Assert.True(fluent.Path("PID.99").Exists);
            Assert.Equal("NewFieldValue", fluent.Path("PID.99").Value);
        }

        [Fact]
        public void PathParsing_WithVariousFormats_ParsesCorrectly()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            
            // Act & Assert - Test various path formats for segment creation
            
            // Simple segment.field
            fluent.Path("TST.1").Set("Test1");
            Assert.Equal("Test1", fluent.Path("TST.1").Value);
            
            // Segment with repetition
            fluent.Path("TST[2].1").Set("Test2");
            Assert.Equal("Test2", fluent.Path("TST[2].1").Value);
            
            // Complex path with component
            fluent.Path("TST[3].5.2").Set("Test3");
            Assert.Equal("Test3", fluent.Path("TST[3].5.2").Value);
            
            // Verify segment repetitions were created correctly
            Assert.True(fluent["TST"].Exists);
            Assert.Equal(3, fluent.Segments("TST").Count);
        }

        #endregion

        #region Segment and Field Repetition Tests (Fix for Endless Loop)

        [Fact]
        public void Set_WithSegmentAndFieldRepetitions_ShouldNotCauseEndlessLoop()
        {
            // Arrange - This test verifies the fix for the endless loop issue
            var (fluent, message) = CreateTestMessagePair();
            
            // Act - This used to cause an endless loop before the fix
            fluent.Path("ZZ2[2].99[2].99").Set("Value2b");
            
            // Assert
            Assert.True(fluent.Path("ZZ2[2].99[2].99").Exists);
            Assert.Equal("Value2b", fluent.Path("ZZ2[2].99[2].99").Value);
            
            // Verify the structure was created correctly
            var segments = fluent.Segments("ZZ2");
            Assert.Equal(2, segments.Count);
            
            // Check second segment, field 99, second repetition, component 99
            var field = segments[1][99];
            Assert.True(field.HasRepetitions);
            Assert.Equal(2, field.RepetitionCount);
            
            var secondRepetition = field.Repetition(2);
            Assert.Equal("Value2b", secondRepetition[99].Value);
        }

        [Fact]
        public void Set_MultipleSegmentRepetitionsWithFieldRepetitions_ShouldWorkCorrectly()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            
            // Act - Set various combinations of segment and field repetitions
            fluent.Path("DG1[1].3[1].1").Set("Diagnosis1a");
            fluent.Path("DG1[1].3[2].1").Set("Diagnosis1b");
            fluent.Path("DG1[2].3[1].1").Set("Diagnosis2a");
            fluent.Path("DG1[2].3[2].1").Set("Diagnosis2b");
            fluent.Path("DG1[3].3[1].1").Set("Diagnosis3a");
            
            // Assert
            Assert.Equal("Diagnosis1a", fluent.Path("DG1[1].3[1].1").Value);
            Assert.Equal("Diagnosis1b", fluent.Path("DG1[1].3[2].1").Value);
            Assert.Equal("Diagnosis2a", fluent.Path("DG1[2].3[1].1").Value);
            Assert.Equal("Diagnosis2b", fluent.Path("DG1[2].3[2].1").Value);
            Assert.Equal("Diagnosis3a", fluent.Path("DG1[3].3[1].1").Value);
            
            // Verify structure
            var dg1Segments = fluent.Segments("DG1");
            Assert.Equal(3, dg1Segments.Count);
            
            // First DG1 segment should have 2 repetitions of field 3
            Assert.Equal(2, dg1Segments[0][3].RepetitionCount);
            Assert.Equal(2, dg1Segments[1][3].RepetitionCount);
            Assert.Equal(1, dg1Segments[2][3].RepetitionCount);
        }

        [Fact]
        public void Set_HighFieldNumberWithRepetitionsOnNonFirstSegment_ShouldWork()
        {
            // Arrange - Test with high field numbers that require field creation
            var (fluent, message) = CreateTestMessagePair();
            
            // Act
            fluent.Path("ZZ1[3].50[3].5").Set("HighFieldValue");
            
            // Assert
            Assert.Equal("HighFieldValue", fluent.Path("ZZ1[3].50[3].5").Value);
            
            // Verify the segment was created
            var segments = fluent.Segments("ZZ1");
            Assert.Equal(3, segments.Count);
            
            // Verify field 50 exists with 3 repetitions
            var field50 = segments[2][50];
            Assert.True(field50.Exists);
            Assert.Equal(3, field50.RepetitionCount);
            
            // Verify the value is in the correct location
            Assert.Equal("HighFieldValue", field50.Repetition(3)[5].Value);
        }

        [Fact]
        public void SetEncoded_WithSegmentAndFieldRepetitions_ShouldHandleDelimiters()
        {
            // Arrange
            var (fluent, message) = CreateTestMessagePair();
            
            // Act - Set encoded value with delimiters on non-first segment with field repetitions
            fluent.Path("OBX[2].5[2].1").SetEncoded("Result|With^Delimiters~And&More");
            
            // Assert - Value should be properly encoded
            var retrievedValue = fluent.Path("OBX[2].5[2].1").Value;
            Assert.Equal("Result|With^Delimiters~And&More", retrievedValue);
            
            // Verify the raw stored value has encoded delimiters
            var segments = fluent.Segments("OBX");
            Assert.Equal(2, segments.Count);
            var field = segments[1][5];
            Assert.Equal(2, field.RepetitionCount);
        }

        [Fact]
        public void Set_CreatingMultipleSegmentInstancesInReverseOrder_ShouldWork()
        {
            // Arrange - Test creating segments in non-sequential order
            var (fluent, message) = CreateTestMessagePair();
            
            // Act - Create segments 3, 1, 2 in that order
            fluent.Path("AL1[3].3.1").Set("Allergy3");
            fluent.Path("AL1[1].3.1").Set("Allergy1");
            fluent.Path("AL1[2].3.1").Set("Allergy2");
            
            // Assert - All should be accessible in correct order
            Assert.Equal("Allergy1", fluent.Path("AL1[1].3.1").Value);
            Assert.Equal("Allergy2", fluent.Path("AL1[2].3.1").Value);
            Assert.Equal("Allergy3", fluent.Path("AL1[3].3.1").Value);
            
            // Verify segment order
            var segments = fluent.Segments("AL1");
            Assert.Equal(3, segments.Count);
            Assert.Equal("Allergy1", segments[0][3][1].Value);
            Assert.Equal("Allergy2", segments[1][3][1].Value);
            Assert.Equal("Allergy3", segments[2][3][1].Value);
        }

        #endregion
    }
}