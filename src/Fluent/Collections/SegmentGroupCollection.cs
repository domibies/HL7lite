using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HL7lite.Fluent.Accessors;

namespace HL7lite.Fluent.Collections
{
    /// <summary>
    /// Represents a collection of segment groups with LINQ support.
    /// Indexer uses 0-based indexing (C# convention), while Group() method uses 1-based indexing (HL7 convention).
    /// </summary>
    public class SegmentGroupCollection : IEnumerable<SegmentGroup>
    {
        private readonly List<SegmentGroup> _groups;

        /// <summary>
        /// Initializes a new SegmentGroupCollection from a SegmentCollection.
        /// </summary>
        /// <param name="segmentCollection">The segment collection to analyze for groups</param>
        /// <param name="message">The underlying message for gap detection</param>
        /// <param name="segmentName">The segment type name</param>
        public SegmentGroupCollection(SegmentCollection segmentCollection, Message message, string segmentName)
        {
            if (segmentCollection == null)
                throw new ArgumentNullException(nameof(segmentCollection));
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (segmentName == null)
                throw new ArgumentNullException(nameof(segmentName));

            _groups = BuildGroups(segmentCollection, message, segmentName);
        }

        /// <summary>
        /// Gets the number of segment groups in this collection
        /// </summary>
        public int Count => _groups.Count;

        /// <summary>
        /// Gets whether this collection contains any groups
        /// </summary>
        public bool HasGroups => _groups.Count > 0;

        /// <summary>
        /// Gets the total number of groups (same as Count)
        /// </summary>
        public int GroupCount => _groups.Count;

        /// <summary>
        /// Gets a segment group by zero-based index (C# convention)
        /// </summary>
        /// <param name="index">The 0-based index of the group</param>
        /// <returns>A SegmentGroup at the specified index</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range</exception>
        public SegmentGroup this[int index]
        {
            get
            {
                if (index < 0 || index >= _groups.Count)
                    throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range. Group count is {_groups.Count}");
                
                return _groups[index];
            }
        }

        /// <summary>
        /// Gets a segment group by one-based group number (HL7 convention)
        /// </summary>
        /// <param name="groupNumber">The 1-based group number</param>
        /// <returns>A SegmentGroup for the specified group number</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when group number is out of range</exception>
        public SegmentGroup Group(int groupNumber)
        {
            if (groupNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(groupNumber), "Group number must be greater than 0 (1-based)");
            
            return this[groupNumber - 1];
        }

        /// <summary>
        /// Returns an enumerator that iterates through the groups
        /// </summary>
        public IEnumerator<SegmentGroup> GetEnumerator()
        {
            return _groups.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Builds segment groups from a SegmentCollection by analyzing segment order in the message.
        /// Groups are formed by consecutive segments of the same type.
        /// </summary>
        private static List<SegmentGroup> BuildGroups(SegmentCollection segmentCollection, Message message, string segmentName)
        {
            var groups = new List<SegmentGroup>();
            
            if (segmentCollection.Count == 0)
                return groups;

            // Get all segments from the message in sequence order
            var allSegments = new List<SegmentInfo>();
            foreach (var kvp in message.SegmentList)
            {
                foreach (var segment in kvp.Value)
                {
                    allSegments.Add(new SegmentInfo 
                    { 
                        Name = segment.Name, 
                        SequenceNo = segment.SequenceNo 
                    });
                }
            }
            allSegments.Sort((a, b) => a.SequenceNo.CompareTo(b.SequenceNo));
            
            // Find positions and sequence numbers of target segments
            var targetPositions = new List<int>();
            var targetSequenceNos = new List<int>();
            
            for (int i = 0; i < allSegments.Count; i++)
            {
                if (allSegments[i].Name == segmentName)
                {
                    targetPositions.Add(i);
                    targetSequenceNos.Add(allSegments[i].SequenceNo);
                }
            }
            
            if (targetPositions.Count == 0)
                return groups;
            
            // Create mapping from sequence number to accessor
            var accessorsBySequenceNo = new Dictionary<int, SegmentAccessor>();
            for (int i = 0; i < segmentCollection.Count; i++)
            {
                var accessor = segmentCollection[i];
                if (accessor.Exists)
                {
                    accessorsBySequenceNo[accessor._segment.SequenceNo] = accessor;
                }
            }
            
            // Group consecutive segments
            var currentGroup = new List<SegmentAccessor>();
            
            for (int i = 0; i < targetPositions.Count; i++)
            {
                var position = targetPositions[i];
                var sequenceNo = targetSequenceNos[i];
                
                // Check if this position is consecutive to the previous one
                bool isConsecutive = i == 0 || position == targetPositions[i - 1] + 1;
                
                if (!isConsecutive && currentGroup.Count > 0)
                {
                    // Gap detected - finish current group
                    groups.Add(new SegmentGroup(currentGroup, segmentName));
                    currentGroup = new List<SegmentAccessor>();
                }
                
                // Add segment to current group
                if (accessorsBySequenceNo.ContainsKey(sequenceNo))
                {
                    currentGroup.Add(accessorsBySequenceNo[sequenceNo]);
                }
            }
            
            // Add final group
            if (currentGroup.Count > 0)
            {
                groups.Add(new SegmentGroup(currentGroup, segmentName));
            }

            return groups;
        }

        /// <summary>
        /// Helper class to store segment information
        /// </summary>
        private class SegmentInfo
        {
            public string Name { get; set; }
            public int SequenceNo { get; set; }
            public Segment Segment { get; set; }
        }
    }
}