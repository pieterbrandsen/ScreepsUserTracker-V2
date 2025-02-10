using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserTrackerShared.Helpers;

namespace UserTracker.Tests.Helper
{
    public static class GetObjectChangesInTick
    {
        private static readonly JsonSerializer _serializer = JsonSerializer.CreateDefault();

        private static void FlattenJson(JToken token, StringBuilder currentPath, IDictionary<string, object> dict)
        {
            switch (token)
            {
                case JObject obj:
                    foreach (var prop in obj.Properties())
                    {
                        if (prop.Name == "PropertiesListDictionary" || prop.Name == "TypeMap" || prop.Name == "UserMap") continue;
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

        public static Dictionary<string, object> GetById(JToken tick, string id)
        {
            var dict = new Dictionary<string, object>();
            if (tick is JObject jTick && jTick.TryGetValue(id, out var idToken))
            {
                FlattenJson(idToken, new StringBuilder(), dict);
            }
            return dict;
        }

        public static Dictionary<string, object> ConvertPropertyListDictionary(PropertiesList propertiesList)
        {
            var dict = new Dictionary<string, object>();
            foreach (var item in propertiesList.NullProperties)
            {
                try
                {
                    dict.Add(item, null);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to set null property {item}: {ex.Message}");
                }
            }

            foreach (var kvp in propertiesList.StringProperties)
            {
                try
                {
                    dict.Add(kvp.Key, kvp.Value);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to set string property {kvp.Key}: {ex.Message}");
                }
            }

            foreach (var kvp in propertiesList.IntegerProperties)
            {
                try
                {
                    dict.Add(kvp.Key, kvp.Value);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to set integer property {kvp.Key}: {ex.Message}");
                }
            }

            foreach (var kvp in propertiesList.BooleanProperties)
            {
                try
                {
                    dict.Add(kvp.Key, kvp.Value);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to set boolean property {kvp.Key}: {ex.Message}");
                }
            }


            return dict;
        }
    }
}
