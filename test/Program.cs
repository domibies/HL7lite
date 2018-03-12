using System.IO;
using System.Reflection;
using Xunit;

using HL7.Dotnetcore;

namespace HL7.Dotnetcore.Test
{
    public class HL7Test
    {
        private string HL7_ORM;
        private string HL7_ADT;

        public HL7Test()
        {
            var path = Path.GetDirectoryName(typeof(HL7Test).GetTypeInfo().Assembly.Location) + "/";
            this.HL7_ORM = File.ReadAllText(path + "Sample-ORM.txt");
            this.HL7_ADT = File.ReadAllText(path + "Sample-ADT.txt");
        }

        [Fact]
        public void SmokeTest()
        {
            Message message = new Message(this.HL7_ORM);
            Assert.NotNull(message);
        }

        [Fact]
        public void ParseTest()
        {
            var message = new Message(this.HL7_ORM);

            var isParsed = message.ParseMessage();
            Assert.True(isParsed);
        }

        [Fact]
        public void ReadSegmentTest()
        {
            var message = new Message(this.HL7_ORM);
            message.ParseMessage();

            Segment MSH_1 = message.Segments("MSH")[0];
            Assert.NotNull(MSH_1);
        }

        [Fact]
        public void ReadDefaultSegmentTest()
        {
            var message = new Message(this.HL7_ADT);
            message.ParseMessage();

            Segment MSH_1 = message.DefaultSegment("MSH");
            Assert.NotNull(MSH_1);
        }

        [Fact]
        public void ReadField()
        {
            var message = new Message(this.HL7_ADT);
            message.ParseMessage();

            var MSH_1_8 = message.GetValue("MSH.8");
            Assert.StartsWith("ADT", MSH_1_8);
        }

        [Fact]
        public void ReadComponent()
        {
            var message = new Message(this.HL7_ADT);
            message.ParseMessage();

            var MSH_1_8_1 = message.GetValue("MSH.8.1");
            Assert.Equal("ADT", MSH_1_8_1);
        }

        [Fact]
        public void EmptyFields()
        {
            var message = new Message(this.HL7_ADT);
            message.ParseMessage();

            var NK1 = message.DefaultSegment("NK1").GetAllFields();
            Assert.Equal(34, NK1.Count);
            Assert.Equal(string.Empty, NK1[33].Value);
        }
    }
}
