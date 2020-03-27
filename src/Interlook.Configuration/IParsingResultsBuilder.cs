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

namespace Interlook.Configuration
{
    /// <summary>
    /// Interface for objects that build <see cref="IParsingResultsDictionary"/> instances.
    /// Supports adding of actual key/value data or errors.
    /// </summary>
    public interface IParsingResultsBuilder
    {
        /// <summary>
        /// Adds the specified key/value data.
        /// </summary>
        /// <param name="kvp">The key/value pair.</param>
        /// <param name="exceptionTextPrefixFactory">An optional function, that will return a prefix for a potential error message.</param>
        /// <returns>Just <see cref="Unit"/>, which is the functional equivalent to <c>void</c></returns>
        Unit Add(KeyValuePair<string, string> kvp, Func<string> exceptionTextPrefixFactory = null);

        /// <summary>
        /// Adds the specified key/value data.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="exceptionTextPrefixFactory">An optional function, that will return a prefix for a potential error message.</param>
        /// <returns>Just <see cref="Unit"/>, which is the functional equivalent to <c>void</c></returns>
        Unit Add(string key, string value, Func<string> exceptionTextPrefixFactory = null);

        Unit ErrorArgumentRequiresValue(string key, string errorMessage = null);

        Unit ErrorArgumentAlreadyDefined(string key, string errorMessage = null);

        Unit ErrorArgumentUnknown(string key, string errorMessage = null);

        Unit ErrorInvalidParsing(string site, string errorMessage);

        Unit Error(Exception error);

        IParsingResultsDictionary GetResults();
    }
}