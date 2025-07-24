using System;

namespace HL7lite.Fluent
{
    /// <summary>
    /// Represents the result of attempting to parse an HL7 message into a FluentMessage.
    /// Provides a safe, exception-free way to handle parsing errors.
    /// </summary>
    public class FluentParseResult
    {
        /// <summary>
        /// Gets whether the parse operation was successful.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets the parsed FluentMessage when successful, null when failed.
        /// </summary>
        public FluentMessage Message { get; }

        /// <summary>
        /// Gets the error message when parsing fails, null when successful.
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// Gets the error code when parsing fails, null when successful.
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// Private constructor for success results.
        /// </summary>
        private FluentParseResult(FluentMessage message)
        {
            IsSuccess = true;
            Message = message;
            ErrorMessage = null;
            ErrorCode = null;
        }

        /// <summary>
        /// Private constructor for failure results.
        /// </summary>
        private FluentParseResult(string errorMessage, string errorCode = null)
        {
            IsSuccess = false;
            Message = null;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Creates a successful parse result.
        /// </summary>
        /// <param name="message">The successfully parsed FluentMessage</param>
        /// <returns>A successful FluentParseResult</returns>
        internal static FluentParseResult Success(FluentMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            return new FluentParseResult(message);
        }

        /// <summary>
        /// Creates a failed parse result.
        /// </summary>
        /// <param name="errorMessage">The error message describing why parsing failed</param>
        /// <param name="errorCode">Optional error code from HL7Exception</param>
        /// <returns>A failed FluentParseResult</returns>
        internal static FluentParseResult Failure(string errorMessage, string errorCode = null)
        {
            if (string.IsNullOrEmpty(errorMessage))
                throw new ArgumentNullException(nameof(errorMessage));
            return new FluentParseResult(errorMessage, errorCode);
        }

        /// <summary>
        /// Implicit conversion to FluentMessage. Returns null if parsing failed.
        /// </summary>
        /// <param name="result">The FluentParseResult to convert</param>
        public static implicit operator FluentMessage(FluentParseResult result)
        {
            return result?.Message;
        }

        /// <summary>
        /// Implicit conversion to bool. Returns true if parsing succeeded.
        /// </summary>
        /// <param name="result">The FluentParseResult to convert</param>
        public static implicit operator bool(FluentParseResult result)
        {
            return result?.IsSuccess ?? false;
        }

        /// <summary>
        /// Deconstructs the result into success flag and message.
        /// </summary>
        /// <param name="isSuccess">Whether the parse was successful</param>
        /// <param name="message">The parsed message (null if failed)</param>
        public void Deconstruct(out bool isSuccess, out FluentMessage message)
        {
            isSuccess = IsSuccess;
            message = Message;
        }

        /// <summary>
        /// Deconstructs the result into all components.
        /// </summary>
        /// <param name="isSuccess">Whether the parse was successful</param>
        /// <param name="message">The parsed message (null if failed)</param>
        /// <param name="errorMessage">The error message (null if successful)</param>
        /// <param name="errorCode">The error code (null if successful)</param>
        public void Deconstruct(out bool isSuccess, out FluentMessage message, out string errorMessage, out string errorCode)
        {
            isSuccess = IsSuccess;
            message = Message;
            errorMessage = ErrorMessage;
            errorCode = ErrorCode;
        }
    }
}