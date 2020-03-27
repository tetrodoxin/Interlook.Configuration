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

namespace Interlook.Configuration
{
    /// <summary>
    /// Interface for objects implementing <see cref="IConfigurationProvider"/>,
    /// which support special error handling behavior.
    /// <seealso cref="ParsingResultsErrorBehavior"/>
    /// </summary>
    public interface IUsingParsingResults
    {
        /// <summary>
        /// Determines the behavior of the parser,
        /// when an error occured.
        /// </summary>
        ParsingResultsErrorBehavior ParsingErrorBehavior { get; }

        /// <summary>
        /// A function, which is to be called to handle parsing errors
        /// instead of raising the exceptions.
        /// <para>If <c>null</c>, the <see cref="UsingParsingResultsExtensions.HandleResultsErrors(IUsingParsingResults, IParsingResultsDictionary)"/>
        /// extension method will throw exceptions, regarding to the behavior defined in <see cref="ParsingErrorBehavior"/></para>
        /// </summary>
        Action<IEnumerable<Exception>> ErrorFunction { get; }
    }

    /// <summary>
    /// Definitions, how to handle occuring errors.
    /// </summary>
    public enum ParsingResultsErrorBehavior
    {
        /// <summary>
        /// Nothing is done for error handling. Not Recommended.
        /// </summary>
        None = 0,

        /// <summary>
        /// An aggregating error is created at the end of parsing process,
        /// containing all occured errors. If no errors occured,
        /// no aggregating error is created.
        /// </summary>
        Aggregate = 2,

        /// <summary>
        /// An error is created as soon as something fails.
        /// </summary>
        OnFirstError = 5
    }

    internal static class UsingParsingResultsExtensions
    {
        /// <summary>
        /// Checks and <see cref="IParsingResultsDictionary"/> for errors
        /// and acts according to settings <see cref="IUsingParsingResults.ParsingErrorBehavior"/>
        /// and if <see cref="IUsingParsingResults.ErrorFunction"/> is set,
        /// by either throwing an exception if necessary and expected
        /// or invoking <see cref="IUsingParsingResults.ErrorFunction"/> respectively.
        /// </summary>
        /// <param name="resultsParserUtilizer">The results parser utilizer.</param>
        /// <param name="resultsDictionary">The results dictionary.</param>
        /// <returns>The <see cref="IParsingResultsDictionary"/> provided as <paramref name="resultsDictionary"/></returns>
        public static IParsingResultsDictionary HandleResultsErrors(this IUsingParsingResults resultsParserUtilizer, IParsingResultsDictionary resultsDictionary)
        {
            if (!resultsDictionary.HasErrors)
                return resultsDictionary;

            if (resultsParserUtilizer.ErrorFunction != null)
                return handleErrorsWithFunction(resultsParserUtilizer.ParsingErrorBehavior, resultsDictionary, resultsParserUtilizer.ErrorFunction);

            switch (resultsParserUtilizer.ParsingErrorBehavior)
            {
                case ParsingResultsErrorBehavior.None:
                    return resultsDictionary;

                case ParsingResultsErrorBehavior.OnFirstError:
                    throw resultsDictionary.Errors.First();

                default:
                case ParsingResultsErrorBehavior.Aggregate:
                    throw new AggregateException(resultsDictionary.Errors);
            }
        }

        private static IParsingResultsDictionary handleErrorsWithFunction(ParsingResultsErrorBehavior errorBehavior, IParsingResultsDictionary resultsDictionary, Action<IEnumerable<Exception>> errorFunction)
        {
            switch (errorBehavior)
            {
                case ParsingResultsErrorBehavior.OnFirstError:
                    errorFunction(resultsDictionary.Errors.First().AsEnumerable().ToList());
                    break;

                case ParsingResultsErrorBehavior.Aggregate:
                    errorFunction(resultsDictionary.Errors.ToList());
                    break;
            }

            return ParsingResultsDictionary.CreateFailed(resultsDictionary.Errors.ToList());
        }
    }
}