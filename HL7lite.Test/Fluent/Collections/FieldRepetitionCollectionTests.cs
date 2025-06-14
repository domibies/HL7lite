using System;
using System.Linq;
using Xunit;
using HL7lite.Fluent.Accessors;
using HL7lite.Fluent.Collections;
using HL7lite.Test.Fluent;

namespace HL7lite.Test.Fluent.Collections
{
    public class FieldRepetitionCollectionTests
    {
        [Fact]
        public void Count_WithSingleRepetition_ShouldReturn1()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001")
                .Build();
            var collection = new FieldRepetitionCollection(message, "PID", 3);
            
            // Act
            var count = collection.Count;
            
            // Assert
            Assert.Equal(1, count);
        }

        [Fact]
        public void Count_WithMultipleRepetitions_ShouldReturnCorrectCount()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001~ID002~ID003")
                .Build();
            var collection = new FieldRepetitionCollection(message, "PID", 3);
            
            // Act
            var count = collection.Count;
            
            // Assert
            Assert.Equal(3, count);
        }

        [Fact]
        public void Count_WithNoField_ShouldReturn0()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID")
                .Build();
            var collection = new FieldRepetitionCollection(message, "PID", 3);
            
            // Act
            var count = collection.Count;
            
            // Assert
            Assert.Equal(0, count);
        }

        [Fact]
        public void Indexer_WithValidIndex_ShouldReturnFieldAccessor()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001~ID002~ID003")
                .Build();
            var collection = new FieldRepetitionCollection(message, "PID", 3);
            
            // Act
            var field = collection[1]; // Second repetition (0-based)
            
            // Assert
            Assert.NotNull(field);
            Assert.IsType<FieldAccessor>(field);
            Assert.Equal("ID002", field.Value);
        }

        [Fact]
        public void Indexer_WithInvalidIndex_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001")
                .Build();
            var collection = new FieldRepetitionCollection(message, "PID", 3);
            
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => collection[1]);
            Assert.Throws<ArgumentOutOfRangeException>(() => collection[-1]);
        }

        [Fact]
        public void GetEnumerator_ShouldIterateAllRepetitions()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001~ID002~ID003")
                .Build();
            var collection = new FieldRepetitionCollection(message, "PID", 3);
            
            // Act
            var values = collection.Select(f => f.Value).ToList();
            
            // Assert
            Assert.Equal(3, values.Count);
            Assert.Equal("ID001", values[0]);
            Assert.Equal("ID002", values[1]);
            Assert.Equal("ID003", values[2]);
        }

        [Fact]
        public void LINQ_Select_ShouldWork()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001~ID002~ID003")
                .Build();
            var collection = new FieldRepetitionCollection(message, "PID", 3);
            
            // Act
            var upperCaseIds = collection.Select(f => f.Value.ToUpper()).ToArray();
            
            // Assert
            Assert.Equal(new[] { "ID001", "ID002", "ID003" }, upperCaseIds);
        }

        [Fact]
        public void LINQ_Where_ShouldWork()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001~ID002~ID003")
                .Build();
            var collection = new FieldRepetitionCollection(message, "PID", 3);
            
            // Act
            var filteredIds = collection.Where(f => f.Value.EndsWith("2")).Select(f => f.Value).ToList();
            
            // Assert
            Assert.Single(filteredIds);
            Assert.Equal("ID002", filteredIds[0]);
        }

        [Fact]
        public void First_WithRepetitions_ShouldReturnFirstRepetition()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001~ID002")
                .Build();
            var collection = new FieldRepetitionCollection(message, "PID", 3);
            
            // Act
            var first = collection.First();
            
            // Assert
            Assert.Equal("ID001", first.Value);
        }

        [Fact]
        public void First_WithNoRepetitions_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID")
                .Build();
            var collection = new FieldRepetitionCollection(message, "PID", 3);
            
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => collection.First());
        }

        [Fact]
        public void Any_WithRepetitions_ShouldReturnTrue()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001")
                .Build();
            var collection = new FieldRepetitionCollection(message, "PID", 3);
            
            // Act
            var hasAny = collection.Any();
            
            // Assert
            Assert.True(hasAny);
        }

        [Fact]
        public void Any_WithNoRepetitions_ShouldReturnFalse()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID")
                .Build();
            var collection = new FieldRepetitionCollection(message, "PID", 3);
            
            // Act
            var hasAny = collection.Any();
            
            // Assert
            Assert.False(hasAny);
        }

        [Fact]
        public void Constructor_WithNullMessage_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FieldRepetitionCollection(null, "PID", 3));
        }

        [Fact]
        public void Constructor_WithNullSegmentName_ShouldThrowArgumentNullException()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID")
                .Build();
            
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FieldRepetitionCollection(message, null, 3));
        }

        [Fact]
        public void Collection_WithNonExistentSegment_ShouldReturnEmptyCollection()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID")
                .Build();
            var collection = new FieldRepetitionCollection(message, "ZZZ", 3);
            
            // Act
            var count = collection.Count;
            
            // Assert
            Assert.Equal(0, count);
            Assert.False(collection.Any());
        }

        [Fact]
        public void Collection_WithComponentValues_ShouldAccessComponents()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001^Type1~ID002^Type2")
                .Build();
            var collection = new FieldRepetitionCollection(message, "PID", 3);
            
            // Act
            var firstIdType = collection[0][2].Value; // First repetition, second component
            var secondIdType = collection[1][2].Value; // Second repetition, second component
            
            // Assert
            Assert.Equal("Type1", firstIdType);
            Assert.Equal("Type2", secondIdType);
        }

        [Fact]
        public void Enumeration_ShouldBeCached()
        {
            // Arrange
            var message = HL7MessageBuilder.Create()
                .WithMSH()
                .WithSegment("PID|||ID001~ID002")
                .Build();
            var collection = new FieldRepetitionCollection(message, "PID", 3);
            
            // Act
            var accessor1 = collection[0];
            var accessor2 = collection[0];
            
            // Assert
            Assert.Same(accessor1, accessor2); // Should be the same cached instance
        }
    }
}