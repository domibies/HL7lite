using System;
using HL7lite.Fluent;
using HL7lite.Fluent.Mutators;
using HL7lite.Test;
using Xunit;

namespace HL7lite.Test.Fluent.Mutators
{
    public class SubComponentMutatorTests
    {
        private FluentMessage CreateTestMessage()
        {
            var hl7Message = @"MSH|^~\&|TESTAPP|TESTFAC|DESTAPP|DESTFAC|20240101120000|SECURITY|ADT^A01|MSG001|P|2.5
PID|1||12345||Doe^John^Middle^Dr.^III|||M||||123 Main St^Apt 4B^Anytown^ST^12345^USA^H
PV1|1|I";
            var message = new Message(hl7Message);
            message.ParseMessage();
            return new FluentMessage(message);
        }

        [Fact]
        public void SubComponentMutator_Value_ShouldSetSubComponentValue()
        {
            var fluent = CreateTestMessage();

            fluent.PID[5][1][1].Set().Value("Johnson");

            Assert.Equal("Johnson", fluent.PID[5][1][1].Value);
            Assert.Equal("Johnson", fluent.UnderlyingMessage.GetValue("PID.5.1.1"));
        }

        [Fact]
        public void SubComponentMutator_Value_WithNullValue_ShouldSetEmptyString()
        {
            var fluent = CreateTestMessage();

            fluent.PID[5][1][1].Set().Value(null);

            Assert.Equal("", fluent.PID[5][1][1].Value);
        }

        [Fact]
        public void SubComponentMutator_EncodedValue_ShouldEncodeDelimiters()
        {
            var fluent = CreateTestMessage();

            fluent.PID[5][1][1].Set().EncodedValue("Test|With^Delimiters");

            var expectedEncoded = fluent.UnderlyingMessage.Encoding.Encode("Test|With^Delimiters");
            Assert.Equal(expectedEncoded, fluent.PID[5][1][1].Value);
        }

        [Fact]
        public void SubComponentMutator_EncodedValue_WithNull_ShouldSetEmptyString()
        {
            var fluent = CreateTestMessage();

            fluent.PID[5][1][1].Set().EncodedValue(null);

            Assert.Equal("", fluent.PID[5][1][1].Value);
        }

        [Fact]
        public void SubComponentMutator_Null_ShouldSetHL7Null()
        {
            var fluent = CreateTestMessage();

            fluent.PID[5][1][1].Set().Null();

            var expectedNull = fluent.UnderlyingMessage.Encoding.PresentButNull;
            Assert.Equal(expectedNull, fluent.UnderlyingMessage.GetValue("PID.5.1.1"));
            Assert.True(fluent.PID[5][1][1].IsNull);
        }

        [Fact]
        public void SubComponentMutator_Clear_ShouldSetEmptyString()
        {
            var fluent = CreateTestMessage();

            fluent.PID[5][1][1].Set().Value("SomeValue");
            fluent.PID[5][1][1].Set().Clear();

            Assert.Equal("", fluent.PID[5][1][1].Value);
        }

        [Fact]
        public void SubComponentMutator_ValueIf_WithTrueCondition_ShouldSetValue()
        {
            var fluent = CreateTestMessage();

            fluent.PID[5][1][1].Set().ValueIf("ConditionalValue", true);

            Assert.Equal("ConditionalValue", fluent.PID[5][1][1].Value);
        }

        [Fact]
        public void SubComponentMutator_ValueIf_WithFalseCondition_ShouldNotSetValue()
        {
            var fluent = CreateTestMessage();
            var originalValue = fluent.PID[5][1][1].Value;

            fluent.PID[5][1][1].Set().ValueIf("ConditionalValue", false);

            Assert.Equal(originalValue, fluent.PID[5][1][1].Value);
        }

        [Fact]
        public void SubComponentMutator_SubComponent_ShouldAllowSameComponentChaining()
        {
            var fluent = CreateTestMessage();

            fluent.PID[5][1][1].Set()
                .Value("First")
                .SubComponent(2).Value("Second");

            Assert.Equal("First", fluent.PID[5][1][1].Value);
            Assert.Equal("Second", fluent.PID[5][1][2].Value);
        }

        [Fact]
        public void SubComponentMutator_Component_ShouldAllowCrossLevelChaining()
        {
            var fluent = CreateTestMessage();

            fluent.PID[5][1][1].Set()
                .Value("FirstSubComp")
                .Component(2).Value("SecondComponent");

            Assert.Equal("FirstSubComp", fluent.PID[5][1][1].Value);
            Assert.Equal("SecondComponent", fluent.PID[5][2].Value);
        }

        [Fact]
        public void SubComponentMutator_Field_ShouldAllowCrossLevelChaining()
        {
            var fluent = CreateTestMessage();

            fluent.PID[5][1][1].Set()
                .Value("SubCompValue")
                .Field(7).Value("19900101");

            Assert.Equal("SubCompValue", fluent.PID[5][1][1].Value);
            Assert.Equal("19900101", fluent.PID[7].Value);
        }

        [Fact]
        public void SubComponentMutator_MethodChaining_ShouldWorkCorrectly()
        {
            var fluent = CreateTestMessage();

            var result = fluent.PID[5][1][1].Set()
                .Value("Test")
                .Clear()
                .Value("Final");

            Assert.Equal("Final", fluent.PID[5][1][1].Value);
            Assert.IsType<HL7lite.Fluent.Mutators.SubComponentMutator>(result);
        }

        [Fact]
        public void SubComponentMutator_WithNonExistentSegment_ShouldCreateSegment()
        {
            var fluent = CreateTestMessage();

            fluent.OBX[1][1][1].Set().Value("NewSegmentValue");

            Assert.Equal("NewSegmentValue", fluent.OBX[1][1][1].Value);
            Assert.Equal("NewSegmentValue", fluent.UnderlyingMessage.GetValue("OBX.1.1.1"));
        }

        [Fact]
        public void SubComponentMutator_WithRepetitions_ShouldHandleCorrectly()
        {
            var fluent = CreateTestMessage();

            // Test with repetition index on field level
            fluent.PID[3].Repetition(1)[1][1].Set().Value("FirstRep");

            Assert.Equal("FirstRep", fluent.PID[3].Repetition(1)[1][1].Value);
        }

        [Fact]
        public void SubComponentMutator_ComplexChaining_ShouldWorkCorrectly()
        {
            var fluent = CreateTestMessage();

            fluent.PID[5][1][1].Set()
                .Value("LastName")
                .SubComponent(2).Value("FirstName")
                .Component(2).Value("GivenName")
                .Field(7).Value("19851225")
                .Field(8).Value("M");

            Assert.Equal("LastName", fluent.PID[5][1][1].Value);
            Assert.Equal("FirstName", fluent.PID[5][1][2].Value);
            Assert.Equal("GivenName", fluent.PID[5][2].Value);
            Assert.Equal("19851225", fluent.PID[7].Value);
            Assert.Equal("M", fluent.PID[8].Value);
        }

        #region Navigation Tests

        [Fact]
        public void Field_ShouldReturnFieldMutator()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new SubComponentMutator(message.UnderlyingMessage, "PID", 5, 1, 1);
            
            // Act
            var fieldMutator = mutator.Field(7);
            
            // Assert
            Assert.NotNull(fieldMutator);
            Assert.IsType<FieldMutator>(fieldMutator);
        }

        [Fact]
        public void Field_WithInvalidIndex_ShouldThrowArgumentException()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new SubComponentMutator(message.UnderlyingMessage, "PID", 5, 1, 1);
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => mutator.Field(0));
            Assert.Throws<ArgumentException>(() => mutator.Field(-1));
        }

        [Fact]
        public void Component_ShouldReturnComponentMutator()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new SubComponentMutator(message.UnderlyingMessage, "PID", 5, 1, 1);
            
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
            var mutator = new SubComponentMutator(message.UnderlyingMessage, "PID", 5, 1, 1);
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => mutator.Component(0));
            Assert.Throws<ArgumentException>(() => mutator.Component(-1));
        }

        [Fact]
        public void SubComponent_ShouldReturnSubComponentMutator()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new SubComponentMutator(message.UnderlyingMessage, "PID", 5, 1, 1);
            
            // Act
            var subComponentMutator = mutator.SubComponent(2);
            
            // Assert
            Assert.NotNull(subComponentMutator);
            Assert.IsType<SubComponentMutator>(subComponentMutator);
        }

        [Fact]
        public void SubComponent_WithInvalidIndex_ShouldThrowArgumentException()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new SubComponentMutator(message.UnderlyingMessage, "PID", 5, 1, 1);
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => mutator.SubComponent(0));
            Assert.Throws<ArgumentException>(() => mutator.SubComponent(-1));
        }

        [Fact]
        public void NavigationChain_SubComponentToFieldToComponent_ShouldWork()
        {
            // Arrange
            var message = CreateTestMessage();
            var mutator = new SubComponentMutator(message.UnderlyingMessage, "PID", 5, 1, 1);
            
            // Act
            mutator.SubComponent(2).Value("Jr")
                .Field(7).Value("19851225")
                .Field(8).Value("M");
            
            // Assert
            Assert.Equal("Jr", message.UnderlyingMessage.GetValue("PID.5.1.2"));
            Assert.Equal("19851225", message.UnderlyingMessage.GetValue("PID.7"));
            Assert.Equal("M", message.UnderlyingMessage.GetValue("PID.8"));
        }

        #endregion
    }
}