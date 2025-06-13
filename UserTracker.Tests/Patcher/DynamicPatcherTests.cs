using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Text;
using UserTracker.Tests.RoomHistory;
using UserTrackerShared;
using UserTrackerShared.Helpers;
using UserTrackerShared.States;
using UserTrackerShared.Utilities;

namespace UserTracker.Tests.Patcher
{
    public class TestObject
    {
        public TestObject[]? Array { get; set; }
        public List<TestObject>? List { get; set; }
        public Dictionary<string, TestObject>? Dictionary { get; set; }

        public string? String { get; set; }
        public int? Int { get; set; }
        public long? Long { get; set; }
    }

    public class DynamicPatcherTests
    {
        public DynamicPatcherTests()
        {
            var configFileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = "App.Live.Config"
            };
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            ConfigSettingsState.InitTest(configuration.AppSettings);
        }
        private static bool AreEqual(object? x, object? y)
        {
            if (x == null || y == null)
                return x == y;

            // Same type? Use Equals.
            if (x.GetType() == y.GetType())
                return x.Equals(y);

            // Try converting to common comparable types
            try
            {
                // Try numeric comparison
                if (double.TryParse(x.ToString(), out double dx) &&
                    double.TryParse(y.ToString(), out double dy))
                {
                    const double tolerance = 1e-9;
                    return Math.Abs(dx - dy) < tolerance;
                }
            }
            catch
            {
                // Accept
            }

            // Fallback: string comparison
            return x.ToString() == y.ToString();
        }

        public static IEnumerable<object?[]> ArrayData()
        {
            return new List<object?[]>
            {
                new object[] { "Array[0].String", "a" },
                new object[] { "Array[0].Int", 5 },
                new object[] { "Array[0].Long", 5 },

                new object[] { "Array>0.String", "a" },
                new object[] { "Array>0.Int", 5 },
                new object[] { "Array>0.Long", 5 },

                new object?[] { "Array[0].String", null },
                new object?[] { "Array[0].Int", null },
                new object?[] { "Array[0].Long", null },

                new object[] { "Array[0].Array[0].String", "a" },
                new object[] { "Array[0].Array[0].Int", 5 },
                new object[] { "Array[0].Array[0].Long", 5 },

                new object[] { "Array[0].Array[0].Array[0].String", "a" },
                new object[] { "Array[0].Array[0].Array[0].Int", 5 },
                new object[] { "Array[0].Array[0].Array[0].Long", 5 },

                new object[] { "Array[0].Array[2].Array[0].String", "a", false },
                new object[] { "Array[0].Array[2].Array[0].Int", 5, false },
                new object[] { "Array[0].Array[2].Array[0].Long", 5, false },
            };
        }

        public static IEnumerable<object?[]> MultipleArrayData()
        {
            var values_ab = new object[] { "a", "b" };
            var values_5_5 = new object[] { 5, 5 };

            var stringLevel1 = new[] { "Array[0].String", "Array[1].String" };
            var intLevel1 = new[] { "Array[0].Int", "Array[1].Int" };
            var longLevel1 = new[] { "Array[0].Long", "Array[1].Long" };

            var stringLevel2 = new[] { "Array[0].Array[0].String", "Array[0].Array[1].String" };
            var intLevel2 = new[] { "Array[0].Array[0].Int", "Array[0].Array[1].Int" };
            var longLevel2 = new[] { "Array[0].Array[0].Long", "Array[0].Array[1].Long" };

            var stringLevel3 = new[] { "Array[0].Array[0].Array[0].String", "Array[0].Array[0].Array[1].String" };
            var intLevel3 = new[] { "Array[0].Array[0].Array[0].Int", "Array[0].Array[0].Array[1].Int" };
            var longLevel3 = new[] { "Array[0].Array[0].Array[0].Long", "Array[0].Array[0].Array[1].Long" };

            var stringMismatch = new[] { "Array[0].Array[0].Array[3].String", "Array[0].Array[0].Array[1].String" };
            var intMismatch = new[] { "Array[0].Array[0].Array[3].Int", "Array[0].Array[0].Array[1].Int" };
            var longMismatch = new[] { "Array[0].Array[0].Array[3].Long", "Array[0].Array[0].Array[1].Long" };

            return new List<object?[]>
            {
                new object[] { stringLevel1, values_ab },
                new object[] { intLevel1, values_5_5 },
                new object[] { longLevel1, values_5_5 },

                new object[] { stringLevel2, values_ab },
                new object[] { intLevel2, values_5_5 },
                new object[] { longLevel2, values_5_5 },

                new object[] { stringLevel3, values_ab },
                new object[] { intLevel3, values_5_5 },
                new object[] { longLevel3, values_5_5 },

                new object[] { stringMismatch, values_ab, false },
                new object[] { intMismatch, values_5_5, false },
                new object[] { longMismatch, values_5_5, false },
            };
        }

        public static IEnumerable<object[]> ListData()
        {
            return new List<object[]>
            {
                new object[] { "List[0].String", "a" },
                new object[] { "List[0].Int", 5 },
                new object[] { "List[0].Long", 5 },

                new object[] { "List>0.String", "a" },
                new object[] { "List>0.Int", 5 },
                new object[] { "List>0.Long", 5 },

                new object[] { "List[0].List[0].String", "a" },
                new object[] { "List[0].List[0].Int", 5 },
                new object[] { "List[0].List[0].Long", 5 },

                new object[] { "List[0].List[0].List[0].String", "a" },
                new object[] { "List[0].List[0].List[0].Int", 5 },
                new object[] { "List[0].List[0].List[0].Long", 5 },

                new object[] { "List[0].List[2].List[0].String", "a", false },
                new object[] { "List[0].List[2].List[0].Int", 5, false },
                new object[] { "List[0].List[2].List[0].Long", 5, false },
            };
        }

        public static IEnumerable<object[]> MultipleListData()
        {
            var values_ab = new object[] { "a", "b" };
            var values_5_5 = new object[] { 5, 5 };

            var stringLevel1 = new[] { "List[0].String", "List[1].String" };
            var intLevel1 = new[] { "List[0].Int", "List[1].Int" };
            var longLevel1 = new[] { "List[0].Long", "List[1].Long" };

            var stringLevel2 = new[] { "List[0].List[0].String", "List[0].List[1].String" };
            var intLevel2 = new[] { "List[0].List[0].Int", "List[0].List[1].Int" };
            var longLevel2 = new[] { "List[0].List[0].Long", "List[0].List[1].Long" };

            var stringLevel3 = new[] { "List[0].List[0].List[0].String", "List[0].List[0].List[1].String" };
            var intLevel3 = new[] { "List[0].List[0].List[0].Int", "List[0].List[0].List[1].Int" };
            var longLevel3 = new[] { "List[0].List[0].List[0].Long", "List[0].List[0].List[1].Long" };

            var stringMismatch = new[] { "List[0].List[0].List[3].String", "List[0].List[0].List[1].String" };
            var intMismatch = new[] { "List[0].List[0].List[3].Int", "List[0].List[0].List[1].Int" };
            var longMismatch = new[] { "List[0].List[0].List[3].Long", "List[0].List[0].List[1].Long" };

            return new List<object[]>
            {
                new object[] { stringLevel1, values_ab },
                new object[] { intLevel1, values_5_5 },
                new object[] { longLevel1, values_5_5 },

                new object[] { stringLevel2, values_ab },
                new object[] { intLevel2, values_5_5 },
                new object[] { longLevel2, values_5_5 },

                new object[] { stringLevel3, values_ab },
                new object[] { intLevel3, values_5_5 },
                new object[] { longLevel3, values_5_5 },

                new object[] { stringMismatch, values_ab, false },
                new object[] { intMismatch, values_5_5, false },
                new object[] { longMismatch, values_5_5, false },
            };
        }

        public static IEnumerable<object[]> DictionaryData()
        {
            return new List<object[]>
            {
                new object[] { "Dictionary.Key.String", "a" },
                new object[] { "Dictionary.Key.Int", 5 },
                new object[] { "Dictionary.Key.Long", 5 },

                new object[] { "Dictionary>0.String", "a" },
                new object[] { "Dictionary>2.Int", 5 },
                new object[] { "Dictionary>5.Long", 5 },

                new object[] { "Dictionary.Key.Dictionary.Key.String", "a" },
                new object[] { "Dictionary.Key.Dictionary.Key.Int", 5 },
                new object[] { "Dictionary.Key.Dictionary.Key.Long", 5 },

                new object[] { "Dictionary.Key.Dictionary.Key.Dictionary.Key.String", "a" },
                new object[] { "Dictionary.Key.Dictionary.Key.Dictionary.Key.Int", 5 },
                new object[] { "Dictionary.Key.Dictionary.Key.Dictionary.Key.Long", 5 },

                new object[] { "Dictionary.Key.Dictionary.Key.Dictionary.String", "a", false },
                new object[] { "Dictionary.Key.Dictionary.Key.Dictionary.Int", 5 , false},
                new object[] { "Dictionary.Key.Dictionary.Key.Dictionary.Long", 5, false },
            };
        }

        public static IEnumerable<object[]> MultipleDictionaryData()
        {
            var values_ab = new object[] { "a", "b" };
            var values_5_5 = new object[] { 5, 5 };

            var stringLevel1 = new[] { "Dictionary.Key.String", "Dictionary.Key.String" };
            var intLevel1 = new[] { "Dictionary.Key.Int", "Dictionary.Key.Int" };
            var longLevel1 = new[] { "Dictionary.Key.Long", "Dictionary.Key.Long" };

            var stringLevel2 = new[] { "Dictionary.Key.Dictionary.Key.String", "Dictionary.Key.Dictionary.Key2.String" };
            var intLevel2 = new[] { "Dictionary.Key.Dictionary.Key.Int", "Dictionary.Key.Dictionary.Key2.Int" };
            var longLevel2 = new[] { "Dictionary.Key.Dictionary.Key.Long", "Dictionary.Key.Dictionary.Key2.Long" };

            var stringLevel3 = new[] { "Dictionary.Key.Dictionary.Key.Dictionary.Key.String", "Dictionary.Key.Dictionary.Key.Dictionary.Key2.String" };
            var intLevel3 = new[] { "Dictionary.Key.Dictionary.Key.Dictionary.Key.Int", "Dictionary.Key.Dictionary.Key.Dictionary.Key2.Int" };
            var longLevel3 = new[] { "Dictionary.Key.Dictionary.Key.Dictionary.Key.Long", "Dictionary.Key.Dictionary.Key.Dictionary.Key2.Long" };

            var stringMismatch = new[] { "Dictionary.Key.Dictionary.Key.Dictionary.Key.3.String", "Dictionary.Key.Dictionary.Key.Dictionary.String" };
            var intMismatch = new[] { "Dictionary.Key.Dictionary.Key.Dictionary.Key.3.Int", "Dictionary.Key.Dictionary.Key.Dictionary.Int" };
            var longMismatch = new[] { "Dictionary.Key.Dictionary.Key.Dictionary.Key.3.Long", "Dictionary.Key.Dictionary.Key.Dictionary.Long" };

            return new List<object[]>
            {
                new object[] { stringLevel1, values_ab },
                new object[] { intLevel1, values_5_5 },
                new object[] { longLevel1, values_5_5 },

                new object[] { stringLevel2, values_ab },
                new object[] { intLevel2, values_5_5 },
                new object[] { longLevel2, values_5_5 },

                new object[] { stringLevel3, values_ab },
                new object[] { intLevel3, values_5_5 },
                new object[] { longLevel3, values_5_5 },

                new object[] { stringMismatch, values_ab, false },
                new object[] { intMismatch, values_5_5, false },
                new object[] { longMismatch, values_5_5, false },
            };
        }

        [Theory]
        [MemberData(nameof(ArrayData), MemberType = typeof(DynamicPatcherTests))]
        public void ArrayData_ReturnsCorrectResult(string path, object expected, bool shouldNotFail = true)
        {
            var obj = new TestObject();
            try
            {
                DynamicPatcher.ApplyPatch(obj, path, expected);
            }
            catch (Exception)
            {
                if (shouldNotFail) throw;
            }

            var changes = new Dictionary<string, object?>();
            var jToken = JToken.Parse(JsonConvert.SerializeObject(obj));
            JsonHelper.FlattenJson(jToken, new StringBuilder(), changes);

            if (path.Contains('>'))
            {
                var separatorIndex = path.IndexOf('>');
                var endOfSeparatorIndex = path.IndexOf('.');
                path = path.Substring(0, separatorIndex) + "[" + path.Substring(separatorIndex + 1, endOfSeparatorIndex - separatorIndex - 1) + "]" + path.Substring(endOfSeparatorIndex);
            }

            var change = changes.FirstOrDefault(c => c.Key == path);
            Assert.True((change.Key == path) == shouldNotFail);
            Assert.True(AreEqual(change.Value, expected) == shouldNotFail);
        }

        [Theory]
        [MemberData(nameof(MultipleArrayData), MemberType = typeof(DynamicPatcherTests))]
        [MemberData(nameof(MultipleListData), MemberType = typeof(DynamicPatcherTests))]
        [MemberData(nameof(MultipleDictionaryData), MemberType = typeof(DynamicPatcherTests))]
        public void MultipleData_ReturnsCorrectResult(string[] paths, object[] expecteds, bool shouldNotFail = true)
        {
            var obj = new TestObject();
            foreach (string path in paths)
            {
                try
                {
                    var expected = expecteds[Array.IndexOf(paths, path)];
                    DynamicPatcher.ApplyPatch(obj, path, expected);
                }
                catch (Exception)
                {
                    if (shouldNotFail) throw;
                }
            }

            var changes = new Dictionary<string, object?>();
            var jToken = JToken.Parse(JsonConvert.SerializeObject(obj));
            JsonHelper.FlattenJson(jToken, new StringBuilder(), changes);
            foreach (string path in paths)
            {
                var change = changes.FirstOrDefault(c => c.Key == path);
                var expected = expecteds[Array.IndexOf(paths, path)];
                Assert.True((change.Key == path) == shouldNotFail);
                Assert.True((AreEqual(change.Value, expected)) == shouldNotFail);
            }
        }

        [Theory]
        [MemberData(nameof(ListData), MemberType = typeof(DynamicPatcherTests))]
        public void ListData_ReturnsCorrectResult(string path, object expected, bool shouldNotFail = true)
        {
            var obj = new TestObject();
            try
            {
                DynamicPatcher.ApplyPatch(obj, path, expected);
            }
            catch (Exception)
            {
                if (shouldNotFail) throw;
            }

            var changes = new Dictionary<string, object?>();
            var jToken = JToken.Parse(JsonConvert.SerializeObject(obj));
            JsonHelper.FlattenJson(jToken, new StringBuilder(), changes);

            if (path.Contains('>'))
            {
                var separatorIndex = path.IndexOf('>');
                var endOfSeparatorIndex = path.IndexOf('.');
                path = path.Substring(0, separatorIndex) + "[" + path.Substring(separatorIndex + 1, endOfSeparatorIndex - separatorIndex - 1) + "]" + path.Substring(endOfSeparatorIndex);
            }

            var change = changes.FirstOrDefault(c => c.Key == path);
            Assert.True((change.Key == path) == shouldNotFail);
            Assert.True((AreEqual(change.Value, expected)) == shouldNotFail);
        }

        [Theory]
        [MemberData(nameof(DictionaryData), MemberType = typeof(DynamicPatcherTests))]
        public void DictionaryData_ReturnsCorrectResult(string path, object expected, bool shouldNotFail = true)
        {
            var obj = new TestObject();
            try
            {
                DynamicPatcher.ApplyPatch(obj, path, expected);
            }
            catch (Exception)
            {
                if (shouldNotFail) throw;
            }

            var changes = new Dictionary<string, object?>();
            var jToken = JToken.Parse(JsonConvert.SerializeObject(obj));
            JsonHelper.FlattenJson(jToken, new StringBuilder(), changes);

            var change = changes.FirstOrDefault(c => c.Key == path);
            Assert.True((change.Key == path) == shouldNotFail);
            Assert.True((AreEqual(change.Value, expected)) == shouldNotFail);
        }

        [Fact]
        public void ParseFile_ReturnsCorrectChanges()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files", "case4.json");
            var (changes, properties) = HistoryFileChecker.ParseFile(path);

            Assert.True(changes == 100, "Expected 100 changes, got " + changes);
            Assert.True(properties.Count == 10, "Expected 10 properties, got " + properties.Count);
            foreach (var kv in properties)
            {
                Assert.True(kv.Value == 10);
            }
        }
    }
}