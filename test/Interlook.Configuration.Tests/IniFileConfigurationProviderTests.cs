using FluentAssertions;
using Interlook.Configuration.IniFile;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Interlook.Configuration.Tests
{
    public class IniFileConfigurationProviderTests
    {
        private const string IniFileOneName = "IniFile1.cfg";

        public static Dictionary<string, string> IniFileOneContents_IgnoringUpdate = new Dictionary<string, string>
        {
            { "global_one", "This is with whitespaces" },
            { "Section1:switch1", "" },
            { "Section1:redefine", "before" },
            { "Section1:data1", "alpha,bravo,tango" },
            { "Section2:data3", "   single-quoted with trailing and leading spaces  " },
            { "Section2:switch1", "" },
            { "Section2:data2", "quoted with trailing spaces   " },
        };

        public static Dictionary<string, string> IniFileOneContents_UsingUpdate = new Dictionary<string, string>
        {
            { "global_one", "This is with whitespaces" },
            { "Section1:switch1", "" },
            { "Section1:redefine", "after" },
            { "Section1:data1", "alpha,bravo,tango" },
            { "Section2:data3", "   single-quoted with trailing and leading spaces  " },
            { "Section2:switch1", "yes" },
            { "Section2:data2", "quoted with trailing spaces   " },
        };

        [Fact]
        public void Load_WithoutUpdatingDuplicateKeys()
        {
            var src = new IniFileConfigurationSource(IniFileOneName)
            {
                DuplicateKeyBehavior = KeyNameBehavior.Ignore,
                FileProvider = new PhysicalFileProvider(AppContext.BaseDirectory),
                Optional = false,
                ReloadOnChange = false
            };

            var provider = new IniFileConfigurationProvider(src);
            provider.Load();
            var allKeysAndValues = provider.GetAllKeysAndValuesFromProvider();

            allKeysAndValues.Should().Equal(IniFileOneContents_IgnoringUpdate, "Keys and values read from ini-file should represent expected values without update for duplicate keys.");
        }

        [Fact]
        public void Load_UsingLastAssignmentForDuplicateKeys()
        {
            var src = new IniFileConfigurationSource(IniFileOneName)
            {
                DuplicateKeyBehavior = KeyNameBehavior.Update,
                FileProvider = new PhysicalFileProvider(AppContext.BaseDirectory),
                Optional = false,
                ReloadOnChange = false
            };

            var provider = new IniFileConfigurationProvider(src);
            provider.Load();
            var allKeysAndValues = provider.GetAllKeysAndValuesFromProvider();

            allKeysAndValues.Should().Equal(IniFileOneContents_UsingUpdate, "Keys and values read from ini-file should represent expected values WITH update for duplicate keys.");
        }

        [Fact]
        public void Load_ThrowOnFirstErrorForDuplicateKeys()
        {
            var src = new IniFileConfigurationSource(IniFileOneName)
            {
                DuplicateKeyBehavior = KeyNameBehavior.Error,
                ParsingErrorBehavior = ParsingResultsErrorBehavior.OnFirstError,
                FileProvider = new PhysicalFileProvider(AppContext.BaseDirectory),
                Optional = false,
                ReloadOnChange = false,
            };

            var provider = new IniFileConfigurationProvider(src);

            provider.Invoking(p => p.Load()).Should().ThrowExactly<ArgumentAlreadyDefinedException>(
                "Redefining a key with provider settings {DuplicateKeyBehavior = KeyNameBehavior.Error, ParsingErrorBehavior = ParsingResultsErrorBehavior.ExceptionOnFirstError} must throw an exception for the first error.");
            provider.GetAllKeysAndValuesFromProvider().Should().BeEmpty("When an error occured, parsed details must not be returned.");
        }

        [Fact]
        public void Load_ThrowForAllDuplicateKeyErrors()
        {
            var src = new IniFileConfigurationSource(IniFileOneName)
            {
                DuplicateKeyBehavior = KeyNameBehavior.Error,
                ParsingErrorBehavior = ParsingResultsErrorBehavior.Aggregate,
                FileProvider = new PhysicalFileProvider(AppContext.BaseDirectory),
                Optional = false,
                ReloadOnChange = false,
            };

            var provider = new IniFileConfigurationProvider(src);

            provider.Invoking(p => p.Load()).Should().ThrowExactly<AggregateException>(
                "Redefining a key with provider settings {DuplicateKeyBehavior = KeyNameBehavior.Error, ParsingErrorBehavior = ParsingResultsErrorBehavior.ExceptionForAllErrors} must throw an exception for the first error.")
                .And.InnerExceptions.Should().AllBeOfType<ArgumentAlreadyDefinedException>("Only errors for duplicate keys are expected.").And.HaveCount(2, "Exactly two errors for duplicate keys was expected.");
            provider.GetAllKeysAndValuesFromProvider().Should().BeEmpty("When an error occured, parsed details must not be returned.");
        }

        [Fact]
        public void Load_DontThrowForDuplicateKeyErrors()
        {
            var src = new IniFileConfigurationSource(IniFileOneName)
            {
                DuplicateKeyBehavior = KeyNameBehavior.Error,
                ParsingErrorBehavior = ParsingResultsErrorBehavior.None,
                FileProvider = new PhysicalFileProvider(AppContext.BaseDirectory),
                Optional = false,
                ReloadOnChange = false,
            };

            var provider = new IniFileConfigurationProvider(src);

            provider.Load();    // should not throw anything
            provider.GetAllKeysAndValuesFromProvider().Should().BeEmpty("When an error occured, parsed details must not be returned.");
        }

        [Fact]
        public void Load_ManuallyHandleFirstErrorForDuplicateKeys()
        {
            IEnumerable<Exception> errors = null;
            Action<IEnumerable<Exception>> function = x => errors = x.ToList();

            var src = new IniFileConfigurationSource(IniFileOneName)
            {
                DuplicateKeyBehavior = KeyNameBehavior.Error,
                ParsingErrorBehavior = ParsingResultsErrorBehavior.OnFirstError,
                FileProvider = new PhysicalFileProvider(AppContext.BaseDirectory),
                Optional = false,
                ReloadOnChange = false,
            }.WithErrorHandlingFunction(function);

            var provider = new IniFileConfigurationProvider(src);

            provider.Load();    // should not throw anything
            provider.GetAllKeysAndValuesFromProvider().Should().BeEmpty("When an error occured, parsed details must not be returned.");
            errors.Should()
                .HaveCount(1, "Exactly one error for duplicate keys was expected for the fail-on-first-error mode.")
                .And.AllBeOfType<ArgumentAlreadyDefinedException>("Only errors for duplicate keys are expected.");
        }

        [Fact]
        public void Load_ManuallyHandleAllDuplicateKeyErrors()
        {
            IEnumerable<Exception> errors = null;
            Action<IEnumerable<Exception>> function = x => errors = x.ToList();

            var src = new IniFileConfigurationSource(IniFileOneName)
            {
                DuplicateKeyBehavior = KeyNameBehavior.Error,
                ParsingErrorBehavior = ParsingResultsErrorBehavior.Aggregate,
                FileProvider = new PhysicalFileProvider(AppContext.BaseDirectory),
                Optional = false,
                ReloadOnChange = false,
            }.WithErrorHandlingFunction(function);

            var provider = new IniFileConfigurationProvider(src);

            provider.Load();    // should not throw anything
            provider.GetAllKeysAndValuesFromProvider().Should().BeEmpty("When an error occured, parsed details must not be returned.");
            errors.Should()
                .HaveCount(2, "Exactly two errors for duplicate keys were expected.")
                .And.AllBeOfType<ArgumentAlreadyDefinedException>("Only errors for duplicate keys are expected.");
        }

        [Fact]
        public void Load_DontManuallyHandleDuplicateKeyErrors()
        {
            IEnumerable<Exception> errors = null;
            Action<IEnumerable<Exception>> function = x => errors = x.ToList();

            var src = new IniFileConfigurationSource(IniFileOneName)
            {
                DuplicateKeyBehavior = KeyNameBehavior.Error,
                ParsingErrorBehavior = ParsingResultsErrorBehavior.None,
                FileProvider = new PhysicalFileProvider(AppContext.BaseDirectory),
                Optional = false,
                ReloadOnChange = false,
            }.WithErrorHandlingFunction(function);

            var provider = new IniFileConfigurationProvider(src);

            provider.Load();    // should not throw anything
            provider.GetAllKeysAndValuesFromProvider().Should().BeEmpty("When an error occured, parsed details must not be returned.");
            errors.Should().BeNull("Since error handling was disbled the error variable was expected to be NULL.");
        }
    }
}