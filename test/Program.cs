using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using HL7.Dotnetcore;

namespace HL7.Dotnetcore.Test
{
    [TestClass]
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

        [TestMethod]
        public void SmokeTest()
        {
            Message message = new Message(this.HL7_ORM);
            Assert.IsNotNull(message);
        }

        [TestMethod]
        public void ParseTest()
        {
            var message = new Message(this.HL7_ORM);

            var isParsed = message.ParseMessage();
            Assert.IsTrue(isParsed);
        }

        [TestMethod]
        public void ReadSegmentTest()
        {
            var message = new Message(this.HL7_ORM);
            message.ParseMessage();

            Segment MSH_1 = message.Segments("MSH")[0];
            Assert.IsNotNull(MSH_1);
        }

        [TestMethod]
        public void ReadDefaultSegmentTest()
        {
            var message = new Message(this.HL7_ADT);
            message.ParseMessage();

            Segment MSH_1 = message.DefaultSegment("MSH");
            Assert.IsNotNull(MSH_1);
        }

        [TestMethod]
        public void ReadField()
        {
            var message = new Message(this.HL7_ADT);
            message.ParseMessage();

            var MSH_1_8 = message.GetValue("MSH.8");
            Assert.IsTrue(MSH_1_8.StartsWith("ADT"));
        }

        [TestMethod]
        public void ReadComponent()
        {
            var message = new Message(this.HL7_ADT);
            message.ParseMessage();

            var MSH_1_8_1 = message.GetValue("MSH.8.1");
            Assert.AreEqual("ADT", MSH_1_8_1);
        }

        [TestMethod]
        public void AddComponents()
        {
            var encoding = new HL7Encoding();
            
            //Create a Segment with name ZIB
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
            var message = new Message(this.HL7_ADT);
            message.AddNewSegment(newSeg);

            string serializedMessage = message.SerializeMessage(false);
            Assert.AreEqual("ZIB|ZIB1||||ZIB5^^ZIB.5.3\r", serializedMessage);
        }

        [TestMethod]
        public void EmptyFields()
        {
            var message = new Message(this.HL7_ADT);
            message.ParseMessage();

            var NK1 = message.DefaultSegment("NK1").GetAllFields();
            Assert.AreEqual(34, NK1.Count);
            Assert.AreEqual(string.Empty, NK1[33].Value);
        }

        [TestMethod]
        public void EncodingForOutput()
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

            Assert.IsFalse(str.Contains("&"));  // Should have \T\ instead
        }
        
        [TestMethod]
        public void AddField()
        {
            var enc = new HL7Encoding();
            Segment mshSeg = new Segment("PID", enc);
            // Creates a new Field
            mshSeg.AddNewField("1", 1);

            // Overwrites the old Field
            mshSeg.AddNewField("2", 1);

            Message message = new Message();
            message.AddNewSegment(mshSeg);
            var str = message.SerializeMessage(false);

            Assert.AreEqual("PID|2\r", str);
        }

        [TestMethod]
        public void GetAck()
        {
            var message = new Message(this.HL7_ADT);
            message.ParseMessage();
            var ack = message.GetACK();

            var MSH_1_9 = message.GetValue("MSH.9");
            var MSH_1_9_A = ack.GetValue("MSH.9");
            var MSA_1_1 = ack.GetValue("MSA.1");
            var MSA_1_2 = ack.GetValue("MSA.2");

            Assert.AreEqual(MSA_1_1, "AA");
            Assert.AreEqual(MSH_1_9, MSH_1_9_A);
            Assert.AreEqual(MSH_1_9, MSA_1_2);
        }

        [TestMethod]
        public void GetNack()
        {
            var message = new Message(this.HL7_ADT);
            message.ParseMessage();

            var error = "Error message";
            var code = "AR";
            var ack = message.GetNACK(code, error);

            var MSH_1_9 = message.GetValue("MSH.9");
            var MSH_1_9_A = ack.GetValue("MSH.9");
            var MSA_1_1 = ack.GetValue("MSA.1");
            var MSA_1_2 = ack.GetValue("MSA.2");
            var MSA_1_3 = ack.GetValue("MSA.3");

            Assert.AreEqual(MSH_1_9, MSH_1_9_A);
            Assert.AreEqual(MSH_1_9, MSA_1_2);
            Assert.AreEqual(MSA_1_1, code);
            Assert.AreEqual(MSA_1_3, error);
        }
    }
}
