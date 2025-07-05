using System;
using System.Text.RegularExpressions;

namespace HL7lite.Fluent.Querying
{
    /// <summary>
    /// Provides fluent access to HL7 message elements using path syntax.
    /// Wraps the legacy GetValue/SetValue/PutValue methods with a modern, chainable API.
    /// </summary>
    public class PathAccessor
    {
        private readonly Message _message;
        private readonly string _path;
        private readonly FluentMessage _fluentMessage;
        
        /// <summary>
        /// Internal class to hold parsed path information (compatible with all .NET Standard versions)
        /// </summary>
        private class ParsedPath
        {
            public string SegmentName { get; set; }
            public int SegmentRepetition { get; set; } = 1;
            public string RemainingPath { get; set; }
            
            public ParsedPath(string segmentName, int segmentRepetition, string remainingPath)
            {
                SegmentName = segmentName;
                SegmentRepetition = segmentRepetition;
                RemainingPath = remainingPath;
            }
        }

        internal PathAccessor(Message message, string path, FluentMessage fluentMessage)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _fluentMessage = fluentMessage ?? throw new ArgumentNullException(nameof(fluentMessage));
        }

        /// <summary>
        /// Gets the raw value at the specified path with structural delimiters and encoded characters.
        /// For human-readable format, use ToString(). Returns empty string for non-existent paths.
        /// Uses the enhanced path parser supporting segment and field repetitions.
        /// </summary>
        public string Raw
        {
            get
            {
                var value = PathParser.GetValue(_message, _path);
                
                // PathParser may return values from core API which converts "" to null
                // Convert back to maintain consistency with fluent API
                if (value == null)
                    return _message.Encoding.PresentButNull;
                    
                // Return raw value without decoding (consistent with other fluent accessors)
                return value;
            }
        }

        /// <summary>
        /// Gets whether the element at this path exists in the message.
        /// Uses the enhanced path parser supporting segment and field repetitions.
        /// </summary>
        public bool Exists
        {
            get
            {
                return PathParser.PathExists(_message, _path);
            }
        }

        /// <summary>
        /// Gets whether the element at this path has a non-empty value.
        /// Returns false for non-existent paths and HL7 null values.
        /// Uses the enhanced path parser supporting segment and field repetitions.
        /// </summary>
        public bool HasValue
        {
            get
            {
                var value = PathParser.GetValue(_message, _path);
                return !string.IsNullOrEmpty(value) && value != _message.Encoding.PresentButNull;
            }
        }

        /// <summary>
        /// Gets whether the element at this path contains an HL7 null value ("").
        /// Uses the enhanced path parser supporting segment and field repetitions.
        /// </summary>
        public bool IsNull
        {
            get
            {
                if (!Exists) return false;
                
                var value = PathParser.GetValue(_message, _path);
                return value == _message.Encoding.PresentButNull; // HL7 null is represented by the encoding's PresentButNull value
            }
        }

        /// <summary>
        /// Sets the value at the specified path, automatically encoding any HL7 delimiter characters.
        /// Creates missing elements automatically. This method never throws exceptions for valid paths
        /// and maintains consistency with the rest of the fluent API.
        /// </summary>
        /// <param name="value">The value to set. Delimiter characters will be automatically encoded.</param>
        /// <returns>The FluentMessage for method chaining</returns>
        public FluentMessage Set(string value)
        {
            PathParser.SetValue(_message, _path, value);
            return _fluentMessage;
        }

        /// <summary>
        /// Conditionally sets the value at the specified path, creating missing elements if needed.
        /// Only sets the value if the condition is true. Delimiter characters are automatically encoded.
        /// </summary>
        /// <param name="value">The value to set if condition is true. Delimiter characters will be automatically encoded.</param>
        /// <param name="condition">The condition that must be true to set the value</param>
        /// <returns>The FluentMessage for method chaining</returns>
        public FluentMessage SetIf(string value, bool condition)
        {
            if (condition)
            {
                PathParser.SetValue(_message, _path, value);
            }
            return _fluentMessage;
        }

        /// <summary>
        /// Sets the raw HL7 value at the specified path with validation based on the target level.
        /// This method validates that the value doesn't contain invalid delimiters for the target element type.
        /// Use this for pre-encoded data or when building structured values.
        /// </summary>
        /// <param name="value">The raw HL7 value to set</param>
        /// <returns>The FluentMessage for method chaining</returns>
        /// <exception cref="ArgumentException">If value contains delimiters invalid for the target level</exception>
        public FluentMessage SetRaw(string value)
        {
            // Basic validation - more detailed validation would require parsing the path
            // to determine the target level (field, component, or subcomponent)
            if (value != null)
            {
                // Always validate against field and repetition delimiters regardless of target level
                if (value.Contains(_message.Encoding.FieldDelimiter.ToString()))
                {
                    throw new ArgumentException(
                        $"Path value cannot contain field delimiter '{_message.Encoding.FieldDelimiter}'. " +
                        "Use separate path assignments or encode the delimiter.");
                }
                
                if (value.Contains(_message.Encoding.RepeatDelimiter.ToString()))
                {
                    throw new ArgumentException(
                        $"Path value cannot contain repetition delimiter '{_message.Encoding.RepeatDelimiter}'. " +
                        "Use separate repetitions or encode the delimiter.");
                }
            }
            
            PathParser.SetValue(_message, _path, value);
            return _fluentMessage;
        }

        /// <summary>
        /// Sets the path to an HL7 null value, creating missing elements if needed.
        /// </summary>
        /// <returns>The FluentMessage for method chaining</returns>
        public FluentMessage SetNull()
        {
            PathParser.SetValue(_message, _path, _message.Encoding.PresentButNull);
            return _fluentMessage;
        }

        /// <summary>
        /// Returns a human-readable representation of the value at this path.
        /// Decodes any encoded delimiters and replaces structural delimiters with spaces.
        /// HL7 null values are displayed as "&lt;null&gt;".
        /// </summary>
        public override string ToString()
        {
            var rawValue = this.Raw;
            
            // Handle empty/missing
            if (string.IsNullOrEmpty(rawValue))
                return "";
            
            // First replace structural delimiters with spaces (before decoding)
            var delimiters = new[] {
                _message.Encoding.FieldDelimiter,
                _message.Encoding.ComponentDelimiter,
                _message.Encoding.RepeatDelimiter,
                _message.Encoding.SubComponentDelimiter
            };
            
            var processed = rawValue;
            // Replace any sequence of structural delimiters with a single space
            var delimiterPattern = "[" + Regex.Escape(new string(delimiters)) + "]+";
            processed = Regex.Replace(processed, delimiterPattern, " ");
            
            // Then decode encoded delimiters (e.g., \T\ → &) - these become literal characters
            var decoded = _message.Encoding.Decode(processed);
            
            // Replace HL7 nulls with readable placeholder
            decoded = decoded.Replace(_message.Encoding.PresentButNull, "<null>");
            
            return decoded.Trim();
        }
        
    }
}