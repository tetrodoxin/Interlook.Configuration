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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Interlook.Configuration
{
    internal class ParsingResultsDictionary : ReadOnlyDictionary<string, string>, IParsingResultsDictionary
    {
        public IEnumerable<Exception> Errors { get; }

        public bool HasErrors { get; }

        /// <summary>
        /// Creates a result dictionary with actual results. Errors won't be contained.
        /// </summary>
        /// <param name="results">The parsing results.</param>
        /// <returns>A new instance of <see cref="ParsingResultsDictionary"/> containing
        /// the results from <paramref name="results"/>.</returns>
        public static ParsingResultsDictionary CreateSuccessful(IDictionary<string, string> results)
            => new ParsingResultsDictionary(results, new List<Exception>());

        /// <summary>
        /// Creates a result dictionary without actual results, because only errors are contained.
        /// </summary>
        /// <param name="errors">The errors.</param>
        /// <returns>A new instance of <see cref="ParsingResultsDictionary"/>. If <see cref="HasErrors"/>
        /// is <c>true</c> depends on <paramref name="errors"/> really contains errors.</returns>
        public static ParsingResultsDictionary CreateFailed(IList<Exception> errors)
            => new ParsingResultsDictionary(new Dictionary<string, string>(), errors);

        private ParsingResultsDictionary(IDictionary<string, string> results, IList<Exception> errors)
            : base(results)
        {
            HasErrors = errors.Any();
            Errors = errors;
        }
    }
}