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
        /// Gets the value at the specified path. Returns null for HL7 null values, empty string for non-existent paths.
        /// Uses the enhanced path parser supporting segment and field repetitions.
        /// </summary>
        public string Value
        {
            get
            {
                var value = PathParser.GetValue(_message, _path);
                
                // Convert HL7 null to actual null (consistent with fluent API)
                if (value == _message.Encoding.PresentButNull)
                    return null;
                    
                // Decode any HL7-encoded delimiter characters
                return string.IsNullOrEmpty(value) ? value : _message.Encoding.Decode(value);
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
        /// Sets the value at the specified path, creating missing elements automatically.
        /// This method never throws exceptions for valid paths and maintains consistency
        /// with the rest of the fluent API.
        /// </summary>
        /// <param name="value">The value to set</param>
        /// <returns>The FluentMessage for method chaining</returns>
        public FluentMessage Set(string value)
        {
            return SetPathValueInternal(_path, value, false);
        }

        /// <summary>
        /// Conditionally sets the value at the specified path, creating missing elements if needed.
        /// Only sets the value if the condition is true.
        /// </summary>
        /// <param name="value">The value to set</param>
        /// <param name="condition">The condition that must be true to set the value</param>
        /// <returns>The FluentMessage for method chaining</returns>
        public FluentMessage SetIf(string value, bool condition)
        {
            if (condition)
            {
                return SetPathValueInternal(_path, value, false);
            }
            return _fluentMessage;
        }

        /// <summary>
        /// Sets the value at the specified path after encoding any HL7 delimiter characters.
        /// Use this method when your value contains characters like |, ^, ~, \, or &amp;
        /// that need to be safely stored in the HL7 message.
        /// </summary>
        /// <param name="value">The value to encode and set</param>
        /// <returns>The FluentMessage for method chaining</returns>
        public FluentMessage SetEncoded(string value)
        {
            return SetPathValueInternal(_path, value, true);
        }

        /// <summary>
        /// Conditionally sets the encoded value at the specified path, creating missing elements if needed.
        /// Only sets the value if the condition is true. The value is encoded before setting.
        /// </summary>
        /// <param name="value">The value to encode and set</param>
        /// <param name="condition">The condition that must be true to set the value</param>
        /// <returns>The FluentMessage for method chaining</returns>
        public FluentMessage SetEncodedIf(string value, bool condition)
        {
            if (condition)
            {
                return SetPathValueInternal(_path, value, true);
            }
            return _fluentMessage;
        }

        /// <summary>
        /// Sets the path to an HL7 null value, creating missing elements if needed.
        /// </summary>
        /// <returns>The FluentMessage for method chaining</returns>
        public FluentMessage SetNull()
        {
            return SetPathValueInternal(_path, _message.Encoding.PresentButNull, false);
        }

        /// <summary>
        /// Returns the path string for debugging purposes.
        /// </summary>
        public override string ToString() => _path;
        
        /// <summary>
        /// Internal method that handles path value setting with automatic segment creation.
        /// Implements DRY principle for all Set operations using the enhanced PathParser.
        /// </summary>
        /// <param name="path">The path to set</param>
        /// <param name="value">The value to set</param>
        /// <param name="isEncoded">Whether the value should be encoded</param>
        /// <returns>The FluentMessage for method chaining</returns>
        private FluentMessage SetPathValueInternal(string path, string value, bool isEncoded)
        {
            bool success = isEncoded 
                ? PathParser.SetEncodedValue(_message, path, value)
                : PathParser.SetValue(_message, path, value);
                
            // Always return the fluent message for chaining (consistent with fluent API behavior)
            return _fluentMessage;
        }
    }
}