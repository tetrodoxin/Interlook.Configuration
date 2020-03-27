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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Interlook.Configuration.CommandLine
{
    public class ArgumentDefinition
    {
        /// <summary>
        /// The full path key name of the corresponding configuration key.
        /// </summary>
        public string ConfigurationKey { get; }

        /// <summary>
        /// Indicates, if the argument is mandatory and omitting it will cause an error.
        /// </summary>
        public bool IsRequired { get; }

        /// <summary>
        /// The long (>1 characters) name of the option.
        /// </summary>
        public Maybe<string> LongName { get; }

        /// <summary>
        /// The short (single character) name of the option.
        /// </summary>
        public Maybe<string> ShortName { get; }

        /// <summary>
        /// Definition for an argument, which is a switch option,
        /// meaning an argument without a value, just enabling/marking something.
        /// </summary>
        public class SwitchOptionDefinition : ArgumentDefinition
        {
            /// <summary>
            /// The pre-set configuration value for the switch.
            /// </summary>
            public string ConfigurationValue { get; }

            internal SwitchOptionDefinition(Maybe<string> shortName, Maybe<string> longName, string configurationKey, string configurationValue)
                : base(shortName, longName, configurationKey, false)
            {
                ConfigurationValue = configurationValue;
            }
        }

        /// <summary>
        /// Definition for an argument, which requires a further value.
        /// </summary>
        public class ValueOptionDefinition : ArgumentDefinition
        {
            internal ValueOptionDefinition(Maybe<string> shortName, Maybe<string> longName, string configurationKey, bool isRequired)
                : base(shortName, longName, configurationKey, isRequired)
            { }
        }

        /// <summary>
        /// The argument is a value itself, which is not mapped
        /// by optionname but by its position in the order of the arguments
        /// </summary>
        public class RequiredValueDefinition : ArgumentDefinition
        {
            internal RequiredValueDefinition(string configurationKey)
                : base(Nothing<string>.Instance, Nothing<string>.Instance, configurationKey, true)
            { }
        }

        private ArgumentDefinition(Maybe<string> shortName, Maybe<string> longName, string configurationKey, bool isRequired)
        {
            if (configurationKey.IsNothing())
                throw new System.ArgumentException($"The configuration key must neither be null nor empty.");

            ShortName = shortName;
            LongName = longName;
            ConfigurationKey = configurationKey;
            IsRequired = isRequired;
        }

        /// <summary>
        /// Creates the definition for a required value, which is an argument without
        /// single or double dash or likewise marking, but an actual value.
        /// Values are mapped by its order, the first value, parsed value, that is
        /// no option, is mapped to the first definition for a required value.
        /// Non-option values which cannot be mapped to a definition will cause
        /// an error as unknown options do.
        /// </summary>
        /// <param name="configurationKey">The name of the configuration key to be used for this value.</param>
        /// <returns></returns>
        public static ArgumentDefinition CreateRequiredValue(string configurationKey)
            => new RequiredValueDefinition(configurationKey);

        /// <summary>
        /// Creates a definition for a switch option.
        /// </summary>
        /// <param name="shortName">The short name of the switch.</param>
        /// <param name="configurationKey">The configuration key.</param>
        /// <param name="configurationValue">The pre-set configuration value.</param>
        /// <returns>A new instance of <see cref="SwitchOptionDefinition"/></returns>
        public static ArgumentDefinition CreateSwitchOption(char shortName, string configurationKey, string configurationValue)
            => new SwitchOptionDefinition(new Just<string>(shortName.ToString()), Nothing<string>.Instance, configurationKey, configurationValue);

        /// <summary>
        /// Creates a definition for a switch option.
        /// </summary>
        /// <param name="longName">The long name of the switch.</param>
        /// <param name="configurationKey">The configuration key.</param>
        /// <param name="configurationValue">The pre-set configuration value.</param>
        /// <returns>A new instance of <see cref="SwitchOptionDefinition"/></returns>
        public static ArgumentDefinition CreateSwitchOption(string longName, string configurationKey, string configurationValue)
            => new SwitchOptionDefinition(Nothing<string>.Instance, new Just<string>(longName), configurationKey, configurationValue);

        /// <summary>
        /// Creates a definition for a switch option.
        /// </summary>
        /// <param name="shortName">The short name of the switch.</param>
        /// <param name="longName">The long name of the switch.</param>
        /// <param name="configurationKey">The configuration key.</param>
        /// <param name="configurationValue">The pre-set configuration value.</param>
        /// <returns>A new instance of <see cref="SwitchOptionDefinition"/></returns>
        public static ArgumentDefinition CreateSwitchOption(char shortName, string longName, string configurationKey, string configurationValue)
            => new SwitchOptionDefinition(new Just<string>(shortName.ToString()), new Just<string>(longName), configurationKey, configurationValue);

        /// <summary>
        /// Creates a definition for an option, that requires a further
        /// argument, which will be mapped to the configuration value.
        /// </summary>
        /// <param name="shortName">The short name of the switch.</param>
        /// <param name="configurationKey">The configuration key.</param>
        /// <param name="isRequired">Determines, if the argument is mandatory.</param>
        /// <returns>A new instance of <see cref="ValueOptionDefinition"/></returns>
        public static ArgumentDefinition CreateValueOption(char shortName, string configurationKey, bool isRequired = false)
            => new ValueOptionDefinition(new Just<string>(shortName.ToString()), Nothing<string>.Instance, configurationKey, isRequired);

        /// <summary>
        /// Creates a definition for an option, that requires a further
        /// argument, which will be mapped to the configuration value.
        /// </summary>
        /// <param name="longName">The long name of the switch.</param>
        /// <param name="configurationKey">The configuration key.</param>
        /// <param name="isRequired">Determines, if the argument is mandatory.</param>
        /// <returns>A new instance of <see cref="ValueOptionDefinition"/></returns>
        public static ArgumentDefinition CreateValueOption(string longName, string configurationKey, bool isRequired = false)
            => new ValueOptionDefinition(Nothing<string>.Instance, new Just<string>(longName), configurationKey, isRequired);

        /// <summary>
        /// Creates a definition for an option, that requires a further
        /// argument, which will be mapped to the configuration value.
        /// </summary>
        /// <param name="shortName">The short name of the switch.</param>
        /// <param name="longName">The long name of the switch.</param>
        /// <param name="configurationKey">The configuration key.</param>
        /// <param name="isRequired">Determines, if the argument is mandatory.</param>
        /// <returns>A new instance of <see cref="ValueOptionDefinition"/></returns>
        public static ArgumentDefinition CreateValueOption(char shortName, string longName, string configurationKey, bool isRequired = false)
            => new ValueOptionDefinition(new Just<string>(shortName.ToString()), new Just<string>(longName), configurationKey, isRequired);

        /// <summary>
        /// Checks, if an argument name (short or long) in a sequence of
        /// <see cref="ArgumentDefinition"/> objects, has been defined twice.
        /// </summary>
        /// <param name="definitions">A sequence of argument definitions that is to be searches for duplicate argument names.</param>
        /// <param name="argumentName">A variable, that receives the name of a double defined argument name, if one exists.</param>
        /// <returns><c>true</c>, if a double defined argument name was found and <paramref name="argumentName"/> has been assigned
        /// that name; otherwise <c>false</c></returns>
        public static bool HasDoubleDefinition(IEnumerable<ArgumentDefinition> definitions, out string argumentName)
        {
            if (definitions == null)
            {
                argumentName = null;
                return false;
            }

            argumentName = definitions
                // required values have no name, thus must be ignored here
                .Where(IsNamedForNamedArguments)
                .SelectMany(GetAllNames)
                .CatMaybes()
                .GroupBy(p => p)
                .Where(p => p.Count() > 1)
                .Select(p => p.Key)
                .FirstOrDefault();

            return argumentName != null;
        }

        /// <summary>
        /// Returns <see cref="LongName"/> and/or <see cref="ShortName"/>
        /// as sequence oft strings, depending on whether they are set.
        /// </summary>
        /// <param name="definition">The argument definition.</param>
        /// <returns>A sequence of strings containing all names set in the provided definition.</returns>
        public static IEnumerable<Maybe<string>> GetAllNames(ArgumentDefinition definition)
        {
            return definition.ShortName.AsEnumerable().Concat(definition.LongName.AsEnumerable());
        }

        /// <summary>
        /// Checks whether an instance of <see cref="ArgumentDefinition"/> is of a type
        /// (see <see cref="Type"/>), determining that <see cref="LongName"/> or <see cref="ShortName"/>
        /// is neither <c>null</c> nor empty.
        /// </summary>
        /// <param name="definition">The argument definition to check.</param>
        /// <returns>
        ///   <c>true</c> if <paramref name="definition"/> is of a type that requires an argument name.
        /// </returns>
        public static bool IsNamedForNamedArguments(ArgumentDefinition definition) => definition is SwitchOptionDefinition || definition is ValueOptionDefinition;
    }
}