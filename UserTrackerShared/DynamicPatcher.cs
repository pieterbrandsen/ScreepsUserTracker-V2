using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

namespace UserTrackerShared
{
    public static class DynamicPatcher
    {
        private enum SimplifiedTypeNameEnum
        {
            Dictionary = 0,
            List = 1,
            Array = 2,
            Other = 3,
            DictionaryValue = 4,
        }
        private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> _propertyCache = new();
        private static readonly ConcurrentDictionary<Type, Type> _dictValueTypeCache = new();

        public static void ApplyPatch(object target, Dictionary<string, object?> changes)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (changes == null || changes.Count == 0) return;
            foreach (var kv in changes)
                ApplyPatch(target, kv.Key, kv.Value);
        }

        public static void ApplyPatch(object target, string path, object? value)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path cannot be empty");

            try
            {
                object current = target;
                var segments = path.Split('.');

                for (int i = 0; i < segments.Length; i++)
                {
                    bool isLast = (i == segments.Length - 1);
                    var rawSegment = segments[i];
                    var type = current.GetType();

                    var (propName, index, dictionaryKeyName) = ParseSegment(rawSegment, type);
                    if (current is IDictionary dict)
                    {
                        var updatedType = current.GetType();
                        var updatedSimplifiedType = GetSimplifiedTypeName(updatedType);
                        HandleDictionary(dict, updatedType, dictionaryKeyName ?? propName, isLast, ref current, value);
                        continue;
                    }
                    var prop = GetPropFromCache(type, propName);
                    HandleProperty(current, prop, prop.GetValue(current), type, null, propName, index, dictionaryKeyName, isLast, ref current, value);
                }
            }
            catch (Exception ex)
            {
                var trace = new StackTrace(ex, true);
                var frame = trace.GetFrames()?.FirstOrDefault(f => f.GetFileLineNumber() > 0);
                var errorLine = frame?.GetFileLineNumber() ?? -1;
                var errorFile = frame?.GetFileName() ?? "Unknown";
                throw new InvalidOperationException($"Error patching object.\nTarget type: '{target?.GetType().FullName}'\nPath: '{path}'\nValue: '{value ?? "null"}'\nException Line: {errorFile}:{errorLine}\nException: {ex.Message}", ex);
            }
        }
        private static PropertyInfo GetPropFromCache(Type type, string propName)
        {
            return _propertyCache
                 .GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                      .ToDictionary(p => p.Name, p => p))
                 .GetValueOrDefault(propName)
                 ?? throw new InvalidOperationException($"Property '{propName}' not found on '{type.Name}'"); ;
        }

        private static void HandleProperty(object current, PropertyInfo prop, object? propValue, Type propType, SimplifiedTypeNameEnum? simplifiedType, string propName, int? index, string? dictionaryKeyName, bool isLast, ref object obj, object? value)
        {
            propType = prop.PropertyType;
            simplifiedType = GetSimplifiedTypeName(propType);

            if (simplifiedType == SimplifiedTypeNameEnum.Array)
            {
                int.TryParse(dictionaryKeyName, out int dictionaryIndex);
                int idx = index.HasValue ? index.Value : dictionaryIndex;
                HandleArrayProperty(current, prop, propValue, propType, propName, idx, isLast, ref obj, value);
            }
            else if (simplifiedType == SimplifiedTypeNameEnum.List)
            {
                int.TryParse(dictionaryKeyName, out int dictionaryIndex);
                int idx = index.HasValue ? index.Value : dictionaryIndex;
                HandleListProperty(current, prop, propValue, propType, propName, idx, isLast, ref obj, value);
            }
            else
            {
                HandleValueProperty(current, prop, propValue, propType, propName, isLast, ref obj, value);
            }

            if (dictionaryKeyName == null)
            {
                return;
                if (simplifiedType == SimplifiedTypeNameEnum.Dictionary) dictionaryKeyName = propName;
                else return;
            }

            if (dictionaryKeyName != null)
            {
                int idx = -1;
                if (obj is IDictionary dict)
                {
                    var updatedType = obj.GetType();
                    var updatedSimplifiedType = GetSimplifiedTypeName(updatedType);
                    HandleDictionary(dict, updatedType, dictionaryKeyName, isLast, ref obj, value);
                    propType = obj.GetType();
                }
                else if (int.TryParse(dictionaryKeyName, out int dictionaryIndex))
                {
                    if (propType.IsArray)
                    {
                        HandleArrayProperty(current, prop, propValue, propType, propName, dictionaryIndex, isLast, ref obj, value);
                    }
                    else
                    {
                        HandleListProperty(current, prop, propValue, propType, propName, dictionaryIndex, isLast, ref obj, value);
                    }
                }
            }
        }

        private static void HandleDictionary(IDictionary dict, Type type, string key, bool isLast, ref object current, object? value)
        {
            if (!dict.Contains(key) || dict[key] == null)
            {
                var valType = _dictValueTypeCache.GetOrAdd(type, t =>
                {
                    var iface = t.GetInterfaces().First(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
                    return iface.GetGenericArguments()[1];
                });
                dict[key] = Activator.CreateInstance(valType)!;
            }
            current = dict[key]!;
        }


        private static void HandleArrayProperty(object current, PropertyInfo? prop, object? propValue, Type? propType, string propName, int idx, bool isLast, ref object obj, object? value)
        {
            var elemType = propType.GetElementType()!;
            var oldArr = (Array?)propValue ?? Array.CreateInstance(elemType, 0);
            int required = idx + 1;
            Array newArr = oldArr.Length < required
                ? ResizeArray(oldArr, elemType, required)
                : oldArr;

            if (isLast)
            {
                newArr.SetValue(ConvertValue(value, elemType), idx);
                prop.SetValue(current, newArr);
            }
            else
            {
                var next = newArr.GetValue(idx) ?? CreateInstanceSafely(elemType);
                newArr.SetValue(next, idx);
                prop.SetValue(current, newArr);
                obj = next;
            }
        }
        private static void HandleListProperty(object current, PropertyInfo? prop, object? propValue, Type? propType, string propName, int idx, bool isLast, ref object obj, object? value)
        {
            var elemType = propType.GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(elemType);
            var list = (IList)(propValue ?? Activator.CreateInstance(listType)!);

            list = list.Count <= idx
              ? ResizeList(list, elemType, idx + 1)
              : list;

            if (isLast)
            {
                list[idx] = ConvertValue(value, elemType);
                prop.SetValue(current, list);
            }
            else
            {
                object? currentElem = list[idx];
                object next = currentElem ?? CreateInstanceSafely(elemType);
                if (currentElem == null)
                {
                    list[idx] = next;
                    prop.SetValue(current, list);
                }
                obj = next;
            }
        }
        private static void HandleValueProperty(object current, PropertyInfo? prop, object? propValue, Type? propType, string propName, bool isLast, ref object obj, object? value)
        {
            if (isLast)
            {
                var targetType = Nullable.GetUnderlyingType(propType) ?? propType;
                prop.SetValue(current, ConvertValue(value, targetType));
            }
            else
            {
                if (propValue == null)
                {
                    propValue = CreateInstanceSafely(propType);
                    prop.SetValue(current, propValue);
                }
                obj = propValue!;
            }
        }

        private static Array ResizeArray(Array oldArr, Type elemType, int required)
        {
            var oldLen = oldArr.Length;
            if (oldLen + 1 < required) throw new IndexOutOfRangeException("Index was not direct one up of current size");
            var newArr = Array.CreateInstance(elemType, required);
            Array.Copy(oldArr, newArr, oldArr.Length);
            return newArr;
        }

        private static IList ResizeList(IList oldList, Type elemType, int required)
        {
            var oldLen = oldList.Count;
            if (oldLen + 1 < required) throw new IndexOutOfRangeException("Index was not direct one up of current size");
            for (int i = 0; i <= oldLen; i++)
                oldList.Add(elemType.IsValueType ? CreateInstanceSafely(elemType) : null!);
            return oldList;
        }

        private static object CreateInstanceSafely(Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t)!;

            if (t == typeof(string))
                return string.Empty;

            if (t.GetConstructor(Type.EmptyTypes) != null && t != typeof(string))
                return Activator.CreateInstance(t)!;
            throw new InvalidOperationException($"Cannot dynamically create an instance of type '{t.FullName}'.");
        }

        private static object? ConvertValue(object? value, Type targetType)
        {
            if (value == null) return null;
            var realType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            return realType.IsInstanceOfType(value) ? value : Convert.ChangeType(value, realType);
        }

        private static SimplifiedTypeNameEnum GetSimplifiedTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(Dictionary<,>))
                    return SimplifiedTypeNameEnum.Dictionary;
                if (genericType == typeof(List<>) || genericType == typeof(IList<>))
                    return SimplifiedTypeNameEnum.List;
            }
            if (type.IsArray)
                return SimplifiedTypeNameEnum.Array;
            return SimplifiedTypeNameEnum.Other;
        }

        private static (string prop, int? index, string? dictionaryKey) ParseSegment(string segment, Type currentType)
        {
            if (segment.EndsWith("]") && segment.Contains("["))
            {
                var open = segment.IndexOf('[');
                var close = segment.IndexOf(']');
                var name = segment.Substring(0, open);
                var idx = int.Parse(segment.Substring(open + 1, close - open - 1));
                return (MapName(name, currentType), idx, null);
            }
            if (segment.Contains(">"))
            {
                var open = segment.IndexOf('>');
                var name = segment.Substring(0, open);
                var close = segment.Length;
                var key = segment.Substring(open + 1, close - open - 1);
                return (MapName(name, currentType), null, key);
            }
            return (MapName(segment, currentType), null, null);
        }

        private static string MapName(string jsonProp, Type type)
        {
            switch (jsonProp)
            {
                case "_id": return "Id";
                case "_updated": return "Updated";
                case "effect": return "EffectType";
                default:
                    if (type.GetProperty(jsonProp) != null)
                        return jsonProp;
                    return char.ToUpperInvariant(jsonProp[0]) + jsonProp.Substring(1);
            }
        }
    }
}
