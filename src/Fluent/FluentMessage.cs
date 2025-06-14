using System;
using System.Collections.Generic;
using HL7lite.Fluent.Accessors;

namespace HL7lite.Fluent
{
    /// <summary>
    /// Provides a fluent, null-safe API for accessing HL7 message elements
    /// </summary>
    public class FluentMessage
    {
        private readonly Message _message;
        private readonly Dictionary<string, SegmentAccessor> _segmentCache = new Dictionary<string, SegmentAccessor>();

        /// <summary>
        /// Initializes a new FluentMessage wrapper around an existing Message
        /// </summary>
        /// <param name="message">The Message to wrap</param>
        /// <exception cref="ArgumentNullException">Thrown when message is null</exception>
        public FluentMessage(Message message)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
        }

        /// <summary>
        /// Gets the underlying Message instance
        /// </summary>
        public Message UnderlyingMessage => _message;

        /// <summary>
        /// Gets a segment accessor by name
        /// </summary>
        /// <param name="segmentName">The segment name (e.g., "PID", "MSH")</param>
        /// <returns>A segment accessor that handles non-existent segments gracefully</returns>
        public SegmentAccessor this[string segmentName]
        {
            get
            {
                if (segmentName == null)
                    throw new ArgumentNullException(nameof(segmentName));

                // Use cached accessor if available
                if (_segmentCache.TryGetValue(segmentName, out var cached))
                    return cached;

                // Create new accessor and cache it
                var accessor = new SegmentAccessor(_message, segmentName);
                _segmentCache[segmentName] = accessor;
                return accessor;
            }
        }

        /// <summary>
        /// Gets accessor for MSH (Message Header) segment
        /// </summary>
        public SegmentAccessor MSH => this["MSH"];

        /// <summary>
        /// Gets accessor for PID (Patient Identification) segment
        /// </summary>
        public SegmentAccessor PID => this["PID"];

        /// <summary>
        /// Gets accessor for PV1 (Patient Visit) segment
        /// </summary>
        public SegmentAccessor PV1 => this["PV1"];

        /// <summary>
        /// Gets accessor for OBR (Observation Request) segment
        /// </summary>
        public SegmentAccessor OBR => this["OBR"];

        /// <summary>
        /// Gets accessor for OBX (Observation/Result) segment
        /// </summary>
        public SegmentAccessor OBX => this["OBX"];

        /// <summary>
        /// Gets accessor for DG1 (Diagnosis) segment
        /// </summary>
        public SegmentAccessor DG1 => this["DG1"];

        /// <summary>
        /// Gets accessor for IN1 (Insurance) segment
        /// </summary>
        public SegmentAccessor IN1 => this["IN1"];
    }
}