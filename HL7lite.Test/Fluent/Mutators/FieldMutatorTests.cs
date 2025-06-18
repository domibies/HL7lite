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

            Assert.Equal("\"\"", message.GetValue("PID.5"));
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
        public void AddRepetition_ShouldAddNewRepetition()
        {
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 3);

            mutator.AddRepetition("67890");

            Assert.Equal("12345", message.GetValue("PID.3(1)"));
            Assert.Equal("67890", message.GetValue("PID.3(2)"));
        }

        [Fact]
        public void AddRepetition_OnEmptyField_ShouldCreateFirstRepetition()
        {
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 20);

            mutator.AddRepetition("FirstValue");

            Assert.Equal("FirstValue", message.GetValue("PID.20"));
        }

        [Fact]
        public void AddRepetition_ShouldReturnSelfForChaining()
        {
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 3);

            var result = mutator.AddRepetition("Test");

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
        public void Field_ShouldSetSpecificFieldOnSegment()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 1); // Start with field 1 mutator
            
            // Act
            mutator.Field(5, "Smith^John");
            
            // Assert
            Assert.Equal("Smith^John", message.GetValue("PID.5"));
        }

        [Fact]
        public void Field_ShouldReturnSelfForChaining()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 1);
            
            // Act
            var result = mutator.Field(5, "Test");
            
            // Assert
            Assert.Same(mutator, result);
        }

        [Fact]
        public void Field_ShouldAllowMultipleFieldsInChain()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 1);
            
            // Act
            mutator
                .Field(5, "Smith^John")
                .Field(7, "19850315")
                .Field(8, "M");
            
            // Assert
            Assert.Equal("Smith^John", message.GetValue("PID.5"));
            Assert.Equal("19850315", message.GetValue("PID.7"));
            Assert.Equal("M", message.GetValue("PID.8"));
        }

        [Fact]
        public void Field_CanBeChainedWithOtherMethods()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 1);
            
            // Act
            mutator
                .Field(5, "Smith^John")
                .Clear()
                .Components("Johnson", "Robert")
                .Field(7, "19900101");
            
            // Assert  
            Assert.Equal("Johnson^Robert", message.GetValue("PID.1")); // Clear and Components worked on original field
            Assert.Equal("19900101", message.GetValue("PID.7")); // Field method set field 7
        }

        [Fact]
        public void Field_WithInvalidFieldIndex_ShouldThrowArgumentException()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 1);
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => mutator.Field(0, "Test"));
            Assert.Throws<ArgumentException>(() => mutator.Field(-1, "Test"));
        }

        [Fact]
        public void Field_WithNullValue_ShouldSetEmptyString()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new FieldMutator(message, "PID", 1);
            
            // Act
            mutator.Field(5, null);
            
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
                .Field(2, "NM")
                .Field(3, "GLUCOSE")
                .Field(5, "120");
            
            // Assert
            Assert.Equal("1", fluent.Segments("OBX")[0][1].Value);
            Assert.Equal("NM", fluent.Segments("OBX")[0][2].Value);
            Assert.Equal("GLUCOSE", fluent.Segments("OBX")[0][3].Value);
            Assert.Equal("120", fluent.Segments("OBX")[0][5].Value);
        }

        #endregion
    }
}