# HL7lite Fluent API Implementation Plan

## Overview

We're adding a modern, fluent API to the existing HL7lite library that provides safer, more intuitive access to HL7 messages. The fluent API will live alongside the existing API in the same package, allowing gradual migration without breaking changes.

## Architecture Principles

1. **Wrapper Pattern**: The fluent API wraps the existing Message/Segment/Field classes
2. **Null Safety**: Never throw NullReferenceException or HL7Exception for missing elements
3. **Immutable Accessors**: Reading data doesn't change state
4. **Explicit Mutation**: Changing data requires calling `.Set()` first
5. **Lazy Evaluation**: Don't parse/process until actually needed

## Project Structure

```
HL7lite/
├── src/
│   ├── [existing files unchanged]
│   ├── Fluent/
│   │   ├── FluentMessage.cs
│   │   ├── Accessors/
│   │   │   ├── SegmentAccessor.cs
│   │   │   ├── FieldAccessor.cs
│   │   │   ├── ComponentAccessor.cs
│   │   │   └── SubComponentAccessor.cs
│   │   ├── Mutators/
│   │   │   ├── FieldMutator.cs
│   │   │   └── ComponentMutator.cs
│   │   ├── Collections/
│   │   │   ├── SegmentCollection.cs
│   │   │   └── FieldRepetitionCollection.cs
│   │   └── Extensions/
│   │       └── FluentExtensions.cs
│   └── Message.Fluent.cs         # Partial class adding Fluent property
```

## Core Classes Design

### 1. FluentMessage
- Wraps existing Message instance
- Provides segment access via indexer: `fluent["PID"]`
- Common segments as properties: `fluent.PID`, `fluent.MSH`, etc.
- Caches accessors for performance
- Delegates actual data operations to wrapped Message

### 2. SegmentAccessor
- Represents access to a segment (which may or may not exist)
- Properties:
  - `Exists`: bool - does this segment exist?
  - `HasMultiple`: bool - are there multiple instances?
  - `Count`: int - total number of instances
  - `IsSingle`: bool - exactly one instance exists
  - `All`: SegmentCollection - access to all instances
  - `Instance(n)`: access specific repetition of segment
  - `[n]` or `Field(n)`: access fields (1-based indexing)
- Common fields as properties for specific segments (e.g., `PID.PatientName`)
- Returns empty accessors for non-existent segments

### 3. FieldAccessor
- Represents access to a field (defaults to first repetition if field has repetitions)
- Properties:
  - `Value`: string - decoded value (null for explicit HL7 nulls, "" for non-existent)
  - `Exists`: field exists in message
  - `IsNull`: field is explicitly null (`""` in HL7)
  - `IsEmpty`: field exists but is empty string
  - `HasValue`: field exists and has actual data
  - `HasRepetitions`: bool - field has multiple repetitions
  - `RepetitionCount`: int - number of repetitions (1 if no repetitions)
  - `Repetitions`: FieldRepetitionCollection - collection of all repetitions
- Methods:
  - `[n]` or `Component(n)`: access components (of first repetition)
  - `Repetition(n)`: access specific repetition by index (1-based)
  - `AsDateTime()`, `AsInt()`, etc.: type conversions
  - `Set()`: returns mutator for changing value

### 4. ComponentAccessor
- Similar to FieldAccessor but for components
- Supports SubComponent access
- Same null-safety principles

### 5. Mutators
- Separate classes for modifying data
- Returned by calling `.Set()` on accessors
- Methods return self for chaining
- Key methods:
  - `Value(string)`: set the value
  - `Null()`: set explicit HL7 null
  - `Clear()`: remove the field/component entirely
  - `Components(params string[])`: set multiple components
  - `ValueIf(string, bool)`: conditional setting
  - `NullIf(bool)`: conditional null setting

### 6. Collections
- SegmentCollection: for multiple segments of same type
- FieldRepetitionCollection: for repeating fields within a single field
- Both implement IEnumerable for LINQ support
- FieldRepetitionCollection provides indexed access and LINQ operations

## Handling Multiple Segment Instances

When accessing segments that may have multiple instances:

### Default Behavior
- `message.Fluent.DG1` returns accessor for **FIRST** instance (matching existing `DefaultSegment` behavior)
- This ensures backward compatibility and predictable behavior

### Detection and Access
1. **Properties to detect multiple instances:**
   - `HasMultiple`: true if more than one instance exists
   - `Count`: total number of instances
   - `IsSingle`: true if exactly one instance exists
   - `All`: returns SegmentCollection with all instances

2. **Access patterns:**
   - Single: `message.Fluent.DG1[3].Value` (implicitly first instance)
   - Specific: `message.Fluent.DG1.Instance(n)[3].Value` (explicit instance)
   - All: `message.Fluent.DG1.All` or `message.Fluent.Segments("DG1")`

3. **Example usage:**
```csharp
// Check before processing
if (message.Fluent.DG1.HasMultiple)
{
    foreach (var dg1 in message.Fluent.DG1.All)
    {
        ProcessDiagnosis(dg1[3].Value);
    }
}
else if (message.Fluent.DG1.Exists)
{
    ProcessDiagnosis(message.Fluent.DG1[3].Value);
}
```

## API Examples

### Reading Data
```csharp
// Access patterns
message.Fluent.PID[5][1].Value           // Last name
message.Fluent["ZZ1"][3].Value           // Custom segment
message.Fluent.PID.PatientName[1].Value  // Using property shortcut

// Null safety
var value = message.Fluent.PID[99].Value;  // Returns "" not exception

// Type conversions
var dob = message.Fluent.PID[7].AsDateTime();
var age = message.Fluent.PID[8].AsInt(defaultValue: 0);

// Checking state
if (message.Fluent.PID[10].IsNull) { }     // Explicit null
if (message.Fluent.PID[10].HasValue) { }   // Has data
if (message.Fluent.PID[10].Exists) { }     // Field exists

// Repeating fields
var firstId = message.Fluent.PID[3].Value;  // First repetition (default)
var secondId = message.Fluent.PID[3].Repetition(2).Value;  // Second repetition
var allIds = message.Fluent.PID[3].Repetitions.Select(r => r.Value);  // All repetitions
var idCount = message.Fluent.PID[3].RepetitionCount;  // How many repetitions
```

### Writing Data
```csharp
// Simple set
message.Fluent.PID[5][1].Set("SMITH");

// Multiple components
message.Fluent.PID[5].Set().Components("SMITH", "JOHN", "M");

// Explicit null
message.Fluent.PID[10].SetNull();

// Clear field entirely
message.Fluent.PID[10].Clear();

// Conditional
message.Fluent.PV1[2].Set()
    .ValueIf("E", isEmergency)
    .ValueIf("I", !isEmergency);

// Add repetition
message.Fluent.PID[3].Repetitions.Add("ID001");
message.Fluent.PID[3].Repetitions.Add("ID002");

// Or using mutator
message.Fluent.PID[3].Set()
    .AddRepetition("ID001")
    .AddRepetition("ID002");
```

### Collections
```csharp
// Multiple segments
foreach (var dg1 in message.Fluent.Segments("DG1"))
{
    var diagnosis = dg1[3].Value;
}

// LINQ support
var activeMeds = message.Fluent.Segments("RXA")
    .Where(rxa => rxa[20].Value == "A")
    .Select(rxa => rxa[5].Value)
    .ToList();

// Add segment
message.Fluent.Segments("DG1").Add()
    .Field(1, "1")
    .Field(3, "250.00");

// Check for multiple
if (message.Fluent.IN1.HasMultiple)
{
    var primary = message.Fluent.IN1.Instance(0);
    var secondary = message.Fluent.IN1.Instance(1);
}
```

### Null Handling
```csharp
// Three-state checking
var field = message.Fluent.PID[10];
if (!field.Exists)
    Console.WriteLine("Field not present");
else if (field.IsNull)
    Console.WriteLine("Field explicitly null");
else if (field.IsEmpty)
    Console.WriteLine("Field empty");
else
    Console.WriteLine($"Field value: {field.Value}");

// Map state to different values
var display = field.MapState(
    whenNotPresent: "Not provided",
    whenNull: "Declined to specify",
    whenEmpty: "Pending",
    whenValue: v => $"Race: {v}"
);
```

## Phase 7: Pure Navigation API Redesign

### Problem Statement

The current fluent API has several issues that reduce its intuitiveness:

1. **Mixed Responsibilities**: Methods like `Component(int componentIndex, string value)` mix navigation with setting operations
2. **Ambiguous Signatures**: Multi-parameter methods create confusion about which parameter represents what
3. **Inconsistent Patterns**: Different mutator types have different cross-navigation capabilities
4. **Context Confusion**: Unclear where you end up after calling cross-setting methods

### Current Problematic Patterns

```csharp
// Confusing: what does Component(11, 3, "Springfield") mean?
fluent.PID[5].Set()
    .Components("Johnson", "Mary", "Elizabeth")
    .Field(11, "123 Main St")           // Returns FieldMutator
    .Component(11, 3, "Springfield");   // ❌ FieldMutator has no Component() method

// Ambiguous: is 11 a field index or component index?
.Component(11, 3, "Springfield")  // Field 11, Component 3? Component 11, ??? 3?
```

### Solution: Pure Navigation Pattern

**Design Principle**: Completely separate navigation from setting operations.

- **Navigation methods**: Only move to a new location, return appropriate mutator type
- **Setting methods**: Only modify current location, return same mutator type for chaining
- **Consistent API**: Every mutator type has the same navigation methods

### Technical Specification

#### FieldMutator
```csharp
public class FieldMutator
{
    // SETTING METHODS (return FieldMutator for chaining)
    public FieldMutator Value(string value)
    public FieldMutator Components(params string[] components)
    public FieldMutator Date(DateTime date)
    public FieldMutator DateTime(DateTime dateTime)
    public FieldMutator DateToday()
    public FieldMutator DateTimeNow()
    public FieldMutator Null()
    public FieldMutator Clear()
    public FieldMutator ValueIf(string value, bool condition)
    public FieldMutator EncodedValue(string value)
    
    // PURE NAVIGATION METHODS (return target mutator type)
    public FieldMutator Field(int fieldIndex)                           // Navigate to other field in same segment
    public ComponentMutator Component(int componentIndex)               // Navigate to component in current field
    public SubComponentMutator SubComponent(int componentIndex, int subIndex) // Navigate to subcomponent in current field
}
```

#### ComponentMutator
```csharp
public class ComponentMutator
{
    // SETTING METHODS (return ComponentMutator for chaining)
    public ComponentMutator Value(string value)
    public ComponentMutator SubComponents(params string[] subComponents)
    public ComponentMutator Null()
    public ComponentMutator Clear()
    public ComponentMutator ValueIf(string value, bool condition)
    public ComponentMutator EncodedValue(string value)
    
    // PURE NAVIGATION METHODS (return target mutator type)
    public FieldMutator Field(int fieldIndex)                          // Navigate to field in same segment
    public ComponentMutator Component(int componentIndex)              // Navigate to other component in same field
    public SubComponentMutator SubComponent(int subIndex)              // Navigate to subcomponent in current component
}
```

#### SubComponentMutator
```csharp
public class SubComponentMutator
{
    // SETTING METHODS (return SubComponentMutator for chaining)
    public SubComponentMutator Value(string value)
    public SubComponentMutator Null()
    public SubComponentMutator Clear()
    public SubComponentMutator ValueIf(string value, bool condition)
    public SubComponentMutator EncodedValue(string value)
    
    // PURE NAVIGATION METHODS (return target mutator type)
    public FieldMutator Field(int fieldIndex)                          // Navigate to field in same segment
    public ComponentMutator Component(int componentIndex)              // Navigate to component in same field
    public SubComponentMutator SubComponent(int subIndex)              // Navigate to other subcomponent in same component
}
```

### Expected Usage Patterns

#### Clear Navigation Flow
```csharp
fluent.PID.Set()
    .Field(5).Components("Johnson", "Mary", "Elizabeth")        // Navigate to field 5, set components
    .Field(7).Date(new DateTime(1990, 12, 25))                 // Navigate to field 7, set date
    .Field(8).Value("F")                                       // Navigate to field 8, set value
    .Field(11).Component(1).Value("123 Main St")               // Navigate to field 11, component 1, set value
           .Component(3).Value("Springfield")                   // Navigate to component 3 (same field), set value
           .Component(4).Value("IL")                           // Navigate to component 4 (same field), set value
           .Component(5).Value("62701")                        // Navigate to component 5 (same field), set value
    .Field(13).Component(1).SubComponent(1).Value("555")       // Deep navigation: field 13, component 1, subcomponent 1
              .SubComponent(2).Value("1234");                  // Navigate to subcomponent 2 (same component), set value
```

#### Natural Language Reading
The API reads like instructions:
- "Go to field 11, then component 1, set value '123 Main St'"
- "Go to component 3, set value 'Springfield'"
- "Go to field 13, then component 1, then subcomponent 1, set value '555'"

### Breaking Changes Required

#### Methods to Remove
1. **FieldMutator**:
   - Remove: `Field(int fieldIndex, string value)` 
   - Replace with: `Field(int fieldIndex)` (navigation) + `.Value(string value)` (setting)

2. **ComponentMutator**:
   - Remove: `Component(int componentIndex, string value)`
   - Remove: `Field(int fieldIndex, string value)`
   - Replace with pure navigation methods

3. **SubComponentMutator**:
   - Remove: `Component(int componentIndex, string value)`
   - Remove: `Field(int fieldIndex, string value)`
   - Remove: `SubComponent(int subIndex, string value)`
   - Replace with pure navigation methods

### Implementation Steps

#### Step 1: Update FieldMutator
- [ ] Remove `Field(int, string)` method
- [ ] Add pure `Field(int)` navigation method  
- [ ] Add `Component(int)` navigation method
- [ ] Add `SubComponent(int, int)` navigation method
- [ ] Update all tests using old cross-setting pattern

#### Step 2: Update ComponentMutator  
- [ ] Remove `Component(int, string)` method
- [ ] Remove `Field(int, string)` method
- [ ] Add pure `Field(int)` navigation method
- [ ] Add pure `Component(int)` navigation method  
- [ ] Add `SubComponent(int)` navigation method
- [ ] Update all tests using old cross-setting pattern

#### Step 3: Update SubComponentMutator
- [ ] Remove `Component(int, string)` method
- [ ] Remove `Field(int, string)` method
- [ ] Remove `SubComponent(int, string)` method
- [ ] Add pure `Field(int)` navigation method
- [ ] Add pure `Component(int)` navigation method
- [ ] Add pure `SubComponent(int)` navigation method
- [ ] Update all tests using old cross-setting pattern

#### Step 4: Update Documentation
- [ ] Fix all broken examples in README.md
- [ ] Update API reference with new method signatures
- [ ] Add comprehensive navigation examples
- [ ] Document migration patterns from old API

#### Step 5: Test Coverage
- [ ] Test all navigation combinations
- [ ] Verify context maintenance across navigation
- [ ] Test complex chaining scenarios
- [ ] Ensure all mutator types return correct types

### Benefits of Pure Navigation

1. **Crystal Clear Intent**: `Field(11).Component(1)` is completely unambiguous
2. **Consistent Pattern**: Same navigation methods on every mutator type  
3. **Natural Reading**: Code reads like step-by-step instructions
4. **No Parameter Confusion**: Single-parameter methods eliminate ambiguity
5. **Full Navigation Matrix**: Can navigate anywhere from anywhere
6. **Type Safety**: Return types clearly indicate current context

### Migration Strategy

Since the fluent API hasn't been released yet, we can make breaking changes without backward compatibility concerns:

1. **Update all existing tests** to use new navigation pattern
2. **Fix README examples** to demonstrate pure navigation
3. **Validate API design** with comprehensive test scenarios
4. **Document new patterns** for future developers

This redesign will result in a much more intuitive and consistent fluent API that truly reads like natural language.

## TDD Approach

### 1. Test Organization

Create test files mirroring the source structure:
```
tests/
├── Unit/
│   ├── Fluent/
│   │   ├── FluentMessageTests.cs
│   │   ├── Accessors/
│   │   │   ├── SegmentAccessorTests.cs
│   │   │   ├── FieldAccessorTests.cs
│   │   │   └── ComponentAccessorTests.cs
│   │   ├── Mutators/
│   │   │   └── FieldMutatorTests.cs
│   │   └── Collections/
│   │       └── SegmentCollectionTests.cs
│   └── Integration/
│       ├── FluentApiIntegrationTests.cs
│       └── BackwardCompatibilityTests.cs
```

### 2. Test-First Development Process

For each class:

1. **Write Interface Tests First**
   - Define expected behavior through tests
   - Start with simplest cases
   - Add edge cases
   - Add error cases

2. **Implementation Phases**
   - Phase 1: Make it work (pass the tests)
   - Phase 2: Make it right (refactor for clarity)
   - Phase 3: Make it fast (optimize if needed)

3. **Test Categories**

   **Basic Functionality Tests**
   - Happy path scenarios
   - Basic read/write operations
   - Expected return values

   **Edge Case Tests**
   - Null/empty inputs
   - Invalid indices (negative, too large)
   - Non-existent segments/fields
   - Empty messages

   **State Tests**
   - Distinguish between null/empty/missing
   - Verify IsNull, IsEmpty, HasValue, Exists

   **Multiple Instance Tests**
   - Single accessor with multiple segments
   - HasMultiple, Count, IsSingle properties
   - All property returns correct collection

   **Integration Tests**
   - Fluent + Traditional API interaction
   - Changes via fluent reflect in base Message
   - Complex real-world scenarios

### 3. Test Data Strategy

Create builders for test data:
- `HL7MessageBuilder` - Fluent builder for creating test messages
- `TestMessages` - Static class with common test scenarios
- Use constants for repeated test values

### 4. Testing Patterns

**AAA Pattern**
- Arrange: Set up test data and expectations
- Act: Execute the operation
- Assert: Verify the result

**Test Naming**
- Method_Scenario_ExpectedBehavior
- Example: `Value_WhenFieldDoesNotExist_ReturnsEmptyString`

**Parameterized Tests**
- Use `[Theory]` for testing multiple inputs
- Use `[InlineData]` for simple parameters
- Use `[MemberData]` for complex scenarios

### 5. Key Test Scenarios

**FluentMessage Tests**
- Parse valid/invalid messages
- Access segments by name
- Cache behavior
- Integration with base Message

**SegmentAccessor Tests**
- Exists for valid/invalid segments
- Field access returns correct type
- Instance access for repeating segments
- Property shortcuts work correctly
- Multiple instance detection (HasMultiple, Count)
- All property returns collection

**FieldAccessor Tests**
- Value returns correct data
- Null handling (explicit nulls vs missing)
- Component access
- Type conversions with various inputs
- Repetition handling
- Mutator creation

**Mutator Tests**
- Value changes reflect in base message
- Null setting works correctly
- Component setting
- Clear functionality
- Conditional operations
- Chaining works properly
- AddRepetition functionality

**Collection Tests**
- Count is correct
- Enumeration works
- LINQ operations
- Add/Remove operations

**Integration Tests**
- Full message processing scenarios
- Migration scenarios (old + new API)
- Performance benchmarks
- Thread safety (if applicable)

## Implementation Order

### Phase 1: Core Infrastructure
- FluentMessage wrapper
- Basic SegmentAccessor (including multiple instance handling)
- Basic FieldAccessor
- Simple value reading

### Phase 2: Complete Accessors
- ComponentAccessor
- SubComponentAccessor
- FieldRepetitionCollection and field repetition support
- All state properties (IsNull, HasValue, etc.)
- Type conversions
- Multiple instance properties

### Phase 3: Mutations
- FieldMutator
- ComponentMutator
- All setting operations
- Repetition handling

### Phase 4: Advanced Collections
- SegmentCollection (for multiple segments of same type)
- LINQ support enhancements

### Phase 5: Polish
- Property shortcuts
- Extension methods
- Performance optimization
- Documentation

## Success Criteria

1. **Zero Breaking Changes**: All existing tests still pass
2. **100% Backward Compatible**: Old code works unchanged
3. **High Test Coverage**: Aim for >90% coverage on new code
4. **No Exceptions**: Fluent API never throws for missing data
5. **Intuitive API**: New developers can use without reading docs
6. **Performance**: No significant performance degradation
7. **Multiple Instance Handling**: Clear behavior for repeating segments

## Documentation Requirements

1. **XML Documentation**: Every public class/method
2. **Examples**: Show both old and new approach
3. **Migration Guide**: How to gradually adopt
4. **README Update**: Show fluent API prominently
5. **Common Patterns**: Document typical usage including:
   - Handling repeating segments
   - Null vs empty vs missing
   - Type conversions
   - LINQ usage

## Integration with Existing API

### Add to Message.cs (Partial Class)
```csharp
public partial class Message
{
    private FluentMessage _fluentWrapper;
    
    /// <summary>
    /// Gets a fluent API wrapper for this message.
    /// </summary>
    public FluentMessage Fluent
    {
        get
        {
            if (_fluentWrapper == null)
                _fluentWrapper = new FluentMessage(this);
            return _fluentWrapper;
        }
    }
}
```

### Usage Examples
```csharp
// Existing API continues to work
var message = new Message(hl7String);
message.ParseMessage();
var pid = message.GetValue("PID.3");

// New fluent API on same object
var pidFluent = message.Fluent.PID[3].Value;

// Or use extension method
using HL7lite.Fluent.Extensions;
var name = message.GetValueFluent(m => m.PID[5][1]);
```

## Version Strategy

- Release as version 1.3.0 (minor version bump)
- No breaking changes to existing API
- Fluent API is additive only
- Future enhancements can be added in 1.3.x releases

Remember: Start with tests, implement minimally to pass, then refactor for clarity. The goal is a safe, intuitive API that makes HL7 processing delightful while maintaining full backward compatibility.