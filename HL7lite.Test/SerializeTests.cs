using HL7lite;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static HL7lite.MessageElement;

namespace HL7Lite.Test
{
    public class SerializeTests
    {
        private static string simpleMessage = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||Z99^Z01|12345|P|2.3||
ZZA|1
ZZB|2
";

        private static string clutteredMessage = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||Z99^Z01|12345|P|2.3||
ZZA|1||^^&~^^&~~||~~~^&&|||&&|^^
ZZB|2||^^^^^^^||~~~^&&||123^|^^&X~^^&~~|^Y^^|^||~~~^&&|||
";

        [Fact]
        public void PutStuffGetsSerialized()
        {
            var message = new Message(simpleMessage);
            message.ParseMessage();

            message.PutValue("ZZA.3.3", "123");
            message.PutValue("ZZB.3(3).3", "456");

            var serialized = message.SerializeMessage(true);

            message = new Message(serialized);
            message.ParseMessage();

            Assert.True(message.ValueExists("ZZA.3.3"));
            Assert.True(message.ValueExists("ZZB.3(3).3"));

            Assert.Equal("123", message.GetValue("ZZA.3.3"));
            Assert.Equal("456", message.GetValue("ZZB.3(3).3"));

        }

        [Fact]
        public void RemoveTrailingDelimiters()
        {
            var message = new Message(clutteredMessage);
            message.ParseMessage();

            message.RemoveTrailingDelimiters(RemoveDelimitersOptions.All);

            var serialized = message.SerializeMessage(true);

            var segmentList = serialized.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal("ZZA|1", segmentList[1]);
            Assert.Equal("ZZB|2||||||123|^^&X|^Y", segmentList[2]);
        }

    }
}
