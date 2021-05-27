namespace HL7lite
{
    public abstract class MessageElement
    {
        protected string _value = string.Empty;
       
        
        public  string Value 
        { 
            get 
            {
                return _value == Encoding.PresentButNull ? null : _value; 
            }
            set 
            { 
                _value = value; 
                ProcessValue(); 
            }
        }

        public HL7Encoding Encoding { get; internal set; }

        protected abstract void ProcessValue();

        public abstract string SerializeValue();


        public class RemoveDelimitersOptions
        {
            public bool SubComponent { get; set; }
            public bool Components { get; set; }
            public bool Fields { get; set; }

            public static RemoveDelimitersOptions All => new RemoveDelimitersOptions { SubComponent = true, Components = true, Fields = true }; 
        }


        public abstract void RemoveTrailingDelimiters(RemoveDelimitersOptions options);
    }
}
