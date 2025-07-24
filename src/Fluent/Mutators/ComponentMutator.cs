using System;
using System.Linq;

namespace HL7lite.Fluent.Mutators
{
    /// <summary>
    /// Provides fluent methods for modifying HL7 component values.
    /// </summary>
    public class ComponentMutator
    {
        private readonly Message _message;
        private readonly string _segmentCode;
        private readonly int _fieldIndex;
        private readonly int _componentIndex;
        private readonly int _repetitionIndex;
        private readonly int _segmentInstanceIndex;
        private readonly string _path;

        /// <summary>
        /// Initializes a new ComponentMutator.
        /// </summary>
        public ComponentMutator(Message message, string segmentCode, int fieldIndex, int componentIndex)
            : this(message, segmentCode, fieldIndex, componentIndex, 1, 0)
        {
        }

        /// <summary>
        /// Initializes a new ComponentMutator with repetition index.
        /// </summary>
        public ComponentMutator(Message message, string segmentCode, int fieldIndex, int componentIndex, int repetitionIndex)
            : this(message, segmentCode, fieldIndex, componentIndex, repetitionIndex, 0)
        {
        }

        /// <summary>
        /// Initializes a new ComponentMutator with repetition and segment instance indices.
        /// </summary>
        public ComponentMutator(Message message, string segmentCode, int fieldIndex, int componentIndex, int repetitionIndex, int segmentInstanceIndex)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            if (segmentCode == null)
                throw new ArgumentNullException(nameof(segmentCode));
            if (string.IsNullOrEmpty(segmentCode))
                throw new ArgumentException("Segment code cannot be empty", nameof(segmentCode));
            _segmentCode = segmentCode;
            _fieldIndex = fieldIndex > 0 ? fieldIndex : throw new ArgumentException("Field index must be greater than 0", nameof(fieldIndex));
            _componentIndex = componentIndex > 0 ? componentIndex : throw new ArgumentException("Component index must be greater than 0", nameof(componentIndex));
            _repetitionIndex = repetitionIndex > 0 ? repetitionIndex : 1;
            _segmentInstanceIndex = segmentInstanceIndex;
            _path = $"{_segmentCode}.{_fieldIndex}({_repetitionIndex}).{_componentIndex}";
        }

        /// <summary>
        /// Sets the component to the specified value, automatically encoding any HL7 delimiter characters.
        /// Creates the segment, field, and component if they don't exist.
        /// Null values are converted to empty strings. For explicit HL7 null values, use the SetNull() method.
        /// </summary>
        /// <param name="value">The value to set. Delimiter characters will be automatically encoded.</param>
        /// <returns>The ComponentMutator for method chaining</returns>
        /// <example>
        /// <code>
        /// // Set component value (delimiters automatically encoded)
        /// fluent.PID[5][1].Set("Smith &amp; Jones");  // &amp; automatically encoded to \T\
        /// 
        /// // Chain multiple operations
        /// fluent.PID[5][1].Set("Smith")
        ///     .Component(2).Set("John");
        /// </code>
        /// </example>
        public ComponentMutator Set(string value)
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
                    throw new InvalidOperationException($"Cannot set component on segment instance {_segmentInstanceIndex} that doesn't exist.");
                }
            }
            
            // Set the component value directly on the target segment
            var field = targetSegment.Fields(_fieldIndex);
            if (field == null)
            {
                targetSegment.AddNewField(string.Empty, _fieldIndex);
                field = targetSegment.Fields(_fieldIndex);
            }
            
            // Handle field repetitions if needed
            if (_repetitionIndex > 1)
            {
                field = field.EnsureRepetition(_repetitionIndex);
            }
            
            // Ensure component exists and set its value
            var component = field.EnsureComponent(_componentIndex);
            component.Value = encodedValue ?? string.Empty;
            
            // Mark field as componentized so it serializes properly
            field.IsComponentized = true;
            
            // Force rebuild of field value from components
            var fieldValue = field.SerializeValue();
            field.Value = fieldValue;
            
            return this;
        }

        /// <summary>
        /// Sets the component to a raw HL7 value that may contain subcomponent delimiters and encoded characters.
        /// This method validates that the value doesn't contain invalid delimiters for the component level.
        /// Use this for pre-encoded data or when building structured values.
        /// </summary>
        /// <param name="value">The raw HL7 value to set</param>
        /// <returns>The ComponentMutator for method chaining</returns>
        /// <exception cref="ArgumentException">If value contains field, repetition, or component delimiters</exception>
        /// <example>
        /// <code>
        /// // Set pre-encoded value with subcomponents
        /// fluent.PID[5][1].SetRaw("Smith\\T\\Jones&amp;Jr");  // \T\ is encoded &amp;, &amp; is subcomponent separator
        /// 
        /// // Invalid - contains component delimiter
        /// fluent.PID[5][1].SetRaw("Value^Other");  // Throws ArgumentException
        /// </code>
        /// </example>
        public ComponentMutator SetRaw(string value)
        {
            // Validate the value doesn't contain invalid delimiters
            if (value != null)
            {
                if (value.Contains(_message.Encoding.FieldDelimiter))
                {
                    throw new ArgumentException(
                        $"Component value cannot contain field delimiter '{_message.Encoding.FieldDelimiter}'. " +
                        "Use separate field assignments or encode the delimiter.");
                }
                
                if (value.Contains(_message.Encoding.RepeatDelimiter))
                {
                    throw new ArgumentException(
                        $"Component value cannot contain repetition delimiter '{_message.Encoding.RepeatDelimiter}'. " +
                        "Use separate repetitions or encode the delimiter.");
                }
                
                if (value.Contains(_message.Encoding.ComponentDelimiter))
                {
                    throw new ArgumentException(
                        $"Component value cannot contain component delimiter '{_message.Encoding.ComponentDelimiter}'. " +
                        "Use separate component assignments or encode the delimiter.");
                }
            }
            
            // Now set the raw value without encoding
            return SetRawInternal(value);
        }
        
        /// <summary>
        /// Internal method to set raw value without encoding
        /// </summary>
        private ComponentMutator SetRawInternal(string value)
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
                    throw new InvalidOperationException($"Cannot set component on segment instance {_segmentInstanceIndex} that doesn't exist.");
                }
            }
            
            // Set the component value directly on the target segment
            var field = targetSegment.Fields(_fieldIndex);
            if (field == null)
            {
                targetSegment.AddNewField(string.Empty, _fieldIndex);
                field = targetSegment.Fields(_fieldIndex);
            }
            
            // Handle field repetitions if needed
            if (_repetitionIndex > 1)
            {
                field = field.EnsureRepetition(_repetitionIndex);
            }
            
            // Ensure component exists and set its value
            var component = field.EnsureComponent(_componentIndex);
            component.Value = value ?? string.Empty;
            
            // Mark field as componentized so it serializes properly
            field.IsComponentized = true;
            
            // Force rebuild of field value from components
            var fieldValue = field.SerializeValue();
            field.Value = fieldValue;
            
            return this;
        }


        /// <summary>
        /// Sets the component to HL7 null value.
        /// In HL7, null values are represented as empty strings ("").
        /// </summary>
        /// <returns>The ComponentMutator for method chaining</returns>
        /// <example>
        /// <code>
        /// // Set component to explicit null
        /// fluent.PID[5][3].SetNull();
        /// 
        /// // Chain with other operations
        /// fluent.PID[5][3].SetNull()
        ///     .Component(4).Set("Jr");
        /// </code>
        /// </example>
        public ComponentMutator SetNull()
        {
            return Set(_message.Encoding.PresentButNull);
        }

        /// <summary>
        /// Clears the component value.
        /// </summary>
        public ComponentMutator Clear()
        {
            return Set(string.Empty);
        }

        /// <summary>
        /// Sets multiple subcomponent values in a single operation.
        /// Subcomponents are automatically encoded and joined with the HL7 subcomponent separator (&amp;).
        /// </summary>
        /// <param name="subComponents">The subcomponent values to set. Delimiter characters will be automatically encoded.</param>
        /// <returns>The ComponentMutator for method chaining</returns>
        /// <example>
        /// <code>
        /// // Set phone number subcomponents (delimiters automatically encoded)
        /// fluent.PID[13][1].SetSubComponents("555", "123-4567", "Home &amp; Office");
        /// 
        /// // Chain with other operations
        /// fluent.PID[13][1].SetSubComponents("555", "123-4567")
        ///     .Component(2).Set("Business");
        /// </code>
        /// </example>
        public ComponentMutator SetSubComponents(params string[] subComponents)
        {
            if (subComponents == null || subComponents.Length == 0)
            {
                return Clear();
            }

            var encoding = _message.Encoding;
            var subComponentSeparator = encoding.SubComponentDelimiter;
            
            // Encode each subcomponent individually to handle delimiter characters
            var encodedSubComponents = subComponents.Select(s => s != null ? encoding.Encode(s) : string.Empty);
            var value = string.Join(subComponentSeparator.ToString(), encodedSubComponents);
            
            // Use SetRawInternal since subcomponents are already encoded and we've added structural delimiters
            return SetRawInternal(value);
        }

        /// <summary>
        /// Sets the component value conditionally based on a boolean condition.
        /// If the condition is false, the component is not modified.
        /// </summary>
        /// <param name="value">The value to set if condition is true</param>
        /// <param name="condition">The condition to evaluate</param>
        /// <returns>The ComponentMutator for method chaining</returns>
        /// <example>
        /// <code>
        /// // Set middle name only if provided
        /// fluent.PID[5][3].SetIf(middleName, !string.IsNullOrEmpty(middleName));
        /// 
        /// // Chain with other conditional operations
        /// fluent.PID[5][3].SetIf(middleName, hasMiddleName)
        ///     .Component(4).SetIf("Jr", hasSuffix);
        /// </code>
        /// </example>
        public ComponentMutator SetIf(string value, bool condition)
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
        /// <returns>A FieldMutator for the target field.</returns>
        public FieldMutator Field(int fieldIndex)
        {
            if (fieldIndex <= 0)
                throw new ArgumentException("Field index must be greater than 0", nameof(fieldIndex));

            // Create and return a new FieldMutator for the target field
            return new FieldMutator(_message, _segmentCode, fieldIndex, null, _segmentInstanceIndex);
        }

        /// <summary>
        /// Navigates to a different component within the same field.
        /// </summary>
        /// <param name="componentIndex">The 1-based component index to navigate to.</param>
        /// <returns>A ComponentMutator for the target component.</returns>
        public ComponentMutator Component(int componentIndex)
        {
            if (componentIndex <= 0)
                throw new ArgumentException("Component index must be greater than 0", nameof(componentIndex));

            // Create and return a new ComponentMutator for the target component
            return new ComponentMutator(_message, _segmentCode, _fieldIndex, componentIndex, _repetitionIndex, _segmentInstanceIndex);
        }

        /// <summary>
        /// Navigates to a subcomponent within the current component.
        /// </summary>
        /// <param name="subComponentIndex">The 1-based subcomponent index to navigate to.</param>
        /// <returns>A SubComponentMutator for the target subcomponent.</returns>
        public SubComponentMutator SubComponent(int subComponentIndex)
        {
            if (subComponentIndex <= 0)
                throw new ArgumentException("SubComponent index must be greater than 0", nameof(subComponentIndex));

            // Create and return a new SubComponentMutator for the target subcomponent
            return new SubComponentMutator(_message, _segmentCode, _fieldIndex, _componentIndex, subComponentIndex, _repetitionIndex, _segmentInstanceIndex);
        }
    }
}