using System;

namespace HL7lite.Fluent.Accessors
{
    /// <summary>
    /// Provides safe, fluent access to HL7 subcomponent values within a component.
    /// Subcomponents are accessed using 1-based indexing to match HL7 conventions.
    /// </summary>
    public class SubComponentAccessor
    {
        private readonly Message _message;
        private readonly string _segmentName;
        private readonly int _fieldIndex;
        private readonly int _componentIndex;
        private readonly int _subComponentIndex;

        /// <summary>
        /// Initializes a new instance of the SubComponentAccessor class.
        /// </summary>
        /// <param name="message">The HL7 message containing the subcomponent.</param>
        /// <param name="segmentName">The name of the segment containing the field.</param>
        /// <param name="fieldIndex">The 1-based field index.</param>
        /// <param name="componentIndex">The 1-based component index.</param>
        /// <param name="subComponentIndex">The 1-based subcomponent index.</param>
        public SubComponentAccessor(Message message, string segmentName, int fieldIndex, int componentIndex, int subComponentIndex)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _segmentName = segmentName ?? throw new ArgumentNullException(nameof(segmentName));
            _fieldIndex = fieldIndex;
            _componentIndex = componentIndex;
            _subComponentIndex = subComponentIndex;
        }

        /// <summary>
        /// Gets the value of the subcomponent. Returns null for HL7 null values ("") and empty string for non-existent subcomponents.
        /// </summary>
        public string Value
        {
            get
            {
                if (_subComponentIndex <= 0 || _componentIndex <= 0)
                    return "";

                try
                {
                    var path = $"{_segmentName}.{_fieldIndex}.{_componentIndex}.{_subComponentIndex}";
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
        /// Gets the value of the subcomponent, never returning null. Returns empty string for both null and non-existent subcomponents.
        /// </summary>
        public string SafeValue => Value ?? "";

        /// <summary>
        /// Gets whether the subcomponent exists in the message.
        /// </summary>
        public bool Exists
        {
            get
            {
                if (_subComponentIndex <= 0 || _componentIndex <= 0)
                    return false;

                try
                {
                    var path = $"{_segmentName}.{_fieldIndex}.{_componentIndex}.{_subComponentIndex}";
                    var segment = _message.DefaultSegment(_segmentName);
                    if (segment == null)
                        return false;

                    var field = segment.Fields(_fieldIndex);
                    if (field == null)
                        return false;

                    if (_componentIndex > field.ComponentList.Count)
                        return false;

                    var component = field.Components(_componentIndex);
                    if (component == null)
                        return false;

                    return component.SubComponentList.Count >= _subComponentIndex;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets whether the subcomponent is explicitly set to HL7 null ("").
        /// </summary>
        public bool IsNull => Value == null && Exists;

        /// <summary>
        /// Gets whether the subcomponent exists but contains an empty string.
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
        /// Gets whether the subcomponent exists and has a non-empty value.
        /// </summary>
        public bool HasValue
        {
            get
            {
                var val = Value;
                return val != null && val != "";
            }
        }
    }
}