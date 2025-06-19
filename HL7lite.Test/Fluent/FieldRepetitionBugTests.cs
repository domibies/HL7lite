using System;
using HL7lite.Fluent;
using Xunit;

namespace HL7lite.Test.Fluent
{
    public class FieldRepetitionBugTests
    {
        [Fact]
        public void FieldAccessor_FluentAPIAddRepetition_WorksCorrectlyAfterFix()
        {
            // Arrange - Create message with single field value
            var message = new FluentMessage(new Message());
            message.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ADT^A01")
                .AutoControlId()
                .Build();
            
            message.Segments("PID").Add();
            message.PID[5].Set().Value("DOE^JOHN^M");
            
            // Verify initial state - single field, no repetitions
            Assert.Equal("DOE^JOHN^M", message.PID[5].Value);
            Assert.False(message.PID[5].HasRepetitions);
            Assert.Equal(1, message.PID[5].RepetitionCount);
            
            // Act - Add repetition using fluent API mutator approach  
            // This demonstrates typical usage where users would expect this to work
            message.PID[5].Set().AddRepetition("SMITH^JANE^F");
            
            // Assert - The bug is fixed, so this should work correctly
            Assert.True(message.PID[5].HasRepetitions);
            Assert.Equal(2, message.PID[5].RepetitionCount);
            Assert.Equal("DOE^JOHN^M", message.PID[5].Value); // Original value preserved as first repetition
            Assert.Equal("DOE^JOHN^M", message.PID[5].Repetition(1).Value);
            Assert.Equal("SMITH^JANE^F", message.PID[5].Repetition(2).Value);
        }
        
        [Fact] 
        public void FieldAccessor_DirectFieldAddRepetition_ChecksHasRepetitionsLogic()
        {
            // Arrange - Create message and verify we can access the underlying field
            var message = new FluentMessage(new Message());
            message.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ADT^A01")
                .AutoControlId()
                .Build();
            
            message.Segments("PID").Add();
            message.PID[5].Set().Value("DOE^JOHN^M");
            
            // Get the underlying field to manipulate it directly
            var underlyingField = message.UnderlyingMessage.DefaultSegment("PID").Fields(5);
            
            // Verify initial state
            Assert.Equal("DOE^JOHN^M", underlyingField.Value);
            Assert.False(underlyingField.HasRepetitions);
            
            // Act - Add a repetition using the core Field API
            var secondRepetition = underlyingField.EnsureRepetition(2);
            secondRepetition.Value = "SMITH^JANE^F";
            
            // Assert - Check if HasRepetitions is properly updated
            Assert.True(underlyingField.HasRepetitions, "Field should have HasRepetitions=true after adding repetition");
            Assert.Equal(2, underlyingField.Repetitions().Count);
            
            // Verify underlying field repetitions are correct
            var repetitions = underlyingField.Repetitions();
            Assert.Equal("DOE^JOHN^M", repetitions[0].Value); // First repetition preserves original value
            Assert.Equal("SMITH^JANE^F", repetitions[1].Value); // Second repetition has new value
            
            // Now test the FieldAccessor behavior
            Assert.Equal("DOE^JOHN^M", message.PID[5].Value); // FieldAccessor.Value should return first repetition
            Assert.True(message.PID[5].HasRepetitions); // FieldAccessor.HasRepetitions should reflect underlying field state
            Assert.Equal(2, message.PID[5].RepetitionCount);
            
            // Test individual repetitions
            Assert.Equal("DOE^JOHN^M", message.PID[5].Repetition(1).Value);
            Assert.Equal("SMITH^JANE^F", message.PID[5].Repetition(2).Value);
        }
    }
}