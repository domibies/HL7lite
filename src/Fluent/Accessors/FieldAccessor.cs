using System;
using System.Collections.Generic;

namespace HL7lite.Fluent.Accessors
{
    /// <summary>
    /// Provides fluent access to HL7 fields with null-safe operations
    /// </summary>
    public class FieldAccessor
    {
        private readonly Message _message;
        private readonly string _segmentName;
        private readonly int _fieldIndex;
        private readonly string _fieldPath;
        private readonly string _rawValue;
        private readonly bool _exists;
        private readonly Dictionary<int, ComponentAccessor> _componentCache = new Dictionary<int, ComponentAccessor>();

        internal FieldAccessor(Message message, string segmentName, int fieldIndex)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _segmentName = segmentName ?? throw new ArgumentNullException(nameof(segmentName));
            _fieldIndex = fieldIndex;
            _fieldPath = $"{_segmentName}.{_fieldIndex}";

            // Use the existing API to check if field exists and get its value
            try
            {
                _rawValue = _message.GetValue(_fieldPath);
                _exists = true;
            }
            catch (HL7Exception)
            {
                // Field doesn't exist
                _rawValue = "";
                _exists = false;
            }
        }

        /// <summary>
        /// Gets whether this field exists in the segment
        /// </summary>
        public bool Exists => _exists;

        /// <summary>
        /// Gets the field value. Returns empty string for non-existent fields, null for explicit HL7 nulls.
        /// </summary>
        public string Value
        {
            get
            {
                if (!_exists)
                    return "";
                
                // Handle HL7 null representation (two double quotes)
                if (_rawValue == "\"\"")
                    return null;
                    
                return _rawValue ?? "";
            }
        }

        /// <summary>
        /// Gets the field value, never returns null (converts nulls to empty string)
        /// </summary>
        public string SafeValue => Value ?? "";

        /// <summary>
        /// Gets whether the field is explicitly null in HL7 format ("")
        /// </summary>
        public bool IsNull => _exists && _rawValue == "\"\"";

        /// <summary>
        /// Gets whether the field exists but is empty
        /// </summary>
        public bool IsEmpty => _exists && string.IsNullOrEmpty(_rawValue) && !IsNull;

        /// <summary>
        /// Gets whether the field exists and has a non-null, non-empty value
        /// </summary>
        public bool HasValue => _exists && !string.IsNullOrEmpty(_rawValue) && !IsNull;

        /// <summary>
        /// Gets a component accessor by index (1-based).
        /// </summary>
        /// <param name="componentIndex">The 1-based component index.</param>
        /// <returns>A ComponentAccessor for the specified component.</returns>
        public ComponentAccessor this[int componentIndex]
        {
            get
            {
                if (!_componentCache.ContainsKey(componentIndex))
                {
                    _componentCache[componentIndex] = new ComponentAccessor(
                        _message, _segmentName, _fieldIndex, componentIndex);
                }
                return _componentCache[componentIndex];
            }
        }

        /// <summary>
        /// Gets a component accessor by index (1-based).
        /// </summary>
        /// <param name="componentIndex">The 1-based component index.</param>
        /// <returns>A ComponentAccessor for the specified component.</returns>
        public ComponentAccessor Component(int componentIndex)
        {
            return this[componentIndex];
        }
    }
}