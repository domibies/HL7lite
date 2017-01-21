using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HL7.Dotnetcore
{
    public class SubComponent
    {
        private String _Value;

        public SubComponent()
        {
        }

        public SubComponent(String pValue)
        {
            _Value = pValue;
        }

        public String Value
        {
            get
            {
                if (_Value == null)
                    return String.Empty;
                else
                    return _Value;
            }
            set
            {
                _Value = value;
            }
        }
    }
}
