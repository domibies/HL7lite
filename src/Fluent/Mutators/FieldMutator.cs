using System;
using System.Linq;

namespace HL7lite.Fluent.Mutators
{
    public class FieldMutator
    {
        private readonly Message _message;
        private readonly string _segmentCode;
        private readonly int _fieldIndex;
        private readonly string _path;

        public FieldMutator(Message message, string segmentCode, int fieldIndex)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            if (segmentCode == null)
                throw new ArgumentNullException(nameof(segmentCode));
            if (string.IsNullOrEmpty(segmentCode))
                throw new ArgumentException("Segment code cannot be empty", nameof(segmentCode));
            _segmentCode = segmentCode;
            _fieldIndex = fieldIndex > 0 ? fieldIndex : throw new ArgumentException("Field index must be greater than 0", nameof(fieldIndex));
            _path = $"{_segmentCode}.{_fieldIndex}";
        }

        public FieldMutator Value(string value)
        {
            // Ensure segment exists
            try
            {
                _message.DefaultSegment(_segmentCode);
            }
            catch (InvalidOperationException)
            {
                // Segment doesn't exist, create it
                var newSegment = new Segment(_message.Encoding)
                {
                    Name = _segmentCode,
                    Value = _segmentCode
                };
                _message.AddNewSegment(newSegment);
            }
            
            _message.PutValue(_path, value ?? string.Empty);
            return this;
        }

        public FieldMutator Null()
        {
            _message.PutValue(_path, "\"\"");
            return this;
        }

        public FieldMutator Clear()
        {
            _message.PutValue(_path, string.Empty);
            return this;
        }

        public FieldMutator Components(params string[] components)
        {
            if (components == null || components.Length == 0)
            {
                return Clear();
            }

            var encoding = _message.Encoding;
            var componentSeparator = encoding.ComponentDelimiter;
            var value = string.Join(componentSeparator.ToString(), components.Select(c => c ?? string.Empty));
            
            return Value(value);
        }

        public FieldMutator ValueIf(string value, bool condition)
        {
            if (condition)
            {
                return Value(value);
            }
            return this;
        }

        public FieldMutator AddRepetition(string value)
        {
            Segment segment;
            try
            {
                segment = _message.DefaultSegment(_segmentCode);
            }
            catch (InvalidOperationException)
            {
                // Create a new segment if it doesn't exist
                var newSegment = new Segment(_message.Encoding)
                {
                    Name = _segmentCode,
                    Value = _segmentCode
                };
                _message.AddNewSegment(newSegment);
                segment = _message.DefaultSegment(_segmentCode);
            }

            var field = segment.Fields(_fieldIndex);
            if (field == null || string.IsNullOrEmpty(field.Value))
            {
                return Value(value);
            }

            // Check if field already has repetitions
            if (field.HasRepetitions)
            {
                var repetitionCount = field.Repetitions().Count;
                var repetitionPath = $"{_path}({repetitionCount + 1})";
                _message.PutValue(repetitionPath, value ?? string.Empty);
            }
            else
            {
                // Convert single field to repetition
                var existingValue = field.Value;
                _message.PutValue($"{_path}(1)", existingValue);
                _message.PutValue($"{_path}(2)", value ?? string.Empty);
            }

            return this;
        }
    }
}