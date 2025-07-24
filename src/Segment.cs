using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace HL7lite
{
    public class Segment : MessageElement
    {
        internal ElementCollection<Field> FieldList { get; set; }
        internal short SequenceNo { get; set; }
                
        public string Name { get; set; }

        public Segment(HL7Encoding encoding)
        {
            this.FieldList = new ElementCollection<Field>();
            this.Encoding = encoding;
        }

        public Segment(string name, HL7Encoding encoding)
        {
            this.FieldList = new ElementCollection<Field>();
            this.Name = name;
            this.Encoding = encoding;
        }

        protected override void ProcessValue()
        {
            List<string> allFields = MessageHelper.SplitString(_value, this.Encoding.FieldDelimiter);

            allFields.RemoveAt(0);
            
            for (int i = 0; i < allFields.Count; i++)
            {
                string strField = allFields[i];
                Field field = new Field(this.Encoding);   

                if (Name == "MSH" && i == 0)
                    field.IsDelimiters = true; // special case

                field.Value = strField;
                this.FieldList.Add(field);
            }

            if (this.Name == "MSH")
            {
                var field1 = new Field(this.Encoding);
                field1.IsDelimiters = true;
                field1.Value = this.Encoding.FieldDelimiter.ToString();

                this.FieldList.Insert(0,field1);
            }
        }

        public Segment DeepCopy()
        {
            var newSegment = new Segment(this.Name, this.Encoding);
            // Use SerializeValue() to ensure we get the full segment content,
            // including fields that were set via fluent API but not yet serialized
            newSegment.Value = this.SerializeValue(); 

            return newSegment;        
        }

        public void AddEmptyField()
        {
            this.AddNewField(string.Empty);
        }

        public void AddNewField(string content, int position = -1)
        {
            this.AddNewField(new Field(content, this.Encoding), position);
        }

        public void AddNewField(string content, bool isDelimiters)
        {
            var newField = new Field(this.Encoding);

            if (isDelimiters)
                newField.IsDelimiters = true; // Prevent decoding

            newField.Value = content;
            this.AddNewField(newField, -1);
        }

        public Field EnsureField(int position, int repetition = 1)
        {
            position--; // position is one based but FieldList[] zero based
  

            Field field = null;
            while (position >= FieldList.Count)
                AddEmptyField();
            field = FieldList[position];


            if (repetition == 0)
            {
                // if no repetition specified (zero explicit), and the field does have repetitions, we remove all subsequent repetitions!!
                if (field.HasRepetitions)
                    field.RemoveRepetitions();
            }
            else
            {
                // if repetition is '1', we will re turn the single field 'or' the first repetition
                field = field.EnsureRepetition(repetition);
            }

            return field;
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
                    this.FieldList.Add(field, position);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                throw new HL7Exception("Unable to add new field in segment " + this.Name + " Error - " + ex.Message);
            }
        }

        public void SwapFields(int position1, int position2)
        {
            if (position1 < 1 || position2 < 1 || position2 > FieldList.Count || position1 > FieldList.Count)
                throw new HL7Exception("Invalid Position in SwapFields()");

            if (position1 != position2)
            {
                Field saveField = FieldList[position1 - 1];
                FieldList[position1 - 1] = FieldList[position2 - 1];
                FieldList[position2 - 1] = saveField;
            }
        }

        public Field Fields(int position)
        {
            position = position - 1;

            try
            {
                return this.FieldList[position];
            }
            catch (Exception ex)
            {
                throw new HL7Exception("Field not available Error - " + ex.Message);
            }
        }

        public List<Field> GetAllFields()
        {
            return this.FieldList;
        }

        public override string SerializeValue()
        {
            var strMessage = new StringBuilder();
            strMessage.Append(Name);

            if (FieldList.Count > 0)
                strMessage.Append(Encoding.FieldDelimiter);

            int startField = Name == "MSH" ? 1 : 0;

            for (int i = startField; i < FieldList.Count; i++)
            {
                if (i > startField)
                    strMessage.Append(Encoding.FieldDelimiter);

                var field = FieldList[i];

                if (field.IsDelimiters)
                {
                    strMessage.Append(field.Value);
                    continue;
                }

                if (field.HasRepetitions)
                {
                    for (int j = 0; j < field.RepetitionList.Count; j++)
                    {
                        if (j > 0)
                            strMessage.Append(Encoding.RepeatDelimiter);

                        serializeField(field.RepetitionList[j], strMessage);
                    }
                }
                else
                    serializeField(field, strMessage);
            }

            return strMessage.ToString();
        }

        /// <summary>
        /// Serializes a field into a string with proper encoding
        /// </summary>
        /// <returns>A serialized string</returns>
        private void serializeField(Field field, StringBuilder strMessage)
        {
            if (field.ComponentList.Count > 0)
            {
                int indexCom = 0;

                foreach (Component com in field.ComponentList)
                {
                    indexCom++;
                    if (com.SubComponentList.Count > 0)
                        strMessage.Append(string.Join(Encoding.SubComponentDelimiter.ToString(), com.SubComponentList.Select(sc => Encoding.Encode(sc.Value))));
                    else
                        strMessage.Append(Encoding.Encode(com.Value));

                    if (indexCom < field.ComponentList.Count)
                        strMessage.Append(Encoding.ComponentDelimiter);
                }
            }
            else
                strMessage.Append(Encoding.Encode(field.Value));

        }



        public override void RemoveTrailingDelimiters(RemoveDelimitersOptions options)
        {
            foreach(var field in FieldList)
            {
                field.RemoveTrailingDelimiters(options);
            }

            if (options.Fields)
            {
                while (FieldList.Count > 1 && FieldList[FieldList.Count - 1].SerializeValue() == string.Empty)
                {
                    FieldList.RemoveAt(FieldList.Count - 1);
                }
            }
        }
    }
}
