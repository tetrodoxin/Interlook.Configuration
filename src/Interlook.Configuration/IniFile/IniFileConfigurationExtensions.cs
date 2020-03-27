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
using Microsoft.Extensions.FileProviders;
using System;

namespace Interlook.Configuration.IniFile
{
    public static class IniFileConfigurationExtensions
    {
        /// <summary>
        /// Adds a non optional, physical INI-file as configuration source to <paramref name="builder"/>
        /// which will not be monitored for changes and will ignore multiple assignments of the same key.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="provider">The <see cref="IFileProvider"/> to use to access the file.</param>
        /// <param name="path">Path relative to the base path stored in
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="optional">Whether the file is optional and thus would not cause exceptions.</param>
        /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddIniFile(this IConfigurationBuilder builder, string path)
        {
            return AddIniFile(builder, provider: null, path: path, optional: false, reloadOnChange: false, duplicateKeyBehavior: KeyNameBehavior.Ignore);
        }

        /// <summary>
        /// Adds a non optional INI-file as configuration source to <paramref name="builder"/>
        /// which will not be monitored for changes and will ignore multiple assignments of the same key.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="provider">The <see cref="IFileProvider"/> to use to access the file.</param>
        /// <param name="path">Path relative to the base path stored in
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddIniFile(this IConfigurationBuilder builder, IFileProvider provider, string path) 
            => AddIniFile(builder, provider, path, optional: false, reloadOnChange: false, duplicateKeyBehavior: KeyNameBehavior.Ignore);

        /// <summary>
        /// Adds a physical INI-file configuration source to <paramref name="builder"/>,
        /// which will not be monitored for changes and will ignore multiple assignments of the same key.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">Path relative to the base path stored in
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="optional">Whether the file is optional and thus would not cause exceptions.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddIniFile(this IConfigurationBuilder builder, string path, bool optional)
            => AddIniFile(builder, provider: null, path, optional, reloadOnChange: false, duplicateKeyBehavior: KeyNameBehavior.Ignore);

        /// <summary>
        /// Adds an INI-file configuration source to <paramref name="builder"/>,
        /// which will not be monitored for changes and will ignore multiple assignments of the same key.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="provider">The <see cref="IFileProvider"/> to use to access the file.</param>
        /// <param name="path">Path relative to the base path stored in
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="optional">Whether the file is optional and thus would not cause exceptions.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddIniFile(this IConfigurationBuilder builder, IFileProvider provider, string path, bool optional)
            => AddIniFile(builder, provider, path, optional, reloadOnChange: false, duplicateKeyBehavior: KeyNameBehavior.Ignore);

        /// <summary>
        /// Adds a physical, non optional INI-file as configuration source to <paramref name="builder"/>,
        /// which will not be monitored for changes.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">Path relative to the base path stored in
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="duplicateKeyBehavior">Determines, what happens if key names are parsed multiple times within the same section.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddIniFile(this IConfigurationBuilder builder, string path, KeyNameBehavior duplicateKeyBehavior)
            => AddIniFile(builder, provider: null, path, optional: false, reloadOnChange: false, duplicateKeyBehavior);

        /// <summary>
        /// Adds a non optional INI-file as configuration source to <paramref name="builder"/>,
        /// which will not be monitored for changes.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="provider">The <see cref="IFileProvider"/> to use to access the file.</param>
        /// <param name="path">Path relative to the base path stored in
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="duplicateKeyBehavior">Determines, what happens if key names are parsed multiple times within the same section.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddIniFile(this IConfigurationBuilder builder, IFileProvider provider, string path, KeyNameBehavior duplicateKeyBehavior)
            => AddIniFile(builder, provider, path, optional: false, reloadOnChange: false, duplicateKeyBehavior);

        /// <summary>
        /// Adds a physical INI-file configuration source to <paramref name="builder"/>
        /// which will ignore multiple assignments of the same key and
        /// using <see cref="AppContext.BaseDirectory"/> as base directory.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">Path relative to the base path stored in
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="optional">Whether the file is optional and thus would not cause exceptions.</param>
        /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddIniFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
            => AddIniFile(builder, provider: null, path, optional, reloadOnChange, duplicateKeyBehavior: KeyNameBehavior.Ignore);

        /// <summary>
        /// Adds an INI-file configuration source to <paramref name="builder"/>
        /// which will ignore multiple assignments of the same key.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="provider">The <see cref="IFileProvider"/> to use to access the file.</param>
        /// <param name="path">Path relative to the base path stored in
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="optional">Whether the file is optional and thus would not cause exceptions.</param>
        /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddIniFile(this IConfigurationBuilder builder, IFileProvider provider, string path, bool optional, bool reloadOnChange)
            => AddIniFile(builder, provider, path, optional, reloadOnChange, duplicateKeyBehavior: KeyNameBehavior.Ignore);

        /// <summary>
        /// Adds a physical INI-file configuration source to <paramref name="builder"/>,
        /// which will not be monitored for changes and using <see cref="AppContext.BaseDirectory"/> as base directory.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">Path relative to the base path stored in
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="optional">Whether the file is optional and thus would not cause exceptions.</param>
        /// <param name="duplicateKeyBehavior">Determines, what happens if key names are parsed multiple times within the same section.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddIniFile(this IConfigurationBuilder builder, string path, bool optional, KeyNameBehavior duplicateKeyBehavior)
            => AddIniFile(builder, provider: null, path, optional, reloadOnChange: false, duplicateKeyBehavior);

        /// <summary>
        /// Adds an INI-file configuration source to <paramref name="builder"/>,
        /// which will not be monitored for changes.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="provider">The <see cref="IFileProvider"/> to use to access the file.</param>
        /// <param name="path">Path relative to the base path stored in
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="optional">Whether the file is optional and thus would not cause exceptions.</param>
        /// <param name="duplicateKeyBehavior">Determines, what happens if key names are parsed multiple times within the same section.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddIniFile(this IConfigurationBuilder builder, IFileProvider provider, string path, bool optional, KeyNameBehavior duplicateKeyBehavior)
            => AddIniFile(builder, provider, path, optional, reloadOnChange: false, duplicateKeyBehavior);

        /// <summary>
        /// Adds a physical INI-file configuration source to <paramref name="builder"/>,
        /// using <see cref="AppContext.BaseDirectory"/> as base directory.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">Path relative to the base path stored in
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="optional">Whether the file is optional and thus would not cause exceptions.</param>
        /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
        /// <param name="duplicateKeyBehavior">Determines, what happens if key names are parsed multiple times within the same section.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddIniFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange, KeyNameBehavior duplicateKeyBehavior)
            => AddIniFile(builder, provider: null, path, optional, reloadOnChange: reloadOnChange, duplicateKeyBehavior);

        /// <summary>
        /// Adds an INI-file configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="provider">The <see cref="IFileProvider"/> to use to access the file.</param>
        /// <param name="path">Path relative to the base path stored in
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="optional">Whether the file is optional and thus would not cause exceptions.</param>
        /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
        /// <param name="duplicateKeyBehavior">Determines, what happens if key names are parsed multiple times within the same section.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddIniFile(this IConfigurationBuilder builder, IFileProvider provider, string path, bool optional, bool reloadOnChange, KeyNameBehavior duplicateKeyBehavior)
        {
            return AddIniFile(builder, new IniFileConfigurationSource(path)
            {
                FileProvider = provider,
                Optional = optional,
                ReloadOnChange = reloadOnChange,
                DuplicateKeyBehavior = duplicateKeyBehavior
            });
        }

        /// <summary>
        /// Adds an INI-file configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="newSource">An instance of <see cref="IniFileConfigurationSource"/> that is used to provide an <see cref="IniFileConfigurationProvider"/></param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddIniFile(this IConfigurationBuilder builder, IniFileConfigurationSource newSource)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.Add(newSource);
        }
    }
}