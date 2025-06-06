using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UserTrackerShared
{
    public interface IDynamicPatchDiagnostics
    {
        void Trace(string message);
        void Error(string path, string segment, Exception ex);
    }

    public static class DynamicPatcher
    {
        public static void ApplyPatch(object target, string path, object? value, IDynamicPatchDiagnostics? diagnostics = null)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path cannot be empty");

            var segments = path.Split('.');
            var current = target;

            for (int i = 0; i < segments.Length; i++)
            {
                var isLast = i == segments.Length - 1;
                var seg = segments[i];

                (string propName, int? index) = ParseSegment(seg);
                var prop = current.GetType().GetProperty(propName);
                if (prop == null)
                    throw new InvalidOperationException($"Property '{propName}' not found on type '{current.GetType().Name}'");

                var propValue = prop.GetValue(current);

                // If index is used, we deal with list or array
                if (index.HasValue)
                {
                    var list = EnsureList(propValue, prop.PropertyType, index.Value, diagnostics);
                    if (propValue == null || !ReferenceEquals(list, propValue))
                        prop.SetValue(current, list);

                    EnsureListItemExists(list, index.Value, diagnostics);

                    if (isLast)
                    {
                        list[index.Value] = value;
                        return;
                    }
                    else
                    {
                        current = list[index.Value] ??= Activator.CreateInstance(list.GetType().GetElementType() ?? list.GetType().GetGenericArguments()[0])!;
                    }
                }
                else
                {
                    if (isLast)
                    {
                        prop.SetValue(current, value);
                        return;
                    }
                    else
                    {
                        if (propValue == null)
                        {
                            propValue = Activator.CreateInstance(prop.PropertyType);
                            prop.SetValue(current, propValue);
                        }
                        current = propValue;
                    }
                }

                diagnostics?.Trace($"Traversed: {seg}");
            }
        }

        private static IList EnsureList(object? value, Type propType, int requiredIndex, IDynamicPatchDiagnostics? diagnostics)
        {
            IList list;

            var isArray = propType.IsArray;
            if (isArray)
            {
                var elemType = propType.GetElementType()!;
                if (value == null)
                {
                    var newArr = Array.CreateInstance(elemType, requiredIndex + 1);
                    diagnostics?.Trace($"Created new array of {elemType.Name} with size {requiredIndex + 1}");
                    return newArr;
                }
                else
                {
                    var oldArr = (Array)value;
                    if (oldArr.Length > requiredIndex) return oldArr;
                    var newArr = Array.CreateInstance(elemType, requiredIndex + 1);
                    Array.Copy(oldArr, newArr, oldArr.Length);
                    diagnostics?.Trace($"Resized array to size {requiredIndex + 1}");
                    return newArr;
                }
            }
            else if (typeof(IList).IsAssignableFrom(propType))
            {
                if (value == null)
                {
                    list = (IList)Activator.CreateInstance(propType)!;
                    diagnostics?.Trace($"Created new list: {propType.Name}");
                }
                else
                {
                    list = (IList)value;
                }

                while (list.Count <= requiredIndex)
                {
                    var elemType = list.GetType().IsGenericType ? list.GetType().GetGenericArguments()[0] : typeof(object);
                    list.Add(Activator.CreateInstance(elemType));
                }
                return list;
            }
            throw new NotSupportedException($"Type {propType.Name} is not supported as a list or array");
        }

        private static void EnsureListItemExists(IList list, int index, IDynamicPatchDiagnostics? diagnostics)
        {
            if (list is Array arr) return; // Already resized
            while (list.Count <= index)
            {
                var elemType = list.GetType().GetGenericArguments()[0];
                list.Add(Activator.CreateInstance(elemType));
                diagnostics?.Trace($"Extended list with new {elemType.Name} at index {index}");
            }
        }

        private static (string prop, int? index) ParseSegment(string segment)
        {
            var open = segment.IndexOf('[');
            if (open < 0) return (segment, null);
            var close = segment.IndexOf(']');
            var prop = segment.Substring(0, open);
            var indexStr = segment.Substring(open + 1, close - open - 1);
            if (!int.TryParse(indexStr, out int index)) throw new FormatException($"Invalid index: {segment}");
            return (prop, index);
        }
    }
}