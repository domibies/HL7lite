using System;
using System.Collections.Generic;
using HL7lite.Fluent.Accessors;
using HL7lite.Fluent.Collections;

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

        /// <summary>
        /// Gets accessor for EVN (Event Type) segment
        /// </summary>
        public SegmentAccessor EVN => this["EVN"];

        /// <summary>
        /// Gets accessor for PV2 (Patient Visit - Additional Info) segment
        /// </summary>
        public SegmentAccessor PV2 => this["PV2"];

        /// <summary>
        /// Gets accessor for NK1 (Next of Kin/Associated Parties) segment
        /// </summary>
        public SegmentAccessor NK1 => this["NK1"];

        /// <summary>
        /// Gets accessor for AL1 (Patient Allergy Information) segment
        /// </summary>
        public SegmentAccessor AL1 => this["AL1"];

        /// <summary>
        /// Gets accessor for PRB (Problem Details) segment
        /// </summary>
        public SegmentAccessor PRB => this["PRB"];

        /// <summary>
        /// Gets accessor for ROL (Role) segment
        /// </summary>
        public SegmentAccessor ROL => this["ROL"];

        /// <summary>
        /// Gets accessor for GT1 (Guarantor) segment
        /// </summary>
        public SegmentAccessor GT1 => this["GT1"];

        /// <summary>
        /// Gets accessor for IN2 (Insurance Additional Info) segment
        /// </summary>
        public SegmentAccessor IN2 => this["IN2"];

        /// <summary>
        /// Gets accessor for IN3 (Insurance Additional Info - Certification) segment
        /// </summary>
        public SegmentAccessor IN3 => this["IN3"];

        /// <summary>
        /// Gets accessor for MRG (Merge Patient Information) segment
        /// </summary>
        public SegmentAccessor MRG => this["MRG"];

        /// <summary>
        /// Gets accessor for IAM (Patient Adverse Reaction Information) segment
        /// </summary>
        public SegmentAccessor IAM => this["IAM"];

        /// <summary>
        /// Gets accessor for ACC (Accident) segment
        /// </summary>
        public SegmentAccessor ACC => this["ACC"];

        /// <summary>
        /// Gets accessor for UB1 (UB82 Data) segment
        /// </summary>
        public SegmentAccessor UB1 => this["UB1"];

        /// <summary>
        /// Gets accessor for UB2 (UB92 Data) segment
        /// </summary>
        public SegmentAccessor UB2 => this["UB2"];

        /// <summary>
        /// Gets accessor for PR1 (Procedures) segment
        /// </summary>
        public SegmentAccessor PR1 => this["PR1"];

        /// <summary>
        /// Gets accessor for NTE (Notes and Comments) segment
        /// </summary>
        public SegmentAccessor NTE => this["NTE"];

        /// <summary>
        /// Gets accessor for ORC (Common Order) segment
        /// </summary>
        public SegmentAccessor ORC => this["ORC"];

        /// <summary>
        /// Gets accessor for RXE (Pharmacy/Treatment Encoded Order) segment
        /// </summary>
        public SegmentAccessor RXE => this["RXE"];

        /// <summary>
        /// Gets accessor for RXR (Pharmacy/Treatment Route) segment
        /// </summary>
        public SegmentAccessor RXR => this["RXR"];

        /// <summary>
        /// Gets accessor for RXA (Pharmacy/Treatment Administration) segment
        /// </summary>
        public SegmentAccessor RXA => this["RXA"];

        /// <summary>
        /// Gets accessor for RXD (Pharmacy/Treatment Dispense) segment
        /// </summary>
        public SegmentAccessor RXD => this["RXD"];

        /// <summary>
        /// Gets accessor for RXG (Pharmacy/Treatment Give) segment
        /// </summary>
        public SegmentAccessor RXG => this["RXG"];

        /// <summary>
        /// Gets accessor for RXO (Pharmacy/Treatment Order) segment
        /// </summary>
        public SegmentAccessor RXO => this["RXO"];

        /// <summary>
        /// Gets accessor for SCH (Scheduling Activity Information) segment
        /// </summary>
        public SegmentAccessor SCH => this["SCH"];

        /// <summary>
        /// Gets accessor for TXA (Transcription Document Header) segment
        /// </summary>
        public SegmentAccessor TXA => this["TXA"];

        /// <summary>
        /// Gets accessor for FT1 (Financial Transaction) segment
        /// </summary>
        public SegmentAccessor FT1 => this["FT1"];

        /// <summary>
        /// Gets accessor for AIS (Appointment Information - Service) segment
        /// </summary>
        public SegmentAccessor AIS => this["AIS"];

        /// <summary>
        /// Gets accessor for AIG (Appointment Information - General Resource) segment
        /// </summary>
        public SegmentAccessor AIG => this["AIG"];

        /// <summary>
        /// Gets accessor for AIL (Appointment Information - Location Resource) segment
        /// </summary>
        public SegmentAccessor AIL => this["AIL"];

        /// <summary>
        /// Gets accessor for AIP (Appointment Information - Personnel Resource) segment
        /// </summary>
        public SegmentAccessor AIP => this["AIP"];

        /// <summary>
        /// Gets a collection of segments of the specified type with LINQ support.
        /// Provides 0-based indexer access and 1-based Segment() method access.
        /// </summary>
        /// <param name="segmentName">The segment name (e.g., "DG1", "OBX", "IN1")</param>
        /// <returns>A SegmentCollection for accessing multiple segments of the same type</returns>
        public SegmentCollection Segments(string segmentName)
        {
            if (segmentName == null)
                throw new ArgumentNullException(nameof(segmentName));

            return new SegmentCollection(_message, segmentName);
        }
    }
}