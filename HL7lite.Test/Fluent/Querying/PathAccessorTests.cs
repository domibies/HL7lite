using System;
using HL7lite.Fluent;
using HL7lite.Fluent.Querying;
using Xunit;

namespace HL7lite.Test.Fluent.Querying
{
    public class PathAccessorTests
    {
        private const string SampleMessage = @"MSH|^~\&|SendingApp|SendingFacility|ReceivingApp|ReceivingFacility|20230101120000||ADT^A01|12345|P|2.5||
PID|1||12345^^^MRN~67890^^^SSN||Doe^John^M^Jr||19800101|M|||123 Main St^^City^ST^12345||5551234567~5559876543|||||||||||||||||
PV1||O|NWSLED^^^NYULHLI^^^^^LI NW SLEEP DISORDER^^DEPID||||1447312459^DOE^MICHAEL^^^^^^EPIC^^^^PNPI~DOEM06^DOE^MICHAEL^^^^^^KID^^^^KID|1447312459^DOE^MICHAEL^^^^^^EPIC^^^^PNPI~DOEM06^DOE^MICHAEL^^^^^^KID^^^^KID|||||||||||496779945|||||||||||||||||||||||||20191107|||||||V";

        private FluentMessage CreateTestMessage()
        {
            var message = new Message(SampleMessage);
            message.ParseMessage();
            return new FluentMessage(message);
        }

        [Fact]
        public void Path_WithValidFieldPath_ReturnsCorrectValue()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act
            var result = fluent.Path("PID.5.1").Value;

            // Assert
            Assert.Equal("Doe", result);
        }

        [Fact]
        public void Path_WithValidComponentPath_ReturnsCorrectValue()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act
            var familyName = fluent.Path("PID.5.1").Value;
            var givenName = fluent.Path("PID.5.2").Value;
            var middleName = fluent.Path("PID.5.3").Value;

            // Assert
            Assert.Equal("Doe", familyName);
            Assert.Equal("John", givenName);
            Assert.Equal("M", middleName);
        }

        [Fact]
        public void Path_WithValidFieldOnlyPath_ReturnsCompleteFieldValue()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act
            var fullName = fluent.Path("PID.5").Value;

            // Assert
            Assert.Equal("Doe^John^M^Jr", fullName);
        }

        [Fact]
        public void Path_WithRepetitionSyntax_ReturnsCorrectValue()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act
            var firstId = fluent.Path("PID.3[1]").Value;
            var secondId = fluent.Path("PID.3[2]").Value;

            // Assert
            Assert.Equal("12345^^^MRN", firstId);
            Assert.Equal("67890^^^SSN", secondId);
        }

        [Fact]
        public void Path_WithRepetitionAndComponent_ReturnsCorrectValue()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act
            var firstPhone = fluent.Path("PID.13[1]").Value;
            var secondPhone = fluent.Path("PID.13[2]").Value;

            // Assert
            Assert.Equal("5551234567", firstPhone);
            Assert.Equal("5559876543", secondPhone);
        }

        [Fact]
        public void Path_WithComplexRepetitionPath_ReturnsCorrectValue()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act
            var doctorId = fluent.Path("PV1.7[1].1").Value;
            var doctorName = fluent.Path("PV1.7[2].1").Value;

            // Assert
            Assert.Equal("1447312459", doctorId);
            Assert.Equal("DOEM06", doctorName);
        }

        [Fact]
        public void Path_WithNonExistentPath_ReturnsEmptyString()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act
            var result = fluent.Path("ZZZ.999").Value;

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void Path_WithNonExistentField_ReturnsEmptyString()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act
            var result = fluent.Path("PID.99").Value;

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void Exists_WithValidPath_ReturnsTrue()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act & Assert
            Assert.True(fluent.Path("PID.5.1").Exists);
            Assert.True(fluent.Path("PID.5").Exists);
            Assert.True(fluent.Path("MSH.9").Exists);
        }

        [Fact]
        public void Exists_WithNonExistentPath_ReturnsFalse()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act & Assert
            Assert.False(fluent.Path("ZZZ.999").Exists);
            Assert.False(fluent.Path("PID.99").Exists);
            Assert.False(fluent.Path("PID.5.99").Exists);
        }

        [Fact]
        public void HasValue_WithNonEmptyField_ReturnsTrue()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act & Assert
            Assert.True(fluent.Path("PID.5.1").HasValue);
            Assert.True(fluent.Path("MSH.9").HasValue);
        }

        [Fact]
        public void HasValue_WithEmptyField_ReturnsFalse()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act & Assert
            Assert.False(fluent.Path("PID.99").HasValue);
            Assert.False(fluent.Path("ZZZ.999").HasValue);
        }

        [Fact]
        public void Set_WithValidPath_UpdatesValue()
        {
            // Arrange
            var fluent = CreateTestMessage();
            var originalValue = fluent.Path("PID.5.1").Value;
            Assert.Equal("Doe", originalValue);

            // Act
            fluent.Path("PID.5.1").Set("Smith");

            // Assert
            Assert.Equal("Smith", fluent.Path("PID.5.1").Value);
        }

        [Fact]
        public void Set_ReturnsFluentMessage_AllowsChaining()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act
            var result = fluent.Path("PID.5.1").Set("Smith")
                              .Path("PID.5.2").Set("Jane");

            // Assert
            Assert.Same(fluent, result);
            Assert.Equal("Smith", fluent.Path("PID.5.1").Value);
            Assert.Equal("Jane", fluent.Path("PID.5.2").Value);
        }

        [Fact]
        public void Put_WithNewPath_CreatesElement()
        {
            // Arrange
            var fluent = CreateTestMessage();
            Assert.False(fluent.Path("PID.39").Exists);

            // Act
            fluent.Path("PID.39").Put("NewValue");

            // Assert
            Assert.True(fluent.Path("PID.39").Exists);
            Assert.Equal("NewValue", fluent.Path("PID.39").Value);
        }

        [Fact]
        public void Put_WithExistingPath_UpdatesValue()
        {
            // Arrange
            var fluent = CreateTestMessage();
            var originalValue = fluent.Path("PID.5.1").Value;
            Assert.Equal("Doe", originalValue);

            // Act
            fluent.Path("PID.5.1").Put("UpdatedValue");

            // Assert
            Assert.Equal("UpdatedValue", fluent.Path("PID.5.1").Value);
        }

        [Fact]
        public void SetIf_WithTrueCondition_UpdatesValue()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act
            fluent.Path("PID.5.1").SetIf("ConditionalValue", true);

            // Assert
            Assert.Equal("ConditionalValue", fluent.Path("PID.5.1").Value);
        }

        [Fact]
        public void SetIf_WithFalseCondition_DoesNotUpdateValue()
        {
            // Arrange
            var fluent = CreateTestMessage();
            var originalValue = fluent.Path("PID.5.1").Value;

            // Act
            fluent.Path("PID.5.1").SetIf("ConditionalValue", false);

            // Assert
            Assert.Equal(originalValue, fluent.Path("PID.5.1").Value);
        }

        [Fact]
        public void PutIf_WithTrueCondition_CreatesElement()
        {
            // Arrange
            var fluent = CreateTestMessage();
            Assert.False(fluent.Path("PID.40").Exists);

            // Act
            fluent.Path("PID.40").PutIf("ConditionalValue", true);

            // Assert
            Assert.True(fluent.Path("PID.40").Exists);
            Assert.Equal("ConditionalValue", fluent.Path("PID.40").Value);
        }

        [Fact]
        public void PutIf_WithFalseCondition_DoesNotCreateElement()
        {
            // Arrange
            var fluent = CreateTestMessage();
            Assert.False(fluent.Path("PID.41").Exists);

            // Act
            fluent.Path("PID.41").PutIf("ConditionalValue", false);

            // Assert
            Assert.False(fluent.Path("PID.41").Exists);
        }

        [Fact]
        public void MultiplePathOperations_CanBeChained()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act
            fluent.Path("PID.5.1").Set("Smith")
                  .Path("PID.5.2").Set("Jane")
                  .Path("PID.7").Put("19900315")
                  .Path("PID.43").Put("CustomValue");

            // Assert
            Assert.Equal("Smith", fluent.Path("PID.5.1").Value);
            Assert.Equal("Jane", fluent.Path("PID.5.2").Value);
            Assert.Equal("19900315", fluent.Path("PID.7").Value);
            Assert.Equal("CustomValue", fluent.Path("PID.43").Value);
        }

        [Fact]
        public void Path_WithNullPath_ThrowsArgumentNullException()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => fluent.Path(null));
        }

        [Fact]
        public void ToString_ReturnsPathString()
        {
            // Arrange
            var fluent = CreateTestMessage();
            var pathAccessor = fluent.Path("PID.5.1");

            // Act
            var result = pathAccessor.ToString();

            // Assert
            Assert.Equal("PID.5.1", result);
        }

        [Fact]
        public void Path_WithComplexMessage_HandlesAllSyntaxVariations()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act & Assert - Basic field access
            Assert.Equal("ADT^A01", fluent.Path("MSH.9").Value);
            
            // Act & Assert - Component access
            Assert.Equal("ADT", fluent.Path("MSH.9.1").Value);
            Assert.Equal("A01", fluent.Path("MSH.9.2").Value);
            
            // Act & Assert - Field with repetitions
            Assert.Equal("12345^^^MRN", fluent.Path("PID.3[1]").Value);
            Assert.Equal("67890^^^SSN", fluent.Path("PID.3[2]").Value);
            
            // Act & Assert - Component within repetition  
            Assert.Equal("12345", fluent.Path("PID.3[1].1").Value);
            Assert.Equal("67890", fluent.Path("PID.3[2].1").Value);
        }

        [Fact]
        public void SetNull_SetsHL7NullValue()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act
            fluent.Path("PID.5.1").SetNull();

            // Assert
            Assert.True(fluent.Path("PID.5.1").IsNull);
        }

        [Fact]
        public void PutNull_CreatesHL7NullValue()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act
            fluent.Path("PID.42").PutNull();

            // Assert
            Assert.True(fluent.Path("PID.42").Exists);
            Assert.True(fluent.Path("PID.42").IsNull);
        }

        [Fact]
        public void IsNull_WithHL7NullValue_ReturnsTrue()
        {
            // Arrange
            var fluent = CreateTestMessage();
            fluent.Path("PID.5.1").SetNull();

            // Act & Assert
            Assert.True(fluent.Path("PID.5.1").IsNull);
        }

        [Fact]
        public void IsNull_WithNormalValue_ReturnsFalse()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act & Assert
            Assert.False(fluent.Path("PID.5.1").IsNull);
        }

        [Fact]
        public void IsNull_WithNonExistentPath_ReturnsFalse()
        {
            // Arrange
            var fluent = CreateTestMessage();

            // Act & Assert
            Assert.False(fluent.Path("ZZZ.999").IsNull);
        }
    }
}