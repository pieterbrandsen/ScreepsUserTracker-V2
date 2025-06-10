using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTrackerShared.Helpers
{
    public static class JsonHelper
    {
        private static bool AllKeysFollowEachOther(IEnumerable<JProperty> keys)
        {
            var indexes = keys
                .Select(p =>
                {
                    bool ok = int.TryParse(p.Name, out int value);
                    return (ok, value);
                })
                .Where(t => t.ok)
                .Select(t => t.value)
                .ToList();

            if (indexes.Count <= 1)
                return true;

            indexes.Sort();
            for (int i = 1; i < indexes.Count; i++)
            {
                if (indexes[i] != indexes[i - 1] + 1)
                    return false;
            }

            return true;
        }
        public static void FlattenJson(JToken token, StringBuilder currentPath, IDictionary<string, object?> dict)
        {
            switch (token)
            {
                case JObject obj:
                    var properties = obj.Properties();
                    foreach (var prop in properties)
                    {
                        int initialLen = currentPath.Length;
                        if (int.TryParse(prop.Name, out int i))
                        {
                            currentPath.Append($">{i}");
                        }
                        else
                        {
                            if (currentPath.Length > 0)
                                currentPath.Append('.');
                            currentPath.Append(prop.Name);
                        }
                        FlattenJson(prop.Value, currentPath, dict);
                        currentPath.Length = initialLen; // Reset path
                    }
                    break;

                case JArray array:
                    for (int i = 0; i < array.Count; i++)
                    {
                        int initialLen = currentPath.Length;
                        currentPath.Append($"[{i}]");
                        FlattenJson(array[i], currentPath, dict);
                        currentPath.Length = initialLen; // Reset path
                    }
                    break;

                case JValue jValue:
                    dict[currentPath.ToString()] = jValue.Value;
                    break;
            }
        }
    }
}
