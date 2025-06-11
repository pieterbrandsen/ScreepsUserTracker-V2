using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTracker.Tests.Helper
{
    public static class HistoryDictionaryHelper
    {
        private static readonly Dictionary<string, string> PropertyNameMapping = new()
        {
            { "_id", "Id" },
            { "_updated", "Updated" },
            { "effect", "EffectType" }
        };
        public static string MapPropertyIfFound(string input)
        {
            var parts = input.Split('.');
            for (int i = 0; i < parts.Length; i++)
            {
                if (PropertyNameMapping.TryGetValue(parts[i], out var mappedName))
                {
                    parts[i] = mappedName;
                }
            }
            return string.Join(".", parts);
        }
        public static string CapitalizeLetters(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            string[] parts = input.Split('.');
            for (int i = 0; i < parts.Length; i++)
            {
                if (!string.IsNullOrEmpty(parts[i]))
                {
                    parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
                }
            }

            return string.Join(".", parts);
        }
        public static string CapitalizeLettersExceptLast(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            string[] parts = input.Split('.');
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (!string.IsNullOrEmpty(parts[i]))
                {
                    parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
                }
            }

            return string.Join(".", parts);
        }
    }
}
