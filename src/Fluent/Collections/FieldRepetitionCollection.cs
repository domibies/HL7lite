using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HL7lite.Fluent.Accessors;

namespace HL7lite.Fluent.Collections
{
    /// <summary>
    /// Represents a collection of field repetitions for a specific field in an HL7 segment.
    /// Provides indexed access and LINQ support for field repetitions.
    /// </summary>
    public class FieldRepetitionCollection : IEnumerable<FieldAccessor>
    {
        private readonly Message _message;
        private readonly string _segmentName;
        private readonly int _fieldIndex;
        private readonly int _segmentInstanceIndex;
        private readonly Dictionary<int, FieldAccessor> _cache = new Dictionary<int, FieldAccessor>();

        /// <summary>
        /// Initializes a new instance of the FieldRepetitionCollection class.
        /// </summary>
        /// <param name="message">The HL7 message containing the field.</param>
        /// <param name="segmentName">The name of the segment containing the field.</param>
        /// <param name="fieldIndex">The 1-based field index.</param>
        public FieldRepetitionCollection(Message message, string segmentName, int fieldIndex)
            : this(message, segmentName, fieldIndex, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the FieldRepetitionCollection class with segment instance index.
        /// </summary>
        /// <param name="message">The HL7 message containing the field.</param>
        /// <param name="segmentName">The name of the segment containing the field.</param>
        /// <param name="fieldIndex">The 1-based field index.</param>
        /// <param name="segmentInstanceIndex">The 0-based segment instance index.</param>
        public FieldRepetitionCollection(Message message, string segmentName, int fieldIndex, int segmentInstanceIndex)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _segmentName = segmentName ?? throw new ArgumentNullException(nameof(segmentName));
            _fieldIndex = fieldIndex;
            _segmentInstanceIndex = segmentInstanceIndex;
        }

        /// <summary>
        /// Gets the number of repetitions for this field.
        /// </summary>
        public int Count
        {
            get
            {
                try
                {
                    var segment = GetSegmentInstance();
                    if (segment == null)
                        return 0;

                    var field = segment.Fields(_fieldIndex);
                    if (field == null)
                        return 0;

                    // If field has repetitions, return the repetition count
                    if (field.HasRepetitions)
                        return field.Repetitions().Count;
                    
                    // For single fields, check if the field is actually empty
                    // An empty field should return count 0, not 1
                    if (string.IsNullOrEmpty(field.Value) && field.ComponentList.Count == 0)
                        return 0;
                    
                    return 1;
                }
                catch
                {
                    // Segment doesn't exist
                    return 0;
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
        /// Gets the field repetition at the specified zero-based index (C# convention).
        /// </summary>
        /// <param name="index">The 0-based index of the repetition.</param>
        /// <returns>A FieldAccessor for the specified repetition.</returns>
        public FieldAccessor this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range. Valid range is 0 to {Count - 1}.");

                if (_cache.TryGetValue(index, out var cached))
                    return cached;

                // Create a new FieldAccessor for this specific repetition
                // repetitionIndex is 1-based in FieldAccessor constructor
                var accessor = new FieldAccessor(_message, _segmentName, _fieldIndex, index + 1, _segmentInstanceIndex);
                _cache[index] = accessor;
                return accessor;
            }
        }

        /// <summary>
        /// Gets the field repetition at the specified one-based repetition number (HL7 convention).
        /// </summary>
        /// <param name="repetitionNumber">The 1-based repetition number.</param>
        /// <returns>A FieldAccessor for the specified repetition.</returns>
        public FieldAccessor Repetition(int repetitionNumber)
        {
            if (repetitionNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(repetitionNumber), "Repetition number must be greater than 0 (1-based)");
            
            return this[repetitionNumber - 1];
        }

        /// <summary>
        /// Returns an enumerator that iterates through the field repetitions.
        /// </summary>
        public IEnumerator<FieldAccessor> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the field repetitions.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region Essential Removal Methods

        /// <summary>
        /// Removes all repetitions from the field.
        /// </summary>
        public void Clear()
        {
            var segment = GetSegmentInstance();
            if (segment == null)
                return;

            var field = segment.Fields(_fieldIndex);
            if (field == null)
                return;

            // Clear all repetitions and field content
            if (field.HasRepetitions)
            {
                field.RemoveRepetitions();
            }
            field.Value = "";
            field.ComponentList.Clear();
            
            _cache.Clear();
        }


        /// <summary>
        /// Removes a repetition at the specified one-based repetition number.
        /// </summary>
        /// <param name="repetitionNumber">The 1-based repetition number to remove.</param>
        public void RemoveRepetition(int repetitionNumber)
        {
            if (repetitionNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(repetitionNumber), "Repetition number must be greater than 0 (1-based)");
            
            if (repetitionNumber > Count)
                throw new ArgumentOutOfRangeException(nameof(repetitionNumber), $"Repetition number {repetitionNumber} is out of range. Valid range is 1 to {Count}.");

            var segment = GetSegmentInstance();
            var field = segment.Fields(_fieldIndex);
            
            // Use Field's method directly (it expects 1-based)
            field.RemoveRepetition(repetitionNumber);
            
            _cache.Clear();
        }

        #endregion

        #region Add Methods

        /// <summary>
        /// Adds a new repetition with the specified value to the field.
        /// Handles the transition from single field to repetitions automatically.
        /// This method follows the same pattern as SegmentCollection.Add().
        /// </summary>
        /// <param name="value">The value for the new repetition</param>
        /// <returns>A FieldAccessor for the newly added repetition</returns>
        public FieldAccessor Add(string value)
        {
            var segment = GetSegmentInstance();
            if (segment == null)
            {
                // Segment doesn't exist, create it
                var newSegment = new Segment(_message.Encoding)
                {
                    Name = _segmentName,
                    Value = _segmentName
                };
                _message.AddNewSegment(newSegment);
                segment = newSegment;
            }

            var field = segment.Fields(_fieldIndex);
            
            // Handle empty field case
            if (field == null || string.IsNullOrEmpty(field.Value))
            {
                // Create the field with the first value
                if (field == null)
                {
                    segment.AddNewField(value ?? string.Empty, _fieldIndex);
                }
                else
                {
                    field.Value = value ?? string.Empty;
                }
                
                // Clear cache and return accessor for the first repetition
                _cache.Clear();
                return new FieldAccessor(_message, _segmentName, _fieldIndex, 1, _segmentInstanceIndex);
            }

            // Handle existing field - add as repetition
            if (field.HasRepetitions)
            {
                // Field already has repetitions, add to the end
                var repetitionCount = field.Repetitions().Count;
                
                // Use direct field manipulation instead of PutValue to respect segment instance
                var newRepetition = field.EnsureRepetition(repetitionCount + 1);
                newRepetition.Value = value ?? string.Empty;
                
                // Clear cache and return accessor for the new repetition
                _cache.Clear();
                return new FieldAccessor(_message, _segmentName, _fieldIndex, repetitionCount + 1, _segmentInstanceIndex);
            }
            else
            {
                // Convert single field to repetitions using direct manipulation
                var existingValue = field.Value;
                
                // Create repetition structure
                var firstRepetition = field.EnsureRepetition(1);
                firstRepetition.Value = existingValue;
                
                var secondRepetition = field.EnsureRepetition(2);
                secondRepetition.Value = value ?? string.Empty;
                
                // Clear cache and return accessor for the new (second) repetition
                _cache.Clear();
                return new FieldAccessor(_message, _segmentName, _fieldIndex, 2, _segmentInstanceIndex);
            }
        }

        /// <summary>
        /// Adds a new empty repetition to the field that can be populated later.
        /// Handles the transition from single field to repetitions automatically.
        /// This method follows the same pattern as SegmentCollection.Add().
        /// </summary>
        /// <returns>A FieldAccessor for the newly added repetition</returns>
        public FieldAccessor Add()
        {
            return Add(string.Empty);
        }

        #endregion
    }
}