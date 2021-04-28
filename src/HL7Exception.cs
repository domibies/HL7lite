﻿using System;

namespace HL7lite
{
    public class HL7Exception : Exception
    {
        public const string REQUIRED_FIELD_MISSING = "Validation Error - Required field missing in message";
        public const string UNSUPPORTED_MESSAGE_TYPE = "Validation Error - Message Type not supported by this implementation";
        public const string BAD_MESSAGE = "Validation Error - Bad Message";
        public const string PARSING_ERROR = "Parsing Error";
        public const string SERIALIZATION_ERROR = "Serialization Error";
        public const string SEGMENT_TOO_SHORT = "Validation Error - Segment Too Short";
        public const string INVALID_REQUEST = "Validation Error - Request Format Not Valid";
        
        public string ErrorCode { get; set; }

        public HL7Exception(string message) : base(message)
        {
        }

        public HL7Exception(string message, string Code) : base(message)
        {
            ErrorCode = Code;
        }

        public override string ToString()
        {
            return ErrorCode + " : " + Message;
        }
    }
}