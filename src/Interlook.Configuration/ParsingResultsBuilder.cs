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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Interlook.Configuration
{
    internal class ParsingResultsBuilder : IParsingResultsBuilder
    {
        private KeyNameBehavior _duplicateKeyBehavior;
        private List<Exception> _exceptions = new List<Exception>();
        private IDictionary<string, string> _internalDict;

        public ParsingResultsBuilder(KeyNameBehavior duplicateKeyBehavior)
        {
            _duplicateKeyBehavior = duplicateKeyBehavior;
            _internalDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Adds a key/value-combination to the config results.
        /// </summary>
        /// <param name="kvp">The key and value to add.</param>
        /// <param name="exceptionTextPrefixFactory">
        /// An optional function, that returns a prefix for the text of the
        /// exception that is thrown, if a duplicate key results in an error.</param>
        public Unit Add(KeyValuePair<string, string> kvp, Func<string> exceptionTextPrefixFactory = null)
            => Add(kvp.Key, kvp.Value, exceptionTextPrefixFactory);

        /// <summary>
        /// Adds a key/value-combination to the config results.
        /// </summary>
        /// <param name="key">The full path key</param>
        /// <param name="value">The corresponding value</param>
        /// <param name="exceptionTextPrefixFactory">
        /// An optional function, that returns a prefix for the text of the
        /// an exception that is thrown in case of an error.</param>
        public Unit Add(string key, string value, Func<string> exceptionTextPrefixFactory = null)
        {
            if (!isValidKeyName(key))
            {
                ErrorArgumentUnknown(key, $"{getPrefix(exceptionTextPrefixFactory)}Unknown argument '{key}'.");
                return Unit.Default;
            }
            if (_internalDict.ContainsKey(key))
            {
                switch (_duplicateKeyBehavior)
                {
                    case KeyNameBehavior.Error:
                        ErrorArgumentAlreadyDefined(key, $"{getPrefix(exceptionTextPrefixFactory)}The key '{key}' is already defined.");
                        break;

                    case KeyNameBehavior.Ignore:
                        return Unit.Default;

                    default:
                        // remaining case is Update, which is done further down the road, so - we're done here
                        break;
                }
            }

            _internalDict[key] = value;
            return Unit.Default;
        }

        public Unit ErrorArgumentRequiresValue(string key, string errorMessage = null)
            => Error(new ValueRequiredException(key, errorMessage));

        public Unit ErrorArgumentAlreadyDefined(string key, string errorMessage = null)
            => Error(new ArgumentAlreadyDefinedException(key, errorMessage));

        public Unit ErrorArgumentUnknown(string key, string errorMessage = null)
            => Error(new ArgumentUnknownException(key, errorMessage));

        public Unit ErrorInvalidParsing(string site, string errorMessage)
            => Error(new ParsingException(site, errorMessage));

        private static string getPrefix(Func<string> exceptionTextPrefixFactory) => exceptionTextPrefixFactory?.Invoke() ?? string.Empty;

        public Unit Error(Exception error)
        {
            if (error != null) _exceptions.Add(error);
            return Unit.Default;
        }

        public IParsingResultsDictionary GetResults()
            => _exceptions.Any()
                ? ParsingResultsDictionary.CreateFailed(_exceptions)
                : ParsingResultsDictionary.CreateSuccessful(_internalDict);

        private bool isValidKeyName(string keyname)
            => Regex.IsMatch(keyname, @"[a-zA-Z0-9_\-]+");
    }
}