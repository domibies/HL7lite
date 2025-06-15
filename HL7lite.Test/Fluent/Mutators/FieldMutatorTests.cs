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
    }
}