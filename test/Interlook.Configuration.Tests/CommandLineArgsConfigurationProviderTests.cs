using FluentAssertions;
using Interlook.Configuration.CommandLine;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Xunit;

namespace Interlook.Configuration.Tests
{
    public class CommandLineArgsConfigurationProviderTests
    {
        private const string ValueTestuser = "testusr";
        private const string ValueNewuser = "newUser";
        private const string ValueLocalMaria = "local_maria";
        private const string KeyDatabase = "database";

        private const string VerbUpload = "upload";
        private const string ValueTargetRemote = "remote.test-xyz.org";
        private const string ValueExclusive = "exclusive";

        private static string[] ArgumentsWithRedefinition = new string[] { "-u", ValueTestuser, "--user", ValueNewuser };
        private static string[] ArgumentsDirectAssignment = new string[] { $"{KeyDatabase}={ValueLocalMaria}" };
        private static string[] ArgumentsDirectAssignmentWithEmptyValue = new string[] { $"{KeyDatabase}=" };
        private static string[] ArgumentsDoubleDashValueMissing = new string[] { "--user" };
        private static string[] ArgumentsGroupedWithValueRight = new string[] { "-qiu", ValueTestuser };
        private static string[] ArgumentsGroupedValueMissing = new string[] { "-qiu" };
        private static string[] ArgumentsGroupedValueNotEnd = new string[] { "-qui", ValueTestuser };
        private static string[] ArgumentsGroupedWithUnknown = new string[] { "-qif" };
        private static string[] ArgumentsUnknownSingleDash = new string[] { "-f" };
        private static string[] ArgumentsUnknownDoubleDash = new string[] { "--missing" };
        private static string[] ArgumentsRequiredValuesRightDry = new string[] { $"{VerbUpload}", $"{ValueTargetRemote}" };
        private static string[] ArgumentsRequiredValuesInvertedDry = new string[] { $"{ValueTargetRemote}", $"{VerbUpload}" };
        private static string[] ArgumentsRequiredValuesSwitchBefore = new string[] { $"--quiet", $"{VerbUpload}", $"{ValueTargetRemote}" };
        private static string[] ArgumentsRequiredValuesSwitchWithin = new string[] { $"{VerbUpload}", "--quiet", $"{ValueTargetRemote}" };
        private static string[] ArgumentsRequiredValuesArgumentOptionAfter = new string[] { $"{VerbUpload}", $"{ValueTargetRemote}", "--mode", $"{ValueExclusive}" };
        private static string[] ArgumentsRequiredValuesArgumentOptionWithin = new string[] { $"{VerbUpload}", "--mode", $"{ValueExclusive}", $"{ValueTargetRemote}" };
        private static string[] ArgumentsRequiredValuesSwitchAfter = new string[] { $"{VerbUpload}", $"{ValueTargetRemote}", "--quiet" };
        private static string[] ArgumentsTooManyValues = new string[] { $"{VerbUpload}", $"{ValueTargetRemote}", "126383" };
        private static string[] ArgumentsInsufficientValues = new string[] { $"{VerbUpload}", "--mode", $"{ValueExclusive}" };

        [Fact]
        public void UnloadedProviderHasNoKeys()
        {
            var provider = createProvider(ArgumentsWithRedefinition, KeyNameBehavior.Ignore);
            var childKeys = provider.GetAllKeysAndValuesFromProvider();

            childKeys.Should().BeEmpty("Provider should contain no keys until Load() was invoked.");
        }

        [Fact]
        public void GetChildKeys_EmptyIfNotLoad()
        {
            var provider = createProvider(ArgumentsWithRedefinition, KeyNameBehavior.Ignore);
            var childKeys = provider.GetChildKeys(new string[0], null);

            childKeys.Should().BeEmpty("GetChildKeys() must not return any keys until Load() was invoked and no parent keys provided.");
        }

        [Fact]
        public void GetChildKeys_ReturnCorrectEarlierKeysIfNotLoad()
        {
            string[] earlierKeys = new string[] { "alpha", "sec" + ConfigurationPath.KeyDelimiter + "bravo", "last.one" };

            var provider = createProvider(ArgumentsWithRedefinition, KeyNameBehavior.Ignore);
            var childKeys = provider.GetChildKeys(earlierKeys, null);

            childKeys.Should().BeEquivalentTo(earlierKeys, "GetChildKeys() must return earlier keys and only until Load() was invoked.");
        }

        [Fact]
        public void Load_UsingLastAssignmentForDuplicateKeys()
        {
            var expectedValues = new Dictionary<string, string>
            {
                {CommandLineArgsTestDefinitions.ConfigNameAccountsUser, ValueNewuser},
            };

            var provider = createLoadedProvider(ArgumentsWithRedefinition, KeyNameBehavior.Update);

            var allKeysAndValues = provider.GetAllKeysAndValuesFromProvider();
            allKeysAndValues.Should().Equal(expectedValues);
        }

        [Fact]
        public void Load_WithoutUpdatingDuplicateKeys()
        {
            var expectedValues = new Dictionary<string, string>
            {
                {CommandLineArgsTestDefinitions.ConfigNameAccountsUser, ValueTestuser},
            };

            var provider = createLoadedProvider(ArgumentsWithRedefinition, KeyNameBehavior.Ignore);

            var allKeysAndValues = provider.GetAllKeysAndValuesFromProvider();
            allKeysAndValues.Should().Equal(expectedValues);
        }

        [Theory]
        [InlineData("-qid")]
        [InlineData("-idq")]
        [InlineData("-qdi")]
        public void Load_GroupedSwitches(string argument)
        {
            var expectedValues = new Dictionary<string, string>
            {
                {CommandLineArgsTestDefinitions.ConfigNameGeneralQuiet, CommandLineArgsTestDefinitions.ValueTrue},
                {CommandLineArgsTestDefinitions.ConfigNameGeneralInteractive, CommandLineArgsTestDefinitions.ValueTrue},
                {CommandLineArgsTestDefinitions.ConfigNameDebugMode, CommandLineArgsTestDefinitions.ValueYes},
            };

            var provider = createLoadedProvider(new string[] { argument }, KeyNameBehavior.Ignore);

            var allKeysAndValues = provider.GetAllKeysAndValuesFromProvider();
            allKeysAndValues.Should().Equal(expectedValues, "Al switches in a group must be identified.");
        }

        [Fact]
        public void Load_GroupedSwitchesWithValueRight()
        {
            var expectedValues = new Dictionary<string, string>
            {
                {CommandLineArgsTestDefinitions.ConfigNameGeneralQuiet, CommandLineArgsTestDefinitions.ValueTrue},
                {CommandLineArgsTestDefinitions.ConfigNameGeneralInteractive, CommandLineArgsTestDefinitions.ValueTrue},
                {CommandLineArgsTestDefinitions.ConfigNameAccountsUser, ValueTestuser },
            };

            var provider = createLoadedProvider(ArgumentsGroupedWithValueRight, KeyNameBehavior.Ignore);

            var allKeysAndValues = provider.GetAllKeysAndValuesFromProvider();
            allKeysAndValues.Should().Equal(expectedValues);
        }

        [Fact]
        public void Load_DoubleDashValueMissing_Throws()
        {
            var provider = createProvider(ArgumentsDoubleDashValueMissing, KeyNameBehavior.Ignore);

            provider.Invoking(p => p.Load())
                .Should().ThrowExactly<ValueRequiredException>("If a required value is not provided, an error must occur.")
                .And.Argument.Should().BeEquivalentTo(ArgumentsDoubleDashValueMissing[0]);

            provider.GetAllKeysAndValuesFromProvider().Should().BeEmpty("If an error occured, GetAllKeysAndValuesFromProvider() must not return any keys or values.");
        }

        [Fact]
        public void Load_GroupedValueMissing_Throws()
        {
            var provider = createProvider(ArgumentsGroupedValueMissing, KeyNameBehavior.Ignore);

            provider.Invoking(p => p.Load())
                .Should().ThrowExactly<ValueRequiredException>("If a required value is not provided, an error must occur.")
                .And.Argument.Should().BeEquivalentTo("-u");

            provider.GetAllKeysAndValuesFromProvider().Should().BeEmpty("If an error occured, GetAllKeysAndValuesFromProvider() must not return any keys or values.");
        }

        [Fact]
        public void Load_GroupedValueNotEnd_Throws()
        {
            var provider = createProvider(ArgumentsGroupedValueNotEnd, KeyNameBehavior.Ignore);

            provider.Invoking(p => p.Load())
                .Should().ThrowExactly<ValueRequiredException>("If a required value is not provided, an error must occur.")
                .And.Argument.Should().BeEquivalentTo("-u");

            provider.GetAllKeysAndValuesFromProvider().Should().BeEmpty("If an error occured, GetAllKeysAndValuesFromProvider() must not return any keys or values.");
        }

        [Fact]
        public void Load_GroupedWithUnknown_Throws()
        {
            var provider = createProvider(ArgumentsGroupedWithUnknown, KeyNameBehavior.Ignore);

            provider.Invoking(p => p.Load())
                .Should().ThrowExactly<ArgumentUnknownException>("If an unknown argument has been provided, an error must occur.")
                .And.Argument.Should().BeEquivalentTo("-f");

            provider.GetAllKeysAndValuesFromProvider().Should().BeEmpty("If an error occured, GetAllKeysAndValuesFromProvider() must not return any keys or values.");
        }

        [Fact]
        public void Load_UnknownSingleDash_Throws()
        {
            var provider = createProvider(ArgumentsUnknownSingleDash, KeyNameBehavior.Ignore);

            provider.Invoking(p => p.Load())
                .Should().ThrowExactly<ArgumentUnknownException>("If an unknown argument has been provided, an error must occur.")
                .And.Argument.Should().BeEquivalentTo(ArgumentsUnknownSingleDash[0]);

            provider.GetAllKeysAndValuesFromProvider().Should().BeEmpty("If an error occured, GetAllKeysAndValuesFromProvider() must not return any keys or values.");
        }

        [Fact]
        public void Load_UnknownDoubleDash_Throws()
        {
            var provider = createProvider(ArgumentsUnknownDoubleDash, KeyNameBehavior.Ignore);

            provider.Invoking(p => p.Load())
                .Should().ThrowExactly<ArgumentUnknownException>("If an unknown argument has been provided, an error must occur.")
                .And.Argument.Should().BeEquivalentTo(ArgumentsUnknownDoubleDash[0]);

            provider.GetAllKeysAndValuesFromProvider().Should().BeEmpty("If an error occured, GetAllKeysAndValuesFromProvider() must not return any keys or values.");
        }

        [Fact]
        public void Load_DirectAssignment()
        {
            var expectedValues = new Dictionary<string, string>
            {
                {KeyDatabase, ValueLocalMaria},
            };

            var provider = createLoadedProvider(ArgumentsDirectAssignment, KeyNameBehavior.Ignore, allowAssignments: true);
            var allKeysAndValues = provider.GetAllKeysAndValuesFromProvider();
            allKeysAndValues.Should().Equal(expectedValues);
        }

        [Fact]
        public void Load_DirectAssignmentWithEmptyValue()
        {
            var expectedValues = new Dictionary<string, string>
            {
                {KeyDatabase, string.Empty},
            };

            var provider = createLoadedProvider(ArgumentsDirectAssignmentWithEmptyValue, KeyNameBehavior.Ignore, allowAssignments: true);
            var allKeysAndValues = provider.GetAllKeysAndValuesFromProvider();
            allKeysAndValues.Should().Equal(expectedValues);
        }

        [Fact]
        public void Load_AssignmentNotAllowed_Throws()
        {
            var provider = createProvider(ArgumentsDirectAssignment, KeyNameBehavior.Ignore);

            provider.Invoking(p => p.Load())
                .Should().ThrowExactly<NotSupportedException>("If command line contains direct assignments while those are forbissen, an error must occur.");

            provider.GetAllKeysAndValuesFromProvider().Should().BeEmpty("If an error occured, GetAllKeysAndValuesFromProvider() must not return any keys or values.");
        }

        [Fact]
        public void Load_RequiredValues_OrderMatters()
        {
            var expectedValues = new Dictionary<string, string>
            {
                {CommandLineArgsTestDefinitions.ConfigNameSecondVerb, VerbUpload},
                {CommandLineArgsTestDefinitions.ConfigNameSecondTarget, ValueTargetRemote},
            };

            var provider = createLoadedProvider(ArgumentsRequiredValuesRightDry, KeyNameBehavior.Ignore, withRequiredValues: true);
            var allKeysAndValues = provider.GetAllKeysAndValuesFromProvider();
            allKeysAndValues.Should().Equal(expectedValues, "order of argument values matters for their mapping.");

            expectedValues = new Dictionary<string, string>
            {
                {CommandLineArgsTestDefinitions.ConfigNameSecondVerb, ValueTargetRemote},
                {CommandLineArgsTestDefinitions.ConfigNameSecondTarget, VerbUpload},
            };

            provider = createLoadedProvider(ArgumentsRequiredValuesInvertedDry, KeyNameBehavior.Ignore, withRequiredValues: true);
            allKeysAndValues = provider.GetAllKeysAndValuesFromProvider();
            allKeysAndValues.Should().Equal(expectedValues, "order of argument values matters for their mapping.");
        }

        [Fact]
        public void Load_RequiredValues_SwitchDoesntDisturb()
        {
            var expectedValues = new Dictionary<string, string>
            {
                {CommandLineArgsTestDefinitions.ConfigNameSecondVerb, VerbUpload},
                {CommandLineArgsTestDefinitions.ConfigNameSecondTarget, ValueTargetRemote},
                {CommandLineArgsTestDefinitions.ConfigNameGeneralQuiet, CommandLineArgsTestDefinitions.ValueTrue},
            };

            var provider = createLoadedProvider(ArgumentsRequiredValuesSwitchBefore, KeyNameBehavior.Ignore, withRequiredValues: true);
            var allKeysAndValues = provider.GetAllKeysAndValuesFromProvider();
            allKeysAndValues.Should().Equal(expectedValues, "any argument option before, between or after value arguments is allowed and must not disturb parsing values.");

            provider = createLoadedProvider(ArgumentsRequiredValuesSwitchWithin, KeyNameBehavior.Ignore, withRequiredValues: true);
            allKeysAndValues = provider.GetAllKeysAndValuesFromProvider();
            allKeysAndValues.Should().Equal(expectedValues, "any argument option before, between or after value arguments is allowed and must not disturb parsing values.");

            provider = createLoadedProvider(ArgumentsRequiredValuesSwitchAfter, KeyNameBehavior.Ignore, withRequiredValues: true);
            allKeysAndValues = provider.GetAllKeysAndValuesFromProvider();
            allKeysAndValues.Should().Equal(expectedValues, "any argument option before, between or after value arguments is allowed and must not disturb parsing values.");
        }

        [Fact]
        public void Load_RequiredValues_ArgumentDoesntDisturb()
        {
            var expectedValues = new Dictionary<string, string>
            {
                {CommandLineArgsTestDefinitions.ConfigNameSecondVerb, VerbUpload},
                {CommandLineArgsTestDefinitions.ConfigNameSecondTarget, ValueTargetRemote},
                {CommandLineArgsTestDefinitions.ConfigNameSecondMode, ValueExclusive },
            };

            var provider = createLoadedProvider(ArgumentsRequiredValuesArgumentOptionAfter, KeyNameBehavior.Ignore, withRequiredValues: true);
            var allKeysAndValues = provider.GetAllKeysAndValuesFromProvider();
            allKeysAndValues.Should().Equal(expectedValues, "any argument option before, between or after value arguments is allowed and must not disturb parsing values.");

            provider = createLoadedProvider(ArgumentsRequiredValuesArgumentOptionWithin, KeyNameBehavior.Ignore, withRequiredValues: true);
            allKeysAndValues = provider.GetAllKeysAndValuesFromProvider();
            allKeysAndValues.Should().Equal(expectedValues, "any argument option before, between or after value arguments is allowed and must not disturb parsing values.");
        }

        [Fact]
        public void Load_AdditionalValue_Throws()
        {
            var provider = createProvider(ArgumentsTooManyValues, KeyNameBehavior.Ignore, withRequiredValues: true);

            provider.Invoking(p => p.Load())
                .Should().ThrowExactly<IndexOutOfRangeException>("An additional direct value is not allowed if all required values have been already parsed.");

            provider.GetAllKeysAndValuesFromProvider().Should().BeEmpty("If an error occured, GetAllKeysAndValuesFromProvider() must not return any keys or values.");
        }

        [Fact]
        public void Load_InsufficientValues_Throws()
        {
            var provider = createProvider(ArgumentsInsufficientValues, KeyNameBehavior.Ignore, withRequiredValues: true);

            provider.Invoking(p => p.Load())
                .Should().ThrowExactly<ValueRequiredException>("if less values have been provided than defined as required, an error must occur.")
                .And.Argument.Should().BeNullOrEmpty();

            provider.GetAllKeysAndValuesFromProvider().Should().BeEmpty("If an error occured, GetAllKeysAndValuesFromProvider() must not return any keys or values.");
        }

        private static CommandLineArgsConfigurationProvider createLoadedProvider(string[] arguments,
            KeyNameBehavior keyBehavior,
            ParsingResultsErrorBehavior errorBehavor = ParsingResultsErrorBehavior.OnFirstError,
            bool allowAssignments = false,
            bool withRequiredValues = false)
        {
            var provider = createProvider(arguments, keyBehavior, errorBehavor, allowAssignments, withRequiredValues);
            provider.Load();
            return provider;
        }

        private static CommandLineArgsConfigurationProvider createProvider(string[] arguments,
            KeyNameBehavior keyBehavior,
            ParsingResultsErrorBehavior errorBehavor = ParsingResultsErrorBehavior.OnFirstError, 
            bool allowAssignments = false,
            bool withRequiredValues = false)
        {
            var src = new CommandLineArgsConfigurationSource(withRequiredValues ? CommandLineArgsTestDefinitions.DefinitionsWithValues : CommandLineArgsTestDefinitions.Definitions, arguments)
            {
                DuplicateKeyBehavior = keyBehavior,
                ParsingErrorBehavior = errorBehavor,
                AllowDirectAssignments = allowAssignments
            };

            return new CommandLineArgsConfigurationProvider(src);
        }
    }
}