using System;
using System.IO;
using System.Text;

namespace HL7lite.Fluent.Builders
{
    /// <summary>
    /// Provides a fluent interface for serializing HL7 messages with various options.
    /// </summary>
    public class SerializationBuilder
    {
        private readonly Message _message;
        private readonly FluentMessage _fluentMessage;
        private bool _validate = false;
        private bool _removeTrailingDelimiters = false;
        private MessageElement.RemoveDelimitersOptions _removeDelimitersOptions = MessageElement.RemoveDelimitersOptions.All;
        private Encoding _encoding = Encoding.UTF8;

        internal SerializationBuilder(Message message, FluentMessage fluentMessage)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _fluentMessage = fluentMessage ?? throw new ArgumentNullException(nameof(fluentMessage));
        }

        /// <summary>
        /// Enables validation before serialization.
        /// </summary>
        /// <returns>The builder for method chaining</returns>
        public SerializationBuilder WithValidation()
        {
            _validate = true;
            return this;
        }

        /// <summary>
        /// Removes trailing delimiters from segments before serialization.
        /// Uses RemoveDelimitersOptions.All by default.
        /// </summary>
        /// <returns>The builder for method chaining</returns>
        public SerializationBuilder WithoutTrailingDelimiters()
        {
            _removeTrailingDelimiters = true;
            _removeDelimitersOptions = MessageElement.RemoveDelimitersOptions.All;
            return this;
        }

        /// <summary>
        /// Removes trailing delimiters from segments before serialization.
        /// </summary>
        /// <param name="options">Specifies which delimiters to remove</param>
        /// <returns>The builder for method chaining</returns>
        public SerializationBuilder WithoutTrailingDelimiters(MessageElement.RemoveDelimitersOptions options)
        {
            _removeTrailingDelimiters = true;
            _removeDelimitersOptions = options;
            return this;
        }

        /// <summary>
        /// Sets the text encoding to use for file/stream operations.
        /// </summary>
        /// <param name="encoding">The encoding to use (default: UTF8)</param>
        /// <returns>The builder for method chaining</returns>
        public SerializationBuilder WithEncoding(Encoding encoding)
        {
            _encoding = encoding ?? Encoding.UTF8;
            return this;
        }

        /// <summary>
        /// Serializes the message to a string.
        /// </summary>
        /// <returns>The serialized HL7 message</returns>
        public override string ToString()
        {
            PrepareMessage();
            return _message.SerializeMessage(_validate);
        }

        /// <summary>
        /// Serializes the message and writes it to a file.
        /// </summary>
        /// <param name="filePath">The file path to write to</param>
        /// <returns>The FluentMessage for method chaining</returns>
        public FluentMessage ToFile(string filePath)
        {
            var content = ToString();
            File.WriteAllText(filePath, content, _encoding);
            return _fluentMessage;
        }

        /// <summary>
        /// Serializes the message and writes it to a stream.
        /// </summary>
        /// <param name="stream">The stream to write to</param>
        /// <returns>The FluentMessage for method chaining</returns>
        public FluentMessage ToStream(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (!stream.CanWrite) throw new ArgumentException("Stream must be writable", nameof(stream));

            var content = ToString();
            var bytes = _encoding.GetBytes(content);
            stream.Write(bytes, 0, bytes.Length);
            return _fluentMessage;
        }

        /// <summary>
        /// Serializes the message to a byte array.
        /// </summary>
        /// <returns>The serialized message as a byte array</returns>
        public byte[] ToBytes()
        {
            var content = ToString();
            return _encoding.GetBytes(content);
        }

        /// <summary>
        /// Validates the message and returns any validation errors without throwing.
        /// </summary>
        /// <param name="serializedMessage">The serialized message output</param>
        /// <returns>True if valid, false if validation errors exist</returns>
        public bool TrySerialize(out string serializedMessage, out string validationError)
        {
            validationError = null;
            serializedMessage = null;

            try
            {
                PrepareMessage();
                
                if (_validate)
                {
                    // Try to validate by parsing
                    var testMessage = new Message(_message.SerializeMessage(false));
                    testMessage.ParseMessage(validate: true);
                }

                serializedMessage = _message.SerializeMessage(false);
                return true;
            }
            catch (Exception ex)
            {
                validationError = ex.Message;
                return false;
            }
        }

        private void PrepareMessage()
        {
            if (_removeTrailingDelimiters)
            {
                _message.RemoveTrailingDelimiters(_removeDelimitersOptions);
            }
        }
    }
}