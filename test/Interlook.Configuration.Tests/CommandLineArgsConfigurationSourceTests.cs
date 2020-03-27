using FluentAssertions;
using Interlook.Configuration.CommandLine;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Xunit;

namespace Interlook.Configuration.Tests
{
    public class CommandLineArgsConfigurationSourceTests
    {
        [Fact]
        public void Constructor_KeyCollisionsThrow_Test()
        {
            this.Invoking(_ => new CommandLineArgsConfigurationSource(CommandLineArgsTestDefinitions.InvalidDefinitions, new string[0]))
                .Should().ThrowExactly<System.ArgumentException>("Argument definitions with ambiquous key name mappings are forbidden.")
                .And.ParamName.Should().Be("argumentDefinitions", "Since invalid argument definitions were provided that parameter was to be declared invalid.");
        }
    }
}