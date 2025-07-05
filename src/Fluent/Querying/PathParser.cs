using System;
using System.Linq;
using System.Text.RegularExpressions;
using HL7lite.Fluent.Mutators;

namespace HL7lite.Fluent.Querying
{
    /// <summary>
    /// Represents a fully parsed HL7 path with all components identified.
    /// Supports the complete HL7 addressing matrix including segment and field repetitions.
    /// </summary>
    public class ParsedPath
    {
        /// <summary>
        /// The segment name (e.g., "PID", "MSH", "DG1")
        /// </summary>
        public string SegmentName { get; set; }
        
        /// <summary>
        /// The 1-based segment repetition index (1 if not specified)
        /// </summary>
        public int SegmentRepetition { get; set; } = 1;
        
        /// <summary>
        /// The 1-based field number (null if not specified)
        /// </summary>
        public int? FieldNumber { get; set; }
        
        /// <summary>
        /// The 1-based field repetition index (1 if not specified)
        /// </summary>
        public int FieldRepetition { get; set; } = 1;
        
        /// <summary>
        /// The 1-based component number (null if not specified)
        /// </summary>
        public int? ComponentNumber { get; set; }
        
        /// <summary>
        /// The 1-based subcomponent number (null if not specified)
        /// </summary>
        public int? SubComponentNumber { get; set; }
        
        /// <summary>
        /// Indicates if this path points to a segment level (no field specified)
        /// </summary>
        public bool IsSegmentLevel => !FieldNumber.HasValue;
        
        /// <summary>
        /// Indicates if this path points to a field level (field specified, no component)
        /// </summary>
        public bool IsFieldLevel => FieldNumber.HasValue && !ComponentNumber.HasValue;
        
        /// <summary>
        /// Indicates if this path points to a component level (component specified, no subcomponent)
        /// </summary>
        public bool IsComponentLevel => ComponentNumber.HasValue && !SubComponentNumber.HasValue;
        
        /// <summary>
        /// Indicates if this path points to a subcomponent level (deepest level)
        /// </summary>
        public bool IsSubComponentLevel => SubComponentNumber.HasValue;
        
        /// <summary>
        /// Returns a string representation of the parsed path for debugging
        /// </summary>
        public override string ToString()
        {
            var result = $"{SegmentName}";
            if (SegmentRepetition > 1) result += $"[{SegmentRepetition}]";
            if (FieldNumber.HasValue)
            {
                result += $".{FieldNumber}";
                if (FieldRepetition > 1) result += $"[{FieldRepetition}]";
            }
            if (ComponentNumber.HasValue) result += $".{ComponentNumber}";
            if (SubComponentNumber.HasValue) result += $".{SubComponentNumber}";
            return result;
        }
    }
    
    /// <summary>
    /// Comprehensive HL7 path parser that supports the full addressing matrix.
    /// Handles segment repetitions, field repetitions, components, and subcomponents.
    /// 
    /// Path Syntax Specification:
    /// SEGMENT[segRep].FIELD[fieldRep].COMPONENT.SUBCOMPONENT
    /// 
    /// Examples:
    /// - "PID" → PID segment (first instance)
    /// - "PID[2]" → PID segment (second instance)
    /// - "PID.5" → PID field 5 (first repetition)
    /// - "PID.3[2]" → PID field 3, second repetition
    /// - "DG1[3].4[1].2" → 3rd DG1 segment, field 4 first repetition, component 2
    /// - "PID[1].3[2].1.2" → 1st PID, field 3 second repetition, component 1, subcomponent 2
    /// </summary>
    public static class PathParser
    {
        // Regex patterns for parsing path components
        private static readonly Regex SegmentPattern = new Regex(@"^([A-Z][A-Z][A-Z0-9])(?:\[(\d+)\])?$", RegexOptions.Compiled);
        private static readonly Regex FieldPattern = new Regex(@"^(\d+)(?:\[(\d+)\])?$", RegexOptions.Compiled);
        private static readonly Regex ComponentPattern = new Regex(@"^(\d+)$", RegexOptions.Compiled);
        private static readonly Regex SubComponentPattern = new Regex(@"^(\d+)$", RegexOptions.Compiled);
        
        /// <summary>
        /// Parses an HL7 path string into its constituent components.
        /// Returns null if the path format is invalid.
        /// </summary>
        /// <param name="path">The path string to parse</param>
        /// <returns>ParsedPath object or null if invalid</returns>
        public static ParsedPath Parse(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;
                
            var parts = path.Split('.');
            if (parts.Length == 0 || parts.Length > 4)
                return null;
                
            var result = new ParsedPath();
            
            // Parse segment (required)
            var segmentMatch = SegmentPattern.Match(parts[0]);
            if (!segmentMatch.Success)
                return null;
                
            result.SegmentName = segmentMatch.Groups[1].Value;
            if (segmentMatch.Groups[2].Success && int.TryParse(segmentMatch.Groups[2].Value, out int segRep))
            {
                if (segRep < 1) return null; // Invalid repetition
                result.SegmentRepetition = segRep;
            }
            
            // Parse field (optional)
            if (parts.Length > 1)
            {
                var fieldMatch = FieldPattern.Match(parts[1]);
                if (!fieldMatch.Success)
                    return null;
                    
                if (int.TryParse(fieldMatch.Groups[1].Value, out int fieldNum))
                {
                    if (fieldNum < 1) return null; // Invalid field number
                    result.FieldNumber = fieldNum;
                }
                
                if (fieldMatch.Groups[2].Success && int.TryParse(fieldMatch.Groups[2].Value, out int fieldRep))
                {
                    if (fieldRep < 1) return null; // Invalid repetition
                    result.FieldRepetition = fieldRep;
                }
            }
            
            // Parse component (optional)
            if (parts.Length > 2)
            {
                var componentMatch = ComponentPattern.Match(parts[2]);
                if (!componentMatch.Success)
                    return null;
                    
                if (int.TryParse(componentMatch.Groups[1].Value, out int compNum))
                {
                    if (compNum < 1) return null; // Invalid component number
                    result.ComponentNumber = compNum;
                }
            }
            
            // Parse subcomponent (optional)
            if (parts.Length > 3)
            {
                var subComponentMatch = SubComponentPattern.Match(parts[3]);
                if (!subComponentMatch.Success)
                    return null;
                    
                if (int.TryParse(subComponentMatch.Groups[1].Value, out int subCompNum))
                {
                    if (subCompNum < 1) return null; // Invalid subcomponent number
                    result.SubComponentNumber = subCompNum;
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Validates if a path string has valid syntax without fully parsing it.
        /// Faster than Parse() when you only need validation.
        /// </summary>
        /// <param name="path">The path string to validate</param>
        /// <returns>True if the path has valid syntax</returns>
        public static bool IsValid(string path)
        {
            return Parse(path) != null;
        }
        
        /// <summary>
        /// Tries to parse a path string, returning success status and parsed result.
        /// </summary>
        /// <param name="path">The path string to parse</param>
        /// <param name="parsedPath">The parsed path result (null if parsing failed)</param>
        /// <returns>True if parsing succeeded</returns>
        public static bool TryParse(string path, out ParsedPath parsedPath)
        {
            parsedPath = Parse(path);
            return parsedPath != null;
        }
        
        /// <summary>
        /// Gets the value at the specified path in the message using the enhanced parser.
        /// Supports segment repetitions, field repetitions, components, and subcomponents.
        /// Returns empty string for non-existent paths (consistent with fluent API behavior).
        /// Uses the existing fluent API for navigation.
        /// </summary>
        /// <param name="message">The HL7 message to read from</param>
        /// <param name="path">The path to read (e.g., "DG1[2].3[1].4")</param>
        /// <returns>The decoded value at the path, or empty string if path doesn't exist</returns>
        public static string GetValue(Message message, string path)
        {
            if (message == null || string.IsNullOrEmpty(path))
                return "";
                
            var parsed = Parse(path);
            if (parsed == null)
                return ""; // Invalid path format
                
            try
            {
                var fluentMessage = new FluentMessage(message);
                
                // Navigate to segment using fluent API
                var segmentAccessor = parsed.SegmentRepetition == 1 
                    ? fluentMessage[parsed.SegmentName]
                    : fluentMessage.Segments(parsed.SegmentName).Segment(parsed.SegmentRepetition);
                
                // If only segment level requested
                if (parsed.IsSegmentLevel)
                {
                    // For segment level, we return the segment name (segments don't have "values" in the traditional sense)
                    return segmentAccessor.Exists ? parsed.SegmentName : "";
                }
                    
                // Navigate to field using fluent API
                var fieldAccessor = segmentAccessor[parsed.FieldNumber.Value];
                if (parsed.FieldRepetition > 1)
                    fieldAccessor = fieldAccessor.Repetition(parsed.FieldRepetition);
                    
                // If only field level requested
                if (parsed.IsFieldLevel)
                {
                    var fieldValue = fieldAccessor.Raw;
                    return fieldValue == null ? message.Encoding.PresentButNull : fieldValue;
                }
                    
                // Navigate to component using fluent API
                var componentAccessor = fieldAccessor[parsed.ComponentNumber.Value];
                    
                // If only component level requested
                if (parsed.IsComponentLevel)
                {
                    var componentValue = componentAccessor.Raw;
                    return componentValue == null ? message.Encoding.PresentButNull : componentValue;
                }
                    
                // Navigate to subcomponent using fluent API
                var subComponentAccessor = componentAccessor[parsed.SubComponentNumber.Value];
                var subComponentValue = subComponentAccessor.Raw;
                return subComponentValue == null ? message.Encoding.PresentButNull : subComponentValue;
            }
            catch
            {
                // Any navigation error returns empty string (consistent with fluent API)
                return "";
            }
        }
        
        /// <summary>
        /// Checks if the specified path exists in the message using the fluent API.
        /// Supports segment repetitions, field repetitions, components, and subcomponents.
        /// </summary>
        /// <param name="message">The HL7 message to check</param>
        /// <param name="path">The path to check</param>
        /// <returns>True if the path exists</returns>
        public static bool PathExists(Message message, string path)
        {
            if (message == null || string.IsNullOrEmpty(path))
                return false;
                
            var parsed = Parse(path);
            if (parsed == null)
                return false; // Invalid path format
                
            try
            {
                var fluentMessage = new FluentMessage(message);
                
                // Check segment existence using fluent API
                var segmentAccessor = parsed.SegmentRepetition == 1 
                    ? fluentMessage[parsed.SegmentName]
                    : fluentMessage.Segments(parsed.SegmentName).Segment(parsed.SegmentRepetition);
                
                if (!segmentAccessor.Exists)
                    return false;
                    
                // If only segment level requested
                if (parsed.IsSegmentLevel)
                    return true;
                    
                // Check field existence using fluent API
                var fieldAccessor = segmentAccessor[parsed.FieldNumber.Value];
                if (parsed.FieldRepetition > 1)
                    fieldAccessor = fieldAccessor.Repetition(parsed.FieldRepetition);
                    
                if (!fieldAccessor.Exists)
                    return false;
                    
                // If only field level requested
                if (parsed.IsFieldLevel)
                    return true;
                    
                // Check component existence using fluent API
                var componentAccessor = fieldAccessor[parsed.ComponentNumber.Value];
                if (!componentAccessor.Exists)
                    return false;
                    
                // If only component level requested
                if (parsed.IsComponentLevel)
                    return true;
                    
                // Check subcomponent existence using fluent API
                var subComponentAccessor = componentAccessor[parsed.SubComponentNumber.Value];
                return subComponentAccessor.Exists;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Sets the value at the specified path in the message using the enhanced parser.
        /// Supports segment repetitions, field repetitions, components, and subcomponents.
        /// Automatically creates missing segments, fields, components, and subcomponents.
        /// Uses the existing fluent API mutators for setting values.
        /// </summary>
        /// <param name="message">The HL7 message to write to</param>
        /// <param name="path">The path to write (e.g., "DG1[2].3[1].4")</param>
        /// <param name="value">The value to set</param>
        /// <returns>True if the value was set successfully</returns>
        public static bool SetValue(Message message, string path, string value)
        {
            if (message == null || string.IsNullOrEmpty(path))
                return false;
                
            var parsed = Parse(path);
            if (parsed == null)
                return false; // Invalid path format
                
            try
            {
                var fluentMessage = new FluentMessage(message);
                
                // Ensure segment exists and navigate to it using fluent API
                var segmentAccessor = EnsureSegment(fluentMessage, parsed.SegmentName, parsed.SegmentRepetition, message);
                
                // If only segment level requested (unusual but possible)
                if (parsed.IsSegmentLevel)
                {
                    // Segment level setting is not typically done, but we'll return true if segment exists
                    return segmentAccessor.Exists;
                }
                    
                // Create FieldMutator directly with proper segment instance
                var segmentInstanceIndex = parsed.SegmentRepetition - 1; // Convert to 0-based
                FieldMutator fieldMutator;
                
                if (parsed.FieldRepetition > 1)
                {
                    // For field repetitions > 1, use direct field manipulation to avoid endless loop
                    // Get the specific segment instance
                    var segment = GetSegmentInstance(message, parsed.SegmentName, parsed.SegmentRepetition - 1);
                    if (segment == null)
                    {
                        // This shouldn't happen as EnsureSegment was called, but handle gracefully
                        return false;
                    }
                    
                    // Directly ensure the field and repetitions exist using Segment.EnsureField
                    // This method properly handles repetitions on the correct segment instance
                    var field = segment.EnsureField(parsed.FieldNumber.Value, parsed.FieldRepetition);
                    
                    // Create FieldMutator for the specific repetition
                    fieldMutator = new FieldMutator(message, parsed.SegmentName, parsed.FieldNumber.Value, 
                        parsed.FieldRepetition, segmentInstanceIndex);
                }
                else
                {
                    // Create FieldMutator for the first repetition or non-repeated field
                    fieldMutator = new FieldMutator(message, parsed.SegmentName, parsed.FieldNumber.Value, 
                        null, segmentInstanceIndex);
                }
                    
                // If only field level requested
                if (parsed.IsFieldLevel)
                {
                    fieldMutator.Set(value);
                    return true;
                }
                    
                // Create ComponentMutator directly with proper parameters
                var componentMutator = new ComponentMutator(message, parsed.SegmentName, 
                    parsed.FieldNumber.Value, parsed.ComponentNumber.Value, parsed.FieldRepetition);
                    
                // If only component level requested
                if (parsed.IsComponentLevel)
                {
                    // Use direct field manipulation on the specific segment instance
                    return SetFieldComponentValue(message, parsed, value);
                }
                    
                // Navigate to subcomponent
                return SetFieldSubComponentValue(message, parsed, value);
            }
            catch
            {
                // Any navigation error returns false
                return false;
            }
        }
        
        
        /// <summary>
        /// Ensures that a segment exists in the message, creating it if necessary.
        /// Supports segment repetitions by creating multiple instances.
        /// Uses the fluent API for segment creation.
        /// </summary>
        /// <param name="fluentMessage">The fluent message to work with</param>
        /// <param name="segmentName">The name of the segment to ensure exists</param>
        /// <param name="repetition">The 1-based repetition index</param>
        /// <param name="message">The underlying message for direct access</param>
        /// <returns>SegmentAccessor for the specified segment repetition</returns>
        private static Accessors.SegmentAccessor EnsureSegment(FluentMessage fluentMessage, string segmentName, int repetition, Message message)
        {
            var segmentCollection = fluentMessage.Segments(segmentName);
            var initialCount = segmentCollection.Count;
            
            // Ensure we have enough segment repetitions
            while (segmentCollection.Count < repetition)
            {
                var segmentAccessor = segmentCollection.Add();
                
                // For newly created segments, ensure they have at least field 1
                // This matches HL7 convention where most segments have basic structure
                if (segmentCollection.Count > initialCount)
                {
                    var segment = GetSegmentInstance(message, segmentName, segmentCollection.Count - 1);
                    if (segment != null)
                    {
                        segment.EnsureField(1, 1); // Ensure field 1 exists
                    }
                }
            }
            
            // Return the specific repetition (using 1-based indexing)
            return segmentCollection.Segment(repetition);
        }
        
        /// <summary>
        /// Gets a specific segment instance by name and 0-based index.
        /// </summary>
        /// <param name="message">The message to search</param>
        /// <param name="segmentName">The segment name</param>
        /// <param name="instanceIndex">The 0-based instance index</param>
        /// <returns>The segment instance or null if not found</returns>
        private static Segment GetSegmentInstance(Message message, string segmentName, int instanceIndex)
        {
            if (!message.SegmentList.ContainsKey(segmentName))
                return null;
                
            var segments = message.SegmentList[segmentName];
            if (instanceIndex < 0 || instanceIndex >= segments.Count)
                return null;
                
            return segments[instanceIndex];
        }
        
        /// <summary>
        /// Sets a component value on a specific segment instance.
        /// </summary>
        private static bool SetFieldComponentValue(Message message, ParsedPath parsed, string value)
        {
            var segment = GetSegmentInstance(message, parsed.SegmentName, parsed.SegmentRepetition - 1);
            if (segment == null)
                return false;

            try
            {
                // Use ComponentMutator to ensure proper encoding
                var segmentInstanceIndex = parsed.SegmentRepetition - 1;
                var componentMutator = new ComponentMutator(message, parsed.SegmentName, 
                    parsed.FieldNumber.Value, parsed.ComponentNumber.Value, parsed.FieldRepetition, segmentInstanceIndex);
                componentMutator.Set(value);
                
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Sets a subcomponent value on a specific segment instance.
        /// </summary>
        private static bool SetFieldSubComponentValue(Message message, ParsedPath parsed, string value)
        {
            var segment = GetSegmentInstance(message, parsed.SegmentName, parsed.SegmentRepetition - 1);
            if (segment == null)
                return false;

            try
            {
                // Use SubComponentMutator to ensure proper encoding
                var segmentInstanceIndex = parsed.SegmentRepetition - 1;
                var subComponentMutator = new SubComponentMutator(message, parsed.SegmentName, 
                    parsed.FieldNumber.Value, parsed.ComponentNumber.Value, parsed.SubComponentNumber.Value, 
                    parsed.FieldRepetition, segmentInstanceIndex);
                subComponentMutator.Set(value);
                
                return true;
            }
            catch
            {
                return false;
            }
        }
        
    }
}