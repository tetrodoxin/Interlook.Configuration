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
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Interlook.Configuration.IniFile
{
    public class IniFileConfigurationProvider : FileConfigurationProvider
    {
        public static readonly char[] CommentCharacters = new char[] { ';', '#' };

        private readonly IniFileConfigurationSource _iniFileConfigSource;

        public IniFileConfigurationProvider(IniFileConfigurationSource source) : base(source)
        {
            _iniFileConfigSource = source.Clone();
        }

        public override void Load(Stream stream)
        {
            Data =  _iniFileConfigSource
                .HandleResultsErrors(readIniFromStream(stream, _iniFileConfigSource.DuplicateKeyBehavior))
                .ToDictionary();
        }

        private static string getValueFromPossiblyQuotedString(string str)
                    => str.Trim().ToMaybe()
                        .Where(s => s[0] == s[s.Length - 1] && (s[0] == '"' || s[0] == '\''))
                        .Bind(s => s.Substring(1, s.Length - 2).ToMaybe())
                        .GetValue(str);

        private static bool iniLineIsNotEmptyOrComment(string currentLine)
        {
            return !currentLine.IsNothing() && !CommentCharacters.Contains(currentLine.Trim()[0]);
        }

        private static Maybe<string> getHeadingFromIniLine(string currentLine)
                            => currentLine.ToMaybe(line => line[0] == '[')
                        .Bind(line => line.TrimEnd().ToMaybe())
                        .Where(line => line[currentLine.Length - 1] == ']')
                        .Select(sectionLine => sectionLine.Substring(1, sectionLine.Length - 2).Trim() + ConfigurationPath.KeyDelimiter);

        private static IParsingResultsDictionary readIniFromStream(Stream stream, KeyNameBehavior duplicateKeyBehavior)
        {
            var resultBuilder = new ParsingResultsBuilder(duplicateKeyBehavior);
            using (StreamReader streamReader = new StreamReader(stream))
            {
                var currentKeyPrefix = string.Empty;
                int lineCounter = 0;
                while (streamReader.Peek() != -1)
                {
                    string currentLine = streamReader.ReadLine();
                    lineCounter++;

                    if (iniLineIsNotEmptyOrComment(currentLine))
                    {
                        var currentLineIsSectionHeading = getHeadingFromIniLine(currentLine);
                        currentKeyPrefix = currentLineIsSectionHeading.GetValue(currentKeyPrefix);

                        if (currentLineIsSectionHeading.IsNothing())
                        {
                            int assignmentOperatorPos = currentLine.IndexOf('=');
                            if (assignmentOperatorPos == 0)
                            {
                                resultBuilder.ErrorInvalidParsing($"Line #{lineCounter}", $"Line cannot start with assignment operator.{Environment.NewLine}  {currentLine}");
                            }
                            else if(assignmentOperatorPos <0)
                            {
                                resultBuilder.Add(currentKeyPrefix + currentLine.Trim(), string.Empty, () => $"Error in line #{lineCounter}:");
                            }
                            else
                            {
                                string keyFullPathName = $"{currentKeyPrefix}{currentLine.Substring(0, assignmentOperatorPos).Trim()}";
                                string currentValue = assignmentOperatorPos < currentLine.Trim().Length - 1
                                    ? getValueFromPossiblyQuotedString(currentLine.Substring(assignmentOperatorPos + 1).Trim())
                                    : string.Empty;

                                resultBuilder.Add(keyFullPathName, currentValue, () => $"Error in line #{lineCounter}:");
                            }
                        }
                    }
                }

                return resultBuilder.GetResults();
            }
        }
    }
}