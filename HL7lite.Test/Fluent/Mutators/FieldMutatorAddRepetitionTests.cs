using System;
using System.Linq;
using HL7lite.Fluent;
using Xunit;

namespace HL7lite.Test.Fluent.Mutators
{
    public class FieldMutatorAddRepetitionTests
    {
        private FluentMessage CreateTestMessage()
        {
            var message = new Message();
            var fluent = new FluentMessage(message);
            fluent.CreateMSH
                .Sender("TEST", "FACILITY")
                .Receiver("DEST", "FACILITY2")
                .MessageType("ADT^A01")
                .AutoControlId()
                .Build();
            
            fluent.Segments("PID").Add();
            return fluent;
        }

        [Fact]
        public void AddRepetition_WithValue_ShouldAddRepetitionAndReturnMutatorForNewRepetition()
        {
            // Arrange
            var fluent = CreateTestMessage();
            fluent.PID[3].Set("FirstID");

            // Act
            var mutator = fluent.PID[3].Set()
                .AddRepetition("SecondID");

            // Assert
            Assert.NotNull(mutator);
            Assert.Equal(2, fluent.PID[3].Repetitions.Count);
            Assert.Equal("FirstID", fluent.PID[3].Repetitions[0].Raw);
            Assert.Equal("SecondID", fluent.PID[3].Repetitions[1].Raw);
            
            // Verify mutator points to the new repetition
            mutator.Set("UpdatedSecondID");
            Assert.Equal("UpdatedSecondID", fluent.PID[3].Repetitions[1].Raw);
            Assert.Equal("FirstID", fluent.PID[3].Repetitions[0].Raw); // First unchanged
        }

        [Fact]
        public void AddRepetition_EmptyMethod_ShouldAddEmptyRepetitionAndReturnMutatorForNewRepetition()
        {
            // Arrange
            var fluent = CreateTestMessage();
            fluent.PID[3].Set("FirstID");

            // Act
            var mutator = fluent.PID[3].Set()
                .AddRepetition();

            // Assert
            Assert.NotNull(mutator);
            Assert.Equal(2, fluent.PID[3].Repetitions.Count);
            Assert.Equal("FirstID", fluent.PID[3].Repetitions[0].Raw);
            Assert.Equal("", fluent.PID[3].Repetitions[1].Raw);
            
            // Verify mutator points to the new repetition
            mutator.SetComponents("MRN", "001", "HOSPITAL");
            Assert.Equal("MRN^001^HOSPITAL", fluent.PID[3].Repetitions[1].Raw);
        }

        [Fact]
        public void AddRepetition_FluentChaining_ShouldMaintainChainWithMultipleRepetitions()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act - Full fluent chain
            fluent.PID[3].Set("FirstID")
                .AddRepetition("SecondID")
                .AddRepetition("ThirdID")
                .Field(5).SetComponents("Smith", "John", "M")
                .Field(7).Set("19850315")
                .Field(8).Set("M");

            // Assert
            Assert.Equal(3, fluent.PID[3].Repetitions.Count);
            Assert.Equal("FirstID", fluent.PID[3].Repetitions[0].Raw);
            Assert.Equal("SecondID", fluent.PID[3].Repetitions[1].Raw);
            Assert.Equal("ThirdID", fluent.PID[3].Repetitions[2].Raw);
            Assert.Equal("Smith^John^M", fluent.PID[5].Raw);
            Assert.Equal("19850315", fluent.PID[7].Raw);
            Assert.Equal("M", fluent.PID[8].Raw);
        }

        [Fact]
        public void AddRepetition_WithComponents_ShouldAllowImmediateComponentSetting()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act - Add repetitions with immediate component setting
            fluent.PID[3].Set("SimpleID")
                .AddRepetition()
                    .SetComponents("MRN", "001", "HOSPITAL")
                .AddRepetition()
                    .SetComponents("ENC", "123", "VISIT")
                .Field(7).Set("19850315");

            // Assert
            Assert.Equal(3, fluent.PID[3].Repetitions.Count);
            Assert.Equal("SimpleID", fluent.PID[3].Repetitions[0].Raw);
            Assert.Equal("MRN^001^HOSPITAL", fluent.PID[3].Repetitions[1].Raw);
            Assert.Equal("ENC^123^VISIT", fluent.PID[3].Repetitions[2].Raw);
            Assert.Equal("19850315", fluent.PID[7].Raw);
        }

        [Fact]
        public void AddRepetition_MixedPattern_ShouldSupportBothSimpleAndComplexValues()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act - Mix simple values and component-based values
            fluent.PID[3].Set("ID1")
                .AddRepetition("SimpleID2")              // Simple value
                .AddRepetition()                         // Empty for components
                    .SetComponents("MRN", "789", "LAB")  // Complex structure
                .AddRepetition("SimpleID4")              // Back to simple
                .Field(5).SetComponents("Doe", "Jane");

            // Assert
            Assert.Equal(4, fluent.PID[3].Repetitions.Count);
            Assert.Equal("ID1", fluent.PID[3].Repetitions[0].Raw);
            Assert.Equal("SimpleID2", fluent.PID[3].Repetitions[1].Raw);
            Assert.Equal("MRN^789^LAB", fluent.PID[3].Repetitions[2].Raw);
            Assert.Equal("SimpleID4", fluent.PID[3].Repetitions[3].Raw);
            Assert.Equal("Doe^Jane", fluent.PID[5].Raw);
        }

        [Fact]
        public void AddRepetition_OnEmptyField_ShouldCreateFirstRepetition()
        {
            // Arrange
            var fluent = CreateTestMessage();
            // PID[3] starts empty

            // Act
            var mutator = fluent.PID[3].Set()
                .AddRepetition("FirstID");

            // Assert
            Assert.Equal(1, fluent.PID[3].Repetitions.Count);
            Assert.Equal("FirstID", fluent.PID[3].Raw);
            Assert.Equal("FirstID", fluent.PID[3].Repetitions[0].Raw);
        }

        [Fact]
        public void AddRepetition_ChainedWithOtherMutatorMethods_ShouldWorkSeamlessly()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act - Complex chaining scenario
            fluent.PID[3].Set("ID1")
                .AddRepetition("ID2")
                    .SetIf("ConditionalID", false)  // Should not change
                .AddRepetition()
                    .SetNull()                      // Set to HL7 null
                .AddRepetition("ID4")
                .Field(5).SetComponents("Smith", "John")
                .Field(7).SetDate(new DateTime(1985, 3, 15));

            // Assert
            Assert.Equal(4, fluent.PID[3].Repetitions.Count);
            Assert.Equal("ID1", fluent.PID[3].Repetitions[0].Raw);
            Assert.Equal("ID2", fluent.PID[3].Repetitions[1].Raw);
            Assert.Equal("\"\"", fluent.PID[3].Repetitions[2].Raw); // HL7 null
            Assert.Equal("ID4", fluent.PID[3].Repetitions[3].Raw);
            Assert.Equal("Smith^John", fluent.PID[5].Raw);
            Assert.Equal("19850315", fluent.PID[7].Raw);
        }

        [Fact]
        public void AddRepetition_MultipleTimes_ShouldCorrectlyTrackRepetitionIndices()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act - Add multiple repetitions and verify each mutator points to correct repetition
            var mutator1 = fluent.PID[3].Set("First").AddRepetition("Second");
            var mutator2 = mutator1.AddRepetition("Third");
            var mutator3 = mutator2.AddRepetition("Fourth");

            // Update each through their mutators
            mutator1.SetComponents("Second", "Updated");
            mutator2.SetComponents("Third", "Updated");
            mutator3.SetComponents("Fourth", "Updated");

            // Assert
            Assert.Equal(4, fluent.PID[3].Repetitions.Count);
            Assert.Equal("First", fluent.PID[3].Repetitions[0].Raw);
            Assert.Equal("Second^Updated", fluent.PID[3].Repetitions[1].Raw);
            Assert.Equal("Third^Updated", fluent.PID[3].Repetitions[2].Raw);
            Assert.Equal("Fourth^Updated", fluent.PID[3].Repetitions[3].Raw);
        }

        [Fact]
        public void AddRepetition_BackwardCompatibility_ExistingPatternShouldStillWork()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act - Use old pattern alongside new pattern
            fluent.PID[3].Set("ID1");
            fluent.PID[3].Repetitions.Add("ID2");  // Old pattern
            fluent.PID[3].Set()
                .AddRepetition("ID3");             // New pattern
            fluent.PID[3].Repetitions.Add("ID4");  // Old pattern again

            // Assert - Both patterns should work together
            Assert.Equal(4, fluent.PID[3].Repetitions.Count);
            Assert.Equal("ID1", fluent.PID[3].Repetitions[0].Raw);
            Assert.Equal("ID2", fluent.PID[3].Repetitions[1].Raw);
            Assert.Equal("ID3", fluent.PID[3].Repetitions[2].Raw);
            Assert.Equal("ID4", fluent.PID[3].Repetitions[3].Raw);
        }

        [Fact]
        public void AddRepetition_RealWorldScenario_PatientIdentifiers()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act - Real-world patient identifier scenario
            fluent.PID[1].SetRaw("1")
                .Field(3).SetRaw("12345^^^MRN^MR")         // Primary MRN
                    .AddRepetition()                     // Add SSN
                        .SetComponents("987654321", "", "", "SS")
                    .AddRepetition()                     // Add encounter number
                        .SetComponents("E2024001", "", "", "VN", "VISIT")
                    .AddRepetition("OLD-MRN-99999")      // Legacy ID as simple string
                .Field(5).SetComponents("Johnson", "Mary", "Elizabeth", "Jr", "Dr")
                .Field(7).SetDate(DateTime.Today.AddYears(-45))
                .Field(8).SetRaw("F");

            // Assert
            var ids = fluent.PID[3].Repetitions.ToList();
            Assert.Equal(4, ids.Count);
            Assert.Equal("12345^^^MRN^MR", ids[0].Raw);
            Assert.Equal("987654321^^^SS", ids[1].Raw);
            Assert.Equal("E2024001^^^VN^VISIT", ids[2].Raw);
            Assert.Equal("OLD-MRN-99999", ids[3].Raw);
            Assert.Equal("Johnson^Mary^Elizabeth^Jr^Dr", fluent.PID[5].Raw);
        }
    }
}