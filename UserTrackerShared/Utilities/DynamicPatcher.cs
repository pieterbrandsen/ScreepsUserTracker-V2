using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

namespace UserTrackerShared.Utilities
{
    public static class DynamicPatcher
    {
        private enum SimplifiedTypeName
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
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(changes);
            foreach (var kv in changes)
                ApplyPatch(target, kv.Key, kv.Value);
        }

        public static void ApplyPatch(object target, string path, object? value)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(path);

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
                        HandleDictionary(dict, updatedType, dictionaryKeyName ?? propName, ref current);
                        continue;
                    }
                    var prop = GetPropFromCache(type, propName);
                    HandleProperty(current, prop, prop.GetValue(current), propName, index, dictionaryKeyName, isLast, ref current, value);
                }
            }
            catch (Exception ex)
            {
                var trace = new StackTrace(ex, true);
                var frame = trace.GetFrames()?.FirstOrDefault(f => f.GetFileLineNumber() > 0);
                var errorLine = frame?.GetFileLineNumber() ?? -1;
                var errorFile = frame?.GetFileName() ?? "Unknown";
                throw new InvalidOperationException($"Error patching object.\nTarget type: '{target.GetType().FullName}'\nPath: '{path}'\nValue: '{value ?? "null"}'\nException Line: {errorFile}:{errorLine}\nException: {ex.Message}", ex);
            }
        }
        private static PropertyInfo GetPropFromCache(Type type, string propName)
        {
            return _propertyCache
                 .GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                      .ToDictionary(p => p.Name, p => p))
                 .GetValueOrDefault(propName)
                 ?? throw new InvalidOperationException($"Property '{propName}' not found on '{type.Name}'");
        }

        private static void HandleProperty(object current, PropertyInfo prop, object? propValue, string propName, int? index, string? dictionaryKeyName, bool isLast, ref object obj, object? value)
        {
            var propType = prop.PropertyType;
            var simplifiedType = GetSimplifiedTypeName(propType);

            if (simplifiedType == SimplifiedTypeName.Array)
            {
                if (!int.TryParse(dictionaryKeyName, out int dictionaryIndex) && !index.HasValue)
                    throw new FormatException(string.Format("Invalid index: '{0}' is not a valid integer.", dictionaryKeyName));
                int idx = index.HasValue ? index.Value : dictionaryIndex;
                HandleArrayProperty(current, prop, propValue, propType, idx, ref obj, value);
                return;
            }
            else if (simplifiedType == SimplifiedTypeName.List)
            {
                if (!int.TryParse(dictionaryKeyName, out int dictionaryIndex) && !index.HasValue)
                    throw new FormatException(string.Format("Invalid index: '{0}' is not a valid integer.", dictionaryKeyName));
                int idx = index.HasValue ? index.Value : dictionaryIndex;
                HandleListProperty(current, prop, propValue, propType, idx, ref obj, value);
                return;
            }
            else
            {
                HandleValueProperty(current, prop, propValue, propType, isLast, ref obj, value);
            }


            if (dictionaryKeyName != null)
            {
                if (obj is IDictionary dict)
                {
                    var updatedType = obj.GetType();
                    HandleDictionary(dict, updatedType, dictionaryKeyName, ref obj);
                }
                else if (int.TryParse(dictionaryKeyName, out int dictionaryIndex))
                {
                    if (propType.IsArray)
                    {
                        HandleArrayProperty(current, prop, propValue, propType, dictionaryIndex, ref obj, value);
                    }
                    else
                    {
                        HandleListProperty(current, prop, propValue, propType, dictionaryIndex, ref obj, value);
                    }
                }
            }
        }

        private static void HandleDictionary(IDictionary dict, Type type, string key, ref object current)
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


        private static void HandleArrayProperty(object current, PropertyInfo prop, object? propValue, Type propType, int idx, ref object obj, object? value)
        {
            var elemType = propType.GetElementType()!;
            var oldArr = (Array?)propValue ?? Array.CreateInstance(elemType, 0);
            int required = idx + 1;
            Array newArr = oldArr.Length < required
                ? ResizeArray(oldArr, elemType, required)
                : oldArr;

            var next = newArr.GetValue(idx) ?? CreateInstanceSafely(elemType);
            newArr.SetValue(next, idx);
            prop.SetValue(current, newArr);
            obj = next;
        }
        private static void HandleListProperty(object current, PropertyInfo prop, object? propValue, Type propType, int idx, ref object obj, object? value)
        {
            var elemType = propType.GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(elemType);
            var list = (IList)(propValue ?? Activator.CreateInstance(listType)!);

            list = list.Count <= idx
              ? ResizeList(list, elemType, idx + 1)
              : list;

            object? currentElem = list[idx];
            object next = currentElem ?? CreateInstanceSafely(elemType);
            if (currentElem == null)
            {
                list[idx] = next;
                prop.SetValue(current, list);
            }
            obj = next;
        }
        private static void HandleValueProperty(object current, PropertyInfo prop, object? propValue, Type propType, bool isLast, ref object obj, object? value)
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
            if (oldLen + 1 < required) throw new ArgumentOutOfRangeException(nameof(required), "Index must be exactly one greater than the current size.");
            var newArr = Array.CreateInstance(elemType, required);
            Array.Copy(oldArr, newArr, oldArr.Length);
            return newArr;
        }

        private static IList ResizeList(IList oldList, Type elemType, int required)
        {
            var oldLen = oldList.Count;
            if (oldLen + 1 < required) throw new ArgumentOutOfRangeException(nameof(required), "Index must be exactly one greater than the current size.");
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

        private static SimplifiedTypeName GetSimplifiedTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(Dictionary<,>))
                    return SimplifiedTypeName.Dictionary;
                if (genericType == typeof(List<>) || genericType == typeof(IList<>))
                    return SimplifiedTypeName.List;
            }
            if (type.IsArray)
                return SimplifiedTypeName.Array;
            return SimplifiedTypeName.Other;
        }

        private static (string prop, int? index, string? dictionaryKey) ParseSegment(string segment, Type currentType)
        {
            if (segment.EndsWith(']') && segment.Contains('['))
            {
                var open = segment.IndexOf('[');
                var close = segment.IndexOf(']');
                var name = segment.Substring(0, open);
                var idx = int.Parse(segment.Substring(open + 1, close - open - 1));
                return (MapName(name, currentType), idx, null);
            }
            if (segment.Contains('>'))
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
