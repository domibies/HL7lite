using System;
using HL7lite.Fluent.Mutators;

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
        private readonly int _repetitionIndex;
        private readonly int _segmentInstanceIndex;

        /// <summary>
        /// Initializes a new instance of the SubComponentAccessor class.
        /// </summary>
        /// <param name="message">The HL7 message containing the subcomponent.</param>
        /// <param name="segmentName">The name of the segment containing the field.</param>
        /// <param name="fieldIndex">The 1-based field index.</param>
        /// <param name="componentIndex">The 1-based component index.</param>
        /// <param name="subComponentIndex">The 1-based subcomponent index.</param>
        public SubComponentAccessor(Message message, string segmentName, int fieldIndex, int componentIndex, int subComponentIndex)
            : this(message, segmentName, fieldIndex, componentIndex, subComponentIndex, 1, 0)
        {
        }

        /// <summary>
        /// Initializes a new SubComponentAccessor.
        /// </summary>
        public SubComponentAccessor(Message message, string segmentName, int fieldIndex, int componentIndex, int subComponentIndex, int repetitionIndex)
            : this(message, segmentName, fieldIndex, componentIndex, subComponentIndex, repetitionIndex, 0)
        {
        }

        /// <summary>
        /// Initializes a new SubComponentAccessor with segment instance.
        /// </summary>
        public SubComponentAccessor(Message message, string segmentName, int fieldIndex, int componentIndex, int subComponentIndex, int repetitionIndex, int segmentInstanceIndex)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _segmentName = segmentName ?? throw new ArgumentNullException(nameof(segmentName));
            _fieldIndex = fieldIndex;
            _componentIndex = componentIndex;
            _subComponentIndex = subComponentIndex;
            _repetitionIndex = repetitionIndex > 0 ? repetitionIndex : 1;
            _segmentInstanceIndex = segmentInstanceIndex;
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
                    
                    // Get the subcomponent
                    if (_subComponentIndex > component.SubComponentList.Count)
                        return "";
                    
                    var subComponent = component.SubComponentList[_subComponentIndex - 1];
                    var rawValue = subComponent.Value;
                    
                    // HL7 null handling is done by the core SubComponent implementation
                    return rawValue;
                }
                catch
                {
                    return "";
                }
            }
        }


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

                    if (_componentIndex > targetField.ComponentList.Count)
                        return false;

                    var component = targetField.Components(_componentIndex);
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
        /// Gets a mutator for modifying this subcomponent's value.
        /// </summary>
        /// <returns>A SubComponentMutator for this subcomponent.</returns>
        public SubComponentMutator Set()
        {
            return new SubComponentMutator(_message, _segmentName, _fieldIndex, _componentIndex, _subComponentIndex, _repetitionIndex);
        }

        /// <summary>
        /// Sets the subcomponent value directly. Shortcut for Set().Set(value).
        /// </summary>
        /// <param name="value">The value to set</param>
        /// <returns>A SubComponentMutator for method chaining</returns>
        public SubComponentMutator Set(string value)
        {
            return Set().Set(value);
        }

        /// <summary>
        /// Sets the subcomponent value with HL7 delimiter encoding. Shortcut for Set().SetEncoded(value).
        /// Use this method when your value contains characters like |, ^, ~, \, or &amp;
        /// that need to be safely stored in the HL7 message.
        /// </summary>
        /// <param name="value">The value to encode and set</param>
        /// <returns>A SubComponentMutator for method chaining</returns>
        /// <example>
        /// <code>
        /// // Set subcomponent with special characters
        /// fluent.PID[5][1][1].SetEncoded("Smith&Jones");  // Becomes "Smith\\T\\Jones"
        /// 
        /// // Equivalent to the verbose form
        /// fluent.PID[5][1][1].SetEncoded("Smith&Jones");
        /// </code>
        /// </example>
        public SubComponentMutator SetEncoded(string value)
        {
            return Set().SetEncoded(value);
        }

        /// <summary>Sets the subcomponent to HL7 null (""). Shortcut for Set().SetNull().</summary>
        public SubComponentMutator SetNull()
        {
            return Set().SetNull();
        }

        /// <summary>Sets the subcomponent value conditionally. Shortcut for Set().SetIf().</summary>
        public SubComponentMutator SetIf(string value, bool condition)
        {
            return Set().SetIf(value, condition);
        }
    }
}