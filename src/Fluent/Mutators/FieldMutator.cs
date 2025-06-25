using System;
using System.Linq;

namespace HL7lite.Fluent.Mutators
{
    /// <summary>
    /// Provides fluent methods for modifying HL7 field values.
    /// </summary>
    public class FieldMutator
    {
        private readonly Message _message;
        private readonly string _segmentCode;
        private readonly int _fieldIndex;
        private readonly int? _repetitionIndex;
        private readonly int _segmentInstanceIndex;
        private readonly string _path;

        /// <summary>
        /// Initializes a new FieldMutator.
        /// </summary>
        public FieldMutator(Message message, string segmentCode, int fieldIndex, int? repetitionIndex = null, int segmentInstanceIndex = 0)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            if (segmentCode == null)
                throw new ArgumentNullException(nameof(segmentCode));
            if (string.IsNullOrEmpty(segmentCode))
                throw new ArgumentException("Segment code cannot be empty", nameof(segmentCode));
            _segmentCode = segmentCode;
            _fieldIndex = fieldIndex > 0 ? fieldIndex : throw new ArgumentException("Field index must be greater than 0", nameof(fieldIndex));
            _repetitionIndex = repetitionIndex;
            _segmentInstanceIndex = segmentInstanceIndex;
            
            // Build path with optional repetition index
            _path = repetitionIndex.HasValue && repetitionIndex.Value > 0
                ? $"{_segmentCode}.{_fieldIndex}[{repetitionIndex.Value}]"
                : $"{_segmentCode}.{_fieldIndex}";
        }

        /// <summary>
        /// Sets the field value.
        /// </summary>
        public FieldMutator Value(string value)
        {
            // Get the specific segment instance
            Segment targetSegment = null;
            
            if (_message.SegmentList.ContainsKey(_segmentCode))
            {
                var segments = _message.SegmentList[_segmentCode];
                if (_segmentInstanceIndex < segments.Count)
                {
                    targetSegment = segments[_segmentInstanceIndex];
                }
            }
            
            if (targetSegment == null)
            {
                // Segment instance doesn't exist, create it if it's the first instance
                if (_segmentInstanceIndex == 0)
                {
                    var newSegment = new Segment(_message.Encoding)
                    {
                        Name = _segmentCode,
                        Value = _segmentCode
                    };
                    _message.AddNewSegment(newSegment);
                    targetSegment = newSegment;
                }
                else
                {
                    throw new InvalidOperationException($"Cannot set field on segment instance {_segmentInstanceIndex} that doesn't exist.");
                }
            }
            
            // Set the field value directly on the target segment
            if (_repetitionIndex.HasValue && _repetitionIndex.Value > 1)
            {
                // Handle repetition-specific field setting
                var field = targetSegment.Fields(_fieldIndex);
                if (field == null)
                {
                    targetSegment.AddNewField(value ?? string.Empty, _fieldIndex);
                    field = targetSegment.Fields(_fieldIndex);
                }
                
                // Set specific repetition
                if (field.HasRepetitions)
                {
                    var repetitions = field.Repetitions();
                    if (_repetitionIndex.Value <= repetitions.Count)
                    {
                        repetitions[_repetitionIndex.Value - 1].Value = value ?? string.Empty;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Repetition {_repetitionIndex.Value} does not exist.");
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Field does not have repetitions.");
                }
            }
            else
            {
                // Set the field value (handles first repetition or non-repeated field)
                var field = targetSegment.Fields(_fieldIndex);
                if (field == null)
                {
                    targetSegment.AddNewField(value ?? string.Empty, _fieldIndex);
                }
                else
                {
                    field.Value = value ?? string.Empty;
                }
            }
            
            return this;
        }

        /// <summary>
        /// Sets the field value after encoding any HL7 delimiter characters.
        /// Use this method when your value contains characters like |, ^, ~, \, or &amp;
        /// that need to be safely stored in the HL7 message.
        /// </summary>
        /// <param name="value">The value to encode and set</param>
        /// <returns>The FieldMutator for method chaining</returns>
        /// <summary>
        /// Sets the field value with HL7 delimiter encoding.
        /// </summary>
        public FieldMutator EncodedValue(string value)
        {
            if (value == null)
            {
                return Value(null);
            }

            var encodedValue = _message.Encoding.Encode(value);
            return Value(encodedValue);
        }

        /// <summary>
        /// Sets the field to HL7 null value.
        /// </summary>
        public FieldMutator Null()
        {
            return Value(_message.Encoding.PresentButNull);
        }

        /// <summary>
        /// Clears the field value.
        /// </summary>
        public FieldMutator Clear()
        {
            return Value(string.Empty);
        }

        /// <summary>
        /// Sets multiple field components.
        /// </summary>
        public FieldMutator Components(params string[] components)
        {
            if (components == null || components.Length == 0)
            {
                return Clear();
            }

            var encoding = _message.Encoding;
            var componentSeparator = encoding.ComponentDelimiter;
            var value = string.Join(componentSeparator.ToString(), components.Select(c => c ?? string.Empty));
            
            return Value(value);
        }

        /// <summary>
        /// Sets the field value to an HL7-formatted datetime string.
        /// Uses the full precision format: yyyyMMddHHmmss.FFFF
        /// </summary>
        /// <param name="dateTime">The DateTime to set</param>
        /// <returns>The FieldMutator for method chaining</returns>
        /// <summary>
        /// Sets the field to a formatted DateTime value.
        /// </summary>
        public FieldMutator DateTime(DateTime dateTime)
        {
            var hl7DateTime = MessageHelper.LongDateWithFractionOfSecond(dateTime);
            return Value(hl7DateTime);
        }

        /// <summary>
        /// Sets the field value to an HL7-formatted datetime string for the current date and time.
        /// Uses the full precision format: yyyyMMddHHmmss.FFFF
        /// </summary>
        /// <returns>The FieldMutator for method chaining</returns>
        /// <summary>
        /// Sets the field to the current DateTime.
        /// </summary>
        public FieldMutator DateTimeNow()
        {
            return DateTime(System.DateTime.Now);
        }

        /// <summary>
        /// Sets the field value to an HL7-formatted date string (date only, no time).
        /// Uses the format: yyyyMMdd
        /// </summary>
        /// <param name="date">The DateTime to extract date from</param>
        /// <returns>The FieldMutator for method chaining</returns>
        /// <summary>
        /// Sets the field to a formatted date value.
        /// </summary>
        public FieldMutator Date(DateTime date)
        {
            var hl7Date = date.ToString("yyyyMMdd");
            return Value(hl7Date);
        }

        /// <summary>
        /// Sets the field value to an HL7-formatted date string for today's date.
        /// Uses the format: yyyyMMdd
        /// </summary>
        /// <returns>The FieldMutator for method chaining</returns>
        /// <summary>
        /// Sets the field to today's date.
        /// </summary>
        public FieldMutator DateToday()
        {
            return Date(System.DateTime.Today);
        }

        /// <summary>
        /// Sets the field value if the condition is true.
        /// </summary>
        public FieldMutator ValueIf(string value, bool condition)
        {
            if (condition)
            {
                return Value(value);
            }
            return this;
        }


        /// <summary>
        /// Navigates to a different field in the same segment.
        /// </summary>
        /// <param name="fieldIndex">The 1-based field index to navigate to.</param>
        /// <returns>A new FieldMutator for the target field.</returns>
        public FieldMutator Field(int fieldIndex)
        {
            if (fieldIndex <= 0)
                throw new ArgumentException("Field index must be greater than 0", nameof(fieldIndex));

            // Create and return a new mutator for the target field
            return new FieldMutator(_message, _segmentCode, fieldIndex, null, _segmentInstanceIndex);
        }

        /// <summary>
        /// Navigates to a component within the current field.
        /// </summary>
        /// <param name="componentIndex">The 1-based component index to navigate to.</param>
        /// <returns>A ComponentMutator for the target component.</returns>
        public ComponentMutator Component(int componentIndex)
        {
            if (componentIndex <= 0)
                throw new ArgumentException("Component index must be greater than 0", nameof(componentIndex));

            // Create and return a new ComponentMutator for the target component
            return new ComponentMutator(_message, _segmentCode, _fieldIndex, componentIndex, _repetitionIndex ?? 1);
        }

        /// <summary>
        /// Navigates to a subcomponent within the current field.
        /// </summary>
        /// <param name="componentIndex">The 1-based component index.</param>
        /// <param name="subComponentIndex">The 1-based subcomponent index to navigate to.</param>
        /// <returns>A SubComponentMutator for the target subcomponent.</returns>
        public SubComponentMutator SubComponent(int componentIndex, int subComponentIndex)
        {
            if (componentIndex <= 0)
                throw new ArgumentException("Component index must be greater than 0", nameof(componentIndex));
            if (subComponentIndex <= 0)
                throw new ArgumentException("SubComponent index must be greater than 0", nameof(subComponentIndex));

            // Create and return a new SubComponentMutator for the target subcomponent
            return new SubComponentMutator(_message, _segmentCode, _fieldIndex, componentIndex, subComponentIndex, _repetitionIndex ?? 1);
        }
    }
}