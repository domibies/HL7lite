using System;
using System.Linq;

namespace HL7lite.Fluent.Mutators
{
    public class ComponentMutator
    {
        private readonly Message _message;
        private readonly string _segmentCode;
        private readonly int _fieldIndex;
        private readonly int _componentIndex;
        private readonly int _repetitionIndex;
        private readonly string _path;

        public ComponentMutator(Message message, string segmentCode, int fieldIndex, int componentIndex)
            : this(message, segmentCode, fieldIndex, componentIndex, 1)
        {
        }

        public ComponentMutator(Message message, string segmentCode, int fieldIndex, int componentIndex, int repetitionIndex)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            if (segmentCode == null)
                throw new ArgumentNullException(nameof(segmentCode));
            if (string.IsNullOrEmpty(segmentCode))
                throw new ArgumentException("Segment code cannot be empty", nameof(segmentCode));
            _segmentCode = segmentCode;
            _fieldIndex = fieldIndex > 0 ? fieldIndex : throw new ArgumentException("Field index must be greater than 0", nameof(fieldIndex));
            _componentIndex = componentIndex > 0 ? componentIndex : throw new ArgumentException("Component index must be greater than 0", nameof(componentIndex));
            _repetitionIndex = repetitionIndex > 0 ? repetitionIndex : 1;
            _path = $"{_segmentCode}.{_fieldIndex}({_repetitionIndex}).{_componentIndex}";
        }

        public ComponentMutator Value(string value)
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

        public ComponentMutator Null()
        {
            _message.PutValue(_path, "\"\"");
            return this;
        }

        public ComponentMutator Clear()
        {
            _message.PutValue(_path, string.Empty);
            return this;
        }

        public ComponentMutator SubComponents(params string[] subComponents)
        {
            if (subComponents == null || subComponents.Length == 0)
            {
                return Clear();
            }

            var encoding = _message.Encoding;
            var subComponentSeparator = encoding.SubComponentDelimiter;
            var value = string.Join(subComponentSeparator.ToString(), subComponents.Select(s => s ?? string.Empty));
            
            return Value(value);
        }

        public ComponentMutator ValueIf(string value, bool condition)
        {
            if (condition)
            {
                return Value(value);
            }
            return this;
        }
    }
}