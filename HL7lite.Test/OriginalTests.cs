using System;
using System.IO;
using HL7lite;
using Xunit;

namespace HL7lite.Test
{
    public class OriginalTests
    {

        private static Lazy<string> HL7_ORM = new Lazy<string>(() => File.ReadAllText(SamplesPath.Value + "Sample-ORM-3149.txt"));
        private static Lazy<string> HL7_ADT = new Lazy<string>(() => File.ReadAllText(SamplesPath.Value + "Sample-ADT-MSGID12349876.txt"));

        [Fact]
        public void SmokeTest()
        {
            Message message = new Message(HL7_ORM.Value);
            Assert.NotNull(message);

            // message.ParseMessage();
            // File.WriteAllText("SmokeTestResult.txt", message.SerializeMessage(false));
        }

        [Fact]
        public void ReadDefaultSegmentTest()
        {
            var message = new Message(HL7_ADT.Value);
            message.ParseMessage();

            Segment MSH = message.DefaultSegment("MSH");
            Assert.NotNull(MSH);
        }

        [Fact]
        public void ReadFieldTest()
        {
            var message = new Message(HL7_ADT.Value);
            message.ParseMessage();

            var MSH_9 = message.GetValue("MSH.9");
            Assert.Equal("ADT^O01", MSH_9);
        }

        [Fact]
        public void ReadComponentTest()
        {
            var message = new Message(HL7_ADT.Value);
            message.ParseMessage();

            var MSH_9_1 = message.GetValue("MSH.9.1");
            Assert.Equal("ADT", MSH_9_1);
        }

        [Fact]
        public void AddComponentsTest()
        {
            var encoding = new HL7Encoding();

            // Create a Segment with name ZIB
            Segment newSeg = new Segment("ZIB", encoding);

            // Create Field ZIB_1
            Field ZIB_1 = new Field("ZIB1", encoding);
            // Create Field ZIB_5
            Field ZIB_5 = new Field("ZIB5", encoding);

            // Create Component ZIB.5.3
            Component com1 = new Component("ZIB.5.3_", encoding);

            // Add Component ZIB.5.3 to Field ZIB_5
            ZIB_5.AddNewComponent(com1, 3);

            // Overwrite the same field again
            ZIB_5.AddNewComponent(new Component("ZIB.5.3", encoding), 3);

            // Add Field ZIB_1 to segment ZIB, this will add a new filed to next field location, in this case first field
            newSeg.AddNewField(ZIB_1);

            // Add Field ZIB_5 to segment ZIB, this will add a new filed as 5th field of segment
            newSeg.AddNewField(ZIB_5, 5);

            // Add segment ZIB to message
            var message = new Message(HL7_ADT.Value);
            message.AddNewSegment(newSeg);

            string serializedMessage = message.SerializeMessage(false);
            Assert.Equal("ZIB|ZIB1||||ZIB5^^ZIB.5.3\r", serializedMessage);
        }

        [Fact]
        public void EmptyFieldsTest()
        {
            var message = new Message(HL7_ADT.Value);
            message.ParseMessage();

            var NK1 = message.DefaultSegment("NK1").GetAllFields();
            Assert.Equal(34, NK1.Count);
            Assert.Equal(string.Empty, NK1[33].Value);
        }

        [Fact]
        public void EncodingForOutputTest()
        {
            const string oruUrl = "domain.com/resource.html?Action=1&ID=2";  // Text with special character (&)

            var obx = new Segment("OBX", new HL7Encoding());
            obx.AddNewField("1");
            obx.AddNewField("RP");
            obx.AddNewField("70030^Radiologic Exam, Eye, Detection, FB^CDIRadCodes");
            obx.AddNewField("1");
            obx.AddNewField(obx.Encoding.Encode(oruUrl));  // Encoded field
            obx.AddNewField("F", 11);
            obx.AddNewField(MessageHelper.LongDateWithFractionOfSecond(DateTime.Now), 14);

            var oru = new Message();
            oru.AddNewSegment(obx);

            var str = oru.SerializeMessage(false);

            Assert.DoesNotContain("&", str);  // Should have \T\ instead
        }

        [Fact]
        public void AddFieldTest()
        {
            var enc = new HL7Encoding();
            Segment PID = new Segment("PID", enc);
            // Creates a new Field
            PID.AddNewField("1", 1);

            // Overwrites the old Field
            PID.AddNewField("2", 1);

            Message message = new Message();
            message.AddNewSegment(PID);
            var str = message.SerializeMessage(false);

            Assert.Equal("PID|2\r", str);
        }

        [Fact]
        public void GetMSH1Test()
        {
            var message = new Message(HL7_ADT.Value);
            message.ParseMessage();

            var MSH_1 = message.GetValue("MSH.1");
            Assert.Equal("|", MSH_1);
        }

        [Fact]
        public void GetAckTest()
        {
            var message = new Message(HL7_ADT.Value);
            message.ParseMessage();
            var ack = message.GetACK();

            var MSH_3 = message.GetValue("MSH.3");
            var MSH_4 = message.GetValue("MSH.4");
            var MSH_5 = message.GetValue("MSH.5");
            var MSH_6 = message.GetValue("MSH.6");
            var MSH_3_A = ack.GetValue("MSH.3");
            var MSH_4_A = ack.GetValue("MSH.4");
            var MSH_5_A = ack.GetValue("MSH.5");
            var MSH_6_A = ack.GetValue("MSH.6");

            Assert.Equal(MSH_3, MSH_5_A);
            Assert.Equal(MSH_4, MSH_6_A);
            Assert.Equal(MSH_5, MSH_3_A);
            Assert.Equal(MSH_6, MSH_4_A);

            var MSH_10 = message.GetValue("MSH.10");
            var MSH_10_A = ack.GetValue("MSH.10");
            var MSA_1_1 = ack.GetValue("MSA.1");
            var MSA_1_2 = ack.GetValue("MSA.2");

            Assert.Equal("AA", MSA_1_1);
            Assert.Equal(MSH_10, MSH_10_A);
            Assert.Equal(MSH_10, MSA_1_2);
        }

        [Fact]
        public void AddSegmentMSHTest()
        {
            var message = new Message();
            message.AddSegmentMSH("test", "sendingFacility", "test", "test", "test", "ADR^A19", "test", "D", "2.5");
        }

        [Fact]
        public void GetNackTest()
        {
            var message = new Message(HL7_ADT.Value);
            message.ParseMessage();

            var error = "Error message";
            var code = "AR";
            var ack = message.GetNACK(code, error);

            var MSH_3 = message.GetValue("MSH.3");
            var MSH_4 = message.GetValue("MSH.4");
            var MSH_5 = message.GetValue("MSH.5");
            var MSH_6 = message.GetValue("MSH.6");
            var MSH_3_A = ack.GetValue("MSH.3");
            var MSH_4_A = ack.GetValue("MSH.4");
            var MSH_5_A = ack.GetValue("MSH.5");
            var MSH_6_A = ack.GetValue("MSH.6");

            Assert.Equal(MSH_3, MSH_5_A);
            Assert.Equal(MSH_4, MSH_6_A);
            Assert.Equal(MSH_5, MSH_3_A);
            Assert.Equal(MSH_6, MSH_4_A);

            var MSH_10 = message.GetValue("MSH.10");
            var MSH_10_A = ack.GetValue("MSH.10");
            var MSA_1_1 = ack.GetValue("MSA.1");
            var MSA_1_2 = ack.GetValue("MSA.2");
            var MSA_1_3 = ack.GetValue("MSA.3");

            Assert.Equal(MSH_10, MSH_10_A);
            Assert.Equal(MSH_10, MSA_1_2);
            Assert.Equal(MSA_1_1, code);
            Assert.Equal(MSA_1_3, error);
        }

        [Fact]
        public void EmptyAndNullFieldsTest()
        {
            const string sampleMessage = "MSH|^~\\&|SA|SF|RA|RF|20110613083617||ADT^A04|123|P|2.7||||\r\nEVN|A04|20110613083617||\"\"\r\n";

            var message = new Message(sampleMessage);
            var isParsed = message.ParseMessage();
            Assert.True(isParsed);
            Assert.True(message.SegmentCount > 0);
            var evn = message.Segments("EVN")[0];
            var expectEmpty = evn.Fields(3).Value;
            Assert.Equal(string.Empty, expectEmpty);
            var expectNull = evn.Fields(4).Value;
            Assert.Null(expectNull);
        }

        [Fact]
        public void MessageWithDoubleNewlineTest()
        {
            const string sampleMessage = "MSH|^~\\&|SA|SF|RA|RF|20110613083617||ADT^A04|123|P|2.7||||\n\nEVN|A04|20110613083617||\r\n";

            var message = new Message(sampleMessage);
            var isParsed = message.ParseMessage();
            Assert.True(isParsed);
            Assert.True(message.SegmentCount > 0);
        }

        [Fact]
        public void MessageWithDoubleCarriageReturnTest()
        {
            const string sampleMessage = "MSH|^~\\&|SA|SF|RA|RF|20110613083617||ADT^A04|123|P|2.7||||\n\nEVN|A04|20110613083617||\r\n";

            var message = new Message(sampleMessage);
            var isParsed = message.ParseMessage();
            Assert.True(isParsed);
            Assert.True(message.SegmentCount > 0);
        }

        [Fact]
        public void MessageWithNullsIsReversable()
        {
            const string sampleMessage = "MSH|^~\\&|SA|SF|RA|RF|20110613083617||ADT^A04|123|P|2.7||||\r\nEVN|A04|20110613083617||\"\"\r\n";
            var message = new Message(sampleMessage);
            message.ParseMessage();
            var serialized = message.SerializeMessage(false);
            Assert.Equal(sampleMessage, serialized);
        }

        [Fact]
        public void MessageWithSegmentNameOnly()
        {
            const string sampleMessage = "MSH|^~\\&|SA|SF|RA|RF|20110613083617||ADT^A04|123|P|2.7||||\r\nPID\r\nEVN|A04|20110613083617||\"\"\r\n";
            var message = new Message(sampleMessage);
            message.ParseMessage();
            var serialized = message.SerializeMessage(false);
            Assert.Equal(sampleMessage, serialized);
        }

        [Fact]
        public void MessageWithTabsIsReversable()
        {
            const string sampleMessage = "MSH|^~\\&|Sending\tApplication|Sending\tFacility|RA|RF|20110613083617||ADT^A04|123|P|2.7||||\r\nEVN|A04|20110613083617\r\n";
            var message = new Message(sampleMessage);
            message.ParseMessage();
            var serialized = message.SerializeMessage(false);
            Assert.Equal(sampleMessage, serialized);
        }

        [Fact]
        public void RemoveSegment()
        {
            var message = new Message(HL7_ADT.Value);
            message.ParseMessage();
            Assert.Equal(2, message.Segments("NK1").Count);
            Assert.Equal(5, message.SegmentCount);

            message.RemoveSegment("NK1", 1);
            Assert.Single(message.Segments("NK1"));
            Assert.Equal(4, message.SegmentCount);

            message.RemoveSegment("NK1");
            Assert.Empty(message.Segments("NK1"));
            Assert.Equal(3, message.SegmentCount);
        }

        [Theory]
        [InlineData("   20151231234500.1234+2358   ")]
        [InlineData("20151231234500.1234+2358")]
        [InlineData("20151231234500.1234-2358")]
        [InlineData("20151231234500.1234")]
        [InlineData("20151231234500.12")]
        [InlineData("20151231234500")]
        [InlineData("201512312345")]
        [InlineData("2015123123")]
        [InlineData("20151231")]
        [InlineData("201512")]
        [InlineData("2015")]
        public void ParseDateTime_Smoke_Positive(string dateTimeString)
        {
            var date = MessageHelper.ParseDateTime(dateTimeString);
            Assert.NotNull(date);
        }

        [Theory]
        [InlineData("   20151231234500.1234+23581")]
        [InlineData("20151231234500.1234+23")]
        [InlineData("20151231234500.12345")]
        [InlineData("20151231234500.")]
        [InlineData("2015123123450")]
        [InlineData("20151231234")]
        [InlineData("201512312")]
        [InlineData("2015123")]
        [InlineData("20151")]
        [InlineData("201")]
        public void ParseDateTime_Smoke_Negative(string dateTimeString)
        {
            var date = MessageHelper.ParseDateTime(dateTimeString);
            Assert.Null(date);
        }

        [Fact]
        public void ParseDateTime_Correctness()
        {
            TimeSpan offset;
            var date = MessageHelper.ParseDateTime("20151231234500.1234-2358", out offset).Value;
            // Assert.Equal(0, d
            Assert.Equal(date, new DateTime(2015, 12, 31, 23, 45, 00, 123));
            Assert.Equal(offset, new TimeSpan(-23, 58, 0));
        }

        [Fact]
        public void ParseDateTime_WithException()
        {
                Assert.Throws<FormatException>(() => {
                    var date = MessageHelper.ParseDateTime("201", true);
                });
        }

        [Fact]
        public void ParseDateTimeOffset_WithException()
        {
            Assert.Throws<FormatException>(() => {
                var date = MessageHelper.ParseDateTime("201", out TimeSpan offset, true);
            }); 
        }

        [Fact]
        public void GetValueTest()
        {
            var sampleMessage =
                @"MSH|^~\&|EPIC||||20191107134803|ALEVIB01|ORM^O01|23|T|2.3|||||||||||
PID|1||MRN_123^^^IDX^MRN||Smith\F\\S\\R\\E\\T\^John||19600101|M";

            var message = new Message(sampleMessage);
            message.ParseMessage();

            string attendingDrId = message.GetValue("PID.5.1");
            Assert.Equal(@"Smith|^~\&", attendingDrId);
        }

        [Fact]
        public void SkipInvalidEscapeSequenceTest()
        {
            var sampleMessage =
                @"MSH|^~\&|TEST^TEST|TEST|||11111||ADT^A08|11111|P|2.4|||AL||D||||||
ZZ1|1|139378|20201230100000|ghg^ghgh-HA||s1^OP-Saal 1|gh^gjhg 1|ghg^ghjg-HA|BSV 4\5 re.||||||";

            var message = new Message(sampleMessage);
            message.ParseMessage();

            string serializedMessage = message.SerializeMessage(false);
        }

        [Fact]
        public void CustomDelimiterTest()
        {
            var encoding = new HL7Encoding
            {
                FieldDelimiter = '1',
                ComponentDelimiter = '2',
                SubComponentDelimiter = '3',
                RepeatDelimiter = '4',
                EscapeCharacter = '5'
            };

            var message = new Message();
            message.Encoding = encoding;
            message.AddSegmentMSH("FIRST", "SECOND", "THIRD", "FOURTH",
                "FIFTH", "ORU2R05F5", "SIXTH", "SEVENTH", "2.8");
            var result = message.SerializeMessage(false);

            Assert.Equal("MSH124531", result.Substring(0, 9));
        }

        [Theory]
        [InlineData("PV1.7.1", "1447312459")]
        [InlineData("PV1.7(1).1", "1447312459")]
        [InlineData("PV1.7[1].1", "1447312459")]
        [InlineData("PV1.7(2).1", "DOEM06")]
        [InlineData("PV1.7[2].1", "DOEM06")]
        [InlineData("PV1.7[2].3", "MICHAEL")]
        public void RepetitionTest(string index, string expected)
        {
            var sampleMessage =
                @"MSH|^~\&|EPIC||||20191107134803|ALEVIB01|ORM^O01|23|T|2.3|||||||||||
PID|1||1005555^^^NYU MRN^MRN||OSTRICH^DODUO||19820605|M||U|000 PARK AVE SOUTH^^NEW YORK^NY^10010^US^^^60|60|(555)555-5555^HOME^PH|||S|||999-99-9999|||U||N||||||||
PV1||O|NWSLED^^^NYULHLI^^^^^LI NW SLEEP DISORDER^^DEPID||||1447312459^DOE^MICHAEL^^^^^^EPIC^^^^PNPI~DOEM06^DOE^MICHAEL^^^^^^KID^^^^KID|1447312459^DOE^MICHAEL^^^^^^EPIC^^^^PNPI~DOEM06^DOE^MICHAEL^^^^^^KID^^^^KID|||||||||||496779945|||||||||||||||||||||||||20191107|||||||V";

            var message = new Message(sampleMessage);
            message.ParseMessage();

            string attendingDrId = message.GetValue(index);
            Assert.Equal(expected, attendingDrId);
        }

        [Fact]
        public void InvalidRepetitionTest()
        {
            var sampleMessage =
                @"MSH|^~\&|SYSTEM1|ABC|SYSTEM2||201803262027||DFT^P03|20180326202737608457|P|2.3||||||8859/1
EVN|P03|20180326202540
PID|1|0002381795|0002381795||Supermann^Peter^^^Herr||19990101|M|||Hamburgerstrasse 123^^Mimamu^BL^12345^CH||123456~123456^^CP||D|2|02|321|8.2.24.||| 
PV1||A|00004620^00001318^1318||||000123456^Superfrau^Maria W.^|^Superarzt^Anton^L|00097012345^Superarzt^Herbert^~~0009723456^Superarzt^Markus^||||||||000998765^Assistent A^ONKO^D||0087123456||||||||||||||||||||2140||O|||201905220600|201908201100|||||";

            var message = new Message(sampleMessage);
            message.ParseMessage();


            // Check for invalid repetition number
            Assert.Throws<HL7Exception>(() => {
                var value = message.GetValue("PV1.8(2).1");
                Assert.Null(value);
                value = message.GetValue("PV1.8(3).1");
                Assert.Null(value);
            });
        }

        [Fact]
        public void RemoveTrailingComponentsTest_OnlyTrailingComponentsRemoved()
        {
            var message = new Message();

            var orcSegment = new Segment("ORC", new HL7Encoding());
            for (int eachField = 1; eachField <= 12; eachField++)
            {
                orcSegment.AddEmptyField();
            }

            for (int eachComponent = 1; eachComponent < 8; eachComponent++)
            {
                orcSegment.Fields(12).AddNewComponent(new Component(new HL7Encoding()));
            }

            orcSegment.Fields(12).Components(1).Value = "should not be removed";
            orcSegment.Fields(12).Components(2).Value = "should not be removed";
            orcSegment.Fields(12).Components(3).Value = "should not be removed";
            orcSegment.Fields(12).Components(4).Value = ""; // should not be removed because in between valid values
            orcSegment.Fields(12).Components(5).Value = "should not be removed";
            orcSegment.Fields(12).Components(6).Value = ""; // should be removed because trailing
            orcSegment.Fields(12).Components(7).Value = ""; // should be removed because trailing
            orcSegment.Fields(12).Components(8).Value = ""; // should be removed because trailing

            orcSegment.Fields(12).RemoveEmptyTrailingComponents();
            message.AddNewSegment(orcSegment);

            string serializedMessage = message.SerializeMessage(false);
            Assert.Equal(5, orcSegment.Fields(12).Components().Count);
            Assert.Equal("ORC||||||||||||should not be removed^should not be removed^should not be removed^^should not be removed\r", serializedMessage);
        }

        [Fact]
        public void RemoveTrailingComponentsTest_RemoveAllFieldComponentsIfEmpty()
        {
            var message = new Message();

            var orcSegment = new Segment("ORC", new HL7Encoding());
            for (int eachField = 1; eachField <= 12; eachField++)
            {
                orcSegment.AddEmptyField();
            }

            for (int eachComponent = 1; eachComponent < 8; eachComponent++)
            {
                orcSegment.Fields(12).AddNewComponent(new Component(new HL7Encoding()));
                orcSegment.Fields(12).Components(eachComponent).Value = "";
            }

            orcSegment.Fields(12).RemoveEmptyTrailingComponents();
            message.AddNewSegment(orcSegment);

            string serializedMessage = message.SerializeMessage(false);
            Assert.Empty(orcSegment.Fields(12).Components());
            Assert.Equal("ORC||||||||||||\r", serializedMessage);
        }
    }
}

