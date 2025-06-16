using HL7lite;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace HL7Lite.Test
{
    public class TestRepeatedFields
    {
        private static readonly string repeatedFieldsMsg1 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||Z99^Z01|12345|P|2.3||
ZZA|1||~1~22^A&B~333|0^A~1~22^A&B~333";

        [Fact]
        public void RepeatedFields_HasRepetions_AfterParse_Correct()
        {
            var message = new Message(repeatedFieldsMsg1);
            message.ParseMessage();

            var zza = message.Segments("ZZA")[0];

            Assert.NotNull(zza);

            var field1 = zza.Fields(1);
            var field3 = zza.Fields(3);

            Assert.False(field1.HasRepetitions);
            Assert.True(field3.HasRepetitions);
        }

        [Fact]
        public void RepeatedFields_HasRepetions_AfterPutValue_Correct()
        {
            var message = new Message(repeatedFieldsMsg1);
            message.ParseMessage();

            var zza = message.Segments("ZZA")[0];
            Assert.NotNull(zza);

            message.PutValue("ZZA.1(1)", "TEST");

            var field1 = zza.Fields(1);

            Assert.False(field1.HasRepetitions);

            message.PutValue("ZZA.1(2)", "ANOTHER TEST");
            Assert.True(field1.HasRepetitions);

        }

        [Fact]
        public void RepeatedFields_HasRepetionsOnMessage_AfterParse_Correct()
        {
            var message = new Message(repeatedFieldsMsg1);
            message.ParseMessage();

            Assert.True(message.HasRepetitions("ZZA.3"));
        }

        [Fact]
        public void RepeatedFields_GetValue()
        {
            var message = new Message(repeatedFieldsMsg1);
            message.ParseMessage();

//            var value = message.GetValue("ZZA.3");
            var value = message.GetValue("ZZA.4.1"); // should get the first component of the first repetition

            Assert.True(value == "0"); 
        }

        [Fact]
        public void Field_RemoveRepetition_FromMultipleRepetitions_ShouldRemoveSpecificRepetition()
        {
            // Arrange
            var message = new Message(repeatedFieldsMsg1);
            message.ParseMessage();
            var field = message.Segments("ZZA")[0].Fields(3); // Field with repetitions: ~1~22^A&B~333
            
            // Initial state: 3 repetitions ("", "1", "22^A&B", "333")
            Assert.True(field.HasRepetitions);
            Assert.Equal(4, field.Repetitions().Count);
            
            // Act - Remove second repetition (1-based: repetition 2 = "1")
            field.RemoveRepetition(2);
            
            // Assert
            Assert.True(field.HasRepetitions); // Should still have repetitions
            Assert.Equal(3, field.Repetitions().Count);
            Assert.Equal("", field.Repetitions()[0].Value); // First repetition unchanged
            Assert.Equal("22^A&B", field.Repetitions()[1].Value); // Third became second
            Assert.Equal("333", field.Repetitions()[2].Value); // Fourth became third
        }

        [Fact]
        public void Field_RemoveRepetition_DownToSingleRepetition_ShouldConvertToSingleField()
        {
            // Arrange - Use the existing field 3 that already has repetitions and modify it
            var message = new Message(repeatedFieldsMsg1);
            message.ParseMessage();
            var field = message.Segments("ZZA")[0].Fields(3); // Field with repetitions: ~1~22^A&B~333
            
            // Verify initial state: 4 repetitions ("", "1", "22^A&B", "333")
            Assert.True(field.HasRepetitions);
            Assert.Equal(4, field.Repetitions().Count);
            
            // Remove last two repetitions to get down to 2
            field.RemoveRepetition(4); // Remove "333"
            field.RemoveRepetition(3); // Remove "22^A&B"
            
            // Should still have repetitions
            Assert.True(field.HasRepetitions);
            Assert.Equal(2, field.Repetitions().Count);
            
            // Act - Remove one more repetition, leaving only one
            field.RemoveRepetition(2); // Remove "1", leaving only ""
            
            // Assert - Should convert back to single field
            Assert.False(field.HasRepetitions);
            Assert.Equal("", field.Value); // The remaining value should be ""
        }

        [Fact]
        public void Field_RemoveRepetition_LastRepetition_ShouldClearField()
        {
            // Arrange - Use existing message and create single field
            var message = new Message(repeatedFieldsMsg1);
            message.ParseMessage();
            
            // Create field with single value
            message.PutValue("ZZA.1", "OnlyValue");
            var field = message.Segments("ZZA")[0].Fields(1);
            
            Assert.False(field.HasRepetitions);
            Assert.Equal("OnlyValue", field.Value);
            
            // Act - Remove the only repetition (repetition 1)
            field.RemoveRepetition(1);
            
            // Assert - Field should be empty
            Assert.False(field.HasRepetitions);
            Assert.Equal("", field.Value);
        }

        [Fact]
        public void Field_RemoveRepetition_InvalidRepetitionNumber_ShouldThrowHL7Exception()
        {
            // Arrange
            var message = new Message(repeatedFieldsMsg1);
            message.ParseMessage();
            var field = message.Segments("ZZA")[0].Fields(3); // Field with 4 repetitions
            
            // Act & Assert - Test invalid repetition numbers
            var ex1 = Assert.Throws<HL7Exception>(() => field.RemoveRepetition(0)); // 0 is invalid (1-based)
            Assert.Contains("Invalid repetition number (0 < 1)", ex1.Message);
            
            var ex2 = Assert.Throws<HL7Exception>(() => field.RemoveRepetition(-1)); // Negative is invalid
            Assert.Contains("Invalid repetition number (-1 < 1)", ex2.Message);
            
            var ex3 = Assert.Throws<HL7Exception>(() => field.RemoveRepetition(5)); // Beyond available repetitions
            Assert.Contains("Repetition 5 does not exist", ex3.Message);
        }

        [Fact]
        public void Field_RemoveRepetition_FromSingleField_WithInvalidNumber_ShouldThrowHL7Exception()
        {
            // Arrange
            var message = new Message(repeatedFieldsMsg1);
            message.ParseMessage();
            message.PutValue("ZZA.1", "SingleValue");
            var field = message.Segments("ZZA")[0].Fields(1);
            
            Assert.False(field.HasRepetitions);
            
            // Act & Assert - Trying to remove repetition 2 from single field should fail
            var ex = Assert.Throws<HL7Exception>(() => field.RemoveRepetition(2));
            Assert.Contains("Repetition 2 does not exist. Field has only 1 repetition", ex.Message);
        }

        [Fact]
        public void Field_RemoveRepetition_RemoveFirstRepetition_ShouldUpdateFieldValue()
        {
            // Arrange
            var message = new Message(repeatedFieldsMsg1);
            message.ParseMessage();
            
            // Create field with 3 repetitions
            message.PutValue("ZZA.1(1)", "First");
            message.PutValue("ZZA.1(2)", "Second"); 
            message.PutValue("ZZA.1(3)", "Third");
            var field = message.Segments("ZZA")[0].Fields(1);
            
            Assert.True(field.HasRepetitions);
            Assert.Equal(3, field.Repetitions().Count);
            
            // Act - Remove first repetition
            field.RemoveRepetition(1);
            
            // Assert - Second repetition should become the new first
            Assert.True(field.HasRepetitions);
            Assert.Equal(2, field.Repetitions().Count);
            Assert.Equal("Second", field.Repetitions()[0].Value);
            Assert.Equal("Third", field.Repetitions()[1].Value);
        }

        [Fact]
        public void Field_RemoveRepetition_RemoveAllRepetitionsSequentially_ShouldLeaveEmptyField()
        {
            // Arrange
            var message = new Message(repeatedFieldsMsg1);
            message.ParseMessage();
            
            // Create field with 3 repetitions
            message.PutValue("ZZA.1(1)", "First");
            message.PutValue("ZZA.1(2)", "Second");
            message.PutValue("ZZA.1(3)", "Third");
            var field = message.Segments("ZZA")[0].Fields(1);
            
            Assert.True(field.HasRepetitions);
            Assert.Equal(3, field.Repetitions().Count);
            
            // Act - Remove all repetitions one by one (always remove repetition 1)
            field.RemoveRepetition(1); // Remove "First", "Second" becomes first
            Assert.True(field.HasRepetitions);
            Assert.Equal(2, field.Repetitions().Count);
            
            field.RemoveRepetition(1); // Remove "Second", "Third" becomes single field
            Assert.False(field.HasRepetitions);
            Assert.Equal("Third", field.Value);
            
            field.RemoveRepetition(1); // Remove "Third", field becomes empty
            Assert.False(field.HasRepetitions);
            Assert.Equal("", field.Value);
        }

        [Fact]
        public void Field_RemoveRepetition_PreservesFieldStructure()
        {
            // Arrange - Create a field with component structures in repetitions
            var message = new Message(repeatedFieldsMsg1);
            message.ParseMessage();
            
            // Create repetitions with component structures
            message.PutValue("ZZA.1(1)", "A^B^C");
            message.PutValue("ZZA.1(2)", "D^E^F");
            message.PutValue("ZZA.1(3)", "G^H^I");
            var field = message.Segments("ZZA")[0].Fields(1);
            
            // Act - Remove middle repetition
            field.RemoveRepetition(2);
            
            // Assert - Remaining repetitions should preserve their component structure
            Assert.True(field.HasRepetitions);
            Assert.Equal(2, field.Repetitions().Count);
            
            var firstRep = field.Repetitions()[0];
            var thirdRep = field.Repetitions()[1]; // Was third, now second
            
            Assert.Equal("A", firstRep.Components(1).Value);
            Assert.Equal("B", firstRep.Components(2).Value);
            Assert.Equal("C", firstRep.Components(3).Value);
            
            Assert.Equal("G", thirdRep.Components(1).Value);
            Assert.Equal("H", thirdRep.Components(2).Value);
            Assert.Equal("I", thirdRep.Components(3).Value);
        }
    }
}
