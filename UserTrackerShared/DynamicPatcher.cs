using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UserTrackerShared
{
    public static class DynamicPatcher
    {
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

            object current = target;
            var segments = path.Split('.');

            for (int i = 0; i < segments.Length; i++)
            {
                bool isLast = (i == segments.Length - 1);
                var rawSegment = segments[i];

                if (current is IDictionary dict)
                {
                    // Use raw key to preserve case and avoid mapping
                    HandleDictionary(dict, rawSegment, isLast, ref current, value);
                    continue;
                }

                var (propName, index) = ParseSegment(rawSegment, current.GetType());
                HandleProperty(current, propName, index, isLast, ref current, value);
            }
        }

        private static void HandleDictionary(IDictionary dict, string key, bool isLast, ref object current, object? value)
        {
            if (isLast)
            {
                dict[key] = value;
                current = dict[key]!;
                return;
            }

            if (!dict.Contains(key) || dict[key] == null)
            {
                var valType = _dictValueTypeCache.GetOrAdd(dict.GetType(), t =>
                {
                    var iface = t.GetInterfaces().First(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
                    return iface.GetGenericArguments()[1];
                });
                dict[key] = Activator.CreateInstance(valType)!;
            }
            current = dict[key]!;
        }

        private static void HandleProperty(object current, string propName, int? index, bool isLast, ref object obj, object? value)
        {
            var type = current.GetType();
            var prop = _propertyCache
                .GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                     .ToDictionary(p => p.Name, p => p))
                .GetValueOrDefault(propName)
                ?? throw new InvalidOperationException($"Property '{propName}' not found on '{type.Name}'");

            var propValue = prop.GetValue(current);
            var propType = prop.PropertyType;

            if (index.HasValue)
            {
                int idx = index.Value;
                if (propType.IsArray)
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
                else
                {
                    var elemType = propType.GetGenericArguments()[0];
                    var listType = typeof(List<>).MakeGenericType(elemType);
                    var list = (IList)(propValue ?? Activator.CreateInstance(listType)!);

                    while (list.Count <= idx)
                        list.Add(elemType.IsValueType ? CreateInstanceSafely(elemType) : null!);

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
            }
            else
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
        }

        private static Array ResizeArray(Array oldArr, Type elemType, int required)
        {
            int newLen = Math.Max(oldArr.Length * 2, required);
            var newArr = Array.CreateInstance(elemType, newLen);
            Array.Copy(oldArr, newArr, oldArr.Length);
            return newArr;
        }

        private static object CreateInstanceSafely(Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t)!;
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

        private static (string prop, int? index) ParseSegment(string segment, Type currentType)
        {
            if (segment.EndsWith("]") && segment.Contains("["))
            {
                var open = segment.IndexOf('[');
                var close = segment.IndexOf(']');
                var name = segment.Substring(0, open);
                var idx = int.Parse(segment.Substring(open + 1, close - open - 1));
                return (MapName(name, currentType), idx);
            }
            return (MapName(segment, currentType), null);
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
