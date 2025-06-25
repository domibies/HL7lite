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
        private readonly int _segmentInstanceIndex;
        private readonly string _fieldPath;
        private readonly Dictionary<int, ComponentAccessor> _componentCache = new Dictionary<int, ComponentAccessor>();
        private FieldRepetitionCollection _repetitions;

        internal FieldAccessor(Message message, string segmentName, int fieldIndex)
            : this(message, segmentName, fieldIndex, 1, 0)
        {
        }

        internal FieldAccessor(Message message, string segmentName, int fieldIndex, int repetitionIndex)
            : this(message, segmentName, fieldIndex, repetitionIndex, 0)
        {
        }

        internal FieldAccessor(Message message, string segmentName, int fieldIndex, int repetitionIndex, int segmentInstanceIndex)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _segmentName = segmentName ?? throw new ArgumentNullException(nameof(segmentName));
            _fieldIndex = fieldIndex;
            _repetitionIndex = repetitionIndex > 0 ? repetitionIndex : 1;
            _segmentInstanceIndex = segmentInstanceIndex;
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
                    var segment = GetSegmentInstance();
                    if (segment == null)
                        return false;
                    
                    var field = segment.Fields(_fieldIndex);
                    if (field == null)
                        return false;
                    
                    // Handle field repetitions
                    if (field.HasRepetitions)
                    {
                        var repetitions = field.Repetitions();
                        return _repetitionIndex <= repetitions.Count;
                    }
                    else if (_repetitionIndex > 1)
                    {
                        // Field doesn't have repetitions but we're asking for repetition > 1
                        return false;
                    }
                    
                    return true;
                }
                catch
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
                    // For repetition index 1 on non-repeating fields, use the original field
                    
                    var rawValue = field.Value;
                    // HL7 null handling is done by the core Field implementation
                    // Don't convert explicit HL7 nulls to empty string
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
        /// Gets whether the field is explicitly null in HL7 format ("")
        /// </summary>
        public bool IsNull
        {
            get
            {
                if (!Exists)
                    return false;
                return Value == null;
            }
        }

        /// <summary>
        /// Gets whether the field exists but is empty
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
        /// Gets whether the field exists and has a non-null, non-empty value
        /// </summary>
        public bool HasValue
        {
            get
            {
                if (!Exists)
                    return false;
                var val = Value;
                return val != null && val != "";
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
                        _message, _segmentName, _fieldIndex, componentIndex, _repetitionIndex, _segmentInstanceIndex);
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
                var segment = GetSegmentInstance();
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

            return new FieldAccessor(_message, _segmentName, _fieldIndex, repetitionIndex, _segmentInstanceIndex);
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
                    _repetitions = new FieldRepetitionCollection(_message, _segmentName, _fieldIndex, _segmentInstanceIndex);
                }
                return _repetitions;
            }
        }

        /// <summary>
        /// Parses the field value as an HL7 datetime and returns a DateTime object.
        /// Supports various HL7 datetime formats from year-only to full precision with timezones.
        /// </summary>
        /// <param name="throwOnError">If true, throws an exception when parsing fails. If false, returns null.</param>
        /// <returns>The parsed DateTime, or null if parsing fails and throwOnError is false</returns>
        public DateTime? AsDateTime(bool throwOnError = false)
        {
            if (!HasValue)
                return null;

            return MessageHelper.ParseDateTime(Value, throwOnError);
        }

        /// <summary>
        /// Parses the field value as an HL7 datetime and returns both the DateTime and timezone offset.
        /// Supports various HL7 datetime formats from year-only to full precision with timezones.
        /// </summary>
        /// <param name="offset">Outputs the timezone offset if present in the datetime string</param>
        /// <param name="throwOnError">If true, throws an exception when parsing fails. If false, returns null.</param>
        /// <returns>The parsed DateTime, or null if parsing fails and throwOnError is false</returns>
        public DateTime? AsDateTime(out TimeSpan offset, bool throwOnError = false)
        {
            offset = TimeSpan.Zero;
            if (!HasValue)
                return null;

            return MessageHelper.ParseDateTime(Value, out offset, throwOnError);
        }

        /// <summary>
        /// Parses the field value as an HL7 date (ignoring any time portion) and returns a DateTime object.
        /// The returned DateTime will have the time set to midnight (00:00:00).
        /// </summary>
        /// <param name="throwOnError">If true, throws an exception when parsing fails. If false, returns null.</param>
        /// <returns>The parsed DateTime with time set to midnight, or null if parsing fails and throwOnError is false</returns>
        public DateTime? AsDate(bool throwOnError = false)
        {
            var dateTime = AsDateTime(throwOnError);
            return dateTime?.Date; // Returns date portion only (time set to 00:00:00)
        }

        /// <summary>
        /// Gets a mutator for modifying this field's value.
        /// </summary>
        /// <returns>A FieldMutator for this field.</returns>
        public FieldMutator Set()
        {
            return new FieldMutator(_message, _segmentName, _fieldIndex, _repetitionIndex, _segmentInstanceIndex);
        }

        /// <summary>
        /// Sets the field value directly. Shortcut for Set().Set(value).
        /// </summary>
        /// <param name="value">The value to set</param>
        /// <returns>A FieldMutator for method chaining</returns>
        public FieldMutator Set(string value)
        {
            return Set().Set(value);
        }

        /// <summary>
        /// Sets the field value with HL7 delimiter encoding. Shortcut for Set().SetEncoded(value).
        /// Use this method when your value contains characters like |, ^, ~, \, or &amp;
        /// that need to be safely stored in the HL7 message.
        /// </summary>
        /// <param name="value">The value to encode and set</param>
        /// <returns>A FieldMutator for method chaining</returns>
        /// <example>
        /// <code>
        /// // Set field with special characters
        /// fluent.PID[5].SetEncoded("Smith|Jones");  // Becomes "Smith\\F\\Jones"
        /// 
        /// // Equivalent to the verbose form
        /// fluent.PID[5].Set().SetEncoded("Smith|Jones");
        /// </code>
        /// </example>
        public FieldMutator SetEncoded(string value)
        {
            return Set().SetEncoded(value);
        }
    }
}