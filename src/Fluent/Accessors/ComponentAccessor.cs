using System;
using System.Collections.Generic;

namespace HL7lite.Fluent.Accessors
{
    /// <summary>
    /// Provides safe, fluent access to HL7 component values within a field.
    /// Components are accessed using 1-based indexing to match HL7 conventions.
    /// </summary>
    public class ComponentAccessor
    {
        private readonly Message _message;
        private readonly string _segmentName;
        private readonly int _fieldIndex;
        private readonly int _componentIndex;
        private readonly Dictionary<int, SubComponentAccessor> _subComponentCache = new Dictionary<int, SubComponentAccessor>();

        /// <summary>
        /// Initializes a new instance of the ComponentAccessor class.
        /// </summary>
        /// <param name="message">The HL7 message containing the component.</param>
        /// <param name="segmentName">The name of the segment containing the field.</param>
        /// <param name="fieldIndex">The 1-based field index.</param>
        /// <param name="componentIndex">The 1-based component index.</param>
        public ComponentAccessor(Message message, string segmentName, int fieldIndex, int componentIndex)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _segmentName = segmentName ?? throw new ArgumentNullException(nameof(segmentName));
            _fieldIndex = fieldIndex;
            _componentIndex = componentIndex;
        }

        /// <summary>
        /// Gets the value of the component. Returns null for HL7 null values ("") and empty string for non-existent components.
        /// </summary>
        public string Value
        {
            get
            {
                if (_componentIndex <= 0)
                    return "";

                try
                {
                    var path = $"{_segmentName}.{_fieldIndex}.{_componentIndex}";
                    var value = _message.GetValue(path);
                    
                    // HL7 null is represented as "" in the message but should return null
                    return value == "\"\"" ? null : value;
                }
                catch
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// Gets the value of the component, never returning null. Returns empty string for both null and non-existent components.
        /// </summary>
        public string SafeValue => Value ?? "";

        /// <summary>
        /// Gets whether the component exists in the message.
        /// </summary>
        public bool Exists
        {
            get
            {
                if (_componentIndex <= 0)
                    return false;

                try
                {
                    var path = $"{_segmentName}.{_fieldIndex}.{_componentIndex}";
                    var segment = _message.DefaultSegment(_segmentName);
                    if (segment == null)
                        return false;

                    var field = segment.Fields(_fieldIndex);
                    if (field == null)
                        return false;

                    return field.ComponentList.Count >= _componentIndex;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets whether the component is explicitly set to HL7 null ("").
        /// </summary>
        public bool IsNull => Value == null && Exists;

        /// <summary>
        /// Gets whether the component exists but contains an empty string.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                if (!Exists)
                    return false;
                var val = Value;
                return val != null && val == "";
            }
        }

        /// <summary>
        /// Gets whether the component exists and has a non-empty value.
        /// </summary>
        public bool HasValue
        {
            get
            {
                var val = Value;
                return val != null && val != "";
            }
        }

        /// <summary>
        /// Gets a subcomponent accessor by index (1-based).
        /// </summary>
        /// <param name="subComponentIndex">The 1-based subcomponent index.</param>
        /// <returns>A SubComponentAccessor for the specified subcomponent.</returns>
        public SubComponentAccessor this[int subComponentIndex]
        {
            get
            {
                if (!_subComponentCache.ContainsKey(subComponentIndex))
                {
                    _subComponentCache[subComponentIndex] = new SubComponentAccessor(
                        _message, _segmentName, _fieldIndex, _componentIndex, subComponentIndex);
                }
                return _subComponentCache[subComponentIndex];
            }
        }

        /// <summary>
        /// Gets a subcomponent accessor by index (1-based).
        /// </summary>
        /// <param name="subComponentIndex">The 1-based subcomponent index.</param>
        /// <returns>A SubComponentAccessor for the specified subcomponent.</returns>
        public SubComponentAccessor SubComponent(int subComponentIndex)
        {
            return this[subComponentIndex];
        }
    }
}