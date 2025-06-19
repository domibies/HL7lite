using System;

namespace HL7lite.Fluent.Builders
{
    /// <summary>
    /// Fluent builder for creating MSH (Message Header) segments with intelligent defaults
    /// and grouped parameter setting to prevent common errors.
    /// </summary>
    public class MSHBuilder
    {
        private readonly Message _message;
        private string _sendingApplication;
        private string _sendingFacility;
        private string _receivingApplication;
        private string _receivingFacility;
        private string _security;
        private string _messageType;
        private string _messageControlId;
        private string _processingId = "P"; // Default to Production
        private string _version = "2.5"; // Default to HL7 v2.5
        private bool _autoControlId = false;

        internal MSHBuilder(Message message)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
        }

        /// <summary>
        /// Sets the sending application and facility information.
        /// This is who is sending the message.
        /// </summary>
        /// <param name="application">Sending application name</param>
        /// <param name="facility">Sending facility name</param>
        /// <returns>The MSHBuilder for method chaining</returns>
        public MSHBuilder Sender(string application, string facility)
        {
            _sendingApplication = application ?? throw new ArgumentNullException(nameof(application));
            _sendingFacility = facility ?? throw new ArgumentNullException(nameof(facility));
            return this;
        }

        /// <summary>
        /// Sets the receiving application and facility information.
        /// This is who should receive the message.
        /// </summary>
        /// <param name="application">Receiving application name</param>
        /// <param name="facility">Receiving facility name</param>
        /// <returns>The MSHBuilder for method chaining</returns>
        public MSHBuilder Receiver(string application, string facility)
        {
            _receivingApplication = application ?? throw new ArgumentNullException(nameof(application));
            _receivingFacility = facility ?? throw new ArgumentNullException(nameof(facility));
            return this;
        }

        /// <summary>
        /// Sets the message type (e.g., "ADT^A01", "ORU^R01").
        /// </summary>
        /// <param name="messageType">The HL7 message type</param>
        /// <returns>The MSHBuilder for method chaining</returns>
        public MSHBuilder MessageType(string messageType)
        {
            _messageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
            return this;
        }

        /// <summary>
        /// Sets the security information for the message.
        /// </summary>
        /// <param name="security">Security information (optional)</param>
        /// <returns>The MSHBuilder for method chaining</returns>
        public MSHBuilder Security(string security)
        {
            _security = security;
            return this;
        }

        /// <summary>
        /// Sets the message control ID manually.
        /// If not called, you can use AutoControlId() to generate one automatically.
        /// </summary>
        /// <param name="controlId">Unique message control identifier</param>
        /// <returns>The MSHBuilder for method chaining</returns>
        public MSHBuilder ControlId(string controlId)
        {
            _messageControlId = controlId ?? throw new ArgumentNullException(nameof(controlId));
            _autoControlId = false;
            return this;
        }

        /// <summary>
        /// Automatically generates a unique message control ID using timestamp and random component.
        /// </summary>
        /// <returns>The MSHBuilder for method chaining</returns>
        public MSHBuilder AutoControlId()
        {
            _autoControlId = true;
            return this;
        }

        /// <summary>
        /// Sets the HL7 version (defaults to "2.5").
        /// </summary>
        /// <param name="version">HL7 version string</param>
        /// <returns>The MSHBuilder for method chaining</returns>
        public MSHBuilder Version(string version)
        {
            _version = version ?? throw new ArgumentNullException(nameof(version));
            return this;
        }

        /// <summary>
        /// Sets processing ID to "P" (Production).
        /// </summary>
        /// <returns>The MSHBuilder for method chaining</returns>
        public MSHBuilder Production()
        {
            _processingId = "P";
            return this;
        }

        /// <summary>
        /// Sets processing ID to "T" (Training/Test).
        /// </summary>
        /// <returns>The MSHBuilder for method chaining</returns>
        public MSHBuilder Test()
        {
            _processingId = "T";
            return this;
        }

        /// <summary>
        /// Sets processing ID to "D" (Debugging).
        /// </summary>
        /// <returns>The MSHBuilder for method chaining</returns>
        public MSHBuilder Debug()
        {
            _processingId = "D";
            return this;
        }

        /// <summary>
        /// Sets a custom processing ID.
        /// </summary>
        /// <param name="processingId">Processing ID (typically "P", "T", or "D")</param>
        /// <returns>The MSHBuilder for method chaining</returns>
        public MSHBuilder ProcessingId(string processingId)
        {
            _processingId = processingId ?? throw new ArgumentNullException(nameof(processingId));
            return this;
        }

        /// <summary>
        /// Enables automatic timestamp generation (default behavior).
        /// The timestamp will be set to the current date/time when Build() is called.
        /// Note: This is always enabled as timestamps are auto-generated by the underlying MSH creation.
        /// </summary>
        /// <returns>The MSHBuilder for method chaining</returns>
        public MSHBuilder AutoTimestamp()
        {
            // Auto-timestamp is always enabled in the underlying AddSegmentMSH method
            return this;
        }

        /// <summary>
        /// Builds and adds the MSH segment to the message.
        /// Validates that all required fields have been set.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when required fields are missing</exception>
        public void Build()
        {
            ValidateRequiredFields();

            // Generate auto values if needed
            var controlId = _autoControlId ? GenerateControlId() : _messageControlId;

            // Use the legacy AddSegmentMSH method to ensure proper encoding and validation
            _message.AddSegmentMSH(
                _sendingApplication,
                _sendingFacility,
                _receivingApplication,
                _receivingFacility,
                _security,
                _messageType,
                controlId,
                _processingId,
                _version
            );
        }

        private void ValidateRequiredFields()
        {
            if (string.IsNullOrEmpty(_sendingApplication))
                throw new InvalidOperationException("Sending application must be set using Sender() method");
            if (string.IsNullOrEmpty(_sendingFacility))
                throw new InvalidOperationException("Sending facility must be set using Sender() method");
            if (string.IsNullOrEmpty(_receivingApplication))
                throw new InvalidOperationException("Receiving application must be set using Receiver() method");
            if (string.IsNullOrEmpty(_receivingFacility))
                throw new InvalidOperationException("Receiving facility must be set using Receiver() method");
            if (string.IsNullOrEmpty(_messageType))
                throw new InvalidOperationException("Message type must be set using MessageType() method");
            if (!_autoControlId && string.IsNullOrEmpty(_messageControlId))
                throw new InvalidOperationException("Message control ID must be set using ControlId() or AutoControlId() method");
        }

        private string GenerateControlId()
        {
            // Generate a unique control ID using timestamp + random component
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"{timestamp}{random}";
        }
    }
}