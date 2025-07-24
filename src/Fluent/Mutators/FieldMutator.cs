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
        /// Sets the field to the specified value, automatically encoding any HL7 delimiter characters.
        /// Creates the field if it doesn't exist. Null values are converted to empty strings.
        /// For explicit HL7 null values, use the SetNull() method.
        /// </summary>
        /// <param name="value">The value to set. Delimiter characters will be automatically encoded.</param>
        /// <returns>The FieldMutator for method chaining</returns>
        /// <example>
        /// <code>
        /// // Set field value (delimiters automatically encoded)
        /// fluent.PID[3].Set("12345");
        /// fluent.PID[5].Set("Smith &amp; Jones");  // &amp; automatically encoded to \T\
        /// 
        /// // Chain multiple operations
        /// fluent.PID[3].Set("12345")
        ///     .Field(5).Set("Smith^John");  // ^ automatically encoded to \S\
        /// </code>
        /// </example>
        public FieldMutator Set(string value)
        {
            // Encode the value if not null
            var encodedValue = value != null ? _message.Encoding.Encode(value) : null;
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
                    targetSegment.AddNewField(encodedValue ?? string.Empty, _fieldIndex);
                    field = targetSegment.Fields(_fieldIndex);
                }
                
                // Set specific repetition
                if (field.HasRepetitions)
                {
                    var repetitions = field.Repetitions();
                    if (_repetitionIndex.Value <= repetitions.Count)
                    {
                        repetitions[_repetitionIndex.Value - 1].Value = encodedValue ?? string.Empty;
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
                    targetSegment.AddNewField(encodedValue ?? string.Empty, _fieldIndex);
                }
                else
                {
                    field.Value = encodedValue ?? string.Empty;
                }
            }
            
            return this;
        }

        /// <summary>
        /// Sets the field to a raw HL7 value that may contain structural delimiters and encoded characters.
        /// This method validates that the value doesn't contain invalid delimiters for the field level.
        /// Use this for pre-encoded data or when building structured values.
        /// </summary>
        /// <param name="value">The raw HL7 value to set</param>
        /// <returns>The FieldMutator for method chaining</returns>
        /// <exception cref="ArgumentException">If value contains field or repetition delimiters</exception>
        /// <example>
        /// <code>
        /// // Set pre-encoded value
        /// fluent.PID[5].SetRaw("Smith\\T\\Jones^John");  // \T\ is encoded &amp;, ^ is component separator
        /// 
        /// // Invalid - contains field delimiter
        /// fluent.PID[5].SetRaw("Value|Other");  // Throws ArgumentException
        /// </code>
        /// </example>
        public FieldMutator SetRaw(string value)
        {
            // Validate the value doesn't contain invalid delimiters
            if (value != null)
            {
                if (value.Contains(_message.Encoding.FieldDelimiter))
                {
                    throw new ArgumentException(
                        $"Field value cannot contain field delimiter '{_message.Encoding.FieldDelimiter}'. " +
                        "Use separate field assignments or encode the delimiter.");
                }
                
                if (value.Contains(_message.Encoding.RepeatDelimiter))
                {
                    throw new ArgumentException(
                        $"Field value cannot contain repetition delimiter '{_message.Encoding.RepeatDelimiter}'. " +
                        "Use AddRepetition() or encode the delimiter.");
                }
            }
            
            // Now set the raw value without encoding
            return SetRawInternal(value);
        }
        
        /// <summary>
        /// Internal method to set raw value without encoding
        /// </summary>
        private FieldMutator SetRawInternal(string value)
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
        /// Components are automatically encoded and joined with the HL7 component separator (^).
        /// </summary>
        /// <param name="components">The component values to set. Delimiter characters will be automatically encoded.</param>
        /// <returns>The FieldMutator for method chaining</returns>
        /// <example>
        /// <code>
        /// // Set patient name components (delimiters automatically encoded)
        /// fluent.PID[5].SetComponents("Smith &amp; Sons", "John", "M", "Jr", "Dr");
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
            
            // Encode each component individually to handle delimiter characters
            var encodedComponents = components.Select(c => c != null ? encoding.Encode(c) : string.Empty);
            var value = string.Join(componentSeparator.ToString(), encodedComponents);
            
            // Use SetRawInternal since components are already encoded and we've added structural delimiters
            return SetRawInternal(value);
        }

        /// <summary>
        /// Sets multiple field components.
        /// </summary>

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
            var hl7DateTime = MessageHelper.LongDateWithFractionOfSecond(dateTime);
            return Set(hl7DateTime);
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
            var hl7Date = date.ToString("yyyyMMdd");
            return Set(hl7Date);
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
            return new ComponentMutator(_message, _segmentCode, _fieldIndex, componentIndex, _repetitionIndex ?? 1, _segmentInstanceIndex);
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
            return new SubComponentMutator(_message, _segmentCode, _fieldIndex, componentIndex, subComponentIndex, _repetitionIndex ?? 1, _segmentInstanceIndex);
        }

        /// <summary>
        /// Adds a new repetition with the specified value to this field.
        /// Returns a FieldMutator pointing to the newly created repetition, enabling immediate component setting.
        /// </summary>
        /// <param name="value">The value for the new repetition</param>
        /// <returns>A FieldMutator for the newly added repetition</returns>
        /// <example>
        /// <code>
        /// // Add repetitions with immediate component setting
        /// fluent.PID[3].Set("FirstID")
        ///     .AddRepetition("MRN001")                    // Returns mutator for repetition 2
        ///         .SetComponents("MRN", "001", "HOSPITAL") // Set components on repetition 2
        ///     .AddRepetition("ENC123")                    // Add simple value as repetition 3
        ///     .Field(7).Set("19850315");                  // Continue with other fields
        /// </code>
        /// </example>
        public FieldMutator AddRepetition(string value)
        {
            var collections = new Collections.FieldRepetitionCollection(_message, _segmentCode, _fieldIndex, _segmentInstanceIndex);
            var newRepetitionAccessor = collections.Add(value);
            
            // Return a FieldMutator pointing to the newly created repetition
            return new FieldMutator(_message, _segmentCode, _fieldIndex, 
                newRepetitionAccessor.GetRepetitionIndex(), _segmentInstanceIndex);
        }

        /// <summary>
        /// Adds a new empty repetition to this field that can be populated with components.
        /// Returns a FieldMutator pointing to the newly created repetition.
        /// </summary>
        /// <returns>A FieldMutator for the newly added repetition</returns>
        /// <example>
        /// <code>
        /// // Add empty repetition and set components
        /// fluent.PID[3].Set("FirstID")
        ///     .AddRepetition()                            // Add empty repetition 2
        ///         .SetComponents("MRN", "001", "HOSPITAL") // Set complex components
        ///     .AddRepetition()                            // Add empty repetition 3
        ///         .SetComponents("ENC", "123", "VISIT")    // Set different components
        ///     .Field(7).Set("19850315");                  // Continue fluent chain
        /// </code>
        /// </example>
        public FieldMutator AddRepetition()
        {
            return AddRepetition(string.Empty);
        }
    }
}