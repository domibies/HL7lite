# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build --configuration Release

# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Run a specific test
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"

# Pack NuGet package
dotnet pack src/HL7lite.csproj --configuration Release

# Generate coverage report locally
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
reportgenerator -reports:coverage/**/coverage.cobertura.xml -targetdir:coverage/report -reporttypes:"Cobertura;HtmlSummary;Badges"
```

## Architecture Overview

HL7lite is a lightweight HL7 2.x message parser following a hierarchical structure:

```
Message
├── Segment (MSH, PID, OBR, etc.)
│   ├── Field
│   │   ├── Component
│   │   │   └── SubComponent
│   │   └── Repetitions (for repeated fields)
│   └── Repetitions (for repeated segments)
```

### Core Class Hierarchy

- **MessageElement** (abstract base) - Common functionality for all elements
  - **Message** - Complete HL7 message with segments
  - **Segment** - Named segment containing fields
  - **Field** - Can contain components or be simple value
  - **Component** - Can contain subcomponents or be simple value
  - **SubComponent** - Leaf node, always simple value

### Key Design Decisions

1. **Path Notation**: Elements accessed via paths like `"PID.5.1"` (1-based indexing)
2. **MSH Special Handling**: MSH segment has extra field at position 1 for field separator
3. **Lazy Parsing**: Components/subcomponents parsed on demand
4. **Auto-creation**: `PutValue()` creates missing elements, `SetValue()` doesn't
5. **Encoding**: Special characters handled by `HL7Encoding` class

### Collections

All collections inherit from `ElementCollection<T>` providing consistent behavior:
- Add/Insert/Remove operations
- Access by index (0-based internally, 1-based in paths)
- Support for repetitions

## Testing Approach

- **Framework**: xUnit with .NET 8.0
- **Test Data**: Sample HL7 messages in `HL7lite.Test/Sample-*.txt`
- **Coverage**: Coverlet for code coverage, reports uploaded to Codecov
- **CI/CD**: GitHub Actions runs tests on multiple .NET versions (6.0, 7.0, 8.0)

### Test Organization and Naming Conventions

1. **Test File Structure**:
   - One test file per production class: `{ClassName}Tests.cs`
   - Group related tests in the same test class
   - Place test files in `HL7lite.Test/` directory

2. **Test Naming Conventions**:
   - Test methods: `MethodName_StateUnderTest_ExpectedBehavior()`
   - Example: `Constructor_WithMessageAndCode_ShouldSetBothProperties()`
   - Use descriptive names that explain what is being tested

3. **Test Structure (AAA Pattern)**:
   ```csharp
   [Fact]
   public void MethodName_StateUnderTest_ExpectedBehavior()
   {
       // Arrange - Set up test data and dependencies
       const string expected = "value";
       
       // Act - Execute the method being tested
       var result = target.Method();
       
       // Assert - Verify the expected outcome
       Assert.Equal(expected, result);
   }
   ```

4. **Test Categories**:
   - Use `[Fact]` for single test cases
   - Use `[Theory]` with `[InlineData]` for parameterized tests
   - Group related tests within the same test class

5. **Best Practices**:
   - Keep tests independent and isolated
   - Test one thing per test method
   - Use meaningful constant names for test data
   - Include edge cases and error scenarios
   - Aim for high code coverage but focus on meaningful tests

## Important Patterns

1. **Message Manipulation**:
   ```csharp
   message.GetValue("PID.5.1");  // Read
   message.SetValue("PID.5.1", "value");  // Update existing
   message.PutValue("PID.5.1", "value");  // Create if needed
   ```

2. **Segment Access**:
   ```csharp
   message.DefaultSegment("PID");  // First occurrence
   message.Segments("PID")[1];     // Second occurrence (0-based)
   ```

3. **Creating Messages**:
   ```csharp
   var message = new Message();
   message.AddSegmentMSH(...);  // or AddSegmentMSH() for minimal
   ```

## NuGet Publishing

Publishing is manual via GitHub Actions:
1. Go to Actions → .NET workflow
2. Run workflow with "Publish to NuGet" checked
3. Requires `NUGET_API_KEY` secret

## Version Support

- **Library**: Targets .NET Standard 1.3, 1.6, 2.0 for broad compatibility
- **Tests**: .NET 8.0
- **Strong Named**: Assembly signed with HL7lite.snk