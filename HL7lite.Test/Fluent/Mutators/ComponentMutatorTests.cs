using System;
using HL7lite.Fluent;
using HL7lite.Fluent.Accessors;
using HL7lite.Fluent.Mutators;
using HL7lite.Test;
using Xunit;

namespace HL7lite.Test.Fluent.Mutators
{
    public class ComponentMutatorTests
    {
        private Message CreateTestMessage()
        {
            var hl7Message = @"MSH|^~\&|TESTAPP|TESTFAC|DESTAPP|DESTFAC|20240101120000|SECURITY|ADT^A01|MSG001|P|2.5
PID|1||12345||Doe^John^Middle^Dr.^III|||M||||123 Main St^Apt 4B^Anytown^ST^12345^USA^H
PV1|1|I";
            var message = new Message(hl7Message);
            message.ParseMessage();
            return message;
        }

        [Fact]
        public void Value_ShouldSetComponentValue()
        {
            var message = CreateTestMessage();
            var mutator = new ComponentMutator(message, "PID", 5, 2);

            mutator.Set("Jane");

            Assert.Equal("Jane", message.GetValue("PID.5.2"));
        }

        [Fact]
        public void Value_ShouldReturnSelfForChaining()
        {
            var message = CreateTestMessage();
            var mutator = new ComponentMutator(message, "PID", 5, 2);

            var result = mutator.Set("Test");

            Assert.Same(mutator, result);
        }

        [Fact]
        public void Value_WithNonExistentComponent_ShouldCreateIt()
        {
            var message = CreateTestMessage();
            var mutator = new ComponentMutator(message, "PID", 5, 6);

            mutator.Set("NewComponent");

            Assert.Equal("NewComponent", message.GetValue("PID.5.6"));
        }

        [Fact]
        public void Null_ShouldSetHL7NullValue()
        {
            var message = CreateTestMessage();
            var mutator = new ComponentMutator(message, "PID", 5, 2);

            mutator.SetNull();

            Assert.Equal("\"\"", message.GetValue("PID.5.2"));
        }

        [Fact]
        public void Null_ShouldReturnSelfForChaining()
        {
            var message = CreateTestMessage();
            var mutator = new ComponentMutator(message, "PID", 5, 2);

            var result = mutator.SetNull();

            Assert.Same(mutator, result);
        }

        [Fact]
        public void Clear_ShouldRemoveComponentValue()
        {
            var message = CreateTestMessage();
            var mutator = new ComponentMutator(message, "PID", 5, 2);

            mutator.Clear();

            Assert.Equal("", message.GetValue("PID.5.2"));
        }

        [Fact]
        public void Clear_ShouldReturnSelfForChaining()
        {
            var message = CreateTestMessage();
            var mutator = new ComponentMutator(message, "PID", 5, 2);

            var result = mutator.Clear();

            Assert.Same(mutator, result);
        }

        [Fact]
        public void SubComponents_ShouldSetMultipleSubComponents()
        {
            var message = CreateTestMessage();
            var mutator = new ComponentMutator(message, "PID", 11, 1);

            mutator.SetSubComponents("123 Main St", "Suite 200", "Building A");

            Assert.Equal("123 Main St&Suite 200&Building A", message.GetValue("PID.11.1"));
        }

        [Fact]
        public void SubComponents_WithNullValues_ShouldHandleCorrectly()
        {
            var message = CreateTestMessage();
            var mutator = new ComponentMutator(message, "PID", 11, 1);

            mutator.SetSubComponents("Line1", null, "Line3");

            Assert.Equal("Line1&&Line3", message.GetValue("PID.11.1"));
        }

        [Fact]
        public void SubComponents_WithEmptyArray_ShouldClearComponent()
        {
            var message = CreateTestMessage();
            var mutator = new ComponentMutator(message, "PID", 5, 2);

            mutator.SetSubComponents();

            Assert.Equal("", message.GetValue("PID.5.2"));
        }

        [Fact]
        public void SubComponents_ShouldReturnSelfForChaining()
        {
            var message = CreateTestMessage();
            var mutator = new ComponentMutator(message, "PID", 5, 2);

            var result = mutator.SetSubComponents("Test");

            Assert.Same(mutator, result);
        }

        [Fact]
        public void ValueIf_WithTrueCondition_ShouldSetValue()
        {
            var message = CreateTestMessage();
            var mutator = new ComponentMutator(message, "PID", 5, 2);

            mutator.SetIf("NewValue", true);

            Assert.Equal("NewValue", message.GetValue("PID.5.2"));
        }

        [Fact]
        public void ValueIf_WithFalseCondition_ShouldNotSetValue()
        {
            var message = CreateTestMessage();
            var originalValue = message.GetValue("PID.5.2");
            var mutator = new ComponentMutator(message, "PID", 5, 2);

            mutator.SetIf("NewValue", false);

            Assert.Equal(originalValue, message.GetValue("PID.5.2"));
        }

        [Fact]
        public void ValueIf_ShouldReturnSelfForChaining()
        {
            var message = CreateTestMessage();
            var mutator = new ComponentMutator(message, "PID", 5, 2);

            var result = mutator.SetIf("Test", true);

            Assert.Same(mutator, result);
        }

        [Fact]
        public void MethodChaining_ShouldWorkCorrectly()
        {
            var message = CreateTestMessage();
            var mutator = new ComponentMutator(message, "PID", 5, 2);

            mutator
                .Clear()
                .Set("Robert")
                .SetIf("Override", false);

            Assert.Equal("Robert", message.GetValue("PID.5.2"));
        }

        [Fact]
        public void Constructor_WithFieldRepetition_ShouldWorkCorrectly()
        {
            var message = CreateTestMessage();
            message.PutValue("PID.3[0]", "ID001");
            message.PutValue("PID.3[1]", "ID002");
            
            var mutator = new ComponentMutator(message, "PID", 3, 1, 2);
            mutator.Set("NewID");

            Assert.Equal("NewID", message.GetValue("PID.3(2).1"));
        }

        [Fact]
        public void Value_WithInvalidSegmentCode_ShouldCreateSegment()
        {
            var message = CreateTestMessage();

            var mutator = new ComponentMutator(message, "ZZZ", 1, 1);
            mutator.Set("Test");

            Assert.Equal("Test", message.GetValue("ZZZ.1.1"));
        }

        [Fact]
        public void Constructor_WithNegativeFieldIndex_ShouldThrowArgumentException()
        {
            var message = CreateTestMessage();

            Assert.Throws<ArgumentException>(() => new ComponentMutator(message, "PID", -1, 1));
        }

        [Fact]
        public void Constructor_WithZeroFieldIndex_ShouldThrowArgumentException()
        {
            var message = CreateTestMessage();

            Assert.Throws<ArgumentException>(() => new ComponentMutator(message, "PID", 0, 1));
        }

        [Fact]
        public void Constructor_WithNegativeComponentIndex_ShouldThrowArgumentException()
        {
            var message = CreateTestMessage();

            Assert.Throws<ArgumentException>(() => new ComponentMutator(message, "PID", 5, -1));
        }

        [Fact]
        public void Constructor_WithZeroComponentIndex_ShouldThrowArgumentException()
        {
            var message = CreateTestMessage();

            Assert.Throws<ArgumentException>(() => new ComponentMutator(message, "PID", 5, 0));
        }

        [Fact]
        public void Constructor_WithNullMessage_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ComponentMutator(null, "PID", 5, 2));
        }

        [Fact]
        public void Constructor_WithNullSegmentCode_ShouldThrowArgumentNullException()
        {
            var message = CreateTestMessage();

            Assert.Throws<ArgumentNullException>(() => new ComponentMutator(message, null, 5, 2));
        }

        [Fact]
        public void Constructor_WithEmptySegmentCode_ShouldThrowArgumentException()
        {
            var message = CreateTestMessage();

            Assert.Throws<ArgumentException>(() => new ComponentMutator(message, "", 5, 2));
        }

        [Fact]
        public void IntegrationTest_WithFluentAPI()
        {
            var message = CreateTestMessage();
            var fluent = new FluentMessage(message);
            
            // Test that mutator works correctly when accessed through fluent API
            fluent.PID[5][2].Set("NewFirstName");
            
            Assert.Equal("NewFirstName", fluent.PID[5][2].Raw);
            Assert.Equal("NewFirstName", message.GetValue("PID.5.2"));
        }

        [Fact]
        public void Component_ShouldSetSpecifiedComponent()
        {
            var message = CreateTestMessage();
            var mutator = new ComponentMutator(message, "PID", 5, 1);

            var result = mutator.Component(3).Set("Updated");

            Assert.Equal("Updated", message.GetValue("PID.5.3"));
            Assert.IsType<ComponentMutator>(result);
        }

        [Fact]
        public void Component_ShouldReturnComponentMutatorForChaining()
        {
            var message = CreateTestMessage();
            var mutator = new ComponentMutator(message, "PID", 5, 1);

            var result = mutator.Component(2).Set("Test")
                              .Component(3).Set("Chain");

            Assert.Equal("Test", message.GetValue("PID.5.2"));
            Assert.Equal("Chain", message.GetValue("PID.5.3"));
        }

        [Fact]
        public void Field_ShouldNavigateToSpecifiedField()
        {
            var message = CreateTestMessage();
            var mutator = new ComponentMutator(message, "PID", 5, 1);

            var result = mutator.Field(7);
            result.Set("19900101");

            Assert.Equal("19900101", message.GetValue("PID.7"));
            Assert.IsType<FieldMutator>(result);
        }

        [Fact]
        public void Field_ShouldReturnFieldMutatorForCrossLevelChaining()
        {
            var message = CreateTestMessage();
            var mutator = new ComponentMutator(message, "PID", 5, 1);

            var result = mutator.Set("Smith")
                              .Field(7).Set("19900101")
                              .Field(8).Set("F");

            Assert.Equal("Smith", message.GetValue("PID.5.1"));
            Assert.Equal("19900101", message.GetValue("PID.7"));
            Assert.Equal("F", message.GetValue("PID.8"));
        }

        [Fact]
        public void CrossLevelChaining_ComponentToFieldToComponent_ShouldWork()
        {
            var message = CreateTestMessage();
            var fluent = new FluentMessage(message);

            fluent.PID[5][1].Set("Johnson")
                .Component(2).Set("Mary")
                .Component(3).Set("Elizabeth")
                .Field(7).Set("19851225")
                .Field(8).Set("F");

            Assert.Equal("Johnson", message.GetValue("PID.5.1"));
            Assert.Equal("Mary", message.GetValue("PID.5.2"));
            Assert.Equal("Elizabeth", message.GetValue("PID.5.3"));
            Assert.Equal("19851225", message.GetValue("PID.7"));
            Assert.Equal("F", message.GetValue("PID.8"));
        }

        #region Navigation Tests

        [Fact]
        public void Component_ShouldReturnComponentMutator()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new ComponentMutator(message, "PID", 5, 1);
            
            // Act
            var componentMutator = mutator.Component(2);
            
            // Assert
            Assert.NotNull(componentMutator);
            Assert.IsType<ComponentMutator>(componentMutator);
        }

        [Fact]
        public void Component_WithInvalidIndex_ShouldThrowArgumentException()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new ComponentMutator(message, "PID", 5, 1);
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => mutator.Component(0));
            Assert.Throws<ArgumentException>(() => mutator.Component(-1));
        }

        [Fact]
        public void SubComponent_ShouldReturnSubComponentMutator()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new ComponentMutator(message, "PID", 5, 1);
            
            // Act
            var subComponentMutator = mutator.SubComponent(1);
            
            // Assert
            Assert.NotNull(subComponentMutator);
            Assert.IsType<SubComponentMutator>(subComponentMutator);
        }

        [Fact]
        public void SubComponent_WithInvalidIndex_ShouldThrowArgumentException()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new ComponentMutator(message, "PID", 5, 1);
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => mutator.SubComponent(0));
            Assert.Throws<ArgumentException>(() => mutator.SubComponent(-1));
        }

        [Fact]
        public void Field_WithInvalidIndex_ShouldThrowArgumentException()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new ComponentMutator(message, "PID", 5, 1);
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => mutator.Field(0));
            Assert.Throws<ArgumentException>(() => mutator.Field(-1));
        }

        [Fact]
        public void NavigationChain_ComponentToSubComponentToValue_ShouldWork()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new ComponentMutator(message, "PID", 5, 1);
            
            // Act
            mutator.Component(2).SubComponent(1).Set("MiddleName");
            
            // Assert
            Assert.Equal("MiddleName", message.GetValue("PID.5.2.1"));
        }

        #endregion
    }
}