using System;

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

        internal PathAccessor(Message message, string path, FluentMessage fluentMessage)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _fluentMessage = fluentMessage ?? throw new ArgumentNullException(nameof(fluentMessage));
        }

        /// <summary>
        /// Gets the value at the specified path. Returns null for HL7 null values, empty string for non-existent paths.
        /// </summary>
        public string Value
        {
            get
            {
                try
                {
                    return _message.GetValue(_path);
                }
                catch
                {
                    // If path doesn't exist, return empty string (consistent with fluent API behavior)
                    return "";
                }
            }
        }

        /// <summary>
        /// Gets whether the element at this path exists in the message.
        /// </summary>
        public bool Exists
        {
            get
            {
                try
                {
                    return _message.ValueExists(_path);
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets whether the element at this path has a non-empty value.
        /// Returns false for non-existent paths and HL7 null values.
        /// </summary>
        public bool HasValue
        {
            get
            {
                try
                {
                    var value = _message.GetValue(_path);
                    return !string.IsNullOrEmpty(value);
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets whether the element at this path contains an HL7 null value ("").
        /// </summary>
        public bool IsNull
        {
            get
            {
                if (!Exists) return false;
                
                try
                {
                    var value = _message.GetValue(_path);
                    return value == _message.Encoding.PresentButNull; // HL7 null is represented by the encoding's PresentButNull value
                }
                catch
                {
                    return false;
                }
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
            _message.PutValue(_path, value);
            return _fluentMessage;
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
                _message.PutValue(_path, value);
            }
            return _fluentMessage;
        }

        /// <summary>
        /// Sets the value at the specified path after encoding any HL7 delimiter characters.
        /// Use this method when your value contains characters like |, ^, ~, \, or &
        /// that need to be safely stored in the HL7 message.
        /// </summary>
        /// <param name="value">The value to encode and set</param>
        /// <returns>The FluentMessage for method chaining</returns>
        public FluentMessage SetEncoded(string value)
        {
            if (value == null)
            {
                return Set("");
            }

            var encodedValue = _message.Encoding.Encode(value);
            return Set(encodedValue);
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
                return SetEncoded(value);
            }
            return _fluentMessage;
        }

        /// <summary>
        /// Sets the path to an HL7 null value, creating missing elements if needed.
        /// </summary>
        /// <returns>The FluentMessage for method chaining</returns>
        public FluentMessage SetNull()
        {
            _message.PutValue(_path, _message.Encoding.PresentButNull);
            return _fluentMessage;
        }

        /// <summary>
        /// Returns the path string for debugging purposes.
        /// </summary>
        public override string ToString() => _path;
    }
}