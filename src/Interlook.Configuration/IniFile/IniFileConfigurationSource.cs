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

namespace Interlook.Configuration.IniFile
{
    public class IniFileConfigurationSource : FileConfigurationSource, IUsingParsingResults
    {
        /// <summary>
        /// This value determines what happens, if the parser hits
        /// a key name multiple times in the same section.
        /// </summary>
        public KeyNameBehavior DuplicateKeyBehavior { get; set; } = KeyNameBehavior.Ignore;

        public ParsingResultsErrorBehavior ParsingErrorBehavior { get; set; } = ParsingResultsErrorBehavior.Aggregate;

        /// <summary>
        /// A function, which is to be called to handle parsing errors
        /// instead of raising the exceptions.
        /// </summary>
        public Action<IEnumerable<Exception>> ErrorFunction { get; private set; }

        ParsingResultsErrorBehavior IUsingParsingResults.ParsingErrorBehavior => ParsingErrorBehavior;

        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            EnsureDefaults(builder);
            return new IniFileConfigurationProvider(this);
        }

        public IniFileConfigurationSource(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new System.ArgumentException("File path must not be null or empty.", nameof(path));
            }

            Path = path;
        }

        /// <summary>
        /// Sets the error handling function.
        /// </summary>
        /// <param name="errorFunction">The error function to use.</param>
        /// <returns>A copy of the current instance with the error function set.</returns>
        public IniFileConfigurationSource WithErrorHandlingFunction(Action<IEnumerable<Exception>> errorFunction)
        {
            var result = Clone();
            result.ErrorFunction = errorFunction;
            return result;
        }

        /// <summary>
        /// Clones this instance by creating a deep copy.
        /// </summary>
        /// <returns>A deep copy of the current instance.</returns>
        public IniFileConfigurationSource Clone()
        {
            return new IniFileConfigurationSource(Path)
            {
                FileProvider = FileProvider,
                Optional = Optional,
                DuplicateKeyBehavior = DuplicateKeyBehavior,
                OnLoadException = OnLoadException,
                ParsingErrorBehavior = ParsingErrorBehavior,
                ReloadDelay = ReloadDelay,
                ReloadOnChange = ReloadOnChange,
                ErrorFunction = ErrorFunction
            };
        }
    }
}