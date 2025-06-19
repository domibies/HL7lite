using System;

namespace HL7lite.Fluent
{
    /// <summary>
    /// Extension methods for string to provide fluent HL7 parsing capabilities
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Parses an HL7 message string and returns a FluentMessage for convenient access.
        /// This is a convenience method that combines parsing and fluent API creation in one step.
        /// </summary>
        /// <param name="hl7Message">The HL7 message string to parse</param>
        /// <param name="validate">Whether to validate the message structure during parsing (default: true)</param>
        /// <returns>A FluentMessage wrapper around the parsed message</returns>
        /// <exception cref="ArgumentNullException">Thrown when hl7Message is null</exception>
        /// <exception cref="HL7Exception">Thrown when the message cannot be parsed</exception>
        /// <example>
        /// <code>
        /// string hl7 = "MSH|^~\\&amp;|SENDER|...";
        /// var fluent = hl7.ToFluentMessage();
        /// string patientName = fluent.PID[5].Value;
        /// </code>
        /// </example>
        public static FluentMessage ToFluentMessage(this string hl7Message, bool validate = true)
        {
            if (hl7Message == null)
                throw new ArgumentNullException(nameof(hl7Message));

            var message = new Message(hl7Message);
            message.ParseMessage(serializeCheck: false, validate: validate);
            return new FluentMessage(message);
        }
    }
}