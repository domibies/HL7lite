using System;
using System.Collections.Generic;
using HL7lite.Fluent.Collections;
using HL7lite.Fluent.Mutators;

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
        private readonly int _repetitionIndex;
        private readonly string _fieldPath;
        private readonly Dictionary<int, ComponentAccessor> _componentCache = new Dictionary<int, ComponentAccessor>();
        private FieldRepetitionCollection _repetitions;

        internal FieldAccessor(Message message, string segmentName, int fieldIndex)
            : this(message, segmentName, fieldIndex, 1)
        {
        }

        internal FieldAccessor(Message message, string segmentName, int fieldIndex, int repetitionIndex)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _segmentName = segmentName ?? throw new ArgumentNullException(nameof(segmentName));
            _fieldIndex = fieldIndex;
            _repetitionIndex = repetitionIndex > 0 ? repetitionIndex : 1;
            _fieldPath = $"{_segmentName}.{_fieldIndex}({_repetitionIndex})";
        }

        /// <summary>
        /// Gets whether this field exists in the segment
        /// </summary>
        public bool Exists
        {
            get
            {
                try
                {
                    _message.GetValue(_fieldPath);
                    return true;
                }
                catch (HL7Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the field value. Returns empty string for non-existent fields, null for explicit HL7 nulls.
        /// </summary>
        public string Value
        {
            get
            {
                try
                {
                    var rawValue = _message.GetValue(_fieldPath);
                    // Handle HL7 null representation (two double quotes)
                    if (rawValue == "\"\"")
                        return null;
                    return rawValue ?? "";
                }
                catch (HL7Exception)
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// Gets the field value, never returns null (converts nulls to empty string)
        /// </summary>
        public string SafeValue => Value ?? "";

        /// <summary>
        /// Gets whether the field is explicitly null in HL7 format ("")
        /// </summary>
        public bool IsNull
        {
            get
            {
                try
                {
                    var rawValue = _message.GetValue(_fieldPath);
                    return rawValue == "\"\"";
                }
                catch (HL7Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets whether the field exists but is empty
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                try
                {
                    var rawValue = _message.GetValue(_fieldPath);
                    return string.IsNullOrEmpty(rawValue) && rawValue != "\"\"";
                }
                catch (HL7Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets whether the field exists and has a non-null, non-empty value
        /// </summary>
        public bool HasValue
        {
            get
            {
                try
                {
                    var rawValue = _message.GetValue(_fieldPath);
                    return !string.IsNullOrEmpty(rawValue) && rawValue != "\"\"";
                }
                catch (HL7Exception)
                {
                    return false;
                }
            }
        }

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
                        _message, _segmentName, _fieldIndex, componentIndex, _repetitionIndex);
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

        /// <summary>
        /// Gets whether this field has multiple repetitions.
        /// </summary>
        public bool HasRepetitions => RepetitionCount > 1;

        /// <summary>
        /// Gets the number of repetitions for this field.
        /// </summary>
        public int RepetitionCount
        {
            get
            {
                var segment = _message.DefaultSegment(_segmentName);
                if (segment == null)
                    return 0;

                var field = segment.Fields(_fieldIndex);
                if (field == null)
                    return 0;

                return field.Repetitions().Count;
            }
        }

        /// <summary>
        /// Gets a specific repetition of this field.
        /// </summary>
        /// <param name="repetitionIndex">The 1-based repetition index.</param>
        /// <returns>A FieldAccessor for the specified repetition.</returns>
        public FieldAccessor Repetition(int repetitionIndex)
        {
            if (repetitionIndex <= 0)
                throw new ArgumentOutOfRangeException(nameof(repetitionIndex), "Repetition index must be greater than 0.");

            return new FieldAccessor(_message, _segmentName, _fieldIndex, repetitionIndex);
        }

        /// <summary>
        /// Gets a collection of all repetitions for this field.
        /// </summary>
        public FieldRepetitionCollection Repetitions
        {
            get
            {
                if (_repetitions == null)
                {
                    _repetitions = new FieldRepetitionCollection(_message, _segmentName, _fieldIndex);
                }
                return _repetitions;
            }
        }

        /// <summary>
        /// Gets a mutator for modifying this field's value.
        /// </summary>
        /// <returns>A FieldMutator for this field.</returns>
        public FieldMutator Set()
        {
            // TODO: FieldMutator needs to support repetition index
            return new FieldMutator(_message, _segmentName, _fieldIndex);
        }
    }
}