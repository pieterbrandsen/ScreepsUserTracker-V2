using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserTrackerShared.Helpers;

namespace UserTrackerShared.Helpers.Tests
{
    public static class GetObjectChangesInTick
    {
        private static void FlattenJson(JToken token, StringBuilder currentPath, IDictionary<string, object?> dict)
        {
            switch (token)
            {
                case JObject obj:
                    foreach (var prop in obj.Properties())
                    {
                        int initialLen = currentPath.Length;
                        currentPath.Append($"{prop.Name}.");
                        FlattenJson(prop.Value, currentPath, dict);
                        currentPath.Length = initialLen; // Reset path
                    }
                    break;

                case JArray array:
                    for (int i = 0; i < array.Count; i++)
                    {
                        int initialLen = currentPath.Length;
                        currentPath.Append($"{i}.");
                        FlattenJson(array[i], currentPath, dict);
                        currentPath.Length = initialLen; // Reset path
                    }
                    break;

                case JValue jValue:
                    if (currentPath.Length > 0)
                        currentPath.Length--; // Remove trailing "."
                    dict[currentPath.ToString()] = jValue.Value; // Directly use Value
                    break;
            }
        }

        public static Dictionary<string, object?> GetById(JToken tick, string id)
        {
            var dict = new Dictionary<string, object?>();
            if (tick is JObject jTick && jTick.TryGetValue(id, out var idToken))
            {
                FlattenJson(idToken, new StringBuilder(), dict);
            }
            return dict;
        }
    }
}
