using HL7lite;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace HL7Lite.Test
{
    public class TestRepeatedFields
    {
        private static readonly string repeatedFieldsMsg1 = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||Z99^Z01|12345|P|2.3||
ZZA|1||~1~22^A&B~333|0^A~1~22^A&B~333";

        [Fact]
        public void RepeatedFields_HasRepetions_AfterParse_Correct()
        {
            var message = new Message(repeatedFieldsMsg1);
            message.ParseMessage();

            var zza = message.Segments("ZZA")[0];

            Assert.NotNull(zza);

            var field1 = zza.Fields(1);
            var field3 = zza.Fields(3);

            Assert.False(field1.HasRepetitions);
            Assert.True(field3.HasRepetitions);
        }

        [Fact]
        public void RepeatedFields_HasRepetions_AfterPutValue_Correct()
        {
            var message = new Message(repeatedFieldsMsg1);
            message.ParseMessage();

            var zza = message.Segments("ZZA")[0];
            Assert.NotNull(zza);

            message.PutValue("ZZA.1(1)", "TEST");

            var field1 = zza.Fields(1);

            Assert.False(field1.HasRepetitions);

            message.PutValue("ZZA.1(2)", "ANOTHER TEST");
            Assert.True(field1.HasRepetitions);

        }

        [Fact]
        public void RepeatedFields_HasRepetionsOnMessage_AfterParse_Correct()
        {
            var message = new Message(repeatedFieldsMsg1);
            message.ParseMessage();

            Assert.True(message.HasRepetitions("ZZA.3"));
        }

        [Fact]
        public void RepeatedFields_GetValue()
        {
            var message = new Message(repeatedFieldsMsg1);
            message.ParseMessage();

//            var value = message.GetValue("ZZA.3");
            var value = message.GetValue("ZZA.4.1"); // should get the first component of the first repetition

            Assert.True(value == "0"); 
        }
    }
}
