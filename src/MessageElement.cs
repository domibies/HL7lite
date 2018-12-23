namespace HL7.Dotnetcore
{
    public abstract class MessageElement
    {
        protected string _value = string.Empty;
        const string presentButNull = "\"\"";
        
        public  string Value 
        { 
            get 
            {
                return _value == presentButNull ? null : _value; 
            }
            set 
            { 
                _value = value; 
                ProcessValue(); 
            }
        }

        public HL7Encoding Encoding { get; protected set; }

        protected abstract void ProcessValue();
    }
}
