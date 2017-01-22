using System;
using System.Collections.Generic;

namespace HL7.Dotnetcore
{
    public class Segment : MessageElement
    {
        internal FieldCollection FieldList { get; set; }
        internal short SequenceNo { get; set; }
                
        public string Name { get; set; }

        public Segment(Encoding encoding)
        {
            this.FieldList = new FieldCollection();
            this.Encoding = encoding;
        }

        public Segment(string name, Encoding encoding)
        {
            this.FieldList = new FieldCollection();
            this.Name = name;
            this.Encoding = encoding;
        }

        protected override void ProcessValue()
        {
            if (_value.Length > 0)
            {
                _value = _value.TrimEnd(this.Encoding.FieldDelimiter);
                List<string> allFields = MessageHelper.SplitString(_value, this.Encoding.FieldDelimiter);

                if (allFields.Count > 1)
                {
                    if (Name == "MSH")
                        allFields[1] = this.Encoding.FieldDelimiter.ToString();
                    else
                        allFields.RemoveAt(0);
                }
                foreach (string strField in allFields)
                {
                    Field field = new Field(strField, this.Encoding);
                    FieldList.Add(field);
                }
            }
        }

        public Segment DeepCopy()
        {
            var newSegment = new Segment(this.Name, this.Encoding);
            newSegment.Value = this.Value; 

            return newSegment;        
        }

        public void AddEmptyField()
        {
            this.AddNewField(string.Empty);
        }

        public void AddNewField(string val, bool isDelimiters)
        {
            var newField = new Field(this.Encoding);

            if (isDelimiters)
                newField.IsDelimiters = true;   // Prevent decoding

            newField.Value = val;
            this.AddNewField(newField, -1);
        }
        public void AddNewField(string content, int position = -1)
        {
            this.AddNewField(new Field(content, this.Encoding), position);
        }

        public bool AddNewField(Field field, int position = -1)
        {
            try
            {
                if (position < 0)
                {
                    this.FieldList.Add(field);
                }
                else 
                {
                    position = position - 1;
                    this.FieldList.Add(field, position);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new HL7Exception("Unable to add new field in segment " + this.Name + " Error - " + ex.Message);
            }
        }

        public Field Fields(int position)
        {
            position = position - 1;
            Field field = null;

            try
            {
                field = FieldList[position];
            }
            catch (Exception ex)
            {
                throw new HL7Exception("Field not availalbe Error - " + ex.Message);
            }

            return field;
        }

        public List<Field> GetAllFields()
        {
            return FieldList;
        }
    }
}
