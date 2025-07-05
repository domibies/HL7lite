using System;
using HL7lite.Fluent;
using Xunit;

namespace HL7lite.Test.Fluent
{
    public class FieldSetVsRepetitionsCollectionTests
    {
        [Fact]
        public void Set_CalledMultipleTimes_ShouldOverwritePreviousValue()
        {
            // Arrange
            var message = new FluentMessage(new Message());
            message.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ADT^A01")
                .AutoControlId()
                .Build();
            
            message.Segments("PID").Add();
            
            // Act - Call Set() multiple times
            message.PID[3].Set("FirstID");
            message.PID[3].Set("SecondID");
            
            // Assert - Should only have the last value, not repetitions
            Assert.Equal("SecondID", message.PID[3].Raw);
            Assert.False(message.PID[3].HasRepetitions);
            Assert.Equal(1, message.PID[3].RepetitionCount);
        }
        
        [Fact]
        public void Set_ThenRepetitionsAdd_ShouldPreserveOriginalAndAddNew()
        {
            // Arrange
            var message = new FluentMessage(new Message());
            message.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ADT^A01")
                .AutoControlId()
                .Build();
            
            message.Segments("PID").Add();
            
            // Act - Set initial value, then add repetition
            message.PID[3].Set("FirstID");
            message.PID[3].Repetitions.Add("SecondID");
            
            // Assert - Should have both values as repetitions
            Assert.True(message.PID[3].HasRepetitions);
            Assert.Equal(2, message.PID[3].RepetitionCount);
            Assert.Equal("FirstID", message.PID[3].Raw); // First repetition
            Assert.Equal("FirstID", message.PID[3].Repetition(1).Raw);
            Assert.Equal("SecondID", message.PID[3].Repetition(2).Raw);
        }
        
        [Fact]
        public void RepetitionsAdd_CalledMultipleTimes_ShouldAddEachAsNewRepetition()
        {
            // Arrange
            var message = new FluentMessage(new Message());
            message.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ADT^A01")
                .AutoControlId()
                .Build();
            
            message.Segments("PID").Add();
            
            // Act - Add multiple repetitions
            message.PID[3].Repetitions.Add("FirstID");
            message.PID[3].Repetitions.Add("SecondID");
            message.PID[3].Repetitions.Add("ThirdID");
            
            // Assert - Should have all three values as repetitions
            Assert.True(message.PID[3].HasRepetitions);
            Assert.Equal(3, message.PID[3].RepetitionCount);
            Assert.Equal("FirstID", message.PID[3].Repetition(1).Raw);
            Assert.Equal("SecondID", message.PID[3].Repetition(2).Raw);
            Assert.Equal("ThirdID", message.PID[3].Repetition(3).Raw);
        }
        
        [Fact]
        public void SetAfterRepetitionsAdd_ActualBehavior()
        {
            // Arrange
            var message = new FluentMessage(new Message());
            message.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ADT^A01")
                .AutoControlId()
                .Build();
            
            message.Segments("PID").Add();
            
            // Act - Add repetitions, then set new value  
            message.PID[3].Repetitions.Add("FirstID");
            message.PID[3].Repetitions.Add("SecondID");
            message.PID[3].Set("NewFirstID"); // This should reset the field
            
            // Debug: Print current state
            var hasReps = message.PID[3].HasRepetitions;
            var repCount = message.PID[3].RepetitionCount;
            var value = message.PID[3].Raw;
            
            // Assert what actually happens - calling Set() after Repetitions.Add() resets the field
            Assert.False(message.PID[3].HasRepetitions, $"HasRepetitions={hasReps}, RepCount={repCount}, Value='{value}'");
            Assert.Equal(1, message.PID[3].RepetitionCount);
            Assert.Equal("NewFirstID", message.PID[3].Raw);
        }
        
        [Fact]
        public void CorrectWayToAddMultipleRepetitions_UsingRepetitionsCollection()
        {
            // Arrange
            var message = new FluentMessage(new Message());
            message.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ADT^A01")
                .AutoControlId()
                .Build();
            
            message.Segments("PID").Add();
            
            // Act - Correct pattern: Use consistent collection approach
            message.PID[3].Repetitions.Add("MRN001");
            message.PID[3].Repetitions.Add("ENC123");
            message.PID[3].Repetitions.Add("SSN456");
            
            // Assert
            Assert.True(message.PID[3].HasRepetitions);
            Assert.Equal(3, message.PID[3].RepetitionCount);
            Assert.Equal("MRN001", message.PID[3].Repetition(1).Raw);
            Assert.Equal("ENC123", message.PID[3].Repetition(2).Raw);
            Assert.Equal("SSN456", message.PID[3].Repetition(3).Raw);
        }
    }
}