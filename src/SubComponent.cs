#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace HL7lite
{
    public class SubComponent : MessageElement
    {
        public SubComponent()
        {
        }

        public SubComponent(string val, HL7Encoding encoding)
        {
            this.Encoding = encoding;
            this.Value = val;
        }

        protected override void ProcessValue()
        {
        }

        public override string SerializeValue()
        {
            return Encoding.Encode(Value);
        }

        public override void RemoveTrailingDelimiters(RemoveDelimitersOptions options)
        {
        }

    }
}
