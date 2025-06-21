using HL7lite.Fluent;
using HL7lite.Test;
using Xunit;

namespace HL7lite.Test.Fluent.Accessors
{
    public class ShortcutMethodsTests
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
        public void FieldAccessor_SetStringValue_ShouldSetFieldValue()
        {
            var fluent = CreateTestMessage();

            var result = fluent.PID[7].Set("19900101");

            Assert.Equal("19900101", fluent.PID[7].Value);
            Assert.Equal("19900101", fluent.UnderlyingMessage.GetValue("PID.7"));
        }

        [Fact]
        public void FieldAccessor_SetStringValue_ShouldReturnFieldMutator()
        {
            var fluent = CreateTestMessage();

            var result = fluent.PID[7].Set("19900101");

            Assert.IsType<HL7lite.Fluent.Mutators.FieldMutator>(result);
        }

        [Fact]
        public void FieldAccessor_SetStringValue_ShouldAllowChaining()
        {
            var fluent = CreateTestMessage();

            fluent.PID[7].Set("19900101")
                .Field(8, "F");

            Assert.Equal("19900101", fluent.PID[7].Value);
            Assert.Equal("F", fluent.PID[8].Value);
        }

        [Fact]
        public void FieldAccessor_SetNull_ShouldSetEmptyString()
        {
            var fluent = CreateTestMessage();

            fluent.PID[7].Set(null);

            Assert.Equal("", fluent.PID[7].Value);
        }

        [Fact]
        public void ComponentAccessor_SetStringValue_ShouldSetComponentValue()
        {
            var fluent = CreateTestMessage();

            var result = fluent.PID[5][2].Set("Jane");

            Assert.Equal("Jane", fluent.PID[5][2].Value);
            Assert.Equal("Jane", fluent.UnderlyingMessage.GetValue("PID.5.2"));
        }

        [Fact]
        public void ComponentAccessor_SetStringValue_ShouldReturnComponentMutator()
        {
            var fluent = CreateTestMessage();

            var result = fluent.PID[5][2].Set("Jane");

            Assert.IsType<HL7lite.Fluent.Mutators.ComponentMutator>(result);
        }

        [Fact]
        public void ComponentAccessor_SetStringValue_ShouldAllowChaining()
        {
            var fluent = CreateTestMessage();

            fluent.PID[5][2].Set("Jane")
                .Component(3, "Marie")
                .Field(7, "19900101");

            Assert.Equal("Jane", fluent.PID[5][2].Value);
            Assert.Equal("Marie", fluent.PID[5][3].Value);
            Assert.Equal("19900101", fluent.PID[7].Value);
        }

        [Fact]
        public void ComponentAccessor_SetNull_ShouldSetEmptyString()
        {
            var fluent = CreateTestMessage();

            fluent.PID[5][2].Set(null);

            Assert.Equal("", fluent.PID[5][2].Value);
        }

        [Fact]
        public void ShortcutVsVerbose_ShouldProduceSameResult()
        {
            var fluent1 = CreateTestMessage();
            var fluent2 = CreateTestMessage();

            // Using shortcuts
            fluent1.PID[3].Set("98765");
            fluent1.PID[5][1].Set("Smith");

            // Using verbose syntax
            fluent2.PID[3].Set().Value("98765");
            fluent2.PID[5][1].Set().Value("Smith");

            Assert.Equal(fluent1.PID[3].Value, fluent2.PID[3].Value);
            Assert.Equal(fluent1.PID[5][1].Value, fluent2.PID[5][1].Value);
        }

        [Fact]
        public void SubComponentAccessor_SetStringValue_ShouldSetSubComponentValue()
        {
            var fluent = CreateTestMessage();

            var result = fluent.PID[5][1][1].Set("LastPart");

            Assert.Equal("LastPart", fluent.PID[5][1][1].Value);
            Assert.Equal("LastPart", fluent.UnderlyingMessage.GetValue("PID.5.1.1"));
        }

        [Fact]
        public void SubComponentAccessor_SetStringValue_ShouldReturnSubComponentMutator()
        {
            var fluent = CreateTestMessage();

            var result = fluent.PID[5][1][1].Set("LastPart");

            Assert.IsType<HL7lite.Fluent.Mutators.SubComponentMutator>(result);
        }

        [Fact]
        public void SubComponentAccessor_SetStringValue_ShouldAllowChaining()
        {
            var fluent = CreateTestMessage();

            fluent.PID[5][1][1].Set("FirstSub")
                .SubComponent(2, "SecondSub")
                .Component(2, "SecondComp")
                .Field(7, "19900101");

            Assert.Equal("FirstSub", fluent.PID[5][1][1].Value);
            Assert.Equal("SecondSub", fluent.PID[5][1][2].Value);
            Assert.Equal("SecondComp", fluent.PID[5][2].Value);
            Assert.Equal("19900101", fluent.PID[7].Value);
        }

        [Fact]
        public void SubComponentAccessor_SetNull_ShouldSetEmptyString()
        {
            var fluent = CreateTestMessage();

            fluent.PID[5][1][1].Set(null);

            Assert.Equal("", fluent.PID[5][1][1].Value);
        }

        [Fact]
        public void MessageBuilding_WithShortcuts_ShouldBeConcise()
        {
            var fluent = CreateTestMessage();

            // Demonstrate concise message building
            fluent.PID[3].Set("98765")
                .Field(5, "Johnson^Mary^Elizabeth")
                .Field(7, "19851225")
                .Field(8, "F");

            fluent.PID[5][1].Set("Johnson")
                .Component(2, "Mary")
                .Component(3, "Elizabeth")
                .Field(11, "456 Oak Ave^Boston^MA^02101");

            Assert.Equal("98765", fluent.PID[3].Value);
            Assert.Equal("Johnson^Mary^Elizabeth", fluent.PID[5].Value);
            Assert.Equal("19851225", fluent.PID[7].Value);
            Assert.Equal("F", fluent.PID[8].Value);
            Assert.Equal("Johnson", fluent.PID[5][1].Value);
            Assert.Equal("Mary", fluent.PID[5][2].Value);
            Assert.Equal("Elizabeth", fluent.PID[5][3].Value);
            Assert.Equal("456 Oak Ave^Boston^MA^02101", fluent.PID[11].Value);
        }

        [Fact]
        public void ShortcutVsVerbose_SubComponents_ShouldProduceSameResult()
        {
            var fluent1 = CreateTestMessage();
            var fluent2 = CreateTestMessage();

            // Using shortcuts
            fluent1.PID[5][1][1].Set("Doe");
            fluent1.PID[5][1][2].Set("Jr");

            // Using verbose syntax
            fluent2.PID[5][1][1].Set().Value("Doe");
            fluent2.PID[5][1][2].Set().Value("Jr");

            Assert.Equal(fluent1.PID[5][1][1].Value, fluent2.PID[5][1][1].Value);
            Assert.Equal(fluent1.PID[5][1][2].Value, fluent2.PID[5][1][2].Value);
        }

        [Fact]
        public void CompleteHierarchy_WithShortcuts_ShouldWorkSeamlessly()
        {
            var fluent = CreateTestMessage();

            // Test complete hierarchy: Field -> Component -> SubComponent shortcuts
            fluent.PID[5].Set("Smith^John^M^Dr^III")
                .Field(7, "19900101")
                .Field(8, "M");

            fluent.PID[5][1].Set("Johnson")
                .Component(2, "Jane")
                .Field(11, "123 Main St");

            fluent.PID[5][1][1].Set("NewLastName")
                .SubComponent(2, "Suffix")
                .Component(3, "MiddleName")
                .Field(13, "555-1234");

            // Check individual subcomponents
            Assert.Equal("NewLastName", fluent.PID[5][1][1].Value);
            Assert.Equal("Suffix", fluent.PID[5][1][2].Value);
            Assert.Equal("Jane", fluent.PID[5][2].Value);
            Assert.Equal("MiddleName", fluent.PID[5][3].Value);
            Assert.Equal("19900101", fluent.PID[7].Value);
            Assert.Equal("M", fluent.PID[8].Value);
            Assert.Equal("123 Main St", fluent.PID[11].Value);
            Assert.Equal("555-1234", fluent.PID[13].Value);
        }
    }
}