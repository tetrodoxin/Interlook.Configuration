#region license

//MIT License

//Copyright(c) 2013-2020 Andreas Hübner

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

#endregion 
using Interlook.Monads;
using Interlook.Text;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Interlook.Configuration.CommandLine
{
    public class CommandLineArgsConfigurationProvider : ConfigurationProvider
    {
        private readonly Dictionary<string, ArgumentDefinition> _indexedArgumentDefinitions;
        private readonly CommandLineArgsConfigurationSource _source;
        private readonly IList<string> _valueDefinitions;

        /// <summary>
        /// Creates a new instance of <see cref="CommandLineArgsConfigurationProvider"/>
        /// </summary>
        public CommandLineArgsConfigurationProvider(CommandLineArgsConfigurationSource source)
        {
            _source = source.Clone();

            _indexedArgumentDefinitions = source.ArgumentDefinitions
                .Where(ArgumentDefinition.IsNamedForNamedArguments)
                .SelectMany(definition => ArgumentDefinition.GetAllNames(definition)
                    .CatMaybes()
                    .Select(name => (Key: name, Definition: definition)))
                .ToDictionary(p => p.Key, p => p.Definition, StringComparer.Ordinal);   // index is case sensitive

            _valueDefinitions = source.ArgumentDefinitions
                .OfType<ArgumentDefinition.RequiredValueDefinition>()
                .Select(p => p.ConfigurationKey)
                .ToList();
        }

        public override IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
        {
            return base.GetChildKeys(earlierKeys, parentPath);
        }

        public override void Load()
        {
            Data = _source.HandleResultsErrors(parseConfigFromCommandLine())
                .ToDictionary();
        }

        private IParsingResultsDictionary parseConfigFromCommandLine()
        {
            var arguments = new Queue<string>(_source.Args);
            var plainValueCounter = 0;
            var resultsBuilder = new ParsingResultsBuilder(_source.DuplicateKeyBehavior);
            var lastArgumentString = _source.Args.First();

            while (arguments.Any())
            {
                var currentArgumentString = arguments.Dequeue();
                var parsedArgument = ArgumentType.CreateFrom(currentArgumentString);
                switch (parsedArgument)
                {
                    case ArgumentType.SingleDash dashArgument:
                        handleSingleDashArgument(resultsBuilder, arguments, dashArgument);
                        break;

                    case ArgumentType.DoubleDash doubleDashArgument:
                        handleDoubledashArgument(resultsBuilder, arguments, doubleDashArgument);
                        break;

                    case ArgumentType.Assignment assignment:
                        if (_source.AllowDirectAssignments)
                            resultsBuilder.Add(assignment.Key, assignment.Value);
                        else
                            resultsBuilder.Error(new NotSupportedException($"Invalid argument '{assignment.Argument}' as direct assignments are not allowed."));
                        break;

                    case ArgumentType.Invalid invalid:
                        resultsBuilder.Error(new ParsingException("Invalid argument.", $"Argument somewhere near: '{lastArgumentString.Limit(16)}'"));
                        break;

                    case ArgumentType.Plain plain:
                        if (plainValueCounter < _valueDefinitions.Count)
                        {
                            resultsBuilder.Add(_valueDefinitions[plainValueCounter], plain.Argument);
                        }
                        else
                        {
                            resultsBuilder.Error(new IndexOutOfRangeException($"No more than {_valueDefinitions.Count} value arguments are allowed."));
                        }

                        plainValueCounter++;
                        break;
                }

                lastArgumentString = currentArgumentString;
            }

            if (_valueDefinitions.Count > plainValueCounter)
                resultsBuilder.Error(new ValueRequiredException(string.Empty, $"Not all required values have been provided. Expected: {_valueDefinitions.Count}, Provided: {plainValueCounter}"));

            return resultsBuilder.GetResults();
        }

        private static Unit addSwitchToResults(ParsingResultsBuilder result, ArgumentDefinition.SwitchOptionDefinition currentDefinition)
            => result.Add(currentDefinition.ConfigurationKey, currentDefinition.ConfigurationValue);

        private static Unit addValueOptionToResults(ParsingResultsBuilder result,
            Queue<string> arguments,
            ArgumentDefinition.ValueOptionDefinition valueOption,
            Func<ArgumentDefinition.ValueOptionDefinition, string> argumentNameSelector)
            => arguments.Any()
                ? result.Add(valueOption.ConfigurationKey, arguments.Dequeue())
                : result.ErrorArgumentRequiresValue($"{argumentNameSelector(valueOption)}", $"The argument '{argumentNameSelector(valueOption)}' requires an additional value.");

        private Unit handleDoubledashArgument(ParsingResultsBuilder result, Queue<string> arguments, ArgumentType.DoubleDash doubleDashArgument)
        {
            if (_indexedArgumentDefinitions.TryGetValue(doubleDashArgument.Argument, out ArgumentDefinition currentDefinition))
            {
                switch (currentDefinition)
                {
                    case ArgumentDefinition.SwitchOptionDefinition switchOption:
                        return addSwitchToResults(result, switchOption);

                    case ArgumentDefinition.ValueOptionDefinition valueOption:
                        return addValueOptionToResults(result, arguments, valueOption, p => $"--{p.LongName.GetValue(null)}");
                }
            }

            // if we're her, it's an error, unknown argument/switch
            return result.ErrorArgumentUnknown($"--{doubleDashArgument.Argument}");
        }

        private Unit handleSingleDashArgument(ParsingResultsBuilder result, Queue<string> arguments, ArgumentType.SingleDash dashArgument)
        {
            // separate every character of a possible switch-group
            var allSwitches = dashArgument.Argument.Select(c => c.ToString()).ToList();

            for (int i = 0; i < allSwitches.Count; i++)
            {
                if (_indexedArgumentDefinitions.TryGetValue(allSwitches[i], out ArgumentDefinition currentDefinition))
                {
                    switch (currentDefinition)
                    {
                        case ArgumentDefinition.SwitchOptionDefinition switchOption:
                            addSwitchToResults(result, switchOption);
                            break;

                        case ArgumentDefinition.ValueOptionDefinition valueOption:
                            if (i == allSwitches.Count - 1)
                            {
                                addValueOptionToResults(result, arguments, valueOption, p => $"-{p.ShortName.GetValue(null)}");
                            }
                            else
                            {
                                // error, value-argument must be last character
                                string argumentName = $"-{valueOption.ShortName.GetValue(null)}";
                                return result.ErrorArgumentRequiresValue(argumentName, $"Argument '-{argumentName}' requires a value and thus must be last character of an argument group.");
                            }
                            break;
                    }
                }
                else
                {
                    // error, unknown argument/switch
                    result.ErrorArgumentUnknown($"-{allSwitches[i]}");
                }
            }

            return Unit.Default;
        }

        private abstract class ArgumentType
        {
            public string Argument { get; }

            private ArgumentType(string arg)
            {
                Argument = arg;
            }

            public static ArgumentType CreateFrom(string commandLineArg)
            {
                if (string.IsNullOrWhiteSpace(commandLineArg))
                {
                    return new Invalid();
                }
                else if (commandLineArg.StartsWith("--"))
                {
                    return commandLineArg.Length > 2
                        ? (ArgumentType)new DoubleDash(commandLineArg.Substring(2))
                        : new Invalid();
                }
                else if (commandLineArg.StartsWith("-"))
                {
                    return commandLineArg.Length > 1
                        ? (ArgumentType)new SingleDash(commandLineArg.Substring(1))
                        : new Invalid();
                }
                else
                {
                    var assignmentPos = commandLineArg.IndexOf('=');

                    return assignmentPos >= 0
                        ? (ArgumentType)new Assignment(commandLineArg, assignmentPos)
                        : new Plain(commandLineArg);
                }
            }

            public class Assignment : ArgumentType
            {
                public string Key { get; }

                public string Value { get; }

                public Assignment(string arg, int assignmentPos) : base(arg)
                {
                    Key = arg.Substring(0, assignmentPos);
                    Value = assignmentPos < arg.Length - 1
                        ? arg.Substring(assignmentPos + 1)
                        : string.Empty;
                }
            }

            public class DoubleDash : ArgumentType
            {
                public DoubleDash(string arg) : base(arg)
                { }
            }

            public class Invalid : ArgumentType
            {
                public Invalid() : base(null)
                { }
            }

            public class Plain : ArgumentType
            {
                public Plain(string arg) : base(arg)
                { }
            }

            public class SingleDash : ArgumentType
            {
                public SingleDash(string arg) : base(arg)
                { }
            }
        }
    }
}