using System;
using System.Linq;
using Xunit;
using HL7lite.Fluent.Accessors;

namespace HL7lite.Test.Fluent.Accessors
{
    public class SegmentAccessorTests
    {
        // Helper methods for common assertion patterns
        private static void AssertThrowsHL7Exception(Action action, string expectedErrorCode)
        {
            var ex = Assert.Throws<HL7Exception>(action);
            Assert.Equal(expectedErrorCode, ex.ErrorCode);
        }

        private static void AssertThrowsHL7Exception(Action action)
        {
            Assert.Throws<HL7Exception>(action);
        }

        [Fact]
        public void Constructor_WithNullMessage_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SegmentAccessor(null, "PID"));
        }

        [Fact]
        public void Constructor_WithNullSegmentName_ShouldThrowArgumentNullException()
        {
            // Arrange
            var message = HL7MessageBuilder.Create().WithMSH().WithPID().Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SegmentAccessor(message, null));
        }

        [Fact]
        public void Exists_WhenSegmentExists_ShouldReturnTrue()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var pidAccessor = fluentMessage["PID"];

            // Assert
            Assert.True(pidAccessor.Exists);
        }

        [Fact]
        public void Exists_WhenSegmentDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var nonExistentAccessor = fluentMessage["ZZZ"];

            // Assert
            Assert.False(nonExistentAccessor.Exists);
        }

        [Fact]
        public void HasMultiple_WhenSingleSegment_ShouldReturnFalse()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var pidAccessor = fluentMessage["PID"];

            // Assert
            Assert.False(pidAccessor.HasMultiple);
        }

        [Fact]
        public void HasMultiple_WhenMultipleSegments_ShouldReturnTrue()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .WithDG1(1, "250.00", "Diabetes")
                .WithDG1(2, "401.9", "Hypertension")
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var dg1Accessor = fluentMessage["DG1"];

            // Assert
            Assert.True(dg1Accessor.HasMultiple);
        }

        [Fact]
        public void HasMultiple_WhenSegmentDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var nonExistentAccessor = fluentMessage["ZZZ"];

            // Assert
            Assert.False(nonExistentAccessor.HasMultiple);
        }

        [Fact]
        public void Count_WhenSingleSegment_ShouldReturnOne()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var pidAccessor = fluentMessage["PID"];

            // Assert
            Assert.Equal(1, pidAccessor.Count);
        }

        [Fact]
        public void Count_WhenMultipleSegments_ShouldReturnCorrectCount()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .WithDG1(1, "250.00", "Diabetes")
                .WithDG1(2, "401.9", "Hypertension")
                .WithDG1(3, "272.4", "Hyperlipidemia")
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var dg1Accessor = fluentMessage["DG1"];

            // Assert
            Assert.Equal(3, dg1Accessor.Count);
        }

        [Fact]
        public void Count_WhenSegmentDoesNotExist_ShouldReturnZero()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var nonExistentAccessor = fluentMessage["ZZZ"];

            // Assert
            Assert.Equal(0, nonExistentAccessor.Count);
        }

        [Fact]
        public void IsSingle_WhenExactlyOneSegment_ShouldReturnTrue()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var pidAccessor = fluentMessage["PID"];

            // Assert
            Assert.True(pidAccessor.IsSingle);
        }

        [Fact]
        public void IsSingle_WhenMultipleSegments_ShouldReturnFalse()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .WithDG1(1, "250.00", "Diabetes")
                .WithDG1(2, "401.9", "Hypertension")
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var dg1Accessor = fluentMessage["DG1"];

            // Assert
            Assert.False(dg1Accessor.IsSingle);
        }

        [Fact]
        public void IsSingle_WhenSegmentDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var nonExistentAccessor = fluentMessage["ZZZ"];

            // Assert
            Assert.False(nonExistentAccessor.IsSingle);
        }

        [Theory]
        [InlineData("MSH")]
        [InlineData("PID")]
        [InlineData("PV1")]
        public void Exists_WithCommonSegments_ShouldDetectCorrectly(string segmentName)
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .WithPV1()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var accessor = fluentMessage[segmentName];

            // Assert
            Assert.True(accessor.Exists);
            Assert.True(accessor.IsSingle);
            Assert.False(accessor.HasMultiple);
            Assert.Equal(1, accessor.Count);
        }

        [Fact]
        public void Indexer_WithValidFieldNumber_ShouldReturnFieldAccessor()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var fieldAccessor = fluentMessage["PID"][3]; // Patient ID field

            // Assert
            Assert.NotNull(fieldAccessor);
            Assert.IsType<HL7lite.Fluent.Accessors.FieldAccessor>(fieldAccessor);
        }

        [Fact]
        public void Indexer_WithZeroFieldNumber_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => fluentMessage["PID"][0]);
        }

        [Fact]
        public void Indexer_WithNegativeFieldNumber_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => fluentMessage["PID"][-1]);
        }

        [Fact]
        public void Field_Method_ShouldReturnSameAsIndexer()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var indexerResult = fluentMessage["PID"][3];
            var methodResult = fluentMessage["PID"].Field(3);

            // Assert
            Assert.Same(indexerResult, methodResult);
        }

        [Fact] 
        public void Instance_WithValidIndex_ShouldReturnSpecificInstance()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .WithDG1(1, "250.00", "Diabetes")
                .WithDG1(2, "401.9", "Hypertension")
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var firstInstance = fluentMessage["DG1"].Instance(0);
            var secondInstance = fluentMessage["DG1"].Instance(1);

            // Assert
            Assert.NotNull(firstInstance);
            Assert.NotNull(secondInstance);
            Assert.True(firstInstance.Exists);
            Assert.True(secondInstance.Exists);
            Assert.NotSame(firstInstance, secondInstance);
        }

        [Fact]
        public void Instance_WithInvalidIndex_ShouldReturnNonExistentAccessor()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .WithDG1(1, "250.00", "Diabetes")
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var nonExistentInstance = fluentMessage["DG1"].Instance(5);

            // Assert
            Assert.NotNull(nonExistentInstance);
            Assert.False(nonExistentInstance.Exists);
        }

        [Fact]
        public void Instance_WithNegativeIndex_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => fluentMessage["PID"].Instance(-1));
        }

        [Fact]
        public void SpecificInstanceSegmentAccessor_WithMultipleSegments_ShouldAccessCorrectSegmentData()
        {
            // This test verifies that SpecificInstanceSegmentAccessor correctly accesses
            // the specific segment instance, not just the first segment of that type.
            // This addresses the bug where _segment property always returned the first segment [0].
            
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .WithDG1(1, "250.00", "Diabetes")
                .WithDG1(2, "401.9", "Hypertension") 
                .WithDG1(3, "272.4", "Hyperlipidemia")
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act - Get specific instances
            var firstDG1 = fluentMessage["DG1"].Instance(0);
            var secondDG1 = fluentMessage["DG1"].Instance(1);
            var thirdDG1 = fluentMessage["DG1"].Instance(2);

            // Assert - Verify each instance accesses correct segment data
            Assert.True(firstDG1.Exists);
            Assert.True(secondDG1.Exists);
            Assert.True(thirdDG1.Exists);

            // Verify field values are different for each instance
            Assert.Equal("1", firstDG1[1].Raw);   // DG1.1 (Set ID) should be "1"
            Assert.Equal("2", secondDG1[1].Raw);  // DG1.1 (Set ID) should be "2"  
            Assert.Equal("3", thirdDG1[1].Raw);   // DG1.1 (Set ID) should be "3"

            // Verify diagnosis codes are different
            Assert.Equal("250.00", firstDG1[3][1].Raw);   // First diagnosis code
            Assert.Equal("401.9", secondDG1[3][1].Raw);   // Second diagnosis code
            Assert.Equal("272.4", thirdDG1[3][1].Raw);    // Third diagnosis code

            // Verify diagnosis descriptions are different
            Assert.Equal("Diabetes", firstDG1[3][2].Raw);      // First diagnosis description
            Assert.Equal("Hypertension", secondDG1[3][2].Raw); // Second diagnosis description
            Assert.Equal("Hyperlipidemia", thirdDG1[3][2].Raw); // Third diagnosis description
        }

        [Fact]
        public void SpecificInstanceSegmentAccessor_Properties_ShouldReflectSpecificInstance()
        {
            // This test verifies that SpecificInstanceSegmentAccessor properties
            // correctly reflect the specific instance, not the general segment type
            
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .WithDG1(1, "250.00", "Diabetes")
                .WithDG1(2, "401.9", "Hypertension")
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var generalDG1 = fluentMessage["DG1"];  // General accessor
            var specificDG1 = fluentMessage["DG1"].Instance(0);  // Specific instance

            // Assert - General accessor properties
            Assert.True(generalDG1.Exists);
            Assert.True(generalDG1.HasMultiple);
            Assert.False(generalDG1.IsSingle);
            Assert.Equal(2, generalDG1.Count);

            // Assert - Specific instance accessor properties
            Assert.True(specificDG1.Exists);
            Assert.False(specificDG1.HasMultiple);  // Specific instance doesn't have multiple
            Assert.True(specificDG1.IsSingle);      // Specific instance is single
            Assert.Equal(1, specificDG1.Count);     // Specific instance count is 1
        }

        [Fact]
        public void SpecificInstanceSegmentAccessor_NonExistentInstance_ShouldReturnCorrectProperties()
        {
            // This test verifies that SpecificInstanceSegmentAccessor correctly handles
            // non-existent instances
            
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithPID()
                .WithDG1(1, "250.00", "Diabetes")
                .Build();
            var fluentMessage = new HL7lite.Fluent.FluentMessage(message);

            // Act
            var nonExistentInstance = fluentMessage["DG1"].Instance(5);

            // Assert
            Assert.False(nonExistentInstance.Exists);
            Assert.False(nonExistentInstance.HasMultiple);
            Assert.False(nonExistentInstance.IsSingle);
            Assert.Equal(0, nonExistentInstance.Count);
        }
    }
}