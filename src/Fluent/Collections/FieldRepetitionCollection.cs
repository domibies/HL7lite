using System;
using System.Collections;
using System.Collections.Generic;
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
        private readonly Dictionary<int, FieldAccessor> _cache = new Dictionary<int, FieldAccessor>();

        /// <summary>
        /// Initializes a new instance of the FieldRepetitionCollection class.
        /// </summary>
        /// <param name="message">The HL7 message containing the field.</param>
        /// <param name="segmentName">The name of the segment containing the field.</param>
        /// <param name="fieldIndex">The 1-based field index.</param>
        public FieldRepetitionCollection(Message message, string segmentName, int fieldIndex)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _segmentName = segmentName ?? throw new ArgumentNullException(nameof(segmentName));
            _fieldIndex = fieldIndex;
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
                    var segment = _message.DefaultSegment(_segmentName);
                    if (segment == null)
                        return 0;

                    var field = segment.Fields(_fieldIndex);
                    if (field == null)
                        return 0;

                    return field.Repetitions().Count;
                }
                catch
                {
                    // Segment doesn't exist
                    return 0;
                }
            }
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
                var accessor = new FieldAccessor(_message, _segmentName, _fieldIndex, index + 1);
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
    }
}