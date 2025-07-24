using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HL7lite.Fluent.Accessors;

namespace HL7lite.Fluent.Collections
{
    /// <summary>
    /// Represents a group of consecutive segments of the same type.
    /// A segment group contains segments that appear consecutively in the message without gaps.
    /// </summary>
    public class SegmentGroup : IEnumerable<SegmentAccessor>
    {
        private readonly List<SegmentAccessor> _segments;
        private readonly string _segmentName;

        /// <summary>
        /// Initializes a new SegmentGroup with the specified segments.
        /// </summary>
        /// <param name="segments">The consecutive segments that form this group</param>
        /// <param name="segmentName">The segment type name</param>
        public SegmentGroup(IEnumerable<SegmentAccessor> segments, string segmentName)
        {
            _segments = segments?.ToList() ?? throw new ArgumentNullException(nameof(segments));
            _segmentName = segmentName ?? throw new ArgumentNullException(nameof(segmentName));
        }

        /// <summary>
        /// Gets the number of segments in this group
        /// </summary>
        public int Count => _segments.Count;

        /// <summary>
        /// Gets whether this group contains any segments
        /// </summary>
        public bool IsEmpty => _segments.Count == 0;

        /// <summary>
        /// Gets the first segment in this group, or null if the group is empty
        /// </summary>
        public SegmentAccessor First => _segments.Count > 0 ? _segments[0] : null;

        /// <summary>
        /// Gets the last segment in this group, or null if the group is empty
        /// </summary>
        public SegmentAccessor Last => _segments.Count > 0 ? _segments[_segments.Count - 1] : null;

        /// <summary>
        /// Gets the segment type name for this group
        /// </summary>
        public string SegmentName => _segmentName;

        /// <summary>
        /// Gets a segment accessor by zero-based index (C# convention)
        /// </summary>
        /// <param name="index">The 0-based index of the segment within the group</param>
        /// <returns>A SegmentAccessor for the specified segment</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range</exception>
        public SegmentAccessor this[int index]
        {
            get
            {
                if (index < 0 || index >= _segments.Count)
                    throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range. Group count is {_segments.Count}");
                
                return _segments[index];
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the segments in this group
        /// </summary>
        public IEnumerator<SegmentAccessor> GetEnumerator()
        {
            return _segments.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}