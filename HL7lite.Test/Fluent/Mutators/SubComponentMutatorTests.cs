using HL7lite.Fluent;
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
                .SubComponent(2, "Second");

            Assert.Equal("First", fluent.PID[5][1][1].Value);
            Assert.Equal("Second", fluent.PID[5][1][2].Value);
        }

        [Fact]
        public void SubComponentMutator_Component_ShouldAllowCrossLevelChaining()
        {
            var fluent = CreateTestMessage();

            fluent.PID[5][1][1].Set()
                .Value("FirstSubComp")
                .Component(2, "SecondComponent");

            Assert.Equal("FirstSubComp", fluent.PID[5][1][1].Value);
            Assert.Equal("SecondComponent", fluent.PID[5][2].Value);
        }

        [Fact]
        public void SubComponentMutator_Field_ShouldAllowCrossLevelChaining()
        {
            var fluent = CreateTestMessage();

            fluent.PID[5][1][1].Set()
                .Value("SubCompValue")
                .Field(7, "19900101");

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
                .SubComponent(2, "FirstName")
                .Component(2, "GivenName")
                .Field(7, "19851225")
                .Field(8, "M");

            Assert.Equal("LastName", fluent.PID[5][1][1].Value);
            Assert.Equal("FirstName", fluent.PID[5][1][2].Value);
            Assert.Equal("GivenName", fluent.PID[5][2].Value);
            Assert.Equal("19851225", fluent.PID[7].Value);
            Assert.Equal("M", fluent.PID[8].Value);
        }
    }
}