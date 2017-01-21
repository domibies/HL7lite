namespace HL7.Dotnetcore
{
    public class SubComponent : MessageElement
    {
        public SubComponent(string pValue)
        {
            this._value = pValue;
        }

        protected override void ProcessValue()
        {}
    }
}
