using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HL7lite.Fluent.Mutators;

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
        private readonly int _repetitionIndex;
        private readonly int _segmentInstanceIndex;
        private readonly Dictionary<int, SubComponentAccessor> _subComponentCache = new Dictionary<int, SubComponentAccessor>();

        /// <summary>
        /// Initializes a new instance of the ComponentAccessor class.
        /// </summary>
        /// <param name="message">The HL7 message containing the component.</param>
        /// <param name="segmentName">The name of the segment containing the field.</param>
        /// <param name="fieldIndex">The 1-based field index.</param>
        /// <param name="componentIndex">The 1-based component index.</param>
        public ComponentAccessor(Message message, string segmentName, int fieldIndex, int componentIndex)
            : this(message, segmentName, fieldIndex, componentIndex, 1, 0)
        {
        }

        /// <summary>
        /// Initializes a new ComponentAccessor.
        /// </summary>
        public ComponentAccessor(Message message, string segmentName, int fieldIndex, int componentIndex, int repetitionIndex)
            : this(message, segmentName, fieldIndex, componentIndex, repetitionIndex, 0)
        {
        }

        /// <summary>
        /// Initializes a new ComponentAccessor with segment instance.
        /// </summary>
        public ComponentAccessor(Message message, string segmentName, int fieldIndex, int componentIndex, int repetitionIndex, int segmentInstanceIndex)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _segmentName = segmentName ?? throw new ArgumentNullException(nameof(segmentName));
            _fieldIndex = fieldIndex;
            _componentIndex = componentIndex;
            _repetitionIndex = repetitionIndex > 0 ? repetitionIndex : 1;
            _segmentInstanceIndex = segmentInstanceIndex;
        }

        /// <summary>
        /// Gets the raw component value with structural delimiters and encoded characters.
        /// Returns HL7 null ("") for explicit nulls and empty string for non-existent components.
        /// </summary>
        public string Raw
        {
            get
            {
                if (_componentIndex <= 0)
                    return "";

                try
                {
                    var segment = GetSegmentInstance();
                    if (segment == null)
                        return "";
                    
                    var field = segment.Fields(_fieldIndex);
                    if (field == null)
                        return "";
                    
                    // Handle field repetitions
                    if (field.HasRepetitions)
                    {
                        var repetitions = field.Repetitions();
                        if (_repetitionIndex > repetitions.Count)
                            return "";
                        field = repetitions[_repetitionIndex - 1];
                    }
                    else if (_repetitionIndex > 1)
                    {
                        // Field doesn't have repetitions but we're asking for repetition > 1
                        return "";
                    }
                    
                    // Get the component
                    if (_componentIndex > field.ComponentList.Count)
                        return "";
                    
                    var component = field.ComponentList[_componentIndex - 1];
                    var rawValue = component.Value;
                    
                    // Core API converts "" to null, convert back for consistency
                    if (rawValue == null)
                        return _message.Encoding.PresentButNull;
                    return rawValue;
                }
                catch
                {
                    return "";
                }
            }
        }

        private Segment GetSegmentInstance()
        {
            if (!_message.SegmentList.ContainsKey(_segmentName))
                return null;
            
            var segments = _message.SegmentList[_segmentName];
            if (_segmentInstanceIndex >= segments.Count)
                return null;
            
            return segments[_segmentInstanceIndex];
        }


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
                    var segment = GetSegmentInstance();
                    if (segment == null)
                        return false;

                    var field = segment.Fields(_fieldIndex);
                    if (field == null)
                        return false;

                    Field targetField = field;
                    if (field.HasRepetitions)
                    {
                        if (_repetitionIndex > field.Repetitions().Count)
                            return false;
                        targetField = field.Repetitions()[_repetitionIndex - 1];
                    }
                    else if (_repetitionIndex > 1)
                    {
                        // Field doesn't have repetitions but we're asking for repetition > 1
                        return false;
                    }

                    return targetField.ComponentList.Count >= _componentIndex;
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
        public bool IsNull => Raw == _message.Encoding.PresentButNull && Exists;

        /// <summary>
        /// Gets whether the component exists but contains an empty string.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                if (!Exists)
                    return false;
                var val = Raw;
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
                var val = Raw;
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
                        _message, _segmentName, _fieldIndex, _componentIndex, subComponentIndex, _repetitionIndex, _segmentInstanceIndex);
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

        /// <summary>
        /// Gets a mutator for modifying this component's value.
        /// </summary>
        /// <returns>A ComponentMutator for this component.</returns>
        public ComponentMutator Set()
        {
            return new ComponentMutator(_message, _segmentName, _fieldIndex, _componentIndex, _repetitionIndex, _segmentInstanceIndex);
        }

        /// <summary>
        /// Sets the component value directly. Shortcut for Set().Set(value).
        /// </summary>
        /// <param name="value">The value to set</param>
        /// <returns>A ComponentMutator for method chaining</returns>
        public ComponentMutator Set(string value)
        {
            return Set().Set(value);
        }

        /// <summary>
        /// Sets the component to a raw HL7 value. Shortcut for Set().SetRaw(value).
        /// Use this for pre-encoded data or when building structured values.
        /// </summary>
        /// <param name="value">The raw HL7 value to set</param>
        /// <returns>A ComponentMutator for method chaining</returns>
        public ComponentMutator SetRaw(string value)
        {
            return Set().SetRaw(value);
        }

        /// <summary>Sets multiple subcomponents. Shortcut for Set().SetSubComponents().</summary>
        public ComponentMutator SetSubComponents(params string[] values)
        {
            return Set().SetSubComponents(values);
        }

        /// <summary>Sets the component to HL7 null (""). Shortcut for Set().SetNull().</summary>
        public ComponentMutator SetNull()
        {
            return Set().SetNull();
        }

        /// <summary>Sets the component value conditionally. Shortcut for Set().SetIf().</summary>
        public ComponentMutator SetIf(string value, bool condition)
        {
            return Set().SetIf(value, condition);
        }
        
        /// <summary>
        /// Returns a human-readable representation of the component value.
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
            // For components, we only replace subcomponent (&) delimiters
            var processed = rawValue;
            var delimiterPattern = "[" + Regex.Escape(_message.Encoding.SubComponentDelimiter.ToString()) + "]+";
            processed = Regex.Replace(processed, delimiterPattern, " ");
            
            // Then decode encoded delimiters (e.g., \T\ → &) - these become literal characters
            var decoded = _message.Encoding.Decode(processed);
            
            // Replace HL7 nulls with readable placeholder
            decoded = decoded.Replace(_message.Encoding.PresentButNull, "<null>");
            
            return decoded.Trim();
        }
    }
}