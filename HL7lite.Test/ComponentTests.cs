using System;
using System.Linq;
using Xunit;

namespace HL7lite.Test
{
    public class ComponentTests
    {
        private readonly HL7Encoding _encoding = new HL7Encoding();

        // Helper method for common exception assertions
        private static void AssertThrowsHL7Exception(Action action, string expectedErrorCode = null)
        {
            var ex = Assert.Throws<HL7Exception>(action);
            if (expectedErrorCode != null)
                Assert.Equal(expectedErrorCode, ex.ErrorCode);
        }

        [Fact]
        public void Constructor_Default_InitializesCorrectly()
        {
            var component = new Component();
            
            Assert.False(component.IsSubComponentized);
            Assert.Empty(component.SubComponents());
        }

        [Fact]
        public void Constructor_WithEncoding_InitializesCorrectly()
        {
            var component = new Component(_encoding);
            
            Assert.False(component.IsSubComponentized);
            Assert.Same(_encoding, component.Encoding);
            Assert.Empty(component.SubComponents());
        }

        [Fact]
        public void Constructor_WithEncodingAndIsDelimiterTrue_InitializesCorrectly()
        {
            var component = new Component(_encoding, isDelimiter: true);
            
            Assert.False(component.IsSubComponentized);
            Assert.Same(_encoding, component.Encoding);
            Assert.Empty(component.SubComponents());
        }

        [Fact]
        public void Constructor_WithValueAndEncoding_InitializesCorrectly()
        {
            const string value = "TestValue";
            var component = new Component(value, _encoding);
            
            Assert.Same(_encoding, component.Encoding);
            Assert.Equal(value, component.Value);
        }

        [Fact]
        public void Constructor_WithValueAndEncoding_ParsesSimpleValue()
        {
            const string value = "SimpleValue";
            var component = new Component(value, _encoding);
            
            Assert.False(component.IsSubComponentized);
            Assert.Single(component.SubComponents());
            Assert.Equal(value, component.SubComponents().First().Value);
        }

        [Fact]
        public void Constructor_WithValueAndEncoding_ParsesSubComponentizedValue()
        {
            const string value = "FirstSub&SecondSub&ThirdSub";
            var component = new Component(value, _encoding);
            
            Assert.True(component.IsSubComponentized);
            Assert.Equal(3, component.SubComponents().Count);
            Assert.Equal("FirstSub", component.SubComponents()[0].Value);
            Assert.Equal("SecondSub", component.SubComponents()[1].Value);
            Assert.Equal("ThirdSub", component.SubComponents()[2].Value);
        }

        [Fact]
        public void ProcessValue_WithDelimiterFlag_DoesNotSplitValue()
        {
            var component = new Component(_encoding, isDelimiter: true);
            component.Value = "Value&With&Delimiters";
            
            Assert.False(component.IsSubComponentized);
            Assert.Single(component.SubComponents());
            Assert.Equal("Value&With&Delimiters", component.SubComponents().First().Value);
        }

        [Fact]
        public void ProcessValue_WithoutDelimiterFlag_SplitsValue()
        {
            var component = new Component(_encoding, isDelimiter: false);
            component.Value = "Value&With&Delimiters";
            
            Assert.True(component.IsSubComponentized);
            Assert.Equal(3, component.SubComponents().Count);
            Assert.Equal("Value", component.SubComponents()[0].Value);
            Assert.Equal("With", component.SubComponents()[1].Value);
            Assert.Equal("Delimiters", component.SubComponents()[2].Value);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("SingleValue")]
        public void ProcessValue_WithSingleValue_NotSubComponentized(string value)
        {
            var component = new Component(value, _encoding);
            
            Assert.False(component.IsSubComponentized);
            Assert.Single(component.SubComponents());
            Assert.Equal(value, component.SubComponents().First().Value);
        }

        [Fact]
        public void ProcessValue_WithEmptySubComponents_HandlesCorrectly()
        {
            var component = new Component("First&&Third", _encoding);
            
            Assert.True(component.IsSubComponentized);
            Assert.Equal(3, component.SubComponents().Count);
            Assert.Equal("First", component.SubComponents()[0].Value);
            Assert.Equal("", component.SubComponents()[1].Value);
            Assert.Equal("Third", component.SubComponents()[2].Value);
        }

        [Fact]
        public void ProcessValue_WithEncodedValues_DecodesCorrectly()
        {
            // Use escape sequences that should be decoded
            var component = new Component("Test\\F\\Value&Second\\S\\Value", _encoding);
            
            Assert.True(component.IsSubComponentized);
            Assert.Equal(2, component.SubComponents().Count);
            Assert.Equal("Test|Value", component.SubComponents()[0].Value);
            Assert.Equal("Second^Value", component.SubComponents()[1].Value);
        }

        [Fact]
        public void EnsureSubComponent_WithValidPosition_ReturnsExistingSubComponent()
        {
            var component = new Component("First&Second&Third", _encoding);
            
            var subComponent = component.EnsureSubComponent(2);
            
            Assert.Equal("Second", subComponent.Value);
            Assert.Equal(3, component.SubComponents().Count); // No new subcomponents added
        }

        [Fact]
        public void EnsureSubComponent_WithPositionBeyondCount_CreatesNewSubComponents()
        {
            var component = new Component("First&Second", _encoding);
            
            var subComponent = component.EnsureSubComponent(5);
            
            Assert.Equal("", subComponent.Value); // New subcomponent is empty
            Assert.Equal(5, component.SubComponents().Count);
            Assert.Equal("First", component.SubComponents()[0].Value);
            Assert.Equal("Second", component.SubComponents()[1].Value);
            Assert.Equal("", component.SubComponents()[2].Value);
            Assert.Equal("", component.SubComponents()[3].Value);
            Assert.Equal("", component.SubComponents()[4].Value);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public void EnsureSubComponent_WithInvalidPosition_ThrowsException(int position)
        {
            var component = new Component("Test", _encoding);
            
            AssertThrowsHL7Exception(() => component.EnsureSubComponent(position));
        }

        [Fact]
        public void AddNewSubComponent_WithValidPosition_AddsSuccessfully()
        {
            var component = new Component("First&Second", _encoding);
            var newSubComponent = new SubComponent("NewSub", _encoding);
            
            var result = component.AddNewSubComponent(newSubComponent, 2);
            
            Assert.True(result);
            Assert.Equal(2, component.SubComponents().Count); // Position 2 replaces "Second"
            Assert.Equal("First", component.SubComponents()[0].Value);
            Assert.Equal("NewSub", component.SubComponents()[1].Value);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-5)]
        public void AddNewSubComponent_WithInvalidPosition_ThrowsException(int position)
        {
            var component = new Component("Test", _encoding);
            var newSubComponent = new SubComponent("New", _encoding);
            
            AssertThrowsHL7Exception(() => component.AddNewSubComponent(newSubComponent, position));
        }

        [Fact]
        public void SubComponents_WithValidPosition_ReturnsCorrectSubComponent()
        {
            var component = new Component("First&Second&Third", _encoding);
            
            var subComponent = component.SubComponents(2);
            
            Assert.Equal("Second", subComponent.Value);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-5)]
        public void SubComponents_WithInvalidPosition_ThrowsException(int position)
        {
            var component = new Component("First&Second&Third", _encoding);
            
            AssertThrowsHL7Exception(() => component.SubComponents(position));
        }

        [Theory]
        [InlineData(4)]
        [InlineData(10)]
        public void SubComponents_WithPositionBeyondRange_ReturnsNull(int position)
        {
            var component = new Component("First&Second&Third", _encoding);
            
            var result = component.SubComponents(position);
            
            Assert.Null(result);
        }

        [Fact]
        public void SubComponents_WithoutParameters_ReturnsAllSubComponents()
        {
            var component = new Component("First&Second&Third", _encoding);
            
            var allSubComponents = component.SubComponents();
            
            Assert.Equal(3, allSubComponents.Count);
            Assert.Equal("First", allSubComponents[0].Value);
            Assert.Equal("Second", allSubComponents[1].Value);
            Assert.Equal("Third", allSubComponents[2].Value);
        }

        [Fact]
        public void SerializeValue_WithSingleSubComponent_ReturnsValue()
        {
            var component = new Component("SingleValue", _encoding);
            
            var result = component.SerializeValue();
            
            Assert.Equal("SingleValue", result);
        }

        [Fact]
        public void SerializeValue_WithMultipleSubComponents_JoinsWithDelimiter()
        {
            var component = new Component("First&Second&Third", _encoding);
            
            var result = component.SerializeValue();
            
            Assert.Equal("First&Second&Third", result);
        }

        [Fact]
        public void SerializeValue_WithEmptySubComponents_HandlesCorrectly()
        {
            var component = new Component("First&&Third", _encoding);
            
            var result = component.SerializeValue();
            
            Assert.Equal("First&&Third", result);
        }

        [Fact]
        public void SerializeValue_WithCustomDelimiter_UsesCorrectDelimiter()
        {
            var customEncoding = new HL7Encoding { SubComponentDelimiter = '$' };
            var component = new Component("First$Second$Third", customEncoding);
            
            var result = component.SerializeValue();
            
            Assert.Equal("First$Second$Third", result);
        }

        [Fact]
        public void SerializeValue_WithSpecialCharacters_EncodesCorrectly()
        {
            // Create component with special characters that need encoding
            var component = new Component("Test|Value&Second^Value", _encoding);
            
            var result = component.SerializeValue();
            
            Assert.Equal("Test\\F\\Value&Second\\S\\Value", result);
        }

        [Fact]
        public void RemoveTrailingDelimiters_WithSubComponentOption_RemovesEmptyTrailingSubComponents()
        {
            var component = new Component("First&Second&&&", _encoding);
            Assert.Equal(5, component.SubComponents().Count);
            Assert.True(component.IsSubComponentized);
            
            component.RemoveTrailingDelimiters(new MessageElement.RemoveDelimitersOptions { SubComponent = true });
            
            Assert.Equal(2, component.SubComponents().Count);
            Assert.Equal("First", component.SubComponents()[0].Value);
            Assert.Equal("Second", component.SubComponents()[1].Value);
            Assert.True(component.IsSubComponentized); // Still has multiple subcomponents
        }

        [Fact]
        public void RemoveTrailingDelimiters_WithSingleNonEmptySubComponent_SetsNotSubComponentized()
        {
            var component = new Component("Value&&&", _encoding);
            Assert.Equal(4, component.SubComponents().Count);
            Assert.True(component.IsSubComponentized);
            
            component.RemoveTrailingDelimiters(new MessageElement.RemoveDelimitersOptions { SubComponent = true });
            
            Assert.Single(component.SubComponents());
            Assert.Equal("Value", component.SubComponents()[0].Value);
            Assert.False(component.IsSubComponentized); // Only one subcomponent left
        }

        [Fact]
        public void RemoveTrailingDelimiters_WithoutSubComponentOption_DoesNotRemove()
        {
            var component = new Component("First&Second&&&", _encoding);
            var originalCount = component.SubComponents().Count;
            
            component.RemoveTrailingDelimiters(new MessageElement.RemoveDelimitersOptions { SubComponent = false });
            
            Assert.Equal(originalCount, component.SubComponents().Count);
        }

        [Fact]
        public void RemoveTrailingDelimiters_CallsRemoveOnAllSubComponents()
        {
            var component = new Component("First&Second", _encoding);
            var options = new MessageElement.RemoveDelimitersOptions { SubComponent = true };
            
            // This test verifies the method doesn't throw and processes all subcomponents
            component.RemoveTrailingDelimiters(options);
            
            Assert.Equal(2, component.SubComponents().Count);
        }

        [Fact]
        public void Value_SetterWithSubComponentizedValue_ParsesCorrectly()
        {
            var component = new Component(_encoding);
            
            component.Value = "Alpha&Beta&Gamma";
            
            Assert.True(component.IsSubComponentized);
            Assert.Equal(3, component.SubComponents().Count);
            Assert.Equal("Alpha", component.SubComponents()[0].Value);
            Assert.Equal("Beta", component.SubComponents()[1].Value);
            Assert.Equal("Gamma", component.SubComponents()[2].Value);
        }

        [Fact]
        public void Value_SetterWithSimpleValue_CreatesOneSubComponent()
        {
            var component = new Component(_encoding);
            
            component.Value = "SimpleValue";
            
            Assert.False(component.IsSubComponentized);
            Assert.Single(component.SubComponents());
            Assert.Equal("SimpleValue", component.SubComponents()[0].Value);
        }

        [Fact]
        public void Component_ComplexScenario_WorksCorrectly()
        {
            // Start with a component, modify it, then serialize
            var component = new Component("Initial&Value", _encoding);
            
            // Add a new subcomponent at position 2 (replaces "Value")
            var newSub = new SubComponent("Added", _encoding);
            component.AddNewSubComponent(newSub, 2);
            
            // Ensure a subcomponent at position 5
            var ensuredSub = component.EnsureSubComponent(5);
            ensuredSub.Value = "Ensured";
            
            // Serialize
            var result = component.SerializeValue();
            
            Assert.Equal("Initial&Added&&&Ensured", result);
            Assert.Equal(5, component.SubComponents().Count);
            Assert.True(component.IsSubComponentized);
        }

        [Fact]
        public void Component_WithEmptyStringValue_HandlesCorrectly()
        {
            var component = new Component("", _encoding);
            
            Assert.False(component.IsSubComponentized);
            Assert.Single(component.SubComponents());
            Assert.Equal("", component.SubComponents()[0].Value);
        }

        [Fact]
        public void Component_RoundTripSerialization_PreservesData()
        {
            const string originalValue = "First&Second&Third&";
            var component1 = new Component(originalValue, _encoding);
            
            var serialized = component1.SerializeValue();
            var component2 = new Component(serialized, _encoding);
            
            Assert.Equal(component1.IsSubComponentized, component2.IsSubComponentized);
            Assert.Equal(component1.SubComponents().Count, component2.SubComponents().Count);
            
            for (int i = 0; i < component1.SubComponents().Count; i++)
            {
                Assert.Equal(component1.SubComponents()[i].Value, component2.SubComponents()[i].Value);
            }
        }
    }
}