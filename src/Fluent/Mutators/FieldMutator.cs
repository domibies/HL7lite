using System;
using System.Linq;

namespace HL7lite.Fluent.Mutators
{
    /// <summary>
    /// Provides fluent methods for modifying HL7 field values with method chaining support.
    /// FieldMutator operates at the field level of the HL7 hierarchy and supports setting
    /// field values, creating components, handling repetitions, and navigating to other fields.
    /// </summary>
    /// <remarks>
    /// FieldMutator follows the HL7 hierarchy: Message → Segment → Field → Component → SubComponent.
    /// All operations are null-safe and will create missing segments as needed.
    /// Supports component creation, field repetitions, datetime formatting, encoding of HL7 delimiter characters,
    /// null value handling, and conditional operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Set a simple field value
    /// fluent.PID[3].Set().Value("12345");
    /// 
    /// // Set structured field with components
    /// fluent.PID[5].Set().Components("Smith", "John", "M", "Dr.", "III");
    /// 
    /// // Set datetime fields
    /// fluent.PID[7].Set().DateTime(DateTime.Parse("1985-03-15"));
    /// fluent.PID[29].Set().DateTimeNow();
    /// 
    /// // Add field repetitions
    /// fluent.PID[13].Set()
    ///     .Value("555-1234")
    ///     .AddRepetition("555-5678");
    /// 
    /// // Chain operations to other fields
    /// fluent.PID[3].Set()
    ///     .Value("12345")
    ///     .Field(5, "Smith^John")
    ///     .Field(7, "19850315");
    /// 
    /// // Handle delimiters safely
    /// fluent.OBX[5].Set().EncodedValue("Result: Normal|Range: 70-100");
    /// </code>
    /// </example>
    public class FieldMutator
    {
        private readonly Message _message;
        private readonly string _segmentCode;
        private readonly int _fieldIndex;
        private readonly int? _repetitionIndex;
        private readonly int _segmentInstanceIndex;
        private readonly string _path;

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
        public FieldMutator EncodedValue(string value)
        {
            if (value == null)
            {
                return Value(null);
            }

            var encodedValue = _message.Encoding.Encode(value);
            return Value(encodedValue);
        }

        public FieldMutator Null()
        {
            return Value(_message.Encoding.PresentButNull);
        }

        public FieldMutator Clear()
        {
            return Value(string.Empty);
        }

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
        public FieldMutator DateToday()
        {
            return Date(System.DateTime.Today);
        }

        public FieldMutator ValueIf(string value, bool condition)
        {
            if (condition)
            {
                return Value(value);
            }
            return this;
        }

        public FieldMutator AddRepetition(string value)
        {
            // AddRepetition doesn't make sense on a specific repetition (other than the first)
            if (_repetitionIndex.HasValue && _repetitionIndex.Value > 1)
            {
                throw new InvalidOperationException("Cannot add a repetition when operating on a specific repetition. Use the field-level accessor instead.");
            }

            // Get the specific segment instance
            Segment segment = null;
            
            if (_message.SegmentList.ContainsKey(_segmentCode))
            {
                var segments = _message.SegmentList[_segmentCode];
                if (_segmentInstanceIndex < segments.Count)
                {
                    segment = segments[_segmentInstanceIndex];
                }
            }
            
            if (segment == null)
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
                    segment = newSegment;
                }
                else
                {
                    throw new InvalidOperationException($"Cannot add repetition to segment instance {_segmentInstanceIndex} that doesn't exist.");
                }
            }

            var field = segment.Fields(_fieldIndex);
            if (field == null || string.IsNullOrEmpty(field.Value))
            {
                return Value(value);
            }

            // Check if field already has repetitions
            if (field.HasRepetitions)
            {
                var repetitionCount = field.Repetitions().Count;
                var repetitionPath = $"{_segmentCode}.{_fieldIndex}({repetitionCount + 1})";
                _message.PutValue(repetitionPath, value ?? string.Empty);
            }
            else
            {
                // Convert single field to repetition
                var existingValue = field.Value;
                _message.PutValue($"{_segmentCode}.{_fieldIndex}(1)", existingValue);
                _message.PutValue($"{_segmentCode}.{_fieldIndex}(2)", value ?? string.Empty);
            }

            return this;
        }

        /// <summary>
        /// Sets a value on a specific field of the segment (allows setting different fields in a chain).
        /// </summary>
        /// <param name="fieldIndex">The 1-based field index to set.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>This FieldMutator for chaining.</returns>
        public FieldMutator Field(int fieldIndex, string value)
        {
            if (fieldIndex <= 0)
                throw new ArgumentException("Field index must be greater than 0", nameof(fieldIndex));

            // Create a new mutator for the target field and set its value
            var targetFieldMutator = new FieldMutator(_message, _segmentCode, fieldIndex, null, _segmentInstanceIndex);
            targetFieldMutator.Value(value ?? string.Empty);
            
            return this; // Return self for chaining
        }
    }
}