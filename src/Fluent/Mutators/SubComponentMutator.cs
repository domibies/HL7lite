using System;

namespace HL7lite.Fluent.Mutators
{
    /// <summary>
    /// Provides fluent methods for modifying HL7 subcomponent values with method chaining support.
    /// SubComponentMutator is the most granular level in the HL7 hierarchy and allows setting values
    /// within components while supporting cross-level navigation to components and fields.
    /// </summary>
    /// <remarks>
    /// SubComponentMutator follows the HL7 hierarchy: Message → Segment → Field → Component → SubComponent.
    /// All operations are null-safe and will create missing segments/fields/components as needed.
    /// Supports encoding of HL7 delimiter characters, null value handling, and conditional operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Set a subcomponent value
    /// fluent.PID[5][1][1].Set("LastName");
    /// 
    /// // Chain operations across levels
    /// fluent.PID[5][1][1].Set("Smith")
    ///     .SubComponent(2).Set("Jr")
    ///     .Component(2).Set("John")
    ///     .Field(7).Set("19851225");
    /// 
    /// // Handle delimiters safely
    /// fluent.OBX[5][1][1].SetEncoded("Data with | delimiters");
    /// 
    /// // Conditional operations
    /// fluent.PID[5][1][1].SetIf("DefaultName", condition);
    /// </code>
    /// </example>
    public class SubComponentMutator
    {
        private readonly Message _message;
        private readonly string _segmentCode;
        private readonly int _fieldIndex;
        private readonly int _componentIndex;
        private readonly int _subComponentIndex;
        private readonly int _repetitionIndex;
        private readonly string _path;

        /// <summary>
        /// Initializes a new SubComponentMutator for the specified subcomponent location.
        /// Creates a mutator targeting the first repetition of the field.
        /// </summary>
        /// <param name="message">The HL7 message to modify</param>
        /// <param name="segmentCode">The segment code (e.g., "PID", "OBX")</param>
        /// <param name="fieldIndex">The 1-based field index</param>
        /// <param name="componentIndex">The 1-based component index</param>
        /// <param name="subComponentIndex">The 1-based subcomponent index</param>
        /// <exception cref="ArgumentNullException">Thrown when message or segmentCode is null</exception>
        /// <exception cref="ArgumentException">Thrown when segmentCode is empty or indices are invalid</exception>
        public SubComponentMutator(Message message, string segmentCode, int fieldIndex, int componentIndex, int subComponentIndex)
            : this(message, segmentCode, fieldIndex, componentIndex, subComponentIndex, 1)
        {
        }

        /// <summary>
        /// Initializes a new SubComponentMutator for the specified subcomponent location and field repetition.
        /// </summary>
        /// <param name="message">The HL7 message to modify</param>
        /// <param name="segmentCode">The segment code (e.g., "PID", "OBX")</param>
        /// <param name="fieldIndex">The 1-based field index</param>
        /// <param name="componentIndex">The 1-based component index</param>
        /// <param name="subComponentIndex">The 1-based subcomponent index</param>
        /// <param name="repetitionIndex">The 1-based field repetition index</param>
        /// <exception cref="ArgumentNullException">Thrown when message or segmentCode is null</exception>
        /// <exception cref="ArgumentException">Thrown when segmentCode is empty or indices are invalid</exception>
        public SubComponentMutator(Message message, string segmentCode, int fieldIndex, int componentIndex, int subComponentIndex, int repetitionIndex)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            if (segmentCode == null)
                throw new ArgumentNullException(nameof(segmentCode));
            if (string.IsNullOrEmpty(segmentCode))
                throw new ArgumentException("Segment code cannot be empty", nameof(segmentCode));
            _segmentCode = segmentCode;
            _fieldIndex = fieldIndex > 0 ? fieldIndex : throw new ArgumentException("Field index must be greater than 0", nameof(fieldIndex));
            _componentIndex = componentIndex > 0 ? componentIndex : throw new ArgumentException("Component index must be greater than 0", nameof(componentIndex));
            _subComponentIndex = subComponentIndex > 0 ? subComponentIndex : throw new ArgumentException("SubComponent index must be greater than 0", nameof(subComponentIndex));
            _repetitionIndex = repetitionIndex > 0 ? repetitionIndex : 1;
            _path = $"{_segmentCode}.{_fieldIndex}({_repetitionIndex}).{_componentIndex}.{_subComponentIndex}";
        }

        /// <summary>
        /// Sets the subcomponent to the specified value. Creates the segment, field, and component if they don't exist.
        /// Null values are converted to empty strings. For explicit HL7 null values, use the SetNull() method.
        /// </summary>
        /// <param name="value">The value to set. Null values are converted to empty strings.</param>
        /// <returns>The SubComponentMutator for method chaining</returns>
        /// <example>
        /// <code>
        /// // Set subcomponent value
        /// fluent.PID[5][1][1].Set("Smith");
        /// 
        /// // Chain multiple operations
        /// fluent.PID[5][1][1].Set("Smith")
        ///     .SubComponent(2).Set("Jr");
        /// </code>
        /// </example>
        public SubComponentMutator Set(string value)
        {
            // Ensure segment exists
            try
            {
                _message.DefaultSegment(_segmentCode);
            }
            catch (InvalidOperationException)
            {
                // Segment doesn't exist, create it
                var newSegment = new Segment(_message.Encoding)
                {
                    Name = _segmentCode,
                    Value = _segmentCode
                };
                _message.AddNewSegment(newSegment);
            }
            
            _message.PutValue(_path, value ?? string.Empty);
            return this;
        }

        /// <summary>
        /// Sets the subcomponent value after encoding any HL7 delimiter characters.
        /// Use this method when your value contains characters like |, ^, ~, \, or &amp;
        /// that need to be safely stored in the HL7 message.
        /// </summary>
        /// <param name="value">The value to encode and set</param>
        /// <returns>The SubComponentMutator for method chaining</returns>
        /// <example>
        /// <code>
        /// // Set subcomponent with special characters
        /// fluent.PID[5][1][1].SetEncoded("Smith&Jones");
        /// 
        /// // Chain with other operations
        /// fluent.PID[5][1][1].SetEncoded("Complex|Value")
        ///     .SubComponent(2).Set("Simple");
        /// </code>
        /// </example>
        public SubComponentMutator SetEncoded(string value)
        {
            if (value == null)
            {
                return Set(null);
            }

            var encodedValue = _message.Encoding.Encode(value);
            return Set(encodedValue);
        }

        /// <summary>
        /// Sets the subcomponent to HL7 null value.
        /// In HL7, null values are represented as empty strings ("").
        /// </summary>
        /// <returns>The SubComponentMutator for method chaining</returns>
        /// <example>
        /// <code>
        /// // Set subcomponent to explicit null
        /// fluent.PID[5][1][2].SetNull();
        /// 
        /// // Chain with other operations
        /// fluent.PID[5][1][2].SetNull()
        ///     .SubComponent(3).Set("Value");
        /// </code>
        /// </example>
        public SubComponentMutator SetNull()
        {
            _message.PutValue(_path, _message.Encoding.PresentButNull);
            return this;
        }

        /// <summary>
        /// Clears the subcomponent by setting it to an empty string. This is different from Null()
        /// which sets an explicit HL7 null value.
        /// </summary>
        /// <returns>The SubComponentMutator for method chaining</returns>
        public SubComponentMutator Clear()
        {
            _message.PutValue(_path, string.Empty);
            return this;
        }

        /// <summary>
        /// Sets the subcomponent value conditionally based on a boolean condition.
        /// If the condition is false, the subcomponent is not modified.
        /// </summary>
        /// <param name="value">The value to set if condition is true</param>
        /// <param name="condition">The condition to evaluate</param>
        /// <returns>The SubComponentMutator for method chaining</returns>
        /// <example>
        /// <code>
        /// // Set suffix only if provided
        /// fluent.PID[5][1][2].SetIf("Jr", patient.HasSuffix);
        /// 
        /// // Chain with other conditional operations
        /// fluent.PID[5][1][2].SetIf("Jr", hasSuffix)
        ///     .SubComponent(3).SetIf("III", hasGenerationSuffix);
        /// </code>
        /// </example>
        public SubComponentMutator SetIf(string value, bool condition)
        {
            if (condition)
            {
                return Set(value);
            }
            return this;
        }

        /// <summary>
        /// Navigates to a different subcomponent within the same component.
        /// </summary>
        /// <param name="subComponentIndex">The 1-based subcomponent index to navigate to.</param>
        /// <returns>A SubComponentMutator for the target subcomponent.</returns>
        public SubComponentMutator SubComponent(int subComponentIndex)
        {
            if (subComponentIndex <= 0)
                throw new ArgumentException("SubComponent index must be greater than 0", nameof(subComponentIndex));

            // Create and return a new SubComponentMutator for the target subcomponent
            return new SubComponentMutator(_message, _segmentCode, _fieldIndex, _componentIndex, subComponentIndex, _repetitionIndex);
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
            return new ComponentMutator(_message, _segmentCode, _fieldIndex, componentIndex, _repetitionIndex);
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
            return new FieldMutator(_message, _segmentCode, fieldIndex);
        }
    }
}