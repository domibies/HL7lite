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

            Assert.Equal("19900101", fluent.PID[7].Raw);
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
                .Field(8).Set("F");

            Assert.Equal("19900101", fluent.PID[7].Raw);
            Assert.Equal("F", fluent.PID[8].Raw);
        }

        [Fact]
        public void FieldAccessor_SetNull_ShouldSetEmptyString()
        {
            var fluent = CreateTestMessage();

            fluent.PID[7].Set(null);

            Assert.Equal("", fluent.PID[7].Raw);
        }

        [Fact]
        public void ComponentAccessor_SetStringValue_ShouldSetComponentValue()
        {
            var fluent = CreateTestMessage();

            var result = fluent.PID[5][2].Set("Jane");

            Assert.Equal("Jane", fluent.PID[5][2].Raw);
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
                .Component(3).Set("Marie")
                .Field(7).Set("19900101");

            Assert.Equal("Jane", fluent.PID[5][2].Raw);
            Assert.Equal("Marie", fluent.PID[5][3].Raw);
            Assert.Equal("19900101", fluent.PID[7].Raw);
        }

        [Fact]
        public void ComponentAccessor_SetNull_ShouldSetEmptyString()
        {
            var fluent = CreateTestMessage();

            fluent.PID[5][2].Set(null);

            Assert.Equal("", fluent.PID[5][2].Raw);
        }

        [Fact]
        public void ShortcutVsDirect_ShouldProduceSameResult()
        {
            var fluent1 = CreateTestMessage();
            var fluent2 = CreateTestMessage();

            // Using shortcuts
            fluent1.PID[3].Set("98765");
            fluent1.PID[5][1].Set("Smith");

            // Using direct Set() method
            fluent2.PID[3].Set("98765");
            fluent2.PID[5][1].Set("Smith");

            Assert.Equal(fluent1.PID[3].Raw, fluent2.PID[3].Raw);
            Assert.Equal(fluent1.PID[5][1].Raw, fluent2.PID[5][1].Raw);
        }

        [Fact]
        public void SubComponentAccessor_SetStringValue_ShouldSetSubComponentValue()
        {
            var fluent = CreateTestMessage();

            var result = fluent.PID[5][1][1].Set("LastPart");

            Assert.Equal("LastPart", fluent.PID[5][1][1].Raw);
            Assert.Equal("LastPart", fluent.UnderlyingMessage.GetValue("PID.5.1.1"));
        }

        [Fact]
        public void Shortcuts_ShouldBeEquivalentToVerboseForm()
        {
            // Test that shortcuts produce identical results to verbose .Set().SetXxx() patterns
            var fluent1 = CreateTestMessage();
            var fluent2 = CreateTestMessage();

            // Test SetComponents shortcut vs verbose
            fluent1.PID[5].SetComponents("Smith", "John", "M");  // Shortcut
            fluent2.PID[5].Set().SetComponents("Smith", "John", "M");  // Verbose

            Assert.Equal(fluent1.PID[5].Raw, fluent2.PID[5].Raw);

            // Test SetNull shortcut vs verbose  
            fluent1.PID[6].SetNull();  // Shortcut
            fluent2.PID[6].Set().SetNull();  // Verbose

            Assert.Equal(fluent1.PID[6].Raw, fluent2.PID[6].Raw);

            // Test SetIf shortcut vs verbose
            fluent1.PID[8].SetIf("M", true);  // Shortcut
            fluent2.PID[8].Set().SetIf("M", true);  // Verbose

            Assert.Equal(fluent1.PID[8].Raw, fluent2.PID[8].Raw);
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
                .SubComponent(2).Set("SecondSub")
                .Component(2).Set("SecondComp")
                .Field(7).Set("19900101");

            Assert.Equal("FirstSub", fluent.PID[5][1][1].Raw);
            Assert.Equal("SecondSub", fluent.PID[5][1][2].Raw);
            Assert.Equal("SecondComp", fluent.PID[5][2].Raw);
            Assert.Equal("19900101", fluent.PID[7].Raw);
        }

        [Fact]
        public void SubComponentAccessor_SetNull_ShouldSetEmptyString()
        {
            var fluent = CreateTestMessage();

            fluent.PID[5][1][1].Set(null);

            Assert.Equal("", fluent.PID[5][1][1].Raw);
        }

        [Fact]
        public void Navigation_Set_ShouldUseCorrectMethods()
        {
            var fluent = CreateTestMessage();

            // Test that navigation methods return accessors that use Set(string), not Value(string)
            // This verifies the pattern shown in the README examples
            
            // FieldMutator navigation to FieldMutator should use Set(string)
            fluent.PID[3].Set("12345")
                .Field(7).Set("19850315")          // FieldMutator.Field() returns FieldMutator, use .Set()
                .Field(8).Set("M");                // FieldMutator.Field() returns FieldMutator, use .Set()

            // ComponentMutator navigation to FieldMutator and ComponentMutator should use Set(string)  
            fluent.PID[5][1].Set("Smith")
                .Component(2).Set("John")          // ComponentMutator.Component() returns ComponentMutator, use .Set()
                .Field(7).Set("19850315");         // ComponentMutator.Field() returns FieldMutator, use .Set()

            // SubComponentMutator navigation should all use Set(string)
            fluent.PID[5][1][1].Set("LastName")
                .SubComponent(2).Set("FirstName")  // SubComponentMutator.SubComponent() returns SubComponentMutator, use .Set()
                .Component(3).Set("MiddleName")    // SubComponentMutator.Component() returns ComponentMutator, use .Set()
                .Field(9).Set("F");                // SubComponentMutator.Field() returns FieldMutator, use .Set()

            // Verify all values were set correctly
            Assert.Equal("12345", fluent.PID[3].Raw);
            Assert.Equal("19850315", fluent.PID[7].Raw);
            Assert.Equal("M", fluent.PID[8].Raw);
            Assert.Equal("LastName&FirstName", fluent.PID[5][1].Raw); // Component 1 now has subcomponents
            Assert.Equal("John", fluent.PID[5][2].Raw);
            Assert.Equal("LastName", fluent.PID[5][1][1].Raw);
            Assert.Equal("FirstName", fluent.PID[5][1][2].Raw);
            Assert.Equal("MiddleName", fluent.PID[5][3].Raw);
            Assert.Equal("F", fluent.PID[9].Raw);
        }

        [Fact]
        public void Navigation_Unified_Set_ShouldWork()
        {
            var fluent = CreateTestMessage();

            // Test unified Set() API - all mutators now have Set(string) method
            fluent.PID[3].Set("12345")
                .Field(7).Set("19850315")
                .Field(8).Set("M");

            // Verify all values were set correctly
            Assert.Equal("12345", fluent.PID[3].Raw);
            Assert.Equal("19850315", fluent.PID[7].Raw);
            Assert.Equal("M", fluent.PID[8].Raw);
        }

        [Fact]
        public void MessageBuilding_WithShortcuts_ShouldBeConcise()
        {
            var fluent = CreateTestMessage();

            // Demonstrate concise message building
            fluent.PID[3].SetRaw("98765")
                .Field(5).SetRaw("Johnson^Mary^Elizabeth")
                .Field(7).SetRaw("19851225")
                .Field(8).SetRaw("F");

            fluent.PID[5][1].SetRaw("Johnson")
                .Component(2).SetRaw("Mary")
                .Component(3).SetRaw("Elizabeth")
                .Field(11).SetRaw("456 Oak Ave^Boston^MA^02101");

            Assert.Equal("98765", fluent.PID[3].Raw);
            Assert.Equal("Johnson^Mary^Elizabeth", fluent.PID[5].Raw);
            Assert.Equal("19851225", fluent.PID[7].Raw);
            Assert.Equal("F", fluent.PID[8].Raw);
            Assert.Equal("Johnson", fluent.PID[5][1].Raw);
            Assert.Equal("Mary", fluent.PID[5][2].Raw);
            Assert.Equal("Elizabeth", fluent.PID[5][3].Raw);
            Assert.Equal("456 Oak Ave^Boston^MA^02101", fluent.PID[11].Raw);
        }

        [Fact]
        public void ShortcutVsDirect_SubComponents_ShouldProduceSameResult()
        {
            var fluent1 = CreateTestMessage();
            var fluent2 = CreateTestMessage();

            // Using shortcuts
            fluent1.PID[5][1][1].Set("Doe");
            fluent1.PID[5][1][2].Set("Jr");

            // Using direct Set() method
            fluent2.PID[5][1][1].Set("Doe");
            fluent2.PID[5][1][2].Set("Jr");

            Assert.Equal(fluent1.PID[5][1][1].Raw, fluent2.PID[5][1][1].Raw);
            Assert.Equal(fluent1.PID[5][1][2].Raw, fluent2.PID[5][1][2].Raw);
        }

        [Fact]
        public void CompleteHierarchy_WithShortcuts_ShouldWorkSeamlessly()
        {
            var fluent = CreateTestMessage();

            // Test complete hierarchy: Field -> Component -> SubComponent shortcuts
            fluent.PID[5].Set("Smith^John^M^Dr^III")
                .Field(7).Set("19900101")
                .Field(8).Set("M");

            fluent.PID[5][1].Set("Johnson")
                .Component(2).Set("Jane")
                .Field(11).Set("123 Main St");

            fluent.PID[5][1][1].Set("NewLastName")
                .SubComponent(2).Set("Suffix")
                .Component(3).Set("MiddleName")
                .Field(13).Set("555-1234");

            // Check individual subcomponents
            Assert.Equal("NewLastName", fluent.PID[5][1][1].Raw);
            Assert.Equal("Suffix", fluent.PID[5][1][2].Raw);
            Assert.Equal("Jane", fluent.PID[5][2].Raw);
            Assert.Equal("MiddleName", fluent.PID[5][3].Raw);
            Assert.Equal("19900101", fluent.PID[7].Raw);
            Assert.Equal("M", fluent.PID[8].Raw);
            Assert.Equal("123 Main St", fluent.PID[11].Raw);
            Assert.Equal("555-1234", fluent.PID[13].Raw);
        }
    }
}