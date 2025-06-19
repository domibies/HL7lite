using System;
using System.Collections.Generic;
using HL7lite.Fluent.Accessors;
using HL7lite.Fluent.Builders;
using HL7lite.Fluent.Collections;
using HL7lite.Fluent.Querying;

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
        /// Gets a fluent builder for creating MSH (Message Header) segments with intelligent defaults.
        /// Provides grouped parameter setting and auto-generation features to prevent common errors.
        /// </summary>
        /// <example>
        /// <code>
        /// fluent.CreateMSH
        ///     .Sender("MyApp", "MyFacility")
        ///     .Receiver("TheirApp", "TheirFacility")
        ///     .MessageType("ADT^A01")
        ///     .AutoControlId()
        ///     .Production()
        ///     .Build();
        /// </code>
        /// </example>
        public MSHBuilder CreateMSH => new MSHBuilder(_message);

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

        /// <summary>
        /// Removes trailing empty delimiters from the message to create cleaner output.
        /// This method removes empty fields, components, and subcomponents from the end of segments.
        /// </summary>
        /// <param name="options">Options specifying which types of trailing delimiters to remove</param>
        public void RemoveTrailingDelimiters(MessageElement.RemoveDelimitersOptions options)
        {
            _message.RemoveTrailingDelimiters(options);
        }

        /// <summary>
        /// Removes all types of trailing empty delimiters from the message.
        /// This is equivalent to calling RemoveTrailingDelimiters(MessageElement.RemoveDelimitersOptions.All).
        /// </summary>
        public void RemoveTrailingDelimiters()
        {
            _message.RemoveTrailingDelimiters(MessageElement.RemoveDelimitersOptions.All);
        }

        /// <summary>
        /// Builds an acknowledgment (ACK) message for the current message.
        /// The ACK message swaps sender/receiver information and includes an MSA segment
        /// with acknowledgment code "AA" (Application Accept).
        /// </summary>
        /// <returns>A FluentMessage wrapping the ACK message, or null if the current message is already an ACK</returns>
        public FluentMessage GetAck()
        {
            var ackMessage = _message.GetACK();
            return ackMessage != null ? new FluentMessage(ackMessage) : null;
        }

        /// <summary>
        /// Builds a negative acknowledgment (NACK) message for the current message.
        /// The NACK message swaps sender/receiver information and includes an MSA segment
        /// with the specified acknowledgment code and error message.
        /// </summary>
        /// <param name="code">Acknowledgment code (e.g., "AR" for Application Reject, "AE" for Application Error)</param>
        /// <param name="errorMessage">Error message to include in MSA.3</param>
        /// <returns>A FluentMessage wrapping the NACK message, or null if the current message is already an ACK</returns>
        public FluentMessage GetNack(string code, string errorMessage)
        {
            var nackMessage = _message.GetNACK(code, errorMessage);
            return nackMessage != null ? new FluentMessage(nackMessage) : null;
        }

        /// <summary>
        /// Provides path-based access to HL7 message elements using the legacy path syntax.
        /// Supports field access ("PID.5.1"), repetitions ("PV1.7[2].1"), and component navigation.
        /// </summary>
        /// <param name="path">The HL7 path in format: SEGMENT.Field[.Component[.SubComponent]][repetition]</param>
        /// <returns>A PathAccessor for reading and writing values at the specified path</returns>
        /// <example>
        /// <code>
        /// // Read operations
        /// string name = fluent.Path("PID.5.1").Value;
        /// bool exists = fluent.Path("PID.5.1").Exists;
        /// 
        /// // Write operations
        /// fluent.Path("PID.5.1").Set("Smith")
        ///       .Path("PID.5.2").Set("John")
        ///       .Path("PID.7").Put("19850315");
        /// </code>
        /// </example>
        public PathAccessor Path(string path)
        {
            return new PathAccessor(_message, path, this);
        }

        /// <summary>
        /// Creates a deep copy of the current message, returning a completely independent FluentMessage.
        /// The copy includes all segments, fields, components, and repetitions in their current state.
        /// Changes to the copy will not affect the original message and vice versa.
        /// </summary>
        /// <returns>A new FluentMessage containing a deep copy of all message data</returns>
        /// <example>
        /// <code>
        /// var copy = original.Copy();
        /// copy.PID[5].Set().Value("NewName"); // Original remains unchanged
        /// </code>
        /// </example>
        public FluentMessage Copy()
        {
            // Handle empty message case
            var segments = _message.Segments();
            if (segments == null || segments.Count == 0)
            {
                return new FluentMessage(new Message());
            }

            // Use serialization-based copying to ensure all data is captured correctly
            // This approach works for both parsed and programmatically built messages
            var serializedMessage = _message.SerializeMessage(validate: false);
            
            // Create new message from serialized string
            var copiedMessage = new Message(serializedMessage);
            copiedMessage.ParseMessage(serializeCheck: false, validate: false);
            
            return new FluentMessage(copiedMessage);
        }

        /// <summary>
        /// Serializes the HL7 message to a string representation with optional validation.
        /// The serialized message includes all segments, fields, and components with proper
        /// HL7 encoding and delimiters.
        /// </summary>
        /// <param name="validate">Whether to validate the message before serialization (default: false)</param>
        /// <returns>The serialized HL7 message as a string with segments separated by carriage returns</returns>
        /// <example>
        /// <code>
        /// // Serialize without validation (faster)
        /// string hl7String = fluent.SerializeMessage();
        /// 
        /// // Serialize with validation
        /// string validatedHL7 = fluent.SerializeMessage(validate: true);
        /// </code>
        /// </example>
        public string SerializeMessage(bool validate = false)
        {
            return _message.SerializeMessage(validate);
        }
    }
}