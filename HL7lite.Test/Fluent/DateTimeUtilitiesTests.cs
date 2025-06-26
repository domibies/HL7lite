using System;
using HL7lite.Fluent;
using Xunit;

namespace HL7lite.Test.Fluent
{
    public class DateTimeUtilitiesTests
    {
        [Fact]
        public void FieldMutator_DateTime_ShouldSetHL7FormattedDateTime()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);
            fluent.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ORU^R01")
                .AutoControlId()
                .Build();
            
            var testDateTime = new DateTime(2023, 12, 19, 14, 55, 30, 123);

            // Act
            fluent.Segments("OBX").Add();
            fluent.OBX[14].SetDateTime(testDateTime);  // Using shortcut

            // Assert
            var expectedFormat = MessageHelper.LongDateWithFractionOfSecond(testDateTime);
            Assert.Equal(expectedFormat, fluent.OBX[14].Value);
            Assert.StartsWith("20231219145530", fluent.OBX[14].Value);
        }

        [Fact]
        public void FieldMutator_DateTimeNow_ShouldSetCurrentDateTime()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);
            fluent.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ORU^R01")
                .AutoControlId()
                .Build();
            
            var beforeTime = DateTime.Now.AddSeconds(-1);

            // Act
            fluent.Segments("OBX").Add();
            fluent.OBX[14].Set().SetDateTime(DateTime.Now);

            var afterTime = DateTime.Now.AddSeconds(1);

            // Assert
            var parsedDateTime = fluent.OBX[14].AsDateTime();
            Assert.NotNull(parsedDateTime);
            Assert.True(parsedDateTime >= beforeTime, "DateTime should be after the before time");
            Assert.True(parsedDateTime <= afterTime, "DateTime should be before the after time");
        }

        [Fact]
        public void FieldMutator_Date_ShouldSetHL7FormattedDateOnly()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);
            fluent.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ADT^A01")
                .AutoControlId()
                .Build();
            
            var testDate = new DateTime(2023, 12, 19, 14, 55, 30); // Time will be ignored

            // Act
            fluent.Segments("PID").Add();
            fluent.PID[7].SetDate(testDate); // Using shortcut for date of birth

            // Assert
            Assert.Equal("20231219", fluent.PID[7].Value);
        }

        [Fact]
        public void FieldMutator_DateToday_ShouldSetTodaysDate()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);
            fluent.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ADT^A01")
                .AutoControlId()
                .Build();
            
            var expectedDate = DateTime.Today.ToString("yyyyMMdd");

            // Act
            fluent.Segments("PID").Add();
            fluent.PID[7].Set().SetDate(DateTime.Today);

            // Assert
            Assert.Equal(expectedDate, fluent.PID[7].Value);
        }

        [Fact]
        public void FieldAccessor_AsDateTime_WithValidHL7DateTime_ShouldParseCorrectly()
        {
            // Arrange - Create OBX with proper field count to reach field 14
            var hl7String = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ORU^R01|12345|P|2.3||
OBX|1|NM|WBC^WHITE BLOOD COUNT^L||7.5|10*3/uL|4.0-11.0|N|||F|||20231219145530.1234||";
            
            var fluent = hl7String.ToFluentMessage();

            // Act
            var parsedDateTime = fluent.OBX[14].AsDateTime();

            // Assert
            Assert.NotNull(parsedDateTime);
            Assert.Equal(2023, parsedDateTime.Value.Year);
            Assert.Equal(12, parsedDateTime.Value.Month);
            Assert.Equal(19, parsedDateTime.Value.Day);
            Assert.Equal(14, parsedDateTime.Value.Hour);
            Assert.Equal(55, parsedDateTime.Value.Minute);
            Assert.Equal(30, parsedDateTime.Value.Second);
            Assert.Equal(123, parsedDateTime.Value.Millisecond); // .1234 -> 123ms
        }

        [Fact]
        public void FieldAccessor_AsDateTime_WithTimezone_ShouldParseWithOffset()
        {
            // Arrange
            var hl7String = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ORU^R01|12345|P|2.3||
OBX|1|NM|WBC^WHITE BLOOD COUNT^L||7.5|10*3/uL|4.0-11.0|N|||F|||20231219145530.1234-0500||";
            
            var fluent = hl7String.ToFluentMessage();

            // Act
            TimeSpan offset;
            var parsedDateTime = fluent.OBX[14].AsDateTime(out offset);

            // Assert
            Assert.NotNull(parsedDateTime);
            Assert.Equal(2023, parsedDateTime.Value.Year);
            Assert.Equal(12, parsedDateTime.Value.Month);
            Assert.Equal(19, parsedDateTime.Value.Day);
            Assert.Equal(-5, offset.Hours);
            Assert.Equal(0, offset.Minutes);
        }

        [Fact]
        public void FieldAccessor_AsDateTime_WithDateOnly_ShouldParseCorrectly()
        {
            // Arrange
            var hl7String = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ADT^A01|12345|P|2.3||
PID|1||12345^^^MRN||DOE^JOHN^M||20231219|M|||123 MAIN ST^^CITY^ST^12345||5551234567|||||||||||||||||";
            
            var fluent = hl7String.ToFluentMessage();

            // Act
            var parsedDateTime = fluent.PID[7].AsDateTime(); // Date of birth

            // Assert
            Assert.NotNull(parsedDateTime);
            Assert.Equal(2023, parsedDateTime.Value.Year);
            Assert.Equal(12, parsedDateTime.Value.Month);
            Assert.Equal(19, parsedDateTime.Value.Day);
            Assert.Equal(0, parsedDateTime.Value.Hour); // Should default to midnight
            Assert.Equal(0, parsedDateTime.Value.Minute);
            Assert.Equal(0, parsedDateTime.Value.Second);
        }

        [Fact]
        public void FieldAccessor_AsDate_ShouldReturnDatePortionOnly()
        {
            // Arrange
            var hl7String = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ORU^R01|12345|P|2.3||
OBX|1|NM|WBC^WHITE BLOOD COUNT^L||7.5|10*3/uL|4.0-11.0|N|||F|||20231219145530.1234||";
            
            var fluent = hl7String.ToFluentMessage();

            // Act
            var parsedDate = fluent.OBX[14].AsDate();

            // Assert
            Assert.NotNull(parsedDate);
            Assert.Equal(2023, parsedDate.Value.Year);
            Assert.Equal(12, parsedDate.Value.Month);
            Assert.Equal(19, parsedDate.Value.Day);
            Assert.Equal(0, parsedDate.Value.Hour); // Time should be stripped to midnight
            Assert.Equal(0, parsedDate.Value.Minute);
            Assert.Equal(0, parsedDate.Value.Second);
            Assert.Equal(0, parsedDate.Value.Millisecond);
        }

        [Fact]
        public void FieldAccessor_AsDateTime_WithEmptyField_ShouldReturnNull()
        {
            // Arrange
            var hl7String = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ORU^R01|12345|P|2.3||
OBX|1|NM|WBC^WHITE BLOOD COUNT^L||7.5|10*3/uL|4.0-11.0|N|||F||||";
            
            var fluent = hl7String.ToFluentMessage();

            // Act
            var parsedDateTime = fluent.OBX[14].AsDateTime();

            // Assert
            Assert.Null(parsedDateTime);
        }

        [Fact]
        public void FieldAccessor_AsDateTime_WithNonExistentField_ShouldReturnNull()
        {
            // Arrange
            var hl7String = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ORU^R01|12345|P|2.3||
OBX|1|NM|WBC^WHITE BLOOD COUNT^L||7.5|10*3/uL|4.0-11.0|N|||F||";
            
            var fluent = hl7String.ToFluentMessage();

            // Act
            var parsedDateTime = fluent.OBX[20].AsDateTime(); // Field doesn't exist

            // Assert
            Assert.Null(parsedDateTime);
        }

        [Fact]
        public void FieldAccessor_AsDateTime_WithInvalidFormat_ShouldReturnNull()
        {
            // Arrange
            var hl7String = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ORU^R01|12345|P|2.3||
OBX|1|NM|WBC^WHITE BLOOD COUNT^L||7.5|10*3/uL|4.0-11.0|N|||F|||NOT_A_DATE||";
            
            var fluent = hl7String.ToFluentMessage();

            // Act
            var parsedDateTime = fluent.OBX[14].AsDateTime();

            // Assert
            Assert.Null(parsedDateTime);
        }

        [Fact]
        public void FieldAccessor_AsDateTime_WithInvalidFormatAndThrowOnError_ShouldThrowException()
        {
            // Arrange
            var hl7String = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ORU^R01|12345|P|2.3||
OBX|1|NM|WBC^WHITE BLOOD COUNT^L||7.5|10*3/uL|4.0-11.0|N|||F|||NOT_A_DATE||";
            
            var fluent = hl7String.ToFluentMessage();

            // Act & Assert
            Assert.Throws<FormatException>(() => fluent.OBX[14].AsDateTime(throwOnError: true));
        }

        [Fact]
        public void FieldMutator_DateTime_ChainedWithOtherMethods_ShouldWork()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);
            fluent.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ORU^R01")
                .AutoControlId()
                .Build();
            
            var testDateTime = new DateTime(2023, 12, 19, 14, 55, 30);

            // Act - Chain DateTime with other operations
            fluent.Segments("OBX").Add();
            fluent.OBX[1].Set("1");
            fluent.OBX[2].Set("NM");
            fluent.OBX[3].Set("GLUCOSE");
            fluent.OBX[14].Set().SetDateTime(testDateTime);

            // Assert
            Assert.Equal("1", fluent.OBX[1].Value);
            Assert.Equal("NM", fluent.OBX[2].Value);
            Assert.Equal("GLUCOSE", fluent.OBX[3].Value);
            var expectedDateTime = MessageHelper.LongDateWithFractionOfSecond(testDateTime);
            Assert.Equal(expectedDateTime, fluent.OBX[14].Value);
        }

        [Fact]
        public void FieldAccessor_AsDateTime_VariousHL7Formats_ShouldParseCorrectly()
        {
            // Arrange
            var message = new Message();
            var fluent = new FluentMessage(message);
            fluent.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ORU^R01")
                .AutoControlId()
                .Build();

            // Add multiple OBX segments with different datetime formats
            fluent.Segments("OBX").Add(); // Full format
            fluent.OBX[14].Set("20231219145530.1234");
            
            fluent.Segments("OBX").Add(); // Date only  
            fluent.Segments("OBX")[1][14].Set("20231219");
            
            fluent.Segments("OBX").Add(); // Year-month only
            fluent.Segments("OBX")[2][14].Set("202312");

            // Act & Assert
            var fullDateTime = fluent.Segments("OBX")[0][14].AsDateTime();
            Assert.NotNull(fullDateTime);
            Assert.Equal(2023, fullDateTime.Value.Year);
            Assert.Equal(12, fullDateTime.Value.Month);
            Assert.Equal(19, fullDateTime.Value.Day);
            Assert.Equal(14, fullDateTime.Value.Hour);

            var dateOnly = fluent.Segments("OBX")[1][14].AsDateTime();
            Assert.NotNull(dateOnly);
            Assert.Equal(2023, dateOnly.Value.Year);
            Assert.Equal(12, dateOnly.Value.Month);
            Assert.Equal(19, dateOnly.Value.Day);
            Assert.Equal(0, dateOnly.Value.Hour); // Defaults to midnight

            var yearMonth = fluent.Segments("OBX")[2][14].AsDateTime();
            Assert.NotNull(yearMonth);
            Assert.Equal(2023, yearMonth.Value.Year);
            Assert.Equal(12, yearMonth.Value.Month);
            Assert.Equal(1, yearMonth.Value.Day); // Defaults to first day of month
        }
    }
}