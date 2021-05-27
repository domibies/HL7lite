using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HL7lite
{
    public class Component : MessageElement
    {
        internal ElementCollection<SubComponent> SubComponentList { get; set; }

        public bool IsSubComponentized { get; set; } = false;

        private bool isDelimiter = false;

        public Component()
        {
            this.isDelimiter = false;
            this.SubComponentList = new ElementCollection<SubComponent>();
        }

        public Component(HL7Encoding encoding, bool isDelimiter = false)
        {
            this.isDelimiter = isDelimiter;
            this.SubComponentList = new ElementCollection<SubComponent>();
            this.Encoding = encoding;
        }
        public Component(string pValue, HL7Encoding encoding)
        {
            this.SubComponentList = new ElementCollection<SubComponent>();
            this.Encoding = encoding;
            this.Value = pValue;
        }

        protected override void ProcessValue()
        {
            List<string> allSubComponents;
            
            if (this.isDelimiter)
                allSubComponents = new List<string>(new [] {this.Value});
            else
                allSubComponents = MessageHelper.SplitString(_value, this.Encoding.SubComponentDelimiter);

            if (allSubComponents.Count > 1)
                this.IsSubComponentized = true;

            this.SubComponentList = new ElementCollection<SubComponent>();

            foreach (string strSubComponent in allSubComponents)
            {
                SubComponent subComponent = new SubComponent(this.Encoding.Decode(strSubComponent), this.Encoding);
                SubComponentList.Add(subComponent);
            }
        }

        public SubComponent EnsureSubComponent(int position)
        {
            if (position < 1)
                throw new HL7Exception($"Invalid subcomponents index ({position} < 1)");

            if (position > SubComponentList.Count)
                SubComponentList.Add(new SubComponent(string.Empty, this.Encoding), position);

            return SubComponentList[position - 1];
        }

        public bool AddNewSubComponent(SubComponent subComponent, int position)
        {
            if (position < 1)
                throw new HL7Exception($"Invalid subcomponents index ({position} < 1)");

            try
            {
                this.SubComponentList.Add(subComponent, position);
                return true;
            }
            catch (Exception ex)
            {
                throw new HL7Exception("Unable to add new subcomponent Error - " + ex.Message);
            }
        }

        public SubComponent SubComponents(int position)
        {
            position = position - 1;

            try
            {
                return SubComponentList[position];
            }
            catch (Exception ex)
            {
                throw new HL7Exception("SubComponent not availalbe Error-" + ex.Message);
            }
        }

        public List<SubComponent> SubComponents()
        {
            return SubComponentList;
        }

        public override string SerializeValue()
        {
            var serialized = new StringBuilder();
            serialized.Append(string.Join(Encoding.SubComponentDelimiter.ToString(), SubComponentList.Select( sc => sc.SerializeValue())));
            return serialized.ToString();
        }

        public override void RemoveTrailingDelimiters(RemoveDelimitersOptions options)
        {
            foreach (var subComponent in SubComponentList)
                subComponent.RemoveTrailingDelimiters(options);

            if (options.SubComponent)
            {
                while (SubComponentList.Count > 1 && SubComponentList[SubComponentList.Count - 1].SerializeValue() == string.Empty)
                {
                    SubComponentList.RemoveAt(SubComponentList.Count - 1);
                }
                if (SubComponentList.Count == 1)
                    IsSubComponentized = false;
            }
        }
    }
}
