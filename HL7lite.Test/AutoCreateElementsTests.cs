using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using HL7lite;
using Xunit;

namespace HL7Lite.Test
{
    public class AutoCreateElementsTests
    {
        private string msg1 = @"MSH|^~\&|SENDAPP||||||ZZZ^Z01|123|P|2.2||||||||
ZZ1|1|A|2^1";

        [Fact]
        public void EnsureField()
        {
            Message message = new Message(msg1);
            message.ParseMessage();

            message.DefaultSegment("ZZ1").EnsureField(5).Value = "X";

            Assert.Equal("X", message.GetValue("ZZ1.5"));
        }

        [Fact]
        public void EnsureFieldWithRepetition()
        {
            Message message = new Message(msg1);
            message.ParseMessage();

            message.DefaultSegment("ZZ1").EnsureField(5).Value = "X";
            message.DefaultSegment("ZZ1").EnsureField(5, 2).Value = "Y";

            Assert.Equal("X", message.GetValue("ZZ1.5(1)"));
            Assert.Equal("Y", message.GetValue("ZZ1.5(2)"));
        }

        [Fact]
        public void EnsureComponent()
        {
            Message message = new Message(msg1);
            message.ParseMessage();

            message.DefaultSegment("ZZ1").Fields(2).EnsureComponent(2).Value = "B";

            Assert.Equal("B", message.GetValue("ZZ1.2.2"));
        }
        [Fact]
        public void EnsureSubComponent()
        {
            Message message = new Message(msg1);
            message.ParseMessage();

            message.DefaultSegment("ZZ1").Fields(3).Components(2).EnsureSubComponent(2).Value = "XY";

            Assert.Equal("1", message.GetValue("ZZ1.3.2.1"));
            Assert.Equal("XY", message.GetValue("ZZ1.3.2.2"));
        }

        [Fact]
        public void PutValueField()
        {
            Message message = new Message(msg1);
            message.ParseMessage();

            message.PutValue("ZZ1.5", "XYZ");

            Assert.Equal("XYZ", message.GetValue("ZZ1.5"));
        }

        [Fact]
        public void PutValueFieldComponent()
        {
            Message message = new Message(msg1);
            message.ParseMessage();


            message.PutValue("ZZ1.5.4", "AA");

            Assert.Equal("AA", message.GetValue("ZZ1.5.4"));
        }

        [Fact]
        public void PutValueFieldAndComponent()
        {
            Message message = new Message(msg1);
            message.ParseMessage();

            message.PutValue("ZZ1.5", "XYZ");
            message.PutValue("ZZ1.5.4", "AA");

            Assert.Equal("XYZ", message.GetValue("ZZ1.5.1"));
            Assert.Equal("AA", message.GetValue("ZZ1.5.4"));
        }

        [Fact]
        public void PutValueFieldRepetition()
        {
            Message message = new Message(msg1);
            message.ParseMessage();


            message.PutValue("ZZ1.5(2).4", "AA");

            Assert.Equal("AA", message.GetValue("ZZ1.5(2).4"));
        }

        [Fact]
        public void ExistsBeforeAndAfter()
        {
            Message message = new Message(msg1);
            message.ParseMessage();

            Assert.False(message.ValueExists("ZZ1.10(2).10"));

            message.PutValue("ZZ1.10(2).10", "HUPSAKEE");

            Assert.True(message.ValueExists("ZZ1.10(2).10"));
        }

        /*
        [Fact]
        public void SomeTest()
        {
            var messageString = @"MSH|^~\&|SENDAPP||||||ZZZ^Z01|123|P|2.2||||||||
ZZ1|1|ID1|abc\R\^def";

            var message = new Message(messageString);
            message.ParseMessage();

            Debug.WriteLine(message.GetValue("ZZ1.3"));
            Debug.WriteLine(message.GetValue("ZZ1.3.1"));

            message.SetValue("ZZ1.3", @"def\R\^abc");

            Debug.WriteLine(message.GetValue("ZZ1.3"));
            Debug.WriteLine(message.GetValue("ZZ1.3.1"));
            Debug.WriteLine(message.DefaultSegment("ZZ1").Fields(3).Components(1).Value);

            Debug.WriteLine(message.SerializeMessage(false));
            int breakHere = 1;
        }
        */

        [Fact]
        public void SwapFields()
        {
            Message message = new Message(msg1);
            message.ParseMessage();

            message.DefaultSegment("ZZ1").SwapFields(2, 3);

            Assert.Equal("A", message.GetValue("ZZ1.3"));
            Assert.Equal("1", message.GetValue("ZZ1.2.2"));

        }

        /// <summary>These are invalid HL7 messages due to missing fields, but examples
        /// of allowing the "ParseMessage" to be less opinionated and allow some sloppy
        /// HL7 text</summary>
        [Theory]
        [InlineData("MSH|^~\\&|")]
        [InlineData("MSH")]
        [InlineData("EVN")]
        public void LessOpinionatedParserWorks(string exampleBadHl7)
        {
            Message message = new Message(exampleBadHl7);
            message.ParseMessage(false, false);

            var output = message.SerializeMessage(false);

            Assert.StartsWith(exampleBadHl7, output);
        }
    }
}

