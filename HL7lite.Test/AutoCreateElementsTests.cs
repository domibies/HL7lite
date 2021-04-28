using System;
using System.Collections.Generic;
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
        public void EnsureFieldWorks()
        {
            Message message = new Message(msg1);
            message.ParseMessage();

            message.DefaultSegment("ZZ1").EnsureField(5).Value = "X";            

            Assert.Equal("X", message.GetValue("ZZ1.5"));
        }

        [Fact]
        public void EnsureFieldWithRepetitionWorks()
        {
            Message message = new Message(msg1);
            message.ParseMessage();

            message.DefaultSegment("ZZ1").EnsureField(5).Value = "X";
            message.DefaultSegment("ZZ1").EnsureField(5, 2).Value = "Y";

            Assert.Equal("X", message.GetValue("ZZ1.5(1)"));
            Assert.Equal("Y", message.GetValue("ZZ1.5(2)"));
        }

        [Fact]
        public void EnsureComponentWorks()
        {
            Message message = new Message(msg1);
            message.ParseMessage();

            message.DefaultSegment("ZZ1").Fields(2).EnsureComponent(2).Value = "B";

            Assert.Equal("B", message.GetValue("ZZ1.2.2"));
        }
        [Fact]
        public void EnsureSubComponentWorks()
        {
            Message message = new Message(msg1);
            message.ParseMessage();

            message.DefaultSegment("ZZ1").Fields(3).Components(2).EnsureSubComponent(2).Value = "XY";

            Assert.Equal("1", message.GetValue("ZZ1.3.2.1"));
            Assert.Equal("XY", message.GetValue("ZZ1.3.2.2"));
        }

        [Fact]
        public void PutValueFieldWorks()
        {
            Message message = new Message(msg1);
            message.ParseMessage();

            message.PutValue("ZZ1.5", "XYZ");

            Assert.Equal("XYZ", message.GetValue("ZZ1.5"));
        }

        [Fact]
        public void PutValueFieldComponentWorks()
        {
            Message message = new Message(msg1);
            message.ParseMessage();


            message.PutValue("ZZ1.5.4", "AA");

            Assert.Equal("AA", message.GetValue("ZZ1.5.4"));
        }

        [Fact]
        public void PutValueFieldAndComponentWorks()
        {
            Message message = new Message(msg1);
            message.ParseMessage();

            message.PutValue("ZZ1.5", "XYZ");
            message.PutValue("ZZ1.5.4", "AA");

            Assert.Equal("XYZ", message.GetValue("ZZ1.5.1"));
            Assert.Equal("AA", message.GetValue("ZZ1.5.4"));
        }

        [Fact]
        public void PutValueFieldRepetitionWorks()
        {
            Message message = new Message(msg1);
            message.ParseMessage();


            message.PutValue("ZZ1.5(2).4", "AA");

            Assert.Equal("AA", message.GetValue("ZZ1.5(2).4"));
        }

        [Fact]
        public void ExistsBeforeAndAfterWorks()
        {
            Message message = new Message(msg1);
            message.ParseMessage();

            Assert.False(message.ValueExists("ZZ1.10(2).10"));

            message.PutValue("ZZ1.10(2).10", "HUPSAKEE");

            Assert.True(message.ValueExists("ZZ1.10(2).10"));
        }

        /*
        [Fact]
        public void PutValueTest()
        {
            Message message = new Message(msg1);
            message.ParseMessage();

            message.PutValue("ZZ1.10.1", "HUP\r");
            message.PutValue("ZZ1.10.2", "SAKEE|");

            var fieldValue = message.GetValue("ZZ1.10");

            Assert.True(true);
        }
        */
    }
}

