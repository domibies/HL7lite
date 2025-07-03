# Core HL7lite Bug Fixes (v1.2.0 → v2.0.0)

This document details bug fixes and improvements made to the core HL7lite implementation (excluding Fluent API additions).

## 1. Field Repetition Data Loss Fix

**File**: `src/Field.cs`  
**Class**: `Field`  
**Method**: `RemoveRepetitions()`  
**Problem**: When converting from multiple repetitions back to a single field, the value from the first repetition was lost.  
**Fix**: Added code to preserve the first repetition's value:
```csharp
// After removing repetitions, copy value from first repetition
if (RepetitionList.Count > 0)
{
    _value = RepetitionList[0].Value;
}
```

## 2. Field Repetition State Preservation Fix

**File**: `src/Field.cs`  
**Class**: `Field`  
**Method**: `AddRepetition(string content)`  
**Problem**: When converting a single field to repetitions, field properties were not properly copied to the first repetition.  
**Fix**: Now copies all field properties to ensure state preservation:
```csharp
var field1 = new Field(this.Encoding);
field1.Value = _value;
field1.IsComponentized = this.IsComponentized;
field1.ComponentList = this.ComponentList;
field1.IsDelimiters = this.IsDelimiters;
this.RepetitionList.Add(field1);
```

## 3. Field Repetition Removal Method

**File**: `src/Field.cs`  
**Class**: `Field`  
**Method**: `RemoveRepetition(int index)` (NEW METHOD)  
**Addition**: Added new method to remove specific repetitions with proper validation and state transitions:
```csharp
public void RemoveRepetition(int index)
{
    if (index < 1)
        throw new ArgumentOutOfRangeException(nameof(index), "Repetition index must be 1-based (greater than 0)");
    
    // Handles conversion back to single field when only one repetition remains
    // Preserves the remaining repetition's value
}
```

## 4. Segment DeepCopy Serialization Fix

**File**: `src/Segment.cs`  
**Class**: `Segment`  
**Method**: `DeepCopy()`  
**Problem**: When copying segments that had fields added individually (not parsed from a string), only the segment name was copied, resulting in empty copied segments.  
**Fix**: Changed to use SerializeValue() to capture all field content:
```csharp
// Before:
newSegment.Value = this.Value; // Only contained segment name if fields were added individually

// After:
newSegment.Value = this.SerializeValue(); // Full "OBX|1|TX|GLUCOSE|..." with all fields
```

## 5. Segment Field Position Fix

**File**: `src/Segment.cs`  
**Class**: `Segment`  
**Method**: `AddNewField(Field field, int position)`  
**Problem**: Off-by-one error causing fields to be placed at wrong positions.  
**Fix**: Removed incorrect position adjustment:
```csharp
// Removed line:
position = position - 1; // This was causing incorrect positioning
```

## 6. Collection Position Validation

**File**: `src/ElementCollection.cs`  
**Class**: `ElementCollection<T>`  
**Method**: `Add(T item, int position)`  
**Problem**: No validation for position parameter, allowing invalid positions.  
**Fix**: Added validation to ensure position is valid:
```csharp
if (position < 1)
    throw new HL7Exception("Element position must be greater than 0 (1-based)", ErrorCode.REQUIRED_FIELD_MISSING);
```

## 7. Field Parameterless Constructor

**File**: `src/Field.cs`  
**Class**: `Field`  
**Addition**: Added parameterless constructor for easier instantiation:
```csharp
public Field() : this(new HL7Encoding())
{
}
```

## 8. Collection Architecture Improvement

**Files**: `src/FieldCollection.cs` (REMOVED), `src/ComponentCollection.cs` (REMOVED)  
**Change**: Replaced specialized collection classes with generic `ElementCollection<T>`  
**Impact**: 
- `Segment.FieldList` changed from `FieldCollection` to `ElementCollection<Field>`
- Unified collection behavior across all element types
- No public API changes (internal implementation detail)

## Summary

All fixes maintain backward compatibility with no breaking changes to the public API. The fixes primarily address:
- Data loss scenarios in field repetition handling
- Incorrect positioning logic
- Missing validation
- Serialization issues with programmatically created segments
- Internal architecture improvements for maintainability