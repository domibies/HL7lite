using System;
using HL7lite.Fluent;
using Xunit;

namespace HL7lite.Test.Fluent
{
    public class DeepCopyTests
    {
        [Fact]
        public void Copy_WithCompleteMessage_ShouldCreateIndependentCopy()
        {
            // Arrange
            var originalHl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ADT^A01|12345|P|2.3||
PID|1||12345^^^MRN||DOE^JOHN^M||19800101|M|||123 MAIN ST^^CITY^ST^12345||5551234567|||||||||||||||||
OBX|1|NM|WBC^WHITE BLOOD COUNT^L||7.5|10*3/uL|4.0-11.0|N|||F|||20231219145530.1234||";
            
            var original = originalHl7.ToFluentMessage();

            // Act
            var copy = original.Copy();

            // Assert - Basic independence
            Assert.NotNull(copy);
            Assert.NotSame(original, copy);
            Assert.NotSame(original.UnderlyingMessage, copy.UnderlyingMessage);
        }

        [Fact]
        public void Copy_ChangesToCopy_ShouldNotAffectOriginal()
        {
            // Arrange
            var originalHl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ADT^A01|12345|P|2.3||
PID|1||12345^^^MRN||DOE^JOHN^M||19800101|M|||123 MAIN ST^^CITY^ST^12345||5551234567|||||||||||||||||";
            
            var original = originalHl7.ToFluentMessage();
            var copy = original.Copy();

            // Act - Modify copy
            copy.PID[5].Set("SMITH^JANE^F");
            copy.PID[8].Set("F");

            // Assert - Original unchanged
            Assert.Equal("DOE^JOHN^M", original.PID[5].Value);
            Assert.Equal("M", original.PID[8].Value);
            
            // Assert - Copy changed
            Assert.Equal("SMITH^JANE^F", copy.PID[5].Value);
            Assert.Equal("F", copy.PID[8].Value);
        }

        [Fact]
        public void Copy_ChangesToOriginal_ShouldNotAffectCopy()
        {
            // Arrange
            var originalHl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ADT^A01|12345|P|2.3||
PID|1||12345^^^MRN||DOE^JOHN^M||19800101|M|||123 MAIN ST^^CITY^ST^12345||5551234567|||||||||||||||||";
            
            var original = originalHl7.ToFluentMessage();
            var copy = original.Copy();

            // Act - Modify original
            original.PID[5].Set("BROWN^ROBERT^L");
            original.PID[8].Set("F");

            // Assert - Copy unchanged  
            Assert.Equal("DOE^JOHN^M", copy.PID[5].Value);
            Assert.Equal("M", copy.PID[8].Value);
            
            // Assert - Original changed
            Assert.Equal("BROWN^ROBERT^L", original.PID[5].Value);
            Assert.Equal("F", original.PID[8].Value);
        }

        [Fact]
        public void Copy_WithMultipleSegments_ShouldCopyAllSegments()
        {
            // Arrange
            var originalHl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ORU^R01|12345|P|2.3||
PID|1||12345^^^MRN||DOE^JOHN^M||19800101|M|||123 MAIN ST^^CITY^ST^12345||5551234567|||||||||||||||||
OBR|1|ORDER123|RESULT456|CBC^COMPLETE BLOOD COUNT^L|||20210330110000|||||||||||||||||||F||
OBX|1|NM|WBC^WHITE BLOOD COUNT^L||7.5|10*3/uL|4.0-11.0|N|||F|||20231219145530.1234||
OBX|2|NM|RBC^RED BLOOD COUNT^L||4.2|10*6/uL|4.2-5.8|N|||F|||20231219145530.1234||";
            
            var original = originalHl7.ToFluentMessage();

            // Act
            var copy = original.Copy();

            // Assert - All segments exist
            Assert.True(copy.MSH.Exists);
            Assert.True(copy.PID.Exists);
            Assert.True(copy.OBR.Exists);
            Assert.True(copy.OBX.Exists);
            
            // Assert - Multiple OBX segments
            Assert.Equal(2, copy.Segments("OBX").Count);
            Assert.Equal("7.5", copy.Segments("OBX")[0][5].Value);
            Assert.Equal("4.2", copy.Segments("OBX")[1][5].Value);
        }

        [Fact]
        public void Copy_WithComplexFieldStructures_ShouldPreserveAllData()
        {
            // Arrange - Message with components, subcomponents, repetitions
            var originalHl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ADT^A01|12345|P|2.3||
PID|1||12345^^^MRN~67890^^^SSN||DOE^JOHN^M^JR~SMITH^J^M||19800101|M|||123 MAIN ST\S\APT 2^SUITE 100^CITY^ST^12345||5551234567~5559876543|||||||||||||||||";
            
            var original = originalHl7.ToFluentMessage();

            // Act
            var copy = original.Copy();

            // Assert - Repetitions are preserved (Value only shows first repetition)
            Assert.Equal("12345^^^MRN", copy.PID[3].Value); // First repetition via .Value
            Assert.Equal("12345^^^MRN", copy.PID[3].Repetition(1).Value); // First repetition explicit
            Assert.Equal("67890^^^SSN", copy.PID[3].Repetition(2).Value); // Second repetition
            Assert.Equal(2, copy.PID[3].Repetitions.Count); // Repetition count preserved
            
            Assert.Equal("DOE^JOHN^M^JR", copy.PID[5].Value); // First repetition
            Assert.Equal("SMITH^J^M", copy.PID[5].Repetition(2).Value); // Second repetition
            Assert.Equal("123 MAIN ST\\S\\APT 2^SUITE 100^CITY^ST^12345", copy.PID[11].Value);
            Assert.Equal("5551234567", copy.PID[13].Value); // First repetition
            Assert.Equal("5559876543", copy.PID[13].Repetition(2).Value); // Second repetition
            
            // Assert - Individual components work
            Assert.Equal("DOE", copy.PID[5].Component(1).Value);
            Assert.Equal("JOHN", copy.PID[5].Component(2).Value);
            Assert.Equal("SMITH", copy.PID[5].Repetition(2).Component(1).Value);
        }

        [Fact]
        public void Copy_WithEmptyMessage_ShouldCreateEmptyCopy()
        {
            // Arrange
            var original = new FluentMessage(new Message());

            // Act
            var copy = original.Copy();

            // Assert
            Assert.NotNull(copy);
            Assert.NotSame(original, copy);
            Assert.False(copy.MSH.Exists);
            Assert.False(copy.PID.Exists);
        }

        [Fact]
        public void Copy_WithMSHOnly_ShouldCopyMSHCorrectly()
        {
            // Arrange
            var originalHl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ADT^A01|12345|P|2.3||
";
            var original = originalHl7.ToFluentMessage();

            // Act
            var copy = original.Copy();

            // Assert
            Assert.True(copy.MSH.Exists);
            Assert.Equal("SENDER", copy.MSH[3].Value);
            Assert.Equal("SFACILITY", copy.MSH[4].Value);
            Assert.Equal("RECEIVER", copy.MSH[5].Value);
            Assert.Equal("RFACILITY", copy.MSH[6].Value);
            Assert.Equal("ADT^A01", copy.MSH[9].Value);
            Assert.Equal("12345", copy.MSH[10].Value);
            
            // MSH special fields (delimiters)
            Assert.Equal("|", copy.MSH[1].Value);
            Assert.Equal("^~\\&", copy.MSH[2].Value);
        }

        [Fact]
        public void Copy_WithProgrammaticallyBuiltMessage_ShouldWorkCorrectly()
        {
            // Arrange - Build message programmatically (tests the current DeepCopy limitation)
            var message = new Message();
            var original = new FluentMessage(message);
            
            original.CreateMSH
                .Sender("MyApp", "MyFac")
                .Receiver("TheirApp", "TheirFac")
                .MessageType("ADT^A01")
                .AutoControlId()
                .Build();
                
            original.Segments("PID").Add();
            original.PID[5].Set("DOE^JOHN^M");
            original.PID[8].Set("M");
            
            original.Segments("OBX").Add();
            original.OBX[1].Set("1");
            original.OBX[2].Set("NM");
            original.OBX[5].Set("120");

            // Act
            var copy = original.Copy();

            // Assert - All programmatically set data preserved
            Assert.True(copy.MSH.Exists);
            Assert.Equal("MyApp", copy.MSH[3].Value);
            Assert.Equal("MyFac", copy.MSH[4].Value);
            
            Assert.True(copy.PID.Exists);
            Assert.Equal("DOE^JOHN^M", copy.PID[5].Value);
            Assert.Equal("M", copy.PID[8].Value);
            
            Assert.True(copy.OBX.Exists);
            Assert.Equal("1", copy.OBX[1].Value);
            Assert.Equal("NM", copy.OBX[2].Value);
            Assert.Equal("120", copy.OBX[5].Value);
        }

        [Fact]
        public void Copy_AddingSegmentsToCopy_ShouldNotAffectOriginal()
        {
            // Arrange
            var originalHl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ADT^A01|12345|P|2.3||
PID|1||12345^^^MRN||DOE^JOHN^M||19800101|M|||123 MAIN ST^^CITY^ST^12345||5551234567|||||||||||||||||";
            
            var original = originalHl7.ToFluentMessage();
            var copy = original.Copy();

            // Act - Add segments to copy
            copy.Segments("OBX").Add();
            copy.OBX[1].Set("1");
            copy.OBX[2].Set("NM");
            copy.OBX[5].Set("150");

            // Assert - Copy has new segment
            Assert.True(copy.OBX.Exists);
            Assert.Equal("1", copy.OBX[1].Value);
            
            // Assert - Original doesn't have new segment
            Assert.False(original.OBX.Exists);
        }

        [Fact]
        public void Copy_WithSpecialCharacters_ShouldPreserveEncoding()
        {
            // Arrange
            var originalHl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ADT^A01|12345|P|2.3||
PID|1||12345^^^MRN||DOE^JOHN~JOHNNY^M||19800101|M|||123 MAIN ST\S\APARTMENT 2^^CITY^ST^12345||5551234567|||||||||||||||||";
            
            var original = originalHl7.ToFluentMessage();

            // Act
            var copy = original.Copy();

            // Assert - Special characters preserved (Value shows first repetition only)
            Assert.Equal("DOE^JOHN", copy.PID[5].Value); // First repetition only
            Assert.Contains("APARTMENT 2", copy.PID[11].Value);
            
            // Assert - Individual repetitions work correctly  
            Assert.Equal("DOE^JOHN", copy.PID[5].Repetition(1).Value); // First repetition
            Assert.Equal("JOHNNY^M", copy.PID[5].Repetition(2).Value); // Second repetition
            
            // Assert - Encoding delimiters work
            Assert.Equal("DOE", copy.PID[5].Component(1).Value);
            Assert.Equal("JOHN", copy.PID[5].Component(2).Value);
            Assert.Equal("JOHNNY", copy.PID[5].Repetition(2).Component(1).Value);
        }

        [Fact]
        public void Copy_WithDateTime_ShouldPreserveDateTimeParsing()
        {
            // Arrange
            var originalHl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ORU^R01|12345|P|2.3||
OBX|1|NM|WBC^WHITE BLOOD COUNT^L||7.5|10*3/uL|4.0-11.0|N|||F|||20231219145530.1234||";
            
            var original = originalHl7.ToFluentMessage();

            // Act
            var copy = original.Copy();

            // Assert - DateTime parsing works on copy
            var originalDateTime = original.OBX[14].AsDateTime();
            var copyDateTime = copy.OBX[14].AsDateTime();
            
            Assert.NotNull(originalDateTime);
            Assert.NotNull(copyDateTime);
            Assert.Equal(originalDateTime, copyDateTime);
            Assert.Equal(2023, copyDateTime.Value.Year);
            Assert.Equal(12, copyDateTime.Value.Month);
            Assert.Equal(19, copyDateTime.Value.Day);
        }

        [Fact]
        public void Copy_MultipleTimes_ShouldCreateIndependentCopies()
        {
            // Arrange
            var originalHl7 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||ADT^A01|12345|P|2.3||
PID|1||12345^^^MRN||DOE^JOHN^M||19800101|M|||123 MAIN ST^^CITY^ST^12345||5551234567|||||||||||||||||";
            
            var original = originalHl7.ToFluentMessage();

            // Act
            var copy1 = original.Copy();
            var copy2 = original.Copy();

            // Modify each copy differently
            copy1.PID[5].Set("SMITH^JANE^F");
            copy2.PID[5].Set("BROWN^ROBERT^L");

            // Assert - All copies are independent
            Assert.Equal("DOE^JOHN^M", original.PID[5].Value);
            Assert.Equal("SMITH^JANE^F", copy1.PID[5].Value);
            Assert.Equal("BROWN^ROBERT^L", copy2.PID[5].Value);
            
            // Assert - Copies are independent of each other
            Assert.NotSame(copy1, copy2);
            Assert.NotSame(copy1.UnderlyingMessage, copy2.UnderlyingMessage);
        }

        [Fact]
        public void Copy_LargeMessageWithManySegments_ShouldCopyAll()
        {
            // Arrange - Create message with multiple segment types
            var message = new Message();
            var original = new FluentMessage(message);
            
            original.CreateMSH
                .Sender("App", "Fac")
                .Receiver("App2", "Fac2")
                .MessageType("ORU^R01")
                .AutoControlId()
                .Build();
                
            // Add multiple segments of different types
            for (int i = 1; i <= 5; i++)
            {
                original.Segments("OBX").Add();
                original.Segments("OBX")[i-1][1].Set(i.ToString());
                original.Segments("OBX")[i-1][2].Set("NM");
                original.Segments("OBX")[i-1][5].Set((100 + i).ToString());
            }

            // Act
            var copy = original.Copy();

            // Assert - All segments copied
            Assert.Equal(5, copy.Segments("OBX").Count);
            for (int i = 1; i <= 5; i++)
            {
                Assert.Equal(i.ToString(), copy.Segments("OBX")[i-1][1].Value);
                Assert.Equal("NM", copy.Segments("OBX")[i-1][2].Value);
                Assert.Equal((100 + i).ToString(), copy.Segments("OBX")[i-1][5].Value);
            }
        }
    }
}