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

namespace Interlook.Configuration.CommandLine
{
    public static class CommandLineArgsConfigurationExtensions
    {
        /// <summary>
        /// Adds a command line configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="args">The arguments provided via command line.</param>
        /// <param name="argumentDefinitions">
        /// The argument definitions, including those for switch options, value options
        /// AND required values. Must not contain ambiguous mappings for argument key names.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddCommandLine(this IConfigurationBuilder builder, IEnumerable<string> args, IEnumerable<ArgumentDefinition> argumentDefinitions)
            => builder.Add(new CommandLineArgsConfigurationSource(argumentDefinitions, args));

        /// <summary>
        /// Adds a command line configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="args">The arguments provided via command line.</param>
        /// <param name="argumentDefinitions">
        /// The argument definitions, including those for switch options, value options
        /// AND required values. Must not contain ambiguous mappings for argument key names.</param>
        /// <param name="allowDirectAssignments">
        /// Determines, if assignments of the format 'key=value' are valid in command line
        /// and directly used as key/value-pairs for configuration source.
        /// </param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddCommandLine(this IConfigurationBuilder builder, IEnumerable<string> args, IEnumerable<ArgumentDefinition> argumentDefinitions, bool allowDirectAssignments)
            => builder.Add(new CommandLineArgsConfigurationSource(argumentDefinitions, args) { AllowDirectAssignments = allowDirectAssignments });

        /// <summary>
        /// Adds a command line configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="args">The arguments provided via command line.</param>
        /// <param name="argumentDefinitions">
        /// The argument definitions, including those for switch options, value options
        /// AND required values. Must not contain ambiguous mappings for argument key names.</param>
        /// <param name="duplicateKeyBehavior">
        /// This value determines what happens, if a key name is parsed multiple times.
        /// </param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddCommandLine(this IConfigurationBuilder builder, IEnumerable<string> args, IEnumerable<ArgumentDefinition> argumentDefinitions, KeyNameBehavior duplicateKeyBehavior)
            => builder.Add(new CommandLineArgsConfigurationSource(argumentDefinitions, args) { DuplicateKeyBehavior = duplicateKeyBehavior });

        /// <summary>
        /// Adds a command line configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="args">The arguments provided via command line.</param>
        /// <param name="argumentDefinitions">
        /// The argument definitions, including those for switch options, value options
        /// AND required values. Must not contain ambiguous mappings for argument key names.</param>
        /// <param name="allowDirectAssignments">
        /// Determines, if assignments of the format 'key=value' are valid in command line
        /// and directly used as key/value-pairs for configuration source.
        /// </param>
        /// <param name="duplicateKeyBehavior">
        /// This value determines what happens, if a key name is parsed multiple times.
        /// </param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddCommandLine(this IConfigurationBuilder builder, IEnumerable<string> args, IEnumerable<ArgumentDefinition> argumentDefinitions, bool allowDirectAssignments, KeyNameBehavior duplicateKeyBehavior)
            => builder.Add(new CommandLineArgsConfigurationSource(argumentDefinitions, args) { AllowDirectAssignments = allowDirectAssignments, DuplicateKeyBehavior = duplicateKeyBehavior });

        /// <summary>
        /// Adds a command line configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="newSource">An instance of <see cref="CommandLineArgsConfigurationSource"/> that is used to provide an <see cref="CommandLineArgsConfigurationProvider"/></param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddCommandLine(this IConfigurationBuilder builder, CommandLineArgsConfigurationSource newSource)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.Add(newSource);
        }
    }
}