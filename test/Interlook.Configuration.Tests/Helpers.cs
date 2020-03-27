using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Interlook.Configuration.Tests
{
    public static class Helpers
    {
        public static IDictionary<string, string> GetAllKeysAndValuesFromProvider(this IConfigurationProvider provider)
        {
            return getChildKeysFromProvider(provider, null)
                .Select(key => getValueFromProvider(provider, key))
                .Where(keyValue => keyValue.HasValue)
                .ToDictionary(p => p.Key, p => p.Value);
        }

        private static IList<string> getChildKeysFromProvider(IConfigurationProvider provider, string path)
        {
            var result = new List<string>();
            var keys = provider.GetChildKeys(new string[0], path).ToList();
            foreach (var subkey in keys)
            {
                var subResults = getChildKeysFromProvider(provider, subkey);
                if (subResults.Any())
                    result.AddRange(subResults);
                else
                    result.Add(string.IsNullOrEmpty(path) ? subkey : $"{path}{ConfigurationPath.KeyDelimiter}{subkey}");
            }

            return result.Distinct().ToList();
        }

        private static (string Key, bool HasValue, string Value) getValueFromProvider(IConfigurationProvider provider, string key)
            => provider.TryGet(key, out string val) ? (key, true, val) : ((string Key, bool HasValue, string Value))(key, false, null);
    }
}