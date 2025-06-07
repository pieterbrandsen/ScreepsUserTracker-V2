using System;
using System.Collections.Generic;
using UserTrackerShared;
using Xunit;

namespace UserTrackerShared.Tests
{
    public class DynamicPatcherTests
    {
        private class Sample
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public NestedSample Nested { get; set; }
            public int[] Numbers { get; set; }
            public List<string> Tags { get; set; }
            public Dictionary<string, object> Data { get; set; }
            public NestedSample[] Children { get; set; }
            public List<NestedSample> NestedList { get; set; }
        }

        private class NestedSample
        {
            public int Id { get; set; }
            public DateTime Updated { get; set; }
            public EffectType EffectType { get; set; }
            public string Info { get; set; }
        }


        private enum EffectType
        {
            None,
            Started,
            Completed
        }

        [Fact]
        public void ApplyPatch_NullTarget_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => DynamicPatcher.ApplyPatch((object)null!, "Id", 1));
        }

        [Fact]
        public void ApplyPatch_EmptyPath_ThrowsArgumentException()
        {
            var sample = new Sample();
            Assert.Throws<ArgumentException>(() => DynamicPatcher.ApplyPatch(sample, string.Empty, 1));
            Assert.Throws<ArgumentException>(() => DynamicPatcher.ApplyPatch(sample, "   ", 1));
        }

        [Fact]
        public void ApplyPatch_NullChanges_DoesNothing()
        {
            var sample = new Sample { Id = 5 };
            DynamicPatcher.ApplyPatch(sample, (Dictionary<string, object?>)null!);
            Assert.Equal(5, sample.Id);
        }

        [Fact]
        public void ApplyPatch_EmptyChanges_DoesNothing()
        {
            var sample = new Sample { Id = 5 };
            DynamicPatcher.ApplyPatch(sample, new Dictionary<string, object?>());
            Assert.Equal(5, sample.Id);
        }

        [Fact]
        public void ApplyPatch_SimpleProperty_UpdatesValue()
        {
            var sample = new Sample { Id = 1, Name = "Old" };
            DynamicPatcher.ApplyPatch(sample, "Name", "New");
            Assert.Equal("New", sample.Name);
        }

        [Fact]
        public void ApplyPatch_NestedProperty_UpdatesValue()
        {
            var sample = new Sample { Nested = new NestedSample() };
            DynamicPatcher.ApplyPatch(sample, new Dictionary<string, object?>
            {
                { "nested.updated", new DateTime(2025, 6, 7) }
            });
            Assert.Equal(new DateTime(2025, 6, 7), sample.Nested.Updated);
        }

        [Fact]
        public void ApplyPatch_ArrayIndex_ExpandsAndSetsValue()
        {
            var sample = new Sample { Numbers = new int[0] };
            DynamicPatcher.ApplyPatch(sample, "Numbers[2]", 42);
            Assert.Equal(3, sample.Numbers.Length);
            Assert.Equal(42, sample.Numbers[2]);
        }

        [Fact]
        public void ApplyPatch_ArrayIndex_NonLast_CreatesAndNavigates()
        {
            var sample = new Sample { Children = new NestedSample[0] };
            DynamicPatcher.ApplyPatch(sample, "Children[1].Info", "hello");
            Assert.Equal(2, sample.Children.Length);
            Assert.Equal("hello", sample.Children[1].Info);
        }

        [Fact]
        public void ApplyPatch_ListIndex_ExpandsAndSetsValue()
        {
            var sample = new Sample { Tags = null };
            DynamicPatcher.ApplyPatch(sample, "Tags[1]", "tag2");
            Assert.NotNull(sample.Tags);
            Assert.Equal(2, sample.Tags.Count);
            Assert.Null(sample.Tags[0]);
            Assert.Equal("tag2", sample.Tags[1]);
        }

        [Fact]
        public void ApplyPatch_ListIndex_NonLast_CreatesAndNavigates()
        {
            var sample = new Sample { NestedList = null };
            DynamicPatcher.ApplyPatch(sample, "NestedList[0].Info", "data");
            Assert.NotNull(sample.NestedList);
            Assert.Single(sample.NestedList);
            Assert.Equal("data", sample.NestedList[0].Info);
        }

        [Fact]
        public void ApplyPatch_Dictionary_CreatesAndSetsValue()
        {
            var sample = new Sample { Data = new Dictionary<string, object>() };
            DynamicPatcher.ApplyPatch(sample, "Data.customKey", 123);
            Assert.True(sample.Data.ContainsKey("customKey"));
            Assert.Equal(123, sample.Data["customKey"]);
        }

        [Fact]
        public void ApplyPatch_PropertyNameMapping_WorksForSpecialNames()
        {
            var sample = new Sample { Nested = new NestedSample() };
            DynamicPatcher.ApplyPatch(sample, "nested.effect", EffectType.Completed);
            Assert.Equal(EffectType.Completed, sample.Nested.EffectType);
            DynamicPatcher.ApplyPatch(sample, "nested._id", 5); // maps to Id
            Assert.Equal(5, sample.Nested.GetType().GetProperty("Id").GetValue(sample.Nested));
        }

        [Fact]
        public void ApplyPatch_ApplyMultipleChanges_WorksCorrectly()
        {
            var sample = new Sample
            {
                Id = 5,
                Name = "A",
                Numbers = new int[1] { 0 },
                Tags = new List<string>(),
                Data = new Dictionary<string, object>(),
                Nested = new NestedSample()
            };
            var changes = new Dictionary<string, object?>
            {
                { "Id", 10 },
                { "Name", "B" },
                { "Numbers[0]", 7 },
                { "Tags[0]", "first" },
                { "Data.key1", "value" },
                { "nested._updated", new DateTime(2025, 1, 1) }
            };
            DynamicPatcher.ApplyPatch(sample, changes);
            Assert.Equal(10, sample.Id);
            Assert.Equal("B", sample.Name);
            Assert.Equal(7, sample.Numbers[0]);
            Assert.Single(sample.Tags);
            Assert.Equal("first", sample.Tags[0]);
            Assert.Equal("value", sample.Data["key1"]);
            Assert.Equal(new DateTime(2025, 1, 1), sample.Nested.Updated);
        }

        [Fact]
        public void ApplyPatch_InvalidPath_ThrowsException()
        {
            var sample = new Sample();
            Assert.Throws<InvalidOperationException>(() => DynamicPatcher.ApplyPatch(sample, "NonExistent", 1));
        }
    }
}
