using System;
using System.Collections.Generic;

namespace HL7lite.Fluent.Accessors
{
    /// <summary>
    /// Provides fluent access to HL7 segments with null-safe operations
    /// </summary>
    public class SegmentAccessor
    {
        protected readonly Message _message;
        protected readonly string _segmentName;
        private readonly Segment _segment;
        private readonly Dictionary<int, FieldAccessor> _fieldCache = new Dictionary<int, FieldAccessor>();

        public SegmentAccessor(Message message, string segmentName)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _segmentName = segmentName ?? throw new ArgumentNullException(nameof(segmentName));
            
            // Try to get the first segment of this type
            _segment = _message.SegmentList.ContainsKey(segmentName) && _message.SegmentList[segmentName].Count > 0
                ? _message.SegmentList[segmentName][0]
                : null;
        }

        /// <summary>
        /// Gets whether this segment exists in the message
        /// </summary>
        public bool Exists => _segment != null;

        /// <summary>
        /// Gets whether there are multiple instances of this segment type
        /// </summary>
        public bool HasMultiple => _message.SegmentList.ContainsKey(_segmentName) && _message.SegmentList[_segmentName].Count > 1;

        /// <summary>
        /// Gets the total count of this segment type
        /// </summary>
        public int Count => _message.SegmentList.ContainsKey(_segmentName) ? _message.SegmentList[_segmentName].Count : 0;

        /// <summary>
        /// Gets whether there is exactly one instance of this segment
        /// </summary>
        public bool IsSingle => Count == 1;

        /// <summary>
        /// Gets a field accessor by 1-based field number
        /// </summary>
        /// <param name="fieldNumber">The field number (1-based)</param>
        /// <returns>A field accessor that handles non-existent fields gracefully</returns>
        public virtual FieldAccessor this[int fieldNumber]
        {
            get
            {
                if (fieldNumber < 1)
                    throw new ArgumentOutOfRangeException(nameof(fieldNumber), "Field numbers must be 1-based (greater than 0)");

                // Use cached accessor if available
                if (_fieldCache.TryGetValue(fieldNumber, out var cached))
                    return cached;

                // Create new accessor and cache it
                var accessor = new FieldAccessor(_message, _segmentName, fieldNumber);
                _fieldCache[fieldNumber] = accessor;
                return accessor;
            }
        }

        /// <summary>
        /// Gets a field accessor by 1-based field number (same as indexer)
        /// </summary>
        /// <param name="fieldNumber">The field number (1-based)</param>
        /// <returns>A field accessor that handles non-existent fields gracefully</returns>
        public virtual FieldAccessor Field(int fieldNumber) => this[fieldNumber];

        /// <summary>
        /// Gets a specific instance of this segment type (for multiple segments)
        /// </summary>
        /// <param name="instanceIndex">The instance index (0-based)</param>
        /// <returns>A segment accessor for the specific instance</returns>
        public SegmentAccessor Instance(int instanceIndex)
        {
            if (instanceIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(instanceIndex), "Instance index must be non-negative");

            // Create a new accessor for the specific instance
            var segments = _message.SegmentList.ContainsKey(_segmentName) ? _message.SegmentList[_segmentName] : null;
            if (segments == null || instanceIndex >= segments.Count)
            {
                // Return a non-existent accessor
                return new SegmentAccessor(_message, _segmentName + "_NONEXISTENT_" + instanceIndex);
            }

            // Create a specialized accessor for this specific instance
            return new SpecificInstanceSegmentAccessor(_message, _segmentName, instanceIndex);
        }
    }

    /// <summary>
    /// Specialized segment accessor for a specific instance of a repeating segment
    /// </summary>
    internal class SpecificInstanceSegmentAccessor : SegmentAccessor
    {
        private readonly int _instanceIndex;
        private readonly Segment _specificSegment;
        private readonly Dictionary<int, FieldAccessor> _specificFieldCache = new Dictionary<int, FieldAccessor>();

        public SpecificInstanceSegmentAccessor(Message message, string segmentName, int instanceIndex) 
            : base(message, segmentName)
        {
            _instanceIndex = instanceIndex;
            
            // Get the specific segment instance
            var segments = message.SegmentList.ContainsKey(segmentName) ? message.SegmentList[segmentName] : null;
            _specificSegment = segments != null && instanceIndex < segments.Count ? segments[instanceIndex] : null;
        }

        /// <summary>
        /// Gets whether this specific segment instance exists
        /// </summary>
        public new bool Exists => _specificSegment != null;

        /// <summary>
        /// Specific instances don't have multiple (they represent a single instance)
        /// </summary>
        public new bool HasMultiple => false;

        /// <summary>
        /// Specific instances have count of 1 if they exist, 0 otherwise
        /// </summary>
        public new int Count => _specificSegment != null ? 1 : 0;

        /// <summary>
        /// Gets whether this specific instance is the only one (same as Exists for specific instances)
        /// </summary>
        public new bool IsSingle => _specificSegment != null;

        /// <summary>
        /// Gets a field accessor by 1-based field number for this specific segment instance
        /// </summary>
        /// <param name="fieldNumber">The field number (1-based)</param>
        /// <returns>A field accessor that handles this specific segment instance</returns>
        public override FieldAccessor this[int fieldNumber]
        {
            get
            {
                if (fieldNumber < 1)
                    throw new ArgumentOutOfRangeException(nameof(fieldNumber), "Field numbers must be 1-based (greater than 0)");

                if (_specificFieldCache.TryGetValue(fieldNumber, out var cached))
                    return cached;

                var accessor = new FieldAccessor(_message, _segmentName, fieldNumber, 1, _instanceIndex);
                _specificFieldCache[fieldNumber] = accessor;
                return accessor;
            }
        }

        /// <summary>
        /// Gets a field accessor by 1-based field number (same as indexer) for this specific segment instance
        /// </summary>
        /// <param name="fieldNumber">The field number (1-based)</param>
        /// <returns>A field accessor that handles this specific segment instance</returns>
        public override FieldAccessor Field(int fieldNumber) => this[fieldNumber];
    }
}