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
        /// Sets the field to the specified value. Creates the field if it doesn't exist.
        /// Null values are converted to empty strings. For explicit HL7 null values, use the SetNull() method.
        /// </summary>
        /// <param name="value">The value to set. Null values are converted to empty strings.</param>
        /// <returns>The FieldMutator for method chaining</returns>
        /// <example>
        /// <code>
        /// // Set field value
        /// fluent.PID[3].Set("12345");
        /// 
        /// // Chain multiple operations
        /// fluent.PID[3].Set("12345")
        ///     .Field(5).Set("Smith^John");
        /// </code>
        /// </example>
        public FieldMutator Set(string value)
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
        /// <example>
        /// <code>
        /// // Set value with special characters
        /// fluent.PID[5].SetEncoded("Smith|Jones");  // Becomes "Smith\\F\\Jones"
        /// 
        /// // Chain with other operations
        /// fluent.PID[5].SetEncoded("Complex|Value")
        ///     .Field(7).Set("19850315");
        /// </code>
        /// </example>
        public FieldMutator SetEncoded(string value)
        {
            if (value == null)
            {
                return Set(null);
            }

            var encodedValue = _message.Encoding.Encode(value);
            return Set(encodedValue);
        }


        /// <summary>
        /// Sets the field to HL7 null value.
        /// In HL7, null values are represented as empty strings ("").
        /// </summary>
        /// <returns>The FieldMutator for method chaining</returns>
        /// <example>
        /// <code>
        /// // Set field to explicit null
        /// fluent.PID[6].SetNull();
        /// 
        /// // Chain with other operations
        /// fluent.PID[6].SetNull()
        ///     .Field(7).Set("19850315");
        /// </code>
        /// </example>
        public FieldMutator SetNull()
        {
            return Set(_message.Encoding.PresentButNull);
        }

        /// <summary>
        /// Sets the field to HL7 null value.
        /// </summary>

        /// <summary>
        /// Clears the field value.
        /// </summary>
        public FieldMutator Clear()
        {
            return Set(string.Empty);
        }

        /// <summary>
        /// Sets multiple field components in a single operation.
        /// Components are joined with the HL7 component separator (^).
        /// </summary>
        /// <param name="components">The component values to set</param>
        /// <returns>The FieldMutator for method chaining</returns>
        /// <example>
        /// <code>
        /// // Set patient name components
        /// fluent.PID[5].SetComponents("Smith", "John", "M", "Jr", "Dr");
        /// 
        /// // Chain with other operations
        /// fluent.PID[5].SetComponents("Smith", "John")
        ///     .Field(7).Set("19850315");
        /// </code>
        /// </example>
        public FieldMutator SetComponents(params string[] components)
        {
            if (components == null || components.Length == 0)
            {
                return Clear();
            }

            var encoding = _message.Encoding;
            var componentSeparator = encoding.ComponentDelimiter;
            var value = string.Join(componentSeparator.ToString(), components.Select(c => c ?? string.Empty));
            
            return Set(value);
        }

        /// <summary>
        /// Sets multiple field components.
        /// </summary>

        /// <summary>
        /// Sets the field value to an HL7-formatted datetime string.
        /// Uses the full precision format: yyyyMMddHHmmss.FFFF
        /// </summary>
        /// <param name="dateTime">The DateTime to set</param>
        /// <returns>The FieldMutator for method chaining</returns>
        /// <summary>
        /// Sets the field to a formatted DateTime value in HL7 format.
        /// Uses the full precision format: yyyyMMddHHmmss.FFFF
        /// </summary>
        /// <param name="dateTime">The DateTime to set</param>
        /// <returns>The FieldMutator for method chaining</returns>
        /// <example>
        /// <code>
        /// // Set observation timestamp
        /// fluent.OBX[14].SetDateTime(DateTime.Now);
        /// 
        /// // Chain with other operations
        /// fluent.OBX[14].SetDateTime(observationTime)
        ///     .Field(15).Set("Lab");
        /// </code>
        /// </example>
        public FieldMutator SetDateTime(DateTime dateTime)
        {
            return DateTime(dateTime);
        }

        /// <summary>
        /// Sets the field to a formatted DateTime value.
        /// </summary>
        public FieldMutator DateTime(DateTime dateTime)
        {
            var hl7DateTime = MessageHelper.LongDateWithFractionOfSecond(dateTime);
            return Set(hl7DateTime);
        }

        /// <summary>
        /// Sets the field value to an HL7-formatted datetime string for the current date and time.
        /// Uses the full precision format: yyyyMMddHHmmss.FFFF
        /// </summary>
        /// <returns>The FieldMutator for method chaining</returns>
        /// <summary>
        /// Sets the field to the current DateTime in HL7 format.
        /// Convenience method that uses the current date and time.
        /// </summary>
        /// <returns>The FieldMutator for method chaining</returns>
        /// <example>
        /// <code>
        /// // Set current timestamp
        /// fluent.EVN[2].SetDateTimeNow();
        /// 
        /// // Chain with other operations
        /// fluent.EVN[2].SetDateTimeNow()
        ///     .Field(6).SetDateTimeNow();
        /// </code>
        /// </example>
        public FieldMutator SetDateTimeNow()
        {
            return DateTimeNow();
        }

        /// <summary>
        /// Sets the field to the current DateTime.
        /// </summary>
        public FieldMutator DateTimeNow()
        {
            return DateTime(System.DateTime.Now);
        }

        /// <summary>
        /// Sets the field to an HL7 date in YYYYMMDD format.
        /// Only the date portion is used, time is ignored.
        /// </summary>
        /// <param name="date">The DateTime to format as a date</param>
        /// <returns>The FieldMutator for method chaining</returns>
        /// <example>
        /// <code>
        /// // Set date of birth
        /// fluent.PID[7].SetDate(new DateTime(1985, 3, 15));
        /// 
        /// // Chain with other operations
        /// fluent.PID[7].SetDate(birthDate)
        ///     .Field(8).Set("M");
        /// </code>
        /// </example>
        public FieldMutator SetDate(DateTime date)
        {
            return Date(date);
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
            return Set(hl7Date);
        }

        /// <summary>
        /// Sets the field to today's date in HL7 format (YYYYMMDD).
        /// Convenience method that uses the current date.
        /// </summary>
        /// <returns>The FieldMutator for method chaining</returns>
        /// <example>
        /// <code>
        /// // Set service date to today
        /// fluent.DG1[5].SetDateToday();
        /// 
        /// // Chain with other operations
        /// fluent.DG1[5].SetDateToday()
        ///     .Field(6).Set("F");
        /// </code>
        /// </example>
        public FieldMutator SetDateToday()
        {
            return DateToday();
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
        /// Sets the field value conditionally based on a boolean condition.
        /// If the condition is false, the field is not modified.
        /// </summary>
        /// <param name="value">The value to set if condition is true</param>
        /// <param name="condition">The condition to evaluate</param>
        /// <returns>The FieldMutator for method chaining</returns>
        /// <example>
        /// <code>
        /// // Set field only if patient has insurance
        /// fluent.PID[18].SetIf("INS001", patient.HasInsurance);
        /// 
        /// // Chain with other conditional operations
        /// fluent.PID[18].SetIf("INS001", patient.HasInsurance)
        ///     .Field(19).SetIf("SSN123", patient.HasSSN);
        /// </code>
        /// </example>
        public FieldMutator SetIf(string value, bool condition)
        {
            if (condition)
            {
                return Set(value);
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