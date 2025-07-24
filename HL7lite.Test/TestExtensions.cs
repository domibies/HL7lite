using HL7lite;

namespace HL7lite.Test
{
    public static class TestExtensions
    {
        /// <summary>
        /// Helper extension method for tests to easily add segments
        /// </summary>
        public static Segment AddSegment(this Message message, string segmentName)
        {
            var segment = new Segment(message.Encoding)
            {
                Name = segmentName,
                Value = segmentName
            };
            message.AddNewSegment(segment);
            return segment;
        }
    }
}