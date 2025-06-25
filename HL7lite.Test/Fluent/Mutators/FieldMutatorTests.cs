using System;
using HL7lite.Fluent;
using HL7lite.Fluent.Accessors;
using HL7lite.Fluent.Mutators;
using HL7lite.Test;
using Xunit;

namespace HL7lite.Test.Fluent.Mutators
{
    public class FieldMutatorTests
    {
        private Message CreateTestMessage()
        {
            var hl7Message = @"MSH|^~\&|TESTAPP|TESTFAC|DESTAPP|DESTFAC|20240101120000|SECURITY|ADT^A01|MSG001|P|2.5
PID|1||12345||Doe^John^Middle|||M||||123 Main St^Apt 4B^Anytown^ST^12345
PV1|1|I";
            var message = new Message(hl7Message);
            message.ParseMessage();
            return message;
        }

        [Fact]
        public void Value_ShouldSetFieldValue()
        {
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);

            mutator.Value("Smith^Jane");

            Assert.Equal("Smith^Jane", message.GetValue("PID.5"));
        }

        [Fact]
        public void Value_ShouldReturnSelfForChaining()
        {
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);

            var result = mutator.Value("Test");

            Assert.Same(mutator, result);
        }

        [Fact]
        public void Value_WithNonExistentField_ShouldCreateIt()
        {
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 20);

            mutator.Value("NewValue");

            Assert.Equal("NewValue", message.GetValue("PID.20"));
        }

        [Fact]
        public void Null_ShouldSetHL7NullValue()
        {
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);

            mutator.Null();

            Assert.Equal(message.Encoding.PresentButNull, message.GetValue("PID.5"));
        }

        [Fact]
        public void Null_ShouldReturnSelfForChaining()
        {
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);

            var result = mutator.Null();

            Assert.Same(mutator, result);
        }

        [Fact]
        public void Clear_ShouldRemoveFieldValue()
        {
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);

            mutator.Clear();

            Assert.Equal("", message.GetValue("PID.5"));
        }

        [Fact]
        public void Clear_ShouldReturnSelfForChaining()
        {
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);

            var result = mutator.Clear();

            Assert.Same(mutator, result);
        }

        [Fact]
        public void Components_ShouldSetMultipleComponents()
        {
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);

            mutator.Components("Smith", "Jane", "Marie", "Dr.");

            Assert.Equal("Smith^Jane^Marie^Dr.", message.GetValue("PID.5"));
        }

        [Fact]
        public void Components_WithNullValues_ShouldHandleCorrectly()
        {
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);

            mutator.Components("Smith", null, "Marie");

            Assert.Equal("Smith^^Marie", message.GetValue("PID.5"));
        }

        [Fact]
        public void Components_WithEmptyArray_ShouldClearField()
        {
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);

            mutator.Components();

            Assert.Equal("", message.GetValue("PID.5"));
        }

        [Fact]
        public void Components_ShouldReturnSelfForChaining()
        {
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);

            var result = mutator.Components("Test");

            Assert.Same(mutator, result);
        }

        [Fact]
        public void ValueIf_WithTrueCondition_ShouldSetValue()
        {
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);

            mutator.ValueIf("NewValue", true);

            Assert.Equal("NewValue", message.GetValue("PID.5"));
        }

        [Fact]
        public void ValueIf_WithFalseCondition_ShouldNotSetValue()
        {
            var message = CreateTestMessage();
            var originalValue = message.GetValue("PID.5");
            var mutator = new FieldMutator(message, "PID", 5);

            mutator.ValueIf("NewValue", false);

            Assert.Equal(originalValue, message.GetValue("PID.5"));
        }

        [Fact]
        public void ValueIf_ShouldReturnSelfForChaining()
        {
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);

            var result = mutator.ValueIf("Test", true);

            Assert.Same(mutator, result);
        }


        [Fact]
        public void MethodChaining_ShouldWorkCorrectly()
        {
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);

            mutator
                .Clear()
                .Components("Johnson", "Robert")
                .ValueIf("Override", false);

            Assert.Equal("Johnson^Robert", message.GetValue("PID.5"));
        }

        [Fact]
        public void Value_WithInvalidSegmentCode_ShouldCreateSegment()
        {
            var message = CreateTestMessage();

            var mutator = new FieldMutator(message, "ZZZ", 1);
            mutator.Value("Test");

            Assert.Equal("Test", message.GetValue("ZZZ.1"));
        }

        [Fact]
        public void Constructor_WithNegativeFieldIndex_ShouldThrowArgumentException()
        {
            var message = CreateTestMessage();

            Assert.Throws<ArgumentException>(() => new FieldMutator(message, "PID", -1));
        }

        [Fact]
        public void Constructor_WithZeroFieldIndex_ShouldThrowArgumentException()
        {
            var message = CreateTestMessage();

            Assert.Throws<ArgumentException>(() => new FieldMutator(message, "PID", 0));
        }

        [Fact]
        public void Constructor_WithNullMessage_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FieldMutator(null, "PID", 5));
        }

        [Fact]
        public void Constructor_WithNullSegmentCode_ShouldThrowArgumentNullException()
        {
            var message = CreateTestMessage();

            Assert.Throws<ArgumentNullException>(() => new FieldMutator(message, null, 5));
        }

        [Fact]
        public void Constructor_WithEmptySegmentCode_ShouldThrowArgumentException()
        {
            var message = CreateTestMessage();

            Assert.Throws<ArgumentException>(() => new FieldMutator(message, "", 5));
        }

        [Fact]
        public void IntegrationTest_WithFluentAPI()
        {
            var message = CreateTestMessage();
            var fluent = new FluentMessage(message);
            
            // Test that mutator works correctly when accessed through fluent API
            fluent.PID[5].Set().Value("NewName^Test");
            
            Assert.Equal("NewName^Test", fluent.PID[5].Value);
            Assert.Equal("NewName^Test", message.GetValue("PID.5"));
        }

        #region Field() Method Tests

        [Fact]
        public void Field_ShouldNavigateToSpecificFieldOnSegment()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 1); // Start with field 1 mutator
            
            // Act
            mutator.Field(5).Value("Smith^John");
            
            // Assert
            Assert.Equal("Smith^John", message.GetValue("PID.5"));
        }

        [Fact]
        public void Field_ShouldReturnNewMutatorForNavigatedField()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 1);
            
            // Act
            var result = mutator.Field(5);
            
            // Assert
            Assert.NotSame(mutator, result);
            Assert.IsType<FieldMutator>(result);
        }

        [Fact]
        public void Field_ShouldAllowMultipleFieldsInChain()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 1);
            
            // Act
            mutator
                .Field(5).Value("Smith^John")
                .Field(7).Value("19850315")
                .Field(8).Value("M");
            
            // Assert
            Assert.Equal("Smith^John", message.GetValue("PID.5"));
            Assert.Equal("19850315", message.GetValue("PID.7"));
            Assert.Equal("M", message.GetValue("PID.8"));
        }

        [Fact]
        public void Field_CanNavigateAndSetMultipleFields()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 1);
            
            // Act
            mutator.Clear()
                .Components("Johnson", "Robert")
                .Field(5).Value("Smith^John")
                .Field(7).Value("19900101");
            
            // Assert  
            Assert.Equal("Johnson^Robert", message.GetValue("PID.1")); // Original field was set
            Assert.Equal("Smith^John", message.GetValue("PID.5")); // Field 5 was set
            Assert.Equal("19900101", message.GetValue("PID.7")); // Field 7 was set
        }

        [Fact]
        public void Field_WithInvalidFieldIndex_ShouldThrowArgumentException()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 1);
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => mutator.Field(0));
            Assert.Throws<ArgumentException>(() => mutator.Field(-1));
        }

        [Fact]
        public void Field_WithNullValue_ShouldSetEmptyString()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 1);
            
            // Act
            mutator.Field(5).Value(null);
            
            // Assert
            Assert.Equal("", message.GetValue("PID.5"));
        }

        [Fact]
        public void Field_IntegrationWithFluentAPI()
        {
            // Arrange
            var message = CreateTestMessage();
            var fluent = new FluentMessage(message);
            
            // Act - Test the typical use case with segment addition
            var seg = fluent.Segments("OBX").Add();
            seg[1].Set()
                .Value("1")
                .Field(2).Value("NM")
                .Field(3).Value("GLUCOSE")
                .Field(5).Value("120");
            
            // Assert
            Assert.Equal("1", fluent.Segments("OBX")[0][1].Value);
            Assert.Equal("NM", fluent.Segments("OBX")[0][2].Value);
            Assert.Equal("GLUCOSE", fluent.Segments("OBX")[0][3].Value);
            Assert.Equal("120", fluent.Segments("OBX")[0][5].Value);
        }

        #endregion

        #region EncodedValue Tests

        [Fact]
        public void EncodedValue_WithDelimiterCharacters_ShouldEncodeThemCorrectly()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);
            var valueWithDelimiters = "Smith|John^Middle~Name\\Test&Co";
            
            // Act
            mutator.EncodedValue(valueWithDelimiters);
            
            // Assert
            // GetValue() automatically decodes, so we need to check the raw field value
            var pidSegment = message.DefaultSegment("PID");
            var field5 = pidSegment.Fields(5);
            var rawValue = field5.Value;
            
            // The encoding should have escaped the delimiter characters
            Assert.Contains("\\F\\", rawValue); // | should be encoded as \F\
            Assert.Contains("\\S\\", rawValue); // ^ should be encoded as \S\
            Assert.Contains("\\R\\", rawValue); // ~ should be encoded as \R\
            Assert.Contains("\\E\\", rawValue); // \ should be encoded as \E\
            Assert.Contains("\\T\\", rawValue); // & should be encoded as \T\
            
            // Verify GetValue() returns the decoded value
            var decodedValue = message.GetValue("PID.5");
            Assert.Equal(valueWithDelimiters, decodedValue);
        }

        [Fact]
        public void EncodedValue_WithFieldSeparator_ShouldEncodeCorrectly()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);
            var valueWithFieldSeparator = "Test|Field|Separator";
            
            // Act
            mutator.EncodedValue(valueWithFieldSeparator);
            
            // Assert
            // GetValue() automatically decodes, so we need to check the raw field value
            var pidSegment = message.DefaultSegment("PID");
            var field5 = pidSegment.Fields(5);
            var rawValue = field5.Value;
            
            Assert.Contains("\\F\\", rawValue);
            Assert.DoesNotContain("|", rawValue);
            
            // Verify GetValue() returns the decoded value
            var decodedValue = message.GetValue("PID.5");
            Assert.Equal(valueWithFieldSeparator, decodedValue);
        }

        [Fact]
        public void EncodedValue_WithComponentSeparator_ShouldEncodeCorrectly()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);
            var valueWithComponentSeparator = "Test^Component^Separator";
            
            // Act
            mutator.EncodedValue(valueWithComponentSeparator);
            
            // Assert
            var pidSegment = message.DefaultSegment("PID");
            var field5 = pidSegment.Fields(5);
            var rawValue = field5.Value;
            
            Assert.Contains("\\S\\", rawValue);
            Assert.DoesNotContain("^", rawValue);
            
            // Verify GetValue() returns the decoded value
            var decodedValue = message.GetValue("PID.5");
            Assert.Equal(valueWithComponentSeparator, decodedValue);
        }

        [Fact]
        public void EncodedValue_WithRepetitionSeparator_ShouldEncodeCorrectly()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);
            var valueWithRepetitionSeparator = "Test~Repetition~Separator";
            
            // Act
            mutator.EncodedValue(valueWithRepetitionSeparator);
            
            // Assert
            var pidSegment = message.DefaultSegment("PID");
            var field5 = pidSegment.Fields(5);
            var rawValue = field5.Value;
            
            Assert.Contains("\\R\\", rawValue);
            Assert.DoesNotContain("~", rawValue);
            
            // Verify GetValue() returns the decoded value
            var decodedValue = message.GetValue("PID.5");
            Assert.Equal(valueWithRepetitionSeparator, decodedValue);
        }

        [Fact]
        public void EncodedValue_WithEscapeCharacter_ShouldEncodeCorrectly()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);
            var valueWithEscapeCharacter = "Test\\Escape\\Character";
            
            // Act
            mutator.EncodedValue(valueWithEscapeCharacter);
            
            // Assert
            var pidSegment = message.DefaultSegment("PID");
            var field5 = pidSegment.Fields(5);
            var rawValue = field5.Value;
            
            Assert.Contains("\\E\\", rawValue);
            
            // Verify GetValue() returns the decoded value
            var decodedValue = message.GetValue("PID.5");
            Assert.Equal(valueWithEscapeCharacter, decodedValue);
        }

        [Fact]
        public void EncodedValue_WithSubComponentSeparator_ShouldEncodeCorrectly()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);
            var valueWithSubComponentSeparator = "Test&SubComponent&Separator";
            
            // Act
            mutator.EncodedValue(valueWithSubComponentSeparator);
            
            // Assert
            var pidSegment = message.DefaultSegment("PID");
            var field5 = pidSegment.Fields(5);
            var rawValue = field5.Value;
            
            Assert.Contains("\\T\\", rawValue);
            Assert.DoesNotContain("&", rawValue);
            
            // Verify GetValue() returns the decoded value
            var decodedValue = message.GetValue("PID.5");
            Assert.Equal(valueWithSubComponentSeparator, decodedValue);
        }

        [Fact]
        public void EncodedValue_WithNullValue_ShouldSetEmptyValue()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);
            
            // Act
            mutator.EncodedValue(null);
            
            // Assert
            var storedValue = message.GetValue("PID.5");
            // EncodedValue(null) calls Value(null), which sets empty string per FieldMutator behavior
            Assert.Equal("", storedValue);
        }

        [Fact]
        public void EncodedValue_WithEmptyString_ShouldSetEmptyString()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);
            
            // Act
            mutator.EncodedValue("");
            
            // Assert
            var storedValue = message.GetValue("PID.5");
            Assert.Equal("", storedValue);
        }

        [Fact]
        public void EncodedValue_WithNormalText_ShouldNotChangeValue()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);
            var normalText = "Smith John Middle";
            
            // Act
            mutator.EncodedValue(normalText);
            
            // Assert
            var storedValue = message.GetValue("PID.5");
            Assert.Equal(normalText, storedValue);
        }

        [Fact]
        public void EncodedValue_ShouldReturnSelfForChaining()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);
            
            // Act
            var result = mutator.EncodedValue("Test|Value");
            
            // Assert
            Assert.Same(mutator, result);
        }

        [Fact]
        public void EncodedValue_CanBeChainedWithOtherMethods()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);
            
            // Act
            mutator
                .EncodedValue("Smith|John^Middle")
                .Field(7).Value("19850315")
                .Field(8).Value("M");
            
            // Assert
            var pidSegment = message.DefaultSegment("PID");
            var field5 = pidSegment.Fields(5);
            var rawValue = field5.Value;
            
            Assert.Contains("\\F\\", rawValue); // | encoded
            Assert.Contains("\\S\\", rawValue); // ^ encoded
            Assert.Equal("19850315", message.GetValue("PID.7"));
            Assert.Equal("M", message.GetValue("PID.8"));
        }

        [Fact]
        public void EncodedValue_WithComplexURLLikeValue_ShouldEncodeCorrectly()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);
            var complexValue = "http://domain.com/resource?Action=1&ID=2|Special^Value~Test\\Path&More";
            
            // Act
            mutator.EncodedValue(complexValue);
            
            // Assert
            var pidSegment = message.DefaultSegment("PID");
            var field5 = pidSegment.Fields(5);
            var rawValue = field5.Value;
            
            // Should contain encoded delimiters
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
            
            // Verify GetValue() returns the decoded value
            var decodedValue = message.GetValue("PID.5");
            Assert.Equal(complexValue, decodedValue);
        }

        [Fact]
        public void EncodedValue_IntegrationWithFluentAPI()
        {
            // Arrange
            var message = CreateTestMessage();
            var fluent = new FluentMessage(message);
            var valueWithDelimiters = "Test|Field^Component~Rep\\Escape&Sub";
            
            // Act - Access through fluent API
            fluent.PID[5].Set().EncodedValue(valueWithDelimiters);
            
            // Assert
            var fluentValue = fluent.PID[5].Value;
            var pidSegment = message.DefaultSegment("PID");
            var field5 = pidSegment.Fields(5);
            var rawValue = field5.Value;
            
            // The fluent API returns the raw encoded value (same as field.Value)
            Assert.Equal(rawValue, fluentValue);
            
            // Both should contain encoded delimiters
            Assert.Contains("\\F\\", fluentValue);
            Assert.Contains("\\S\\", fluentValue);
            Assert.Contains("\\R\\", fluentValue);
            Assert.Contains("\\E\\", fluentValue);
            Assert.Contains("\\T\\", fluentValue);
            
            // Verify GetValue() returns the decoded value
            var decodedValue = message.GetValue("PID.5");
            Assert.Equal(valueWithDelimiters, decodedValue);
        }

        #endregion

        #region Navigation Tests

        [Fact]
        public void Component_ShouldReturnComponentMutator()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);
            
            // Act
            var componentMutator = mutator.Component(1);
            
            // Assert
            Assert.NotNull(componentMutator);
            Assert.IsType<ComponentMutator>(componentMutator);
        }

        [Fact]
        public void Component_WithInvalidIndex_ShouldThrowArgumentException()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => mutator.Component(0));
            Assert.Throws<ArgumentException>(() => mutator.Component(-1));
        }

        [Fact]
        public void SubComponent_ShouldReturnSubComponentMutator()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);
            
            // Act
            var subComponentMutator = mutator.SubComponent(1, 1);
            
            // Assert
            Assert.NotNull(subComponentMutator);
            Assert.IsType<SubComponentMutator>(subComponentMutator);
        }

        [Fact]
        public void SubComponent_WithInvalidIndex_ShouldThrowArgumentException()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 5);
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => mutator.SubComponent(0, 1));
            Assert.Throws<ArgumentException>(() => mutator.SubComponent(1, 0));
            Assert.Throws<ArgumentException>(() => mutator.SubComponent(-1, 1));
            Assert.Throws<ArgumentException>(() => mutator.SubComponent(1, -1));
        }

        [Fact]
        public void NavigationChain_FieldToComponentToValue_ShouldWork()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 3);
            
            // Act
            mutator.Field(5).Component(1).Value("Johnson");
            
            // Assert
            Assert.Equal("Johnson", message.GetValue("PID.5.1"));
        }

        [Fact]
        public void NavigationChain_FieldToSubComponentToValue_ShouldWork()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 3);
            
            // Act
            mutator.Field(11).SubComponent(1, 1).Value("123 Main St");
            
            // Assert
            Assert.Equal("123 Main St", message.GetValue("PID.11.1"));
        }

        #endregion
    }
}