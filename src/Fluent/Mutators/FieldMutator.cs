using System;
using System.Linq;

namespace HL7lite.Fluent.Mutators
{
    public class FieldMutator
    {
        private readonly Message _message;
        private readonly string _segmentCode;
        private readonly int _fieldIndex;
        private readonly int? _repetitionIndex;
        private readonly int _segmentInstanceIndex;
        private readonly string _path;

        public FieldMutator(Message message, string segmentCode, int fieldIndex, int? repetitionIndex = null, int segmentInstanceIndex = 0)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            if (segmentCode == null)
                throw new ArgumentNullException(nameof(segmentCode));
            if (string.IsNullOrEmpty(segmentCode))
                throw new ArgumentException("Segment code cannot be empty", nameof(segmentCode));
            _segmentCode = segmentCode;
            _fieldIndex = fieldIndex > 0 ? fieldIndex : throw new ArgumentException("Field index must be greater than 0", nameof(fieldIndex));
            _repetitionIndex = repetitionIndex;
            _segmentInstanceIndex = segmentInstanceIndex;
            
            // Build path with optional repetition index
            _path = repetitionIndex.HasValue && repetitionIndex.Value > 0
                ? $"{_segmentCode}.{_fieldIndex}[{repetitionIndex.Value}]"
                : $"{_segmentCode}.{_fieldIndex}";
        }

        public FieldMutator Value(string value)
        {
            // Get the specific segment instance
            Segment targetSegment = null;
            
            if (_message.SegmentList.ContainsKey(_segmentCode))
            {
                var segments = _message.SegmentList[_segmentCode];
                if (_segmentInstanceIndex < segments.Count)
                {
                    targetSegment = segments[_segmentInstanceIndex];
                }
            }
            
            if (targetSegment == null)
            {
                // Segment instance doesn't exist, create it if it's the first instance
                if (_segmentInstanceIndex == 0)
                {
                    var newSegment = new Segment(_message.Encoding)
                    {
                        Name = _segmentCode,
                        Value = _segmentCode
                    };
                    _message.AddNewSegment(newSegment);
                    targetSegment = newSegment;
                }
                else
                {
                    throw new InvalidOperationException($"Cannot set field on segment instance {_segmentInstanceIndex} that doesn't exist.");
                }
            }
            
            // Set the field value directly on the target segment
            if (_repetitionIndex.HasValue && _repetitionIndex.Value > 1)
            {
                // Handle repetition-specific field setting
                var field = targetSegment.Fields(_fieldIndex);
                if (field == null)
                {
                    targetSegment.AddNewField(value ?? string.Empty, _fieldIndex);
                    field = targetSegment.Fields(_fieldIndex);
                }
                
                // Set specific repetition
                if (field.HasRepetitions)
                {
                    var repetitions = field.Repetitions();
                    if (_repetitionIndex.Value <= repetitions.Count)
                    {
                        repetitions[_repetitionIndex.Value - 1].Value = value ?? string.Empty;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Repetition {_repetitionIndex.Value} does not exist.");
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Field does not have repetitions.");
                }
            }
            else
            {
                // Set the field value (handles first repetition or non-repeated field)
                var field = targetSegment.Fields(_fieldIndex);
                if (field == null)
                {
                    targetSegment.AddNewField(value ?? string.Empty, _fieldIndex);
                }
                else
                {
                    field.Value = value ?? string.Empty;
                }
            }
            
            return this;
        }

        public FieldMutator Null()
        {
            return Value("\"\"");
        }

        public FieldMutator Clear()
        {
            return Value(string.Empty);
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
            // AddRepetition doesn't make sense on a specific repetition (other than the first)
            if (_repetitionIndex.HasValue && _repetitionIndex.Value > 1)
            {
                throw new InvalidOperationException("Cannot add a repetition when operating on a specific repetition. Use the field-level accessor instead.");
            }

            // Get the specific segment instance
            Segment segment = null;
            
            if (_message.SegmentList.ContainsKey(_segmentCode))
            {
                var segments = _message.SegmentList[_segmentCode];
                if (_segmentInstanceIndex < segments.Count)
                {
                    segment = segments[_segmentInstanceIndex];
                }
            }
            
            if (segment == null)
            {
                // Segment instance doesn't exist, create it if it's the first instance
                if (_segmentInstanceIndex == 0)
                {
                    var newSegment = new Segment(_message.Encoding)
                    {
                        Name = _segmentCode,
                        Value = _segmentCode
                    };
                    _message.AddNewSegment(newSegment);
                    segment = newSegment;
                }
                else
                {
                    throw new InvalidOperationException($"Cannot add repetition to segment instance {_segmentInstanceIndex} that doesn't exist.");
                }
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
                var repetitionPath = $"{_segmentCode}.{_fieldIndex}({repetitionCount + 1})";
                _message.PutValue(repetitionPath, value ?? string.Empty);
            }
            else
            {
                // Convert single field to repetition
                var existingValue = field.Value;
                _message.PutValue($"{_segmentCode}.{_fieldIndex}(1)", existingValue);
                _message.PutValue($"{_segmentCode}.{_fieldIndex}(2)", value ?? string.Empty);
            }

            return this;
        }
    }
}