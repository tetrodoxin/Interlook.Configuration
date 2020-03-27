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
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Interlook.Configuration.CommandLine
{
    public class CommandLineArgsConfigurationSource : IConfigurationSource, IUsingParsingResults
    {
        /// <summary>
        /// Gets all argument definitions. It is ensured, that no collisions
        /// of <see cref="ArgumentDefinition.LongName"/> or <see cref="ArgumentDefinition.ShortName"/>
        /// are contained.
        /// </summary>
        public IEnumerable<ArgumentDefinition> ArgumentDefinitions { get; }

        /// <summary>
        /// Gets or sets the command line arguments.
        /// </summary>
        public IEnumerable<string> Args { get; }

        /// <summary>
        /// Determines, if assignments of the format 'key=value' are valid in command line
        /// and directly used as key/value-pairs for configuration source.
        /// </summary>
        public bool AllowDirectAssignments { get; set; }

        /// <summary>
        /// This value determines what happens, if a key name
        /// is parsed multiple times.
        /// </summary>
        public KeyNameBehavior DuplicateKeyBehavior { get; set; } = KeyNameBehavior.Ignore;

        /// <summary>
        /// A function, which is to be called to handle parsing errors
        /// instead of raising the exceptions.
        /// </summary>
        public Action<IEnumerable<Exception>> ErrorFunction { get; private set; }

        public ParsingResultsErrorBehavior ParsingErrorBehavior { get; set; } = ParsingResultsErrorBehavior.Aggregate;

        /// <summary>
        /// Builds the <see cref="CommandLineConfigurationProvider"/> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>A <see cref="CommandLineConfigurationProvider"/></returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
            => new CommandLineArgsConfigurationProvider(this);

        public CommandLineArgsConfigurationSource(IEnumerable<ArgumentDefinition> argumentDefinitions, IEnumerable<string> args)
        {
            ArgumentDefinitions = argumentDefinitions.ToList();

            if (ArgumentDefinition.HasDoubleDefinition(argumentDefinitions, out string firstDoubleDefinition))
                throw new System.ArgumentException($"The argument key '{firstDoubleDefinition}' has been defined twice.", nameof(argumentDefinitions));

            Args = args.ToList();
        }

        /// <summary>
        /// Clones this instance by creating a deep copy.
        /// </summary>
        /// <returns>A new instance of <see cref="CommandLineArgsConfigurationSource"/>
        /// as deep copy of the current instance.</returns>
        public CommandLineArgsConfigurationSource Clone()
        {
            return new CommandLineArgsConfigurationSource(ArgumentDefinitions, Args)
            {
                AllowDirectAssignments = AllowDirectAssignments,
                DuplicateKeyBehavior = DuplicateKeyBehavior,
                ParsingErrorBehavior = ParsingErrorBehavior,
            };
        }
    }
}