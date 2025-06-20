using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HL7lite.Fluent;
using Xunit;

namespace HL7lite.Test.Fluent
{
    public class SerializationBuilderTests : IDisposable
    {
        private readonly string _testMessage = @"MSH|^~\&|SENDING|FACILITY|RECEIVING|FACILITY|20200101120000||ADT^A01|12345|P|2.5||
PID|1||123456^^^MRN||Doe^John^M||19800101|M|||123 Main St^^City^ST^12345||5551234567||||||||||||||||||||||||||";
        
        private readonly List<string> _tempFiles = new List<string>();

        public void Dispose()
        {
            // Clean up any temp files created during tests
            foreach (var file in _tempFiles)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }

        private string CreateTempFile()
        {
            var tempFile = Path.GetTempFileName();
            _tempFiles.Add(tempFile);
            return tempFile;
        }

        [Fact]
        public void Serialize_ToString_ReturnsSerializedMessage()
        {
            // Arrange
            var message = new Message(_testMessage);
            message.ParseMessage();
            var fluent = new FluentMessage(message);

            // Act
            var serialized = fluent.Serialize().ToString();

            // Assert
            Assert.NotNull(serialized);
            Assert.Contains("MSH|^~\\&|SENDING|FACILITY", serialized);
            Assert.Contains("PID|1||123456^^^MRN", serialized);
        }

        [Fact]
        public void Serialize_WithValidation_ValidatesBeforeSerialization()
        {
            // Arrange
            var message = new Message(_testMessage);
            message.ParseMessage();
            var fluent = new FluentMessage(message);

            // Act
            var serialized = fluent.Serialize()
                .WithValidation()
                .ToString();

            // Assert
            Assert.NotNull(serialized);
            Assert.Contains("MSH|^~\\&|SENDING|FACILITY", serialized);
        }

        [Fact]
        public void Serialize_WithoutTrailingDelimiters_RemovesDelimiters()
        {
            // Arrange
            var message = new Message(_testMessage);
            message.ParseMessage();
            var fluent = new FluentMessage(message);
            
            // Add some empty fields to create trailing delimiters
            fluent.PID[20].Set().Value("");
            fluent.PID[21].Set().Value("");

            // Act
            var serialized = fluent.Serialize()
                .WithoutTrailingDelimiters()
                .ToString();

            // Assert
            Assert.NotNull(serialized);
            // The PID segment should not end with excessive trailing pipes
            var lines = serialized.Replace("\r\n", "\n").Split('\n');
            var pidLine = lines.FirstOrDefault(l => l.StartsWith("PID"));
            Assert.NotNull(pidLine);
            Assert.DoesNotContain("||||||||||||||||||||||", pidLine);
        }

        [Fact]
        public void Serialize_ToFile_WritesMessageToFile()
        {
            // Arrange
            var message = new Message(_testMessage);
            message.ParseMessage();
            var fluent = new FluentMessage(message);
            var tempFile = CreateTempFile();

            // Act
            var result = fluent.Serialize()
                .WithValidation()
                .WithoutTrailingDelimiters()
                .ToFile(tempFile);

            // Assert
            Assert.Same(fluent, result); // Should return FluentMessage for chaining
            Assert.True(File.Exists(tempFile));
            
            var content = File.ReadAllText(tempFile);
            Assert.Contains("MSH|^~\\&|SENDING|FACILITY", content);
            Assert.Contains("PID|1||123456^^^MRN", content);
        }

        [Fact]
        public void Serialize_ToStream_WritesMessageToStream()
        {
            // Arrange
            var message = new Message(_testMessage);
            message.ParseMessage();
            var fluent = new FluentMessage(message);
            
            using (var stream = new MemoryStream())
            {
                // Act
                var result = fluent.Serialize()
                    .WithEncoding(Encoding.UTF8)
                    .ToStream(stream);

                // Assert
                Assert.Same(fluent, result); // Should return FluentMessage for chaining
                
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    var content = reader.ReadToEnd();
                    Assert.Contains("MSH|^~\\&|SENDING|FACILITY", content);
                    Assert.Contains("PID|1||123456^^^MRN", content);
                }
            }
        }

        [Fact]
        public void Serialize_ToBytes_ReturnsByteArray()
        {
            // Arrange
            var message = new Message(_testMessage);
            message.ParseMessage();
            var fluent = new FluentMessage(message);

            // Act
            var bytes = fluent.Serialize()
                .WithEncoding(Encoding.UTF8)
                .ToBytes();

            // Assert
            Assert.NotNull(bytes);
            Assert.True(bytes.Length > 0);
            
            var content = Encoding.UTF8.GetString(bytes);
            Assert.Contains("MSH|^~\\&|SENDING|FACILITY", content);
            Assert.Contains("PID|1||123456^^^MRN", content);
        }

        [Fact]
        public void Serialize_TrySerialize_WithValidMessage_ReturnsTrue()
        {
            // Arrange
            var message = new Message(_testMessage);
            message.ParseMessage();
            var fluent = new FluentMessage(message);

            // Act
            string serialized;
            string error;
            var success = fluent.Serialize()
                .TrySerialize(out serialized, out error);

            // Assert
            Assert.True(success);
            Assert.NotNull(serialized);
            Assert.Null(error);
            Assert.Contains("MSH|^~\\&|SENDING|FACILITY", serialized);
        }

        [Fact]
        public void Serialize_ToStream_WithNullStream_ThrowsException()
        {
            // Arrange
            var message = new Message(_testMessage);
            message.ParseMessage();
            var fluent = new FluentMessage(message);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                fluent.Serialize().ToStream(null));
        }

        [Fact]
        public void Serialize_ChainedOperations_WorkCorrectly()
        {
            // Arrange
            var message = new Message(_testMessage);
            message.ParseMessage();
            var fluent = new FluentMessage(message);
            var tempFile = CreateTempFile();

            // Act - Chain multiple operations  
            fluent.PID[5].Set().Components("Smith", "Jane", "Marie");
            fluent.PID[7].Set().Value("19901231");
            fluent
                  .Serialize()
                      .WithValidation()
                      .WithoutTrailingDelimiters()
                      .ToFile(tempFile);

            // Assert
            var content = File.ReadAllText(tempFile);
            Assert.Contains("Smith^Jane^Marie", content);
            Assert.Contains("19901231", content);
            Assert.DoesNotContain("Doe^John^M", content);
        }

        [Fact]
        public void Serialize_WithDifferentEncodings_ProducesCorrectBytes()
        {
            // Arrange
            var message = new Message(_testMessage);
            message.ParseMessage();
            var fluent = new FluentMessage(message);

            // Act
            var utf8Bytes = fluent.Serialize()
                .WithEncoding(Encoding.UTF8)
                .ToBytes();

            var asciiBytes = fluent.Serialize()
                .WithEncoding(Encoding.ASCII)
                .ToBytes();

            // Assert
            Assert.NotNull(utf8Bytes);
            Assert.NotNull(asciiBytes);
            
            // For basic ASCII content, lengths should be the same
            Assert.Equal(utf8Bytes.Length, asciiBytes.Length);
        }

        [Fact]
        public void Serialize_WithoutTrailingDelimiters_SpecificOptions()
        {
            // Arrange
            var message = new Message(_testMessage);
            message.ParseMessage();
            var fluent = new FluentMessage(message);
            
            // Add some empty fields
            fluent.PID[20].Set().Value("");

            // Act
            var serializedAll = fluent.Serialize()
                .WithoutTrailingDelimiters(MessageElement.RemoveDelimitersOptions.All)
                .ToString();

            var serializedFields = fluent.Serialize()
                .WithoutTrailingDelimiters(new MessageElement.RemoveDelimitersOptions { Fields = true })
                .ToString();

            // Assert
            Assert.NotNull(serializedAll);
            Assert.NotNull(serializedFields);
            // Both should have removed trailing delimiters
            var pidLineAll = serializedAll.Replace("\r\n", "\n").Split('\n').FirstOrDefault(l => l.StartsWith("PID"));
            var pidLineFields = serializedFields.Replace("\r\n", "\n").Split('\n').FirstOrDefault(l => l.StartsWith("PID"));
            Assert.DoesNotContain("||||||||||||||||||||||", pidLineAll);
            Assert.DoesNotContain("||||||||||||||||||||||", pidLineFields);
        }
    }
}