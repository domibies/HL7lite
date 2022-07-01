using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HL7lite.Test
{
    public class TestGetValueAfterRemoveSegment
    {
        private static readonly string someMessage = @"MSH|^~\&|SENDER|SFACILITY|RECEIVER|RFACILITY|20210330110056||Z99^Z01|12345|P|2.3||
ZZA|1||~1~22^A&B~333|0^A~1~22^A&B~333
ZZB|2";

        // bug : after removing a segment, accessing data from the segment via GetValue, throws a 'Sequence contains no elements'
        [Fact]
        public void GetValue_AfterRemove_Segment_DoesntThrow()
        {
            var message = new Message(someMessage);
            message.ParseMessage();

            message.RemoveSegment("ZZA");

            try
            {
                var value = message.GetValue("ZZA.1");
            }
            catch (HL7Exception)
            {
                // that's ok, we expect a hl7exception
            }
        }
    }
}
