namespace HL7.Dotnetcore
{
    public class SubComponent : MessageElement
    {
        public SubComponent(string val)
        {
            this.Value = val;
        }

        protected override void ProcessValue()
        {
            
        }
    }
}
