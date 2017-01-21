// Based on https://blog.interfaceware.com/delimiter-escape-sequences/

namespace HL7.Dotnetcore
{
    public class Encoding
    {
        public string AllDelimiters { get; private set; } = @"|^~\&";
        public char FieldDelimiter { get; set; } = '|'; // \F\
        public char ComponentDelimiter { get; set; } = '^'; // \S\
        public char RepeatDelimiter { get; set; } = '~';  // \R\
        public char EscapeDelimiter { get; set; } = '\\'; // \E\
        public char SubComponentDelimiter { get; set; } = '&'; // \T\
        public string SegmentDelimiter { get; set; } = "\r";

        public Encoding()
        {            
        }

        public void EvaluateDelimiters(string delimiters)
        {
            this.FieldDelimiter = delimiters[0];
            this.ComponentDelimiter = delimiters[1];
            this.RepeatDelimiter = delimiters[2];
            this.EscapeDelimiter = delimiters[3];
            this.SubComponentDelimiter = delimiters[4];
        }

        public void EvaluateSegmentDelimiter(string message)
        {
            string[] delimiters = new [] { "\r\n", "\n\r", "\r", "\n" };

            foreach (var delim in delimiters)
            {
                if (message.Contains(delim))
                {
                    this.SegmentDelimiter = delim;
                    return;
                }
            }

            throw new HL7Exception("Segment delimiter not found in message", HL7Exception.BAD_MESSAGE);
        }

        public string Encode(string field)
        {
            return field;
        }

        public string Decode(string field)
        {
            return field;
        }
    }
}

/*
Cursor Return (ASCII 13) "\.br\"

\Cxxyy\	Single-byte character set escape sequence with two hexadecimal values not converted
\H\	Start highlighting not converted
\Mxxyyzz\	Multi-byte character set escape sequence with two or three hexadecimal values (zz is optional) not converted
\N\	Normal text (end highlighting) not converted
\Xdd…\	Hexadecimal data (dd must be hexadecimal characters) converted to the characters identified by each pair of digits
\Zdd…\	Locally defined escape sequence not converted
*/    
