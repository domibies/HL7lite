﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace HL7lite
{
    public class Field : MessageElement
    {
        private List<Field> _RepetitionList;

        internal ComponentCollection ComponentList { get; set; }

        public bool IsComponentized { get; set; } = false;
        public bool HasRepetitions { get; set; } = false;
        public bool IsDelimiters { get; set; } = false;

        internal List<Field> RepetitionList
        {
            get
            {
                if (_RepetitionList == null)
                    _RepetitionList = new List<Field>();
                    
                return _RepetitionList;
            }
            set
            {
                _RepetitionList = value;
            }
        }

        protected override void ProcessValue()
        {
            if (this.IsDelimiters)  // Special case for the delimiters fields (MSH)
            {
                var subcomponent = new SubComponent(_value, this.Encoding);

                this.ComponentList = new ComponentCollection();
                Component component = new Component(this.Encoding, true);

                component.SubComponentList.Add(subcomponent);

                this.ComponentList.Add(component);
                return;
            }

            this.HasRepetitions = _value.Contains(this.Encoding.RepeatDelimiter);

            if (this.HasRepetitions)
            {
                _RepetitionList = new List<Field>();
                List<string> individualFields = MessageHelper.SplitString(_value, this.Encoding.RepeatDelimiter);

                for (int index = 0; index < individualFields.Count; index++)
                {
                    Field field = new Field(individualFields[index], this.Encoding);
                    _RepetitionList.Add(field);
                }
            }
            else
            {
                List<string> allComponents = MessageHelper.SplitString(_value, this.Encoding.ComponentDelimiter);

                this.ComponentList = new ComponentCollection();

                foreach (string strComponent in allComponents)
                {
                    Component component = new Component(this.Encoding);
                    component.Value = strComponent;
                    this.ComponentList.Add(component);
                }

                this.IsComponentized = this.ComponentList.Count > 1;
            }
        }

        public Field(HL7Encoding encoding)
        {
            this.ComponentList = new ComponentCollection();
            this.Encoding = encoding;
        }

        public Field(string value, HL7Encoding encoding)
        {
            this.ComponentList = new ComponentCollection();
            this.Encoding = encoding;
            this.Value = value;
        }

        public bool AddNewComponent(Component com)
        {
            try
            {
                this.ComponentList.Add(com);
                return true;
            }
            catch (Exception ex)
            {
                throw new HL7Exception("Unable to add new component Error - " + ex.Message);
            }
        }

        public bool AddNewComponent(Component component, int position)
        {
            try
            {
                this.ComponentList.Add(component, position);
                return true;
            }
            catch (Exception ex)
            {
                throw new HL7Exception("Unable to add new component Error - " + ex.Message);
            }
        }

        public Component Components(int position)
        {
            position = position - 1;

            try
            {
                return ComponentList[position];
            }
            catch (Exception ex)
            {
                throw new HL7Exception("Component not available Error - " + ex.Message);
            }
        }

        public List<Component> Components()
        {
            return ComponentList;
        }

        public List<Field> Repetitions()
        {
            if (this.HasRepetitions)
            {
                return RepetitionList;
            }
            return null;
        }

        public Field Repetitions(int repeatitionNumber)
        {
            if (this.HasRepetitions)
            {
                return RepetitionList[repeatitionNumber - 1];
            }
            return null;
        }

        public bool RemoveEmptyTrailingComponents()
        {
            try
            {
                for (var eachComponent = ComponentList.Count - 1; eachComponent >= 0; eachComponent--)
                {
                    if (ComponentList[eachComponent].Value == "")
                        ComponentList.Remove(ComponentList[eachComponent]);
                    else
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new HL7Exception("Error removing trailing comonents - " + ex.Message);
            }
        }
    }
}
