using System;
using System.Collections.Generic;
using System.Linq;

namespace HL7.Dotnetcore
{
    public class Message
    {
        private List<string> allSegments = null;
        private char[] messageTrimChars = new char[] {(char)11, (char)13, (char)28, (char)32 };    // 11 - VT(\v), 28 - FS, 13 - CR(\r)
        internal Dictionary<string, List<Segment>> SegmentList { get; set;} = new Dictionary<string, List<Segment>>();

        public string HL7Message { get; set; }
        public string Version { get; set; }
        public string MessageStructure { get; set; }
        public string MessageControlID { get; set; }
        public string ProcessingID { get; set; }
        public short SegmentCount { get; set; }
        public Encoding Encoding { get; set; } = new Encoding();

        public Message()
        {
        }

        public Message(string strMessage)
        {
            HL7Message = strMessage;
        }

        /// <summary>
        /// Parse the HL7 message in text format, throws HL7Exception if error occurs
        /// </summary>
        /// <returns>boolean</returns>
        public bool ParseMessage()
        {
            bool isValid = false;
            bool isParsed = false;
            try
            {
                isValid = ValidateMessage();
            }
            catch (HL7Exception ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new HL7Exception("Unhandeled Exceiption in validation - " + ex.Message, HL7Exception.BAD_MESSAGE);
            }

            if (isValid)
            {
                try
                {
                    if (allSegments == null || allSegments.Count <= 0)
                    {
                        allSegments = MessageHelper.SplitString(HL7Message, this.Encoding.SegmentDelimiter);
                    }

                    short SegSeqNo = 0;
                    foreach (string strSegment in allSegments)
                    {
                        Segment newSegment = new Segment(this.Encoding);
                        string segmentName = strSegment.Substring(0, 3);
                        newSegment.Name = segmentName;
                        newSegment.Value = strSegment;
                        newSegment.SequenceNo = SegSeqNo++;

                        this.AddNewSegment(newSegment);
                    }
                    this.SegmentCount = SegSeqNo;

                    string strSerializedMessage = string.Empty;
                    try
                    {
                        strSerializedMessage = SerializeMessage(false); 
                    }
                    catch (HL7Exception ex)
                    {
                        throw new HL7Exception("Failed to serialize parsed message with error - " + ex.Message, HL7Exception.PARSING_ERROR);
                    }

                    if (!string.IsNullOrEmpty(strSerializedMessage))
                    {
                        if (HL7Message.Equals(strSerializedMessage))
                            isParsed = true;
                    }
                    else
                    {
                        throw new HL7Exception("Unable to serialize to origional message - ", HL7Exception.PARSING_ERROR);
                    }
                }
                catch (Exception ex)
                {
                    throw new HL7Exception("Failed to parse the message with error - " + ex.Message, HL7Exception.PARSING_ERROR);
                }
            }
            return isParsed;
        }

        /// <summary>
        /// validates the HL7 message for basic syntax
        /// </summary>
        /// <returns>boolean</returns>
        private bool ValidateMessage()
        {
            try
            {
                if (!string.IsNullOrEmpty(HL7Message))
                {
                    HL7Message = HL7Message.Trim(messageTrimChars);

                    //check message length - MSH+Delemeters+12Fields in MSH
                    if (HL7Message.Length < 20)
                    {
                        throw new HL7Exception("Message Length too short" + HL7Message.Length + " fields.", HL7Exception.BAD_MESSAGE);
                    }

                    //check if message starts with header segment
                    if (!HL7Message.StartsWith("MSH"))
                    {
                        throw new HL7Exception("MSH Segment not found at the beggining of the message", HL7Exception.BAD_MESSAGE);
                    }

                    this.Encoding.EvaluateSegmentDelimiter(this.HL7Message);

                    //check Segment Name & 4th character of each segment
                    char fourthCharMSH = HL7Message[3];
                    allSegments = MessageHelper.SplitString(HL7Message, Encoding.SegmentDelimiter);

                    foreach (string strSegment in allSegments)
                    {
                        bool isValidSegName = false;
                        string segmentName = strSegment.Substring(0, 3);
                        string segNameRegEx = "[A-Z][A-Z][A-Z1-9]";
                        isValidSegName = System.Text.RegularExpressions.Regex.IsMatch(segmentName, segNameRegEx);

                        if (!isValidSegName)
                        {
                            throw new HL7Exception("Invalid Segment Name Found :" + strSegment, HL7Exception.BAD_MESSAGE);
                        }

                        char fourthCharSEG = strSegment[3];

                        if (fourthCharMSH != fourthCharSEG)
                        {
                            throw new HL7Exception("Invalid Segment Found :" + strSegment, HL7Exception.BAD_MESSAGE);
                        }
                    }

                    string _fieldDelimiters_Message = allSegments[0].Substring(3, 8 - 3);
                    this.Encoding.EvaluateDelimiters(_fieldDelimiters_Message);

                    //count field separators, MSH.12 is required so there should be at least 11 field separators in MSH
                    int countFieldSepInMSH = allSegments[0].Count(f => f == Encoding.FieldDelimiter);

                    if (countFieldSepInMSH < 11)
                    {
                        throw new HL7Exception("MSH segment doesn't contain all required fields", HL7Exception.BAD_MESSAGE);
                    }

                    //Find Message Version
                    var MSHFields = MessageHelper.SplitString(allSegments[0], Encoding.FieldDelimiter);
                    if (MSHFields.Count >= 12)
                    {
                        this.Version = MessageHelper.SplitString(MSHFields[11], Encoding.ComponentDelimiter)[0];
                    }
                    else
                    {
                        throw new HL7Exception("HL7 version not found in MSH Segment", HL7Exception.REQUIRED_FIELD_MISSING);
                    }

                    //Find Message Type & Trigger Event

                    try
                    {
                        string MSH_9 = MSHFields[8];
                        if (!string.IsNullOrEmpty(MSH_9))
                        {
                            var MSH_9_comps = MessageHelper.SplitString(MSH_9, this.Encoding.ComponentDelimiter);
                            if (MSH_9_comps.Count >= 3)
                            {
                                this.MessageStructure = MSH_9_comps[2];
                            }
                            else if (MSH_9_comps.Count > 0 && MSH_9_comps[0] != null && MSH_9_comps[0].Equals("ACK"))
                            {
                                this.MessageStructure = "ACK";
                            }
                            else if (MSH_9_comps.Count == 2)
                            {
                                this.MessageStructure = MSH_9_comps[0] + "_" + MSH_9_comps[1];
                            }
                            else
                            {
                                throw new HL7Exception("Message Type & Trigger Event value not found in message", HL7Exception.UNSUPPORTED_MESSAGE_TYPE);
                            }
                        }
                        else
                            throw new HL7Exception("MSH.10 not available", HL7Exception.UNSUPPORTED_MESSAGE_TYPE);
                    }
                    catch (System.IndexOutOfRangeException e)
                    {
                        throw new HL7Exception("Can't find message structure (MSH.9.3) - " + e.Message, HL7Exception.UNSUPPORTED_MESSAGE_TYPE);
                    }

                    try
                    {
                        this.MessageControlID = MSHFields[9];

                        if (string.IsNullOrEmpty(this.MessageControlID))
                            throw new HL7Exception("MSH.10 - Message Control ID not found", HL7Exception.REQUIRED_FIELD_MISSING);
                    }
                    catch (Exception ex)
                    {
                        throw new HL7Exception("Error occured while accessing MSH.10 - " + ex.Message, HL7Exception.REQUIRED_FIELD_MISSING);
                    }

                    try
                    {
                        this.ProcessingID = MSHFields[10];

                        if (string.IsNullOrEmpty(this.ProcessingID))
                            throw new HL7Exception("MSH.11 - Processing ID not found", HL7Exception.REQUIRED_FIELD_MISSING);
                    }
                    catch (Exception ex)
                    {
                        throw new HL7Exception("Error occured while accessing MSH.11 - " + ex.Message, HL7Exception.REQUIRED_FIELD_MISSING);
                    }
                }
                else
                    throw new HL7Exception("No Message Found", HL7Exception.BAD_MESSAGE);
            }
            catch (Exception ex)
            {
                throw new HL7Exception("Failed to validate the message with error - " + ex.Message, HL7Exception.BAD_MESSAGE);
            }

            return true;
        }

        /// <summary>
        /// Serialize the message in text format
        /// </summary>
        /// <param name="validate">Validate the message before serializing</param>
        /// <returns>string with HL7 message</returns>

        public string SerializeMessage(bool validate)
        {
            if (validate && !this.ValidateMessage())
                throw new HL7Exception("Failed to validate updated message", HL7Exception.BAD_MESSAGE);

            string strMessage = string.Empty;
            string currentSegName = string.Empty;;
            List<Segment> _segListOrdered = getAllSegmentsInOrder();

            try
            {
                try
                {
                    foreach (Segment seg in _segListOrdered)
                    {
                        currentSegName = seg.Name;

                        strMessage += seg.Name + this.Encoding.FieldDelimiter;
                        for (int i = 0; i<seg.FieldList.Count; i++)
                        {
                            if (i > 0)
                                strMessage += this.Encoding.FieldDelimiter;

                            var field = seg.FieldList[i];
                            if (field.IsDelimiters)
                            {
                                strMessage += field.Value;
                                continue;
                            }

                            if (field.HasRepetitions)
                            {
                                for (int j=0; j<field.RepeatitionList.Count; j++)
                                {
                                    if (j>0)
                                        strMessage += this.Encoding.RepeatDelimiter;

                                    strMessage += SerializeField(field.RepeatitionList[j]);
                                }
                            }
                            else
                                strMessage += SerializeField(field);

                        }
                        strMessage += this.Encoding.SegmentDelimiter;
                    }
                }
                catch (Exception ex)
                {
                    if (currentSegName == "MSH")
                        throw new HL7Exception("Failed to serialize MSH segment with error - " + ex.Message, HL7Exception.SERIALIZATION_ERROR);
                    else throw;
                }

                //strMessage = mshString + DefaultSegmentSeparatorString[msgSegmentSeparatorIndex] + strMessage;
                return strMessage.Trim(messageTrimChars);
            }
            catch (Exception ex)
            {
                throw new HL7Exception("Failed to serialize the message with error - " + ex.Message, HL7Exception.SERIALIZATION_ERROR);
            }
        }

        private string SerializeField(Field field)
        {
            var strMessage = string.Empty;

            if (field.ComponentList.Count > 0)
            {
                int indexCom = 0;
                foreach (Component com in field.ComponentList)
                {
                    indexCom++;
                    if (com.SubComponentList.Count > 0)
                    {
                        strMessage += string.Join(this.Encoding.SubComponentDelimiter.ToString(), com.SubComponentList.Select(sc => this.Encoding.Encode(sc.Value)));
                    }
                    else
                        strMessage += this.Encoding.Encode(com.Value);

                    if (indexCom < field.ComponentList.Count)
                        strMessage += this.Encoding.ComponentDelimiter;
                }
            }
            else
                strMessage = this.Encoding.Encode(field.Value);

            return strMessage;
        }

        //get all segments in order as they appear in origional message
        //like this is the usual order IN1|1 IN2|1 IN1|2 IN2|2
        //segmentlist stores segments based on key, so all the IN1s will be stored together without mainting the order
        private List<Segment> getAllSegmentsInOrder()
        {
            List<Segment> _list = new List<Segment>();

            foreach (string segName in SegmentList.Keys)
            {
                foreach (Segment seg in SegmentList[segName])
                {
                    _list.Add(seg);
                }
            }

            return _list.OrderBy(o => o.SequenceNo).ToList();
        }
        
        /// <summary>
        /// Get the Value of specific Field/Component/SubCpomponent, throws error if field/component index is not valid
        /// </summary>
        /// <param name="strValueFormat">Field/Component position in format SEGMENTNAME.FieldIndex.ComponentIndex.SubComponentIndex example PID.5.2</param>
        /// <returns>Value of specified field/component/subcomponent</returns>
        public string getValue(string strValueFormat)
        {
            bool isValid = false;

            string segmentName = string.Empty;
            int fieldIndex = 0;
            int componentIndex = 0;
            int subComponentIndex = 0;
            int comCount = 0;
            string strValue = string.Empty;

            List<string> AllComponents = MessageHelper.SplitString(strValueFormat, new char[] { '.' });
            comCount = AllComponents.Count;

            isValid = validateValueFormat(AllComponents);

            if (isValid)
            {
                segmentName = AllComponents[0];
                if (SegmentList.ContainsKey(segmentName))
                {
                    if (comCount == 4)
                    {
                        Int32.TryParse(AllComponents[1], out fieldIndex);
                        Int32.TryParse(AllComponents[2], out componentIndex);
                        Int32.TryParse(AllComponents[3], out subComponentIndex);

                        try
                        {
                            strValue = SegmentList[segmentName].First().FieldList[fieldIndex - 1].ComponentList[componentIndex - 1].SubComponentList[subComponentIndex - 1].Value;
                        }
                        catch (Exception ex)
                        {
                            throw new HL7Exception("SubComponent not available - " + strValueFormat + " Error: " + ex.Message);
                        }
                    }
                    else if (comCount == 3)
                    {
                        Int32.TryParse(AllComponents[1], out fieldIndex);
                        Int32.TryParse(AllComponents[2], out componentIndex);

                        try
                        {
                            strValue = SegmentList[segmentName].First().FieldList[fieldIndex - 1].ComponentList[componentIndex - 1].Value;
                        }
                        catch (Exception ex)
                        {
                            throw new HL7Exception("Component not available - " + strValueFormat + " Error: " + ex.Message);
                        }
                    }
                    else if (comCount == 2)
                    {
                        Int32.TryParse(AllComponents[1], out fieldIndex);
                        try
                        {
                            strValue = SegmentList[segmentName].First().FieldList[fieldIndex - 1].Value;
                        }
                        catch (Exception ex)
                        {
                            throw new HL7Exception("Field not available - " + strValueFormat + " Error: " + ex.Message);
                        }
                    }
                    else
                    {
                        try
                        {
                            strValue = SegmentList[segmentName].First().Value;
                        }
                        catch (Exception ex)
                        {
                            throw new HL7Exception("Segment Value not available - " + strValueFormat + " Error: " + ex.Message);
                        }
                    }
                }
                else
                    throw new HL7Exception("Segment Name not available");
            }
            else
                throw new HL7Exception("Request Format is not valid");

            return strValue;
        }

        /// <summary>
        /// Sets the Value of specific Field/Component/SubCpomponent, throws error if field/component index is not valid
        /// </summary>
        /// <param name="strValueFormat">Field/Component position in format SEGMENTNAME.FieldIndex.ComponentIndex.SubComponentIndex example PID.5.2</param>
        /// <param name="strValue">Value for the specified field/component</param>
        /// <returns>boolean</returns>
        public bool setValue(string strValueFormat, string strValue)
        {
            bool isValid = false;
            bool isSet = false;

            string segmentName = string.Empty;
            int fieldIndex = 0;
            int componentIndex = 0;
            int subComponentIndex = 0;
            int comCount = 0;

            List<string> AllComponents = MessageHelper.SplitString(strValueFormat, new char[] { '.' });
            comCount = AllComponents.Count;

            isValid = validateValueFormat(AllComponents);

            if (isValid)
            {
                segmentName = AllComponents[0];
                if (SegmentList.ContainsKey(segmentName))
                {
                    if (comCount == 4)
                    {
                        Int32.TryParse(AllComponents[1], out fieldIndex);
                        Int32.TryParse(AllComponents[2], out componentIndex);
                        Int32.TryParse(AllComponents[3], out subComponentIndex);

                        try
                        {
                            SegmentList[segmentName].First().FieldList[fieldIndex - 1].ComponentList[componentIndex - 1].SubComponentList[subComponentIndex - 1].Value = strValue;
                            isSet = true;
                        }
                        catch (Exception ex)
                        {
                            throw new HL7Exception("SubComponent not available - " + strValueFormat + " Error: " + ex.Message);
                        }
                    }
                    else if (comCount == 3)
                    {
                        Int32.TryParse(AllComponents[1], out fieldIndex);
                        Int32.TryParse(AllComponents[2], out componentIndex);

                        try
                        {
                            SegmentList[segmentName].First().FieldList[fieldIndex - 1].ComponentList[componentIndex - 1].Value = strValue;
                            isSet = true;
                        }
                        catch (Exception ex)
                        {
                            throw new HL7Exception("Component not available - " + strValueFormat + " Error: " + ex.Message);
                        }
                    }
                    else if (comCount == 2)
                    {
                        Int32.TryParse(AllComponents[1], out fieldIndex);
                        try
                        {
                            SegmentList[segmentName].First().FieldList[fieldIndex - 1].Value = strValue;
                            isSet = true;
                        }
                        catch (Exception ex)
                        {
                            throw new HL7Exception("Field not available - " + strValueFormat + " Error: " + ex.Message);
                        }
                    }
                    else
                    {
                        throw new HL7Exception("Cannot overwrite a Segment Value");
                    }
                }
                else
                    throw new HL7Exception("Segment Name not available");
            }
            else
                throw new HL7Exception("Request Format is not valid");

            return isSet;
        }

        private bool validateValueFormat(List<string> AllComponents)
        {
            string segNameRegEx = "[A-Z][A-Z][A-Z1-9]";
            string otherRegEx = @"^[1-9]([0-9]{1,2})?$";
            bool isValid = false;

            if (AllComponents.Count > 0)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(AllComponents[0], segNameRegEx))
                {
                    for (int i = 1; i < AllComponents.Count; i++)
                    {
                        if (System.Text.RegularExpressions.Regex.IsMatch(AllComponents[i], otherRegEx))
                            isValid = true;
                        else
                            return false;

                    }
                }
                else
                    isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// check if specified field has components
        /// </summary>
        /// <param name="strValueFormat">Field/Component position in format SEGMENTNAME.FieldIndex.ComponentIndex.SubComponentIndex example PID.5.2</param>
        /// <returns>boolean</returns>
        public bool IsComponentized(string strValueFormat)
        {
            bool isComponentized = false;
            bool isValid = false;

            string segmentName = string.Empty;
            int fieldIndex = 0;
            int comCount = 0;

            List<string> AllComponents = MessageHelper.SplitString(strValueFormat, new char[] { '.' });
            comCount = AllComponents.Count;

            isValid = validateValueFormat(AllComponents);

            if (isValid)
            {
                segmentName = AllComponents[0];
                if (comCount >= 2)
                {
                    try
                    {
                        Int32.TryParse(AllComponents[1], out fieldIndex);

                        isComponentized = SegmentList[segmentName].First().FieldList[fieldIndex - 1].IsComponentized;
                    }
                    catch (Exception ex)
                    {
                        throw new HL7Exception("Field not available - " + strValueFormat + " Error: " + ex.Message);
                    }
                }
                else
                    throw new HL7Exception("Field not identified in request");
            }
            else
                throw new HL7Exception("Request Format is not valid");

            return isComponentized;
        }

        /// <summary>
        /// check if specified fields has repeatitions
        /// </summary>
        /// <param name="strValueFormat">Field/Component position in format SEGMENTNAME.FieldIndex.ComponentIndex.SubComponentIndex example PID.5.2</param>
        /// <returns>boolean</returns>
        public bool HasRepeatitions(string strValueFormat)
        {
            bool hasRepeatitions = false;
            bool isValid = false;

            string segmentName = string.Empty;
            int fieldIndex = 0;
            int comCount = 0;

            List<string> AllComponents = MessageHelper.SplitString(strValueFormat, new char[] { '.' });
            comCount = AllComponents.Count;

            isValid = validateValueFormat(AllComponents);

            if (isValid)
            {
                segmentName = AllComponents[0];
                if (comCount >= 2)
                {
                    try
                    {
                        Int32.TryParse(AllComponents[1], out fieldIndex);

                        hasRepeatitions = SegmentList[segmentName].First().FieldList[fieldIndex - 1].HasRepetitions;
                    }
                    catch (Exception ex)
                    {
                        throw new HL7Exception("Field not available - " + strValueFormat + " Error: " + ex.Message);
                    }
                }
                else
                    throw new HL7Exception("Field not identified in request");
            }
            else
                throw new HL7Exception("Request Format is not valid");

            return hasRepeatitions;
        }

        /// <summary>
        /// check if specified component has sub components
        /// </summary>
        /// <param name="strValueFormat">Field/Component position in format SEGMENTNAME.FieldIndex.ComponentIndex.SubComponentIndex example PID.5.2</param>
        /// <returns>boolean</returns>
        public bool IsSubComponentized(string strValueFormat)
        {
            bool isSubComponentized = false;
            bool isValid = false;

            string segmentName = string.Empty;
            int fieldIndex = 0;
            int componentIndex = 0;
            int comCount = 0;

            List<string> AllComponents = MessageHelper.SplitString(strValueFormat, new char[] { '.' });
            comCount = AllComponents.Count;

            isValid = validateValueFormat(AllComponents);

            if (isValid)
            {
                segmentName = AllComponents[0];
                if (comCount >= 3)
                {
                    try
                    {
                        Int32.TryParse(AllComponents[1], out fieldIndex);
                        Int32.TryParse(AllComponents[2], out componentIndex);

                        isSubComponentized = SegmentList[segmentName].First().FieldList[fieldIndex - 1].ComponentList[componentIndex - 1].IsSubComponentized;
                    }
                    catch (Exception ex)
                    {
                        throw new HL7Exception("Component not available - " + strValueFormat + " Error: " + ex.Message);
                    }
                }
                else
                    throw new HL7Exception("Component not identified in request");
            }
            else
                throw new HL7Exception("Request Format is not valid");

            return isSubComponentized;
        }

        /// <summary>
        /// get acknowledgement message for this message
        /// </summary>
        /// <returns>string with ack message</returns>
        public string getACK()
        {
            string ackMsg = string.Empty;
            if (this.MessageStructure != "ACK")
            {
                var dateString = MessageHelper.LongDateWithFractionOfSecond(DateTime.Now);
                var msh = this.SegmentList["MSH"].First();
                var delim = this.Encoding.FieldDelimiter;

                ackMsg += "MSH" + this.Encoding.AllDelimiters + delim + msh.FieldList[4].Value + delim + msh.FieldList[5].Value + delim + msh.FieldList[2].Value + delim 
                + msh.FieldList[3].Value + delim + dateString + delim + msh.FieldList[7].Value + delim + "ACK" + delim + this.MessageControlID + delim 
                + this.ProcessingID + delim + this.Version + this.Encoding.SegmentDelimiter;

                ackMsg += "MSA" + delim + "AA" + delim + this.MessageControlID + this.Encoding.SegmentDelimiter;
            }
            return ackMsg;
        }

        /// <summary>
        /// get negative ack for this message
        /// </summary>
        /// <param name="code">ack code like AR, AE</param>
        /// <param name="errMsg">error message to be sent with ACK</param>
        /// <returns>string with ack message</returns>
        public string getNACK(string code, string errMsg)
        {
            string ackMsg = string.Empty;
            if (this.MessageStructure != "ACK")
            {
                var dateString = MessageHelper.LongDateWithFractionOfSecond(DateTime.Now);
                var msh = this.SegmentList["MSH"].First();
                var delim = this.Encoding.FieldDelimiter;
                
                ackMsg = "MSH" + this.Encoding.AllDelimiters + delim + msh.FieldList[4].Value + delim + msh.FieldList[5].Value + delim + msh.FieldList[2].Value + delim 
                + msh.FieldList[3].Value + delim + dateString + delim + msh.FieldList[7].Value + delim + "ACK" + delim + this.MessageControlID + delim 
                + this.ProcessingID + delim + this.Version + this.Encoding.SegmentDelimiter;
                
                ackMsg += "MSA" + delim + code + delim + this.MessageControlID + delim + errMsg + this.Encoding.SegmentDelimiter;
            }
            return ackMsg;
        }

        public bool AddNewSegment(Segment newSeg)
        {
            try
            {
                newSeg.SequenceNo = SegmentCount++;
                if (!SegmentList.ContainsKey(newSeg.Name))
                    SegmentList[newSeg.Name] = new List<Segment>();

                SegmentList[newSeg.Name].Add(newSeg);
                return true;
            }
            catch (Exception ex)
            {
                SegmentCount--;
                throw new HL7Exception("Unable to add new segment Error - " + ex.Message);
            }
        }

        public List<Segment> Segments()
        {
            return getAllSegmentsInOrder();
        }

        public List<Segment> Segments(string segmentName)
        {
            return getAllSegmentsInOrder().FindAll(o=> o.Name.Equals(segmentName));
        }

        public Segment DefaultSegment(string segmentName)
        {
            return getAllSegmentsInOrder().First(o => o.Name.Equals(segmentName));
        }
    }
}
