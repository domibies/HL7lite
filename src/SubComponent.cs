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
    }
}
