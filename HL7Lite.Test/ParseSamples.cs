using System;
using System.IO;
using HL7lite;
using Xunit;

namespace HL7lite.Test
{
    public class ParseSamples
    {
        [Fact]
        public void SampleORMIsParsed()
        {
            var messageText = File.ReadAllText(SamplesPath.Value + "Sample-ORM-3149.txt");

            var message = new Message(messageText);

            var isParsed = message.ParseMessage();
            Assert.True(isParsed);
        }

        [Fact]
        public void SampleADTIsParsed()
        {
            var messageText = File.ReadAllText(SamplesPath.Value + "Sample-ADT-MSGID12349876.txt");

            var message = new Message(messageText);

            var isParsed = message.ParseMessage();
            Assert.True(isParsed);
        }

        [Fact]
        public void SampleADTA37IsParsed()
        {
            var messageText = File.ReadAllText(SamplesPath.Value + "Sample-ADT-9287901.txt");

            var message = new Message(messageText);

            var isParsed = message.ParseMessage();
            Assert.True(isParsed);
        }
    }
}
