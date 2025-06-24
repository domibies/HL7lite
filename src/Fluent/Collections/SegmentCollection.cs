using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HL7lite.Fluent.Accessors;

namespace HL7lite.Fluent.Collections
{
    /// <summary>
    /// Represents a collection of segments of the same type with LINQ support.
    /// Indexer uses 0-based indexing (C# convention), while Segment() method uses 1-based indexing (HL7 convention).
    /// </summary>
    public class SegmentCollection : IEnumerable<SegmentAccessor>
    {
        private readonly Message _message;
        private readonly string _segmentName;
        private readonly Dictionary<int, SegmentAccessor> _cache = new Dictionary<int, SegmentAccessor>();

        /// <summary>
        /// Initializes a new SegmentCollection.
        /// </summary>
        public SegmentCollection(Message message, string segmentName)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            if (segmentName == null)
                throw new ArgumentNullException(nameof(segmentName));
            if (string.IsNullOrEmpty(segmentName))
                throw new ArgumentException("Segment name cannot be empty", nameof(segmentName));
            _segmentName = segmentName;
        }

        /// <summary>
        /// Gets the number of segments of this type in the message
        /// </summary>
        public int Count
        {
            get
            {
                if (!_message.SegmentList.ContainsKey(_segmentName))
                    return 0;
                return _message.SegmentList[_segmentName].Count;
            }
        }

        /// <summary>
        /// Gets a segment accessor by zero-based index (C# convention)
        /// </summary>
        /// <param name="index">The 0-based index of the segment</param>
        public SegmentAccessor this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range. Count is {Count}");

                if (!_cache.ContainsKey(index))
                {
                    _cache[index] = new SegmentAccessor(_message, _segmentName).Instance(index);
                }
                return _cache[index];
            }
        }

        /// <summary>
        /// Gets a segment accessor by one-based segment number (HL7 convention)
        /// </summary>
        /// <param name="segmentNumber">The 1-based segment number</param>
        /// <returns>A SegmentAccessor for the specified segment</returns>
        public SegmentAccessor Segment(int segmentNumber)
        {
            if (segmentNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(segmentNumber), "Segment number must be greater than 0 (1-based)");
            
            return this[segmentNumber - 1];
        }

        /// <summary>
        /// Adds a new segment of this type to the message
        /// </summary>
        /// <returns>A SegmentAccessor for the newly created segment</returns>
        public SegmentAccessor Add()
        {
            var newSegment = new Segment(_message.Encoding)
            {
                Name = _segmentName,
                Value = _segmentName
            };
            _message.AddNewSegment(newSegment);
            
            // Clear cache for the new index to ensure fresh accessor
            var newIndex = Count - 1;
            if (_cache.ContainsKey(newIndex))
                _cache.Remove(newIndex);
            
            return this[newIndex];
        }

        /// <summary>
        /// Adds a deep copy of an existing segment to the message.
        /// The segment is deep copied to ensure independence between messages.
        /// </summary>
        /// <param name="segment">The segment to copy and add</param>
        /// <returns>A SegmentAccessor for the added segment copy</returns>
        /// <exception cref="ArgumentNullException">Thrown when segment is null</exception>
        /// <exception cref="ArgumentException">Thrown when segment name doesn't match collection type</exception>
        public SegmentAccessor AddCopy(Segment segment)
        {
            if (segment == null)
                throw new ArgumentNullException(nameof(segment));
            
            if (segment.Name != _segmentName)
                throw new ArgumentException($"Segment name '{segment.Name}' does not match collection type '{_segmentName}'", nameof(segment));
            
            // Create a deep copy to ensure independence
            var segmentCopy = segment.DeepCopy();
            _message.AddNewSegment(segmentCopy);
            
            // Clear cache for the new index to ensure fresh accessor
            var newIndex = Count - 1;
            if (_cache.ContainsKey(newIndex))
                _cache.Remove(newIndex);
            
            return this[newIndex];
        }

        /// <summary>
        /// Removes a segment at the specified one-based segment number
        /// </summary>
        /// <param name="segmentNumber">The 1-based segment number to remove</param>
        public void RemoveSegment(int segmentNumber)
        {
            if (segmentNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(segmentNumber), "Segment number must be greater than 0 (1-based)");
            
            int index = segmentNumber - 1;
            if (index >= Count)
                throw new ArgumentOutOfRangeException(nameof(segmentNumber), $"Segment number {segmentNumber} is out of range. Count is {Count}");

            _message.RemoveSegment(_segmentName, index);
            
            // Clear cache for all indices from the removed index onwards
            var keysToRemove = _cache.Keys.Where(k => k >= index).ToList();
            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }
        }

        /// <summary>
        /// Removes all segments of this type from the message
        /// </summary>
        public void Clear()
        {
            while (Count > 0)
            {
                _message.RemoveSegment(_segmentName, 0);
            }
            _cache.Clear();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection
        /// </summary>
        public IEnumerator<SegmentAccessor> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}