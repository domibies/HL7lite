using System;

namespace HL7lite.Fluent
{
    /// <summary>
    /// Extension methods for string to provide fluent HL7 parsing capabilities
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Attempts to parse an HL7 message string into a FluentMessage.
        /// Never throws exceptions - returns a result object indicating success or failure.
        /// </summary>
        /// <param name="hl7Message">The HL7 message string to parse</param>
        /// <param name="validate">Whether to validate the message structure during parsing (default: true)</param>
        /// <returns>A FluentParseResult containing either the parsed message or error information</returns>
        public static FluentParseResult TryParse(this string hl7Message, bool validate = true)
        {
            if (hl7Message == null)
                return FluentParseResult.Failure("HL7 message cannot be null");

            if (string.IsNullOrWhiteSpace(hl7Message))
                return FluentParseResult.Failure("HL7 message cannot be empty");

            try
            {
                var message = new Message(hl7Message);
                message.ParseMessage(serializeCheck: false, validate: validate);
                return FluentParseResult.Success(new FluentMessage(message));
            }
            catch (HL7Exception ex)
            {
                return FluentParseResult.Failure(ex.Message, ex.ErrorCode);
            }
            catch (Exception ex)
            {
                return FluentParseResult.Failure($"Unexpected error during parsing: {ex.Message}");
            }
        }
    }
}